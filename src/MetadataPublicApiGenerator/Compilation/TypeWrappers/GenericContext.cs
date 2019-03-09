// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Reflection.Metadata;
using MetadataPublicApiGenerator.Extensions;

namespace MetadataPublicApiGenerator.Compilation.TypeWrappers
{
    internal readonly struct GenericContext
    {
        public GenericContext(ImmutableArray<TypeParameterWrapper> classTypeParameters)
        {
            ClassTypeParameters = classTypeParameters;
            MethodTypeParameters = ImmutableArray<TypeParameterWrapper>.Empty;
        }

        public GenericContext(ImmutableArray<TypeParameterWrapper> classTypeParameters, ImmutableArray<TypeParameterWrapper> methodTypeParameters)
        {
            ClassTypeParameters = classTypeParameters;
            MethodTypeParameters = methodTypeParameters;
        }

        internal GenericContext(CompilationModule module, Handle context)
        {
            switch (context.Kind)
            {
                case HandleKind.TypeDefinition:
                    var typeDefinitionHandle = (TypeDefinitionHandle)context;
                    var typeDefinition = typeDefinitionHandle.Resolve(module);

                    ClassTypeParameters = TypeParameterWrapper.Create(module, context, typeDefinition.GetGenericParameters());
                    MethodTypeParameters = ImmutableArray<TypeParameterWrapper>.Empty;
                    break;
                case HandleKind.MethodDefinition:
                    var methodDefinitionHandle = (MethodDefinitionHandle)context;
                    var methodDefinition = methodDefinitionHandle.Resolve(module);
                    var declaringTypeDefinition = methodDefinition.GetDeclaringType().Resolve(module);

                    ClassTypeParameters = TypeParameterWrapper.Create(module, methodDefinition.GetDeclaringType(), declaringTypeDefinition.GetGenericParameters());
                    MethodTypeParameters = TypeParameterWrapper.Create(module, methodDefinitionHandle, methodDefinition.GetGenericParameters());
                    break;
                default:
                    ClassTypeParameters = ImmutableArray<TypeParameterWrapper>.Empty;
                    MethodTypeParameters = ImmutableArray<TypeParameterWrapper>.Empty;
                    break;
            }
        }

        public ImmutableArray<TypeParameterWrapper> ClassTypeParameters { get; }

        public ImmutableArray<TypeParameterWrapper> MethodTypeParameters { get; }

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
