// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reflection.Metadata;

namespace MetadataPublicApiGenerator.Compilation.TypeWrappers
{
    /// <summary>
    /// Represents a wrapper that has a handle.
    /// </summary>
    internal interface IHandleWrapper
    {
        /// <summary>
        /// Gets the handle for the wrapped type.
        /// </summary>
        Handle Handle { get; }
    }
}
