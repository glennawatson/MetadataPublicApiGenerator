// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using MetadataPublicApiGenerator.Compilation;
using MetadataPublicApiGenerator.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MetadataPublicApiGenerator.Generators.TypeGenerators
{
    /// <summary>
    /// Contains the base information how to generate for a type.
    /// </summary>
    internal abstract class TypeGeneratorBase : GeneratorBase, ITypeGenerator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TypeGeneratorBase"/> class.
        /// </summary>
        /// <param name="excludeAttributes">A set of attributes to exclude from being generated.</param>
        /// <param name="excludeMembersAttributes">A set of attributes for any types we should avoid that are decorated with these attribute types.</param>
        /// <param name="excludeFunc">An exclusion func which will potentially exclude attributes.</param>
        /// <param name="factory">The factory for generating children.</param>
        protected TypeGeneratorBase(ISet<string> excludeAttributes, ISet<string> excludeMembersAttributes, Func<TypeDefinition, bool> excludeFunc, IGeneratorFactory factory)
            : base(excludeAttributes, excludeMembersAttributes, factory)
        {
            ExcludeFunc = excludeFunc;
        }

        /// <inheritdoc />
        public abstract TypeKind TypeKind { get; }

        /// <inheritdoc />
        public Func<TypeDefinition, bool> ExcludeFunc { get; }

        /// <summary>
        /// Generates the syntax required.
        /// </summary>
        /// <returns>The syntax.</returns>
        public abstract TypeDeclarationSyntax GenerateSyntax(CompilationModule compilation, TypeDefinition typeDefinition);

        /// <inheritdoc />
        public MemberDeclarationSyntax Generate(CompilationModule compilation, TypeDefinitionHandle typeHandle)
        {
            var type = typeHandle.Resolve(compilation);
            if (ExcludeFunc(type))
            {
                return null;
            }

            var item = GenerateSyntax(compilation, type);

            item = item.WithGenericParameterList(compilation, type);
            return item.WithModifiers(type.GetModifiers())
                .WithAttributeLists(AttributeGenerator.GenerateAttributes(compilation, type.GetCustomAttributes(), ExcludeAttributes))
                .WithMembers(GenerateMemberDeclaration(compilation, type));
        }

        internal SyntaxList<MemberDeclarationSyntax> GenerateMemberDeclaration(CompilationModule compilation, TypeDefinition typeDefinition)
        {
            var validHandles = typeDefinition.GetFields().Select(x => (Handle)x)
                .Concat(typeDefinition.GetEvents().Select(x => (Handle)x))
                .Concat(typeDefinition.GetProperties().Select(x => (Handle)x))
                .Concat(typeDefinition.GetMethods().Select(x => (Handle)x))
                .OrderByAndExclude(ExcludeMembersAttributes, compilation).ToList();

            if (validHandles.Count == 0)
            {
                return SyntaxFactory.List<MemberDeclarationSyntax>();
            }

            var members = validHandles.Select(x => Factory.Generate<MemberDeclarationSyntax>(x, compilation)).Where(x => x != null);

            return SyntaxFactory.List(members);
        }
    }
}
