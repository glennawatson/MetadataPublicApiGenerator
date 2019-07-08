// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text;
using System.Threading;

using MetadataPublicApiGenerator.Extensions;

using Microsoft.CodeAnalysis;

namespace MetadataPublicApiGenerator.Compilation.TypeWrappers
{
    /// <summary>
    /// A wrapper around a type.
    /// </summary>
    internal class TypeWrapper : IHandleTypeNamedWrapper, IHasAttributes
    {
        private static readonly IDictionary<TypeDefinitionHandle, TypeWrapper> _types = new Dictionary<TypeDefinitionHandle, TypeWrapper>();

        private readonly Lazy<string> _name;
        private readonly Lazy<string> _namespace;
        private readonly Lazy<string> _fullName;
        private readonly Lazy<bool> _isKnownType;
        private readonly Lazy<bool> _isEnumType;
        private readonly Lazy<bool> _isPublic;
        private readonly Lazy<bool> _isAbstract;
        private readonly Lazy<IHandleTypeNamedWrapper> _base;
        private readonly Lazy<TypeKind> _typeKind;
        private readonly Lazy<IReadOnlyList<AttributeWrapper>> _attributes;
        private readonly Lazy<IReadOnlyList<TypeParameterWrapper>> _genericParameters;
        private readonly Lazy<IReadOnlyList<MethodWrapper>> _methods;
        private readonly Lazy<IReadOnlyList<PropertyWrapper>> _properties;
        private readonly Lazy<IReadOnlyList<FieldWrapper>> _fields;
        private readonly Lazy<IReadOnlyList<EventWrapper>> _events;
        private readonly Lazy<IReadOnlyList<TypeWrapper>> _nestedTypes;
        private readonly Lazy<TypeWrapper> _declaringType;
        private readonly Lazy<string> _fullGenericName;

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeWrapper"/> class.
        /// </summary>
        /// <param name="module">The main compilation module.</param>
        /// <param name="typeDefinition">The type definition we are wrapping.</param>
        private TypeWrapper(CompilationModule module, TypeDefinitionHandle typeDefinition)
        {
            Module = module ?? throw new ArgumentNullException(nameof(module));
            TypeDefinitionHandle = typeDefinition;
            Handle = typeDefinition;
            TypeDefinition = Module.MetadataReader.GetTypeDefinition(typeDefinition);

            _name = new Lazy<string>(() => Module.MetadataReader.GetString(TypeDefinition.Name), LazyThreadSafetyMode.PublicationOnly);
            _namespace = new Lazy<string>(() => TypeDefinition.Namespace.GetName(Module), LazyThreadSafetyMode.PublicationOnly);
            _fullName = new Lazy<string>(() => GetFullName(this), LazyThreadSafetyMode.PublicationOnly);
            _fullGenericName = new Lazy<string>(GenerateFullGenericName, LazyThreadSafetyMode.PublicationOnly);
            _isKnownType = new Lazy<bool>(() => this.IsKnownType() != KnownTypeCode.None, LazyThreadSafetyMode.PublicationOnly);
            _isEnumType = new Lazy<bool>(IsEnum, LazyThreadSafetyMode.PublicationOnly);
            _isAbstract = new Lazy<bool>(() => (TypeDefinition.Attributes & TypeAttributes.Abstract) != 0, LazyThreadSafetyMode.PublicationOnly);
            _typeKind = new Lazy<TypeKind>(GetTypeKind, LazyThreadSafetyMode.PublicationOnly);
            _attributes = new Lazy<IReadOnlyList<AttributeWrapper>>(() => TypeDefinition.GetCustomAttributes().Select(x => AttributeWrapper.Create(x, Module)).ToList(), LazyThreadSafetyMode.PublicationOnly);
            _genericParameters = new Lazy<IReadOnlyList<TypeParameterWrapper>>(() => TypeParameterWrapper.Create(Module, TypeDefinitionHandle, TypeDefinition.GetGenericParameters()), LazyThreadSafetyMode.PublicationOnly);
            _declaringType = new Lazy<TypeWrapper>(() => TypeWrapper.Create(TypeDefinition.GetDeclaringType(), Module));
            _methods = new Lazy<IReadOnlyList<MethodWrapper>>(() => TypeDefinition.GetMethods().Select(x => MethodWrapper.Create(x, Module)).ToList(), LazyThreadSafetyMode.PublicationOnly);
            _properties = new Lazy<IReadOnlyList<PropertyWrapper>>(() => TypeDefinition.GetProperties().Select(x => PropertyWrapper.Create(x, Module)).ToList(), LazyThreadSafetyMode.PublicationOnly);
            _fields = new Lazy<IReadOnlyList<FieldWrapper>>(() => TypeDefinition.GetFields().Select(x => FieldWrapper.Create(x, Module)).ToList(), LazyThreadSafetyMode.PublicationOnly);
            _events = new Lazy<IReadOnlyList<EventWrapper>>(() => TypeDefinition.GetEvents().Select(x => EventWrapper.Create(x, Module)).ToList(), LazyThreadSafetyMode.PublicationOnly);
            _nestedTypes = new Lazy<IReadOnlyList<TypeWrapper>>(() => TypeDefinition.GetNestedTypes().Select(x => TypeWrapper.Create(x, Module)).ToList(), LazyThreadSafetyMode.PublicationOnly);

            _base = new Lazy<IHandleTypeNamedWrapper>(
                () =>
                    {
                        var baseType = GetBaseTypeOrNil();

                        if (baseType.IsNil)
                        {
                            return null;
                        }

                        return WrapperFactory.Create(baseType, Module);
                    }, LazyThreadSafetyMode.PublicationOnly);

            _isPublic = new Lazy<bool>(() => (TypeDefinition.Attributes & TypeAttributes.Public) != 0, LazyThreadSafetyMode.PublicationOnly);

            IsStatic = (TypeDefinition.Attributes & (TypeAttributes.Abstract | TypeAttributes.Sealed)) == (TypeAttributes.Abstract | TypeAttributes.Sealed);
            IsSealed = (TypeDefinition.Attributes & TypeAttributes.Sealed) != 0;
        }

