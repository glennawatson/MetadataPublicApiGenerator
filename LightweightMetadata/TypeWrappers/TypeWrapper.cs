// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text;
using System.Threading;
using LightweightMetadata.Extensions;

namespace LightweightMetadata.TypeWrappers
{
    /// <summary>
    /// A wrapper around a type.
    /// </summary>
    [DebuggerDisplay("{" + nameof(FullName) + "}")]
    public class TypeWrapper : IHandleTypeNamedWrapper, IHasAttributes
    {
        private static readonly IDictionary<TypeDefinitionHandle, TypeWrapper> _types = new Dictionary<TypeDefinitionHandle, TypeWrapper>();

        private readonly Lazy<string> _name;
        private readonly Lazy<string> _namespace;
        private readonly Lazy<string> _fullName;
        private readonly Lazy<string> _reflectionFullName;
        private readonly Lazy<bool> _isKnownType;
        private readonly Lazy<bool> _isEnumType;
        private readonly Lazy<IHandleTypeNamedWrapper> _base;
        private readonly Lazy<SymbolTypeKind> _typeKind;
        private readonly Lazy<IReadOnlyList<AttributeWrapper>> _attributes;
        private readonly Lazy<IReadOnlyList<TypeParameterWrapper>> _genericParameters;
        private readonly Lazy<IReadOnlyList<MethodWrapper>> _methods;
        private readonly Lazy<IReadOnlyList<PropertyWrapper>> _properties;
        private readonly Lazy<IReadOnlyList<FieldWrapper>> _fields;
        private readonly Lazy<IReadOnlyList<EventWrapper>> _events;
        private readonly Lazy<IReadOnlyList<TypeWrapper>> _nestedTypes;
        private readonly Lazy<IReadOnlyList<InterfaceImplementationWrapper>> _interfaceImplementations;
        private readonly Lazy<TypeWrapper> _declaringType;
        private readonly Lazy<KnownTypeCode> _knownTypeCode;

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeWrapper"/> class.
        /// </summary>
        /// <param name="module">The main compilation module.</param>
        /// <param name="typeDefinition">The type definition we are wrapping.</param>
        private TypeWrapper(CompilationModule module, TypeDefinitionHandle typeDefinition)
        {
            CompilationModule = module ?? throw new ArgumentNullException(nameof(module));
            TypeDefinitionHandle = typeDefinition;
            Handle = typeDefinition;
            TypeDefinition = CompilationModule.MetadataReader.GetTypeDefinition(typeDefinition);

            _name = new Lazy<string>(() => CompilationModule.MetadataReader.GetString(TypeDefinition.Name), LazyThreadSafetyMode.PublicationOnly);
            _namespace = new Lazy<string>(() => TypeDefinition.Namespace.GetName(CompilationModule), LazyThreadSafetyMode.PublicationOnly);
            _fullName = new Lazy<string>(GetFullName, LazyThreadSafetyMode.PublicationOnly);
            _reflectionFullName = new Lazy<string>(GetReflectionFullName, LazyThreadSafetyMode.PublicationOnly);
            _isKnownType = new Lazy<bool>(() => this.ToKnownTypeCode() != KnownTypeCode.None, LazyThreadSafetyMode.PublicationOnly);
            _isEnumType = new Lazy<bool>(IsEnum, LazyThreadSafetyMode.PublicationOnly);
            _typeKind = new Lazy<SymbolTypeKind>(GetTypeKind, LazyThreadSafetyMode.PublicationOnly);
            _attributes = new Lazy<IReadOnlyList<AttributeWrapper>>(() => TypeDefinition.GetCustomAttributes().Select(x => AttributeWrapper.Create(x, CompilationModule)).ToList(), LazyThreadSafetyMode.PublicationOnly);
            _genericParameters = new Lazy<IReadOnlyList<TypeParameterWrapper>>(() => TypeParameterWrapper.Create(CompilationModule, TypeDefinitionHandle, TypeDefinition.GetGenericParameters()), LazyThreadSafetyMode.PublicationOnly);
            _declaringType = new Lazy<TypeWrapper>(() => TypeWrapper.Create(TypeDefinition.GetDeclaringType(), CompilationModule));
            _methods = new Lazy<IReadOnlyList<MethodWrapper>>(() => TypeDefinition.GetMethods().Select(x => MethodWrapper.Create(x, CompilationModule)).ToList(), LazyThreadSafetyMode.PublicationOnly);
            _properties = new Lazy<IReadOnlyList<PropertyWrapper>>(() => TypeDefinition.GetProperties().Select(x => PropertyWrapper.Create(x, CompilationModule)).ToList(), LazyThreadSafetyMode.PublicationOnly);
            _fields = new Lazy<IReadOnlyList<FieldWrapper>>(() => TypeDefinition.GetFields().Select(x => FieldWrapper.Create(x, CompilationModule)).ToList(), LazyThreadSafetyMode.PublicationOnly);
            _events = new Lazy<IReadOnlyList<EventWrapper>>(() => TypeDefinition.GetEvents().Select(x => EventWrapper.Create(x, CompilationModule)).ToList(), LazyThreadSafetyMode.PublicationOnly);
            _nestedTypes = new Lazy<IReadOnlyList<TypeWrapper>>(() => TypeDefinition.GetNestedTypes().Select(x => TypeWrapper.Create(x, CompilationModule)).ToList(), LazyThreadSafetyMode.PublicationOnly);
            _interfaceImplementations = new Lazy<IReadOnlyList<InterfaceImplementationWrapper>>(() => TypeDefinition.GetInterfaceImplementations().Select(x => InterfaceImplementationWrapper.Create(x, CompilationModule)).ToList(), LazyThreadSafetyMode.PublicationOnly);
            _knownTypeCode = new Lazy<KnownTypeCode>(this.ToKnownTypeCode, LazyThreadSafetyMode.PublicationOnly);

            _base = new Lazy<IHandleTypeNamedWrapper>(
                () =>
                    {
                        var baseType = GetBaseTypeOrNil();

                        if (baseType.IsNil)
                        {
                            return null;
                        }

                        return WrapperFactory.Create(baseType, CompilationModule);
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
        public IReadOnlyList<FieldWrapper> Fields => _fields.Value;

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
        public virtual string Name => _name.Value;

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
        public IHandleTypeNamedWrapper Base => _base.Value;

        /// <summary>
        /// Gets the kind of type this type is.
        /// </summary>
        public SymbolTypeKind TypeKind => _typeKind.Value;

        /// <summary>
        /// Gets the type that declares this type, if any.
        /// </summary>
        public TypeWrapper DeclaringType => _declaringType.Value;

        /// <inheritdoc />
        public IReadOnlyList<AttributeWrapper> Attributes => _attributes.Value;

        /// <summary>
        /// Gets the interface implementations.
        /// </summary>
        public IReadOnlyList<InterfaceImplementationWrapper> InterfaceImplementations => _interfaceImplementations.Value;

        /// <summary>
        /// Gets a list of generic parameters.
        /// </summary>
        public IReadOnlyList<TypeParameterWrapper> GenericParameters => _genericParameters.Value;

        /// <inheritdoc />
        public CompilationModule CompilationModule { get; }

        /// <summary>
        /// Gets the type definition handle.
        /// </summary>
        public TypeDefinitionHandle TypeDefinitionHandle { get; }

        /// <inheritdoc />
        public string ReflectionFullName => _reflectionFullName.Value;

        /// <summary>
        /// Creates a new instance of the type wrapper.
        /// </summary>
        /// <param name="handle">The handle to wrap.</param>
        /// <param name="module">The module containing the handle.</param>
        /// <returns>The wrapped instance if the handle is not nil, otherwise null.</returns>
        public static TypeWrapper Create(TypeDefinitionHandle handle, CompilationModule module)
        {
            if (handle.IsNil)
            {
                return null;
            }

            return _types.GetOrAdd(handle, createHandle => new TypeWrapper(module, createHandle));
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
            underlyingType = null;
            if (baseType == null)
            {
                return false;
            }

            if (baseType.ToKnownTypeCode() != KnownTypeCode.Enum)
            {
                return false;
            }

            if (Fields.Count == 0)
            {
                return false;
            }

            underlyingType = Fields[0].FieldType;

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
            outEnumValue = null;
            var baseType = Base;
            if (baseType == null)
            {
                return false;
            }

            var knownType = baseType.ToKnownTypeCode();
            if (knownType != KnownTypeCode.Enum)
            {
                return false;
            }

            if (Fields.Count == 0)
            {
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
                outEnumValue = HandleFlagsEnum(value);
                return outEnumValue != null;
            }

            int index = Fields.BinarySearchIndexOfBy((item, compare) => item.LongDefaultValue.CompareTo(compare), value);

            if (index >= 0)
            {
                outEnumValue = new[] { Fields[index].Name };
                return true;
            }

            return false;
        }

        private bool IsValueType()
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

            if (baseType.ToKnownTypeCode() != KnownTypeCode.ValueType)
            {
                return false;
            }

            return true;
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

            if (IsValueType())
            {
                return SymbolTypeKind.Struct;
            }

            if (IsDelegate())
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

                stringBuilder.Append(Name);
            }
            else
            {
                stringBuilder.Append(DeclaringType.FullName)
                    .Append('.')
                    .Append(Name);
            }

            return stringBuilder.ToString();
        }

        private string GetReflectionFullName()
        {
            var declaringType = DeclaringType;

            var stringBuilder = new StringBuilder();

            int index = Name.IndexOf("`", StringComparison.InvariantCulture);
            string strippedName = index >= 0 ? Name.AsSpan().Slice(0, index).ToString() : Name;

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
                stringBuilder.Append(DeclaringType.ReflectionFullName)
                    .Append('.')
                    .Append(strippedName);
            }

            return stringBuilder.ToString().GetRealTypeName();
        }

        private IReadOnlyList<string> HandleFlagsEnum(ulong value)
        {
            var foundItems = new List<string>(Fields.Count);

            // Walk largest to smallest, this is in case we have a direct match early on.
            for (int i = Fields.Count - 1; i >= 0; i--)
            {
                var field = Fields[i];

                var fieldValue = field.LongDefaultValue;
                if (value == fieldValue)
                {
                    return new[] { field.Name };
                }

                if ((value & fieldValue) == fieldValue)
                {
                    foundItems.Add(field.Name);
                    value -= fieldValue;
                }
            }

            // If we exhausted looking through all the values and we still have
            // a non-zero result, we couldn't match the result to only named values.
            // In that case, we return null and let the call site just generate
            // a string for the integral value.
            if (value != 0)
            {
                return null;
            }

            return foundItems;
        }
    }
}
