// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Text;
using System.Threading;

namespace LightweightMetadata
{
    /// <summary>
    /// Represents a wrapper around the MemberReference.
    /// </summary>
    public class MemberReferenceWrapper : IHandleTypeNamedWrapper, IHasAttributes
    {
        private static readonly ConcurrentDictionary<(MemberReferenceHandle handle, AssemblyMetadata assemblyMetadata), MemberReferenceWrapper> _registerTypes = new ConcurrentDictionary<(MemberReferenceHandle handle, AssemblyMetadata assemblyMetadata), MemberReferenceWrapper>();

        private readonly Lazy<string> _name;
        private readonly Lazy<IHandleTypeNamedWrapper> _parent;
        private readonly Lazy<string> _fullName;
        private readonly Lazy<string> _reflectionFullName;
        private readonly Lazy<IReadOnlyList<AttributeWrapper>> _attributes;

        private MemberReferenceWrapper(MemberReferenceHandle handle, AssemblyMetadata assemblyMetadata)
        {
            MemberReferenceHandle = handle;
            AssemblyMetadata = assemblyMetadata;
            Handle = handle;
            Definition = Resolve();

            _name = new Lazy<string>(() => Definition.Name.GetName(assemblyMetadata), LazyThreadSafetyMode.PublicationOnly);
            _parent = new Lazy<IHandleTypeNamedWrapper>(() => WrapperFactory.Create(Definition.Parent, AssemblyMetadata), LazyThreadSafetyMode.PublicationOnly);
            _attributes = new Lazy<IReadOnlyList<AttributeWrapper>>(() => AttributeWrapper.Create(Definition.GetCustomAttributes(), assemblyMetadata), LazyThreadSafetyMode.PublicationOnly);
            _fullName = new Lazy<string>(() => GetName(x => x.FullName), LazyThreadSafetyMode.PublicationOnly);
            _reflectionFullName = new Lazy<string>(() => GetName(x => x.ReflectionFullName), LazyThreadSafetyMode.PublicationOnly);
        }

        /// <summary>
        /// Gets the resolved method definition.
        /// </summary>
        public MemberReference Definition { get; }

        /// <summary>
        /// Gets the method definition handle.
        /// </summary>
        public MemberReferenceHandle MemberReferenceHandle { get; }

        /// <inheritdoc />
        public string Name => _name.Value;

        /// <inheritdoc />
        public AssemblyMetadata AssemblyMetadata { get; }

        /// <inheritdoc />
        public Handle Handle { get; }

        /// <summary>
        /// Gets the parent instance of the member reference.
        /// </summary>
        public IHandleTypeNamedWrapper Parent => _parent.Value;

        /// <inheritdoc/>
        public IReadOnlyList<AttributeWrapper> Attributes => _attributes.Value;

        /// <inheritdoc />
        public string ReflectionFullName => _reflectionFullName.Value;

        /// <inheritdoc />
        public string TypeNamespace => Parent?.TypeNamespace;

        /// <inheritdoc />
        public string FullName => _fullName.Value;

        /// <inheritdoc />
        public EntityAccessibility Accessibility => Parent?.Accessibility ?? EntityAccessibility.None;

        /// <inheritdoc />
        public bool IsAbstract => Parent?.IsAbstract ?? false;

        /// <inheritdoc />
        public bool IsValueType => Parent?.IsValueType ?? true;

        /// <inheritdoc />
        public KnownTypeCode KnownType => Parent?.KnownType ?? KnownTypeCode.None;

        /// <summary>
        /// Creates a instance of the method, if there is already not an instance.
        /// </summary>
        /// <param name="handle">The handle to the instance.</param>
        /// <param name="assemblyMetadata">The module that contains the instance.</param>
        /// <returns>The wrapper.</returns>
        public static MemberReferenceWrapper Create(MemberReferenceHandle handle, AssemblyMetadata assemblyMetadata)
        {
            if (handle.IsNil)
            {
                return null;
            }

            return _registerTypes.GetOrAdd((handle, assemblyMetadata), data => new MemberReferenceWrapper(data.handle, data.assemblyMetadata));
        }

        /// <summary>
        /// Creates a array instances of a type.
        /// </summary>
        /// <param name="collection">The collection to create.</param>
        /// <param name="assemblyMetadata">The module to use in creation.</param>
        /// <returns>The list of the type.</returns>
        public static IReadOnlyList<MemberReferenceWrapper> Create(in MemberReferenceHandleCollection collection, AssemblyMetadata assemblyMetadata)
        {
            var output = new MemberReferenceWrapper[collection.Count];

            int i = 0;
            foreach (var element in collection)
            {
                output[i] = Create(element, assemblyMetadata);
                i++;
            }

            return output;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return FullName;
        }

        private MemberReference Resolve()
        {
            return AssemblyMetadata.MetadataReader.GetMemberReference(MemberReferenceHandle);
        }

        private string GetName(Func<IHandleTypeNamedWrapper, string> nameGetter)
        {
            var stringBuilder = new StringBuilder();

            var list = new List<string>(10);
            var current = Parent;
            while (current != null)
            {
                var name = nameGetter(current);

                if (!string.IsNullOrWhiteSpace(name))
                {
                    list.Insert(0, name);
                }

                current = current.Handle.Kind == HandleKind.MemberReference ? ((MemberReferenceWrapper)current).Parent : default;
            }

            if (list.Count > 0)
            {
                stringBuilder.Append(string.Join(".", list)).Append('.');
            }

            stringBuilder.Append(Name);

            return stringBuilder.ToString();
        }
    }
}
