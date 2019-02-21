// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.Decompiler.TypeSystem;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SymbolKind = ICSharpCode.Decompiler.TypeSystem.SymbolKind;

namespace MetadataPublicApiGenerator
{
    /// <summary>
    /// Contains methods for generating the different types of a <see cref="ITypeDefinition"/>.
    /// </summary>
    internal static class TypeMemberGenerator
    {
        internal static readonly IDictionary<SymbolKind, int> PreferredOrderWeights = new Dictionary<SymbolKind, int>
        {
            { SymbolKind.None,  10 },
            { SymbolKind.Module, 9 },
            { SymbolKind.TypeDefinition, 8 },
            { SymbolKind.Field, 7 },
            { SymbolKind.Property, 5 },
            { SymbolKind.Indexer, 4 },
            { SymbolKind.Event, 6 },
            { SymbolKind.Method, 3 },
            { SymbolKind.Operator, 2 },
            { SymbolKind.Constructor, 0 },
            { SymbolKind.Destructor, 1 },
            { SymbolKind.Accessor, 11 },
            { SymbolKind.Namespace, 12 },
            { SymbolKind.Variable, 13 },
            { SymbolKind.Parameter, 14 },
            { SymbolKind.TypeParameter, 15 },
        };

        internal static SyntaxList<MemberDeclarationSyntax> GenerateMemberDeclarations(ICompilation compilation, ITypeDefinition typeDefinition, ISet<string> excludeAttributes, ISet<string> excludeMembersAttributes)
        {
            return SyntaxFactory.List(GenerateEventsDeclarations(compilation, typeDefinition.Events.OrderBy(x => x.Name), excludeAttributes, excludeMembersAttributes)
                .Concat(GenerateMethodDeclarations(compilation, typeDefinition.Methods.OrderBy(x => PreferredOrderWeights[x.SymbolKind]).ThenBy(x => x.Name), excludeAttributes, excludeMembersAttributes)));
        }

        internal static DelegateDeclarationSyntax GenerateDelegateDeclaration(ICompilation compilation, ITypeDefinition typeDefinition, ISet<string> excludeAttributes, ISet<string> excludeMembersAttributes)
        {
            return SyntaxFactory.DelegateDeclaration(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)), typeDefinition.Name).WithModifiers(typeDefinition.GetModifiers());
        }

        internal static IList<MemberDeclarationSyntax> GenerateEventsDeclarations(ICompilation compilation, IEnumerable<IEvent> events, ISet<string> excludeAttributes, ISet<string> excludeMembersAttributes)
        {
            var validMembers = events.Where(x => SyntaxHelper.ShouldIncludeEntity(x, excludeMembersAttributes)).ToList();

            if (validMembers.Count == 0)
            {
                return Array.Empty<MemberDeclarationSyntax>();
            }

            return new List<MemberDeclarationSyntax>(validMembers.Select(x => SyntaxFactory.EventFieldDeclaration(
                    SyntaxFactory.VariableDeclaration(SyntaxFactory.IdentifierName(x.DeclaringType.GetRealType(compilation).GenerateFullGenericName()))
                        .WithVariables(SyntaxFactory.SingletonSeparatedList(SyntaxFactory.VariableDeclarator(SyntaxFactory.Identifier(x.Name)))))
                    .WithModifiers(x.GetModifiers())
                    .WithAttributeLists(AttributeGenerator.GenerateAttributes(compilation, x.GetAttributes(), excludeAttributes))));
        }

        internal static IEnumerable<BaseMethodDeclarationSyntax> GenerateMethodDeclarations(ICompilation compilation, IEnumerable<IMethod> methods, ISet<string> excludeAttributes, ISet<string> excludeMembersAttributes)
        {
            var validMembers = methods.Where(x => SyntaxHelper.ShouldIncludeEntity(x, excludeMembersAttributes)).ToList();

            var syntaxList = new List<BaseMethodDeclarationSyntax>();

            if (validMembers.Count == 0)
            {
                return Array.Empty<BaseMethodDeclarationSyntax>();
            }

            foreach (var item in validMembers)
            {
                BaseMethodDeclarationSyntax member;
                switch (item.SymbolKind)
                {
                    case SymbolKind.Constructor:
                        member = SyntaxFactory.ConstructorDeclaration(item.DeclaringTypeDefinition.Name);
                        break;
                    case SymbolKind.Destructor:
                        member = SyntaxFactory.DestructorDeclaration(item.DeclaringTypeDefinition.Name);
                        break;
                    case SymbolKind.Operator:
                        switch (item.Name)
                        {
                            case "op_Explicit":
                                member = SyntaxFactory.ConversionOperatorDeclaration(SyntaxFactory.Token(SyntaxKind.ExplicitKeyword), SyntaxFactory.IdentifierName(item.ReturnType.GenerateFullGenericName()));
                                break;
                            case "op_Implicit":
                                member = SyntaxFactory.ConversionOperatorDeclaration(SyntaxFactory.Token(SyntaxKind.ImplicitKeyword), SyntaxFactory.IdentifierName(item.ReturnType.GenerateFullGenericName()));
                                break;
                            default:
                                member = SyntaxFactory.OperatorDeclaration(SyntaxFactory.IdentifierName(item.DeclaringType.GetRealType(compilation).FullName), SyntaxHelper.OperatorNameToToken(item.Name));
                                break;
                        }

                        break;
                    default:
                        member = SyntaxFactory.MethodDeclaration(SyntaxFactory.IdentifierName(item.DeclaringType.GetRealType(compilation).FullName), item.Name);
                        break;
                }

                if (member != null)
                {
                    syntaxList.Add(member
                        .WithAttributeLists(AttributeGenerator.GenerateAttributes(compilation, item.GetAttributes(), excludeAttributes))
                        .WithModifiers(item.GetModifiers())
                        .WithParameterList(GenerateParameters(compilation, item, excludeAttributes))
                        .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)));
                }
            }

            return syntaxList;
        }

        internal static ParameterListSyntax GenerateParameters(ICompilation compilation, IMethod method, ISet<string> excludeAttributes)
        {
            var parameterList = new List<ParameterSyntax>();

            foreach (var parameter in method.Parameters)
            {
                parameterList.Add(
                    SyntaxFactory.Parameter(SyntaxFactory.Identifier(parameter.Name))
                        .WithModifiers(parameter.GetModifiers())
                        .WithAttributeLists(AttributeGenerator.GenerateAttributes(compilation, parameter.GetAttributes(), excludeAttributes))
                        .WithType(SyntaxFactory.IdentifierName(parameter.Type.GenerateFullGenericName())));
            }

            if (parameterList.Count == 0)
            {
                return SyntaxFactory.ParameterList();
            }

            return SyntaxFactory.ParameterList(SyntaxFactory.SeparatedList(parameterList));
        }
    }
}
