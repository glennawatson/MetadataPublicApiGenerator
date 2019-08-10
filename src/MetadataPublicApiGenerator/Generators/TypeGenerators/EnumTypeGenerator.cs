// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using LightweightMetadata;

using MetadataPublicApiGenerator.Extensions;
using MetadataPublicApiGenerator.Helpers;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using static MetadataPublicApiGenerator.Helpers.SyntaxFactoryHelpers;

namespace MetadataPublicApiGenerator.Generators.TypeGenerators
{
    /// <summary>
    /// Generates enum types.
    /// </summary>
    internal static class EnumTypeGenerator
    {
        public static MemberDeclarationSyntax Generate(TypeWrapper type, ISet<string> excludeMembersAttributes, ISet<string> excludeAttributes, Func<TypeWrapper, bool> excludeFunc, int level)
        {
            if (excludeFunc(type))
            {
                return null;
            }

            if (!type.TryGetEnumType(out var enumType))
            {
                return null;
            }

            var enumKnownType = enumType.KnownType;

            var members = type.Fields.Where(x => x.ShouldIncludeEntity(excludeMembersAttributes, excludeAttributes) && x.IsStatic && x.Accessibility == EntityAccessibility.Public).Select(field =>
                {
                    var memberName = field.Name;
                    var constant = field.DefaultValue;

                    return EnumMemberDeclaration(GeneratorFactory.Generate(field.Attributes, excludeMembersAttributes, excludeAttributes), memberName, constant == null ? null : EqualsValueClause(SyntaxHelper.GetValueExpressionForKnownType(enumKnownType, constant)));
                }).ToList();

            string enumName = null;
            if (enumKnownType != KnownTypeCode.Int32)
            {
                enumName = enumType.FullName;
            }

            return EnumDeclaration(type.Name, GeneratorFactory.Generate(type.Attributes, excludeMembersAttributes, excludeAttributes), members, type.GetModifiers(), enumName, level);
        }
    }
}
