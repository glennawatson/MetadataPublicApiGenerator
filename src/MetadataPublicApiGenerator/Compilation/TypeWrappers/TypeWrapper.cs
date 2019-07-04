// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Reflection.Metadata;
using System.Threading;

using MetadataPublicApiGenerator.Extensions;

namespace MetadataPublicApiGenerator.Compilation.TypeWrappers
{
    /// <summary>
    /// A wrapper around a type.
    /// </summary>
    internal class TypeWrapper : ITypeWrapper
    {
        private readonly Lazy<TypeDefinition> _typeDefinition;

        private readonly Lazy<string> _name;

        private readonly Lazy<string> _namespace;

        private readonly Lazy<string> _fullName;

        private readonly Lazy<bool> _isKnownType;

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeWrapper"/> class.
        /// </summary>
        /// <param name="module">The main compilation module.</param>
        /// <param name="typeDefinition">The type definition we are wrapping.</param>
        public TypeWrapper(CompilationModule module, TypeDefinitionHandle typeDefinition)
        {
            Module = module ?? throw new ArgumentNullException(nameof(module));
            TypeDefinitionHandle = typeDefinition;
            Handle = typeDefinition;

            _typeDefinition = new Lazy<TypeDefinition>(() => ((TypeDefinitionHandle)Handle).Resolve(Module), LazyThreadSafetyMode.PublicationOnly);
            _name = new Lazy<string>(() => TypeDefinition.GetName(Module), LazyThreadSafetyMode.PublicationOnly);
            _namespace = new Lazy<string>(() => TypeDefinition.Namespace.GetName(Module), LazyThreadSafetyMode.PublicationOnly);
            _fullName = new Lazy<string>(() => TypeDefinition.GetFullName(Module), LazyThreadSafetyMode.PublicationOnly);
            _isKnownType = new Lazy<bool>(() => TypeDefinition.IsKnownType(Module) != KnownTypeCode.None, LazyThreadSafetyMode.PublicationOnly);
        }

        /// <summary>
        /// Gets the type definition for the type.
        /// </summary>
        public TypeDefinition TypeDefinition => _typeDefinition.Value;

        /// <inheritdoc />
        public virtual string Name => _name.Value;

        /// <inheritdoc />
        public string Namespace => _namespace.Value;

        /// <inheritdoc />
        public string FullName => _fullName.Value;

        /// <inheritdoc />
        public virtual bool IsKnownType => _isKnownType.Value;

        /// <inheritdoc />
        public Handle Handle { get; }

        /// <inheritdoc />
        public CompilationModule Module { get; }

        /// <summary>
        /// Gets the type definition handle.
        /// </summary>
        public TypeDefinitionHandle TypeDefinitionHandle { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return FullName;
        }
    }
}
