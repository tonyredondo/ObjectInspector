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
|                Method |       Mean |     Error |    StdDev |        Min |        Max |  Ratio | RatioSD | Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------- |-----------:|----------:|----------:|-----------:|-----------:|-------:|--------:|------:|------:|------:|----------:|
|                Direct |  0.0049 ns | 0.0044 ns | 0.0041 ns |  0.0000 ns |  0.0115 ns |  0.002 |    0.00 |     - |     - |     - |         - |
|              DuckType |  2.1896 ns | 0.0149 ns | 0.0139 ns |  2.1626 ns |  2.2004 ns |  1.000 |    0.00 |     - |     - |     - |         - |
| ExpressionTreeFetcher |  3.4983 ns | 0.0392 ns | 0.0348 ns |  3.4502 ns |  3.5663 ns |  1.598 |    0.02 |     - |     - |     - |         - |
|           EmitFetcher |  3.5037 ns | 0.0707 ns | 0.0661 ns |  3.4087 ns |  3.6137 ns |  1.600 |    0.03 |     - |     - |     - |         - |
|       DelegateFetcher |  5.8822 ns | 0.0184 ns | 0.0163 ns |  5.8655 ns |  5.9208 ns |  2.687 |    0.02 |     - |     - |     - |         - |
|            Reflection | 90.1075 ns | 0.1937 ns | 0.1618 ns | 89.8911 ns | 90.4457 ns | 41.175 |    0.28 |     - |     - |     - |         - |

#### Public Property / Getter / Value Type
|                Method |        Mean |     Error |    StdDev |      Median |         Min |         Max |  Ratio | RatioSD |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------- |------------:|----------:|----------:|------------:|------------:|------------:|-------:|--------:|-------:|------:|------:|----------:|
|                Direct |   0.0005 ns | 0.0008 ns | 0.0008 ns |   0.0000 ns |   0.0000 ns |   0.0028 ns |  0.000 |    0.00 |      - |     - |     - |         - |
|              DuckType |   1.7120 ns | 0.0065 ns | 0.0061 ns |   1.7122 ns |   1.6916 ns |   1.7171 ns |  1.000 |    0.00 |      - |     - |     - |         - |
| ExpressionTreeFetcher |   7.5391 ns | 0.0117 ns | 0.0098 ns |   7.5400 ns |   7.5201 ns |   7.5555 ns |  4.403 |    0.02 | 0.0057 |     - |     - |      24 B |
|           EmitFetcher |   7.3048 ns | 0.0376 ns | 0.0352 ns |   7.2924 ns |   7.2576 ns |   7.3758 ns |  4.267 |    0.03 | 0.0057 |     - |     - |      24 B |
|       DelegateFetcher |   7.5995 ns | 0.0476 ns | 0.0445 ns |   7.6058 ns |   7.5208 ns |   7.6810 ns |  4.439 |    0.03 | 0.0057 |     - |     - |      24 B |
|            Reflection | 122.6644 ns | 0.5362 ns | 0.5016 ns | 122.8011 ns | 121.2207 ns | 123.2138 ns | 71.649 |    0.39 | 0.0057 |     - |     - |      24 B |

#### Public Property / Setter / Object Type
|                Method |       Mean |     Error |    StdDev |        Min |        Max | Ratio | RatioSD |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------- |-----------:|----------:|----------:|-----------:|-----------:|------:|--------:|-------:|------:|------:|----------:|
|                Direct |   1.460 ns | 0.0141 ns | 0.0132 ns |   1.439 ns |   1.473 ns |  0.54 |    0.00 |      - |     - |     - |         - |
|              DuckType |   2.691 ns | 0.0036 ns | 0.0032 ns |   2.688 ns |   2.698 ns |  1.00 |    0.00 |      - |     - |     - |         - |
| ExpressionTreeFetcher |   4.880 ns | 0.0286 ns | 0.0253 ns |   4.822 ns |   4.918 ns |  1.81 |    0.01 |      - |     - |     - |         - |
|           EmitFetcher |   4.892 ns | 0.0312 ns | 0.0292 ns |   4.849 ns |   4.947 ns |  1.82 |    0.01 |      - |     - |     - |         - |
|       DelegateFetcher |   6.613 ns | 0.0117 ns | 0.0110 ns |   6.599 ns |   6.639 ns |  2.46 |    0.00 |      - |     - |     - |         - |
|            Reflection | 154.357 ns | 1.7632 ns | 1.6493 ns | 152.475 ns | 157.291 ns | 57.34 |    0.62 | 0.0153 |     - |     - |      64 B |

