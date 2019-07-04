// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using MetadataPublicApiGenerator.Compilation;
using MetadataPublicApiGenerator.Compilation.TypeWrappers;

using Microsoft.CodeAnalysis;

namespace MetadataPublicApiGenerator.Extensions
{
    internal static class HandleNameExtensions
    {
        private static readonly ConcurrentDictionary<CompilationModule, ConcurrentDictionary<StringHandle, string>> _stringHandleNames = new ConcurrentDictionary<CompilationModule, ConcurrentDictionary<StringHandle, string>>();

        public static string GetName(this TypeDefinitionHandle handle, CompilationModule compilation)
        {
            return handle.Resolve(compilation).Name.GetName(compilation);
        }

        public static string GetName(this TypeDefinition handle, CompilationModule compilation)
        {
            return handle.Name.GetName(compilation);
        }

        public static string GetName(this MethodDefinitionHandle handle, CompilationModule compilation)
        {
            return handle.Resolve(compilation).Name.GetName(compilation);
        }

        public static string GetName(this MethodDefinition handle, CompilationModule compilation)
        {
            return handle.Name.GetName(compilation);
        }

        public static string GetName(this EventDefinitionHandle handle, CompilationModule compilation)
        {
            return handle.Resolve(compilation).Name.GetName(compilation);
        }

        public static string GetName(this EventDefinition handle, CompilationModule compilation)
        {
            return handle.Name.GetName(compilation);
        }

        public static string GetName(this FieldDefinitionHandle handle, CompilationModule compilation)
        {
            return handle.Resolve(compilation).Name.GetName(compilation);
        }

        public static string GetName(this FieldDefinition handle, CompilationModule compilation)
        {
            return handle.Name.GetName(compilation);
        }

        public static string GetName(this CustomAttributeHandle handle, CompilationModule compilation)
        {
            return handle.Resolve(compilation).GetName(compilation);
        }

        public static string GetName(this CustomAttribute handle, CompilationModule compilation)
        {
            return ((EntityHandle)handle.Constructor).GetName(compilation);
        }

        public static string GetName(this PropertyDefinitionHandle handle, CompilationModule compilation)
        {
            return handle.Resolve(compilation).GetName(compilation);
        }

        public static string GetName(this PropertyDefinition handle, CompilationModule compilation)
        {
            return handle.Name.GetName(compilation);
        }

        public static string GetName(this AssemblyDefinition handle, CompilationModule compilation)
        {
            return handle.Name.GetName(compilation);
        }

        public static string GetName(this GenericParameterHandle handle, CompilationModule compilation)
        {
            return handle.Resolve(compilation).GetName(compilation);
        }

        public static string GetName(this GenericParameter handle, CompilationModule compilation)
        {
            return handle.Name.GetName(compilation);
        }

        public static string GetName(this GenericParameterConstraintHandle handle, CompilationModule compilation)
        {
            return handle.Resolve(compilation).GetName(compilation);
        }

        public static string GetName(this GenericParameterConstraint handle, CompilationModule compilation)
        {
            return handle.Parameter.GetName(compilation);
        }

        public static string GetName(this NamespaceDefinitionHandle handle, CompilationModule compilation)
        {
            return handle.Resolve(compilation).GetName(compilation);
        }

        public static string GetName(this NamespaceDefinition handle, CompilationModule compilation)
        {
            return handle.Name.GetName(compilation);
        }

        public static string GetName(this TypeReferenceHandle handle, CompilationModule compilation)
        {
            return handle.Resolve(compilation).GetName(compilation);
        }

        public static string GetName(this TypeReference handle, CompilationModule compilation)
        {
            return handle.Name.GetName(compilation);
        }

        public static string GetName(this ParameterHandle handle, CompilationModule compilation)
        {
            return handle.Resolve(compilation).GetName(compilation);
        }

        public static string GetName(this Parameter handle, CompilationModule compilation)
        {
            return handle.Name.GetName(compilation);
        }

        public static string GetName(this MemberReferenceHandle handle, CompilationModule compilation)
        {
            return handle.Resolve(compilation).GetName(compilation);
        }

        public static string GetName(this MemberReference handle, CompilationModule compilation)
        {
            return handle.Name.GetName(compilation);
        }

        public static string GetName(this UserStringHandle handle, CompilationModule compilation)
        {
            return compilation.MetadataReader.GetUserString(handle);
        }

        public static string GetName(this StringHandle handle, CompilationModule compilation)
        {
            var map = _stringHandleNames.GetOrAdd(compilation, _ => new ConcurrentDictionary<StringHandle, string>());

            return map.GetOrAdd(handle, stringHandle => compilation.MetadataReader.GetString(stringHandle));
        }

        public static string GetName(this EntityHandle entity, CompilationModule module)
        {
            return ((Handle)entity).GetName(module);
        }

