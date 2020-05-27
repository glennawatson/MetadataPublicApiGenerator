// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text;
using System.Threading;

namespace LightweightMetadata
{
    /// <summary>
    /// A wrapper around a type.
    /// </summary>
    public class TypeWrapper : IHandleTypeNamedWrapper, IHasAttributes, IHasGenericParameters
    {
        private static readonly ConcurrentDictionary<(TypeDefinitionHandle Handle, string ModulePath), TypeWrapper> _handleToWrapperDictionary
            = new ConcurrentDictionary<(TypeDefinitionHandle, string), TypeWrapper>();

        private readonly Lazy<string> _name;
        private readonly Lazy<string> _namespace;
        private readonly Lazy<string> _fullName;
        private readonly Lazy<string> _reflectionFullName;
        private readonly Lazy<string> _nameWithNumeric;
        private readonly Lazy<bool> _isKnownType;
        private readonly Lazy<bool> _isEnumType;
        private readonly Lazy<bool> _isDelegateType;
        private readonly Lazy<IHandleTypeNamedWrapper?> _base;
        private readonly Lazy<SymbolTypeKind> _typeKind;
        private readonly Lazy<IReadOnlyList<AttributeWrapper>> _attributes;
        private readonly Lazy<IReadOnlyList<GenericParameterWrapper>> _genericParameters;
        private readonly Lazy<IReadOnlyList<MethodWrapper>> _methods;
        private readonly Lazy<IReadOnlyList<PropertyWrapper>> _properties;
        private readonly Lazy<(FieldWrapper? EnumMetadata, IReadOnlyList<FieldWrapper> Fields)> _fields;
        private readonly Lazy<IReadOnlyList<EventWrapper>> _events;
        private readonly Lazy<IReadOnlyList<TypeWrapper>> _nestedTypes;
        private readonly Lazy<IReadOnlyList<InterfaceImplementationWrapper>> _interfaceImplementations;
        private readonly Lazy<TypeWrapper?> _declaringType;
        private readonly Lazy<KnownTypeCode> _knownTypeCode;
        private readonly Lazy<bool> _isValueType;

