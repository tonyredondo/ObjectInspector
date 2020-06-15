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
|                Direct |  0.0023 ns | 0.0035 ns | 0.0031 ns |  0.0007 ns |  0.0000 ns |  0.0101 ns |  0.001 |    0.00 |     - |     - |     - |         - |
|              DuckType |  2.2133 ns | 0.0047 ns | 0.0039 ns |  2.2121 ns |  2.2078 ns |  2.2211 ns |  1.000 |    0.00 |     - |     - |     - |         - |
| ExpressionTreeFetcher |  3.5102 ns | 0.0428 ns | 0.0400 ns |  3.5221 ns |  3.4493 ns |  3.5803 ns |  1.588 |    0.02 |     - |     - |     - |         - |
|           EmitFetcher |  3.5919 ns | 0.0679 ns | 0.0635 ns |  3.5842 ns |  3.4518 ns |  3.6732 ns |  1.619 |    0.03 |     - |     - |     - |         - |
|       DelegateFetcher |  5.9079 ns | 0.0043 ns | 0.0036 ns |  5.9084 ns |  5.9013 ns |  5.9129 ns |  2.669 |    0.00 |     - |     - |     - |         - |
|            Reflection | 90.2937 ns | 0.7775 ns | 0.6892 ns | 90.2260 ns | 89.1883 ns | 91.6522 ns | 40.782 |    0.32 |     - |     - |     - |         - |

#### Public Property / Getter / Value Type
|                Method |        Mean |     Error |    StdDev |      Median |         Min |         Max |  Ratio | RatioSD |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------- |------------:|----------:|----------:|------------:|------------:|------------:|-------:|--------:|-------:|------:|------:|----------:|
|                Direct |   0.0006 ns | 0.0011 ns | 0.0009 ns |   0.0000 ns |   0.0000 ns |   0.0027 ns |  0.000 |    0.00 |      - |     - |     - |         - |
|              DuckType |   1.7264 ns | 0.0068 ns | 0.0060 ns |   1.7250 ns |   1.7162 ns |   1.7374 ns |  1.000 |    0.00 |      - |     - |     - |         - |
| ExpressionTreeFetcher |   7.7038 ns | 0.1458 ns | 0.1364 ns |   7.7093 ns |   7.3411 ns |   7.8954 ns |  4.470 |    0.09 | 0.0057 |     - |     - |      24 B |
|           EmitFetcher |   7.2748 ns | 0.0177 ns | 0.0148 ns |   7.2787 ns |   7.2539 ns |   7.2968 ns |  4.216 |    0.01 | 0.0057 |     - |     - |      24 B |
|       DelegateFetcher |   7.6228 ns | 0.0165 ns | 0.0129 ns |   7.6226 ns |   7.6035 ns |   7.6404 ns |  4.417 |    0.02 | 0.0057 |     - |     - |      24 B |
|            Reflection | 124.0161 ns | 1.9395 ns | 1.8142 ns | 123.5298 ns | 122.3042 ns | 127.9401 ns | 71.878 |    1.21 | 0.0057 |     - |     - |      24 B |

#### Public Property / Setter / Object Type
|                Method |       Mean |     Error |    StdDev |        Min |        Max | Ratio | RatioSD |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------- |-----------:|----------:|----------:|-----------:|-----------:|------:|--------:|-------:|------:|------:|----------:|
|                Direct |   1.469 ns | 0.0020 ns | 0.0018 ns |   1.466 ns |   1.472 ns |  0.55 |    0.00 |      - |     - |     - |         - |
|              DuckType |   2.676 ns | 0.0065 ns | 0.0058 ns |   2.669 ns |   2.690 ns |  1.00 |    0.00 |      - |     - |     - |         - |
| ExpressionTreeFetcher |   4.933 ns | 0.0649 ns | 0.0607 ns |   4.839 ns |   5.037 ns |  1.84 |    0.02 |      - |     - |     - |         - |
|           EmitFetcher |   4.900 ns | 0.0545 ns | 0.0510 ns |   4.784 ns |   4.967 ns |  1.83 |    0.02 |      - |     - |     - |         - |
|       DelegateFetcher |   6.623 ns | 0.0163 ns | 0.0145 ns |   6.601 ns |   6.654 ns |  2.48 |    0.01 |      - |     - |     - |         - |
|            Reflection | 152.594 ns | 0.7596 ns | 0.6733 ns | 151.424 ns | 153.742 ns | 57.03 |    0.29 | 0.0153 |     - |     - |      64 B |

