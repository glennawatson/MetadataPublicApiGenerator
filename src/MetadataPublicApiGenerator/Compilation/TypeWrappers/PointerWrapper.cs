// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reflection.Metadata;

namespace MetadataPublicApiGenerator.Compilation.TypeWrappers
{
    /// <summary>
    /// Represents a type that is wrapped by a pointer.
    /// </summary>
    internal class PointerWrapper : IHandleTypeNamedWrapper
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PointerWrapper"/> class.
        /// </summary>
        /// <param name="typeDefinition">The type definition.</param>
        public PointerWrapper(TypeWrapper typeDefinition)
        {
            TypeDefinition = typeDefinition;
        }

        /// <summary>
        /// Gets the type definition that is wrapped by the pointer.
        /// </summary>
        public TypeWrapper TypeDefinition { get; }

        /// <inheritdoc />
        public bool IsAbstract => TypeDefinition.IsAbstract;

        /// <inheritdoc />
        public virtual string Name => TypeDefinition.Name + "*";

        /// <inheritdoc />
        public string Namespace => TypeDefinition.Namespace;

        /// <inheritdoc />
        public bool IsPublic => TypeDefinition.IsPublic;

        /// <inheritdoc />
        public string FullName => Namespace + "." + Name;

        /// <inheritdoc />
        public Handle Handle => TypeDefinition.Handle;

        /// <inheritdoc />
        public CompilationModule Module => TypeDefinition.Module;
    }
}
