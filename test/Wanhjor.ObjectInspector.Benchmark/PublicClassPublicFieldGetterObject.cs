using System;
using System.Reflection;
using BenchmarkDotNet.Attributes;

namespace Wanhjor.ObjectInspector.Benchmark
{
    [MinColumn, MaxColumn, MemoryDiagnoser, MarkdownExporter]
    public class PublicClassPublicFieldGetterObject
    {
        private readonly SomeObject _testObject = new SomeObject();
        private readonly ISomeObject _duckObjectInterface;
        private readonly AbstractSomeObject _duckObjectAbstract;
        private readonly VirtualClassSomeObject _duckObjectVirtualClass;
        private readonly DynamicFetcher _expressionFetcher;
        private readonly DynamicFetcher _emitFetcher;
        private readonly FieldInfo _fInfo;
        
        public PublicClassPublicFieldGetterObject()
        {
            _duckObjectInterface = _testObject.DuckAs<ISomeObject>();
            _duckObjectAbstract = _testObject.DuckAs<AbstractSomeObject>();
            _duckObjectVirtualClass = _testObject.DuckAs<VirtualClassSomeObject>();
            _expressionFetcher = new DynamicFetcher("NameField") { FetcherType = FetcherType.ExpressionTree };
            _expressionFetcher.Load(_testObject);
            _emitFetcher = new DynamicFetcher("NameField") { FetcherType = FetcherType.Emit };
            _emitFetcher.Load(_testObject);
            _fInfo = typeof(SomeObject).GetField("NameField", DuckAttribute.AllFlags);
        }

        [Benchmark]
        public void Direct() => _ = _testObject.NameField;
        [Benchmark]
        public void DuckTypeInterface() => _ = _duckObjectInterface.NameField;
        [Benchmark(Baseline = true)]
        public void DuckTypeAbstract() => _ = _duckObjectAbstract.NameField;
        [Benchmark]
        public void DuckTypeVirtual() => _ = _duckObjectVirtualClass.NameField;
        [Benchmark]
        public void ExpressionTreeFetcher() => _ = (string)_expressionFetcher.Fetch(_testObject);
        [Benchmark]
        public void EmitFetcher() => _ = (string)_emitFetcher.Fetch(_testObject);
        [Benchmark]
        public void DelegateFetcher() => throw new NotImplementedException();
        [Benchmark]
        public void Reflection() => _ = (string)_fInfo.GetValue(_testObject);
    }
}