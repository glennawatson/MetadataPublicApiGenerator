// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Threading;

using LightweightMetadata.Extensions;
using LightweightMetadata.TypeWrappers;

namespace LightweightMetadata
{
    /// <summary>
    /// A wrapper around the MethodSpecification.
    /// </summary>
    public class MethodSpecificationWrapper : IHandleWrapper, IHasGenericParameters
    {
        private static readonly ConcurrentDictionary<(MethodSpecificationHandle handle, AssemblyMetadata module), MethodSpecificationWrapper> _registerTypes = new ConcurrentDictionary<(MethodSpecificationHandle handle, AssemblyMetadata module), MethodSpecificationWrapper>();

        private readonly Lazy<IReadOnlyList<ITypeNamedWrapper>> _signature;
        private readonly Lazy<MethodWrapper> _method;

        private MethodSpecificationWrapper(MethodSpecificationHandle handle, AssemblyMetadata module)
        {
            MethodSpecificationHandle = handle;
            CompilationModule = module;
            Handle = handle;
            Definition = Resolve(handle, module);

            _signature = new Lazy<IReadOnlyList<ITypeNamedWrapper>>(() => Definition.DecodeSignature(module.TypeProvider, new GenericContext(this)).ToList());
            _method = new Lazy<MethodWrapper>(() => MethodWrapper.Create((MethodDefinitionHandle)Definition.Method, module), LazyThreadSafetyMode.PublicationOnly);
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
        public AssemblyMetadata CompilationModule { get; }

        /// <inheritdoc />
        public IReadOnlyList<GenericParameterWrapper> GenericParameters => Method.GenericParameters;

        /// <summary>
        /// Creates a instance of the method, if there is already not an instance.
        /// </summary>
        /// <param name="handle">The handle to the instance.</param>
        /// <param name="module">The module that contains the instance.</param>
        /// <returns>The wrapper.</returns>
        public static MethodSpecificationWrapper Create(MethodSpecificationHandle handle, AssemblyMetadata module)
        {
            if (handle.IsNil)
            {
                return null;
            }

            return _registerTypes.GetOrAdd((handle, module), data => new MethodSpecificationWrapper(data.handle, data.module));
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return Method.FullName;
        }

        private static MethodSpecification Resolve(MethodSpecificationHandle handle, AssemblyMetadata compilation)
        {
            return compilation.MetadataReader.GetMethodSpecification(handle);
        }
    }
}