        private TypeWrapper(AssemblyMetadata assemblyMetadata, TypeDefinitionHandle typeDefinition)
        {
            AssemblyMetadata = assemblyMetadata ?? throw new ArgumentNullException(nameof(assemblyMetadata));
            TypeDefinitionHandle = typeDefinition;
            Handle = typeDefinition;
            TypeDefinition = AssemblyMetadata.MetadataReader.GetTypeDefinition(typeDefinition);

            _nameWithNumeric = new Lazy<string>(() => AssemblyMetadata.MetadataReader.GetString(TypeDefinition.Name), LazyThreadSafetyMode.PublicationOnly);
            _name = new Lazy<string>(GetNameWithoutNumeric, LazyThreadSafetyMode.PublicationOnly);
            _namespace = new Lazy<string>(() => TypeDefinition.Namespace.GetName(AssemblyMetadata), LazyThreadSafetyMode.PublicationOnly);
            _fullName = new Lazy<string>(GetFullName, LazyThreadSafetyMode.PublicationOnly);
            _reflectionFullName = new Lazy<string>(GetReflectionFullName, LazyThreadSafetyMode.PublicationOnly);
            _isKnownType = new Lazy<bool>(() => this.ToKnownTypeCode() != KnownTypeCode.None, LazyThreadSafetyMode.PublicationOnly);
            _isEnumType = new Lazy<bool>(IsEnum, LazyThreadSafetyMode.PublicationOnly);
            _typeKind = new Lazy<SymbolTypeKind>(GetTypeKind, LazyThreadSafetyMode.PublicationOnly);
            _isDelegateType = new Lazy<bool>(IsDelegate, LazyThreadSafetyMode.PublicationOnly);
            _attributes = new Lazy<IReadOnlyList<AttributeWrapper>>(() => AttributeWrapper.CreateChecked(TypeDefinition.GetCustomAttributes(), assemblyMetadata), LazyThreadSafetyMode.PublicationOnly);
            _genericParameters = new Lazy<IReadOnlyList<GenericParameterWrapper>>(() => GenericParameterWrapper.Create(TypeDefinition.GetGenericParameters(), this, AssemblyMetadata), LazyThreadSafetyMode.PublicationOnly);
            _declaringType = new Lazy<TypeWrapper?>(() => Create(TypeDefinition.GetDeclaringType(), AssemblyMetadata));
            _methods = new Lazy<IReadOnlyList<MethodWrapper>>(() => MethodWrapper.CreateChecked(TypeDefinition.GetMethods(), AssemblyMetadata), LazyThreadSafetyMode.PublicationOnly);
            _properties = new Lazy<IReadOnlyList<PropertyWrapper>>(() => PropertyWrapper.CreateChecked(TypeDefinition.GetProperties(), AssemblyMetadata), LazyThreadSafetyMode.PublicationOnly);
            _fields = new Lazy<(FieldWrapper? EnumMetadata, IReadOnlyList<FieldWrapper> Fields)>(CreateFields, LazyThreadSafetyMode.PublicationOnly);
            _events = new Lazy<IReadOnlyList<EventWrapper>>(() => EventWrapper.CreateChecked(TypeDefinition.GetEvents(), AssemblyMetadata), LazyThreadSafetyMode.PublicationOnly);
            _nestedTypes = new Lazy<IReadOnlyList<TypeWrapper>>(() => CreateChecked(TypeDefinition.GetNestedTypes(), AssemblyMetadata), LazyThreadSafetyMode.PublicationOnly);
            _interfaceImplementations = new Lazy<IReadOnlyList<InterfaceImplementationWrapper>>(() => InterfaceImplementationWrapper.CreateChecked(TypeDefinition.GetInterfaceImplementations(), AssemblyMetadata), LazyThreadSafetyMode.PublicationOnly);
            _knownTypeCode = new Lazy<KnownTypeCode>(this.ToKnownTypeCode, LazyThreadSafetyMode.PublicationOnly);
            _isValueType = new Lazy<bool>(GetIsValueType, LazyThreadSafetyMode.PublicationOnly);

            _base = new Lazy<IHandleTypeNamedWrapper?>(
                () =>
                    {
                        var baseType = GetBaseTypeOrNil();

                        if (baseType.IsNil)
                        {
                            return null;
                        }

                        return WrapperFactory.Create(baseType, AssemblyMetadata);
                    }, LazyThreadSafetyMode.PublicationOnly);

            switch (TypeDefinition.Attributes & TypeAttributes.VisibilityMask)
            {
                case TypeAttributes.NotPublic:
                case TypeAttributes.NestedAssembly:
                    Accessibility = EntityAccessibility.Internal;
                    break;
                case TypeAttributes.Public:
                case TypeAttributes.NestedPublic:
                    Accessibility = EntityAccessibility.Public;
                    break;
                case TypeAttributes.NestedPrivate:
                    Accessibility = EntityAccessibility.Private;
                    break;
                case TypeAttributes.NestedFamily:
                    Accessibility = EntityAccessibility.Protected;
                    break;
                case TypeAttributes.NestedFamANDAssem:
                    Accessibility = EntityAccessibility.PrivateProtected;
                    break;
                case TypeAttributes.NestedFamORAssem:
                    Accessibility = EntityAccessibility.ProtectedInternal;
                    break;
                default:
                    Accessibility = EntityAccessibility.None;
                    break;
            }

            IsAbstract = (TypeDefinition.Attributes & TypeAttributes.Abstract) != 0;

            IsStatic = (TypeDefinition.Attributes & (TypeAttributes.Abstract | TypeAttributes.Sealed)) == (TypeAttributes.Abstract | TypeAttributes.Sealed);
            IsSealed = (TypeDefinition.Attributes & TypeAttributes.Sealed) != 0;
        }

        /// <summary>
        /// Gets the type definition for the type.
        /// </summary>
        public TypeDefinition TypeDefinition { get; }

        /// <summary>
        /// Gets the events on the type.
        /// </summary>
        public IReadOnlyList<EventWrapper> Events => _events.Value;

        /// <summary>
        /// Gets the fields on the type.
        /// </summary>
        public IReadOnlyList<FieldWrapper> Fields => _fields.Value.Fields;

        /// <summary>
        /// Gets the methods on the type.
        /// </summary>
        public IReadOnlyList<MethodWrapper> Methods => _methods.Value;

        /// <summary>
        /// Gets the properties on the type.
        /// </summary>
        public IReadOnlyList<PropertyWrapper> Properties => _properties.Value;

        /// <summary>
        /// Gets nested type son the type.
        /// </summary>
        public IReadOnlyList<TypeWrapper> NestedTypes => _nestedTypes.Value;

        /// <inheritdoc />
        public string Name => _name.Value;

        /// <summary>
        /// Gets a value indicating whether this type is nested.
        /// </summary>
        public bool IsNested => DeclaringType != null;

