using System.Reflection;
using BenchmarkDotNet.Attributes;

namespace Wanhjor.ObjectInspector.Benchmark
{
    [MinColumn, MaxColumn, MemoryDiagnoser, MarkdownExporter]
    public class PublicClassPublicPropertySetterValue
    {
        private readonly SomeObject _testObject = new SomeObject();
        private readonly ISomeObject _duckObjectInterface;
        private readonly AbstractSomeObject _duckObjectAbstract;
        private readonly VirtualClassSomeObject _duckObjectVirtualClass;
        private readonly DynamicFetcher _expressionFetcher;
        private readonly DynamicFetcher _emitFetcher;
        private readonly Fetcher _delegateFetcher;
        private readonly PropertyInfo _pInfo;

        public PublicClassPublicPropertySetterValue()
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
        public void Direct() => _testObject.Value = 42;
        [Benchmark]
        public void DuckTypeInterface() => _duckObjectInterface.Value = 42;
        [Benchmark(Baseline = true)]
        public void DuckTypeAbstract() => _duckObjectAbstract.Value = 42;
        [Benchmark]
        public void DuckTypeVirtual() => _duckObjectVirtualClass.Value = 42;
        [Benchmark]
        public void ExpressionTreeFetcher() => _expressionFetcher.Shove(_testObject, 42);
        [Benchmark]
        public void EmitFetcher() => _emitFetcher.Shove(_testObject, 42);
        [Benchmark]
        public void DelegateFetcher() => _delegateFetcher.Shove(_testObject, 42);
        [Benchmark]
        public void Reflection() => _pInfo.SetValue(_testObject, 42);
    }
}