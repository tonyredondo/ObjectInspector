using System;
using System.Reflection;
using BenchmarkDotNet.Attributes;

namespace Wanhjor.ObjectInspector.Benchmark
{
    [MinColumn, MaxColumn, MemoryDiagnoser, MarkdownExporter]
    public class PublicClassPublicFieldSetterValue
    {
        private readonly SomeObject _testObject = new SomeObject();
        private readonly ISomeObject _duckObject;
        private readonly DynamicFetcher _expressionFetcher;
        private readonly DynamicFetcher _emitFetcher;
        private readonly FieldInfo _fInfo;

        public PublicClassPublicFieldSetterValue()
        {
            _duckObject = _testObject.DuckAs<ISomeObject>();
            _expressionFetcher = new DynamicFetcher("ValueField") { FetcherType = FetcherType.ExpressionTree };
            _expressionFetcher.Load(_testObject);
            _emitFetcher = new DynamicFetcher("ValueField") { FetcherType = FetcherType.Emit };
            _emitFetcher.Load(_testObject);
            _fInfo = typeof(SomeObject).GetField("ValueField", DuckAttribute.AllFlags);
        }

        [Benchmark]
        public void Direct() => _testObject.ValueField = 42;
        [Benchmark(Baseline = true)]
        public void DuckType() => _duckObject.ValueField = 42;
        [Benchmark]
        public void ExpressionTreeFetcher() => _expressionFetcher.Shove(_testObject, 42);
        [Benchmark]
        public void EmitFetcher() => _emitFetcher.Shove(_testObject, 42);
        [Benchmark]
        public void DelegateFetcher() => throw new NotImplementedException();
        [Benchmark]
        public void Reflection() => _fInfo.SetValue(_testObject, 42);
    }
}