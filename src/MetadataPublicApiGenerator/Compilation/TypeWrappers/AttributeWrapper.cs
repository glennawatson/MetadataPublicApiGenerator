// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

using MetadataPublicApiGenerator.Extensions;
using MetadataPublicApiGenerator.Helpers;

using Microsoft.CodeAnalysis.CSharp;

namespace MetadataPublicApiGenerator.Compilation.TypeWrappers
{
    internal class AttributeWrapper : ITypeNamedWrapper
    {
        private static readonly string[] typeNames =
        {
            default,
            "System.Runtime.CompilerServices." + nameof(CompilerGeneratedAttribute),
            "System.Runtime.CompilerServices." + nameof(ExtensionAttribute),
            "System.Runtime.CompilerServices." + nameof(DynamicAttribute),
            "System.Runtime.CompilerServices." + nameof(TupleElementNamesAttribute),
            "System.Diagnostics." + nameof(ConditionalAttribute),
            "System." + nameof(ObsoleteAttribute),
            "System.Runtime.CompilerServices." + "IsReadOnlyAttribute",
            "System.Diagnostics." + nameof(DebuggerHiddenAttribute),
            "System.Diagnostics." + nameof(DebuggerStepThroughAttribute),

            // Assembly attributes:
            "System.Reflection." + nameof(AssemblyVersionAttribute),
            "System.Runtime.CompilerServices." + nameof(InternalsVisibleToAttribute),
            "System.Runtime.CompilerServices." + nameof(TypeForwardedToAttribute),
            "System.Runtime.CompilerServices." + nameof(ReferenceAssemblyAttribute),

            // Type attributes:
            "System." + nameof(SerializableAttribute),
            "System." + nameof(FlagsAttribute),
            "System.Runtime.InteropServices." + nameof(ComImportAttribute),
            "System.Runtime.InteropServices." + nameof(CoClassAttribute),
            "System.Runtime.InteropServices." + nameof(StructLayoutAttribute),
            "System.Reflection." + nameof(DefaultMemberAttribute),
            "System.Runtime.CompilerServices." + "IsByRefLikeAttribute",
            "System.Runtime.CompilerServices." + nameof(IteratorStateMachineAttribute),
            "System.Runtime.CompilerServices." + nameof(AsyncStateMachineAttribute),

            // Field attributes:
            "System.Runtime.InteropServices." + nameof(FieldOffsetAttribute),
            "System." + nameof(NonSerializedAttribute),
            "System.Runtime.CompilerServices." + nameof(DecimalConstantAttribute),
            "System.Runtime.CompilerServices." + nameof(FixedBufferAttribute),

            // Method attributes:
            "System.Runtime.InteropServices." + nameof(DllImportAttribute),
            "System.Runtime.InteropServices." + nameof(PreserveSigAttribute),
            "System.Runtime.CompilerServices." + nameof(MethodImplAttribute),

            // Property attributes:
            "System.Runtime.CompilerServices." + nameof(IndexerNameAttribute),

            // Parameter attributes:
            "System." + nameof(ParamArrayAttribute),
            "System.Runtime.InteropServices." + nameof(InAttribute),
            "System.Runtime.InteropServices." + nameof(OutAttribute),
            "System.Runtime.InteropServices." + nameof(OptionalAttribute),
            "System.Runtime.CompilerServices." + nameof(CallerMemberNameAttribute),
            "System.Runtime.CompilerServices." + nameof(CallerFilePathAttribute),
            "System.Runtime.CompilerServices." + nameof(CallerLineNumberAttribute),

            // Marshalling attributes:
            "System.Runtime.InteropServices." + nameof(MarshalAsAttribute),

            // Security attributes:
            "System.Security.Permissions." + "PermissionSetAttribute",
        };

        private static readonly Dictionary<CustomAttributeHandle, AttributeWrapper> _registeredTypes = new Dictionary<CustomAttributeHandle, AttributeWrapper>();

