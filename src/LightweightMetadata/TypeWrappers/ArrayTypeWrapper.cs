// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace LightweightMetadata
{
    /// <summary>
    /// Represents an array.
    /// </summary>
    public class ArrayTypeWrapper : AbstractEnclosedTypeWrapper
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ArrayTypeWrapper"/> class.
        /// </summary>
        /// <param name="elementType">The wrapper to the element type.</param>
        /// <param name="arrayShapeData">The dimension of the array.</param>
        public ArrayTypeWrapper(IHandleTypeNamedWrapper elementType, ArrayShapeData arrayShapeData)
            : base(elementType)
        {
            ArrayShapeData = arrayShapeData;
        }

        /// <summary>
        /// Gets the array shape data.
        /// </summary>
        public ArrayShapeData ArrayShapeData { get; }
    }
}
