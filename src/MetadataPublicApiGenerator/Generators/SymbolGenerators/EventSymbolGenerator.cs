// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Generic;
using LightweightMetadata.TypeWrappers;
using MetadataPublicApiGenerator.Extensions;

using Microsoft.CodeAnalysis.CSharp.Syntax;

using static MetadataPublicApiGenerator.Helpers.SyntaxFactoryHelpers;

namespace MetadataPublicApiGenerator.Generators.SymbolGenerators
{
    internal class EventSymbolGenerator : SymbolGeneratorBase<EventFieldDeclarationSyntax>
    {
        public EventSymbolGenerator(ISet<string> excludeAttributes, ISet<string> excludeMembersAttributes, IGeneratorFactory factory)
            : base(excludeAttributes, excludeMembersAttributes, factory)
        {
        }

        public override EventFieldDeclarationSyntax Generate(IHandleWrapper member, int level)
        {
            if (!(member is EventWrapper eventWrapper))
            {
                return null;
            }

            var variable = VariableDeclaration(eventWrapper.GetTypeSyntax());
            return EventFieldDeclaration(Factory.Generate(eventWrapper.Attributes, 0), eventWrapper.GetModifiers(), variable, level);
        }
    }
}
