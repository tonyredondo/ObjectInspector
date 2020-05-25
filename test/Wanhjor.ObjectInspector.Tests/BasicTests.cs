using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using Xunit;
using Xunit.Abstractions;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable InconsistentNaming
#pragma warning disable 414

namespace Wanhjor.ObjectInspector.Tests
{
    public class BasicTests
    {
        [Fact]
        public void BasicObjectInspectorGetTest()
        {
            var tObject = new TestObject { Name = "Tony", Value = "Redondo" };

            var objInsp = new ObjectInspector("Name", "Value", "PrivateName", "_privateValue", "Sum", "ShowEnum");
            var objData = objInsp.With(tObject);

            Assert.Equal("Tony", objData["Name"]);
            Assert.Equal("Redondo", objData["Value"]);
            Assert.Equal("My private name", objData["PrivateName"]);
            Assert.Equal("my private value", objData["_privateValue"]);
            Assert.Equal(4, objData.Invoke("Sum", 2, 2));
            Assert.Equal(TestEnum.Second, objData.Invoke("ShowEnum", 1));
            Assert.Equal(4, objData.Invoke("Sum", 2d, 2f));
        }

        [Fact]
        public void BasicObjectInspectorSetTest()
        {
            var tObject = new TestObject { Name = "Tony", Value = "Redondo" };

            var objInsp = new ObjectInspector("Name", "Value", "PrivateName", "_privateValue", "Sum");
            var objData = objInsp.With(tObject);

            objData["Name"] = "Hola Mundo";
            objData["Value"] = "My Value";
            objData["PrivateName"] = "Changed!";

            Assert.Equal("Hola Mundo", objData["Name"]);
            Assert.Equal("My Value", objData["Value"]);
            Assert.Equal("Changed!", objData["PrivateName"]);
            Assert.Equal(2, objData.Invoke("Sum", 0, 2));
        }

        [Fact]
        public void ObjectInspectorWithInspectNameTest()
        {
            var tObject = new TestObject { Name = "Tony", Value = "Redondo" };

            var objInsp2 = new ObjectInspector(
                new InspectName("_privateStaticProp", Fetcher.BindStatic),
                new InspectName("_privateStaticField", Fetcher.BindStatic),
                new InspectName("InternalSum", Fetcher.BindInstance)
                );
            var objData = objInsp2.With(tObject);

            Assert.Equal("private static prop", objData["_privateStaticProp"]);
            Assert.Equal("private static field", objData["_privateStaticField"]);
            objData["_privateStaticProp"] = "hello";
            objData["_privateStaticField"] = "hello";

            Assert.Equal("private static prop", objData["_privateStaticProp"]);
            Assert.Equal("private static field", objData["_privateStaticField"]);
            
            Assert.Equal(4, objData.Invoke("InternalSum", 2, 2));
        }

        [Fact]
        public void ObjectInspectorWithInspectorTuplesTest()
        {
            var tObject = new TestObject { Name = "Tony", Value = "Redondo" };

            var iTuple = new InspectorTuple<string, string>("Name", "Value");
            iTuple.SetInstance(tObject);

            Assert.Equal("Tony", iTuple.Item1);
            Assert.Equal("Redondo", iTuple.Item2);


            var iTuple2 = new InspectorTuple<string, string, string, string, int>("Name", "Value", "PrivateName", "_privateValue", "Sum");
            iTuple2.SetInstance(tObject);


            Assert.Equal("Tony", iTuple2.Item1);
            Assert.Equal("Redondo", iTuple2.Item2);
            Assert.Equal("My private name", iTuple2.Item3);
            Assert.Equal("my private value", iTuple2.Item4);
            Assert.Equal(4, iTuple2.InvokeItem5(2, 2));
        }

        [Fact]
        public void ObjectInspectorAutogrowTest()
        {
            var tObject = new TestObject { Name = "Tony", Value = "Redondo" };

            var objInsp = new ObjectInspector();
            var objData = objInsp.With(tObject);

            Assert.Equal("Tony", objData["Name"]);
            Assert.Equal("Redondo", objData["Value"]);
            Assert.Equal("My private name", objData["PrivateName"]);
            Assert.Equal("my private value", objData["_privateValue"]);

            objData["Name"] = "Hola Mundo";
            objData["Value"] = "My Value";
            objData["PrivateName"] = "Changed!";

            Assert.Equal("Hola Mundo", objData["Name"]);
            Assert.Equal("My Value", objData["Value"]);
            Assert.Equal("Changed!", objData["PrivateName"]);
        }

