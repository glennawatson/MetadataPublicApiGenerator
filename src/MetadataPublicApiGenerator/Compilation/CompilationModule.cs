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

using MetadataPublicApiGenerator.Compilation.TypeWrappers;
using MetadataPublicApiGenerator.Extensions;

namespace MetadataPublicApiGenerator.Compilation
{
    internal sealed class CompilationModule : IDisposable
    {
        private readonly Lazy<IReadOnlyList<TypeWrapper>> _publicTypes;
        private readonly Lazy<IReadOnlyList<TypeReferenceHandle>> _typeReferenceHandles;
        private readonly Lazy<MethodSemanticsLookup> _methodSemanticsLookup;
        private readonly PEReader _reader;

        public CompilationModule(string fileName, ICompilation compilation)
        {
            FileName = fileName ?? throw new ArgumentNullException(nameof(fileName));
            Compilation = compilation ?? throw new ArgumentNullException(nameof(compilation));

            _reader = new PEReader(new FileStream(fileName, FileMode.Open, FileAccess.Read), PEStreamOptions.PrefetchMetadata);
            MetadataReader = _reader.GetMetadataReader();
            TypeProvider = Compilation.TypeProvider;
            _publicTypes = new Lazy<IReadOnlyList<TypeWrapper>>(() => MetadataReader.TypeDefinitions.Where(x => (x.Resolve(this).Attributes & System.Reflection.TypeAttributes.Public) != 0).Select(x => new TypeWrapper(this, x)).ToList(), LazyThreadSafetyMode.PublicationOnly);
            _typeReferenceHandles = new Lazy<IReadOnlyList<TypeReferenceHandle>>(() => MetadataReader.TypeReferences.ToList(), LazyThreadSafetyMode.PublicationOnly);
            _methodSemanticsLookup = new Lazy<MethodSemanticsLookup>(() => new MethodSemanticsLookup(MetadataReader));
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
        /// Gets details about methods.
        /// </summary>
        public MethodSemanticsLookup MethodSemanticsLookup => _methodSemanticsLookup.Value;

        /// <summary>
        /// Gets the type provider.
        /// </summary>
        public TypeProvider TypeProvider { get; }

        /// <summary>
        /// Gets the file name.
        /// </summary>
        public string FileName { get; }

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
