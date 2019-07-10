// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;
using LightweightMetadata.TypeWrappers;
using MetadataPublicApiGenerator.Helpers;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MetadataPublicApiGenerator.Generators.SymbolGenerators
{
    internal class AttributeSymbolGenerator : SymbolGeneratorBase<AttributeSyntax>
    {
        public AttributeSymbolGenerator(ISet<string> excludeAttributes, ISet<string> excludeMembersAttributes, IGeneratorFactory factory)
            : base(excludeAttributes, excludeMembersAttributes, factory)
        {
        }

        /// <inheritdoc />
        public override AttributeSyntax Generate(IHandleNameWrapper member)
        {
            if (!(member is AttributeWrapper customAttribute))
            {
                return null;
            }

            var arguments = new List<AttributeArgumentSyntax>();

            foreach (var fixedArgument in customAttribute.FixedArguments)
            {
                arguments.Add(SyntaxFactory.AttributeArgument(SyntaxHelper.GetValueExpression(fixedArgument.Type, fixedArgument.Value)));
            }

            foreach (var namedArgument in customAttribute.NamedArguments)
            {
                arguments.Add(SyntaxFactory.AttributeArgument(SyntaxHelper.GetValueExpression(namedArgument.Type, namedArgument.Value)).WithNameEquals(SyntaxFactory.NameEquals(SyntaxFactory.IdentifierName(namedArgument.Name))));
            }

            var attributeName = SyntaxFactory.IdentifierName(customAttribute.FullName);
            var attribute = SyntaxFactory.Attribute(attributeName);

            if (arguments.Count > 0)
            {
                attribute = attribute.WithArgumentList(SyntaxFactory.AttributeArgumentList(SyntaxFactory.SeparatedList(arguments)));
            }

            return attribute;
        }
    }
}
