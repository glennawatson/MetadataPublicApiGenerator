﻿// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Security.Cryptography;
using System.Text;
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

        [SuppressMessage("Design", "CA5350: Compromised hash algorithm", Justification = "Deliberate usage")]
        [SuppressMessage("Design", "CA5351: Compromised hash algorithm", Justification = "Deliberate usage")]
        internal static HashAlgorithm GetHashAlgorithm(this AssemblyHashAlgorithm hashAlgorithm)
        {
            switch (hashAlgorithm)
            {
                case AssemblyHashAlgorithm.None:
                    // only for multi-module assemblies?
                    return SHA1.Create();
                case AssemblyHashAlgorithm.MD5:
                    return MD5.Create();
                case AssemblyHashAlgorithm.Sha1:
                    return SHA1.Create();
                case AssemblyHashAlgorithm.Sha256:
                    return SHA256.Create();
                case AssemblyHashAlgorithm.Sha384:
                    return SHA384.Create();
                case AssemblyHashAlgorithm.Sha512:
                    return SHA512.Create();
                default:
                    return SHA1.Create(); // default?
            }
        }

        internal static string CalculatePublicKeyToken(this BlobHandle publicKeyBlob, CompilationModule module, AssemblyHashAlgorithm assemblyHashAlgorithm)
        {
            if (publicKeyBlob.IsNil)
            {
                return "null";
            }

            var reader = module.MetadataReader;
            using (var hashAlgorithm = assemblyHashAlgorithm.GetHashAlgorithm())
            {
                // Calculate public key token:
                // 1. hash the public key using the appropriate algorithm.
                byte[] publicKeyTokenBytes = hashAlgorithm.ComputeHash(reader.GetBlobBytes(publicKeyBlob));

                // 2. take the last 8 bytes
                // 3. according to Cecil we need to reverse them, other sources did not mention this.
                var bytes = publicKeyTokenBytes.Skip(publicKeyTokenBytes.Length - 8).Reverse().ToArray();

                var sb = new StringBuilder(bytes.Length * 2);
                foreach (var b in bytes)
                {
                    sb.AppendFormat(CultureInfo.InvariantCulture, "{0:x2}", b);
                }

                return sb.ToString();
            }
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