        /// <summary>
        /// Gets the name with numeric portion.
        /// </summary>
        public string NameWithNumeric => _nameWithNumeric.Value;

        /// <inheritdoc />
        public string TypeNamespace => _namespace.Value;

        /// <inheritdoc />
        public string FullName => _fullName.Value;

        /// <summary>
        /// Gets a value indicating whether the type is known.
        /// </summary>
        public virtual bool IsKnownType => _isKnownType.Value;

        /// <summary>
        /// Gets a value indicating whether the type is a enum type.
        /// </summary>
        public virtual bool IsEnumType => _isEnumType.Value;

        /// <summary>
        /// Gets a value indicating whether the type is a delegate type.
        /// </summary>
        public bool IsDelegateType => _isDelegateType.Value;

        /// <inheritdoc />
        public bool IsAbstract { get; }

        /// <inheritdoc />
        public KnownTypeCode KnownType => _knownTypeCode.Value;

        /// <inheritdoc />
        public EntityAccessibility Accessibility { get; }

        /// <summary>
        /// Gets a value indicating whether the type is static.
        /// </summary>
        public bool IsStatic { get; }

        /// <summary>
        /// Gets a value indicating whether the type is sealed.
        /// </summary>
        public bool IsSealed { get; }

        /// <inheritdoc />
        public Handle Handle { get; }

        /// <summary>
        /// Gets the base type.
        /// </summary>
        public IHandleTypeNamedWrapper? Base => _base.Value;

        /// <summary>
        /// Gets the kind of type this type is.
        /// </summary>
        public SymbolTypeKind TypeKind => _typeKind.Value;

        /// <summary>
        /// Gets the type that declares this type, if any.
        /// </summary>
        public TypeWrapper? DeclaringType => _declaringType.Value;

        /// <inheritdoc />
        public IReadOnlyList<AttributeWrapper> Attributes => _attributes.Value;

        /// <summary>
        /// Gets the interface implementations.
        /// </summary>
        public IReadOnlyList<InterfaceImplementationWrapper> InterfaceImplementations => _interfaceImplementations.Value;

        /// <summary>
        /// Gets a list of generic parameters.
        /// </summary>
        public IReadOnlyList<GenericParameterWrapper> GenericParameters => _genericParameters.Value;

        /// <inheritdoc />
        public AssemblyMetadata AssemblyMetadata { get; }

        /// <summary>
        /// Gets the type definition handle.
        /// </summary>
        public TypeDefinitionHandle TypeDefinitionHandle { get; }

        /// <inheritdoc />
        public string ReflectionFullName => _reflectionFullName.Value;

        /// <inheritdoc />
        public bool IsValueType => _isValueType.Value;

        /// <summary>
        /// Creates a new instance of the type wrapper.
        /// </summary>
        /// <param name="handle">The handle to wrap.</param>
        /// <param name="assemblyMetadata">The module containing the handle.</param>
        /// <returns>The wrapped instance if the handle is not nil, otherwise null.</returns>
        public static TypeWrapper? Create(TypeDefinitionHandle handle, AssemblyMetadata assemblyMetadata)
        {
            if (handle.IsNil)
            {
                return null;
            }

            if (assemblyMetadata is null)
            {
                throw new ArgumentNullException(nameof(assemblyMetadata));
            }

            return _handleToWrapperDictionary.GetOrAdd((handle, assemblyMetadata.FileName), _ => new TypeWrapper(assemblyMetadata, handle));
        }

        /// <summary>
        /// Creates a new instance of the type wrapper.
        /// </summary>
        /// <param name="handle">The handle to wrap.</param>
        /// <param name="assemblyMetadata">The module containing the handle.</param>
        /// <returns>The wrapped instance.</returns>
        public static TypeWrapper CreateChecked(TypeDefinitionHandle handle, AssemblyMetadata assemblyMetadata)
        {
            var type = Create(handle, assemblyMetadata);

            if (type == null)
            {
                throw new ArgumentException("Unable to create the type wrapper.", nameof(handle));
            }

            return type;
        }

        /// <summary>
        /// Creates a array instances of a type.
        /// </summary>
        /// <param name="collection">The collection to create.</param>
        /// <param name="assemblyMetadata">The module to use in creation.</param>
        /// <returns>The list of the type.</returns>
        public static IReadOnlyList<TypeWrapper?> Create(in TypeDefinitionHandleCollection collection, AssemblyMetadata assemblyMetadata)
        {
            var output = new TypeWrapper?[collection.Count];

            int i = 0;
            foreach (var element in collection)
            {
                output[i] = Create(element, assemblyMetadata);
                i++;
            }

            return output;
        }

