// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using System.Reflection.Metadata;
using MetadataPublicApiGenerator.Compilation;
using MetadataPublicApiGenerator.Compilation.TypeWrappers;

namespace MetadataPublicApiGenerator.Extensions
{
    internal static class HandleNameExtensions
    {
        private static readonly ConcurrentDictionary<CompilationModule, ConcurrentDictionary<StringHandle, string>> _stringHandleNames = new ConcurrentDictionary<CompilationModule, ConcurrentDictionary<StringHandle, string>>();

        public static string GetName(this UserStringHandle handle, CompilationModule compilation)
        {
            return compilation.MetadataReader.GetUserString(handle);
        }

        public static string GetName(this StringHandle handle, CompilationModule compilation)
        {
            var map = _stringHandleNames.GetOrAdd(compilation, _ => new ConcurrentDictionary<StringHandle, string>());

            return map.GetOrAdd(handle, stringHandle => compilation.MetadataReader.GetString(stringHandle));
        }

        public static string GetRealTypeName(this TypeWrapper type)
        {
            var typeCode = type.IsKnownType();

            if (typeCode == KnownTypeCode.Array)
            {
                ////var arrayType = type.
                ////var elementType = arrayType.ElementType;

                ////return elementType.GenerateFullGenericName(compilation) + "[]";

                return null;
            }

            return typeCode.GetRealTypeName() ?? type.FullName;
        }

        public static string GetRealTypeName(this KnownTypeCode typeCode)
        {
            switch (typeCode)
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
                default:
                    return null;
            }
        }
    }
}