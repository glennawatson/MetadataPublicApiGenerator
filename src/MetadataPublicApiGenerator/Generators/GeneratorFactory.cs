// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading.Tasks;

using LightweightMetadata;
using LightweightMetadata.TypeWrappers;

using MetadataPublicApiGenerator.Extensions;
using MetadataPublicApiGenerator.Extensions.HandleTypeNamedWrapper;
using MetadataPublicApiGenerator.Generators.SymbolGenerators;
using MetadataPublicApiGenerator.Generators.TypeGenerators;
using MetadataPublicApiGenerator.Helpers;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using static MetadataPublicApiGenerator.Helpers.SyntaxFactoryHelpers;

namespace MetadataPublicApiGenerator.Generators
{
    /// <summary>
    /// A factory which will produce generators that create CSharp syntax for Roslyn.
    /// </summary>
    internal class GeneratorFactory : IGeneratorFactory
    {
        private readonly Dictionary<SymbolTypeKind, ITypeGenerator> _typeKindGenerators;

        private readonly Dictionary<HandleKind, ISymbolGenerator> _symbolKindGenerators;

        private readonly bool _shouldIncludeAssemblyAttributes;

        /// <summary>
        /// Initializes a new instance of the <see cref="GeneratorFactory"/> class.
        /// </summary>
        /// <param name="excludeAttributes">A set of attributes to exclude from being generated.</param>
        /// <param name="excludeMembersAttributes">A set of attributes for any types we should avoid that are decorated with these attribute types.</param>
        /// <param name="excludeFunc">An exclusion func which will potentially exclude attributes.</param>
        /// <param name="shouldIncludeAssemblyAttributes">If we should include assembly attributes or not.</param>
        public GeneratorFactory(ISet<string> excludeAttributes, ISet<string> excludeMembersAttributes, Func<TypeWrapper, bool> excludeFunc, bool shouldIncludeAssemblyAttributes)
        {
            _shouldIncludeAssemblyAttributes = shouldIncludeAssemblyAttributes;
            ExcludeAttributes = excludeAttributes;
            ExcludeMembersAttributes = excludeMembersAttributes;
            ExcludeFunc = excludeFunc ?? (_ => false);

            _typeKindGenerators = new Dictionary<SymbolTypeKind, ITypeGenerator>
                                      {
                                          [SymbolTypeKind.Class] = new ClassDefinitionGenerator(ExcludeAttributes, ExcludeMembersAttributes, ExcludeFunc, this),
                                          [SymbolTypeKind.Struct] = new StructTypeGenerator(ExcludeAttributes, ExcludeMembersAttributes, ExcludeFunc, this),
                                          [SymbolTypeKind.Enum] = new EnumTypeGenerator(ExcludeAttributes, ExcludeMembersAttributes, ExcludeFunc, this),
                                          [SymbolTypeKind.Interface] = new InterfaceTypeGenerator(ExcludeAttributes, ExcludeMembersAttributes, ExcludeFunc, this),
                                          [SymbolTypeKind.Delegate] = new DelegateTypeGenerator(ExcludeAttributes, ExcludeMembersAttributes, ExcludeFunc, this),
                                      };

            _symbolKindGenerators = new Dictionary<HandleKind, ISymbolGenerator>
                                        {
                                            [HandleKind.Parameter] = new ParameterSymbolGenerator(ExcludeAttributes, ExcludeMembersAttributes, this),
                                            [HandleKind.FieldDefinition] = new FieldSymbolGenerator(ExcludeAttributes, ExcludeMembersAttributes, this),
                                            [HandleKind.PropertyDefinition] = new PropertySymbolGenerator(ExcludeAttributes, ExcludeMembersAttributes, this),
                                            [HandleKind.EventDefinition] = new EventSymbolGenerator(ExcludeAttributes, ExcludeMembersAttributes, this),
                                            [HandleKind.MethodDefinition] = new MethodSymbolGenerator(ExcludeAttributes, ExcludeMembersAttributes, this),
                                            [HandleKind.CustomAttribute] = new AttributeSymbolGenerator(ExcludeAttributes, ExcludeMembersAttributes, this),
                                            [HandleKind.GenericParameter] = new TypeParameterSymbolGenerator(ExcludeAttributes, ExcludeMembersAttributes, this),
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
        public MemberDeclarationSyntax Generate(TypeWrapper typeWrapper, int level)
        {
            var typeKind = typeWrapper.TypeKind;
            return _typeKindGenerators[typeKind].Generate(typeWrapper, level);
        }

        /// <inheritdoc />
        public TOutput Generate<TOutput>(IHandleWrapper wrapper, int level)
            where TOutput : CSharpSyntaxNode =>
            (TOutput)_symbolKindGenerators[wrapper.Handle.Kind].Generate(wrapper, level);

        /// <inheritdoc />
        public CompilationUnitSyntax Generate(ICompilation compilation)
        {
            var compilationUnit = CompilationUnit();

            if (_shouldIncludeAssemblyAttributes)
            {
                compilationUnit = compilationUnit.WithAttributeLists(Generate(compilation.MainModule.MainAssembly.Attributes, 0, SyntaxKind.AssemblyKeyword));
            }

            var members = Generate(compilation.RootNamespace);
            return compilationUnit.WithMembers(List(members));
        }

        /// <inheritdoc />
        public SyntaxList<AttributeListSyntax> Generate(IEnumerable<AttributeWrapper> attributes, int level, SyntaxKind? target = null)
        {
            var validHandles = attributes
                .OrderByAndExclude(ExcludeMembersAttributes, ExcludeAttributes)
                .Select(attribute => Generate<AttributeSyntax>(attribute, 0))
                .Where(x => x != null)
                .Select(x => AttributeList(x, target, level))
                .ToList();

            return validHandles.Count == 0 ? List<AttributeListSyntax>() : List(validHandles);
        }

        /// <inheritdoc />
        public IEnumerable<MemberDeclarationSyntax> Generate(NamespaceWrapper root)
        {
            foreach (var namespaceInfo in GetAllNamespaces(root))
            {
                // Get a list of valid types that don't have attributes matching our exclude list.
                var types = namespaceInfo
                    .Members
                    .OrderByAndExclude(ExcludeMembersAttributes, ExcludeAttributes)
                    .ToList();

                if (types.Count == 0)
                {
                    continue;
                }

                var members = new MemberDeclarationSyntax[types.Count];

                Parallel.For(
                    0,
                    types.Count,
                    i =>
                        {
                            var current = types[i];
                            members[i] = Generate(current, 1).AddTrialingNewLines().AddLeadingNewLines(i == 0 ? 1 : 0);
                        });

                var namespaceName = namespaceInfo.FullName;
                if (string.IsNullOrEmpty(namespaceName))
                {
                    foreach (var member in members)
                    {
                        yield return member;
                    }
                }
                else
                {
                    yield return NamespaceDeclaration(namespaceName).WithMembers(List(members)).AddTrialingNewLines();
                }
            }
        }

        private static IEnumerable<NamespaceWrapper> GetAllNamespaces(NamespaceWrapper rootNamespace)
        {
            var namespaceProcessingStack = new Stack<NamespaceWrapper>(new[] { rootNamespace });

            while (namespaceProcessingStack.Count > 0)
            {
                var current = namespaceProcessingStack.Pop();

                yield return current;

                foreach (var child in current.ChildNamespaces.OrderByDescending(x => x.Name))
                {
                    namespaceProcessingStack.Push(child);
                }
            }
        }
    }
}