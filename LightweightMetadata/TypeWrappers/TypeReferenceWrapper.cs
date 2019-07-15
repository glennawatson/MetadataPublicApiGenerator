// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection.Metadata;
using System.Text;
using System.Threading;
using LightweightMetadata.Extensions;

namespace LightweightMetadata.TypeWrappers
{
    /// <summary>
    /// A wrapper around the TypeReference class.
    /// </summary>
    public class TypeReferenceWrapper : IHandleNameWrapper
    {
        private static readonly Dictionary<TypeReferenceHandle, TypeReferenceWrapper> _registerTypes = new Dictionary<TypeReferenceHandle, TypeReferenceWrapper>();

        private readonly Lazy<string> _name;
        private readonly Lazy<IHandleTypeNamedWrapper> _resolutionScope;
        private readonly Lazy<AssemblyReferenceWrapper> _declaringModule;

        private TypeReferenceWrapper(TypeReferenceHandle handle, CompilationModule module)
        {
            TypeReferenceHandle = handle;
            CompilationModule = module;
            Handle = handle;
            Definition = Resolve();

            _name = new Lazy<string>(() => Definition.Name.GetName(module), LazyThreadSafetyMode.PublicationOnly);
            _resolutionScope = new Lazy<IHandleTypeNamedWrapper>(() => WrapperFactory.Create(Definition.ResolutionScope, CompilationModule), LazyThreadSafetyMode.PublicationOnly);
            _declaringModule = new Lazy<AssemblyReferenceWrapper>(() => GetDeclaringModule(this), LazyThreadSafetyMode.PublicationOnly);
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
        public CompilationModule CompilationModule { get; }

        /// <inheritdoc />
        public Handle Handle { get; }

        /// <summary>
        /// Gets the resolution scope of the type reference.
        /// </summary>
        public IHandleTypeNamedWrapper ResolutionScope => _resolutionScope.Value;

        /// <summary>
        /// Gets the declaring module.
        /// </summary>
        public AssemblyReferenceWrapper DeclaringModule => _declaringModule.Value;

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

        /// <summary>
        /// Creates a array instances of a type.
        /// </summary>
        /// <param name="collection">The collection to create.</param>
        /// <param name="module">The module to use in creation.</param>
        /// <returns>The list of the type.</returns>
        public static IReadOnlyList<TypeReferenceWrapper> Create(in TypeReferenceHandleCollection collection, CompilationModule module)
        {
            var output = new TypeReferenceWrapper[collection.Count];

            int i = 0;
            foreach (var element in collection)
            {
                output[i] = Create(element, module);
                i++;
            }

            return output;
        }

        private static AssemblyReferenceWrapper GetDeclaringModule(TypeReferenceWrapper typeReference)
        {
            switch (typeReference.Definition.ResolutionScope.Kind)
            {
                case HandleKind.TypeReference:
                    var typeReferenceScope = TypeReferenceWrapper.Create((TypeReferenceHandle)typeReference.Definition.ResolutionScope, typeReference.CompilationModule);
                    return GetDeclaringModule(typeReferenceScope);
                case HandleKind.AssemblyReference:
                    var asmRef = AssemblyReferenceWrapper.Create((AssemblyReferenceHandle)typeReference.Definition.ResolutionScope, typeReference.CompilationModule);
                    return asmRef;
                default:
                    return default;
            }
        }

        private TypeReference Resolve()
        {
            return CompilationModule.MetadataReader.GetTypeReference(TypeReferenceHandle);
        }
    }
}
