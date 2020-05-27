﻿// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Security.Cryptography;
using System.Threading;

namespace LightweightMetadata
{
    /// <summary>
    /// Wraps the <see cref="AssemblyReference" /> class.
    /// </summary>
    public class AssemblyReferenceWrapper : IEquatable<AssemblyReferenceWrapper>
    {
        private static readonly ConcurrentDictionary<(AssemblyReferenceHandle Handle, AssemblyMetadata AssemblyMetadata), AssemblyReferenceWrapper> _registerTypes = new ConcurrentDictionary<(AssemblyReferenceHandle, AssemblyMetadata), AssemblyReferenceWrapper>();

        private readonly Lazy<string> _name;
        private readonly Lazy<string> _culture;
        private readonly Lazy<AssemblyName> _assemblyName;
        private readonly Lazy<string> _publicKey;
        private readonly Lazy<string> _fullName;
        private readonly Lazy<IReadOnlyList<AttributeWrapper>> _attributes;

        private AssemblyReferenceWrapper(AssemblyReferenceHandle handle, AssemblyMetadata assemblyMetadata)
        {
            AssemblyReferenceHandle = handle;
            ParentCompilationModule = assemblyMetadata;
            Handle = handle;
            Definition = assemblyMetadata.MetadataReader.GetAssemblyReference(handle);

            _name = new Lazy<string>(() => assemblyMetadata.MetadataReader.GetString(Definition.Name), LazyThreadSafetyMode.PublicationOnly);
            _culture = new Lazy<string>(GetCulture, LazyThreadSafetyMode.PublicationOnly);
            Version = Definition.Version;
            _assemblyName = new Lazy<AssemblyName>(() => Definition.GetAssemblyName(), LazyThreadSafetyMode.PublicationOnly);

            _publicKey = new Lazy<string>(() => Definition.PublicKeyOrToken.CalculatePublicKeyToken(assemblyMetadata, AssemblyHashAlgorithm.None), LazyThreadSafetyMode.PublicationOnly);
            _fullName = new Lazy<string>(GetFullName, LazyThreadSafetyMode.PublicationOnly);
            IsWindowsRuntime = (Definition.Flags & AssemblyFlags.WindowsRuntime) != 0;
            IsRetargetable = (Definition.Flags & AssemblyFlags.Retargetable) != 0;

            _attributes = new Lazy<IReadOnlyList<AttributeWrapper>>(() => AttributeWrapper.CreateChecked(Definition.GetCustomAttributes(), assemblyMetadata), LazyThreadSafetyMode.PublicationOnly);
        }

        /// <summary>
        /// Gets the full name of the assembly.
        /// </summary>
        public string FullName => _fullName.Value;

        /// <summary>
        /// Gets the resolved method definition.
        /// </summary>
        public AssemblyReference Definition { get; }

        /// <summary>
        /// Gets the name of the assembly.
        /// </summary>
        public string Name => _name.Value;

        /// <summary>
        /// Gets the handle to the assembly reference.
        /// </summary>
        public AssemblyReferenceHandle AssemblyReferenceHandle { get; }

        /// <summary>
        /// Gets the parent's MetadataRepository module.
        /// </summary>
        public AssemblyMetadata ParentCompilationModule { get; }

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
        /// Gets a value indicating whether this assembly is retargetable to another assembly.
        /// </summary>
        public bool IsRetargetable { get; }

        /// <summary>
        /// Gets the assembly name of the assembly.
        /// </summary>
        public AssemblyName AssemblyName => _assemblyName.Value;

        /// <summary>
        /// Gets a string representation of the public token.
        /// </summary>
        public string PublicKey => _publicKey.Value;

        /// <summary>
        /// Gets a list of attributes.
        /// </summary>
        public IReadOnlyList<AttributeWrapper> Attributes => _attributes.Value;

        /// <summary>
        /// Gets the handle for the item.
        /// </summary>
        public Handle Handle { get; }

        /// <summary>
        /// Compares the left and the right to see if they are logically equal.
        /// </summary>
        /// <param name="left">The left ot compare.</param>
        /// <param name="right">The right to compare.</param>
        /// <returns>If they are logically equal.</returns>
        public static bool operator ==(AssemblyReferenceWrapper left, AssemblyReferenceWrapper right)
        {
            return Equals(left, right);
        }

        /// <summary>
        /// Compares the left and the right to see if they are logically not equal.
        /// </summary>
        /// <param name="left">The left ot compare.</param>
        /// <param name="right">The right to compare.</param>
        /// <returns>If they are logically not equal.</returns>
        public static bool operator !=(AssemblyReferenceWrapper left, AssemblyReferenceWrapper right)
        {
            return !Equals(left, right);
        }

        /// <summary>
        /// Creates a instance of the method, if there is already not an instance.
        /// </summary>
        /// <param name="handle">The handle to the instance.</param>
        /// <param name="assemblyMetadata">The module that contains the instance.</param>
        /// <returns>The wrapper.</returns>
        public static AssemblyReferenceWrapper? Create(AssemblyReferenceHandle handle, AssemblyMetadata assemblyMetadata)
        {
            if (handle.IsNil)
            {
                return null;
            }

            return _registerTypes.GetOrAdd((handle, assemblyMetadata), data => new AssemblyReferenceWrapper(data.Handle, data.AssemblyMetadata));
        }

        /// <summary>
        /// Creates a array instances of a type.
        /// </summary>
        /// <param name="collection">The collection to create.</param>
        /// <param name="assemblyMetadata">The module to use in creation.</param>
        /// <returns>The list of the type.</returns>
        public static IReadOnlyList<AssemblyReferenceWrapper?> Create(in AssemblyReferenceHandleCollection collection, AssemblyMetadata assemblyMetadata)
        {
            var output = new AssemblyReferenceWrapper?[collection.Count];

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
        public static IReadOnlyList<AssemblyReferenceWrapper> CreateChecked(in AssemblyReferenceHandleCollection collection, AssemblyMetadata assemblyMetadata)
        {
            var entities = Create(collection, assemblyMetadata);

            if (entities.Any(x => x is null))
            {
                throw new ArgumentException("Have invalid assembly references.", nameof(collection));
            }

            return entities.Select(x => x!).ToList();
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return FullName;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != GetType())
            {
                return false;
            }

            return Equals((AssemblyReferenceWrapper)obj);
        }

        /// <inheritdoc />
        public bool Equals(AssemblyReferenceWrapper other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Equals(FullName, other.FullName);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return FullName != null ? FullName.GetHashCode() : 0;
        }

        private string GetCulture()
        {
            if (Definition.Culture.IsNil)
            {
                return "neutral";
            }

            return ParentCompilationModule.MetadataReader.GetString(Definition.Culture);
        }

        private string GetFullName()
        {
            return $"{Name}, Version={Version}, Culture={Culture}, PublicKeyToken={PublicKey}";
        }
    }
}