#### Public Property / Setter / Value Type
|                Method |        Mean |     Error |    StdDev |      Median |         Min |         Max |  Ratio | RatioSD |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------- |------------:|----------:|----------:|------------:|------------:|------------:|-------:|--------:|-------:|------:|------:|----------:|
|                Direct |   0.0008 ns | 0.0018 ns | 0.0016 ns |   0.0000 ns |   0.0000 ns |   0.0044 ns |  0.000 |    0.00 |      - |     - |     - |         - |
|              DuckType |   3.8312 ns | 0.0068 ns | 0.0063 ns |   3.8308 ns |   3.8197 ns |   3.8428 ns |  1.000 |    0.00 |      - |     - |     - |         - |
| ExpressionTreeFetcher |   6.7028 ns | 0.0082 ns | 0.0072 ns |   6.7000 ns |   6.6934 ns |   6.7169 ns |  1.750 |    0.00 | 0.0057 |     - |     - |      24 B |
|           EmitFetcher |   7.0991 ns | 0.0723 ns | 0.0677 ns |   7.0896 ns |   6.9878 ns |   7.2077 ns |  1.853 |    0.02 | 0.0057 |     - |     - |      24 B |
|       DelegateFetcher |   7.1269 ns | 0.0322 ns | 0.0301 ns |   7.1346 ns |   7.0702 ns |   7.1778 ns |  1.860 |    0.01 | 0.0057 |     - |     - |      24 B |
|            Reflection | 168.2425 ns | 0.8112 ns | 0.7588 ns | 168.2032 ns | 167.2194 ns | 169.8516 ns | 43.914 |    0.21 | 0.0210 |     - |     - |      88 B |

#### Public Field / Getter / Object Type
|                Method |       Mean |     Error |    StdDev |        Min |        Max |  Ratio | RatioSD | Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------- |-----------:|----------:|----------:|-----------:|-----------:|-------:|--------:|------:|------:|------:|----------:|
|                Direct |  0.0005 ns | 0.0006 ns | 0.0005 ns |  0.0000 ns |  0.0016 ns |  0.000 |    0.00 |     - |     - |     - |         - |
|              DuckType |  2.2046 ns | 0.0081 ns | 0.0068 ns |  2.1967 ns |  2.2199 ns |  1.000 |    0.00 |     - |     - |     - |         - |
| ExpressionTreeFetcher |  3.5455 ns | 0.0235 ns | 0.0208 ns |  3.5034 ns |  3.5790 ns |  1.608 |    0.01 |     - |     - |     - |         - |
|           EmitFetcher |  3.5215 ns | 0.0999 ns | 0.0935 ns |  3.3968 ns |  3.7083 ns |  1.590 |    0.04 |     - |     - |     - |         - |
|       DelegateFetcher |         NA |        NA |        NA |         NA |         NA |      ? |       ? |     - |     - |     - |         - |
|            Reflection | 38.8769 ns | 0.3901 ns | 0.3258 ns | 38.1393 ns | 39.3205 ns | 17.635 |    0.16 |     - |     - |     - |         - |

#### Public Field / Getter / Value Type
|                Method |       Mean |     Error |    StdDev |     Median |        Min |        Max |  Ratio | RatioSD |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------- |-----------:|----------:|----------:|-----------:|-----------:|-----------:|-------:|--------:|-------:|------:|------:|----------:|
|                Direct |  0.0003 ns | 0.0005 ns | 0.0005 ns |  0.0000 ns |  0.0000 ns |  0.0015 ns |  0.000 |    0.00 |      - |     - |     - |         - |
|              DuckType |  2.1954 ns | 0.0025 ns | 0.0021 ns |  2.1957 ns |  2.1924 ns |  2.1998 ns |  1.000 |    0.00 |      - |     - |     - |         - |
| ExpressionTreeFetcher |  6.5630 ns | 0.1004 ns | 0.0939 ns |  6.5700 ns |  6.3781 ns |  6.7005 ns |  2.993 |    0.04 | 0.0057 |     - |     - |      24 B |
|           EmitFetcher |  6.5225 ns | 0.1580 ns | 0.1622 ns |  6.6015 ns |  6.3197 ns |  6.7179 ns |  2.997 |    0.06 | 0.0057 |     - |     - |      24 B |
|       DelegateFetcher |         NA |        NA |        NA |         NA |         NA |         NA |      ? |       ? |      - |     - |     - |         - |
|            Reflection | 55.5570 ns | 1.0717 ns | 1.0025 ns | 55.6185 ns | 53.7931 ns | 57.3727 ns | 25.270 |    0.47 | 0.0057 |     - |     - |      24 B |

