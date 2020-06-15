using System;
using System.Reflection;
using BenchmarkDotNet.Attributes;

namespace Wanhjor.ObjectInspector.Benchmark
{
    [MinColumn, MaxColumn, MemoryDiagnoser, MarkdownExporter]
    public class PrivateClassPrivatePropertyGetterValue
    {
        private readonly PrivateSomeObject _testObject = new PrivateSomeObject();
        private readonly IPrivateSomeObject _duckObject;
        private readonly DynamicFetcher _expressionFetcher;
        private readonly DynamicFetcher _emitFetcher;
        private readonly Fetcher _delegateFetcher;
        private readonly PropertyInfo _pInfo;

        public PrivateClassPrivatePropertyGetterValue()
        {
            _duckObject = _testObject.DuckAs<IPrivateSomeObject>();
            _expressionFetcher = new DynamicFetcher("Value") { FetcherType = FetcherType.ExpressionTree };
            _expressionFetcher.Load(_testObject);
            _emitFetcher = new DynamicFetcher("Value") { FetcherType = FetcherType.Emit };
            _emitFetcher.Load(_testObject);
            _delegateFetcher = new DelegatePropertyFetcher<SomeObject, int>(typeof(SomeObject).GetProperty("Value")!);
            _pInfo = typeof(SomeObject).GetProperty("Value", DuckAttribute.AllFlags);
        }

        [Benchmark]
        public void Direct() => throw new NotImplementedException();
        [Benchmark(Baseline = true)]
        public void DuckType() => _ = _duckObject.Value;
        [Benchmark]
        public void ExpressionTreeFetcher() => _ = (int)_expressionFetcher.Fetch(_testObject);
        [Benchmark]
        public void EmitFetcher() => _ = (int)_emitFetcher.Fetch(_testObject);
        [Benchmark]
        public void DelegateFetcher() => _ = (int)_delegateFetcher.Fetch(_testObject);
        [Benchmark]
        public void Reflection() => _ = (int)_pInfo.GetValue(_testObject);
    }
}