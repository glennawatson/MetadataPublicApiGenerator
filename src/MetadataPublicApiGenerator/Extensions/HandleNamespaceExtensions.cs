// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reflection.Metadata;

using MetadataPublicApiGenerator.Compilation;

namespace MetadataPublicApiGenerator.Extensions
{
    internal static class HandleNamespaceExtensions
    {
        public static string GetNamespace(this TypeDefinitionHandle handle, CompilationModule compilation)
        {
            return handle.Resolve(compilation).Name.GetNamespace(compilation);
        }

        public static string GetNamespace(this TypeDefinition handle, CompilationModule compilation)
        {
            return handle.Name.GetNamespace(compilation);
        }

        public static string GetNamespace(this MethodDefinitionHandle handle, CompilationModule compilation)
        {
            return handle.Resolve(compilation).Name.GetNamespace(compilation);
        }

        public static string GetNamespace(this MethodDefinition handle, CompilationModule compilation)
        {
            return handle.Name.GetNamespace(compilation);
        }

        public static string GetNamespace(this EventDefinitionHandle handle, CompilationModule compilation)
        {
            return handle.Resolve(compilation).Name.GetNamespace(compilation);
        }

        public static string GetNamespace(this EventDefinition handle, CompilationModule compilation)
        {
            return handle.Name.GetNamespace(compilation);
        }

        public static string GetNamespace(this FieldDefinitionHandle handle, CompilationModule compilation)
        {
            return handle.Resolve(compilation).Name.GetNamespace(compilation);
        }

        public static string GetNamespace(this FieldDefinition handle, CompilationModule compilation)
        {
            return handle.Name.GetNamespace(compilation);
        }

        public static string GetNamespace(this CustomAttributeHandle handle, CompilationModule compilation)
        {
            return handle.Resolve(compilation).GetNamespace(compilation);
        }

        public static string GetNamespace(this CustomAttribute handle, CompilationModule compilation)
        {
            return ((EntityHandle)handle.Constructor).GetNamespace(compilation);
        }

        public static string GetNamespace(this PropertyDefinitionHandle handle, CompilationModule compilation)
        {
            return handle.Resolve(compilation).GetNamespace(compilation);
        }

        public static string GetNamespace(this PropertyDefinition handle, CompilationModule compilation)
        {
            return handle.Name.GetNamespace(compilation);
        }

        public static string GetNamespace(this AssemblyDefinition handle, CompilationModule compilation)
        {
            return handle.Name.GetNamespace(compilation);
        }

        public static string GetNamespace(this GenericParameterHandle handle, CompilationModule compilation)
        {
            return handle.Resolve(compilation).GetNamespace(compilation);
        }

        public static string GetNamespace(this GenericParameter handle, CompilationModule compilation)
        {
            return handle.Name.GetNamespace(compilation);
        }

        public static string GetNamespace(this GenericParameterConstraintHandle handle, CompilationModule compilation)
        {
            return handle.Resolve(compilation).GetNamespace(compilation);
        }

        public static string GetNamespace(this GenericParameterConstraint handle, CompilationModule compilation)
        {
            return handle.Parameter.GetNamespace(compilation);
        }

        public static string GetNamespace(this NamespaceDefinitionHandle handle, CompilationModule compilation)
        {
            return handle.Resolve(compilation).GetNamespace(compilation);
        }

        public static string GetNamespace(this NamespaceDefinition handle, CompilationModule compilation)
        {
            return handle.Name.GetNamespace(compilation);
        }

        public static string GetNamespace(this TypeReferenceHandle handle, CompilationModule compilation)
        {
            return handle.Resolve(compilation).GetNamespace(compilation);
        }

        public static string GetNamespace(this TypeReference handle, CompilationModule compilation)
        {
            return handle.Name.GetNamespace(compilation);
        }

        public static string GetNamespace(this ParameterHandle handle, CompilationModule compilation)
        {
            return handle.Resolve(compilation).GetNamespace(compilation);
        }

        public static string GetNamespace(this Parameter handle, CompilationModule compilation)
        {
            return handle.Name.GetNamespace(compilation);
        }

        public static string GetNamespace(this MemberReferenceHandle handle, CompilationModule compilation)
        {
            return handle.Resolve(compilation).GetNamespace(compilation);
        }

        public static string GetNamespace(this MemberReference handle, CompilationModule compilation)
        {
            return handle.Name.GetNamespace(compilation);
        }

        public static string GetNamespace(this UserStringHandle handle, CompilationModule compilation)
        {
            return compilation.MetadataReader.GetUserString(handle);
        }

        public static string GetNamespace(this StringHandle handle, CompilationModule compilation)
        {
            return compilation.MetadataReader.GetString(handle);
        }

        public static string GetNamespace(this EntityHandle entity, CompilationModule module)
        {
            return ((Handle)entity).GetNamespace(module);
        }

        public static string GetNamespace(this Handle entity, CompilationModule module)
        {
            switch (entity.Kind)
            {
                case HandleKind.EventDefinition:
                    return ((EventDefinitionHandle)entity).GetNamespace(module);
                case HandleKind.FieldDefinition:
                    return ((FieldDefinitionHandle)entity).GetNamespace(module);
                case HandleKind.MethodDefinition:
                    return ((MethodDefinitionHandle)entity).GetNamespace(module);
                case HandleKind.PropertyDefinition:
                    return ((PropertyDefinitionHandle)entity).GetNamespace(module);
                case HandleKind.TypeDefinition:
                    return ((TypeDefinitionHandle)entity).GetNamespace(module);
                case HandleKind.GenericParameter:
                    return ((GenericParameterHandle)entity).GetNamespace(module);
                case HandleKind.GenericParameterConstraint:
                    return ((GenericParameterConstraintHandle)entity).GetNamespace(module);
                case HandleKind.NamespaceDefinition:
                    return ((NamespaceDefinitionHandle)entity).GetNamespace(module);
                case HandleKind.Parameter:
                    return ((ParameterHandle)entity).GetNamespace(module);
                case HandleKind.String:
                    return ((StringHandle)entity).GetNamespace(module);
                case HandleKind.UserString:
                    return ((UserStringHandle)entity).GetNamespace(module);
                case HandleKind.TypeReference:
                    return ((TypeReferenceHandle)entity).GetNamespace(module);
                case HandleKind.MemberReference:
                    return ((MemberReferenceHandle)entity).GetNamespace(module);
            }

            return null;
        }
    }
}
