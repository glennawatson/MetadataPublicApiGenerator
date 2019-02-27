// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using MetadataPublicApiGenerator.Compilation;
using MetadataPublicApiGenerator.Compilation.TypeWrappers;
using Microsoft.CodeAnalysis;

namespace MetadataPublicApiGenerator.Extensions
{
    internal static class ReflectionMetadataExtensions
    {
        private static readonly ConcurrentDictionary<ICompilation, ImmutableDictionary<string, ImmutableList<(CompilationModule, TypeDefinitionHandle)>>> _typeNameMapping
            = new ConcurrentDictionary<ICompilation, ImmutableDictionary<string, ImmutableList<(CompilationModule, TypeDefinitionHandle)>>>();

        /// <summary>
        /// Gets type definitions matching the full name and in the reference and main libraries.
        /// </summary>
        /// <param name="compilation">The compilation to scan.</param>
        /// <param name="name">The name of the item to get.</param>
        /// <returns>The name of the items.</returns>
        public static IReadOnlyCollection<(CompilationModule module, TypeDefinitionHandle typeDefinitionHandle)> GetTypeDefinitionByName(this ICompilation compilation, string name)
        {
            void GetTypeMappings(CompilationModule module, Dictionary<string, List<(CompilationModule, TypeDefinitionHandle)>> list)
            {
                var reader = module.MetadataReader;
                foreach (var typeDefinition in module.PublicTypeDefinitionHandles)
                {
                    var typeName = reader.GetString(typeDefinition.Resolve(module).Name);
                    if (!list.TryGetValue(typeName, out var listCurrent))
                    {
                        listCurrent = new List<(CompilationModule, TypeDefinitionHandle)>();
                        list[typeName] = listCurrent;
                    }

                    listCurrent.Add((module, typeDefinition));
                }
            }

            var map = _typeNameMapping.GetOrAdd(compilation, comp =>
            {
                var list = new Dictionary<string, List<(CompilationModule, TypeDefinitionHandle)>>();
                GetTypeMappings(comp.MainModule, list);
                foreach (var subModule in comp.ReferencedModules)
                {
                    GetTypeMappings(subModule, list);
                }

                return list.ToImmutableDictionary(key => key.Key, value => value.Value.ToImmutableList());
            });

            return map.GetValueOrDefault(name);
        }

        public static bool IsValueType(this TypeDefinitionHandle handle, CompilationModule reader)
        {
            return handle.Resolve(reader).IsValueType(reader);
        }

        public static bool IsValueType(this TypeDefinition typeDefinition, CompilationModule reader)
        {
            Handle baseType = typeDefinition.GetBaseTypeOrNil();
            if (baseType.IsNil)
            {
                return false;
            }

            if (baseType.IsKnownType(reader) == KnownTypeCode.Enum)
            {
                return true;
            }

            if (baseType.IsKnownType(reader) != KnownTypeCode.ValueType)
            {
                return false;
            }

            return false;
        }

        public static bool IsDelegate(this TypeDefinitionHandle handle, CompilationModule reader)
        {
            return handle.Resolve(reader).IsDelegate(reader);
        }

        public static bool IsDelegate(this TypeDefinition typeDefinition, CompilationModule reader)
        {
            Handle baseType = typeDefinition.GetBaseTypeOrNil();
            var knownType = baseType.IsKnownType(reader);
            return !baseType.IsNil && knownType == KnownTypeCode.MulticastDelegate;
        }

        public static bool IsEnum(this TypeDefinitionHandle handle, CompilationModule module)
        {
            return handle.Resolve(module).IsEnum(module);
        }

        public static bool IsEnum(this TypeDefinition typeDefinition, CompilationModule module)
        {
            var baseType = (TypeDefinitionHandle)typeDefinition.GetBaseTypeOrNil();
            if (baseType.IsNil)
            {
                return false;
            }

            return baseType.IsKnownType(module) == KnownTypeCode.Enum;
        }

        public static bool IsEnum(this TypeDefinitionHandle handle, CompilationModule module, out PrimitiveTypeCode underlyingType)
        {
            return handle.Resolve(module).IsEnum(module, out underlyingType);
        }

