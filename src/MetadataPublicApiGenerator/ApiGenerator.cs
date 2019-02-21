// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.PortableExecutable;
using ICSharpCode.Decompiler.Metadata;
using ICSharpCode.Decompiler.TypeSystem;
using MetadataPublicApiGenerator.Compilation;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Accessibility = ICSharpCode.Decompiler.TypeSystem.Accessibility;
using SymbolKind = ICSharpCode.Decompiler.TypeSystem.SymbolKind;
using TypeKind = ICSharpCode.Decompiler.TypeSystem.TypeKind;

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
            var attributesToExclude = excludeAttributes == null ? DefaultSkipAttributeNames : new HashSet<string>(excludeAttributes.Union(DefaultSkipAttributeNames));

            var attributesMembersToExclude = excludeMembersAttributes == null ? DefaultSkipMemberAttributeNames : new HashSet<string>(excludeMembersAttributes.Union(DefaultSkipMemberAttributeNames));

            var assemblyPath = assembly.Location;
            var searchDirectories = new[]
            {
                Path.GetDirectoryName(assemblyPath),
                AppDomain.CurrentDomain.BaseDirectory,
                System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory()
            };

            var compilation = new EventBuilderCompiler(new IModuleReference[] { new PEFile(assemblyPath, PEStreamOptions.PrefetchMetadata) }, searchDirectories);

            Func<ITypeDefinition, bool> includeTypesFunc = tr => includeTypes == null || includeTypes.Any(t => t.FullName == tr.FullName);

            return CreatePublicApiForAssembly(compilation, includeTypesFunc, shouldIncludeAssemblyAttributes, whitelistedNamespacePrefixes, attributesToExclude, attributesMembersToExclude);
        }

        internal static string CreatePublicApiForAssembly(ICompilation compilation, Func<ITypeDefinition, bool> shouldIncludeType, bool shouldIncludeAssemblyAttributes, IEnumerable<string> whitelistedNamespacePrefixes, ISet<string> excludeAttributes, ISet<string> excludeMembersAttributes)
        {
            var compilationUnit = SyntaxFactory.CompilationUnit();

            var assemblyAttributes = compilation.MainModule.GetAssemblyAttributes().OrderBy(x => x.AttributeType.Name).Where(x => !excludeAttributes.Contains(x.AttributeType.FullName)).ToList();
            if (assemblyAttributes.Count > 0 && shouldIncludeAssemblyAttributes)
            {
                compilationUnit = AttributeGenerator.GenerateAssemblyCustomAttributes(compilation, compilationUnit, assemblyAttributes);
            }

            compilationUnit = GenerateNamespaces(compilation, compilationUnit, excludeAttributes, excludeMembersAttributes);

            return compilationUnit.NormalizeWhitespace().ToFullString();
        }

        internal static CompilationUnitSyntax GenerateNamespaces(ICompilation compilation, CompilationUnitSyntax compilationUnit, ISet<string> excludeAttributes, ISet<string> excludeMembersAttributes)
        {
            var namespaceProcessingStack = new Stack<INamespace>(new[] { compilation.RootNamespace });

            var list = new List<NamespaceDeclarationSyntax>();

            while (namespaceProcessingStack.Count > 0)
            {
                var namespaceInfo = namespaceProcessingStack.Pop();

                // Get a list of valid types that don't have attributes matching our exclude list.
                var validTypes = namespaceInfo.Types
                    .Where(x => SyntaxHelper.ShouldIncludeEntity(x, excludeMembersAttributes))
                    .OrderBy(x => x.Name)
                    .Select(x => GenerateMemberDeclaration(compilation, x, excludeAttributes, excludeMembersAttributes))
                    .ToList();

                if (validTypes.Count > 0)
                {
                    if (string.IsNullOrWhiteSpace(namespaceInfo.FullName))
                    {
                        compilationUnit = compilationUnit.WithMembers(SyntaxFactory.List(validTypes));
                        continue;
                    }

                    list.Add(SyntaxFactory.NamespaceDeclaration(SyntaxFactory.IdentifierName(namespaceInfo.FullName)).WithMembers(SyntaxFactory.List(validTypes)));
                }

                foreach (var child in namespaceInfo.ChildNamespaces)
                {
                    namespaceProcessingStack.Push(child);
                }
            }

            return compilationUnit.WithMembers(SyntaxFactory.List<MemberDeclarationSyntax>(list));
        }

        internal static MemberDeclarationSyntax GenerateMemberDeclaration(ICompilation compilation, ITypeDefinition typeDefinition, ISet<string> excludeAttributes, ISet<string> excludeMembersAttributes)
        {
            switch (typeDefinition.Kind)
            {
                case TypeKind.Class:
                    return GenerateTypeDeclaration(compilation, SyntaxFactory.ClassDeclaration(typeDefinition.Name), typeDefinition, excludeAttributes, excludeMembersAttributes);
                case TypeKind.Interface:
                    return GenerateTypeDeclaration(compilation, SyntaxFactory.InterfaceDeclaration(typeDefinition.Name), typeDefinition, excludeAttributes, excludeMembersAttributes);
                case TypeKind.Struct:
                    return GenerateTypeDeclaration(compilation, SyntaxFactory.StructDeclaration(typeDefinition.Name), typeDefinition, excludeAttributes, excludeMembersAttributes);
                case TypeKind.Delegate:
                    return TypeMemberGenerator.GenerateDelegateDeclaration(compilation, typeDefinition, excludeAttributes, excludeMembersAttributes);
                case TypeKind.Enum:
                    return GenerateEnumDeclaration(compilation, typeDefinition, excludeAttributes, excludeMembersAttributes);
            }

            throw new Exception(
                $"Cannot handle a class of type {typeDefinition.Kind} with name {typeDefinition.FullName}.");
        }

        internal static T GenerateTypeDeclaration<T>(ICompilation compilation, T item, ITypeDefinition typeDefinition, ISet<string> excludeAttributes, ISet<string> excludeMembersAttributes)
            where T : TypeDeclarationSyntax
        {
            return (T)item.WithModifiers(typeDefinition.GetModifiers())
                .WithAttributeLists(AttributeGenerator.GenerateAttributes(compilation, typeDefinition.GetAttributes().OrderBy(x => x.AttributeType.Name), excludeAttributes))
                .WithMembers(TypeMemberGenerator.GenerateMemberDeclarations(compilation, typeDefinition, excludeAttributes, excludeMembersAttributes));
        }

        internal static EnumDeclarationSyntax GenerateEnumDeclaration(ICompilation compilation, ITypeDefinition typeDefinition, ISet<string> excludeAttributes, ISet<string> excludeMembersAttributes)
        {
            var enumDeclaration = SyntaxFactory.EnumDeclaration(typeDefinition.Name)
                .WithModifiers(typeDefinition.GetModifiers())
                .WithAttributeLists(AttributeGenerator.GenerateAttributes(compilation, typeDefinition.GetAttributes(), excludeAttributes));

            if (typeDefinition.EnumUnderlyingType.FullName != "System.Int32")
            {
                enumDeclaration = enumDeclaration.WithBaseList(
                    SyntaxFactory.BaseList(
                        SyntaxFactory.SingletonSeparatedList<BaseTypeSyntax>(
                            SyntaxFactory.SimpleBaseType(
                                SyntaxHelper.EnumType(compilation, typeDefinition.EnumUnderlyingType)))));
            }

            var members = typeDefinition.Fields.Where(x => SyntaxHelper.ShouldIncludeEntity(x, excludeMembersAttributes)).Select(x =>
            {
                var enumMember = SyntaxFactory.EnumMemberDeclaration(x.Name)
                                    .WithAttributeLists(AttributeGenerator.GenerateAttributes(compilation, x.GetAttributes(), excludeAttributes));

                if (x.IsConst)
                {
                    enumMember = enumMember.WithEqualsValue(SyntaxFactory.EqualsValueClause(SyntaxHelper.LiteralParameterFromType(compilation, typeDefinition.EnumUnderlyingType, x.GetConstantValue())));
                }

                return enumMember;
            });

            return enumDeclaration.WithMembers(SyntaxFactory.SeparatedList(members));
        }
    }
}
