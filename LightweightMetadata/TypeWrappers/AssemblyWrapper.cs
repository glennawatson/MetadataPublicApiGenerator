// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Metadata;
using System.Threading;
using LightweightMetadata.Extensions;

namespace LightweightMetadata.TypeWrappers
{
    /// <summary>
    /// Wraps a <see cref="AssemblyDefinition"/>.
    /// </summary>
    [DebuggerDisplay("{" + nameof(AssemblyName) + "}")]
    public class AssemblyWrapper : INamedWrapper, IHasAttributes
    {
        private readonly Lazy<IReadOnlyList<AttributeWrapper>> _attributes;
        private readonly Lazy<string> _name;
        private readonly Lazy<string> _culture;
        private readonly Lazy<AssemblyName> _assemblyName;
        private readonly Lazy<string> _publicKey;
        private readonly Lazy<string> _fullName;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyWrapper"/> class.
        /// </summary>
        /// <param name="module">The module containing the definition.</param>
        internal AssemblyWrapper(CompilationModule module)
            : this(module.MetadataReader.GetAssemblyDefinition(), module)
        {
        }

        internal AssemblyWrapper(AssemblyDefinition reference, CompilationModule module)
        {
            Definition = reference;
            _name = new Lazy<string>(() => module.MetadataReader.GetString(Definition.Name), LazyThreadSafetyMode.PublicationOnly);
            _culture = new Lazy<string>(GetCulture, LazyThreadSafetyMode.PublicationOnly);
            Version = Definition.Version;
            _assemblyName = new Lazy<AssemblyName>(() => Definition.GetAssemblyName(), LazyThreadSafetyMode.PublicationOnly);

            HashAlgorithm = Definition.HashAlgorithm;

            _publicKey = new Lazy<string>(() => Definition.PublicKey.CalculatePublicKeyToken(module, HashAlgorithm), LazyThreadSafetyMode.PublicationOnly);
            _fullName = new Lazy<string>(GetFullName, LazyThreadSafetyMode.PublicationOnly);
            IsWindowsRuntime = (Definition.Flags & AssemblyFlags.WindowsRuntime) != 0;

            _attributes = new Lazy<IReadOnlyList<AttributeWrapper>>(() => AttributeWrapper.Create(Definition.GetCustomAttributes(), module), LazyThreadSafetyMode.PublicationOnly);
        }

        /// <summary>
        /// Gets the full name of the assembly.
        /// </summary>
        public string FullName => _fullName.Value;

        /// <summary>
        /// Gets the resolved method definition.
        /// </summary>
        public AssemblyDefinition Definition { get; }

        /// <inheritdoc />
        public string Name => _name.Value;

        /// <summary>
        /// Gets the version of the assembly.
        /// </summary>
        public Version Version { get; }

        /// <summary>
        /// Gets the culture of the assembly.
        /// </summary>
        public string Culture => _culture.Value;

        /// <summary>
        /// Gets a value indicating whether this assembly is a windows runtime.
        /// </summary>
        public bool IsWindowsRuntime { get; }

        /// <summary>
        /// Gets the assembly name of the assembly.
        /// </summary>
        public AssemblyName AssemblyName => _assemblyName.Value;

        /// <summary>
        /// Gets the hash algorithm used to sign the assembly.
        /// </summary>
        public AssemblyHashAlgorithm HashAlgorithm { get; }

        /// <summary>
        /// Gets the compilation module that holds the assembly.
        /// </summary>
        public CompilationModule CompilationModule { get; }

        /// <summary>
        /// Gets a string representation of the public token.
        /// </summary>
        public string PublicKey => _publicKey.Value;

        /// <inheritdoc/>
        public IReadOnlyList<AttributeWrapper> Attributes => _attributes.Value;

        private string GetCulture()
        {
            if (Definition.Culture.IsNil)
            {
                return "neutral";
            }

            return CompilationModule.MetadataReader.GetString(Definition.Culture);
        }

        private string GetFullName()
        {
            return $"{Name}, Version={Version}, Culture={Culture}, PublicKeyToken={PublicKey}";
        }
    }
}
