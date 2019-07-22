﻿// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using LightweightMetadata.TypeWrappers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using static MetadataPublicApiGenerator.Helpers.SyntaxFactoryHelpers;

namespace MetadataPublicApiGenerator.Generators.TypeGenerators
{
    internal class InterfaceTypeGenerator : TypeGeneratorBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InterfaceTypeGenerator"/> class.
        /// </summary>
        /// <param name="excludeAttributes">A set of attributes to exclude from being generated.</param>
        /// <param name="excludeMembersAttributes">A set of attributes for any types we should avoid that are decorated with these attribute types.</param>
        /// <param name="excludeFunc">An exclusion func which will potentially exclude attributes.</param>
        /// <param name="factory">A factory for generating sub types.</param>
        public InterfaceTypeGenerator(ISet<string> excludeAttributes, ISet<string> excludeMembersAttributes, Func<TypeWrapper, bool> excludeFunc, IGeneratorFactory factory)
            : base(excludeAttributes, excludeMembersAttributes, excludeFunc, factory)
        {
        }

        /// <inheritdoc />
        public override TypeKind TypeKind => TypeKind.Interface;

        /// <inheritdoc />
        public override TypeDeclarationSyntax GenerateSyntax(TypeWrapper typeDefinition, IReadOnlyCollection<AttributeListSyntax> attributes, IReadOnlyCollection<SyntaxKind> modifiers, IReadOnlyCollection<MemberDeclarationSyntax> members, IReadOnlyCollection<TypeParameterConstraintClauseSyntax> typeParameterConstraintClauses, IReadOnlyCollection<TypeParameterSyntax> typeParameters, IReadOnlyCollection<BaseTypeSyntax> bases, int level)
        {
            return InterfaceDeclaration(typeDefinition.Name, attributes, modifiers, members, typeParameterConstraintClauses, typeParameters, bases, level);
        }
    }
}
