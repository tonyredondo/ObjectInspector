namespace Wanhjor.ObjectInspector.Benchmark
{
    public abstract class AbstractSomeObject
    {
        public abstract string Name { get; set; }
        public abstract int Value { get; set; }
        
        [Duck(Kind = DuckKind.Field)]
        public abstract string NameField { get; set; }
        
        [Duck(Kind = DuckKind.Field)]
        public abstract int ValueField { get; set; }

        public abstract int Sum(int a, int b);
    }
    
    public abstract class AbstractPrivateSomeObject
    {
        [Duck(Flags = DuckAttribute.AllFlags)]
        public abstract string Name { get; set; }
        [Duck(Flags = DuckAttribute.AllFlags)]
        public abstract int Value { get; set; }
        
        [Duck(Kind = DuckKind.Field, Flags = DuckAttribute.AllFlags)]
        public abstract string NameField { get; set; }
        
        [Duck(Kind = DuckKind.Field, Flags = DuckAttribute.AllFlags)]
        public abstract int ValueField { get; set; }

        [Duck(Flags = DuckAttribute.AllFlags)]
        public abstract int Sum(int a, int b);
    }
}