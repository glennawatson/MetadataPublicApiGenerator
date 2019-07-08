// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Text;
using System.Threading;
using MetadataPublicApiGenerator.Extensions;

namespace MetadataPublicApiGenerator.Compilation.TypeWrappers
{
    internal class TypeReferenceWrapper : IHandleTypeNamedWrapper
    {
        private static readonly Dictionary<TypeReferenceHandle, TypeReferenceWrapper> _registerTypes = new Dictionary<TypeReferenceHandle, TypeReferenceWrapper>();

        private readonly Lazy<string> _name;
        private readonly Lazy<IHandleTypeNamedWrapper> _resolutionScope;
        private readonly Lazy<string> _namespace;
        private readonly Lazy<string> _fullName;

        private TypeReferenceWrapper(TypeReferenceHandle handle, CompilationModule module)
        {
            TypeReferenceHandle = handle;
            Module = module;
            Handle = handle;
            Definition = Resolve();

            _name = new Lazy<string>(() => Definition.Name.GetName(module), LazyThreadSafetyMode.PublicationOnly);
            _resolutionScope = new Lazy<IHandleTypeNamedWrapper>(() => WrapperFactory.Create(Definition.ResolutionScope, Module), LazyThreadSafetyMode.PublicationOnly);
            _namespace = new Lazy<string>(() => Module.MetadataReader.GetString(Definition.Namespace));
            _fullName = new Lazy<string>(GetFullName, LazyThreadSafetyMode.PublicationOnly);
        }

        /// <summary>
        /// Gets the resolved method definition.
        /// </summary>
        public TypeReference Definition { get; }

        /// <summary>
        /// Gets the method definition handle.
        /// </summary>
        public TypeReferenceHandle TypeReferenceHandle { get; }

        /// <inheritdoc />
        public string Name => _name.Value;

        /// <inheritdoc />
        public CompilationModule Module { get; }

        public Handle Handle { get; }

        public IHandleTypeNamedWrapper ResolutionScope => _resolutionScope.Value;

        /// <inheritdoc />
        public string Namespace => _namespace.Value;

        /// <inheritdoc />
        public string FullName => _fullName.Value;

        /// <inheritdoc />
        public bool IsPublic => ResolutionScope.Handle.Kind == HandleKind.TypeReference || ResolutionScope.IsPublic;

        /// <inheritdoc />
        public bool IsAbstract => ResolutionScope.Handle.Kind != HandleKind.TypeReference && ResolutionScope.IsAbstract;

        /// <summary>
        /// Creates a instance of the method, if there is already not an instance.
        /// </summary>
        /// <param name="handle">The handle to the instance.</param>
        /// <param name="module">The module that contains the instance.</param>
        /// <returns>The wrapper.</returns>
        public static TypeReferenceWrapper Create(TypeReferenceHandle handle, CompilationModule module)
        {
            if (handle.IsNil)
            {
                return null;
            }

            return _registerTypes.GetOrAdd(handle, handleCreate => new TypeReferenceWrapper(handleCreate, module));
        }

        private TypeReference Resolve()
        {
            return Module.MetadataReader.GetTypeReference(TypeReferenceHandle);
        }

        private string GetFullName()
        {
            var stringBuilder = new StringBuilder();
            var namespaceName = Namespace;

            if (!string.IsNullOrEmpty(namespaceName))
            {
                stringBuilder.Append(namespaceName).Append('.');
            }

            var list = new List<string>();
            var current = ResolutionScope;
            while (current != null)
            {
                var name = current.FullName;

                if (!string.IsNullOrWhiteSpace(name))
                {
                    list.Insert(0, name);
                }

                current = current.Handle.Kind == HandleKind.TypeReference ?
                    ((TypeReferenceWrapper)current).ResolutionScope :
                    default;
            }

            if (list.Count > 0)
            {
                stringBuilder.Append(string.Join(".", list)).Append('.');
            }

            stringBuilder.Append(Name);

            return stringBuilder.ToString();
        }
    }
}
