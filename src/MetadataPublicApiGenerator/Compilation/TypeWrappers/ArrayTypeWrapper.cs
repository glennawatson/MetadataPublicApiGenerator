// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Linq;
using System.Reflection.Metadata;

using MetadataPublicApiGenerator.Extensions;

namespace MetadataPublicApiGenerator.Compilation.TypeWrappers
{
    internal class ArrayTypeWrapper : ITypeWrapper
    {
        private readonly TypeWrapper _parentWrapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="ArrayTypeWrapper"/> class.
        /// </summary>
        /// <param name="module">The module that owns the array.</param>
        /// <param name="elementType">The wrapper to the element type.</param>
        /// <param name="dimensions">The dimension of the array.</param>
        public ArrayTypeWrapper(ICompilation module, ITypeNamedWrapper elementType, int dimensions)
        {
            if (module == null)
            {
                throw new System.ArgumentNullException(nameof(module));
            }

            _parentWrapper = module.GetTypeDefinitionByName("System.Array").FirstOrDefault().typeWrapper;
            ElementType = elementType ?? throw new System.ArgumentNullException(nameof(elementType));
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
        public string Name => ElementType.Name + "[" + new string(',', Dimensions - 1) + "]";

        /// <inheritdoc />
        public string FullName => _parentWrapper.FullName;

        /// <inheritdoc />
        public string Namespace => _parentWrapper.Namespace;

        /// <inheritdoc />
        public bool IsKnownType => true;

        /// <inheritdoc />
        public CompilationModule Module => _parentWrapper?.Module;

        /// <inheritdoc />
        public Handle Handle => _parentWrapper.Handle;
    }
}
