// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Reflection.Metadata;
using MetadataPublicApiGenerator.Compilation;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MetadataPublicApiGenerator.Generators.TypeGenerators
{
    /// <summary>
    /// A type generator that will generate the <see cref="MemberDeclarationSyntax" /> based on a <see cref="TypeKind" />.
    /// </summary>
    internal interface ITypeGenerator : IGenerator
    {
        /// <summary>
        /// Gets the type kind we are generating for.
        /// </summary>
        TypeKind TypeKind { get; }

        /// <summary>
        /// Gets a exclusion func which will potentially exclude attributes.
        /// </summary>
        Func<TypeDefinition, bool> ExcludeFunc { get; }

        /// <summary>
        /// Generate the member declaration.
        /// </summary>
        /// <param name="compilation">The compilation that contains the information for the assembly we are generating for.</param>
        /// <param name="type">The type we are generating for.</param>
        /// <returns>a member declaration syntax.</returns>
        MemberDeclarationSyntax Generate(CompilationModule compilation, TypeDefinitionHandle type);
    }
}