        [Fact]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public void PerformanceTest()
        {
            var w1 = new Stopwatch();            
            object name;

            for (var i = 0; i < 10000; i++)
            {
                w1 = Stopwatch.StartNew();
                _ = w1.Elapsed;
            }

            var tObject = new TestObject { Name = "Tony", Value = "Redondo" };

            var objInsp = new ObjectInspector();
            var objData = objInsp.With(tObject);

            const int times = 1_000_000;
            
            if (objData.TryGetFetcher("Name", out var nameFetcher))
            {
                w1 = Stopwatch.StartNew();
                for (var i = 0; i < times; i++)
                {
                    name = nameFetcher.Fetch(tObject);
                }
                w1.Stop();
            }
            Console.WriteLine($"Property Fetcher Elapsed: {w1.Elapsed.TotalMilliseconds} - Per call: {w1.Elapsed.TotalMilliseconds / times}");

            w1 = Stopwatch.StartNew();
            for (var i = 0; i < times; i++)
            {
                name = tObject.Name;
            }
            w1.Stop();
            Console.WriteLine($"Direct Property Elapsed: {w1.Elapsed.TotalMilliseconds} - Per call: {w1.Elapsed.TotalMilliseconds / times}");
            
            
            
            
            if (objData.TryGetFetcher("Value", out var valueFetcher))
            {
                w1 = Stopwatch.StartNew();
                for (var i = 0; i < times; i++)
                {
                    name = valueFetcher.Fetch(tObject);
                }
                w1.Stop();
            }
            Console.WriteLine($"Field Fetcher Elapsed: {w1.Elapsed.TotalMilliseconds} - Per call: {w1.Elapsed.TotalMilliseconds / times}");

            w1 = Stopwatch.StartNew();
            for (var i = 0; i < times; i++)
            {
                name = tObject.Value;
            }
            w1.Stop();
            Console.WriteLine($"Direct Field Elapsed: {w1.Elapsed.TotalMilliseconds} - Per call: {w1.Elapsed.TotalMilliseconds / times}");



            var res = 0;
            if (objData.TryGetFetcher("Sum", out var sumFetcher))
            {
                var p = new object[] { 2, 2 };
                w1 = Stopwatch.StartNew();
                for (var i = 0; i < times; i++)
                {
                    _ = sumFetcher.Invoke(tObject, p)!;
                }
                w1.Stop();
            }
            Console.WriteLine($"Method Invoke Elapsed: {w1.Elapsed.TotalMilliseconds} - Per call: {w1.Elapsed.TotalMilliseconds / times}");
            
            w1 = Stopwatch.StartNew();
            for (var i = 0; i < times; i++)
            {
                res = tObject.Sum(2, 2);
            }
            w1.Stop();
            Console.WriteLine($"Direct Method Elapsed: {w1.Elapsed.TotalMilliseconds} - Per call: {w1.Elapsed.TotalMilliseconds / times}");

        }

