// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.TypeSystem;
using ICSharpCode.Decompiler.TypeSystem.Implementation;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MetadataPublicApiGenerator
{
    /// <summary>
    /// A helper for generating expressions for arguments.
    /// </summary>
    internal static class SyntaxHelper
    {
        /// <summary>
        /// Get the ExpressionSyntax from a type.
        /// </summary>
        /// <param name="compilation">The compilation for the current class.</param>
        /// <param name="type">The type to convert from.</param>
        /// <param name="value">The value to set.</param>
        /// <returns>The expression syntax.</returns>
        public static ExpressionSyntax AttributeParameterFromType(ICompilation compilation, IType type, object value)
        {
            if (type is UnknownType)
            {
                type = compilation.GetReferenceTypeDefinitionsWithFullName(type.FullName).FirstOrDefault();
            }

            switch (type.GetDefinition()?.KnownTypeCode)
            {
                case KnownTypeCode.Char:
                    return SyntaxFactory.LiteralExpression(SyntaxKind.CharacterLiteralExpression, SyntaxFactory.Literal((char)value));
                case KnownTypeCode.Boolean:
                    bool testValue = (bool)value;
                    return SyntaxFactory.LiteralExpression(testValue ? SyntaxKind.TrueLiteralExpression : SyntaxKind.FalseLiteralExpression);
                case KnownTypeCode.SByte:
                    return SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal((sbyte)value));
                case KnownTypeCode.Byte:
                    return SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal((byte)value));
                case KnownTypeCode.Int16:
                    return SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal((short)value));
                case KnownTypeCode.UInt16:
                    return SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal((ushort)value));
                case KnownTypeCode.Int32:
                    return SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal((int)value));
                case KnownTypeCode.UInt32:
                    return SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal((uint)value));
                case KnownTypeCode.Int64:
                    return SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal((long)value));
                case KnownTypeCode.UInt64:
                    return SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal((ulong)value));
                case KnownTypeCode.Single:
                    return SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal((float)value));
                case KnownTypeCode.Double:
                    return SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal((double)value));
                case KnownTypeCode.String:
                    return SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal((string)value));
                case KnownTypeCode.Type:
                    return SyntaxFactory.TypeOfExpression(SyntaxFactory.IdentifierName(((Type)value).FullName));
                case KnownTypeCode.Array:
                    return SyntaxFactory.ArrayCreationExpression(SyntaxFactory.ArrayType(
                        SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.StringLiteralExpression))));
            }

            return null;
        }
    }
}
