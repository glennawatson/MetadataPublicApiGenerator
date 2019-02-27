// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using MetadataPublicApiGenerator.Compilation;
using MetadataPublicApiGenerator.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MetadataPublicApiGenerator.Generators
{
    /// <summary>
    /// Generates attributes for a <see cref="TypeDefinition"/> or assembly.
    /// </summary>
    internal static class AttributeGenerator
    {
        internal static SyntaxList<AttributeListSyntax> GenerateAttributes(CompilationModule compilation, IEnumerable<CustomAttributeHandle> attributes, ISet<string> excludeAttributes)
        {
            return GenerateAttributes(compilation, attributes.Select(x => x.Resolve(compilation)), excludeAttributes);
        }

        internal static SyntaxList<AttributeListSyntax> GenerateAttributes(CompilationModule compilation, IEnumerable<CustomAttribute> attributes, ISet<string> excludeAttributes)
        {
            var validAttributes = new List<CustomAttribute>();
            foreach (var attribute in attributes)
            {
                var attributeType = ((MethodDefinitionHandle)attribute.Constructor).Resolve(compilation).GetDeclaringType().Resolve(compilation);

                if (excludeAttributes.Contains(attributeType.GetFullName(compilation)))
                {
                    continue;
                }

                validAttributes.Add(attribute);
            }

            if (validAttributes.Count == 0)
            {
                return SyntaxFactory.List<AttributeListSyntax>();
            }

            return SyntaxFactory.List(validAttributes.Select(attribute => attribute.GenerateAttributeList(compilation)));
        }

        internal static SyntaxList<AttributeListSyntax> GenerateAssemblyCustomAttributes(CompilationModule compilation, ISet<string> excludeAttributes)
        {
            var validAttributes = new List<CustomAttribute>();
            foreach (var attribute in compilation.MetadataReader.GetAssemblyDefinition().GetCustomAttributes().Select(x => x.Resolve(compilation)))
            {
                var attributeType = ((MethodDefinitionHandle)attribute.Constructor).Resolve(compilation).GetDeclaringType().Resolve(compilation);

                if (excludeAttributes.Contains(attributeType.GetFullName(compilation)))
                {
                    continue;
                }

                validAttributes.Add(attribute);
            }

            if (validAttributes.Count == 0)
            {
                return SyntaxFactory.List<AttributeListSyntax>();
            }

            return SyntaxFactory.List(validAttributes.Select(attribute => attribute.GenerateAttributeList(compilation).WithTarget(SyntaxFactory.AttributeTargetSpecifier(SyntaxFactory.Token(SyntaxKind.AssemblyKeyword)))));
        }
    }
}
