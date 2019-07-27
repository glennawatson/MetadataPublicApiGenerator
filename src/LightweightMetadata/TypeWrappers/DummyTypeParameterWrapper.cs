// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reflection.Metadata;

namespace LightweightMetadata
{
    /// <summary>
    /// If there is no type parameter at the index, this is a dummy value for the placement.
    /// This class exists because sometimes the TypeProvider will ask for a entry beyond the bounds.
    /// </summary>
    public class DummyTypeParameterWrapper : IHandleTypeNamedWrapper
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DummyTypeParameterWrapper"/> class.
        /// </summary>
        /// <param name="index">The index of the parameter.</param>
        /// <param name="type">The type name of the parameter.</param>
        /// <param name="module">The module where the calling was requested through.</param>
        public DummyTypeParameterWrapper(int index, string type, AssemblyMetadata module)
        {
            Name = index + " " + type;
            FullName = Name;
            TypeNamespace = string.Empty;
            Accessibility = EntityAccessibility.None;
            IsAbstract = false;
            CompilationModule = module;
            Handle = default;
        }

        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public string FullName { get; }

        /// <inheritdoc />
        public string ReflectionFullName => FullName;

        /// <inheritdoc />
        public string TypeNamespace { get; }

        /// <inheritdoc />
        public EntityAccessibility Accessibility { get; }

        /// <inheritdoc />
        public bool IsAbstract { get; }

        /// <inheritdoc />
        public Handle Handle { get; }

        /// <inheritdoc />
        public AssemblyMetadata CompilationModule { get; }

        /// <inheritdoc />
        public KnownTypeCode KnownType => KnownTypeCode.None;

        /// <inheritdoc />
        public override string ToString()
        {
            return FullName;
        }
    }
}
