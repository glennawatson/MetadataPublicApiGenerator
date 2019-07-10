// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using LightweightMetadata.TypeWrappers;

namespace LightweightMetadata.Extensions
{
    /// <summary>
    /// Extension methods that help with various reflection based operations.
    /// </summary>
    public static class ReflectionMetadataExtensions
    {
        /// <summary>
        /// If any of the attributes are known types.
        /// </summary>
        /// <param name="attributes">The attribute to check.</param>
        /// <param name="type">The known type to check.</param>
        /// <returns>If any of the attributes are of the known type.</returns>
        public static bool HasKnownAttribute(this IEnumerable<AttributeWrapper> attributes, KnownAttribute type)
        {
            if (attributes == null)
            {
                throw new ArgumentNullException(nameof(attributes));
            }

            foreach (var customAttribute in attributes)
            {
                if (customAttribute.IsKnownAttribute(type))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets if the attribute is of a known type.
        /// </summary>
        /// <param name="attr">The attribute to check.</param>
        /// <param name="attrType">The type to check.</param>
        /// <returns>If the attribute is of the known type.</returns>
        public static bool IsKnownAttribute(this AttributeWrapper attr, KnownAttribute attrType)
        {
            if (attr == null)
            {
                throw new ArgumentNullException(nameof(attr));
            }

            return attr.KnownAttribute == attrType;
        }

        /// <summary>
        /// Gets the delegate invoke method for the type. Useful for delegate types.
        /// </summary>
        /// <param name="type">The type to get the delegate invoke method for.</param>
        /// <returns>The delegate invoke method.</returns>
        public static MethodWrapper GetDelegateInvokeMethod(this TypeWrapper type)
        {
            var handle = type?.Methods.FirstOrDefault(x => x.Name == "Invoke");

            if (handle == null)
            {
                throw new Exception("Cannot find Invoke method for delegate.");
            }

            return handle;
        }

        internal static object ReadConstant(this ConstantHandle constantHandle, CompilationModule module)
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
