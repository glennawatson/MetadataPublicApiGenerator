// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace LightweightMetadata
{
    /// <summary>
    /// Contains information about a array's shape data.
    /// </summary>
    public class ArrayShapeData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ArrayShapeData"/> class.
        /// </summary>
        /// <param name="rank">The rank of the array.</param>
        /// <param name="sizes">The sizes of the array.</param>
        /// <param name="lowerBounds">The lower bound of the array.</param>
        public ArrayShapeData(int rank, IEnumerable<int> sizes, IEnumerable<int> lowerBounds)
        {
            Rank = rank;
            Sizes = sizes?.ToArray() ?? Array.Empty<int>();
            LowerBounds = lowerBounds?.ToArray() ?? Array.Empty<int>();
        }

        /// <summary>
        /// Gets the number of dimensions in the array.
        /// </summary>
        public int Rank { get; }

        /// <summary>
        /// Gets the sizes of each dimension. Length may be smaller than rank, in which case the trailing dimensions have unspecified sizes.
        /// </summary>
        public IReadOnlyList<int> Sizes { get; }

        /// <summary>
        /// Gets the lower-bounds of each dimension. Length may be smaller than rank, in which case the trailing dimensions have unspecified lower bounds.
        /// </summary>
        public IReadOnlyList<int> LowerBounds { get; }
    }
}
