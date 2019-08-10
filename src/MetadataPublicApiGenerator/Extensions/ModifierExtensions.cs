// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

using LightweightMetadata;

using Microsoft.CodeAnalysis.CSharp;

namespace MetadataPublicApiGenerator.Extensions
{
    /// <summary>
    /// Extension methods that produce modifiers.
    /// </summary>
    internal static class ModifierExtensions
    {
        public static IReadOnlyCollection<SyntaxKind> GetModifiers(this TypeWrapper typeDefinition, MethodWrapper method)
        {
            var modifierList = GetModifiersList(typeDefinition);

            if (method.ReturningType is PointerWrapper || method.Parameters.Any(x => x.ParameterType is PointerWrapper))
            {
                modifierList.Add(SyntaxKind.UnsafeKeyword);
            }

            return modifierList;
        }

        public static IReadOnlyCollection<SyntaxKind> GetModifiers(this TypeWrapper typeDefinition)
        {
            return GetModifiersList(typeDefinition);
        }

        public static IReadOnlyCollection<SyntaxKind> GetModifiers(this MethodWrapper method)
        {
            if (method.DeclaringType.TypeKind == SymbolTypeKind.Interface)
            {
                return Array.Empty<SyntaxKind>();
            }

            return GetModifiersList(method);
        }

        public static IReadOnlyCollection<SyntaxKind> GetModifiers(this PropertyWrapper property)
        {
            if (property.DeclaringType.TypeKind == SymbolTypeKind.Interface)
            {
                return Array.Empty<SyntaxKind>();
            }

            var modifierList = new List<SyntaxKind>(6);

            modifierList.AddRange(AccessibilityToSyntaxKind(property.Accessibility));

            var anyGetter = property.AnyAccessor;

            if (anyGetter == null)
            {
                return modifierList;
            }

            foreach (var value in GetModifiersList(anyGetter))
            {
                if (!modifierList.Contains(value))
                {
                    modifierList.Add(value);
                }
            }

            if (property.ReturnType is PointerWrapper)
            {
                modifierList.Add(SyntaxKind.UnsafeKeyword);
            }

            return modifierList;
        }

        public static IReadOnlyCollection<SyntaxKind> GetModifiers(this MethodWrapper accessor, PropertyWrapper property)
        {
            var modifierList = new List<SyntaxKind>(6);

            if (property.Accessibility != accessor.Accessibility)
            {
                modifierList.AddRange(AccessibilityToSyntaxKind(accessor.Accessibility));
            }

            return modifierList;
        }

        public static IReadOnlyCollection<SyntaxKind> GetModifiers(this FieldWrapper field)
        {
            var modifierList = new List<SyntaxKind>(6);

            modifierList.AddRange(AccessibilityToSyntaxKind(field.Accessibility));

            if (field.IsConst)
            {
                modifierList.Add(SyntaxKind.ConstKeyword);
            }
            else
            {
                if (field.IsStatic)
                {
                    modifierList.Add(SyntaxKind.StaticKeyword);
                }

                if (field.IsReadOnly)
                {
                    modifierList.Add(SyntaxKind.ReadOnlyKeyword);
                }
            }

            if (field.FieldType is PointerWrapper)
            {
                modifierList.Add(SyntaxKind.UnsafeKeyword);
            }

            if (field.Attributes.HasKnownAttribute(KnownAttribute.FixedBuffer))
            {
                modifierList.Add(SyntaxKind.UnsafeKeyword);
                modifierList.Add(SyntaxKind.FixedKeyword);
            }

            return modifierList;
        }

        public static IReadOnlyCollection<SyntaxKind> GetModifiers(this EventWrapper eventDefinition)
        {
            var method = eventDefinition.AnyAccessor;

            return GetModifiersList(method);
        }

        public static IReadOnlyCollection<SyntaxKind> GetModifiers(this ParameterWrapper parameter, bool isExtensionMethod)
        {
            var modifierList = new List<SyntaxKind>(6);

            if (isExtensionMethod)
            {
                modifierList.Add(SyntaxKind.ThisKeyword);
            }

            switch (parameter.ReferenceKind)
            {
                case ParameterReferenceKind.In:
                    modifierList.Add(SyntaxKind.InKeyword);
                    break;
                case ParameterReferenceKind.Out:
                    modifierList.Add(SyntaxKind.OutKeyword);
                    break;
                case ParameterReferenceKind.Ref:
                    modifierList.Add(SyntaxKind.RefKeyword);
                    break;
            }

            if (parameter.Attributes.HasKnownAttribute(KnownAttribute.ParamArray))
            {
                modifierList.Add(SyntaxKind.ParamsKeyword);
            }

            return modifierList;
        }

        private static List<SyntaxKind> GetModifiersList(MethodWrapper method)
        {
            var modifierList = new List<SyntaxKind>(6);

            modifierList.AddRange(AccessibilityToSyntaxKind(method.Accessibility));

            if (method.IsAbstract && !method.IsStatic)
            {
                modifierList.Add(SyntaxKind.AbstractKeyword);
            }

            if (method.IsStatic)
            {
                modifierList.Add(SyntaxKind.StaticKeyword);
            }

            if (method.IsSealed && !method.IsStatic)
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

            if (method.ReturningType is PointerWrapper || method.Parameters.Any(x => x.ParameterType is PointerWrapper))
            {
                modifierList.Add(SyntaxKind.UnsafeKeyword);
            }

            return modifierList;
        }

        private static List<SyntaxKind> GetModifiersList(TypeWrapper typeWrapper)
        {
            var modifierList = new List<SyntaxKind>(AccessibilityToSyntaxKind(typeWrapper.Accessibility));

            if (typeWrapper.TypeKind != SymbolTypeKind.Interface && typeWrapper.IsAbstract && !typeWrapper.IsStatic)
            {
                modifierList.Add(SyntaxKind.AbstractKeyword);
            }

            if (typeWrapper.TypeKind != SymbolTypeKind.Interface && typeWrapper.IsStatic)
            {
                modifierList.Add(SyntaxKind.StaticKeyword);
            }

            if (typeWrapper.TypeKind != SymbolTypeKind.Interface && typeWrapper.TypeKind != SymbolTypeKind.Struct && typeWrapper.IsSealed && !typeWrapper.IsStatic && !typeWrapper.IsEnumType && !typeWrapper.IsDelegateType)
            {
                modifierList.Add(SyntaxKind.SealedKeyword);
            }

            return modifierList;
        }

        private static IReadOnlyCollection<SyntaxKind> AccessibilityToSyntaxKind(EntityAccessibility accessibility)
        {
            switch (accessibility)
            {
                case EntityAccessibility.Internal:
                    return new[] { SyntaxKind.InternalKeyword };
                case EntityAccessibility.Private:
                    return new[] { SyntaxKind.PrivateKeyword };
                case EntityAccessibility.PrivateProtected:
                    return new[] { SyntaxKind.ProtectedKeyword, SyntaxKind.PrivateKeyword };
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
