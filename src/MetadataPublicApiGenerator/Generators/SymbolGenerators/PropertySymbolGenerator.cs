// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using MetadataPublicApiGenerator.Compilation;
using MetadataPublicApiGenerator.Compilation.TypeWrappers;
using MetadataPublicApiGenerator.Extensions;
using Microsoft.CodeAnalysis;
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

        public override PropertyDeclarationSyntax Generate(CompilationModule compilation, Handle handle)
        {
            var propertyHandle = (PropertyDefinitionHandle)handle;
            var property = propertyHandle.Resolve(compilation);
            var signature = propertyHandle.DecodeSignature(compilation);
            var accessorList = new List<AccessorDeclarationSyntax>();

            if (signature == null)
            {
                throw new Exception("Unable to find a proper signature for the property");
            }

            var accessors = property.GetAccessors();

            if (!accessors.Getter.IsNil)
            {
                accessorList.Add(Generate(accessors.Getter, SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration), compilation));
            }

            if (!accessors.Setter.IsNil)
            {
                accessorList.Add(Generate(accessors.Setter, SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration), compilation));
            }

            return SyntaxFactory.PropertyDeclaration(SyntaxFactory.IdentifierName(((ITypeNamedWrapper)signature.Value.ReturnType).FullName), property.GetName(compilation))
                .WithAccessorList(SyntaxFactory.AccessorList(SyntaxFactory.List(accessorList)))
                .WithAttributeLists(AttributeGenerator.GenerateAttributes(compilation, property.GetCustomAttributes(), ExcludeAttributes))
                .WithModifiers(property.GetModifiers(compilation));
        }

        private AccessorDeclarationSyntax Generate(MethodDefinitionHandle methodHandle, AccessorDeclarationSyntax syntax, CompilationModule compilation)
        {
                var method = methodHandle.Resolve(compilation);
                return syntax
                    .WithAttributeLists(AttributeGenerator.GenerateAttributes(compilation, method.GetCustomAttributes(), ExcludeAttributes))
                    .WithModifiers(method.GetModifiers())
                    .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
        }
    }
}
