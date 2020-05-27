// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Generic;

using LightweightMetadata;
using MetadataPublicApiGenerator.Extensions;
using MetadataPublicApiGenerator.Helpers;

using Microsoft.CodeAnalysis.CSharp.Syntax;

using static MetadataPublicApiGenerator.Helpers.SyntaxFactoryHelpers;

namespace MetadataPublicApiGenerator.Generators.SymbolGenerators
{
    /// <summary>
    /// Generates parameters syntax.
    /// </summary>
    internal static class ParameterSymbolGenerator
    {
        public static ParameterSyntax? Generate(IHandleNameWrapper nameWrapper, ISet<string> excludeMembersAttributes, ISet<string> excludeAttributes, Nullability currentNullability, bool isExtensionMethod)
        {
            if (!(nameWrapper is ParameterWrapper parameterWrapper))
            {
                return null;
            }

            EqualsValueClauseSyntax? equals = null;
            if (parameterWrapper.HasDefaultValue)
            {
                var valueSyntax = SyntaxHelper.GetValueExpression(parameterWrapper.ParameterType, parameterWrapper.DefaultValue!);

                if (valueSyntax != null)
                {
                    equals = EqualsValueClause(valueSyntax);
                }
            }

            parameterWrapper.Attributes.TryGetNullable(out var nullability);

            var name = parameterWrapper.Name;
            var attributes = GeneratorFactory.Generate(parameterWrapper.Attributes, excludeMembersAttributes, excludeAttributes);
            var modifiers = parameterWrapper.GetModifiers(isExtensionMethod);
            var parameterType = parameterWrapper.ParameterType.GetTypeSyntax(nameWrapper, currentNullability, nullability, false);
            return Parameter(attributes, modifiers, parameterType, name, equals!);
        }
    }
}
