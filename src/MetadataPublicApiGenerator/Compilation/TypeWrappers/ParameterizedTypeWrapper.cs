// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

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
        private readonly Lazy<string> _name;

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterizedTypeWrapper"/> class.
        /// </summary>
        /// <param name="module">The module that owns the parameterized type.</param>
        /// <param name="genericType">The type that is generic.</param>
        /// <param name="typeArguments">The type arguments provided to the class.</param>
        public ParameterizedTypeWrapper(CompilationModule module, ITypeNamedWrapper genericType, IReadOnlyList<ITypeNamedWrapper> typeArguments)
        {
            if (typeArguments == null)
            {
                throw new ArgumentNullException(nameof(typeArguments));
            }

            if (typeArguments.Any(x => x == null))
            {
                throw new ArgumentNullException(nameof(typeArguments));
            }

            GenericType = genericType ?? throw new ArgumentNullException(nameof(genericType));
            TypeArguments = typeArguments.ToImmutableArray();
            Module = module ?? throw new ArgumentNullException(nameof(module));

            _name = new Lazy<string>(
                () =>
                    {
                        var sb = new StringBuilder(GenericType.Name);

                        if (TypeArguments.Length > 0)
                        {
                            sb.Append("<")
                                .Append(string.Join(", ", TypeArguments.Select(x => x.FullName)))
                                .Append(">");
                        }

                        return sb.ToString();
                    });

            IsPublic = genericType.IsPublic;
            IsAbstract = genericType.IsAbstract;
        }

        /// <summary>
        /// Gets the main type.
        /// </summary>
        public ITypeNamedWrapper GenericType { get; }

        /// <summary>
        /// Gets the type arguments.
        /// </summary>
        public ImmutableArray<ITypeNamedWrapper> TypeArguments { get; }

        public string FullName => Namespace + "." + Name;

        public string Namespace => GenericType.Namespace;

        /// <inheritdoc />
        public bool IsPublic { get; }

        /// <inheritdoc />
        public bool IsAbstract { get; }

        /// <inheritdoc />
        public CompilationModule Module { get; }

        public string Name => _name.Value;
    }
}
