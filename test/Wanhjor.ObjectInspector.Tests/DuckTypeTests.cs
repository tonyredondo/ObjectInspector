using System;
using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using Xunit;

namespace Wanhjor.ObjectInspector.Tests
{
    public class DuckTypeTests
    {
        [Fact]
        [MethodImpl(MethodImplOptions.NoOptimization)]
        public void DuckTypeTest()
        {
            Console.WriteLine();
            var tObject = new TestObject { Name = "Tony", Value = "Redondo" };

            var iObj = (IDuckTestObject) DuckType.Create(typeof(IDuckTestObject), tObject);

            const int times = 1_000_000;

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

            name = null;
            w1 = Stopwatch.StartNew();
            for (var i = 0; i < times; i++)
            {
                name = iObj.PublicStaticProp;
            }
            w1.Stop();
            Console.WriteLine($"DuckType Static Get Property Elapsed: {w1.Elapsed.TotalMilliseconds} - Per call: {w1.Elapsed.TotalMilliseconds / times}");
            
            w1 = Stopwatch.StartNew();
            for (var i = 0; i < times; i++)
            {
                iObj.PublicStaticProp = "Daniel";
            }
            w1.Stop();
            Console.WriteLine($"DuckType Static Set Property Elapsed: {w1.Elapsed.TotalMilliseconds} - Per call: {w1.Elapsed.TotalMilliseconds / times}");
            
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
            Console.WriteLine($"DuckType Set Int->Enum Property Elapsed: {w1.Elapsed.TotalMilliseconds} - Per call: {w1.Elapsed.TotalMilliseconds / times}");

            
            
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
            Console.WriteLine($"DuckType Set Object(Float)->Float Property Elapsed: {w1.Elapsed.TotalMilliseconds} - Per call: {w1.Elapsed.TotalMilliseconds / times}");

            w1 = Stopwatch.StartNew();
            for (var i = 0; i < times; i++)
            {
                iObj.NumberObject = 51;
            }
            w1.Stop();
            Console.WriteLine($"DuckType Set Object(Int)->Float Property Elapsed: {w1.Elapsed.TotalMilliseconds} - Per call: {w1.Elapsed.TotalMilliseconds / times}");

            
            
            w1 = Stopwatch.StartNew();
            for (var i = 0; i < times; i++)
            {
                _ = iObj.MyList;
            }
            w1.Stop();
            Console.WriteLine($"DuckType Get List<string>->IList Property Elapsed: {w1.Elapsed.TotalMilliseconds} - Per call: {w1.Elapsed.TotalMilliseconds / times}");
            
            
            
            
            
            Console.WriteLine();
            
            string value = null;
            w1 = Stopwatch.StartNew();
            for (var i = 0; i < times; i++)
            {
                value = iObj.Value;
            }
            w1.Stop();
            Console.WriteLine($"DuckType Get Field Elapsed: {w1.Elapsed.TotalMilliseconds} - Per call: {w1.Elapsed.TotalMilliseconds / times}");
            
            w1 = Stopwatch.StartNew();
            for (var i = 0; i < times; i++)
            {
                iObj.Value = "Smith";
            }
            w1.Stop();
            Console.WriteLine($"DuckType Set Field Elapsed: {w1.Elapsed.TotalMilliseconds} - Per call: {w1.Elapsed.TotalMilliseconds / times}");

            
            value = null;
            w1 = Stopwatch.StartNew();
            for (var i = 0; i < times; i++)
            {
                value = iObj.PublicStaticField;
            }
            w1.Stop();
            Console.WriteLine($"DuckType Static Get Field Elapsed: {w1.Elapsed.TotalMilliseconds} - Per call: {w1.Elapsed.TotalMilliseconds / times}");
            
            w1 = Stopwatch.StartNew();
            for (var i = 0; i < times; i++)
            {
                iObj.PublicStaticField = "Smith static field";
            }
            w1.Stop();
            Console.WriteLine($"DuckType Static Set Field Elapsed: {w1.Elapsed.TotalMilliseconds} - Per call: {w1.Elapsed.TotalMilliseconds / times}");

            
            w1 = Stopwatch.StartNew();
            for (var i = 0; i < times; i++)
            {
                name = iObj.PrivateValue;
            }
            w1.Stop();
            Console.WriteLine($"DuckType Get Private Field Elapsed: {w1.Elapsed.TotalMilliseconds} - Per call: {w1.Elapsed.TotalMilliseconds / times}");
            
            w1 = Stopwatch.StartNew();
            for (var i = 0; i < times; i++)
            {
                iObj.PrivateValue = "private value change";
            }
            w1.Stop();
            Console.WriteLine($"DuckType Set Private Field Elapsed: {w1.Elapsed.TotalMilliseconds} - Per call: {w1.Elapsed.TotalMilliseconds / times}");

            w1 = Stopwatch.StartNew();
            for (var i = 0; i < times; i++)
            {
                _ = iObj.ValueSelf;
            }
            w1.Stop();
            Console.WriteLine($"DuckType Get DuckType Self Field Elapsed: {w1.Elapsed.TotalMilliseconds} - Per call: {w1.Elapsed.TotalMilliseconds / times}");

            w1 = Stopwatch.StartNew();
            for (var i = 0; i < times; i++)
            {
                name = iObj.PrivateStaticField;
            }
            w1.Stop();
            Console.WriteLine($"DuckType Get Private Static Field Elapsed: {w1.Elapsed.TotalMilliseconds} - Per call: {w1.Elapsed.TotalMilliseconds / times}");

            
            w1 = Stopwatch.StartNew();
            for (var i = 0; i < times; i++)
            {
                _ = iObj.FieldNumberInteger;
            }
            w1.Stop();
            Console.WriteLine($"DuckType Get Float->Int Field Elapsed: {w1.Elapsed.TotalMilliseconds} - Per call: {w1.Elapsed.TotalMilliseconds / times}");
            
            w1 = Stopwatch.StartNew();
            for (var i = 0; i < times; i++)
            {
                iObj.FieldNumberInteger = 42;
            }
            w1.Stop();
            Console.WriteLine($"DuckType Set Int->Float Field Elapsed: {w1.Elapsed.TotalMilliseconds} - Per call: {w1.Elapsed.TotalMilliseconds / times}");

            
            w1 = Stopwatch.StartNew();
            for (var i = 0; i < times; i++)
            {
                _ = iObj.MyEnumFieldValue;
            }
            w1.Stop();
            Console.WriteLine($"DuckType Get Enum->Int Field Elapsed: {w1.Elapsed.TotalMilliseconds} - Per call: {w1.Elapsed.TotalMilliseconds / times}");
            
            w1 = Stopwatch.StartNew();
            for (var i = 0; i < times; i++)
            {
                iObj.MyEnumFieldValue = 0;
            }
            w1.Stop();
            Console.WriteLine($"DuckType Set Int->Enum Field Elapsed: {w1.Elapsed.TotalMilliseconds} - Per call: {w1.Elapsed.TotalMilliseconds / times}");


            w1 = Stopwatch.StartNew();
            for (var i = 0; i < times; i++)
            {
                _ = iObj.FieldNumberObject;
            }
            w1.Stop();
            Console.WriteLine($"DuckType Get Float->Object Field Elapsed: {w1.Elapsed.TotalMilliseconds} - Per call: {w1.Elapsed.TotalMilliseconds / times}");
            
            w1 = Stopwatch.StartNew();
            for (var i = 0; i < times; i++)
            {
                iObj.FieldNumberObject = 51f;
            }
            w1.Stop();
            Console.WriteLine($"DuckType Set Object(Float)->Float Field Elapsed: {w1.Elapsed.TotalMilliseconds} - Per call: {w1.Elapsed.TotalMilliseconds / times}");

            w1 = Stopwatch.StartNew();
            for (var i = 0; i < times; i++)
            {
                iObj.FieldNumberObject = 51;
            }
            w1.Stop();
            Console.WriteLine($"DuckType Set Object(Int)->Float Field Elapsed: {w1.Elapsed.TotalMilliseconds} - Per call: {w1.Elapsed.TotalMilliseconds / times}");

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
        
        [Duck(Flags = BindingFlags.NonPublic | BindingFlags.Instance)]
        string PrivateName { get; set; }
        
        IDuckTestName Self { get; }
        
        int Number { get; set; }
        
        int MyEnumValue { get; set; }
        
        [Duck(Name="Number")]
        object NumberObject { get; set; }
        
        IList MyList { get; }
        
        [Duck(Flags = BindingFlags.Public | BindingFlags.Static)]
        string PublicStaticProp { get; set; } 
        
        
        [Duck(Kind = DuckKind.Field)]
        string Value { get; set; }
        
        [Duck(Kind = DuckKind.Field)]
        IDuckTestName ValueSelf { get; }
        
        [Duck(Name="_privateValue", Flags = BindingFlags.NonPublic | BindingFlags.Instance, Kind = DuckKind.Field)]
        string PrivateValue { get; set; }
        
        [Duck(Name="_privateStaticField", Flags = BindingFlags.NonPublic | BindingFlags.Static, Kind = DuckKind.Field)]
        string PrivateStaticField { get; }
        
        [Duck(Name="_publicStaticField", Flags = BindingFlags.Public | BindingFlags.Static, Kind = DuckKind.Field)]
        string PublicStaticField { get; set; }
        
        [Duck(Name="FieldNumber", Kind = DuckKind.Field)]
        int FieldNumberInteger { get; set; }
        
        [Duck(Name="FieldNumber", Kind = DuckKind.Field)]
        object FieldNumberObject { get; set; }
        
        [Duck(Kind = DuckKind.Field)]
        object MyEnumFieldValue { get; set; }
        
        int Sum(int a, int b);
        float Sum(float a, float b);
        double Sum(double a, double b);
        short Sum(short a, short b);
        TestEnum ShowEnum(TestEnum val);
        [Duck(Flags = BindingFlags.Instance | BindingFlags.NonPublic)]
        object InternalSum(int a, int b);
    }
}