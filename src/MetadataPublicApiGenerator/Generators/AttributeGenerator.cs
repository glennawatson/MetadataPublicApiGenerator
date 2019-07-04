// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using MetadataPublicApiGenerator.Compilation;
using MetadataPublicApiGenerator.Compilation.TypeWrappers;
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
        public static SyntaxList<AttributeListSyntax> GenerateAttributes(CompilationModule compilation, IEnumerable<AttributeWrapper> attributes, ISet<string> excludeAttributes)
        {
            var validAttributes = new List<AttributeWrapper>();
            foreach (var attribute in attributes)
            {
                var attributeType = attribute.Constructor.DeclaringType;

                if (!attributeType.IsPublic)
                {
                    continue;
                }

                var attributeName = attributeType.FullName;
                if (excludeAttributes.Contains(attributeName))
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

        public static SyntaxList<AttributeListSyntax> GenerateAssemblyCustomAttributes(CompilationModule compilation, ISet<string> excludeAttributes)
        {
            var validAttributes = new List<AttributeWrapper>();
            foreach (var attribute in compilation.MetadataReader.GetAssemblyDefinition().GetCustomAttributes().Select(x => AttributeWrapper.Create(x, compilation)))
            {
                var attributeType = ((MethodDefinitionHandle)attribute.Constructor).Resolve(compilation).GetDeclaringType().Resolve(compilation);

                if ((attributeType.Attributes & System.Reflection.TypeAttributes.Public) == 0)
                {
                    continue;
                }

                MethodSignature<ITypeNamedWrapper>? methodSignature;

                switch (attribute.Constructor.Kind)
                {
                }

                if (excludeAttributes.Contains(methodSignature.Value.ReturnType.FullName))
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
