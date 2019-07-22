// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Generic;
using LightweightMetadata.TypeWrappers;
using MetadataPublicApiGenerator.Extensions;

using Microsoft.CodeAnalysis.CSharp.Syntax;

using static MetadataPublicApiGenerator.Helpers.SyntaxFactoryHelpers;

namespace MetadataPublicApiGenerator.Generators.SymbolGenerators
{
    internal class FieldSymbolGenerator : SymbolGeneratorBase<FieldDeclarationSyntax>
    {
        public FieldSymbolGenerator(ISet<string> excludeAttributes, ISet<string> excludeMembersAttributes, IGeneratorFactory factory)
            : base(excludeAttributes, excludeMembersAttributes, factory)
        {
        }

        public override FieldDeclarationSyntax Generate(IHandleWrapper member, int level)
        {
            if (!(member is FieldWrapper field))
            {
                return null;
            }

            var variables = new[] { VariableDeclarator(field.Name) };

            var declaration = VariableDeclaration(field.FieldType.GetTypeSyntax(), variables);

            return FieldDeclaration(Factory.Generate(field.Attributes, 0), field.GetModifiers(), declaration, level);
        }
    }
}
