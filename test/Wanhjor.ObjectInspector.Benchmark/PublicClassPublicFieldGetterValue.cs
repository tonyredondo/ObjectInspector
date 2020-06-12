using System;
using BenchmarkDotNet.Attributes;

namespace Wanhjor.ObjectInspector.Benchmark
{
    [MinColumn, MaxColumn, MemoryDiagnoser, MarkdownExporter]
    public class PublicClassPublicFieldGetterValue
    {
        private readonly SomeObject _testObject = new SomeObject();
        private readonly ISomeObject _duckObject;
        private readonly DynamicFetcher _expressionFetcher;
        private readonly DynamicFetcher _emitFetcher;
        
        public PublicClassPublicFieldGetterValue()
        {
            _duckObject = _testObject.DuckAs<ISomeObject>();
            _expressionFetcher = new DynamicFetcher("ValueField") { FetcherType = FetcherType.ExpressionTree };
            _expressionFetcher.Load(_testObject);
            _emitFetcher = new DynamicFetcher("ValueField") { FetcherType = FetcherType.Emit };
            _emitFetcher.Load(_testObject);
        }

        [Benchmark]
        public void Direct() => _ = _testObject.ValueField;
        [Benchmark(Baseline = true)]
        public void DuckType() => _ = _duckObject.ValueField;
        [Benchmark]
        public void ExpressionTreeFetcher() => _ = (int)_expressionFetcher.Fetch(_testObject);
        [Benchmark]
        public void EmitFetcher() => _ = (int)_emitFetcher.Fetch(_testObject);
        [Benchmark]
        public void DelegateFetcher() => throw new NotImplementedException();
    }
}