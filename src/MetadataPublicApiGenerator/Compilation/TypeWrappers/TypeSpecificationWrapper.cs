// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading;
using System.Xml.Schema;
using MetadataPublicApiGenerator.Extensions;

namespace MetadataPublicApiGenerator.Compilation.TypeWrappers
{
    internal class TypeSpecificationWrapper : IHandleTypeNamedWrapper, IHasAttributes
    {
        private static readonly Dictionary<TypeSpecificationHandle, TypeSpecificationWrapper> _registerTypes = new Dictionary<TypeSpecificationHandle, TypeSpecificationWrapper>();

        private readonly Lazy<IReadOnlyList<AttributeWrapper>> _attributes;
        private readonly Lazy<IHandleTypeNamedWrapper> _type;

        private TypeSpecificationWrapper(TypeSpecificationHandle handle, CompilationModule module)
        {
            TypeSpecificationHandle = handle;
            Module = module;
            Handle = handle;
            Definition = Resolve();

            _attributes = new Lazy<IReadOnlyList<AttributeWrapper>>(() => Definition.GetCustomAttributes().Select(x => AttributeWrapper.Create(x, Module)).ToList(), LazyThreadSafetyMode.PublicationOnly);

            _type = new Lazy<IHandleTypeNamedWrapper>(() => Definition.DecodeSignature(module.TypeProvider, new GenericContext(this)), LazyThreadSafetyMode.PublicationOnly);
        }

        /// <summary>
        /// Gets the resolved method definition.
        /// </summary>
        public TypeSpecification Definition { get; }

        /// <summary>
        /// Gets the method definition handle.
        /// </summary>
        public TypeSpecificationHandle TypeSpecificationHandle { get; }

        /// <inheritdoc />
        public string Name => Type.Name;

        /// <inheritdoc />
        public CompilationModule Module { get; }

        public Handle Handle { get; }

        public IReadOnlyList<AttributeWrapper> Attributes => _attributes.Value;

        public IHandleTypeNamedWrapper Type => _type.Value;

        /// <inheritdoc />
        public string FullName => Type.FullName;

        /// <inheritdoc />
        public string Namespace => Type.Namespace;

        /// <inheritdoc />
        public bool IsPublic => Type.IsPublic;

        /// <inheritdoc />
        public bool IsAbstract => Type.IsAbstract;

        /// <summary>
        /// Creates a instance of the method, if there is already not an instance.
        /// </summary>
        /// <param name="handle">The handle to the instance.</param>
        /// <param name="module">The module that contains the instance.</param>
        /// <returns>The wrapper.</returns>
        public static TypeSpecificationWrapper Create(TypeSpecificationHandle handle, CompilationModule module)
        {
            if (handle.IsNil)
            {
                return null;
            }

            return _registerTypes.GetOrAdd(handle, handleCreate => new TypeSpecificationWrapper(handleCreate, module));
        }

        private TypeSpecification Resolve()
        {
            return Module.MetadataReader.GetTypeSpecification(TypeSpecificationHandle);
        }
    }
}
