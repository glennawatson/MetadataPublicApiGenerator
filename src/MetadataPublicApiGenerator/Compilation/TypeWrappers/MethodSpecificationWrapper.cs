// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text;
using System.Threading;

using MetadataPublicApiGenerator.Extensions;

namespace MetadataPublicApiGenerator.Compilation.TypeWrappers
{
    internal class MethodSpecificationWrapper
    {
        private static readonly Dictionary<MethodSpecificationHandle, MethodSpecificationWrapper> _registerTypes = new Dictionary<MethodSpecificationHandle, MethodSpecificationWrapper>();

        private readonly Lazy<IReadOnlyList<ITypeNamedWrapper>> _signature;
        private readonly Lazy<MethodWrapper> _method;

        private MethodSpecificationWrapper(MethodSpecificationHandle handle, CompilationModule module)
        {
            Definition = Resolve(handle, module);
            MethodSpecificationHandle = handle;
            Module = module;

            _signature = new Lazy<IReadOnlyList<ITypeNamedWrapper>>(() => Definition.DecodeSignature(module.TypeProvider, new GenericContext(module, MethodSpecificationHandle)));
            _method = new Lazy<MethodWrapper>(() => MethodWrapper.Create((MethodDefinitionHandle)Definition.Method, module), LazyThreadSafetyMode.PublicationOnly);

            _registerTypes.TryAdd(handle, this);
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

        /// <summary>
        /// Gets the module that this method belongs to.
        /// </summary>
        public CompilationModule Module { get; }

        /// <summary>
        /// Creates a instance of the method, if there is already not an instance.
        /// </summary>
        /// <param name="handle">The handle to the instance.</param>
        /// <param name="module">The module that contains the instance.</param>
        /// <returns>The wrapper.</returns>
        public static MethodSpecificationWrapper Create(MethodSpecificationHandle handle, CompilationModule module)
        {
            return _registerTypes.GetOrAdd(handle, handleCreate => new MethodSpecificationWrapper(handleCreate, module));
        }

        private static MethodSpecification Resolve(MethodSpecificationHandle handle, CompilationModule compilation)
        {
            return compilation.MetadataReader.GetMethodSpecification(handle);
        }
    }
}
