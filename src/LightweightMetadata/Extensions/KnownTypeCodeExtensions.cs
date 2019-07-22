// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection.Metadata;
using LightweightMetadata.TypeWrappers;

namespace LightweightMetadata.Extensions
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
        /// <param name="compilation">The compilation of all known modules.</param>
        /// <returns>The type wrapper if its available, null otherwise.</returns>
        internal static IHandleTypeNamedWrapper ToTypeWrapper(this KnownTypeCode knownTypeCode, ICompilation compilation)
        {
            if (compilation == null)
            {
                throw new ArgumentNullException(nameof(compilation));
            }

            var name = ToTypeName(knownTypeCode);
            return compilation.GetTypeByName(name);
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
        internal static KnownTypeCode ToKnownTypeCode(this ITypeNamedWrapper typeDefinition)
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

        internal static string GetRealTypeName(this string typeDefinitionName) => GetRealTypeName(ToKnownTypeCode(typeDefinitionName), typeDefinitionName);

        internal static string GetRealTypeName(this KnownTypeCode knownTypeCode, string defaultTypeName = null)
        {
            switch (knownTypeCode)
            {
                case KnownTypeCode.Boolean:
                    return "bool";
                case KnownTypeCode.Byte:
                    return "byte";
                case KnownTypeCode.Char:
                    return "char";
                case KnownTypeCode.Decimal:
                    return "decimal";
                case KnownTypeCode.Double:
                    return "double";
                case KnownTypeCode.Int16:
                    return "short";
                case KnownTypeCode.Int32:
                    return "int";
                case KnownTypeCode.Int64:
                    return "long";
                case KnownTypeCode.SByte:
                    return "sbyte";
                case KnownTypeCode.Single:
                    return "single";
                case KnownTypeCode.String:
                    return "string";
                case KnownTypeCode.UInt16:
                    return "ushort";
                case KnownTypeCode.UInt32:
                    return "uint";
                case KnownTypeCode.UInt64:
                    return "ulong";
                case KnownTypeCode.Object:
                    return "object";
                case KnownTypeCode.Void:
                    return "void";
                default:
                    return defaultTypeName ?? knownTypeCode.ToTypeName();
            }
        }
    }
}
