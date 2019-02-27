// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using MetadataPublicApiGenerator.Compilation;
using MetadataPublicApiGenerator.Extensions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MetadataPublicApiGenerator.Generators
{
    internal class NamespaceGenerator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NamespaceGenerator"/> class.
        /// </summary>
        /// <param name="excludeAttributes">A set of attributes to exclude from being generated.</param>
        /// <param name="excludeMembersAttributes">A set of attributes for any types we should avoid that are decorated with these attribute types.</param>
        /// <param name="factory">The factory for generating children.</param>
        public NamespaceGenerator(ISet<string> excludeAttributes, ISet<string> excludeMembersAttributes, IGeneratorFactory factory)
        {
            ExcludeAttributes = excludeAttributes;
            ExcludeMembersAttributes = excludeMembersAttributes;
            Factory = factory;
        }

        public ISet<string> ExcludeAttributes { get; }

        public ISet<string> ExcludeMembersAttributes { get; }

        public IGeneratorFactory Factory { get; }

        public NamespaceDeclarationSyntax Generate(CompilationModule compilation, NamespaceDefinition namespaceInfo)
        {
            // Get a list of valid types that don't have attributes matching our exclude list.
            var childMembers = namespaceInfo.TypeDefinitions.Select(x => (Handle)x).OrderByAndExclude(ExcludeMembersAttributes, compilation)
                .Select(x => Factory.Generate((TypeDefinitionHandle)x, compilation))
                .ToList();

            return SyntaxFactory.NamespaceDeclaration(SyntaxFactory.IdentifierName(namespaceInfo.GetName(compilation))).WithMembers(SyntaxFactory.List(childMembers));
        }
    }
}