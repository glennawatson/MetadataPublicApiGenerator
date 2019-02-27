// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using MetadataPublicApiGenerator.Compilation;
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
        internal EnumTypeGenerator(ISet<string> excludeAttributes, ISet<string> excludeMembersAttributes, Func<TypeDefinition, bool> excludeFunc, IGeneratorFactory factory)
            : base(excludeAttributes, excludeMembersAttributes, factory)
        {
            ExcludeFunc = excludeFunc;
        }

        /// <inheritdoc />
        public TypeKind TypeKind => TypeKind.Enum;

        /// <inheritdoc />
        public Func<TypeDefinition, bool> ExcludeFunc { get; }

        /// <inheritdoc />
        public MemberDeclarationSyntax Generate(CompilationModule compilation, TypeDefinitionHandle typeHandle)
        {
            var type = typeHandle.Resolve(compilation);

            if (ExcludeFunc(type))
            {
                return null;
            }

            if (!type.IsEnum(compilation, out var enumType))
            {
                throw new Exception("Processing enum type despite it not having a underlying type.");
            }

            var enumDeclaration = SyntaxFactory.EnumDeclaration(type.GetName(compilation))
                .WithModifiers(type.GetModifiers())
                .WithAttributeLists(AttributeGenerator.GenerateAttributes(compilation, type.GetCustomAttributes(), ExcludeAttributes));

            var name = type.GetFullName(compilation);

            var enumTypeName = enumType.ToKnownTypeCode();

            if (enumType != PrimitiveTypeCode.Int32)
            {
                enumDeclaration = enumDeclaration.WithBaseList(
                    SyntaxFactory.BaseList(
                        SyntaxFactory.SingletonSeparatedList<BaseTypeSyntax>(
                            SyntaxFactory.SimpleBaseType(
                                EnumType(enumTypeName, name)))));
            }

            var members = type.GetFields().Where(x => ((Handle)x).ShouldIncludeEntity(ExcludeMembersAttributes, compilation)).Select(x =>
            {
                var memberName = x.GetName(compilation);
                var field = x.Resolve(compilation);
                var enumMember = SyntaxFactory.EnumMemberDeclaration(memberName).WithAttributeLists(AttributeGenerator.GenerateAttributes(compilation, field.GetCustomAttributes(), ExcludeAttributes));

                if (!field.GetDefaultValue().IsNil)
                {
                    var constant = field.GetDefaultValue().ReadConstant(compilation);
                    enumMember = enumMember.WithEqualsValue(SyntaxFactory.EqualsValueClause(SyntaxHelper.LiteralParameterFromType(enumTypeName, constant)));
                }

                return enumMember;
            });

            return enumDeclaration.WithMembers(SyntaxFactory.SeparatedList(members));
        }

        private static PredefinedTypeSyntax EnumType(KnownTypeCode type, string name)
        {
            switch (type)
            {
                case KnownTypeCode.SByte:
                    return SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.SByteKeyword));
                case KnownTypeCode.Byte:
                    return SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.ByteKeyword));
                case KnownTypeCode.Int16:
                    return SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.ShortKeyword));
                case KnownTypeCode.UInt16:
                    return SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.UShortKeyword));
                case KnownTypeCode.Int32:
                    return SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.IntKeyword));
                case KnownTypeCode.UInt32:
                    return SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.UIntKeyword));
                case KnownTypeCode.Int64:
                    return SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.LongKeyword));
                case KnownTypeCode.UInt64:
                    return SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.ULongKeyword));
            }

            throw new Exception($"Unknown parameter type for a enum base type: {name}");
        }
    }
}
