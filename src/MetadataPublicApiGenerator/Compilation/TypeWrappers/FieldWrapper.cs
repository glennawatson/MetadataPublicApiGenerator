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

namespace MetadataPublicApiGenerator.Compilation.TypeWrappers
{
    internal class FieldWrapper : IHandleTypeNamedWrapper, IHasAttributes
    {
        private static readonly Dictionary<FieldDefinitionHandle, FieldWrapper> _registerTypes = new Dictionary<FieldDefinitionHandle, FieldWrapper>();

        private readonly Lazy<string> _name;

        private readonly Lazy<IReadOnlyList<AttributeWrapper>> _attributes;
        private readonly Lazy<TypeWrapper> _declaringType;
        private readonly Lazy<object> _defaultValue;
        private readonly Lazy<IHandleTypeNamedWrapper> _fieldType;

        private FieldWrapper(FieldDefinitionHandle handle, CompilationModule module)
        {
            FieldDefinitionHandle = handle;
            Module = module;
            Handle = handle;
            Definition = Resolve();

            _declaringType = new Lazy<TypeWrapper>(() => TypeWrapper.Create(Definition.GetDeclaringType(), Module), LazyThreadSafetyMode.PublicationOnly);

            _name = new Lazy<string>(() => Definition.Name.GetName(module), LazyThreadSafetyMode.PublicationOnly);
            _attributes = new Lazy<IReadOnlyList<AttributeWrapper>>(() => Definition.GetCustomAttributes().Select(x => AttributeWrapper.Create(x, module)).ToList(), LazyThreadSafetyMode.PublicationOnly);

            _defaultValue = new Lazy<object>(() => Definition.GetDefaultValue().ReadConstant(module));
            IsPublic = (Definition.Attributes & FieldAttributes.Public) != 0;
            IsStatic = (Definition.Attributes & FieldAttributes.Static) != 0;

            _fieldType = new Lazy<IHandleTypeNamedWrapper>(() => Definition.DecodeSignature(module.TypeProvider, new GenericContext(this)), LazyThreadSafetyMode.PublicationOnly);
        }

        /// <summary>
        /// Gets the resolved method definition.
        /// </summary>
        public FieldDefinition Definition { get; }

        /// <summary>
        /// Gets the method definition handle.
        /// </summary>
        public FieldDefinitionHandle FieldDefinitionHandle { get; }

        public IReadOnlyList<AttributeWrapper> Attributes => _attributes.Value;

        /// <inheritdoc />
        public string Name => _name.Value;

        /// <inheritdoc />
        public CompilationModule Module { get; }

        public Handle Handle { get; }

        /// <inheritdoc />
        public string FullName => DeclaringType.FullName + "." + Name;

        /// <inheritdoc />
        public string Namespace => DeclaringType.Namespace;

        /// <inheritdoc />
        public bool IsPublic { get; }

        /// <inheritdoc />
        public bool IsAbstract => false;

        public bool IsStatic { get; }

        public object DefaultValue => _defaultValue.Value;

        public TypeWrapper DeclaringType => _declaringType.Value;

        public IHandleTypeNamedWrapper FieldType => _fieldType.Value;

        /// <summary>
        /// Creates a instance of the method, if there is already not an instance.
        /// </summary>
        /// <param name="handle">The handle to the instance.</param>
        /// <param name="module">The module that contains the instance.</param>
        /// <returns>The wrapper.</returns>
        public static FieldWrapper Create(FieldDefinitionHandle handle, CompilationModule module)
        {
            if (handle.IsNil)
            {
                return null;
            }

            return _registerTypes.GetOrAdd(handle, handleCreate => new FieldWrapper(handleCreate, module));
        }

        private FieldDefinition Resolve()
        {
            return Module.MetadataReader.GetFieldDefinition(FieldDefinitionHandle);
        }
    }
}