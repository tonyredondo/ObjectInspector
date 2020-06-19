using System.Reflection;
using BenchmarkDotNet.Attributes;

namespace Wanhjor.ObjectInspector.Benchmark
{
    [MinColumn, MaxColumn, MemoryDiagnoser, MarkdownExporter]
    public class PublicClassPublicPropertyGetterObject
    {
        private readonly SomeObject _testObject = new SomeObject();
        private readonly ISomeObject _duckObjectInterface;
        private readonly AbstractSomeObject _duckObjectAbstract;
        private readonly VirtualClassSomeObject _duckObjectVirtualClass;
        private readonly DynamicFetcher _expressionFetcher;
        private readonly DynamicFetcher _emitFetcher;
        private readonly Fetcher _delegateFetcher;
        private readonly PropertyInfo _pInfo;

        public PublicClassPublicPropertyGetterObject()
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
        public void Direct() => _ = _testObject.Name;
        [Benchmark(Baseline = true)]
        public void DuckTypeInterface() => _ = _duckObjectInterface.Name;
        [Benchmark]
        public void DuckTypeAbstract() => _ = _duckObjectAbstract.Name;
        [Benchmark]
        public void DuckTypeVirtual() => _ = _duckObjectVirtualClass.Name;
        [Benchmark]
        public void ExpressionTreeFetcher() => _ = (string)_expressionFetcher.Fetch(_testObject);
        [Benchmark]
        public void EmitFetcher() => _ = (string)_emitFetcher.Fetch(_testObject);
        [Benchmark]
        public void DelegateFetcher() => _ = (string)_delegateFetcher.Fetch(_testObject);
        [Benchmark]
        public void Reflection() => _ = (string)_pInfo.GetValue(_testObject);
    }
}