// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace LightweightMetadata
{
    /// <summary>
    /// If the type has attributes associated with return values.
    /// </summary>
    public interface IHasReturnAttributes
    {
        /// <summary>
        /// Gets return attributes.
        /// </summary>
        IReadOnlyList<AttributeWrapper> ReturnAttributes { get; }
    }
}
