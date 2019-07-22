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
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using static MetadataPublicApiGenerator.Helpers.SyntaxFactoryHelpers;

namespace MetadataPublicApiGenerator.Generators.TypeGenerators
{
    /// <summary>
    /// Generates enum types.
    /// </summary>
    internal class EnumTypeGenerator : GeneratorBase, ITypeGenerator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EnumTypeGenerator"/> class.
        /// </summary>
        /// <param name="excludeAttributes">A set of attributes to exclude from being generated.</param>
        /// <param name="excludeMembersAttributes">A set of attributes for any types we should avoid that are decorated with these attribute types.</param>
        /// <param name="excludeFunc">A function to determine if we should exclude a type definition.</param>
        /// <param name="factory">The factory for generating children.</param>
        internal EnumTypeGenerator(ISet<string> excludeAttributes, ISet<string> excludeMembersAttributes, Func<TypeWrapper, bool> excludeFunc, IGeneratorFactory factory)
            : base(excludeAttributes, excludeMembersAttributes, factory)
        {
            ExcludeFunc = excludeFunc;
        }

        /// <inheritdoc />
        public TypeKind TypeKind => TypeKind.Enum;

        /// <inheritdoc />
        public Func<TypeWrapper, bool> ExcludeFunc { get; }

        /// <inheritdoc />
        public MemberDeclarationSyntax Generate(TypeWrapper type, int level)
        {
            if (ExcludeFunc(type))
            {
                return null;
            }

            if (!type.TryGetEnumType(out var enumType))
            {
                return null;
            }

            var enumKnownType = enumType.KnownType;

            var members = type.Fields.Where(x => x.ShouldIncludeEntity(ExcludeMembersAttributes, ExcludeAttributes) && x.IsStatic && x.Accessibility == EntityAccessibility.Public).Select(field =>
                {
                    var memberName = field.Name;
                    var constant = field.DefaultValue;

                    return EnumMemberDeclaration(Factory.Generate(field.Attributes, level), memberName, constant == null ? null : EqualsValueClause(SyntaxHelper.GetValueExpressionForKnownType(enumKnownType, constant)));
                }).ToList();

            string enumName = null;
            if (enumKnownType != KnownTypeCode.Int32)
            {
                enumName = enumType.FullName;
            }

            return EnumDeclaration(type.Name, Factory.Generate(type.Attributes, level), members, type.GetModifiers(), enumName, level);
        }
    }
}
