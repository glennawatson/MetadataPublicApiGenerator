// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Security.Cryptography;
using System.Text;
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
        {
            CompilationModule = module;
            Definition = module.MetadataReader.GetAssemblyDefinition();

            _name = new Lazy<string>(() => module.MetadataReader.GetString(Definition.Name), LazyThreadSafetyMode.PublicationOnly);
            _culture = new Lazy<string>(GetCulture, LazyThreadSafetyMode.PublicationOnly);
            Version = Definition.Version;
            _assemblyName = new Lazy<AssemblyName>(() => Definition.GetAssemblyName(), LazyThreadSafetyMode.PublicationOnly);

            HashAlgorithm = Definition.HashAlgorithm;

            _publicKey = new Lazy<string>(CalculatePublicKeyToken, LazyThreadSafetyMode.PublicationOnly);
            _fullName = new Lazy<string>(GetFullName, LazyThreadSafetyMode.PublicationOnly);
            IsWindowsRuntime = (Definition.Flags & AssemblyFlags.WindowsRuntime) != 0;

            _attributes = new Lazy<IReadOnlyList<AttributeWrapper>>(() => Definition.GetCustomAttributes().Select(x => AttributeWrapper.Create(x, CompilationModule)).ToList(), LazyThreadSafetyMode.PublicationOnly);
        }

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

        [SuppressMessage("Design", "CA5350: Compromised hash algorithm", Justification = "Deliberate usage")]
        [SuppressMessage("Design", "CA5351: Compromised hash algorithm", Justification = "Deliberate usage")]
        private HashAlgorithm GetHashAlgorithm()
        {
            switch (HashAlgorithm)
            {
                case AssemblyHashAlgorithm.None:
                    // only for multi-module assemblies?
                    return SHA1.Create();
                case AssemblyHashAlgorithm.MD5:
                    return MD5.Create();
                case AssemblyHashAlgorithm.Sha1:
                    return SHA1.Create();
                case AssemblyHashAlgorithm.Sha256:
                    return SHA256.Create();
                case AssemblyHashAlgorithm.Sha384:
                    return SHA384.Create();
                case AssemblyHashAlgorithm.Sha512:
                    return SHA512.Create();
                default:
                    return SHA1.Create(); // default?
            }
        }

        private string GetCulture()
        {
            if (Definition.Culture.IsNil)
            {
                return "neutral";
            }

            return CompilationModule.MetadataReader.GetString(Definition.Culture);
        }

        private string CalculatePublicKeyToken()
        {
            if (Definition.PublicKey.IsNil)
            {
                return "null";
            }

            var reader = CompilationModule.MetadataReader;
            var blob = Definition.PublicKey;
            using (var hashAlgorithm = GetHashAlgorithm())
            {
                // Calculate public key token:
                // 1. hash the public key using the appropriate algorithm.
                byte[] publicKeyTokenBytes = hashAlgorithm.ComputeHash(reader.GetBlobBytes(blob));

                // 2. take the last 8 bytes
                // 3. according to Cecil we need to reverse them, other sources did not mention this.
                var bytes = publicKeyTokenBytes.Skip(publicKeyTokenBytes.Length - 8).Reverse().ToArray();

                var sb = new StringBuilder(bytes.Length * 2);
                foreach (var b in bytes)
                {
                    sb.AppendFormat(CultureInfo.InvariantCulture, "{0:x2}", b);
                }

                return sb.ToString();
            }
        }

        private string GetFullName()
        {
            return $"{Name}, Version={Version}, Culture={Culture}, PublicKeyToken={PublicKey}";
        }
    }
}
