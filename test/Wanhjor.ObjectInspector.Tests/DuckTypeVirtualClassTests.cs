using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Xunit;

namespace Wanhjor.ObjectInspector.Tests
{
    public class DuckTypeVirtualClassTests
    {
        [Fact]
        [MethodImpl(MethodImplOptions.NoOptimization)]
        public void DuckTypeVirtualClassTest()
        {
            lock (Runner.Locker)
            {
                Console.WriteLine();

                var tObject = new TestObject {Name = "Tony", Value = "Redondo"};
                var iObj = tObject.DuckAs<VirtualClassDuckTestObject>();
                    
                var tTmp = Activator.CreateInstance(iObj.Type!);
                var tObj = tTmp!.DuckAs<VirtualClassDuckTestName>();
                tObj.Name = "My new setter";
                
                Console.WriteLine($"Type = {iObj.Type}");
                Console.WriteLine($"Version = {iObj.AssemblyVersion}");
                Console.WriteLine();
                
                Assert.Equal(tObject.Status, iObj.Status);

                Runner.RunF("Get Public Property", () => tObject.Name, () => iObj.Name);
                Runner.RunA("Set Public Property", () => tObject.Name = "SetTest", () => iObj.Name = "SetTest");
                Runner.RunF("Get Public Static Property", () => TestObject.PublicStaticProp, () => iObj.PublicStaticProp);
                Runner.RunA("Set Public Static Property", () => TestObject.PublicStaticProp = "PSP", () => _ = iObj.PublicStaticProp = "PSP");
                Runner.RunF("Get Private Static Property", null, () => iObj.PrivateStaticProp);
                Runner.RunF("Get Self Property as DuckType", () => DuckType.Create<VirtualClassDuckTestName>(tObject.Self), () => iObj.Self);
                Runner.RunA("Set Self Property as DuckType", () => tObject.Self = (TestObject)tObj.Instance, () => iObj.Self = tObj);
                Runner.RunF("Get Public Property. Float->Int conversion", () => (int) tObject.Number, () => iObj.Number);
                Runner.RunA("Set Public Property. Int->Float conversion", () => tObject.Number = (int) 42, () => iObj.Number = 42);
                Runner.RunF("Get Public Property. Enum->Int conversion", () => (int) tObject.MyEnumValue, () => iObj.MyEnumValue);
                Runner.RunA("Set Public Property. Int->Enum conversion", () => tObject.MyEnumValue = (TestEnum) 0, () => iObj.MyEnumValue = 0);
                Runner.RunF("Get Public Property. Enum->Enum conversion", () => (TestEnum2)(int) tObject.MyEnumValue, () => iObj.MyEnumValueConverted);
                Runner.RunF("Get Public Property. Float->Object conversion", () => (object) tObject.Number, () => iObj.NumberObject);
                Runner.RunA("Set Public Property. Object(Float)->Float conversion", () => tObject.Number = (float) (object) 51f, () => iObj.NumberObject = 51f);
                Runner.RunA("Set Public Property. Object(Int)->Float conversion", () => tObject.Number = (float) Convert.ChangeType(42, typeof(float)), () => iObj.NumberObject = 42);
                Runner.RunF("Get Public Property. IList conversion", () => (IList) tObject.MyList, () => iObj.MyList);
                Runner.RunF("Get Indexer Property", () => tObject[50], () => iObj[50]);
                Runner.RunA("Set Indexer Property", () => tObject[51] = "next one", () => iObj[51] = "next one with duck");
                Runner.RunF("Generic Method Call", () => tObject.GetDefault<Guid>(), () => iObj.GetDefault<Guid>());
                Console.WriteLine();
                Console.WriteLine();

                Runner.RunF("Get Public Field", () => tObject.Value, () => iObj.Value);
                Runner.RunA("Set Public Field", () => tObject.Value = "SetTest", () => iObj.Value = "SetTest");
                Runner.RunF("Get Public Static Field", () => TestObject._publicStaticField, () => iObj.PublicStaticField);
                Runner.RunA("Set Public Static Field", () => TestObject._publicStaticField = "PSP", () => _ = iObj.PublicStaticField = "PSP");
                Runner.RunF("Get Private Field", null, () => iObj.PrivateValue);
                Runner.RunA("Set Private Field", null, () => iObj.PrivateValue = "PrivateValue");
                Runner.RunF("Get Self Field as DuckType", () => tObject.ValueSelf.DuckAs<VirtualClassDuckTestName>(), () => iObj.ValueSelf);
                Runner.RunA("Set Self Field as DuckType", () => tObject.ValueSelf = (TestObject)tObj.Instance, () => iObj.ValueSelf = tObj);
                Runner.RunF("Get Private Static Field", null, () => iObj.PrivateStaticField);
                Runner.RunF("Get Public Field. Float->Int conversion", () => (int) tObject.FieldNumber, () => iObj.FieldNumberInteger);
                Runner.RunA("Set Public Field. Int->Float conversion", () => tObject.FieldNumber = (int) 42, () => iObj.FieldNumberInteger = 42);
                Runner.RunF("Get Public Field. Enum->Int conversion", () => (int) tObject.MyEnumFieldValue, () => iObj.MyEnumFieldValue);
                Runner.RunA("Set Public Field. Int->Enum conversion", () => tObject.MyEnumFieldValue = (TestEnum) 0, () => iObj.MyEnumFieldValue = 0);
                Runner.RunF("Get Public Field. Float->Object conversion", () => (object) tObject.FieldNumber, () => iObj.FieldNumberObject);
                Runner.RunA("Set Public Field. Object(Float)->Float conversion", () => tObject.FieldNumber = (float) (object) 51f, () => iObj.FieldNumberObject = 51f);
                Runner.RunA("Set Public Field. Object(Int)->Float conversion", () => tObject.FieldNumber = (float) Convert.ChangeType(42, typeof(float)), () => iObj.FieldNumberObject = 42);
            }
        }

