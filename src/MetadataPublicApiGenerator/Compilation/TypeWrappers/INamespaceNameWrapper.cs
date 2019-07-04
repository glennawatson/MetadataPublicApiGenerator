// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;

namespace MetadataPublicApiGenerator.Compilation.TypeWrappers
{
    /// <summary>
    /// Contains a wrapper which has a namespace and a name.
    /// </summary>
    internal interface INamespaceNameWrapper : INamedWrapper
    {
        /// <summary>
        /// Gets the name of the namespace.
        /// </summary>
        string Namespace { get; }

        /// <summary>
        /// Gets the full name.
        /// </summary>
        string FullName { get; }
    }
}
