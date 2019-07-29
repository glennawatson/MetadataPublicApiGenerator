// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Xunit;

namespace MetadataPublicApiGenerator.Tests
{
    /// <summary>
    /// Checks to make sure generic types generate correctly.
    /// </summary>
    public class GenericTypesTests
    {
        /// <summary>
        /// Checks to make sure that the inheritance of a class is correct.
        /// </summary>
        [Fact]
        public void GenericInheritanceCorrect()
        {
            RoslynTestHelper.CheckApi("GenericType");
        }
    }
}
