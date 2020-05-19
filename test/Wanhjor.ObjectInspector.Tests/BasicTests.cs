using System;
using System.Diagnostics;
using System.Reflection;
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

            var objInsp = new ObjectInspector("Name", "Value", "PrivateName", "_privateValue", "Sum");
            var objData = objInsp.With(tObject);

            Assert.Equal("Tony", objData["Name"]);
            Assert.Equal("Redondo", objData["Value"]);
            Assert.Equal("My private name", objData["PrivateName"]);
            Assert.Equal("my private value", objData["_privateValue"]);
            Assert.Equal(4, objData.Invoke("Sum", 2, 2));
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

            
            
            if (objData.TryGetFetcher("Name", out var nameFetcher))
            {
                w1 = Stopwatch.StartNew();
                for (var i = 0; i < 100000; i++)
                {
                    name = nameFetcher.Fetch(tObject);
                }
                w1.Stop();
            }
            Console.WriteLine("Property Fetcher Elapsed: " + w1.Elapsed.TotalMilliseconds);

            w1 = Stopwatch.StartNew();
            for (var i = 0; i < 100000; i++)
            {
                name = tObject.Name;
            }
            w1.Stop();
            Console.WriteLine("Direct Property Elapsed: " + w1.Elapsed.TotalMilliseconds);
            
            
            
            
            if (objData.TryGetFetcher("Value", out var valueFetcher))
            {
                w1 = Stopwatch.StartNew();
                for (var i = 0; i < 100000; i++)
                {
                    name = valueFetcher.Fetch(tObject);
                }
                w1.Stop();
            }
            Console.WriteLine("Field Fetcher Elapsed: " + w1.Elapsed.TotalMilliseconds);

            w1 = Stopwatch.StartNew();
            for (var i = 0; i < 100000; i++)
            {
                name = tObject.Value;
            }
            w1.Stop();
            Console.WriteLine("Direct Field Elapsed: " + w1.Elapsed.TotalMilliseconds);



            var res = 0;
            if (objData.TryGetFetcher("Sum", out var sumFetcher))
            {
                var p = new object[] { 2, 2 };
                w1 = Stopwatch.StartNew();
                for (var i = 0; i < 100000; i++)
                {
                    _ = sumFetcher.Invoke(tObject, p)!;
                }
                w1.Stop();
            }
            Console.WriteLine("Method Invoke Elapsed: " + w1.Elapsed.TotalMilliseconds);
            
            w1 = Stopwatch.StartNew();
            for (var i = 0; i < 100000; i++)
            {
                res = tObject.Sum(2, 2);
            }
            w1.Stop();
            Console.WriteLine("Direct Method Elapsed: " + w1.Elapsed.TotalMilliseconds);

        }
    }
    
    


    public class TestObject
    {
        private static string _privateStaticProp { get; } = "private static prop";
        private static readonly string _privateStaticField = "private static field";

        public string Name { get; set; }

        public string Value;

        private string PrivateName { get; set; } = "My private name";

        private string _privateValue = "my private value";

        public int Sum(int a, int b) => a + b;
        private int InternalSum(int a, int b) => a + b;
    }
}
