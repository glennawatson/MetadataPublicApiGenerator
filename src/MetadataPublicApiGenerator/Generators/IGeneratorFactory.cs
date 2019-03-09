﻿// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reflection.Metadata;
using MetadataPublicApiGenerator.Compilation;
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
        /// <param name="compilation">The compilation which contains the type information.</param>
        /// <returns>The generated <see cref="MemberDeclarationSyntax"/>.</returns>
        MemberDeclarationSyntax Generate(TypeDefinitionHandle type, CompilationModule compilation);

        /// <summary>
        /// Gets the generator for the symbol kind.
        /// </summary>
        /// <param name="symbol">The symbol to generate.</param>
        /// <param name="compilation">The compilation which contains the type information.</param>
        /// <typeparam name="TOutput">The output syntax node.</typeparam>
        /// <returns>The generated syntax node.</returns>
        TOutput Generate<TOutput>(Handle symbol, CompilationModule compilation)
            where TOutput : CSharpSyntaxNode;

        /// <summary>
        /// Generates a namespace declaration.
        /// </summary>
        /// <param name="namespaceInfo">The namespace to generate for.</param>
        /// <param name="compilation">The compilation which contains the type information.</param>
        /// <returns>The generated namespace.</returns>
        NamespaceDeclarationSyntax Generate(NamespaceDefinition namespaceInfo, CompilationModule compilation);
    }
}