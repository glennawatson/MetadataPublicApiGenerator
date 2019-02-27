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
using TypeKind = ICSharpCode.Decompiler.TypeSystem.TypeKind;

namespace MetadataPublicApiGenerator
{
    /// <summary>
    /// Contains methods for generating the different types of a <see cref="ITypeDefinition"/>.
    /// </summary>
    internal static class TypeMemberGenerator
    {
        /// <summary>
        /// A dictionary of <see cref="SymbolKind"/> mapped to their corresponding weights.
        /// </summary>
        private static readonly IDictionary<SymbolKind, int> SymbolKindPreferredOrderWeights = new Dictionary<SymbolKind, int>
        {
            { SymbolKind.None, 15 },
            { SymbolKind.Module, 9 },
            { SymbolKind.TypeDefinition, 10 },
            { SymbolKind.Field, 1 },
            { SymbolKind.Property, 5 },
            { SymbolKind.Indexer, 6 },
            { SymbolKind.Event, 2 },
            { SymbolKind.Method, 8 },
            { SymbolKind.Operator, 7 },
            { SymbolKind.Constructor, 3 },
            { SymbolKind.Destructor, 4 },
            { SymbolKind.Accessor, 11 },
            { SymbolKind.Namespace, 0 },
            { SymbolKind.Variable, 12 },
            { SymbolKind.Parameter, 13 },
            { SymbolKind.TypeParameter, 14 },
        };

        private static readonly IDictionary<TypeKind, int> TypeKindPreferredOrderWeights = new Dictionary<TypeKind, int>
        {
            { TypeKind.Other, 21 },
            { TypeKind.Class, 3 },
            { TypeKind.Interface, 1 },
            { TypeKind.Struct, 4 },
            { TypeKind.Delegate, 0 },
            { TypeKind.Enum, 2 },
            { TypeKind.Void, 5 },
            { TypeKind.Unknown, 6 },
            { TypeKind.Null, 7 },
            { TypeKind.None, 8},
            { TypeKind.Dynamic, 9 },
            { TypeKind.UnboundTypeArgument, 10 },
            { TypeKind.TypeParameter, 11 },
            { TypeKind.Array, 12 },
            { TypeKind.Pointer, 13 },
            { TypeKind.ByReference, 14 },
            { TypeKind.Anonymous, 15 },
            { TypeKind.Intersection, 16 },
            { TypeKind.ArgList, 17 },
            { TypeKind.Tuple, 18 },
            { TypeKind.ModOpt, 19 },
            { TypeKind.ModReq, 20 },
        };

        internal static SyntaxList<MemberDeclarationSyntax> GenerateMemberDeclarations(ICompilation compilation, IEnumerable<ITypeDefinition> typeDefinitions, ISet<string> excludeAttributes, ISet<string> excludeMembersAttributes)
        {
            var list = new List<MemberDeclarationSyntax>();

            var validMembers = typeDefinitions.Where(x => SyntaxHelper.ShouldIncludeEntity(x, excludeMembersAttributes)).OrderBy(x => TypeKindPreferredOrderWeights[x.Kind]).ThenBy(x => x.Name).ToList();

            foreach (var typeDefinition in validMembers)
            {
                switch (typeDefinition.Kind)
                {
                    case TypeKind.Class:
                        list.Add(GenerateTypeDeclaration(compilation, SyntaxFactory.ClassDeclaration(typeDefinition.Name), typeDefinition, excludeAttributes, excludeMembersAttributes));
                        break;
                    case TypeKind.Interface:
                        list.Add(GenerateTypeDeclaration(compilation, SyntaxFactory.InterfaceDeclaration(typeDefinition.Name), typeDefinition, excludeAttributes, excludeMembersAttributes));
                        break;
                    case TypeKind.Struct:
                        list.Add(GenerateTypeDeclaration(compilation, SyntaxFactory.StructDeclaration(typeDefinition.Name), typeDefinition, excludeAttributes, excludeMembersAttributes));
                        break;
                    case TypeKind.Delegate:
                        list.Add(GenerateDelegateDeclaration(compilation, typeDefinition, excludeAttributes, excludeMembersAttributes));
                        break;
                    case TypeKind.Enum:
                        list.Add(EnumGenerator.GenerateEnumDeclaration(compilation, typeDefinition, excludeAttributes, excludeMembersAttributes));
                        break;
                    default:
                        throw new Exception($"Cannot handle a class of type {typeDefinition.Kind} with name {typeDefinition.FullName}.");
                }
            }

            return SyntaxFactory.List(list.Where(x => x != null));
        }

        /// <summary>
        /// Generates <see cref="TypeDeclarationSyntax"/> for the corresponding <see cref="ITypeDefinition"/>.
        /// </summary>
        /// <typeparam name="T">The derived class from <see cref="TypeDeclarationSyntax"/> which we are filling.</typeparam>
        /// <param name="compilation">The compilation which contains all the data about the assembly we are analyzing.</param>
        /// <param name="item">The item we are generating data for.</param>
        /// <param name="typeDefinition">The type definition we use to populate the type definition.</param>
        /// <param name="excludeAttributes">Any attributes we don't want to generate.</param>
        /// <param name="excludeMembersAttributes">Attributes we use to indicate we don't want to generate the members.</param>
        /// <returns>The fill in <see cref="TypeDeclarationSyntax"/>.</returns>
        internal static T GenerateTypeDeclaration<T>(ICompilation compilation, T item, ITypeDefinition typeDefinition, ISet<string> excludeAttributes, ISet<string> excludeMembersAttributes)
            where T : TypeDeclarationSyntax
        {
            item = (T)GenericParameterGeneratorHelper.GenerateGenericParameterList(compilation, typeDefinition, item);
            return (T)item.WithModifiers(typeDefinition.GetModifiers())
                .WithAttributeLists(AttributeGenerator.GenerateAttributes(compilation, typeDefinition.GetAttributes().OrderBy(x => x.AttributeType.Name), excludeAttributes))
                .WithMembers(GenerateMemberDeclaration(compilation, typeDefinition, excludeAttributes, excludeMembersAttributes));
        }