#### Public Field / Setter / Object Type
|                Method |      Mean |     Error |    StdDev |       Min |       Max | Ratio | RatioSD | Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------- |----------:|----------:|----------:|----------:|----------:|------:|--------:|------:|------:|------:|----------:|
|                Direct |  1.217 ns | 0.0016 ns | 0.0014 ns |  1.215 ns |  1.219 ns |  0.45 |    0.00 |     - |     - |     - |         - |
|              DuckType |  2.680 ns | 0.0151 ns | 0.0141 ns |  2.650 ns |  2.696 ns |  1.00 |    0.00 |     - |     - |     - |         - |
| ExpressionTreeFetcher |  4.858 ns | 0.0567 ns | 0.0530 ns |  4.743 ns |  4.910 ns |  1.81 |    0.02 |     - |     - |     - |         - |
|           EmitFetcher |  4.834 ns | 0.0792 ns | 0.0741 ns |  4.707 ns |  4.951 ns |  1.80 |    0.03 |     - |     - |     - |         - |
|       DelegateFetcher |        NA |        NA |        NA |        NA |        NA |     ? |       ? |     - |     - |     - |         - |
|            Reflection | 59.740 ns | 0.1097 ns | 0.0972 ns | 59.607 ns | 59.947 ns | 22.30 |    0.13 |     - |     - |     - |         - |

#### Public Field / Setter / Value Type
|                Method |       Mean |     Error |    StdDev |     Median |        Min |        Max |  Ratio | RatioSD |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------- |-----------:|----------:|----------:|-----------:|-----------:|-----------:|-------:|--------:|-------:|------:|------:|----------:|
|                Direct |  0.0004 ns | 0.0006 ns | 0.0005 ns |  0.0000 ns |  0.0000 ns |  0.0014 ns |  0.000 |    0.00 |      - |     - |     - |         - |
|              DuckType |  3.8100 ns | 0.0082 ns | 0.0076 ns |  3.8078 ns |  3.8003 ns |  3.8217 ns |  1.000 |    0.00 |      - |     - |     - |         - |
| ExpressionTreeFetcher |  6.6940 ns | 0.1197 ns | 0.1120 ns |  6.6186 ns |  6.5973 ns |  6.8862 ns |  1.757 |    0.03 | 0.0057 |     - |     - |      24 B |
|           EmitFetcher |  6.6951 ns | 0.0123 ns | 0.0115 ns |  6.6924 ns |  6.6803 ns |  6.7218 ns |  1.757 |    0.00 | 0.0057 |     - |     - |      24 B |
|       DelegateFetcher |         NA |        NA |        NA |         NA |         NA |         NA |      ? |       ? |      - |     - |     - |         - |
|            Reflection | 66.9361 ns | 0.1540 ns | 0.1286 ns | 66.9208 ns | 66.8001 ns | 67.2204 ns | 17.568 |    0.05 | 0.0057 |     - |     - |      24 B |

#### Public Method / Invoker
|                Method |        Mean |     Error |    StdDev |      Median |         Min |         Max |   Ratio | RatioSD |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------- |------------:|----------:|----------:|------------:|------------:|------------:|--------:|--------:|-------:|------:|------:|----------:|
|                Direct |   0.0002 ns | 0.0004 ns | 0.0003 ns |   0.0000 ns |   0.0000 ns |   0.0011 ns |   0.000 |    0.00 |      - |     - |     - |         - |
|              DuckType |   2.1958 ns | 0.0025 ns | 0.0024 ns |   2.1954 ns |   2.1926 ns |   2.1995 ns |   1.000 |    0.00 |      - |     - |     - |         - |
| ExpressionTreeFetcher |  28.5860 ns | 0.2491 ns | 0.2330 ns |  28.6757 ns |  28.0360 ns |  28.9169 ns |  13.018 |    0.11 | 0.0268 |     - |     - |     112 B |
|           EmitFetcher |  26.7180 ns | 0.3651 ns | 0.3237 ns |  26.7631 ns |  26.2570 ns |  27.1209 ns |  12.167 |    0.15 | 0.0268 |     - |     - |     112 B |
|       DelegateFetcher |          NA |        NA |        NA |          NA |          NA |          NA |       ? |       ? |      - |     - |     - |         - |
|            Reflection | 236.4381 ns | 1.4285 ns | 1.3362 ns | 236.1021 ns | 234.5859 ns | 238.7684 ns | 107.677 |    0.61 | 0.0362 |     - |     - |     152 B |

