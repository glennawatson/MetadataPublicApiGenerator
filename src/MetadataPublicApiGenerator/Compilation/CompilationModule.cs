// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using Lazy;
using MetadataPublicApiGenerator.Extensions;

namespace MetadataPublicApiGenerator.Compilation
{
    internal class CompilationModule
    {
        public CompilationModule(PEReader reader, ICompilation compilation)
        {
            MetadataReader = reader.GetMetadataReader();
            Compilation = compilation;
            MethodSemanticsLookup = new MethodSemanticsLookup(MetadataReader);
            TypeProvider = Compilation.TypeProvider;
        }

        /// <summary>
        /// Gets the module reader.
        /// </summary>
        public MetadataReader MetadataReader { get; }

        /// <summary>
        /// Gets the compilation that this module belongs to.
        /// </summary>
        public ICompilation Compilation { get; }

        /// <summary>
        /// Gets all the public type definition handles for this module.
        /// </summary>
        [Lazy]
        public ImmutableList<TypeDefinitionHandle> PublicTypeDefinitionHandles => MetadataReader.TypeDefinitions.Where(x => (x.Resolve(this).Attributes & System.Reflection.TypeAttributes.Public) != 0).ToImmutableList();

        /// <summary>
        /// Gets details about methods.
        /// </summary>
        [Lazy]
        public MethodSemanticsLookup MethodSemanticsLookup { get; }

        /// <summary>
        /// Gets the type provider.
        /// </summary>
        public TypeProvider TypeProvider { get; }
    }
}
