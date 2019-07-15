// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Generic;
using LightweightMetadata.TypeWrappers;
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

        public override FieldDeclarationSyntax Generate(IHandleWrapper member)
        {
            if (!(member is FieldWrapper field))
            {
                return null;
            }

            return SyntaxFactory.FieldDeclaration(SyntaxFactory
                .VariableDeclaration(SyntaxFactory.IdentifierName(field.FieldType.FullName))
                .WithVariables(SyntaxFactory.SingletonSeparatedList(SyntaxFactory.VariableDeclarator(field.Name))))
                .WithAttributeLists(Factory.Generate(field.Attributes))
                .WithModifiers(field.GetModifiers());
        }
    }
}
