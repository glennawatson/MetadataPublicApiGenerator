// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace MetadataPublicApiGenerator.Generators
{
    /// <summary>
    /// A base class for all generators.
    /// </summary>
    internal abstract class GeneratorBase : IGenerator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GeneratorBase"/> class.
        /// </summary>
        /// <param name="excludeAttributes">A set of attributes to exclude from being generated.</param>
        /// <param name="excludeMembersAttributes">A set of attributes for any types we should avoid that are decorated with these attribute types.</param>
        /// <param name="factory">The factory for generating children.</param>
        protected GeneratorBase(ISet<string> excludeAttributes, ISet<string> excludeMembersAttributes, IGeneratorFactory factory)
        {
            ExcludeAttributes = excludeAttributes;
            ExcludeMembersAttributes = excludeMembersAttributes;
            Factory = factory;
        }

        /// <inheritdoc />
        public ISet<string> ExcludeAttributes { get; }

        /// <inheritdoc />
        public ISet<string> ExcludeMembersAttributes { get; }

        /// <summary>
        /// Gets the factory.
        /// </summary>
        internal IGeneratorFactory Factory { get; }
    }
}
