using System;
using System.Reflection;
using BenchmarkDotNet.Attributes;

namespace Wanhjor.ObjectInspector.Benchmark
{
    [MinColumn, MaxColumn, MemoryDiagnoser, MarkdownExporter]
    public class PrivateClassPrivateMethod
    {
        private readonly PrivateSomeObject _testObject = new PrivateSomeObject();
        private readonly IPrivateSomeObject _duckObjectInterface;
        private readonly AbstractPrivateSomeObject _duckObjectAbstract;
        private readonly VirtualClassPrivateSomeObject _duckObjectVirtualClass;
        private readonly DynamicFetcher _expressionFetcher;
        private readonly DynamicFetcher _emitFetcher;
        private readonly MethodInfo _mInfo;

        public PrivateClassPrivateMethod()
        {
            _duckObjectInterface = _testObject.DuckAs<IPrivateSomeObject>();
            _duckObjectAbstract = _testObject.DuckAs<AbstractPrivateSomeObject>();
            _duckObjectVirtualClass = _testObject.DuckAs<VirtualClassPrivateSomeObject>();
            _expressionFetcher = new DynamicFetcher("Sum") { FetcherType = FetcherType.ExpressionTree };
            _expressionFetcher.Load(_testObject);
            _emitFetcher = new DynamicFetcher("Sum") { FetcherType = FetcherType.Emit };
            _emitFetcher.Load(_testObject);
            _mInfo = typeof(PrivateSomeObject).GetMethod("Sum", DuckAttribute.AllFlags);
        }

        [Benchmark]
        public void Direct() => throw new NotImplementedException();
        [Benchmark]
        public void DuckTypeInterface() => _ = _duckObjectInterface.Sum(2,2);
        [Benchmark(Baseline = true)]
        public void DuckTypeAbstract() => _ = _duckObjectAbstract.Sum(2,2);
        [Benchmark]
        public void DuckTypeVirtual() => _ = _duckObjectVirtualClass.Sum(2,2);
        [Benchmark]
        public void ExpressionTreeFetcher() => _ = (int)_expressionFetcher.Invoke(_testObject, 2, 2);
        [Benchmark]
        public void EmitFetcher() => _ = (int)_emitFetcher.Invoke(_testObject, 2, 2);
        [Benchmark]
        public void DelegateFetcher() => throw new NotImplementedException();
        [Benchmark]
        public void Reflection() => _ = (int)_mInfo.Invoke(_testObject, new object[]{2, 2});
    }
}