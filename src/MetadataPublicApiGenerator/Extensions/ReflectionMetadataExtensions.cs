// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using MetadataPublicApiGenerator.Compilation;
using MetadataPublicApiGenerator.Compilation.TypeWrappers;
using Microsoft.CodeAnalysis;

namespace MetadataPublicApiGenerator.Extensions
{
    internal static class ReflectionMetadataExtensions
    {
        public static bool HasKnownAttribute(this IEnumerable<AttributeWrapper> attributes, KnownAttribute type)
        {
            foreach (var customAttribute in attributes)
            {
                if (customAttribute.IsKnownAttribute(type))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsKnownAttribute(this AttributeWrapper attr, KnownAttribute attrType)
        {
            return attr.KnownAttribute == attrType;
        }

        public static MethodWrapper GetDelegateInvokeMethod(this TypeWrapper type)
        {
            var handle = type.Methods.FirstOrDefault(x => x.Name == "Invoke");

            if (handle == null)
            {
                throw new Exception("Cannot find Invoke method for delegate.");
            }

            return handle;
        }

        public static object ReadConstant(this ConstantHandle constantHandle, CompilationModule module)
        {
            if (constantHandle.IsNil)
            {
                return null;
            }

            var constant = module.MetadataReader.GetConstant(constantHandle);
            var blobReader = module.MetadataReader.GetBlobReader(constant.Value);
            try
            {
                return blobReader.ReadConstant(constant.TypeCode);
            }
            catch (ArgumentOutOfRangeException)
            {
                throw new BadImageFormatException($"Constant with invalid type code: {constant.TypeCode}");
            }
        }
    }
}
