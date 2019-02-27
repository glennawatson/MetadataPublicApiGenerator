// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
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

            switch (methodKind)
            {
                case MethodKind.Constructor:
                    return GenerateFromMethodSyntax(Factory, compilation, SyntaxFactory.ConstructorDeclaration(method.GetDeclaringType().GetName(compilation)), method);
                case MethodKind.Destructor:
                    return GenerateFromMethodSyntax(Factory, compilation, SyntaxFactory.DestructorDeclaration(method.GetDeclaringType().GetName(compilation)), method);
                case MethodKind.Ordinary:
                    var methodDeclaration = SyntaxFactory.MethodDeclaration(SyntaxFactory.IdentifierName(GetReturnTypeName(signature)), method.GetName(compilation))
                        .WithAttributeLists(AttributeGenerator.GenerateAttributes(compilation, method.GetCustomAttributes(), ExcludeAttributes));

                    return GenerateFromMethodSyntax(Factory, compilation, methodDeclaration, method);
                case MethodKind.BuiltinOperator:
                case MethodKind.UserDefinedOperator:
                    return GenerateFromMethodSyntax(Factory, compilation, SyntaxFactory.OperatorDeclaration(SyntaxFactory.IdentifierName(GetReturnTypeName(signature)), SyntaxHelper.OperatorNameToToken(method.GetName(compilation))), method);
                default:
                    throw new Exception("Unknown method type: " + methodKind);
            }
        }

        private static string GetReturnTypeName(MethodSignature<IWrapper> signature)
        {
            return ((ITypeNamedWrapper)signature.ReturnType).FullName;
        }

        private static BaseMethodDeclarationSyntax GenerateFromMethodSyntax(IGeneratorFactory factory, CompilationModule compilation, BaseMethodDeclarationSyntax item, MethodDefinition member)
        {
            return item
                .WithModifiers(member.GetModifiers())
                .WithParameterList(SyntaxFactory.ParameterList(SyntaxFactory.SeparatedList(member.GetParameters().Select(x => factory.Generate<ParameterSyntax>(x, compilation)))))
                .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
        }
    }
}
