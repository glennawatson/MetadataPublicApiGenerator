// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace LightweightMetadata
{
    /// <summary>
    /// A type that has been pinned.
    /// </summary>
    public class PinnedTypeWrapper : AbstractEnclosedTypeWrapper
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PinnedTypeWrapper"/> class.
        /// </summary>
        /// <param name="typeDefinition">The type definition.</param>
        public PinnedTypeWrapper(TypeWrapper typeDefinition)
             : base(typeDefinition)
        {
        }
    }
}
