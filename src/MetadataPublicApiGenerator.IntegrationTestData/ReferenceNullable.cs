using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace MetadataPublicApiGenerator.IntegrationTestData
{
    public class ReferenceNullable
    {
        public string? s;
        public Dictionary<string, object?>? d;
        public int[]? b;
        public object?[]? c;

        public string? SProp { get; }
        public Dictionary<string, object?>? DProp { get; }
        public int[]? BProp { get; }
        public object?[]? CProp { get; }

        public void NullableForgive2(string? test, string nonNullable, string? test2) { }

        public void NullableForgive(string? test) { }

        // Return value is non-null, and elements aren't null
        public List<string> GetNames1() { return new List<string>(); }

        // Return value is non-null, but elements may be null
        public List<string?> GetNames2() { return new List<string?>(); }

        // Return value may be null, but elements aren't null
        public List<string>? GetNames3() { return new List<string>(); }

        // Return value may be null, and elements may be null
        public List<string?>? GetNames4() { return new List<string?>(); }

        public class DerivedNullableInterface : IEnumerable<MyBase?>
        {
            public IEnumerator<MyBase?> GetEnumerator()
            {
                throw new NotImplementedException();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                throw new NotImplementedException();
            }
        }

        public class DerivedNullableInterface1 : IEnumerable<IEnumerable<string?>?>
        {
            public IEnumerator<IEnumerable<string?>?> GetEnumerator()
            {
                throw new NotImplementedException();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                throw new NotImplementedException();
            }
        }

        public class DerivedNullableInterface2 : IEnumerable<IEnumerable<string?>>
        {
            public IEnumerator<IEnumerable<string?>> GetEnumerator()
            {
                throw new NotImplementedException();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                throw new NotImplementedException();
            }
        }

        public class DerivedNullableInterface3 : IEnumerable<IEnumerable<string>?>
        {
            public IEnumerator<IEnumerable<string>?> GetEnumerator()
            {
                throw new NotImplementedException();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                throw new NotImplementedException();
            }
        }

        public abstract class MyBase<T> { }

        public class MyBase { }

        public class DerivedNullableBase1 : MyBase<string?>
        {
        }

        public class DerivedNullableBase2 : MyBase<MyBase<string?>?>
        {
        }

        public class DerivedNullableBase3 : MyBase<MyBase<string?>>
        {
        }

        public class DerivedNullableBase4 : MyBase<MyBase<string>?>
        {
        }

        public class DerivedNullableBase5 : MyBase<MyBase<MyBase<string?>?>?>
        {
        }

        public class DerivedNullableBase6 : MyBase<MyBase<MyBase<string?>>>
        {
        }
    }
}
