// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text;
using MetadataPublicApiGenerator.Extensions;

namespace MetadataPublicApiGenerator.Compilation.TypeWrappers
{
    /// <summary>
    /// Type parameter of a generic class/method.
    /// </summary>
    internal class TypeParameterWrapper : IHandleWrapper, ITypeNamedWrapper
    {
        private TypeParameterWrapper(CompilationModule module, Handle owner, int index, string name, GenericParameterHandle handle, GenericParameterAttributes attr)
        {
            Module = module;
            Owner = owner;
            Handle = handle;
            Attributes = attr;
            Name = name;
            Index = index;
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

        /// <inheritdoc />
        public CompilationModule Module { get; }

        /// <summary>
        /// Gets the ordering index of the parameter.
        /// </summary>
        public int Index { get; }

        /// <inheritdoc />
        public string Name { get; }

        public string FullName => Name;

        public string Namespace => string.Empty;

        /// <inheritdoc />
        public bool IsKnownType => false;

        /// <summary>
        /// Creates the instances of the <see cref="TypeParameterWrapper"/> from the specified handles.
        /// </summary>
        /// <param name="module">The module that owns the type.</param>
        /// <param name="owner">The owner of the type parameters.</param>
        /// <param name="handles">The type parameter handles to wrap individually.</param>
        /// <returns>A list of <see cref="TypeParameterWrapper"/>.</returns>
        public static ImmutableArray<TypeParameterWrapper> Create(CompilationModule module, Handle owner, GenericParameterHandleCollection handles)
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
        public static TypeParameterWrapper Create(CompilationModule module, Handle owner, int index, GenericParameterHandle handle)
        {
            var genericParameter = handle.Resolve(module);
            var name = handle.GetName(module);
            Debug.Assert(genericParameter.Index == index, "The index must match on the generic parameter: " + name);
            return new TypeParameterWrapper(module, owner, index, name, handle, genericParameter.Attributes);
        }
    }
}
