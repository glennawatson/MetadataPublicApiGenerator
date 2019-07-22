using System;
using System.Collections.Generic;
using System.Text;

namespace MetadataPublicApiGenerator.IntegrationTestData
{
    public static class GenericType
    {
        public static T MyLife<T>(int helloWorld, T blah) => default;
    }
}
