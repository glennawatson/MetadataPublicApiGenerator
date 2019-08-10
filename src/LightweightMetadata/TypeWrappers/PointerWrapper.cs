// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace LightweightMetadata
{
    /// <summary>
    /// Represents a type that is wrapped by a pointer.
    /// </summary>
    public class PointerWrapper : AbstractEnclosedTypeWrapper
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PointerWrapper"/> class.
        /// </summary>
        /// <param name="typeDefinition">The type definition.</param>
        public PointerWrapper(TypeWrapper typeDefinition)
            : base(typeDefinition)
        {
        }
    }
}
