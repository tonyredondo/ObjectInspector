using System;
using BenchmarkDotNet.Running;

namespace Wanhjor.ObjectInspector.Benchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run(typeof(Program).Assembly);
        }
    }
}