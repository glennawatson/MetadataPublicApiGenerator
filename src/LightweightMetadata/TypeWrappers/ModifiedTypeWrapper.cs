﻿// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;

namespace LightweightMetadata
{
    /// <summary>
    /// Represents a modified type.
    /// </summary>
    public class ModifiedTypeWrapper : AbstractEnclosedTypeWrapper
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ModifiedTypeWrapper"/> class.
        /// </summary>
        /// <param name="modifier">The modifier of the first type.</param>
        /// <param name="unmodifiedType">The unmodified type.</param>
        /// <param name="isRequired">If the type is required.</param>
        public ModifiedTypeWrapper(IHandleTypeNamedWrapper modifier, IHandleTypeNamedWrapper unmodifiedType, bool isRequired)
            : base(unmodifiedType)
        {
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
    }
}
