// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace LightweightMetadata
{
    /// <summary>
    /// Contains a wrapper which has a namespace and a name.
    /// </summary>
    public interface INamespaceNameWrapper : INamedWrapper
    {
        /// <summary>
        /// Gets the name of the namespace.
        /// </summary>
        string NamespaceName { get; }
    }
}
