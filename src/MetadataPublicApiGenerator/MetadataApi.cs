// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

using LightweightMetadata;

using MetadataPublicApiGenerator.Generators;

namespace MetadataPublicApiGenerator
{
    /// <summary>
    /// Generates a string based on the contents of a assembly.
    /// </summary>
    public static class MetadataApi
    {
        /// <summary>
        /// A list of default attributes to skip.
        /// </summary>
        private static readonly HashSet<string> DefaultSkipAttributeNames = new HashSet<string>
        {
            "System.ParamArrayAttribute",
            "System.CodeDom.Compiler.GeneratedCodeAttribute",
            "System.ComponentModel.EditorBrowsableAttribute",
            "System.Diagnostics.DebuggableAttribute",
            "System.Diagnostics.DebuggerNonUserCodeAttribute",
            "System.Diagnostics.DebuggerStepThroughAttribute",
            "System.Runtime.CompilerServices.AsyncStateMachineAttribute",
            "System.Runtime.CompilerServices.CompilerGeneratedAttribute",
            "System.Runtime.CompilerServices.CompilationRelaxationsAttribute",
            "System.Runtime.CompilerServices.ExtensionAttribute",
            "System.Runtime.CompilerServices.FixedBufferAttribute",
            "System.Runtime.CompilerServices.IteratorStateMachineAttribute",
            "System.Runtime.CompilerServices.NullableAttribute",
            "System.Runtime.CompilerServices.NullableContextAttribute",
            "System.Runtime.CompilerServices.RuntimeCompatibilityAttribute",
            "System.Runtime.CompilerServices.TupleElementNamesAttribute",
            "System.Reflection.DefaultMemberAttribute",
            "System.Reflection.AssemblyCompanyAttribute",
            "System.Reflection.AssemblyConfigurationAttribute",
            "System.Reflection.AssemblyCopyrightAttribute",
            "System.Reflection.AssemblyDescriptionAttribute",
            "System.Reflection.AssemblyFileVersionAttribute",
            "System.Reflection.AssemblyInformationalVersionAttribute",
            "System.Reflection.AssemblyProductAttribute",
            "System.Reflection.AssemblyTitleAttribute",
            "System.Reflection.AssemblyTrademarkAttribute"
        };

        private static readonly HashSet<string> DefaultSkipMemberAttributeNames = new HashSet<string>
        {
            "System.Runtime.CompilerServices.CompilerGeneratedAttribute",
        };

        /// <summary>
        /// Generates a string of the public exposed API within the specified assembly.
        /// </summary>
        /// <param name="assemblyFilePath">The file path to the assembly to extract the public API from.</param>
        /// <param name="shouldIncludeAssemblyAttributes">Optional parameter indicating if the results should include assembly attributes within the results.</param>
        /// <param name="excludeAttributes">Optional parameter of any to the attributes to exclude.</param>
        /// <param name="excludeMembersAttributes">Optional parameter of any attributes to use to discard members.</param>
        /// <param name="excludeFunc">Determines if a type should be included.</param>
        /// <returns>The string containing the public available API.</returns>
        public static string GeneratePublicApi(string assemblyFilePath, bool shouldIncludeAssemblyAttributes = true, IEnumerable<string> excludeAttributes = null, IEnumerable<string> excludeMembersAttributes = null, Func<TypeWrapper, bool> excludeFunc = null)
        {
            var attributesToExclude = excludeAttributes == null ? DefaultSkipAttributeNames : new HashSet<string>(excludeAttributes.Union(DefaultSkipAttributeNames));

            var attributesMembersToExclude = excludeMembersAttributes == null ? DefaultSkipMemberAttributeNames : new HashSet<string>(excludeMembersAttributes.Union(DefaultSkipMemberAttributeNames));

            var searchDirectories = new HashSet<string>
                                        {
                                            Path.GetDirectoryName(assemblyFilePath),
                                            AppDomain.CurrentDomain.BaseDirectory,
                                            RuntimeEnvironment.GetRuntimeDirectory(),
                                        };

            searchDirectories.UnionWith(AppDomain.CurrentDomain.GetAssemblies().Where(x => !x.IsDynamic).Select(x => Path.GetDirectoryName(x.Location)));

            excludeFunc = excludeFunc ?? (_ => false);

            using (var compilationMetadata = new MetadataRepository(assemblyFilePath, searchDirectories))
            {
                return GeneratorFactory.Generate(compilationMetadata, attributesMembersToExclude, attributesToExclude, excludeFunc, shouldIncludeAssemblyAttributes).ToFullString();
            }
        }

        /// <summary>
        /// Generates a string of the public exposed API within the specified assembly.
        /// </summary>
        /// <param name="assembly">The assembly to extract the public API from.</param>
        /// <param name="shouldIncludeAssemblyAttributes">Optional parameter indicating if the results should include assembly attributes within the results.</param>
        /// <param name="excludeAttributes">Optional parameter of any to the attributes to exclude.</param>
        /// <param name="excludeMembersAttributes">Optional parameter of any attributes to use to discard members.</param>
        /// <param name="excludeFunc">Determines if a type should be included.</param>
        /// <returns>The string containing the public available API.</returns>
        public static string GeneratePublicApi(Assembly assembly, bool shouldIncludeAssemblyAttributes = true, IEnumerable<string> excludeAttributes = null, IEnumerable<string> excludeMembersAttributes = null, Func<TypeWrapper, bool> excludeFunc = null)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            var assemblyPath = assembly.Location;

            return GeneratePublicApi(assemblyPath, shouldIncludeAssemblyAttributes, excludeAttributes, excludeMembersAttributes, excludeFunc);
        }
    }
}
