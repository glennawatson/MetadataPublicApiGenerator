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
    public class InterfaceImplementationWrapper : IHandleTypeNamedWrapper, IHasAttributes, IHasGenericParameters
    {
        private static readonly ConcurrentDictionary<InterfaceImplementationHandle, InterfaceImplementationWrapper> _registerTypes = new ConcurrentDictionary<InterfaceImplementationHandle, InterfaceImplementationWrapper>();

        private readonly Lazy<IReadOnlyList<AttributeWrapper>> _attributes;
        private readonly Lazy<IHandleTypeNamedWrapper> _interface;

        private InterfaceImplementationWrapper(InterfaceImplementationHandle handle, AssemblyMetadata module)
        {
            InterfaceImplementationHandle = handle;
            CompilationModule = module;
            Handle = handle;
            Definition = Resolve();

            _attributes = new Lazy<IReadOnlyList<AttributeWrapper>>(() => Definition.GetCustomAttributes().Select(x => AttributeWrapper.Create(x, CompilationModule)).ToList(), LazyThreadSafetyMode.PublicationOnly);
            _interface = new Lazy<IHandleTypeNamedWrapper>(() => WrapperFactory.Create(Definition.Interface, CompilationModule), LazyThreadSafetyMode.PublicationOnly);
        }

        /// <summary>
        /// Gets the resolved method definition.
        /// </summary>
        public InterfaceImplementation Definition { get; }

        /// <summary>
        /// Gets the method definition handle.
        /// </summary>
        public InterfaceImplementationHandle InterfaceImplementationHandle { get; }

        /// <inheritdoc />
        public string Name => Interface.Name;

        /// <inheritdoc />
        public AssemblyMetadata CompilationModule { get; }

        /// <inheritdoc/>
        public Handle Handle { get; }

        /// <inheritdoc/>
        public IReadOnlyList<AttributeWrapper> Attributes => _attributes.Value;

        /// <summary>
        /// Gets the type that this specification represents.
        /// </summary>
        public IHandleTypeNamedWrapper Interface => _interface.Value;

        /// <inheritdoc />
        public string FullName => Interface.FullName;

        /// <inheritdoc />
        public string ReflectionFullName => Interface.ReflectionFullName;

        /// <inheritdoc />
        public string TypeNamespace => Interface.TypeNamespace;

        /// <inheritdoc />
        public EntityAccessibility Accessibility => Interface.Accessibility;

        /// <inheritdoc />
        public bool IsAbstract => Interface.IsAbstract;

        /// <inheritdoc />
        public KnownTypeCode KnownType => Interface.KnownType;

        /// <inheritdoc />
        public IReadOnlyList<GenericParameterWrapper> GenericParameters => Interface is IHasGenericParameters parameters ? parameters.GenericParameters : Array.Empty<GenericParameterWrapper>();

        /// <summary>
        /// Creates a instance of the method, if there is already not an instance.
        /// </summary>
        /// <param name="handle">The handle to the instance.</param>
        /// <param name="module">The module that contains the instance.</param>
        /// <returns>The wrapper.</returns>
        public static InterfaceImplementationWrapper Create(InterfaceImplementationHandle handle, AssemblyMetadata module)
        {
            if (handle.IsNil)
            {
                return null;
            }

            return _registerTypes.GetOrAdd(handle, handleCreate => new InterfaceImplementationWrapper(handleCreate, module));
        }

        /// <summary>
        /// Creates a array instances of a type.
        /// </summary>
        /// <param name="collection">The collection to create.</param>
        /// <param name="module">The module to use in creation.</param>
        /// <returns>The list of the type.</returns>
        public static IReadOnlyList<InterfaceImplementationWrapper> Create(in InterfaceImplementationHandleCollection collection, AssemblyMetadata module)
        {
            var output = new InterfaceImplementationWrapper[collection.Count];

            int i = 0;
            foreach (var element in collection)
            {
                output[i] = Create(element, module);
                i++;
            }

            return output;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return FullName;
        }

        private InterfaceImplementation Resolve()
        {
            return CompilationModule.MetadataReader.GetInterfaceImplementation(InterfaceImplementationHandle);
        }
    }
}
