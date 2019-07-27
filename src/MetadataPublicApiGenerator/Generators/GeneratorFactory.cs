// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading.Tasks;

using LightweightMetadata;
using LightweightMetadata.TypeWrappers;

using MetadataPublicApiGenerator.Extensions;
using MetadataPublicApiGenerator.Generators.SymbolGenerators;
using MetadataPublicApiGenerator.Generators.TypeGenerators;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using static MetadataPublicApiGenerator.Helpers.SyntaxFactoryHelpers;

namespace MetadataPublicApiGenerator.Generators
{
    /// <summary>
    /// A factory which will produce generators that create CSharp syntax for Roslyn.
    /// </summary>
    internal static class GeneratorFactory
    {
        public static CompilationUnitSyntax Generate(IMetadataRepository compilation, ISet<string> excludeMembersAttributes, ISet<string> excludeAttributes, Func<TypeWrapper, bool> excludeFunc, bool shouldIncludeAssemblyAttributes)
        {
            var attributes = shouldIncludeAssemblyAttributes ?
                                 Generate(compilation.MainModule.MainAssembly.Attributes, excludeMembersAttributes, excludeAttributes, SyntaxKind.AssemblyKeyword) :
                                 Array.Empty<AttributeListSyntax>();

            var members = Generate(compilation.RootNamespace, excludeMembersAttributes, excludeAttributes, excludeFunc);
            return CompilationUnit(attributes, members);
        }

        public static MemberDeclarationSyntax Generate(TypeWrapper typeWrapper, ISet<string> excludeMembersAttributes, ISet<string> excludeAttributes, Func<TypeWrapper, bool> excludeFunc, int level)
        {
            switch (typeWrapper.TypeKind)
            {
                case SymbolTypeKind.Class:
                    return ClassDefinitionGenerator.Generate(typeWrapper, excludeMembersAttributes, excludeAttributes, excludeFunc, level);
                case SymbolTypeKind.Delegate:
                    return DelegateTypeGenerator.Generate(typeWrapper, excludeMembersAttributes, excludeAttributes, excludeFunc, level);
                case SymbolTypeKind.Enum:
                    return EnumTypeGenerator.Generate(typeWrapper, excludeMembersAttributes, excludeAttributes, excludeFunc, level);
                case SymbolTypeKind.Interface:
                    return InterfaceTypeGenerator.Generate(typeWrapper, excludeMembersAttributes, excludeAttributes, excludeFunc, level);
                case SymbolTypeKind.Struct:
                    return StructTypeGenerator.Generate(typeWrapper, excludeMembersAttributes, excludeAttributes, excludeFunc, level);
            }

            return null;
        }

        public static TOutput Generate<TOutput>(IHandleWrapper wrapper, ISet<string> excludeMembersAttributes, ISet<string> excludeAttributes, int level)
            where TOutput : SyntaxNode
        {
            switch (wrapper.Handle.Kind)
            {
                case HandleKind.CustomAttribute:
                    return AttributeSymbolGenerator.Generate(wrapper) as TOutput;
                case HandleKind.EventDefinition:
                    return EventSymbolGenerator.Generate(wrapper, excludeMembersAttributes, excludeAttributes, level) as TOutput;
                case HandleKind.FieldDefinition:
                    return FieldSymbolGenerator.Generate(wrapper, excludeMembersAttributes, excludeAttributes, level) as TOutput;
                case HandleKind.MethodDefinition:
                    return MethodSymbolGenerator.Generate(wrapper, excludeMembersAttributes, excludeAttributes, level) as TOutput;
                case HandleKind.PropertyDefinition:
                    return PropertySymbolGenerator.Generate(wrapper, excludeMembersAttributes, excludeAttributes, level) as TOutput;
                case HandleKind.GenericParameter:
                    return TypeParameterSymbolGenerator.Generate(wrapper, excludeMembersAttributes, excludeAttributes) as TOutput;
            }

            return null;
        }

        public static IReadOnlyCollection<AttributeListSyntax> Generate(IEnumerable<AttributeWrapper> attributes, ISet<string> excludeMembersAttributes, ISet<string> excludeAttributes, SyntaxKind? target = null)
        {
            return attributes
                .OrderByAndExclude(excludeMembersAttributes, excludeAttributes)
                .Select(AttributeSymbolGenerator.Generate)
                .Where(x => x != null)
                .Select(x => AttributeList(x, target))
                .ToList();
        }

        public static IReadOnlyCollection<MemberDeclarationSyntax> Generate(NamespaceWrapper root, ISet<string> excludeMembersAttributes, ISet<string> excludeAttributes, Func<TypeWrapper, bool> excludeFunc)
        {
            var namespaces = GetAllNamespaces(root).ToList();
            var output = new List<MemberDeclarationSyntax>[namespaces.Count];

            Parallel.For(0, namespaces.Count, namespaceIndex =>
                {
                    var namespaceInfo = namespaces[namespaceIndex];

                    // Get a list of valid types that don't have attributes matching our exclude list.
                    var types = namespaceInfo
                        .Members
                        .OrderByAndExclude(excludeMembersAttributes, excludeAttributes)
                        .Where(x => !x.IsNested)
                        .ToList();

                    if (types.Count == 0)
                    {
                        return;
                    }

                    var list = new List<MemberDeclarationSyntax>(types.Count);
                    output[namespaceIndex] = list;

                    var members = new MemberDeclarationSyntax[types.Count];

                    Parallel.For(
                        0,
                        types.Count,
                        i =>
                            {
                                var current = types[i];
                                members[i] = Generate(current, excludeMembersAttributes, excludeAttributes, excludeFunc, 1).AddTrialingNewLines().AddLeadingNewLines(i == 0 ? 1 : 0);
                            });

                    var namespaceName = namespaceInfo.FullName;
                    if (string.IsNullOrEmpty(namespaceName))
                    {
                        list.AddRange(members);
                    }
                    else
                    {
                        list.Add(NamespaceDeclaration(namespaceName, members));
                    }
                });

            return output.Where(x => x != null).SelectMany(x => x).ToList();
        }

        private static IEnumerable<NamespaceWrapper> GetAllNamespaces(NamespaceWrapper rootNamespace)
        {
            var namespaceProcessingStack = new Stack<NamespaceWrapper>(new[] { rootNamespace });

            while (namespaceProcessingStack.Count > 0)
            {
                var current = namespaceProcessingStack.Pop();

                yield return current;

                foreach (var child in current.ChildNamespaces.OrderByDescending(x => x.Name))
                {
                    namespaceProcessingStack.Push(child);
                }
            }
        }
    }
}