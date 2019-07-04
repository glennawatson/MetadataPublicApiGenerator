// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Text;

using MetadataPublicApiGenerator.Compilation;

namespace MetadataPublicApiGenerator.Extensions
{
    internal static class AttributeExtensions
    {
        public static EntityHandle GetAttributeType(this CustomAttribute attribute, CompilationModule module)
        {
            var reader = module.MetadataReader;

            switch (attribute.Constructor.Kind)
            {
                case HandleKind.MethodDefinition:
                    var methodDefinition = reader.GetMethodDefinition((MethodDefinitionHandle)attribute.Constructor);
                    return methodDefinition.GetDeclaringType();
                case HandleKind.MemberReference:
                    var memberReference = reader.GetMemberReference((MemberReferenceHandle)attribute.Constructor);
                    return memberReference.Parent;
                default:
                    throw new BadImageFormatException("Unexpected token kind for attribute constructor: " + attribute.Constructor.Kind);
            }
        }
    }
}
