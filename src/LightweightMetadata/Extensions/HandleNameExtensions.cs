// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection.Metadata;

namespace LightweightMetadata
{
    internal static class HandleNameExtensions
    {
        /// <summary>
        /// Gets a set of CSharp keywords.
        /// </summary>
        private static readonly ISet<string> CSharpKeywords = new HashSet<string>(StringComparer.InvariantCulture)
        {
             "abstract",
             "as",
             "base",
             "bool",
             "break",
             "byte",
             "case",
             "catch",
             "char",
             "checked",
             "class",
             "const",
             "continue",
             "decimal",
             "default",
             "delegate",
             "do",
             "double",
             "else",
             "enum",
             "event",
             "explicit",
             "extern",
             "false",
             "finally",
             "fixed",
             "float",
             "for",
             "foreach",
             "goto",
             "if",
             "implicit",
             "in",
             "int",
             "interface",
             "internal",
             "is",
             "lock",
             "long",
             "namespace",
             "new",
             "null",
             "object",
             "operator",
             "out",
             "override",
             "params",
             "private",
             "protected",
             "public",
             "readonly",
             "ref",
             "return",
             "sbyte",
             "sealed",
             "short",
             "sizeof",
             "stackalloc",
             "static",
             "string",
             "struct",
             "switch",
             "this",
             "throw",
             "true",
             "try",
             "typeof",
             "uint",
             "ulong",
             "unchecked",
             "unsafe",
             "ushort",
             "using",
             "using",
             "static",
             "virtual void",
             "volatile",
             "while"
        };

        private static readonly ConcurrentDictionary<AssemblyMetadata, ConcurrentDictionary<StringHandle, string>> _stringHandleNames = new ConcurrentDictionary<AssemblyMetadata, ConcurrentDictionary<StringHandle, string>>();

        public static string GetName(this StringHandle handle, AssemblyMetadata assemblyMetadata)
        {
            var map = _stringHandleNames.GetOrAdd(assemblyMetadata, _ => new ConcurrentDictionary<StringHandle, string>());

            return map.GetOrAdd(handle, stringHandle => assemblyMetadata.MetadataReader.GetString(stringHandle));
        }

        /// <summary>
        /// Removes the ` with type parameter count from the reflection name.
        /// </summary>
        /// <param name="reflectionName">The reflection name.</param>
        /// <param name="typeParameterCount">Output variable which optionally has the number of type parameters.</param>
        /// <returns>The name of the type without the type parameter count.</returns>
        /// <remarks>Do not use this method with the full name of inner classes.</remarks>
        public static string SplitTypeParameterCountFromReflectionName(this string reflectionName, out int typeParameterCount)
        {
            int pos = reflectionName.LastIndexOf('`');
            if (pos < 0)
            {
                typeParameterCount = 0;
                return reflectionName;
            }

            string typeCount = reflectionName.Substring(pos + 1);
            if (int.TryParse(typeCount, out typeParameterCount))
            {
                return reflectionName.Substring(0, pos);
            }

            return reflectionName;
        }

        internal static string GetKeywordSafeName(this string name)
        {
            return CSharpKeywords.Contains(name) ? '@' + name : name;
        }

        internal static string GetRealTypeName(this string typeDefinitionName) => GetRealTypeName(typeDefinitionName.ToKnownTypeCode(), typeDefinitionName);

        private static string GetRealTypeName(this KnownTypeCode knownTypeCode, string defaultTypeName)
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
                    return defaultTypeName;
            }
        }
    }
}