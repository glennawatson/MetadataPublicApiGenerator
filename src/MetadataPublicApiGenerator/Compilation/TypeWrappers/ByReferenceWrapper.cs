// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Reflection.Metadata;

namespace MetadataPublicApiGenerator.Compilation.TypeWrappers
{
    internal class ByReferenceWrapper : IHandleTypeNamedWrapper
    {
        public ByReferenceWrapper(IHandleTypeNamedWrapper typeDefinition)
        {
            TypeDefinition = typeDefinition ?? throw new ArgumentNullException(nameof(typeDefinition));
        }

        public IHandleTypeNamedWrapper TypeDefinition { get; }

        /// <inheritdoc />
        public bool IsAbstract => TypeDefinition.IsAbstract;

        /// <inheritdoc />
        public virtual string Name => TypeDefinition.Name + "&";

        /// <inheritdoc />
        public string Namespace => TypeDefinition.Namespace;

        /// <inheritdoc />
        public bool IsPublic => TypeDefinition.IsPublic;

        /// <inheritdoc />
        public string FullName => Namespace + "." + Name;

        /// <inheritdoc />
        public Handle Handle => TypeDefinition.Handle;

        /// <inheritdoc />
        public CompilationModule Module => TypeDefinition.Module;
    }
}
