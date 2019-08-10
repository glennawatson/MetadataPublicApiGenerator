using System;
using System.Collections.Generic;
using System.Linq;

namespace MetadataPublicApiGenerator.IntegrationTestData
{
    public class Parameters
    {
        private readonly int X, Y, Distance;

        public static void WithParamsStatic(string test1, int test2, params string[] test) => Console.WriteLine(test1, test2, test);

        public void WithParamsInstance(string text1, int int2, params string[] test) => Console.WriteLine(text1, int2, test);

        public void DefaultsWithParamsInstance(string noDefault, string defaultText = "test", int intDefault = 42, params int[] test) => Console.WriteLine(noDefault, defaultText, intDefault, test);

        public void NoParameters() { }

        public void SingleParameter(int testValue) { }

        public int WithGenericParameter(IEnumerable<int> test) => test.Count();

        public void WithGeneric<T>(T test) => Console.WriteLine(test);

        public void WithGenericClassConstraint<T>(T test) where T : class { }

        public void WithGenericNewConstraint<T>(T test) where T : new() { }

        public void WithGenericValueConstraint<T>(T test) where T : struct { }

        public void WithGenericComplexConstraint<T>(T test) where T : IEnumerable<T> { }

        public void WithGenericComplexClassNewConstraint<T>(T test) where T : class, IEnumerable<T>, new() { }

        public void WithGenericComplexClassMultipleTypes<TValue, TNext>(TValue test) where TValue : Tuple<int> where TNext : class { }

        public void WithNestedContraints<TValue>(TValue test) where TValue : IEnumerable<IEnumerable<IEnumerable<IEnumerable<Tuple<int>>>>> { }

        public void WithRefParameter(ref string test) { }

        public void WithOutParameter(out string value) { value = null; }

        public void WithGenericOutParameter(out IEnumerable<string> value) {  value = null; }

        public string DistanceString() =>
            $"({X}, {Y}) is {Distance} from the origin";
    }
}
