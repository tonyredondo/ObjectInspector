using System;
using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;

namespace Wanhjor.ObjectInspector.Benchmark
{
    [MinColumn, MaxColumn, MemoryDiagnoser, MarkdownExporter]
    public class PublicClassPublicMethod
    {
        private readonly SomeObject _testObject = new SomeObject();
        private readonly ISomeObject _duckObject;
        private readonly DynamicFetcher _expressionFetcher;
        private readonly DynamicFetcher _emitFetcher;
        
        public PublicClassPublicMethod()
        {
            _duckObject = _testObject.DuckAs<ISomeObject>();
            _expressionFetcher = new DynamicFetcher("Sum") { FetcherType = FetcherType.ExpressionTree };
            _expressionFetcher.Load(_testObject);
            _emitFetcher = new DynamicFetcher("Sum") { FetcherType = FetcherType.Emit };
            _emitFetcher.Load(_testObject);
        }

        [Benchmark]
        public void Direct() => _ = _testObject.Sum(2,2);
        [Benchmark(Baseline = true)]
        public void DuckType() => _ = _duckObject.Sum(2,2);
        [Benchmark]
        public void ExpressionTreeFetcher() => _ = (int)_expressionFetcher.Invoke(_testObject, 2, 2);
        [Benchmark]
        public void EmitFetcher() => _ = (int)_emitFetcher.Invoke(_testObject, 2, 2);
        [Benchmark]
        public void DelegateFetcher() => throw new NotImplementedException();
    }
}