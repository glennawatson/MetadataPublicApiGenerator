// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace LightweightMetadata
{
    internal static class KnownAttributeNames
    {
        public static readonly string?[] TypeNames =
        {
            default,
            CompilerServices + nameof(CompilerGeneratedAttribute),
            CompilerServices + nameof(ExtensionAttribute),
            CompilerServices + nameof(DynamicAttribute),
            CompilerServices + nameof(TupleElementNamesAttribute),
            Diagnostics + nameof(ConditionalAttribute),
            System + nameof(ObsoleteAttribute),
            CompilerServices + "IsReadOnlyAttribute",
            Diagnostics + nameof(DebuggerHiddenAttribute),
            Diagnostics + nameof(DebuggerStepThroughAttribute),

            // Assembly attributes:
            Reflection + nameof(AssemblyVersionAttribute),
            CompilerServices + nameof(InternalsVisibleToAttribute),
            CompilerServices + nameof(TypeForwardedToAttribute),
            CompilerServices + nameof(ReferenceAssemblyAttribute),

            // Type attributes:
            System + nameof(SerializableAttribute),
            System + nameof(FlagsAttribute),
            InteropServices + nameof(ComImportAttribute),
            InteropServices + nameof(CoClassAttribute),
            InteropServices + nameof(StructLayoutAttribute),
            Reflection + nameof(DefaultMemberAttribute),
            CompilerServices + "IsByRefLikeAttribute",
            CompilerServices + nameof(IteratorStateMachineAttribute),
            CompilerServices + nameof(AsyncStateMachineAttribute),

            // Field attributes:
            InteropServices + nameof(FieldOffsetAttribute),
            System + nameof(NonSerializedAttribute),
            CompilerServices + nameof(DecimalConstantAttribute),
            CompilerServices + nameof(FixedBufferAttribute),

            // Method attributes:
            InteropServices + nameof(DllImportAttribute),
            InteropServices + nameof(PreserveSigAttribute),
            CompilerServices + nameof(MethodImplAttribute),

            // Property attributes:
            CompilerServices + nameof(IndexerNameAttribute),

            // Parameter attributes:
            System + nameof(ParamArrayAttribute),
            InteropServices + nameof(InAttribute),
            InteropServices + nameof(OutAttribute),
            InteropServices + nameof(OptionalAttribute),
            CompilerServices + nameof(CallerMemberNameAttribute),
            CompilerServices + nameof(CallerFilePathAttribute),
            CompilerServices + nameof(CallerLineNumberAttribute),

            // Marshalling attributes:
            InteropServices + nameof(MarshalAsAttribute),

            // Security attributes:
            Permissions + "PermissionSetAttribute",

            // Null context:
            CompilerServices + "NullableContextAttribute",
            CompilerServices + "NullableAttribute",
        };

        private const string CompilerServices = "System.Runtime.CompilerServices.";
        private const string InteropServices = "System.Runtime.InteropServices.";
        private const string Diagnostics = "System.Diagnostics.";
        private const string System = "System.";
        private const string Reflection = "System.Reflection.";
        private const string Permissions = "System.Security.Permissions.";
    }
}
