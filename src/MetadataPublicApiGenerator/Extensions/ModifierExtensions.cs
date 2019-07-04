// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using MetadataPublicApiGenerator.Compilation;
using MetadataPublicApiGenerator.Compilation.TypeWrappers;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace MetadataPublicApiGenerator.Extensions
{
    /// <summary>
    /// Extension methods that produce modifiers.
    /// </summary>
    internal static class ModifierExtensions
    {
        public static SyntaxTokenList GetModifiers(this TypeDefinition typeDefinition)
        {
            var modifierList = new List<SyntaxKind>();

            if ((typeDefinition.Attributes & TypeAttributes.Public) != 0)
            {
                modifierList.Add(SyntaxKind.PublicKeyword);
            }

            if ((typeDefinition.Attributes & TypeAttributes.Abstract) != 0)
            {
                modifierList.Add(SyntaxKind.AbstractKeyword);
            }

            if ((typeDefinition.Attributes & (TypeAttributes.Abstract | TypeAttributes.Sealed)) == (TypeAttributes.Abstract | TypeAttributes.Sealed))
            {
                modifierList.Add(SyntaxKind.StaticKeyword);
            }

            if ((typeDefinition.Attributes & TypeAttributes.Sealed) != 0)
            {
                modifierList.Add(SyntaxKind.SealedKeyword);
            }

            return SyntaxFactory.TokenList(modifierList.Select(SyntaxFactory.Token));
        }

        public static SyntaxTokenList GetModifiers(this MethodWrapper method)
        {
            var modifierList = new List<SyntaxKind>();

            if (method.IsPublic)
            {
                modifierList.Add(SyntaxKind.PublicKeyword);
            }

            if (method.IsAbstract)
            {
                modifierList.Add(SyntaxKind.AbstractKeyword);
            }

            if (method.IsStatic)
            {
                modifierList.Add(SyntaxKind.StaticKeyword);
            }

            if (method.IsSealed)
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

            return SyntaxFactory.TokenList(modifierList.Select(SyntaxFactory.Token));
        }

        public static SyntaxTokenList GetModifiers(this PropertyDefinition property, CompilationModule module)
        {
            var modifierList = new List<SyntaxKind>();

            var accessors = property.GetAccessors();

            bool isPublicSet = false;

            if (!accessors.Getter.IsNil)
            {
                if ((accessors.Getter.Resolve(module).Attributes & MethodAttributes.Public) != 0)
                {
                    modifierList.Add(SyntaxKind.PublicKeyword);
                    isPublicSet = true;
                }
            }

            if (!isPublicSet && !accessors.Setter.IsNil)
            {
                if ((accessors.Setter.Resolve(module).Attributes & MethodAttributes.Public) != 0)
                {
                    modifierList.Add(SyntaxKind.PublicKeyword);
                }
            }

            var anyGetter = accessors.Getter.IsNil ? accessors.Setter : accessors.Getter;

            if (anyGetter.IsNil)
            {
                return SyntaxFactory.TokenList(modifierList.Select(SyntaxFactory.Token));
            }

            var method = anyGetter.Resolve(module);

            if ((method.Attributes & MethodAttributes.Abstract) != 0)
            {
                modifierList.Add(SyntaxKind.AbstractKeyword);
            }

            if ((method.Attributes & MethodAttributes.Static) != 0)
            {
                modifierList.Add(SyntaxKind.StaticKeyword);
            }

            if ((method.Attributes & (MethodAttributes.Abstract | MethodAttributes.Final | MethodAttributes.NewSlot | MethodAttributes.Static)) == MethodAttributes.Final)
            {
                modifierList.Add(SyntaxKind.SealedKeyword);
            }

            if ((method.Attributes & (MethodAttributes.NewSlot | MethodAttributes.Virtual)) == MethodAttributes.Virtual)
            {
                modifierList.Add(SyntaxKind.OverrideKeyword);
            }

            if ((method.Attributes & (MethodAttributes.Abstract | MethodAttributes.Virtual | MethodAttributes.NewSlot | MethodAttributes.Final)) == (MethodAttributes.Virtual | MethodAttributes.NewSlot))
            {
                modifierList.Add(SyntaxKind.VirtualKeyword);
            }

            return SyntaxFactory.TokenList(modifierList.Select(SyntaxFactory.Token));
        }

        public static SyntaxTokenList GetModifiers(this FieldDefinition field)
        {
            var modifierList = new List<SyntaxKind>();

            if ((field.Attributes & FieldAttributes.Public) != 0)
            {
                modifierList.Add(SyntaxKind.PublicKeyword);
            }

            if ((field.Attributes & FieldAttributes.Static) != 0)
            {
                modifierList.Add(SyntaxKind.StaticKeyword);
            }

            return SyntaxFactory.TokenList(modifierList.Select(SyntaxFactory.Token));
        }

        public static SyntaxTokenList GetModifiers(this EventDefinition eventDefinition, CompilationModule module)
        {
            var modifierList = new List<SyntaxKind>();

            var method = eventDefinition.GetAnyAccessor().Resolve(module);

            if ((method.Attributes & MethodAttributes.Public) != 0)
            {
                modifierList.Add(SyntaxKind.PublicKeyword);
            }

            if ((method.Attributes & MethodAttributes.Abstract) != 0)
            {
                modifierList.Add(SyntaxKind.AbstractKeyword);
            }

            if ((method.Attributes & MethodAttributes.Static) != 0)
            {
                modifierList.Add(SyntaxKind.StaticKeyword);
            }

            if ((method.Attributes & (MethodAttributes.Abstract | MethodAttributes.Final | MethodAttributes.NewSlot | MethodAttributes.Static)) == MethodAttributes.Final)
            {
                modifierList.Add(SyntaxKind.SealedKeyword);
            }

            if ((method.Attributes & (MethodAttributes.NewSlot | MethodAttributes.Virtual)) == MethodAttributes.Virtual)
            {
                modifierList.Add(SyntaxKind.OverrideKeyword);
            }

            if ((method.Attributes & (MethodAttributes.Abstract | MethodAttributes.Virtual | MethodAttributes.NewSlot | MethodAttributes.Final)) == (MethodAttributes.Virtual | MethodAttributes.NewSlot))
            {
                modifierList.Add(SyntaxKind.VirtualKeyword);
            }

            return SyntaxFactory.TokenList(modifierList.Select(SyntaxFactory.Token));
        }

        public static SyntaxTokenList GetModifiers(this Parameter parameter, CompilationModule module)
        {
            var modifierList = new List<SyntaxKind>();
            if ((parameter.Attributes & ParameterAttributes.In) != 0)
            {
                modifierList.Add(SyntaxKind.InKeyword);
            }

            if ((parameter.Attributes & ParameterAttributes.Out) != 0)
            {
                modifierList.Add(SyntaxKind.OutKeyword);
            }

            if (parameter.GetCustomAttributes().HasKnownAttribute(module, Compilation.KnownAttribute.ParamArray))
            {
                modifierList.Add(SyntaxKind.ParamsKeyword);
            }

            return SyntaxFactory.TokenList(modifierList.Select(SyntaxFactory.Token));
        }
    }
}
