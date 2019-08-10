// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace LightweightMetadata
{
    /// <summary>
    /// Represents the variance of a type parameter.
    /// </summary>
    public enum VarianceType
    {
        /// <summary>
        /// The type parameter is not variant.
        /// </summary>
        Invariant,

        /// <summary>
        /// The type parameter is covariant (used in output position).
        /// </summary>
        Covariant,

        /// <summary>
        /// The type parameter is contravariant (used in input position).
        /// </summary>
        Contravariant
    }
}
