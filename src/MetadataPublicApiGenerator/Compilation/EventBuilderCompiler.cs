// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using ICSharpCode.Decompiler.Metadata;
using ICSharpCode.Decompiler.TypeSystem;
using ICSharpCode.Decompiler.TypeSystem.Implementation;
using ICSharpCode.Decompiler.Util;

namespace MetadataPublicApiGenerator.Compilation
{
    /// <summary>
    /// This class is based on ICSharpCode.Decompiler SimpleCompiler.
    /// This has been changed to allow searching through reference types.
    /// </summary>
    /// <summary>
    /// Simple compilation implementation.
    /// </summary>
    internal class EventBuilderCompiler : ICompilation
    {
        private readonly KnownTypeCache _knownTypeCache;
        private readonly List<IModule> _assemblies = new List<IModule>();
        private readonly List<IModule> _referencedAssemblies = new List<IModule>();
        private bool _initialized;
        private INamespace _rootNamespace;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventBuilderCompiler"/> class.
        /// </summary>
        /// <param name="modules">Module references to load by default.</param>
        /// <param name="searchDirectories">The directories to search for additional dependencies.</param>
        public EventBuilderCompiler(IEnumerable<IModuleReference> modules, IEnumerable<string> searchDirectories)
        {
            _knownTypeCache = new KnownTypeCache(this);
            Init(modules, searchDirectories.ToList());
        }

        /// <inheritdoc />
        public IModule MainModule
        {
            get
            {
                if (!_initialized)
                {
                    throw new InvalidOperationException("Compilation isn't initialized yet");
                }

                return _assemblies.FirstOrDefault();
            }
        }

        /// <inheritdoc />
        public IReadOnlyList<IModule> Modules
        {
            get
            {
                if (!_initialized)
                {
                    throw new InvalidOperationException("Compilation isn't initialized yet");
                }

                return _assemblies;
            }
        }

        /// <inheritdoc />
        public IReadOnlyList<IModule> ReferencedModules
        {
            get
            {
                if (!_initialized)
                {
                    throw new InvalidOperationException("Compilation isn't initialized yet");
                }

                return _referencedAssemblies;
            }
        }

        /// <inheritdoc />
        public INamespace RootNamespace
        {
            get
            {
                INamespace ns = LazyInit.VolatileRead(ref _rootNamespace);
                if (ns != null)
                {
                    return ns;
                }

                if (!_initialized)
                {
                    throw new InvalidOperationException("Compilation isn't initialized yet");
                }

                return LazyInit.GetOrSet(ref _rootNamespace, CreateRootNamespace());
            }
        }

        /// <inheritdoc />
        public StringComparer NameComparer => StringComparer.Ordinal;

        /// <inheritdoc />
        public CacheManager CacheManager { get; } = new CacheManager();

        /// <inheritdoc />
        public virtual INamespace GetNamespaceForExternAlias(string alias)
        {
            if (string.IsNullOrEmpty(alias))
            {
                return RootNamespace;
            }

            // SimpleCompilation does not support extern aliases; but derived classes might.
            return null;
        }

        /// <inheritdoc />
        public IType FindType(KnownTypeCode typeCode)
        {
            return _knownTypeCache.FindType(typeCode);
        }

        /// <summary>
        /// Initializes the main project.
        /// </summary>
        /// <param name="mainAssemblies">The list of main assemblies to include in the compilation.</param>
        /// <param name="searchDirectories">A directory where to search for other types if we can't find it.</param>
        protected void Init(IEnumerable<IModuleReference> mainAssemblies, IReadOnlyCollection<string> searchDirectories)
        {
            if (mainAssemblies == null)
            {
                throw new ArgumentNullException(nameof(mainAssemblies));
            }

            if (searchDirectories == null)
            {
                throw new ArgumentNullException(nameof(searchDirectories));
            }

            var context = new SimpleTypeResolveContext(this);
            _assemblies.AddRange(mainAssemblies.Select(x => x.Resolve(context)));

            List<IModule> referencedAssemblies = new List<IModule>();

            var referenceModulesToProcess = new Stack<IAssemblyReference>(_assemblies.SelectMany(x => x.PEFile.AssemblyReferences));
            var assemblyReferencesVisited = new HashSet<string>();

            while (referenceModulesToProcess.Count > 0)
            {
                var currentAssemblyReference = referenceModulesToProcess.Pop();

                if (assemblyReferencesVisited.Contains(currentAssemblyReference.FullName))
                {
                    continue;
                }

                assemblyReferencesVisited.Add(currentAssemblyReference.FullName);

                IModule asm;
                try
                {
                    var currentModule = currentAssemblyReference.Resolve(searchDirectories);

                    if (currentModule == null)
                    {
                        continue;
                    }

                    asm = ((IModuleReference)currentModule).Resolve(context);
                }
                catch (InvalidOperationException)
                {
                    throw new InvalidOperationException("Tried to initialize compilation with an invalid assembly reference. (Forgot to load the assembly reference ? - see CecilLoader)");
                }

                if (asm != null)
                {
                    referencedAssemblies.Add(asm);
                    foreach (var element in asm.PEFile.AssemblyReferences)
                    {
                        referenceModulesToProcess.Push(element);
                    }
                }
            }

            _referencedAssemblies.AddRange(referencedAssemblies);
            _initialized = true;
        }

        /// <summary>
        /// Creates the root namespace for the project.
        /// </summary>
        /// <returns>The namespace information.</returns>
        protected virtual INamespace CreateRootNamespace()
        {
            var namespaces = new List<INamespace>();
            foreach (var module in _assemblies)
            {
                // SimpleCompilation does not support extern aliases; but derived classes might.
                // CreateRootNamespace() is virtual so that derived classes can change the global namespace.
                namespaces.Add(module.RootNamespace);
            }

            return new MergedNamespace(this, namespaces.ToArray());
        }
    }
}
