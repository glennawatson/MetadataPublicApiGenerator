// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Text;
using Lazy;
using MetadataPublicApiGenerator.Extensions;

namespace MetadataPublicApiGenerator.Compilation.TypeWrappers
{
    internal class ByReferenceWrapper : ITypeNamedWrapper
    {
        public ByReferenceWrapper(CompilationModule module, ITypeWrapper typeDefinition)
        {
            TypeDefinition = typeDefinition;
            Module = module;
            Handle = typeDefinition.Handle;
        }

        public ITypeWrapper TypeDefinition { get; }

        public bool IsKnownType => TypeDefinition.IsKnownType;

        public Handle Handle { get; }

        public CompilationModule Module { get; }

        /// <inheritdoc />
        [Lazy]
        public virtual string Name => TypeDefinition.Name + "&";

        /// <inheritdoc />
        [Lazy]
        public string Namespace => TypeDefinition.Namespace;

        /// <inheritdoc />
        [Lazy]
        public string FullName => Namespace + "." + Name;
    }
}
