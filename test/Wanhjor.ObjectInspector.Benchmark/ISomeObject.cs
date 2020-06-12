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
}