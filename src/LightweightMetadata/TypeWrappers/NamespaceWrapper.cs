// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading;
using LightweightMetadata.Extensions;

namespace LightweightMetadata.TypeWrappers
{
    /// <summary>
    /// A wrapper around the NamespaceDefinition.
    /// </summary>
    public class NamespaceWrapper : IHandleNameWrapper
    {
        private static readonly ConcurrentDictionary<(NamespaceDefinitionHandle handle, CompilationModule module), NamespaceWrapper> _registeredNamespaces = new ConcurrentDictionary<(NamespaceDefinitionHandle handle, CompilationModule module), NamespaceWrapper>();

        private readonly Lazy<string> _fullName;

        private readonly Lazy<string> _name;

        private readonly Lazy<NamespaceWrapper> _parent;

        private readonly Lazy<IReadOnlyList<TypeWrapper>> _members;

        private readonly Lazy<IReadOnlyList<NamespaceWrapper>> _childNamespaces;

        internal NamespaceWrapper(NamespaceDefinition definition, CompilationModule module)
        {
            CompilationModule = module;
            Definition = definition;
            Handle = default;

            _parent = new Lazy<NamespaceWrapper>(() => null);
            _name = new Lazy<string>(() => GetName(Definition, module), LazyThreadSafetyMode.PublicationOnly);
            _fullName = new Lazy<string>(GetFullName, LazyThreadSafetyMode.PublicationOnly);
            _members = new Lazy<IReadOnlyList<TypeWrapper>>(() => Definition.TypeDefinitions.Select(x => TypeWrapper.Create(x, module)).ToList(), LazyThreadSafetyMode.PublicationOnly);
            _childNamespaces = new Lazy<IReadOnlyList<NamespaceWrapper>>(() => Definition.NamespaceDefinitions.Select(x => Create(x, module)).ToList(), LazyThreadSafetyMode.PublicationOnly);
        }

        private NamespaceWrapper(NamespaceDefinitionHandle handle, CompilationModule module)
        {
            CompilationModule = module;
            NamespaceHandle = handle;
            Handle = handle;
            Definition = Resolve(handle, module);

            _parent = new Lazy<NamespaceWrapper>(() => Create(Definition.Parent, module), LazyThreadSafetyMode.PublicationOnly);
            _name = new Lazy<string>(() => GetName(Definition, module), LazyThreadSafetyMode.PublicationOnly);
            _fullName = new Lazy<string>(GetFullName, LazyThreadSafetyMode.PublicationOnly);
            _members = new Lazy<IReadOnlyList<TypeWrapper>>(() => Definition.TypeDefinitions.Select(x => TypeWrapper.Create(x, module)).ToList(), LazyThreadSafetyMode.PublicationOnly);
            _childNamespaces = new Lazy<IReadOnlyList<NamespaceWrapper>>(() => Definition.NamespaceDefinitions.Select(x => Create(x, module)).ToList(), LazyThreadSafetyMode.PublicationOnly);
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
        public CompilationModule CompilationModule { get; }

        /// <summary>
        /// Gets the parent namespace.
        /// </summary>
        public NamespaceWrapper Parent => _parent.Value;

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
        /// <param name="module">The module hosting the handle.</param>
        /// <returns>A namespace wrapper or null if the handle is nil.</returns>
        public static NamespaceWrapper Create(NamespaceDefinitionHandle handle, CompilationModule module)
        {
            if (handle.IsNil)
            {
                return null;
            }

            return _registeredNamespaces.GetOrAdd((handle, module), data => new NamespaceWrapper(data.handle, data.module));
        }

        private static NamespaceDefinition Resolve(NamespaceDefinitionHandle handle, CompilationModule compilation)
        {
            return compilation.MetadataReader.GetNamespaceDefinition(handle);
        }

        private static string GetName(NamespaceDefinition handle, CompilationModule compilation)
        {
            return handle.Name.GetName(compilation);
        }

        private string GetFullName()
        {
            NamespaceWrapper current = this;

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
