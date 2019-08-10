// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection.Metadata;

namespace LightweightMetadata
{
    /// <summary>
    /// Gets extension methods associated with type wrappers.
    /// </summary>
    public static class TypeExtensions
    {
        /// <summary>
        /// Gets the value tuple elements and attribute if the class has one.
        /// </summary>
        /// <param name="wrapper">The wrapper to get the value tuples for.</param>
        /// <param name="tupleElementNames">The tuple element names if there is any.</param>
        /// <returns>If we were able to retrieve the values.</returns>
        public static bool HasTupleElementNamesAttribute(this IHandleNameWrapper wrapper, out string[] tupleElementNames)
        {
            if (wrapper is null)
            {
                throw new ArgumentNullException(nameof(wrapper));
            }

            if (wrapper is IHasAttributes hasAttributes && hasAttributes.Attributes.TryGetKnownAttribute(KnownAttribute.TupleElementNames, out var attributeWrapper))
            {
                tupleElementNames = ProcessStringFixedValue(attributeWrapper.FixedArguments[0].Value);
                return true;
            }

            if (wrapper is IHasReturnAttributes returnAttributes)
            {
                if (returnAttributes.ReturnAttributes.TryGetKnownAttribute(KnownAttribute.TupleElementNames, out attributeWrapper))
                {
                    tupleElementNames = ProcessStringFixedValue(attributeWrapper.FixedArguments[0].Value);
                    return true;
                }
            }

            tupleElementNames = Array.Empty<string>();
            return false;
        }

        /// <summary>
        /// Gets the nullability of a item.
        /// </summary>
        /// <param name="attributes">The attributes to get nullability for.</param>
        /// <param name="nullability">The nullability of the type.</param>
        /// <returns>If we were able to retrieve the values.</returns>
        public static bool TryGetNullable(this IEnumerable<AttributeWrapper> attributes, out Nullability[] nullability)
        {
            if (attributes.TryGetKnownAttribute(KnownAttribute.Nullable, out var attributeWrapper))
            {
                var paramValue = attributeWrapper.FixedArguments[0].Value;
                byte[] values = paramValue is IEnumerable ? ProcessByteValue(paramValue) : new[] { (byte)paramValue };

                nullability = values.Cast<Nullability>().ToArray();
                return true;
            }

            nullability = Array.Empty<Nullability>();
            return false;
        }

        /// <summary>
        /// Gets the nullable context of a item.
        /// </summary>
        /// <param name="attributes">The attributes to get nullable context for.</param>
        /// <param name="nullability">The nullability of the type.</param>
        /// <returns>If we were able to retrieve the values.</returns>
        public static bool TryGetNullableContext(this IEnumerable<AttributeWrapper> attributes, out Nullability nullability)
        {
            nullability = default;
            if (attributes.TryGetKnownAttribute(KnownAttribute.NullableContext, out var attributeWrapper))
            {
                var paramValue = attributeWrapper.FixedArguments[0].Value;
                byte values = (byte)paramValue;

                nullability = (Nullability)values;
                return true;
            }

            return false;
        }

        internal static IReadOnlyList<AttributeWrapper> GetReturnAttributes(this in ParameterHandleCollection parameters, AssemblyMetadata metadata)
        {
            foreach (var parameterHandle in parameters)
            {
                var parameterInstance = metadata.MetadataReader.GetParameter(parameterHandle);

                if (parameterInstance.SequenceNumber == 0)
                {
                    return AttributeWrapper.Create(parameterInstance.GetCustomAttributes(), metadata);
                }
            }

            return Array.Empty<AttributeWrapper>();
        }

        private static string[] ProcessStringFixedValue(object value)
        {
            var array = (ImmutableArray<CustomAttributeTypedArgument<IHandleTypeNamedWrapper>>)value;

            return array.Select(x => (string)x.Value).ToArray();
        }

        private static byte[] ProcessByteValue(object value)
        {
            var array = (ImmutableArray<CustomAttributeTypedArgument<IHandleTypeNamedWrapper>>)value;

            return array.Select(x => (byte)x.Value).ToArray();
        }
    }
}
