// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using LightweightMetadata;
using LightweightMetadata.TypeWrappers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using static MetadataPublicApiGenerator.Helpers.SyntaxFactoryHelpers;

namespace MetadataPublicApiGenerator.Generators.SymbolGenerators
{
    internal class TypeParameterSymbolGenerator : SymbolGeneratorBase<TypeParameterSyntax>
    {
        public TypeParameterSymbolGenerator(ISet<string> excludeAttributes, ISet<string> excludeMembersAttributes, IGeneratorFactory factory)
            : base(excludeAttributes, excludeMembersAttributes, factory)
        {
        }

        /// <inheritdoc />
        public override TypeParameterSyntax Generate(IHandleWrapper handle, int level)
        {
            if (!(handle is GenericParameterWrapper typeParameterWrapper))
            {
                return null;
            }

            return TypeParameter(Factory.Generate(typeParameterWrapper.Attributes, 0), GetVarianceToken(typeParameterWrapper.Variance), typeParameterWrapper.Name);
        }

        private static SyntaxToken GetVarianceToken(VarianceType variance)
        {
            switch (variance)
            {
                case VarianceType.Contravariant:
                    return Token(SyntaxKind.InKeyword);
                case VarianceType.Covariant:
                    return Token(SyntaxKind.OutKeyword);
                default:
                    return Token(SyntaxKind.None);
            }
        }
    }
}
