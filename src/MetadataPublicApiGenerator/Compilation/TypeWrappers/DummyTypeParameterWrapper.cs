// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace MetadataPublicApiGenerator.Compilation.TypeWrappers
{
    internal class DummyTypeParameterWrapper : ITypeNamedWrapper
    {
        public DummyTypeParameterWrapper(int index, string type, CompilationModule module)
        {
            Name = index + " " + type;
            FullName = Name;
            Namespace = string.Empty;
            IsKnownType = false;
            IsEnumType = false;
            Module = module;
        }

        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public string FullName { get; }

        /// <inheritdoc />
        public string Namespace { get; }

        /// <inheritdoc />
        public bool IsKnownType { get; }

        /// <inheritdoc />
        public bool IsEnumType { get; }

        /// <inheritdoc />
        public CompilationModule Module { get; }
    }
}
