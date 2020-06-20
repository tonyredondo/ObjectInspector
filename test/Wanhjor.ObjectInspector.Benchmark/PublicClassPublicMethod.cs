using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;

namespace Wanhjor.ObjectInspector.Benchmark
{
    [MinColumn, MaxColumn, MemoryDiagnoser, MarkdownExporter]
    public class PublicClassPublicMethod
    {
        private readonly SomeObject _testObject = new SomeObject();
        private readonly ISomeObject _duckObjectInterface;
        private readonly AbstractSomeObject _duckObjectAbstract;
        private readonly VirtualClassSomeObject _duckObjectVirtualClass;
        private readonly DynamicFetcher _expressionFetcher;
        private readonly DynamicFetcher _emitFetcher;
        private readonly MethodInfo _mInfo;

        public PublicClassPublicMethod()
        {
            _duckObjectInterface = _testObject.DuckAs<ISomeObject>();
            _duckObjectAbstract = _testObject.DuckAs<AbstractSomeObject>();
            _duckObjectVirtualClass = _testObject.DuckAs<VirtualClassSomeObject>();
            _expressionFetcher = new DynamicFetcher("Sum") { FetcherType = FetcherType.ExpressionTree };
            _expressionFetcher.Load(_testObject);
            _emitFetcher = new DynamicFetcher("Sum") { FetcherType = FetcherType.Emit };
            _emitFetcher.Load(_testObject);
            _mInfo = typeof(SomeObject).GetMethod("Sum", DuckAttribute.AllFlags);
        }

        [Benchmark]
        public void Direct() => _ = _testObject.Sum(2,2);
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