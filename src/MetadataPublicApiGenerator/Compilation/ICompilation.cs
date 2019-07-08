// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Reflection.Metadata;

using MetadataPublicApiGenerator.Compilation.TypeWrappers;

namespace MetadataPublicApiGenerator.Compilation
{
    /// <summary>
    /// Represents a compilation input.
    /// </summary>
    internal interface ICompilation
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
        /// Gets any referenced modules.
        /// </summary>
        IReadOnlyList<CompilationModule> ReferencedModules { get; }

        /// <summary>
        /// Gets the type provider.
        /// </summary>
        TypeProvider TypeProvider { get; }

        /// <summary>
        /// Gets the compilation module for a reader.
        /// </summary>
        /// <param name="reader">The reader to use.</param>
        /// <returns>The compilation module.</returns>
        CompilationModule GetCompilationModuleForReader(MetadataReader reader);

        /// <summary>
        /// Gets a type by the full name.
        /// </summary>
        /// <param name="fullName">The full name.</param>
        /// <returns>The type wrapper.</returns>
        TypeWrapper GetTypeByName(string fullName);
    }
}
