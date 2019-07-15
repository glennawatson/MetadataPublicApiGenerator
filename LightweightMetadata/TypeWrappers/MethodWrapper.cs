﻿// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Threading;
using LightweightMetadata.Extensions;

namespace LightweightMetadata.TypeWrappers
{
    /// <summary>
    /// A wrapper around the MethodDefinition.
    /// </summary>
    [DebuggerDisplay("{" + nameof(FullName) + "}")]
    public sealed class MethodWrapper : IHandleTypeNamedWrapper, IHasAttributes, IHasGenericParameters
    {
        private static readonly Dictionary<MethodDefinitionHandle, MethodWrapper> _registerTypes = new Dictionary<MethodDefinitionHandle, MethodWrapper>();

        private readonly Lazy<string> _name;
        private readonly Lazy<MethodSignature<IHandleTypeNamedWrapper>> _signature;
        private readonly Lazy<TypeWrapper> _declaringType;
        private readonly Lazy<IReadOnlyDictionary<string, IReadOnlyList<string>>> _constraints;
        private readonly Lazy<IReadOnlyList<ParameterWrapper>> _parameters;
        private readonly Lazy<IReadOnlyList<AttributeWrapper>> _attributes;
        private readonly Lazy<IReadOnlyList<GenericParameterWrapper>> _genericParameters;

        private readonly Lazy<(ITypeNamedWrapper owner, SymbolMethodKind symbolKind)> _semanticData;

        private readonly Lazy<bool> _isDelegate;

        private MethodWrapper(MethodDefinitionHandle handle, CompilationModule module)
        {
            MethodDefinitionHandle = handle;
            CompilationModule = module;
            Handle = handle;
            Definition = Resolve(handle, module);

            _declaringType = new Lazy<TypeWrapper>(() => TypeWrapper.Create(Definition.GetDeclaringType(), module), LazyThreadSafetyMode.PublicationOnly);

            _signature = new Lazy<MethodSignature<IHandleTypeNamedWrapper>>(() => Definition.DecodeSignature(module.TypeProvider, new GenericContext(this)), LazyThreadSafetyMode.PublicationOnly);
            _name = new Lazy<string>(GetName, LazyThreadSafetyMode.PublicationOnly);
            _constraints = new Lazy<IReadOnlyDictionary<string, IReadOnlyList<string>>>(GetConstraints, LazyThreadSafetyMode.PublicationOnly);

            IsAbstract = (Definition.Attributes & MethodAttributes.Abstract) != 0;
            IsStatic = (Definition.Attributes & MethodAttributes.Static) != 0;

            IsSealed = (Definition.Attributes & (MethodAttributes.Abstract | MethodAttributes.Final | MethodAttributes.NewSlot | MethodAttributes.Static)) == MethodAttributes.Final;

            IsOverride = (Definition.Attributes & (MethodAttributes.NewSlot | MethodAttributes.Virtual)) == MethodAttributes.Virtual;
            IsVirtual = (Definition.Attributes & (MethodAttributes.Abstract | MethodAttributes.Virtual | MethodAttributes.NewSlot | MethodAttributes.Final)) == (MethodAttributes.Virtual | MethodAttributes.NewSlot);

            _genericParameters = new Lazy<IReadOnlyList<GenericParameterWrapper>>(() => GenericParameterWrapper.Create(Definition.GetGenericParameters(), this, CompilationModule), LazyThreadSafetyMode.PublicationOnly);

            _semanticData = new Lazy<(ITypeNamedWrapper owner, SymbolMethodKind symbolKind)>(GetMethodSymbolKind, LazyThreadSafetyMode.PublicationOnly);

            _parameters = new Lazy<IReadOnlyList<ParameterWrapper>>(GetParameters, LazyThreadSafetyMode.PublicationOnly);

            _attributes = new Lazy<IReadOnlyList<AttributeWrapper>>(() => AttributeWrapper.Create(Definition.GetCustomAttributes(), CompilationModule), LazyThreadSafetyMode.PublicationOnly);

            _registerTypes.TryAdd(handle, this);

            _isDelegate = new Lazy<bool>(() => MethodKind == SymbolMethodKind.DelegateInvoke, LazyThreadSafetyMode.PublicationOnly);

            switch (Definition.Attributes & MethodAttributes.MemberAccessMask)
            {
                case MethodAttributes.Public:
                    Accessibility = EntityAccessibility.Public;
                    break;
                case MethodAttributes.Assembly:
                    Accessibility = EntityAccessibility.Internal;
                    break;
                case MethodAttributes.Private:
                    Accessibility = EntityAccessibility.Private;
                    break;
                case MethodAttributes.Family:
                    Accessibility = EntityAccessibility.Protected;
                    break;
                case MethodAttributes.FamANDAssem:
                    Accessibility = EntityAccessibility.PrivateProtected;
                    break;
                case MethodAttributes.FamORAssem:
                    Accessibility = EntityAccessibility.ProtectedInternal;
                    break;
                default:
                    Accessibility = EntityAccessibility.None;
                    break;
            }
        }

