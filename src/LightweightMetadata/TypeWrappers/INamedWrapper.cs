// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace LightweightMetadata
{
    /// <summary>
    /// A element that is named.
    /// </summary>
    public interface INamedWrapper
    {
        /// <summary>
        /// Gets the short name of the class the return type is pointing to.
        /// </summary>
        /// <returns>
        /// "Int32[]" for int[]<br/>
        /// "List" for List&lt;string&gt;
        /// "SpecialFolder" for Environment.SpecialFolder.
        /// </returns>
        string Name { get; }

        /// <summary>
        /// Gets the fully qualified name of the class the return type is pointing to.
        /// </summary>
        /// <returns>
        /// "System.Int32[]" for int[]<br/>
        /// "System.Collections.Generic.List" for List&lt;string&gt;
        /// "System.Environment.SpecialFolder" for Environment.SpecialFolder.
        /// </returns>
        string FullName { get; }
    }
}
