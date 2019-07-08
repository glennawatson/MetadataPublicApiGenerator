// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text;
using System.Threading;

using MetadataPublicApiGenerator.Extensions;

namespace MetadataPublicApiGenerator.Compilation.TypeWrappers
{
    internal class ParameterWrapper : IHandleNameWrapper, IHasAttributes
    {
        private static readonly Dictionary<ParameterHandle, ParameterWrapper> _registeredTypes = new Dictionary<ParameterHandle, ParameterWrapper>();

        private readonly Lazy<string> _name;

        private readonly Lazy<IReadOnlyList<AttributeWrapper>> _attributes;

        private ParameterWrapper(ParameterHandle handle, IHandleTypeNamedWrapper typeWrapper, CompilationModule module)
        {
            Module = module;
            ParameterHandle = handle;
            Handle = handle;
            Definition = Resolve(handle, module);

            _name = new Lazy<string>(() => Definition.Name.GetName(module), LazyThreadSafetyMode.PublicationOnly);
            _attributes = new Lazy<IReadOnlyList<AttributeWrapper>>(() => Definition.GetCustomAttributes().Select(x => AttributeWrapper.Create(x, module)).ToList(), LazyThreadSafetyMode.PublicationOnly);

            ParameterType = typeWrapper;

            IsOut = (Definition.Attributes & ParameterAttributes.Out) != 0;
            IsIn = (Definition.Attributes & ParameterAttributes.In) != 0;
            Optional = (Definition.Attributes & ParameterAttributes.Optional) != 0;
            HasDefaultValue = (Definition.Attributes & ParameterAttributes.HasDefault) != 0;
        }

        public Parameter Definition { get; }

        public ParameterHandle ParameterHandle { get; }

        public IReadOnlyList<AttributeWrapper> Attributes => _attributes.Value;

        public bool IsOut { get; }

        public bool IsIn { get; }

        public bool Optional { get; }

        public bool HasDefaultValue { get; }

        public IHandleTypeNamedWrapper ParameterType { get; }

        /// <inheritdoc />
        public string Name => _name.Value;

        public CompilationModule Module { get; }

        public Handle Handle { get; }

        public static ParameterWrapper Create(ParameterHandle handle, IHandleTypeNamedWrapper typeWrapper, CompilationModule module)
        {
            if (handle.IsNil)
            {
                return null;
            }

            return _registeredTypes.GetOrAdd(handle, handleCreate => new ParameterWrapper(handleCreate, typeWrapper, module));
        }

        private static Parameter Resolve(ParameterHandle handle, CompilationModule compilation)
        {
            return compilation.MetadataReader.GetParameter(handle);
        }
    }
}
