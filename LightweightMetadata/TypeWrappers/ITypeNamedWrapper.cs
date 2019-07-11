// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics;

namespace LightweightMetadata.TypeWrappers
{
    /// <summary>
    /// Represents a element that is named.
    /// </summary>
    public interface ITypeNamedWrapper : INamedWrapper
    {
        /// <summary>
        /// Gets the fully qualified name of the class the return type is pointing to.
        /// </summary>
        /// <returns>
        /// "System.Int32[]" for int[]<br/>
        /// "System.Collections.Generic.List" for List&lt;string&gt;
        /// "System.Environment.SpecialFolder" for Environment.SpecialFolder.
        /// </returns>
        string FullName { get; }

        /// <summary>
        /// Gets the name using reflection.
        /// </summary>
        string ReflectionFullName { get; }

        /// <summary>
        /// Gets the full name of the namespace containing this entity.
        /// </summary>
        string TypeNamespace { get; }

        /// <summary>
        /// Gets the accessibility of the member.
        /// </summary>
        EntityAccessibility Accessibility { get; }

        /// <summary>
        /// Gets a value indicating whether if this type if abstract.
        /// </summary>
        bool IsAbstract { get; }

        /// <summary>
        /// Gets the known type code of this entity. If it is not known then it will be set to None.
        /// </summary>
        KnownTypeCode KnownType { get; }
    }
}
