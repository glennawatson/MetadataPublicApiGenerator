// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection.Metadata;
using MetadataPublicApiGenerator.Extensions;

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

        internal GenericContext(CompilationModule module, Handle context)
        {
            _module = module;
            if (module == null)
            {
                throw new System.ArgumentNullException(nameof(module));
            }

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

            if (ClassTypeParameters.Any(x => x == null))
            {
                throw new ArgumentNullException(nameof(context));
            }
        }

        public ImmutableArray<TypeParameterWrapper> ClassTypeParameters { get; }

        public ImmutableArray<TypeParameterWrapper> MethodTypeParameters { get; }

        public ITypeNamedWrapper GetClassTypeParameter(int index)
        {
            return index < ClassTypeParameters.Length ? (ITypeNamedWrapper)ClassTypeParameters[index] : new DummyTypeParameterWrapper(index, "Class", _module);
        }

        public ITypeNamedWrapper GetMethodTypeParameter(int index)
        {
            return index < MethodTypeParameters.Length ? (ITypeNamedWrapper)MethodTypeParameters[index] : new DummyTypeParameterWrapper(index, "Method", _module);
        }
    }
}
