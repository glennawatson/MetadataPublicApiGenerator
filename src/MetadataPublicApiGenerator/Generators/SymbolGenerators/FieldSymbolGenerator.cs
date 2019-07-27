// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Generic;

using LightweightMetadata;
using LightweightMetadata.TypeWrappers;
using MetadataPublicApiGenerator.Extensions;
using MetadataPublicApiGenerator.Helpers;

using Microsoft.CodeAnalysis.CSharp.Syntax;

using static MetadataPublicApiGenerator.Helpers.SyntaxFactoryHelpers;

namespace MetadataPublicApiGenerator.Generators.SymbolGenerators
{
    internal static class FieldSymbolGenerator
    {
        public static FieldDeclarationSyntax Generate(IHandleWrapper member, ISet<string> excludeMembersAttributes, ISet<string> excludeAttributes, int level)
        {
            if (!(member is FieldWrapper field))
            {
                return null;
            }

            var variables = new[]
                                {
                                    field.IsConst ? VariableDeclarator(field.Name, EqualsValueClause(SyntaxHelper.GetValueExpression(field.FieldType, field.DefaultValue))) : VariableDeclarator(field.Name)
                                };

            var declaration = VariableDeclaration(field.FieldType.GetTypeSyntax(), variables);

            return FieldDeclaration(GeneratorFactory.Generate(field.Attributes, excludeMembersAttributes, excludeAttributes), field.GetModifiers(), declaration, level);
        }
    }
}
