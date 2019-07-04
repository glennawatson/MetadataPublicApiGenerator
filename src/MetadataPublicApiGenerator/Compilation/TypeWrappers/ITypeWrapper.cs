// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Text;

using Microsoft.CodeAnalysis;

namespace MetadataPublicApiGenerator.Compilation.TypeWrappers
{
    /// <summary>
    /// Represents a type wrapper with a definition.
    /// </summary>
    internal interface ITypeWrapper : IHandleWrapper, ITypeNamedWrapper
    {
        /// <summary>
        /// Gets the base instance.
        /// </summary>
        ITypeNamedWrapper Base { get; }

        /// <summary>
        /// Gets the type kind of the type.
        /// </summary>
        TypeKind TypeKind { get; }

        /// <summary>
        /// Gets a value indicating whether if this is a known type.
        /// </summary>
        bool IsKnownType { get; }

        /// <summary>
        /// Gets a value indicating whether if this is a enum type.
        /// </summary>
        bool IsEnumType { get; }

        /// <summary>
        /// Gets the attributes on the type.
        /// </summary>
        IReadOnlyList<AttributeWrapper> Attributes { get; }

        /// <summary>
        /// Gets a set of generic constraints on the type.
        /// </summary>
        IReadOnlyDictionary<string, IReadOnlyList<string>> Constraints { get; }

        /// <summary>
        /// Gets the enum type, if it's available.
        /// </summary>
        /// <param name="underlyingType">Output parameter of the underlying type.</param>
        /// <returns>If the value is enum or not.</returns>
        bool TryGetEnumType(out PrimitiveTypeCode underlyingType);
    }
}
