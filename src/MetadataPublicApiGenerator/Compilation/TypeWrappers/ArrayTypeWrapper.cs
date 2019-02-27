// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Linq;
using Lazy;

using MetadataPublicApiGenerator.Extensions;

namespace MetadataPublicApiGenerator.Compilation.TypeWrappers
{
    internal class ArrayTypeWrapper : TypeWrapper
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ArrayTypeWrapper"/> class.
        /// </summary>
        /// <param name="module">The module that owns the array.</param>
        /// <param name="elementType">The wrapper to the element type.</param>
        /// <param name="dimensions">The dimension of the array.</param>
        public ArrayTypeWrapper(CompilationModule module, ITypeNamedWrapper elementType, int dimensions)
            : base(module, module.Compilation.GetTypeDefinitionByName("System.Array").FirstOrDefault().typeDefinitionHandle)
        {
            ElementType = elementType;
            Dimensions = dimensions;
        }

        /// <summary>
        /// Gets the type that the array elements.
        /// </summary>
        public ITypeNamedWrapper ElementType { get; }

        /// <summary>
        /// Gets the number of dimensions ot the array.
        /// </summary>
        public int Dimensions { get; }

        /// <inheritdoc />
        [Lazy]
        public override string Name => ElementType.Name + "[" + new string(',', Dimensions - 1) + "]";

        /// <inheritdoc />
        public override bool IsKnownType => true;
    }
}