        /// <summary>
        /// Gets the type definition for the type.
        /// </summary>
        public TypeDefinition TypeDefinition { get; }

        public IReadOnlyList<EventWrapper> Events => _events.Value;

        public IReadOnlyList<FieldWrapper> Fields => _fields.Value;

        public IReadOnlyList<MethodWrapper> Methods => _methods.Value;

        public IReadOnlyList<PropertyWrapper> Properties => _properties.Value;

        public IReadOnlyList<TypeWrapper> NestedTypes => _nestedTypes.Value;

        /// <inheritdoc />
        public virtual string Name => _name.Value;

        /// <inheritdoc />
        public string Namespace => _namespace.Value;

        /// <inheritdoc />
        public string FullName => _fullName.Value;

        public virtual bool IsKnownType => _isKnownType.Value;

        public virtual bool IsEnumType => _isEnumType.Value;

        /// <inheritdoc />
        public bool IsAbstract => _isAbstract.Value;

        /// <inheritdoc />
        public bool IsPublic => _isPublic.Value;

        public bool IsStatic { get; }

        public bool IsSealed { get; }

        /// <inheritdoc />
        public Handle Handle { get; }

        public IHandleTypeNamedWrapper Base => _base.Value;

        public TypeKind TypeKind => _typeKind.Value;

        public TypeWrapper DeclaringType => _declaringType.Value;

        public IReadOnlyList<AttributeWrapper> Attributes => _attributes.Value;

        public IReadOnlyList<TypeParameterWrapper> GenericParameters => _genericParameters.Value;

        public string FullGenericName => _fullGenericName.Value;

        /// <inheritdoc />
        public CompilationModule Module { get; }

        /// <summary>
        /// Gets the type definition handle.
        /// </summary>
        public TypeDefinitionHandle TypeDefinitionHandle { get; }

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

        public bool TryGetEnumType(out IHandleTypeNamedWrapper underlyingType)
        {
            var baseType = Base;
            underlyingType = null;
            if (baseType == null)
            {
                return false;
            }

            if (baseType.IsKnownType() != KnownTypeCode.Enum)
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

        private bool IsValueType()
        {
            var baseType = Base;
            if (baseType == null)
            {
                return false;
            }

            if (baseType.IsKnownType() == KnownTypeCode.Enum)
            {
                return true;
            }

            if (baseType.IsKnownType() != KnownTypeCode.ValueType)
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

            var knownType = baseType.IsKnownType();
            return knownType == KnownTypeCode.MulticastDelegate;
        }

        private bool IsEnum()
        {
            var baseType = Base;
            if (baseType == null)
            {
                return false;
            }

            return baseType.IsKnownType() == KnownTypeCode.Enum;
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

        private TypeKind GetTypeKind()
        {
            var attributes = TypeDefinition.Attributes;

            if ((attributes & TypeAttributes.ClassSemanticsMask) == TypeAttributes.Interface)
            {
                return TypeKind.Interface;
            }

            if (IsEnumType)
            {
                return TypeKind.Enum;
            }

            if (IsValueType())
            {
                return TypeKind.Struct;
            }

            if (IsDelegate())
            {
                return TypeKind.Delegate;
            }

            return TypeKind.Class;
        }

        private string GetFullName(TypeWrapper typeWrapper)
        {
            var reader = Module.MetadataReader;

            var declaringType = typeWrapper.DeclaringType;

            var stringBuilder = new StringBuilder();
            if (declaringType == null)
            {
                if (!string.IsNullOrWhiteSpace(typeWrapper.Namespace))
                {
                        stringBuilder.Append(typeWrapper.Namespace).Append('.');
                }

                stringBuilder.Append(typeWrapper.Name);
            }
            else
            {
                stringBuilder.Append(GetFullName(declaringType)).Append('.')
                    .Append(typeWrapper.Name);
            }

            return stringBuilder.ToString();
        }

        /// <summary>
        /// Gets a string form of the type and generic arguments for a type.
        /// </summary>
        /// <returns>A type descriptor including the generic arguments.</returns>
        private string GenerateFullGenericName()
        {
            var sb = new StringBuilder(this.GetRealTypeName());

            if (GenericParameters.Count > 0)
            {
                sb.Append("<")
                    .Append(string.Join(", ", GenericParameters.Select(x => x.Name)))
                    .Append(">");
            }

            return sb.ToString();
        }
    }
}
