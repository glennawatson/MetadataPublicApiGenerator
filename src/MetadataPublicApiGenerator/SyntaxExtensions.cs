// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;
using ICSharpCode.Decompiler.TypeSystem;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MetadataPublicApiGenerator
{
    internal static class SyntaxExtensions
    {
        /// <summary>
        /// Generate a attribute list individually for a single attribute.
        /// </summary>
        /// <param name="attribute">The attribute to generate the attribute list for.</param>
        /// <param name="compilation">The compilation unit for details about types.</param>
        /// <returns>The attribute list syntax containing the single attribute.</returns>
        public static AttributeListSyntax GenerateAttributeList(this IAttribute attribute, ICompilation compilation)
        {
            return SyntaxFactory.AttributeList(SyntaxFactory.SeparatedList(new[] { GenerateAttributeSyntax(attribute, compilation) }));
        }

        /// <summary>
        /// Generates the attribute syntax for a specified attribute.
        /// </summary>
        /// <param name="customAttribute">The attribute to generate the AttributeSyntax for.</param>
        /// <param name="compilation">The compilation unit for details about types.</param>
        /// <returns>The attribute syntax for the single attribute.</returns>
        public static AttributeSyntax GenerateAttributeSyntax(this IAttribute customAttribute, ICompilation compilation)
        {
            var arguments = new List<AttributeArgumentSyntax>();

            foreach (var fixedArgument in customAttribute.FixedArguments)
            {
                arguments.Add(SyntaxFactory.AttributeArgument(SyntaxHelper.AttributeParameterFromType(compilation, fixedArgument.Type, fixedArgument.Value)));
            }

            foreach (var namedArgument in customAttribute.NamedArguments)
            {
                arguments.Add(SyntaxFactory.AttributeArgument(SyntaxHelper.AttributeParameterFromType(compilation, namedArgument.Type, namedArgument.Value)).WithNameEquals(SyntaxFactory.NameEquals(SyntaxFactory.IdentifierName(namedArgument.Name))));
            }

            return SyntaxFactory.Attribute(SyntaxFactory.IdentifierName(customAttribute.AttributeType.FullName)).WithArgumentList(SyntaxFactory.AttributeArgumentList(SyntaxFactory.SeparatedList(arguments)));
        }
    }
}
