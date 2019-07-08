// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;

using MetadataPublicApiGenerator.Compilation.TypeWrappers;
using MetadataPublicApiGenerator.Helpers;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MetadataPublicApiGenerator.Extensions
{
    internal static class SyntaxExtensions
    {
        private static readonly Dictionary<HandleKind, int> _symbolKindPreferredOrderWeights = new Dictionary<HandleKind, int>
        {
            [HandleKind.ModuleDefinition] = 0,
            [HandleKind.TypeReference] = 2,
            [HandleKind.TypeDefinition] = 3,
            [HandleKind.FieldDefinition] = 7,
            [HandleKind.MethodDefinition] = 8,
            [HandleKind.Parameter] = 9,
            [HandleKind.InterfaceImplementation] = 10,
            [HandleKind.MemberReference] = 11,
            [HandleKind.Constant] = 12,
            [HandleKind.CustomAttribute] = 13,
            [HandleKind.DeclarativeSecurityAttribute] = 14,
            [HandleKind.StandaloneSignature] = 15,
            [HandleKind.EventDefinition] = 5,
            [HandleKind.PropertyDefinition] = 6,
            [HandleKind.MethodImplementation] = 16,
            [HandleKind.ModuleReference] = 17,
            [HandleKind.TypeSpecification] = 18,
            [HandleKind.AssemblyDefinition] = 1,
            [HandleKind.AssemblyReference] = 19,
            [HandleKind.AssemblyFile] = 20,
            [HandleKind.ExportedType] = 21,
            [HandleKind.ManifestResource] = 22,
            [HandleKind.GenericParameter] = 23,
            [HandleKind.MethodSpecification] = 24,
            [HandleKind.GenericParameterConstraint] = 25,
            [HandleKind.Document] = 26,
            [HandleKind.MethodDebugInformation] = 27,
            [HandleKind.LocalScope] = 28,
            [HandleKind.LocalVariable] = 29,
            [HandleKind.LocalConstant] = 30,
            [HandleKind.ImportScope] = 31,
            [HandleKind.CustomDebugInformation] = 32,
            [HandleKind.UserString] = 33,
            [HandleKind.Blob] = 34,
            [HandleKind.Guid] = 35,
            [HandleKind.String] = 36,
            [HandleKind.NamespaceDefinition] = 4,
        };

        /// <summary>
        /// Generate a attribute list individually for a single attribute.
        /// </summary>
        /// <param name="attribute">The attribute to generate the attribute list for.</param>
        /// <returns>The attribute list syntax containing the single attribute.</returns>
        public static AttributeListSyntax GenerateAttributeList(this AttributeWrapper attribute)
        {
            return SyntaxFactory.AttributeList(SyntaxFactory.SeparatedList(new[] { GenerateAttributeSyntax(attribute) }));
        }

        /// <summary>
        /// Generates the attribute syntax for a specified attribute.
        /// </summary>
        /// <param name="customAttribute">The attribute to generate the AttributeSyntax for.</param>
        /// <returns>The attribute syntax for the single attribute.</returns>
        public static AttributeSyntax GenerateAttributeSyntax(this AttributeWrapper customAttribute)
        {
            var arguments = new List<AttributeArgumentSyntax>();

            foreach (var fixedArgument in customAttribute.FixedArguments)
            {
                arguments.Add(SyntaxFactory.AttributeArgument(SyntaxHelper.LiteralParameterFromType(fixedArgument.Type, fixedArgument.Value)));
            }

            foreach (var namedArgument in customAttribute.NamedArguments)
            {
                arguments.Add(SyntaxFactory.AttributeArgument(SyntaxHelper.LiteralParameterFromType(namedArgument.Type, namedArgument.Value)).WithNameEquals(SyntaxFactory.NameEquals(SyntaxFactory.IdentifierName(namedArgument.Name))));
            }

            var attributeName = SyntaxFactory.IdentifierName(customAttribute.FullName);
            var attribute = SyntaxFactory.Attribute(attributeName);

            if (arguments.Count > 0)
            {
                attribute = attribute.WithArgumentList(SyntaxFactory.AttributeArgumentList(SyntaxFactory.SeparatedList(arguments)));
            }

            return attribute;
        }

        internal static bool ShouldIncludeEntity(this IHasAttributes entity, ISet<string> excludeMembersAttributes)
        {
            if (entity is IHandleTypeNamedWrapper typeNameWrapper && !typeNameWrapper.IsPublic)
            {
                return false;
            }

            var attributes = entity.Attributes;

            if (attributes == null)
            {
                return true;
            }

            return !attributes.Any(attr => excludeMembersAttributes.Contains(attr.FullName));
        }

        internal static IEnumerable<T> OrderByAndExclude<T>(this IEnumerable<T> entities, ISet<string> excludeMembersAttributes)
            where T : IHasAttributes
        {
            return entities.Where(x => ShouldIncludeEntity(x, excludeMembersAttributes)).OrderBy(x => _symbolKindPreferredOrderWeights[x.Handle.Kind]).ThenBy(x => x.Name);
        }
    }
}
