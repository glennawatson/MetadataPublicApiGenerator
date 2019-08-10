// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace LightweightMetadata
{
    /// <summary>
    /// Enum that describes the accessibility of an entity.
    /// </summary>
    public enum EntityAccessibility
    {
        // note: some code depends on the fact that these values are within the range 0-7

        /// <summary>
        /// The entity is completely inaccessible. This is used for C# explicit interface implementations.
        /// </summary>
        None,

        /// <summary>
        /// The entity is accessible everywhere.
        /// </summary>
        Public,

        /// <summary>
        /// The entity is accessible within the same project content.
        /// </summary>
        Internal,

        /// <summary>
        /// The entity is accessible both everywhere in the project content, and in all derived classes.
        /// </summary>
        /// <remarks>This corresponds to C# 'protected internal'.</remarks>
        ProtectedInternal,

        /// <summary>
        /// The entity is only accessible within the same class and in derived classes.
        /// </summary>
        Protected,

        /// <summary>
        /// The entity is accessible in derived classes within the same project content.
        /// </summary>
        /// <remarks>This corresponds to C# 'private protected'.</remarks>
        PrivateProtected,

        /// <summary>
        /// The entity is only accessible within the same class.
        /// </summary>
        Private,
    }
}
