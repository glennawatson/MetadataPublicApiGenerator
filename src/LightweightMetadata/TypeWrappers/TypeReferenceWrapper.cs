// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading;

namespace LightweightMetadata
{
    /// <summary>
    /// A wrapper around the TypeReference class.
    /// </summary>
    public class TypeReferenceWrapper : IHandleNameWrapper
    {
        private static readonly ConcurrentDictionary<(TypeReferenceHandle Handle, AssemblyMetadata AssemblyMetadata), TypeReferenceWrapper> _registerTypes = new ConcurrentDictionary<(TypeReferenceHandle, AssemblyMetadata), TypeReferenceWrapper>();

        private readonly Lazy<string> _name;
        private readonly Lazy<string> _fullName;
        private readonly Lazy<string> _namespace;
        private readonly Lazy<AssemblyMetadata?> _declaringModule;
        private readonly Lazy<IHandleTypeNamedWrapper> _type;

        private TypeReferenceWrapper(TypeReferenceHandle handle, AssemblyMetadata assemblyMetadata)
        {
            TypeReferenceHandle = handle;
            AssemblyMetadata = assemblyMetadata;
            Handle = handle;
            Definition = Resolve();
            _type = new Lazy<IHandleTypeNamedWrapper>(GetDeclaringType, LazyThreadSafetyMode.PublicationOnly);
            _namespace = new Lazy<string>(() => Definition.Namespace.GetName(assemblyMetadata), LazyThreadSafetyMode.PublicationOnly);
            _name = new Lazy<string>(() => Definition.Name.GetName(assemblyMetadata), LazyThreadSafetyMode.PublicationOnly);
            _declaringModule = new Lazy<AssemblyMetadata?>(GetDeclaringModule, LazyThreadSafetyMode.PublicationOnly);
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
        public AssemblyMetadata AssemblyMetadata { get; }

        /// <inheritdoc />
        public Handle Handle { get; }

        /// <summary>
        /// Gets the full name of the type.
        /// </summary>
        public string FullName => _fullName.Value;

        /// <summary>
        /// Gets the types namespace.
        /// </summary>
        public string TypeNamespace => _namespace.Value;

        /// <summary>
        /// Gets the type if available of this reference.
        /// </summary>
        public IHandleTypeNamedWrapper Type => _type.Value;

        /// <summary>
        /// Gets the declaring module.
        /// </summary>
        public AssemblyMetadata? DeclaringModule => _declaringModule.Value;

        /// <summary>
        /// Creates a instance of the method, if there is already not an instance.
        /// </summary>
        /// <param name="handle">The handle to the instance.</param>
        /// <param name="assemblyMetadata">The module that contains the instance.</param>
        /// <returns>The wrapper.</returns>
        public static TypeReferenceWrapper? Create(TypeReferenceHandle handle, AssemblyMetadata assemblyMetadata)
        {
            if (handle.IsNil)
            {
                return null;
            }

            return _registerTypes.GetOrAdd((handle, assemblyMetadata), data => new TypeReferenceWrapper(data.Handle, data.AssemblyMetadata));
        }

        /// <summary>
        /// Creates a array instances of a type.
        /// </summary>
        /// <param name="collection">The collection to create.</param>
        /// <param name="assemblyMetadata">The module to use in creation.</param>
        /// <returns>The list of the type.</returns>
        public static IReadOnlyList<TypeReferenceWrapper?> Create(in TypeReferenceHandleCollection collection, AssemblyMetadata assemblyMetadata)
        {
            var output = new TypeReferenceWrapper?[collection.Count];

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
        public static IReadOnlyList<TypeReferenceWrapper> CreateChecked(in TypeReferenceHandleCollection collection, AssemblyMetadata assemblyMetadata)
        {
            var entities = Create(collection, assemblyMetadata);

            if (entities.Any(x => x is null))
            {
                throw new ArgumentException("Have invalid type references.", nameof(collection));
            }

            return entities.Select(x => x!).ToList();
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return FullName;
        }

        private AssemblyMetadata? GetDeclaringModule()
        {
            TypeReferenceWrapper? current = this;

            while (current != null)
            {
                switch (current.Definition.ResolutionScope.Kind)
                {
                    case HandleKind.TypeReference:
                        current = Create((TypeReferenceHandle)current.Definition.ResolutionScope, current.AssemblyMetadata);
                        break;
                    case HandleKind.AssemblyReference:
                        var assemblyReference = AssemblyReferenceWrapper.Create((AssemblyReferenceHandle)current.Definition.ResolutionScope, current.AssemblyMetadata);
                        return MetadataRepository.GetAssemblyMetadataForAssemblyReference(assemblyReference);
                    case HandleKind.ModuleReference:
                        var assemblyModuleReference = ModuleReferenceWrapper.Create((ModuleReferenceHandle)current.Definition.ResolutionScope, current.AssemblyMetadata);

                        if (assemblyModuleReference != null)
                        {
                            return assemblyModuleReference.AssemblyMetadata;
                        }

                        break;
                    default:
                        return default;
                }
            }

            return default;
        }

        private IHandleTypeNamedWrapper GetDeclaringType()
        {
            var declaredType = AssemblyMetadata.GetTypeByName(FullName);

            if (declaredType == null)
            {
                throw new Exception("Cannot find valid declaring type for " + FullName);
            }

            return declaredType;
        }

        private TypeReference Resolve()
        {
            return AssemblyMetadata.MetadataReader.GetTypeReference(TypeReferenceHandle);
        }

        private string GetFullName()
        {
            if (Definition.ResolutionScope.IsNil || Definition.ResolutionScope.Kind != HandleKind.TypeReference)
            {
                return TypeNamespace + "." + Name;
            }

            var typeReference = Create((TypeReferenceHandle)Definition.ResolutionScope, AssemblyMetadata);

            if (typeReference == null)
            {
                throw new Exception("Cannot generate valid FullName for " + Name);
            }

            return typeReference.FullName + "." + Name;
        }
    }
}
