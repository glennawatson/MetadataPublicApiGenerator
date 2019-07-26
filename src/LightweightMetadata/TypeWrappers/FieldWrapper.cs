// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Threading;
using LightweightMetadata.Extensions;

namespace LightweightMetadata.TypeWrappers
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

        private FieldWrapper(FieldDefinitionHandle handle, CompilationModule module)
        {
            FieldDefinitionHandle = handle;
            CompilationModule = module;
            Handle = handle;
            Definition = Resolve();

            _declaringType = new Lazy<TypeWrapper>(() => TypeWrapper.Create(Definition.GetDeclaringType(), CompilationModule), LazyThreadSafetyMode.PublicationOnly);

            _name = new Lazy<string>(() => Definition.Name.GetName(module), LazyThreadSafetyMode.PublicationOnly);
            _attributes = new Lazy<IReadOnlyList<AttributeWrapper>>(() => AttributeWrapper.Create(Definition.GetCustomAttributes(), module), LazyThreadSafetyMode.PublicationOnly);

            _defaultValue = new Lazy<object>(() => Definition.GetDefaultValue().ReadConstant(module));
            IsStatic = (Definition.Attributes & FieldAttributes.Static) != 0;

            _longEnumValue = new Lazy<ulong>(() => Convert.ToUInt64(DefaultValue, CultureInfo.InvariantCulture), LazyThreadSafetyMode.PublicationOnly);

            _fieldType = new Lazy<IHandleTypeNamedWrapper>(() => Definition.DecodeSignature(module.TypeProvider, new GenericContext(this)), LazyThreadSafetyMode.PublicationOnly);

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
        public CompilationModule CompilationModule { get; }

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

        /// <summary>
        /// Gets a value indicating whether the field is static.
        /// </summary>
        public bool IsStatic { get; }

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
        /// <param name="module">The module that contains the instance.</param>
        /// <returns>The wrapper.</returns>
        public static FieldWrapper Create(FieldDefinitionHandle handle, CompilationModule module)
        {
            if (handle.IsNil)
            {
                return null;
            }

            return new FieldWrapper(handle, module);
        }

        /// <summary>
        /// Creates a array instances of a type.
        /// </summary>
        /// <param name="collection">The collection to create.</param>
        /// <param name="module">The module to use in creation.</param>
        /// <returns>The list of the type.</returns>
        public static IReadOnlyList<FieldWrapper> Create(in FieldDefinitionHandleCollection collection, CompilationModule module)
        {
            var output = new FieldWrapper[collection.Count];

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
            return Name;
        }

        private FieldDefinition Resolve()
        {
            return CompilationModule.MetadataReader.GetFieldDefinition(FieldDefinitionHandle);
        }
    }
}