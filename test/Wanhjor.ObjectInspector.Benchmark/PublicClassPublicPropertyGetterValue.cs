using System.Reflection;
using BenchmarkDotNet.Attributes;

namespace Wanhjor.ObjectInspector.Benchmark
{
    [MinColumn, MaxColumn, MemoryDiagnoser, MarkdownExporter]
    public class PublicClassPublicPropertyGetterValue
    {
        private readonly SomeObject _testObject = new SomeObject();
        private readonly ISomeObject _duckObjectInterface;
        private readonly AbstractSomeObject _duckObjectAbstract;
        private readonly VirtualClassSomeObject _duckObjectVirtualClass;
        private readonly DynamicFetcher _expressionFetcher;
        private readonly DynamicFetcher _emitFetcher;
        private readonly Fetcher _delegateFetcher;
        private readonly PropertyInfo _pInfo;

        public PublicClassPublicPropertyGetterValue()
        {
            _duckObjectInterface = _testObject.DuckAs<ISomeObject>();
            _duckObjectAbstract = _testObject.DuckAs<AbstractSomeObject>();
            _duckObjectVirtualClass = _testObject.DuckAs<VirtualClassSomeObject>();
            _expressionFetcher = new DynamicFetcher("Value") { FetcherType = FetcherType.ExpressionTree };
            _expressionFetcher.Load(_testObject);
            _emitFetcher = new DynamicFetcher("Value") { FetcherType = FetcherType.Emit };
            _emitFetcher.Load(_testObject);
            _delegateFetcher = new DelegatePropertyFetcher<SomeObject, int>(typeof(SomeObject).GetProperty("Value")!);
            _pInfo = typeof(SomeObject).GetProperty("Value", DuckAttribute.AllFlags);
        }

        [Benchmark]
        public void Direct() => _ = _testObject.Value;
        [Benchmark]
        public void DuckTypeInterface() => _ = _duckObjectInterface.Value;
        [Benchmark(Baseline = true)]
        public void DuckTypeAbstract() => _ = _duckObjectAbstract.Value;
        [Benchmark]
        public void DuckTypeVirtual() => _ = _duckObjectVirtualClass.Value;
        [Benchmark]
        public void ExpressionTreeFetcher() => _ = (int)_expressionFetcher.Fetch(_testObject);
        [Benchmark]
        public void EmitFetcher() => _ = (int)_emitFetcher.Fetch(_testObject);
        [Benchmark]
        public void DelegateFetcher() => _ = (int)_delegateFetcher.Fetch(_testObject);
        [Benchmark]
        public void Reflection() => _ = (int)_pInfo.GetValue(_testObject);
    }
}