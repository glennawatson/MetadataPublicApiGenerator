// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace LightweightMetadata
{
    /// <summary>
    /// The type includes type arguments.
    /// </summary>
    public interface IHasTypeArguments
    {
        /// <summary>
        /// Gets the type arguments for the type.
        /// </summary>
        IReadOnlyList<IHandleTypeNamedWrapper> TypeArguments { get; }
    }
}