### Private Class

#### Private Property / Getter / Object Type
|                Method |      Mean |     Error |    StdDev |       Min |       Max | Ratio | RatioSD | Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------- |----------:|----------:|----------:|----------:|----------:|------:|--------:|------:|------:|------:|----------:|
|                Direct |        NA |        NA |        NA |        NA |        NA |     ? |       ? |     - |     - |     - |         - |
|              DuckType |  2.936 ns | 0.0072 ns | 0.0064 ns |  2.926 ns |  2.948 ns |  1.00 |    0.00 |     - |     - |     - |         - |
| ExpressionTreeFetcher |  3.789 ns | 0.0858 ns | 0.0802 ns |  3.667 ns |  3.923 ns |  1.29 |    0.03 |     - |     - |     - |         - |
|           EmitFetcher |  3.467 ns | 0.0255 ns | 0.0238 ns |  3.414 ns |  3.495 ns |  1.18 |    0.01 |     - |     - |     - |         - |
|       DelegateFetcher |  5.608 ns | 0.0243 ns | 0.0227 ns |  5.562 ns |  5.632 ns |  1.91 |    0.01 |     - |     - |     - |         - |
|            Reflection | 93.496 ns | 0.1863 ns | 0.1556 ns | 93.281 ns | 93.788 ns | 31.84 |    0.08 |     - |     - |     - |         - |

#### Private Property / Getter / Value Type
|                Method |       Mean |     Error |    StdDev |        Min |        Max | Ratio | RatioSD |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------- |-----------:|----------:|----------:|-----------:|-----------:|------:|--------:|-------:|------:|------:|----------:|
|                Direct |         NA |        NA |        NA |         NA |         NA |     ? |       ? |      - |     - |     - |         - |
|              DuckType |   2.671 ns | 0.0186 ns | 0.0174 ns |   2.654 ns |   2.698 ns |  1.00 |    0.00 |      - |     - |     - |         - |
| ExpressionTreeFetcher |   6.684 ns | 0.0629 ns | 0.0491 ns |   6.608 ns |   6.809 ns |  2.50 |    0.02 | 0.0057 |     - |     - |      24 B |
|           EmitFetcher |   7.072 ns | 0.0596 ns | 0.0558 ns |   6.967 ns |   7.158 ns |  2.65 |    0.03 | 0.0057 |     - |     - |      24 B |
|       DelegateFetcher |   7.384 ns | 0.0280 ns | 0.0234 ns |   7.354 ns |   7.432 ns |  2.77 |    0.02 | 0.0057 |     - |     - |      24 B |
|            Reflection | 124.924 ns | 0.8842 ns | 0.8270 ns | 123.838 ns | 126.599 ns | 46.77 |    0.33 | 0.0057 |     - |     - |      24 B |

#### Private Property / Setter / Object Type
|                Method |       Mean |     Error |    StdDev |        Min |        Max | Ratio | RatioSD |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------- |-----------:|----------:|----------:|-----------:|-----------:|------:|--------:|-------:|------:|------:|----------:|
|                Direct |         NA |        NA |        NA |         NA |         NA |     ? |       ? |      - |     - |     - |         - |
|              DuckType |   3.435 ns | 0.0069 ns | 0.0061 ns |   3.423 ns |   3.443 ns |  1.00 |    0.00 |      - |     - |     - |         - |
| ExpressionTreeFetcher |   4.994 ns | 0.1262 ns | 0.1240 ns |   4.790 ns |   5.228 ns |  1.46 |    0.04 |      - |     - |     - |         - |
|           EmitFetcher |   4.789 ns | 0.1132 ns | 0.1059 ns |   4.657 ns |   4.962 ns |  1.39 |    0.03 |      - |     - |     - |         - |
|       DelegateFetcher |   6.606 ns | 0.0324 ns | 0.0303 ns |   6.540 ns |   6.649 ns |  1.92 |    0.01 |      - |     - |     - |         - |
|            Reflection | 162.113 ns | 0.4446 ns | 0.3713 ns | 161.621 ns | 162.959 ns | 47.20 |    0.12 | 0.0153 |     - |     - |      64 B |

