// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics;
using System.Reflection.Metadata;

namespace LightweightMetadata.TypeWrappers
{
    /// <summary>
    /// Represents an array.
    /// </summary>
    [DebuggerDisplay("{" + nameof(FullName) + "}")]
    public class ArrayTypeWrapper : IHandleTypeNamedWrapper
    {
        private readonly TypeWrapper _parentWrapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="ArrayTypeWrapper"/> class.
        /// </summary>
        /// <param name="module">The module that owns the array.</param>
        /// <param name="elementType">The wrapper to the element type.</param>
        /// <param name="dimensions">The dimension of the array.</param>
        public ArrayTypeWrapper(ICompilation module, IHandleTypeNamedWrapper elementType, int dimensions)
        {
            if (module == null)
            {
                throw new System.ArgumentNullException(nameof(module));
            }

            _parentWrapper = module.GetTypeByName("System.Array");
            ElementType = elementType ?? throw new System.ArgumentNullException(nameof(elementType));
            Dimensions = dimensions;
        }

        /// <summary>
        /// Gets the type that the array elements.
        /// </summary>
        public IHandleTypeNamedWrapper ElementType { get; }

        /// <summary>
        /// Gets the number of dimensions ot the array.
        /// </summary>
        public int Dimensions { get; }

        /// <inheritdoc />
        public string Name => ElementType.Name + "[" + new string(',', Dimensions - 1) + "]";

        /// <inheritdoc />
        public string FullName => ElementType.FullName + "[" + new string(',', Dimensions - 1) + "]";

        /// <inheritdoc />
        public string ReflectionFullName => ElementType.ReflectionFullName + "[" + new string(',', Dimensions - 1) + "]";

        /// <inheritdoc />
        public string TypeNamespace => _parentWrapper.TypeNamespace;

        /// <inheritdoc />
        public EntityAccessibility Accessibility => _parentWrapper.Accessibility;

        /// <inheritdoc />
        public bool IsAbstract => _parentWrapper.IsAbstract;

        /// <inheritdoc />
        public KnownTypeCode KnownType => _parentWrapper.KnownType;

        /// <inheritdoc />
        public CompilationModule CompilationModule => _parentWrapper?.CompilationModule;

        /// <inheritdoc />
        public Handle Handle => _parentWrapper.Handle;
    }
}
