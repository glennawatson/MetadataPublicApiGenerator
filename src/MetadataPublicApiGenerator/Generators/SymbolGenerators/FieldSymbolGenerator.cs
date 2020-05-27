// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

using LightweightMetadata;
using MetadataPublicApiGenerator.Extensions;
using MetadataPublicApiGenerator.Helpers;

using Microsoft.CodeAnalysis.CSharp.Syntax;

using static MetadataPublicApiGenerator.Helpers.SyntaxFactoryHelpers;

namespace MetadataPublicApiGenerator.Generators.SymbolGenerators
{
    internal static class FieldSymbolGenerator
    {
        public static FieldDeclarationSyntax? Generate(IHandleWrapper member, ISet<string> excludeMembersAttributes, ISet<string> excludeAttributes, Nullability currentNullability, int level)
        {
            if (!(member is FieldWrapper field))
            {
                return null;
            }

            var modifiers = field.GetModifiers();

            field.Attributes.TryGetNullable(out var nullability);

            VariableDeclarationSyntax variableDeclaration;
            if (field.TryGetFixed(out var bufferSize, out var fixedArrayType))
            {
                var arrayType = ArrayType(fixedArrayType.GetTypeSyntax(field, currentNullability, nullability), new[] { ArrayRankSpecifier(new int?[] { bufferSize }) });

                var variables = new[] { VariableDeclarator(field.Name, EqualsValueClause(ArrayCreationExpression(arrayType))) };
                variableDeclaration = VariableDeclaration(arrayType, variables);
            }
            else if (field.IsConst)
            {
                var valueSyntax = SyntaxHelper.GetValueExpression(field.FieldType, field.DefaultValue!);

                if (valueSyntax == null)
                {
                    throw new Exception("Could not generate a const value for field " + field.Name);
                }

                var variables = new[] { VariableDeclarator(field.Name, EqualsValueClause(valueSyntax)) };

                variableDeclaration = VariableDeclaration(field.FieldType.GetTypeSyntax(field, currentNullability, nullability), variables);
            }
            else
            {
                variableDeclaration = VariableDeclaration(field.FieldType.GetTypeSyntax(field, currentNullability, nullability), new[] { VariableDeclarator(field.Name) });
            }

            return FieldDeclaration(GeneratorFactory.Generate(field.Attributes, excludeMembersAttributes, excludeAttributes), modifiers, variableDeclaration, level);
        }
    }
}
