﻿// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Threading;
using LightweightMetadata.TypeWrappers;

namespace LightweightMetadata
{
    /// <summary>
    /// Represents a assembly or module.
    /// </summary>
    public sealed class CompilationModule : IDisposable
    {
        private readonly Lazy<IReadOnlyDictionary<string, TypeWrapper>> _publicTypesFromName;
        private readonly Lazy<IReadOnlyList<TypeWrapper>> _publicTypes;
        private readonly Lazy<IReadOnlyList<TypeReferenceHandle>> _typeReferenceHandles;
        private readonly Lazy<AssemblyWrapper> _mainAssembly;
        private readonly Lazy<MethodSemanticsLookup> _methodSemanticsLookup;
        private readonly PEReader _reader;

        /// <summary>
        /// Initializes a new instance of the <see cref="CompilationModule"/> class.
        /// </summary>
        /// <param name="fileName">The file name to the module.</param>
        /// <param name="compilation">The compilation unit that holds the module.</param>
        /// <param name="typeProvider">A type provider for decoding signatures.</param>
        internal CompilationModule(string fileName, ICompilation compilation, TypeProvider typeProvider)
        {
            FileName = fileName ?? throw new ArgumentNullException(nameof(fileName));
            Compilation = compilation ?? throw new ArgumentNullException(nameof(compilation));
            TypeProvider = typeProvider;

            _reader = new PEReader(new FileStream(fileName, FileMode.Open, FileAccess.Read), PEStreamOptions.PrefetchMetadata);
            MetadataReader = _reader.GetMetadataReader();
            _publicTypes = new Lazy<IReadOnlyList<TypeWrapper>>(() => (IReadOnlyList<TypeWrapper>)MetadataReader.TypeDefinitions.Select(x => TypeWrapper.Create(x, this)).Where(x => x.Accessibility == EntityAccessibility.Public).ToList(), LazyThreadSafetyMode.PublicationOnly);
            _publicTypesFromName = new Lazy<IReadOnlyDictionary<string, TypeWrapper>>(() => PublicTypes.ToDictionary(x => x.FullName, x => x), LazyThreadSafetyMode.PublicationOnly);
            _typeReferenceHandles = new Lazy<IReadOnlyList<TypeReferenceHandle>>(() => MetadataReader.TypeReferences.ToList(), LazyThreadSafetyMode.PublicationOnly);
            _methodSemanticsLookup = new Lazy<MethodSemanticsLookup>(() => new MethodSemanticsLookup(MetadataReader));
            _mainAssembly = new Lazy<AssemblyWrapper>(() => new AssemblyWrapper(this), LazyThreadSafetyMode.PublicationOnly);
        }

        /// <summary>
        /// Gets the module reader.
        /// </summary>
        public MetadataReader MetadataReader { get; }

        /// <summary>
        /// Gets the compilation that this module belongs to.
        /// </summary>
        public ICompilation Compilation { get; }

        /// <summary>
        /// Gets all the public type definition handles for this module.
        /// </summary>
        public IReadOnlyList<TypeWrapper> PublicTypes => _publicTypes.Value;

        /// <summary>
        /// Gets all the public type reference handles for this module.
        /// </summary>
        public IReadOnlyList<TypeReferenceHandle> TypeReferenceHandles => _typeReferenceHandles.Value;

        /// <summary>
        /// Gets a dictionary that maps types full names to their type wrapper.
        /// </summary>
        public IReadOnlyDictionary<string, TypeWrapper> PublicTypesByFullName => _publicTypesFromName.Value;

        /// <summary>
        /// Gets the main assembly reference inside this module.
        /// </summary>
        public AssemblyWrapper MainAssembly => _mainAssembly.Value;

        /// <summary>
        /// Gets the file name.
        /// </summary>
        public string FileName { get; }

        /// <summary>
        /// Gets details about methods.
        /// </summary>
        internal MethodSemanticsLookup MethodSemanticsLookup => _methodSemanticsLookup.Value;

        /// <summary>
        /// Gets the type provider.
        /// </summary>
        internal TypeProvider TypeProvider { get; }

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
    }
}
