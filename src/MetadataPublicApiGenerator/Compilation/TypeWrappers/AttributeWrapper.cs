// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Threading;

using MetadataPublicApiGenerator.Extensions;

namespace MetadataPublicApiGenerator.Compilation.TypeWrappers
{
    internal class AttributeWrapper : IHandleTypeNamedWrapper
    {
        private static readonly Dictionary<CustomAttributeHandle, AttributeWrapper> _registeredTypes = new Dictionary<CustomAttributeHandle, AttributeWrapper>();

        private readonly Lazy<MethodSignature<IHandleTypeNamedWrapper>> _methodSignature;
        private readonly Lazy<ITypeNamedWrapper> _attributeType;

        private readonly Lazy<KnownAttribute> _knownType;

        private readonly Lazy<(IReadOnlyList<CustomAttributeTypedArgument<IHandleTypeNamedWrapper>> fixedArguments, IReadOnlyList<CustomAttributeNamedArgument<IHandleTypeNamedWrapper>> namedArguments)> _arguments;

        private AttributeWrapper(CustomAttributeHandle handle, CompilationModule module)
        {
            Module = module;
            AttributeHandle = handle;
            Handle = handle;
            Definition = Resolve(handle, module);

            _methodSignature = new Lazy<MethodSignature<IHandleTypeNamedWrapper>>(GetMethodSignature, LazyThreadSafetyMode.PublicationOnly);

            _attributeType = new Lazy<ITypeNamedWrapper>(GetAttributeType, LazyThreadSafetyMode.PublicationOnly);
            _arguments = new Lazy<(IReadOnlyList<CustomAttributeTypedArgument<IHandleTypeNamedWrapper>> fixedArguments, IReadOnlyList<CustomAttributeNamedArgument<IHandleTypeNamedWrapper>> namedArguments)>(GetArguments, LazyThreadSafetyMode.PublicationOnly);
            _knownType = new Lazy<KnownAttribute>(IsKnownAttributeType, LazyThreadSafetyMode.PublicationOnly);

            _registeredTypes.TryAdd(handle, this);
        }

        public CustomAttribute Definition { get; }

        public CustomAttributeHandle AttributeHandle { get; }

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
        public IReadOnlyList<ITypeNamedWrapper> ParameterTypes => _methodSignature.Value.ParameterTypes;

        public KnownAttribute KnownAttribute => _knownType.Value;

        /// <inheritdoc />
        public string Name => _attributeType.Value.Name;

        /// <inheritdoc />
        public string FullName => _attributeType.Value.FullName;

        /// <inheritdoc />
        public string Namespace => _attributeType.Value.Namespace;

        /// <inheritdoc />
        public bool IsPublic => _attributeType.Value.IsPublic;

        /// <inheritdoc />
        public bool IsAbstract => _attributeType.Value.IsAbstract;

        public KnownAttribute KnownType => _knownType.Value;

        public bool IsKnownType => KnownType != KnownAttribute.None;

        public IReadOnlyList<CustomAttributeTypedArgument<IHandleTypeNamedWrapper>> FixedArguments => _arguments.Value.fixedArguments;

        public IReadOnlyList<CustomAttributeNamedArgument<IHandleTypeNamedWrapper>> NamedArguments => _arguments.Value.namedArguments;

        /// <inheritdoc />
        public CompilationModule Module { get; }

        public static AttributeWrapper Create(CustomAttributeHandle handle, CompilationModule module)
        {
            if (handle.IsNil)
            {
                return null;
            }

            return _registeredTypes.GetOrAdd(handle, handleCreate => new AttributeWrapper(handleCreate, module));
        }

        private static CustomAttribute Resolve(CustomAttributeHandle handle, CompilationModule compilation)
        {
            return compilation.MetadataReader.GetCustomAttribute(handle);
        }

        private MethodSignature<IHandleTypeNamedWrapper> GetMethodSignature()
        {
            MethodSignature<IHandleTypeNamedWrapper> methodSignature;
            switch (Definition.Constructor.Kind)
            {
                case HandleKind.MethodDefinition:
                    var methodDefinition = Module.MetadataReader.GetMethodDefinition((MethodDefinitionHandle)Definition.Constructor);

                    methodSignature = methodDefinition.DecodeSignature(new TypeProvider(Module.Compilation), new GenericContext(Module));
                    break;

                case HandleKind.MemberReference:
                    var memberReference = Module.MetadataReader.GetMemberReference((MemberReferenceHandle)Definition.Constructor);

                    // Attribute types shouldn't be generic (and certainly not open), so we don't need a generic context.
                    methodSignature = memberReference.DecodeMethodSignature(new TypeProvider(Module.Compilation), new GenericContext(Module));
                    break;
                default:
                    throw new Exception("Unknown method type");
            }

            return methodSignature;
        }

        private ITypeNamedWrapper GetAttributeType()
        {
            switch (Definition.Constructor.Kind)
            {
                case HandleKind.MethodDefinition:
                    return MethodWrapper.Create((MethodDefinitionHandle)Definition.Constructor, Module);
                case HandleKind.MemberReference:
                    var memberReference = Module.MetadataReader.GetMemberReference((MemberReferenceHandle)Definition.Constructor);
                    return WrapperFactory.Create(memberReference.Parent, Module);
                default:
                    throw new BadImageFormatException("Unexpected token kind for attribute constructor: " + Definition.Constructor.Kind);
            }
        }

        private (IReadOnlyList<CustomAttributeTypedArgument<IHandleTypeNamedWrapper>> fixedArguments, IReadOnlyList<CustomAttributeNamedArgument<IHandleTypeNamedWrapper>> namedArguments) GetArguments()
        {
            var wrapper = Definition.DecodeValue(new TypeProvider(Module.Compilation));
            return (wrapper.FixedArguments, wrapper.NamedArguments);
        }

        private KnownAttribute IsKnownAttributeType()
        {
            var methodDefinition = MethodWrapper.Create((MethodDefinitionHandle)Definition.Constructor, Module);
            var declaredType = methodDefinition.DeclaringType.Name;
            var index = Array.IndexOf(KnownTypeCodeNames.TypeNames, declaredType);
            if (index < 0)
            {
                return KnownAttribute.None;
            }

            return (KnownAttribute)index;
        }
    }
}
