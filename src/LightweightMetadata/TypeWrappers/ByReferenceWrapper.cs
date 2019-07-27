// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;
using System.Reflection.Metadata;

namespace LightweightMetadata
{
    /// <summary>
    /// A type that is passed by reference.
    /// </summary>
    [DebuggerDisplay("{" + nameof(FullName) + "}")]
    public class ByReferenceWrapper : IHandleTypeNamedWrapper
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ByReferenceWrapper"/> class.
        /// </summary>
        /// <param name="typeDefinition">The handle to the type definition it's wrapping.</param>
        public ByReferenceWrapper(IHandleTypeNamedWrapper typeDefinition)
        {
            TypeDefinition = typeDefinition ?? throw new ArgumentNullException(nameof(typeDefinition));
        }

        /// <summary>
        /// Gets the hosting type definition.
        /// </summary>
        public IHandleTypeNamedWrapper TypeDefinition { get; }

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
        public AssemblyMetadata AssemblyMetadata => TypeDefinition.AssemblyMetadata;

        /// <inheritdoc />
        public override string ToString()
        {
            return FullName;
        }
    }
}
