// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Threading;

using MetadataPublicApiGenerator.Extensions;

using Microsoft.CodeAnalysis;

namespace MetadataPublicApiGenerator.Compilation.TypeWrappers
{
    /// <summary>
    /// A wrapper around a type.
    /// </summary>
    internal class TypeWrapper : ITypeWrapper
    {
        private static readonly IDictionary<TypeDefinitionHandle, ITypeWrapper> _types = new Dictionary<TypeDefinitionHandle, ITypeWrapper>();

        private readonly Lazy<TypeDefinition> _typeDefinition;

        private readonly Lazy<string> _name;

        private readonly Lazy<string> _namespace;

        private readonly Lazy<string> _fullName;

        private readonly Lazy<bool> _isKnownType;

        private readonly Lazy<bool> _isEnumType;

        private readonly Lazy<bool> _isPublic;

        private readonly Lazy<bool> _isAbstract;

        private readonly Lazy<ITypeNamedWrapper> _base;

        private readonly Lazy<TypeKind> _typeKind;

        private readonly Lazy<IReadOnlyList<AttributeWrapper>> _attributes;

        private readonly Lazy<IReadOnlyDictionary<string, IReadOnlyList<string>>> _constraints;

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

            _typeDefinition = new Lazy<TypeDefinition>(() => ((TypeDefinitionHandle)Handle).Resolve(Module), LazyThreadSafetyMode.PublicationOnly);
            _name = new Lazy<string>(() => GetName(TypeDefinition, Module), LazyThreadSafetyMode.PublicationOnly);
            _namespace = new Lazy<string>(() => TypeDefinition.Namespace.GetName(Module), LazyThreadSafetyMode.PublicationOnly);
            _fullName = new Lazy<string>(() => TypeDefinition.GetFullName(Module), LazyThreadSafetyMode.PublicationOnly);
            _isKnownType = new Lazy<bool>(() => TypeDefinition.IsKnownType(Module) != KnownTypeCode.None, LazyThreadSafetyMode.PublicationOnly);
            _isEnumType = new Lazy<bool>(IsEnum, LazyThreadSafetyMode.PublicationOnly);
            _isAbstract = new Lazy<bool>(() => (TypeDefinition.Attributes & TypeAttributes.Abstract) != 0, LazyThreadSafetyMode.PublicationOnly);
            _typeKind = new Lazy<TypeKind>(GetTypeKind, LazyThreadSafetyMode.PublicationOnly);
            _attributes = new Lazy<IReadOnlyList<AttributeWrapper>>(() => TypeDefinition.GetCustomAttributes().Select(x => AttributeWrapper.Create(x, Module)).ToList(), LazyThreadSafetyMode.PublicationOnly);
            _constraints = new Lazy<IReadOnlyDictionary<string, IReadOnlyList<string>>>(GetConstraints, LazyThreadSafetyMode.PublicationOnly);

            _base = new Lazy<ITypeNamedWrapper>(
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
        }

        /// <summary>
        /// Gets the type definition for the type.
        /// </summary>
        public TypeDefinition TypeDefinition => _typeDefinition.Value;

        /// <inheritdoc />
        public virtual string Name => _name.Value;

        /// <inheritdoc />
        public string Namespace => _namespace.Value;

        /// <inheritdoc />
        public string FullName => _fullName.Value;

        /// <inheritdoc />
        public virtual bool IsKnownType => _isKnownType.Value;

        /// <inheritdoc />
        public virtual bool IsEnumType => _isEnumType.Value;

        /// <inheritdoc />
        public bool IsAbstract => _isAbstract.Value;

        /// <inheritdoc />
        public bool IsPublic => _isPublic.Value;

        /// <inheritdoc />
        public Handle Handle { get; }

        /// <inheritdoc />
        public ITypeNamedWrapper Base => _base.Value;

        /// <inheritdoc />
        public TypeKind TypeKind => _typeKind.Value;

        public IReadOnlyList<AttributeWrapper> Attributes => _attributes.Value;

        public IReadOnlyDictionary<string, IReadOnlyList<string>> Constraints => _constraints.Value;

        /// <inheritdoc />
        public CompilationModule Module { get; }

        /// <summary>
        /// Gets the type definition handle.
        /// </summary>
        public TypeDefinitionHandle TypeDefinitionHandle { get; }

        public static ITypeWrapper Create(TypeDefinitionHandle handle, CompilationModule module)
        {
            return _types.GetOrAdd(handle, createHandle => new TypeWrapper(module, createHandle));
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return FullName;
        }

        public bool IsValueType()
        {
            Handle baseType = GetBaseTypeOrNil();
            if (baseType.IsNil)
            {
                return false;
            }

            if (baseType.IsKnownType(Module) == KnownTypeCode.Enum)
            {
                return true;
            }

            if (baseType.IsKnownType(Module) != KnownTypeCode.ValueType)
            {
                return false;
            }

            return false;
        }

        public bool IsDelegate()
        {
            Handle baseType = GetBaseTypeOrNil();
            var knownType = baseType.IsKnownType(Module);
            return !baseType.IsNil && knownType == KnownTypeCode.MulticastDelegate;
        }

        public bool IsEnum()
        {
            var baseType = GetBaseTypeOrNil();
            if (baseType.IsNil)
            {
                return false;
            }

            if (baseType.Kind == HandleKind.TypeDefinition)
            {
                return ((TypeDefinitionHandle)baseType).IsKnownType(Module) == KnownTypeCode.Enum;
            }

            return false;
        }

        public bool TryGetEnumType(out PrimitiveTypeCode underlyingType)
        {
            underlyingType = 0;
            Handle baseType = GetBaseTypeOrNil();
            if (baseType.IsNil)
            {
                return false;
            }

            if (baseType.IsKnownType(Module) != KnownTypeCode.Enum)
            {
                return false;
            }

            var field = Module.MetadataReader.GetFieldDefinition(TypeDefinition.GetFields().First());
            var blob = Module.MetadataReader.GetBlobReader(field.Signature);
            if (blob.ReadSignatureHeader().Kind != SignatureKind.Field)
            {
                return false;
            }

            underlyingType = (PrimitiveTypeCode)blob.ReadByte();
            return true;
        }

        private static string GetName(TypeDefinition handle, CompilationModule compilation)
        {
            return handle.Name.GetName(compilation);
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

        private IReadOnlyDictionary<string, IReadOnlyList<string>> GetConstraints()
        {
            var constraintDictionary = new Dictionary<string, ISet<string>>();

            foreach (var typeParameterHandle in TypeDefinition.GetGenericParameters())
            {
                var typeParameter = typeParameterHandle.Resolve(Module);
                foreach (var constraint in typeParameter.GetConstraints().Select(x => x.Resolve(Module)))
                {
                    var parameter = constraint.Parameter.Resolve(Module);
                    var parameterName = parameter.Name.GetName(Module);

                    if (constraint.Type.IsNil)
                    {
                        continue;
                    }

                    var constraintType = WrapperFactory.Create(constraint.Type, Module);
                    if (constraintType.FullName != "System.Object")
                    {
                        if (!constraintDictionary.TryGetValue(parameterName, out var constraints))
                        {
                            constraints = new HashSet<string>();
                            constraintDictionary[parameterName] = constraints;
                        }

                        constraints.Add(constraintType.FullName);
                    }
                }
            }

            return constraintDictionary.ToDictionary(x => x.Key, x => (IReadOnlyList<string>)x.Value.ToList());
        }
    }
}
