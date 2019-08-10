namespace MetadataPublicApiGenerator.IntegrationTestData
{
    public class ClassWithNestedClass
    {
        public class NestedClass
        {
            public void Method() { }
        }

        public void Method() { }
    }

    public class ClassWithPrivateNestedClass
    {
        private class NestedClass
        {
            public void Method() { }
        }

        public void Method() { }
    }

    public class ClassWithProtectedNestedClass
    {
        protected class NestedClass
        {
            public void Method() { }
        }

        public void Method() { }
    }

    public class ClassWithDeeplyNestedClasses
    {
        public class ClassWithOneNestedClass
        {
            public class InnerNestedClass
            {
                public void Method1() { }
            }

            public void Method2() { }
        }

        public void Method3() { }
    }

    public class ClassWithDeeplyNestedStructs
    {
        public struct StructWithOneNestedStruct
        {
            public struct InnerNestedStruct
            {
                public void Method1() { }
            }

            public void Method2() { }
        }

        public class ClassNestedAlongsideStruct
        {
            public void Method4() { }
        }

        public void Method3() { }
    }

    public struct StructWithNestedStruct
    {
        public struct NestedStruct
        {
            public void Method() { }
        }

        public void Method() { }
    }
}