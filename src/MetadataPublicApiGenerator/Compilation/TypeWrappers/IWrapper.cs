// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reflection.Metadata;

namespace MetadataPublicApiGenerator.Compilation.TypeWrappers
{
    /// <summary>
    /// A wrapper around the Metadata property system. Required for decoding stuff.
    /// </summary>
    internal interface IWrapper
    {
        /// <summary>
        /// Gets a value indicating whether this is a known type.
        /// </summary>
        bool IsKnownType { get; }

        /// <summary>
        /// Gets the module that this wrapped item belongs to.
        /// </summary>
        CompilationModule Module { get; }
    }
}
