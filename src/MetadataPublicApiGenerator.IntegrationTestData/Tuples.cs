using System;

namespace MetadataPublicApiGenerator.IntegrationTestData
{
    public class Tuples
    {
        private static readonly int X = 1;
        private static readonly int Y = 2;

        public (int x, int y) Deconstruct(out int x, out int y) => (x, y) = (X, Y);

        public (int x, int y) PropertyTuple { get; } = (X, Y);

        public void TestTupleInput((int x, int y) input) => Console.WriteLine(input);
    }
}
