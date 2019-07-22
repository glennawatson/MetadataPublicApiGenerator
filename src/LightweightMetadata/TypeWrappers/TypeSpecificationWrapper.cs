// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Threading;
using LightweightMetadata.Extensions;

namespace LightweightMetadata.TypeWrappers
{
    /// <summary>
    /// A wrapper around the <see cref="TypeSpecification"/>.
    /// </summary>
    public class TypeSpecificationWrapper : IHandleTypeNamedWrapper, IHasAttributes
    {
        private static readonly ConcurrentDictionary<(TypeSpecificationHandle handle, CompilationModule module), TypeSpecificationWrapper> _registerTypes = new ConcurrentDictionary<(TypeSpecificationHandle handle, CompilationModule module), TypeSpecificationWrapper>();

        private readonly Lazy<IReadOnlyList<AttributeWrapper>> _attributes;
        private readonly Lazy<IHandleTypeNamedWrapper> _type;

        private TypeSpecificationWrapper(TypeSpecificationHandle handle, CompilationModule module)
        {
            TypeSpecificationHandle = handle;
            CompilationModule = module;
            Handle = handle;
            Definition = Resolve();

            _attributes = new Lazy<IReadOnlyList<AttributeWrapper>>(() => AttributeWrapper.Create(Definition.GetCustomAttributes(), module), LazyThreadSafetyMode.PublicationOnly);

            _type = new Lazy<IHandleTypeNamedWrapper>(() => Definition.DecodeSignature(module.TypeProvider, new GenericContext(this)), LazyThreadSafetyMode.PublicationOnly);
        }

        /// <summary>
        /// Gets the resolved method definition.
        /// </summary>
        public TypeSpecification Definition { get; }

        /// <summary>
        /// Gets the method definition handle.
        /// </summary>
        public TypeSpecificationHandle TypeSpecificationHandle { get; }

        /// <inheritdoc />
        public string Name => Type.Name;

        /// <inheritdoc />
        public CompilationModule CompilationModule { get; }

        /// <inheritdoc/>
        public Handle Handle { get; }

        /// <inheritdoc/>
        public IReadOnlyList<AttributeWrapper> Attributes => _attributes.Value;

        /// <summary>
        /// Gets the type that this specification represents.
        /// </summary>
        public IHandleTypeNamedWrapper Type => _type.Value;

        /// <inheritdoc />
        public string FullName => Type.FullName;

        /// <inheritdoc />
        public string ReflectionFullName => Type.ReflectionFullName;

        /// <inheritdoc />
        public string TypeNamespace => Type.TypeNamespace;

        /// <inheritdoc />
        public EntityAccessibility Accessibility => Type.Accessibility;

        /// <inheritdoc />
        public bool IsAbstract => Type.IsAbstract;

        /// <inheritdoc />
        public KnownTypeCode KnownType => Type.KnownType;

        /// <summary>
        /// Creates a instance of the method, if there is already not an instance.
        /// </summary>
        /// <param name="handle">The handle to the instance.</param>
        /// <param name="module">The module that contains the instance.</param>
        /// <returns>The wrapper.</returns>
        public static TypeSpecificationWrapper Create(TypeSpecificationHandle handle, CompilationModule module)
        {
            if (handle.IsNil)
            {
                return null;
            }

            return _registerTypes.GetOrAdd((handle, module), data => new TypeSpecificationWrapper(data.handle, data.module));
        }

        private TypeSpecification Resolve()
        {
            return CompilationModule.MetadataReader.GetTypeSpecification(TypeSpecificationHandle);
        }
    }
}
