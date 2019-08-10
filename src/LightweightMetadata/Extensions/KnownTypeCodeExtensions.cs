// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Reflection.Metadata;

namespace LightweightMetadata
{
    /// <summary>
    /// Extension methods associated with the <see cref="KnownTypeCode"/>.
    /// </summary>
    internal static class KnownTypeCodeExtensions
    {
        private static readonly string[] _knownTypeReferences =
        {
            null, // None
            "System.Object",
            "System.DBNull",
            "System.Boolean",
            "System.Char",
            "System.SByte",
            "System.Byte",
            "System.Int16",
            "System.UInt16",
            "System.Int32",
            "System.UInt32",
            "System.Int64",
            "System.UInt64",
            "System.Single",
            "System.Double",
            "System.Decimal",
            "System.DateTime",
            null,
            "System.String",
            "System.Void",
            "System.Type",
            "System.Array",
            "System.Attribute",
            "System.ValueType",
            "System.Enum",
            "System.Delegate",
            "System.MulticastDelegate",
            "System.Exception",
            "System.IntPtr",
            "System.UIntPtr",
            "System.Collections.IEnumerable",
            "System.Collections.IEnumerator",
            "System.Collections.Generic.IEnumerable`1",
            "System.Collections.Generic.IEnumerator`1",
            "System.Collections.ICollection",
            "System.Collections.Generic.ICollection`1",
            "System.Collections.IList",
            "System.Collections.Generic.IList`1",

            "System.Collections.Generic.IReadOnlyCollection`1",
            "System.Collections.Generic.IReadOnlyList`1",
            "System.Threading.Tasks.Task",
            "System.Threading.Tasks.Task`1",
            "System.Nullable`1",
            "System.IDisposable",
            "System.Runtime.CompilerServices.INotifyCompletion",
            "System.Runtime.CompilerServices.ICriticalNotifyCompletion",

            "System.TypedReference",
            "System.IFormattable",
            "System.FormattableString",
            "System.Span`1",
            "System.ReadOnlySpan`1",
            "System.Memory`1",
        };

        private static readonly IDictionary<string, KnownTypeCode> _nameToTypeCodes;

        static KnownTypeCodeExtensions()
        {
            _nameToTypeCodes = new Dictionary<string, KnownTypeCode>(_knownTypeReferences.Length);
            for (int i = 0; i < _knownTypeReferences.Length; ++i)
            {
                var current = _knownTypeReferences[i];
                if (current == null)
                {
                    continue;
                }

                _nameToTypeCodes[current] = (KnownTypeCode)i;
            }
        }

        /// <summary>
        /// Converts a KnownTypeCode to a Type.
        /// </summary>
        /// <param name="knownTypeCode">The known type code to convert.</param>
        /// <param name="metadataRepository">The MetadataRepository of all known modules.</param>
        /// <returns>The type wrapper if its available, null otherwise.</returns>
        internal static IHandleTypeNamedWrapper ToTypeWrapper(this KnownTypeCode knownTypeCode, MetadataRepository metadataRepository)
        {
            if (metadataRepository == null)
            {
                throw new ArgumentNullException(nameof(metadataRepository));
            }

            var name = ToTypeName(knownTypeCode);
            return metadataRepository.GetTypeByName(name);
        }

        /// <summary>
        /// Converts a KnownTypeCode into a human readable type name.
        /// </summary>
        /// <param name="knownType">The known type code to convert.</param>
        /// <returns>A string representation of the type.</returns>
        internal static string ToTypeName(this KnownTypeCode knownType)
        {
            return _knownTypeReferences[(int)knownType];
        }

        /// <summary>
        /// Determines if the specified type is a KnownTypeCode.
        /// </summary>
        /// <param name="typeDefinition">The type to check.</param>
        /// <returns>The known type code, None if it's not a known type code.</returns>
        internal static KnownTypeCode ToKnownTypeCode(this IHandleNameWrapper typeDefinition)
        {
            if (typeDefinition == null)
            {
                return KnownTypeCode.None;
            }

            string name = typeDefinition.FullName;

            if (_nameToTypeCodes.TryGetValue(name, out var knownTypeCode))
            {
                return knownTypeCode;
            }

            return KnownTypeCode.None;
        }

        /// <summary>
        /// Determines if the specified type is a KnownTypeCode.
        /// </summary>
        /// <param name="typeDefinitionName">The type to check.</param>
        /// <returns>The known type code, None if it's not a known type code.</returns>
        internal static KnownTypeCode ToKnownTypeCode(this string typeDefinitionName)
        {
            if (string.IsNullOrWhiteSpace(typeDefinitionName))
            {
                return KnownTypeCode.None;
            }

            if (_nameToTypeCodes.TryGetValue(typeDefinitionName, out var knownTypeCode))
            {
                return knownTypeCode;
            }

            return KnownTypeCode.None;
        }

        /// <summary>
        /// Converts a PrimitiveTypeCode to a KnownTypeCode.
        /// </summary>
        /// <param name="typeCode">The primitive type code to convert.</param>
        /// <returns>The known type code.</returns>
        internal static KnownTypeCode ToKnownTypeCode(this PrimitiveTypeCode typeCode)
        {
            switch (typeCode)
            {
                case PrimitiveTypeCode.Boolean:
                    return KnownTypeCode.Boolean;
                case PrimitiveTypeCode.Byte:
                    return KnownTypeCode.Byte;
                case PrimitiveTypeCode.SByte:
                    return KnownTypeCode.SByte;
                case PrimitiveTypeCode.Char:
                    return KnownTypeCode.Char;
                case PrimitiveTypeCode.Int16:
                    return KnownTypeCode.Int16;
                case PrimitiveTypeCode.UInt16:
                    return KnownTypeCode.UInt16;
                case PrimitiveTypeCode.Int32:
                    return KnownTypeCode.Int32;
                case PrimitiveTypeCode.UInt32:
                    return KnownTypeCode.UInt32;
                case PrimitiveTypeCode.Int64:
                    return KnownTypeCode.Int64;
                case PrimitiveTypeCode.UInt64:
                    return KnownTypeCode.UInt64;
                case PrimitiveTypeCode.Single:
                    return KnownTypeCode.Single;
                case PrimitiveTypeCode.Double:
                    return KnownTypeCode.Double;
                case PrimitiveTypeCode.IntPtr:
                    return KnownTypeCode.IntPtr;
                case PrimitiveTypeCode.UIntPtr:
                    return KnownTypeCode.UIntPtr;
                case PrimitiveTypeCode.Object:
                    return KnownTypeCode.Object;
                case PrimitiveTypeCode.String:
                    return KnownTypeCode.String;
                case PrimitiveTypeCode.TypedReference:
                    return KnownTypeCode.TypedReference;
                case PrimitiveTypeCode.Void:
                    return KnownTypeCode.Void;
                default:
                    return KnownTypeCode.None;
            }
        }
    }
}
