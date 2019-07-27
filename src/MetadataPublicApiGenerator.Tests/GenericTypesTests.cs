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
            var code = @"
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
namespace MyNamespace
{
    public class GenericType<T> : IEnumerable<T>
    {
        public IEnumerator<T> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}";

            var expectedApi =
@"namespace MyNamespace
{
    public class GenericType<T> : System.Collections.Generic.IEnumerable<T>, System.Collections.IEnumerable
    {
        public GenericType();
        public System.Collections.Generic.IEnumerator<T> GetEnumerator();
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator();
    }
}";

            RoslynTestHelper.CheckApi(code, expectedApi);
        }
    }
}
