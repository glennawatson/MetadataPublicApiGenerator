// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Text;
using MetadataPublicApiGenerator.Compilation;
using MetadataPublicApiGenerator.Extensions;
using Microsoft.CodeAnalysis;
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

        public override EventFieldDeclarationSyntax Generate(CompilationModule compilation, Handle handle)
        {
            var memberHandle = (EventDefinitionHandle)handle;

            if (memberHandle.IsNil)
            {
                return null;
            }

            var member = memberHandle.Resolve(compilation);

            string memberName = null;

            switch (member.Type.Kind)
            {
                case HandleKind.TypeDefinition:
                    memberName = ((TypeDefinitionHandle)member.Type).GenerateFullGenericName(compilation);
                    break;
                case HandleKind.TypeSpecification:
                    memberName = ((TypeSpecificationHandle)member.Type).GenerateFullGenericName(compilation);
                    break;
            }

            if (string.IsNullOrWhiteSpace(memberName))
            {
                return null;
            }

            return SyntaxFactory.EventFieldDeclaration(
                    SyntaxFactory.VariableDeclaration(SyntaxFactory.IdentifierName(memberName))
                        .WithVariables(SyntaxFactory.SingletonSeparatedList(SyntaxFactory.VariableDeclarator(SyntaxFactory.Identifier(member.GetName(compilation))))))
                    .WithModifiers(member.GetModifiers(compilation))
                    .WithAttributeLists(AttributeGenerator.GenerateAttributes(compilation, member.GetCustomAttributes(), ExcludeAttributes));
        }
    }
}
