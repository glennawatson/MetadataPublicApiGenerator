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
        public static BaseMethodDeclarationSyntax Generate(IHandleWrapper handle, ISet<string> excludeMembersAttributes, ISet<string> excludeAttributes)
        {
            if (!(handle is MethodWrapper method))
            {
                return null;
            }

            var parameters = method.Parameters.Select(x => ParameterSymbolGenerator.Generate(x, excludeMembersAttributes, excludeAttributes)).Where(x => x != null).ToList();
            var attributes = GeneratorFactory.Generate(method.Attributes, excludeMembersAttributes, excludeAttributes);
            var modifiers = method.GetModifiers();

            switch (method.MethodKind)
            {
                case SymbolMethodKind.Constructor:
                    return ConstructorDeclaration(attributes, modifiers, parameters, method.DeclaringType.Name);
                case SymbolMethodKind.Destructor:
                    return DestructorDeclaration(attributes, modifiers, method.DeclaringType.Name);
                case SymbolMethodKind.Ordinary:
                    var (constraints, typeParameters) = method.GetTypeParameters(excludeMembersAttributes, excludeAttributes);

                    return MethodDeclaration(attributes, modifiers, method.ReturningType.GetTypeSyntax(), method.Name, parameters, constraints, typeParameters);
                case SymbolMethodKind.BuiltinOperator:
                case SymbolMethodKind.UserDefinedOperator:
                    switch (method.Name)
                    {
                        case "op_Implicit":
                        case "op_Explicit":
                            return ConversionOperatorDeclaration(attributes, modifiers, SyntaxHelper.OperatorNameToToken(method.Name), method.ReturningType.ReflectionFullName, parameters);
                        default:
                            return OperatorDeclaration(attributes, modifiers, parameters, method.ReturningType.GetTypeSyntax(), SyntaxHelper.OperatorNameToToken(method.Name));
                    }

                default:
                    return null;
            }
        }
    }
}
