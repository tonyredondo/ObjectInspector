# <img src="https://raw.githubusercontent.com/tonyredondo/ObjectInspector/master/icon.png" alt="Duck" width="45px" height="45px" /> .NET Object Inspector
[![Nuget](https://img.shields.io/nuget/vpre/Wanhjor.ObjectInspector?style=for-the-badge)](https://www.nuget.org/packages/Wanhjor.ObjectInspector/)

An efficient .NET object inspector/accesor to avoid reflection usage. 

PropertyFetcher, FieldFetcher, MethodCaller and DuckTyping for .NET by Expression Trees or Emitting IL at runtime.

## Target Frameworks
`netstandard2.0`, `netstandard2.1`, `netcoreapp2.0`, `netcoreapp2.1`, `net461`, `net462`, `net471`, `net472`, `net48` and `net45`.

### Dependencies for *netstandard2.0*:

- `System.Reflection.Emit (>= 4.7.0)`
- `System.Reflection.Emit.Lightweight (>= 4.7.0)`

## Usage

Given the following kind of class, the object inspector can help you to Get, Set or Invoke members of the object instance:
```cs
namespace OtherAssembly 
{
	public class SomeClass 
	{
		private string _privateField = "This field it's so private...";
		private string PrivateProperty { get; set; } = "You can't read this";

		private void PrivateConsoleWrite(string line) 
		{
			Console.WriteLine(line);
		}
	}
	
	public static class ObjectFactory
	{
		public static object New() => new SomeClass();
	}
}
```

### By using a Duck Typing mechanism
```cs
namespace MyAssembly
{
	public interface IDuckSomeClass 
	{
		[Duck(Name="_privateField", Kind = DuckKind.Field, Flags = DuckAttribute.AllFlags)]
		string PrivateField { get; set; }

		[Duck(Flags = DuckAttribute.AllFlags)]
		string PrivateProperty { get; set; }

		[Duck(Flags = DuckAttribute.AllFlags)]
		void PrivateConsoleWrite(string line);
	}

	public class Program
	{
		public static void Main()
		{
			var obj = OtherAssembly.ObjectFactory.New();
			HandleObject(obj);
		}
		
		public static void HandleObject(object obj) 
		{
			// We can treat the object as it were implementing the IDuckSomeClass interface
			var duck = obj.DuckAs<IDuckSomeClass>();

			Console.WriteLine(duck.PrivateField);
			Console.WriteLine(duck.PrivateProperty);
			Console.WriteLine();

			duck.PrivateField += " or not!";
			duck.PrivateProperty += ". I think I can!";

			Console.WriteLine(duck.PrivateField);
			Console.WriteLine(duck.PrivateProperty);
			Console.WriteLine();

			duck.PrivateConsoleWrite("Sooo private...");
		}
	}
}
```
[![**Execute in .NET Fiddle**](https://img.shields.io/badge/.NET%20Fiddle-Execute_with_Duck_Typing-blue?style=for-the-badge)](https://dotnetfiddle.net/39RPbz)

### By using a DynamicFetcher
```cs
namespace MyAssembly
{
	public class Program
	{
		public static void Main()
		{
			var obj = OtherAssembly.ObjectFactory.New();
			HandleObject(obj);
		}
		
		private static readonly DynamicFetcher _privateFieldFetcher = new DynamicFetcher("_privateField", DuckAttribute.AllFlags);
		private static readonly DynamicFetcher _privatePropertyFetcher = new DynamicFetcher("PrivateProperty", DuckAttribute.AllFlags);
		private static readonly DynamicFetcher _privateConsoleWriteFetcher = new DynamicFetcher("PrivateConsoleWrite", DuckAttribute.AllFlags);
		
		public static void HandleObject(object obj) 
		{
			// We can handle the object using a dynamic fetcher
			Console.WriteLine((string)_privateFieldFetcher.Fetch(obj));
			Console.WriteLine((string)_privatePropertyFetcher.Fetch(obj));
			Console.WriteLine();

			_privateFieldFetcher.Shove(obj, "This field it's so private... or not!");
			_privatePropertyFetcher.Shove(obj, "You can't read this. I think I can!");

			Console.WriteLine((string)_privateFieldFetcher.Fetch(obj));
			Console.WriteLine((string)_privatePropertyFetcher.Fetch(obj));
			Console.WriteLine();

			_privateConsoleWriteFetcher.Invoke(obj, "Sooo private...");
		}
	}
}
```
[![**Execute in .NET Fiddle**](https://img.shields.io/badge/.NET%20Fiddle-Execute_with_DynamicFetcher-blue?style=for-the-badge)](https://dotnetfiddle.net/mJlk9c)

### By using an Object Inspector
```cs
namespace MyAssembly
{
	public class Program
	{
		public static void Main()
		{
			var obj = OtherAssembly.ObjectFactory.New();
			HandleObject(obj);
		}
		
		public static void HandleObject(object obj) 
		{
			var inspector = new ObjectInspector(new InspectName("_privateField", DuckAttribute.AllFlags), 
			                    new InspectName("PrivateProperty", DuckAttribute.AllFlags), 
			                    new InspectName("PrivateConsoleWrite", DuckAttribute.AllFlags));
			
			var viewer = inspector.With(obj);
			
			Console.WriteLine((string)viewer["_privateField"]);
			Console.WriteLine((string)viewer["PrivateProperty"]);
			Console.WriteLine();

			viewer["_privateField"] = "This field it's so private... or not!";
			viewer["PrivateProperty"] = "You can't read this. I think I can!";

			Console.WriteLine((string)viewer["_privateField"]);
			Console.WriteLine((string)viewer["PrivateProperty"]);
			Console.WriteLine();

			viewer.Invoke("PrivateConsoleWrite", "Sooo private...");
		}
	}
}
```
[![**Execute in .NET Fiddle**](https://img.shields.io/badge/.NET%20Fiddle-Execute_with_Object_Inspector-blue?style=for-the-badge)](https://dotnetfiddle.net/dLXp8L)

### By using an Inspector Tuple
```cs
namespace MyAssembly
{
	public class Program
	{
		public static void Main()
		{
			var obj = OtherAssembly.ObjectFactory.New();
			HandleObject(obj);
		}
		
		public static void HandleObject(object obj) 
		{
			var inspTuple = new InspectorTuple<string, string, object>(new InspectName("_privateField", DuckAttribute.AllFlags), 
					    new InspectName("PrivateProperty", DuckAttribute.AllFlags), 
					    new InspectName("PrivateConsoleWrite", DuckAttribute.AllFlags));
			
			inspTuple.SetInstance(obj);
			
			Console.WriteLine(inspTuple.Item1);
			Console.WriteLine(inspTuple.Item2);
			Console.WriteLine();

			inspTuple.Item1 = "This field it's so private... or not!";
			inspTuple.Item2 = "You can't read this. I think I can!";

			Console.WriteLine(inspTuple.Item1);
			Console.WriteLine(inspTuple.Item2);
			Console.WriteLine();

			inspTuple.InvokeItem3("Sooo private...");
		}
	}
}
```
[![**Execute in .NET Fiddle**](https://img.shields.io/badge/.NET%20Fiddle-Execute_with_Inspector_Tuple-blue?style=for-the-badge)](https://dotnetfiddle.net/s1jkCD)


## Benchmarks
``` ini

BenchmarkDotNet=v0.12.1, OS=ubuntu 19.10
AMD Ryzen Threadripper 2950X, 1 CPU, 32 logical and 16 physical cores
.NET Core SDK=3.1.101
  [Host]     : .NET Core 3.1.1 (CoreCLR 4.700.19.60701, CoreFX 4.700.19.60801), X64 RyuJIT
  DefaultJob : .NET Core 3.1.1 (CoreCLR 4.700.19.60701, CoreFX 4.700.19.60801), X64 RyuJIT

```

### Public Class / Public Property / Getter / Object Type
|                Method |      Mean |     Error |    StdDev |       Min |       Max | Ratio | RatioSD | Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------- |----------:|----------:|----------:|----------:|----------:|------:|--------:|------:|------:|------:|----------:|
|                Direct | 0.0000 ns | 0.0000 ns | 0.0000 ns | 0.0000 ns | 0.0000 ns | 0.000 |    0.00 |     - |     - |     - |         - |
|              DuckType | 2.2114 ns | 0.0475 ns | 0.0421 ns | 2.1651 ns | 2.3086 ns | 1.000 |    0.00 |     - |     - |     - |         - |
| ExpressionTreeFetcher | 4.5575 ns | 0.0328 ns | 0.0307 ns | 4.5179 ns | 4.6236 ns | 2.062 |    0.04 |     - |     - |     - |         - |
|           EmitFetcher | 4.5295 ns | 0.0332 ns | 0.0311 ns | 4.4849 ns | 4.5786 ns | 2.049 |    0.04 |     - |     - |     - |         - |
|       DelegateFetcher | 6.9000 ns | 0.1674 ns | 0.2117 ns | 6.5945 ns | 7.4069 ns | 3.161 |    0.13 |     - |     - |     - |         - |

### Public Class / Public Property / Getter / Value Type
|                Method |       Mean |     Error |    StdDev |        Min |        Max | Ratio | RatioSD |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------- |-----------:|----------:|----------:|-----------:|-----------:|------:|--------:|-------:|------:|------:|----------:|
|                Direct |  0.0030 ns | 0.0027 ns | 0.0025 ns |  0.0000 ns |  0.0072 ns | 0.002 |    0.00 |      - |     - |     - |         - |
|              DuckType |  1.7111 ns | 0.0189 ns | 0.0177 ns |  1.6827 ns |  1.7458 ns | 1.000 |    0.00 |      - |     - |     - |         - |
| ExpressionTreeFetcher | 12.0056 ns | 0.2629 ns | 0.3418 ns | 11.2932 ns | 12.6295 ns | 7.017 |    0.22 | 0.0014 |     - |     - |      24 B |
|           EmitFetcher | 12.3734 ns | 0.2756 ns | 0.4970 ns | 11.4959 ns | 13.6574 ns | 7.385 |    0.29 | 0.0014 |     - |     - |      24 B |
|       DelegateFetcher | 15.6678 ns | 0.3361 ns | 0.4251 ns | 14.9725 ns | 16.3108 ns | 9.292 |    0.20 | 0.0014 |     - |     - |      24 B |

### Public Class / Public Property / Setter / Object Type
|                Method |      Mean |     Error |    StdDev |       Min |       Max | Ratio | RatioSD | Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------- |----------:|----------:|----------:|----------:|----------:|------:|--------:|------:|------:|------:|----------:|
|                Direct |  1.482 ns | 0.0356 ns | 0.0316 ns |  1.449 ns |  1.563 ns |  0.55 |    0.02 |     - |     - |     - |         - |
|              DuckType |  2.712 ns | 0.0420 ns | 0.0393 ns |  2.648 ns |  2.777 ns |  1.00 |    0.00 |     - |     - |     - |         - |
| ExpressionTreeFetcher |  4.424 ns | 0.0387 ns | 0.0323 ns |  4.356 ns |  4.464 ns |  1.63 |    0.02 |     - |     - |     - |         - |
|           EmitFetcher |  4.982 ns | 0.1186 ns | 0.1051 ns |  4.830 ns |  5.176 ns |  1.84 |    0.05 |     - |     - |     - |         - |
|       DelegateFetcher | 10.993 ns | 0.2399 ns | 0.2244 ns | 10.690 ns | 11.544 ns |  4.05 |    0.08 |     - |     - |     - |         - |

### Public Class / Public Property / Setter / Value Type
|                Method |       Mean |     Error |    StdDev |     Median |        Min |        Max | Ratio | RatioSD |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------- |-----------:|----------:|----------:|-----------:|-----------:|-----------:|------:|--------:|-------:|------:|------:|----------:|
|                Direct |  0.0000 ns | 0.0001 ns | 0.0001 ns |  0.0000 ns |  0.0000 ns |  0.0004 ns | 0.000 |    0.00 |      - |     - |     - |         - |
|              DuckType |  4.2136 ns | 0.1114 ns | 0.1561 ns |  4.2567 ns |  3.7230 ns |  4.3818 ns | 1.000 |    0.00 |      - |     - |     - |         - |
| ExpressionTreeFetcher | 13.7354 ns | 0.3124 ns | 0.9211 ns | 14.0423 ns | 10.2601 ns | 14.5011 ns | 3.060 |    0.29 | 0.0014 |     - |     - |      24 B |
|           EmitFetcher | 11.3130 ns | 0.3970 ns | 1.1705 ns | 10.6852 ns | 10.0962 ns | 14.7175 ns | 3.035 |    0.30 | 0.0014 |     - |     - |      24 B |
|       DelegateFetcher | 15.4818 ns | 0.3191 ns | 0.2985 ns | 15.4007 ns | 14.9891 ns | 16.1280 ns | 3.688 |    0.15 | 0.0014 |     - |     - |      24 B |

### Public Class / Public Field / Getter / Object Type
|                Method |      Mean |     Error |    StdDev |    Median |       Min |       Max | Ratio | RatioSD | Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------- |----------:|----------:|----------:|----------:|----------:|----------:|------:|--------:|------:|------:|------:|----------:|
|                Direct | 0.0017 ns | 0.0040 ns | 0.0035 ns | 0.0000 ns | 0.0000 ns | 0.0100 ns | 0.001 |    0.00 |     - |     - |     - |         - |
|              DuckType | 1.7539 ns | 0.0425 ns | 0.0398 ns | 1.7658 ns | 1.6914 ns | 1.8023 ns | 1.000 |    0.00 |     - |     - |     - |         - |
| ExpressionTreeFetcher | 4.5588 ns | 0.0487 ns | 0.0432 ns | 4.5623 ns | 4.4865 ns | 4.6220 ns | 2.606 |    0.07 |     - |     - |     - |         - |
|           EmitFetcher | 4.6230 ns | 0.0920 ns | 0.0860 ns | 4.5990 ns | 4.5082 ns | 4.7722 ns | 2.637 |    0.07 |     - |     - |     - |         - |
|       DelegateFetcher |        NA |        NA |        NA |        NA |        NA |        NA |     ? |       ? |     - |     - |     - |         - |

### Public Class / Public Field / Getter / Value Type
|                Method |       Mean |     Error |    StdDev |     Median |        Min |        Max | Ratio | RatioSD |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------- |-----------:|----------:|----------:|-----------:|-----------:|-----------:|------:|--------:|-------:|------:|------:|----------:|
|                Direct |  0.0011 ns | 0.0022 ns | 0.0021 ns |  0.0000 ns |  0.0000 ns |  0.0069 ns | 0.001 |    0.00 |      - |     - |     - |         - |
|              DuckType |  1.7445 ns | 0.0167 ns | 0.0139 ns |  1.7439 ns |  1.7179 ns |  1.7683 ns | 1.000 |    0.00 |      - |     - |     - |         - |
| ExpressionTreeFetcher | 12.2848 ns | 0.2715 ns | 0.3434 ns | 12.3956 ns | 11.5606 ns | 12.6538 ns | 7.011 |    0.22 | 0.0014 |     - |     - |      24 B |
|           EmitFetcher | 12.4600 ns | 0.2786 ns | 0.4170 ns | 12.4498 ns | 11.7362 ns | 13.1916 ns | 7.074 |    0.29 | 0.0014 |     - |     - |      24 B |
|       DelegateFetcher |         NA |        NA |        NA |         NA |         NA |         NA |     ? |       ? |      - |     - |     - |         - |

### Public Class / Public Field / Setter / Object Type
|                Method |     Mean |     Error |    StdDev |      Min |      Max | Ratio | RatioSD | Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------- |---------:|----------:|----------:|---------:|---------:|------:|--------:|------:|------:|------:|----------:|
|                Direct | 1.220 ns | 0.0191 ns | 0.0170 ns | 1.191 ns | 1.251 ns |  0.35 |    0.03 |     - |     - |     - |         - |
|              DuckType | 3.441 ns | 0.0992 ns | 0.2338 ns | 2.774 ns | 3.740 ns |  1.00 |    0.00 |     - |     - |     - |         - |
| ExpressionTreeFetcher | 4.935 ns | 0.1226 ns | 0.1312 ns | 4.791 ns | 5.216 ns |  1.42 |    0.10 |     - |     - |     - |         - |
|           EmitFetcher | 5.031 ns | 0.1228 ns | 0.1314 ns | 4.829 ns | 5.283 ns |  1.44 |    0.08 |     - |     - |     - |         - |
|       DelegateFetcher |       NA |        NA |        NA |       NA |       NA |     ? |       ? |     - |     - |     - |         - |

### Public Class / Public Field / Setter / Value Type
|                Method |       Mean |     Error |    StdDev |     Median |        Min |        Max | Ratio | RatioSD |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------- |-----------:|----------:|----------:|-----------:|-----------:|-----------:|------:|--------:|-------:|------:|------:|----------:|
|                Direct |  0.0075 ns | 0.0093 ns | 0.0087 ns |  0.0052 ns |  0.0000 ns |  0.0276 ns | 0.002 |    0.00 |      - |     - |     - |         - |
|              DuckType |  4.3461 ns | 0.0347 ns | 0.0307 ns |  4.3490 ns |  4.3007 ns |  4.3929 ns | 1.000 |    0.00 |      - |     - |     - |         - |
| ExpressionTreeFetcher | 11.5149 ns | 0.2588 ns | 0.7425 ns | 11.2100 ns | 10.4822 ns | 13.5046 ns | 2.846 |    0.22 | 0.0014 |     - |     - |      24 B |
|           EmitFetcher | 12.1096 ns | 0.2713 ns | 0.7288 ns | 12.3327 ns | 10.2920 ns | 13.0633 ns | 2.610 |    0.25 | 0.0014 |     - |     - |      24 B |
|       DelegateFetcher |         NA |        NA |        NA |         NA |         NA |         NA |     ? |       ? |      - |     - |     - |         - |

### Public Class / Public Method / Invoker
|                Method |       Mean |     Error |    StdDev |     Median |        Min |        Max |  Ratio | RatioSD |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------- |-----------:|----------:|----------:|-----------:|-----------:|-----------:|-------:|--------:|-------:|------:|------:|----------:|
|                Direct |  0.0014 ns | 0.0027 ns | 0.0025 ns |  0.0000 ns |  0.0000 ns |  0.0069 ns |  0.001 |    0.00 |      - |     - |     - |         - |
|              DuckType |  1.9597 ns | 0.0446 ns | 0.0395 ns |  1.9451 ns |  1.9113 ns |  2.0573 ns |  1.000 |    0.00 |      - |     - |     - |         - |
| ExpressionTreeFetcher | 50.6261 ns | 0.4818 ns | 0.4507 ns | 50.5896 ns | 49.8771 ns | 51.4285 ns | 25.853 |    0.52 | 0.0067 |     - |     - |     112 B |
|           EmitFetcher | 50.9856 ns | 1.0154 ns | 0.9499 ns | 51.0516 ns | 49.9053 ns | 52.9302 ns | 26.026 |    0.84 | 0.0067 |     - |     - |     112 B |
|       DelegateFetcher |         NA |        NA |        NA |         NA |         NA |         NA |      ? |       ? |      - |     - |     - |         - |


## Powered By
<img src="https://raw.githubusercontent.com/tonyredondo/TWCore2/master/doc/rider.jpg" alt="Rider" width="50px" height="50px" /><img src="https://raw.githubusercontent.com/tonyredondo/TWCore2/master/doc/dotTrace.png" alt="dotTrace" width="50px" height="50px" /><img src="https://raw.githubusercontent.com/tonyredondo/TWCore2/master/doc/dotMemory.png" alt="dotMemory" width="50px" height="50px" />

Thanks to @jetbrains for helping on this development with the licenses for Rider, dotTrace and dotMemory
