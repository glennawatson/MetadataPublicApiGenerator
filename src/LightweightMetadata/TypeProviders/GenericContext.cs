// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace LightweightMetadata
{
    internal class GenericContext
    {
        internal GenericContext(AssemblyMetadata assemblyMetadata)
        {
            ClassTypeParameters = Array.Empty<GenericParameterWrapper>();
            MethodTypeParameters = Array.Empty<GenericParameterWrapper>();
        }

        internal GenericContext(IHandleWrapper wrapper)
        {
            if (wrapper == null)
            {
                throw new ArgumentNullException(nameof(wrapper));
            }

            switch (wrapper)
            {
                case TypeWrapper typeWrapper:
                    ClassTypeParameters = typeWrapper.GenericParameters;
                    MethodTypeParameters = Array.Empty<GenericParameterWrapper>();
                    break;
                case MethodWrapper methodWrapper:
                    var declaringTypeDefinition = methodWrapper.DeclaringType;

                    ClassTypeParameters = declaringTypeDefinition.GenericParameters;
                    MethodTypeParameters = methodWrapper.GenericParameters;
                    break;
                case PropertyWrapper propertyWrapper:
                    ClassTypeParameters = propertyWrapper.DeclaringType.GenericParameters;
                    MethodTypeParameters = Array.Empty<GenericParameterWrapper>();
                    break;
                case IHasGenericParameters genericParameters:
                    ClassTypeParameters = genericParameters.GenericParameters;
                    break;
                default:
                    ClassTypeParameters = Array.Empty<GenericParameterWrapper>();
                    MethodTypeParameters = Array.Empty<GenericParameterWrapper>();
                    break;
            }

            if (ClassTypeParameters.Any(x => x == null))
            {
                throw new ArgumentNullException(nameof(wrapper));
            }
        }

        public IReadOnlyList<GenericParameterWrapper> ClassTypeParameters { get; }

        public IReadOnlyList<GenericParameterWrapper> MethodTypeParameters { get; }

        public IHandleTypeNamedWrapper GetClassTypeParameter(int index)
        {
            return index < ClassTypeParameters.Count ? ClassTypeParameters[index] : default;
        }

        public IHandleTypeNamedWrapper GetMethodTypeParameter(int index)
        {
            return index < MethodTypeParameters.Count ? MethodTypeParameters[index] : default;
        }
    }
}