        /// <summary>
        /// Creates a array instances of a type.
        /// </summary>
        /// <param name="collection">The collection to create.</param>
        /// <param name="assemblyMetadata">The module to use in creation.</param>
        /// <returns>The list of the type.</returns>
        public static IReadOnlyList<TypeWrapper> CreateChecked(in TypeDefinitionHandleCollection collection, AssemblyMetadata assemblyMetadata)
        {
            var entities = Create(collection, assemblyMetadata);

            if (entities.Any(x => x == null))
            {
                throw new ArgumentException("Have invalid types.", nameof(collection));
            }

            return entities.Select(x => x!).ToList();
        }

        /// <summary>
        /// Creates a array instances of a type.
        /// </summary>
        /// <param name="collection">The collection to create.</param>
        /// <param name="assemblyMetadata">The module to use in creation.</param>
        /// <returns>The list of the type.</returns>
        public static IReadOnlyList<TypeWrapper> CreateChecked(in ImmutableArray<TypeDefinitionHandle> collection, AssemblyMetadata assemblyMetadata)
        {
            var entities = Create(collection, assemblyMetadata);

            if (entities.Any(x => x == null))
            {
                throw new ArgumentException("Have invalid types.", nameof(collection));
            }

            return entities.Select(x => x!).ToList();
        }

        /// <summary>
        /// Creates a array instances of a type.
        /// </summary>
        /// <param name="collection">The collection to create.</param>
        /// <param name="assemblyMetadata">The module to use in creation.</param>
        /// <returns>The list of the type.</returns>
        public static IReadOnlyList<TypeWrapper?> Create(in ImmutableArray<TypeDefinitionHandle> collection, AssemblyMetadata assemblyMetadata)
        {
            var output = new TypeWrapper?[collection.Length];

            int i = 0;
            foreach (var element in collection)
            {
                output[i] = Create(element, assemblyMetadata);
                i++;
            }

            return output;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return FullName;
        }

        /// <summary>
        /// Attempt and get the backing type if this value is a enum.
        /// </summary>
        /// <param name="underlyingType">The underlying type if this is a enum type.</param>
        /// <returns>True if it's a enum type, false otherwise.</returns>
        public bool TryGetEnumType(out IHandleTypeNamedWrapper underlyingType)
        {
            var baseType = Base;
            if (baseType == null)
            {
                underlyingType = null!;
                return false;
            }

            if (baseType.ToKnownTypeCode() != KnownTypeCode.Enum)
            {
                underlyingType = null!;
                return false;
            }

            if (Fields.Count == 0)
            {
                underlyingType = null!;
                return false;
            }

            var enumMetadata = _fields.Value.EnumMetadata;

            if (enumMetadata == null)
            {
                underlyingType = null!;
                return false;
            }

            underlyingType = enumMetadata.FieldType;

            return true;
        }

        /// <summary>
        /// Tries to get the value in Enum value name form.
        /// If it cannot be converted to the name, it will keep the default value.
        /// This code is based around https://github.com/dotnet/corefx/blob/b2097cbdcb26f7f317252334ddcce101a20b7f3d/src/Common/src/CoreLib/System/Enum.cs
        /// and you should check there for further information.
        /// </summary>
        /// <param name="enumValue">The enum value to compare.</param>
        /// <param name="outEnumValue">The output variable with the name.</param>
        /// <returns>If the conversion was successful or not.</returns>
        public bool TryGetEnumName(object enumValue, out IReadOnlyList<string> outEnumValue)
        {
            var baseType = Base;
            if (baseType == null)
            {
                outEnumValue = null!;
                return false;
            }

            var knownType = baseType.ToKnownTypeCode();
            if (knownType != KnownTypeCode.Enum)
            {
                outEnumValue = null!;
                return false;
            }

            if (Fields.Count == 0)
            {
                outEnumValue = null!;
                return false;
            }

            // store in ulong value, since that's the maximum size of a enum.
            ulong value = Convert.ToUInt64(enumValue, CultureInfo.InvariantCulture);

            // Values are sorted, so if the incoming value is 0, we can check to see whether
            // the first entry matches it, in which case we can return its name; otherwise,
            // we can just return "0".
            if (value == 0)
            {
                if (Fields.Count > 0 && Fields[0].LongDefaultValue == 0)
                {
                    outEnumValue = new[] { Fields[0].Name };
                    return true;
                }
            }

            if (Attributes.HasKnownAttribute(KnownAttribute.Flags))
            {
                outEnumValue = HandleFlagsEnum(value)!;
                return outEnumValue != null;
            }

            var matchedValue = Fields.FirstOrDefault(x => x.LongDefaultValue == value);

            if (matchedValue != null)
            {
                outEnumValue = new[] { matchedValue.Name };
                return true;
            }

            outEnumValue = null!;
            return false;
        }

