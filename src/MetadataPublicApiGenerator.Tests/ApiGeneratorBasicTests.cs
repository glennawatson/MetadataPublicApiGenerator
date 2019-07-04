// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reflection;

using Xunit;

namespace MetadataPublicApiGenerator.Tests
{
    /// <summary>
    /// The API generator basic tests.
    /// </summary>
    public class ApiGeneratorBasicTests
    {
        /// <summary>
        /// Tests against a fixed version of a common library to make sure the contents are as expected.
        /// </summary>
        [Fact]
        public void FirstTest()
        {
            var value = ApiGenerator.GeneratePublicApi(Assembly.GetAssembly(typeof(string)));

            Assert.NotNull(value);
        }
    }
}
