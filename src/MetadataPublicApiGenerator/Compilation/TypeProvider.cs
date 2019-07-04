// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection.Metadata;
using MetadataPublicApiGenerator.Compilation.TypeWrappers;
using MetadataPublicApiGenerator.Extensions;

namespace MetadataPublicApiGenerator.Compilation
{
    /// <summary>
    /// A type provider for the System.Reflection.Metadata based set of methods to decode attributes and method signatures.
    /// </summary>
    internal class TypeProvider : ISignatureTypeProvider<ITypeNamedWrapper, GenericContext>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TypeProvider"/> class.
        /// </summary>
        /// <param name="compilation">The compilation to use to determine types.</param>
        public TypeProvider(ICompilation compilation)
        {
            Compilation = compilation;
        }

        protected ICompilation Compilation { get; }

        /// <inheritdoc />
        public ITypeNamedWrapper GetPrimitiveType(PrimitiveTypeCode typeCode)
        {
            var element = typeCode.ToKnownTypeCode().ToTypeDefinitionHandle(Compilation);

            if (element.typeDefinition == null)
            {
                throw new InvalidOperationException("type definition is null for a primitive type.");
            }

            return element.typeDefinition;
        }

        /// <inheritdoc />
        public ITypeNamedWrapper GetTypeFromDefinition(MetadataReader reader, TypeDefinitionHandle handle, byte rawTypeKind)
        {
            var module = Compilation.GetCompilationModuleForReader(reader);
            return TypeWrapper.Create(handle, module);
        }

        /// <inheritdoc />
        public ITypeNamedWrapper GetTypeFromReference(MetadataReader reader, TypeReferenceHandle handle, byte rawTypeKind)
        {
            var module = Compilation.GetCompilationModuleForReader(reader);

            var name = handle.GetFullName(module);

            var resolve = Compilation.GetTypeDefinitionByName(name).FirstOrDefault();

            if (resolve.module != null)
            {
                return resolve.typeWrapper;
            }

            return new UnknownType(module, handle);
        }

        /// <inheritdoc />
        public ITypeNamedWrapper GetSZArrayType(ITypeNamedWrapper elementType)
        {
            return new ArrayTypeWrapper(Compilation, elementType, 1);
        }

        /// <inheritdoc />
        public ITypeNamedWrapper GetGenericInstantiation(ITypeNamedWrapper genericType, ImmutableArray<ITypeNamedWrapper> typeArguments)
        {
            return new ParameterizedTypeWrapper(genericType.Module, genericType, typeArguments);
        }

        /// <inheritdoc />
        public ITypeNamedWrapper GetArrayType(ITypeNamedWrapper elementType, ArrayShape shape)
        {
            return new ArrayTypeWrapper(Compilation, elementType, shape.Rank);
        }

        /// <inheritdoc />
        public ITypeNamedWrapper GetByReferenceType(ITypeNamedWrapper elementType)
        {
            return new ByReferenceWrapper(elementType.Module, elementType);
        }

        /// <inheritdoc />
        public ITypeNamedWrapper GetPointerType(ITypeNamedWrapper elementType)
        {
            return new PointerWrapper(elementType.Module, (ITypeWrapper)elementType);
        }

        /// <inheritdoc />
        public ITypeNamedWrapper GetFunctionPointerType(MethodSignature<ITypeNamedWrapper> signature)
        {
            var element = KnownTypeCode.IntPtr.ToTypeDefinitionHandle(Compilation);
            return element.typeDefinition;
        }

        /// <inheritdoc />
        public ITypeNamedWrapper GetGenericMethodParameter(GenericContext genericContext, int index)
        {
            return genericContext.GetMethodTypeParameter(index);
        }

        /// <inheritdoc />
        public ITypeNamedWrapper GetGenericTypeParameter(GenericContext genericContext, int index)
        {
            return genericContext.GetClassTypeParameter(index);
        }

        /// <inheritdoc />
        public ITypeNamedWrapper GetModifiedType(ITypeNamedWrapper modifier, ITypeNamedWrapper unmodifiedType, bool isRequired)
        {
            return new ModifiedTypeWrapper(modifier.Module, modifier, unmodifiedType, isRequired);
        }

        /// <inheritdoc />
        public ITypeNamedWrapper GetPinnedType(ITypeNamedWrapper elementType)
        {
            return new PinnedTypeWrapper(elementType.Module, (ITypeWrapper)elementType);
        }

        /// <inheritdoc />
        public ITypeNamedWrapper GetTypeFromSpecification(MetadataReader reader, GenericContext genericContext, TypeSpecificationHandle handle, byte rawTypeKind)
        {
            return reader.GetTypeSpecification(handle).DecodeSignature(this, genericContext);
        }
    }
}
