// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using MetadataPublicApiGenerator.Compilation;

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
            return ((MethodDefinitionHandle)handle.Constructor).GetName(compilation);
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

        public static string GetName(this ParameterHandle handle, CompilationModule compilation)
        {
            return handle.Resolve(compilation).GetName(compilation);
        }

        public static string GetName(this Parameter handle, CompilationModule compilation)
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
            }

            return null;
        }

        public static string GetFullName(this TypeDefinitionHandle entity, CompilationModule module)
        {
            return entity.Resolve(module).GetFullName(module);
        }

        public static string GetFullName(this TypeDefinition handle, CompilationModule compilation)
        {
            return handle.Namespace.GetName(compilation) + "." + handle.GenerateFullGenericName(compilation);
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
            var typeName = type.GetName(compilation);

            var typeCode = type.IsKnownType(compilation);

            if (typeCode == KnownTypeCode.Array)
            {
                ////var arrayType = type.
                ////var elementType = arrayType.ElementType;

                ////return elementType.GenerateFullGenericName(compilation) + "[]";

                return null;
            }

            return typeCode.GetRealTypeName() ?? typeName;
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