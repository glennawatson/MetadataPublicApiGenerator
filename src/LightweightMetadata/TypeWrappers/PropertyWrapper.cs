// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading;

namespace LightweightMetadata
{
    /// <summary>
    /// A wrapper around the PropertyDefinition class.
    /// </summary>
    public class PropertyWrapper : IHandleTypeNamedWrapper, IHasAttributes
    {
        private readonly Lazy<string> _name;

        private readonly Lazy<IReadOnlyList<AttributeWrapper>> _attributes;
        private readonly Lazy<MethodWrapper?> _getterMethod;
        private readonly Lazy<MethodWrapper?> _setterMethod;
        private readonly Lazy<MethodWrapper> _anyAccessor;
        private readonly Lazy<TypeWrapper> _declaringType;
        private readonly Lazy<MethodSignature<IHandleTypeNamedWrapper>> _signature;
        private readonly Lazy<EntityAccessibility> _accessibility;

        private PropertyWrapper(PropertyDefinitionHandle handle, AssemblyMetadata assemblyMetadata)
        {
            PropertyDefinitionHandle = handle;
            AssemblyMetadata = assemblyMetadata;
            Handle = handle;
            Definition = Resolve();

            _name = new Lazy<string>(() => Definition.Name.GetName(assemblyMetadata), LazyThreadSafetyMode.PublicationOnly);
            _attributes = new Lazy<IReadOnlyList<AttributeWrapper>>(() => AttributeWrapper.CreateChecked(Definition.GetCustomAttributes(), assemblyMetadata), LazyThreadSafetyMode.PublicationOnly);

            _getterMethod = new Lazy<MethodWrapper?>(() => MethodWrapper.Create(Definition.GetAccessors().Getter, assemblyMetadata), LazyThreadSafetyMode.PublicationOnly);
            _setterMethod = new Lazy<MethodWrapper?>(() => MethodWrapper.Create(Definition.GetAccessors().Setter, assemblyMetadata), LazyThreadSafetyMode.PublicationOnly);

            _anyAccessor = new Lazy<MethodWrapper>(GetAnyAccessor, LazyThreadSafetyMode.PublicationOnly);

            _declaringType = new Lazy<TypeWrapper>(() => AnyAccessor.DeclaringType, LazyThreadSafetyMode.PublicationOnly);

            _accessibility = new Lazy<EntityAccessibility>(GetAccessibility, LazyThreadSafetyMode.PublicationOnly);

            _signature = new Lazy<MethodSignature<IHandleTypeNamedWrapper>>(() => Definition.DecodeSignature(assemblyMetadata.TypeProvider, new GenericContext(this)), LazyThreadSafetyMode.PublicationOnly);
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
        public AssemblyMetadata AssemblyMetadata { get; }

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
        public bool IsAbstract => (Getter?.IsAbstract ?? false) || (Setter?.IsAbstract ?? false);

        /// <inheritdoc />
        public KnownTypeCode KnownType => KnownTypeCode.None;

        /// <summary>
        /// Gets the getter method for the property.
        /// </summary>
        public MethodWrapper? Getter => _getterMethod.Value;

        /// <summary>
        /// Gets the setter method for the property.
        /// </summary>
        public MethodWrapper? Setter => _setterMethod.Value;

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

        /// <inheritdoc />
        public bool IsValueType => ReturnType.IsValueType;

        /// <summary>
        /// Creates a instance of the method, if there is already not an instance.
        /// </summary>
        /// <param name="handle">The handle to the instance.</param>
        /// <param name="assemblyMetadata">The module that contains the instance.</param>
        /// <returns>The wrapper.</returns>
        public static PropertyWrapper? Create(PropertyDefinitionHandle handle, AssemblyMetadata assemblyMetadata)
        {
            if (handle.IsNil)
            {
                return null;
            }

            return new PropertyWrapper(handle, assemblyMetadata);
        }

        /// <summary>
        /// Creates a array instances of a type.
        /// </summary>
        /// <param name="collection">The collection to create.</param>
        /// <param name="assemblyMetadata">The module to use in creation.</param>
        /// <returns>The list of the type.</returns>
        public static IReadOnlyList<PropertyWrapper?> Create(in PropertyDefinitionHandleCollection collection, AssemblyMetadata assemblyMetadata)
        {
            var output = new PropertyWrapper?[collection.Count];

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
        public static IReadOnlyList<PropertyWrapper> CreateChecked(in PropertyDefinitionHandleCollection collection, AssemblyMetadata assemblyMetadata)
        {
            var entities = Create(collection, assemblyMetadata);

            if (entities.Any(x => x is null))
            {
                throw new ArgumentException("Have invalid properties.", nameof(collection));
            }

            return entities.Select(x => x!).ToList();
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return FullName;
        }

        private PropertyDefinition Resolve()
        {
            return AssemblyMetadata.MetadataReader.GetPropertyDefinition(PropertyDefinitionHandle);
        }

        private MethodWrapper GetAnyAccessor()
        {
            if (Getter != null)
            {
                return Getter;
            }

            if (Setter != null)
            {
                return Setter;
            }

            throw new Exception("Cannot find a getter or setter on the property.");
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
