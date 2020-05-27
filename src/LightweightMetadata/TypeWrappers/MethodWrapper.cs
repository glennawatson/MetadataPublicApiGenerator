// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Threading;

namespace LightweightMetadata
{
    /// <summary>
    /// A wrapper around the MethodDefinition.
    /// </summary>
    public sealed class MethodWrapper : IHandleTypeNamedWrapper, IHasGenericParameters, IHasReturnAttributes
    {
        private readonly Lazy<string> _name;
        private readonly Lazy<string> _nameWithFullType;
        private readonly Lazy<MethodSignature<IHandleTypeNamedWrapper>> _signature;
        private readonly Lazy<TypeWrapper> _declaringType;
        private readonly Lazy<IReadOnlyList<ParameterWrapper>> _parameters;
        private readonly Lazy<IReadOnlyList<AttributeWrapper>> _attributes;
        private readonly Lazy<IReadOnlyList<GenericParameterWrapper>> _genericParameters;
        private readonly Lazy<(ITypeNamedWrapper? Owner, SymbolMethodKind SymbolKind)> _semanticData;
        private readonly Lazy<bool> _isDelegate;
        private readonly Lazy<bool> _isExtensionMethod;
        private readonly Lazy<EntityAccessibility> _accessibility;
        private readonly Lazy<IHandleTypeNamedWrapper?> _explicitType;
        private readonly Lazy<IReadOnlyList<AttributeWrapper>> _returnAttributes;

