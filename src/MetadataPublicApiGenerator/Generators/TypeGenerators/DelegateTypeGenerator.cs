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

using static MetadataPublicApiGenerator.Helpers.SyntaxFactoryHelpers;

namespace MetadataPublicApiGenerator.Generators.TypeGenerators
{
    internal static class DelegateTypeGenerator
    {
        internal static MemberDeclarationSyntax Generate(TypeWrapper typeWrapper, ISet<string> excludeMembersAttributes, ISet<string> excludeAttributes, Func<TypeWrapper, bool> excludeFunc, Nullability currentNullability, int level)
        {
            if (excludeFunc(typeWrapper))
            {
                return null;
            }

            if (typeWrapper.Attributes.TryGetNullableContext(out var nullableContext))
            {
                currentNullability = nullableContext;
            }

            var invokeMember = typeWrapper.GetDelegateInvokeMethod();

            var parameters = invokeMember.Parameters.Select(x => ParameterSymbolGenerator.Generate(x, excludeMembersAttributes, excludeAttributes, currentNullability, false)).Where(x => x != null).ToList();
            var (constraints, typeParameters) = typeWrapper.GetTypeParameters(excludeMembersAttributes, excludeAttributes, currentNullability);
            var attributes = GeneratorFactory.Generate(typeWrapper.Attributes, excludeMembersAttributes, excludeAttributes);
            var modifiers = typeWrapper.GetModifiers(invokeMember);
            var type = invokeMember.ReturningType.GetTypeSyntax(typeWrapper, currentNullability, Array.Empty<Nullability>());
            return DelegateDeclaration(attributes, modifiers, type, typeWrapper.Name, parameters, constraints, typeParameters, level);
        }
    }
}
