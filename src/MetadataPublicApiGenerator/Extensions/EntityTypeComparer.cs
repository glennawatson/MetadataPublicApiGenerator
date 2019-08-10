// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Text;

using LightweightMetadata;

namespace MetadataPublicApiGenerator.Extensions
{
    internal class EntityTypeComparer : IComparer<IHandleNameWrapper>
    {
        public static EntityTypeComparer Default { get; } = new EntityTypeComparer();

        /// <inheritdoc />
        public int Compare(IHandleNameWrapper x, IHandleNameWrapper y)
        {
            if (ReferenceEquals(x, y))
            {
                return 0;
            }

            if (x == null)
            {
                return -1;
            }

            if (y == null)
            {
                return 1;
            }

            if (x.Handle.Kind == y.Handle.Kind)
            {
                return CompareTypeName(x, y);
            }

            var xWeight = GetTypeWeight(x);
            var yWeight = GetTypeWeight(y);

            if (xWeight == yWeight)
            {
                return CompareTypeName(x, y);
            }

            return xWeight < yWeight ? -1 : 1;
        }

        private static int GetTypeWeight(IHandleNameWrapper handle)
        {
            switch (handle.Handle.Kind)
            {
                case HandleKind.FieldDefinition:
                    return 1;
                case HandleKind.EventDefinition:
                    return 4;
                case HandleKind.PropertyDefinition:
                    return 5;
                case HandleKind.MethodSpecification:
                case HandleKind.MethodDefinition:
                    if (handle is MethodWrapper method)
                    {
                        switch (method.MethodKind)
                        {
                            case SymbolMethodKind.Constructor:
                                return 2;
                            case SymbolMethodKind.Destructor:
                                return 3;
                            case SymbolMethodKind.BuiltinOperator:
                                return 6;
                        }
                    }

                    return 7;
                case HandleKind.TypeDefinition:
                case HandleKind.TypeSpecification:
                    return 8;
                case HandleKind.InterfaceImplementation:
                    return 9;
            }

            return 10;
        }

        private static int CompareTypeName(IHandleNameWrapper x, IHandleNameWrapper y)
        {
            if (!(x is IHandleTypeNamedWrapper typeX) || !(y is IHandleTypeNamedWrapper typeY))
            {
                return string.Compare(x.FullName, y.FullName, StringComparison.InvariantCulture);
            }

            var accessibility = typeX.Accessibility.CompareTo(typeY.Accessibility);

            if (accessibility != 0)
            {
                return accessibility;
            }

            return string.Compare(x.FullName, y.FullName, StringComparison.InvariantCulture);
        }
    }
}
