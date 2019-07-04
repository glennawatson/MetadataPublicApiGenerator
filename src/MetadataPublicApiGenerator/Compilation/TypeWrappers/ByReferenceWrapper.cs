// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;

namespace MetadataPublicApiGenerator.Compilation.TypeWrappers
{
    internal class ByReferenceWrapper : ITypeNamedWrapper
    {
        public ByReferenceWrapper(CompilationModule module, ITypeNamedWrapper typeDefinition)
        {
            TypeDefinition = typeDefinition ?? throw new ArgumentNullException(nameof(typeDefinition));
            Module = module ?? throw new ArgumentNullException(nameof(module));
        }

        public ITypeNamedWrapper TypeDefinition { get; }

        public bool IsKnownType => TypeDefinition.IsKnownType;

        /// <inheritdoc />
        public bool IsEnumType => TypeDefinition.IsEnumType;

        public CompilationModule Module { get; }

        /// <inheritdoc />
        public virtual string Name => TypeDefinition.Name + "&";

        /// <inheritdoc />
        public string Namespace => TypeDefinition.Namespace;

        /// <inheritdoc />
        public string FullName => Namespace + "." + Name;
    }
}
