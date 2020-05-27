﻿// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Threading;

namespace LightweightMetadata
{
    /// <summary>
    /// A wrapper around the MethodSpecification.
    /// </summary>
    public class MethodSpecificationWrapper : IHandleWrapper, IHasGenericParameters
    {
        private static readonly ConcurrentDictionary<(MethodSpecificationHandle Handle, AssemblyMetadata AssemblyMetadata), MethodSpecificationWrapper> _registerTypes = new ConcurrentDictionary<(MethodSpecificationHandle, AssemblyMetadata), MethodSpecificationWrapper>();

        private readonly Lazy<IReadOnlyList<ITypeNamedWrapper>> _signature;
        private readonly Lazy<MethodWrapper> _method;

        private MethodSpecificationWrapper(MethodSpecificationHandle handle, AssemblyMetadata assemblyMetadata)
        {
            MethodSpecificationHandle = handle;
            AssemblyMetadata = assemblyMetadata;
            Handle = handle;
            Definition = Resolve(handle, assemblyMetadata);

            _signature = new Lazy<IReadOnlyList<ITypeNamedWrapper>>(() => Definition.DecodeSignature(assemblyMetadata.TypeProvider, new GenericContext(this)).ToList());
            _method = new Lazy<MethodWrapper>(() => MethodWrapper.CreateChecked((MethodDefinitionHandle)Definition.Method, assemblyMetadata), LazyThreadSafetyMode.PublicationOnly);
        }

        /// <summary>
        /// Gets the resolved method definition.
        /// </summary>
        public MethodSpecification Definition { get; }

        /// <summary>
        /// Gets the method definition handle.
        /// </summary>
        public MethodSpecificationHandle MethodSpecificationHandle { get; }

        /// <summary>
        /// Gets the method for the specification.
        /// </summary>
        public MethodWrapper Method => _method.Value;

        /// <summary>
        /// Gets the types for the specification.
        /// </summary>
        public IReadOnlyList<ITypeNamedWrapper> Types => _signature.Value;

        /// <inheritdoc />
        public Handle Handle { get; }

        /// <summary>
        /// Gets the module that this method belongs to.
        /// </summary>
        public AssemblyMetadata AssemblyMetadata { get; }

        /// <inheritdoc />
        public IReadOnlyList<GenericParameterWrapper> GenericParameters => Method.GenericParameters;

        /// <summary>
        /// Creates a instance of the method, if there is already not an instance.
        /// </summary>
        /// <param name="handle">The handle to the instance.</param>
        /// <param name="assemblyMetadata">The module that contains the instance.</param>
        /// <returns>The wrapper.</returns>
        public static MethodSpecificationWrapper? Create(MethodSpecificationHandle handle, AssemblyMetadata assemblyMetadata)
        {
            if (handle.IsNil)
            {
                return null;
            }

            return _registerTypes.GetOrAdd((handle, assemblyMetadata), data => new MethodSpecificationWrapper(data.Handle, data.AssemblyMetadata));
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return Method.FullName;
        }

        private static MethodSpecification Resolve(MethodSpecificationHandle handle, AssemblyMetadata assemblyMetadata)
        {
            return assemblyMetadata.MetadataReader.GetMethodSpecification(handle);
        }
    }
}
