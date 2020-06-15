#pragma warning disable 414
namespace Wanhjor.ObjectInspector.Benchmark
{
    public class SomeObject
    {
        public string Name { get; set; } = "Name value";
        public int Value { get; set; } = 42;

        public string NameField = "Name field";
        public int ValueField = 66;

        public int Sum(int a, int b) => a + b;
    }

    class PrivateSomeObject
    {
        string Name { get; set; } = "Name value";
        int Value { get; set; } = 42;

        string NameField = "Name field";
        int ValueField = 66;

        int Sum(int a, int b) => a + b;
    }
}