        [Fact]
        [MethodImpl(MethodImplOptions.NoOptimization)]
        public void DuckTypeTest()
        {
            Console.WriteLine();
            var tObject = new TestObject { Name = "Tony", Value = "Redondo" };

            var iObj = (IDuckTestObject) DuckType.Create(typeof(IDuckTestObject), tObject);

            const int times = 1_000_000;

            iObj.NumberObject = 51;
            
            string name = null;
            var w1 = Stopwatch.StartNew();
            for (var i = 0; i < times; i++)
            {
                name = iObj.Name;
            }
            w1.Stop();
            Console.WriteLine($"DuckType Get Property Elapsed: {w1.Elapsed.TotalMilliseconds} - Per call: {w1.Elapsed.TotalMilliseconds / times}");
            
            w1 = Stopwatch.StartNew();
            for (var i = 0; i < times; i++)
            {
                iObj.Name = "Daniel";
            }
            w1.Stop();
            Console.WriteLine($"DuckType Set Property Elapsed: {w1.Elapsed.TotalMilliseconds} - Per call: {w1.Elapsed.TotalMilliseconds / times}");

            
            
            w1 = Stopwatch.StartNew();
            for (var i = 0; i < times; i++)
            {
                name = iObj.PrivateStaticProp;
            }
            w1.Stop();
            Console.WriteLine($"DuckType Get Private Property Elapsed: {w1.Elapsed.TotalMilliseconds} - Per call: {w1.Elapsed.TotalMilliseconds / times}");
            
            w1 = Stopwatch.StartNew();
            for (var i = 0; i < times; i++)
            {
                _ = iObj.Self;
            }
            w1.Stop();
            Console.WriteLine($"DuckType Get DuckType Self Property Elapsed: {w1.Elapsed.TotalMilliseconds} - Per call: {w1.Elapsed.TotalMilliseconds / times}");
            
            
            
            w1 = Stopwatch.StartNew();
            for (var i = 0; i < times; i++)
            {
                _ = iObj.Number;
            }
            w1.Stop();
            Console.WriteLine($"DuckType Get Float->Int Property Elapsed: {w1.Elapsed.TotalMilliseconds} - Per call: {w1.Elapsed.TotalMilliseconds / times}");
            
            w1 = Stopwatch.StartNew();
            for (var i = 0; i < times; i++)
            {
                iObj.Number = 42;
            }
            w1.Stop();
            Console.WriteLine($"DuckType Set Int->Float Property Elapsed: {w1.Elapsed.TotalMilliseconds} - Per call: {w1.Elapsed.TotalMilliseconds / times}");
            
            
            
            w1 = Stopwatch.StartNew();
            for (var i = 0; i < times; i++)
            {
                _ = iObj.MyEnumValue;
            }
            w1.Stop();
            Console.WriteLine($"DuckType Get Enum->Int Property Elapsed: {w1.Elapsed.TotalMilliseconds} - Per call: {w1.Elapsed.TotalMilliseconds / times}");
            
            w1 = Stopwatch.StartNew();
            for (var i = 0; i < times; i++)
            {
                iObj.MyEnumValue = 0;
            }
            w1.Stop();
            Console.WriteLine($"DuckType Get Int->Enum Property Elapsed: {w1.Elapsed.TotalMilliseconds} - Per call: {w1.Elapsed.TotalMilliseconds / times}");

            
            
            w1 = Stopwatch.StartNew();
            for (var i = 0; i < times; i++)
            {
                _ = iObj.NumberObject;
            }
            w1.Stop();
            Console.WriteLine($"DuckType Get Float->Object Property Elapsed: {w1.Elapsed.TotalMilliseconds} - Per call: {w1.Elapsed.TotalMilliseconds / times}");
            
            w1 = Stopwatch.StartNew();
            for (var i = 0; i < times; i++)
            {
                iObj.NumberObject = 51f;
            }
            w1.Stop();
            Console.WriteLine($"DuckType Get Object(Float)->Float Property Elapsed: {w1.Elapsed.TotalMilliseconds} - Per call: {w1.Elapsed.TotalMilliseconds / times}");

            w1 = Stopwatch.StartNew();
            for (var i = 0; i < times; i++)
            {
                iObj.NumberObject = 51;
            }
            w1.Stop();
            Console.WriteLine($"DuckType Get Object(Int)->Float Property Elapsed: {w1.Elapsed.TotalMilliseconds} - Per call: {w1.Elapsed.TotalMilliseconds / times}");

            
            
            w1 = Stopwatch.StartNew();
            for (var i = 0; i < times; i++)
            {
                _ = iObj.MyList;
            }
            w1.Stop();
            Console.WriteLine($"DuckType Get List<string>->IList Property Elapsed: {w1.Elapsed.TotalMilliseconds} - Per call: {w1.Elapsed.TotalMilliseconds / times}");
        }
    }

    public interface IDuckTestName
    {
        string Name { get; }
    }

    public interface IDuckTestObject
    {
        string Name { get; set; }
        
        [Duck(Name="privateStaticProp", Flags = BindingFlags.NonPublic | BindingFlags.Static, UpToVersion = "0.5")]
        [Duck(Name="_privateStaticProp", Flags = BindingFlags.NonPublic | BindingFlags.Static)]
        string PrivateStaticProp { get; }
        
        IDuckTestName Self { get; }
        
        int Number { get; set; }
        
        int MyEnumValue { get; set; }
        
        [Duck(Name="Number")]
        object NumberObject { get; set; }
        
        IList MyList { get; }
        
        /*
        [Duck(Kind = DuckKind.Field)]
        string Value { get; set; }

        [Duck(Flags = BindingFlags.NonPublic | BindingFlags.Instance)]
        string PrivateName { get; set; }
        
        [Duck(Name="_privateValue", Flags = BindingFlags.NonPublic | BindingFlags.Instance, Kind = DuckKind.Field)]
        string PrivateValue { get; set; }
        
        
        [Duck(Name="_privateStaticField", Flags = BindingFlags.NonPublic | BindingFlags.Static, Kind = DuckKind.Field)]
        string PrivateStaticField { get; }

        int Sum(int a, int b);

        [Duck(Flags = BindingFlags.Instance | BindingFlags.NonPublic)]
        object InternalSum(int a, int b);
        */
    }


    public class TestObject
    {
        private static string _privateStaticProp { get; } = "private static prop";
        private static readonly string _privateStaticField = "private static field";

        public string Name { get; set; }

        public string Value;

        private string PrivateName { get; set; } = "My private name";

        private string _privateValue = "my private value";

        public float Number { get; set; } = 3.225f;

        public TestEnum MyEnumValue { get; set; } = TestEnum.Second;

        public TestObject Self => this;

        public List<string> MyList { get; set; } = new List<string>();
        
        public int Sum(int a, int b) => a + b;
        private int InternalSum(int a, int b) => a + b;

        public TestEnum ShowEnum(TestEnum val)
        {
            return val;
        }
        
        
    }

    public enum TestEnum
    {
        First,
        Second
    }
}
