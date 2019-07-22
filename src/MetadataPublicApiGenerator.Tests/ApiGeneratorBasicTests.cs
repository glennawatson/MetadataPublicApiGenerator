// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reflection;
using MetadataPublicApiGenerator.IntegrationTestData;
using PublicApiGenerator;
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
        public void GeneratesContent()
        {
            var value = MetadataApi.GeneratePublicApi(Assembly.GetAssembly(typeof(string)));

            Assert.NotNull(value);
        }

        /// <summary>
        /// Make sure that the content equal the current metadata and the older one.
        /// </summary>
        [Fact]
        public void ContentEquals()
        {
            var assembly = Assembly.GetAssembly(typeof(string));
            var metadata = MetadataApi.GeneratePublicApi(assembly);
            var cecil = ApiGenerator.GeneratePublicApi(assembly);

            Assert.Equal(metadata, cecil);
        }
    }
}
