// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading;
using LightweightMetadata.Helpers;
using LightweightMetadata.TypeWrappers;

namespace LightweightMetadata
{
    /// <summary>
    /// This class is based on ICSharpCode.Decompiler SimpleCompiler.
    /// This has been changed to allow searching through reference types.
    /// </summary>
    /// <summary>
    /// Simple compilation implementation.
    /// </summary>
    public sealed class EventBuilderCompiler : ICompilation, IDisposable
    {
        private readonly Lazy<TypeProvider> _typeProvider;
        private readonly IReadOnlyDictionary<string, IHandleTypeNamedWrapper> _namesToTypes;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventBuilderCompiler"/> class.
        /// </summary>
        /// <param name="mainModulePath">The path to the main module.</param>
        /// <param name="searchDirectories">The directories to search for additional dependencies.</param>
        public EventBuilderCompiler(string mainModulePath, IEnumerable<string> searchDirectories)
        {
            if (searchDirectories == null)
            {
                throw new ArgumentNullException(nameof(searchDirectories));
            }

            if (mainModulePath == null)
            {
                throw new ArgumentNullException(nameof(mainModulePath));
            }

            _typeProvider = new Lazy<TypeProvider>(() => new TypeProvider(this), LazyThreadSafetyMode.PublicationOnly);
            MainModule = new CompilationModule(mainModulePath, this, TypeProvider);
            SearchDirectories = searchDirectories.ToList();
            ReferencedModules = GetCompilationModules();
            _namesToTypes = GetTypes();
        }

        /// <inheritdoc />
        public CompilationModule MainModule { get; }

        /// <summary>
        /// Gets a list of the referenced modules.
        /// </summary>
        public IReadOnlyList<CompilationModule> ReferencedModules { get; }

        /// <inheritdoc />
        public NamespaceWrapper RootNamespace => new NamespaceWrapper(MainModule.MetadataReader.GetNamespaceDefinitionRoot(), MainModule);

        /// <summary>
        /// Gets the search directories.
        /// </summary>
        public IReadOnlyList<string> SearchDirectories { get; }

        /// <summary>
        /// Gets the type provider which is used by the reflection metadata classes.
        /// </summary>
        internal TypeProvider TypeProvider => _typeProvider.Value;

        /// <inheritdoc />
        public CompilationModule GetCompilationModuleForReader(MetadataReader reader)
        {
            if (MainModule.MetadataReader == reader)
            {
                return MainModule;
            }

            return null;
        }

        /// <inheritdoc />
        public CompilationModule GetCompilationModuleForName(string name, CompilationModule parent, Version version = null, bool isWindowsRuntime = false, bool isRetargetable = false, string publicKey = null)
        {
            if (parent == null)
            {
                throw new ArgumentNullException(nameof(parent));
            }

            return AssemblyLoadingHelper.ResolveCompilationModule(name, parent, version, isWindowsRuntime, isRetargetable, publicKey);
        }

        /// <inheritdoc />
        public CompilationModule GetCompilationModuleForAssemblyReference(AssemblyReferenceWrapper wrapper)
        {
            if (wrapper == null)
            {
                throw new ArgumentNullException(nameof(wrapper));
            }

            return GetCompilationModuleForName(wrapper.Name, wrapper.ParentCompilationModule, wrapper.Version, wrapper.IsWindowsRuntime, wrapper.IsRetargetable, wrapper.PublicKey);
        }

        /// <inheritdoc />
        public IHandleTypeNamedWrapper GetTypeByName(string fullName)
        {
            if (_namesToTypes.TryGetValue(fullName, out var type))
            {
                return type;
            }

            return null;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            MainModule?.Dispose();
            foreach (var referencedModule in ReferencedModules)
            {
                referencedModule?.Dispose();
            }
        }

        private IReadOnlyList<CompilationModule> GetCompilationModules()
        {
            var list = new List<CompilationModule>();

            var processed = new HashSet<string>();

            var toProcess = new Stack<AssemblyReferenceWrapper>(MainModule.AssemblyReferences);

            while (toProcess.Count != 0)
            {
                var current = toProcess.Pop();

                var compilation = GetCompilationModuleForAssemblyReference(current);
                if (!processed.Contains(compilation.FileName))
                {
                    processed.Add(compilation.FileName);
                    list.Add(compilation);
                    foreach (var subReference in compilation.AssemblyReferences)
                    {
                        toProcess.Push(subReference);
                    }
                }
            }

            System.IO.File.WriteAllLines("files.txt", list.Select(x => x.FileName).OrderBy(x => x).ToList());
            return list.ToList();
        }

        private IReadOnlyDictionary<string, IHandleTypeNamedWrapper> GetTypes()
        {
            var output = new Dictionary<string, IHandleTypeNamedWrapper>();

            foreach (var subReference in ReferencedModules)
            {
                foreach (var type in subReference.Types)
                {
                    output[type.FullName] = type;
                }
            }

            System.IO.File.WriteAllLines("types.txt", output.Keys.OrderBy(x => x).ToList());
            return output;
        }
    }
}