        /// <summary>
        /// Gets the resolved method definition.
        /// </summary>
        public MethodDefinition Definition { get; }

        /// <summary>
        /// Gets the method definition handle.
        /// </summary>
        public MethodDefinitionHandle MethodDefinitionHandle { get; }

        /// <inheritdoc />
        public string Name => _name.Value;

        /// <summary>
        /// Gets the type that declares this method.
        /// </summary>
        public TypeWrapper DeclaringType => _declaringType.Value;

        /// <summary>
        /// Gets the constraints that this method has if any.
        /// </summary>
        public IReadOnlyDictionary<string, IReadOnlyList<string>> Constraints => _constraints.Value;

        /// <summary>
        /// Gets the type that this method returns.
        /// </summary>
        public ITypeNamedWrapper ReturningType => _signature.Value.ReturnType;

        /// <summary>
        /// Gets the method kind.
        /// </summary>
        public SymbolMethodKind MethodKind => _semanticData.Value.symbolKind;

        /// <summary>
        /// Gets the owner of the method.
        /// </summary>
        public ITypeNamedWrapper Owner => _semanticData.Value.owner;

        /// <inheritdoc />
        public IReadOnlyList<AttributeWrapper> Attributes => _attributes.Value;

        /// <inheritdoc />
        public string FullName => DeclaringType.FullName + "." + Name;

        /// <inheritdoc />
        public string ReflectionFullName => DeclaringType.ReflectionFullName + "." + Name;

        /// <inheritdoc />
        public string TypeNamespace => DeclaringType.TypeNamespace;

        /// <inheritdoc />
        public EntityAccessibility Accessibility { get; }

        /// <inheritdoc />
        public bool IsAbstract { get; }

        /// <summary>
        /// Gets a value indicating whether the method is a delegate type.
        /// </summary>
        public bool IsDelegate => _isDelegate.Value;

        /// <summary>
        /// Gets a value indicating whether the method is static.
        /// </summary>
        public bool IsStatic { get; }

        /// <summary>
        /// Gets a value indicating whether the method is sealed.
        /// </summary>
        public bool IsSealed { get; }

        /// <summary>
        /// Gets a value indicating whether the method is virtual.
        /// </summary>
        public bool IsVirtual { get; }

        /// <summary>
        /// Gets a value indicating whether the method is override.
        /// </summary>
        public bool IsOverride { get; }

        /// <summary>
        /// Gets the parameters of the method.
        /// </summary>
        public IReadOnlyList<ParameterWrapper> Parameters => _parameters.Value;

        /// <summary>
        /// Gets the parameter types for generic orientated methods.
        /// </summary>
        public IReadOnlyList<ITypeNamedWrapper> ParameterTypes => _signature.Value.ParameterTypes.ToArray();

        /// <summary>
        /// Gets the number of generic parameters for the method.
        /// </summary>
        public int GenericParameterCount => _signature.Value.GenericParameterCount;

        /// <summary>
        /// Gets the number of required parameters for the method.
        /// </summary>
        public int RequiredParameterCount => _signature.Value.RequiredParameterCount;

        /// <summary>
        /// Gets a list of the generic type parameters.
        /// </summary>
        public IReadOnlyList<GenericParameterWrapper> GenericParameters => _genericParameters.Value;

        /// <inheritdoc />
        public KnownTypeCode KnownType => KnownTypeCode.None;

        /// <summary>
        /// Gets the module that this method belongs to.
        /// </summary>
        public CompilationModule CompilationModule { get; }

        /// <inheritdoc />
        public Handle Handle { get; }

        /// <summary>
        /// Creates a instance of the method, if there is already not an instance.
        /// </summary>
        /// <param name="handle">The handle to the instance.</param>
        /// <param name="module">The module that contains the instance.</param>
        /// <returns>The wrapper.</returns>
        public static MethodWrapper Create(MethodDefinitionHandle handle, CompilationModule module)
        {
            if (handle.IsNil)
            {
                return null;
            }

            return _registerTypes.GetOrAdd(handle, handleCreate => new MethodWrapper(handleCreate, module));
        }

        /// <summary>
        /// Creates a array instances of a type.
        /// </summary>
        /// <param name="collection">The collection to create.</param>
        /// <param name="module">The module to use in creation.</param>
        /// <returns>The list of the type.</returns>
        public static IReadOnlyList<MethodWrapper> Create(in MethodDefinitionHandleCollection collection, CompilationModule module)
        {
            var output = new MethodWrapper[collection.Count];

            int i = 0;
            foreach (var element in collection)
            {
                output[i] = Create(element, module);
                i++;
            }

            return output;
        }

