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
    /// Wraps the <see cref="GenericParameterConstraint"/>.
    /// </summary>
    public class GenericParameterConstraintWrapper : IHandleWrapper
    {
        private static readonly ConcurrentDictionary<(GenericParameterConstraintHandle handle, AssemblyMetadata assemblyMetadata), GenericParameterConstraintWrapper> _registerTypes = new ConcurrentDictionary<(GenericParameterConstraintHandle handle, AssemblyMetadata assemblyMetadata), GenericParameterConstraintWrapper>();

        private readonly Lazy<IHandleTypeNamedWrapper> _type;

        private GenericParameterConstraintWrapper(GenericParameterConstraintHandle handle, GenericParameterWrapper parent, AssemblyMetadata assemblyMetadata)
        {
            GenericParameterConstraintHandle = handle;
            AssemblyMetadata = assemblyMetadata;
            Handle = handle;
            Parent = parent;
            Definition = Resolve();

            _type = new Lazy<IHandleTypeNamedWrapper>(() => WrapperFactory.Create(Definition.Type, AssemblyMetadata), LazyThreadSafetyMode.PublicationOnly);
        }

        /// <summary>
        /// Gets the resolved method definition.
        /// </summary>
        public GenericParameterConstraint Definition { get; }

        /// <summary>
        /// Gets the method definition handle.
        /// </summary>
        public GenericParameterConstraintHandle GenericParameterConstraintHandle { get; }

        /// <summary>
        /// Gets the type of the constraint.
        /// </summary>
        public IHandleTypeNamedWrapper Type => _type.Value;

        /// <summary>
        /// Gets the parent of the constraint.
        /// </summary>
        public GenericParameterWrapper Parent { get; }

        /// <inheritdoc />
        public AssemblyMetadata AssemblyMetadata { get; }

        /// <inheritdoc />
        public Handle Handle { get; }

        /// <summary>
        /// Creates a instance of the method, if there is already not an instance.
        /// </summary>
        /// <param name="handle">The handle to the instance.</param>
        /// <param name="parent">The parent of the constraint.</param>
        /// <param name="assemblyMetadata">The module that contains the instance.</param>
        /// <returns>The wrapper.</returns>
        public static GenericParameterConstraintWrapper Create(GenericParameterConstraintHandle handle, GenericParameterWrapper parent, AssemblyMetadata assemblyMetadata)
        {
            if (handle.IsNil)
            {
                return null;
            }

            return _registerTypes.GetOrAdd((handle, assemblyMetadata), data => new GenericParameterConstraintWrapper(data.handle, parent, data.assemblyMetadata));
        }

        /// <summary>
        /// Creates a array instances of a type.
        /// </summary>
        /// <param name="collection">The collection to create.</param>
        /// <param name="parent">The parent of the constraint.</param>
        /// <param name="assemblyMetadata">The module to use in creation.</param>
        /// <returns>The list of the type.</returns>
        public static IReadOnlyList<GenericParameterConstraintWrapper> Create(in GenericParameterConstraintHandleCollection collection, GenericParameterWrapper parent, AssemblyMetadata assemblyMetadata)
        {
            var output = new GenericParameterConstraintWrapper[collection.Count];

            int i = 0;
            foreach (var element in collection)
            {
                output[i] = Create(element, parent, assemblyMetadata);
                i++;
            }

            return output;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return "Constraint " + Type.FullName;
        }

        private GenericParameterConstraint Resolve()
        {
            return AssemblyMetadata.MetadataReader.GetGenericParameterConstraint(GenericParameterConstraintHandle);
        }
    }
}
