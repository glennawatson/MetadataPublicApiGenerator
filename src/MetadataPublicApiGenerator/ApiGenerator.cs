// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Text;
using ICSharpCode.Decompiler.CSharp.TypeSystem;
using ICSharpCode.Decompiler.Metadata;
using ICSharpCode.Decompiler.TypeSystem;
using MetadataPublicApiGenerator.Compilation;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Accessibility = ICSharpCode.Decompiler.TypeSystem.Accessibility;

namespace MetadataPublicApiGenerator
{
    /// <summary>
    /// Generates a string based on the contents of a assembly.
    /// </summary>
    public static class ApiGenerator
    {
        /// <summary>
        /// A list of default attributes to skip.
        /// </summary>
        private static readonly HashSet<string> SkipAttributeNames = new HashSet<string>
        {
            "System.CodeDom.Compiler.GeneratedCodeAttribute",
            "System.ComponentModel.EditorBrowsableAttribute",
            "System.Runtime.CompilerServices.AsyncStateMachineAttribute",
            "System.Runtime.CompilerServices.CompilerGeneratedAttribute",
            "System.Runtime.CompilerServices.CompilationRelaxationsAttribute",
            "System.Runtime.CompilerServices.ExtensionAttribute",
            "System.Runtime.CompilerServices.RuntimeCompatibilityAttribute",
            "System.Runtime.CompilerServices.IteratorStateMachineAttribute",
            "System.Reflection.DefaultMemberAttribute",
            "System.Diagnostics.DebuggableAttribute",
            "System.Diagnostics.DebuggerNonUserCodeAttribute",
            "System.Diagnostics.DebuggerStepThroughAttribute",
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

        /// <summary>
        /// Generates a string of the public exposed API within the specified assembly.
        /// </summary>
        /// <param name="assembly">The assembly to extract the public API from.</param>
        /// <param name="includeTypes">Optional parameter which will restrict which types to restrict the API to.</param>
        /// <param name="shouldIncludeAssemblyAttributes">Optional parameter indicating if the results should include assembly attributes within the results.</param>
        /// <param name="whitelistedNamespacePrefixes">Optional parameter of namespaces we should white list.</param>
        /// <param name="excludeAttributes">Optional parameter of any to the attributes to use to discard members.</param>
        /// <returns>The string containing the public available API.</returns>
        public static string GeneratePublicApi(Assembly assembly, IEnumerable<Type> includeTypes = null, bool shouldIncludeAssemblyAttributes = true, IEnumerable<string> whitelistedNamespacePrefixes = null, IEnumerable<string> excludeAttributes = null)
        {
            var attributesToExclude = excludeAttributes == null
                ? SkipAttributeNames
                : new HashSet<string>(excludeAttributes.Union(SkipAttributeNames));

            var assemblyPath = assembly.Location;
            var searchDirectories = new[]
            {
                Path.GetDirectoryName(assemblyPath),
                AppDomain.CurrentDomain.BaseDirectory,
                System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory()
            };

            var compilation = new EventBuilderCompiler(new IModuleReference[] { new PEFile(assemblyPath, PEStreamOptions.PrefetchMetadata) }, searchDirectories);

            Func<ITypeDefinition, bool> includeTypesFunc = tr => includeTypes == null || includeTypes.Any(t => t.FullName == tr.FullName);

            return CreatePublicApiForAssembly(compilation, includeTypesFunc, shouldIncludeAssemblyAttributes, whitelistedNamespacePrefixes, attributesToExclude);
        }

        internal static string CreatePublicApiForAssembly(ICompilation compilation, Func<ITypeDefinition, bool> shouldIncludeType, bool shouldIncludeAssemblyAttributes, IEnumerable<string> whitelistedNamespacePrefixes, ISet<string> excludeAttributes)
        {
            var compilationUnit = SyntaxFactory.CompilationUnit();

            var assemblyAttributes = compilation.MainModule.GetAssemblyAttributes().Where(x => excludeAttributes.Contains(x.AttributeType.FullName)).ToList();
            if (assemblyAttributes.Count > 0 && shouldIncludeAssemblyAttributes)
            {
                compilationUnit = GenerateAssemblyCustomAttributes(compilation, compilationUnit, assemblyAttributes);
            }

            var namespaceProcessingStack = new Stack<INamespace>(new[] { compilation.RootNamespace });

            while (namespaceProcessingStack.Count > 0)
            {
                var current = namespaceProcessingStack.Pop();

                GenerateNamespace(compilation, compilationUnit, current);

                foreach (var child in current.ChildNamespaces)
                {
                    namespaceProcessingStack.Push(child);
                }
            }

            return compilationUnit.NormalizeWhitespace().ToFullString();
        }

        internal static CompilationUnitSyntax GenerateAssemblyCustomAttributes(ICompilation compilation, CompilationUnitSyntax compilationUnitSyntax, IReadOnlyCollection<IAttribute> attributes)
        {
            return compilationUnitSyntax
                .WithAttributeLists(
                    SyntaxFactory.List(
                        attributes.Select(
                            attribute =>
                                attribute
                                    .GenerateAttributeList(compilation)
                                    .WithTarget(SyntaxFactory.AttributeTargetSpecifier(SyntaxFactory.Token(SyntaxKind.AssemblyKeyword))))));
        }

        internal static CompilationUnitSyntax GenerateNamespace(ICompilation compilation, CompilationUnitSyntax compilationUnitSyntax, INamespace namespaceInfo)
        {

        }
    }
}
