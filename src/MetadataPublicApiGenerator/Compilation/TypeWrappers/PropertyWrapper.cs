// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading;

using MetadataPublicApiGenerator.Extensions;

namespace MetadataPublicApiGenerator.Compilation.TypeWrappers
{
    internal class PropertyWrapper : IHandleTypeNamedWrapper, IHasAttributes
    {
        private static readonly Dictionary<PropertyDefinitionHandle, PropertyWrapper> _registerTypes = new Dictionary<PropertyDefinitionHandle, PropertyWrapper>();

        private readonly Lazy<string> _name;

        private readonly Lazy<IReadOnlyList<AttributeWrapper>> _attributes;
        private readonly Lazy<MethodWrapper> _getterMethod;
        private readonly Lazy<MethodWrapper> _setterMethod;
        private readonly Lazy<MethodWrapper> _anyAccessor;
        private readonly Lazy<TypeWrapper> _declaringType;
        private readonly Lazy<MethodSignature<IHandleTypeNamedWrapper>> _signature;

        private PropertyWrapper(PropertyDefinitionHandle handle, CompilationModule module)
        {
            PropertyDefinitionHandle = handle;
            Module = module;
            Handle = handle;
            Definition = Resolve();

            _name = new Lazy<string>(() => Definition.Name.GetName(module), LazyThreadSafetyMode.PublicationOnly);
            _attributes = new Lazy<IReadOnlyList<AttributeWrapper>>(() => Definition.GetCustomAttributes().Select(x => AttributeWrapper.Create(x, module)).ToList(), LazyThreadSafetyMode.PublicationOnly);

            _getterMethod = new Lazy<MethodWrapper>(() => MethodWrapper.Create(Definition.GetAccessors().Getter, module), LazyThreadSafetyMode.PublicationOnly);
            _setterMethod = new Lazy<MethodWrapper>(() => MethodWrapper.Create(Definition.GetAccessors().Setter, module), LazyThreadSafetyMode.PublicationOnly);

            _anyAccessor = new Lazy<MethodWrapper>(GetAnyAccessor, LazyThreadSafetyMode.PublicationOnly);

            _declaringType = new Lazy<TypeWrapper>(() => _anyAccessor.Value.DeclaringType, LazyThreadSafetyMode.PublicationOnly);

            _signature = new Lazy<MethodSignature<IHandleTypeNamedWrapper>>(() => Definition.DecodeSignature(module.TypeProvider, new GenericContext(this)), LazyThreadSafetyMode.PublicationOnly);
        }

        /// <summary>
        /// Gets the resolved method definition.
        /// </summary>
        public PropertyDefinition Definition { get; }

        /// <summary>
        /// Gets the method definition handle.
        /// </summary>
        public PropertyDefinitionHandle PropertyDefinitionHandle { get; }

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
        public bool IsPublic => AnyAccessor.IsPublic;

        /// <inheritdoc />
        public bool IsAbstract => Getter.IsAbstract || Setter.IsAbstract;

        public MethodWrapper Getter => _getterMethod.Value;

        public MethodWrapper Setter => _setterMethod.Value;

        public MethodWrapper AnyAccessor => _anyAccessor.Value;

        public TypeWrapper DeclaringType => _declaringType.Value;

        public IHandleTypeNamedWrapper ReturnType => _signature.Value.ReturnType;

        /// <summary>
        /// Creates a instance of the method, if there is already not an instance.
        /// </summary>
        /// <param name="handle">The handle to the instance.</param>
        /// <param name="module">The module that contains the instance.</param>
        /// <returns>The wrapper.</returns>
        public static PropertyWrapper Create(PropertyDefinitionHandle handle, CompilationModule module)
        {
            if (handle.IsNil)
            {
                return null;
            }

            return _registerTypes.GetOrAdd(handle, handleCreate => new PropertyWrapper(handleCreate, module));
        }

        private PropertyDefinition Resolve()
        {
            return Module.MetadataReader.GetPropertyDefinition(PropertyDefinitionHandle);
        }

        private MethodWrapper GetAnyAccessor()
        {
            if (Getter != null)
            {
                return Getter;
            }

            return Setter;
        }
    }
}
