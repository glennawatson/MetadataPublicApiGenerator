// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MetadataPublicApiGenerator.Compilation;

namespace MetadataPublicApiGenerator.Extensions
{
    internal static class KnownAttributeExtensions
    {
        private static readonly string[] typeNames =
        {
            default,
            "System.Runtime.CompilerServices." + nameof(CompilerGeneratedAttribute),
            "System.Runtime.CompilerServices." + nameof(ExtensionAttribute),
            "System.Runtime.CompilerServices." + nameof(DynamicAttribute),
            "System.Runtime.CompilerServices." + nameof(TupleElementNamesAttribute),
            "System.Diagnostics." + nameof(ConditionalAttribute),
            "System." + nameof(ObsoleteAttribute),
            "System.Runtime.CompilerServices." + "IsReadOnlyAttribute",
            "System.Diagnostics." + nameof(DebuggerHiddenAttribute),
            "System.Diagnostics." + nameof(DebuggerStepThroughAttribute),

            // Assembly attributes:
            "System.Reflection." + nameof(AssemblyVersionAttribute),
            "System.Runtime.CompilerServices." + nameof(InternalsVisibleToAttribute),
            "System.Runtime.CompilerServices." + nameof(TypeForwardedToAttribute),
            "System.Runtime.CompilerServices." + nameof(ReferenceAssemblyAttribute),

            // Type attributes:
            "System." + nameof(SerializableAttribute),
            "System." + nameof(FlagsAttribute),
            "System.Runtime.InteropServices." + nameof(ComImportAttribute),
            "System.Runtime.InteropServices." + nameof(CoClassAttribute),
            "System.Runtime.InteropServices." + nameof(StructLayoutAttribute),
            "System.Reflection." + nameof(DefaultMemberAttribute),
            "System.Runtime.CompilerServices." + "IsByRefLikeAttribute",
            "System.Runtime.CompilerServices." + nameof(IteratorStateMachineAttribute),
            "System.Runtime.CompilerServices." + nameof(AsyncStateMachineAttribute),

            // Field attributes:
            "System.Runtime.InteropServices." + nameof(FieldOffsetAttribute),
            "System." + nameof(NonSerializedAttribute),
            "System.Runtime.CompilerServices." + nameof(DecimalConstantAttribute),
            "System.Runtime.CompilerServices." + nameof(FixedBufferAttribute),

            // Method attributes:
            "System.Runtime.InteropServices." + nameof(DllImportAttribute),
            "System.Runtime.InteropServices." + nameof(PreserveSigAttribute),
            "System.Runtime.CompilerServices." + nameof(MethodImplAttribute),

            // Property attributes:
            "System.Runtime.CompilerServices." + nameof(IndexerNameAttribute),

            // Parameter attributes:
            "System." + nameof(ParamArrayAttribute),
            "System.Runtime.InteropServices." + nameof(InAttribute),
            "System.Runtime.InteropServices." + nameof(OutAttribute),
            "System.Runtime.InteropServices." + nameof(OptionalAttribute),
            "System.Runtime.CompilerServices." + nameof(CallerMemberNameAttribute),
            "System.Runtime.CompilerServices." + nameof(CallerFilePathAttribute),
            "System.Runtime.CompilerServices." + nameof(CallerLineNumberAttribute),

            // Marshalling attributes:
            "System.Runtime.InteropServices." + nameof(MarshalAsAttribute),

            // Security attributes:
            "System.Security.Permissions." + "PermissionSetAttribute",
        };

        public static KnownAttribute IsKnownAttributeType(this CustomAttribute attributeType, CompilationModule compilation)
        {
            var method = ((MethodDefinitionHandle)attributeType.Constructor).Resolve(compilation);
            var declaredType = method.GetDeclaringType().GetName(compilation);
            var index = Array.IndexOf(typeNames, declaredType);
            if (index < 0)
            {
                return KnownAttribute.None;
            }

            return (KnownAttribute)index;
        }
    }
}
