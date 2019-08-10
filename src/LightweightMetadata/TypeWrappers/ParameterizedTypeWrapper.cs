// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace LightweightMetadata
{
    /// <summary>
    /// ParameterizedTypeWrapper represents an instance of a generic type.
    /// Example: List&lt;string&gt;.
    /// </summary>
    /// <remarks>
    /// When getting the members, this type modifies the lists so that
    /// type parameters in the signatures of the members are replaced with
    /// the type arguments.
    /// </remarks>
    public class ParameterizedTypeWrapper : AbstractEnclosedTypeWrapper, IHasTypeArguments
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterizedTypeWrapper"/> class.
        /// </summary>
        /// <param name="genericType">The type that is generic.</param>
        /// <param name="typeArguments">The type arguments provided to the class.</param>
        public ParameterizedTypeWrapper(IHandleTypeNamedWrapper genericType, IReadOnlyList<IHandleTypeNamedWrapper> typeArguments)
            : base(genericType)
        {
            if (typeArguments == null)
            {
                throw new ArgumentNullException(nameof(typeArguments));
            }

            if (typeArguments.Any(x => x == null))
            {
                throw new ArgumentNullException(nameof(typeArguments));
            }

            TypeArguments = typeArguments.ToList();
        }

        /// <summary>
        /// Gets the type arguments.
        /// </summary>
        public override IReadOnlyList<IHandleTypeNamedWrapper> TypeArguments { get; }
    }
}
