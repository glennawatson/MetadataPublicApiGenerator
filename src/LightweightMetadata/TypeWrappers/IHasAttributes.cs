// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace LightweightMetadata
{
    /// <summary>
    /// A class that has attributes.
    /// </summary>
    public interface IHasAttributes
    {
        /// <summary>
        /// Gets the attributes.
        /// </summary>
        IReadOnlyList<AttributeWrapper> Attributes { get; }
    }
}
