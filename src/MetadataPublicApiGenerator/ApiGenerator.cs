// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using MetadataPublicApiGenerator.Compilation;
using MetadataPublicApiGenerator.Compilation.TypeWrappers;
using MetadataPublicApiGenerator.Generators;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

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
        private static readonly HashSet<string> DefaultSkipAttributeNames = new HashSet<string>
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

        private static readonly HashSet<string> DefaultSkipMemberAttributeNames = new HashSet<string>
        {
            "System.Runtime.CompilerServices.CompilerGeneratedAttribute",
        };

        /// <summary>
        /// Generates a string of the public exposed API within the specified assembly.
        /// </summary>
        /// <param name="assembly">The assembly to extract the public API from.</param>
        /// <param name="includeTypes">Optional parameter which will restrict which types to restrict the API to.</param>
        /// <param name="shouldIncludeAssemblyAttributes">Optional parameter indicating if the results should include assembly attributes within the results.</param>
        /// <param name="whitelistedNamespacePrefixes">Optional parameter of namespaces we should white list.</param>
        /// <param name="excludeAttributes">Optional parameter of any to the attributes to exclude.</param>
        /// <param name="excludeMembersAttributes">Optional parameter of any attributes to use to discard members.</param>
        /// <returns>The string containing the public available API.</returns>
        public static string GeneratePublicApi(Assembly assembly, IEnumerable<Type> includeTypes = null, bool shouldIncludeAssemblyAttributes = true, IEnumerable<string> whitelistedNamespacePrefixes = null, IEnumerable<string> excludeAttributes = null, IEnumerable<string> excludeMembersAttributes = null)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            var attributesToExclude = excludeAttributes == null ? DefaultSkipAttributeNames : new HashSet<string>(excludeAttributes.Union(DefaultSkipAttributeNames));

            var attributesMembersToExclude = excludeMembersAttributes == null ? DefaultSkipMemberAttributeNames : new HashSet<string>(excludeMembersAttributes.Union(DefaultSkipMemberAttributeNames));

            var assemblyPath = assembly.Location;
            var searchDirectories = new[]
            {
                Path.GetDirectoryName(assemblyPath),
                AppDomain.CurrentDomain.BaseDirectory,
                System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory(),
            };

            using (var compilation = new EventBuilderCompiler(assemblyPath, searchDirectories))
            {
                Func<TypeWrapper, bool> excludeFunc = tr => false;

                return CreatePublicApiForAssembly(compilation, excludeFunc, shouldIncludeAssemblyAttributes, whitelistedNamespacePrefixes, attributesToExclude, attributesMembersToExclude);
            }
        }

        internal static string CreatePublicApiForAssembly(ICompilation compilation, Func<TypeWrapper, bool> excludeFunc, bool shouldIncludeAssemblyAttributes, IEnumerable<string> whitelistedNamespacePrefixes, ISet<string> excludeAttributes, ISet<string> excludeMembersAttributes)
        {
            var compilationUnit = SyntaxFactory.CompilationUnit();

            if (shouldIncludeAssemblyAttributes)
            {
                compilationUnit = compilationUnit.WithAttributeLists(AttributeGenerator.GenerateAssemblyCustomAttributes(compilation.MainModule, excludeAttributes));
            }

            var factory = new GeneratorFactory(excludeAttributes, excludeMembersAttributes, excludeFunc);

            compilationUnit = GenerateCompilationUnit(compilation, compilationUnit, factory);

            return compilationUnit.NormalizeWhitespace().ToFullString();
        }

        internal static CompilationUnitSyntax GenerateCompilationUnit(ICompilation compilation, CompilationUnitSyntax compilationUnit, IGeneratorFactory factory)
        {
            var namespaceProcessingStack = new Stack<NamespaceWrapper>(new[] { compilation.RootNamespace });

            var list = new List<MemberDeclarationSyntax>(128);

            var outsideNamespaceList = new List<MemberDeclarationSyntax>();

            while (namespaceProcessingStack.Count > 0)
            {
                var namespaceInfo = namespaceProcessingStack.Pop();

                var members = factory.GenerateMembers(namespaceInfo);

                if (members.Count != 0)
                {
                    var namespaceName = namespaceInfo.FullName;
                    if (string.IsNullOrEmpty(namespaceName))
                    {
                        outsideNamespaceList.AddRange(members);
                    }
                    else
                    {
                        var namespaceDeclaration = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.IdentifierName(namespaceName)).WithMembers(SyntaxFactory.List(members));

                        list.Add(namespaceDeclaration);
                    }
                }

                foreach (var child in namespaceInfo.ChildNamespaces)
                {
                    namespaceProcessingStack.Push(child);
                }
            }

            return compilationUnit.WithMembers(SyntaxFactory.List(outsideNamespaceList.Concat(list)));
        }
    }
}
