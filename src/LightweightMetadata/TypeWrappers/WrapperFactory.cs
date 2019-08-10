// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reflection.Metadata;

namespace LightweightMetadata
{
    /// <summary>
    /// Creates wrapper given a handle.
    /// </summary>
    public static class WrapperFactory
    {
        /// <summary>
        /// Creates a wrapper given a handle.
        /// </summary>
        /// <param name="entity">The handle of the element to be wrapped.</param>
        /// <param name="assemblyMetadata">The module hosting the handle.</param>
        /// <returns>A wrapper or null if one cannot be created.</returns>
        public static IHandleTypeNamedWrapper Create(EntityHandle entity, AssemblyMetadata assemblyMetadata)
        {
            if (entity.IsNil)
            {
                return null;
            }

            switch (entity.Kind)
            {
                case HandleKind.EventDefinition:
                    return EventWrapper.Create((EventDefinitionHandle)entity, assemblyMetadata);
                case HandleKind.FieldDefinition:
                    return FieldWrapper.Create((FieldDefinitionHandle)entity, assemblyMetadata);
                case HandleKind.MethodDefinition:
                    return MethodWrapper.Create((MethodDefinitionHandle)entity, assemblyMetadata);
                case HandleKind.PropertyDefinition:
                    return PropertyWrapper.Create((PropertyDefinitionHandle)entity, assemblyMetadata);
                case HandleKind.TypeDefinition:
                    return TypeWrapper.Create((TypeDefinitionHandle)entity, assemblyMetadata);
                case HandleKind.MemberReference:
                    return MemberReferenceWrapper.Create((MemberReferenceHandle)entity, assemblyMetadata);
                case HandleKind.TypeSpecification:
                    var specification = TypeSpecificationWrapper.Create((TypeSpecificationHandle)entity, assemblyMetadata);
                    return specification.Type;
                case HandleKind.InterfaceImplementation:
                    return InterfaceImplementationWrapper.Create((InterfaceImplementationHandle)entity, assemblyMetadata);
                case HandleKind.TypeReference:
                    var typeWrapper = TypeReferenceWrapper.Create((TypeReferenceHandle)entity, assemblyMetadata);
                    return typeWrapper.Type;
            }

            return null;
        }
    }
}
