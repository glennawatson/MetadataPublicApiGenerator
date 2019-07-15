// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics;
using System.Reflection.Metadata;

namespace LightweightMetadata.TypeWrappers
{
    /// <summary>
    /// A type that has been pinned.
    /// </summary>
    public class PinnedTypeWrapper : IHandleTypeNamedWrapper
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
        public KnownTypeCode KnownType => TypeDefinition.KnownType;

        /// <inheritdoc />
        public virtual string Name => TypeDefinition.Name + " pinned";

        /// <inheritdoc />
        public string ReflectionFullName => TypeDefinition.ReflectionFullName + " pinned";

        /// <inheritdoc />
        public string TypeNamespace => TypeDefinition.TypeNamespace;

        /// <inheritdoc />
        public EntityAccessibility Accessibility => TypeDefinition.Accessibility;

        /// <inheritdoc />
        public string FullName => TypeDefinition.FullName + " pinned";

        /// <inheritdoc />
        public Handle Handle => TypeDefinition.Handle;

        /// <inheritdoc />
        public CompilationModule CompilationModule => TypeDefinition.CompilationModule;
    }
}
