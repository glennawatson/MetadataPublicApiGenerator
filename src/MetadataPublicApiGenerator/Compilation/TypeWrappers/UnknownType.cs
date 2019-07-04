// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Reflection.Metadata;
using System.Threading;

using MetadataPublicApiGenerator.Extensions;

namespace MetadataPublicApiGenerator.Compilation.TypeWrappers
{
    internal class UnknownType : ITypeWrapper
    {
        private readonly Lazy<string> _name;

        private readonly Lazy<string> _namespace;

        private readonly Lazy<string> _fullName;

        public UnknownType(CompilationModule module, EntityHandle entityHandle)
        {
            Handle = entityHandle;
            Module = module;

            _name = new Lazy<string>(() => entityHandle.GetName(Module), LazyThreadSafetyMode.PublicationOnly);
            _namespace = new Lazy<string>(() => entityHandle.GetNamespace(Module), LazyThreadSafetyMode.PublicationOnly);
            _fullName = new Lazy<string>(() => entityHandle.GetFullName(Module), LazyThreadSafetyMode.PublicationOnly);
        }

        /// <inheritdoc />
        public Handle Handle { get; }

        /// <inheritdoc />
        public string Name => _name.Value;

        /// <inheritdoc />
        public string FullName => _fullName.Value;

        /// <inheritdoc />
        public string Namespace => _namespace.Value;

        /// <inheritdoc />
        public bool IsKnownType => false;

        /// <inheritdoc />
        public bool IsEnumType => false;

        /// <inheritdoc />
        public CompilationModule Module { get; }
    }
}
