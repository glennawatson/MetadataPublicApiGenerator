// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;
using ICSharpCode.Decompiler.TypeSystem;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MetadataPublicApiGenerator
{
    /// <summary>
    /// Generates attributes for a <see cref="ITypeDefinition"/> or assembly.
    /// </summary>
    internal static class AttributeGenerator
    {
        internal static SyntaxList<AttributeListSyntax> GenerateAttributes(ICompilation compilation, IEnumerable<IAttribute> attributes, ISet<string> excludeAttributes)
        {
            var validAttributes = attributes.Where(x => !excludeAttributes.Contains(x.AttributeType.FullName)).ToList();

            if (validAttributes.Count == 0)
            {
                return SyntaxFactory.List<AttributeListSyntax>();
            }

            return SyntaxFactory.List(validAttributes.Select(
                attribute =>
                    attribute
                        .GenerateAttributeList(compilation)));
        }

        internal static CompilationUnitSyntax GenerateAssemblyCustomAttributes(ICompilation compilation, CompilationUnitSyntax compilationUnit, IReadOnlyCollection<IAttribute> attributes)
        {
            return compilationUnit.WithAttributeLists(SyntaxFactory.List(
                attributes.Select(
                    attribute =>
                        attribute
                            .GenerateAttributeList(compilation)
                            .WithTarget(SyntaxFactory.AttributeTargetSpecifier(SyntaxFactory.Token(SyntaxKind.AssemblyKeyword))))));
        }
    }
}
