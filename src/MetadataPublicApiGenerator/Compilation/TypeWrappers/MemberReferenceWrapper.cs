// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading;
using MetadataPublicApiGenerator.Extensions;

namespace MetadataPublicApiGenerator.Compilation.TypeWrappers
{
    internal class MemberReferenceWrapper : IHandleTypeNamedWrapper
    {
        private static readonly Dictionary<MemberReferenceHandle, MemberReferenceWrapper> _registerTypes = new Dictionary<MemberReferenceHandle, MemberReferenceWrapper>();

        private readonly Lazy<string> _name;
        private readonly Lazy<IHandleTypeNamedWrapper> _parent;
        private readonly Lazy<string> _fullName;
        private readonly Lazy<IReadOnlyList<AttributeWrapper>> _attributes;

        private MemberReferenceWrapper(MemberReferenceHandle handle, CompilationModule module)
        {
            MemberReferenceHandle = handle;
            Module = module;
            Handle = handle;
            Definition = Resolve();

            _name = new Lazy<string>(() => Definition.Name.GetName(module), LazyThreadSafetyMode.PublicationOnly);
            _parent = new Lazy<IHandleTypeNamedWrapper>(() => WrapperFactory.Create(Definition.Parent, Module), LazyThreadSafetyMode.PublicationOnly);
            _attributes = new Lazy<IReadOnlyList<AttributeWrapper>>(() => Definition.GetCustomAttributes().Select(x => AttributeWrapper.Create(x, module)).ToList(), LazyThreadSafetyMode.PublicationOnly);
            _fullName = new Lazy<string>(GetFullName, LazyThreadSafetyMode.PublicationOnly);
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
        public CompilationModule Module { get; }

        public Handle Handle { get; }

        public IHandleTypeNamedWrapper Parent => _parent.Value;

        public IReadOnlyList<AttributeWrapper> Attributes => _attributes.Value;

        /// <inheritdoc />
        public string Namespace => Parent?.Namespace;

        /// <inheritdoc />
        public string FullName => _fullName.Value;

        /// <inheritdoc />
        public bool IsPublic => Parent?.IsPublic ?? false;

        /// <inheritdoc />
        public bool IsAbstract => Parent?.IsAbstract ?? false;

        /// <summary>
        /// Creates a instance of the method, if there is already not an instance.
        /// </summary>
        /// <param name="handle">The handle to the instance.</param>
        /// <param name="module">The module that contains the instance.</param>
        /// <returns>The wrapper.</returns>
        public static MemberReferenceWrapper Create(MemberReferenceHandle handle, CompilationModule module)
        {
            if (handle.IsNil)
            {
                return null;
            }

            return _registerTypes.GetOrAdd(handle, handleCreate => new MemberReferenceWrapper(handleCreate, module));
        }

        private MemberReference Resolve()
        {
            return Module.MetadataReader.GetMemberReference(MemberReferenceHandle);
        }

        private string GetFullName()
        {
            var stringBuilder = new StringBuilder();

            var list = new List<string>();
            var current = Parent;
            while (current != null)
            {
                var name = current.FullName;

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
