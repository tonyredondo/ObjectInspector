using System;
using System.Reflection;
using BenchmarkDotNet.Attributes;

namespace Wanhjor.ObjectInspector.Benchmark
{
    [MinColumn, MaxColumn, MemoryDiagnoser, MarkdownExporter]
    public class PrivateClassPrivatePropertySetterObject
    {
        private readonly PrivateSomeObject _testObject = new PrivateSomeObject();
        private readonly IPrivateSomeObject _duckObject;
        private readonly DynamicFetcher _expressionFetcher;
        private readonly DynamicFetcher _emitFetcher;
        private readonly Fetcher _delegateFetcher;
        private readonly PropertyInfo _pInfo;

        public PrivateClassPrivatePropertySetterObject()
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
        public void DuckType() => _duckObject.Name = "Value";
        [Benchmark]
        public void ExpressionTreeFetcher() => _expressionFetcher.Shove(_testObject, "Value");
        [Benchmark]
        public void EmitFetcher() => _emitFetcher.Shove(_testObject, "Value");
        [Benchmark]
        public void DelegateFetcher() => _delegateFetcher.Shove(_testObject, "Value");
        [Benchmark]
        public void Reflection() => _pInfo.SetValue(_testObject, "Value");
    }
}