// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading;

namespace LightweightMetadata
{
    /// <summary>
    /// This class is based on ICSharpCode.Decompiler SimpleCompiler.
    /// This has been changed to allow searching through reference types.
    /// </summary>
    /// <summary>
    /// Simple MetadataRepository implementation.
    /// </summary>
    public sealed class MetadataRepository : IDisposable
    {
        private readonly Lazy<IReadOnlyList<AssemblyMetadata>> _referenceAssemblies;

        /// <summary>
        /// Initializes a new instance of the <see cref="MetadataRepository"/> class.
        /// </summary>
        /// <param name="mainModulePath">The path to the main module.</param>
        /// <param name="searchDirectories">The directories to search for additional dependencies.</param>
        public MetadataRepository(string mainModulePath, IEnumerable<string> searchDirectories)
        {
            if (searchDirectories == null)
            {
                throw new ArgumentNullException(nameof(searchDirectories));
            }

            if (mainModulePath == null)
            {
                throw new ArgumentNullException(nameof(mainModulePath));
            }

            TypeProvider = new TypeProvider(this);
            MainAssemblyMetadata = new AssemblyMetadata(mainModulePath, this, TypeProvider);
            SearchDirectories = searchDirectories.ToList();

            RootNamespace = new NamespaceWrapper(MainAssemblyMetadata.MetadataReader.GetNamespaceDefinitionRoot(), MainAssemblyMetadata);

            _referenceAssemblies = new Lazy<IReadOnlyList<AssemblyMetadata>>(GetReferenceAssemblies, LazyThreadSafetyMode.PublicationOnly);
        }

        /// <summary>
        /// Gets the main module.
        /// </summary>
        public AssemblyMetadata MainAssemblyMetadata { get; }

        /// <summary>
        /// Gets the main namespace.
        /// </summary>
        public NamespaceWrapper RootNamespace { get; }

        /// <summary>
        /// Gets the search directories.
        /// </summary>
        public IReadOnlyList<string> SearchDirectories { get; }

        /// <summary>
        /// Gets all the reference assemblies referenced by the main assembly metadata.
        /// </summary>
        public IReadOnlyList<AssemblyMetadata> ReferenceAssemblies => _referenceAssemblies.Value;

        /// <summary>
        /// Gets the type provider which is used by the reflection metadata classes.
        /// </summary>
        internal TypeProvider TypeProvider { get; }

        /// <summary>
        /// Gets the MetadataRepository module for the specified name.
        /// </summary>
        /// <param name="name">The name to fetch.</param>
        /// <param name="parent">The parent of the MetadataRepository module.</param>
        /// <param name="version">The version to fetch for.</param>
        /// <param name="isWindowsRuntime">If the assembly is a windows runtime.</param>
        /// <param name="isRetargetable">If the assembly can be targeting another assembly.</param>
        /// <param name="publicKey">The optional public key.</param>
        /// <returns>The MetadataRepository module.</returns>
        public static AssemblyMetadata? GetAssemblyMetadataForName(string name, AssemblyMetadata parent, Version? version = null, bool isWindowsRuntime = false, bool isRetargetable = false, string? publicKey = null)
        {
            if (parent is null)
            {
                throw new ArgumentNullException(nameof(parent));
            }

            return AssemblyLoadingHelper.ResolveCompilationModule(name, parent, version, isWindowsRuntime, isRetargetable, publicKey);
        }

        /// <summary>
        /// Gets the MetadataRepository module for the specified assembly reference.
        /// </summary>
        /// <param name="wrapper">The wrapper to get for.</param>
        /// <returns>The MetadataRepository module.</returns>
        public static AssemblyMetadata? GetAssemblyMetadataForAssemblyReference(AssemblyReferenceWrapper? wrapper)
        {
            if (wrapper is null)
            {
                throw new ArgumentNullException(nameof(wrapper));
            }

            return GetAssemblyMetadataForName(wrapper.Name, wrapper.ParentCompilationModule, wrapper.Version, wrapper.IsWindowsRuntime, wrapper.IsRetargetable, wrapper.PublicKey);
        }

        /// <summary>
        /// Gets the MetadataRepository module for a reader.
        /// </summary>
        /// <param name="reader">The reader to use.</param>
        /// <returns>The MetadataRepository module.</returns>
        public AssemblyMetadata GetAssemblyMetadataForReader(MetadataReader reader)
        {
            if (MainAssemblyMetadata.MetadataReader == reader)
            {
                return MainAssemblyMetadata;
            }

            foreach (var subAssembly in ReferenceAssemblies)
            {
                if (subAssembly.MetadataReader == reader)
                {
                    return subAssembly;
                }
            }

            throw new ArgumentException("Could not find Assembly Metadata for reader.", nameof(reader));
        }

        /// <summary>
        /// Gets a type by the full name.
        /// </summary>
        /// <param name="fullName">The full name.</param>
        /// <returns>The type wrapper.</returns>
        public TypeWrapper? GetTypeByName(string? fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
            {
                return null;
            }

            var type = MainAssemblyMetadata.GetTypeByName(fullName, false);

            if (type != null)
            {
                return type;
            }

            foreach (var referenceAssembly in ReferenceAssemblies)
            {
                type = referenceAssembly.GetTypeByName(fullName, false);

                if (type != null)
                {
                    return type;
                }
            }

            return null;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            MainAssemblyMetadata?.Dispose();
        }

        private IReadOnlyList<AssemblyMetadata> GetReferenceAssemblies()
        {
            var assemblies = new List<AssemblyMetadata>(1024);

            var assembliesVisited = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);

            var referenceModulesToProcess = new Stack<(AssemblyMetadata Parent, AssemblyReferenceWrapper AssemblyReference)>(MainAssemblyMetadata.AssemblyReferences.Select(x => (MainModule: MainAssemblyMetadata, x)));

            while (referenceModulesToProcess.Count > 0)
            {
                var (parent, current) = referenceModulesToProcess.Pop();
                if (assembliesVisited.Contains(current.Name))
                {
                    continue;
                }

                assembliesVisited.Add(current.Name);

                var assemblyMetadata = AssemblyLoadingHelper.ResolveCompilationModule(current.Name, parent, current.Version, current.IsWindowsRuntime, current.IsRetargetable, current.PublicKey);

                if (assemblyMetadata is null)
                {
                    continue;
                }

                assemblies.Add(assemblyMetadata);

                foreach (var child in assemblyMetadata.AssemblyReferences)
                {
                    referenceModulesToProcess.Push((assemblyMetadata, child));
                }
            }

            return assemblies;
        }
    }
}