// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Text;

namespace MetadataPublicApiGenerator.Compilation.TypeWrappers
{
    internal static class WrapperFactory
    {
        public static ITypeNamedWrapper Create(EntityHandle entity, CompilationModule module)
        {
            if (handle.IsNil)
            {
                return null;
            }

            switch (handle.Kind)
            {
            }

            switch (entity.Kind)
            {
                case HandleKind.EventDefinition:
                    return ((EventDefinitionHandle)entity).GetName(module);
                case HandleKind.FieldDefinition:
                    return ((FieldDefinitionHandle)entity).GetName(module);
                case HandleKind.MethodDefinition:
                    return ((MethodDefinitionHandle)entity).GetName(module);
                case HandleKind.PropertyDefinition:
                    return ((PropertyDefinitionHandle)entity).GetName(module);
                case HandleKind.TypeDefinition:
                    return TypeWrapper.Create((TypeDefinitionHandle)handle, module);
                case HandleKind.GenericParameter:
                    return ((GenericParameterHandle)entity).GetName(module);
                case HandleKind.GenericParameterConstraint:
                    return ((GenericParameterConstraintHandle)entity).GetName(module);
                case HandleKind.Parameter:
                    return ((ParameterHandle)entity).GetName(module);
                case HandleKind.String:
                    return ((StringHandle)entity).GetName(module);
                case HandleKind.UserString:
                    return ((UserStringHandle)entity).GetName(module);
                case HandleKind.TypeReference:
                    return ((TypeReferenceHandle)entity).GetName(module);
                case HandleKind.MemberReference:
                    return ((MemberReferenceHandle)entity).GetName(module);
            }

            return null;

            return null;
        }
    }
}