        private MethodWrapper(MethodDefinitionHandle handle, AssemblyMetadata assemblyMetadata)
        {
            MethodDefinitionHandle = handle;
            AssemblyMetadata = assemblyMetadata;
            Handle = handle;
            Definition = Resolve(handle, assemblyMetadata);

            IsAbstract = (Definition.Attributes & MethodAttributes.Abstract) != 0;
            IsStatic = (Definition.Attributes & MethodAttributes.Static) != 0;

            IsSealed = (Definition.Attributes & (MethodAttributes.Abstract | MethodAttributes.Final | MethodAttributes.NewSlot | MethodAttributes.Static)) == MethodAttributes.Final;

            IsOverride = (Definition.Attributes & (MethodAttributes.NewSlot | MethodAttributes.Virtual)) == MethodAttributes.Virtual;
            IsVirtual = (Definition.Attributes & (MethodAttributes.Abstract | MethodAttributes.Virtual | MethodAttributes.NewSlot | MethodAttributes.Final)) == (MethodAttributes.Virtual | MethodAttributes.NewSlot);

            _declaringType = new Lazy<TypeWrapper>(() => TypeWrapper.CreateChecked(Definition.GetDeclaringType(), assemblyMetadata), LazyThreadSafetyMode.PublicationOnly);
            _signature = new Lazy<MethodSignature<IHandleTypeNamedWrapper>>(() => Definition.DecodeSignature(assemblyMetadata.TypeProvider, new GenericContext(this)), LazyThreadSafetyMode.PublicationOnly);
            _nameWithFullType = new Lazy<string>(GetNameWithFullType, LazyThreadSafetyMode.PublicationOnly);
            _genericParameters = new Lazy<IReadOnlyList<GenericParameterWrapper>>(() => GenericParameterWrapper.Create(Definition.GetGenericParameters(), this, AssemblyMetadata), LazyThreadSafetyMode.PublicationOnly);
            _semanticData = new Lazy<(ITypeNamedWrapper?, SymbolMethodKind)>(GetMethodSymbolKind, LazyThreadSafetyMode.PublicationOnly);
            _parameters = new Lazy<IReadOnlyList<ParameterWrapper>>(GetParameters, LazyThreadSafetyMode.PublicationOnly);
            _attributes = new Lazy<IReadOnlyList<AttributeWrapper>>(() => AttributeWrapper.CreateChecked(Definition.GetCustomAttributes(), AssemblyMetadata), LazyThreadSafetyMode.PublicationOnly);
            _isDelegate = new Lazy<bool>(() => MethodKind == SymbolMethodKind.DelegateInvoke, LazyThreadSafetyMode.PublicationOnly);
            _isExtensionMethod = new Lazy<bool>(() => IsStatic & Attributes.HasKnownAttribute(KnownAttribute.Extension), LazyThreadSafetyMode.PublicationOnly);
            _accessibility = new Lazy<EntityAccessibility>(GetAccessibility, LazyThreadSafetyMode.PublicationOnly);
            _explicitType = new Lazy<IHandleTypeNamedWrapper?>(GetExplicitType, LazyThreadSafetyMode.PublicationOnly);
            _returnAttributes = new Lazy<IReadOnlyList<AttributeWrapper>>(() => Definition.GetParameters().GetReturnAttributes(AssemblyMetadata), LazyThreadSafetyMode.PublicationOnly);
            _name = new Lazy<string>(GetName, LazyThreadSafetyMode.PublicationOnly);
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
        /// Gets the name with any included explicit interface names.
        /// </summary>
        public string NameWithFullType => _nameWithFullType.Value;

        /// <summary>
        /// Gets the type that declares this method.
        /// </summary>
        public TypeWrapper DeclaringType => _declaringType.Value;

        /// <summary>
        /// Gets the type that this method returns.
        /// </summary>
        public IHandleTypeNamedWrapper ReturningType => _signature.Value.ReturnType;

        /// <summary>
        /// Gets the method kind.
        /// </summary>
        public SymbolMethodKind MethodKind => _semanticData.Value.SymbolKind;

        /// <summary>
        /// Gets the owner of the method.
        /// </summary>
        public ITypeNamedWrapper? Owner => _semanticData.Value.Owner;

        /// <inheritdoc />
        public IReadOnlyList<AttributeWrapper> Attributes => _attributes.Value;

        /// <inheritdoc />
        public string FullName => DeclaringType.FullName + "." + Name;

        /// <inheritdoc />
        public string ReflectionFullName => DeclaringType.ReflectionFullName + "." + Name;

        /// <inheritdoc />
        public string TypeNamespace => DeclaringType.TypeNamespace;

        /// <inheritdoc />
        public EntityAccessibility Accessibility => _accessibility.Value;

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
        /// Gets a value indicating whether the method is a extension method.
        /// </summary>
        public bool IsExtensionMethod => _isExtensionMethod.Value;

        /// <inheritdoc />
        public bool IsValueType => true;

        /// <summary>
        /// Gets the parameters of the method.
        /// </summary>
        public IReadOnlyList<ParameterWrapper> Parameters => _parameters.Value;

        /// <summary>
        /// Gets the parameter types for generic orientated methods.
        /// </summary>
        public IReadOnlyList<ITypeNamedWrapper> ParameterTypes => _signature.Value.ParameterTypes.ToArray();

        /// <summary>
        /// Gets any attributes which are prefixed with the return specifier.
        /// </summary>
        public IReadOnlyList<AttributeWrapper> ReturnAttributes => _returnAttributes.Value;

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
        public AssemblyMetadata AssemblyMetadata { get; }

        /// <inheritdoc />
        public Handle Handle { get; }

        /// <summary>
        /// Gets a value indicating whether the method is explicitly implemented.
        /// </summary>
        public bool IsExplicitImplementation => NameWithFullType.Contains(".") && !NameWithFullType.Equals(".ctor", StringComparison.InvariantCulture) && !NameWithFullType.Equals(".cctor", StringComparison.CurrentCultureIgnoreCase);

        /// <summary>
        /// Gets the explicit type if the value is implemented explicitly from a interface.
        /// </summary>
        public IHandleTypeNamedWrapper? ExplicitType => _explicitType.Value;

        /// <summary>
        /// Creates a instance of the method, if there is already not an instance.
        /// </summary>
        /// <param name="handle">The handle to the instance.</param>
        /// <param name="assemblyMetadata">The module that contains the instance.</param>
        /// <returns>The wrapper.</returns>
        public static MethodWrapper? Create(MethodDefinitionHandle handle, AssemblyMetadata assemblyMetadata)
        {
            if (handle.IsNil)
            {
                return null;
            }

            return new MethodWrapper(handle, assemblyMetadata);
        }

        /// <summary>
        /// Creates a instance of the method, if there is already not an instance.
        /// </summary>
        /// <param name="handle">The handle to the instance.</param>
        /// <param name="assemblyMetadata">The module that contains the instance.</param>
        /// <returns>The wrapper.</returns>
        public static MethodWrapper CreateChecked(MethodDefinitionHandle handle, AssemblyMetadata assemblyMetadata)
        {
            var instance = Create(handle, assemblyMetadata);

            if (instance == null)
            {
                throw new ArgumentException("Cannot create an instance of the method.", nameof(handle));
            }

            return instance;
        }

        /// <summary>
        /// Creates a array instances of a type.
        /// </summary>
        /// <param name="collection">The collection to create.</param>
        /// <param name="assemblyMetadata">The module to use in creation.</param>
        /// <returns>The list of the type.</returns>
        public static IReadOnlyList<MethodWrapper?> Create(in MethodDefinitionHandleCollection collection, AssemblyMetadata assemblyMetadata)
        {
            var output = new MethodWrapper?[collection.Count];

            int i = 0;
            foreach (var element in collection)
            {
                output[i] = Create(element, assemblyMetadata);
                i++;
            }

            return output;
        }

        /// <summary>
        /// Creates a array instances of a type.
        /// </summary>
        /// <param name="collection">The collection to create.</param>
        /// <param name="assemblyMetadata">The module to use in creation.</param>
        /// <returns>The list of the type.</returns>
        public static IReadOnlyList<MethodWrapper> CreateChecked(in MethodDefinitionHandleCollection collection, AssemblyMetadata assemblyMetadata)
        {
            var entities = Create(collection, assemblyMetadata);

            if (entities.Any(x => x is null))
            {
                throw new ArgumentException("Have invalid methods.", nameof(collection));
            }

            return entities.Select(x => x!).ToList();
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return FullName;
        }

        private static MethodDefinition Resolve(MethodDefinitionHandle handle, AssemblyMetadata assemblyMetadata)
        {
            return assemblyMetadata.MetadataReader.GetMethodDefinition(handle);
        }

        private string GetNameWithFullType()
        {
            return Resolve(MethodDefinitionHandle, AssemblyMetadata).Name.GetName(AssemblyMetadata);
        }

        private (ITypeNamedWrapper? Owner, SymbolMethodKind SymbolKind) GetMethodSymbolKind()
        {
            var (accessorOwnerHandle, semanticsAttribute) = AssemblyMetadata.MethodSemanticsLookup.GetSemantics(MethodDefinitionHandle);

            var accessorOwner = WrapperFactory.Create(accessorOwnerHandle, AssemblyMetadata);

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

            if (IsExplicitImplementation)
            {
                return (default, SymbolMethodKind.ExplicitInterfaceImplementation);
            }

            return (default, SymbolMethodKind.Ordinary);
        }

        private IReadOnlyList<ParameterWrapper> GetParameters()
        {
            var parameterCollection = Definition.GetParameters();

            var parameterList = new List<ParameterWrapper>(parameterCollection.Count);
            int i = 0;
            foreach (var parameterHandle in parameterCollection)
            {
                var parameterInstance = AssemblyMetadata.MetadataReader.GetParameter(parameterHandle);

                if (parameterInstance.SequenceNumber > 0)
                {
                    if (parameterInstance.SequenceNumber > 0 && i < _signature.Value.RequiredParameterCount)
                    {
                        var parameterType = _signature.Value.ParameterTypes[i];

                        var parameter = ParameterWrapper.CreateChecked(parameterHandle, parameterType, AssemblyMetadata);

                        parameterList.Add(parameter);
                    }

                    i++;
                }
            }

            return parameterList;
        }

        private EntityAccessibility GetAccessibility()
        {
            if (IsExplicitImplementation)
            {
                return ExplicitType?.Accessibility ?? EntityAccessibility.None;
            }

            switch (Definition.Attributes & MethodAttributes.MemberAccessMask)
            {
                case MethodAttributes.Public:
                    return EntityAccessibility.Public;
                case MethodAttributes.Assembly:
                    return EntityAccessibility.Internal;
                case MethodAttributes.Private:
                    return EntityAccessibility.Private;
                case MethodAttributes.Family:
                    return EntityAccessibility.Protected;
                case MethodAttributes.FamANDAssem:
                    return EntityAccessibility.PrivateProtected;
                case MethodAttributes.FamORAssem:
                    return EntityAccessibility.ProtectedInternal;
                default:
                    return EntityAccessibility.None;
            }
        }

        private IHandleTypeNamedWrapper? GetExplicitType()
        {
            int lastDot = NameWithFullType.LastIndexOf('.');
            if (IsExplicitImplementation && lastDot >= 0)
            {
                var typeName = NameWithFullType.Substring(0, lastDot);

                return AssemblyMetadata.MetadataRepository.GetTypeByName(typeName);
            }

            return null;
        }

        private string GetName()
        {
            if (!IsExplicitImplementation)
            {
                return NameWithFullType;
            }

            var index = NameWithFullType.LastIndexOf('.');

            if (index >= 0)
            {
                return NameWithFullType.Substring(index + 1);
            }

            return NameWithFullType;
        }
    }
}
