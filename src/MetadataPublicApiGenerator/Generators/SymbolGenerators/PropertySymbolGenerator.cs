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

namespace MetadataPublicApiGenerator.Generators.SymbolGenerators
{
    internal class PropertySymbolGenerator : SymbolGeneratorBase<PropertyDeclarationSyntax>
    {
        public PropertySymbolGenerator(ISet<string> excludeAttributes, ISet<string> excludeMembersAttributes, IGeneratorFactory factory)
            : base(excludeAttributes, excludeMembersAttributes, factory)
        {
        }

        public override PropertyDeclarationSyntax Generate(IHandleWrapper handle)
        {
            if (!(handle is PropertyWrapper property))
            {
                return null;
            }

            var accessorList = new List<AccessorDeclarationSyntax>();

            var accessors = property.AnyAccessor;

            if (property.Getter != null && property.Getter.Accessibility == EntityAccessibility.Public)
            {
                accessorList.Add(Generate(property.Getter, property, SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)));
            }

            if (property.Setter != null && property.Setter.Accessibility == EntityAccessibility.Public)
            {
                accessorList.Add(Generate(property.Setter, property, SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)));
            }

            return SyntaxFactory.PropertyDeclaration(SyntaxFactory.IdentifierName(property.ReturnType.ReflectionFullName), property.Name)
                .WithAccessorList(SyntaxFactory.AccessorList(SyntaxFactory.List(accessorList)))
                .WithAttributeLists(Factory.Generate(property.Attributes))
                .WithModifiers(property.GetModifiers());
        }

        private AccessorDeclarationSyntax Generate(MethodWrapper method, PropertyWrapper property, AccessorDeclarationSyntax syntax)
        {
                return syntax
                    .WithAttributeLists(Factory.Generate(method.Attributes))
                    .WithModifiers(method.GetModifiers(property))
                    .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
        }
    }
}
