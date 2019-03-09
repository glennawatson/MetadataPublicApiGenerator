﻿// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Text;
using Lazy;

namespace MetadataPublicApiGenerator.Compilation.TypeWrappers
{
    internal class PinnedTypeWrapper : ITypeWrapper
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PinnedTypeWrapper"/> class.
        /// </summary>
        /// <param name="module">The module where the type belongs.</param>
        /// <param name="typeDefinition">The type definition.</param>
        public PinnedTypeWrapper(CompilationModule module, ITypeWrapper typeDefinition)
        {
            TypeDefinition = typeDefinition;
            Module = module;
        }

        /// <summary>
        /// Gets the type definition that is wrapped by the pointer.
        /// </summary>
        public ITypeWrapper TypeDefinition { get; }

        /// <inheritdoc />
        public bool IsKnownType => TypeDefinition.IsKnownType;

        /// <inheritdoc />
        public CompilationModule Module { get; }

        /// <inheritdoc />
        public Handle Handle => TypeDefinition.Handle;

        /// <inheritdoc />
        [Lazy]
        public virtual string Name => TypeDefinition.Name + " pinned";

        /// <inheritdoc />
        [Lazy]
        public string Namespace => TypeDefinition.Namespace;

        /// <inheritdoc />
        [Lazy]
        public string FullName => Namespace + "." + Name;
    }
}