        [Fact]
        [MethodImpl(MethodImplOptions.NoOptimization)]
        public void DictioTest()
        {
            var dictio = new Dictionary<string, string>();
            dictio.Add("Key1", "Value1");
            dictio.Add("Key2", "Value2");
            dictio.Add("Key3", "Value3");

            var idct = dictio.DuckAs<VirtualClassDictio>();
            var keys = idct.Keys;

            idct["Key1"] = "Edited";
            idct["Key4"] = "Value4";
        }

        [Fact]
        [MethodImpl(MethodImplOptions.NoOptimization)]
        public void FactoryTest()
        {
            var factory = typeof(IDictionary<string, string>).DuckFactoryAs<VirtualClassDictio>();

            for (var i = 0; i < 100; i++)
            {
                var inst = factory.Create(new Dictionary<string, string>());
                inst["Changes"] = "Changed";
            }
        }
        
        [Fact]
        [MethodImpl(MethodImplOptions.NoOptimization)]
        public void FactoryRentTest()
        {
            var factory = typeof(IDictionary<string, string>).DuckFactoryAs<VirtualClassDictio>();

            for (var i = 0; i < 100; i++)
            {
                using var lease = factory.Rent(new Dictionary<string, string>());
                
                lease.Instance["Changes"] = "Changed";
            }
        }
    }

    public class VirtualClassDictio : DuckType
    {
        public virtual string this[string key]
        {
            get => null;
            set { }
        }
        public virtual ICollection<string> Keys { get; } 
    }
    
    public class VirtualClassDuckTestName : DuckType
    {
        public virtual string Name { get; set; }
    }

    public class VirtualClassDuckTestObject : DuckType
    {
        public virtual string Name { get; set; }
        
        [Duck(Name="privateStaticProp", Flags = BindingFlags.NonPublic | BindingFlags.Static, UpToVersion = "0.5")]
        [Duck(Name="_privateStaticProp", Flags = BindingFlags.NonPublic | BindingFlags.Static)]
        public virtual string PrivateStaticProp { get; }
        
        [Duck(Flags = BindingFlags.NonPublic | BindingFlags.Instance)]
        public virtual string PrivateName { get; set; }
        
        public virtual VirtualClassDuckTestName Self { get; set; }
        
        public virtual int Number { get; set; }
        
        public virtual int MyEnumValue { get; set; }
        
        [Duck(Name = "MyEnumValue")]
        public virtual TestEnum2 MyEnumValueConverted { get; set; }

        [Duck(Name="Number")]
        public virtual object NumberObject { get; set; }
        
        public virtual IList MyList { get; }
        
        [Duck(Flags = BindingFlags.Public | BindingFlags.Static)]
        public virtual string PublicStaticProp { get; set; } 
        
        
        [Duck(Kind = DuckKind.Field)]
        public virtual string Value { get; set; }
        
        [Duck(Kind = DuckKind.Field)]
        public virtual VirtualClassDuckTestName ValueSelf { get; set; }
        
        [Duck(Name="_privateValue", Flags = BindingFlags.NonPublic | BindingFlags.Instance, Kind = DuckKind.Field)]
        public virtual string PrivateValue { get; set; }
        
        [Duck(Name="_privateStaticField", Flags = BindingFlags.NonPublic | BindingFlags.Static, Kind = DuckKind.Field)]
        public virtual string PrivateStaticField { get; }
        
        [Duck(Name="_publicStaticField", Flags = BindingFlags.Public | BindingFlags.Static, Kind = DuckKind.Field)]
        public virtual string PublicStaticField { get; set; }
        
        [Duck(Name="FieldNumber", Kind = DuckKind.Field)]
        public virtual int FieldNumberInteger { get; set; }
        
        [Duck(Name="FieldNumber", Kind = DuckKind.Field)]
        public virtual object FieldNumberObject { get; set; }
        
        [Duck(Kind = DuckKind.Field)]
        public virtual object MyEnumFieldValue { get; set; }

        public virtual int Sum(int a, int b) => 0;
        public virtual float Sum(float a, float b)=> 0;
        public virtual double Sum(double a, double b)=> 0;
        public virtual short Sum(short a, short b)=> 0;
        public virtual TestEnum ShowEnum(TestEnum val)=> 0;
        [Duck(Flags = BindingFlags.Instance | BindingFlags.NonPublic)]
        public virtual object InternalSum(int a, int b)=> 0;
        
        
        public virtual string this[int idx]
        {
            get => null; 
            set {} 
        }

        public virtual T GetDefault<T>() => throw new NotImplementedException();
        
        public virtual TaskStatus Status { get; set; }
    }
    
}