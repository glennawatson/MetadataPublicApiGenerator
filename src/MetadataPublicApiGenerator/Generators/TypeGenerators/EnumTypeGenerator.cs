// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using LightweightMetadata;
using LightweightMetadata.Extensions;
using LightweightMetadata.TypeWrappers;
using MetadataPublicApiGenerator.Extensions;
using MetadataPublicApiGenerator.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

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
        public MemberDeclarationSyntax Generate(TypeWrapper type)
        {
            if (ExcludeFunc(type))
            {
                return null;
            }

            if (!type.TryGetEnumType(out var enumType))
            {
                return null;
            }

            var enumDeclaration = SyntaxFactory.EnumDeclaration(type.Name)
                .WithModifiers(type.GetModifiers())
                .WithAttributeLists(Factory.Generate(type.Attributes));

            var enumKnownType = enumType.ToKnownTypeCode();

            if (enumKnownType != KnownTypeCode.Int32)
            {
                enumDeclaration = enumDeclaration.WithBaseList(
                    SyntaxFactory.BaseList(
                        SyntaxFactory.SingletonSeparatedList<BaseTypeSyntax>(
                            SyntaxFactory.SimpleBaseType(
                                SyntaxFactory.IdentifierName(enumType.FullName)))));
            }

            var members = type.Fields.Where(x => x.ShouldIncludeEntity(ExcludeMembersAttributes, ExcludeAttributes) && x.IsStatic && x.IsPublic).Select(field =>
            {
                var memberName = field.Name;
                var enumMember = SyntaxFactory.EnumMemberDeclaration(memberName).WithAttributeLists(Factory.Generate(field.Attributes));

                if (field.DefaultValue != null)
                {
                    var constant = field.DefaultValue;
                    enumMember = enumMember.WithEqualsValue(SyntaxFactory.EqualsValueClause(SyntaxHelper.GetValueExpressionForKnownType(enumKnownType, constant)));
                }

                return enumMember;
            });

            return enumDeclaration.WithMembers(SyntaxFactory.SeparatedList(members));
        }
    }
}