#### Public Property / Setter / Value Type
|                Method |        Mean |     Error |    StdDev |      Median |         Min |         Max |  Ratio | RatioSD |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------- |------------:|----------:|----------:|------------:|------------:|------------:|-------:|--------:|-------:|------:|------:|----------:|
|                Direct |   0.0076 ns | 0.0110 ns | 0.0103 ns |   0.0000 ns |   0.0000 ns |   0.0345 ns |  0.002 |    0.00 |      - |     - |     - |         - |
|              DuckType |   3.8392 ns | 0.0112 ns | 0.0105 ns |   3.8364 ns |   3.8259 ns |   3.8609 ns |  1.000 |    0.00 |      - |     - |     - |         - |
| ExpressionTreeFetcher |   6.7522 ns | 0.1256 ns | 0.1175 ns |   6.6889 ns |   6.6395 ns |   6.9530 ns |  1.759 |    0.03 | 0.0057 |     - |     - |      24 B |
|           EmitFetcher |   7.1082 ns | 0.0897 ns | 0.0749 ns |   7.1217 ns |   6.8716 ns |   7.1747 ns |  1.851 |    0.02 | 0.0057 |     - |     - |      24 B |
|       DelegateFetcher |   7.2348 ns | 0.1606 ns | 0.1341 ns |   7.3027 ns |   6.9570 ns |   7.3383 ns |  1.884 |    0.04 | 0.0057 |     - |     - |      24 B |
|            Reflection | 174.1714 ns | 2.3270 ns | 2.1767 ns | 173.9244 ns | 171.3595 ns | 178.7492 ns | 45.367 |    0.56 | 0.0210 |     - |     - |      88 B |

#### Public Field / Getter / Object Type
|                Method |       Mean |     Error |    StdDev |     Median |        Min |        Max |  Ratio | RatioSD | Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------- |-----------:|----------:|----------:|-----------:|-----------:|-----------:|-------:|--------:|------:|------:|------:|----------:|
|                Direct |  0.0002 ns | 0.0007 ns | 0.0006 ns |  0.0000 ns |  0.0000 ns |  0.0022 ns |  0.000 |    0.00 |     - |     - |     - |         - |
|              DuckType |  2.2140 ns | 0.0057 ns | 0.0048 ns |  2.2136 ns |  2.2080 ns |  2.2226 ns |  1.000 |    0.00 |     - |     - |     - |         - |
| ExpressionTreeFetcher |  3.5390 ns | 0.0621 ns | 0.0581 ns |  3.5424 ns |  3.4653 ns |  3.6475 ns |  1.597 |    0.02 |     - |     - |     - |         - |
|           EmitFetcher |  3.5524 ns | 0.0625 ns | 0.0585 ns |  3.5271 ns |  3.4662 ns |  3.6496 ns |  1.608 |    0.03 |     - |     - |     - |         - |
|       DelegateFetcher |         NA |        NA |        NA |         NA |         NA |         NA |      ? |       ? |     - |     - |     - |         - |
|            Reflection | 38.3814 ns | 0.2766 ns | 0.2452 ns | 38.3146 ns | 38.1105 ns | 38.9707 ns | 17.329 |    0.13 |     - |     - |     - |         - |

#### Public Field / Getter / Value Type
|                Method |       Mean |     Error |    StdDev |     Median |        Min |        Max |  Ratio | RatioSD |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------- |-----------:|----------:|----------:|-----------:|-----------:|-----------:|-------:|--------:|-------:|------:|------:|----------:|
|                Direct |  0.0020 ns | 0.0028 ns | 0.0024 ns |  0.0008 ns |  0.0000 ns |  0.0071 ns |  0.001 |    0.00 |      - |     - |     - |         - |
|              DuckType |  1.7189 ns | 0.0032 ns | 0.0025 ns |  1.7189 ns |  1.7154 ns |  1.7228 ns |  1.000 |    0.00 |      - |     - |     - |         - |
| ExpressionTreeFetcher |  6.4483 ns | 0.0441 ns | 0.0412 ns |  6.4444 ns |  6.3806 ns |  6.5182 ns |  3.751 |    0.03 | 0.0057 |     - |     - |      24 B |
|           EmitFetcher |  7.0470 ns | 0.0178 ns | 0.0149 ns |  7.0470 ns |  7.0188 ns |  7.0687 ns |  4.099 |    0.01 | 0.0057 |     - |     - |      24 B |
|       DelegateFetcher |         NA |        NA |        NA |         NA |         NA |         NA |      ? |       ? |      - |     - |     - |         - |
|            Reflection | 53.9989 ns | 1.0655 ns | 1.3854 ns | 54.0414 ns | 51.5927 ns | 56.7966 ns | 31.600 |    0.76 | 0.0057 |     - |     - |      24 B |

