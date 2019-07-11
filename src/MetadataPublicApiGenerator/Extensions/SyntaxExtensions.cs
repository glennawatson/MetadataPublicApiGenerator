// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LightweightMetadata;
using LightweightMetadata.TypeWrappers;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MetadataPublicApiGenerator.Extensions
{
    internal static class SyntaxExtensions
    {
        public static TypeDeclarationSyntax AddBaseList(this TypeDeclarationSyntax typeSyntax, IHandleTypeNamedWrapper baseEntity, IReadOnlyList<InterfaceImplementationWrapper> interfaces)
        {
            var bases = new List<BaseTypeSyntax>(1 + interfaces.Count);

            if (baseEntity != null && baseEntity.KnownType != KnownTypeCode.Object)
            {
                bases.Add(SyntaxFactory.SimpleBaseType(SyntaxFactory.IdentifierName(baseEntity.ReflectionFullName)));
            }

            bases.AddRange(interfaces.Select(x => SyntaxFactory.SimpleBaseType(SyntaxFactory.IdentifierName(x.ReflectionFullName))));

            if (bases.Count != 0)
            {
                return typeSyntax.WithBaseList(SyntaxFactory.BaseList(SyntaxFactory.SeparatedList(bases)));
            }

            return typeSyntax;
        }
    }
}
