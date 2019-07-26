// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Linq;
using LightweightMetadata;
using LightweightMetadata.TypeWrappers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using static MetadataPublicApiGenerator.Helpers.SyntaxFactoryHelpers;

namespace MetadataPublicApiGenerator.Helpers
{
    /// <summary>
    /// A helper for generating expressions for arguments.
    /// </summary>
    internal static class SyntaxHelper
    {
        /// <summary>
        /// Get the ExpressionSyntax from a type.
        /// </summary>
        /// <param name="wrapper">The type to convert from.</param>
        /// <param name="value">The value to set.</param>
        /// <returns>The expression syntax.</returns>
        public static ExpressionSyntax GetValueExpression(ITypeNamedWrapper wrapper, object value)
        {
            switch (wrapper)
            {
                case ArrayTypeWrapper arrayTypeWrapper:
                    return ArrayCreationExpression(ArrayType(IdentifierName(arrayTypeWrapper.ElementType.ReflectionFullName), Array.Empty<ArrayRankSpecifierSyntax>()));
                case TypeWrapper typeWrapper when typeWrapper.IsEnumType:
                    return GetEnumNames(typeWrapper, value);
                case TypeWrapper typeWrapper:
                    return GetValueExpressionForKnownType(typeWrapper.KnownType, value);
                default:
                    return null;
            }
        }

        public static ExpressionSyntax GetValueExpressionForKnownType(KnownTypeCode underlyingType, object value)
        {
            if (value == null)
            {
                return LiteralExpression(SyntaxKind.NullLiteralExpression);
            }

            switch (underlyingType)
            {
                case KnownTypeCode.Char:
                    return LiteralExpression(SyntaxKind.CharacterLiteralExpression, Literal((char)value));
                case KnownTypeCode.Boolean:
                    bool testValue = (bool)value;
                    return LiteralExpression(testValue ? SyntaxKind.TrueLiteralExpression : SyntaxKind.FalseLiteralExpression);
                case KnownTypeCode.SByte:
                    return LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal((sbyte)value));
                case KnownTypeCode.Byte:
                    return LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal((byte)value));
                case KnownTypeCode.Int16:
                    return LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal((short)value));
                case KnownTypeCode.UInt16:
                    return LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal((ushort)value));
                case KnownTypeCode.Int32:
                    return LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal((int)value));
                case KnownTypeCode.UInt32:
                    return LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal((uint)value));
                case KnownTypeCode.Int64:
                    return LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal((long)value));
                case KnownTypeCode.UInt64:
                    return LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal((ulong)value));
                case KnownTypeCode.Single:
                    return LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal((float)value));
                case KnownTypeCode.Double:
                    return LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal((double)value));
                case KnownTypeCode.String:
                    return LiteralExpression(SyntaxKind.StringLiteralExpression, Literal((string)value));
                case KnownTypeCode.Object:
                    return TypeOfExpression(IdentifierName(((Type)value).FullName));
                case KnownTypeCode.Type:
                    return TypeOfExpression(IdentifierName(value.ToString()));
            }

            throw new Exception($"Unknown parameter type for a parameter: {underlyingType}");
        }

        public static SyntaxToken OperatorNameToToken(string operatorName)
        {
            switch (operatorName)
            {
                case "op_Equality":
                    return Token(SyntaxKind.EqualsEqualsToken);
                case "op_Inequality":
                    return Token(SyntaxKind.ExclamationEqualsToken);
                case "op_GreaterThan":
                    return Token(SyntaxKind.GreaterThanToken);
                case "op_LessThan":
                    return Token(SyntaxKind.LessThanToken);
                case "op_GreaterThanOrEqual":
                    return Token(SyntaxKind.GreaterThanEqualsToken);
                case "op_LessThanOrEqual":
                    return Token(SyntaxKind.LessThanEqualsToken);
                case "op_BitwiseAnd":
                    return Token(SyntaxKind.AmpersandToken);
                case "op_BitwiseOr":
                    return Token(SyntaxKind.BarToken);
                case "op_Addition":
                    return Token(SyntaxKind.PlusToken);
                case "op_Subtraction":
                    return Token(SyntaxKind.MinusToken);
                case "op_Division":
                    return Token(SyntaxKind.SlashToken);
                case "op_Modulus":
                    return Token(SyntaxKind.PercentToken);
                case "op_Multiply":
                    return Token(SyntaxKind.AsteriskToken);
                case "op_LeftShift":
                    return Token(SyntaxKind.LessThanLessThanToken);
                case "op_RightShift":
                    return Token(SyntaxKind.GreaterThanGreaterThanToken);
                case "op_ExclusiveOr":
                    return Token(SyntaxKind.CaretToken);
                case "op_UnaryNegation":
                    return Token(SyntaxKind.MinusToken);
                case "op_UnaryPlus":
                    return Token(SyntaxKind.PlusToken);
                case "op_LogicalNot":
                    return Token(SyntaxKind.ExclamationEqualsToken);
                case "op_False":
                    return Token(SyntaxKind.FalseKeyword);
                case "op_True":
                    return Token(SyntaxKind.TrueKeyword);
                case "op_Increment":
                    return Token(SyntaxKind.PlusPlusToken);
                case "op_Decrement":
                    return Token(SyntaxKind.MinusMinusToken);
                case "op_OnesComplement":
                    return Token(SyntaxKind.TildeToken);
                case "op_Implicit":
                    return Token(SyntaxKind.ImplicitKeyword);
                case "op_Explicit":
                    return Token(SyntaxKind.ExplicitKeyword);
            }

            throw new Exception($"Unknown name for a operator: {operatorName}");
        }

        private static ExpressionSyntax GetEnumNames(TypeWrapper enumType, object enumValue)
        {
            if (enumType.TryGetEnumName(enumValue, out var enumNames))
            {
                var memberAccesses = enumNames.Select(x => MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, IdentifierName(enumType.FullName), IdentifierName(x))).ToList();
                if (enumNames.Count == 1)
                {
                    return memberAccesses[0];
                }

                if (enumNames.Count > 1)
                {
                    var first = BinaryExpression(SyntaxKind.BitwiseOrExpression, memberAccesses[0], memberAccesses[1]);
                    for (int i = 2; i < enumNames.Count; ++i)
                    {
                        first = BinaryExpression(SyntaxKind.BitwiseOrExpression, first, memberAccesses[i]);
                    }

                    return first;
                }
            }

            if (!enumType.TryGetEnumType(out var underlyingType))
            {
                throw new ArgumentException("Invalid enum type.", nameof(enumType));
            }

            var knownType = underlyingType.KnownType;

            return GetValueExpressionForKnownType(knownType, enumValue);
        }
    }
}
