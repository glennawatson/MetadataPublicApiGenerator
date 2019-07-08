// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace MetadataPublicApiGenerator.Compilation.TypeWrappers
{
    internal class GenericContext
    {
        private readonly CompilationModule _module;

        internal GenericContext(CompilationModule module)
        {
            ClassTypeParameters = ImmutableArray<TypeParameterWrapper>.Empty;
            MethodTypeParameters = ImmutableArray<TypeParameterWrapper>.Empty;
            _module = module;
        }

        internal GenericContext(IHandleWrapper wrapper)
        {
            if (wrapper == null)
            {
                throw new System.ArgumentNullException(nameof(wrapper));
            }

            _module = wrapper.Module;

            switch (wrapper)
            {
                case TypeWrapper typeWrapper:
                    ClassTypeParameters = typeWrapper.GenericParameters;
                    MethodTypeParameters = ImmutableArray<TypeParameterWrapper>.Empty;
                    break;
                case MethodWrapper methodWrapper:
                    var declaringTypeDefinition = methodWrapper.DeclaringType;

                    ClassTypeParameters = declaringTypeDefinition.GenericParameters;
                    MethodTypeParameters = methodWrapper.GenericParameters;
                    break;
                default:
                    ClassTypeParameters = ImmutableArray<TypeParameterWrapper>.Empty;
                    MethodTypeParameters = ImmutableArray<TypeParameterWrapper>.Empty;
                    break;
            }

            if (ClassTypeParameters.Any(x => x == null))
            {
                throw new ArgumentNullException(nameof(wrapper));
            }
        }

        public IReadOnlyList<TypeParameterWrapper> ClassTypeParameters { get; }

        public IReadOnlyList<TypeParameterWrapper> MethodTypeParameters { get; }

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
