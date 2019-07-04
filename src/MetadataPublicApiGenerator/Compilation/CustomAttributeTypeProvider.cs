// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Linq;
using System.Reflection.Metadata;

using MetadataPublicApiGenerator.Compilation.TypeWrappers;
using MetadataPublicApiGenerator.Extensions;

namespace MetadataPublicApiGenerator.Compilation
{
    internal class CustomAttributeTypeProvider : ICustomAttributeTypeProvider<ITypeNamedWrapper>
    {
        public CustomAttributeTypeProvider(ICompilation compilation)
        {
            Compilation = compilation;
        }

        private ICompilation Compilation { get; }

        /// <inheritdoc />
        public ITypeNamedWrapper GetSystemType()
        {
            return Compilation.GetTypeDefinitionByName("System.Type").First().typeWrapper;
        }

        /// <inheritdoc />
        public bool IsSystemType(ITypeNamedWrapper type)
        {
            return type.IsKnownType;
        }

        /// <inheritdoc />
        public ITypeNamedWrapper GetTypeFromSerializedName(string name)
        {
            var value = Compilation.GetTypeDefinitionByName(name).FirstOrDefault();

            if (value.typeWrapper != null)
            {
                return value.typeWrapper;
            }

            return null;
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
            return new TypeWrapper(module, handle);
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
    }
}
