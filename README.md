# <img src="https://raw.githubusercontent.com/tonyredondo/ObjectInspector/master/icon.png" alt="Duck" width="45px" height="45px" /> .NET Object Inspector
![GH Actions](https://github.com/tonyredondo/ObjectInspector/workflows/.NET%20Core/badge.svg)
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
BenchmarkDotNet=v0.12.1, OS=Windows 10.0.18363.657 (1909/November2018Update/19H2)
AMD Ryzen Threadripper 2950X, 1 CPU, 32 logical and 16 physical cores
.NET Core SDK=3.1.400-preview-015151
  [Host]     : .NET Core 3.1.2 (CoreCLR 4.700.20.6602, CoreFX 4.700.20.6702), X64 RyuJIT
  DefaultJob : .NET Core 3.1.2 (CoreCLR 4.700.20.6602, CoreFX 4.700.20.6702), X64 RyuJIT
```
### Public Class

#### Public Property / Getter / Object Type
|                Method |       Mean |     Error |    StdDev |     Median |        Min |        Max |  Ratio | RatioSD | Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------- |-----------:|----------:|----------:|-----------:|-----------:|-----------:|-------:|--------:|------:|------:|------:|----------:|
|                Direct |  0.0161 ns | 0.0290 ns | 0.0257 ns |  0.0035 ns |  0.0000 ns |  0.0728 ns |  0.007 |    0.01 |     - |     - |     - |         - |
|              DuckType |  2.2125 ns | 0.0046 ns | 0.0043 ns |  2.2116 ns |  2.2055 ns |  2.2202 ns |  1.000 |    0.00 |     - |     - |     - |         - |
| ExpressionTreeFetcher |  3.4792 ns | 0.0302 ns | 0.0283 ns |  3.4853 ns |  3.4342 ns |  3.5224 ns |  1.573 |    0.01 |     - |     - |     - |         - |
|           EmitFetcher |  3.6111 ns | 0.0906 ns | 0.0848 ns |  3.6054 ns |  3.4889 ns |  3.7224 ns |  1.632 |    0.04 |     - |     - |     - |         - |
|       DelegateFetcher |  4.9922 ns | 0.0385 ns | 0.0360 ns |  5.0016 ns |  4.9133 ns |  5.0446 ns |  2.256 |    0.02 |     - |     - |     - |         - |
|            Reflection | 90.5706 ns | 0.1098 ns | 0.0974 ns | 90.5540 ns | 90.3597 ns | 90.7462 ns | 40.927 |    0.08 |     - |     - |     - |         - |

#### Public Property / Getter / Value Type
|                Method |        Mean |     Error |    StdDev |      Median |         Min |         Max |  Ratio | RatioSD |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------- |------------:|----------:|----------:|------------:|------------:|------------:|-------:|--------:|-------:|------:|------:|----------:|
|                Direct |   0.0003 ns | 0.0008 ns | 0.0007 ns |   0.0000 ns |   0.0000 ns |   0.0021 ns |  0.000 |    0.00 |      - |     - |     - |         - |
|              DuckType |   1.7231 ns | 0.0030 ns | 0.0026 ns |   1.7235 ns |   1.7188 ns |   1.7264 ns |  1.000 |    0.00 |      - |     - |     - |         - |
| ExpressionTreeFetcher |   7.3774 ns | 0.0181 ns | 0.0161 ns |   7.3738 ns |   7.3532 ns |   7.4054 ns |  4.281 |    0.01 | 0.0057 |     - |     - |      24 B |
|           EmitFetcher |   7.7896 ns | 0.1194 ns | 0.2154 ns |   7.6887 ns |   7.3799 ns |   8.6753 ns |  4.512 |    0.19 | 0.0057 |     - |     - |      24 B |
|       DelegateFetcher |   7.5427 ns | 0.0436 ns | 0.0386 ns |   7.5405 ns |   7.4823 ns |   7.6281 ns |  4.377 |    0.02 | 0.0057 |     - |     - |      24 B |
|            Reflection | 123.6325 ns | 0.7970 ns | 0.7455 ns | 123.4649 ns | 122.7538 ns | 125.1706 ns | 71.754 |    0.44 | 0.0057 |     - |     - |      24 B |

#### Public Property / Setter / Object Type
|                Method |       Mean |     Error |    StdDev |        Min |        Max | Ratio | RatioSD |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------- |-----------:|----------:|----------:|-----------:|-----------:|------:|--------:|-------:|------:|------:|----------:|
|                Direct |   1.469 ns | 0.0095 ns | 0.0084 ns |   1.455 ns |   1.487 ns |  0.54 |    0.00 |      - |     - |     - |         - |
|              DuckType |   2.711 ns | 0.0050 ns | 0.0044 ns |   2.705 ns |   2.718 ns |  1.00 |    0.00 |      - |     - |     - |         - |
| ExpressionTreeFetcher |   4.928 ns | 0.0594 ns | 0.0555 ns |   4.827 ns |   5.006 ns |  1.82 |    0.02 |      - |     - |     - |         - |
|           EmitFetcher |   4.925 ns | 0.0780 ns | 0.0730 ns |   4.805 ns |   5.029 ns |  1.82 |    0.02 |      - |     - |     - |         - |
|       DelegateFetcher |   5.987 ns | 0.1030 ns | 0.0963 ns |   5.757 ns |   6.129 ns |  2.21 |    0.04 |      - |     - |     - |         - |
|            Reflection | 163.913 ns | 1.6321 ns | 1.5267 ns | 161.401 ns | 166.120 ns | 60.46 |    0.58 | 0.0153 |     - |     - |      64 B |

#### Public Property / Setter / Value Type
|                Method |        Mean |     Error |    StdDev |      Median |         Min |         Max |  Ratio | RatioSD |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------- |------------:|----------:|----------:|------------:|------------:|------------:|-------:|--------:|-------:|------:|------:|----------:|
|                Direct |   0.0016 ns | 0.0031 ns | 0.0029 ns |   0.0000 ns |   0.0000 ns |   0.0093 ns |  0.000 |    0.00 |      - |     - |     - |         - |
|              DuckType |   3.9491 ns | 0.0387 ns | 0.0343 ns |   3.9541 ns |   3.8982 ns |   3.9997 ns |  1.000 |    0.00 |      - |     - |     - |         - |
| ExpressionTreeFetcher |   7.2476 ns | 0.1542 ns | 0.1442 ns |   7.2403 ns |   6.9978 ns |   7.4962 ns |  1.831 |    0.04 | 0.0057 |     - |     - |      24 B |
|           EmitFetcher |   6.9371 ns | 0.0407 ns | 0.0381 ns |   6.9350 ns |   6.8701 ns |   6.9919 ns |  1.757 |    0.02 | 0.0057 |     - |     - |      24 B |
|       DelegateFetcher |   7.5060 ns | 0.0535 ns | 0.0474 ns |   7.5057 ns |   7.3920 ns |   7.5812 ns |  1.901 |    0.02 | 0.0057 |     - |     - |      24 B |
|            Reflection | 175.6977 ns | 0.7946 ns | 0.7432 ns | 175.8352 ns | 174.4911 ns | 176.6142 ns | 44.477 |    0.42 | 0.0210 |     - |     - |      88 B |

#### Public Field / Getter / Object Type
|                Method |       Mean |     Error |    StdDev |        Min |        Max |  Ratio | RatioSD | Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------- |-----------:|----------:|----------:|-----------:|-----------:|-------:|--------:|------:|------:|------:|----------:|
|                Direct |  0.0075 ns | 0.0116 ns | 0.0091 ns |  0.0000 ns |  0.0331 ns |  0.004 |    0.01 |     - |     - |     - |         - |
|              DuckType |  1.7084 ns | 0.0072 ns | 0.0067 ns |  1.6884 ns |  1.7172 ns |  1.000 |    0.00 |     - |     - |     - |         - |
| ExpressionTreeFetcher |  3.5019 ns | 0.0438 ns | 0.0388 ns |  3.4391 ns |  3.5804 ns |  2.050 |    0.02 |     - |     - |     - |         - |
|           EmitFetcher |  3.5379 ns | 0.0464 ns | 0.0434 ns |  3.4572 ns |  3.6206 ns |  2.071 |    0.03 |     - |     - |     - |         - |
|       DelegateFetcher |         NA |        NA |        NA |         NA |         NA |      ? |       ? |     - |     - |     - |         - |
|            Reflection | 39.6455 ns | 0.2205 ns | 0.2062 ns | 39.3000 ns | 40.0471 ns | 23.207 |    0.16 |     - |     - |     - |         - |

#### Public Field / Getter / Value Type
|                Method |       Mean |     Error |    StdDev |     Median |        Min |        Max |  Ratio | RatioSD |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------- |-----------:|----------:|----------:|-----------:|-----------:|-----------:|-------:|--------:|-------:|------:|------:|----------:|
|                Direct |  0.0006 ns | 0.0010 ns | 0.0009 ns |  0.0000 ns |  0.0000 ns |  0.0025 ns |  0.000 |    0.00 |      - |     - |     - |         - |
|              DuckType |  1.7133 ns | 0.0018 ns | 0.0017 ns |  1.7135 ns |  1.7104 ns |  1.7162 ns |  1.000 |    0.00 |      - |     - |     - |         - |
| ExpressionTreeFetcher |  7.1653 ns | 0.0382 ns | 0.0357 ns |  7.1446 ns |  7.1205 ns |  7.2369 ns |  4.182 |    0.02 | 0.0057 |     - |     - |      24 B |
|           EmitFetcher |  7.1751 ns | 0.0155 ns | 0.0130 ns |  7.1759 ns |  7.1516 ns |  7.1999 ns |  4.188 |    0.01 | 0.0057 |     - |     - |      24 B |
|       DelegateFetcher |         NA |        NA |        NA |         NA |         NA |         NA |      ? |       ? |      - |     - |     - |         - |
|            Reflection | 51.1339 ns | 0.3108 ns | 0.2907 ns | 51.1279 ns | 50.6730 ns | 51.6586 ns | 29.844 |    0.17 | 0.0057 |     - |     - |      24 B |

#### Public Field / Setter / Object Type
|                Method |      Mean |     Error |    StdDev |       Min |       Max | Ratio | RatioSD | Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------- |----------:|----------:|----------:|----------:|----------:|------:|--------:|------:|------:|------:|----------:|
|                Direct |  1.232 ns | 0.0028 ns | 0.0026 ns |  1.226 ns |  1.235 ns |  0.50 |    0.00 |     - |     - |     - |         - |
|              DuckType |  2.462 ns | 0.0066 ns | 0.0058 ns |  2.453 ns |  2.474 ns |  1.00 |    0.00 |     - |     - |     - |         - |
| ExpressionTreeFetcher |  4.775 ns | 0.0429 ns | 0.0401 ns |  4.687 ns |  4.848 ns |  1.94 |    0.02 |     - |     - |     - |         - |
|           EmitFetcher |  4.803 ns | 0.0970 ns | 0.0908 ns |  4.650 ns |  5.010 ns |  1.95 |    0.04 |     - |     - |     - |         - |
|       DelegateFetcher |        NA |        NA |        NA |        NA |        NA |     ? |       ? |     - |     - |     - |         - |
|            Reflection | 60.918 ns | 0.0491 ns | 0.0459 ns | 60.830 ns | 60.995 ns | 24.75 |    0.07 |     - |     - |     - |         - |

#### Public Field / Setter / Value Type
|                Method |       Mean |     Error |    StdDev |        Min |        Max |  Ratio | RatioSD |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------- |-----------:|----------:|----------:|-----------:|-----------:|-------:|--------:|-------:|------:|------:|----------:|
|                Direct |  0.0016 ns | 0.0012 ns | 0.0010 ns |  0.0000 ns |  0.0030 ns |  0.000 |    0.00 |      - |     - |     - |         - |
|              DuckType |  3.8448 ns | 0.0032 ns | 0.0030 ns |  3.8408 ns |  3.8496 ns |  1.000 |    0.00 |      - |     - |     - |         - |
| ExpressionTreeFetcher |  7.2409 ns | 0.0835 ns | 0.0781 ns |  7.0971 ns |  7.3457 ns |  1.883 |    0.02 | 0.0057 |     - |     - |      24 B |
|           EmitFetcher |  6.7437 ns | 0.0542 ns | 0.0453 ns |  6.6875 ns |  6.8413 ns |  1.754 |    0.01 | 0.0057 |     - |     - |      24 B |
|       DelegateFetcher |         NA |        NA |        NA |         NA |         NA |      ? |       ? |      - |     - |     - |         - |
|            Reflection | 67.3292 ns | 0.5001 ns | 0.4678 ns | 66.6080 ns | 68.0917 ns | 17.512 |    0.11 | 0.0057 |     - |     - |      24 B |


#### Public Method / Invoker
|                Method |        Mean |     Error |    StdDev |      Median |         Min |         Max |   Ratio | RatioSD |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------- |------------:|----------:|----------:|------------:|------------:|------------:|--------:|--------:|-------:|------:|------:|----------:|
|                Direct |   0.0003 ns | 0.0009 ns | 0.0007 ns |   0.0000 ns |   0.0000 ns |   0.0024 ns |   0.000 |    0.00 |      - |     - |     - |         - |
|              DuckType |   1.7245 ns | 0.0023 ns | 0.0022 ns |   1.7244 ns |   1.7215 ns |   1.7292 ns |   1.000 |    0.00 |      - |     - |     - |         - |
| ExpressionTreeFetcher |  28.7078 ns | 0.4197 ns | 0.3926 ns |  28.7469 ns |  28.1677 ns |  29.4684 ns |  16.647 |    0.23 | 0.0268 |     - |     - |     112 B |
|           EmitFetcher |  27.2764 ns | 0.3193 ns | 0.2830 ns |  27.3490 ns |  26.7955 ns |  27.6619 ns |  15.817 |    0.16 | 0.0268 |     - |     - |     112 B |
|       DelegateFetcher |          NA |        NA |        NA |          NA |          NA |          NA |       ? |       ? |      - |     - |     - |         - |
|            Reflection | 235.0135 ns | 0.5114 ns | 0.4533 ns | 234.8543 ns | 234.3358 ns | 235.7639 ns | 136.278 |    0.34 | 0.0362 |     - |     - |     152 B |

### Private Class

#### Private Property / Getter / Object Type
|                Method |      Mean |     Error |    StdDev |       Min |       Max | Ratio | RatioSD | Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------- |----------:|----------:|----------:|----------:|----------:|------:|--------:|------:|------:|------:|----------:|
|                Direct |        NA |        NA |        NA |        NA |        NA |     ? |       ? |     - |     - |     - |         - |
|              DuckType |  2.949 ns | 0.0032 ns | 0.0026 ns |  2.945 ns |  2.955 ns |  1.00 |    0.00 |     - |     - |     - |         - |
| ExpressionTreeFetcher |  3.784 ns | 0.1023 ns | 0.1294 ns |  3.454 ns |  3.937 ns |  1.28 |    0.04 |     - |     - |     - |         - |
|           EmitFetcher |  3.522 ns | 0.0822 ns | 0.0769 ns |  3.392 ns |  3.667 ns |  1.20 |    0.02 |     - |     - |     - |         - |
|       DelegateFetcher |  5.281 ns | 0.0480 ns | 0.0449 ns |  5.167 ns |  5.346 ns |  1.79 |    0.01 |     - |     - |     - |         - |
|            Reflection | 94.705 ns | 0.4683 ns | 0.4152 ns | 94.260 ns | 95.391 ns | 32.10 |    0.14 |     - |     - |     - |         - |

#### Private Property / Getter / Value Type
|                Method |       Mean |     Error |    StdDev |        Min |        Max | Ratio | RatioSD |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------- |-----------:|----------:|----------:|-----------:|-----------:|------:|--------:|-------:|------:|------:|----------:|
|                Direct |         NA |        NA |        NA |         NA |         NA |     ? |       ? |      - |     - |     - |         - |
|              DuckType |   2.688 ns | 0.0141 ns | 0.0132 ns |   2.663 ns |   2.708 ns |  1.00 |    0.00 |      - |     - |     - |         - |
| ExpressionTreeFetcher |   7.151 ns | 0.0669 ns | 0.0593 ns |   7.078 ns |   7.220 ns |  2.66 |    0.02 | 0.0057 |     - |     - |      24 B |
|           EmitFetcher |   7.057 ns | 0.0222 ns | 0.0197 ns |   7.021 ns |   7.090 ns |  2.62 |    0.01 | 0.0057 |     - |     - |      24 B |
|       DelegateFetcher |   7.673 ns | 0.1596 ns | 0.1493 ns |   7.460 ns |   7.956 ns |  2.85 |    0.05 | 0.0057 |     - |     - |      24 B |
|            Reflection | 126.631 ns | 0.3470 ns | 0.3076 ns | 126.076 ns | 127.139 ns | 47.09 |    0.21 | 0.0057 |     - |     - |      24 B |

#### Private Property / Setter / Object Type
|                Method |       Mean |     Error |    StdDev |        Min |        Max | Ratio | RatioSD |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------- |-----------:|----------:|----------:|-----------:|-----------:|------:|--------:|-------:|------:|------:|----------:|
|                Direct |         NA |        NA |        NA |         NA |         NA |     ? |       ? |      - |     - |     - |         - |
|              DuckType |   3.936 ns | 0.0045 ns | 0.0040 ns |   3.930 ns |   3.945 ns |  1.00 |    0.00 |      - |     - |     - |         - |
| ExpressionTreeFetcher |   5.060 ns | 0.1229 ns | 0.1090 ns |   4.744 ns |   5.180 ns |  1.29 |    0.03 |      - |     - |     - |         - |
|           EmitFetcher |   4.893 ns | 0.0392 ns | 0.0366 ns |   4.830 ns |   4.960 ns |  1.24 |    0.01 |      - |     - |     - |         - |
|       DelegateFetcher |   5.931 ns | 0.0732 ns | 0.0685 ns |   5.842 ns |   6.077 ns |  1.51 |    0.02 |      - |     - |     - |         - |
|            Reflection | 165.955 ns | 0.6392 ns | 0.5667 ns | 164.564 ns | 166.636 ns | 42.16 |    0.15 | 0.0153 |     - |     - |      64 B |

#### Private Property / Setter / Value Type
|                Method |       Mean |     Error |    StdDev |        Min |        Max | Ratio | RatioSD |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------- |-----------:|----------:|----------:|-----------:|-----------:|------:|--------:|-------:|------:|------:|----------:|
|                Direct |         NA |        NA |        NA |         NA |         NA |     ? |       ? |      - |     - |     - |         - |
|              DuckType |   5.439 ns | 0.1307 ns | 0.1342 ns |   5.236 ns |   5.659 ns |  1.00 |    0.00 | 0.0057 |     - |     - |      24 B |
| ExpressionTreeFetcher |   6.998 ns | 0.0769 ns | 0.0720 ns |   6.927 ns |   7.107 ns |  1.29 |    0.03 | 0.0057 |     - |     - |      24 B |
|           EmitFetcher |   6.738 ns | 0.0559 ns | 0.0496 ns |   6.668 ns |   6.831 ns |  1.24 |    0.03 | 0.0057 |     - |     - |      24 B |
|       DelegateFetcher |   6.936 ns | 0.0384 ns | 0.0359 ns |   6.846 ns |   6.988 ns |  1.27 |    0.04 | 0.0057 |     - |     - |      24 B |
|            Reflection | 176.797 ns | 0.8537 ns | 0.7985 ns | 175.256 ns | 178.369 ns | 32.49 |    0.88 | 0.0210 |     - |     - |      88 B |

#### Private Field / Getter / Object Type
|                Method |      Mean |     Error |    StdDev |       Min |       Max | Ratio | RatioSD | Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------- |----------:|----------:|----------:|----------:|----------:|------:|--------:|------:|------:|------:|----------:|
|                Direct |        NA |        NA |        NA |        NA |        NA |     ? |       ? |     - |     - |     - |         - |
|              DuckType |  2.949 ns | 0.0143 ns | 0.0134 ns |  2.919 ns |  2.967 ns |  1.00 |    0.00 |     - |     - |     - |         - |
| ExpressionTreeFetcher |  3.123 ns | 0.0226 ns | 0.0212 ns |  3.083 ns |  3.168 ns |  1.06 |    0.01 |     - |     - |     - |         - |
|           EmitFetcher |  3.681 ns | 0.0812 ns | 0.0759 ns |  3.436 ns |  3.746 ns |  1.25 |    0.03 |     - |     - |     - |         - |
|       DelegateFetcher |        NA |        NA |        NA |        NA |        NA |     ? |       ? |     - |     - |     - |         - |
|            Reflection | 37.283 ns | 0.3230 ns | 0.2863 ns | 36.818 ns | 37.816 ns | 12.64 |    0.10 |     - |     - |     - |         - |

#### Private Field / Getter / Value Type
|                Method |      Mean |     Error |    StdDev |       Min |       Max | Ratio | RatioSD |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------- |----------:|----------:|----------:|----------:|----------:|------:|--------:|-------:|------:|------:|----------:|
|                Direct |        NA |        NA |        NA |        NA |        NA |     ? |       ? |      - |     - |     - |         - |
|              DuckType |  2.724 ns | 0.0049 ns | 0.0046 ns |  2.717 ns |  2.731 ns |  1.00 |    0.00 |      - |     - |     - |         - |
| ExpressionTreeFetcher |  7.579 ns | 0.0468 ns | 0.0438 ns |  7.510 ns |  7.641 ns |  2.78 |    0.02 | 0.0057 |     - |     - |      24 B |
|           EmitFetcher |  6.980 ns | 0.0474 ns | 0.0420 ns |  6.909 ns |  7.047 ns |  2.56 |    0.02 | 0.0057 |     - |     - |      24 B |
|       DelegateFetcher |        NA |        NA |        NA |        NA |        NA |     ? |       ? |      - |     - |     - |         - |
|            Reflection | 51.788 ns | 0.2470 ns | 0.2062 ns | 51.502 ns | 52.116 ns | 19.01 |    0.08 | 0.0057 |     - |     - |      24 B |

#### Private Field / Setter / Object Type
|                Method |      Mean |     Error |    StdDev |       Min |       Max | Ratio | RatioSD | Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------- |----------:|----------:|----------:|----------:|----------:|------:|--------:|------:|------:|------:|----------:|
|                Direct |        NA |        NA |        NA |        NA |        NA |     ? |       ? |     - |     - |     - |         - |
|              DuckType |  3.932 ns | 0.0045 ns | 0.0042 ns |  3.925 ns |  3.940 ns |  1.00 |    0.00 |     - |     - |     - |         - |
| ExpressionTreeFetcher |  4.415 ns | 0.0488 ns | 0.0381 ns |  4.378 ns |  4.494 ns |  1.12 |    0.01 |     - |     - |     - |         - |
|           EmitFetcher |  4.431 ns | 0.0082 ns | 0.0077 ns |  4.423 ns |  4.443 ns |  1.13 |    0.00 |     - |     - |     - |         - |
|       DelegateFetcher |        NA |        NA |        NA |        NA |        NA |     ? |       ? |     - |     - |     - |         - |
|            Reflection | 56.062 ns | 0.2700 ns | 0.2394 ns | 55.334 ns | 56.319 ns | 14.26 |    0.05 |     - |     - |     - |         - |

#### Private Field / Setter / Value Type
|                Method |      Mean |     Error |    StdDev |       Min |       Max | Ratio | RatioSD |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------- |----------:|----------:|----------:|----------:|----------:|------:|--------:|-------:|------:|------:|----------:|
|                Direct |        NA |        NA |        NA |        NA |        NA |     ? |       ? |      - |     - |     - |         - |
|              DuckType |  2.473 ns | 0.0154 ns | 0.0137 ns |  2.453 ns |  2.495 ns |  1.00 |    0.00 |      - |     - |     - |         - |
| ExpressionTreeFetcher |  6.947 ns | 0.1329 ns | 0.1178 ns |  6.839 ns |  7.236 ns |  2.81 |    0.05 | 0.0057 |     - |     - |      24 B |
|           EmitFetcher |  6.877 ns | 0.0339 ns | 0.0301 ns |  6.838 ns |  6.933 ns |  2.78 |    0.02 | 0.0057 |     - |     - |      24 B |
|       DelegateFetcher |        NA |        NA |        NA |        NA |        NA |     ? |       ? |      - |     - |     - |         - |
|            Reflection | 69.359 ns | 0.2287 ns | 0.2139 ns | 69.042 ns | 69.661 ns | 28.05 |    0.20 | 0.0057 |     - |     - |      24 B |

#### Private Method / Invoker
|                Method |      Mean |    Error |   StdDev |       Min |       Max | Ratio | RatioSD |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------- |----------:|---------:|---------:|----------:|----------:|------:|--------:|-------:|------:|------:|----------:|
|                Direct |        NA |       NA |       NA |        NA |        NA |     ? |       ? |      - |     - |     - |         - |
|              DuckType |  35.37 ns | 0.357 ns | 0.334 ns |  34.68 ns |  35.98 ns |  1.00 |    0.00 | 0.0268 |     - |     - |     112 B |
| ExpressionTreeFetcher |  28.61 ns | 0.269 ns | 0.210 ns |  28.26 ns |  28.85 ns |  0.81 |    0.01 | 0.0268 |     - |     - |     112 B |
|           EmitFetcher |  27.68 ns | 0.579 ns | 0.968 ns |  26.15 ns |  29.06 ns |  0.79 |    0.03 | 0.0268 |     - |     - |     112 B |
|       DelegateFetcher |        NA |       NA |       NA |        NA |        NA |     ? |       ? |      - |     - |     - |         - |
|            Reflection | 241.35 ns | 1.475 ns | 1.380 ns | 238.81 ns | 243.54 ns |  6.82 |    0.06 | 0.0362 |     - |     - |     152 B |


## Powered By
<img src="https://raw.githubusercontent.com/tonyredondo/TWCore2/master/doc/rider.jpg" alt="Rider" width="50px" height="50px" /><img src="https://raw.githubusercontent.com/tonyredondo/TWCore2/master/doc/dotTrace.png" alt="dotTrace" width="50px" height="50px" /><img src="https://raw.githubusercontent.com/tonyredondo/TWCore2/master/doc/dotMemory.png" alt="dotMemory" width="50px" height="50px" />

Thanks to @jetbrains for helping on this development with the licenses for Rider, dotTrace and dotMemory
