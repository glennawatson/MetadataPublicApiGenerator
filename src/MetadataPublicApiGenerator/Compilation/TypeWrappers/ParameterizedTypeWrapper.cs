// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using Lazy;

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
    internal class ParameterizedTypeWrapper : ITypeNamedWrapper
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterizedTypeWrapper"/> class.
        /// </summary>
        /// <param name="module">The module that owns the parameterized type.</param>
        /// <param name="genericType">The type that is generic.</param>
        /// <param name="typeArguments">The type arguments provided to the class.</param>
        public ParameterizedTypeWrapper(CompilationModule module, ITypeNamedWrapper genericType, IEnumerable<ITypeNamedWrapper> typeArguments)
        {
            GenericType = genericType;
            TypeArguments = typeArguments.ToImmutableArray();
            Module = module;
        }

        /// <summary>
        /// Gets the main type.
        /// </summary>
        public ITypeNamedWrapper GenericType { get; }

        /// <summary>
        /// Gets the type arguments.
        /// </summary>
        public ImmutableArray<ITypeNamedWrapper> TypeArguments { get; }

        [Lazy]
        public string FullName => Namespace + "." + Name;

        public string Namespace => GenericType.Namespace;

        /// <inheritdoc />
        public bool IsKnownType => false;

        /// <inheritdoc />
        public CompilationModule Module { get; }

        [Lazy]
        public string Name
        {
            get
            {
                StringBuilder sb = new StringBuilder(GenericType.Name);

                if (TypeArguments.Length > 0)
                {
                    sb.Append("<")
                        .Append(string.Join(", ", TypeArguments.Select(x => x.FullName)))
                        .Append(">");
                }

                return sb.ToString();
            }
        }
    }
}
