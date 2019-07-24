// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Generic;
using LightweightMetadata.TypeWrappers;
using MetadataPublicApiGenerator.Extensions;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using static MetadataPublicApiGenerator.Helpers.SyntaxFactoryHelpers;

namespace MetadataPublicApiGenerator.Generators.SymbolGenerators
{
    /// <summary>
    /// Generates parameters syntax.
    /// </summary>
    internal static class ParameterSymbolGenerator
    {
        public static ParameterSyntax Generate(IHandleWrapper nameWrapper, ISet<string> excludeMembersAttributes, ISet<string> excludeAttributes)
        {
            if (!(nameWrapper is ParameterWrapper parameterWrapper))
            {
                return null;
            }

            return Parameter(GeneratorFactory.Generate(parameterWrapper.Attributes, excludeMembersAttributes, excludeAttributes), parameterWrapper.GetModifiers(), parameterWrapper.ParameterType.GetTypeSyntax(), parameterWrapper.Name);
        }
    }
}
