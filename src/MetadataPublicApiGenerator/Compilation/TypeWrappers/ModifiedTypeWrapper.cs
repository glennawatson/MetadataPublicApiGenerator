// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;

namespace MetadataPublicApiGenerator.Compilation.TypeWrappers
{
    internal class ModifiedTypeWrapper : ITypeNamedWrapper
    {
        public ModifiedTypeWrapper(CompilationModule module, ITypeNamedWrapper modifier, ITypeNamedWrapper unmodifiedType, bool isRequired)
        {
            Module = module ?? throw new ArgumentNullException(nameof(module));
            Modifier = modifier ?? throw new ArgumentNullException(nameof(modifier));
            Unmodified = unmodifiedType ?? throw new ArgumentNullException(nameof(unmodifiedType));
            IsRequired = isRequired;
        }

        public ITypeNamedWrapper Modifier { get; }

        public ITypeNamedWrapper Unmodified { get; }

        public bool IsRequired { get; }

        public string Name => Unmodified.Name + (IsRequired ? " modreq" : " modopt") + $"({Modifier.Name})";

        public string FullName => Namespace + "." + Name;

        public string Namespace => Unmodified.Namespace;

        public bool IsKnownType => false;

        /// <inheritdoc />
        public bool IsEnumType => Unmodified.IsEnumType;

        public CompilationModule Module { get; }
    }
}
