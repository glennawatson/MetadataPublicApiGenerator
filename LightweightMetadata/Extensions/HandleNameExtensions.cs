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
    }
}