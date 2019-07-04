// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Reflection.Metadata;

using MetadataPublicApiGenerator.Compilation;

using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MetadataPublicApiGenerator.Generators.SymbolGenerators
{
    /// <summary>
    /// Generates parameters syntax.
    /// </summary>
    internal class ParameterSymbolGenerator : SymbolGeneratorBase<ParameterSyntax>
    {
        public ParameterSymbolGenerator(ISet<string> excludeAttributes, ISet<string> excludeMembersAttributes, IGeneratorFactory factory)
            : base(excludeAttributes, excludeMembersAttributes, factory)
        {
        }

        public override ParameterSyntax Generate(CompilationModule compilation, Handle handle)
        {
            ////return SyntaxFactory.Parameter(SyntaxFactory.Identifier(parameter.GetName(compilation)))
            ////    .WithModifiers(parameter.GetModifiers(compilation))
            ////    .WithAttributeLists(AttributeGenerator.GenerateAttributes(compilation, parameter.GetCustomAttributes(), ExcludeAttributes))
            ////    .WithType(SyntaxFactory.IdentifierName(parameter..GenerateFullGenericName(compilation)));

            return null;
        }
    }
}
