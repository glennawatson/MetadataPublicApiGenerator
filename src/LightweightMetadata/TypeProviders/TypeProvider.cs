// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection.Metadata;

#nullable disable
namespace LightweightMetadata
{
    /// <summary>
    /// A type provider for the System.Reflection.Metadata based set of methods to decode attributes and method signatures.
    /// </summary>
    internal class TypeProvider : ISignatureTypeProvider<IHandleTypeNamedWrapper, GenericContext>, ICustomAttributeTypeProvider<IHandleTypeNamedWrapper>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TypeProvider"/> class.
        /// </summary>
        /// <param name="metadataRepository">The MetadataRepository to use to determine types.</param>
        public TypeProvider(MetadataRepository metadataRepository)
        {
            MetadataRepository = metadataRepository;
        }

        protected MetadataRepository MetadataRepository { get; }

        /// <inheritdoc />
        public IHandleTypeNamedWrapper GetPrimitiveType(PrimitiveTypeCode typeCode)
        {
            var element = typeCode.ToKnownTypeCode().ToTypeWrapper(MetadataRepository);

            if (element == null)
            {
                throw new InvalidOperationException("type definition is null for a primitive type.");
            }

            return element;
        }

        /// <inheritdoc />
        public IHandleTypeNamedWrapper GetTypeFromDefinition(MetadataReader reader, TypeDefinitionHandle handle, byte rawTypeKind)
        {
            var assemblyMetadata = MetadataRepository.GetAssemblyMetadataForReader(reader);
            return TypeWrapper.Create(handle, assemblyMetadata);
        }

        /// <inheritdoc />
        public IHandleTypeNamedWrapper GetTypeFromReference(MetadataReader reader, TypeReferenceHandle handle, byte rawTypeKind)
        {
            var assemblyMetadata = MetadataRepository.GetAssemblyMetadataForReader(reader);
            return WrapperFactory.Create(handle, assemblyMetadata);
        }

        /// <inheritdoc />
        public IHandleTypeNamedWrapper GetSZArrayType(IHandleTypeNamedWrapper elementType)
        {
            return new ArrayTypeWrapper(elementType, null);
        }

        /// <inheritdoc />
        public IHandleTypeNamedWrapper GetGenericInstantiation(IHandleTypeNamedWrapper genericType, ImmutableArray<IHandleTypeNamedWrapper> typeArguments)
        {
            if (typeArguments.Any(x => x == null))
            {
                return MetadataRepository.GetTypeByName(genericType.FullName);
            }

            return new ParameterizedTypeWrapper(genericType, typeArguments.ToArray());
        }

        /// <inheritdoc />
        public IHandleTypeNamedWrapper GetArrayType(IHandleTypeNamedWrapper elementType, ArrayShape shape)
        {
            return new ArrayTypeWrapper(elementType, new ArrayShapeData(shape.Rank, shape.Sizes, shape.LowerBounds));
        }

        /// <inheritdoc />
        public IHandleTypeNamedWrapper GetByReferenceType(IHandleTypeNamedWrapper elementType)
        {
            return new ByReferenceWrapper(elementType);
        }

        /// <inheritdoc />
        public IHandleTypeNamedWrapper GetPointerType(IHandleTypeNamedWrapper elementType)
        {
            return new PointerWrapper((TypeWrapper)elementType);
        }

        /// <inheritdoc />
        public IHandleTypeNamedWrapper GetFunctionPointerType(MethodSignature<IHandleTypeNamedWrapper> signature)
        {
            return KnownTypeCode.IntPtr.ToTypeWrapper(MetadataRepository);
        }

        /// <inheritdoc />
        public IHandleTypeNamedWrapper GetGenericMethodParameter(GenericContext genericContext, int index)
        {
            return genericContext?.GetMethodTypeParameter(index);
        }

        /// <inheritdoc />
        public IHandleTypeNamedWrapper GetGenericTypeParameter(GenericContext genericContext, int index)
        {
            return genericContext?.GetClassTypeParameter(index);
        }

