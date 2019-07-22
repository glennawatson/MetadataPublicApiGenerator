// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using LightweightMetadata.TypeWrappers;

namespace LightweightMetadata
{
    /// <summary>
    /// Represents a compilation input.
    /// </summary>
    public interface ICompilation
    {
        /// <summary>
        /// Gets the main module.
        /// </summary>
        CompilationModule MainModule { get; }

        /// <summary>
        /// Gets the main namespace.
        /// </summary>
        NamespaceWrapper RootNamespace { get; }

        /// <summary>
        /// Gets the search directories.
        /// </summary>
        IReadOnlyList<string> SearchDirectories { get; }

        /// <summary>
        /// Gets the compilation module for a reader.
        /// </summary>
        /// <param name="reader">The reader to use.</param>
        /// <returns>The compilation module.</returns>
        CompilationModule GetCompilationModuleForReader(MetadataReader reader);

        /// <summary>
        /// Gets the compilation module for the specified name.
        /// </summary>
        /// <param name="name">The name to fetch.</param>
        /// <param name="parent">The parent of the compilation module.</param>
        /// <param name="version">The version to fetch for.</param>
        /// <param name="isWindowsRuntime">If the assembly is a windows runtime.</param>
        /// <param name="isRetargetable">If the assembly can be targeting another assembly.</param>
        /// <param name="publicKey">The optional public key.</param>
        /// <returns>The compilation module.</returns>
        CompilationModule GetCompilationModuleForName(string name, CompilationModule parent, Version version = null, bool isWindowsRuntime = false, bool isRetargetable = false, string publicKey = null);

        /// <summary>
        /// Gets the compilation module for the specified assembly reference.
        /// </summary>
        /// <param name="wrapper">The wrapper to get for.</param>
        /// <returns>The compilation module.</returns>
        CompilationModule GetCompilationModuleForAssemblyReference(AssemblyReferenceWrapper wrapper);

        /// <summary>
        /// Gets a type by the full name.
        /// </summary>
        /// <param name="fullName">The full name.</param>
        /// <returns>The type wrapper.</returns>
        IHandleTypeNamedWrapper GetTypeByName(string fullName);
    }
}
