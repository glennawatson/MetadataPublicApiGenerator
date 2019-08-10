using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace MetadataPublicApiGenerator.IntegrationTestData
{
    //public static class GenericType
    //{
    //    public static T MyLife<T>(int helloWorld, T blah) => default;
    //}

    //public class GenericType<T> : IEnumerable<T>
    //{
    //    public IEnumerator<T> GetEnumerator()
    //    {
    //        throw new NotImplementedException();
    //    }

    //    IEnumerator IEnumerable.GetEnumerator()
    //    {
    //        throw new NotImplementedException();
    //    }
    //}

    public class GenericWithTypeInParams<T> : IEnumerable<IEnumerable<T>>
    {
        public int X, Y;

        /// <inheritdoc />
        public IEnumerator<IEnumerable<T>> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
