// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Threading;
using LightweightMetadata.Extensions;

namespace LightweightMetadata.TypeWrappers
{
    /// <summary>
    /// Type parameter of a generic class/method.
    /// </summary>
    [DebuggerDisplay("{" + nameof(FullName) + "}")]
    public class TypeParameterWrapper : IHandleTypeNamedWrapper
    {
        private readonly Lazy<IReadOnlyList<string>> _constraints;
        private readonly Lazy<IHandleTypeNamedWrapper> _owner;

        private TypeParameterWrapper(CompilationModule module, EntityHandle owner, int index, string name, GenericParameterHandle handle, GenericParameterAttributes attr)
        {
            CompilationModule = module;
            Owner = owner;
            Handle = handle;
            Attributes = attr;
            Name = name;
            Index = index;
            GenericParameter = module.MetadataReader.GetGenericParameter(handle);

            _owner = new Lazy<IHandleTypeNamedWrapper>(() => WrapperFactory.Create(owner, module));
            _constraints = new Lazy<IReadOnlyList<string>>(GetConstraints, LazyThreadSafetyMode.PublicationOnly);
        }

        /// <summary>
        /// Gets the handle to the owner.
        /// </summary>
        public Handle Owner { get; }

        /// <summary>
        /// Gets the handle to the type parameter.
        /// </summary>
        public Handle Handle { get; }

        /// <summary>
        /// Gets a set of attributes about the type.
        /// </summary>
        public GenericParameterAttributes Attributes { get; }

        /// <summary>
        /// Gets the generic parameter instance.
        /// </summary>
        public GenericParameter GenericParameter { get; }

        /// <inheritdoc />
        public CompilationModule CompilationModule { get; }

        /// <summary>
        /// Gets a list of constraints.
        /// </summary>
        public IReadOnlyList<string> Constraints => _constraints.Value;

        /// <summary>
        /// Gets the ordering index of the parameter.
        /// </summary>
        public int Index { get; }

        /// <inheritdoc />
        public string Name { get; }

        /// <summary>
        /// Gets the instance of the owner.
        /// </summary>
        public IHandleTypeNamedWrapper OwnerInstance => _owner.Value;

        /// <inheritdoc />
        public string FullName => OwnerInstance.FullName + "." + Name;

        /// <inheritdoc />
        public string ReflectionFullName => OwnerInstance.ReflectionFullName + "." + Name;

        /// <inheritdoc />
        public string TypeNamespace => OwnerInstance.TypeNamespace;

        /// <inheritdoc />
        public bool IsPublic => true;

        /// <inheritdoc />
        public bool IsAbstract => false;

        /// <summary>
        /// Creates the instances of the <see cref="TypeParameterWrapper"/> from the specified handles.
        /// </summary>
        /// <param name="module">The module that owns the type.</param>
        /// <param name="owner">The owner of the type parameters.</param>
        /// <param name="handles">The type parameter handles to wrap individually.</param>
        /// <returns>A list of <see cref="TypeParameterWrapper"/>.</returns>
        public static IReadOnlyList<TypeParameterWrapper> Create(CompilationModule module, EntityHandle owner, GenericParameterHandleCollection handles)
        {
            if (handles.Count == 0)
            {
                return ImmutableArray<TypeParameterWrapper>.Empty;
            }

            var builder = ImmutableArray.CreateBuilder<TypeParameterWrapper>(handles.Count);

            int i = 0;
            foreach (var handle in handles)
            {
                builder.Add(Create(module, owner, i, handle));
                i++;
            }

            return builder.ToImmutable();
        }

        /// <summary>
        /// Creates the instances of the <see cref="TypeParameterWrapper"/> from the specified handle.
        /// </summary>
        /// <param name="module">The module that owns the type.</param>
        /// <param name="owner">The owner of the type parameter.</param>
        /// <param name="index">The index of the type parameter.</param>
        /// <param name="handle">The type parameter handle to wrap.</param>
        /// <returns>A <see cref="TypeParameterWrapper"/>.</returns>
        public static TypeParameterWrapper Create(CompilationModule module, EntityHandle owner, int index, GenericParameterHandle handle)
        {
            if (module == null)
            {
                throw new ArgumentNullException(nameof(module));
            }

            var genericParameter = module.MetadataReader.GetGenericParameter(handle);
            var name = genericParameter.Name.GetName(module);
            Debug.Assert(genericParameter.Index == index, "The index must match on the generic parameter: " + name);
            return new TypeParameterWrapper(module, owner, index, name, handle, genericParameter.Attributes);
        }

        private IReadOnlyList<string> GetConstraints()
        {
            var constraints = new HashSet<string>();
            foreach (var constraint in GenericParameter.GetConstraints().Select(x => CompilationModule.MetadataReader.GetGenericParameterConstraint(x)))
            {
                if (constraint.Type.IsNil)
                {
                    continue;
                }

                var constraintType = WrapperFactory.Create(constraint.Type, CompilationModule);
                if (constraintType.FullName != "System.Object")
                {
                    constraints.Add(constraintType.FullName);
                }
            }

            return constraints.ToList();
        }
    }
}
