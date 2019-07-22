// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection.Metadata;

namespace LightweightMetadata.TypeWrappers
{
    /// <summary>
    /// Represents a type that is wrapped by a pointer.
    /// </summary>
    [DebuggerDisplay("{" + nameof(FullName) + "}")]
    public class PointerWrapper : IHandleTypeNamedWrapper, IHasGenericParameters
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
        public KnownTypeCode KnownType => TypeDefinition.KnownType;

        /// <inheritdoc />
        public virtual string Name => TypeDefinition.Name;

        /// <inheritdoc />
        public string ReflectionFullName => TypeDefinition.ReflectionFullName;

        /// <inheritdoc />
        public string TypeNamespace => TypeDefinition.TypeNamespace;

        /// <inheritdoc />
        public EntityAccessibility Accessibility => TypeDefinition.Accessibility;

        /// <inheritdoc />
        public string FullName => TypeDefinition.FullName;

        /// <inheritdoc />
        public Handle Handle => TypeDefinition.Handle;

        /// <inheritdoc />
        public CompilationModule CompilationModule => TypeDefinition.CompilationModule;

        /// <inheritdoc />
        public IReadOnlyList<GenericParameterWrapper> GenericParameters => TypeDefinition.GenericParameters;
    }
}
