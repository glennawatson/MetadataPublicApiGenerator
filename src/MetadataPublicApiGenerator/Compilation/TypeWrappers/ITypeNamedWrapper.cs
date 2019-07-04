// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace MetadataPublicApiGenerator.Compilation.TypeWrappers
{
    /// <summary>
    /// Represents a element that is named.
    /// </summary>
    internal interface ITypeNamedWrapper : INamedWrapper
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
        /// Gets the full name of the namespace containing this entity.
        /// </summary>
        string Namespace { get; }

        /// <summary>
        /// Gets a value indicating whether if this is a known type.
        /// </summary>
        bool IsKnownType { get; }

        /// <summary>
        /// Gets a value indicating whether if this is a enum type.
        /// </summary>
        bool IsEnumType { get; }

        /// <summary>
        /// Gets a value indicating whether if this is a public type.
        /// </summary>
        bool IsPublic { get; }

        /// <summary>
        /// Gets a value indicating whether if this type if abstract.
        /// </summary>
        bool IsAbstract { get; }

        /// <summary>
        /// Gets the module that this wrapped item belongs to.
        /// </summary>
        CompilationModule Module { get; }
    }
}
