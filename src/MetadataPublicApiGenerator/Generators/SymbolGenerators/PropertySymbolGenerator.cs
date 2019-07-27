// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using LightweightMetadata;
using LightweightMetadata.TypeWrappers;
using MetadataPublicApiGenerator.Extensions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using static MetadataPublicApiGenerator.Helpers.SyntaxFactoryHelpers;

namespace MetadataPublicApiGenerator.Generators.SymbolGenerators
{
    internal static class PropertySymbolGenerator
    {
        public static PropertyDeclarationSyntax Generate(IHandleWrapper handle, ISet<string> excludeMembersAttributes, ISet<string> excludeAttributes, int level)
        {
            if (!(handle is PropertyWrapper property))
            {
                return null;
            }

            var accessorList = new List<AccessorDeclarationSyntax>(2);

            if (property.Getter != null && property.Getter.Accessibility == EntityAccessibility.Public)
            {
                accessorList.Add(AccessorDeclaration(SyntaxKind.GetAccessorDeclaration, GeneratorFactory.Generate(property.Getter.Attributes, excludeMembersAttributes, excludeAttributes), property.Getter.GetModifiers(property)));
            }

            if (property.Setter != null && property.Setter.Accessibility == EntityAccessibility.Public)
            {
                accessorList.Add(AccessorDeclaration(SyntaxKind.SetAccessorDeclaration, GeneratorFactory.Generate(property.Setter.Attributes, excludeMembersAttributes, excludeAttributes), property.Setter.GetModifiers(property)));
            }

            return PropertyDeclaration(property.ReturnType.GetTypeSyntax(), property.Name, GeneratorFactory.Generate(property.Attributes, excludeMembersAttributes, excludeAttributes), property.GetModifiers(), accessorList, level);
        }
    }
}
