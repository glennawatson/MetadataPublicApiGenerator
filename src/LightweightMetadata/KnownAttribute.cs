// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace LightweightMetadata
{
    /// <summary>
    /// Represents known attribute types.
    /// </summary>
    public enum KnownAttribute
    {
        /// <summary>
        /// Not a known attribute.
        /// </summary>
        None,

        /// <summary>
        /// That the element it represents is compiler generated.
        /// </summary>
        CompilerGenerated,

        /// <summary>
        /// Marks a method as extension method; or a class as containing extension methods.
        /// </summary>
        Extension,

        /// <summary>
        /// The element is dynamic.
        /// </summary>
        Dynamic,

        /// <summary>
        /// Represents the name of Tuple elements.
        /// </summary>
        TupleElementNames,

        /// <summary>
        /// That the element is conditional compiled.
        /// </summary>
        Conditional,

        /// <summary>
        /// That the element is obsolete.
        /// </summary>
        Obsolete,

        /// <summary>
        /// That the element is read only.
        /// </summary>
        IsReadOnly,

        /// <summary>
        /// That the element is hidden from the debugger.
        /// </summary>
        DebuggerHidden,

        /// <summary>
        /// That the element will be skipped when break pointing the element.
        /// </summary>
        DebuggerStepThrough,

        /// <summary>
        /// The assembly version.
        /// </summary>
        AssemblyVersion,

        /// <summary>
        /// That the internal contents of the current assembly will be visible to another assembly.
        /// </summary>
        InternalsVisibleTo,

        /// <summary>
        /// That the type is forwarding to another.
        /// </summary>
        TypeForwardedTo,

        /// <summary>
        /// Represents the reference assembly.
        /// </summary>
        ReferenceAssembly,

        /// <summary>
        /// That the type is serializable.
        /// </summary>
        Serializable,

        /// <summary>
        /// That a enum represents flags.
        /// </summary>
        Flags,

        /// <summary>
        /// That the type imports com objects.
        /// </summary>
        ComImport,

        /// <summary>
        /// That the class is a com constructed object.
        /// </summary>
        CoClass,

        /// <summary>
        /// Represents how a struct should be laid out in memory.
        /// </summary>
        StructLayout,

        /// <summary>
        /// Represents a default member on the class.
        /// </summary>
        DefaultMember,

        /// <summary>
        /// That it is ref-like type.
        /// </summary>
        IsByRefLike,

        /// <summary>
        /// That it is a iterator state machine that is auto generated.
        /// </summary>
        IteratorStateMachine,

        /// <summary>
        /// That it is a async state machine that is auto generated.
        /// </summary>
        AsyncStateMachine,

        /// <summary>
        /// Gets a offset in memory where the field should be located.
        /// </summary>
        FieldOffset,

        /// <summary>
        /// Indicates the field should not be serialized.
        /// </summary>
        NonSerialized,

        /// <summary>
        /// A decimal constant.
        /// </summary>
        DecimalConstant,

        /// <summary>
        /// That the field is a fixed buffer.
        /// </summary>
        FixedBuffer,

        /// <summary>
        /// That the method uses p-invoke to execute.
        /// </summary>
        DllImport,

        /// <summary>
        /// If a method's p-invoke should automatically invoke exceptions or not.
        /// </summary>
        PreserveSig,

        /// <summary>
        /// Specifies the details of how a method is implemented.
        /// </summary>
        MethodImpl,

        /// <summary>
        /// Indicates the name by which an indexer is known in programming languages that do not support indexers directly.
        /// </summary>
        IndexerName,

        /// <summary>
        /// That this will use a param style array.
        /// </summary>
        ParamArray,

        /// <summary>
        /// This is a input parameter.
        /// </summary>
        In,

        /// <summary>
        /// This is a output parameter.
        /// </summary>
        Out,

        /// <summary>
        /// This parameter is optional.
        /// </summary>
        Optional,

        /// <summary>
        /// The parameter will automatically be filled with the caller member name.
        /// </summary>
        CallerMemberName,

        /// <summary>
        /// The parameter will automatically be filled with the caller file path.
        /// </summary>
        CallerFilePath,

        /// <summary>
        /// The parameter will automatically be filled with the caller line number.
        /// </summary>
        CallerLineNumber,

        /// <summary>
        /// That the value will be marshaled as a certain type of value.
        /// </summary>
        MarshalAs,

        /// <summary>
        /// A set of permissions needed.
        /// </summary>
        PermissionSet,

        /// <summary>
        /// The nullable context of the item.
        /// </summary>
        NullableContext,

        /// <summary>
        /// The nullable of the item.
        /// </summary>
        Nullable,
    }
}