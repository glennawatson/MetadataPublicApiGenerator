// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Reflection.Metadata;

namespace LightweightMetadata
{
    /// <summary>
    /// Represents an array.
    /// </summary>
    public class ArrayTypeWrapper : IHandleTypeNamedWrapper, IHasGenericParameters
    {
        private readonly IHandleTypeNamedWrapper _parentWrapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="ArrayTypeWrapper"/> class.
        /// </summary>
        /// <param name="module">The module that owns the array.</param>
        /// <param name="elementType">The wrapper to the element type.</param>
        /// <param name="arrayShapeData">The dimension of the array.</param>
        public ArrayTypeWrapper(IMetadataRepository module, IHandleTypeNamedWrapper elementType, ArrayShapeData arrayShapeData)
        {
            if (module == null)
            {
                throw new ArgumentNullException(nameof(module));
            }

            _parentWrapper = module.GetTypeByName("System.Array");
            ElementType = elementType ?? throw new ArgumentNullException(nameof(elementType));
            ArrayShapeData = arrayShapeData;
        }

        /// <summary>
        /// Gets the type that the array elements.
        /// </summary>
        public IHandleTypeNamedWrapper ElementType { get; }

        /// <summary>
        /// Gets the array shape data.
        /// </summary>
        public ArrayShapeData ArrayShapeData { get; }

        /// <inheritdoc />
        public string Name => ElementType.Name;

        /// <inheritdoc />
        public string FullName => ElementType.FullName;

        /// <inheritdoc />
        public string ReflectionFullName => ElementType.ReflectionFullName;

        /// <inheritdoc />
        public string TypeNamespace => _parentWrapper.TypeNamespace;

        /// <inheritdoc />
        public EntityAccessibility Accessibility => _parentWrapper.Accessibility;

        /// <inheritdoc />
        public bool IsAbstract => _parentWrapper.IsAbstract;

        /// <inheritdoc />
        public KnownTypeCode KnownType => _parentWrapper.KnownType;

        /// <inheritdoc />
        public AssemblyMetadata CompilationModule => _parentWrapper?.CompilationModule;

        /// <inheritdoc />
        public Handle Handle => _parentWrapper.Handle;

        /// <inheritdoc />
        public IReadOnlyList<GenericParameterWrapper> GenericParameters => _parentWrapper is IHasGenericParameters ? ((IHasGenericParameters)_parentWrapper).GenericParameters : Array.Empty<GenericParameterWrapper>();

        /// <inheritdoc />
        public override string ToString()
        {
            return FullName;
        }
    }
}
