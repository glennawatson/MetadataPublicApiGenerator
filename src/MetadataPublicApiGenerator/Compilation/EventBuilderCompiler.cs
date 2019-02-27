// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using MetadataPublicApiGenerator.Extensions;

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
        private List<CompilationModule> _referencedAssemblies;
        private CompilationModule _mainModule;
        private bool _initialized;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventBuilderCompiler"/> class.
        /// </summary>
        /// <param name="mainModulePath">The path to the main module.</param>
        /// <param name="searchDirectories">The directories to search for additional dependencies.</param>
        public EventBuilderCompiler(string mainModulePath, IEnumerable<string> searchDirectories)
        {
            Init(mainModulePath, searchDirectories.ToList());
        }

        /// <inheritdoc />
        public CompilationModule MainModule
        {
            get
            {
                if (!_initialized)
                {
                    throw new InvalidOperationException("Compilation isn't initialized yet");
                }

                return _mainModule;
            }
        }

        /// <inheritdoc />
        public IReadOnlyList<CompilationModule> ReferencedModules
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
        public NamespaceDefinition RootNamespace => MainModule.MetadataReader.GetNamespaceDefinitionRoot();

        /// <inheritdoc />
        public CompilationModule GetCompilationModuleForReader(MetadataReader reader)
        {
            if (_mainModule.MetadataReader == reader)
            {
                return _mainModule;
            }

            return _referencedAssemblies.First(x => x.MetadataReader == reader);
        }

        /// <summary>
        /// Initializes the main project.
        /// </summary>
        /// <param name="mainAssembliesFilePath">The path to the main module.</param>
        /// <param name="searchDirectories">A directory where to search for other types if we can't find it.</param>
        protected void Init(string mainAssembliesFilePath, IReadOnlyCollection<string> searchDirectories)
        {
            if (mainAssembliesFilePath == null)
            {
                throw new ArgumentNullException(nameof(mainAssembliesFilePath));
            }

            if (searchDirectories == null)
            {
                throw new ArgumentNullException(nameof(searchDirectories));
            }

            _mainModule = new CompilationModule(new PEReader(new FileStream(mainAssembliesFilePath, FileMode.Open, FileAccess.Read), PEStreamOptions.PrefetchMetadata), this);

            var referencedAssemblies = new List<CompilationModule>();

            var referenceModulesToProcess = new Stack<CompilationModule>(new[] { _mainModule });

            var assemblyReferencesVisited = new HashSet<string>();

            while (referenceModulesToProcess.Count > 0)
            {
                var currentAssemblyReference = referenceModulesToProcess.Pop();

                var name = currentAssemblyReference.MetadataReader.GetString(currentAssemblyReference.MetadataReader.GetModuleDefinition().Name);

                if (assemblyReferencesVisited.Contains(name))
                {
                    continue;
                }

                assemblyReferencesVisited.Add(name);
                referencedAssemblies.Add(currentAssemblyReference);

                foreach (var moduleReferenceHandle in currentAssemblyReference.MetadataReader.AssemblyReferences)
                {
                    var compilationModule = moduleReferenceHandle.Resolve(this, currentAssemblyReference, searchDirectories);
                    referenceModulesToProcess.Push(compilationModule);
                }
            }

            _referencedAssemblies = referencedAssemblies;
            _initialized = true;
        }
    }
}
