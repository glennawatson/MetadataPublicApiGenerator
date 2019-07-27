// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

using LightweightMetadata;

using MetadataPublicApiGenerator.Extensions;
using MetadataPublicApiGenerator.Generators.SymbolGenerators;

using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MetadataPublicApiGenerator.Generators.TypeGenerators
{
    /// <summary>
    /// Contains the base information how to generate for a type.
    /// </summary>
    internal static class TypeGeneratorHelpers
    {
        internal static IReadOnlyCollection<MemberDeclarationSyntax> GenerateMemberDeclaration(TypeWrapper typeWrapper, ISet<string> excludeMembersAttributes, ISet<string> excludeAttributes, Func<TypeWrapper, bool> excludeFunc, int level)
        {
            var returnValueList = new List<MemberDeclarationSyntax>(typeWrapper.Events.Count + typeWrapper.Properties.Count + typeWrapper.Methods.Count + typeWrapper.NestedTypes.Count);

            returnValueList.AddRange(typeWrapper.Events.OrderByAndExclude(excludeMembersAttributes, excludeAttributes).Select(x => EventSymbolGenerator.Generate(x, excludeMembersAttributes, excludeAttributes, level + 1)));
            returnValueList.AddRange(typeWrapper.Properties.OrderByAndExclude(excludeMembersAttributes, excludeAttributes).Select(x => PropertySymbolGenerator.Generate(x, excludeMembersAttributes, excludeAttributes, level + 1)));
            returnValueList.AddRange(typeWrapper.Methods.OrderByAndExclude(excludeMembersAttributes, excludeAttributes).Select(x => MethodSymbolGenerator.Generate(x, excludeMembersAttributes, excludeAttributes, level + 1)));
            returnValueList.AddRange(typeWrapper.NestedTypes.OrderByAndExclude(excludeMembersAttributes, excludeAttributes).Select(x => GeneratorFactory.Generate(x, excludeMembersAttributes, excludeAttributes, excludeFunc, level + 1)).Where(x => x != null));

            return returnValueList.Where(x => x != null).ToList();
        }
    }
}
