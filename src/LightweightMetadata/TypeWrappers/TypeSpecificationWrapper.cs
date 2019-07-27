// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Threading;

using LightweightMetadata.TypeWrappers;

namespace LightweightMetadata
{
    /// <summary>
    /// A wrapper around the <see cref="TypeSpecification"/>.
    /// </summary>
    public class TypeSpecificationWrapper : IHandleTypeNamedWrapper, IHasAttributes, IHasGenericParameters
    {
        private static readonly ConcurrentDictionary<(TypeSpecificationHandle handle, AssemblyMetadata module), TypeSpecificationWrapper> _registerTypes = new ConcurrentDictionary<(TypeSpecificationHandle handle, AssemblyMetadata module), TypeSpecificationWrapper>();

        private readonly Lazy<IReadOnlyList<AttributeWrapper>> _attributes;
        private readonly Lazy<IHandleTypeNamedWrapper> _type;

        private TypeSpecificationWrapper(TypeSpecificationHandle handle, AssemblyMetadata module)
        {
            TypeSpecificationHandle = handle;
            AssemblyMetadata = module;
            Handle = handle;
            Definition = Resolve();

            _attributes = new Lazy<IReadOnlyList<AttributeWrapper>>(() => AttributeWrapper.Create(Definition.GetCustomAttributes(), module), LazyThreadSafetyMode.PublicationOnly);

            _type = new Lazy<IHandleTypeNamedWrapper>(GetHandleType, LazyThreadSafetyMode.PublicationOnly);
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
        public AssemblyMetadata AssemblyMetadata { get; }

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

        /// <inheritdoc />
        public IReadOnlyList<GenericParameterWrapper> GenericParameters => Type is IHasGenericParameters parameters ? parameters.GenericParameters : Array.Empty<GenericParameterWrapper>();

        /// <summary>
        /// Creates a instance of the method, if there is already not an instance.
        /// </summary>
        /// <param name="handle">The handle to the instance.</param>
        /// <param name="module">The module that contains the instance.</param>
        /// <returns>The wrapper.</returns>
        public static TypeSpecificationWrapper Create(TypeSpecificationHandle handle, AssemblyMetadata module)
        {
            if (handle.IsNil)
            {
                return null;
            }

            return _registerTypes.GetOrAdd((handle, module), data => new TypeSpecificationWrapper(data.handle, data.module));
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return FullName;
        }

        private TypeSpecification Resolve()
        {
            return AssemblyMetadata.MetadataReader.GetTypeSpecification(TypeSpecificationHandle);
        }

        private IHandleTypeNamedWrapper GetHandleType()
        {
            return Definition.DecodeSignature(new TypeSpecificationSignatureDecoder(AssemblyMetadata.Compilation), TypeSpecificationSignatureDecoder.Unit.Default);
        }
    }
}
