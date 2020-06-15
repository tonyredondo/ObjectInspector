using System;
using System.Reflection;
using BenchmarkDotNet.Attributes;

namespace Wanhjor.ObjectInspector.Benchmark
{
    [MinColumn, MaxColumn, MemoryDiagnoser, MarkdownExporter]
    public class PrivateClassPrivateFieldSetterObject
    {
        private readonly PrivateSomeObject _testObject = new PrivateSomeObject();
        private readonly IPrivateSomeObject _duckObject;
        private readonly DynamicFetcher _expressionFetcher;
        private readonly DynamicFetcher _emitFetcher;
        private readonly FieldInfo _fInfo;

        public PrivateClassPrivateFieldSetterObject()
        {
            _duckObject = _testObject.DuckAs<IPrivateSomeObject>();
            _expressionFetcher = new DynamicFetcher("NameField") { FetcherType = FetcherType.ExpressionTree };
            _expressionFetcher.Load(_testObject);
            _emitFetcher = new DynamicFetcher("NameField") { FetcherType = FetcherType.Emit };
            _emitFetcher.Load(_testObject);
            _fInfo = typeof(SomeObject).GetField("NameField", DuckAttribute.AllFlags);
        }

        [Benchmark]
        public void Direct() => throw new NotImplementedException();
        [Benchmark(Baseline = true)]
        public void DuckType() => _duckObject.NameField = "Value";
        [Benchmark]
        public void ExpressionTreeFetcher() => _expressionFetcher.Shove(_testObject, "Value");
        [Benchmark]
        public void EmitFetcher() => _emitFetcher.Shove(_testObject, "Value");
        [Benchmark]
        public void DelegateFetcher() => throw new NotImplementedException();
        [Benchmark]
        public void Reflection() => _fInfo.SetValue(_testObject, "Value");
    }
}