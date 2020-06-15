using System;
using System.Reflection;
using BenchmarkDotNet.Attributes;

namespace Wanhjor.ObjectInspector.Benchmark
{
    [MinColumn, MaxColumn, MemoryDiagnoser, MarkdownExporter]
    public class PrivateClassPrivatePropertyGetterObject
    {
        private readonly PrivateSomeObject _testObject = new PrivateSomeObject();
        private readonly IPrivateSomeObject _duckObject;
        private readonly DynamicFetcher _expressionFetcher;
        private readonly DynamicFetcher _emitFetcher;
        private readonly Fetcher _delegateFetcher;
        private readonly PropertyInfo _pInfo;

        public PrivateClassPrivatePropertyGetterObject()
        {
            _duckObject = _testObject.DuckAs<IPrivateSomeObject>();
            _expressionFetcher = new DynamicFetcher("Name") { FetcherType = FetcherType.ExpressionTree };
            _expressionFetcher.Load(_testObject);
            _emitFetcher = new DynamicFetcher("Name") { FetcherType = FetcherType.Emit };
            _emitFetcher.Load(_testObject);
            _delegateFetcher = new DelegatePropertyFetcher<SomeObject, string>(typeof(SomeObject).GetProperty("Name")!);
            _pInfo = typeof(SomeObject).GetProperty("Name", DuckAttribute.AllFlags);
        }

        [Benchmark]
        public void Direct() => throw new NotImplementedException();
        [Benchmark(Baseline = true)]
        public void DuckType() => _ = _duckObject.Name;
        [Benchmark]
        public void ExpressionTreeFetcher() => _ = (string)_expressionFetcher.Fetch(_testObject);
        [Benchmark]
        public void EmitFetcher() => _ = (string)_emitFetcher.Fetch(_testObject);
        [Benchmark]
        public void DelegateFetcher() => _ = (string)_delegateFetcher.Fetch(_testObject);
        [Benchmark]
        public void Reflection() => _ = (string)_pInfo.GetValue(_testObject);
    }
}