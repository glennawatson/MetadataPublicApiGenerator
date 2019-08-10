// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace LightweightMetadata
{
    /// <summary>
    /// The nullability of a item.
    /// </summary>
    [SuppressMessage("Design", "CA1028: make the underlying type not byte", Justification = "Intentional to match the attributes.")]
    public enum Nullability : byte
    {
        /// <summary>
        /// If the item has no awareness of it's nullability.
        /// </summary>
        Oblivious = 0,

        /// <summary>
        /// If the item is not allowed to be nullable.
        /// </summary>
        NotNullable = 1,

        /// <summary>
        /// If the item is allowed to be null.
        /// </summary>
        Nullable = 2
    }
}
