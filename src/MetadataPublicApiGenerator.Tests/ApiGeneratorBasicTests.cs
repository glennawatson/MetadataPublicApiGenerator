// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.IO;
using System.Reflection;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using PublicApiGenerator;

using ReactiveUI;

namespace MetadataPublicApiGenerator.Tests
{
    /// <summary>
    /// The API generator basic tests.
    /// </summary>
    [TestClass]
    public class ApiGeneratorBasicTests
    {
        /// <summary>
        /// Tests against a fixed version of a common library to make sure the contents are as expected.
        /// </summary>
        [TestMethod]
        public void RxUIGeneratesContent()
        {
            var value = MetadataApi.GeneratePublicApi(Assembly.GetAssembly(typeof(RxApp)));

            Assert.IsNotNull(value);
        }

        [TestMethod]
        public void CoreLibProducesContent()
        {
            var value = MetadataApi.GeneratePublicApi(Assembly.GetAssembly(typeof(string)));
            Assert.IsNotNull(value);
        }

        [TestMethod]
        public void TestEquals()
        {
            var assembly = Assembly.GetAssembly(typeof(RxApp));
            var oldApi = ApiGenerator.GeneratePublicApi(assembly);

            var newApi = MetadataApi.GeneratePublicApi(assembly);

            File.WriteAllText("old.txt", oldApi);
            File.WriteAllText("new.txt", newApi);

            TestHelpers.CheckEquals(newApi, oldApi, "new.txt", "old.txt");
        }
    }
}
