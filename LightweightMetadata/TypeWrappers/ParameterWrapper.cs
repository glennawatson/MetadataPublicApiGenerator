// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Threading;
using LightweightMetadata.Extensions;

namespace LightweightMetadata.TypeWrappers
{
    /// <summary>
    /// Wraps a Parameter class.
    /// </summary>
    [DebuggerDisplay("{" + nameof(Name) + "}")]
    public class ParameterWrapper : IHandleNameWrapper, IHasAttributes
    {
        private static readonly Dictionary<ParameterHandle, ParameterWrapper> _registeredTypes = new Dictionary<ParameterHandle, ParameterWrapper>();

        private readonly Lazy<string> _name;

        private readonly Lazy<IReadOnlyList<AttributeWrapper>> _attributes;

        private ParameterWrapper(ParameterHandle handle, IHandleTypeNamedWrapper typeWrapper, CompilationModule module)
        {
            CompilationModule = module;
            ParameterHandle = handle;
            Handle = handle;
            Definition = Resolve(handle, module);

            _name = new Lazy<string>(() => Definition.Name.GetName(module), LazyThreadSafetyMode.PublicationOnly);
            _attributes = new Lazy<IReadOnlyList<AttributeWrapper>>(() => AttributeWrapper.Create(Definition.GetCustomAttributes(), module), LazyThreadSafetyMode.PublicationOnly);

            ParameterType = typeWrapper;

            IsOut = (Definition.Attributes & ParameterAttributes.Out) != 0;
            IsIn = (Definition.Attributes & ParameterAttributes.In) != 0;
            Optional = (Definition.Attributes & ParameterAttributes.Optional) != 0;
            HasDefaultValue = (Definition.Attributes & ParameterAttributes.HasDefault) != 0;
        }

        /// <summary>
        /// Gets the parameter definition.
        /// </summary>
        public Parameter Definition { get; }

        /// <summary>
        /// Gets the parameter handle.
        /// </summary>
        public ParameterHandle ParameterHandle { get; }

        /// <inheritdoc/>
        public IReadOnlyList<AttributeWrapper> Attributes => _attributes.Value;

        /// <summary>
        /// Gets a value indicating whether the parameter is a output parameter.
        /// </summary>
        public bool IsOut { get; }

        /// <summary>
        /// Gets a value indicating whether the parameter is a input parameter.
        /// </summary>
        public bool IsIn { get; }

        /// <summary>
        /// Gets a value indicating whether the parameter is optional.
        /// </summary>
        public bool Optional { get; }

        /// <summary>
        /// Gets a value indicating whether the parameter has a default value.
        /// </summary>
        public bool HasDefaultValue { get; }

        /// <summary>
        /// Gets the type of the parameter.
        /// </summary>
        public IHandleTypeNamedWrapper ParameterType { get; }

        /// <inheritdoc />
        public string Name => _name.Value;

        /// <inheritdoc/>
        public CompilationModule CompilationModule { get; }

        /// <inheritdoc/>
        public Handle Handle { get; }

        /// <summary>
        /// Creates a instance of the method, if there is already not an instance.
        /// </summary>
        /// <param name="handle">The handle to the instance.</param>
        /// <param name="typeWrapper">The type of the parameter.</param>
        /// <param name="module">The module that contains the instance.</param>
        /// <returns>The wrapper.</returns>
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
