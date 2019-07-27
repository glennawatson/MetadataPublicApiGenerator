// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

using LightweightMetadata;
using LightweightMetadata.Extensions;
using LightweightMetadata.TypeWrappers;
using MetadataPublicApiGenerator.Extensions;
using MetadataPublicApiGenerator.Generators.SymbolGenerators;

using Microsoft.CodeAnalysis.CSharp.Syntax;

using static MetadataPublicApiGenerator.Helpers.SyntaxFactoryHelpers;

namespace MetadataPublicApiGenerator.Generators.TypeGenerators
{
    internal static class DelegateTypeGenerator
    {
        internal static MemberDeclarationSyntax Generate(TypeWrapper type, ISet<string> excludeMembersAttributes, ISet<string> excludeAttributes, Func<TypeWrapper, bool> excludeFunc, int level)
        {
            if (excludeFunc(type))
            {
                return null;
            }

            return GenerateSyntax(type, excludeMembersAttributes, excludeAttributes, level);
        }

        private static MemberDeclarationSyntax GenerateSyntax(TypeWrapper type, ISet<string> excludeMembersAttributes, ISet<string> excludeAttributes, int level)
        {
            var invokeMember = type.GetDelegateInvokeMethod();

            var parameters = invokeMember.Parameters.Select(x => ParameterSymbolGenerator.Generate(x, excludeMembersAttributes, excludeAttributes, false)).Where(x => x != null).ToList();

            var (constraints, typeParameters) = type.GetTypeParameters(excludeMembersAttributes, excludeAttributes);

            return DelegateDeclaration(GeneratorFactory.Generate(type.Attributes, excludeMembersAttributes, excludeAttributes), type.GetModifiers(), invokeMember.ReturningType.GetTypeSyntax(), type.Name, parameters, constraints, typeParameters, level);
        }
    }
}
