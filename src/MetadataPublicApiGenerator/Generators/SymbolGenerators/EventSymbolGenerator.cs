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
    internal static class EventSymbolGenerator
    {
        public static EventFieldDeclarationSyntax Generate(IHandleWrapper member, ISet<string> excludeMembersAttributes, ISet<string> excludeAttributes)
        {
            if (!(member is EventWrapper eventWrapper))
            {
                return null;
            }

            var variable = VariableDeclaration(eventWrapper.GetTypeSyntax());
            return EventFieldDeclaration(GeneratorFactory.Generate(eventWrapper.Attributes, excludeMembersAttributes, excludeAttributes), eventWrapper.GetModifiers(), variable);
        }
    }
}
