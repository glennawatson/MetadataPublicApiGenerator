// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Threading;

using LightweightMetadata.Extensions;

namespace LightweightMetadata
{
    /// <summary>
    /// Wraps a event definition.
    /// </summary>
    public class EventWrapper : IHandleTypeNamedWrapper, IHasAttributes
    {
        private static readonly ConcurrentDictionary<(EventDefinitionHandle handle, AssemblyMetadata module), EventWrapper> _registerTypes = new ConcurrentDictionary<(EventDefinitionHandle handle, AssemblyMetadata module), EventWrapper>();

        private readonly Lazy<string> _name;

        private readonly Lazy<IReadOnlyList<AttributeWrapper>> _attributes;
        private readonly Lazy<MethodWrapper> _adderAccessor;
        private readonly Lazy<MethodWrapper> _removerAccessor;
        private readonly Lazy<MethodWrapper> _raiserAccessor;
        private readonly Lazy<MethodWrapper> _anyAccessor;
        private readonly Lazy<IHandleTypeNamedWrapper> _eventType;

        private EventWrapper(EventDefinitionHandle handle, AssemblyMetadata module)
        {
            EventDefinitionHandle = handle;
            CompilationModule = module;
            Handle = handle;
            Definition = Resolve();

            _name = new Lazy<string>(() => Definition.Name.GetName(module), LazyThreadSafetyMode.PublicationOnly);
            _attributes = new Lazy<IReadOnlyList<AttributeWrapper>>(() => AttributeWrapper.Create(Definition.GetCustomAttributes(), module), LazyThreadSafetyMode.PublicationOnly);

            _eventType = new Lazy<IHandleTypeNamedWrapper>(() => WrapperFactory.Create(Definition.Type, module), LazyThreadSafetyMode.PublicationOnly);

            _adderAccessor = new Lazy<MethodWrapper>(() => MethodWrapper.Create(Definition.GetAccessors().Adder, CompilationModule), LazyThreadSafetyMode.PublicationOnly);
            _removerAccessor = new Lazy<MethodWrapper>(() => MethodWrapper.Create(Definition.GetAccessors().Remover, CompilationModule), LazyThreadSafetyMode.PublicationOnly);
            _raiserAccessor = new Lazy<MethodWrapper>(() => MethodWrapper.Create(Definition.GetAccessors().Raiser, CompilationModule), LazyThreadSafetyMode.PublicationOnly);
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

        /// <inheritdoc />
        public IReadOnlyList<AttributeWrapper> Attributes => _attributes.Value;

        /// <inheritdoc />
        public string Name => _name.Value;

        /// <inheritdoc />
        public AssemblyMetadata CompilationModule { get; }

        /// <inheritdoc />
        public Handle Handle { get; }

        /// <inheritdoc />
        public string FullName => Name;

        /// <summary>
        /// Gets the event type.
        /// </summary>
        public TypeWrapper DeclaringType => AnyAccessor.DeclaringType;

        /// <summary>
        /// Gets the event type.
        /// </summary>
        public ITypeNamedWrapper EventType => _eventType.Value;

        /// <inheritdoc />
        public string ReflectionFullName => AnyAccessor.DeclaringType?.ReflectionFullName;

        /// <inheritdoc />
        public string TypeNamespace => AnyAccessor.TypeNamespace;

        /// <inheritdoc />
        public EntityAccessibility Accessibility => AnyAccessor.DeclaringType?.Accessibility ?? EntityAccessibility.None;

        /// <inheritdoc />
        public bool IsAbstract => AnyAccessor.IsAbstract;

        /// <summary>
        /// Gets the method that raises the event.
        /// </summary>
        public MethodWrapper RaiserAccessor => _raiserAccessor.Value;

        /// <summary>
        /// Gets the method that unregisters from the event.
        /// </summary>
        public MethodWrapper RemoverAccessor => _removerAccessor.Value;

        /// <summary>
        /// Gets the method that registers for the event.
        /// </summary>
        public MethodWrapper AdderAccessor => _adderAccessor.Value;

        /// <summary>
        /// Gets any available accessor method.
        /// </summary>
        public MethodWrapper AnyAccessor => _anyAccessor.Value;

        /// <inheritdoc />
        public KnownTypeCode KnownType => AnyAccessor.DeclaringType?.KnownType ?? KnownTypeCode.None;

        /// <summary>
        /// Creates a instance of the method, if there is already not an instance.
        /// </summary>
        /// <param name="handle">The handle to the instance.</param>
        /// <param name="module">The module that contains the instance.</param>
        /// <returns>The wrapper.</returns>
        public static EventWrapper Create(EventDefinitionHandle handle, AssemblyMetadata module)
        {
            if (handle.IsNil)
            {
                return null;
            }

            return _registerTypes.GetOrAdd((handle, module), data => new EventWrapper(data.handle, data.module));
        }

        /// <summary>
        /// Creates a array instances of a type.
        /// </summary>
        /// <param name="collection">The collection to create.</param>
        /// <param name="module">The module to use in creation.</param>
        /// <returns>The list of the type.</returns>
        public static IReadOnlyList<EventWrapper> Create(in EventDefinitionHandleCollection collection, AssemblyMetadata module)
        {
            var output = new EventWrapper[collection.Count];

            int i = 0;
            foreach (var element in collection)
            {
                output[i] = Create(element, module);
                i++;
            }

            return output;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return FullName;
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
            return CompilationModule.MetadataReader.GetEventDefinition(EventDefinitionHandle);
        }
    }
}
