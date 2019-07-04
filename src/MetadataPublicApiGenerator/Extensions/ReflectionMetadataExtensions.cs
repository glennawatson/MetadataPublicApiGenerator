// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
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
        private static readonly Dictionary<ICompilation, Dictionary<string, IReadOnlyList<(CompilationModule, TypeWrapper)>>> _typeNameMapping
            = new Dictionary<ICompilation, Dictionary<string, IReadOnlyList<(CompilationModule, TypeWrapper)>>>();

        /// <summary>
        /// Gets type definitions matching the full name and in the reference and main libraries.
        /// </summary>
        /// <param name="compilation">The compilation to scan.</param>
        /// <param name="name">The name of the item to get.</param>
        /// <returns>The name of the items.</returns>
        public static IReadOnlyList<(CompilationModule module, TypeWrapper typeWrapper)> GetTypeDefinitionByName(this ICompilation compilation, string name)
        {
            void GetTypeMappings(CompilationModule module, Dictionary<string, List<(CompilationModule, TypeWrapper)>> list)
            {
                foreach (var typeDefinitionHandle in module.PublicTypes)
                {
                    var fullName = typeDefinitionHandle.FullName;
                    if (!list.TryGetValue(fullName, out var listCurrent))
                    {
                        listCurrent = new List<(CompilationModule, TypeWrapper)>();
                        list[fullName] = listCurrent;
                    }

                    listCurrent.Add((module, typeDefinitionHandle));
                }
            }

            var map = _typeNameMapping.GetOrAdd(compilation, comp =>
            {
                var list = new Dictionary<string, List<(CompilationModule, TypeWrapper)>>();
                GetTypeMappings(comp.MainModule, list);
                foreach (var subModule in comp.ReferencedModules)
                {
                    GetTypeMappings(subModule, list);
                }

                var returnValue = list.ToDictionary(key => key.Key, value => (IReadOnlyList<(CompilationModule, TypeWrapper)>)value.Value);

                File.WriteAllLines("knownmodules.txt", returnValue.Values.SelectMany(x => x.Select(y => y.Item1.FileName)).Distinct().OrderBy(x => x));
                File.WriteAllLines("knowntypes.txt", returnValue.Keys.OrderBy(x => x).ToList());

                return returnValue;
            });

            return map.GetValueOrDefault(name) ?? Array.Empty<(CompilationModule, TypeWrapper)>();
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
            if (entity.IsNil)
            {
                return false;
            }

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

        public static MethodSignature<ITypeNamedWrapper>? DecodeSignature(this PropertyDefinitionHandle handle, CompilationModule module, GenericContext genericContext = default)
        {
            var instance = handle.Resolve(module);
            return instance.DecodeSignature(new TypeProvider(module.Compilation), genericContext ?? new GenericContext(module, handle));
        }

        public static MethodSignature<ITypeNamedWrapper>? DecodeSignature(this MethodDefinitionHandle handle, CompilationModule module, GenericContext genericContext = default)
        {
            var instance = handle.Resolve(module);
            return instance.DecodeSignature(new TypeProvider(module.Compilation), genericContext ?? new GenericContext(module, handle));
        }

        public static MethodSignature<ITypeNamedWrapper>? DecodeSignature(this MethodSpecificationHandle handle, CompilationModule module, GenericContext genericContext = default)
        {
            var instance = handle.Resolve(module);
            var methodTypeArgs = instance.DecodeSignature(new TypeProvider(module.Compilation), genericContext ?? new GenericContext(module, handle));

            if (instance.Method.Kind == HandleKind.MethodDefinition)
            {
                return ((MethodDefinitionHandle)instance.Method).DecodeSignature(module, genericContext);
            }

            return ((MemberReferenceHandle)instance.Method).DecodeSignature(module, genericContext);
        }

        public static MethodSignature<ITypeNamedWrapper>? DecodeSignature(this MemberReferenceHandle handle, CompilationModule module, GenericContext context = default)
        {
            var reference = handle.Resolve(module);

            if (reference.GetKind() != MemberReferenceKind.Method)
            {
                throw new ArgumentException("Must be a valid method reference", nameof(handle));
            }

            if (reference.Parent.Kind == HandleKind.MethodDefinition)
            {
                return DecodeSignature((MethodDefinitionHandle)reference.Parent, module, context);
            }

            if (reference.Parent.IsNil)
            {
                return null;
            }

            ITypeWrapper typeWrapper;
            switch (reference.Parent.Kind)
            {
                case HandleKind.TypeDefinition:
                    typeWrapper = module.TypeProvider.GetTypeFromDefinition(module.MetadataReader, (TypeDefinitionHandle)reference.Parent, 0) as ITypeWrapper;
                    break;
                case HandleKind.TypeReference:
                    typeWrapper = module.TypeProvider.GetTypeFromReference(module.MetadataReader, (TypeReferenceHandle)reference.Parent, 0) as ITypeWrapper;
                    break;
                case HandleKind.TypeSpecification:
                    var typeSpec = ((TypeSpecificationHandle)reference.Parent).Resolve(module);
                    var genericContext = new GenericContext(module, reference.Parent);
                    typeWrapper = typeSpec.DecodeSignature(module.TypeProvider, genericContext) as ITypeWrapper;
                    break;
                default:
                    throw new BadImageFormatException("Not a type handle");
            }

            return reference.DecodeMethodSignature(module.TypeProvider, new GenericContext(module, typeWrapper.Handle));
        }

        public static MethodSignature<ITypeNamedWrapper>? DecodeSignature(this EntityHandle handle, CompilationModule compilation, GenericContext context = default)
        {
            if (handle.IsNil)
            {
                throw new ArgumentNullException(nameof(handle));
            }

            switch (handle.Kind)
            {
                case HandleKind.MethodDefinition:
                    return ((MethodDefinitionHandle)handle).DecodeSignature(compilation, context);
                case HandleKind.MemberReference:
                    return ((MemberReferenceHandle)handle).DecodeSignature(compilation, context);
                case HandleKind.MethodSpecification:
                    return ((MethodSpecificationHandle)handle).DecodeSignature(compilation, context);
                default:
                    throw new BadImageFormatException("Metadata token must be either a methoddef, memberref or methodspec");
            }
        }
    }
}
