// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using LightweightMetadata;
using LightweightMetadata.TypeWrappers;

namespace MetadataPublicApiGenerator.Extensions
{
    internal static class EntitySortingExtensions
    {
        internal static Dictionary<HandleKind, int> SymbolKindPreferredOrderWeights { get; } = new Dictionary<HandleKind, int>
        {
            [HandleKind.ModuleDefinition] = 0,
            [HandleKind.TypeReference] = 2,
            [HandleKind.TypeDefinition] = 3,
            [HandleKind.FieldDefinition] = 7,
            [HandleKind.MethodDefinition] = 8,
            [HandleKind.Parameter] = 9,
            [HandleKind.InterfaceImplementation] = 10,
            [HandleKind.MemberReference] = 11,
            [HandleKind.Constant] = 12,
            [HandleKind.CustomAttribute] = 13,
            [HandleKind.DeclarativeSecurityAttribute] = 14,
            [HandleKind.StandaloneSignature] = 15,
            [HandleKind.EventDefinition] = 5,
            [HandleKind.PropertyDefinition] = 6,
            [HandleKind.MethodImplementation] = 16,
            [HandleKind.ModuleReference] = 17,
            [HandleKind.TypeSpecification] = 18,
            [HandleKind.AssemblyDefinition] = 1,
            [HandleKind.AssemblyReference] = 19,
            [HandleKind.AssemblyFile] = 20,
            [HandleKind.ExportedType] = 21,
            [HandleKind.ManifestResource] = 22,
            [HandleKind.GenericParameter] = 23,
            [HandleKind.MethodSpecification] = 24,
            [HandleKind.GenericParameterConstraint] = 25,
            [HandleKind.Document] = 26,
            [HandleKind.MethodDebugInformation] = 27,
            [HandleKind.LocalScope] = 28,
            [HandleKind.LocalVariable] = 29,
            [HandleKind.LocalConstant] = 30,
            [HandleKind.ImportScope] = 31,
            [HandleKind.CustomDebugInformation] = 32,
            [HandleKind.UserString] = 33,
            [HandleKind.Blob] = 34,
            [HandleKind.Guid] = 35,
            [HandleKind.String] = 36,
            [HandleKind.NamespaceDefinition] = 4,
        };

        internal static bool ShouldIncludeEntity(this IHandleNameWrapper entity, ISet<string> excludeMembersAttributes, ISet<string> excludeAttributes)
        {
            if (entity is IHandleTypeNamedWrapper typeNameWrapper && typeNameWrapper.Accessibility != EntityAccessibility.Public)
            {
                return false;
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
