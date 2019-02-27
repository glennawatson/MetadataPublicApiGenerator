// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Reflection.Metadata;

namespace MetadataPublicApiGenerator.Compilation.TypeWrappers
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
    internal class ParameterizedTypeWrapper : IWrapper
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterizedTypeWrapper"/> class.
        /// </summary>
        /// <param name="module">The module that owns the parameterized type.</param>
        /// <param name="genericType">The type that is generic.</param>
        /// <param name="typeArguments">The type arguments provided to the class.</param>
        public ParameterizedTypeWrapper(CompilationModule module, IWrapper genericType, IEnumerable<IWrapper> typeArguments)
        {
            GenericType = genericType;
            TypeArguments = typeArguments;
            Module = module;
        }

        /// <summary>
        /// Gets the main type.
        /// </summary>
        public IWrapper GenericType { get; }

        /// <summary>
        /// Gets the type arguments.
        /// </summary>
        public IEnumerable<IWrapper> TypeArguments { get; }

        /// <inheritdoc />
        public bool IsKnownType => false;

        /// <inheritdoc />
        public CompilationModule Module { get; }
    }
}
