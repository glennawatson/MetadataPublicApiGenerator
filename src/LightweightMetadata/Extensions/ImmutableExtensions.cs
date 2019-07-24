// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Collections.Immutable;

namespace LightweightMetadata.Extensions
{
    internal static class ImmutableExtensions
    {
        /// <summary>
        /// To list override to avoid boxing.
        /// </summary>
        /// <param name="input">The input array.</param>
        /// <typeparam name="T">The type of element.</typeparam>
        /// <returns>The new read only list.</returns>
        public static IReadOnlyList<T> ToList<T>(this in ImmutableArray<T> input)
        {
            var list = new List<T>(input.Length);

            foreach (var value in input)
            {
                list.Add(value);
            }

            return list;
        }
    }
}