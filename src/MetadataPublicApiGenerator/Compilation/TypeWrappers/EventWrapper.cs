// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading;

using MetadataPublicApiGenerator.Extensions;

namespace MetadataPublicApiGenerator.Compilation.TypeWrappers
{
    /// <summary>
    /// Wraps a event definition.
    /// </summary>
    internal class EventWrapper : IHandleTypeNamedWrapper, IHasAttributes
    {
        private static readonly Dictionary<EventDefinitionHandle, EventWrapper> _registerTypes = new Dictionary<EventDefinitionHandle, EventWrapper>();

        private readonly Lazy<string> _name;

        private readonly Lazy<IReadOnlyList<AttributeWrapper>> _attributes;
        private readonly Lazy<MethodWrapper> _adderAccessor;
        private readonly Lazy<MethodWrapper> _removerAccessor;
        private readonly Lazy<MethodWrapper> _raiserAccessor;
        private readonly Lazy<MethodWrapper> _anyAccessor;

        private EventWrapper(EventDefinitionHandle handle, CompilationModule module)
        {
            EventDefinitionHandle = handle;
            Module = module;
            Handle = handle;
            Definition = Resolve();

            _name = new Lazy<string>(() => Definition.Name.GetName(module), LazyThreadSafetyMode.PublicationOnly);
            _attributes = new Lazy<IReadOnlyList<AttributeWrapper>>(() => Definition.GetCustomAttributes().Select(x => AttributeWrapper.Create(x, module)).ToList(), LazyThreadSafetyMode.PublicationOnly);

            _adderAccessor = new Lazy<MethodWrapper>(() => MethodWrapper.Create(Definition.GetAccessors().Adder, Module), LazyThreadSafetyMode.PublicationOnly);
            _removerAccessor = new Lazy<MethodWrapper>(() => MethodWrapper.Create(Definition.GetAccessors().Remover, Module), LazyThreadSafetyMode.PublicationOnly);
            _raiserAccessor = new Lazy<MethodWrapper>(() => MethodWrapper.Create(Definition.GetAccessors().Raiser, Module), LazyThreadSafetyMode.PublicationOnly);
            _anyAccessor = new Lazy<MethodWrapper>(GetAnyAccessor, LazyThreadSafetyMode.PublicationOnly);
        }

        /// <summary>
        /// Gets the resolved method definition.
        /// </summary>
        public EventDefinition Definition { get; }

        /// <summary>
        /// Gets the method definition handle.
        /// </summary>
        public EventDefinitionHandle EventDefinitionHandle { get; }

        public IReadOnlyList<AttributeWrapper> Attributes => _attributes.Value;

        /// <inheritdoc />
        public string Name => _name.Value;

        /// <inheritdoc />
        public CompilationModule Module { get; }

        public Handle Handle { get; }

        /// <inheritdoc />
        public string FullName => AnyAccessor.DeclaringType?.FullName;

        /// <inheritdoc />
        public string Namespace => AnyAccessor.Namespace;

        /// <inheritdoc />
        public bool IsPublic => AnyAccessor.DeclaringType?.IsPublic ?? false;

        /// <inheritdoc />
        public bool IsAbstract => AnyAccessor.IsAbstract;

        public MethodWrapper RaiserAccessor => _raiserAccessor.Value;

        public MethodWrapper RemoverAccessor => _removerAccessor.Value;

        public MethodWrapper AdderAccessor => _adderAccessor.Value;

        public MethodWrapper AnyAccessor => _anyAccessor.Value;

        /// <summary>
        /// Creates a instance of the method, if there is already not an instance.
        /// </summary>
        /// <param name="handle">The handle to the instance.</param>
        /// <param name="module">The module that contains the instance.</param>
        /// <returns>The wrapper.</returns>
        public static EventWrapper Create(EventDefinitionHandle handle, CompilationModule module)
        {
            if (handle.IsNil)
            {
                return null;
            }

            return _registerTypes.GetOrAdd(handle, handleCreate => new EventWrapper(handleCreate, module));
        }

        private MethodWrapper GetAnyAccessor()
        {
            if (AdderAccessor != null)
            {
                return AdderAccessor;
            }

            if (RemoverAccessor != null)
            {
                return RemoverAccessor;
            }

            return RaiserAccessor;
        }

        private EventDefinition Resolve()
        {
            return Module.MetadataReader.GetEventDefinition(EventDefinitionHandle);
        }
    }
}
