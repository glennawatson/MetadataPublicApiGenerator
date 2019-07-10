// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using LightweightMetadata;
using LightweightMetadata.TypeWrappers;
using MetadataPublicApiGenerator.Extensions;
using MetadataPublicApiGenerator.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MetadataPublicApiGenerator.Generators.SymbolGenerators
{
    internal class MethodSymbolGenerator : SymbolGeneratorBase<BaseMethodDeclarationSyntax>
    {
        public MethodSymbolGenerator(ISet<string> excludeAttributes, ISet<string> excludeMembersAttributes, IGeneratorFactory factory)
            : base(excludeAttributes, excludeMembersAttributes, factory)
        {
        }

        public override BaseMethodDeclarationSyntax Generate(IHandleNameWrapper handle)
        {
            if (!(handle is MethodWrapper method))
            {
                return null;
            }

            var methodKind = method.MethodKind;

            var methodName = method.Name;
            var methodDeclaringTypeName = method.DeclaringType.Name;
            switch (methodKind)
            {
                case SymbolMethodKind.Constructor:
                    return GenerateFromMethodSyntax(Factory, SyntaxFactory.ConstructorDeclaration(methodDeclaringTypeName), method);
                case SymbolMethodKind.Destructor:
                    return GenerateFromMethodSyntax(Factory, SyntaxFactory.DestructorDeclaration(methodDeclaringTypeName), method);
                case SymbolMethodKind.Ordinary:
                    var methodDeclaration = SyntaxFactory.MethodDeclaration(SyntaxFactory.IdentifierName(method.ReturningType.ReflectionFullName), methodName)
                        .WithAttributeLists(Factory.Generate(method.Attributes));

                    return GenerateFromMethodSyntax(Factory, methodDeclaration, method);
                case SymbolMethodKind.BuiltinOperator:
                case SymbolMethodKind.UserDefinedOperator:
                    switch (methodName)
                    {
                        case "op_Implicit":
                        case "op_Explicit":
                            return GenerateFromMethodSyntax(Factory, SyntaxFactory.ConversionOperatorDeclaration(SyntaxHelper.OperatorNameToToken(methodName), SyntaxFactory.IdentifierName(method.ReturningType.ReflectionFullName)), method);
                        default:
                            return GenerateFromMethodSyntax(Factory, SyntaxFactory.OperatorDeclaration(SyntaxFactory.IdentifierName(method.ReturningType.ReflectionFullName), SyntaxHelper.OperatorNameToToken(methodName)), method);
                    }

                default:
                    return null;
            }
        }

        private static string GetReturnTypeName(in MethodSignature<ITypeNamedWrapper> signature)
        {
            return signature.ReturnType?.FullName ?? "System.Void";
        }

        private static BaseMethodDeclarationSyntax GenerateFromMethodSyntax(IGeneratorFactory factory, BaseMethodDeclarationSyntax item, in MethodWrapper member)
        {
            var parameters = member.Parameters.Select(factory.Generate<ParameterSyntax>).Where(x => x != null).ToList();

            var returnItem = item.WithModifiers(member.GetModifiers());

            if (parameters.Count > 0)
            {
                return returnItem.WithParameterList(SyntaxFactory.ParameterList(SyntaxFactory.SeparatedList(parameters)))
                    .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
            }

            return returnItem.WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
        }
    }
}
