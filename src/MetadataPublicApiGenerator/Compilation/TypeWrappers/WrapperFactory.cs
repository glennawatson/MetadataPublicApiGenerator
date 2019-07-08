// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Text;

using MetadataPublicApiGenerator.Extensions;

namespace MetadataPublicApiGenerator.Compilation.TypeWrappers
{
    internal static class WrapperFactory
    {
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
                case HandleKind.TypeReference:
                    return TypeReferenceWrapper.Create((TypeReferenceHandle)entity, module);
                case HandleKind.MemberReference:
                    return MemberReferenceWrapper.Create((MemberReferenceHandle)entity, module);
                case HandleKind.TypeSpecification:
                    return TypeSpecificationWrapper.Create((TypeSpecificationHandle)entity, module);
            }

            return null;
        }
    }
}
