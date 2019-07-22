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
    internal class PropertySymbolGenerator : SymbolGeneratorBase<PropertyDeclarationSyntax>
    {
        public PropertySymbolGenerator(ISet<string> excludeAttributes, ISet<string> excludeMembersAttributes, IGeneratorFactory factory)
            : base(excludeAttributes, excludeMembersAttributes, factory)
        {
        }

        public override PropertyDeclarationSyntax Generate(IHandleWrapper handle, int level)
        {
            if (!(handle is PropertyWrapper property))
            {
                return null;
            }

            var accessorList = new List<AccessorDeclarationSyntax>();

            if (property.Getter != null && property.Getter.Accessibility == EntityAccessibility.Public)
            {
                accessorList.Add(AccessorDeclaration(SyntaxKind.GetAccessorDeclaration, Factory.Generate(property.Getter.Attributes, level), property.Getter.GetModifiers(property)));
            }

            if (property.Setter != null && property.Setter.Accessibility == EntityAccessibility.Public)
            {
                accessorList.Add(AccessorDeclaration(SyntaxKind.SetAccessorDeclaration, Factory.Generate(property.Setter.Attributes, level), property.Setter.GetModifiers(property)));
            }

            return PropertyDeclaration(property.ReturnType.GetTypeSyntax(), property.Name, Factory.Generate(property.Attributes, level), property.GetModifiers(), accessorList, level);
        }
    }
}