#### Public Field / Setter / Object Type
|                Method |      Mean |     Error |    StdDev |       Min |       Max | Ratio | RatioSD | Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------- |----------:|----------:|----------:|----------:|----------:|------:|--------:|------:|------:|------:|----------:|
|                Direct |  1.219 ns | 0.0091 ns | 0.0076 ns |  1.202 ns |  1.226 ns |  0.50 |    0.00 |     - |     - |     - |         - |
|              DuckType |  2.450 ns | 0.0046 ns | 0.0041 ns |  2.445 ns |  2.457 ns |  1.00 |    0.00 |     - |     - |     - |         - |
| ExpressionTreeFetcher |  4.798 ns | 0.0935 ns | 0.0875 ns |  4.703 ns |  4.973 ns |  1.96 |    0.04 |     - |     - |     - |         - |
|           EmitFetcher |  4.707 ns | 0.0480 ns | 0.0425 ns |  4.592 ns |  4.757 ns |  1.92 |    0.02 |     - |     - |     - |         - |
|       DelegateFetcher |        NA |        NA |        NA |        NA |        NA |     ? |       ? |     - |     - |     - |         - |
|            Reflection | 59.901 ns | 0.1571 ns | 0.1393 ns | 59.526 ns | 60.074 ns | 24.45 |    0.06 |     - |     - |     - |         - |

#### Public Field / Setter / Value Type
|                Method |       Mean |     Error |    StdDev |     Median |        Min |        Max |  Ratio | RatioSD |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------- |-----------:|----------:|----------:|-----------:|-----------:|-----------:|-------:|--------:|-------:|------:|------:|----------:|
|                Direct |  0.0009 ns | 0.0013 ns | 0.0011 ns |  0.0004 ns |  0.0000 ns |  0.0038 ns |  0.000 |    0.00 |      - |     - |     - |         - |
|              DuckType |  3.8483 ns | 0.0051 ns | 0.0043 ns |  3.8477 ns |  3.8432 ns |  3.8541 ns |  1.000 |    0.00 |      - |     - |     - |         - |
| ExpressionTreeFetcher |  6.8112 ns | 0.0280 ns | 0.0249 ns |  6.8088 ns |  6.7713 ns |  6.8596 ns |  1.770 |    0.01 | 0.0057 |     - |     - |      24 B |
|           EmitFetcher |  6.8094 ns | 0.0428 ns | 0.0400 ns |  6.8104 ns |  6.7537 ns |  6.8846 ns |  1.770 |    0.01 | 0.0057 |     - |     - |      24 B |
|       DelegateFetcher |         NA |        NA |        NA |         NA |         NA |         NA |      ? |       ? |      - |     - |     - |         - |
|            Reflection | 67.9435 ns | 0.1974 ns | 0.1847 ns | 68.0028 ns | 67.6364 ns | 68.2644 ns | 17.657 |    0.06 | 0.0057 |     - |     - |      24 B |

#### Public Method / Invoker
|                Method |        Mean |     Error |    StdDev |      Median |         Min |         Max |   Ratio | RatioSD |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------- |------------:|----------:|----------:|------------:|------------:|------------:|--------:|--------:|-------:|------:|------:|----------:|
|                Direct |   0.0003 ns | 0.0008 ns | 0.0007 ns |   0.0000 ns |   0.0000 ns |   0.0025 ns |   0.000 |    0.00 |      - |     - |     - |         - |
|              DuckType |   1.7190 ns | 0.0033 ns | 0.0028 ns |   1.7189 ns |   1.7134 ns |   1.7244 ns |   1.000 |    0.00 |      - |     - |     - |         - |
| ExpressionTreeFetcher |  28.7270 ns | 0.4115 ns | 0.3849 ns |  28.5705 ns |  28.3032 ns |  29.4773 ns |  16.682 |    0.21 | 0.0268 |     - |     - |     112 B |
|           EmitFetcher |  29.2437 ns | 0.6051 ns | 0.5660 ns |  29.1503 ns |  28.5012 ns |  30.5212 ns |  17.049 |    0.33 | 0.0268 |     - |     - |     112 B |
|       DelegateFetcher |          NA |        NA |        NA |          NA |          NA |          NA |       ? |       ? |      - |     - |     - |         - |
|            Reflection | 235.2166 ns | 1.1395 ns | 0.9515 ns | 235.0327 ns | 233.9495 ns | 236.9853 ns | 136.832 |    0.64 | 0.0362 |     - |     - |     152 B |

