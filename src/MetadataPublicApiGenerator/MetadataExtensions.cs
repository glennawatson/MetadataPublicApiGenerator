// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using ICSharpCode.Decompiler.TypeSystem;

namespace MetadataPublicApiGenerator
{
    /// <summary>
    /// Extensions methods to help with the type metadata system.
    /// </summary>
    internal static class MetadataExtensions
    {
        private static readonly ConcurrentDictionary<ICompilation, ImmutableDictionary<string, ImmutableList<ITypeDefinition>>> _typeNameMapping = new ConcurrentDictionary<ICompilation, ImmutableDictionary<string, ImmutableList<ITypeDefinition>>>();

        /// <summary>
        /// Gets type definitions matching the full name and in the reference and main libraries.
        /// </summary>
        /// <param name="compilation">The compilation to scan.</param>
        /// <param name="name">The name of the item to get.</param>
        /// <returns>The name of the items.</returns>
        public static IReadOnlyCollection<ITypeDefinition> GetReferenceTypeDefinitionsWithFullName(this ICompilation compilation, string name)
        {
            var map = _typeNameMapping.GetOrAdd(compilation, comp => comp.ReferencedModules.Concat(compilation.Modules).SelectMany(x => x.TypeDefinitions).GroupBy(x => x.FullName).ToImmutableDictionary(x => x.Key, x => x.ToImmutableList()));

            return map.GetValueOrDefault(name);
        }
    }
}
