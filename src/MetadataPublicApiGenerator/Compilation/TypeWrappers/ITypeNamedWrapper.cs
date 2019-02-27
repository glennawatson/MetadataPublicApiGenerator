// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;

namespace MetadataPublicApiGenerator.Compilation.TypeWrappers
{
    /// <summary>
    /// Represents a element that is named.
    /// </summary>
    internal interface ITypeNamedWrapper : INamedWrapper, IWrapper
    {
        /// <summary>
        /// Gets the fully qualified name of the class the return type is pointing to.
        /// </summary>
        /// <returns>
        /// "System.Int32[]" for int[]<br/>
        /// "System.Collections.Generic.List" for List&lt;string&gt;
        /// "System.Environment.SpecialFolder" for Environment.SpecialFolder.
        /// </returns>
        string FullName { get; }

        /// <summary>
        /// Gets the full name of the namespace containing this entity.
        /// </summary>
        string Namespace { get; }
    }
}
