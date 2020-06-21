using System;
using System.Reflection;
using BenchmarkDotNet.Attributes;

namespace Wanhjor.ObjectInspector.Benchmark
{
    [MinColumn, MaxColumn, MemoryDiagnoser, MarkdownExporter]
    public class PublicClassPublicFieldGetterValue
    {
        private readonly SomeObject _testObject = new SomeObject();
        private readonly ISomeObject _duckObjectInterface;
        private readonly AbstractSomeObject _duckObjectAbstract;
        private readonly VirtualClassSomeObject _duckObjectVirtualClass;
        private readonly DynamicFetcher _expressionFetcher;
        private readonly DynamicFetcher _emitFetcher;
        private readonly FieldInfo _fInfo;

        public PublicClassPublicFieldGetterValue()
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
        public void Direct() => _ = _testObject.ValueField;
        [Benchmark]
        public void DuckTypeInterface() => _ = _duckObjectInterface.ValueField;
        [Benchmark(Baseline = true)]
        public void DuckTypeAbstract() => _ = _duckObjectAbstract.ValueField;
        [Benchmark]
        public void DuckTypeVirtual() => _ = _duckObjectVirtualClass.ValueField;
        [Benchmark]
        public void ExpressionTreeFetcher() => _ = (int)_expressionFetcher.Fetch(_testObject);
        [Benchmark]
        public void EmitFetcher() => _ = (int)_emitFetcher.Fetch(_testObject);
        [Benchmark]
        public void DelegateFetcher() => throw new NotImplementedException();
        [Benchmark]
        public void Reflection() => _ = (int)_fInfo.GetValue(_testObject);
    }
}