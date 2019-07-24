// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

using LightweightMetadata;
using LightweightMetadata.TypeWrappers;
using MetadataPublicApiGenerator.Generators;
using MetadataPublicApiGenerator.Generators.SymbolGenerators;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using static MetadataPublicApiGenerator.Helpers.SyntaxFactoryHelpers;

namespace MetadataPublicApiGenerator.Extensions
{
    internal static class SyntaxExtensions
    {
        public static IReadOnlyCollection<BaseTypeSyntax> GetBaseTypes(this TypeWrapper type)
        {
            var baseEntity = type.Base;
            var interfaces = type.InterfaceImplementations;

            var bases = new List<BaseTypeSyntax>(1 + interfaces.Count);

            if (baseEntity != null && baseEntity.KnownType != KnownTypeCode.Object)
            {
                bases.Add(SimpleBaseType(baseEntity.ReflectionFullName));
            }

            bases.AddRange(interfaces.Select(x => SimpleBaseType(x.ReflectionFullName)));

            return bases;
        }

        public static TypeSyntax GetTypeSyntax(this ITypeNamedWrapper wrapper)
        {
            if (wrapper is ArrayTypeWrapper arrayType)
            {
                if (arrayType.ArrayShapeData != null)
                {
                    var shapeExpressions = new List<int?>(arrayType.ArrayShapeData.Rank);
                    for (int i = 0; i < arrayType.ArrayShapeData.Rank; ++i)
                    {
                        int? size = arrayType.ArrayShapeData.Sizes.Count > 0 ? new int?(arrayType.ArrayShapeData.Sizes[i]) : 0;

                        shapeExpressions.Add(size);
                    }

                    return ArrayType(IdentifierName(arrayType.ElementType.ReflectionFullName), new[] { ArrayRankSpecifier(shapeExpressions) });
                }

                return ArrayType(IdentifierName(arrayType.ElementType.ReflectionFullName), null);
            }

            var type = IdentifierName(wrapper.ReflectionFullName);
            if (wrapper is ByReferenceWrapper)
            {
                return RefType(type, false);
            }

            if (wrapper is PointerWrapper)
            {
                return PointerType(type);
            }

            return type;
        }

        public static (IReadOnlyCollection<TypeParameterConstraintClauseSyntax> typeParameterConstraintClauses, IReadOnlyCollection<TypeParameterSyntax> typeParameters) GetTypeParameters(this IHasGenericParameters genericParameterContainer, ISet<string> excludeMembersAttributes, ISet<string> excludeAttributes)
        {
            if (genericParameterContainer.GenericParameters.Count == 0)
            {
                return default;
            }

            var constraintClauses = new List<TypeParameterConstraintClauseSyntax>(genericParameterContainer.GenericParameters.Count * 2);

            foreach (var genericParameter in genericParameterContainer.GenericParameters)
            {
                var constraints = new List<TypeParameterConstraintSyntax>(genericParameter.Constraints.Count + 3);
                if (genericParameter.HasDefaultConstructorConstraint)
                {
                    constraints.Add(ConstructorConstraint());
                }

                if (genericParameter.HasReferenceTypeConstraint)
                {
                    constraints.Add(ClassOrStructConstraint(SyntaxKind.ClassConstraint));
                }

                if (genericParameter.HasValueTypeConstraint)
                {
                    constraints.Add(ClassOrStructConstraint(SyntaxKind.StructConstraint));
                }

                constraints.AddRange(genericParameter.Constraints.Select(x => TypeConstraint(x.Type.ReflectionFullName)));

                if (constraints.Count > 0)
                {
                    constraintClauses.Add(TypeParameterConstraintClause(genericParameter.Name, constraints));
                }
            }

            var parameters = genericParameterContainer.GenericParameters.Select(x => TypeParameterSymbolGenerator.Generate(x, excludeMembersAttributes, excludeAttributes)).ToList();

            return (constraintClauses, parameters);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SyntaxToken AddLeadingNewLines(this SyntaxToken item, int number = 1)
        {
            if (number == 0)
            {
                return item;
            }

            var carriageReturnList = Enumerable.Repeat(CarriageReturnLineFeed, number);
            return item.WithLeadingTrivia(carriageReturnList);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SyntaxToken AddTrialingNewLines(this SyntaxToken item, int number = 1)
        {
            if (number == 0)
            {
                return item;
            }

            var carriageReturnList = Enumerable.Repeat(CarriageReturnLineFeed, number);
            return item.WithTrailingTrivia(carriageReturnList);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SyntaxToken AddLeadingNewLinesAndSpaces(this SyntaxToken item, int numberNewLines = 1, int numberSpaces = 1)
        {
            if (numberNewLines == 0 && numberSpaces == 0)
            {
                return item;
            }

            var carriageReturnList = Enumerable.Repeat(CarriageReturnLineFeed, numberNewLines);
            var leadingSpaces = Enumerable.Repeat(Space, numberSpaces);

            return item.WithLeadingTrivia(carriageReturnList.Concat(leadingSpaces));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SyntaxToken AddLeadingSpaces(this SyntaxToken item, int number = 1)
        {
            if (number == 0)
            {
                return item;
            }

            var leadingSpaces = Enumerable.Repeat(Space, number);
            return item.WithLeadingTrivia(leadingSpaces);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SyntaxToken AddTrialingSpaces(this SyntaxToken item, int number = 1)
        {
            if (number == 0)
            {
                return item;
            }

            var leadingSpaces = Enumerable.Repeat(Space, number);
            return item.WithTrailingTrivia(leadingSpaces);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T AddLeadingNewLinesAndSpaces<T>(this T item, int numberNewLines = 1, int numberSpaces = 1)
            where T : SyntaxNode
        {
            if (numberNewLines == 0 && numberSpaces == 0)
            {
                return item;
            }

            var carriageReturnList = Enumerable.Repeat(CarriageReturnLineFeed, numberNewLines);
            var leadingSpaces = Enumerable.Repeat(Space, numberSpaces);

            return item.WithLeadingTrivia(carriageReturnList.Concat(leadingSpaces));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T AddLeadingNewLines<T>(this T item, int number = 1)
            where T : SyntaxNode
        {
            if (number == 0)
            {
                return item;
            }

            var carriageReturnList = Enumerable.Repeat(CarriageReturnLineFeed, number);
            return item.WithLeadingTrivia(carriageReturnList);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T AddTrialingNewLines<T>(this T item, int number = 1)
            where T : SyntaxNode
        {
            if (number == 0)
            {
                return item;
            }

            var carriageReturnList = Enumerable.Repeat(CarriageReturnLineFeed, number);
            return item.WithTrailingTrivia(carriageReturnList);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T AddLeadingSpaces<T>(this T item, int number = 1)
            where T : SyntaxNode
        {
            if (number == 0)
            {
                return item;
            }

            var leadingSpaces = Enumerable.Repeat(Space, number);
            return item.WithLeadingTrivia(leadingSpaces);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T AddTrialingSpaces<T>(this T item, int number = 1)
            where T : SyntaxNode
        {
            if (number == 0)
            {
                return item;
            }

            var leadingSpaces = Enumerable.Repeat(Space, number);
            return item.WithTrailingTrivia(leadingSpaces);
        }
    }
}
