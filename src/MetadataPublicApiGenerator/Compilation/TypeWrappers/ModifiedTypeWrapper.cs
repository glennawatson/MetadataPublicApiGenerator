// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Reflection.Metadata;

namespace MetadataPublicApiGenerator.Compilation.TypeWrappers
{
    internal class ModifiedTypeWrapper : IHandleTypeNamedWrapper
    {
        public ModifiedTypeWrapper(CompilationModule module, IHandleTypeNamedWrapper modifier, IHandleTypeNamedWrapper unmodifiedType, bool isRequired)
        {
            Module = module ?? throw new ArgumentNullException(nameof(module));
            Modifier = modifier ?? throw new ArgumentNullException(nameof(modifier));
            Unmodified = unmodifiedType ?? throw new ArgumentNullException(nameof(unmodifiedType));
            IsRequired = isRequired;
        }

        public IHandleTypeNamedWrapper Modifier { get; }

        public IHandleTypeNamedWrapper Unmodified { get; }

        public bool IsRequired { get; }

        public string Name => Unmodified.Name + (IsRequired ? " modreq" : " modopt") + $"({Modifier.Name})";

        public string FullName => Namespace + "." + Name;

        public string Namespace => Unmodified.Namespace;

        /// <inheritdoc />
        public bool IsPublic => Unmodified.IsPublic;

        /// <inheritdoc />
        public bool IsAbstract => Unmodified.IsAbstract;

        /// <inheritdoc />
        public Handle Handle => Unmodified.Handle;

        public CompilationModule Module { get; }
    }
}
