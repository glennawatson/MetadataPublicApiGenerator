// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using ICSharpCode.Decompiler.TypeSystem;
using Microsoft.CodeAnalysis;
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
        public static ExpressionSyntax LiteralParameterFromType(ICompilation compilation, IType type, object value)
        {
            type = type.GetRealType(compilation);

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

            throw new Exception($"Unknown parameter type for a parameter: {type.Name}");
        }

        public static PredefinedTypeSyntax EnumType(ICompilation compilation, IType enumType)
        {
            enumType = enumType.GetRealType(compilation);

            switch (enumType.GetDefinition()?.KnownTypeCode)
            {
                case KnownTypeCode.SByte:
                    return SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.SByteKeyword));
                case KnownTypeCode.Byte:
                    return SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.ByteKeyword));
                case KnownTypeCode.Int16:
                    return SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.ShortKeyword));
                case KnownTypeCode.UInt16:
                    return SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.UShortKeyword));
                case KnownTypeCode.Int32:
                    return SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.IntKeyword));
                case KnownTypeCode.UInt32:
                    return SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.UIntKeyword));
                case KnownTypeCode.Int64:
                    return SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.LongKeyword));
                case KnownTypeCode.UInt64:
                    return SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.ULongKeyword));
            }

            throw new Exception($"Unknown parameter type for a enum base type: {enumType.Name}");
        }

        public static SyntaxToken OperatorNameToToken(string operatorName)
        {
            switch (operatorName)
            {
                case "op_Explicit":
                    return SyntaxFactory.Token(SyntaxKind.ExplicitKeyword);
                case "op_Equality":
                    return SyntaxFactory.Token(SyntaxKind.EqualsKeyword);
                case "op_Inequality":
                    return SyntaxFactory.Token(SyntaxKind.ExclamationEqualsToken);
                case "op_GreaterThan":
                    return SyntaxFactory.Token(SyntaxKind.GreaterThanToken);
                case "op_LessThan":
                    return SyntaxFactory.Token(SyntaxKind.LessThanToken);
                case "op_GreaterThanOrEqual":
                    return SyntaxFactory.Token(SyntaxKind.GreaterThanGreaterThanEqualsToken);
                case "op_LessThanOrEqual:":
                    return SyntaxFactory.Token(SyntaxKind.LessThanLessThanEqualsToken);
                case "op_BitwiseAnd":
                    return SyntaxFactory.Token(SyntaxKind.BitwiseNotExpression);
                case "op_BitwiseOr":
                    return SyntaxFactory.Token(SyntaxKind.BitwiseOrExpression);
                case "op_Addition":
                    return SyntaxFactory.Token(SyntaxKind.AddKeyword);
                case "op_Subtraction":
                    return SyntaxFactory.Token(SyntaxKind.SubtractExpression);
                case "op_Division":
                    return SyntaxFactory.Token(SyntaxKind.DivideExpression);
                case "op_Modulus":
                    return SyntaxFactory.Token(SyntaxKind.ModuloExpression);
                case "op_Multiply":
                    return SyntaxFactory.Token(SyntaxKind.MultiplyExpression);
                case "op_LeftShift":
                    return SyntaxFactory.Token(SyntaxKind.LeftShiftExpression);
                case "op_RightShift":
                    return SyntaxFactory.Token(SyntaxKind.RightShiftExpression);
                case "op_ExclusiveOr":
                    return SyntaxFactory.Token(SyntaxKind.ExclusiveOrExpression);
                case "op_UnaryNegation":
                    return SyntaxFactory.Token(SyntaxKind.UnaryMinusExpression);
                case "op_UnaryPlus":
                    return SyntaxFactory.Token(SyntaxKind.UnaryPlusExpression);
                case "op_LogicalNot":
                    return SyntaxFactory.Token(SyntaxKind.LogicalNotExpression);
                case "op_False":
                    return SyntaxFactory.Token(SyntaxKind.FalseKeyword);
                case "op_True":
                    return SyntaxFactory.Token(SyntaxKind.TrueKeyword);
                case "op_Increment":
                    return SyntaxFactory.Token(SyntaxKind.PostIncrementExpression);
                case "op_Decrement":
                    return SyntaxFactory.Token(SyntaxKind.PostDecrementExpression);
                case "op_Implicit":
                    return SyntaxFactory.Token(SyntaxKind.ImplicitKeyword);
                case "op_OnesComplement":
                    return SyntaxFactory.Token(SyntaxKind.complement)
            }
        }
    }
}
