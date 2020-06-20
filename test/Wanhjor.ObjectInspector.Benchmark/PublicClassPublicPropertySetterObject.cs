using System.Reflection;
using BenchmarkDotNet.Attributes;

namespace Wanhjor.ObjectInspector.Benchmark
{
    [MinColumn, MaxColumn, MemoryDiagnoser, MarkdownExporter]
    public class PublicClassPublicPropertySetterObject
    {
        private readonly SomeObject _testObject = new SomeObject();
        private readonly ISomeObject _duckObjectInterface;
        private readonly AbstractSomeObject _duckObjectAbstract;
        private readonly VirtualClassSomeObject _duckObjectVirtualClass;
        private readonly DynamicFetcher _expressionFetcher;
        private readonly DynamicFetcher _emitFetcher;
        private readonly Fetcher _delegateFetcher;
        private readonly PropertyInfo _pInfo;

        public PublicClassPublicPropertySetterObject()
        {
            _duckObjectInterface = _testObject.DuckAs<ISomeObject>();
            _duckObjectAbstract = _testObject.DuckAs<AbstractSomeObject>();
            _duckObjectVirtualClass = _testObject.DuckAs<VirtualClassSomeObject>();
            _expressionFetcher = new DynamicFetcher("Name") { FetcherType = FetcherType.ExpressionTree };
            _expressionFetcher.Load(_testObject);
            _emitFetcher = new DynamicFetcher("Name") { FetcherType = FetcherType.Emit };
            _emitFetcher.Load(_testObject);
            _delegateFetcher = new DelegatePropertyFetcher<SomeObject, string>(typeof(SomeObject).GetProperty("Name")!);
            _pInfo = typeof(SomeObject).GetProperty("Name", DuckAttribute.AllFlags);
        }

        [Benchmark]
        public void Direct() => _testObject.Name = "Value";
        [Benchmark]
        public void DuckTypeInterface() => _duckObjectInterface.Name = "Value";
        [Benchmark(Baseline = true)]
        public void DuckTypeAbstract() => _duckObjectAbstract.Name = "Value";
        [Benchmark]
        public void DuckTypeVirtual() => _duckObjectVirtualClass.Name = "Value";
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