### Private Class

#### Private Property / Getter / Object Type
|                Method |      Mean |     Error |    StdDev |       Min |       Max | Ratio | RatioSD | Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------- |----------:|----------:|----------:|----------:|----------:|------:|--------:|------:|------:|------:|----------:|
|                Direct |        NA |        NA |        NA |        NA |        NA |     ? |       ? |     - |     - |     - |         - |
|              DuckType |  4.023 ns | 0.0622 ns | 0.0519 ns |  3.926 ns |  4.130 ns |  1.00 |    0.00 |     - |     - |     - |         - |
| ExpressionTreeFetcher |  3.503 ns | 0.0488 ns | 0.0433 ns |  3.430 ns |  3.572 ns |  0.87 |    0.02 |     - |     - |     - |         - |
|           EmitFetcher |  3.509 ns | 0.0140 ns | 0.0124 ns |  3.480 ns |  3.528 ns |  0.87 |    0.01 |     - |     - |     - |         - |
|       DelegateFetcher |  5.645 ns | 0.0174 ns | 0.0163 ns |  5.617 ns |  5.669 ns |  1.40 |    0.02 |     - |     - |     - |         - |
|            Reflection | 95.503 ns | 0.3657 ns | 0.2855 ns | 95.142 ns | 96.014 ns | 23.70 |    0.27 |     - |     - |     - |         - |

#### Private Property / Getter / Value Type
|                Method |       Mean |     Error |    StdDev |        Min |        Max | Ratio | RatioSD |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------- |-----------:|----------:|----------:|-----------:|-----------:|------:|--------:|-------:|------:|------:|----------:|
|                Direct |         NA |        NA |        NA |         NA |         NA |     ? |       ? |      - |     - |     - |         - |
|              DuckType |  15.777 ns | 0.2834 ns | 0.2512 ns |  15.267 ns |  16.011 ns |  1.00 |    0.00 | 0.0057 |     - |     - |      24 B |
| ExpressionTreeFetcher |   6.895 ns | 0.1334 ns | 0.1114 ns |   6.802 ns |   7.199 ns |  0.44 |    0.01 | 0.0057 |     - |     - |      24 B |
|           EmitFetcher |   7.215 ns | 0.0498 ns | 0.0466 ns |   7.150 ns |   7.298 ns |  0.46 |    0.01 | 0.0057 |     - |     - |      24 B |
|       DelegateFetcher |   7.421 ns | 0.0966 ns | 0.0856 ns |   7.336 ns |   7.649 ns |  0.47 |    0.01 | 0.0057 |     - |     - |      24 B |
|            Reflection | 119.032 ns | 0.7923 ns | 0.7023 ns | 117.977 ns | 120.597 ns |  7.55 |    0.13 | 0.0057 |     - |     - |      24 B |

#### Private Property / Setter / Object Type
|                Method |       Mean |     Error |    StdDev |        Min |        Max | Ratio | RatioSD |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------- |-----------:|----------:|----------:|-----------:|-----------:|------:|--------:|-------:|------:|------:|----------:|
|                Direct |         NA |        NA |        NA |         NA |         NA |     ? |       ? |      - |     - |     - |         - |
|              DuckType |   4.173 ns | 0.0082 ns | 0.0068 ns |   4.164 ns |   4.188 ns |  1.00 |    0.00 |      - |     - |     - |         - |
| ExpressionTreeFetcher |   5.427 ns | 0.0759 ns | 0.0710 ns |   5.291 ns |   5.568 ns |  1.30 |    0.02 |      - |     - |     - |         - |
|           EmitFetcher |   5.373 ns | 0.0654 ns | 0.0611 ns |   5.260 ns |   5.468 ns |  1.29 |    0.02 |      - |     - |     - |         - |
|       DelegateFetcher |   6.761 ns | 0.0081 ns | 0.0067 ns |   6.747 ns |   6.769 ns |  1.62 |    0.00 |      - |     - |     - |         - |
|            Reflection | 169.404 ns | 1.1030 ns | 0.8611 ns | 168.348 ns | 171.475 ns | 40.59 |    0.19 | 0.0153 |     - |     - |      64 B |

