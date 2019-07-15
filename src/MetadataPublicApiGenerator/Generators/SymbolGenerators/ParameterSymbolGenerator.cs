// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Generic;
using LightweightMetadata.TypeWrappers;
using MetadataPublicApiGenerator.Extensions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace MetadataPublicApiGenerator.Generators.SymbolGenerators
{
    /// <summary>
    /// Generates parameters syntax.
    /// </summary>
    internal class ParameterSymbolGenerator : SymbolGeneratorBase<ParameterSyntax>
    {
        public ParameterSymbolGenerator(ISet<string> excludeAttributes, ISet<string> excludeMembersAttributes, IGeneratorFactory factory)
            : base(excludeAttributes, excludeMembersAttributes, factory)
        {
        }

        public override ParameterSyntax Generate(IHandleWrapper nameWrapper)
        {
            if (!(nameWrapper is ParameterWrapper parameterWrapper))
            {
                return null;
            }

            return Parameter(Identifier(parameterWrapper.Name))
                .WithModifiers(parameterWrapper.GetModifiers())
                .WithAttributeLists(Factory.Generate(parameterWrapper.Attributes))
                .WithType(IdentifierName(parameterWrapper.ParameterType.ReflectionFullName));
        }
    }
}
