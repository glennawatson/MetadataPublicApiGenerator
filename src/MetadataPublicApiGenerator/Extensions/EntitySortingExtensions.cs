// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;
using LightweightMetadata;

namespace MetadataPublicApiGenerator.Extensions
{
    internal static class EntitySortingExtensions
    {
        internal static IEnumerable<T> OrderByAndExclude<T>(this IEnumerable<T> entities, ISet<string> excludeMembersAttributes, ISet<string> excludeAttributes)
            where T : IHandleNameWrapper
        {
            return entities.Where(x => x.ShouldIncludeEntity(excludeMembersAttributes, excludeAttributes)).OrderBy(x => x, EntityTypeComparer.Default);
        }

        internal static bool ShouldIncludeEntity(this IHandleNameWrapper entity, ISet<string> excludeMembersAttributes, ISet<string> excludeAttributes)
        {
            if (entity is IHandleTypeNamedWrapper typeNameWrapper)
            {
                switch (typeNameWrapper.Accessibility)
                {
                    case EntityAccessibility.PrivateProtected:
                    case EntityAccessibility.Protected:
                    case EntityAccessibility.ProtectedInternal:
                    case EntityAccessibility.Public:
                        break;
                    default:
                        return false;
                }
            }

            if (entity is AttributeWrapper attributeWrapper && excludeAttributes.Contains(attributeWrapper.ReflectionFullName))
            {
                return false;
            }

            if (entity is IHasAttributes hasAttributes)
            {
                var attributes = hasAttributes.Attributes;

                if (attributes == null)
                {
                    return true;
                }

                return !attributes.Any(attr => excludeMembersAttributes.Contains(attr.ReflectionFullName));
            }

            return true;
        }
    }
}
