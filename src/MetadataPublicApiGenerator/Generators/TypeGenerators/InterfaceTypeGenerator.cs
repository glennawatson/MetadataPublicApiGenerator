// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Text;
using MetadataPublicApiGenerator.Compilation;
using MetadataPublicApiGenerator.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MetadataPublicApiGenerator.Generators.TypeGenerators
{
    internal class InterfaceTypeGenerator : TypeGeneratorBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InterfaceTypeGenerator"/> class.
        /// </summary>
        /// <param name="excludeAttributes">A set of attributes to exclude from being generated.</param>
        /// <param name="excludeMembersAttributes">A set of attributes for any types we should avoid that are decorated with these attribute types.</param>
        /// <param name="excludeFunc">An exclusion func which will potentially exclude attributes.</param>
        /// <param name="factory">A factory for generating sub types.</param>
        public InterfaceTypeGenerator(ISet<string> excludeAttributes, ISet<string> excludeMembersAttributes, Func<TypeDefinition, bool> excludeFunc, IGeneratorFactory factory)
            : base(excludeAttributes, excludeMembersAttributes, excludeFunc, factory)
        {
        }

        /// <inheritdoc />
        public override TypeKind TypeKind => TypeKind.Interface;

        /// <inheritdoc />
        public override TypeDeclarationSyntax GenerateSyntax(CompilationModule compilation, TypeDefinition typeDefinition)
        {
            return SyntaxFactory.InterfaceDeclaration(typeDefinition.GetName(compilation));
        }
    }
}
