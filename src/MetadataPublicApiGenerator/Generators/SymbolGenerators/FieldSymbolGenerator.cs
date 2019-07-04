// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Reflection.Metadata;

using MetadataPublicApiGenerator.Compilation;
using MetadataPublicApiGenerator.Extensions;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MetadataPublicApiGenerator.Generators.SymbolGenerators
{
    internal class FieldSymbolGenerator : SymbolGeneratorBase<FieldDeclarationSyntax>
    {
        public FieldSymbolGenerator(ISet<string> excludeAttributes, ISet<string> excludeMembersAttributes, IGeneratorFactory factory)
            : base(excludeAttributes, excludeMembersAttributes, factory)
        {
        }

        public override FieldDeclarationSyntax Generate(CompilationModule compilation, Handle handle)
        {
            var memberHandle = (FieldDefinitionHandle)handle;
            var field = memberHandle.Resolve(compilation);

            return SyntaxFactory.FieldDeclaration(SyntaxFactory
                .VariableDeclaration(SyntaxFactory.IdentifierName(field.GetDeclaringType().GenerateFullGenericName(compilation)))
                .WithVariables(SyntaxFactory.SingletonSeparatedList(SyntaxFactory.VariableDeclarator(field.GetName(compilation)))))
                .WithAttributeLists(AttributeGenerator.GenerateAttributes(compilation, field.GetCustomAttributes(), ExcludeAttributes))
                .WithModifiers(field.GetModifiers());
        }
    }
}
