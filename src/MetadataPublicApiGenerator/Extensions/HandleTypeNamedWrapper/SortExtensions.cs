// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LightweightMetadata.TypeWrappers;

namespace MetadataPublicApiGenerator.Extensions.HandleTypeNamedWrapper
{
    internal static class SortExtensions
    {
        internal static IEnumerable<T> OrderByAndExclude<T>(this IEnumerable<T> entities, ISet<string> excludeMembersAttributes, ISet<string> excludeAttributes)
            where T : IHandleTypeNamedWrapper
        {
            return entities.Where(x => x.ShouldIncludeEntity(excludeMembersAttributes, excludeAttributes)).OrderBy(x => EntitySortingExtensions.SymbolKindPreferredOrderWeights[x.Handle.Kind]).ThenBy(x => x.FullName);
        }
    }
}
