// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Reflection.Metadata;

namespace LightweightMetadata
{
    /// <summary>
    /// Represents a modified type.
    /// </summary>
    public class ModifiedTypeWrapper : IHandleTypeNamedWrapper
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ModifiedTypeWrapper"/> class.
        /// </summary>
        /// <param name="module">The module that hosts the type.</param>
        /// <param name="modifier">The modifier of the first type.</param>
        /// <param name="unmodifiedType">The unmodified type.</param>
        /// <param name="isRequired">If the type is required.</param>
        public ModifiedTypeWrapper(AssemblyMetadata module, IHandleTypeNamedWrapper modifier, IHandleTypeNamedWrapper unmodifiedType, bool isRequired)
        {
            CompilationModule = module ?? throw new ArgumentNullException(nameof(module));
            Modifier = modifier ?? throw new ArgumentNullException(nameof(modifier));
            Unmodified = unmodifiedType ?? throw new ArgumentNullException(nameof(unmodifiedType));
            IsRequired = isRequired;
        }

        /// <summary>
        /// Gets the modifier type.
        /// </summary>
        public IHandleTypeNamedWrapper Modifier { get; }

        /// <summary>
        /// Gets the unmodified type.
        /// </summary>
        public IHandleTypeNamedWrapper Unmodified { get; }

        /// <summary>
        /// Gets a value indicating whether the modification is required.
        /// </summary>
        public bool IsRequired { get; }

        /// <inheritdoc/>
        public string Name => Unmodified.Name + (IsRequired ? " modreq" : " modopt") + $"({Modifier.Name})";

        /// <inheritdoc/>
        public string FullName => Unmodified.FullName + (IsRequired ? " modreq" : " modopt") + $"({Modifier.Name})";

        /// <inheritdoc/>
        public string ReflectionFullName => Unmodified.ReflectionFullName + (IsRequired ? " modreq" : " modopt") + $"({Modifier.Name})";

        /// <inheritdoc/>
        public string TypeNamespace => Unmodified.TypeNamespace;

        /// <inheritdoc />
        public EntityAccessibility Accessibility => Unmodified.Accessibility;

        /// <inheritdoc />
        public bool IsAbstract => Unmodified.IsAbstract;

        /// <inheritdoc />
        public KnownTypeCode KnownType => Unmodified.KnownType;

        /// <inheritdoc />
        public Handle Handle => Unmodified.Handle;

        /// <inheritdoc/>
        public AssemblyMetadata CompilationModule { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return FullName;
        }
    }
}
