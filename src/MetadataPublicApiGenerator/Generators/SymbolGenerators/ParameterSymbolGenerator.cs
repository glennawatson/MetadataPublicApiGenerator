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
        public static ParameterSyntax Generate(IHandleWrapper nameWrapper, ISet<string> excludeMembersAttributes, ISet<string> excludeAttributes, bool isExtensionMethod)
        {
            if (!(nameWrapper is ParameterWrapper parameterWrapper))
            {
                return null;
            }

            EqualsValueClauseSyntax equals = null;
            if (parameterWrapper.HasDefaultValue)
            {
                equals = EqualsValueClause(SyntaxHelper.GetValueExpression(parameterWrapper.ParameterType, parameterWrapper.DefaultValue));
            }

            return Parameter(GeneratorFactory.Generate(parameterWrapper.Attributes, excludeMembersAttributes, excludeAttributes), parameterWrapper.GetModifiers(isExtensionMethod), parameterWrapper.ParameterType.GetTypeSyntax(false), parameterWrapper.Name, equals);
        }
    }
}