        internal static SyntaxList<MemberDeclarationSyntax> GenerateMemberDeclaration(ICompilation compilation, ITypeDefinition typeDefinition, ISet<string> excludeAttributes, ISet<string> excludeMembersAttributes)
        {
            var validMembers = typeDefinition.Members.Where(x => SyntaxHelper.ShouldIncludeEntity(x, excludeMembersAttributes)).OrderBy(x => SymbolKindPreferredOrderWeights[x.SymbolKind]).ThenBy(x => x.Name).ToList();

            if (validMembers.Count == 0)
            {
                return SyntaxFactory.List<MemberDeclarationSyntax>();
            }

            foreach (var member in validMembers)
            {
                switch (member.SymbolKind):
                    
            }

        }

        internal static DelegateDeclarationSyntax GenerateDelegateDeclaration(ICompilation compilation, ITypeDefinition typeDefinition, ISet<string> excludeAttributes, ISet<string> excludeMembersAttributes)
        {
            var invokeMember = typeDefinition.Methods.First(x => x.Name == "Invoke");

            return SyntaxFactory.DelegateDeclaration(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)), typeDefinition.Name)
                .WithModifiers(typeDefinition.GetModifiers())
                .WithParameterList(GenerateParameters(compilation, invokeMember, excludeAttributes));
        }

        internal static IList<MemberDeclarationSyntax> GenerateEventsDeclarations(ICompilation compilation, IEnumerable<IEvent> events, ISet<string> excludeAttributes, ISet<string> excludeMembersAttributes)
        {
            var validMembers = events.Where(x => SyntaxHelper.ShouldIncludeEntity(x, excludeMembersAttributes)).ToList();

            if (validMembers.Count == 0)
            {
                return Array.Empty<MemberDeclarationSyntax>();
            }

            return new List<MemberDeclarationSyntax>(validMembers.Select(x => SyntaxFactory.EventFieldDeclaration(
                    SyntaxFactory.VariableDeclaration(SyntaxFactory.IdentifierName(x.DeclaringType.GetRealType(compilation).GenerateFullGenericName(compilation)))
                        .WithVariables(SyntaxFactory.SingletonSeparatedList(SyntaxFactory.VariableDeclarator(SyntaxFactory.Identifier(x.Name)))))
                    .WithModifiers(x.GetModifiers())
                    .WithAttributeLists(AttributeGenerator.GenerateAttributes(compilation, x.GetAttributes(), excludeAttributes))));
        }

        internal static FieldDeclarationSyntax GenerateFieldDeclaration(ICompilation compilation, IField field, ISet<string> excludeAttributes, ISet<string> excludeMembersAttributes)
        {
            return null;
        }

        internal static BaseMethodDeclarationSyntax GenerateMethodDeclaration(ICompilation compilation, IMethod item, ISet<string> excludeAttributes, ISet<string> excludeMembersAttributes)
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
                            member = SyntaxFactory.ConversionOperatorDeclaration(SyntaxFactory.Token(SyntaxKind.ExplicitKeyword), SyntaxFactory.IdentifierName(item.ReturnType.GenerateFullGenericName(compilation)));
                            break;
                        case "op_Implicit":
                            member = SyntaxFactory.ConversionOperatorDeclaration(SyntaxFactory.Token(SyntaxKind.ImplicitKeyword), SyntaxFactory.IdentifierName(item.ReturnType.GenerateFullGenericName(compilation)));
                            break;
                        default:
                            member = SyntaxFactory.OperatorDeclaration(SyntaxFactory.IdentifierName(item.DeclaringType.GetRealType(compilation).FullName), SyntaxHelper.OperatorNameToToken(item.Name));
                            break;
                    }

                    break;
                default:
                    var method = SyntaxFactory.MethodDeclaration(SyntaxFactory.IdentifierName(item.DeclaringType.GetRealType(compilation).FullName), item.Name);
                    if (item.TypeParameters.Count > 0)
                    {
                        method = GenericParameterGeneratorHelper.GenerateGenericParameterList(compilation, item, method);
                    }

                    member = method;
                    break;
            }

            if (member != null)
            {
                return member
                    .WithAttributeLists(AttributeGenerator.GenerateAttributes(compilation, item.GetAttributes(), excludeAttributes))
                    .WithModifiers(item.GetModifiers())
                    .WithParameterList(GenerateParameters(compilation, item, excludeAttributes))
                    .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
            }

            return null;
        }

        internal static ParameterListSyntax GenerateParameters(ICompilation compilation, IMethod method, ISet<string> excludeAttributes)
        {
            var parameterList = new List<ParameterSyntax>();

            foreach (var parameter in method.Parameters)
            {
                parameterList.Add(
);
            }

            if (parameterList.Count == 0)
            {
                return SyntaxFactory.ParameterList();
            }

            return SyntaxFactory.ParameterList(SyntaxFactory.SeparatedList(parameterList));
        }
    }
}
