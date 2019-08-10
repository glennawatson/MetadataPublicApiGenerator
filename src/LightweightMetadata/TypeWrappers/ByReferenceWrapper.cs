// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace LightweightMetadata
{
    /// <summary>
    /// A type that is passed by reference.
    /// </summary>
    public class ByReferenceWrapper : AbstractEnclosedTypeWrapper
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ByReferenceWrapper"/> class.
        /// </summary>
        /// <param name="typeDefinition">The handle to the type definition it's wrapping.</param>
        public ByReferenceWrapper(IHandleTypeNamedWrapper typeDefinition)
            : base(typeDefinition)
        {
        }
    }
}