        /// <inheritdoc />
        public IHandleTypeNamedWrapper GetModifiedType(IHandleTypeNamedWrapper modifier, IHandleTypeNamedWrapper unmodifiedType, bool isRequired)
        {
            return new ModifiedTypeWrapper(modifier, unmodifiedType, isRequired);
        }

        /// <inheritdoc />
        public IHandleTypeNamedWrapper GetPinnedType(IHandleTypeNamedWrapper elementType)
        {
            return new PinnedTypeWrapper((TypeWrapper)elementType);
        }

        /// <inheritdoc />
        public IHandleTypeNamedWrapper GetTypeFromSpecification(MetadataReader reader, GenericContext genericContext, TypeSpecificationHandle handle, byte rawTypeKind)
        {
            return reader.GetTypeSpecification(handle).DecodeSignature(this, genericContext);
        }

        /// <inheritdoc />
        public IHandleTypeNamedWrapper GetSystemType()
        {
            return MetadataRepository.GetTypeByName("System.Type");
        }

        /// <inheritdoc />
        public bool IsSystemType(IHandleTypeNamedWrapper type)
        {
            return type.ToKnownTypeCode() != KnownTypeCode.None;
        }

        /// <inheritdoc />
        public IHandleTypeNamedWrapper GetTypeFromSerializedName(string name)
        {
            int index = name.IndexOf(',');

            if (index >= 0)
            {
                name = name.Substring(0, index);
            }

            return MetadataRepository.GetTypeByName(name);
        }

        /// <inheritdoc />
        public PrimitiveTypeCode GetUnderlyingEnumType(IHandleTypeNamedWrapper type)
        {
            if (type is TypeWrapper typeWrapper)
            {
                if (typeWrapper.TryGetEnumType(out var primitiveType))
                {
                    return KnownTypeToPrimitiveType(primitiveType.ToKnownTypeCode());
                }
            }

            return PrimitiveTypeCode.Int32;
        }

        private PrimitiveTypeCode KnownTypeToPrimitiveType(KnownTypeCode knownType)
        {
            switch (knownType)
            {
                case KnownTypeCode.Byte:
                    return PrimitiveTypeCode.Byte;
                case KnownTypeCode.Boolean:
                    return PrimitiveTypeCode.Boolean;
                case KnownTypeCode.Char:
                    return PrimitiveTypeCode.Char;
                case KnownTypeCode.Double:
                    return PrimitiveTypeCode.Double;
                case KnownTypeCode.Single:
                    return PrimitiveTypeCode.Single;
                case KnownTypeCode.Int16:
                    return PrimitiveTypeCode.Int16;
                case KnownTypeCode.Int32:
                    return PrimitiveTypeCode.Int32;
                case KnownTypeCode.Int64:
                    return PrimitiveTypeCode.Int64;
                case KnownTypeCode.IntPtr:
                    return PrimitiveTypeCode.IntPtr;
                case KnownTypeCode.UIntPtr:
                    return PrimitiveTypeCode.UIntPtr;
                case KnownTypeCode.Object:
                    return PrimitiveTypeCode.Object;
                case KnownTypeCode.SByte:
                    return PrimitiveTypeCode.SByte;
                case KnownTypeCode.UInt16:
                    return PrimitiveTypeCode.UInt16;
                case KnownTypeCode.UInt32:
                    return PrimitiveTypeCode.UInt32;
                case KnownTypeCode.UInt64:
                    return PrimitiveTypeCode.UInt64;
                case KnownTypeCode.String:
                    return PrimitiveTypeCode.String;
                case KnownTypeCode.TypedReference:
                    return PrimitiveTypeCode.TypedReference;
                case KnownTypeCode.Void:
                    return PrimitiveTypeCode.Void;
                default:
                    throw new Exception("Unsupported known type code: " + knownType);
            }
        }
    }
}
