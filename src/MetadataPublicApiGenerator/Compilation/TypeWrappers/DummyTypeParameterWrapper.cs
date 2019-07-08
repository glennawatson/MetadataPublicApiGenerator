// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reflection.Metadata;

namespace MetadataPublicApiGenerator.Compilation.TypeWrappers
{
    internal class DummyTypeParameterWrapper : IHandleTypeNamedWrapper
    {
        public DummyTypeParameterWrapper(int index, string type, CompilationModule module)
        {
            Name = index + " " + type;
            FullName = Name;
            Namespace = string.Empty;
            IsPublic = false;
            IsAbstract = false;
            Module = module;
            Handle = default;
        }

        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public string FullName { get; }

        /// <inheritdoc />
        public string Namespace { get; }

        /// <inheritdoc />
        public bool IsPublic { get; }

        /// <inheritdoc />
        public bool IsAbstract { get; }

        /// <inheritdoc />
        public Handle Handle { get; }

        /// <inheritdoc />
        public CompilationModule Module { get; }
    }
}
