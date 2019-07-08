// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using MetadataPublicApiGenerator.Compilation;
using MetadataPublicApiGenerator.Compilation.TypeWrappers;
using MetadataPublicApiGenerator.Extensions;
using MetadataPublicApiGenerator.Generators.SymbolGenerators;
using MetadataPublicApiGenerator.Generators.TypeGenerators;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MetadataPublicApiGenerator.Generators
{
    /// <summary>
    /// A factory which will produce generators that create CSharp syntax for Roslyn.
    /// </summary>
    internal class GeneratorFactory : IGeneratorFactory
    {
        private readonly Dictionary<TypeKind, ITypeGenerator> _typeKindGenerators;
        private readonly Dictionary<HandleKind, ISymbolGenerator> _symbolKindGenerators;
        private readonly NamespaceMembersGenerator _namespaceGenerator;

        /// <summary>
        /// Initializes a new instance of the <see cref="GeneratorFactory"/> class.
        /// </summary>
        /// <param name="excludeAttributes">A set of attributes to exclude from being generated.</param>
        /// <param name="excludeMembersAttributes">A set of attributes for any types we should avoid that are decorated with these attribute types.</param>
        /// <param name="excludeFunc">An exclusion func which will potentially exclude attributes.</param>
        public GeneratorFactory(ISet<string> excludeAttributes, ISet<string> excludeMembersAttributes, Func<TypeWrapper, bool> excludeFunc)
        {
            ExcludeAttributes = excludeAttributes;
            ExcludeMembersAttributes = excludeMembersAttributes;
            ExcludeFunc = excludeFunc ?? (_ => false);

            _namespaceGenerator = new NamespaceMembersGenerator(ExcludeAttributes, ExcludeMembersAttributes, this);
            _typeKindGenerators = new Dictionary<TypeKind, ITypeGenerator>
            {
                [TypeKind.Class] = new ClassDefinitionGenerator(ExcludeAttributes, ExcludeMembersAttributes, ExcludeFunc, this),
                [TypeKind.Struct] = new StructTypeGenerator(ExcludeAttributes, ExcludeMembersAttributes, ExcludeFunc, this),
                [TypeKind.Enum] = new EnumTypeGenerator(ExcludeAttributes, ExcludeMembersAttributes, ExcludeFunc, this),
                [TypeKind.Interface] = new InterfaceTypeGenerator(ExcludeAttributes, ExcludeMembersAttributes, ExcludeFunc, this),
                [TypeKind.Delegate] = new DelegateTypeGenerator(ExcludeAttributes, ExcludeMembersAttributes, ExcludeFunc, this),
            };

            _symbolKindGenerators = new Dictionary<HandleKind, ISymbolGenerator>
            {
                [HandleKind.Parameter] = new ParameterSymbolGenerator(ExcludeAttributes, ExcludeMembersAttributes, this),
                [HandleKind.FieldDefinition] = new FieldSymbolGenerator(ExcludeAttributes, ExcludeMembersAttributes, this),
                [HandleKind.PropertyDefinition] = new PropertySymbolGenerator(ExcludeAttributes, ExcludeMembersAttributes, this),
                [HandleKind.EventDefinition] = new EventSymbolGenerator(ExcludeAttributes, ExcludeMembersAttributes, this),
                [HandleKind.MethodDefinition] = new MethodSymbolGenerator(ExcludeAttributes, ExcludeMembersAttributes, this),
            };
        }

        /// <summary>
        /// Gets a set of attributes to exclude from being generated.
        /// </summary>
        public ISet<string> ExcludeAttributes { get; }

        /// <summary>
        /// Gets a set of attributes for any types we should avoid that are decorated with these attribute types.
        /// </summary>
        public ISet<string> ExcludeMembersAttributes { get; }

        /// <summary>
        /// Gets an exclusion func which will potentially exclude attributes.
        /// </summary>
        public Func<TypeWrapper, bool> ExcludeFunc { get; }

        /// <inheritdoc />
        public MemberDeclarationSyntax Generate(TypeWrapper typeWrapper)
        {
            var typeKind = typeWrapper.TypeKind;
            return _typeKindGenerators[typeKind].Generate(typeWrapper);
        }

        /// <inheritdoc />
        public TOutput Generate<TOutput>(IHandleNameWrapper wrapper)
            where TOutput : CSharpSyntaxNode => (TOutput)_symbolKindGenerators[wrapper.Handle.Kind].Generate(wrapper);

        /// <inheritdoc />
        public IReadOnlyCollection<MemberDeclarationSyntax> GenerateMembers(NamespaceWrapper namespaceInfo)
        {
            return _namespaceGenerator.Generate(namespaceInfo);
        }
    }
}
