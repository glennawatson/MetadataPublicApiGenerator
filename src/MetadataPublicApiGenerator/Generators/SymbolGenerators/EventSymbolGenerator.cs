// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Generic;

using LightweightMetadata;
using MetadataPublicApiGenerator.Extensions;

using Microsoft.CodeAnalysis.CSharp.Syntax;

using static MetadataPublicApiGenerator.Helpers.SyntaxFactoryHelpers;

namespace MetadataPublicApiGenerator.Generators.SymbolGenerators
{
    internal static class EventSymbolGenerator
    {
        public static EventFieldDeclarationSyntax Generate(IHandleWrapper member, ISet<string> excludeMembersAttributes, ISet<string> excludeAttributes, Nullability currentNullability, int level)
        {
            if (!(member is EventWrapper eventWrapper))
            {
                return null;
            }

            var variables = new[] { VariableDeclarator(eventWrapper.Name) };

            eventWrapper.Attributes.TryGetNullable(out var nullability);

            var declaration = VariableDeclaration(eventWrapper.EventType.GetTypeSyntax(eventWrapper, currentNullability, nullability), variables);

            return EventFieldDeclaration(GeneratorFactory.Generate(eventWrapper.Attributes, excludeMembersAttributes, excludeAttributes), eventWrapper.GetModifiers(), declaration, level);
        }
    }
}