#### Private Property / Setter / Value Type
|                Method |       Mean |     Error |    StdDev |        Min |        Max | Ratio | RatioSD |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------- |-----------:|----------:|----------:|-----------:|-----------:|------:|--------:|-------:|------:|------:|----------:|
|                Direct |         NA |        NA |        NA |         NA |         NA |     ? |       ? |      - |     - |     - |         - |
|              DuckType |   2.956 ns | 0.0083 ns | 0.0074 ns |   2.945 ns |   2.969 ns |  1.00 |    0.00 |      - |     - |     - |         - |
| ExpressionTreeFetcher |   6.941 ns | 0.0409 ns | 0.0362 ns |   6.846 ns |   6.996 ns |  2.35 |    0.01 | 0.0057 |     - |     - |      24 B |
|           EmitFetcher |   6.764 ns | 0.0877 ns | 0.0732 ns |   6.655 ns |   6.909 ns |  2.29 |    0.03 | 0.0057 |     - |     - |      24 B |
|       DelegateFetcher |   7.213 ns | 0.0517 ns | 0.0459 ns |   7.155 ns |   7.301 ns |  2.44 |    0.02 | 0.0057 |     - |     - |      24 B |
|            Reflection | 172.387 ns | 0.7338 ns | 0.6505 ns | 171.029 ns | 173.528 ns | 58.31 |    0.25 | 0.0210 |     - |     - |      88 B |

#### Private Field / Getter / Object Type
|                Method |      Mean |     Error |    StdDev |       Min |       Max | Ratio | RatioSD | Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------- |----------:|----------:|----------:|----------:|----------:|------:|--------:|------:|------:|------:|----------:|
|                Direct |        NA |        NA |        NA |        NA |        NA |     ? |       ? |     - |     - |     - |         - |
|              DuckType |  2.950 ns | 0.0133 ns | 0.0125 ns |  2.934 ns |  2.975 ns |  1.00 |    0.00 |     - |     - |     - |         - |
| ExpressionTreeFetcher |  3.424 ns | 0.0386 ns | 0.0361 ns |  3.372 ns |  3.483 ns |  1.16 |    0.01 |     - |     - |     - |         - |
|           EmitFetcher |  3.547 ns | 0.0956 ns | 0.0895 ns |  3.383 ns |  3.723 ns |  1.20 |    0.03 |     - |     - |     - |         - |
|       DelegateFetcher |        NA |        NA |        NA |        NA |        NA |     ? |       ? |     - |     - |     - |         - |
|            Reflection | 37.010 ns | 0.1787 ns | 0.1492 ns | 36.813 ns | 37.375 ns | 12.54 |    0.06 |     - |     - |     - |         - |

#### Private Field / Getter / Value Type
|                Method |      Mean |     Error |    StdDev |       Min |       Max | Ratio | RatioSD |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------- |----------:|----------:|----------:|----------:|----------:|------:|--------:|-------:|------:|------:|----------:|
|                Direct |        NA |        NA |        NA |        NA |        NA |     ? |       ? |      - |     - |     - |         - |
|              DuckType |  2.691 ns | 0.0047 ns | 0.0042 ns |  2.685 ns |  2.699 ns |  1.00 |    0.00 |      - |     - |     - |         - |
| ExpressionTreeFetcher |  6.855 ns | 0.0674 ns | 0.0598 ns |  6.752 ns |  6.938 ns |  2.55 |    0.02 | 0.0057 |     - |     - |      24 B |
|           EmitFetcher |  6.412 ns | 0.1476 ns | 0.1450 ns |  6.112 ns |  6.646 ns |  2.38 |    0.06 | 0.0057 |     - |     - |      24 B |
|       DelegateFetcher |        NA |        NA |        NA |        NA |        NA |     ? |       ? |      - |     - |     - |         - |
|            Reflection | 49.348 ns | 0.5626 ns | 0.5263 ns | 48.456 ns | 50.299 ns | 18.34 |    0.20 | 0.0057 |     - |     - |      24 B |

