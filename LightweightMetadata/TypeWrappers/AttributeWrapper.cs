// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection.Metadata;
using System.Threading;
using LightweightMetadata.Extensions;

namespace LightweightMetadata.TypeWrappers
{
    /// <summary>
    /// Wraps a AttributeDefinition and represents a .NET attribute.
    /// </summary>
    [DebuggerDisplay("{" + nameof(FullName) + "}")]
    public class AttributeWrapper : IHandleTypeNamedWrapper
    {
        private static readonly Dictionary<CustomAttributeHandle, AttributeWrapper> _registeredTypes = new Dictionary<CustomAttributeHandle, AttributeWrapper>();

        private readonly Lazy<MethodSignature<IHandleTypeNamedWrapper>> _methodSignature;
        private readonly Lazy<ITypeNamedWrapper> _attributeType;

        private readonly Lazy<KnownAttribute> _knownAttribute;
        private readonly Lazy<KnownTypeCode> _knownTypeCode;

        private readonly Lazy<(IReadOnlyList<CustomAttributeTypedArgument<IHandleTypeNamedWrapper>> fixedArguments, IReadOnlyList<CustomAttributeNamedArgument<IHandleTypeNamedWrapper>> namedArguments)> _arguments;

        private AttributeWrapper(CustomAttributeHandle handle, CompilationModule module)
        {
            CompilationModule = module;
            AttributeHandle = handle;
            Handle = handle;
            Definition = Resolve(handle, module);

            _methodSignature = new Lazy<MethodSignature<IHandleTypeNamedWrapper>>(GetMethodSignature, LazyThreadSafetyMode.PublicationOnly);

            _attributeType = new Lazy<ITypeNamedWrapper>(GetAttributeType, LazyThreadSafetyMode.PublicationOnly);
            _arguments = new Lazy<(IReadOnlyList<CustomAttributeTypedArgument<IHandleTypeNamedWrapper>> fixedArguments, IReadOnlyList<CustomAttributeNamedArgument<IHandleTypeNamedWrapper>> namedArguments)>(GetArguments, LazyThreadSafetyMode.PublicationOnly);
            _knownAttribute = new Lazy<KnownAttribute>(IsKnownAttributeType, LazyThreadSafetyMode.PublicationOnly);
            _knownTypeCode = new Lazy<KnownTypeCode>(this.ToKnownTypeCode, LazyThreadSafetyMode.PublicationOnly);
            _registeredTypes.TryAdd(handle, this);
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
        public IReadOnlyList<ITypeNamedWrapper> ParameterTypes => _methodSignature.Value.ParameterTypes;

        /// <summary>
        /// Gets the known attribute type for this attribute.
        /// </summary>
        public KnownAttribute KnownAttribute => _knownAttribute.Value;

        /// <inheritdoc />
        public string Name => _attributeType.Value.Name;

        /// <inheritdoc />
        public string FullName => _attributeType.Value.FullName;

        /// <inheritdoc />
        public string ReflectionFullName => _attributeType.Value.ReflectionFullName;

        /// <inheritdoc />
        public string TypeNamespace => _attributeType.Value.TypeNamespace;

        /// <inheritdoc />
        public EntityAccessibility Accessibility => _attributeType.Value.Accessibility;

        /// <inheritdoc />
        public bool IsAbstract => _attributeType.Value.IsAbstract;

        /// <summary>
        /// Gets the known type. This indicates if the attribute is a known type.
        /// </summary>
        public KnownTypeCode KnownType => _knownTypeCode.Value;

        /// <summary>
        /// Gets a value indicating whether the attribute is a known type.
        /// </summary>
        public bool IsKnownAttribute => KnownAttribute != KnownAttribute.None;

        /// <summary>
        /// Gets a list of the fixed arguments.
        /// </summary>
        public IReadOnlyList<CustomAttributeTypedArgument<IHandleTypeNamedWrapper>> FixedArguments => _arguments.Value.fixedArguments;

        /// <summary>
        /// Gets a list of the named arguments.
        /// </summary>
        public IReadOnlyList<CustomAttributeNamedArgument<IHandleTypeNamedWrapper>> NamedArguments => _arguments.Value.namedArguments;

        /// <inheritdoc />
        public CompilationModule CompilationModule { get; }

        /// <summary>
        /// Creates a new instance of the AttributeWrapper class.
        /// </summary>
        /// <param name="handle">The handle to the attribute.</param>
        /// <param name="module">The module that contains the attribute.</param>
        /// <returns>The new attribute if the handle is not nil, otherwise null.</returns>
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
            switch (Definition.Constructor.Kind)
            {
                case HandleKind.MethodDefinition:
                    return MethodWrapper.Create((MethodDefinitionHandle)Definition.Constructor, CompilationModule).DeclaringType;
                case HandleKind.MemberReference:
                    var memberReference = CompilationModule.MetadataReader.GetMemberReference((MemberReferenceHandle)Definition.Constructor);
                    return WrapperFactory.Create(memberReference.Parent, CompilationModule);
                default:
                    throw new BadImageFormatException("Unexpected token kind for attribute constructor: " + Definition.Constructor.Kind);
            }
        }

        private (IReadOnlyList<CustomAttributeTypedArgument<IHandleTypeNamedWrapper>> fixedArguments, IReadOnlyList<CustomAttributeNamedArgument<IHandleTypeNamedWrapper>> namedArguments) GetArguments()
        {
            var wrapper = Definition.DecodeValue(new TypeProvider(CompilationModule.Compilation));
            return (wrapper.FixedArguments, wrapper.NamedArguments);
        }

        private KnownAttribute IsKnownAttributeType()
        {
            var methodDefinition = MethodWrapper.Create((MethodDefinitionHandle)Definition.Constructor, CompilationModule);
            var declaredType = methodDefinition.DeclaringType.FullName;
            var index = Array.IndexOf(KnownTypeCodeNames.TypeNames, declaredType);
            if (index < 0)
            {
                return KnownAttribute.None;
            }

            return (KnownAttribute)index;
        }
    }
}