        private static MethodDefinition Resolve(MethodDefinitionHandle handle, CompilationModule compilation)
        {
            return compilation.MetadataReader.GetMethodDefinition(handle);
        }

        private string GetName()
        {
            return Resolve(MethodDefinitionHandle, CompilationModule).Name.GetName(CompilationModule);
        }

        private IReadOnlyDictionary<string, IReadOnlyList<string>> GetConstraints()
        {
            var constraintDictionary = new Dictionary<string, ISet<string>>();

            foreach (var typeParameterHandle in Definition.GetGenericParameters())
            {
                var typeParameter = CompilationModule.MetadataReader.GetGenericParameter(typeParameterHandle);
                foreach (var constraint in typeParameter.GetConstraints().Select(x => CompilationModule.MetadataReader.GetGenericParameterConstraint(x)))
                {
                    var parameter = CompilationModule.MetadataReader.GetGenericParameter(constraint.Parameter);
                    var parameterName = parameter.Name.GetName(CompilationModule);

                    if (constraint.Type.IsNil)
                    {
                        continue;
                    }

                    var constraintType = WrapperFactory.Create(constraint.Type, CompilationModule);
                    if (constraintType.FullName != "System.Object")
                    {
                        if (!constraintDictionary.TryGetValue(parameterName, out var constraints))
                        {
                            constraints = new HashSet<string>();
                            constraintDictionary[parameterName] = constraints;
                        }

                        constraints.Add(constraintType.FullName);
                    }
                }
            }

            return constraintDictionary.ToDictionary(x => x.Key, x => (IReadOnlyList<string>)x.Value.ToList());
        }

        private (ITypeNamedWrapper owner, SymbolMethodKind symbolKind) GetMethodSymbolKind()
        {
            var (accessorOwnerHandle, semanticsAttribute) = CompilationModule.MethodSemanticsLookup.GetSemantics(MethodDefinitionHandle);

            var accessorOwner = WrapperFactory.Create(accessorOwnerHandle, CompilationModule);

            var name = Name;
            var parameterCount = Parameters.Count;

            const MethodAttributes finalizerAttributes = MethodAttributes.Virtual | MethodAttributes.Family | MethodAttributes.HideBySig;

            if ((semanticsAttribute & MethodSemanticsAttributes.Adder) != 0)
            {
                return (accessorOwner, SymbolMethodKind.EventAdd);
            }

            if ((semanticsAttribute & MethodSemanticsAttributes.Remover) != 0)
            {
                return (accessorOwner, SymbolMethodKind.EventRemove);
            }

            if ((semanticsAttribute & MethodSemanticsAttributes.Raiser) != 0)
            {
                return (accessorOwner, SymbolMethodKind.EventRaise);
            }

            if ((semanticsAttribute & MethodSemanticsAttributes.Getter) != 0)
            {
                return (accessorOwner, SymbolMethodKind.PropertyGet);
            }

            if ((semanticsAttribute & MethodSemanticsAttributes.Setter) != 0)
            {
                return (accessorOwner, SymbolMethodKind.PropertySet);
            }

            var attributes = Definition.Attributes;

            if ((attributes & (MethodAttributes.SpecialName | MethodAttributes.RTSpecialName)) != 0)
            {
                if (name == ".cctor" || name == ".ctor")
                {
                    return (default, SymbolMethodKind.Constructor);
                }

                if (name.StartsWith("op_", StringComparison.Ordinal))
                {
                    return (default, SymbolMethodKind.UserDefinedOperator);
                }
            }
            else if ((attributes & finalizerAttributes) == finalizerAttributes)
            {
                if (name == "Finalize" && parameterCount == 0)
                {
                    return (default, SymbolMethodKind.Destructor);
                }
            }

            return (default, SymbolMethodKind.Ordinary);
        }

        private IReadOnlyList<ParameterWrapper> GetParameters()
        {
            var parameterList = new List<ParameterWrapper>();
            var parameterHandles = Definition.GetParameters().ToList();
            int i = 0;
            foreach (var parameterHandle in parameterHandles)
            {
                var parameterInstance = CompilationModule.MetadataReader.GetParameter(parameterHandle);

                if (parameterInstance.SequenceNumber > 0 && i < _signature.Value.RequiredParameterCount)
                {
                    var parameterType = _signature.Value.ParameterTypes[parameterInstance.SequenceNumber - 1];

                    var parameter = ParameterWrapper.Create(parameterHandle, parameterType, CompilationModule);

                    parameterList.Add(parameter);
                }

                i++;
            }

            return parameterList;
        }
    }
}