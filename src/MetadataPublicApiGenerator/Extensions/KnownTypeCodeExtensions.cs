// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using MetadataPublicApiGenerator.Compilation;
using Microsoft.CodeAnalysis;

namespace MetadataPublicApiGenerator.Extensions
{
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

        public static KnownTypeCode IsKnownType(this TypeDefinition typeDefinition, CompilationModule compilation)
        {
            string name = typeDefinition.GetFullName(compilation);
            var index = Array.IndexOf(_knownTypeReferences, name);
            if (index < 0)
            {
                return KnownTypeCode.None;
            }

            return (KnownTypeCode)index;
        }

        public static (CompilationModule module, TypeDefinitionHandle typeDefinition) ToTypeDefinitionHandle(this KnownTypeCode knownType, ICompilation compilation)
        {
            var name = _knownTypeReferences[(int)knownType];
            return compilation.GetTypeDefinitionByName(name).FirstOrDefault();
        }

        public static string ToTypeName(this KnownTypeCode knownType)
        {
            return _knownTypeReferences[(int)knownType];
        }

        public static KnownTypeCode IsKnownType(this TypeDefinitionHandle typeDefinition, CompilationModule compilation)
        {
            string name = typeDefinition.GetName(compilation);
            var index = Array.IndexOf(_knownTypeReferences, name);
            if (index < 0)
            {
                return KnownTypeCode.None;
            }

            return (KnownTypeCode)index;
        }

        public static KnownTypeCode IsKnownType(this Handle typeDefinition, CompilationModule compilation)
        {
            string name = typeDefinition.GetName(compilation);
            var index = Array.IndexOf(_knownTypeReferences, name);
            if (index < 0)
            {
                return KnownTypeCode.None;
            }

            return (KnownTypeCode)index;
        }

        public static KnownTypeCode ToKnownTypeCode(this PrimitiveTypeCode typeCode)
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
