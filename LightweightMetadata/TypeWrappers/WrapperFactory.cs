// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reflection.Metadata;

namespace LightweightMetadata.TypeWrappers
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
        /// <param name="module">The module hosting the handle.</param>
        /// <returns>A wrapper or null if one cannot be created.</returns>
        public static IHandleTypeNamedWrapper Create(EntityHandle entity, CompilationModule module)
        {
            if (entity.IsNil)
            {
                return null;
            }

            switch (entity.Kind)
            {
                case HandleKind.EventDefinition:
                    return EventWrapper.Create((EventDefinitionHandle)entity, module);
                case HandleKind.FieldDefinition:
                    return FieldWrapper.Create((FieldDefinitionHandle)entity, module);
                case HandleKind.MethodDefinition:
                    return MethodWrapper.Create((MethodDefinitionHandle)entity, module);
                case HandleKind.PropertyDefinition:
                    return PropertyWrapper.Create((PropertyDefinitionHandle)entity, module);
                case HandleKind.TypeDefinition:
                    return TypeWrapper.Create((TypeDefinitionHandle)entity, module);
                case HandleKind.MemberReference:
                    return MemberReferenceWrapper.Create((MemberReferenceHandle)entity, module);
                case HandleKind.TypeSpecification:
                    return TypeSpecificationWrapper.Create((TypeSpecificationHandle)entity, module);
                case HandleKind.InterfaceImplementation:
                    return InterfaceImplementationWrapper.Create((InterfaceImplementationHandle)entity, module);
                case HandleKind.TypeReference:
                {
                    var current = TypeReferenceWrapper.Create((TypeReferenceHandle)entity, module).ResolutionScope;

                    while (current is TypeReferenceWrapper child)
                    {
                        current = child.ResolutionScope;
                    }

                    return current;
                }
            }

            return null;
        }
    }
}
