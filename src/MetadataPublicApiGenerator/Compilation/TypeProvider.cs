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
    internal class TypeProvider : ICustomAttributeTypeProvider<IWrapper>, ISignatureTypeProvider<IWrapper, GenericContext>
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
        public IWrapper GetSystemType()
        {
            var value = _compilation.GetTypeDefinitionByName("System.Type").First();
            return new TypeWrapper(value.module, _compilation.GetTypeDefinitionByName("System.Type").First().typeDefinitionHandle);
        }

        /// <inheritdoc />
        public bool IsSystemType(IWrapper type)
        {
            return type.IsKnownType;
        }

        /// <inheritdoc />
        public IWrapper GetTypeFromSerializedName(string name)
        {
            var value = _compilation.GetTypeDefinitionByName(name).First();
            return new TypeWrapper(value.module, value.typeDefinitionHandle);
        }

        /// <inheritdoc />
        public PrimitiveTypeCode GetUnderlyingEnumType(IWrapper type)
        {
            ((TypeDefinitionHandle)((IHandleWrapper)type).Handle).IsEnum(type.Module, out var primitiveType);

            return primitiveType;
        }

        /// <inheritdoc />
        public IWrapper GetPrimitiveType(PrimitiveTypeCode typeCode)
        {
            var element = typeCode.ToKnownTypeCode().ToTypeDefinitionHandle(_compilation);
            return new TypeWrapper(element.module, element.typeDefinition);
        }

        /// <inheritdoc />
        public IWrapper GetTypeFromDefinition(MetadataReader reader, TypeDefinitionHandle handle, byte rawTypeKind)
        {
            var module = _compilation.GetCompilationModuleForReader(reader);
            return new TypeWrapper(module, handle);
        }

        /// <inheritdoc />
        public IWrapper GetTypeFromReference(MetadataReader reader, TypeReferenceHandle handle, byte rawTypeKind)
        {
            var module = _compilation.GetCompilationModuleForReader(reader);
            var typeReference = handle.Resolve(module);

            var name = typeReference.Namespace.GetName(module) + "." + typeReference.Name.GetName(module);

            var resolve = _compilation.GetTypeDefinitionByName(name).FirstOrDefault();
            return new TypeWrapper(resolve.module, resolve.typeDefinitionHandle);
        }

        /// <inheritdoc />
        public IWrapper GetSZArrayType(IWrapper elementType)
        {
            return new ArrayTypeWrapper(elementType.Module, (ITypeNamedWrapper)elementType, 1);
        }

        /// <inheritdoc />
        public IWrapper GetGenericInstantiation(IWrapper genericType, ImmutableArray<IWrapper> typeArguments)
        {
            return new ParameterizedTypeWrapper(genericType.Module, genericType, typeArguments);
        }

        /// <inheritdoc />
        public IWrapper GetArrayType(IWrapper elementType, ArrayShape shape)
        {
            return new ArrayTypeWrapper(elementType.Module, (ITypeNamedWrapper)elementType, shape.Rank);
        }

        /// <inheritdoc />
        public IWrapper GetByReferenceType(IWrapper elementType)
        {
            return new ByReferenceWrapper(elementType.Module, (ITypeWrapper)elementType);
        }

        /// <inheritdoc />
        public IWrapper GetPointerType(IWrapper elementType)
        {
            return new PointerWrapper(elementType.Module, (ITypeWrapper)elementType);
        }

        /// <inheritdoc />
        public IWrapper GetFunctionPointerType(MethodSignature<IWrapper> signature)
        {
            var element = KnownTypeCode.IntPtr.ToTypeDefinitionHandle(_compilation);
            return new TypeWrapper(element.module, element.typeDefinition);
        }

        /// <inheritdoc />
        public IWrapper GetGenericMethodParameter(GenericContext genericContext, int index)
        {
            return genericContext.GetMethodTypeParameter(index);
        }

        /// <inheritdoc />
        public IWrapper GetGenericTypeParameter(GenericContext genericContext, int index)
        {
            return genericContext.GetClassTypeParameter(index);
        }

        /// <inheritdoc />
        public IWrapper GetModifiedType(IWrapper modifier, IWrapper unmodifiedType, bool isRequired)
        {
            return new ModifiedTypeWrapper(modifier.Module, (ITypeNamedWrapper)modifier, (ITypeNamedWrapper)unmodifiedType, isRequired);
        }

        /// <inheritdoc />
        public IWrapper GetPinnedType(IWrapper elementType)
        {
            return new PinnedTypeWrapper(elementType.Module, (ITypeWrapper)elementType);
        }

        /// <inheritdoc />
        public IWrapper GetTypeFromSpecification(MetadataReader reader, GenericContext genericContext, TypeSpecificationHandle handle, byte rawTypeKind)
        {
            return reader.GetTypeSpecification(handle).DecodeSignature(this, genericContext);
        }
    }
}
