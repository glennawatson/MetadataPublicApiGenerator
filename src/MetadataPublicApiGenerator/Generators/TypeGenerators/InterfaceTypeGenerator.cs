// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using LightweightMetadata.TypeWrappers;

using MetadataPublicApiGenerator.Extensions;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using static MetadataPublicApiGenerator.Helpers.SyntaxFactoryHelpers;

namespace MetadataPublicApiGenerator.Generators.TypeGenerators
{
    internal static class InterfaceTypeGenerator
    {
        internal static MemberDeclarationSyntax Generate(TypeWrapper type, ISet<string> excludeMembersAttributes, ISet<string> excludeAttributes, Func<TypeWrapper, bool> excludeFunc, int level)
        {
            if (excludeFunc(type))
            {
                return null;
            }

            var (constraints, typeParameters) = type.GetTypeParameters(excludeMembersAttributes, excludeAttributes);

            var baseTypes = type.GetBaseTypes();

            return GenerateSyntax(type, GeneratorFactory.Generate(type.Attributes, excludeMembersAttributes, excludeAttributes), type.GetModifiers(), TypeGeneratorHelpers.GenerateMemberDeclaration(type, excludeMembersAttributes, excludeAttributes, excludeFunc, level), constraints, typeParameters, baseTypes, level);
        }

        private static TypeDeclarationSyntax GenerateSyntax(TypeWrapper typeDefinition, IReadOnlyCollection<AttributeListSyntax> attributes, IReadOnlyCollection<SyntaxKind> modifiers, IReadOnlyCollection<MemberDeclarationSyntax> members, IReadOnlyCollection<TypeParameterConstraintClauseSyntax> typeParameterConstraintClauses, IReadOnlyCollection<TypeParameterSyntax> typeParameters, IReadOnlyCollection<BaseTypeSyntax> bases, int level)
        {
            return InterfaceDeclaration(typeDefinition.Name, attributes, modifiers, members, typeParameterConstraintClauses, typeParameters, bases, level);
        }
    }
}
