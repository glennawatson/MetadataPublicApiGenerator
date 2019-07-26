// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading;
using LightweightMetadata.Extensions;

namespace LightweightMetadata.TypeWrappers
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
    public class ParameterizedTypeWrapper : IHandleTypeNamedWrapper
    {
        private readonly Lazy<string> _name;
        private readonly Lazy<string> _reflectionName;

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterizedTypeWrapper"/> class.
        /// </summary>
        /// <param name="genericType">The type that is generic.</param>
        /// <param name="typeArguments">The type arguments provided to the class.</param>
        public ParameterizedTypeWrapper(IHandleTypeNamedWrapper genericType, IReadOnlyList<IHandleTypeNamedWrapper> typeArguments)
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
            TypeArguments = typeArguments.ToList();
            CompilationModule = genericType.CompilationModule;

            _name = new Lazy<string>(() => GenericType.Name, LazyThreadSafetyMode.PublicationOnly);
            _reflectionName = new Lazy<string>(GetReflectionName, LazyThreadSafetyMode.PublicationOnly);

            Accessibility = genericType.Accessibility;
            IsAbstract = genericType.IsAbstract;
        }

        /// <summary>
        /// Gets the main type.
        /// </summary>
        public IHandleTypeNamedWrapper GenericType { get; }

        /// <summary>
        /// Gets the type arguments.
        /// </summary>
        public IReadOnlyList<IHandleTypeNamedWrapper> TypeArguments { get; }

        /// <inheritdoc />
        public string FullName => GenericType.FullName;

        /// <inheritdoc />
        public string ReflectionFullName => _reflectionName.Value;

        /// <inheritdoc />
        public string TypeNamespace => GenericType.TypeNamespace;

        /// <inheritdoc />
        public EntityAccessibility Accessibility { get; }

        /// <inheritdoc />
        public bool IsAbstract { get; }

        /// <inheritdoc />
        public KnownTypeCode KnownType => KnownTypeCode.None;

        /// <inheritdoc />
        public CompilationModule CompilationModule { get; }

        /// <inheritdoc />
        public string Name => _name.Value;

        /// <inheritdoc />
        public Handle Handle => GenericType.Handle;

        private string GetReflectionName()
        {
            string strippedName = GenericType.ReflectionFullName;

            var sb = new StringBuilder(strippedName);

            if (TypeArguments.Count > 0)
            {
                sb.Append("<")
                    .Append(string.Join(", ", TypeArguments.Select(x => x.ReflectionFullName)))
                    .Append(">");
            }

            return sb.ToString();
        }
    }
}
