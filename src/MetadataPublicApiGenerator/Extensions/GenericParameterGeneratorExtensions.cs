// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using MetadataPublicApiGenerator.Compilation;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MetadataPublicApiGenerator.Extensions
{
    internal static class GenericParameterGeneratorExtensions
    {
        internal static MethodDeclarationSyntax WithGenericParameterList(this MethodDeclarationSyntax methodSyntax, CompilationModule compilation, MethodDefinition method)
        {
            var (constraintDictionary, parameterList) = GenerateTypeParameters(compilation, method.GetGenericParameters());

            if (parameterList.Count == 0)
            {
                return methodSyntax;
            }

            var typeConstraints = constraintDictionary.Select(kvp =>
                SyntaxFactory.TypeParameterConstraintClause(kvp.Key)
                    .WithConstraints(SyntaxFactory.SeparatedList<TypeParameterConstraintSyntax>(kvp.Value.Select(c => SyntaxFactory.TypeConstraint(SyntaxFactory.IdentifierName(c))))));

            return methodSyntax.WithTypeParameterList(SyntaxFactory.TypeParameterList(SyntaxFactory.SeparatedList(parameterList))).WithConstraintClauses(SyntaxFactory.List(typeConstraints));
        }

        internal static DelegateDeclarationSyntax WithGenericParameterList(this DelegateDeclarationSyntax methodSyntax, CompilationModule compilation, MethodDefinition method)
        {
            var (constraintDictionary, parameterList) = GenerateTypeParameters(compilation, method.GetGenericParameters());

            if (parameterList.Count == 0)
            {
                return methodSyntax;
            }

            var typeConstraints = constraintDictionary.Select(kvp =>
                SyntaxFactory.TypeParameterConstraintClause(kvp.Key)
                    .WithConstraints(SyntaxFactory.SeparatedList<TypeParameterConstraintSyntax>(kvp.Value.Select(c => SyntaxFactory.TypeConstraint(SyntaxFactory.IdentifierName(c))))));

            return methodSyntax.WithTypeParameterList(SyntaxFactory.TypeParameterList(SyntaxFactory.SeparatedList(parameterList))).WithConstraintClauses(SyntaxFactory.List(typeConstraints));
        }

        internal static T WithGenericParameterList<T>(this T typeDeclarationSyntax, CompilationModule compilation, TypeDefinitionHandle type)
            where T : TypeDeclarationSyntax
        {
            return typeDeclarationSyntax.WithGenericParameterList(compilation, type.Resolve(compilation));
        }

        internal static T WithGenericParameterList<T>(this T typeDeclarationSyntax, CompilationModule compilation, TypeDefinition type)
            where T : TypeDeclarationSyntax
        {
            var (constraintDictionary, parameterList) = GenerateTypeParameters(compilation, type.GetGenericParameters());

            if (parameterList.Count == 0)
            {
                return typeDeclarationSyntax;
            }

            var typeConstraints = constraintDictionary.Where(x => x.Value.Any(y => y != null)).Select(kvp =>
                SyntaxFactory.TypeParameterConstraintClause(kvp.Key)
                    .WithConstraints(SyntaxFactory.SeparatedList<TypeParameterConstraintSyntax>(kvp.Value.Where(c => c != null).Select(c => SyntaxFactory.TypeConstraint(SyntaxFactory.IdentifierName(c))))));

            return (T)typeDeclarationSyntax.WithTypeParameterList(SyntaxFactory.TypeParameterList(SyntaxFactory.SeparatedList(parameterList))).WithConstraintClauses(SyntaxFactory.List(typeConstraints));
        }

        private static (IDictionary<string, ISet<string>> constraints, IReadOnlyCollection<TypeParameterSyntax> typeParams) GenerateTypeParameters(CompilationModule compilation, IReadOnlyList<GenericParameterHandle> typeParameterHandles)
        {
            var parameterList = new List<TypeParameterSyntax>();
            var constraintDictionary = new Dictionary<string, ISet<string>>();
            foreach (var typeParameterHandle in typeParameterHandles)
            {
                var typeParameter = typeParameterHandle.Resolve(compilation);
                var typeParameterSyntax = SyntaxFactory.TypeParameter(typeParameter.GetName(compilation));
                foreach (var constraint in typeParameter.GetConstraints().Select(x => x.Resolve(compilation)))
                {
                    var parameter = constraint.Parameter.Resolve(compilation);
                    var parameterName = parameter.Name.GetName(compilation);

                    if (constraint.Type.IsNil)
                    {
                        continue;
                    }

                    var constraintTypeName = constraint.Type.GetFullName(compilation);
                    if (constraintTypeName != "System.Object")
                    {
                        if (!constraintDictionary.TryGetValue(parameterName, out var constraints))
                        {
                            constraints = new HashSet<string>();
                            constraintDictionary[parameterName] = constraints;
                        }

                        constraints.Add(constraintTypeName);
                    }
                }

                parameterList.Add(typeParameterSyntax);
            }

            return (constraintDictionary, parameterList);
        }
    }
}
