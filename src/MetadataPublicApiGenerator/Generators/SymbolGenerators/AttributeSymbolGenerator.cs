// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

using LightweightMetadata;

using MetadataPublicApiGenerator.Helpers;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using static MetadataPublicApiGenerator.Helpers.SyntaxFactoryHelpers;

namespace MetadataPublicApiGenerator.Generators.SymbolGenerators
{
    internal static class AttributeSymbolGenerator
    {
        public static AttributeSyntax? Generate(IHandleWrapper member)
        {
            if (!(member is AttributeWrapper customAttribute))
            {
                return null;
            }

            var arguments = new List<AttributeArgumentSyntax>(customAttribute.FixedArguments.Count + customAttribute.NamedArguments.Count);

            foreach (var fixedArgument in customAttribute.FixedArguments)
            {
                var valueSyntax = SyntaxHelper.GetValueExpression(fixedArgument.Type, fixedArgument.Value);

                if (valueSyntax == null)
                {
                    throw new Exception("Invalid fixed arguments for attribute.");
                }

                arguments.Add(AttributeArgument(valueSyntax));
            }

            foreach (var namedArgument in customAttribute.NamedArguments)
            {
                var valueSyntax = SyntaxHelper.GetValueExpression(namedArgument.Type, namedArgument.Value);

                if (valueSyntax == null)
                {
                    throw new Exception("Invalid named arguments for attribute:" + namedArgument.Name);
                }

                arguments.Add(AttributeArgument(NameEquals(IdentifierName(namedArgument.Name)), valueSyntax));
            }

            return Attribute(customAttribute.FullName, arguments);
        }
    }
}
