// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Threading;

namespace LightweightMetadata
{
    /// <summary>
    /// A wrapper for the ModuleDefinition.
    /// </summary>
    public class ModuleDefinitionWrapper : IHandleNameWrapper, IHasAttributes
    {
        private readonly Lazy<IReadOnlyList<AttributeWrapper>> _attributes;
        private readonly Lazy<string> _name;
        private readonly Lazy<Guid> _generationId;
        private readonly Lazy<Guid> _baseGenerationId;
        private readonly Lazy<Guid> _mvid;

        private ModuleDefinitionWrapper(ModuleDefinition moduleDefinition, AssemblyMetadata assemblyMetadata)
        {
            AssemblyMetadata = assemblyMetadata;
            ModuleDefinition = moduleDefinition;

            _attributes = new Lazy<IReadOnlyList<AttributeWrapper>>(() => AttributeWrapper.CreateChecked(ModuleDefinition.GetCustomAttributes(), assemblyMetadata), LazyThreadSafetyMode.PublicationOnly);
            _name = new Lazy<string>(() => moduleDefinition.Name.GetName(assemblyMetadata), LazyThreadSafetyMode.PublicationOnly);
            Generation = ModuleDefinition.Generation;

            _generationId = new Lazy<Guid>(() => assemblyMetadata.MetadataReader.GetGuid(ModuleDefinition.GenerationId), LazyThreadSafetyMode.PublicationOnly);
            _baseGenerationId = new Lazy<Guid>(() => assemblyMetadata.MetadataReader.GetGuid(ModuleDefinition.BaseGenerationId), LazyThreadSafetyMode.PublicationOnly);
            _mvid = new Lazy<Guid>(() => assemblyMetadata.MetadataReader.GetGuid(ModuleDefinition.Mvid), LazyThreadSafetyMode.PublicationOnly);
        }

        /// <inheritdoc />
        public Handle Handle { get; }

        /// <inheritdoc />
        public AssemblyMetadata AssemblyMetadata { get; }

        /// <summary>
        /// Gets a reference to the module definition.
        /// </summary>
        public ModuleDefinitionHandle ModuleDefinitionHandle { get; }

        /// <summary>
        /// Gets the module definition.
        /// </summary>
        public ModuleDefinition ModuleDefinition { get; }

        /// <inheritdoc />
        public IReadOnlyList<AttributeWrapper> Attributes => _attributes.Value;

        /// <summary>
        /// Gets the generation id.
        /// </summary>
        public Guid GenerationId => _generationId.Value;

        /// <summary>
        /// Gets the base generation id.
        /// </summary>
        public Guid BaseGenerationId => _baseGenerationId.Value;

        /// <summary>
        /// Gets the module version id.
        /// </summary>
        public Guid ModuleVersionId => _mvid.Value;

        /// <summary>
        /// Gets the generation of the module.
        /// </summary>
        public int Generation { get; }

        /// <inheritdoc />
        public string Name => _name.Value;

        /// <inheritdoc />
        public string FullName => Name;

        /// <summary>
        /// Creates a new instance of the type wrapper.
        /// </summary>
        /// <param name="handle">The handle to wrap.</param>
        /// <param name="assemblyMetadata">The module containing the handle.</param>
        /// <returns>The wrapped instance if the handle is not nil, otherwise null.</returns>
        public static ModuleDefinitionWrapper Create(ModuleDefinition handle, AssemblyMetadata assemblyMetadata)
        {
            if (assemblyMetadata is null)
            {
                throw new ArgumentNullException(nameof(assemblyMetadata));
            }

            return new ModuleDefinitionWrapper(handle, assemblyMetadata);
        }
    }
}
