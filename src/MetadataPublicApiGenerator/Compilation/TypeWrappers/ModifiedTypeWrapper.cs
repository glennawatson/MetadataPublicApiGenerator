// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Text;
using Lazy;

namespace MetadataPublicApiGenerator.Compilation.TypeWrappers
{
    internal class ModifiedTypeWrapper : IWrapper
    {
        public ModifiedTypeWrapper(CompilationModule module, ITypeNamedWrapper modifier, ITypeNamedWrapper unmodifiedType, bool isRequired)
        {
            Module = module;
            Modifier = modifier;
            Unmodified = unmodifiedType;
            IsRequired = isRequired;
        }

        public ITypeNamedWrapper Modifier { get; }

        public ITypeNamedWrapper Unmodified { get; }

        public bool IsRequired { get; }

        [Lazy]
        public string Name => Unmodified.Name + (IsRequired ? " modreq" : " modopt") + $"({Modifier.Name})";

        public bool IsKnownType => false;

        public CompilationModule Module { get; }
    }
}
