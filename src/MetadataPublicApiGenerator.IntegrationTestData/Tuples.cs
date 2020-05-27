using System;
using System.Linq.Expressions;

namespace MetadataPublicApiGenerator.IntegrationTestData
{
    public interface Test<TViewModel, TView, TOutput>
    {
    }

    public class Tuples
    {
        private static readonly int X = 1;
        private static readonly int Y = 2;

        public (int x, int y) Deconstruct(out int x, out int y) => (x, y) = (X, Y);

        public (int x, int y) PropertyTuple { get; } = (X, Y);

        public void TestTupleInput((int x, int y) input) => Console.WriteLine(input);

        public Test<TView, TViewModel, (object view, bool isViewModel)> Bind<TViewModel, TView, TVMProp, TVProp, TDontCare>(
            TViewModel viewModel,
            TView view,
            Expression<Func<TViewModel, TVMProp>> vmProperty,
            Expression<Func<TView, TVProp>> viewProperty,
            IObservable<TDontCare> signalViewUpdate,
            object conversionHint)
            where TViewModel : class
            where TView : class
        {
            return default;
        }
    }
}
