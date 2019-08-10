// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Metadata;
using System.Threading;

namespace LightweightMetadata
{
    /// <summary>
    /// Wraps a Parameter class.
    /// </summary>
    public class ParameterWrapper : IHandleNameWrapper, IHasAttributes
    {
        private readonly Lazy<string> _name;
        private readonly Lazy<object> _defaultValue;
        private readonly Lazy<IReadOnlyList<AttributeWrapper>> _attributes;
        private readonly Lazy<ParameterReferenceKind> _referenceKind;

        private ParameterWrapper(ParameterHandle handle, IHandleTypeNamedWrapper typeWrapper, AssemblyMetadata assemblyMetadata)
        {
            AssemblyMetadata = assemblyMetadata;
            ParameterHandle = handle;
            Handle = handle;
            Definition = Resolve(handle, assemblyMetadata);

            _name = new Lazy<string>(() => Definition.Name.GetName(assemblyMetadata).GetKeywordSafeName(), LazyThreadSafetyMode.PublicationOnly);
            _attributes = new Lazy<IReadOnlyList<AttributeWrapper>>(() => AttributeWrapper.Create(Definition.GetCustomAttributes(), assemblyMetadata), LazyThreadSafetyMode.PublicationOnly);

            ParameterType = typeWrapper;

            Optional = (Definition.Attributes & ParameterAttributes.Optional) != 0;
            HasDefaultValue = (Definition.Attributes & ParameterAttributes.HasDefault) != 0;

            _defaultValue = new Lazy<object>(() => !HasDefaultValue ? null : Definition.GetDefaultValue().ReadConstant(assemblyMetadata), LazyThreadSafetyMode.PublicationOnly);
            _referenceKind = new Lazy<ParameterReferenceKind>(GetReferenceKind, LazyThreadSafetyMode.PublicationOnly);
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

        /// <inheritdoc />
        public string FullName => Name;

        /// <inheritdoc/>
        public AssemblyMetadata AssemblyMetadata { get; }

        /// <inheritdoc/>
        public Handle Handle { get; }

        /// <summary>
        /// Gets the default value if there is one.
        /// </summary>
        public object DefaultValue => _defaultValue.Value;

        /// <summary>
        /// Gets the type of reference this parameter is.
        /// </summary>
        public ParameterReferenceKind ReferenceKind => _referenceKind.Value;

        /// <summary>
        /// Creates a instance of the method, if there is already not an instance.
        /// </summary>
        /// <param name="handle">The handle to the instance.</param>
        /// <param name="typeWrapper">The type of the parameter.</param>
        /// <param name="assemblyMetadata">The module that contains the instance.</param>
        /// <returns>The wrapper.</returns>
        public static ParameterWrapper Create(ParameterHandle handle, IHandleTypeNamedWrapper typeWrapper, AssemblyMetadata assemblyMetadata)
        {
            if (handle.IsNil)
            {
                return null;
            }

            return new ParameterWrapper(handle, typeWrapper, assemblyMetadata);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return FullName;
        }

        private static Parameter Resolve(ParameterHandle handle, AssemblyMetadata assemblyMetadata)
        {
            return assemblyMetadata.MetadataReader.GetParameter(handle);
        }

        private ParameterReferenceKind GetReferenceKind()
        {
            if ((Definition.Attributes & ParameterAttributes.Out) != 0)
            {
                return ParameterReferenceKind.Out;
            }

            if (Attributes.HasKnownAttribute(KnownAttribute.IsReadOnly))
            {
                return ParameterReferenceKind.In;
            }

            if (ParameterType is ByReferenceWrapper)
            {
                return ParameterReferenceKind.Ref;
            }

            return ParameterReferenceKind.None;
        }
    }
}
