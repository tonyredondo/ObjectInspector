using System;
using System.Reflection;
using BenchmarkDotNet.Attributes;

namespace Wanhjor.ObjectInspector.Benchmark
{
    [MinColumn, MaxColumn, MemoryDiagnoser, MarkdownExporter]
    public class PrivateClassPrivatePropertySetterValue
    {
        private readonly PrivateSomeObject _testObject = new PrivateSomeObject();
        private readonly IPrivateSomeObject _duckObject;
        private readonly DynamicFetcher _expressionFetcher;
        private readonly DynamicFetcher _emitFetcher;
        private readonly Fetcher _delegateFetcher;
        private readonly PropertyInfo _pInfo;

        public PrivateClassPrivatePropertySetterValue()
        {
            _duckObject = _testObject.DuckAs<IPrivateSomeObject>();
            _expressionFetcher = new DynamicFetcher("Value") { FetcherType = FetcherType.ExpressionTree };
            _expressionFetcher.Load(_testObject);
            _emitFetcher = new DynamicFetcher("Value") { FetcherType = FetcherType.Emit };
            _emitFetcher.Load(_testObject);
            _delegateFetcher = new DelegatePropertyFetcher<PrivateSomeObject, int>(typeof(PrivateSomeObject).GetProperty("Value", DuckAttribute.AllFlags)!);
            _pInfo = typeof(PrivateSomeObject).GetProperty("Value", DuckAttribute.AllFlags);
        }

        [Benchmark]
        public void Direct() => throw new NotImplementedException();
        [Benchmark(Baseline = true)]
        public void DuckType() => _duckObject.Value = 42;
        [Benchmark]
        public void ExpressionTreeFetcher() => _expressionFetcher.Shove(_testObject, 42);
        [Benchmark]
        public void EmitFetcher() => _emitFetcher.Shove(_testObject, 42);
        [Benchmark]
        public void DelegateFetcher() => _delegateFetcher.Shove(_testObject, 42);
        [Benchmark]
        public void Reflection() => _pInfo.SetValue(_testObject, 42);
    }
}