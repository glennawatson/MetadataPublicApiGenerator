// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading;

namespace LightweightMetadata
{
    /// <summary>
    /// A wrapper around the <see cref="InterfaceImplementation" />.
    /// </summary>
    public class InterfaceImplementationWrapper : AbstractEnclosedTypeWrapper
    {
        private static readonly ConcurrentDictionary<(InterfaceImplementationHandle handle, AssemblyMetadata assemblyMetadata), InterfaceImplementationWrapper> _registerTypes = new ConcurrentDictionary<(InterfaceImplementationHandle handle, AssemblyMetadata assemblyMetadata), InterfaceImplementationWrapper>();

        private readonly Lazy<IReadOnlyList<AttributeWrapper>> _attributes;

        private InterfaceImplementationWrapper(InterfaceImplementationHandle handle, AssemblyMetadata assemblyMetadata)
            : base(WrapperFactory.Create(Resolve(assemblyMetadata, handle).Interface, assemblyMetadata))
        {
            InterfaceImplementationHandle = handle;
            Definition = Resolve(assemblyMetadata, handle);

            _attributes = new Lazy<IReadOnlyList<AttributeWrapper>>(() => AttributeWrapper.Create(Definition.GetCustomAttributes(), AssemblyMetadata), LazyThreadSafetyMode.PublicationOnly);
        }

        /// <summary>
        /// Gets the resolved method definition.
        /// </summary>
        public InterfaceImplementation Definition { get; }

        /// <summary>
        /// Gets the method definition handle.
        /// </summary>
        public InterfaceImplementationHandle InterfaceImplementationHandle { get; }

        /// <summary>
        /// Gets the attributes contained on the interface.
        /// </summary>
        public IReadOnlyCollection<AttributeWrapper> InterfaceAttributes => _attributes.Value;

        /// <summary>
        /// Creates a instance of the method, if there is already not an instance.
        /// </summary>
        /// <param name="handle">The handle to the instance.</param>
        /// <param name="assemblyMetadata">The module that contains the instance.</param>
        /// <returns>The wrapper.</returns>
        public static InterfaceImplementationWrapper Create(InterfaceImplementationHandle handle, AssemblyMetadata assemblyMetadata)
        {
            if (handle.IsNil)
            {
                return null;
            }

            return _registerTypes.GetOrAdd((handle, assemblyMetadata), data => new InterfaceImplementationWrapper(data.handle, data.assemblyMetadata));
        }

        /// <summary>
        /// Creates a array instances of a type.
        /// </summary>
        /// <param name="collection">The collection to create.</param>
        /// <param name="assemblyMetadata">The module to use in creation.</param>
        /// <returns>The list of the type.</returns>
        public static IReadOnlyList<InterfaceImplementationWrapper> Create(in InterfaceImplementationHandleCollection collection, AssemblyMetadata assemblyMetadata)
        {
            var output = new InterfaceImplementationWrapper[collection.Count];

            int i = 0;
            foreach (var element in collection)
            {
                output[i] = Create(element, assemblyMetadata);
                i++;
            }

            return output;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return FullName;
        }

        private static InterfaceImplementation Resolve(AssemblyMetadata assemblyMetadata, InterfaceImplementationHandle handle)
        {
            return assemblyMetadata.MetadataReader.GetInterfaceImplementation(handle);
        }
    }
}