        public static string GetName(this Handle entity, CompilationModule module)
        {
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
                    return ((TypeDefinitionHandle)entity).GetName(module);
                case HandleKind.GenericParameter:
                    return ((GenericParameterHandle)entity).GetName(module);
                case HandleKind.GenericParameterConstraint:
                    return ((GenericParameterConstraintHandle)entity).GetName(module);
                case HandleKind.NamespaceDefinition:
                    return ((NamespaceDefinitionHandle)entity).GetName(module);
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
        }

        public static string GetFullName(this EntityHandle handle, CompilationModule module)
        {
            if (handle.IsNil)
            {
                return null;
            }

            switch (handle.Kind)
            {
                case HandleKind.TypeReference:
                    return GetFullName((TypeReferenceHandle)handle, module);
                case HandleKind.TypeDefinition:
                    return GetFullName((TypeDefinitionHandle)handle, module);
                case HandleKind.MemberReference:
                    return GetFullName((MemberReferenceHandle)handle, module);
                case HandleKind.MethodDefinition:
                    return GetFullName((MethodDefinitionHandle)handle, module);
            }

            return null;
        }

        public static string GetFullName(this CustomAttribute attribute, CompilationModule compilation)
        {
            var attributeType = attribute.GetAttributeType(compilation);

            return attributeType.GetFullName(compilation);
        }

        public static string GetFullName(this TypeDefinitionHandle typeDefinitionHandle, CompilationModule module)
        {
            var typeDefinition = typeDefinitionHandle.Resolve(module);

            return typeDefinition.GetFullName(module);
        }

        public static string GetFullName(this TypeDefinition typeDefinition, CompilationModule module)
        {
            var reader = module.MetadataReader;

            var declaringType = typeDefinition.GetDeclaringType();

            var stringBuilder = new StringBuilder();
            if (declaringType.IsNil)
            {
                if (!typeDefinition.Namespace.IsNil)
                {
                    var namespaceName = reader.GetString(typeDefinition.Namespace);
                    if (!string.IsNullOrEmpty(namespaceName))
                    {
                        stringBuilder.Append(namespaceName).Append('.');
                    }
                }

                stringBuilder.Append(reader.GetString(typeDefinition.Name));
            }
            else
            {
                stringBuilder.Append(GetFullName(declaringType, module)).Append('.')
                    .Append(reader.GetString(typeDefinition.Name));
            }

            return stringBuilder.ToString();
        }

        public static string GetFullName(this MemberReferenceHandle handle, CompilationModule module)
        {
            var memberReferenceHandle = handle.Resolve(module);

            return memberReferenceHandle.GetFullName(module);
        }

        public static string GetFullName(this MemberReference memberReference, CompilationModule module)
        {
            var reader = module.MetadataReader;
            var stringBuilder = new StringBuilder();

            var list = new List<string>();
            var current = memberReference.Parent;
            while (!current.IsNil)
            {
                var name = current.GetFullName(module);

                if (!string.IsNullOrWhiteSpace(name))
                {
                    list.Insert(0, name);
                }

                current = current.Kind == HandleKind.MemberReference ?
                    ((MemberReferenceHandle)current).Resolve(module).Parent :
                    default;
            }

            if (list.Count > 0)
            {
                stringBuilder.Append(string.Join(".", list)).Append('.');
            }

            stringBuilder.Append(reader.GetString(memberReference.Name));

            return stringBuilder.ToString();
        }

        public static string GetFullName(this MethodDefinitionHandle handle, CompilationModule module)
        {
            var memberReferenceHandle = handle.Resolve(module);

            return memberReferenceHandle.GetFullName(module);
        }

        public static string GetFullName(this MethodDefinition memberReference, CompilationModule module)
        {
            var reader = module.MetadataReader;
            var stringBuilder = new StringBuilder();

            stringBuilder.Append(reader.GetString(memberReference.Name));

            return stringBuilder.ToString();
        }

        public static string GetFullName(this TypeReferenceHandle typeReferenceHandle, CompilationModule module)
        {
            return typeReferenceHandle.Resolve(module).GetFullName(module);
        }

        public static string GetFullName(this TypeReference typeReference, CompilationModule module)
        {
            var reader = module.MetadataReader;
            var stringBuilder = new StringBuilder();
            var namespaceName = reader.GetString(typeReference.Namespace);

            if (!string.IsNullOrEmpty(namespaceName))
            {
                stringBuilder.Append(namespaceName).Append('.');
            }

            var list = new List<string>();
            var current = typeReference.ResolutionScope;
            while (!current.IsNil)
            {
                var name = current.GetFullName(module);

                if (!string.IsNullOrWhiteSpace(name))
                {
                    list.Insert(0, name);
                }

                current = current.Kind == HandleKind.TypeReference ?
                    ((TypeReferenceHandle)current).Resolve(module).ResolutionScope :
                    default;
            }

            if (list.Count > 0)
            {
                stringBuilder.Append(string.Join(".", list)).Append('.');
            }

            stringBuilder.Append(reader.GetString(typeReference.Name));

            return stringBuilder.ToString();
        }

