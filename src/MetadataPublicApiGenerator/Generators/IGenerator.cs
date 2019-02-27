// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace MetadataPublicApiGenerator.Generators
{
    /// <summary>
    /// A generator of metadata syntax.
    /// </summary>
    internal interface IGenerator
    {
        /// <summary>
        /// Gets a set of attributes to exclude from being generated.
        /// </summary>
        ISet<string> ExcludeAttributes { get; }

        /// <summary>
        /// Gets a set of attributes for any types we should avoid that are decorated with these attribute types.
        /// </summary>
        ISet<string> ExcludeMembersAttributes { get; }
    }
}
