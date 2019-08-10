// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading.Tasks;

using LightweightMetadata;

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
        public static CompilationUnitSyntax Generate(MetadataRepository compilation, ISet<string> excludeMembersAttributes, ISet<string> excludeAttributes, Func<TypeWrapper, bool> excludeFunc, bool shouldIncludeAssemblyAttributes)
        {
            var attributes = shouldIncludeAssemblyAttributes ?
                                 Generate(compilation.MainAssemblyMetadata.MainAssembly.Attributes, excludeMembersAttributes, excludeAttributes, SyntaxKind.AssemblyKeyword) :
                                 Array.Empty<AttributeListSyntax>();

            compilation.MainAssemblyMetadata.ModuleDefinition.Attributes.TryGetNullableContext(out var currentNullability);

            var members = Generate(compilation.RootNamespace, excludeMembersAttributes, excludeAttributes, currentNullability, excludeFunc);
            return CompilationUnit(attributes, members);
        }

        public static MemberDeclarationSyntax Generate(TypeWrapper typeWrapper, ISet<string> excludeMembersAttributes, ISet<string> excludeAttributes, Func<TypeWrapper, bool> excludeFunc, Nullability currentNullability, int level)
        {
            switch (typeWrapper.TypeKind)
            {
                case SymbolTypeKind.Class:
                    return ClassDefinitionGenerator.Generate(typeWrapper, excludeMembersAttributes, excludeAttributes, excludeFunc, currentNullability, level);
                case SymbolTypeKind.Delegate:
                    return DelegateTypeGenerator.Generate(typeWrapper, excludeMembersAttributes, excludeAttributes, excludeFunc, currentNullability, level);
                case SymbolTypeKind.Enum:
                    return EnumTypeGenerator.Generate(typeWrapper, excludeMembersAttributes, excludeAttributes, excludeFunc, level);
                case SymbolTypeKind.Interface:
                    return InterfaceTypeGenerator.Generate(typeWrapper, excludeMembersAttributes, excludeAttributes, excludeFunc, currentNullability, level);
                case SymbolTypeKind.Struct:
                    return StructTypeGenerator.Generate(typeWrapper, excludeMembersAttributes, excludeAttributes, excludeFunc, currentNullability, level);
            }

            return null;
        }

        public static TOutput Generate<TOutput>(IHandleTypeNamedWrapper wrapper, ISet<string> excludeMembersAttributes, ISet<string> excludeAttributes, Func<TypeWrapper, bool> excludeFunc, Nullability currentNullability, int level)
            where TOutput : SyntaxNode
        {
            switch (wrapper.Handle.Kind)
            {
                case HandleKind.CustomAttribute:
                    return AttributeSymbolGenerator.Generate(wrapper) as TOutput;
                case HandleKind.EventDefinition:
                    return EventSymbolGenerator.Generate(wrapper, excludeMembersAttributes, excludeAttributes, currentNullability, level) as TOutput;
                case HandleKind.FieldDefinition:
                    return FieldSymbolGenerator.Generate(wrapper, excludeMembersAttributes, excludeAttributes, currentNullability, level) as TOutput;
                case HandleKind.MethodDefinition:
                    return MethodSymbolGenerator.Generate(wrapper, excludeMembersAttributes, excludeAttributes, currentNullability, level) as TOutput;
                case HandleKind.PropertyDefinition:
                    return PropertySymbolGenerator.Generate(wrapper, excludeMembersAttributes, excludeAttributes, currentNullability, level) as TOutput;
                case HandleKind.GenericParameter:
                    return TypeParameterSymbolGenerator.Generate(wrapper, excludeMembersAttributes, excludeAttributes) as TOutput;
                case HandleKind.TypeDefinition:
                    return Generate(wrapper as TypeWrapper, excludeMembersAttributes, excludeAttributes, excludeFunc, currentNullability, level) as TOutput;
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

        public static IReadOnlyCollection<MemberDeclarationSyntax> Generate(NamespaceWrapper root, ISet<string> excludeMembersAttributes, ISet<string> excludeAttributes, Nullability currentNullability, Func<TypeWrapper, bool> excludeFunc)
        {
            var namespaces = GetAllNamespaces(root)
                .AsParallel()
                .Select(namespaceInfo =>
                    (namespaceInfo, members: namespaceInfo
                        .Members
                        .OrderByAndExclude(excludeMembersAttributes, excludeAttributes)
                        .Where(x => !x.IsNested)
                        .ToList()))
                .Where(x => x.members.Count > 0)
                .OrderBy(x => x.namespaceInfo.FullName)
                .ToList();

            var output = new List<MemberDeclarationSyntax>[namespaces.Count];

            Parallel.For(
                0,
                namespaces.Count,
                namespaceIndex =>
                {
                    var (namespaceInfo, types) = namespaces[namespaceIndex];

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
                                members[i] = Generate(current, excludeMembersAttributes, excludeAttributes, excludeFunc, currentNullability, 1).AddTrialingNewLines().AddLeadingNewLines(i == 0 ? 1 : 0);
                            });

                    var namespaceName = namespaceInfo.FullName;
                    if (string.IsNullOrEmpty(namespaceName))
                    {
                        list.AddRange(members);
                    }
                    else
                    {
                        list.Add(NamespaceDeclaration(namespaceName, members, namespaceIndex != 0));
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