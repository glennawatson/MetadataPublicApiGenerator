// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using LightweightMetadata;
using LightweightMetadata.Extensions;
using LightweightMetadata.TypeWrappers;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace MetadataPublicApiGenerator.Extensions
{
    /// <summary>
    /// Extension methods that produce modifiers.
    /// </summary>
    internal static class ModifierExtensions
    {
        public static SyntaxTokenList GetModifiers(this TypeWrapper typeDefinition)
        {
            var modifierList = new List<SyntaxKind>();

            modifierList.AddRange(AccessibilityToSyntaxKind(typeDefinition.Accessibility));

            if (typeDefinition.TypeKind != SymbolTypeKind.Interface && typeDefinition.IsAbstract && !typeDefinition.IsStatic)
            {
                modifierList.Add(SyntaxKind.AbstractKeyword);
            }

            if (typeDefinition.TypeKind != SymbolTypeKind.Interface && typeDefinition.IsStatic)
            {
                modifierList.Add(SyntaxKind.StaticKeyword);
            }

            if (typeDefinition.TypeKind != SymbolTypeKind.Interface && typeDefinition.IsSealed && !typeDefinition.IsStatic && !typeDefinition.IsEnumType)
            {
                modifierList.Add(SyntaxKind.SealedKeyword);
            }

            return SyntaxFactory.TokenList(modifierList.Select(SyntaxFactory.Token));
        }

        public static SyntaxTokenList GetModifiers(this MethodWrapper method)
        {
            return SyntaxFactory.TokenList(GetModifiersList(method).Select(SyntaxFactory.Token));
        }

        public static SyntaxTokenList GetModifiers(this PropertyWrapper property)
        {
            var modifierList = new List<SyntaxKind>();

            modifierList.AddRange(AccessibilityToSyntaxKind(property.Accessibility));

            var anyGetter = property.AnyAccessor;

            if (anyGetter == null)
            {
                return SyntaxFactory.TokenList(modifierList.Select(SyntaxFactory.Token));
            }

            foreach (var value in GetModifiersList(anyGetter))
            {
                if (!modifierList.Contains(value))
                {
                    modifierList.Add(value);
                }
            }

            return SyntaxFactory.TokenList(modifierList.Select(SyntaxFactory.Token));
        }

        public static SyntaxTokenList GetModifiers(this MethodWrapper accessor, PropertyWrapper property)
        {
            var modifierList = new List<SyntaxKind>();

            if (property.Accessibility != accessor.Accessibility)
            {
                modifierList.AddRange(AccessibilityToSyntaxKind(accessor.Accessibility));
            }

            return SyntaxFactory.TokenList(modifierList.Select(SyntaxFactory.Token));
        }

        public static SyntaxTokenList GetModifiers(this FieldWrapper field)
        {
            var modifierList = new List<SyntaxKind>();

            modifierList.AddRange(AccessibilityToSyntaxKind(field.Accessibility));

            if (field.IsStatic)
            {
                modifierList.Add(SyntaxKind.StaticKeyword);
            }

            return SyntaxFactory.TokenList(modifierList.Select(SyntaxFactory.Token));
        }

        public static SyntaxTokenList GetModifiers(this EventWrapper eventDefinition)
        {
            var method = eventDefinition.AnyAccessor;

            return SyntaxFactory.TokenList(GetModifiersList(method).Select(SyntaxFactory.Token));
        }

        public static SyntaxTokenList GetModifiers(this ParameterWrapper parameter)
        {
            var modifierList = new List<SyntaxKind>();
            if (parameter.IsIn)
            {
                modifierList.Add(SyntaxKind.InKeyword);
            }

            if (parameter.IsOut)
            {
                modifierList.Add(SyntaxKind.OutKeyword);
            }

            if (parameter.Attributes.HasKnownAttribute(KnownAttribute.ParamArray))
            {
                modifierList.Add(SyntaxKind.ParamsKeyword);
            }

            return SyntaxFactory.TokenList(modifierList.Select(SyntaxFactory.Token));
        }

        private static IEnumerable<SyntaxKind> GetModifiersList(MethodWrapper method)
        {
            var modifierList = new List<SyntaxKind>();

            modifierList.AddRange(AccessibilityToSyntaxKind(method.Accessibility));

            if (method.IsAbstract && !method.IsStatic)
            {
                modifierList.Add(SyntaxKind.AbstractKeyword);
            }

            if (method.IsStatic)
            {
                modifierList.Add(SyntaxKind.StaticKeyword);
            }

            if (method.IsSealed && !method.IsStatic && !method.IsDelegate)
            {
                modifierList.Add(SyntaxKind.SealedKeyword);
            }

            if (method.IsOverride)
            {
                modifierList.Add(SyntaxKind.OverrideKeyword);
            }

            if (method.IsVirtual)
            {
                modifierList.Add(SyntaxKind.VirtualKeyword);
            }

            return modifierList;
        }

        private static IEnumerable<SyntaxKind> AccessibilityToSyntaxKind(EntityAccessibility accessibility)
        {
            switch (accessibility)
            {
                case EntityAccessibility.Internal:
                    return new[] { SyntaxKind.InternalKeyword };
                case EntityAccessibility.Private:
                    return new[] { SyntaxKind.PrivateKeyword };
                case EntityAccessibility.PrivateProtected:
                    return new[] { SyntaxKind.PrivateKeyword, SyntaxKind.ProtectedKeyword };
                case EntityAccessibility.Protected:
                    return new[] { SyntaxKind.ProtectedKeyword };
                case EntityAccessibility.ProtectedInternal:
                    return new[] { SyntaxKind.ProtectedKeyword, SyntaxKind.InternalKeyword };
                case EntityAccessibility.Public:
                    return new[] { SyntaxKind.PublicKeyword };
                default:
                    return Array.Empty<SyntaxKind>();
            }
        }
    }
}
