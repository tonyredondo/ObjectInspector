// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable IdentifierTypo
namespace Wanhjor.ObjectInspector.Benchmark
{
    public class VirtualClassSomeObject : DuckType
    {
        public virtual string Name { get; set; }
        public virtual int Value { get; set; }
        
        [Duck(Kind = DuckKind.Field)]
        public virtual string NameField { get; set; }
        
        [Duck(Kind = DuckKind.Field)]
        public virtual int ValueField { get; set; }

        public virtual int Sum(int a, int b) => 0;
    }
    
    public class VirtualClassPrivateSomeObject : DuckType
    {
        [Duck(Flags = DuckAttribute.AllFlags)]
        public virtual string Name { get; set; }
        [Duck(Flags = DuckAttribute.AllFlags)]
        public virtual int Value { get; set; }
        
        [Duck(Kind = DuckKind.Field, Flags = DuckAttribute.AllFlags)]
        public virtual string NameField { get; set; }
        
        [Duck(Kind = DuckKind.Field, Flags = DuckAttribute.AllFlags)]
        public virtual int ValueField { get; set; }

        [Duck(Flags = DuckAttribute.AllFlags)]
        public virtual int Sum(int a, int b) => 0;
    }
}