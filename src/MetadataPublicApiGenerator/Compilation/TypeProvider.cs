// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

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
    internal class TypeProvider : ICustomAttributeTypeProvider<ITypeNamedWrapper>, ISignatureTypeProvider<ITypeNamedWrapper, GenericContext>
    {
        private readonly ICompilation _compilation;

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeProvider"/> class.
        /// </summary>
        /// <param name="compilation">The compilation to use to determine types.</param>
        public TypeProvider(ICompilation compilation)
        {
            _compilation = compilation;
        }

        /// <inheritdoc />
        public ITypeNamedWrapper GetSystemType()
        {
            var value = _compilation.GetTypeDefinitionByName("System.Type").First();
            return _compilation.GetTypeDefinitionByName("System.Type").First().typeWrapper;
        }

        /// <inheritdoc />
        public bool IsSystemType(ITypeNamedWrapper type)
        {
            return type.IsKnownType;
        }

        /// <inheritdoc />
        public ITypeNamedWrapper GetTypeFromSerializedName(string name)
        {
            var value = _compilation.GetTypeDefinitionByName(name).FirstOrDefault();
            return value.typeWrapper;
        }

        /// <inheritdoc />
        public PrimitiveTypeCode GetUnderlyingEnumType(ITypeNamedWrapper type)
        {
            ((TypeDefinitionHandle)((IHandleWrapper)type).Handle).IsEnum(type.Module, out var primitiveType);

            return primitiveType;
        }

        /// <inheritdoc />
        public ITypeNamedWrapper GetPrimitiveType(PrimitiveTypeCode typeCode)
        {
            var element = typeCode.ToKnownTypeCode().ToTypeDefinitionHandle(_compilation);

            return element.module == null ? null : element.typeDefinition;
        }

        /// <inheritdoc />
        public ITypeNamedWrapper GetTypeFromDefinition(MetadataReader reader, TypeDefinitionHandle handle, byte rawTypeKind)
        {
            var module = _compilation.GetCompilationModuleForReader(reader);
            return new TypeWrapper(module, handle);
        }

        /// <inheritdoc />
        public ITypeNamedWrapper GetTypeFromReference(MetadataReader reader, TypeReferenceHandle handle, byte rawTypeKind)
        {
            var module = _compilation.GetCompilationModuleForReader(reader);

            var name = handle.GetFullName(module);

            var resolve = _compilation.GetTypeDefinitionByName(name).FirstOrDefault();

            if (resolve.module != null)
            {
                return resolve.typeWrapper;
            }

            return new UnknownType(module, handle);
        }

        /// <inheritdoc />
        public ITypeNamedWrapper GetSZArrayType(ITypeNamedWrapper elementType)
        {
            return new ArrayTypeWrapper(_compilation, elementType, 1);
        }

        /// <inheritdoc />
        public ITypeNamedWrapper GetGenericInstantiation(ITypeNamedWrapper genericType, ImmutableArray<ITypeNamedWrapper> typeArguments)
        {
            return new ParameterizedTypeWrapper(genericType.Module, genericType, typeArguments);
        }

        /// <inheritdoc />
        public ITypeNamedWrapper GetArrayType(ITypeNamedWrapper elementType, ArrayShape shape)
        {
            return new ArrayTypeWrapper(_compilation, elementType, shape.Rank);
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
            var element = KnownTypeCode.IntPtr.ToTypeDefinitionHandle(_compilation);
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
