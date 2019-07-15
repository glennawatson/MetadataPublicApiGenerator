// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using LightweightMetadata;
using LightweightMetadata.TypeWrappers;
using MetadataPublicApiGenerator.Generators;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace MetadataPublicApiGenerator.Extensions
{
    internal static class SyntaxExtensions
    {
        public static TypeDeclarationSyntax AddBaseList(this TypeDeclarationSyntax typeSyntax, IHandleTypeNamedWrapper baseEntity, IReadOnlyList<InterfaceImplementationWrapper> interfaces)
        {
            var bases = new List<BaseTypeSyntax>(1 + interfaces.Count);

            if (baseEntity != null && baseEntity.KnownType != KnownTypeCode.Object)
            {
                bases.Add(SimpleBaseType(IdentifierName(baseEntity.ReflectionFullName)));
            }

            bases.AddRange(interfaces.Select(x => SimpleBaseType(IdentifierName(x.ReflectionFullName))));

            if (bases.Count != 0)
            {
                return typeSyntax.WithBaseList(BaseList(SeparatedList(bases)));
            }

            return typeSyntax;
        }

        public static TypeSyntax GetTypeSyntax(this IHandleTypeNamedWrapper wrapper)
        {
            var type = IdentifierName(wrapper.ReflectionFullName);
            if (wrapper is ByReferenceWrapper)
            {
                return RefType(type);
            }

            if (wrapper is PointerWrapper)
            {
                return PointerType(type);
            }

            if (wrapper is ArrayTypeWrapper arrayType)
            {
                if (arrayType.ArrayShapeData != null)
                {
                    var shapeExpressions = new List<ExpressionSyntax>();
                    for (int i = 0; i < arrayType.ArrayShapeData.Rank; ++i)
                    {
                        int? size = arrayType.ArrayShapeData.Sizes.Count > 0 ? new int?(arrayType.ArrayShapeData.Sizes[i]) : null;

                        shapeExpressions.Add(size == null ? LiteralExpression(SyntaxKind.None) : LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal((int)size)));
                    }

                    return ArrayType(IdentifierName(arrayType.ElementType.ReflectionFullName), SingletonList(ArrayRankSpecifier(SeparatedList(shapeExpressions))));
                }

                return ArrayType(IdentifierName(arrayType.ElementType.ReflectionFullName));
            }

            return type;
        }

        public static TypeDeclarationSyntax AddTypeParameters(this TypeDeclarationSyntax typeSyntax, IHasGenericParameters attributeContainer, IGeneratorFactory factory)
        {
            if (attributeContainer.GenericParameters.Count == 0)
            {
                return typeSyntax;
            }

            var (constraints, parameters) = attributeContainer.GetConstraints(factory);

            typeSyntax = typeSyntax
                .WithTypeParameterList(TypeParameterList(SeparatedList(parameters)));

            if (constraints.Count > 0)
            {
                typeSyntax = typeSyntax.WithConstraintClauses(List(constraints));
            }

            return typeSyntax;
        }

        public static MethodDeclarationSyntax AddTypeParameters(this MethodDeclarationSyntax typeSyntax, IHasGenericParameters attributeContainer, IGeneratorFactory factory)
        {
            if (attributeContainer.GenericParameters.Count == 0)
            {
                return typeSyntax;
            }

            var (constraints, parameters) = attributeContainer.GetConstraints(factory);

            typeSyntax = typeSyntax
                .WithTypeParameterList(TypeParameterList(SeparatedList(parameters)));

            if (constraints.Count > 0)
            {
                typeSyntax = typeSyntax.WithConstraintClauses(List(constraints));
            }

            return typeSyntax;
        }

        public static DelegateDeclarationSyntax AddTypeParameters(this DelegateDeclarationSyntax typeSyntax, IHasGenericParameters attributeContainer, IGeneratorFactory factory)
        {
            if (attributeContainer.GenericParameters.Count == 0)
            {
                return typeSyntax;
            }

            var (constraints, parameters) = attributeContainer.GetConstraints(factory);

            typeSyntax = typeSyntax
                .WithTypeParameterList(TypeParameterList(SeparatedList(parameters)));

            if (constraints.Count > 0)
            {
                typeSyntax = typeSyntax.WithConstraintClauses(List(constraints));
            }

            return typeSyntax;
        }

        private static (IReadOnlyList<TypeParameterConstraintClauseSyntax> constraintClauses, IEnumerable<TypeParameterSyntax> parameters) GetConstraints(this IHasGenericParameters genericParameterContainer, IGeneratorFactory factory)
        {
            var constraintClauses = new List<TypeParameterConstraintClauseSyntax>();

            foreach (var genericParameter in genericParameterContainer.GenericParameters)
            {
                var constraints = new List<TypeParameterConstraintSyntax>();
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

                constraints.AddRange(genericParameter.Constraints.Select(x => TypeConstraint(IdentifierName(x.Type.ReflectionFullName))));

                if (constraints.Count > 0)
                {
                    constraintClauses.Add(TypeParameterConstraintClause(IdentifierName(genericParameter.Name), SeparatedList(constraints)));
                }
            }

            var parameters = genericParameterContainer.GenericParameters.Select(factory.Generate<TypeParameterSyntax>);

            return (constraintClauses, parameters);
        }
    }
}
