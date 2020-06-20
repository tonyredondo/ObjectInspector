using System;
using System.Reflection;
using BenchmarkDotNet.Attributes;

namespace Wanhjor.ObjectInspector.Benchmark
{
    [MinColumn, MaxColumn, MemoryDiagnoser, MarkdownExporter]
    public class PrivateClassPrivateFieldSetterValue
    {
        private readonly PrivateSomeObject _testObject = new PrivateSomeObject();
        private readonly IPrivateSomeObject _duckObjectInterface;
        private readonly AbstractPrivateSomeObject _duckObjectAbstract;
        private readonly VirtualClassPrivateSomeObject _duckObjectVirtualClass;
        private readonly DynamicFetcher _expressionFetcher;
        private readonly DynamicFetcher _emitFetcher;
        private readonly FieldInfo _fInfo;

        public PrivateClassPrivateFieldSetterValue()
        {
            _duckObjectInterface = _testObject.DuckAs<IPrivateSomeObject>();
            _duckObjectAbstract = _testObject.DuckAs<AbstractPrivateSomeObject>();
            _duckObjectVirtualClass = _testObject.DuckAs<VirtualClassPrivateSomeObject>();
            _expressionFetcher = new DynamicFetcher("ValueField") { FetcherType = FetcherType.ExpressionTree };
            _expressionFetcher.Load(_testObject);
            _emitFetcher = new DynamicFetcher("ValueField") { FetcherType = FetcherType.Emit };
            _emitFetcher.Load(_testObject);
            _fInfo = typeof(PrivateSomeObject).GetField("ValueField", DuckAttribute.AllFlags);
        }

        [Benchmark]
        public void Direct() => throw new NotImplementedException();
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