        private readonly Lazy<MethodSignature<ITypeNamedWrapper>> _methodSignature;
        private readonly Lazy<ITypeNamedWrapper> _attributeType;

        private readonly Lazy<KnownAttribute> _knownType;

        private readonly Lazy<(IReadOnlyList<CustomAttributeTypedArgument<ITypeNamedWrapper>> fixedArguments, IReadOnlyList<CustomAttributeNamedArgument<ITypeNamedWrapper>> namedArguments)> _arguments;

        private AttributeWrapper(CustomAttributeHandle handle, CompilationModule module)
        {
            Module = module;
            Definition = Resolve(handle, module);
            AttributeHandle = handle;

            _methodSignature = new Lazy<MethodSignature<ITypeNamedWrapper>>(GetMethodSignature, LazyThreadSafetyMode.PublicationOnly);

            _attributeType = new Lazy<ITypeNamedWrapper>(GetAttributeType, LazyThreadSafetyMode.PublicationOnly);
            _arguments = new Lazy<(IReadOnlyList<CustomAttributeTypedArgument<ITypeNamedWrapper>> fixedArguments, IReadOnlyList<CustomAttributeNamedArgument<ITypeNamedWrapper>> namedArguments)>(GetArguments, LazyThreadSafetyMode.PublicationOnly);
            _knownType = new Lazy<KnownAttribute>(IsKnownAttributeType, LazyThreadSafetyMode.PublicationOnly);

            _registeredTypes.TryAdd(handle, this);
        }

        public CustomAttribute Definition { get; }

        public CustomAttributeHandle AttributeHandle { get; }

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

        public IReadOnlyList<CustomAttributeTypedArgument<ITypeNamedWrapper>> FixedArguments => _arguments.Value.fixedArguments;

        public IReadOnlyList<CustomAttributeNamedArgument<ITypeNamedWrapper>> NamedArguments => _arguments.Value.namedArguments;

        /// <inheritdoc />
        public CompilationModule Module { get; }

        public static AttributeWrapper Create(CustomAttributeHandle handle, CompilationModule module)
        {
            return _registeredTypes.GetOrAdd(handle, handleCreate => new AttributeWrapper(handleCreate, module));
        }

        private static CustomAttribute Resolve(CustomAttributeHandle handle, CompilationModule compilation)
        {
            return compilation.MetadataReader.GetCustomAttribute(handle);
        }

        private MethodSignature<ITypeNamedWrapper> GetMethodSignature()
        {
            MethodSignature<ITypeNamedWrapper> methodSignature;
            switch (Definition.Constructor.Kind)
            {
                case HandleKind.MethodDefinition:
                    var methodDefinition = ((MethodDefinitionHandle)Definition.Constructor).Resolve(Module);

                    methodSignature = methodDefinition.DecodeSignature(new TypeProvider(Module.Compilation), new GenericContext(Module));
                    break;

                case HandleKind.MemberReference:
                    var memberReference = ((MemberReferenceHandle)Definition.Constructor).Resolve(Module);

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

        private (IReadOnlyList<CustomAttributeTypedArgument<ITypeNamedWrapper>> fixedArguments, IReadOnlyList<CustomAttributeNamedArgument<ITypeNamedWrapper>> namedArguments) GetArguments()
        {
            var wrapper = Definition.DecodeValue(new CustomAttributeTypeProvider(Module.Compilation));
            return (wrapper.FixedArguments, wrapper.NamedArguments);
        }

        private KnownAttribute IsKnownAttributeType()
        {
            var method = ((MethodDefinitionHandle)Definition.Constructor).Resolve(Module);
            var declaredType = method.GetDeclaringType().GetName(Module);
            var index = Array.IndexOf(typeNames, declaredType);
            if (index < 0)
            {
                return KnownAttribute.None;
            }

            return (KnownAttribute)index;
        }

    }
}
