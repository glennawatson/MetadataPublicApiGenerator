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

namespace MetadataPublicApiGenerator.Generators.SymbolGenerators
{
    internal class TypeParameterSymbolGenerator : SymbolGeneratorBase<TypeParameterSyntax>
    {
        public TypeParameterSymbolGenerator(ISet<string> excludeAttributes, ISet<string> excludeMembersAttributes, IGeneratorFactory factory)
            : base(excludeAttributes, excludeMembersAttributes, factory)
        {
        }

        public override TypeParameterSyntax Generate(IHandleWrapper handle)
        {
            if (!(handle is GenericParameterWrapper typeParameterWrapper))
            {
                return null;
            }

            TypeParameterSyntax typeParameterSyntax = SyntaxFactory.TypeParameter(typeParameterWrapper.Name).WithVarianceKeyword(GetVarianceToken(typeParameterWrapper.Variance));

            return typeParameterSyntax.WithAttributeLists(Factory.Generate(typeParameterWrapper.Attributes));
        }

        private static SyntaxToken GetVarianceToken(VarianceType variance)
        {
            switch (variance)
            {
                case VarianceType.Contravariant:
                    return SyntaxFactory.Token(SyntaxKind.InKeyword);
                case VarianceType.Covariant:
                    return SyntaxFactory.Token(SyntaxKind.OutKeyword);
                default:
                    return SyntaxFactory.Token(SyntaxKind.None);
            }
        }
    }
}
