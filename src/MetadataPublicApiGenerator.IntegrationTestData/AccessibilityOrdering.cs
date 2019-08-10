// Copyright (c) 2019 Glenn Watson. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;

namespace MetadataPublicApiGenerator.IntegrationTestData
{
    public class AccessibilityOrderClass
    {
        public void TestPublic() => Console.WriteLine();

        protected void TestProtected() => Console.WriteLine();

        protected internal void TestProtectedInternal() => Console.WriteLine();

        protected private void TestProtectedPrivate() => Console.WriteLine();

        internal void TestInternal() => Console.WriteLine();

        private void TestPrivate() => Console.WriteLine();
    }

    public class NestedClasses
    {
        public class PublicNested
        {
            public void TestPublic() => Console.WriteLine();
        }

        internal class InternalNested
        {
            public void TestPublic() => Console.WriteLine();
        }

        protected private class PrivateProtectedNested
        {
            public void TestPublic() => Console.WriteLine();
        }

        protected internal class ProtectedInternalNested
        {
            public void TestPublic() => Console.WriteLine();
        }

        protected class ProtectedNested
        {
            public void TestPublic() => Console.WriteLine();
        }

        private class PrivateNested
        {
            public void TestPublic() => Console.WriteLine();
        }
    }

    public class MixedClass
    {
        public void TestPublic() => Console.WriteLine();

        protected void TestProtected() => Console.WriteLine();

        protected internal void TestProtectedInternal() => Console.WriteLine();

        protected private void TestProtectedPrivate() => Console.WriteLine();

        internal void TestInternal() => Console.WriteLine();

        private void TestPrivate() => Console.WriteLine();

        public int TestPropertyPublic { get; }
        internal int TestPropertyInternal { get; }
        protected internal int TestPropertyProtectedInternal { get; }
        protected private int TestPropertyProtectedPrivate { get; }
        protected int TestPropertyProtected { get; }
        private int TestPropertyPrivate { get; }

        public int TestFieldPublic;
        internal int TestFieldInternal;
        protected internal int TestFieldProtectedInternal;
        protected private int TestFieldProtectedPrivate;
        protected int TestFieldProtected;
        private int TestFieldPrivate;

        public class PublicNestedClass
        {
            public void TestPublic() => Console.WriteLine();
        }

        internal class InternalNestedClass
        {
            public void TestPublic() => Console.WriteLine();
        }

        protected private class PrivateProtectedNestedClass
        {
            public void TestPublic() => Console.WriteLine();
        }

        protected internal class ProtectedInternalNestedClass
        {
            public void TestPublic() => Console.WriteLine();
        }

        protected class ProtectedNestedClass
        {
            public void TestPublic() => Console.WriteLine();
        }

        private class PrivateNestedClass
        {
            public void TestPublic() => Console.WriteLine();
        }

        public struct PublicNestedStruct
        {
            public void TestPublic() => Console.WriteLine();
        }

        internal struct InternalNestedStruct
        {
            public void TestPublic() => Console.WriteLine();
        }

        protected private struct PrivateProtectedNestedStruct
        {
            public void TestPublic() => Console.WriteLine();
        }

        protected internal struct ProtectedInternalNestedStruct
        {
            public void TestPublic() => Console.WriteLine();
        }

        protected struct ProtectedNestedStruct
        {
            public void TestPublic() => Console.WriteLine();
        }

        private struct PrivateNestedStruct
        {
            public void TestPublic() => Console.WriteLine();
        }
    }
}
