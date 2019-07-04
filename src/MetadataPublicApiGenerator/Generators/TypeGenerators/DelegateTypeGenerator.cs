// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using MetadataPublicApiGenerator.Compilation;
using MetadataPublicApiGenerator.Extensions;
using MetadataPublicApiGenerator.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

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
        internal DelegateTypeGenerator(ISet<string> excludeAttributes, ISet<string> excludeMembersAttributes, Func<TypeDefinition, bool> excludeFunc, IGeneratorFactory factory)
            : base(excludeAttributes, excludeMembersAttributes, factory)
        {
            ExcludeFunc = excludeFunc;
        }

        /// <inheritdoc />
        public TypeKind TypeKind => TypeKind.Delegate;

        /// <inheritdoc />
        public Func<TypeDefinition, bool> ExcludeFunc { get; }

        /// <inheritdoc />
        public MemberDeclarationSyntax Generate(CompilationModule compilation, TypeDefinitionHandle typeHandle)
        {
            var type = typeHandle.Resolve(compilation);
            if (ExcludeFunc(type))
            {
                return null;
            }

            var invokeMember = type.GetDelegateInvokeMethod(compilation);

            var parameters = invokeMember.GetParameters().Select(x => Factory.Generate<ParameterSyntax>(x, compilation)).Where(x => x != null).ToList();

            var returnValue = SyntaxFactory.DelegateDeclaration(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)), type.GetName(compilation))
                .WithGenericParameterList(compilation, invokeMember)
                .WithAttributeLists(AttributeGenerator.GenerateAttributes(compilation, type.GetCustomAttributes().Select(x => x.Resolve(compilation)), ExcludeAttributes))
                .WithModifiers(type.GetModifiers());

            if (parameters.Count > 0)
            {
                returnValue.WithParameterList(SyntaxFactory.ParameterList(SyntaxFactory.SeparatedList(parameters)));
            }

            return returnValue;
        }
    }
}
