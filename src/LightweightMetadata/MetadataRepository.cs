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
    public sealed class MetadataRepository : IMetadataRepository, IDisposable
    {
        private readonly Lazy<TypeProvider> _typeProvider;

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

            _typeProvider = new Lazy<TypeProvider>(() => new TypeProvider(this), LazyThreadSafetyMode.PublicationOnly);
            MainModule = new AssemblyMetadata(mainModulePath, this, TypeProvider);
            SearchDirectories = searchDirectories.ToList();
        }

        /// <inheritdoc />
        public AssemblyMetadata MainModule { get; }

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
        public AssemblyMetadata GetCompilationModuleForReader(MetadataReader reader)
        {
            if (MainModule.MetadataReader == reader)
            {
                return MainModule;
            }

            return null;
        }

        /// <inheritdoc />
        public AssemblyMetadata GetCompilationModuleForName(string name, AssemblyMetadata parent, Version version = null, bool isWindowsRuntime = false, bool isRetargetable = false, string publicKey = null)
        {
            if (parent == null)
            {
                throw new ArgumentNullException(nameof(parent));
            }

            return AssemblyLoadingHelper.ResolveCompilationModule(name, parent, version, isWindowsRuntime, isRetargetable, publicKey);
        }

        /// <inheritdoc />
        public AssemblyMetadata GetCompilationModuleForAssemblyReference(AssemblyReferenceWrapper wrapper)
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
            return MainModule.GetTypeByName(fullName);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            MainModule?.Dispose();
        }
    }
}