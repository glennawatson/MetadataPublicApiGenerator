// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading;

using MetadataPublicApiGenerator.Extensions;

namespace MetadataPublicApiGenerator.Compilation.TypeWrappers
{
    internal class NamespaceWrapper
    {
        private static readonly Dictionary<NamespaceDefinitionHandle, NamespaceWrapper> _registeredNamespaces = new Dictionary<NamespaceDefinitionHandle, NamespaceWrapper>();

        private readonly Lazy<string> _fullName;

        private readonly Lazy<string> _name;

        private readonly Lazy<NamespaceWrapper> _parent;

        private readonly Lazy<IReadOnlyList<TypeWrapper>> _members;

        private readonly Lazy<IReadOnlyList<NamespaceWrapper>> _childNamespaces;

        internal NamespaceWrapper(NamespaceDefinition definition, CompilationModule module)
        {
            Definition = definition;

            _parent = new Lazy<NamespaceWrapper>(() => null);
            _name = new Lazy<string>(() => GetName(Definition, module), LazyThreadSafetyMode.PublicationOnly);
            _fullName = new Lazy<string>(GetFullName, LazyThreadSafetyMode.PublicationOnly);
            _members = new Lazy<IReadOnlyList<TypeWrapper>>(() => Definition.TypeDefinitions.Select(x => TypeWrapper.Create(x, module)).ToList(), LazyThreadSafetyMode.PublicationOnly);
            _childNamespaces = new Lazy<IReadOnlyList<NamespaceWrapper>>(() => Definition.NamespaceDefinitions.Select(x => Create(x, module)).ToList(), LazyThreadSafetyMode.PublicationOnly);
        }

        private NamespaceWrapper(NamespaceDefinitionHandle handle, CompilationModule module)
        {
            NamespaceHandle = handle;
            Definition = Resolve(handle, module);

            _parent = new Lazy<NamespaceWrapper>(() => GetParent(Definition.Parent, module), LazyThreadSafetyMode.PublicationOnly);
            _name = new Lazy<string>(() => GetName(Definition, module), LazyThreadSafetyMode.PublicationOnly);
            _fullName = new Lazy<string>(GetFullName, LazyThreadSafetyMode.PublicationOnly);
            _members = new Lazy<IReadOnlyList<TypeWrapper>>(() => Definition.TypeDefinitions.Select(x => TypeWrapper.Create(x, module)).ToList(), LazyThreadSafetyMode.PublicationOnly);
            _childNamespaces = new Lazy<IReadOnlyList<NamespaceWrapper>>(() => Definition.NamespaceDefinitions.Select(x => Create(x, module)).ToList(), LazyThreadSafetyMode.PublicationOnly);
        }

        public NamespaceDefinition Definition { get; }

        public NamespaceDefinitionHandle NamespaceHandle { get; }

        public NamespaceWrapper Parent => _parent.Value;

        public string FullName => _fullName.Value;

        public string Name => _name.Value;

        public IEnumerable<TypeWrapper> Members => _members.Value;

        public IReadOnlyList<NamespaceWrapper> ChildNamespaces => _childNamespaces.Value;

        public static NamespaceWrapper Create(NamespaceDefinitionHandle handle, CompilationModule module)
        {
            if (handle.IsNil)
            {
                return null;
            }

            return _registeredNamespaces.GetOrAdd(handle, handleCreate => new NamespaceWrapper(handleCreate, module));
        }

        private static NamespaceDefinition Resolve(NamespaceDefinitionHandle handle, CompilationModule compilation)
        {
            return compilation.MetadataReader.GetNamespaceDefinition(handle);
        }

        private static string GetName(NamespaceDefinition handle, CompilationModule compilation)
        {
            return handle.Name.GetName(compilation);
        }

        private static NamespaceWrapper GetParent(NamespaceDefinitionHandle handle, CompilationModule module)
        {
            if (handle.IsNil)
            {
                return null;
            }

            return _registeredNamespaces.GetOrAdd(handle, handleToGenerate => new NamespaceWrapper(handleToGenerate, module));
        }

        private string GetFullName()
        {
            NamespaceWrapper current = this;

            var names = new List<string>();
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
