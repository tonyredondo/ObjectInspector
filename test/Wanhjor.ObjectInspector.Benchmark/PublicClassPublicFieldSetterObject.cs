using System;
using System.Reflection;
using BenchmarkDotNet.Attributes;

namespace Wanhjor.ObjectInspector.Benchmark
{
    [MinColumn, MaxColumn, MemoryDiagnoser, MarkdownExporter]
    public class PublicClassPublicFieldSetterObject
    {
        private readonly SomeObject _testObject = new SomeObject();
        private readonly ISomeObject _duckObjectInterface;
        private readonly AbstractSomeObject _duckObjectAbstract;
        private readonly VirtualClassSomeObject _duckObjectVirtualClass;
        private readonly DynamicFetcher _expressionFetcher;
        private readonly DynamicFetcher _emitFetcher;
        private readonly FieldInfo _fInfo;

        public PublicClassPublicFieldSetterObject()
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
        public void Direct() => _testObject.NameField = "Value";
        [Benchmark]
        public void DuckTypeInterface() => _duckObjectInterface.NameField = "Value";
        [Benchmark(Baseline = true)]
        public void DuckTypeAbstract() => _duckObjectAbstract.NameField = "Value";
        [Benchmark]
        public void DuckTypeVirtual() => _duckObjectVirtualClass.NameField = "Value";
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