// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection.Metadata;
using System.Threading;
using LightweightMetadata.Extensions;

namespace LightweightMetadata.TypeWrappers
{
    /// <summary>
    /// A wrapper around the TypeReference class.
    /// </summary>
    public class TypeReferenceWrapper : IHandleNameWrapper
    {
        private static readonly ConcurrentDictionary<(TypeReferenceHandle handle, CompilationModule module), TypeReferenceWrapper> _registerTypes = new ConcurrentDictionary<(TypeReferenceHandle handle, CompilationModule module), TypeReferenceWrapper>();

        private readonly Lazy<string> _name;
        private readonly Lazy<string> _fullName;
        private readonly Lazy<string> _namespace;
        private readonly Lazy<CompilationModule> _declaringModule;
        private readonly Lazy<IHandleTypeNamedWrapper> _type;

        private TypeReferenceWrapper(TypeReferenceHandle handle, CompilationModule module)
        {
            TypeReferenceHandle = handle;
            CompilationModule = module;
            Handle = handle;
            Definition = Resolve();
            _type = new Lazy<IHandleTypeNamedWrapper>(GetDeclaringType, LazyThreadSafetyMode.PublicationOnly);
            _namespace = new Lazy<string>(() => Definition.Namespace.GetName(module), LazyThreadSafetyMode.PublicationOnly);
            _name = new Lazy<string>(() => Definition.Name.GetName(module), LazyThreadSafetyMode.PublicationOnly);
            _declaringModule = new Lazy<CompilationModule>(GetDeclaringModule, LazyThreadSafetyMode.PublicationOnly);
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
        public CompilationModule CompilationModule { get; }

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
        public CompilationModule DeclaringModule => _declaringModule.Value;

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

            return _registerTypes.GetOrAdd((handle, module), data => new TypeReferenceWrapper(data.handle, data.module));
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

        private CompilationModule GetDeclaringModule()
        {
            var current = this;

            while (current != null)
            {
                switch (current.Definition.ResolutionScope.Kind)
                {
                    case HandleKind.TypeReference:
                        current = Create((TypeReferenceHandle)current.Definition.ResolutionScope, current.CompilationModule);
                        break;
                    case HandleKind.AssemblyReference:
                        var assemblyReference = AssemblyReferenceWrapper.Create((AssemblyReferenceHandle)current.Definition.ResolutionScope, current.CompilationModule);
                        return current.CompilationModule.Compilation.GetCompilationModuleForAssemblyReference(assemblyReference);
                    case HandleKind.ModuleReference:
                        var assemblyModuleReference = ModuleReferenceWrapper.Create((ModuleReferenceHandle)current.Definition.ResolutionScope, current.CompilationModule);
                        return assemblyModuleReference.CompilationModule;
                    default:
                        return default;
                }
            }

            return null;
        }

        private IHandleTypeNamedWrapper GetDeclaringType()
        {
            return CompilationModule.GetTypeByName(FullName);
        }

        private TypeReference Resolve()
        {
            return CompilationModule.MetadataReader.GetTypeReference(TypeReferenceHandle);
        }

        private string GetFullName()
        {
            if (Definition.ResolutionScope.IsNil || Definition.ResolutionScope.Kind != HandleKind.TypeReference)
            {
                return TypeNamespace + "." + Name;
            }

            var typeReference = Create((TypeReferenceHandle)Definition.ResolutionScope, CompilationModule);

            return typeReference.FullName + "." + Name;
        }
    }
}
