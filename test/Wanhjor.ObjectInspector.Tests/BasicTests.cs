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
            var tObject = new TestObject {Name = "Tony", Value = "Redondo"};

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
            var tObject = new TestObject {Name = "Tony", Value = "Redondo"};

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
            var tObject = new TestObject {Name = "Tony", Value = "Redondo"};

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
            var tObject = new TestObject {Name = "Tony", Value = "Redondo"};

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
            var tObject = new TestObject {Name = "Tony", Value = "Redondo"};

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
            lock (Runner.Locker)
            {
                var tObject = new TestObject {Name = "Tony", Value = "Redondo"};

                var objInsp = new ObjectInspector();
                var objData = objInsp.With(tObject);

                if (objData.TryGetFetcher("Name", out var nameFetcher))
                    Runner.RunF("Property Fetcher", () => tObject.Name, () => nameFetcher.Fetch(tObject));

                if (objData.TryGetFetcher("Value", out var valueFetcher))
                    Runner.RunF("Field Fetcher", () => tObject.Value, () => valueFetcher.Fetch(tObject));

                if (objData.TryGetFetcher("Sum", out var sumFetcher))
                {
                    var p = new object[] {2, 2};
                    Runner.RunF("Method Fetcher", () => tObject.Sum(2, 2), () => sumFetcher.Invoke(tObject, p)!);
                }
            }
        }

        [Fact]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public void DynamicFetcherExpressionTreeTest()
        {
            var keyFetcher = new DynamicFetcher("Key") {FetcherType = FetcherType.ExpressionTree};
            var valueFetcher = new DynamicFetcher("Value") {FetcherType = FetcherType.ExpressionTree};
            var dictio = new Dictionary<string, string>();
            dictio.Add("Key1", "Value1");
            dictio.Add("Key2", "Value2");

            foreach (var item in (IEnumerable) dictio)
            {
                var key = keyFetcher.Fetch(item);
                var value = valueFetcher.Fetch(item);
                
                Assert.NotNull(key);
                Assert.NotNull(value);
            }
        }
        
        
        [Fact]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public void DynamicFetcherEmitTest()
        {
            var keyFetcher = new DynamicFetcher("Key") {FetcherType = FetcherType.Emit};
            var valueFetcher = new DynamicFetcher("Value") {FetcherType = FetcherType.Emit};
            var dictio = new Dictionary<string, string>();
            dictio.Add("Key1", "Value1");
            dictio.Add("Key2", "Value2");

            foreach (var item in (IEnumerable) dictio)
            {
                var key = keyFetcher.Fetch(item);
                var value = valueFetcher.Fetch(item);
                
                Assert.NotNull(key);
                Assert.NotNull(value);
            }
        }
    }
}
