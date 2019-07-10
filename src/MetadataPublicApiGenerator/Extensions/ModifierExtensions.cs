// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

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

            if (typeDefinition.IsPublic)
            {
                modifierList.Add(SyntaxKind.PublicKeyword);
            }

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

            bool isPublicSet = false;

            if (property.Getter != null)
            {
                if (property.Getter.IsPublic)
                {
                    modifierList.Add(SyntaxKind.PublicKeyword);
                    isPublicSet = true;
                }
            }

            if (!isPublicSet && property.Setter != null)
            {
                if (property.Setter.IsPublic)
                {
                    modifierList.Add(SyntaxKind.PublicKeyword);
                }
            }

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

        public static SyntaxTokenList GetModifiers(this FieldWrapper field)
        {
            var modifierList = new List<SyntaxKind>();

            if (field.IsPublic)
            {
                modifierList.Add(SyntaxKind.PublicKeyword);
            }

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

            if (method.IsPublic)
            {
                modifierList.Add(SyntaxKind.PublicKeyword);
            }

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

            return modifierList;
        }
    }
}
