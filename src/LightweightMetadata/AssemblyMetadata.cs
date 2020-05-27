// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Threading;

namespace LightweightMetadata
{
    /// <summary>
    /// Represents a assembly or module.
    /// </summary>
    public sealed class AssemblyMetadata : IDisposable, IEquatable<AssemblyMetadata>
    {
        private readonly Lazy<IReadOnlyDictionary<string, TypeWrapper>> _namesToTypes;
        private readonly Lazy<IReadOnlyList<TypeWrapper>> _types;
        private readonly Lazy<IReadOnlyList<TypeReferenceWrapper>> _typeReferences;
        private readonly Lazy<IReadOnlyList<AssemblyReferenceWrapper>> _assemblyReferences;
        private readonly Lazy<AssemblyWrapper> _mainAssembly;
        private readonly Lazy<MethodSemanticsLookup> _methodSemanticsLookup;
        private readonly Lazy<ModuleDefinitionWrapper> _moduleDefinition;
        private readonly PEReader _reader;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyMetadata"/> class.
        /// </summary>
        /// <param name="fileName">The file name to the module.</param>
        /// <param name="metadataRepository">The MetadataRepository unit that holds the module.</param>
        /// <param name="typeProvider">A type provider for decoding signatures.</param>
        internal AssemblyMetadata(string fileName, MetadataRepository metadataRepository, TypeProvider typeProvider)
        {
            FileName = fileName ?? throw new ArgumentNullException(nameof(fileName));
            MetadataRepository = metadataRepository ?? throw new ArgumentNullException(nameof(metadataRepository));
            TypeProvider = typeProvider;

            _reader = new PEReader(new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read), PEStreamOptions.PrefetchMetadata);
            MetadataReader = _reader.GetMetadataReader();

            _types = new Lazy<IReadOnlyList<TypeWrapper>>(() => TypeWrapper.CreateChecked(MetadataReader.TypeDefinitions, this), LazyThreadSafetyMode.PublicationOnly);
            _namesToTypes = new Lazy<IReadOnlyDictionary<string, TypeWrapper>>(() => Types.ToDictionary(x => x.FullName), LazyThreadSafetyMode.PublicationOnly);
            _typeReferences = new Lazy<IReadOnlyList<TypeReferenceWrapper>>(() => TypeReferenceWrapper.CreateChecked(MetadataReader.TypeReferences, this), LazyThreadSafetyMode.PublicationOnly);
            _assemblyReferences = new Lazy<IReadOnlyList<AssemblyReferenceWrapper>>(() => AssemblyReferenceWrapper.CreateChecked(MetadataReader.AssemblyReferences, this), LazyThreadSafetyMode.PublicationOnly);
            _methodSemanticsLookup = new Lazy<MethodSemanticsLookup>(() => new MethodSemanticsLookup(MetadataReader), LazyThreadSafetyMode.PublicationOnly);
            _mainAssembly = new Lazy<AssemblyWrapper>(() => new AssemblyWrapper(this), LazyThreadSafetyMode.PublicationOnly);
            _moduleDefinition = new Lazy<ModuleDefinitionWrapper>(() => ModuleDefinitionWrapper.Create(MetadataReader.GetModuleDefinition(), this), LazyThreadSafetyMode.PublicationOnly);
        }

        /// <summary>
        /// Gets the module reader.
        /// </summary>
        public MetadataReader MetadataReader { get; }

        /// <summary>
        /// Gets the MetadataRepository that this module belongs to.
        /// </summary>
        public MetadataRepository MetadataRepository { get; }

        /// <summary>
        /// Gets all the types.
        /// </summary>
        public IReadOnlyList<TypeWrapper> Types => _types.Value;

        /// <summary>
        /// Gets all the public type reference handles for this module.
        /// </summary>
        public IReadOnlyList<TypeReferenceWrapper> TypeReferences => _typeReferences.Value;

        /// <summary>
        /// Gets a list of assembly references.
        /// </summary>
        public IReadOnlyList<AssemblyReferenceWrapper> AssemblyReferences => _assemblyReferences.Value;

        /// <summary>
        /// Gets the main assembly reference inside this module.
        /// </summary>
        public AssemblyWrapper MainAssembly => _mainAssembly.Value;

        /// <summary>
        /// Gets the file name.
        /// </summary>
        public string FileName { get; }

        /// <summary>
        /// Gets the module definition.
        /// </summary>
        public ModuleDefinitionWrapper ModuleDefinition => _moduleDefinition.Value;

        /// <summary>
        /// Gets details about methods.
        /// </summary>
        internal MethodSemanticsLookup MethodSemanticsLookup => _methodSemanticsLookup.Value;

        /// <summary>
        /// Gets the type provider.
        /// </summary>
        internal TypeProvider TypeProvider { get; }

        /// <summary>
        /// Compares the equality of the left side and the right side.
        /// </summary>
        /// <param name="left">The left element to compare.</param>
        /// <param name="right">The right element to compare.</param>
        /// <returns>If the two sides are equal.</returns>
        public static bool operator ==(AssemblyMetadata left, AssemblyMetadata right)
        {
            return Equals(left, right);
        }

        /// <summary>
        /// Compares the inequality of the left side and the right side.
        /// </summary>
        /// <param name="left">The left element to compare.</param>
        /// <param name="right">The right element to compare.</param>
        /// <returns>If the two sides are not equal.</returns>
        public static bool operator !=(AssemblyMetadata left, AssemblyMetadata right)
        {
            return !Equals(left, right);
        }

        /// <summary>
        /// Gets the type by a name if available.
        /// </summary>
        /// <param name="name">The name to check.</param>
        /// <param name="checkRepository">If we should check repository on fail.</param>
        /// <returns>The wrapper if available, null otherwise.</returns>
        public TypeWrapper? GetTypeByName(string? name, bool checkRepository = true)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return null;
            }

            if (_namesToTypes.Value.TryGetValue(name!, out var item))
            {
                return item;
            }

            return checkRepository ? MetadataRepository.GetTypeByName(name) : null;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _reader?.Dispose();
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return FileName;
        }

        /// <inheritdoc />
        public bool Equals(AssemblyMetadata other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return string.Equals(FileName, other.FileName, StringComparison.InvariantCultureIgnoreCase);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || (obj is AssemblyMetadata other && Equals(other));
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return FileName != null ? StringComparer.InvariantCultureIgnoreCase.GetHashCode(FileName) : 0;
        }
    }
}
