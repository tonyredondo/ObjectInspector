using System;
using System.Reflection;
using BenchmarkDotNet.Attributes;

namespace Wanhjor.ObjectInspector.Benchmark
{
    [MinColumn, MaxColumn, MemoryDiagnoser, MarkdownExporter]
    public class PublicClassPublicFieldSetterValue
    {
        private readonly SomeObject _testObject = new SomeObject();
        private readonly ISomeObject _duckObjectInterface;
        private readonly AbstractSomeObject _duckObjectAbstract;
        private readonly VirtualClassSomeObject _duckObjectVirtualClass;
        private readonly DynamicFetcher _expressionFetcher;
        private readonly DynamicFetcher _emitFetcher;
        private readonly FieldInfo _fInfo;

        public PublicClassPublicFieldSetterValue()
        {
            _duckObjectInterface = _testObject.DuckAs<ISomeObject>();
            _duckObjectAbstract = _testObject.DuckAs<AbstractSomeObject>();
            _duckObjectVirtualClass = _testObject.DuckAs<VirtualClassSomeObject>();
            _expressionFetcher = new DynamicFetcher("ValueField") { FetcherType = FetcherType.ExpressionTree };
            _expressionFetcher.Load(_testObject);
            _emitFetcher = new DynamicFetcher("ValueField") { FetcherType = FetcherType.Emit };
            _emitFetcher.Load(_testObject);
            _fInfo = typeof(SomeObject).GetField("ValueField", DuckAttribute.AllFlags);
        }

        [Benchmark]
        public void Direct() => _testObject.ValueField = 42;
        [Benchmark]
        public void DuckTypeInterface() => _duckObjectInterface.ValueField = 42;
        [Benchmark(Baseline = true)]
        public void DuckTypeAbstract() => _duckObjectAbstract.ValueField = 42;
        [Benchmark]
        public void DuckTypeVirtual() => _duckObjectVirtualClass.ValueField = 42;
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