#### Private Property / Setter / Value Type
|                Method |       Mean |     Error |    StdDev |        Min |        Max | Ratio | RatioSD |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------- |-----------:|----------:|----------:|-----------:|-----------:|------:|--------:|-------:|------:|------:|----------:|
|                Direct |         NA |        NA |        NA |         NA |         NA |     ? |       ? |      - |     - |     - |         - |
|              DuckType |   5.956 ns | 0.0274 ns | 0.0228 ns |   5.915 ns |   5.986 ns |  1.00 |    0.00 | 0.0057 |     - |     - |      24 B |
| ExpressionTreeFetcher |   6.677 ns | 0.0700 ns | 0.0655 ns |   6.596 ns |   6.797 ns |  1.12 |    0.01 | 0.0057 |     - |     - |      24 B |
|           EmitFetcher |   6.778 ns | 0.0904 ns | 0.0801 ns |   6.629 ns |   6.898 ns |  1.14 |    0.01 | 0.0057 |     - |     - |      24 B |
|       DelegateFetcher |   7.154 ns | 0.1309 ns | 0.1225 ns |   6.838 ns |   7.268 ns |  1.20 |    0.02 | 0.0057 |     - |     - |      24 B |
|            Reflection | 170.614 ns | 1.4150 ns | 1.2544 ns | 168.728 ns | 173.248 ns | 28.67 |    0.28 | 0.0210 |     - |     - |      88 B |

#### Private Field / Getter / Object Type
|                Method |      Mean |     Error |    StdDev |       Min |       Max | Ratio | RatioSD | Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------- |----------:|----------:|----------:|----------:|----------:|------:|--------:|------:|------:|------:|----------:|
|                Direct |        NA |        NA |        NA |        NA |        NA |     ? |       ? |     - |     - |     - |         - |
|              DuckType |  3.904 ns | 0.0207 ns | 0.0193 ns |  3.883 ns |  3.938 ns |  1.00 |    0.00 |     - |     - |     - |         - |
| ExpressionTreeFetcher |  3.582 ns | 0.0297 ns | 0.0263 ns |  3.514 ns |  3.620 ns |  0.92 |    0.01 |     - |     - |     - |         - |
|           EmitFetcher |  3.613 ns | 0.0986 ns | 0.0968 ns |  3.507 ns |  3.833 ns |  0.93 |    0.02 |     - |     - |     - |         - |
|       DelegateFetcher |        NA |        NA |        NA |        NA |        NA |     ? |       ? |     - |     - |     - |         - |
|            Reflection | 37.172 ns | 0.1171 ns | 0.1095 ns | 36.985 ns | 37.339 ns |  9.52 |    0.05 |     - |     - |     - |         - |

#### Private Field / Getter / Value Type
|                Method |      Mean |     Error |    StdDev |       Min |       Max | Ratio | RatioSD |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------- |----------:|----------:|----------:|----------:|----------:|------:|--------:|-------:|------:|------:|----------:|
|                Direct |        NA |        NA |        NA |        NA |        NA |     ? |       ? |      - |     - |     - |         - |
|              DuckType | 14.377 ns | 0.0591 ns | 0.0461 ns | 14.288 ns | 14.464 ns |  1.00 |    0.00 | 0.0057 |     - |     - |      24 B |
| ExpressionTreeFetcher |  6.781 ns | 0.1606 ns | 0.1912 ns |  6.304 ns |  7.092 ns |  0.48 |    0.01 | 0.0057 |     - |     - |      24 B |
|           EmitFetcher |  6.383 ns | 0.0329 ns | 0.0307 ns |  6.331 ns |  6.431 ns |  0.44 |    0.00 | 0.0057 |     - |     - |      24 B |
|       DelegateFetcher |        NA |        NA |        NA |        NA |        NA |     ? |       ? |      - |     - |     - |         - |
|            Reflection | 50.260 ns | 0.3705 ns | 0.3466 ns | 49.621 ns | 50.943 ns |  3.49 |    0.02 | 0.0057 |     - |     - |      24 B |

