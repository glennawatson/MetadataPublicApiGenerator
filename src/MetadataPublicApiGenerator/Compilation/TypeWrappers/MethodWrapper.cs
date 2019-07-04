// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text;
using System.Threading;

using MetadataPublicApiGenerator.Extensions;

using Microsoft.CodeAnalysis;

namespace MetadataPublicApiGenerator.Compilation.TypeWrappers
{
    internal sealed class MethodWrapper : ITypeNamedWrapper
    {
        private static readonly Dictionary<MethodDefinitionHandle, MethodWrapper> _registerTypes = new Dictionary<MethodDefinitionHandle, MethodWrapper>();

        private readonly Lazy<string> _name;
        private readonly Lazy<MethodSignature<ITypeNamedWrapper>> _signature;
        private readonly Lazy<ITypeWrapper> _declaringType;
        private readonly Lazy<IReadOnlyDictionary<string, IReadOnlyList<string>>> _constraints;
        private readonly Lazy<IReadOnlyList<ParameterWrapper>> _parameters;

        private readonly Lazy<(ITypeNamedWrapper owner, MethodKind symbolKind)> _semanticData;

        private MethodWrapper(MethodDefinitionHandle handle, CompilationModule module)
        {
            Definition = Resolve(handle, module);
            MethodDefinitionHandle = handle;
            Module = module;

            _declaringType = new Lazy<ITypeWrapper>(() => TypeWrapper.Create(Definition.GetDeclaringType(), module), LazyThreadSafetyMode.PublicationOnly);

            _signature = new Lazy<MethodSignature<ITypeNamedWrapper>>(() => Definition.DecodeSignature(module.TypeProvider, new GenericContext(module, MethodDefinitionHandle)), LazyThreadSafetyMode.PublicationOnly);
            _name = new Lazy<string>(GetName, LazyThreadSafetyMode.PublicationOnly);
            _constraints = new Lazy<IReadOnlyDictionary<string, IReadOnlyList<string>>>(GetConstraints, LazyThreadSafetyMode.PublicationOnly);

            IsPublic = (Definition.Attributes & MethodAttributes.Public) == 0;
            IsAbstract = (Definition.Attributes & MethodAttributes.Abstract) == 0;
            IsStatic = (Definition.Attributes & MethodAttributes.Static) == 0;

            IsSealed = (Definition.Attributes & (MethodAttributes.Abstract | MethodAttributes.Final | MethodAttributes.NewSlot | MethodAttributes.Static)) == MethodAttributes.Final;

            IsOverride = (Definition.Attributes & (MethodAttributes.NewSlot | MethodAttributes.Virtual)) == MethodAttributes.Virtual;
            IsVirtual = (Definition.Attributes & (MethodAttributes.Abstract | MethodAttributes.Virtual | MethodAttributes.NewSlot | MethodAttributes.Final)) == (MethodAttributes.Virtual | MethodAttributes.NewSlot);

            _semanticData = new Lazy<(ITypeNamedWrapper owner, MethodKind symbolKind)>(GetMethodSymbolKind, LazyThreadSafetyMode.PublicationOnly);

            _parameters = new Lazy<IReadOnlyList<ParameterWrapper>>(() => Definition.GetParameters().Select(x => ParameterWrapper.Create(x, module)).ToList(), LazyThreadSafetyMode.PublicationOnly);

            _registerTypes.TryAdd(handle, this);
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
        public ITypeWrapper DeclaringType => _declaringType.Value;

        public IReadOnlyDictionary<string, IReadOnlyList<string>> Constraints => _constraints.Value;

        /// <summary>
        /// Gets the type that this method returns.
        /// </summary>
        public ITypeNamedWrapper ReturningType => _signature.Value.ReturnType;

        public MethodKind MethodKind => _semanticData.Value.symbolKind;

        public ITypeNamedWrapper Owner => _semanticData.Value.owner;

        /// <inheritdoc />
        public string FullName => DeclaringType.FullName + "." + Name;

        /// <inheritdoc />
        public string Namespace => DeclaringType.Namespace;

        /// <inheritdoc />
        public bool IsPublic { get; }

        /// <inheritdoc />
        public bool IsAbstract { get; }

        public bool IsStatic { get; }

        public bool IsSealed { get; }

        public bool IsVirtual { get; }

        public bool IsOverride { get; }

        public IReadOnlyList<ParameterWrapper> Parameters => _parameters.Value;

        /// <summary>
        /// Gets the parameter types for generic orientated methods.
        /// </summary>
        public IReadOnlyCollection<ITypeNamedWrapper> ParameterTypes => _signature.Value.ParameterTypes;

        /// <summary>
        /// Gets the number of generic parameters for the method.
        /// </summary>
        public int GenericParameterCount => _signature.Value.GenericParameterCount;

        /// <summary>
        /// Gets the number of required parameters for the method.
        /// </summary>
        public int RequiredParameterCount => _signature.Value.RequiredParameterCount;

        /// <summary>
        /// Gets the module that this method belongs to.
        /// </summary>
        public CompilationModule Module { get; }

        /// <summary>
        /// Creates a instance of the method, if there is already not an instance.
        /// </summary>
        /// <param name="handle">The handle to the instance.</param>
        /// <param name="module">The module that contains the instance.</param>
        /// <returns>The wrapper.</returns>
        public static MethodWrapper Create(MethodDefinitionHandle handle, CompilationModule module)
        {
            return _registerTypes.GetOrAdd(handle, handleCreate => new MethodWrapper(handleCreate, module));
        }

        private static MethodDefinition Resolve(MethodDefinitionHandle handle, CompilationModule compilation)
        {
            return compilation.MetadataReader.GetMethodDefinition(handle);
        }

        private string GetName()
        {
            return Resolve(MethodDefinitionHandle, Module).Name.GetName(Module);
        }

        private IReadOnlyDictionary<string, IReadOnlyList<string>> GetConstraints()
        {
            var constraintDictionary = new Dictionary<string, ISet<string>>();

            foreach (var typeParameterHandle in Definition.GetGenericParameters())
            {
                var typeParameter = typeParameterHandle.Resolve(Module);
                foreach (var constraint in typeParameter.GetConstraints().Select(x => x.Resolve(Module)))
                {
                    var parameter = constraint.Parameter.Resolve(Module);
                    var parameterName = parameter.Name.GetName(Module);

                    if (constraint.Type.IsNil)
                    {
                        continue;
                    }

                    var constraintType = WrapperFactory.Create(constraint.Type, Module);
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

        public (ITypeNamedWrapper owner, MethodKind symbolKind) GetMethodSymbolKind()
        {
            var (accessorOwnerHandle, semanticsAttribute) = Module.MethodSemanticsLookup.GetSemantics(MethodDefinitionHandle);

            var accessorOwner = WrapperFactory.Create(accessorOwnerHandle, Module);

            var name = Name;
            var parameterCount = Parameters.Count;

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

            var attributes = Definition.Attributes;

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
    }
}
