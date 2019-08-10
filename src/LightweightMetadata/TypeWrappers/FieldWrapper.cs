// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Reflection.Metadata;
using System.Threading;

namespace LightweightMetadata
{
    /// <summary>
    /// Wraps a FieldDefinition.
    /// </summary>
    public class FieldWrapper : IHandleTypeNamedWrapper, IHasAttributes
    {
        private readonly Lazy<string> _name;

        private readonly Lazy<IReadOnlyList<AttributeWrapper>> _attributes;
        private readonly Lazy<TypeWrapper> _declaringType;
        private readonly Lazy<object> _defaultValue;
        private readonly Lazy<IHandleTypeNamedWrapper> _fieldType;
        private readonly Lazy<ulong> _longEnumValue;

        private FieldWrapper(FieldDefinitionHandle handle, AssemblyMetadata assemblyMetadata)
        {
            FieldDefinitionHandle = handle;
            AssemblyMetadata = assemblyMetadata;
            Handle = handle;
            Definition = Resolve();

            _declaringType = new Lazy<TypeWrapper>(() => TypeWrapper.Create(Definition.GetDeclaringType(), AssemblyMetadata), LazyThreadSafetyMode.PublicationOnly);

            _name = new Lazy<string>(() => Definition.Name.GetName(assemblyMetadata), LazyThreadSafetyMode.PublicationOnly);
            _attributes = new Lazy<IReadOnlyList<AttributeWrapper>>(() => AttributeWrapper.Create(Definition.GetCustomAttributes(), assemblyMetadata), LazyThreadSafetyMode.PublicationOnly);

            _defaultValue = new Lazy<object>(() => Definition.GetDefaultValue().ReadConstant(assemblyMetadata));
            IsStatic = (Definition.Attributes & FieldAttributes.Static) != 0;

            _longEnumValue = new Lazy<ulong>(() => Convert.ToUInt64(DefaultValue, CultureInfo.InvariantCulture), LazyThreadSafetyMode.PublicationOnly);

            _fieldType = new Lazy<IHandleTypeNamedWrapper>(() => Definition.DecodeSignature(assemblyMetadata.TypeProvider, new GenericContext(this)), LazyThreadSafetyMode.PublicationOnly);

            switch (Definition.Attributes & FieldAttributes.FieldAccessMask)
            {
                case FieldAttributes.Public:
                    Accessibility = EntityAccessibility.Public;
                    break;
                case FieldAttributes.FamANDAssem:
                    Accessibility = EntityAccessibility.PrivateProtected;
                    break;
                case FieldAttributes.Assembly:
                    Accessibility = EntityAccessibility.Internal;
                    break;
                case FieldAttributes.Family:
                    Accessibility = EntityAccessibility.Protected;
                    break;
                case FieldAttributes.FamORAssem:
                    Accessibility = EntityAccessibility.ProtectedInternal;
                    break;
                default:
                    Accessibility = EntityAccessibility.Private;
                    break;
            }

            IsReadOnly = (Definition.Attributes & FieldAttributes.InitOnly) != 0;
            IsConst = (Definition.Attributes & FieldAttributes.Literal) != 0;
        }

        /// <summary>
        /// Gets the resolved method definition.
        /// </summary>
        public FieldDefinition Definition { get; }

        /// <summary>
        /// Gets the method definition handle.
        /// </summary>
        public FieldDefinitionHandle FieldDefinitionHandle { get; }

        /// <inheritdoc />
        public IReadOnlyList<AttributeWrapper> Attributes => _attributes.Value;

        /// <inheritdoc />
        public string Name => _name.Value;

        /// <inheritdoc />
        public AssemblyMetadata AssemblyMetadata { get; }

        /// <inheritdoc />
        public Handle Handle { get; }

        /// <inheritdoc />
        public string FullName => DeclaringType.FullName + "." + Name;

        /// <inheritdoc />
        public string ReflectionFullName => DeclaringType.ReflectionFullName + "." + Name;

        /// <inheritdoc />
        public string TypeNamespace => DeclaringType.TypeNamespace;

        /// <inheritdoc />
        public EntityAccessibility Accessibility { get; }

        /// <inheritdoc />
        public bool IsAbstract => false;

        /// <inheritdoc />
        public bool IsValueType => FieldType.IsValueType;

        /// <summary>
        /// Gets a value indicating whether the field is static.
        /// </summary>
        public bool IsStatic { get; }

        /// <summary>
        /// Gets a value indicating whether the field is read only.
        /// </summary>
        public bool IsReadOnly { get; }

        /// <summary>
        /// Gets a value indicating whether the field is a constant.
        /// </summary>
        public bool IsConst { get; }

        /// <summary>
        /// Gets the default value of the field.
        /// </summary>
        public object DefaultValue => _defaultValue.Value;

        /// <summary>
        /// Gets the type that is declaring this field.
        /// </summary>
        public TypeWrapper DeclaringType => _declaringType.Value;

        /// <summary>
        /// Gets the type of the field.
        /// </summary>
        public IHandleTypeNamedWrapper FieldType => _fieldType.Value;

        /// <inheritdoc />
        public KnownTypeCode KnownType => KnownTypeCode.None;

        /// <summary>
        /// Gets an internal value for helping with numeric to enum conversion since we compare in uint64.
        /// Note be careful to only call if a enum type.
        /// </summary>
        internal ulong LongDefaultValue => _longEnumValue.Value;

        /// <summary>
        /// Creates a instance of the method, if there is already not an instance.
        /// </summary>
        /// <param name="handle">The handle to the instance.</param>
        /// <param name="assemblyMetadata">The module that contains the instance.</param>
        /// <returns>The wrapper.</returns>
        public static FieldWrapper Create(FieldDefinitionHandle handle, AssemblyMetadata assemblyMetadata)
        {
            if (handle.IsNil)
            {
                return null;
            }

            return new FieldWrapper(handle, assemblyMetadata);
        }

        /// <summary>
        /// Creates a array instances of a type.
        /// </summary>
        /// <param name="collection">The collection to create.</param>
        /// <param name="assemblyMetadata">The module to use in creation.</param>
        /// <returns>The list of the type.</returns>
        public static IReadOnlyList<FieldWrapper> Create(in FieldDefinitionHandleCollection collection, AssemblyMetadata assemblyMetadata)
        {
            var output = new FieldWrapper[collection.Count];

            int i = 0;
            foreach (var element in collection)
            {
                output[i] = Create(element, assemblyMetadata);
                i++;
            }

            return output;
        }

        /// <summary>
        /// Determines if the field is fixed.
        /// </summary>
        /// <param name="bufferSize">The output buffer size.</param>
        /// <param name="type">The output type of the fixed buffer.</param>
        /// <returns>If the field is fixed buffer or not.</returns>
        public bool TryGetFixed(out int bufferSize, out IHandleTypeNamedWrapper type)
        {
            if (Attributes.TryGetKnownAttribute(KnownAttribute.FixedBuffer, out var fixedBuffer))
            {
                bufferSize = (int)fixedBuffer.FixedArguments[1].Value;
                type = (IHandleTypeNamedWrapper)fixedBuffer.FixedArguments[0].Value;
                return true;
            }

            bufferSize = 0;
            type = null;
            return false;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return Name;
        }

        private FieldDefinition Resolve()
        {
            return AssemblyMetadata.MetadataReader.GetFieldDefinition(FieldDefinitionHandle);
        }
    }
}