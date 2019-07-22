// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

using MetadataPublicApiGenerator.Extensions;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MetadataPublicApiGenerator.Helpers
{
    /// <summary>
    /// Versions of the syntax factory that handles some trivia for us automatically, to avoid the NormalizeWhitespace command.
    /// </summary>
    internal static class SyntaxFactoryHelpers
    {
        private const int LeadingSpacesPerLevel = 4;

        public static SyntaxTrivia CarriageReturn => SyntaxFactory.CarriageReturn;

        public static SyntaxTrivia Space => SyntaxFactory.Space;

        public static SyntaxTrivia CarriageReturnLineFeed => SyntaxFactory.CarriageReturnLineFeed;

        public static NamespaceDeclarationSyntax NamespaceDeclaration(string nameText)
        {
            var name = IdentifierName(nameText).AddLeadingSpaces();

            return SyntaxFactory.NamespaceDeclaration(SyntaxFactory.Token(SyntaxKind.NamespaceKeyword).AddLeadingNewLines(), name, SyntaxFactory.Token(SyntaxKind.OpenBraceToken).AddLeadingNewLines(), default, default, default, SyntaxFactory.Token(SyntaxKind.CloseBraceToken), default);
        }

        public static AttributeArgumentSyntax AttributeArgument(ExpressionSyntax expression)
        {
            return SyntaxFactory.AttributeArgument(expression);
        }

        public static AttributeArgumentSyntax AttributeArgument(NameEqualsSyntax nameEquals, ExpressionSyntax expression)
        {
            return SyntaxFactory.AttributeArgument(nameEquals, default, expression);
        }

        public static NameEqualsSyntax NameEquals(IdentifierNameSyntax name)
        {
            return SyntaxFactory.NameEquals(name);
        }

        public static IdentifierNameSyntax IdentifierName(string name)
        {
            return SyntaxFactory.IdentifierName(name);
        }

        public static AttributeSyntax Attribute(string name, IReadOnlyCollection<AttributeArgumentSyntax> arguments)
        {
            var argumentsList = AttributeArgumentList(SeparatedList(arguments));

            return SyntaxFactory.Attribute(IdentifierName(name), argumentsList);
        }

        public static AttributeArgumentListSyntax AttributeArgumentList(IEnumerable<AttributeArgumentSyntax> arguments)
        {
            return SyntaxFactory.AttributeArgumentList(SyntaxFactory.SeparatedList(arguments));
        }

        public static CompilationUnitSyntax CompilationUnit()
        {
            return SyntaxFactory.CompilationUnit();
        }

        public static AttributeListSyntax AttributeList(AttributeSyntax attribute, SyntaxKind? target, int level)
        {
            var attributeList = SyntaxFactory.SingletonSeparatedList(attribute);

            AttributeTargetSpecifierSyntax attributeTarget = null;
            if (target != null)
            {
                attributeTarget = SyntaxFactory.AttributeTargetSpecifier(SyntaxFactory.Token(target.Value), SyntaxFactory.Token(SyntaxKind.ColonToken).AddTrialingSpaces());
            }

            return SyntaxFactory.AttributeList(SyntaxFactory.Token(SyntaxKind.OpenBracketToken).AddLeadingNewLinesAndSpaces(1, level * LeadingSpacesPerLevel), attributeTarget, attributeList, SyntaxFactory.Token(SyntaxKind.CloseBracketToken));
        }

        public static SeparatedSyntaxList<TNode> SeparatedList<TNode>(IReadOnlyCollection<TNode> nodes)
            where TNode : SyntaxNode
        {
            if (nodes == null || nodes.Count == 0)
            {
                return default;
            }

            if (nodes.Count == 1)
            {
                return SingletonSeparatedList(nodes.First());
            }

            var commaSeparation = Enumerable.Repeat(SyntaxFactory.Token(SyntaxKind.CommaToken).AddTrialingSpaces(), nodes.Count - 1);
            return SyntaxFactory.SeparatedList(nodes, commaSeparation);
        }

        public static SeparatedSyntaxList<EnumMemberDeclarationSyntax> SeparatedList(IReadOnlyCollection<EnumMemberDeclarationSyntax> nodes, int level)
        {
            if (nodes == null || nodes.Count == 0)
            {
                return default;
            }

            if (nodes.Count == 1)
            {
                return SingletonSeparatedList(nodes.First());
            }

            return SyntaxFactory.SeparatedList(AddMemberSpacing(nodes, level));
        }

        public static SeparatedSyntaxList<TNode> SingletonSeparatedList<TNode>(TNode node)
            where TNode : SyntaxNode
        {
            return SyntaxFactory.SingletonSeparatedList(node);
        }

        public static SyntaxToken Identifier(string text)
        {
            return SyntaxFactory.Identifier(text).AddLeadingSpaces();
        }

        public static VariableDeclaratorSyntax VariableDeclarator(string identifier)
        {
            return SyntaxFactory.VariableDeclarator(SyntaxFactory.Identifier(identifier));
        }

        public static EventFieldDeclarationSyntax EventFieldDeclaration(IEnumerable<AttributeListSyntax> attributes, IEnumerable<SyntaxKind> modifiers, VariableDeclarationSyntax declaration, int level)
        {
            var modifiersList = SyntaxFactory.TokenList(GetSpacedModifiers(modifiers, level));
            var attributesList = SyntaxFactory.List(attributes);

            return SyntaxFactory.EventFieldDeclaration(attributesList, modifiersList, SyntaxFactory.Token(SyntaxKind.EventKeyword), declaration, SyntaxFactory.Token(SyntaxKind.SemicolonToken));
        }

        public static VariableDeclarationSyntax VariableDeclaration(TypeSyntax type)
        {
            return SyntaxFactory.VariableDeclaration(type);
        }

        public static VariableDeclarationSyntax VariableDeclaration(TypeSyntax type, IReadOnlyCollection<VariableDeclaratorSyntax> variableDeclaratorSyntaxes)
        {
            var variableDeclaratorList = SeparatedList(variableDeclaratorSyntaxes);
            return SyntaxFactory.VariableDeclaration(type, variableDeclaratorList);
        }

        public static FieldDeclarationSyntax FieldDeclaration(IEnumerable<AttributeListSyntax> attributes, IEnumerable<SyntaxKind> modifiers, VariableDeclarationSyntax declaration, int level)
        {
            var modifiersList = SyntaxFactory.TokenList(GetSpacedModifiers(modifiers, level));
            var attributesList = SyntaxFactory.List(attributes);
            return SyntaxFactory.FieldDeclaration(attributesList, modifiersList, declaration, SyntaxFactory.Token(SyntaxKind.SemicolonToken));
        }

        public static ConstructorDeclarationSyntax ConstructorDeclaration(IEnumerable<AttributeListSyntax> attributes, IReadOnlyCollection<SyntaxKind> modifiers, IReadOnlyCollection<ParameterSyntax> parameters, string identifier, int level)
        {
            var name = SyntaxFactory.Identifier(identifier).AddLeadingSpaces();
            var parametersList = SyntaxFactory.ParameterList(SeparatedList(parameters));
            var modifiersList = SyntaxFactory.TokenList(GetSpacedModifiers(modifiers, level));
            var attributesList = SyntaxFactory.List(attributes);
            return SyntaxFactory.ConstructorDeclaration(attributesList, modifiersList, name, parametersList, default, default, default, SyntaxFactory.Token(SyntaxKind.SemicolonToken));
        }

        public static DestructorDeclarationSyntax DestructorDeclaration(IEnumerable<AttributeListSyntax> attributes, IReadOnlyCollection<SyntaxKind> modifiers, string identifier, int level)
        {
            var name = SyntaxFactory.Identifier(identifier).AddLeadingSpaces();
            var modifiersList = SyntaxFactory.TokenList(GetSpacedModifiers(modifiers, level));
            var attributesList = SyntaxFactory.List(attributes);
            return SyntaxFactory.DestructorDeclaration(attributesList, modifiersList, SyntaxFactory.Token(SyntaxKind.TildeToken), name, SyntaxFactory.ParameterList(), default, default, SyntaxFactory.Token(SyntaxKind.SemicolonToken));
        }

        public static MethodDeclarationSyntax MethodDeclaration(IReadOnlyCollection<AttributeListSyntax> attributes, IReadOnlyCollection<SyntaxKind> modifiers, TypeSyntax type, string identifier, IReadOnlyCollection<ParameterSyntax> parameters, IReadOnlyCollection<TypeParameterConstraintClauseSyntax> typeParameterConstraintClauses, IReadOnlyCollection<TypeParameterSyntax> typeParameters, int level)
        {
            var name = SyntaxFactory.Identifier(identifier).AddLeadingSpaces();
            var modifiersList = modifiers.Count > 0 ? SyntaxFactory.TokenList(GetSpacedModifiers(modifiers, level)) : default;
            var attributesList = attributes.Count > 0 ? SyntaxFactory.List(attributes) : default;

            var typeParameterList = typeParameters != null && typeParameters.Count > 0 ? SyntaxFactory.TypeParameterList(SeparatedList(typeParameters)) : default;
            var typeParameterConstraintList = typeParameterConstraintClauses != null && typeParameterConstraintClauses.Count > 0 ? SyntaxFactory.List(typeParameterConstraintClauses) : default;

            var parametersList = SyntaxFactory.ParameterList(SeparatedList(parameters));

            return SyntaxFactory.MethodDeclaration(attributesList, modifiersList, type, default, name, typeParameterList, parametersList, typeParameterConstraintList, default, default, SyntaxFactory.Token(SyntaxKind.SemicolonToken));
        }

        public static ConversionOperatorDeclarationSyntax ConversionOperatorDeclaration(IReadOnlyCollection<AttributeListSyntax> attributes, IReadOnlyCollection<SyntaxKind> modifiers, SyntaxToken implicitOrExplicitKeyword, string type, IReadOnlyCollection<ParameterSyntax> parameters, int level)
        {
            var modifiersList = SyntaxFactory.TokenList(GetSpacedModifiers(modifiers, level));
            var attributesList = SyntaxFactory.List(attributes);
            var typeName = SyntaxFactory.IdentifierName(type).AddLeadingSpaces();

            var parametersList = SyntaxFactory.ParameterList(SeparatedList(parameters));

            return SyntaxFactory.ConversionOperatorDeclaration(attributesList, modifiersList, implicitOrExplicitKeyword, SyntaxFactory.Token(SyntaxKind.OperatorKeyword), typeName, parametersList, default, default, SyntaxFactory.Token(SyntaxKind.SemicolonToken));
        }

        public static OperatorDeclarationSyntax OperatorDeclaration(IReadOnlyCollection<AttributeListSyntax> attributes, IReadOnlyCollection<SyntaxKind> modifiers, IReadOnlyCollection<ParameterSyntax> parameters, TypeSyntax returnType, SyntaxToken operatorToken, int level)
        {
            var modifiersList = SyntaxFactory.TokenList(GetSpacedModifiers(modifiers, level));
            var attributesList = SyntaxFactory.List(attributes);
            var parametersList = SyntaxFactory.ParameterList(SeparatedList(parameters));
            return SyntaxFactory.OperatorDeclaration(attributesList, modifiersList, returnType, SyntaxFactory.Token(SyntaxKind.OperatorKeyword), operatorToken, parametersList, default, default, SyntaxFactory.Token(SyntaxKind.SemicolonToken));
        }

        public static ParameterListSyntax ParameterList(IReadOnlyCollection<ParameterSyntax> parameters)
        {
            return SyntaxFactory.ParameterList(SeparatedList(parameters));
        }

        public static SyntaxToken Token(SyntaxKind kind)
        {
            return SyntaxFactory.Token(kind);
        }

        public static ParameterSyntax Parameter(IEnumerable<AttributeListSyntax> attributes, IEnumerable<SyntaxKind> modifiers, TypeSyntax type, string identifier)
        {
            var name = SyntaxFactory.Identifier(identifier);
            type = type.AddTrialingSpaces();
            var modifiersList = SyntaxFactory.TokenList(modifiers.Select(SyntaxFactory.Token));
            var attributesList = SyntaxFactory.List(attributes);
            return SyntaxFactory.Parameter(attributesList, modifiersList, type, name, default);
        }

        public static PropertyDeclarationSyntax PropertyDeclaration(TypeSyntax type, string identifier, IReadOnlyCollection<AttributeListSyntax> attributes, IReadOnlyCollection<SyntaxKind> modifiers, IReadOnlyCollection<AccessorDeclarationSyntax> accessors, int level)
        {
            var name = SyntaxFactory.Identifier(identifier).AddLeadingSpaces();
            var modifiersList = SyntaxFactory.TokenList(GetSpacedModifiers(modifiers, level));
            var attributesList = SyntaxFactory.List(attributes);
            var accessorList = SyntaxFactory.AccessorList(SyntaxFactory.Token(SyntaxKind.OpenBraceToken).AddTrialingSpaces().AddLeadingSpaces(), SyntaxFactory.List(GetSpacedAccessors(accessors)), SyntaxFactory.Token(SyntaxKind.CloseBraceToken).AddLeadingSpaces());

            return SyntaxFactory.PropertyDeclaration(attributesList, modifiersList, type, default, name, accessorList, default, default, default);
        }

        public static AccessorListSyntax AccessorList(IEnumerable<AccessorDeclarationSyntax> accessors)
        {
            return SyntaxFactory.AccessorList(SyntaxFactory.List(accessors));
        }

        public static AccessorDeclarationSyntax AccessorDeclaration(SyntaxKind kind, IEnumerable<AttributeListSyntax> attributes, IEnumerable<SyntaxKind> modifiers)
        {
            var modifiersList = SyntaxFactory.TokenList(modifiers.Select(SyntaxFactory.Token));
            var attributesList = SyntaxFactory.List(attributes);
            return SyntaxFactory.AccessorDeclaration(kind, attributesList, modifiersList, default, default).WithSemicolonToken(Token(SyntaxKind.SemicolonToken));
        }

        public static TypeParameterSyntax TypeParameter(IEnumerable<AttributeListSyntax> attributes, SyntaxToken varianceKeyword, string identifier)
        {
            var name = SyntaxFactory.Identifier(identifier);
            var attributesList = SyntaxFactory.List(attributes);
            return SyntaxFactory.TypeParameter(attributesList, varianceKeyword, name);
        }

        public static EnumMemberDeclarationSyntax EnumMemberDeclaration(IReadOnlyCollection<AttributeListSyntax> attributes, string identifier, EqualsValueClauseSyntax equalsValue)
        {
            var attributesList = attributes.Count > 0 ? SyntaxFactory.List(attributes) : default;
            var name = SyntaxFactory.Identifier(identifier);
            return SyntaxFactory.EnumMemberDeclaration(attributesList, name, equalsValue);
        }

        public static BaseListSyntax BaseList(BaseTypeSyntax baseType)
        {
            return SyntaxFactory.BaseList(SingletonSeparatedList(baseType));
        }

        public static SimpleBaseTypeSyntax SimpleBaseType(string type)
        {
            return SyntaxFactory.SimpleBaseType(SyntaxFactory.IdentifierName(type));
        }

        public static EqualsValueClauseSyntax EqualsValueClause(ExpressionSyntax value)
        {
            return SyntaxFactory.EqualsValueClause(SyntaxFactory.Token(SyntaxKind.EqualsToken).AddLeadingSpaces().AddTrialingSpaces(), value);
        }

        public static ClassDeclarationSyntax ClassDeclaration(string identifier, IReadOnlyCollection<AttributeListSyntax> attributes, IReadOnlyCollection<SyntaxKind> modifiers, IReadOnlyCollection<MemberDeclarationSyntax> members, IReadOnlyCollection<TypeParameterConstraintClauseSyntax> typeParameterConstraintClauses, IReadOnlyCollection<TypeParameterSyntax> typeParameters, IReadOnlyCollection<BaseTypeSyntax> bases, int level)
        {
            var attributesList = attributes.Count > 0 ? SyntaxFactory.List(attributes) : default;
            var name = SyntaxFactory.Identifier(identifier).AddLeadingSpaces();
            var classSyntax = SyntaxFactory.Token(SyntaxKind.ClassKeyword);
            var modifiersList = modifiers.Count > 0 ? SyntaxFactory.TokenList(GetSpacedModifiers(modifiers, level)) : default;
            var baseList = bases.Count > 0 ? SyntaxFactory.BaseList(SeparatedList(bases)) : default;
            var membersList = members.Count > 0 ? SyntaxFactory.List(members) : default;
            var typeParameterList = typeParameters != null && typeParameters.Count > 0 ? SyntaxFactory.TypeParameterList(SeparatedList(typeParameters)) : default;
            var typeParameterConstraintList = typeParameterConstraintClauses != null && typeParameterConstraintClauses.Count > 0 ? SyntaxFactory.List(typeParameterConstraintClauses) : default;
            var (openingBrace, closingBrace) = GetBraces(level);
            return SyntaxFactory.ClassDeclaration(attributesList, modifiersList, classSyntax, name, typeParameterList, baseList, typeParameterConstraintList, openingBrace, membersList, closingBrace, default).AddLeadingSpaces(level * LeadingSpacesPerLevel);
        }

        public static DelegateDeclarationSyntax DelegateDeclaration(IEnumerable<AttributeListSyntax> attributes, IReadOnlyCollection<SyntaxKind> modifiers, string returnType, string identifier, IReadOnlyCollection<ParameterSyntax> parameters, IReadOnlyCollection<TypeParameterConstraintClauseSyntax> typeParameterConstraintClauses, IReadOnlyCollection<TypeParameterSyntax> typeParameters, int level)
        {
            var attributesList = SyntaxFactory.List(attributes);
            var name = SyntaxFactory.Identifier(identifier).AddLeadingSpaces();
            var type = SyntaxFactory.IdentifierName(returnType).AddLeadingSpaces();
            var modifiersList = SyntaxFactory.TokenList(GetSpacedModifiers(modifiers, level));
            var typeParameterList = SyntaxFactory.TypeParameterList(SeparatedList(typeParameters));
            var typeParameterConstraintList = SyntaxFactory.List(typeParameterConstraintClauses);
            var parametersList = SyntaxFactory.ParameterList(SeparatedList(parameters));
            return SyntaxFactory.DelegateDeclaration(attributesList, modifiersList, SyntaxFactory.Token(SyntaxKind.DelegateKeyword), type, name, typeParameterList, parametersList, typeParameterConstraintList, SyntaxFactory.Token(SyntaxKind.SemicolonToken));
        }

        public static EnumDeclarationSyntax EnumDeclaration(string identifier, IReadOnlyCollection<AttributeListSyntax> attributes, IReadOnlyCollection<EnumMemberDeclarationSyntax> members, IReadOnlyCollection<SyntaxKind> modifiers, string baseIdentifier, int level)
        {
            var attributesList = SyntaxFactory.List(attributes);
            var modifiersList = SyntaxFactory.TokenList(GetSpacedModifiers(modifiers, level));

            var membersList = SeparatedList(members, level + 1);
            var name = SyntaxFactory.Identifier(identifier).AddLeadingSpaces();

            BaseListSyntax baseList = default;
            if (!string.IsNullOrWhiteSpace(baseIdentifier))
            {
                baseList = SyntaxFactory.BaseList(SyntaxFactory.SingletonSeparatedList<BaseTypeSyntax>(SyntaxFactory.SimpleBaseType(SyntaxFactory.IdentifierName(baseIdentifier))));
            }

            var (openingBrace, closingBrace) = GetBraces(level);
            return SyntaxFactory.EnumDeclaration(attributesList, modifiersList, SyntaxFactory.Token(SyntaxKind.EnumKeyword), name, baseList, openingBrace, membersList, closingBrace, SyntaxFactory.Token(SyntaxKind.SemicolonToken));
        }

        public static InterfaceDeclarationSyntax InterfaceDeclaration(string identifier, IEnumerable<AttributeListSyntax> attributes, IEnumerable<SyntaxKind> modifiers, IEnumerable<MemberDeclarationSyntax> members, IEnumerable<TypeParameterConstraintClauseSyntax> typeParameterConstraintClauses, IEnumerable<TypeParameterSyntax> typeParameters, IEnumerable<BaseTypeSyntax> bases, int level)
        {
            var attributesList = SyntaxFactory.List(attributes);
            var name = SyntaxFactory.Identifier(identifier).AddLeadingSpaces();
            var modifiersList = SyntaxFactory.TokenList(GetSpacedModifiers(modifiers, level));
            var baseList = SyntaxFactory.BaseList(SyntaxFactory.SeparatedList(bases));
            var membersList = SyntaxFactory.List(members);
            var typeParameterList = SyntaxFactory.TypeParameterList(SyntaxFactory.SeparatedList(typeParameters));
            var typeParameterConstraintList = SyntaxFactory.List(typeParameterConstraintClauses);
            var (openingBrace, closingBrace) = GetBraces(level);
            var typeIdentifier = SyntaxFactory.Token(SyntaxKind.InterfaceKeyword);

            return SyntaxFactory.InterfaceDeclaration(attributesList, modifiersList, typeIdentifier, name, typeParameterList, baseList, typeParameterConstraintList, openingBrace, membersList, closingBrace, default(SyntaxToken));
        }

        public static StructDeclarationSyntax StructDeclaration(string identifier, IEnumerable<AttributeListSyntax> attributes, IEnumerable<SyntaxKind> modifiers, IEnumerable<MemberDeclarationSyntax> members, IEnumerable<TypeParameterConstraintClauseSyntax> typeParameterConstraintClauses, IEnumerable<TypeParameterSyntax> typeParameters, IEnumerable<BaseTypeSyntax> bases, int level)
        {
            var attributesList = SyntaxFactory.List(attributes);
            var name = SyntaxFactory.Identifier(identifier).AddLeadingSpaces();
            var modifiersList = SyntaxFactory.TokenList(GetSpacedModifiers(modifiers, level));
            var baseList = SyntaxFactory.BaseList(SyntaxFactory.SeparatedList(bases));
            var membersList = SyntaxFactory.List(members);
            var typeParameterList = SyntaxFactory.TypeParameterList(SyntaxFactory.SeparatedList(typeParameters));
            var typeParameterConstraintList = SyntaxFactory.List(typeParameterConstraintClauses);
            var (openingBrace, closingBrace) = GetBraces(level);
            var typeIdentifier = SyntaxFactory.Token(SyntaxKind.StructKeyword);

            return SyntaxFactory.StructDeclaration(attributesList, modifiersList, typeIdentifier, name, typeParameterList, baseList, typeParameterConstraintList, openingBrace, membersList, closingBrace, default(SyntaxToken));
        }

        public static SyntaxList<TNode> List<TNode>()
            where TNode : SyntaxNode
        {
            return SyntaxFactory.List<TNode>();
        }

        public static SyntaxList<TNode> List<TNode>(IEnumerable<TNode> nodes)
            where TNode : SyntaxNode
        {
            return SyntaxFactory.List(nodes);
        }

        public static RefTypeSyntax RefType(TypeSyntax type, bool isReadOnly)
        {
            var readOnlySyntax = isReadOnly ? SyntaxFactory.Token(SyntaxKind.ReadOnlyKeyword).AddTrialingSpaces() : SyntaxFactory.Token(SyntaxKind.None);
            return SyntaxFactory.RefType(SyntaxFactory.Token(SyntaxKind.RefKeyword).AddTrialingSpaces(), readOnlySyntax, type);
        }

        public static PointerTypeSyntax PointerType(TypeSyntax type)
        {
            return SyntaxFactory.PointerType(type);
        }

        public static ArrayTypeSyntax ArrayType(TypeSyntax elementType, IReadOnlyCollection<ArrayRankSpecifierSyntax> rankSpecifiers)
        {
            var rank = rankSpecifiers == null || rankSpecifiers.Count == 0 ? List(new[] { SyntaxFactory.ArrayRankSpecifier() }) : List(rankSpecifiers);
            return SyntaxFactory.ArrayType(elementType, rank);
        }

        public static ArrayRankSpecifierSyntax ArrayRankSpecifier(IReadOnlyCollection<int?> sizes)
        {
            var sizeSpecifier = sizes.Select(x => x == null ? SyntaxFactory.LiteralExpression(SyntaxKind.None) : SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(x.Value))).ToList();
            return SyntaxFactory.ArrayRankSpecifier(SeparatedList<ExpressionSyntax>(sizeSpecifier));
        }

        public static ConstructorConstraintSyntax ConstructorConstraint()
        {
            return SyntaxFactory.ConstructorConstraint();
        }

        public static ClassOrStructConstraintSyntax ClassOrStructConstraint(SyntaxKind syntaxKind)
        {
            return SyntaxFactory.ClassOrStructConstraint(syntaxKind);
        }

        public static TypeConstraintSyntax TypeConstraint(string typeName)
        {
            return SyntaxFactory.TypeConstraint(SyntaxFactory.IdentifierName(typeName));
        }

        public static TypeParameterConstraintClauseSyntax TypeParameterConstraintClause(string name, IReadOnlyCollection<TypeParameterConstraintSyntax> constraints)
        {
            var constraintsList = SeparatedList(constraints);
            return SyntaxFactory.TypeParameterConstraintClause(SyntaxFactory.IdentifierName(name), constraintsList);
        }

        private static IEnumerable<SyntaxToken> GetSpacedModifiers(IEnumerable<SyntaxKind> modifiers, int level)
        {
            int i = 0;
            foreach (var modifier in modifiers)
            {
                var modifierSyntax = SyntaxFactory.Token(modifier).AddTrialingSpaces();
                if (i == 0)
                {
                    modifierSyntax = modifierSyntax.AddLeadingNewLinesAndSpaces(1, level * LeadingSpacesPerLevel);
                }

                yield return modifierSyntax;
                i++;
            }
        }

        private static IEnumerable<T> AddMemberSpacing<T>(IReadOnlyCollection<T> modifiers, int level)
            where T : SyntaxNode
        {
            int i = 0;
            foreach (var modifier in modifiers)
            {
                yield return modifier.AddLeadingNewLinesAndSpaces(1, level * LeadingSpacesPerLevel);
                i++;
            }
        }

        private static (SyntaxToken openingBrace, SyntaxToken closingBrace) GetBraces(int level)
        {
            var openingBrace = SyntaxFactory.Token(SyntaxKind.OpenBraceToken).AddLeadingNewLinesAndSpaces(1, level * LeadingSpacesPerLevel).AddTrialingNewLines();
            var closingBrace = SyntaxFactory.Token(SyntaxKind.CloseBraceToken).AddLeadingNewLinesAndSpaces(1, level * LeadingSpacesPerLevel);

            return (openingBrace, closingBrace);
        }

        private static IEnumerable<AccessorDeclarationSyntax> GetSpacedAccessors(IReadOnlyCollection<AccessorDeclarationSyntax> accessors)
        {
            int i = 0;

            foreach (var accessor in accessors)
            {
                var returnValue = accessor;
                if (i != accessors.Count - 1)
                {
                    returnValue = returnValue.AddTrialingSpaces();
                }

                yield return returnValue;
                i++;
            }
        }
    }
}
