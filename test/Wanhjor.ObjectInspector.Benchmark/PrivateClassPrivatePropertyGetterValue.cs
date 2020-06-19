using System;
using System.Reflection;
using BenchmarkDotNet.Attributes;

namespace Wanhjor.ObjectInspector.Benchmark
{
    [MinColumn, MaxColumn, MemoryDiagnoser, MarkdownExporter]
    public class PrivateClassPrivatePropertyGetterValue
    {
        private readonly PrivateSomeObject _testObject = new PrivateSomeObject();
        private readonly IPrivateSomeObject _duckObjectInterface;
        private readonly AbstractPrivateSomeObject _duckObjectAbstract;
        private readonly VirtualClassPrivateSomeObject _duckObjectVirtualClass;
        private readonly DynamicFetcher _expressionFetcher;
        private readonly DynamicFetcher _emitFetcher;
        private readonly Fetcher _delegateFetcher;
        private readonly PropertyInfo _pInfo;

        public PrivateClassPrivatePropertyGetterValue()
        {
            _duckObjectInterface = _testObject.DuckAs<IPrivateSomeObject>();
            _duckObjectAbstract = _testObject.DuckAs<AbstractPrivateSomeObject>();
            _duckObjectVirtualClass = _testObject.DuckAs<VirtualClassPrivateSomeObject>();
            _expressionFetcher = new DynamicFetcher("Value") { FetcherType = FetcherType.ExpressionTree };
            _expressionFetcher.Load(_testObject);
            _emitFetcher = new DynamicFetcher("Value") { FetcherType = FetcherType.Emit };
            _emitFetcher.Load(_testObject);
            _delegateFetcher = new DelegatePropertyFetcher<PrivateSomeObject, int>(typeof(PrivateSomeObject).GetProperty("Value", DuckAttribute.AllFlags)!);
            _pInfo = typeof(PrivateSomeObject).GetProperty("Value", DuckAttribute.AllFlags);
        }

        [Benchmark]
        public void Direct() => throw new NotImplementedException();
        [Benchmark(Baseline = true)]
        public void DuckTypeInterface() => _ = _duckObjectInterface.Value;
        [Benchmark]
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