// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using LightweightMetadata.TypeWrappers;
using MetadataPublicApiGenerator.Extensions;
using MetadataPublicApiGenerator.Extensions.HandleNameWrapper;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using static MetadataPublicApiGenerator.Helpers.SyntaxFactoryHelpers;

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
        /// <param name="attributes">A list of attributes.</param>
        /// <param name="modifiers">A list of modifiers.</param>
        /// <param name="members">A list of members.</param>
        /// <param name="typeParameterConstraintClauses">Constraints on the parameters.</param>
        /// <param name="typeParameters">The type parameters of the type.</param>
        /// <param name="bases">Gets the base classes.</param>
        /// <param name="level">The level of indentation.</param>
        /// <returns>The syntax.</returns>
        public abstract TypeDeclarationSyntax GenerateSyntax(
            TypeWrapper typeDefinition,
            IReadOnlyCollection<AttributeListSyntax> attributes,
            IReadOnlyCollection<SyntaxKind> modifiers,
            IReadOnlyCollection<MemberDeclarationSyntax> members,
            IReadOnlyCollection<TypeParameterConstraintClauseSyntax> typeParameterConstraintClauses,
            IReadOnlyCollection<TypeParameterSyntax> typeParameters,
            IReadOnlyCollection<BaseTypeSyntax> bases,
            int level);

        /// <inheritdoc />
        public MemberDeclarationSyntax Generate(TypeWrapper type, int level)
        {
            if (ExcludeFunc(type))
            {
                return null;
            }

            var (constraints, typeParameters) = type.GetTypeParameters(Factory);

            var baseTypes = type.GetBaseTypes();

            return GenerateSyntax(type, Factory.Generate(type.Attributes, level), type.GetModifiers(), GenerateMemberDeclaration(type, level), constraints, typeParameters, baseTypes, level);
        }

        internal SyntaxList<MemberDeclarationSyntax> GenerateMemberDeclaration(TypeWrapper typeWrapper, int level)
        {
            var items = typeWrapper.Fields.Cast<IHandleNameWrapper>()
                .Concat(typeWrapper.Events)
                .Concat(typeWrapper.Properties)
                .Concat(typeWrapper.Methods)
                .OrderByAndExclude(ExcludeMembersAttributes, ExcludeAttributes)
                .Select(x => Factory.Generate<MemberDeclarationSyntax>(x, level + 1))
                .Where(x => x != null)
                .ToList();

            return items.Count == 0 ? List<MemberDeclarationSyntax>() : List(items);
        }
    }
}
