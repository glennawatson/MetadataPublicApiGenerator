// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Generic;
using LightweightMetadata;
using MetadataPublicApiGenerator.Extensions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using static MetadataPublicApiGenerator.Helpers.SyntaxFactoryHelpers;

namespace MetadataPublicApiGenerator.Generators.SymbolGenerators
{
    internal static class TypeParameterSymbolGenerator
    {
        public static TypeParameterSyntax Generate(IHandleWrapper handle, ISet<string> excludeMembersAttributes, ISet<string> excludeAttributes)
        {
            if (!(handle is GenericParameterWrapper typeParameterWrapper))
            {
                return null;
            }

            return TypeParameter(GeneratorFactory.Generate(typeParameterWrapper.Attributes, excludeMembersAttributes, excludeAttributes), GetVarianceToken(typeParameterWrapper.Variance), typeParameterWrapper.Name);
        }

        private static SyntaxKind GetVarianceToken(VarianceType variance)
        {
            switch (variance)
            {
                case VarianceType.Contravariant:
                    return SyntaxKind.InKeyword;
                case VarianceType.Covariant:
                    return SyntaxKind.OutKeyword;
                default:
                    return SyntaxKind.None;
            }
        }
    }
}
