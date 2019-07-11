// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using LightweightMetadata;
using LightweightMetadata.Extensions;
using LightweightMetadata.TypeWrappers;
using MetadataPublicApiGenerator.Extensions;
using MetadataPublicApiGenerator.Extensions.HandleNameWrapper;
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
        protected TypeGeneratorBase(ISet<string> excludeAttributes, ISet<string> excludeMembersAttributes, Func<TypeWrapper, bool> excludeFunc, IGeneratorFactory factory)
            : base(excludeAttributes, excludeMembersAttributes, factory)
        {
            ExcludeFunc = excludeFunc;
        }

        /// <inheritdoc />
        public abstract TypeKind TypeKind { get; }

        /// <inheritdoc />
        public Func<TypeWrapper, bool> ExcludeFunc { get; }

        /// <summary>
        /// Generates the syntax required.
        /// </summary>
        /// <param name="typeDefinition">The definition to generate for.</param>
        /// <returns>The syntax.</returns>
        public abstract TypeDeclarationSyntax GenerateSyntax(TypeWrapper typeDefinition);

        /// <inheritdoc />
        public MemberDeclarationSyntax Generate(TypeWrapper type)
        {
            if (ExcludeFunc(type))
            {
                return null;
            }

            var item = GenerateSyntax(type);

            item = item.WithGenericParameterList(type);
            return item.WithModifiers(type.GetModifiers())
                .WithAttributeLists(Factory.Generate(type.Attributes))
                .WithMembers(GenerateMemberDeclaration(type))
                .AddBaseList(type.Base, type.InterfaceImplementations);
        }

        internal SyntaxList<MemberDeclarationSyntax> GenerateMemberDeclaration(TypeWrapper typeWrapper)
        {
            return GenerateTypeList<MemberDeclarationSyntax>(typeWrapper.Fields.Cast<IHandleNameWrapper>()
                .Concat(typeWrapper.Events)
                .Concat(typeWrapper.Properties)
                .Concat(typeWrapper.Methods));
        }

        private SyntaxList<T> GenerateTypeList<T>(IEnumerable<IHandleNameWrapper> items)
            where T : CSharpSyntaxNode
        {
            var validHandles = items
                .OrderByAndExclude(ExcludeMembersAttributes, ExcludeAttributes)
                .Select(x => Factory.Generate<T>(x))
                .Where(x => x != null)
                .ToList();

            return validHandles.Count == 0 ? SyntaxFactory.List<T>() : SyntaxFactory.List(validHandles);
        }
    }
}
