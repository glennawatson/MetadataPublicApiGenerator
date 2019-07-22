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
    internal class MethodSymbolGenerator : SymbolGeneratorBase<BaseMethodDeclarationSyntax>
    {
        public MethodSymbolGenerator(ISet<string> excludeAttributes, ISet<string> excludeMembersAttributes, IGeneratorFactory factory)
            : base(excludeAttributes, excludeMembersAttributes, factory)
        {
        }

        public override BaseMethodDeclarationSyntax Generate(IHandleWrapper handle, int level)
        {
            if (!(handle is MethodWrapper method))
            {
                return null;
            }

            var parameters = method.Parameters.Select(x => Factory.Generate<ParameterSyntax>(x, 0)).Where(x => x != null).ToList();
            var attributes = Factory.Generate(method.Attributes, level);
            var modifiers = method.GetModifiers();

            switch (method.MethodKind)
            {
                case SymbolMethodKind.Constructor:
                    return ConstructorDeclaration(attributes, modifiers, parameters, method.DeclaringType.Name, level);
                case SymbolMethodKind.Destructor:
                    return DestructorDeclaration(attributes, modifiers, method.DeclaringType.Name, level);
                case SymbolMethodKind.Ordinary:
                    var (constraints, typeParameters) = method.GetTypeParameters(Factory);

                    return MethodDeclaration(attributes, modifiers, method.ReturningType.GetTypeSyntax(), method.Name, parameters, constraints, typeParameters, level);
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
    }
}