#### Private Field / Setter / Object Type
|                Method |      Mean |     Error |    StdDev |       Min |       Max | Ratio | RatioSD | Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------- |----------:|----------:|----------:|----------:|----------:|------:|--------:|------:|------:|------:|----------:|
|                Direct |        NA |        NA |        NA |        NA |        NA |     ? |       ? |     - |     - |     - |         - |
|              DuckType |  3.992 ns | 0.0497 ns | 0.0465 ns |  3.942 ns |  4.085 ns |  1.00 |    0.00 |     - |     - |     - |         - |
| ExpressionTreeFetcher |  4.710 ns | 0.0348 ns | 0.0326 ns |  4.640 ns |  4.757 ns |  1.18 |    0.02 |     - |     - |     - |         - |
|           EmitFetcher |  4.676 ns | 0.0656 ns | 0.0547 ns |  4.550 ns |  4.745 ns |  1.17 |    0.02 |     - |     - |     - |         - |
|       DelegateFetcher |        NA |        NA |        NA |        NA |        NA |     ? |       ? |     - |     - |     - |         - |
|            Reflection | 59.743 ns | 0.2165 ns | 0.1919 ns | 59.442 ns | 60.114 ns | 14.98 |    0.19 |     - |     - |     - |         - |

#### Private Field / Setter / Value Type
|                Method |      Mean |     Error |    StdDev |       Min |       Max | Ratio | RatioSD |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------- |----------:|----------:|----------:|----------:|----------:|------:|--------:|-------:|------:|------:|----------:|
|                Direct |        NA |        NA |        NA |        NA |        NA |     ? |       ? |      - |     - |     - |         - |
|              DuckType |  5.817 ns | 0.1022 ns | 0.0906 ns |  5.720 ns |  5.951 ns |  1.00 |    0.00 | 0.0057 |     - |     - |      24 B |
| ExpressionTreeFetcher |  6.726 ns | 0.0451 ns | 0.0422 ns |  6.635 ns |  6.804 ns |  1.16 |    0.02 | 0.0057 |     - |     - |      24 B |
|           EmitFetcher |  6.731 ns | 0.0691 ns | 0.0612 ns |  6.574 ns |  6.807 ns |  1.16 |    0.02 | 0.0057 |     - |     - |      24 B |
|       DelegateFetcher |        NA |        NA |        NA |        NA |        NA |     ? |       ? |      - |     - |     - |         - |
|            Reflection | 68.764 ns | 0.3051 ns | 0.2382 ns | 68.348 ns | 69.267 ns | 11.82 |    0.21 | 0.0057 |     - |     - |      24 B |

#### Private Method / Invoker
|                Method |      Mean |    Error |   StdDev |       Min |       Max | Ratio | RatioSD |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------- |----------:|---------:|---------:|----------:|----------:|------:|--------:|-------:|------:|------:|----------:|
|                Direct |        NA |       NA |       NA |        NA |        NA |     ? |       ? |      - |     - |     - |         - |
|              DuckType |  36.22 ns | 0.251 ns | 0.235 ns |  35.85 ns |  36.52 ns |  1.00 |    0.00 | 0.0268 |     - |     - |     112 B |
| ExpressionTreeFetcher |  26.78 ns | 0.435 ns | 0.363 ns |  26.46 ns |  27.64 ns |  0.74 |    0.01 | 0.0268 |     - |     - |     112 B |
|           EmitFetcher |  28.33 ns | 0.594 ns | 0.556 ns |  27.67 ns |  29.59 ns |  0.78 |    0.02 | 0.0268 |     - |     - |     112 B |
|       DelegateFetcher |        NA |       NA |       NA |        NA |        NA |     ? |       ? |      - |     - |     - |         - |
|            Reflection | 233.41 ns | 3.443 ns | 3.220 ns | 227.69 ns | 238.55 ns |  6.44 |    0.10 | 0.0362 |     - |     - |     152 B |


## Powered By
<img src="https://raw.githubusercontent.com/tonyredondo/TWCore2/master/doc/rider.jpg" alt="Rider" width="50px" height="50px" /><img src="https://raw.githubusercontent.com/tonyredondo/TWCore2/master/doc/dotTrace.png" alt="dotTrace" width="50px" height="50px" /><img src="https://raw.githubusercontent.com/tonyredondo/TWCore2/master/doc/dotMemory.png" alt="dotMemory" width="50px" height="50px" />

Thanks to @jetbrains for helping on this development with the licenses for Rider, dotTrace and dotMemory