        public static bool IsEnum(this TypeDefinition typeDefinition, CompilationModule module, out PrimitiveTypeCode underlyingType)
        {
            underlyingType = 0;
            var baseType = (TypeDefinitionHandle)typeDefinition.GetBaseTypeOrNil();
            if (baseType.IsNil)
            {
                return false;
            }

            if (baseType.IsKnownType(module) != KnownTypeCode.Enum)
            {
                return false;
            }

            var field = module.MetadataReader.GetFieldDefinition(typeDefinition.GetFields().First());
            var blob = module.MetadataReader.GetBlobReader(field.Signature);
            if (blob.ReadSignatureHeader().Kind != SignatureKind.Field)
            {
                return false;
            }

            underlyingType = (PrimitiveTypeCode)blob.ReadByte();
            return true;
        }

        public static EntityHandle GetBaseTypeOrNil(this TypeDefinition definition)
        {
            try
            {
                return definition.BaseType;
            }
            catch (BadImageFormatException)
            {
                return default;
            }
        }

        public static bool HasKnownAttribute(this CustomAttributeHandleCollection customAttributes, CompilationModule metadata, KnownAttribute type)
        {
            foreach (var customAttribute in customAttributes.Select(x => x.Resolve(metadata)))
            {
                if (customAttribute.IsKnownAttribute(metadata, type))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsKnownAttribute(this CustomAttribute attr, CompilationModule metadata, KnownAttribute attrType)
        {
            return attr.IsKnownAttributeType(metadata) == attrType;
        }

        public static CustomAttributeHandleCollection? GetEntityCustomAttributes(this Handle entity, CompilationModule module)
        {
            switch (entity.Kind)
            {
                case HandleKind.EventDefinition:
                    var eventDefinition = ((EventDefinitionHandle)entity).Resolve(module);
                    return eventDefinition.GetCustomAttributes();
                case HandleKind.FieldDefinition:
                    var field = ((FieldDefinitionHandle)entity).Resolve(module);
                    return field.GetCustomAttributes();
                case HandleKind.MethodDefinition:
                    var method = ((MethodDefinitionHandle)entity).Resolve(module);
                    return method.GetCustomAttributes();
                case HandleKind.PropertyDefinition:
                    var property = ((PropertyDefinitionHandle)entity).Resolve(module);
                    return property.GetCustomAttributes();
                case HandleKind.TypeDefinition:
                    var typeDefinition = ((TypeDefinitionHandle)entity).Resolve(module);
                    return typeDefinition.GetCustomAttributes();
            }

            return null;
        }

        public static bool IsEntityPublic(this Handle entity, CompilationModule module)
        {
            switch (entity.Kind)
            {
                case HandleKind.EventDefinition:
                    var eventDefinition = ((EventDefinitionHandle)entity).Resolve(module);
                    var accessor = eventDefinition.GetAnyAccessor();

                    if (accessor.IsNil)
                    {
                        return false;
                    }

                    return (accessor.Resolve(module).Attributes & MethodAttributes.Public) != 0;
                case HandleKind.CustomAttribute:
                    var customAttribute = ((CustomAttributeHandle)entity).Resolve(module);
                    if (customAttribute.Constructor.IsNil)
                    {
                        return false;
                    }

                    return (((MethodDefinitionHandle)customAttribute.Constructor).Resolve(module).Attributes & MethodAttributes.Public) != 0;
                case HandleKind.FieldDefinition:
                    var field = ((FieldDefinitionHandle)entity).Resolve(module);
                    return (field.Attributes & FieldAttributes.Public) != 0;
                case HandleKind.MethodDefinition:
                    var method = ((MethodDefinitionHandle)entity).Resolve(module);

                    return (method.Attributes & MethodAttributes.Public) != 0;
                case HandleKind.PropertyDefinition:
                    var property = ((PropertyDefinitionHandle)entity).Resolve(module);

                    return IsEntityPublic(property.GetAccessors().Getter, module) || IsEntityPublic(property.GetAccessors().Setter, module);
                case HandleKind.TypeDefinition:
                    var typeDefinition = ((TypeDefinitionHandle)entity).Resolve(module);

                    return (typeDefinition.Attributes & TypeAttributes.Public) != 0;
            }

            return false;
        }

        public static MethodDefinition GetDelegateInvokeMethod(this TypeDefinition type, CompilationModule module)
        {
            var handle = type.GetMethods().FirstOrDefault(x => x.Resolve(module).GetName(module) == "Invoke");

            if (handle.IsNil)
            {
                throw new Exception("Cannot find Invoke method for delegate.");
            }

            return handle.Resolve(module);
        }

        public static MethodDefinitionHandle GetAnyAccessor(this EventDefinition eventDefinition)
        {
            var accessors = eventDefinition.GetAccessors();

            if (!accessors.Adder.IsNil)
            {
                return accessors.Adder;
            }

            if (!accessors.Remover.IsNil)
            {
                return accessors.Remover;
            }

            return accessors.Raiser;
        }

        public static object ReadConstant(this ConstantHandle constantHandle, CompilationModule module)
        {
            if (constantHandle.IsNil)
            {
                return null;
            }

            var constant = constantHandle.Resolve(module);
            var blobReader = module.MetadataReader.GetBlobReader(constant.Value);
            try
            {
                return blobReader.ReadConstant(constant.TypeCode);
            }
            catch (ArgumentOutOfRangeException)
            {
                throw new BadImageFormatException($"Constant with invalid type code: {constant.TypeCode}");
            }
        }

        public static (EntityHandle owner, MethodKind symbolKind) GetMethodSymbolKind(this MethodDefinitionHandle handle, CompilationModule module)
        {
            var (accessorOwner, semanticsAttribute) = module.MethodSemanticsLookup.GetSemantics(handle);

            var methodDefinition = handle.Resolve(module);
            var name = methodDefinition.GetName(module);
            var parameterCount = methodDefinition.GetParameters().Count;

            const MethodAttributes finalizerAttributes = MethodAttributes.Virtual | MethodAttributes.Family | MethodAttributes.HideBySig;

            if ((semanticsAttribute & MethodSemanticsAttributes.Adder) != 0)
            {
                return (accessorOwner, MethodKind.EventAdd);
            }

            if ((semanticsAttribute & MethodSemanticsAttributes.Remover) != 0)
            {
                return (accessorOwner, MethodKind.EventRemove);
            }

            if ((semanticsAttribute & MethodSemanticsAttributes.Raiser) != 0)
            {
                return (accessorOwner, MethodKind.EventRaise);
            }

            if ((semanticsAttribute & MethodSemanticsAttributes.Getter) != 0)
            {
                return (accessorOwner, MethodKind.PropertyGet);
            }

            if ((semanticsAttribute & MethodSemanticsAttributes.Setter) != 0)
            {
                return (accessorOwner, MethodKind.PropertySet);
            }

            var attributes = methodDefinition.Attributes;

            if ((attributes & (MethodAttributes.SpecialName | MethodAttributes.RTSpecialName)) != 0)
            {
                if (name == ".cctor" || name == ".ctor")
                {
                    return (default, MethodKind.Constructor);
                }

                if (name.StartsWith("op_", StringComparison.Ordinal))
                {
                    return (default, MethodKind.UserDefinedOperator);
                }
            }
            else if ((attributes & finalizerAttributes) == finalizerAttributes)
            {
                if (name == "Finalize" && parameterCount == 0)
                {
                    return (default, MethodKind.Destructor);
                }
            }

            return (default, MethodKind.Ordinary);
        }

        public static MethodSignature<IWrapper> DecodeSignature(this PropertyDefinitionHandle handle, CompilationModule module)
        {
            var genericContext = new GenericContext(module, handle);
            var instance = handle.Resolve(module);
            return instance.DecodeSignature(new TypeProvider(module.Compilation), genericContext);
        }

        public static MethodSignature<IWrapper> DecodeSignature(this MethodDefinitionHandle handle, CompilationModule module)
        {
            var genericContext = new GenericContext(module, handle);
            var instance = handle.Resolve(module);
            return instance.DecodeSignature(new TypeProvider(module.Compilation), genericContext);
        }
    }
}
