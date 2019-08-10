// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

using LightweightMetadata;

using MetadataPublicApiGenerator.Extensions;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using static MetadataPublicApiGenerator.Helpers.SyntaxFactoryHelpers;

namespace MetadataPublicApiGenerator.Generators.TypeGenerators
{
    /// <summary>
    /// Generates Class declarations.
    /// </summary>
    internal static class ClassDefinitionGenerator
    {
        internal static MemberDeclarationSyntax Generate(TypeWrapper type, ISet<string> excludeMembersAttributes, ISet<string> excludeAttributes, Func<TypeWrapper, bool> excludeFunc, Nullability currentNullability, int level)
        {
            if (excludeFunc(type))
            {
                return null;
            }

            if (type.Attributes.TryGetNullableContext(out var nullableContext))
            {
                currentNullability = nullableContext;
            }

            var (constraints, typeParameters) = type.GetTypeParameters(excludeMembersAttributes, excludeAttributes, currentNullability);
            var name = type.Name;
            var baseTypes = type.GetBaseTypes(currentNullability);
            var members = TypeGeneratorHelpers.GenerateMemberDeclaration(type, excludeMembersAttributes, excludeAttributes, excludeFunc, currentNullability, level);
            var attributes = GeneratorFactory.Generate(type.Attributes, excludeMembersAttributes, excludeAttributes);
            var modifiers = type.GetModifiers();
            return ClassDeclaration(name, attributes, modifiers, members, constraints, typeParameters, baseTypes, level);
        }
    }
}
