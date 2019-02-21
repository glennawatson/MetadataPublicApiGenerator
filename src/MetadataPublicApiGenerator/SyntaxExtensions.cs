// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.Decompiler.TypeSystem;
using ICSharpCode.Decompiler.TypeSystem.Implementation;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Accessibility = ICSharpCode.Decompiler.TypeSystem.Accessibility;

namespace MetadataPublicApiGenerator
{
    internal static class SyntaxExtensions
    {
        /// <summary>
        /// Generate a attribute list individually for a single attribute.
        /// </summary>
        /// <param name="attribute">The attribute to generate the attribute list for.</param>
        /// <param name="compilation">The compilation unit for details about types.</param>
        /// <returns>The attribute list syntax containing the single attribute.</returns>
        public static AttributeListSyntax GenerateAttributeList(this IAttribute attribute, ICompilation compilation)
        {
            return SyntaxFactory.AttributeList(SyntaxFactory.SeparatedList(new[] { GenerateAttributeSyntax(attribute, compilation) }));
        }

        /// <summary>
        /// Generates the attribute syntax for a specified attribute.
        /// </summary>
        /// <param name="customAttribute">The attribute to generate the AttributeSyntax for.</param>
        /// <param name="compilation">The compilation unit for details about types.</param>
        /// <returns>The attribute syntax for the single attribute.</returns>
        public static AttributeSyntax GenerateAttributeSyntax(this IAttribute customAttribute, ICompilation compilation)
        {
            var arguments = new List<AttributeArgumentSyntax>();

            foreach (var fixedArgument in customAttribute.FixedArguments)
            {
                arguments.Add(SyntaxFactory.AttributeArgument(SyntaxHelper.LiteralParameterFromType(compilation, fixedArgument.Type, fixedArgument.Value)));
            }

            foreach (var namedArgument in customAttribute.NamedArguments)
            {
                arguments.Add(SyntaxFactory.AttributeArgument(SyntaxHelper.LiteralParameterFromType(compilation, namedArgument.Type, namedArgument.Value)).WithNameEquals(SyntaxFactory.NameEquals(SyntaxFactory.IdentifierName(namedArgument.Name))));
            }

            return SyntaxFactory.Attribute(SyntaxFactory.IdentifierName(customAttribute.AttributeType.FullName)).WithArgumentList(SyntaxFactory.AttributeArgumentList(SyntaxFactory.SeparatedList(arguments)));
        }

        /// <summary>
        /// Gets a string form of the type and generic arguments for a type.
        /// </summary>
        /// <param name="currentType">The type to generate the arguments for.</param>
        /// <returns>A type descriptor including the generic arguments.</returns>
        public static string GenerateFullGenericName(this IType currentType)
        {
            var sb = new StringBuilder(currentType.FullName);

            if (currentType.TypeParameterCount > 0)
            {
                sb.Append("<")
                    .Append(string.Join(", ", currentType.TypeArguments.Select(GenerateFullGenericName)))
                    .Append(">");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Sometimes types can be returned as <see cref="UnknownType" />, this will use the Referenced Assemblies to find the real type.
        /// </summary>
        /// <param name="type">The type we want to make sure is valid.</param>
        /// <param name="compilation">The compilation unit.</param>
        /// <returns>The type found.</returns>
        public static IType GetRealType(this IType type, ICompilation compilation)
        {
            if (type is UnknownType)
            {
                type = compilation.GetReferenceTypeDefinitionsWithFullName(type.FullName).FirstOrDefault();
            }

            return type;
        }

        public static SyntaxTokenList GetModifiers(this IEntity entity)
        {
            var modifierList = new List<SyntaxKind>();

            if (entity.Accessibility == Accessibility.Public)
            {
                modifierList.Add(SyntaxKind.PublicKeyword);
            }

            if (entity.IsAbstract)
            {
                modifierList.Add(SyntaxKind.AbstractKeyword);
            }

            if (entity.IsStatic)
            {
                modifierList.Add(SyntaxKind.StaticKeyword);
            }

            if (entity.IsSealed)
            {
                modifierList.Add(SyntaxKind.SealedKeyword);
            }

            return SyntaxFactory.TokenList(modifierList.Select(SyntaxFactory.Token));
        }

        public static SyntaxTokenList GetModifiers(this ITypeDefinition typeDefinition)
        {
            var syntaxList = GetModifiers((IEntity)typeDefinition);

            if (typeDefinition.IsReadOnly)
            {
                syntaxList = syntaxList.Add(SyntaxFactory.Token(SyntaxKind.ReadOnlyKeyword));
            }

            if (typeDefinition.IsByRefLike)
            {
                syntaxList = syntaxList.Add(SyntaxFactory.Token(SyntaxKind.RefKeyword));
            }

            return syntaxList;
        }

        public static SyntaxTokenList GetModifiers(this IMethod method)
        {
            var syntaxList = GetModifiers((IEntity)method);

            if (method.IsOverride)
            {
                syntaxList = syntaxList.Add(SyntaxFactory.Token(SyntaxKind.OverrideKeyword));
            }

            if (method.IsVirtual)
            {
                syntaxList = syntaxList.Add(SyntaxFactory.Token(SyntaxKind.VirtualKeyword));
            }

            return syntaxList;
        }

        public static SyntaxTokenList GetModifiers(this IVariable variable)
        {
            var modifierList = new List<SyntaxKind>();

            if (variable.IsConst)
            {
                modifierList.Add(SyntaxKind.ConstKeyword);
            }

            return SyntaxFactory.TokenList(modifierList.Select(SyntaxFactory.Token));
        }

        public static SyntaxTokenList GetModifiers(this IParameter parameter)
        {
            var syntaxList = GetModifiers((IVariable)parameter);

            if (parameter.IsIn)
            {
                syntaxList = syntaxList.Add(SyntaxFactory.Token(SyntaxKind.InKeyword));
            }

            if (parameter.IsOut)
            {
                syntaxList = syntaxList.Add(SyntaxFactory.Token(SyntaxKind.OutKeyword));
            }

            if (parameter.IsParams)
            {
                syntaxList = syntaxList.Add(SyntaxFactory.Token(SyntaxKind.ParamsKeyword));
            }

            if (parameter.IsRef)
            {
                syntaxList = syntaxList.Add(SyntaxFactory.Token(SyntaxKind.RefKeyword));
            }

            return syntaxList;
        }
    }
}