#### Private Field / Setter / Object Type
|                Method |      Mean |     Error |    StdDev |       Min |       Max | Ratio | RatioSD | Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------- |----------:|----------:|----------:|----------:|----------:|------:|--------:|------:|------:|------:|----------:|
|                Direct |        NA |        NA |        NA |        NA |        NA |     ? |       ? |     - |     - |     - |         - |
|              DuckType |  3.427 ns | 0.0072 ns | 0.0060 ns |  3.417 ns |  3.435 ns |  1.00 |    0.00 |     - |     - |     - |         - |
| ExpressionTreeFetcher |  4.255 ns | 0.0142 ns | 0.0119 ns |  4.240 ns |  4.278 ns |  1.24 |    0.00 |     - |     - |     - |         - |
|           EmitFetcher |  4.224 ns | 0.0316 ns | 0.0296 ns |  4.174 ns |  4.278 ns |  1.23 |    0.01 |     - |     - |     - |         - |
|       DelegateFetcher |        NA |        NA |        NA |        NA |        NA |     ? |       ? |     - |     - |     - |         - |
|            Reflection | 55.264 ns | 0.2157 ns | 0.1912 ns | 55.045 ns | 55.632 ns | 16.13 |    0.06 |     - |     - |     - |         - |

#### Private Field / Setter / Value Type
|                Method |      Mean |     Error |    StdDev |    Median |       Min |       Max | Ratio | RatioSD |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------- |----------:|----------:|----------:|----------:|----------:|----------:|------:|--------:|-------:|------:|------:|----------:|
|                Direct |        NA |        NA |        NA |        NA |        NA |        NA |     ? |       ? |      - |     - |     - |         - |
|              DuckType |  2.458 ns | 0.0061 ns | 0.0057 ns |  2.458 ns |  2.440 ns |  2.464 ns |  1.00 |    0.00 |      - |     - |     - |         - |
| ExpressionTreeFetcher |  6.939 ns | 0.0464 ns | 0.0362 ns |  6.934 ns |  6.880 ns |  7.011 ns |  2.82 |    0.02 | 0.0057 |     - |     - |      24 B |
|           EmitFetcher |  7.008 ns | 0.1701 ns | 0.4512 ns |  6.787 ns |  6.577 ns |  7.999 ns |  3.02 |    0.20 | 0.0057 |     - |     - |      24 B |
|       DelegateFetcher |        NA |        NA |        NA |        NA |        NA |        NA |     ? |       ? |      - |     - |     - |         - |
|            Reflection | 67.276 ns | 0.5043 ns | 0.4471 ns | 67.366 ns | 66.603 ns | 68.024 ns | 27.37 |    0.18 | 0.0057 |     - |     - |      24 B |

#### Private Method / Invoker
|                Method |      Mean |    Error |   StdDev |       Min |       Max | Ratio | RatioSD |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------- |----------:|---------:|---------:|----------:|----------:|------:|--------:|-------:|------:|------:|----------:|
|                Direct |        NA |       NA |       NA |        NA |        NA |     ? |       ? |      - |     - |     - |         - |
|              DuckType |  37.40 ns | 0.161 ns | 0.142 ns |  37.18 ns |  37.68 ns |  1.00 |    0.00 | 0.0268 |     - |     - |     112 B |
| ExpressionTreeFetcher |  34.45 ns | 0.089 ns | 0.074 ns |  34.35 ns |  34.63 ns |  0.92 |    0.00 | 0.0268 |     - |     - |     112 B |
|           EmitFetcher |  33.02 ns | 0.188 ns | 0.167 ns |  32.73 ns |  33.27 ns |  0.88 |    0.00 | 0.0268 |     - |     - |     112 B |
|       DelegateFetcher |        NA |       NA |       NA |        NA |        NA |     ? |       ? |      - |     - |     - |         - |
|            Reflection | 235.67 ns | 1.190 ns | 0.994 ns | 233.50 ns | 237.68 ns |  6.30 |    0.04 | 0.0362 |     - |     - |     152 B |

## Powered By
<img src="https://raw.githubusercontent.com/tonyredondo/TWCore2/master/doc/rider.jpg" alt="Rider" width="50px" height="50px" /><img src="https://raw.githubusercontent.com/tonyredondo/TWCore2/master/doc/dotTrace.png" alt="dotTrace" width="50px" height="50px" /><img src="https://raw.githubusercontent.com/tonyredondo/TWCore2/master/doc/dotMemory.png" alt="dotMemory" width="50px" height="50px" />

Thanks to @jetbrains for helping on this development with the licenses for Rider, dotTrace and dotMemory