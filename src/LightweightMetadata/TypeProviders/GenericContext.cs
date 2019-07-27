// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace LightweightMetadata.TypeWrappers
{
    internal class GenericContext
    {
        private readonly AssemblyMetadata _module;

        private readonly IHandleWrapper _handleWrapper;

        internal GenericContext(AssemblyMetadata module)
        {
            ClassTypeParameters = ImmutableArray<GenericParameterWrapper>.Empty;
            MethodTypeParameters = ImmutableArray<GenericParameterWrapper>.Empty;
            _module = module;
        }

        internal GenericContext(IHandleWrapper wrapper)
        {
            if (wrapper == null)
            {
                throw new System.ArgumentNullException(nameof(wrapper));
            }

            _module = wrapper.AssemblyMetadata;
            _handleWrapper = wrapper;

            switch (wrapper)
            {
                case TypeWrapper typeWrapper:
                    ClassTypeParameters = typeWrapper.GenericParameters;
                    MethodTypeParameters = ImmutableArray<GenericParameterWrapper>.Empty;
                    break;
                case MethodWrapper methodWrapper:
                    var declaringTypeDefinition = methodWrapper.DeclaringType;

                    ClassTypeParameters = declaringTypeDefinition.GenericParameters;
                    MethodTypeParameters = methodWrapper.GenericParameters;
                    break;
                case PropertyWrapper propertyWrapper:
                    ClassTypeParameters = propertyWrapper.DeclaringType.GenericParameters;
                    MethodTypeParameters = ImmutableArray<GenericParameterWrapper>.Empty;
                    break;
                default:
                    ClassTypeParameters = ImmutableArray<GenericParameterWrapper>.Empty;
                    MethodTypeParameters = ImmutableArray<GenericParameterWrapper>.Empty;
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
            return index < ClassTypeParameters.Count ? (IHandleTypeNamedWrapper)ClassTypeParameters[index] : new DummyTypeParameterWrapper(index, "Class", _module);
        }

        public IHandleTypeNamedWrapper GetMethodTypeParameter(int index)
        {
            return index < MethodTypeParameters.Count ? (IHandleTypeNamedWrapper)MethodTypeParameters[index] : new DummyTypeParameterWrapper(index, "Method", _module);
        }
    }
}
