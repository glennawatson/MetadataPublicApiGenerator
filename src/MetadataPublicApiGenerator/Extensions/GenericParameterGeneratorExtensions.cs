// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using MetadataPublicApiGenerator.Compilation;
using MetadataPublicApiGenerator.Compilation.TypeWrappers;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MetadataPublicApiGenerator.Extensions
{
    internal static class GenericParameterGeneratorExtensions
    {
        internal static MethodDeclarationSyntax WithGenericParameterList(this MethodDeclarationSyntax methodSyntax, MethodWrapper method)
        {
            var constraintDictionary = method.Constraints;

            var parameterList = constraintDictionary.Select(constraint => SyntaxFactory.TypeParameter(constraint.Key)).ToList();

            if (parameterList.Count == 0)
            {
                return methodSyntax;
            }

            var typeConstraints = constraintDictionary.Select(
                kvp =>
                    SyntaxFactory.TypeParameterConstraintClause(kvp.Key)
                        .WithConstraints(SyntaxFactory.SeparatedList<TypeParameterConstraintSyntax>(kvp.Value.Select(c => SyntaxFactory.TypeConstraint(SyntaxFactory.IdentifierName(c))))));

            return methodSyntax.WithTypeParameterList(SyntaxFactory.TypeParameterList(SyntaxFactory.SeparatedList(parameterList))).WithConstraintClauses(SyntaxFactory.List(typeConstraints));
        }

        internal static DelegateDeclarationSyntax WithGenericParameterList(this DelegateDeclarationSyntax methodSyntax, MethodWrapper method)
        {
            var constraintDictionary = method.Constraints;

            var parameterList = constraintDictionary.Select(constraint => SyntaxFactory.TypeParameter(constraint.Key)).ToList();

            if (parameterList.Count == 0)
            {
                return methodSyntax;
            }

            var typeConstraints = constraintDictionary.Select(
                kvp =>
                    SyntaxFactory.TypeParameterConstraintClause(kvp.Key)
                        .WithConstraints(SyntaxFactory.SeparatedList<TypeParameterConstraintSyntax>(kvp.Value.Select(c => SyntaxFactory.TypeConstraint(SyntaxFactory.IdentifierName(c))))));

            return methodSyntax.WithTypeParameterList(SyntaxFactory.TypeParameterList(SyntaxFactory.SeparatedList(parameterList))).WithConstraintClauses(SyntaxFactory.List(typeConstraints));
        }

        internal static T WithGenericParameterList<T>(this T typeDeclarationSyntax, TypeWrapper type)
            where T : TypeDeclarationSyntax
        {
            var (constraintDictionary, parameterList) = GenerateTypeParameters(type);

            if (parameterList.Count == 0)
            {
                return typeDeclarationSyntax;
            }

            var typeConstraints = constraintDictionary.Where(x => x.Value.Any(y => y != null)).Select(
                kvp =>
                    SyntaxFactory.TypeParameterConstraintClause(kvp.Key)
                        .WithConstraints(SyntaxFactory.SeparatedList<TypeParameterConstraintSyntax>(kvp.Value.Where(c => c != null).Select(c => SyntaxFactory.TypeConstraint(SyntaxFactory.IdentifierName(c))))));

            return (T)typeDeclarationSyntax.WithTypeParameterList(SyntaxFactory.TypeParameterList(SyntaxFactory.SeparatedList(parameterList))).WithConstraintClauses(SyntaxFactory.List(typeConstraints));
        }

        private static (IReadOnlyDictionary<string, IReadOnlyList<string>> constraints, IReadOnlyCollection<TypeParameterSyntax> typeParams) GenerateTypeParameters(TypeWrapper typeWrapper)
        {
            var constraints = typeWrapper.GenericParameters.Select(x => (x.Name, x.Constraints));

            Dictionary<string, IReadOnlyList<string>> elements = new Dictionary<string, IReadOnlyList<string>>();
            var parameterList = new List<TypeParameterSyntax>();

            foreach (var constraint in constraints)
            {
                parameterList.Add(SyntaxFactory.TypeParameter(constraint.Name));

                elements.Add(constraint.Name, constraint.Constraints);
            }

            return (elements, parameterList);
        }
    }
}
