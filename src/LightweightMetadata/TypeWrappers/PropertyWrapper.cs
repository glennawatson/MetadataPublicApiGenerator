// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading;
using LightweightMetadata.Extensions;

namespace LightweightMetadata.TypeWrappers
{
    /// <summary>
    /// A wrapper around the PropertyDefinition class.
    /// </summary>
    public class PropertyWrapper : IHandleTypeNamedWrapper, IHasAttributes
    {
        private readonly Lazy<string> _name;

        private readonly Lazy<IReadOnlyList<AttributeWrapper>> _attributes;
        private readonly Lazy<MethodWrapper> _getterMethod;
        private readonly Lazy<MethodWrapper> _setterMethod;
        private readonly Lazy<MethodWrapper> _anyAccessor;
        private readonly Lazy<TypeWrapper> _declaringType;
        private readonly Lazy<MethodSignature<IHandleTypeNamedWrapper>> _signature;
        private readonly Lazy<EntityAccessibility> _accessibility;

        private PropertyWrapper(PropertyDefinitionHandle handle, CompilationModule module)
        {
            PropertyDefinitionHandle = handle;
            CompilationModule = module;
            Handle = handle;
            Definition = Resolve();

            _name = new Lazy<string>(() => Definition.Name.GetName(module), LazyThreadSafetyMode.PublicationOnly);
            _attributes = new Lazy<IReadOnlyList<AttributeWrapper>>(() => AttributeWrapper.Create(Definition.GetCustomAttributes(), module), LazyThreadSafetyMode.PublicationOnly);

            _getterMethod = new Lazy<MethodWrapper>(() => MethodWrapper.Create(Definition.GetAccessors().Getter, module), LazyThreadSafetyMode.PublicationOnly);
            _setterMethod = new Lazy<MethodWrapper>(() => MethodWrapper.Create(Definition.GetAccessors().Setter, module), LazyThreadSafetyMode.PublicationOnly);

            _anyAccessor = new Lazy<MethodWrapper>(GetAnyAccessor, LazyThreadSafetyMode.PublicationOnly);

            _declaringType = new Lazy<TypeWrapper>(() => _anyAccessor.Value.DeclaringType, LazyThreadSafetyMode.PublicationOnly);

            _accessibility = new Lazy<EntityAccessibility>(GetAccessibility, LazyThreadSafetyMode.PublicationOnly);

            _signature = new Lazy<MethodSignature<IHandleTypeNamedWrapper>>(() => Definition.DecodeSignature(module.TypeProvider, new GenericContext(this)), LazyThreadSafetyMode.PublicationOnly);
        }

        /// <summary>
        /// Gets the resolved method definition.
        /// </summary>
        public PropertyDefinition Definition { get; }

        /// <summary>
        /// Gets the method definition handle.
        /// </summary>
        public PropertyDefinitionHandle PropertyDefinitionHandle { get; }

        /// <inheritdoc/>
        public IReadOnlyList<AttributeWrapper> Attributes => _attributes.Value;

        /// <inheritdoc />
        public string Name => _name.Value;

        /// <inheritdoc />
        public CompilationModule CompilationModule { get; }

        /// <inheritdoc/>
        public Handle Handle { get; }

        /// <inheritdoc />
        public string FullName => DeclaringType.FullName + "." + Name;

        /// <inheritdoc />
        public string ReflectionFullName => DeclaringType.ReflectionFullName + "." + Name;

        /// <inheritdoc />
        public string TypeNamespace => DeclaringType.TypeNamespace;

        /// <inheritdoc />
        public EntityAccessibility Accessibility => _accessibility.Value;

        /// <inheritdoc />
        public bool IsAbstract => Getter.IsAbstract || Setter.IsAbstract;

        /// <inheritdoc />
        public KnownTypeCode KnownType => KnownTypeCode.None;

        /// <summary>
        /// Gets the getter method for the property.
        /// </summary>
        public MethodWrapper Getter => _getterMethod.Value;

        /// <summary>
        /// Gets the setter method for the property.
        /// </summary>
        public MethodWrapper Setter => _setterMethod.Value;

        /// <summary>
        /// Gets any available accessor.
        /// </summary>
        public MethodWrapper AnyAccessor => _anyAccessor.Value;

        /// <summary>
        /// Gets the type that is declaring the property.
        /// </summary>
        public TypeWrapper DeclaringType => _declaringType.Value;

        /// <summary>
        /// Gets the return type of the property.
        /// </summary>
        public IHandleTypeNamedWrapper ReturnType => _signature.Value.ReturnType;

        /// <summary>
        /// Creates a instance of the method, if there is already not an instance.
        /// </summary>
        /// <param name="handle">The handle to the instance.</param>
        /// <param name="module">The module that contains the instance.</param>
        /// <returns>The wrapper.</returns>
        public static PropertyWrapper Create(PropertyDefinitionHandle handle, CompilationModule module)
        {
            if (handle.IsNil)
            {
                return null;
            }

            return new PropertyWrapper(handle, module);
        }

        /// <summary>
        /// Creates a array instances of a type.
        /// </summary>
        /// <param name="collection">The collection to create.</param>
        /// <param name="module">The module to use in creation.</param>
        /// <returns>The list of the type.</returns>
        public static IReadOnlyList<PropertyWrapper> Create(in PropertyDefinitionHandleCollection collection, CompilationModule module)
        {
            var output = new PropertyWrapper[collection.Count];

            int i = 0;
            foreach (var element in collection)
            {
                output[i] = Create(element, module);
                i++;
            }

            return output;
        }

        private PropertyDefinition Resolve()
        {
            return CompilationModule.MetadataReader.GetPropertyDefinition(PropertyDefinitionHandle);
        }

        private MethodWrapper GetAnyAccessor()
        {
            if (Getter != null)
            {
                return Getter;
            }

            return Setter;
        }

        private EntityAccessibility GetAccessibility()
        {
            EntityAccessibility MergePropertyAccessibility(EntityAccessibility left, EntityAccessibility right)
            {
                if (left == EntityAccessibility.Public || right == EntityAccessibility.Public)
                {
                    return EntityAccessibility.Public;
                }

                if (left == EntityAccessibility.ProtectedInternal || right == EntityAccessibility.ProtectedInternal)
                {
                    return EntityAccessibility.ProtectedInternal;
                }

                if ((left == EntityAccessibility.Protected && right == EntityAccessibility.Internal) ||
                    (left == EntityAccessibility.Internal && right == EntityAccessibility.Protected))
                {
                    return EntityAccessibility.ProtectedInternal;
                }

                if (left == EntityAccessibility.Protected || right == EntityAccessibility.Protected)
                {
                    return EntityAccessibility.Protected;
                }

                if (left == EntityAccessibility.Internal || right == EntityAccessibility.Internal)
                {
                    return EntityAccessibility.Internal;
                }

                if (left == EntityAccessibility.PrivateProtected || right == EntityAccessibility.PrivateProtected)
                {
                    return EntityAccessibility.PrivateProtected;
                }

                if (left == EntityAccessibility.Private || right == EntityAccessibility.Private)
                {
                    return EntityAccessibility.Private;
                }

                return left;
            }

            return MergePropertyAccessibility(
                Getter?.Accessibility ?? EntityAccessibility.None,
                Setter?.Accessibility ?? EntityAccessibility.None);
        }
    }
}