        /// <summary>
        /// Gets a string form of the type and generic arguments for a type.
        /// </summary>
        /// <param name="typeHandle">The type to generate the arguments for.</param>
        /// <param name="compilation">The compilation information source.</param>
        /// <returns>A type descriptor including the generic arguments.</returns>
        public static string GenerateFullGenericName(this TypeSpecificationHandle typeHandle, CompilationModule compilation)
        {
            return GenerateFullGenericName(typeHandle.Resolve(compilation), compilation);
        }

        /// <summary>
        /// Gets a string form of the type and generic arguments for a type.
        /// </summary>
        /// <param name="typeSpecification">The type to generate the arguments for.</param>
        /// <param name="compilation">The compilation information source.</param>
        /// <returns>A type descriptor including the generic arguments.</returns>
        public static string GenerateFullGenericName(this TypeSpecification typeSpecification, CompilationModule compilation)
        {
            return typeSpecification.DecodeSignature(compilation.TypeProvider, new GenericContext(compilation))?.FullName;
        }

        /// <summary>
        /// Gets a string form of the type and generic arguments for a type.
        /// </summary>
        /// <param name="typeHandle">The type to generate the arguments for.</param>
        /// <param name="compilation">The compilation information source.</param>
        /// <returns>A type descriptor including the generic arguments.</returns>
        public static string GenerateFullGenericName(this TypeDefinitionHandle typeHandle, CompilationModule compilation)
        {
            return GenerateFullGenericName(typeHandle.Resolve(compilation), compilation);
        }

        /// <summary>
        /// Gets a string form of the type and generic arguments for a type.
        /// </summary>
        /// <param name="type">The type to generate the arguments for.</param>
        /// <param name="compilation">The compilation information source.</param>
        /// <returns>A type descriptor including the generic arguments.</returns>
        public static string GenerateFullGenericName(this TypeDefinition type, CompilationModule compilation)
        {
            var sb = new StringBuilder(type.GetRealTypeName(compilation));

            if (type.GetGenericParameters().Count > 0)
            {
                sb.Append("<")
                    .Append(string.Join(", ", type.GetGenericParameters().Select(x => GenerateFullGenericName(x, compilation))))
                    .Append(">");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Gets a string form of the type and generic arguments for a type.
        /// </summary>
        /// <param name="parameterHandle">The type to generate the arguments for.</param>
        /// <param name="compilation">The compilation information source.</param>
        /// <returns>A type descriptor including the generic arguments.</returns>
        public static string GenerateFullGenericName(this GenericParameterHandle parameterHandle, CompilationModule compilation)
        {
            var parameter = parameterHandle.Resolve(compilation);

            return parameter.GetName(compilation);
        }

        public static string GetRealTypeName(this TypeDefinitionHandle typeHandle, CompilationModule compilation)
        {
            return GetRealTypeName(typeHandle.Resolve(compilation), compilation);
        }

        public static string GetRealTypeName(this TypeDefinition type, CompilationModule compilation)
        {
            var typeCode = type.IsKnownType(compilation);

            if (typeCode == KnownTypeCode.Array)
            {
                ////var arrayType = type.
                ////var elementType = arrayType.ElementType;

                ////return elementType.GenerateFullGenericName(compilation) + "[]";

                return null;
            }

            return typeCode.GetRealTypeName() ?? type.GetFullName(compilation);
        }

        public static string GetRealTypeName(this KnownTypeCode typeCode)
        {
            switch (typeCode)
            {
                case KnownTypeCode.Boolean:
                    return "bool";
                case KnownTypeCode.Byte:
                    return "byte";
                case KnownTypeCode.Char:
                    return "char";
                case KnownTypeCode.Decimal:
                    return "decimal";
                case KnownTypeCode.Double:
                    return "double";
                case KnownTypeCode.Int16:
                    return "short";
                case KnownTypeCode.Int32:
                    return "int";
                case KnownTypeCode.Int64:
                    return "long";
                case KnownTypeCode.SByte:
                    return "sbyte";
                case KnownTypeCode.Single:
                    return "single";
                case KnownTypeCode.String:
                    return "string";
                case KnownTypeCode.UInt16:
                    return "ushort";
                case KnownTypeCode.UInt32:
                    return "uint";
                case KnownTypeCode.UInt64:
                    return "ulong";
                default:
                    return null;
            }
        }
    }
}