// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using System.Reflection.Metadata;
using MetadataPublicApiGenerator.Compilation;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MetadataPublicApiGenerator.Generators.SymbolGenerators
{
    /// <summary>
    /// A class which will generate members.
    /// </summary>
    internal interface ISymbolGenerator
    {
        /// <summary>
        /// Generates a member syntax from the specified member.
        /// </summary>
        /// <param name="compilation">The compilation that contains the information for the assembly we are generating for.</param>
        /// <param name="member">The member we are generating for.</param>
        /// <returns>a member declaration syntax.</returns>
        CSharpSyntaxNode Generate(CompilationModule compilation, Handle member);
    }
}
