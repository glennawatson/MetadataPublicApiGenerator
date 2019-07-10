// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Generic;
using LightweightMetadata.TypeWrappers;
using Microsoft.CodeAnalysis.CSharp;

namespace MetadataPublicApiGenerator.Generators.SymbolGenerators
{
    internal abstract class SymbolGeneratorBase<TOutput> : GeneratorBase, ISymbolGenerator
        where TOutput : CSharpSyntaxNode
    {
        protected SymbolGeneratorBase(ISet<string> excludeAttributes, ISet<string> excludeMembersAttributes, IGeneratorFactory factory)
            : base(excludeAttributes, excludeMembersAttributes, factory)
        {
        }

        /// <summary>
        /// Generates the symbol for the item.
        /// </summary>
        /// <param name="member">The member we are generating for.</param>
        /// <returns>a member declaration syntax.</returns>
        public abstract TOutput Generate(IHandleNameWrapper member);

        CSharpSyntaxNode ISymbolGenerator.Generate(IHandleNameWrapper member)
        {
            return Generate(member);
        }
    }
}
