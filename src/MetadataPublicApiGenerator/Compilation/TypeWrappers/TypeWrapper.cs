// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reflection.Metadata;
using Lazy;
using MetadataPublicApiGenerator.Extensions;

namespace MetadataPublicApiGenerator.Compilation.TypeWrappers
{
    /// <summary>
    /// A wrapper around a type.
    /// </summary>
    internal class TypeWrapper : ITypeWrapper
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TypeWrapper"/> class.
        /// </summary>
        /// <param name="module">The main compilation module.</param>
        /// <param name="typeDefinition">The type definition we are wrapping.</param>
        public TypeWrapper(CompilationModule module, TypeDefinitionHandle typeDefinition)
        {
            Handle = typeDefinition;
            Module = module;
        }

        /// <summary>
        /// Gets the type definition for the type.
        /// </summary>
        [Lazy]
        public TypeDefinition TypeDefinition => ((TypeDefinitionHandle)Handle).Resolve(Module);

        /// <inheritdoc />
        [Lazy]
        public virtual string Name => TypeDefinition.GetName(Module);

        /// <inheritdoc />
        [Lazy]
        public string Namespace => TypeDefinition.Namespace.GetName(Module);

        /// <inheritdoc />
        [Lazy]
        public string FullName => TypeDefinition.GetFullName(Module);

        /// <inheritdoc />
        [Lazy]
        public virtual bool IsKnownType => TypeDefinition.IsKnownType(Module) != KnownTypeCode.None;

        /// <inheritdoc />
        public Handle Handle { get; }

        /// <inheritdoc />
        public CompilationModule Module { get; }
    }
}
