// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using MetadataPublicApiGenerator.Compilation;
using MetadataPublicApiGenerator.Compilation.TypeWrappers;
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

        public override BaseMethodDeclarationSyntax Generate(CompilationModule compilation, Handle handle)
        {
            var member = (MethodDefinitionHandle)handle;
            var (_, methodKind) = member.GetMethodSymbolKind(compilation);

            var method = member.Resolve(compilation);

            var signature = member.DecodeSignature(compilation);

            if (signature == null)
            {
                throw new Exception("Could not get a valid signature");
            }

            var methodName = method.GetName(compilation);
            var methodDeclaringTypeName = method.GetDeclaringType().GetName(compilation);
            switch (methodKind)
            {
                case MethodKind.Constructor:
                    return GenerateFromMethodSyntax(Factory, compilation, SyntaxFactory.ConstructorDeclaration(methodDeclaringTypeName), method);
                case MethodKind.Destructor:
                    return GenerateFromMethodSyntax(Factory, compilation, SyntaxFactory.DestructorDeclaration(methodDeclaringTypeName), method);
                case MethodKind.Ordinary:
                    var methodDeclaration = SyntaxFactory.MethodDeclaration(SyntaxFactory.IdentifierName(GetReturnTypeName(signature.Value)), methodName)
                        .WithAttributeLists(AttributeGenerator.GenerateAttributes(compilation, method.GetCustomAttributes(), ExcludeAttributes));

                    return GenerateFromMethodSyntax(Factory, compilation, methodDeclaration, method);
                case MethodKind.BuiltinOperator:
                case MethodKind.UserDefinedOperator:
                    switch (methodName)
                    {
                        case "op_Implicit":
                        case "op_Explicit":
                            return GenerateFromMethodSyntax(Factory, compilation, SyntaxFactory.ConversionOperatorDeclaration(SyntaxHelper.OperatorNameToToken(methodName), SyntaxFactory.IdentifierName(GetReturnTypeName(signature.Value))), method);
                        default:
                            return GenerateFromMethodSyntax(Factory, compilation, SyntaxFactory.OperatorDeclaration(SyntaxFactory.IdentifierName(GetReturnTypeName(signature.Value)), SyntaxHelper.OperatorNameToToken(methodName)), method);
                    }

                default:
                    return null;
            }
        }

        private static string GetReturnTypeName(in MethodSignature<ITypeNamedWrapper> signature)
        {
            return signature.ReturnType?.FullName ?? "System.Void";
        }

        private static BaseMethodDeclarationSyntax GenerateFromMethodSyntax(IGeneratorFactory factory, CompilationModule compilation, BaseMethodDeclarationSyntax item, in MethodDefinition member)
        {
            var parameters = member.GetParameters().Select(x => factory.Generate<ParameterSyntax>(x, compilation)).Where(x => x != null).ToList();

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
