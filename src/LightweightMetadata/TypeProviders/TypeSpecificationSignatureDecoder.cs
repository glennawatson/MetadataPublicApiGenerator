// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Immutable;
using System.Reflection.Metadata;

using LightweightMetadata.Extensions;

namespace LightweightMetadata.TypeWrappers
{
    internal class TypeSpecificationSignatureDecoder : ISignatureTypeProvider<IHandleTypeNamedWrapper, TypeSpecificationSignatureDecoder.Unit>
    {
        private readonly IMetadataRepository _compilation;

        public TypeSpecificationSignatureDecoder(IMetadataRepository compilation)
        {
            _compilation = compilation;
        }

        /// <inheritdoc />
        public IHandleTypeNamedWrapper GetPrimitiveType(PrimitiveTypeCode typeCode)
        {
            return typeCode.ToKnownTypeCode().ToTypeWrapper(_compilation);
        }

        /// <inheritdoc />
        public IHandleTypeNamedWrapper GetTypeFromDefinition(MetadataReader reader, TypeDefinitionHandle handle, byte rawTypeKind)
        {
            return TypeWrapper.Create(handle, _compilation.GetAssemblyMetadataForReader(reader));
        }

        /// <inheritdoc />
        public IHandleTypeNamedWrapper GetTypeFromReference(MetadataReader reader, TypeReferenceHandle handle, byte rawTypeKind)
        {
            var module = _compilation.GetAssemblyMetadataForReader(reader);

            return WrapperFactory.Create(handle, module);
        }

        /// <inheritdoc />
        public IHandleTypeNamedWrapper GetSZArrayType(IHandleTypeNamedWrapper elementType)
        {
            return elementType;
        }

        /// <inheritdoc />
        public IHandleTypeNamedWrapper GetGenericInstantiation(IHandleTypeNamedWrapper genericType, ImmutableArray<IHandleTypeNamedWrapper> typeArguments)
        {
            return genericType;
        }

        /// <inheritdoc />
        public IHandleTypeNamedWrapper GetArrayType(IHandleTypeNamedWrapper elementType, ArrayShape shape)
        {
            return elementType;
        }

        /// <inheritdoc />
        public IHandleTypeNamedWrapper GetByReferenceType(IHandleTypeNamedWrapper elementType)
        {
            return elementType;
        }

        /// <inheritdoc />
        public IHandleTypeNamedWrapper GetPointerType(IHandleTypeNamedWrapper elementType)
        {
            return elementType;
        }

        /// <inheritdoc />
        public IHandleTypeNamedWrapper GetFunctionPointerType(MethodSignature<IHandleTypeNamedWrapper> signature)
        {
            return default;
        }

        /// <inheritdoc />
        public IHandleTypeNamedWrapper GetGenericMethodParameter(Unit genericContext, int index)
        {
            return default;
        }

        /// <inheritdoc />
        public IHandleTypeNamedWrapper GetGenericTypeParameter(Unit genericContext, int index)
        {
            return default;
        }

        /// <inheritdoc />
        public IHandleTypeNamedWrapper GetModifiedType(IHandleTypeNamedWrapper modifier, IHandleTypeNamedWrapper unmodifiedType, bool isRequired)
        {
            return unmodifiedType;
        }

        /// <inheritdoc />
        public IHandleTypeNamedWrapper GetPinnedType(IHandleTypeNamedWrapper elementType)
        {
            return elementType;
        }

        /// <inheritdoc />
        public IHandleTypeNamedWrapper GetTypeFromSpecification(MetadataReader reader, Unit genericContext, TypeSpecificationHandle handle, byte rawTypeKind)
        {
            return reader.GetTypeSpecification(handle).DecodeSignature(new TypeSpecificationSignatureDecoder(_compilation), Unit.Default);
        }

        internal struct Unit
        {
            public static Unit Default = default(Unit);
        }
    }
}
