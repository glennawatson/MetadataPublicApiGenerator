// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;

namespace MetadataPublicApiGenerator.Compilation.TypeWrappers
{
    /// <summary>
    /// A wrapper of a type.
    /// </summary>
    internal interface ITypeWrapper : IHandleWrapper, ITypeNamedWrapper
    {
    }
}
