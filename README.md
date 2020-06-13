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

Given the following class with private members, the object inspector can help you to Get, Set or Invoke members of the object instance:
```cs
namespace OtherAssembly 
{
	public class SomeClass 
	{
		private string _privateField = "This field it's so private...";
		private string PrivateProperty { get; set; } = "You can't read this";
		private void PrivateConsoleWrite(string line) => Console.WriteLine(line);
	}
	public static class ObjectFactory
	{
		public static object New() => new SomeClass();
	}
}
```

> **Note** You can inspect, public or non public types with public or non public members, also it's compatible with anonymous objects. 
>As you can see in the following code: [![**Execute in .NET Fiddle**](https://img.shields.io/badge/.NET%20Fiddle-Execute_with_Anonymous_object-blue?style=for-the-badge)](https://dotnetfiddle.net/cBAmGG)

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
|              DuckType | 2.1771 ns | 0.0149 ns | 0.0139 ns | 2.1569 ns | 2.2025 ns | 1.000 |    0.00 |     - |     - |     - |         - |
| ExpressionTreeFetcher | 4.4639 ns | 0.0220 ns | 0.0195 ns | 4.4298 ns | 4.5089 ns | 2.050 |    0.02 |     - |     - |     - |         - |
|           EmitFetcher | 4.4590 ns | 0.0237 ns | 0.0210 ns | 4.4104 ns | 4.4850 ns | 2.048 |    0.01 |     - |     - |     - |         - |
|       DelegateFetcher | 6.5749 ns | 0.0366 ns | 0.0325 ns | 6.5349 ns | 6.6446 ns | 3.020 |    0.03 |     - |     - |     - |         - |

### Public Class / Public Property / Getter / Value Type
|                Method |       Mean |     Error |    StdDev |     Median |        Min |        Max | Ratio | RatioSD |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------- |-----------:|----------:|----------:|-----------:|-----------:|-----------:|------:|--------:|-------:|------:|------:|----------:|
|                Direct |  0.0012 ns | 0.0015 ns | 0.0013 ns |  0.0010 ns |  0.0000 ns |  0.0038 ns | 0.001 |    0.00 |      - |     - |     - |         - |
|              DuckType |  1.6618 ns | 0.0110 ns | 0.0097 ns |  1.6619 ns |  1.6434 ns |  1.6832 ns | 1.000 |    0.00 |      - |     - |     - |         - |
| ExpressionTreeFetcher | 12.0046 ns | 0.2628 ns | 0.2699 ns | 12.0642 ns | 11.3202 ns | 12.3705 ns | 7.233 |    0.18 | 0.0014 |     - |     - |      24 B |
|           EmitFetcher | 11.1023 ns | 0.2681 ns | 0.7905 ns | 11.0096 ns |  9.9167 ns | 12.9606 ns | 6.911 |    0.55 | 0.0014 |     - |     - |      24 B |
|       DelegateFetcher | 15.6983 ns | 0.2620 ns | 0.2451 ns | 15.7760 ns | 14.9105 ns | 15.9420 ns | 9.450 |    0.19 | 0.0014 |     - |     - |      24 B |

### Public Class / Public Property / Setter / Object Type
|                Method |      Mean |     Error |    StdDev |       Min |       Max | Ratio | RatioSD | Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------- |----------:|----------:|----------:|----------:|----------:|------:|--------:|------:|------:|------:|----------:|
|                Direct |  1.445 ns | 0.0066 ns | 0.0061 ns |  1.435 ns |  1.454 ns |  0.54 |    0.00 |     - |     - |     - |         - |
|              DuckType |  2.670 ns | 0.0059 ns | 0.0055 ns |  2.663 ns |  2.681 ns |  1.00 |    0.00 |     - |     - |     - |         - |
| ExpressionTreeFetcher |  4.374 ns | 0.0183 ns | 0.0162 ns |  4.345 ns |  4.399 ns |  1.64 |    0.01 |     - |     - |     - |         - |
|           EmitFetcher |  4.480 ns | 0.0746 ns | 0.0697 ns |  4.413 ns |  4.635 ns |  1.68 |    0.03 |     - |     - |     - |         - |
|       DelegateFetcher | 10.975 ns | 0.1836 ns | 0.1533 ns | 10.719 ns | 11.234 ns |  4.11 |    0.06 |     - |     - |     - |         - |

### Public Class / Public Property / Setter / Value Type
|                Method |       Mean |     Error |    StdDev |     Median |        Min |        Max | Ratio | RatioSD |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------- |-----------:|----------:|----------:|-----------:|-----------:|-----------:|------:|--------:|-------:|------:|------:|----------:|
|                Direct |  0.0006 ns | 0.0015 ns | 0.0014 ns |  0.0000 ns |  0.0000 ns |  0.0047 ns | 0.000 |    0.00 |      - |     - |     - |         - |
|              DuckType |  4.0913 ns | 0.1105 ns | 0.2076 ns |  4.1439 ns |  3.3813 ns |  4.3337 ns | 1.000 |    0.00 |      - |     - |     - |         - |
| ExpressionTreeFetcher | 12.5344 ns | 0.3732 ns | 1.1004 ns | 12.7067 ns | 10.0312 ns | 14.0507 ns | 3.035 |    0.34 | 0.0014 |     - |     - |      24 B |
|           EmitFetcher | 12.7996 ns | 0.3630 ns | 1.0703 ns | 13.1127 ns | 10.1776 ns | 14.2688 ns | 3.061 |    0.33 | 0.0014 |     - |     - |      24 B |
|       DelegateFetcher | 14.7395 ns | 0.1736 ns | 0.1624 ns | 14.7218 ns | 14.4874 ns | 15.0738 ns | 3.715 |    0.31 | 0.0014 |     - |     - |      24 B |

### Public Class / Public Field / Getter / Object Type
|                Method |      Mean |     Error |    StdDev |       Min |       Max | Ratio | RatioSD | Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------- |----------:|----------:|----------:|----------:|----------:|------:|--------:|------:|------:|------:|----------:|
|                Direct | 0.0153 ns | 0.0031 ns | 0.0026 ns | 0.0127 ns | 0.0205 ns | 0.009 |    0.00 |     - |     - |     - |         - |
|              DuckType | 1.7270 ns | 0.0384 ns | 0.0340 ns | 1.6797 ns | 1.7787 ns | 1.000 |    0.00 |     - |     - |     - |         - |
| ExpressionTreeFetcher | 4.5179 ns | 0.0631 ns | 0.0560 ns | 4.4148 ns | 4.6047 ns | 2.617 |    0.06 |     - |     - |     - |         - |
|           EmitFetcher | 4.4617 ns | 0.0176 ns | 0.0147 ns | 4.4412 ns | 4.4877 ns | 2.587 |    0.05 |     - |     - |     - |         - |
|       DelegateFetcher |        NA |        NA |        NA |        NA |        NA |     ? |       ? |     - |     - |     - |         - |

### Public Class / Public Field / Getter / Value Type
|                Method |       Mean |     Error |    StdDev |        Min |        Max | Ratio | RatioSD |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------- |-----------:|----------:|----------:|-----------:|-----------:|------:|--------:|-------:|------:|------:|----------:|
|                Direct |  0.0296 ns | 0.0117 ns | 0.0092 ns |  0.0196 ns |  0.0569 ns |  0.02 |    0.01 |      - |     - |     - |         - |
|              DuckType |  1.7358 ns | 0.0218 ns | 0.0204 ns |  1.7067 ns |  1.7824 ns |  1.00 |    0.00 |      - |     - |     - |         - |
| ExpressionTreeFetcher | 11.8694 ns | 0.2632 ns | 0.4813 ns | 10.4145 ns | 12.7367 ns |  6.70 |    0.29 | 0.0014 |     - |     - |      24 B |
|           EmitFetcher | 11.4417 ns | 0.2581 ns | 0.5773 ns | 10.1663 ns | 12.7976 ns |  6.48 |    0.42 | 0.0014 |     - |     - |      24 B |
|       DelegateFetcher |         NA |        NA |        NA |         NA |         NA |     ? |       ? |      - |     - |     - |         - |

### Public Class / Public Field / Setter / Object Type
|                Method |     Mean |     Error |    StdDev |      Min |      Max | Ratio | RatioSD | Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------- |---------:|----------:|----------:|---------:|---------:|------:|--------:|------:|------:|------:|----------:|
|                Direct | 1.196 ns | 0.0085 ns | 0.0071 ns | 1.184 ns | 1.206 ns |  0.38 |    0.03 |     - |     - |     - |         - |
|              DuckType | 3.301 ns | 0.0957 ns | 0.2777 ns | 2.665 ns | 3.848 ns |  1.00 |    0.00 |     - |     - |     - |         - |
| ExpressionTreeFetcher | 4.750 ns | 0.0461 ns | 0.0431 ns | 4.647 ns | 4.795 ns |  1.48 |    0.13 |     - |     - |     - |         - |
|           EmitFetcher | 4.804 ns | 0.0468 ns | 0.0438 ns | 4.678 ns | 4.843 ns |  1.50 |    0.13 |     - |     - |     - |         - |
|       DelegateFetcher |       NA |        NA |        NA |       NA |       NA |     ? |       ? |     - |     - |     - |         - |

### Public Class / Public Field / Setter / Value Type
|                Method |       Mean |     Error |    StdDev |        Min |        Max | Ratio | RatioSD |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------- |-----------:|----------:|----------:|-----------:|-----------:|------:|--------:|-------:|------:|------:|----------:|
|                Direct |  0.0081 ns | 0.0058 ns | 0.0049 ns |  0.0000 ns |  0.0170 ns | 0.002 |    0.00 |      - |     - |     - |         - |
|              DuckType |  4.1982 ns | 0.1131 ns | 0.2179 ns |  3.1635 ns |  4.3023 ns | 1.000 |    0.00 |      - |     - |     - |         - |
| ExpressionTreeFetcher | 10.5569 ns | 0.2295 ns | 0.2356 ns |  9.9981 ns | 10.9312 ns | 2.569 |    0.27 | 0.0014 |     - |     - |      24 B |
|           EmitFetcher | 12.0149 ns | 0.3725 ns | 1.0982 ns | 10.2851 ns | 14.1851 ns | 2.780 |    0.31 | 0.0014 |     - |     - |      24 B |
|       DelegateFetcher |         NA |        NA |        NA |         NA |         NA |     ? |       ? |      - |     - |     - |         - |

### Public Class / Public Method / Invoker
|                Method |       Mean |     Error |    StdDev |        Min |        Max |  Ratio | RatioSD |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------- |-----------:|----------:|----------:|-----------:|-----------:|-------:|--------:|-------:|------:|------:|----------:|
|                Direct |  0.0036 ns | 0.0036 ns | 0.0032 ns |  0.0000 ns |  0.0086 ns |  0.002 |    0.00 |      - |     - |     - |         - |
|              DuckType |  1.9246 ns | 0.0104 ns | 0.0092 ns |  1.9110 ns |  1.9444 ns |  1.000 |    0.00 |      - |     - |     - |         - |
| ExpressionTreeFetcher | 50.0761 ns | 0.6917 ns | 0.6470 ns | 49.0773 ns | 51.3142 ns | 26.046 |    0.35 | 0.0067 |     - |     - |     112 B |
|           EmitFetcher | 49.7584 ns | 0.3779 ns | 0.3350 ns | 49.2297 ns | 50.4590 ns | 25.854 |    0.22 | 0.0067 |     - |     - |     112 B |
|       DelegateFetcher |         NA |        NA |        NA |         NA |         NA |      ? |       ? |      - |     - |     - |         - |


## Powered By
<img src="https://raw.githubusercontent.com/tonyredondo/TWCore2/master/doc/rider.jpg" alt="Rider" width="50px" height="50px" /><img src="https://raw.githubusercontent.com/tonyredondo/TWCore2/master/doc/dotTrace.png" alt="dotTrace" width="50px" height="50px" /><img src="https://raw.githubusercontent.com/tonyredondo/TWCore2/master/doc/dotMemory.png" alt="dotMemory" width="50px" height="50px" />

Thanks to @jetbrains for helping on this development with the licenses for Rider, dotTrace and dotMemory
