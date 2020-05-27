// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using LightweightMetadata;

using MetadataPublicApiGenerator.Extensions;
using MetadataPublicApiGenerator.Helpers;

using Microsoft.CodeAnalysis.CSharp.Syntax;

using static MetadataPublicApiGenerator.Helpers.SyntaxFactoryHelpers;

namespace MetadataPublicApiGenerator.Generators.SymbolGenerators
{
    internal static class MethodSymbolGenerator
    {
        public static BaseMethodDeclarationSyntax? Generate(IHandleWrapper handle, ISet<string> excludeMembersAttributes, ISet<string> excludeAttributes, Nullability currentNullability, int level)
        {
            if (!(handle is MethodWrapper method))
            {
                return null;
            }

            if (method.Attributes.TryGetNullableContext(out var nullableContext))
            {
                currentNullability = nullableContext;
            }

            var attributes = GeneratorFactory.Generate(method.Attributes, excludeMembersAttributes, excludeAttributes);
            var modifiers = method.GetModifiers();
            var parameters = GetParameters(method, excludeMembersAttributes, excludeAttributes, currentNullability);
            var (constraints, typeParameters) = method.GetTypeParameters(excludeMembersAttributes, excludeAttributes, currentNullability);
            var name = method.Name;

            method.ReturnAttributes.TryGetNullable(out var returnNullability);

            switch (method.MethodKind)
            {
                case SymbolMethodKind.Constructor:
                    return ConstructorDeclaration(attributes, modifiers, parameters, method.DeclaringType.Name, level);
                case SymbolMethodKind.Destructor:
                    return DestructorDeclaration(attributes, modifiers, method.DeclaringType.Name, level);
                case SymbolMethodKind.ExplicitInterfaceImplementation:
                {
                    var explicitInterface = ExplicitInterfaceSpecifier(method.ExplicitType!.ReflectionFullName);
                    var returnType = method.ReturningType.GetTypeSyntax(method, currentNullability, returnNullability);
                    return MethodDeclaration(attributes, default!, returnType, explicitInterface, name, parameters, constraints, typeParameters, level);
                }

                case SymbolMethodKind.Ordinary:
                {
                    var returnType = method.ReturningType.GetTypeSyntax(method, currentNullability, returnNullability);
                    return MethodDeclaration(attributes, modifiers, returnType, default!, name, parameters, constraints, typeParameters, level);
                }

                case SymbolMethodKind.BuiltinOperator:
                case SymbolMethodKind.UserDefinedOperator:
                    switch (name)
                    {
                        case "op_Implicit":
                        case "op_Explicit":
                            return ConversionOperatorDeclaration(attributes, modifiers, SyntaxHelper.OperatorNameToToken(name), method.ReturningType.ReflectionFullName, parameters, level);
                        default:
                            return OperatorDeclaration(attributes, modifiers, parameters, method.ReturningType.GetTypeSyntax(method, currentNullability, returnNullability), SyntaxHelper.OperatorNameToToken(name), level);
                    }

                default:
                    return null;
            }
        }

        private static IReadOnlyCollection<ParameterSyntax> GetParameters(MethodWrapper method, ISet<string> excludeMembersAttributes, ISet<string> excludeAttributes, Nullability nullability)
        {
            var parameters = new List<ParameterSyntax>(method.Parameters.Count);

            int i = 0;

            foreach (var parameter in method.Parameters)
            {
                var parameterSyntax = ParameterSymbolGenerator.Generate(parameter, excludeMembersAttributes, excludeAttributes, nullability, i == 0 && method.IsExtensionMethod);

                if (parameterSyntax == null)
                {
                    throw new ArgumentException("Method has invalid parameters : " + method.Name, nameof(method));
                }

                parameters.Add(parameterSyntax);
                i++;
            }

            return parameters;
        }
    }
}
