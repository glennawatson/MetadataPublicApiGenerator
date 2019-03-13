// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text;
using MetadataPublicApiGenerator.Compilation;
using MetadataPublicApiGenerator.Helpers;
using Microsoft.CodeAnalysis;
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
        /// <param name="compilation">The compilation unit for details about types.</param>
        /// <returns>The attribute list syntax containing the single attribute.</returns>
        public static AttributeListSyntax GenerateAttributeList(this CustomAttribute attribute, CompilationModule compilation)
        {
            return SyntaxFactory.AttributeList(SyntaxFactory.SeparatedList(new[] { GenerateAttributeSyntax(attribute, compilation) }));
        }

        public static TypeKind GetTypeKind(this TypeDefinitionHandle typeDefinitionHandle, CompilationModule module)
        {
            return GetTypeKind(typeDefinitionHandle.Resolve(module), module);
        }

        public static TypeKind GetTypeKind(this TypeDefinition typeDefinition, CompilationModule module)
        {
            var attributes = typeDefinition.Attributes;

            if ((attributes & TypeAttributes.ClassSemanticsMask) == TypeAttributes.Interface)
            {
                return TypeKind.Interface;
            }

            if (typeDefinition.IsEnum(module))
            {
                return TypeKind.Enum;
            }

            var knownType = typeDefinition.IsKnownType(module);

            if (typeDefinition.IsValueType(module))
            {
                return TypeKind.Struct;
            }

            if (typeDefinition.IsDelegate(module))
            {
                return TypeKind.Delegate;
            }

            return TypeKind.Class;
        }

        /// <summary>
        /// Generate a attribute list individually for a single attribute.
        /// </summary>
        /// <param name="attribute">The attribute to generate the attribute list for.</param>
        /// <param name="compilation">The compilation unit for details about types.</param>
        /// <returns>The attribute list syntax containing the single attribute.</returns>
        public static AttributeListSyntax GenerateAttributeList(this CustomAttributeHandle attribute, CompilationModule compilation)
        {
            return GenerateAttributeList(attribute.Resolve(compilation), compilation);
        }

        /// <summary>
        /// Generates the attribute syntax for a specified attribute.
        /// </summary>
        /// <param name="customAttributeHandle">The attribute to generate the AttributeSyntax for.</param>
        /// <param name="compilation">The compilation unit for details about types.</param>
        /// <returns>The attribute syntax for the single attribute.</returns>
        public static AttributeSyntax GenerateAttributeSyntax(this CustomAttributeHandle customAttributeHandle, CompilationModule compilation)
        {
            var customAttribute = customAttributeHandle.Resolve(compilation);

            return GenerateAttributeSyntax(customAttribute, compilation);
        }

        /// <summary>
        /// Generates the attribute syntax for a specified attribute.
        /// </summary>
        /// <param name="customAttribute">The attribute to generate the AttributeSyntax for.</param>
        /// <param name="compilation">The compilation unit for details about types.</param>
        /// <returns>The attribute syntax for the single attribute.</returns>
        public static AttributeSyntax GenerateAttributeSyntax(this CustomAttribute customAttribute, CompilationModule compilation)
        {
            var arguments = new List<AttributeArgumentSyntax>();

            var wrapper = customAttribute.DecodeValue(compilation.TypeProvider);

            foreach (var fixedArgument in wrapper.FixedArguments)
            {
                arguments.Add(SyntaxFactory.AttributeArgument(SyntaxHelper.LiteralParameterFromType(fixedArgument.Type.Module, fixedArgument.Type, fixedArgument.Value)));
            }

            foreach (var namedArgument in wrapper.NamedArguments)
            {
                arguments.Add(SyntaxFactory.AttributeArgument(SyntaxHelper.LiteralParameterFromType(namedArgument.Type.Module, namedArgument.Type, namedArgument.Value)).WithNameEquals(SyntaxFactory.NameEquals(SyntaxFactory.IdentifierName(namedArgument.Name))));
            }

            return SyntaxFactory.Attribute(SyntaxFactory.IdentifierName(customAttribute.GetFullName(compilation))).WithArgumentList(SyntaxFactory.AttributeArgumentList(SyntaxFactory.SeparatedList(arguments)));
        }

        internal static bool ShouldIncludeEntity(this Handle entity, ISet<string> excludeMembersAttributes, CompilationModule module)
        {
            var isPublic = entity.IsEntityPublic(module);

            if (!isPublic)
            {
                return false;
            }

            var attributes = entity.GetEntityCustomAttributes(module);

            if (attributes == null)
            {
                return true;
            }

            return !attributes.Value.Any(attr => excludeMembersAttributes.Contains(attr.GetName(module)));
        }

        internal static IEnumerable<Handle> OrderByAndExclude(this IEnumerable<Handle> entities, ISet<string> excludeMembersAttributes, CompilationModule module)
        {
            return entities.Where(x => ShouldIncludeEntity(x, excludeMembersAttributes, module)).OrderBy(x => _symbolKindPreferredOrderWeights[x.Kind]).ThenBy(x => x.GetName(module));
        }
    }
}
