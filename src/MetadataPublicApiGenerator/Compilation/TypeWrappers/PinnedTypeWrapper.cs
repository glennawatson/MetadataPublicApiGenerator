// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reflection.Metadata;

namespace MetadataPublicApiGenerator.Compilation.TypeWrappers
{
    internal class PinnedTypeWrapper : IHandleTypeNamedWrapper
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PinnedTypeWrapper"/> class.
        /// </summary>
        /// <param name="typeDefinition">The type definition.</param>
        public PinnedTypeWrapper(TypeWrapper typeDefinition)
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
        public virtual string Name => TypeDefinition.Name + " pinned";

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
