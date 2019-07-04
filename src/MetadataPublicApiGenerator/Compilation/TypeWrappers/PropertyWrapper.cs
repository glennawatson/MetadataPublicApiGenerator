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
    internal class PropertyWrapper : ITypeNamedWrapper
    {
        private static readonly Dictionary<PropertyDefinitionHandle, PropertyWrapper> _registerTypes = new Dictionary<PropertyDefinitionHandle, PropertyWrapper>();

        private readonly Lazy<string> _name;

        private PropertyWrapper(PropertyDefinitionHandle handle, CompilationModule module)
        {
            Definition = Resolve(handle, module);
            PropertyDefinitionHandle = handle;
            Module = module;

            _name = new Lazy<string>(() => Definition.Name.GetName(module), LazyThreadSafetyMode.PublicationOnly);
        }

        /// <summary>
        /// Gets the resolved method definition.
        /// </summary>
        public PropertyDefinition Definition { get; }

        /// <summary>
        /// Gets the method definition handle.
        /// </summary>
        public PropertyDefinitionHandle PropertyDefinitionHandle { get; }

        /// <inheritdoc />
        public string Name => _name.Value;

        /// <inheritdoc />
        public string FullName { get; }

        /// <inheritdoc />
        public string Namespace { get; }

        /// <inheritdoc />
        public bool IsPublic { get; }

        /// <inheritdoc />
        public bool IsAbstract { get; }

        /// <inheritdoc />
        public CompilationModule Module { get; }

        /// <summary>
        /// Creates a instance of the method, if there is already not an instance.
        /// </summary>
        /// <param name="handle">The handle to the instance.</param>
        /// <param name="module">The module that contains the instance.</param>
        /// <returns>The wrapper.</returns>
        public static PropertyWrapper Create(PropertyDefinitionHandle handle, CompilationModule module)
        {
            return _registerTypes.GetOrAdd(handle, handleCreate => new PropertyWrapper(handleCreate, module));
        }

        private static PropertyDefinition Resolve(PropertyDefinitionHandle handle, CompilationModule compilation)
        {
            return compilation.MetadataReader.GetPropertyDefinition(handle);
        }
    }
}
