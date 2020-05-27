﻿// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Metadata;
using System.Threading;

namespace LightweightMetadata
{
    /// <summary>
    /// Type parameter of a generic class/method.
    /// </summary>
    public class GenericParameterWrapper : IHandleTypeNamedWrapper
    {
        private readonly Lazy<IReadOnlyList<GenericParameterConstraintWrapper>> _constraints;
        private readonly Lazy<IReadOnlyList<AttributeWrapper>> _attributes;
        private readonly Lazy<IHandleTypeNamedWrapper?> _parent;
        private readonly Lazy<string> _name;
        private readonly GenericParameterAttributes _genericParameterAttribute;

        private GenericParameterWrapper(AssemblyMetadata assemblyMetadata, IHandleTypeNamedWrapper owner, GenericParameterHandle handle, GenericParameterAttributes genericParameterAttribute)
        {
            AssemblyMetadata = assemblyMetadata;
            Owner = owner;
            Handle = handle;
            _genericParameterAttribute = genericParameterAttribute;
            GenericParameter = assemblyMetadata.MetadataReader.GetGenericParameter(handle);

            _attributes = new Lazy<IReadOnlyList<AttributeWrapper>>(() => AttributeWrapper.CreateChecked(GenericParameter.GetCustomAttributes(), assemblyMetadata), LazyThreadSafetyMode.PublicationOnly);
            _parent = new Lazy<IHandleTypeNamedWrapper?>(() => WrapperFactory.Create(GenericParameter.Parent, assemblyMetadata), LazyThreadSafetyMode.PublicationOnly);
            _name = new Lazy<string>(() => GenericParameter.Name.GetName(assemblyMetadata));
            switch (genericParameterAttribute & GenericParameterAttributes.VarianceMask)
            {
                case GenericParameterAttributes.Contravariant:
                    Variance = VarianceType.Contravariant;
                    break;
                case GenericParameterAttributes.Covariant:
                    Variance = VarianceType.Covariant;
                    break;
                default:
                    Variance = VarianceType.Invariant;
                    break;
            }

            _constraints = new Lazy<IReadOnlyList<GenericParameterConstraintWrapper>>(() => GenericParameterConstraintWrapper.CreateChecked(GenericParameter.GetConstraints(), this, AssemblyMetadata), LazyThreadSafetyMode.PublicationOnly);
        }

        /// <summary>
        /// Gets the parent of the parameter.
        /// </summary>
        public IHandleTypeNamedWrapper? Parent => _parent.Value;

        /// <summary>
        /// Gets the handle to the owner.
        /// </summary>
        public IHandleTypeNamedWrapper Owner { get; }

        /// <summary>
        /// Gets the handle to the type parameter.
        /// </summary>
        public Handle Handle { get; }

        /// <summary>
        /// Gets the variance of the type parameter.
        /// </summary>
        public VarianceType Variance { get; }

        /// <summary>
        /// Gets a set of attributes about the type.
        /// </summary>
        public IReadOnlyList<AttributeWrapper> Attributes => _attributes.Value;

        /// <summary>
        /// Gets the generic parameter instance.
        /// </summary>
        public GenericParameter GenericParameter { get; }

        /// <inheritdoc />
        public AssemblyMetadata AssemblyMetadata { get; }

        /// <summary>
        /// Gets a list of constraints.
        /// </summary>
        public IReadOnlyList<GenericParameterConstraintWrapper> Constraints => _constraints.Value;

        /// <summary>
        /// Gets the ordering index of the parameter.
        /// </summary>
        public int Index => GenericParameter.Index;

        /// <inheritdoc />
        public string Name => _name.Value;

        /// <inheritdoc />
        public string FullName => Owner.FullName + "." + Name;

        /// <inheritdoc />
        public string ReflectionFullName => Name;

        /// <inheritdoc />
        public string? TypeNamespace => Owner.TypeNamespace;

        /// <inheritdoc />
        public EntityAccessibility Accessibility => EntityAccessibility.Public;

        /// <inheritdoc />
        public bool IsValueType => true;

        /// <inheritdoc />
        public bool IsAbstract => false;

        /// <inheritdoc />
        public KnownTypeCode KnownType => KnownTypeCode.None;

        /// <summary>
        /// Gets a value indicating whether this has a constraint that there must be a default constructor.
        /// </summary>
        public bool HasDefaultConstructorConstraint => (_genericParameterAttribute & GenericParameterAttributes.DefaultConstructorConstraint) != 0;

        /// <summary>
        /// Gets a value indicating whether this must be a reference type.
        /// </summary>
        public bool HasReferenceTypeConstraint => (_genericParameterAttribute & GenericParameterAttributes.ReferenceTypeConstraint) != 0;

        /// <summary>
        /// Gets a value indicating whether this must be a value type.
        /// </summary>
        public bool HasValueTypeConstraint => (_genericParameterAttribute & GenericParameterAttributes.NotNullableValueTypeConstraint) != 0;

        /// <summary>
        /// Creates a array instances of a type.
        /// </summary>
        /// <param name="collection">The collection to create.</param>
        /// <param name="owner">The owner of the generic property.</param>
        /// <param name="assemblyMetadata">The module to use in creation.</param>
        /// <returns>The list of the type.</returns>
        public static IReadOnlyList<GenericParameterWrapper> Create(in GenericParameterHandleCollection collection, IHandleTypeNamedWrapper owner, AssemblyMetadata assemblyMetadata)
        {
            var output = new GenericParameterWrapper[collection.Count];

            int i = 0;
            foreach (var element in collection)
            {
                output[i] = Create(element, owner, i, assemblyMetadata);
                i++;
            }

            return output;
        }

        /// <summary>
        /// Creates the instances of the <see cref="GenericParameterWrapper"/> from the specified handle.
        /// </summary>
        /// <param name="handle">The type parameter handle to wrap.</param>
        /// <param name="owner">The owner of the type parameter.</param>
        /// <param name="index">The index of the type parameter.</param>
        /// <param name="assemblyMetadata">The module that owns the type.</param>
        /// <returns>A <see cref="GenericParameterWrapper"/>.</returns>
        public static GenericParameterWrapper Create(GenericParameterHandle handle, IHandleTypeNamedWrapper owner, int index, AssemblyMetadata assemblyMetadata)
        {
            if (assemblyMetadata is null)
            {
                throw new ArgumentNullException(nameof(assemblyMetadata));
            }

            var genericParameter = assemblyMetadata.MetadataReader.GetGenericParameter(handle);
            var name = genericParameter.Name.GetName(assemblyMetadata);
            Debug.Assert(genericParameter.Index == index, "The index must match on the generic parameter: " + name);
            return new GenericParameterWrapper(assemblyMetadata, owner, handle, genericParameter.Attributes);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return FullName;
        }
    }
}
