// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Generic;

using LightweightMetadata;
using LightweightMetadata.TypeWrappers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MetadataPublicApiGenerator.Generators
{
    /// <summary>
    /// A factory which will create generators.
    /// </summary>
    internal interface IGeneratorFactory
    {
        /// <summary>
        /// Generates a <see cref="MemberDeclarationSyntax"/>.
        /// </summary>
        /// <param name="type">The type to generate the <see cref="MemberDeclarationSyntax"/> for.</param>
        /// <param name="level">The level in the hierarchy where this item is located.</param>
        /// <returns>The generated <see cref="MemberDeclarationSyntax"/>.</returns>
        MemberDeclarationSyntax Generate(TypeWrapper type, int level);

        /// <summary>
        /// Gets the generator for the symbol kind.
        /// </summary>
        /// <param name="symbol">The symbol to generate.</param>
        /// <typeparam name="TOutput">The output syntax node.</typeparam>
        /// <param name="level">The level in the hierarchy where this item is located.</param>
        /// <returns>The generated syntax node.</returns>
        TOutput Generate<TOutput>(IHandleWrapper symbol, int level)
            where TOutput : CSharpSyntaxNode;

        /// <summary>
        /// Generates the members for the namespace.
        /// </summary>
        /// <param name="root">The root namespace to generate for.</param>
        /// <returns>The generated members.</returns>
        IEnumerable<MemberDeclarationSyntax> Generate(NamespaceWrapper root);

        /// <summary>
        /// Generates for a attribute holder.
        /// </summary>
        /// <param name="attributes">A list of attributes.</param>
        /// <param name="level">The level to indent items.</param>
        /// <param name="target">Optional target to add to the attribute lists.</param>
        /// <returns>A list of attribute lists.</returns>
        SyntaxList<AttributeListSyntax> Generate(IEnumerable<AttributeWrapper> attributes, int level, SyntaxKind? target = null);

        /// <summary>
        /// Generates the compilation unit with public members.
        /// </summary>
        /// <param name="compilation">The compilation unit to generate for.</param>
        /// <returns>The compilation unit.</returns>
        CompilationUnitSyntax Generate(ICompilation compilation);
    }
}