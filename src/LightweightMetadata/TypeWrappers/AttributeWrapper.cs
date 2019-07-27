// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading;

using LightweightMetadata.Extensions;
using LightweightMetadata.TypeWrappers;

namespace LightweightMetadata
{
    /// <summary>
    /// Wraps a AttributeDefinition and represents a .NET attribute.
    /// </summary>
    public class AttributeWrapper : IHandleTypeNamedWrapper
    {
        private static readonly ConcurrentDictionary<(CustomAttributeHandle handle, AssemblyMetadata module), AttributeWrapper> _registeredTypes = new ConcurrentDictionary<(CustomAttributeHandle handle, AssemblyMetadata module), AttributeWrapper>();

        private readonly Lazy<MethodSignature<IHandleTypeNamedWrapper>> _methodSignature;
        private readonly Lazy<ITypeNamedWrapper> _attributeType;

        private readonly Lazy<KnownAttribute> _knownAttribute;
        private readonly Lazy<KnownTypeCode> _knownTypeCode;
        private readonly Lazy<(IReadOnlyList<CustomAttributeTypedArgument<IHandleTypeNamedWrapper>> fixedArguments, IReadOnlyList<CustomAttributeNamedArgument<IHandleTypeNamedWrapper>> namedArguments)> _arguments;
        private readonly Lazy<IReadOnlyList<ITypeNamedWrapper>> _parameterTypes;

        private AttributeWrapper(CustomAttributeHandle handle, AssemblyMetadata module)
        {
            CompilationModule = module;
            AttributeHandle = handle;
            Handle = handle;
            Definition = Resolve(handle, module);

            _methodSignature = new Lazy<MethodSignature<IHandleTypeNamedWrapper>>(GetMethodSignature, LazyThreadSafetyMode.PublicationOnly);

            _attributeType = new Lazy<ITypeNamedWrapper>(GetAttributeType, LazyThreadSafetyMode.PublicationOnly);
            _arguments = new Lazy<(IReadOnlyList<CustomAttributeTypedArgument<IHandleTypeNamedWrapper>> fixedArguments, IReadOnlyList<CustomAttributeNamedArgument<IHandleTypeNamedWrapper>> namedArguments)>(GetArguments, LazyThreadSafetyMode.PublicationOnly);
            _knownAttribute = new Lazy<KnownAttribute>(GetKnownAttributeType, LazyThreadSafetyMode.PublicationOnly);
            _knownTypeCode = new Lazy<KnownTypeCode>(this.ToKnownTypeCode, LazyThreadSafetyMode.PublicationOnly);
            _parameterTypes = new Lazy<IReadOnlyList<ITypeNamedWrapper>>(() => _methodSignature.Value.ParameterTypes.ToArray(), LazyThreadSafetyMode.PublicationOnly);
        }

        /// <summary>
        /// Gets the definition of the attribute.
        /// </summary>
        public CustomAttribute Definition { get; }

        /// <summary>
        /// Gets the handle to the attribute.
        /// </summary>
        public CustomAttributeHandle AttributeHandle { get; }

        /// <inheritdoc />
        public Handle Handle { get; }

        /// <summary>Gets the return type of the method.</summary>
        /// <returns>The return type.</returns>
        public ITypeNamedWrapper ReturnType => _methodSignature.Value.ReturnType;

        /// <summary>Gets the number of parameters that are required for the method.</summary>
        /// <returns>The number of required parameters.</returns>
        public int RequiredParameterCount => _methodSignature.Value.RequiredParameterCount;

        /// <summary>Gets the number of generic type parameters for the method.</summary>
        /// <returns>The number of generic type parameters, or 0 for non-generic methods.</returns>
        public int GenericParameterCount => _methodSignature.Value.GenericParameterCount;

        /// <summary>Gets the method&amp;#39;s parameter types.</summary>
        /// <returns>An immutable collection of parameter types.</returns>
        public IReadOnlyList<ITypeNamedWrapper> ParameterTypes => _parameterTypes.Value;

        /// <summary>
        /// Gets the known attribute type for this attribute.
        /// </summary>
        public KnownAttribute KnownAttributeType => _knownAttribute.Value;

        /// <inheritdoc />
        public string Name => AttributeType?.Name;

        /// <inheritdoc />
        public string FullName => AttributeType?.FullName;

        /// <inheritdoc />
        public string ReflectionFullName => AttributeType?.ReflectionFullName;

        /// <inheritdoc />
        public string TypeNamespace => AttributeType?.TypeNamespace;

        /// <inheritdoc />
        public EntityAccessibility Accessibility => AttributeType?.Accessibility ?? EntityAccessibility.None;

