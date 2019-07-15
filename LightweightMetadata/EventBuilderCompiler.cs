// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading;
using LightweightMetadata.Extensions;
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
            if (searchDirectories == null)
            {
                throw new ArgumentNullException(nameof(searchDirectories));
            }

            SearchDirectories = searchDirectories.ToList();
            _typeProvider = new Lazy<TypeProvider>(() => new TypeProvider(this), LazyThreadSafetyMode.PublicationOnly);
            Init(mainModulePath);
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
        public NamespaceWrapper RootNamespace => new NamespaceWrapper(MainModule.MetadataReader.GetNamespaceDefinitionRoot(), MainModule);

        /// <summary>
        /// Gets the search directories.
        /// </summary>
        public IReadOnlyList<string> SearchDirectories { get; }

        internal TypeProvider TypeProvider => _typeProvider.Value;

        /// <inheritdoc />
        public CompilationModule GetCompilationModuleForReader(MetadataReader reader)
        {
            if (_mainModule.MetadataReader == reader)
            {
                return _mainModule;
            }

            return _referencedAssemblies.First(x => x.MetadataReader == reader);
        }

        /// <inheritdoc />
        public TypeWrapper GetTypeByName(string fullName)
        {
            if (MainModule.PublicTypesByFullName.TryGetValue(fullName, out var typeWrapper))
            {
                return typeWrapper;
            }

            foreach (var referenceModule in ReferencedModules)
            {
                if (referenceModule.PublicTypesByFullName.TryGetValue(fullName, out typeWrapper))
                {
                    return typeWrapper;
                }
            }

            return null;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _mainModule?.Dispose();
            _referencedAssemblies?.ForEach(x => x?.Dispose());
        }

        private static AssemblyReferenceHandle GetDeclaringModule(TypeReferenceHandle handle, MetadataReader reader)
        {
            var tr = reader.GetTypeReference(handle);
            switch (tr.ResolutionScope.Kind)
            {
                case HandleKind.TypeReference:
                    var typeReferenceHandle = (TypeReferenceHandle)tr.ResolutionScope;
                    return GetDeclaringModule(typeReferenceHandle, reader);
                case HandleKind.AssemblyReference:
                    var asmRef = (AssemblyReferenceHandle)tr.ResolutionScope;
                    return asmRef;
                default:
                    return default;
            }
        }

        /// <summary>
        /// Initializes the main project.
        /// </summary>
        /// <param name="mainAssembliesFilePath">The path to the main module.</param>
        [SuppressMessage("Design", "CA2000: Dispose variable", Justification = "Disposed in the Dispose method.")]
        private void Init(string mainAssembliesFilePath)
        {
            if (mainAssembliesFilePath == null)
            {
                throw new ArgumentNullException(nameof(mainAssembliesFilePath));
            }

            _mainModule = new CompilationModule(mainAssembliesFilePath, this, TypeProvider);

            var referencedAssemblies = new List<CompilationModule>();

            var referenceModulesToProcess = new Stack<(CompilationModule parent, AssemblyReferenceWrapper current)>(_mainModule.AssemblyReferences.Select(x => (_mainModule, x)));

            foreach (var reference in _mainModule.TypeReferences.Select(typeHandle => typeHandle.DeclaringModule).Where(x => x != null))
            {
                referenceModulesToProcess.Push((_mainModule, reference));
            }

            var assemblyReferencesVisited = new HashSet<string>();

            while (referenceModulesToProcess.Count > 0)
            {
                var (parent, currentAssemblyReference) = referenceModulesToProcess.Pop();

                var name = currentAssemblyReference.Name;

                if (assemblyReferencesVisited.Contains(name))
                {
                    continue;
                }

                assemblyReferencesVisited.Add(name);

                var currentModule = currentAssemblyReference.CompilationModule;

                if (currentModule != null)
                {
                    referencedAssemblies.Add(currentModule);

                    foreach (var moduleReferenceHandle in currentModule.AssemblyReferences)
                    {
                        referenceModulesToProcess.Push((currentModule, moduleReferenceHandle));
                    }

                    foreach (var typeReference in currentModule.TypeReferences.Select(x => x.DeclaringModule).Where(x => x != null))
                    {
                        referenceModulesToProcess.Push((currentModule, typeReference));
                    }
                }
            }

            _referencedAssemblies = referencedAssemblies;
            _initialized = true;
        }
    }
}
