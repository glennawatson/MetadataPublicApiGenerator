// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata;
using MetadataPublicApiGenerator.Extensions;

namespace MetadataPublicApiGenerator.Compilation.TypeWrappers
{
    internal readonly struct GenericContext
    {
        public readonly IImmutableList<TypeParameterWrapper> ClassTypeParameters;
        public readonly IImmutableList<TypeParameterWrapper> MethodTypeParameters;

        public GenericContext(IImmutableList<TypeParameterWrapper> classTypeParameters)
        {
            ClassTypeParameters = classTypeParameters ?? ImmutableList<TypeParameterWrapper>.Empty;
            MethodTypeParameters = ImmutableList<TypeParameterWrapper>.Empty;
        }

        public GenericContext(IImmutableList<TypeParameterWrapper> classTypeParameters, IImmutableList<TypeParameterWrapper> methodTypeParameters)
        {
            ClassTypeParameters = classTypeParameters ?? ImmutableList<TypeParameterWrapper>.Empty;
            MethodTypeParameters = methodTypeParameters ?? ImmutableList<TypeParameterWrapper>.Empty;
        }

        internal GenericContext(CompilationModule module, Handle context)
        {
            var typeDefinitionHandle = (TypeDefinitionHandle)context;
            var methodDefinitionHandle = (MethodDefinitionHandle)context;
            if (!typeDefinitionHandle.IsNil)
            {
                var typeDefinition = typeDefinitionHandle.Resolve(module);
                ClassTypeParameters = TypeParameterWrapper.Create(module, context, typeDefinition.GetGenericParameters());
                MethodTypeParameters = ImmutableList<TypeParameterWrapper>.Empty;
            }
            else if (!methodDefinitionHandle.IsNil)
            {
                var methodDefinition = methodDefinitionHandle.Resolve(module);
                var declaringTypeDefinition = methodDefinition.GetDeclaringType().Resolve(module);

                ClassTypeParameters = TypeParameterWrapper.Create(module, methodDefinition.GetDeclaringType(), declaringTypeDefinition.GetGenericParameters());
                MethodTypeParameters = TypeParameterWrapper.Create(module, methodDefinitionHandle, methodDefinition.GetGenericParameters());
            }
            else
            {
                ClassTypeParameters = ImmutableList<TypeParameterWrapper>.Empty;
                MethodTypeParameters = ImmutableList<TypeParameterWrapper>.Empty;
            }
        }

        public TypeParameterWrapper GetClassTypeParameter(int index)
        {
            return ClassTypeParameters[index];
        }

        public TypeParameterWrapper GetMethodTypeParameter(int index)
        {
            return MethodTypeParameters[index];
        }
    }
}
