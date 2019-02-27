// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace MetadataPublicApiGenerator.Compilation
{
    internal enum KnownAttribute
    {
        /// <summary>
        /// Not a known attribute
        /// </summary>
        None,

        CompilerGenerated,

        /// <summary>
        /// Marks a method as extension method; or a class as containing extension methods.
        /// </summary>
        Extension,
        Dynamic,
        TupleElementNames,
        Conditional,
        Obsolete,
        IsReadOnly,
        DebuggerHidden,
        DebuggerStepThrough,

        // Assembly attributes:
        AssemblyVersion,
        InternalsVisibleTo,
        TypeForwardedTo,
        ReferenceAssembly,

        // Type attributes:
        Serializable,
        Flags,
        ComImport,
        CoClass,
        StructLayout,
        DefaultMember,
        IsByRefLike,
        IteratorStateMachine,
        AsyncStateMachine,

        // Field attributes:
        FieldOffset,
        NonSerialized,
        DecimalConstant,
        FixedBuffer,

        // Method attributes:
        DllImport,
        PreserveSig,
        MethodImpl,

        // Property attributes:
        IndexerName,

        // Parameter attributes:
        ParamArray,
        In,
        Out,
        Optional,
        CallerMemberName,
        CallerFilePath,
        CallerLineNumber,

        // Marshalling attributes:
        MarshalAs,

        // Security attributes:
        PermissionSet,
    }
}