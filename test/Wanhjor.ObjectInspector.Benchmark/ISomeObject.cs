// ReSharper disable IdentifierTypo
namespace Wanhjor.ObjectInspector.Benchmark
{
    public interface ISomeObject
    {
        string Name { get; set; }
        int Value { get; set; }
        
        [Duck(Kind = DuckKind.Field)]
        string NameField { get; set; }
        
        [Duck(Kind = DuckKind.Field)]
        int ValueField { get; set; }

        int Sum(int a, int b);
    }
    
    public interface IPrivateSomeObject
    {
        [Duck(Flags = DuckAttribute.AllFlags)]
        string Name { get; set; }
        [Duck(Flags = DuckAttribute.AllFlags)]
        int Value { get; set; }
        
        [Duck(Kind = DuckKind.Field, Flags = DuckAttribute.AllFlags)]
        string NameField { get; set; }
        
        [Duck(Kind = DuckKind.Field, Flags = DuckAttribute.AllFlags)]
        int ValueField { get; set; }

        [Duck(Flags = DuckAttribute.AllFlags)]
        int Sum(int a, int b);
    }
}