// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Reflection.Metadata;
using LightweightMetadata.TypeWrappers;
using MetadataPublicApiGenerator.Extensions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MetadataPublicApiGenerator.Generators.SymbolGenerators
{
    internal class EventSymbolGenerator : SymbolGeneratorBase<EventFieldDeclarationSyntax>
    {
        public EventSymbolGenerator(ISet<string> excludeAttributes, ISet<string> excludeMembersAttributes, IGeneratorFactory factory)
            : base(excludeAttributes, excludeMembersAttributes, factory)
        {
        }

        public override EventFieldDeclarationSyntax Generate(IHandleWrapper member)
        {
            if (!(member is EventWrapper eventWrapper))
            {
                return null;
            }

            var memberName = eventWrapper.FullName;

            if (string.IsNullOrWhiteSpace(memberName))
            {
                return null;
            }

            return SyntaxFactory.EventFieldDeclaration(
                    SyntaxFactory.VariableDeclaration(SyntaxFactory.IdentifierName(memberName))
                        .WithVariables(SyntaxFactory.SingletonSeparatedList(SyntaxFactory.VariableDeclarator(SyntaxFactory.Identifier(eventWrapper.Name)))))
                    .WithModifiers(eventWrapper.GetModifiers())
                    .WithAttributeLists(Factory.Generate(eventWrapper.Attributes));
        }
    }
}
