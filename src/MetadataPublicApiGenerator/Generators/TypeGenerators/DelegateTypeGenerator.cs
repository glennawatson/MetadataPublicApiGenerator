// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using LightweightMetadata.Extensions;
using LightweightMetadata.TypeWrappers;
using MetadataPublicApiGenerator.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using static MetadataPublicApiGenerator.Helpers.SyntaxFactoryHelpers;

namespace MetadataPublicApiGenerator.Generators.TypeGenerators
{
    internal class DelegateTypeGenerator : GeneratorBase, ITypeGenerator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DelegateTypeGenerator"/> class.
        /// </summary>
        /// <param name="excludeAttributes">A set of attributes to exclude from being generated.</param>
        /// <param name="excludeMembersAttributes">A set of attributes for any types we should avoid that are decorated with these attribute types.</param>
        /// <param name="excludeFunc">A func to determine if we exclude a type or not.</param>
        /// <param name="factory">The factory for generating children.</param>
        internal DelegateTypeGenerator(ISet<string> excludeAttributes, ISet<string> excludeMembersAttributes, Func<TypeWrapper, bool> excludeFunc, IGeneratorFactory factory)
            : base(excludeAttributes, excludeMembersAttributes, factory)
        {
            ExcludeFunc = excludeFunc;
        }

        /// <inheritdoc />
        public TypeKind TypeKind => TypeKind.Delegate;

        /// <inheritdoc />
        public Func<TypeWrapper, bool> ExcludeFunc { get; }

        /// <inheritdoc />
        public MemberDeclarationSyntax Generate(TypeWrapper type, int level)
        {
            var invokeMember = type.GetDelegateInvokeMethod();

            var parameters = invokeMember.Parameters.Select(x => Factory.Generate<ParameterSyntax>(x, level)).Where(x => x != null).ToList();

            var (constraints, typeParameters) = type.GetTypeParameters(Factory);

            return DelegateDeclaration(Factory.Generate(type.Attributes, 0), type.GetModifiers(), "void", type.Name, parameters, constraints, typeParameters, level);
        }
    }
}
