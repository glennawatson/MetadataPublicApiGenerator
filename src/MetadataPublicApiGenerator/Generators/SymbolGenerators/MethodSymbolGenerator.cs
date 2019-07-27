// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using LightweightMetadata;
using LightweightMetadata.TypeWrappers;
using MetadataPublicApiGenerator.Extensions;
using MetadataPublicApiGenerator.Helpers;

using Microsoft.CodeAnalysis.CSharp.Syntax;

using static MetadataPublicApiGenerator.Helpers.SyntaxFactoryHelpers;

namespace MetadataPublicApiGenerator.Generators.SymbolGenerators
{
    internal static class MethodSymbolGenerator
    {
        public static BaseMethodDeclarationSyntax Generate(IHandleWrapper handle, ISet<string> excludeMembersAttributes, ISet<string> excludeAttributes, int level)
        {
            if (!(handle is MethodWrapper method))
            {
                return null;
            }

            var attributes = GeneratorFactory.Generate(method.Attributes, excludeMembersAttributes, excludeAttributes);
            var modifiers = method.GetModifiers();
            var parameters = GetParameters(method, excludeMembersAttributes, excludeAttributes);
            var (constraints, typeParameters) = method.GetTypeParameters(excludeMembersAttributes, excludeAttributes);

            switch (method.MethodKind)
            {
                case SymbolMethodKind.Constructor:
                    return ConstructorDeclaration(attributes, modifiers, parameters, method.DeclaringType.Name, level);
                case SymbolMethodKind.Destructor:
                    return DestructorDeclaration(attributes, modifiers, method.DeclaringType.Name, level);
                case SymbolMethodKind.ExplicitInterfaceImplementation:
                    var explicitInterface = ExplicitInterfaceSpecifier(method.ExplicitType.ReflectionFullName);

                    return MethodDeclaration(attributes, default, method.ReturningType.GetTypeSyntax(), explicitInterface, method.Name, parameters, constraints, typeParameters, level);
                case SymbolMethodKind.Ordinary:
                    return MethodDeclaration(attributes, modifiers, method.ReturningType.GetTypeSyntax(), default, method.Name, parameters, constraints, typeParameters, level);
                case SymbolMethodKind.BuiltinOperator:
                case SymbolMethodKind.UserDefinedOperator:
                    switch (method.Name)
                    {
                        case "op_Implicit":
                        case "op_Explicit":
                            return ConversionOperatorDeclaration(attributes, modifiers, SyntaxHelper.OperatorNameToToken(method.Name), method.ReturningType.ReflectionFullName, parameters, level);
                        default:
                            return OperatorDeclaration(attributes, modifiers, parameters, method.ReturningType.GetTypeSyntax(), SyntaxHelper.OperatorNameToToken(method.Name), level);
                    }

                default:
                    return null;
            }
        }

        private static IReadOnlyCollection<ParameterSyntax> GetParameters(MethodWrapper method, ISet<string> excludeMembersAttributes, ISet<string> excludeAttributes)
        {
            var parameters = new List<ParameterSyntax>(method.Parameters.Count);

            int i = 0;

            foreach (var parameter in method.Parameters)
            {
                var parameterSyntax = ParameterSymbolGenerator.Generate(parameter, excludeMembersAttributes, excludeAttributes, i == 0 && method.IsExtensionMethod);

                parameters.Add(parameterSyntax);
                i++;
            }

            return parameters;
        }
    }
}
