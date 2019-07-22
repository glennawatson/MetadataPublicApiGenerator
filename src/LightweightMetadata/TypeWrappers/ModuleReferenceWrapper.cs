// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Text;
using System.Threading;
using LightweightMetadata.Extensions;

namespace LightweightMetadata.TypeWrappers
{
    /// <summary>
    /// Wraps the ModuleReference.
    /// </summary>
    public class ModuleReferenceWrapper : IHandleNameWrapper
    {
        private static readonly ConcurrentDictionary<(ModuleReferenceHandle handle, CompilationModule module), ModuleReferenceWrapper> _registerTypes = new ConcurrentDictionary<(ModuleReferenceHandle handle, CompilationModule module), ModuleReferenceWrapper>();

        private readonly Lazy<string> _name;
        private readonly Lazy<IReadOnlyList<AttributeWrapper>> _attributes;
        private readonly Lazy<CompilationModule> _compilationModule;

        private ModuleReferenceWrapper(ModuleReferenceHandle handle, CompilationModule parent)
        {
            ModuleReferenceHandle = handle;
            Handle = handle;
            Definition = Resolve();

            ParentCompilationModule = parent;

            _name = new Lazy<string>(() => parent.MetadataReader.GetString(Definition.Name), LazyThreadSafetyMode.PublicationOnly);
            _attributes = new Lazy<IReadOnlyList<AttributeWrapper>>(() => AttributeWrapper.Create(Definition.GetCustomAttributes(), parent), LazyThreadSafetyMode.PublicationOnly);
            _compilationModule = new Lazy<CompilationModule>(GetDeclaringModule, LazyThreadSafetyMode.PublicationOnly);
        }

        /// <summary>
        /// Gets a reference to the module reference.
        /// </summary>
        public ModuleReferenceHandle ModuleReferenceHandle { get; }

        /// <inheritdoc />
        public CompilationModule CompilationModule => _compilationModule.Value;

        /// <inheritdoc />
        public Handle Handle { get; }

        /// <summary>
        /// Gets the definition to the module reference.
        /// </summary>
        public ModuleReference Definition { get; }

        /// <inheritdoc />
        public string Name => _name.Value;

        /// <summary>
        /// Gets the attributes for the module reference.
        /// </summary>
        public IReadOnlyList<AttributeWrapper> Attributes => _attributes.Value;

        /// <summary>
        /// Gets the parent compilation module.
        /// </summary>
        public CompilationModule ParentCompilationModule { get; }

        /// <summary>
        /// Creates a instance of the method, if there is already not an instance.
        /// </summary>
        /// <param name="handle">The handle to the instance.</param>
        /// <param name="module">The module that contains the instance.</param>
        /// <returns>The wrapper.</returns>
        public static ModuleReferenceWrapper Create(ModuleReferenceHandle handle, CompilationModule module)
        {
            if (handle.IsNil)
            {
                return null;
            }

            return _registerTypes.GetOrAdd((handle, module), data => new ModuleReferenceWrapper(data.handle, data.module));
        }

        private ModuleReference Resolve()
        {
            return ParentCompilationModule.MetadataReader.GetModuleReference(ModuleReferenceHandle);
        }

        private CompilationModule GetDeclaringModule()
        {
            return ParentCompilationModule.Compilation.GetCompilationModuleForName(Name, ParentCompilationModule);
        }
    }
}