        private bool GetIsValueType()
        {
            var baseType = Base;
            if (baseType == null)
            {
                return false;
            }

            if (baseType.ToKnownTypeCode() == KnownTypeCode.Enum)
            {
                return true;
            }

            return baseType.ToKnownTypeCode() == KnownTypeCode.ValueType;
        }

        private bool IsDelegate()
        {
            var baseType = Base;
            if (baseType == null)
            {
                return false;
            }

            var knownType = baseType.ToKnownTypeCode();
            return knownType == KnownTypeCode.MulticastDelegate;
        }

        private bool IsEnum()
        {
            var baseType = Base;
            if (baseType == null)
            {
                return false;
            }

            return baseType.ToKnownTypeCode() == KnownTypeCode.Enum;
        }

        private EntityHandle GetBaseTypeOrNil()
        {
            try
            {
                return TypeDefinition.BaseType;
            }
            catch (BadImageFormatException)
            {
                return default;
            }
        }

        private SymbolTypeKind GetTypeKind()
        {
            var attributes = TypeDefinition.Attributes;

            if ((attributes & TypeAttributes.ClassSemanticsMask) == TypeAttributes.Interface)
            {
                return SymbolTypeKind.Interface;
            }

            if (IsEnumType)
            {
                return SymbolTypeKind.Enum;
            }

            if (IsValueType)
            {
                return SymbolTypeKind.Struct;
            }

            if (IsDelegateType)
            {
                return SymbolTypeKind.Delegate;
            }

            return SymbolTypeKind.Class;
        }

        private string GetFullName()
        {
            var declaringType = DeclaringType;

            var stringBuilder = new StringBuilder();
            if (declaringType == null)
            {
                if (!string.IsNullOrWhiteSpace(TypeNamespace))
                {
                    stringBuilder.Append(TypeNamespace).Append('.');
                }

                stringBuilder.Append(NameWithNumeric);
            }
            else
            {
                stringBuilder.Append(declaringType.FullName)
                    .Append('.')
                    .Append(NameWithNumeric);
            }

            return stringBuilder.ToString();
        }

        private string GetReflectionFullName()
        {
            var declaringType = DeclaringType;

            var stringBuilder = new StringBuilder();

            var strippedName = Name;

            if (declaringType == null)
            {
                if (!string.IsNullOrWhiteSpace(TypeNamespace))
                {
                    stringBuilder.Append(TypeNamespace).Append('.');
                }

                stringBuilder.Append(strippedName);
            }
            else
            {
                stringBuilder.Append(declaringType.ReflectionFullName)
                    .Append('.')
                    .Append(strippedName);
            }

            return stringBuilder.ToString().GetRealTypeName();
        }

        private string GetNameWithoutNumeric()
        {
            return NameWithNumeric.SplitTypeParameterCountFromReflectionName(out _);
        }

        private IReadOnlyList<string>? HandleFlagsEnum(ulong value)
        {
            var foundItems = new List<string>(Fields.Count - 1);

            ulong checkValue = value;

            foreach (var field in Fields)
            {
                var fieldValue = field.LongDefaultValue;
                if ((value & fieldValue) == fieldValue)
                {
                    foundItems.Add(field.Name);
                    checkValue -= fieldValue;
                }
            }

            // If we exhausted looking through all the values and we still have
            // a non-zero result, we couldn't match the result to only named values.
            // In that case, we return null and let the call site just generate
            // a string for the integral value.
            if (checkValue != 0)
            {
                return null;
            }

            return foundItems;
        }

        private (FieldWrapper? EnumMetadata, IReadOnlyList<FieldWrapper> Fields) CreateFields()
        {
            var fields = FieldWrapper.CreateChecked(TypeDefinition.GetFields(), AssemblyMetadata);

            // skip the value__ field, which is a metadata only field for enums.
            if (fields.Count > 0 && fields[0].Name == "value__")
            {
                return (fields[0], fields.Skip(1).ToList());
            }

            return (null, fields);
        }
    }
}
