// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Reflection.Metadata;

namespace LightweightMetadata
{
    /// <summary>
    /// A abstract class which handles wrappers that enclose other wrappers.
    /// </summary>
    public abstract class AbstractEnclosedTypeWrapper : IHandleTypeNamedWrapper, IHasGenericParameters, IEnclosesType, IHasTypeArguments
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractEnclosedTypeWrapper"/> class.
        /// </summary>
        /// <param name="enclosedWrapper">The wrapper which the type is enclosing.</param>
        protected AbstractEnclosedTypeWrapper(IHandleTypeNamedWrapper enclosedWrapper)
        {
            EnclosedType = enclosedWrapper;
            GenericParameters = (enclosedWrapper as IHasGenericParameters)?.GenericParameters ?? Array.Empty<GenericParameterWrapper>();
            Attributes = enclosedWrapper.Attributes ?? Array.Empty<AttributeWrapper>();
            TypeArguments = (enclosedWrapper as IHasTypeArguments)?.TypeArguments ?? Array.Empty<IHandleTypeNamedWrapper>();
        }

        /// <inheritdoc />
        public string Name => EnclosedType.Name;

        /// <inheritdoc />
        public string FullName => EnclosedType.FullName;

        /// <inheritdoc />
        public string ReflectionFullName => EnclosedType.ReflectionFullName;

        /// <inheritdoc />
        public string? TypeNamespace => EnclosedType.TypeNamespace;

        /// <inheritdoc />
        public EntityAccessibility Accessibility => EnclosedType.Accessibility;

        /// <inheritdoc />
        public bool IsAbstract => EnclosedType.IsAbstract;

        /// <inheritdoc />
        public KnownTypeCode KnownType => EnclosedType.KnownType;

        /// <inheritdoc />
        public Handle Handle => EnclosedType.Handle;

        /// <inheritdoc />
        public AssemblyMetadata AssemblyMetadata => EnclosedType.AssemblyMetadata;

        /// <inheritdoc />
        public IReadOnlyList<GenericParameterWrapper> GenericParameters { get; }

        /// <inheritdoc />
        public IReadOnlyList<AttributeWrapper> Attributes { get; }

        /// <inheritdoc />
        public virtual IReadOnlyList<IHandleTypeNamedWrapper> TypeArguments { get; }

        /// <inheritdoc />
        public bool IsValueType => EnclosedType.IsValueType;

        /// <summary>
        /// Gets the enclosed wrapper.
        /// </summary>
        public IHandleTypeNamedWrapper EnclosedType { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return FullName;
        }
    }
}