        /// <inheritdoc />
        public bool IsAbstract => AttributeType?.IsAbstract ?? false;

        /// <summary>
        /// Gets the known type. This indicates if the attribute is a known type.
        /// </summary>
        public KnownTypeCode KnownType => _knownTypeCode.Value;

        /// <summary>
        /// Gets a list of the fixed arguments.
        /// </summary>
        public IReadOnlyList<CustomAttributeTypedArgument<IHandleTypeNamedWrapper>> FixedArguments => _arguments.Value.fixedArguments;

        /// <summary>
        /// Gets a list of the named arguments.
        /// </summary>
        public IReadOnlyList<CustomAttributeNamedArgument<IHandleTypeNamedWrapper>> NamedArguments => _arguments.Value.namedArguments;

        /// <inheritdoc />
        public AssemblyMetadata CompilationModule { get; }

        /// <summary>
        /// Gets the attribute type.
        /// </summary>
        public ITypeNamedWrapper AttributeType => _attributeType.Value;

        /// <summary>
        /// Creates a new instance of the AttributeWrapper class.
        /// </summary>
        /// <param name="handle">The handle to the attribute.</param>
        /// <param name="module">The module that contains the attribute.</param>
        /// <returns>The new attribute if the handle is not nil, otherwise null.</returns>
        public static AttributeWrapper Create(CustomAttributeHandle handle, AssemblyMetadata module)
        {
            if (handle.IsNil)
            {
                return null;
            }

            return _registeredTypes.GetOrAdd((handle, module), data => new AttributeWrapper(data.handle, data.module));
        }

        /// <summary>
        /// Creates a array instances of a type.
        /// </summary>
        /// <param name="collection">The collection to create.</param>
        /// <param name="module">The module to use in creation.</param>
        /// <returns>The list of the type.</returns>
        public static IReadOnlyList<AttributeWrapper> Create(in CustomAttributeHandleCollection collection, AssemblyMetadata module)
        {
            var output = new AttributeWrapper[collection.Count];

            int i = 0;
            foreach (var element in collection)
            {
                output[i] = Create(element, module);
                i++;
            }

            return output.ToArray();
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return FullName;
        }

        private static CustomAttribute Resolve(CustomAttributeHandle handle, AssemblyMetadata compilation)
        {
            return compilation.MetadataReader.GetCustomAttribute(handle);
        }

        private MethodSignature<IHandleTypeNamedWrapper> GetMethodSignature()
        {
            MethodSignature<IHandleTypeNamedWrapper> methodSignature;
            switch (Definition.Constructor.Kind)
            {
                case HandleKind.MethodDefinition:
                    var methodDefinition = CompilationModule.MetadataReader.GetMethodDefinition((MethodDefinitionHandle)Definition.Constructor);

                    methodSignature = methodDefinition.DecodeSignature(new TypeProvider(CompilationModule.Compilation), new GenericContext(CompilationModule));
                    break;

                case HandleKind.MemberReference:
                    var memberReference = CompilationModule.MetadataReader.GetMemberReference((MemberReferenceHandle)Definition.Constructor);

                    // Attribute types shouldn't be generic (and certainly not open), so we don't need a generic context.
                    methodSignature = memberReference.DecodeMethodSignature(new TypeProvider(CompilationModule.Compilation), new GenericContext(CompilationModule));
                    break;
                default:
                    throw new Exception("Unknown method type");
            }

            return methodSignature;
        }

        private ITypeNamedWrapper GetAttributeType()
        {
            var type = WrapperFactory.Create(Definition.Constructor, CompilationModule);

            if (type is MemberReferenceWrapper memberReference)
            {
                return memberReference.Parent;
            }

            if (type is MethodWrapper methodWrapper)
            {
                return methodWrapper.DeclaringType;
            }

            return type;
        }

        private (IReadOnlyList<CustomAttributeTypedArgument<IHandleTypeNamedWrapper>> fixedArguments, IReadOnlyList<CustomAttributeNamedArgument<IHandleTypeNamedWrapper>> namedArguments) GetArguments()
        {
            var wrapper = Definition.DecodeValue(new TypeProvider(CompilationModule.Compilation));

            var fixedArgumentsList = wrapper.FixedArguments.ToArray();

            var namedArgumentsList = wrapper.NamedArguments.ToArray();

            return (fixedArgumentsList, namedArgumentsList);
        }

        private KnownAttribute GetKnownAttributeType()
        {
            var fullName = AttributeType.FullName;
            var index = Array.IndexOf(KnownTypeCodeNames.TypeNames, fullName);
            if (index < 0)
            {
                return KnownAttribute.None;
            }

            return (KnownAttribute)index;
        }
    }
}
