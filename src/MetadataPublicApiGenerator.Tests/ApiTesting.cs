// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.VisualStudio.TestTools.UnitTesting;

[assembly: Parallelize(Scope = ExecutionScope.MethodLevel)]

namespace MetadataPublicApiGenerator.Tests
{
    /// <summary>
    /// Tests various API portions of the application.
    /// </summary>
    [TestClass]
    public class ApiTesting
    {
        [TestMethod]
        public void GenericType() => RoslynTestHelper.CheckApi();

        [TestMethod]
        public void ArrayTypes() => RoslynTestHelper.CheckApi();

        [TestMethod]
        public void Unsafe() => RoslynTestHelper.CheckApi();

        [TestMethod]
        public void NestedTypes() => RoslynTestHelper.CheckApi();

        [TestMethod]
        public void NamespaceOrdering() => RoslynTestHelper.CheckApi();

        [TestMethod]
        public void NamespaceMemberAccessibility() => RoslynTestHelper.CheckApi();

        [TestMethod]
        public void AccessibilityOrdering() => RoslynTestHelper.CheckApi();

        [TestMethod]
        public void Tuples() => RoslynTestHelper.CheckApi();

        [TestMethod]
        public void Parameters() => RoslynTestHelper.CheckApi();

        [TestMethod]
        public void NullableContext() => RoslynTestHelper.CheckApi();

        [TestMethod]
        public void ReferenceNullable() => RoslynTestHelper.CheckApi();
    }
}
