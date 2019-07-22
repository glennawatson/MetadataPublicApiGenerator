// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Concurrent;
using System.Reflection.Metadata;
using System.Text;
using LightweightMetadata.TypeWrappers;

namespace LightweightMetadata.Extensions
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
    }
}