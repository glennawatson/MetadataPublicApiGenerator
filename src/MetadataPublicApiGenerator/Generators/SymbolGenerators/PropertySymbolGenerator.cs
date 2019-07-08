﻿// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using MetadataPublicApiGenerator.Compilation;
using MetadataPublicApiGenerator.Compilation.TypeWrappers;
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

        public override PropertyDeclarationSyntax Generate(IHandleNameWrapper handle)
        {
            if (!(handle is PropertyWrapper property))
            {
                return null;
            }

            var accessorList = new List<AccessorDeclarationSyntax>();

            var accessors = property.AnyAccessor;

            if (property.Getter != null)
            {
                accessorList.Add(Generate(property.Getter, SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)));
            }

            if (property.Setter != null)
            {
                accessorList.Add(Generate(property.Setter, SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)));
            }

            return SyntaxFactory.PropertyDeclaration(SyntaxFactory.IdentifierName(property.ReturnType.FullName), property.Name)
                .WithAccessorList(SyntaxFactory.AccessorList(SyntaxFactory.List(accessorList)))
                .WithAttributeLists(AttributeGenerator.GenerateAttributes(property.Attributes, ExcludeAttributes))
                .WithModifiers(property.GetModifiers());
        }

        private AccessorDeclarationSyntax Generate(MethodWrapper method, AccessorDeclarationSyntax syntax)
        {
                return syntax
                    .WithAttributeLists(AttributeGenerator.GenerateAttributes(method.Attributes, ExcludeAttributes))
                    .WithModifiers(method.GetModifiers())
                    .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
        }
    }
}
