// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

using LightweightMetadata;

using MetadataPublicApiGenerator.Extensions;

using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MetadataPublicApiGenerator.Generators.TypeGenerators
{
    /// <summary>
    /// Contains the base information how to generate for a type.
    /// </summary>
    internal static class TypeGeneratorHelpers
    {
        internal static IReadOnlyCollection<MemberDeclarationSyntax> GenerateMemberDeclaration(TypeWrapper typeWrapper, ISet<string> excludeMembersAttributes, ISet<string> excludeAttributes, Func<TypeWrapper, bool> excludeFunc, Nullability currentNullability, int level)
        {
            return typeWrapper.Fields.Cast<IHandleTypeNamedWrapper>()
                .Concat(typeWrapper.Events)
                .Concat(typeWrapper.Properties)
                .Concat(typeWrapper.Methods)
                .Concat(typeWrapper.NestedTypes)
                .OrderByAndExclude(excludeMembersAttributes, excludeAttributes)
                .Select(x => GeneratorFactory.Generate<MemberDeclarationSyntax>(x, excludeMembersAttributes, excludeAttributes, excludeFunc, currentNullability, level + 1))
                .Where(x => x != null)
                .Select(x => x!)
                .ToList();
        }
    }
}
