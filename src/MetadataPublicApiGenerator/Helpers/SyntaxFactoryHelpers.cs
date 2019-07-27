// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

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

        public static SyntaxTrivia Space => SyntaxFactory.Space;

        public static SyntaxTrivia CarriageReturnLineFeed => SyntaxFactory.CarriageReturnLineFeed;

        public static NamespaceDeclarationSyntax NamespaceDeclaration(string nameText, IReadOnlyCollection<MemberDeclarationSyntax> members)
        {
            var name = IdentifierName(nameText).AddLeadingSpaces();

            var membersList = List(GetIndentedNodes(members, 1));

            return SyntaxFactory.NamespaceDeclaration(SyntaxFactory.Token(SyntaxKind.NamespaceKeyword).AddLeadingNewLines(), name, SyntaxFactory.Token(SyntaxKind.OpenBraceToken).AddLeadingNewLines(), default, default, membersList, SyntaxFactory.Token(SyntaxKind.CloseBraceToken), default);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AttributeArgumentSyntax AttributeArgument(ExpressionSyntax expression)
        {
            return SyntaxFactory.AttributeArgument(expression);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AttributeArgumentSyntax AttributeArgument(NameEqualsSyntax nameEquals, ExpressionSyntax expression)
        {
            return SyntaxFactory.AttributeArgument(nameEquals, default, expression);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NameEqualsSyntax NameEquals(IdentifierNameSyntax name)
        {
            return SyntaxFactory.NameEquals(name);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IdentifierNameSyntax IdentifierName(string name)
        {
            return SyntaxFactory.IdentifierName(name);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AttributeSyntax Attribute(string name, IReadOnlyCollection<AttributeArgumentSyntax> arguments)
        {
            var argumentsList = AttributeArgumentList(arguments);

            return SyntaxFactory.Attribute(IdentifierName(name), argumentsList);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AttributeArgumentListSyntax AttributeArgumentList(IReadOnlyCollection<AttributeArgumentSyntax> arguments)
        {
            return SyntaxFactory.AttributeArgumentList(SeparatedList(arguments));
        }

        public static CompilationUnitSyntax CompilationUnit(IReadOnlyCollection<AttributeListSyntax> attributes, IReadOnlyCollection<MemberDeclarationSyntax> members)
        {
            var attributesList = List(GetIndentedNodes(attributes, 0));
            var membersList = members != null && members.Count > 0 ? List(members) : default;
            return SyntaxFactory.CompilationUnit(default, default, attributesList, membersList);
        }

        public static AttributeListSyntax AttributeList(AttributeSyntax attribute, SyntaxKind? target)
        {
            var attributeList = SyntaxFactory.SingletonSeparatedList(attribute);

            AttributeTargetSpecifierSyntax attributeTarget = null;
            if (target != null)
            {
                attributeTarget = SyntaxFactory.AttributeTargetSpecifier(SyntaxFactory.Token(target.Value), SyntaxFactory.Token(SyntaxKind.ColonToken).AddTrialingSpaces());
            }

            return SyntaxFactory.AttributeList(SyntaxFactory.Token(SyntaxKind.OpenBracketToken), attributeTarget, attributeList, SyntaxFactory.Token(SyntaxKind.CloseBracketToken));
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

            return SyntaxFactory.SeparatedList(GetIndentedNodes(nodes, level));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SeparatedSyntaxList<TNode> SingletonSeparatedList<TNode>(TNode node)
            where TNode : SyntaxNode
        {
            return SyntaxFactory.SingletonSeparatedList(node);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SyntaxToken Identifier(string text)
        {
            return SyntaxFactory.Identifier(text).AddLeadingSpaces();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VariableDeclaratorSyntax VariableDeclarator(string identifier)
        {
            var name = SyntaxFactory.Identifier(identifier);
            return SyntaxFactory.VariableDeclarator(name, default, default);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VariableDeclaratorSyntax VariableDeclarator(string identifier, EqualsValueClauseSyntax initializer)
        {
            var name = initializer == null ? SyntaxFactory.Identifier(identifier) : SyntaxFactory.Identifier(identifier).AddTrialingSpaces();
            return SyntaxFactory.VariableDeclarator(name, default, initializer);
        }

        public static EventFieldDeclarationSyntax EventFieldDeclaration(IReadOnlyCollection<AttributeListSyntax> attributes, IReadOnlyCollection<SyntaxKind> modifiers, VariableDeclarationSyntax declaration, int level)
        {
            var attributeList = List(GetIndentedNodes(attributes, level, true));
            var modifiersList = attributes != null && attributes.Count > 0 ? TokenList(modifiers, level) : TokenList(modifiers);

            return SyntaxFactory.EventFieldDeclaration(attributeList, modifiersList, SyntaxFactory.Token(SyntaxKind.EventKeyword).AddTrialingSpaces(), declaration, SyntaxFactory.Token(SyntaxKind.SemicolonToken));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VariableDeclarationSyntax VariableDeclaration(TypeSyntax type)
        {
            return SyntaxFactory.VariableDeclaration(type);
        }

        public static VariableDeclarationSyntax VariableDeclaration(TypeSyntax type, IReadOnlyCollection<VariableDeclaratorSyntax> variableDeclaratorSyntaxes)
        {
            var variableDeclaratorList = SeparatedList(variableDeclaratorSyntaxes);
            return SyntaxFactory.VariableDeclaration(type.AddTrialingSpaces(), variableDeclaratorList);
        }

        public static FieldDeclarationSyntax FieldDeclaration(IReadOnlyCollection<AttributeListSyntax> attributes, IReadOnlyCollection<SyntaxKind> modifiers, VariableDeclarationSyntax declaration, int level)
        {
            var attributeList = List(GetIndentedNodes(attributes, level, true));
            var modifiersList = attributes != null && attributes.Count > 0 ? TokenList(modifiers, level) : TokenList(modifiers);
            return SyntaxFactory.FieldDeclaration(attributeList, modifiersList, declaration, SyntaxFactory.Token(SyntaxKind.SemicolonToken));
        }

        public static ConstructorDeclarationSyntax ConstructorDeclaration(IReadOnlyCollection<AttributeListSyntax> attributes, IReadOnlyCollection<SyntaxKind> modifiers, IReadOnlyCollection<ParameterSyntax> parameters, string identifier, int level)
        {
            var name = SyntaxFactory.Identifier(identifier);
            var parametersList = SyntaxFactory.ParameterList(SeparatedList(parameters));
            var attributeList = List(GetIndentedNodes(attributes, level, true));
            var modifiersList = attributes != null && attributes.Count > 0 ? TokenList(modifiers, level) : TokenList(modifiers);
            return SyntaxFactory.ConstructorDeclaration(attributeList, modifiersList, name, parametersList, default, default, default, SyntaxFactory.Token(SyntaxKind.SemicolonToken));
        }

        public static DestructorDeclarationSyntax DestructorDeclaration(IReadOnlyCollection<AttributeListSyntax> attributes, IReadOnlyCollection<SyntaxKind> modifiers, string identifier, int level)
        {
            var name = SyntaxFactory.Identifier(identifier).AddLeadingSpaces();
            var attributeList = List(GetIndentedNodes(attributes, level, true));
            var modifiersList = attributes != null && attributes.Count > 0 ? TokenList(modifiers, level) : TokenList(modifiers);
            return SyntaxFactory.DestructorDeclaration(attributeList, modifiersList, SyntaxFactory.Token(SyntaxKind.TildeToken), name, SyntaxFactory.ParameterList(), default, default, SyntaxFactory.Token(SyntaxKind.SemicolonToken));
        }

        public static MethodDeclarationSyntax MethodDeclaration(IReadOnlyCollection<AttributeListSyntax> attributes, IReadOnlyCollection<SyntaxKind> modifiers, TypeSyntax type, ExplicitInterfaceSpecifierSyntax explicitInterface, string identifier, IReadOnlyCollection<ParameterSyntax> parameters, IReadOnlyCollection<TypeParameterConstraintClauseSyntax> typeParameterConstraintClauses, IReadOnlyCollection<TypeParameterSyntax> typeParameters, int level)
        {
            var name = SyntaxFactory.Identifier(identifier);
            if (explicitInterface == null)
            {
                name = name.AddLeadingSpaces();
            }

            var attributeList = List(GetIndentedNodes(attributes, level, true));
            var modifiersList = attributes != null && attributes.Count > 0 ? TokenList(modifiers, level) : TokenList(modifiers);

            var typeParameterList = TypeParameterList(typeParameters);
            var typeParameterConstraintList = typeParameterConstraintClauses != null && typeParameterConstraintClauses.Count > 0 ? List(GetIndentedNodes(typeParameterConstraintClauses, level + 1)) : default;

            var parametersList = SyntaxFactory.ParameterList(SeparatedList(parameters));

            explicitInterface = explicitInterface?.AddLeadingSpaces();

            return SyntaxFactory.MethodDeclaration(attributeList, modifiersList, type, explicitInterface, name, typeParameterList, parametersList, typeParameterConstraintList, default, default, SyntaxFactory.Token(SyntaxKind.SemicolonToken));
        }

        public static ConversionOperatorDeclarationSyntax ConversionOperatorDeclaration(IReadOnlyCollection<AttributeListSyntax> attributes, IReadOnlyCollection<SyntaxKind> modifiers, SyntaxToken implicitOrExplicitKeyword, string type, IReadOnlyCollection<ParameterSyntax> parameters, int level)
        {
            var attributeList = List(GetIndentedNodes(attributes, level, true));
            var modifiersList = attributes != null && attributes.Count > 0 ? TokenList(modifiers, level) : TokenList(modifiers);
            var typeName = SyntaxFactory.IdentifierName(type).AddLeadingSpaces();

            var parametersList = SyntaxFactory.ParameterList(SeparatedList(parameters));

            return SyntaxFactory.ConversionOperatorDeclaration(attributeList, modifiersList, implicitOrExplicitKeyword, SyntaxFactory.Token(SyntaxKind.OperatorKeyword), typeName, parametersList, default, default, SyntaxFactory.Token(SyntaxKind.SemicolonToken));
        }

        public static OperatorDeclarationSyntax OperatorDeclaration(IReadOnlyCollection<AttributeListSyntax> attributes, IReadOnlyCollection<SyntaxKind> modifiers, IReadOnlyCollection<ParameterSyntax> parameters, TypeSyntax returnType, SyntaxToken operatorToken, int level)
        {
            var attributeList = List(GetIndentedNodes(attributes, level, true));
            var modifiersList = attributes != null && attributes.Count > 0 ? TokenList(modifiers, level) : TokenList(modifiers);
            var parametersList = SyntaxFactory.ParameterList(SeparatedList(parameters));
            return SyntaxFactory.OperatorDeclaration(attributeList, modifiersList, returnType, SyntaxFactory.Token(SyntaxKind.OperatorKeyword), operatorToken, parametersList, default, default, SyntaxFactory.Token(SyntaxKind.SemicolonToken));
        }

        public static SyntaxToken Token(SyntaxKind kind)
        {
            return SyntaxFactory.Token(kind);
        }

        public static ParameterSyntax Parameter(IReadOnlyCollection<AttributeListSyntax> attributes, IReadOnlyCollection<SyntaxKind> modifiers, TypeSyntax type, string identifier, EqualsValueClauseSyntax equals)
        {
            var name = SyntaxFactory.Identifier(identifier);
            type = type.AddTrialingSpaces();
            var modifiersList = TokenList(modifiers);
            var attributesList = List(attributes);
            equals = equals?.AddLeadingSpaces();

            return SyntaxFactory.Parameter(attributesList, modifiersList, type, name, equals);
        }

        public static PropertyDeclarationSyntax PropertyDeclaration(TypeSyntax type, string identifier, IReadOnlyCollection<AttributeListSyntax> attributes, IReadOnlyCollection<SyntaxKind> modifiers, IReadOnlyCollection<AccessorDeclarationSyntax> accessors, int level)
        {
            var name = SyntaxFactory.Identifier(identifier).AddLeadingSpaces();
            var attributeList = List(GetIndentedNodes(attributes, level, true));
            var modifiersList = attributes != null && attributes.Count > 0 ? TokenList(modifiers, level) : TokenList(modifiers);
            var accessorList = SyntaxFactory.AccessorList(SyntaxFactory.Token(SyntaxKind.OpenBraceToken).AddTrialingSpaces().AddLeadingSpaces(), List(GetSpacedAccessors(accessors)), SyntaxFactory.Token(SyntaxKind.CloseBraceToken).AddLeadingSpaces());

            return SyntaxFactory.PropertyDeclaration(attributeList, modifiersList, type, default, name, accessorList, default, default, default);
        }

        public static AccessorDeclarationSyntax AccessorDeclaration(SyntaxKind kind, IReadOnlyCollection<AttributeListSyntax> attributes, IReadOnlyCollection<SyntaxKind> modifiers)
        {
            var modifiersList = TokenList(modifiers);
            var attributesList = List(attributes);
            return SyntaxFactory.AccessorDeclaration(kind, attributesList, modifiersList, default, default).WithSemicolonToken(Token(SyntaxKind.SemicolonToken));
        }

        public static TypeParameterSyntax TypeParameter(IReadOnlyCollection<AttributeListSyntax> attributes, SyntaxKind varianceKind, string identifier)
        {
            var name = SyntaxFactory.Identifier(identifier);
            var varianceKeyword = varianceKind == SyntaxKind.None ? default : SyntaxFactory.Token(varianceKind).AddTrialingSpaces();
            var attributesList = List(attributes);
            return SyntaxFactory.TypeParameter(attributesList, varianceKeyword, name);
        }

        public static EnumMemberDeclarationSyntax EnumMemberDeclaration(IReadOnlyCollection<AttributeListSyntax> attributes, string identifier, EqualsValueClauseSyntax equalsValue)
        {
            var attributesList = List(attributes);
            var name = SyntaxFactory.Identifier(identifier);
            return SyntaxFactory.EnumMemberDeclaration(attributesList, name, equalsValue);
        }

        public static BaseListSyntax BaseList(BaseTypeSyntax baseType)
        {
            if (baseType == null)
            {
                return default;
            }

            return SyntaxFactory.BaseList(SyntaxFactory.Token(SyntaxKind.ColonToken).AddLeadingSpaces().AddTrialingSpaces(), SingletonSeparatedList(baseType));
        }

        public static BaseListSyntax BaseList(IReadOnlyCollection<BaseTypeSyntax> baseItems)
        {
            if (baseItems == null || baseItems.Count == 0)
            {
                return default;
            }

            return SyntaxFactory.BaseList(SyntaxFactory.Token(SyntaxKind.ColonToken).AddLeadingSpaces(), SeparatedList(baseItems));
        }

        public static SimpleBaseTypeSyntax SimpleBaseType(TypeSyntax type)
        {
            return SyntaxFactory.SimpleBaseType(type);
        }

        public static GenericNameSyntax GenericName(string name, IReadOnlyCollection<TypeSyntax> types)
        {
            var typesList = types == null || types.Count == 0 ? default : SyntaxFactory.TypeArgumentList(SeparatedList(types));
            return SyntaxFactory.GenericName(Identifier(name), typesList);
        }

        public static EqualsValueClauseSyntax EqualsValueClause(ExpressionSyntax value)
        {
            return SyntaxFactory.EqualsValueClause(SyntaxFactory.Token(SyntaxKind.EqualsToken).AddLeadingSpaces().AddTrialingSpaces(), value);
        }

        public static DelegateDeclarationSyntax DelegateDeclaration(IReadOnlyCollection<AttributeListSyntax> attributes, IReadOnlyCollection<SyntaxKind> modifiers, TypeSyntax returnType, string identifier, IReadOnlyCollection<ParameterSyntax> parameters, IReadOnlyCollection<TypeParameterConstraintClauseSyntax> typeParameterConstraintClauses, IReadOnlyCollection<TypeParameterSyntax> typeParameters, int level)
        {
            var attributeList = List(GetIndentedNodes(attributes, level, true));
            var name = SyntaxFactory.Identifier(identifier).AddLeadingSpaces();
            var modifiersList = attributes != null && attributes.Count > 0 ? TokenList(modifiers, level) : TokenList(modifiers);
            var typeParameterList = TypeParameterList(typeParameters);
            var typeParameterConstraintList = List(typeParameterConstraintClauses);
            var parametersList = SyntaxFactory.ParameterList(SeparatedList(parameters));
            return SyntaxFactory.DelegateDeclaration(attributeList, modifiersList, SyntaxFactory.Token(SyntaxKind.DelegateKeyword), returnType.AddLeadingSpaces(), name, typeParameterList, parametersList, typeParameterConstraintList, SyntaxFactory.Token(SyntaxKind.SemicolonToken));
        }

        public static EnumDeclarationSyntax EnumDeclaration(string identifier, IReadOnlyCollection<AttributeListSyntax> attributes, IReadOnlyCollection<EnumMemberDeclarationSyntax> members, IReadOnlyCollection<SyntaxKind> modifiers, string baseIdentifier, int level)
        {
            var attributeList = List(GetIndentedNodes(attributes, level, true));
            var name = SyntaxFactory.Identifier(identifier).AddLeadingSpaces();
            var modifiersList = attributes != null && attributes.Count > 0 ? TokenList(modifiers, level) : TokenList(modifiers);
            var membersList = SeparatedList(members, level + 1);

            BaseListSyntax baseList = !string.IsNullOrWhiteSpace(baseIdentifier) ?
                BaseList(SyntaxFactory.SimpleBaseType(SyntaxFactory.IdentifierName(baseIdentifier))) :
                default;

            var (openingBrace, closingBrace) = GetBraces(level);
            return SyntaxFactory.EnumDeclaration(attributeList, modifiersList, SyntaxFactory.Token(SyntaxKind.EnumKeyword), name, baseList, openingBrace, membersList, closingBrace, default);
        }

        public static ClassDeclarationSyntax ClassDeclaration(string identifier, IReadOnlyCollection<AttributeListSyntax> attributes, IReadOnlyCollection<SyntaxKind> modifiers, IReadOnlyCollection<MemberDeclarationSyntax> members, IReadOnlyCollection<TypeParameterConstraintClauseSyntax> typeParameterConstraintClauses, IReadOnlyCollection<TypeParameterSyntax> typeParameters, IReadOnlyCollection<BaseTypeSyntax> bases, int level)
        {
            var classSyntax = SyntaxFactory.Token(SyntaxKind.ClassKeyword);
            GetTypeValues(identifier, attributes, modifiers, members, typeParameterConstraintClauses, typeParameters, bases, level, out var attributesList, out var name, out var modifiersList, out var baseList, out var membersList, out var typeParameterList, out var typeParameterConstraintList, out var openingBrace, out var closingBrace);

            return SyntaxFactory.ClassDeclaration(attributesList, modifiersList, classSyntax, name, typeParameterList, baseList, typeParameterConstraintList, openingBrace, membersList, closingBrace, default);
        }

        public static InterfaceDeclarationSyntax InterfaceDeclaration(string identifier, IReadOnlyCollection<AttributeListSyntax> attributes, IReadOnlyCollection<SyntaxKind> modifiers, IReadOnlyCollection<MemberDeclarationSyntax> members, IReadOnlyCollection<TypeParameterConstraintClauseSyntax> typeParameterConstraintClauses, IReadOnlyCollection<TypeParameterSyntax> typeParameters, IReadOnlyCollection<BaseTypeSyntax> bases, int level)
        {
            GetTypeValues(identifier, attributes, modifiers, members, typeParameterConstraintClauses, typeParameters, bases, level, out var attributesList, out var name, out var modifiersList, out var baseList, out var membersList, out var typeParameterList, out var typeParameterConstraintList, out var openingBrace, out var closingBrace);

            var typeIdentifier = SyntaxFactory.Token(SyntaxKind.InterfaceKeyword);

            return SyntaxFactory.InterfaceDeclaration(attributesList, modifiersList, typeIdentifier, name, typeParameterList, baseList, typeParameterConstraintList, openingBrace, membersList, closingBrace, default);
        }

        public static StructDeclarationSyntax StructDeclaration(string identifier, IReadOnlyCollection<AttributeListSyntax> attributes, IReadOnlyCollection<SyntaxKind> modifiers, IReadOnlyCollection<MemberDeclarationSyntax> members, IReadOnlyCollection<TypeParameterConstraintClauseSyntax> typeParameterConstraintClauses, IReadOnlyCollection<TypeParameterSyntax> typeParameters, IReadOnlyCollection<BaseTypeSyntax> bases, int level)
        {
            GetTypeValues(identifier, attributes, modifiers, members, typeParameterConstraintClauses, typeParameters, bases, level, out var attributesList, out var name, out var modifiersList, out var baseList, out var membersList, out var typeParameterList, out var typeParameterConstraintList, out var openingBrace, out var closingBrace);
            var typeIdentifier = SyntaxFactory.Token(SyntaxKind.StructKeyword);

            return SyntaxFactory.StructDeclaration(attributesList, modifiersList, typeIdentifier, name, typeParameterList, baseList, typeParameterConstraintList, openingBrace, membersList, closingBrace, default);
        }

        public static SyntaxTokenList TokenList(IReadOnlyCollection<SyntaxKind> tokens)
        {
            if (tokens == null || tokens.Count == 0)
            {
                return default;
            }

            var items = new List<SyntaxToken>(tokens.Count);

            foreach (var token in tokens)
            {
                items.Add(SyntaxFactory.Token(token).AddTrialingSpaces());
            }

            return SyntaxFactory.TokenList(items);
        }

        public static SyntaxTokenList TokenList(IReadOnlyCollection<SyntaxKind> tokens, int level)
        {
            if (tokens == null || tokens.Count == 0)
            {
                return default;
            }

            var items = new List<SyntaxToken>(tokens.Count);

            int i = 0;

            foreach (var token in tokens)
            {
                items.Add(i == 0 ? SyntaxFactory.Token(token).AddLeadingSpaces(level * LeadingSpacesPerLevel).AddTrialingSpaces() : SyntaxFactory.Token(token).AddTrialingSpaces());

                i++;
            }

            return SyntaxFactory.TokenList(items);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SyntaxList<TNode> List<TNode>()
            where TNode : SyntaxNode
        {
            return default;
        }

        public static SyntaxList<TNode> List<TNode>(IReadOnlyCollection<TNode> nodes)
            where TNode : SyntaxNode
        {
            return nodes == null || nodes.Count == 0 ? default : SyntaxFactory.List(nodes);
        }

        public static RefTypeSyntax RefType(TypeSyntax type, bool isReadOnly)
        {
            var readOnlySyntax = isReadOnly ? SyntaxFactory.Token(SyntaxKind.ReadOnlyKeyword).AddTrialingSpaces() : SyntaxFactory.Token(SyntaxKind.None);
            return SyntaxFactory.RefType(SyntaxFactory.Token(SyntaxKind.RefKeyword).AddTrialingSpaces(), readOnlySyntax, type);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ConstructorConstraintSyntax ConstructorConstraint()
        {
            return SyntaxFactory.ConstructorConstraint();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ClassOrStructConstraintSyntax ClassOrStructConstraint(SyntaxKind syntaxKind)
        {
            return SyntaxFactory.ClassOrStructConstraint(syntaxKind);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TypeConstraintSyntax TypeConstraint(string typeName)
        {
            return SyntaxFactory.TypeConstraint(SyntaxFactory.IdentifierName(typeName));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TypeParameterConstraintClauseSyntax TypeParameterConstraintClause(string name, IReadOnlyCollection<TypeParameterConstraintSyntax> constraints)
        {
            var constraintsList = SeparatedList(constraints);
            return SyntaxFactory.TypeParameterConstraintClause(SyntaxFactory.Token(SyntaxKind.WhereKeyword).AddLeadingSpaces().AddTrialingSpaces(), SyntaxFactory.IdentifierName(name), SyntaxFactory.Token(SyntaxKind.ColonToken).AddLeadingSpaces().AddTrialingSpaces(), constraintsList);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TypeParameterListSyntax TypeParameterList(IReadOnlyCollection<TypeParameterSyntax> parameters)
        {
            if (parameters == null || parameters.Count == 0)
            {
                return default;
            }

            return SyntaxFactory.TypeParameterList(SeparatedList(parameters));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BinaryExpressionSyntax BinaryExpression(SyntaxKind kind, ExpressionSyntax left, ExpressionSyntax right)
        {
            return SyntaxFactory.BinaryExpression(kind, left.AddTrialingSpaces(), right.AddLeadingSpaces());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MemberAccessExpressionSyntax MemberAccessExpression(SyntaxKind kind, ExpressionSyntax expression, SimpleNameSyntax name)
        {
            return SyntaxFactory.MemberAccessExpression(kind, expression, name);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ArrayCreationExpressionSyntax ArrayCreationExpression(ArrayTypeSyntax type)
        {
            return SyntaxFactory.ArrayCreationExpression(SyntaxFactory.Token(SyntaxKind.NewKeyword).AddTrialingSpaces(), type, default);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LiteralExpressionSyntax LiteralExpression(SyntaxKind syntaxKind)
        {
            return SyntaxFactory.LiteralExpression(syntaxKind);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LiteralExpressionSyntax LiteralExpression(SyntaxKind syntaxKind, SyntaxToken token)
        {
            return SyntaxFactory.LiteralExpression(syntaxKind, token);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SyntaxToken Literal(ulong value)
        {
            return SyntaxFactory.Literal(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SyntaxToken Literal(uint value)
        {
            return SyntaxFactory.Literal(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SyntaxToken Literal(int value)
        {
            return SyntaxFactory.Literal(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SyntaxToken Literal(long value)
        {
            return SyntaxFactory.Literal(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SyntaxToken Literal(char value)
        {
            return SyntaxFactory.Literal(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SyntaxToken Literal(double value)
        {
            return SyntaxFactory.Literal(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SyntaxToken Literal(float value)
        {
            return SyntaxFactory.Literal(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SyntaxToken Literal(string value)
        {
            return SyntaxFactory.Literal(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TypeOfExpressionSyntax TypeOfExpression(TypeSyntax type)
        {
            return SyntaxFactory.TypeOfExpression(SyntaxFactory.Token(SyntaxKind.TypeOfKeyword), SyntaxFactory.Token(SyntaxKind.OpenParenToken), type, SyntaxFactory.Token(SyntaxKind.CloseParenToken));
        }

        public static ExplicitInterfaceSpecifierSyntax ExplicitInterfaceSpecifier(string name)
        {
            return SyntaxFactory.ExplicitInterfaceSpecifier(IdentifierName(name));
        }

        private static IReadOnlyCollection<T> GetIndentedNodes<T>(IReadOnlyCollection<T> modifiers, int level, bool lastNodeTrailingLine = false)
            where T : SyntaxNode
        {
            var items = new List<T>(modifiers.Count);

            int i = 0;
            foreach (var modifier in modifiers)
            {
                var addModifier = modifier.AddLeadingNewLinesAndSpaces(1, level * LeadingSpacesPerLevel);
                if (i == modifiers.Count - 1 && lastNodeTrailingLine)
                {
                    addModifier = addModifier.AddTrialingNewLines();
                }

                items.Add(addModifier);

                i++;
            }

            return items;
        }

        private static (SyntaxToken openingBrace, SyntaxToken closingBrace) GetBraces(int level)
        {
            var openingBrace = SyntaxFactory.Token(SyntaxKind.OpenBraceToken).AddLeadingNewLinesAndSpaces(1, level * LeadingSpacesPerLevel);
            var closingBrace = SyntaxFactory.Token(SyntaxKind.CloseBraceToken).AddLeadingNewLinesAndSpaces(1, level * LeadingSpacesPerLevel);

            return (openingBrace, closingBrace);
        }

        private static IReadOnlyCollection<AccessorDeclarationSyntax> GetSpacedAccessors(IReadOnlyCollection<AccessorDeclarationSyntax> accessors)
        {
            int i = 0;

            var items = new List<AccessorDeclarationSyntax>(accessors.Count);

            foreach (var accessor in accessors)
            {
                var returnValue = accessor;
                if (i != accessors.Count - 1)
                {
                    returnValue = returnValue.AddTrialingSpaces();
                }

                items.Add(returnValue);
                i++;
            }

            return items;
        }

        private static void GetTypeValues(string identifier, IReadOnlyCollection<AttributeListSyntax> attributes, IReadOnlyCollection<SyntaxKind> modifiers, IReadOnlyCollection<MemberDeclarationSyntax> members, IReadOnlyCollection<TypeParameterConstraintClauseSyntax> typeParameterConstraintClauses, IReadOnlyCollection<TypeParameterSyntax> typeParameters, IReadOnlyCollection<BaseTypeSyntax> bases, int level, out SyntaxList<AttributeListSyntax> attributeList, out SyntaxToken name, out SyntaxTokenList modifiersList, out BaseListSyntax baseList, out SyntaxList<MemberDeclarationSyntax> membersList, out TypeParameterListSyntax typeParameterList, out SyntaxList<TypeParameterConstraintClauseSyntax> typeParameterConstraintList, out SyntaxToken openingBrace, out SyntaxToken closingBrace)
        {
            attributeList = List(GetIndentedNodes(attributes, level, true));
            name = SyntaxFactory.Identifier(identifier).AddLeadingSpaces();
            modifiersList = attributes != null && attributes.Count > 0 ? TokenList(modifiers, level) : TokenList(modifiers);
            baseList = BaseList(bases);
            membersList = members.Count > 0 ? List(GetIndentedNodes(members, level + 1)) : default;
            typeParameterList = TypeParameterList(typeParameters);
            typeParameterConstraintList = typeParameterConstraintClauses != null && typeParameterConstraintClauses.Count > 0 ? List(typeParameterConstraintClauses) : default;
            (openingBrace, closingBrace) = GetBraces(level);
        }
    }
}
