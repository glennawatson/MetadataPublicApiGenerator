// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Reflection.Metadata;
using MetadataPublicApiGenerator.Compilation;
using MetadataPublicApiGenerator.Compilation.TypeWrappers;

namespace MetadataPublicApiGenerator.Extensions
{
    internal static class HandleResolveExtensions
    {
        private static readonly ConcurrentDictionary<CompilationModule, ImmutableDictionary<CustomAttributeHandle, CustomAttribute>> _customAttributeCollection = new ConcurrentDictionary<CompilationModule, ImmutableDictionary<CustomAttributeHandle, CustomAttribute>>();
        private static readonly ConcurrentDictionary<CompilationModule, ImmutableDictionary<TypeDefinitionHandle, TypeDefinition>> _typeDefinitions = new ConcurrentDictionary<CompilationModule, ImmutableDictionary<TypeDefinitionHandle, TypeDefinition>>();
        private static readonly ConcurrentDictionary<CompilationModule, ImmutableDictionary<MethodDefinitionHandle, MethodDefinition>> _methodDefinitions = new ConcurrentDictionary<CompilationModule, ImmutableDictionary<MethodDefinitionHandle, MethodDefinition>>();
        private static readonly ConcurrentDictionary<CompilationModule, ImmutableDictionary<EventDefinitionHandle, EventDefinition>> _eventDefinitions = new ConcurrentDictionary<CompilationModule, ImmutableDictionary<EventDefinitionHandle, EventDefinition>>();
        private static readonly ConcurrentDictionary<CompilationModule, ImmutableDictionary<FieldDefinitionHandle, FieldDefinition>> _fieldDefinitions = new ConcurrentDictionary<CompilationModule, ImmutableDictionary<FieldDefinitionHandle, FieldDefinition>>();
        private static readonly ConcurrentDictionary<CompilationModule, ImmutableDictionary<PropertyDefinitionHandle, PropertyDefinition>> _propertyDefinition = new ConcurrentDictionary<CompilationModule, ImmutableDictionary<PropertyDefinitionHandle, PropertyDefinition>>();
        private static readonly ConcurrentDictionary<CompilationModule, ImmutableDictionary<MemberReferenceHandle, MemberReference>> _memberReferences = new ConcurrentDictionary<CompilationModule, ImmutableDictionary<MemberReferenceHandle, MemberReference>>();

        public static CustomAttribute Resolve(this CustomAttributeHandle handle, CompilationModule compilation)
        {
            var map = _customAttributeCollection.GetOrAdd(compilation, comp => comp.MetadataReader.CustomAttributes.ToImmutableDictionary(x => x, x => comp.MetadataReader.GetCustomAttribute(x)));

            return map.GetValueOrDefault(handle);
        }

        public static TypeDefinition Resolve(this TypeDefinitionHandle handle, CompilationModule compilation)
        {
            var map = _typeDefinitions.GetOrAdd(compilation, comp => comp.MetadataReader.TypeDefinitions.ToImmutableDictionary(x => x, x => compilation.MetadataReader.GetTypeDefinition(x)));

            return map.GetValueOrDefault(handle);
        }

        public static MethodDefinition Resolve(this MethodDefinitionHandle handle, CompilationModule compilation)
        {
            var map = _methodDefinitions.GetOrAdd(compilation, comp => comp.MetadataReader.MethodDefinitions.ToImmutableDictionary(x => x, x => compilation.MetadataReader.GetMethodDefinition(x)));

            return map.GetValueOrDefault(handle);
        }

        public static EventDefinition Resolve(this EventDefinitionHandle handle, CompilationModule compilation)
        {
            var map = _eventDefinitions.GetOrAdd(compilation, comp => comp.MetadataReader.EventDefinitions.ToImmutableDictionary(x => x, x => compilation.MetadataReader.GetEventDefinition(x)));

            return map.GetValueOrDefault(handle);
        }

        public static FieldDefinition Resolve(this FieldDefinitionHandle handle, CompilationModule compilation)
        {
            var map = _fieldDefinitions.GetOrAdd(compilation, comp => comp.MetadataReader.FieldDefinitions.ToImmutableDictionary(x => x, x => compilation.MetadataReader.GetFieldDefinition(x)));

            return map.GetValueOrDefault(handle);
        }

        public static PropertyDefinition Resolve(this PropertyDefinitionHandle handle, CompilationModule compilation)
        {
            var map = _propertyDefinition.GetOrAdd(compilation, comp => comp.MetadataReader.PropertyDefinitions.ToImmutableDictionary(x => x, x => compilation.MetadataReader.GetPropertyDefinition(x)));

            return map.GetValueOrDefault(handle);
        }

        public static MemberReference Resolve(this MemberReferenceHandle handle, CompilationModule compilation)
        {
            var map = _memberReferences.GetOrAdd(compilation, comp => comp.MetadataReader.MemberReferences.ToImmutableDictionary(x => x, x => compilation.MetadataReader.GetMemberReference(x)));

            return map.GetValueOrDefault(handle);
        }

        public static Constant Resolve(this ConstantHandle handle, CompilationModule compilation)
        {
            return compilation.MetadataReader.GetConstant(handle);
        }

        public static TypeReference Resolve(this TypeReferenceHandle handle, CompilationModule compilation)
        {
            return compilation.MetadataReader.GetTypeReference(handle);
        }

        public static GenericParameter Resolve(this GenericParameterHandle handle, CompilationModule compilation)
        {
            return compilation.MetadataReader.GetGenericParameter(handle);
        }

        public static GenericParameterConstraint Resolve(this GenericParameterConstraintHandle handle, CompilationModule compilation)
        {
            return compilation.MetadataReader.GetGenericParameterConstraint(handle);
        }

        public static NamespaceDefinition Resolve(this NamespaceDefinitionHandle handle, CompilationModule compilation)
        {
            return compilation.MetadataReader.GetNamespaceDefinition(handle);
        }

        public static Parameter Resolve(this ParameterHandle handle, CompilationModule compilation)
        {
            return compilation.MetadataReader.GetParameter(handle);
        }

        public static TypeSpecification Resolve(this TypeSpecificationHandle handle, CompilationModule compilation)
        {
            return compilation.MetadataReader.GetTypeSpecification(handle);
        }

        public static ExportedType Resolve(this ExportedTypeHandle handle, CompilationModule compilation)
        {
            return compilation.MetadataReader.GetExportedType(handle);
        }

        public static MethodSpecification Resolve(this MethodSpecificationHandle handle, CompilationModule compilation)
        {
            return compilation.MetadataReader.GetMethodSpecification(handle);
        }
    }
}