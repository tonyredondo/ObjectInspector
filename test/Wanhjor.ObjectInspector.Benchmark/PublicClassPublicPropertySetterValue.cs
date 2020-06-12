using BenchmarkDotNet.Attributes;

namespace Wanhjor.ObjectInspector.Benchmark
{
    [MinColumn, MaxColumn, MemoryDiagnoser, MarkdownExporter]
    public class PublicClassPublicPropertySetterValue
    {
        private readonly SomeObject _testObject = new SomeObject();
        private readonly ISomeObject _duckObject;
        private readonly DynamicFetcher _expressionFetcher;
        private readonly DynamicFetcher _emitFetcher;
        private readonly Fetcher _delegateFetcher;
        
        public PublicClassPublicPropertySetterValue()
        {
            _duckObject = _testObject.DuckAs<ISomeObject>();
            _expressionFetcher = new DynamicFetcher("Value") { FetcherType = FetcherType.ExpressionTree };
            _expressionFetcher.Load(_testObject);
            _emitFetcher = new DynamicFetcher("Value") { FetcherType = FetcherType.Emit };
            _emitFetcher.Load(_testObject);
            _delegateFetcher = new DelegatePropertyFetcher<SomeObject, int>(typeof(SomeObject).GetProperty("Value")!);
        }

        [Benchmark]
        public void Direct() => _testObject.Value = 42;
        [Benchmark(Baseline = true)]
        public void DuckType() => _duckObject.Value = 42;
        [Benchmark]
        public void ExpressionTreeFetcher() => _expressionFetcher.Shove(_testObject, 42);
        [Benchmark]
        public void EmitFetcher() => _emitFetcher.Shove(_testObject, 42);
        [Benchmark]
        public void DelegateFetcher() => _delegateFetcher.Shove(_testObject, 42);
    }
}