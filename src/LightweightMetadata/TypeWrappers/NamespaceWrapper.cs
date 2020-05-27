﻿// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading;

namespace LightweightMetadata
{
    /// <summary>
    /// A wrapper around the NamespaceDefinition.
    /// </summary>
    public class NamespaceWrapper : IHandleNameWrapper
    {
        private static readonly ConcurrentDictionary<(NamespaceDefinitionHandle Handle, AssemblyMetadata AssemblyMetadata), NamespaceWrapper> _registeredNamespaces = new ConcurrentDictionary<(NamespaceDefinitionHandle, AssemblyMetadata), NamespaceWrapper>();

        private readonly Lazy<string> _fullName;

        private readonly Lazy<string> _name;

        private readonly Lazy<NamespaceWrapper?> _parent;

        private readonly Lazy<IReadOnlyList<TypeWrapper>> _members;

        private readonly Lazy<IReadOnlyList<NamespaceWrapper>> _childNamespaces;

        internal NamespaceWrapper(NamespaceDefinition definition, AssemblyMetadata assemblyMetadata)
        {
            AssemblyMetadata = assemblyMetadata;
            Definition = definition;
            Handle = default;

            _parent = new Lazy<NamespaceWrapper?>(() => null);
            _name = new Lazy<string>(() => GetName(Definition, assemblyMetadata), LazyThreadSafetyMode.PublicationOnly);
            _fullName = new Lazy<string>(GetFullName, LazyThreadSafetyMode.PublicationOnly);
            _members = new Lazy<IReadOnlyList<TypeWrapper>>(() => TypeWrapper.CreateChecked(Definition.TypeDefinitions, assemblyMetadata), LazyThreadSafetyMode.PublicationOnly);
            _childNamespaces = new Lazy<IReadOnlyList<NamespaceWrapper>>(() => CreateChecked(Definition.NamespaceDefinitions, assemblyMetadata), LazyThreadSafetyMode.PublicationOnly);
        }

        private NamespaceWrapper(NamespaceDefinitionHandle handle, AssemblyMetadata assemblyMetadata)
        {
            AssemblyMetadata = assemblyMetadata;
            NamespaceHandle = handle;
            Handle = handle;
            Definition = Resolve(handle, assemblyMetadata);

            _parent = new Lazy<NamespaceWrapper?>(() => Create(Definition.Parent, assemblyMetadata), LazyThreadSafetyMode.PublicationOnly);
            _name = new Lazy<string>(() => GetName(Definition, assemblyMetadata), LazyThreadSafetyMode.PublicationOnly);
            _fullName = new Lazy<string>(GetFullName, LazyThreadSafetyMode.PublicationOnly);
            _members = new Lazy<IReadOnlyList<TypeWrapper>>(() => TypeWrapper.CreateChecked(Definition.TypeDefinitions, assemblyMetadata), LazyThreadSafetyMode.PublicationOnly);
            _childNamespaces = new Lazy<IReadOnlyList<NamespaceWrapper>>(() => CreateChecked(Definition.NamespaceDefinitions, assemblyMetadata), LazyThreadSafetyMode.PublicationOnly);
        }

        /// <summary>
        /// Gets the namespace definition.
        /// </summary>
        public NamespaceDefinition Definition { get; }

        /// <summary>
        /// Gets the handle to the namespace definition.
        /// </summary>
        public NamespaceDefinitionHandle NamespaceHandle { get; }

        /// <inheritdoc />
        public Handle Handle { get; }

        /// <inheritdoc />
        public AssemblyMetadata AssemblyMetadata { get; }

        /// <summary>
        /// Gets the parent namespace.
        /// </summary>
        public NamespaceWrapper? Parent => _parent.Value;

        /// <summary>
        /// Gets the full name of the namespace including any parents.
        /// </summary>
        public string FullName => _fullName.Value;

        /// <inheritdoc/>
        public string Name => _name.Value;

        /// <summary>
        /// Gets a list of members of the class.
        /// </summary>
        public IReadOnlyList<TypeWrapper> Members => _members.Value;

        /// <summary>
        /// Gets a list of child namespaces.
        /// </summary>
        public IReadOnlyList<NamespaceWrapper> ChildNamespaces => _childNamespaces.Value;

        /// <summary>
        /// Creates a new instance of the NamespaceWrapper class given a NamespaceDefinition.
        /// </summary>
        /// <param name="handle">The namespace definition to generate.</param>
        /// <param name="assemblyMetadata">The module hosting the handle.</param>
        /// <returns>A namespace wrapper or null if the handle is nil.</returns>
        public static NamespaceWrapper? Create(NamespaceDefinitionHandle handle, AssemblyMetadata assemblyMetadata)
        {
            if (handle.IsNil)
            {
                return null;
            }

            return _registeredNamespaces.GetOrAdd((handle, assemblyMetadata), data => new NamespaceWrapper(data.Handle, data.AssemblyMetadata));
        }

        /// <summary>
        /// Creates a new instance of the NamespaceWrapper class given a NamespaceDefinition.
        /// </summary>
        /// <param name="collection">The namespace definitions to generate.</param>
        /// <param name="assemblyMetadata">The module hosting the handle.</param>
        /// <returns>A namespace wrapper or null if the handle is nil.</returns>
        public static IReadOnlyList<NamespaceWrapper?> Create(in ImmutableArray<NamespaceDefinitionHandle> collection, AssemblyMetadata assemblyMetadata)
        {
            var output = new NamespaceWrapper?[collection.Length];

            int i = 0;
            foreach (var element in collection)
            {
                output[i] = Create(element, assemblyMetadata);
                i++;
            }

            return output;
        }

        /// <summary>
        /// Creates a array instances of a type.
        /// </summary>
        /// <param name="collection">The collection to create.</param>
        /// <param name="assemblyMetadata">The module to use in creation.</param>
        /// <returns>The list of the type.</returns>
        public static IReadOnlyList<NamespaceWrapper> CreateChecked(in ImmutableArray<NamespaceDefinitionHandle> collection, AssemblyMetadata assemblyMetadata)
        {
            var entities = Create(collection, assemblyMetadata);

            if (entities.Any(x => x is null))
            {
                throw new ArgumentException("Have invalid assembly references.", nameof(collection));
            }

            return entities.Select(x => x!).ToList();
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return FullName;
        }

        private static NamespaceDefinition Resolve(NamespaceDefinitionHandle handle, AssemblyMetadata assemblyMetadata)
        {
            return assemblyMetadata.MetadataReader.GetNamespaceDefinition(handle);
        }

        private static string GetName(NamespaceDefinition handle, AssemblyMetadata assemblyMetadata)
        {
            return handle.Name.GetName(assemblyMetadata);
        }

        private string GetFullName()
        {
            NamespaceWrapper? current = this;

            var names = new List<string>(10);
            while (current != null)
            {
                names.Add(current.Name);
                current = current.Parent;
            }

            names.Reverse();

            return string.Join(".", names);
        }
    }
}
