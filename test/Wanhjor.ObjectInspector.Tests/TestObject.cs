using System.Collections.Generic;
using System.Threading.Tasks;

// ReSharper disable UnusedMember.Local
#pragma warning disable 414

namespace Wanhjor.ObjectInspector.Tests
{
    public class TestObject
    {
        private static string _privateStaticProp { get; } = "private static prop";
        public static string PublicStaticProp { get; set; } = "public static prop";
        public string Name { get; set; }
        private string PrivateName { get; set; } = "My private name";
        public float Number { get; set; } = 3.225f;
        public TestEnum MyEnumValue { get; set; } = TestEnum.Second;
        public TestObject Self { get; set; }
        public List<string> MyList { get; set; } = new List<string>();

        
        private static readonly string _privateStaticField = "private static field";
        public static string _publicStaticField = "My public static field";
        public string Value;
        private string _privateValue = "my private value";
        public float FieldNumber = 3.225f;
        public TestEnum MyEnumFieldValue = TestEnum.Second;
        public TestObject ValueSelf;

        
        public TestObject()
        {
            Self = this;
            ValueSelf = this;
            _arr[0] = "Hello";
            _arr[50] = "World";
        }

        private readonly string[] _arr = new string[100];
        public string this[int idx]
        {
            get => _arr[idx];
            set => _arr[idx] = value;
        }
        
        public int Sum(int a, int b) => a + b;
        public float Sum(float a, float b) => a + b;
        public double Sum(double a, double b) => a + b;
        public short Sum(short a, short b) => (short)(a + b);
        private int InternalSum(int a, int b) => a + b;

        public TestEnum ShowEnum(TestEnum val)
        {
            return val;
        }

        public T GetDefault<T>() => default;
        
        public TaskStatus Status { get; set; } = TaskStatus.RanToCompletion;
    }
    
    public enum TestEnum
    {
        First,
        Second
    }

}