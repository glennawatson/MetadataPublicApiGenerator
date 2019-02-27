// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using MetadataPublicApiGenerator.Compilation;

namespace MetadataPublicApiGenerator.Extensions
{
    internal static class PathSearchExtensions
    {
        /// <summary>
        /// Resolves the specified full assembly name.
        /// </summary>
        /// <param name="reference">A reference with details about the assembly.</param>
        /// <param name="compilation">The compilation that is the parent.</param>
        /// <param name="baseReader">The base reader to use.</param>
        /// <param name="targetAssemblyDirectories">The directories potentially containing the assemblies.</param>
        /// <param name="parameters">Parameters to provide to the reflection system..</param>
        /// <returns>The assembly definition.</returns>
        public static CompilationModule Resolve(this AssemblyReferenceHandle reference, ICompilation compilation, CompilationModule baseReader, IReadOnlyCollection<string> targetAssemblyDirectories, PEStreamOptions parameters = PEStreamOptions.PrefetchMetadata)
        {
            var module = baseReader.MetadataReader.GetAssemblyReference(reference);

            var name = baseReader.MetadataReader.GetString(module.Name);

            var dllName = name + ".dll";

            var fullPath = targetAssemblyDirectories.Select(x => Path.Combine(x, dllName)).FirstOrDefault(File.Exists);
            if (fullPath == null)
            {
                dllName = name + ".winmd";
                fullPath = targetAssemblyDirectories.Select(x => Path.Combine(x, dllName)).FirstOrDefault(File.Exists);
            }

            // NB: This hacks WinRT's weird mscorlib to just use the regular one
            // We forget why this was needed, maybe it's not needed anymore?
            if (name.IndexOf("mscorlib", StringComparison.InvariantCultureIgnoreCase) >= 0 && name.Contains("255"))
            {
                fullPath = Environment.ExpandEnvironmentVariables(@"%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\mscorlib.dll");
            }

            if (fullPath == null)
            {
                return null;
            }

            return new CompilationModule(new PEReader(new FileStream(fullPath, FileMode.Open, FileAccess.Read), parameters), compilation);
        }
    }
}
