using System;
using System.Diagnostics;

namespace Wanhjor.ObjectInspector.Tests
{
    internal static class Runner
    {
        public static readonly object Locker = new object();
        private const int times = 1_000_000;

        internal static void RunA(string name, Action directAction, Action duckTypeAction)
        {
            lock (Locker)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                Stopwatch w = null, w1 = null;
                
                if (directAction != null)
                {
                    w = Stopwatch.StartNew();
                    for (var i = 0; i < times; i++)
                        directAction();
                    w.Stop();
                }

                w1 = Stopwatch.StartNew();
                for (var i = 0; i < times; i++)
                    duckTypeAction();
                w1.Stop();

                Console.WriteLine($"{name}:");
                if (directAction != null)
                {
                    Console.WriteLine($"\tDirect: {w.Elapsed.TotalMilliseconds} ms\tPer call: {w.Elapsed.TotalMilliseconds / times} ms");
                }
                Console.WriteLine($"\tDuckType: {w1.Elapsed.TotalMilliseconds} ms\tPer call: {w1.Elapsed.TotalMilliseconds / times} ms");
            }
        }
        
        internal static void RunF<TR>(string name, Func<TR> directFunc, Func<TR> duckTypeFunc)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            Stopwatch w = null, w1 = null;
            TR res = default;
            if (directFunc != null)
            {
                w = Stopwatch.StartNew();
                for (var i = 0; i < times; i++)
                    res = directFunc();
                w.Stop();
                GC.KeepAlive(res);
            }

            w1 = Stopwatch.StartNew();
            for (var i = 0; i < times; i++)
                res = duckTypeFunc();
            w1.Stop();
            GC.KeepAlive(res);
                
            Console.WriteLine($"{name}:");
            if (directFunc != null)
            {
                Console.WriteLine($"\tDirect: {w.Elapsed.TotalMilliseconds} ms\tPer call: {w.Elapsed.TotalMilliseconds / times} ms");
            }
            Console.WriteLine($"\tDuckType: {w1.Elapsed.TotalMilliseconds} ms\tPer call: {w1.Elapsed.TotalMilliseconds / times} ms");
        }
        
    }
}