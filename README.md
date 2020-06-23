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
Duck Typing can be done by using an Interface, Abstract class or a class with virtual members. The library will create a proxy type inheriting/implementing the base type.
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
### Public Class

#### Public Property / Getter / Object Type
|                Method |        Mean |     Error |    StdDev |         Min |         Max |  Ratio | RatioSD | Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------- |------------:|----------:|----------:|------------:|------------:|-------:|--------:|------:|------:|------:|----------:|
|                Direct |   0.0057 ns | 0.0032 ns | 0.0030 ns |   0.0000 ns |   0.0088 ns |  0.004 |    0.00 |     - |     - |     - |         - |
|     DuckTypeInterface |   2.1733 ns | 0.0088 ns | 0.0082 ns |   2.1614 ns |   2.1859 ns |  1.498 |    0.01 |     - |     - |     - |         - |
|      DuckTypeAbstract |   1.4507 ns | 0.0058 ns | 0.0055 ns |   1.4409 ns |   1.4586 ns |  1.000 |    0.00 |     - |     - |     - |         - |
|       DuckTypeVirtual |   1.4454 ns | 0.0027 ns | 0.0026 ns |   1.4419 ns |   1.4499 ns |  0.996 |    0.00 |     - |     - |     - |         - |
| ExpressionTreeFetcher |   4.3870 ns | 0.0163 ns | 0.0136 ns |   4.3561 ns |   4.4048 ns |  3.024 |    0.01 |     - |     - |     - |         - |
|           EmitFetcher |   4.4331 ns | 0.0154 ns | 0.0144 ns |   4.4125 ns |   4.4654 ns |  3.056 |    0.01 |     - |     - |     - |         - |
|       DelegateFetcher |   6.4340 ns | 0.0188 ns | 0.0176 ns |   6.3985 ns |   6.4617 ns |  4.435 |    0.02 |     - |     - |     - |         - |
|            Reflection | 107.0668 ns | 0.2081 ns | 0.1845 ns | 106.6127 ns | 107.2895 ns | 73.802 |    0.32 |     - |     - |     - |         - |

#### Public Property / Getter / Value Type
|                Method |        Mean |     Error |    StdDev |      Median |         Min |         Max |   Ratio | RatioSD |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------- |------------:|----------:|----------:|------------:|------------:|------------:|--------:|--------:|-------:|------:|------:|----------:|
|                Direct |   0.0000 ns | 0.0000 ns | 0.0000 ns |   0.0000 ns |   0.0000 ns |   0.0000 ns |   0.000 |    0.00 |      - |     - |     - |         - |
|     DuckTypeInterface |   1.6644 ns | 0.0080 ns | 0.0075 ns |   1.6657 ns |   1.6513 ns |   1.6773 ns |   1.193 |    0.01 |      - |     - |     - |         - |
|      DuckTypeAbstract |   1.3947 ns | 0.0074 ns | 0.0069 ns |   1.3967 ns |   1.3803 ns |   1.4034 ns |   1.000 |    0.00 |      - |     - |     - |         - |
|       DuckTypeVirtual |   1.4578 ns | 0.0124 ns | 0.0116 ns |   1.4568 ns |   1.4429 ns |   1.4750 ns |   1.045 |    0.01 |      - |     - |     - |         - |
| ExpressionTreeFetcher |  12.2679 ns | 0.2723 ns | 0.5438 ns |  12.5072 ns |  10.9949 ns |  12.9669 ns |   8.489 |    0.45 | 0.0014 |     - |     - |      24 B |
|           EmitFetcher |  12.0793 ns | 0.2696 ns | 0.2648 ns |  12.1251 ns |  11.6069 ns |  12.6324 ns |   8.654 |    0.21 | 0.0014 |     - |     - |      24 B |
|       DelegateFetcher |  15.7135 ns | 0.1862 ns | 0.1742 ns |  15.8033 ns |  15.3563 ns |  15.8849 ns |  11.267 |    0.16 | 0.0014 |     - |     - |      24 B |
|            Reflection | 165.7264 ns | 0.7783 ns | 0.7280 ns | 165.4518 ns | 164.8954 ns | 167.2588 ns | 118.828 |    0.89 | 0.0014 |     - |     - |      24 B |

#### Public Property / Setter / Object Type
|                Method |       Mean |     Error |    StdDev |        Min |        Max | Ratio | RatioSD |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------- |-----------:|----------:|----------:|-----------:|-----------:|------:|--------:|-------:|------:|------:|----------:|
|                Direct |   1.433 ns | 0.0095 ns | 0.0079 ns |   1.421 ns |   1.446 ns |  0.60 |    0.00 |      - |     - |     - |         - |
|     DuckTypeInterface |   2.870 ns | 0.0080 ns | 0.0071 ns |   2.856 ns |   2.885 ns |  1.21 |    0.00 |      - |     - |     - |         - |
|      DuckTypeAbstract |   2.371 ns | 0.0063 ns | 0.0049 ns |   2.357 ns |   2.377 ns |  1.00 |    0.00 |      - |     - |     - |         - |
|       DuckTypeVirtual |   2.366 ns | 0.0081 ns | 0.0068 ns |   2.349 ns |   2.376 ns |  1.00 |    0.00 |      - |     - |     - |         - |
| ExpressionTreeFetcher |   5.161 ns | 0.0469 ns | 0.0439 ns |   5.074 ns |   5.232 ns |  2.18 |    0.01 |      - |     - |     - |         - |
|           EmitFetcher |   5.181 ns | 0.0524 ns | 0.0490 ns |   5.073 ns |   5.256 ns |  2.18 |    0.03 |      - |     - |     - |         - |
|       DelegateFetcher |   9.957 ns | 0.1754 ns | 0.1641 ns |   9.701 ns |  10.243 ns |  4.22 |    0.07 |      - |     - |     - |         - |
|            Reflection | 215.519 ns | 0.7459 ns | 0.6977 ns | 213.798 ns | 216.511 ns | 90.86 |    0.29 | 0.0038 |     - |     - |      64 B |

#### Public Property / Setter / Value Type
|                Method |        Mean |     Error |    StdDev |      Median |         Min |         Max |  Ratio | RatioSD |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------- |------------:|----------:|----------:|------------:|------------:|------------:|-------:|--------:|-------:|------:|------:|----------:|
|                Direct |   0.0000 ns | 0.0000 ns | 0.0000 ns |   0.0000 ns |   0.0000 ns |   0.0000 ns |  0.000 |    0.00 |      - |     - |     - |         - |
|     DuckTypeInterface |   4.0533 ns | 0.1463 ns | 0.4199 ns |   4.1939 ns |   2.6805 ns |   4.9533 ns |  1.095 |    0.07 |      - |     - |     - |         - |
|      DuckTypeAbstract |   3.8792 ns | 0.0550 ns | 0.0514 ns |   3.9012 ns |   3.7611 ns |   3.9309 ns |  1.000 |    0.00 |      - |     - |     - |         - |
|       DuckTypeVirtual |   3.9063 ns | 0.0546 ns | 0.0511 ns |   3.9051 ns |   3.7578 ns |   3.9538 ns |  1.007 |    0.01 |      - |     - |     - |         - |
| ExpressionTreeFetcher |  12.7433 ns | 0.2835 ns | 0.6572 ns |  13.0110 ns |  11.1596 ns |  13.6258 ns |  3.222 |    0.21 | 0.0014 |     - |     - |      24 B |
|           EmitFetcher |  11.2177 ns | 0.2420 ns | 0.4110 ns |  11.2254 ns |  10.5157 ns |  12.1097 ns |  2.898 |    0.13 | 0.0014 |     - |     - |      24 B |
|       DelegateFetcher |  15.0292 ns | 0.0974 ns | 0.0864 ns |  15.0117 ns |  14.9219 ns |  15.2370 ns |  3.877 |    0.06 | 0.0014 |     - |     - |      24 B |
|            Reflection | 226.4618 ns | 1.1025 ns | 0.9774 ns | 226.7582 ns | 223.5496 ns | 227.3624 ns | 58.419 |    0.91 | 0.0052 |     - |     - |      88 B |

#### Public Field / Getter / Object Type
|                Method |       Mean |     Error |    StdDev |        Min |        Max | Ratio | RatioSD | Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------- |-----------:|----------:|----------:|-----------:|-----------:|------:|--------:|------:|------:|------:|----------:|
|                Direct |  0.0162 ns | 0.0024 ns | 0.0023 ns |  0.0127 ns |  0.0196 ns |  0.01 |    0.00 |     - |     - |     - |         - |
|     DuckTypeInterface |  1.6780 ns | 0.0035 ns | 0.0033 ns |  1.6705 ns |  1.6825 ns |  1.20 |    0.01 |     - |     - |     - |         - |
|      DuckTypeAbstract |  1.4006 ns | 0.0050 ns | 0.0046 ns |  1.3933 ns |  1.4075 ns |  1.00 |    0.00 |     - |     - |     - |         - |
|       DuckTypeVirtual |  1.3839 ns | 0.0032 ns | 0.0025 ns |  1.3799 ns |  1.3882 ns |  0.99 |    0.00 |     - |     - |     - |         - |
| ExpressionTreeFetcher |  4.4193 ns | 0.0225 ns | 0.0211 ns |  4.3882 ns |  4.4517 ns |  3.16 |    0.02 |     - |     - |     - |         - |
|           EmitFetcher |  4.4563 ns | 0.0221 ns | 0.0207 ns |  4.4206 ns |  4.4880 ns |  3.18 |    0.02 |     - |     - |     - |         - |
|       DelegateFetcher |         NA |        NA |        NA |         NA |         NA |     ? |       ? |     - |     - |     - |         - |
|            Reflection | 48.7989 ns | 0.1460 ns | 0.1294 ns | 48.6077 ns | 49.0602 ns | 34.84 |    0.12 |     - |     - |     - |         - |

#### Public Field / Getter / Value Type
|                Method |       Mean |     Error |    StdDev |        Min |        Max |  Ratio | RatioSD |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------- |-----------:|----------:|----------:|-----------:|-----------:|-------:|--------:|-------:|------:|------:|----------:|
|                Direct |  0.0104 ns | 0.0051 ns | 0.0048 ns |  0.0029 ns |  0.0198 ns |  0.008 |    0.00 |      - |     - |     - |         - |
|     DuckTypeInterface |  1.6868 ns | 0.0067 ns | 0.0063 ns |  1.6743 ns |  1.6995 ns |  1.216 |    0.01 |      - |     - |     - |         - |
|      DuckTypeAbstract |  1.3874 ns | 0.0045 ns | 0.0042 ns |  1.3799 ns |  1.3937 ns |  1.000 |    0.00 |      - |     - |     - |         - |
|       DuckTypeVirtual |  1.3971 ns | 0.0042 ns | 0.0038 ns |  1.3869 ns |  1.4013 ns |  1.007 |    0.00 |      - |     - |     - |         - |
| ExpressionTreeFetcher | 12.3567 ns | 0.2692 ns | 0.3773 ns | 11.3392 ns | 13.0537 ns |  8.883 |    0.33 | 0.0014 |     - |     - |      24 B |
|           EmitFetcher | 12.7230 ns | 0.2337 ns | 0.2186 ns | 12.1087 ns | 12.9269 ns |  9.170 |    0.15 | 0.0014 |     - |     - |      24 B |
|       DelegateFetcher |         NA |        NA |        NA |         NA |         NA |      ? |       ? |      - |     - |     - |         - |
|            Reflection | 74.5795 ns | 0.6475 ns | 0.6057 ns | 73.7307 ns | 75.4323 ns | 53.754 |    0.41 | 0.0014 |     - |     - |      24 B |

#### Public Field / Setter / Object Type
|                Method |      Mean |     Error |    StdDev |       Min |       Max | Ratio | RatioSD | Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------- |----------:|----------:|----------:|----------:|----------:|------:|--------:|------:|------:|------:|----------:|
|                Direct |  1.195 ns | 0.0070 ns | 0.0065 ns |  1.186 ns |  1.205 ns |  0.50 |    0.00 |     - |     - |     - |         - |
|     DuckTypeInterface |  2.638 ns | 0.0171 ns | 0.0160 ns |  2.610 ns |  2.662 ns |  1.09 |    0.01 |     - |     - |     - |         - |
|      DuckTypeAbstract |  2.409 ns | 0.0069 ns | 0.0064 ns |  2.392 ns |  2.417 ns |  1.00 |    0.00 |     - |     - |     - |         - |
|       DuckTypeVirtual |  2.411 ns | 0.0056 ns | 0.0053 ns |  2.400 ns |  2.419 ns |  1.00 |    0.00 |     - |     - |     - |         - |
| ExpressionTreeFetcher |  4.879 ns | 0.0398 ns | 0.0372 ns |  4.816 ns |  4.948 ns |  2.03 |    0.02 |     - |     - |     - |         - |
|           EmitFetcher |  4.798 ns | 0.0265 ns | 0.0248 ns |  4.756 ns |  4.840 ns |  1.99 |    0.01 |     - |     - |     - |         - |
|       DelegateFetcher |        NA |        NA |        NA |        NA |        NA |     ? |       ? |     - |     - |     - |         - |
|            Reflection | 62.681 ns | 0.1221 ns | 0.1142 ns | 62.487 ns | 62.922 ns | 26.02 |    0.08 |     - |     - |     - |         - |

#### Public Field / Setter / Value Type
|                Method |       Mean |     Error |    StdDev |        Min |        Max |  Ratio | RatioSD |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------- |-----------:|----------:|----------:|-----------:|-----------:|-------:|--------:|-------:|------:|------:|----------:|
|                Direct |  0.0161 ns | 0.0066 ns | 0.0058 ns |  0.0069 ns |  0.0277 ns |  0.004 |    0.00 |      - |     - |     - |         - |
|     DuckTypeInterface |  4.1358 ns | 0.1109 ns | 0.1442 ns |  3.6628 ns |  4.2307 ns |  1.028 |    0.04 |      - |     - |     - |         - |
|      DuckTypeAbstract |  3.9717 ns | 0.0101 ns | 0.0085 ns |  3.9483 ns |  3.9830 ns |  1.000 |    0.00 |      - |     - |     - |         - |
|       DuckTypeVirtual |  3.9130 ns | 0.0182 ns | 0.0171 ns |  3.8800 ns |  3.9475 ns |  0.985 |    0.01 |      - |     - |     - |         - |
| ExpressionTreeFetcher | 10.2799 ns | 0.1900 ns | 0.1777 ns | 10.1394 ns | 10.6424 ns |  2.590 |    0.05 | 0.0014 |     - |     - |      24 B |
|           EmitFetcher | 10.2794 ns | 0.1665 ns | 0.1710 ns | 10.0566 ns | 10.6314 ns |  2.586 |    0.05 | 0.0014 |     - |     - |      24 B |
|       DelegateFetcher |         NA |        NA |        NA |         NA |         NA |      ? |       ? |      - |     - |     - |         - |
|            Reflection | 78.7027 ns | 0.2133 ns | 0.1891 ns | 78.3381 ns | 78.9225 ns | 19.823 |    0.06 | 0.0014 |     - |     - |      24 B |

#### Public Method / Invoker
|                Method |        Mean |     Error |    StdDev |      Median |         Min |         Max |   Ratio | RatioSD |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------- |------------:|----------:|----------:|------------:|------------:|------------:|--------:|--------:|-------:|------:|------:|----------:|
|                Direct |   0.0042 ns | 0.0042 ns | 0.0039 ns |   0.0028 ns |   0.0000 ns |   0.0118 ns |   0.002 |    0.00 |      - |     - |     - |         - |
|     DuckTypeInterface |   2.4260 ns | 0.0095 ns | 0.0089 ns |   2.4228 ns |   2.4145 ns |   2.4431 ns |   1.415 |    0.01 |      - |     - |     - |         - |
|      DuckTypeAbstract |   1.7151 ns | 0.0077 ns | 0.0072 ns |   1.7147 ns |   1.7045 ns |   1.7298 ns |   1.000 |    0.00 |      - |     - |     - |         - |
|       DuckTypeVirtual |   1.7322 ns | 0.0082 ns | 0.0077 ns |   1.7330 ns |   1.7175 ns |   1.7472 ns |   1.010 |    0.01 |      - |     - |     - |         - |
| ExpressionTreeFetcher |  48.6474 ns | 0.6056 ns | 0.5665 ns |  48.8195 ns |  46.8674 ns |  49.2839 ns |  28.364 |    0.35 | 0.0067 |     - |     - |     112 B |
|           EmitFetcher |  48.7790 ns | 0.1421 ns | 0.1186 ns |  48.7748 ns |  48.5905 ns |  49.0109 ns |  28.432 |    0.14 | 0.0067 |     - |     - |     112 B |
|       DelegateFetcher |          NA |        NA |        NA |          NA |          NA |          NA |       ? |       ? |      - |     - |     - |         - |
|            Reflection | 351.2802 ns | 0.9316 ns | 0.8258 ns | 351.4524 ns | 349.1993 ns | 352.0959 ns | 204.823 |    0.96 | 0.0091 |     - |     - |     152 B |

### Private Class

#### Private Property / Getter / Object Type
|                Method |       Mean |     Error |    StdDev |        Min |        Max | Ratio | RatioSD | Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------- |-----------:|----------:|----------:|-----------:|-----------:|------:|--------:|------:|------:|------:|----------:|
|                Direct |         NA |        NA |        NA |         NA |         NA |     ? |       ? |     - |     - |     - |         - |
|     DuckTypeInterface |   2.671 ns | 0.0046 ns | 0.0043 ns |   2.666 ns |   2.681 ns |  1.11 |    0.00 |     - |     - |     - |         - |
|      DuckTypeAbstract |   2.408 ns | 0.0077 ns | 0.0072 ns |   2.398 ns |   2.422 ns |  1.00 |    0.00 |     - |     - |     - |         - |
|       DuckTypeVirtual |   2.154 ns | 0.0070 ns | 0.0065 ns |   2.145 ns |   2.166 ns |  0.89 |    0.00 |     - |     - |     - |         - |
| ExpressionTreeFetcher |   4.444 ns | 0.0162 ns | 0.0144 ns |   4.421 ns |   4.469 ns |  1.85 |    0.01 |     - |     - |     - |         - |
|           EmitFetcher |   4.427 ns | 0.0241 ns | 0.0225 ns |   4.398 ns |   4.470 ns |  1.84 |    0.01 |     - |     - |     - |         - |
|       DelegateFetcher |   6.422 ns | 0.0151 ns | 0.0134 ns |   6.402 ns |   6.439 ns |  2.67 |    0.01 |     - |     - |     - |         - |
|            Reflection | 106.718 ns | 0.2564 ns | 0.2273 ns | 106.414 ns | 107.070 ns | 44.31 |    0.16 |     - |     - |     - |         - |

#### Private Property / Getter / Value Type
|                Method |       Mean |     Error |    StdDev |        Min |        Max | Ratio | RatioSD |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------- |-----------:|----------:|----------:|-----------:|-----------:|------:|--------:|-------:|------:|------:|----------:|
|                Direct |         NA |        NA |        NA |         NA |         NA |     ? |       ? |      - |     - |     - |         - |
|     DuckTypeInterface |   2.881 ns | 0.0089 ns | 0.0083 ns |   2.863 ns |   2.891 ns |  1.19 |    0.01 |      - |     - |     - |         - |
|      DuckTypeAbstract |   2.413 ns | 0.0105 ns | 0.0099 ns |   2.388 ns |   2.426 ns |  1.00 |    0.00 |      - |     - |     - |         - |
|       DuckTypeVirtual |   2.415 ns | 0.0088 ns | 0.0074 ns |   2.397 ns |   2.426 ns |  1.00 |    0.01 |      - |     - |     - |         - |
| ExpressionTreeFetcher |  12.329 ns | 0.2724 ns | 0.2548 ns |  11.700 ns |  12.732 ns |  5.11 |    0.12 | 0.0014 |     - |     - |      24 B |
|           EmitFetcher |  12.127 ns | 0.2682 ns | 0.4482 ns |  10.431 ns |  12.552 ns |  4.99 |    0.19 | 0.0014 |     - |     - |      24 B |
|       DelegateFetcher |  14.618 ns | 0.2838 ns | 0.2655 ns |  14.230 ns |  14.984 ns |  6.06 |    0.10 | 0.0014 |     - |     - |      24 B |
|            Reflection | 170.210 ns | 1.0057 ns | 0.9407 ns | 168.702 ns | 171.906 ns | 70.55 |    0.55 | 0.0014 |     - |     - |      24 B |

#### Private Property / Setter / Object Type
|                Method |       Mean |     Error |    StdDev |        Min |        Max | Ratio | RatioSD |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------- |-----------:|----------:|----------:|-----------:|-----------:|------:|--------:|-------:|------:|------:|----------:|
|                Direct |         NA |        NA |        NA |         NA |         NA |     ? |       ? |      - |     - |     - |         - |
|     DuckTypeInterface |   3.967 ns | 0.0965 ns | 0.0903 ns |   3.764 ns |   4.097 ns |  1.09 |    0.02 |      - |     - |     - |         - |
|      DuckTypeAbstract |   3.639 ns | 0.0086 ns | 0.0076 ns |   3.626 ns |   3.652 ns |  1.00 |    0.00 |      - |     - |     - |         - |
|       DuckTypeVirtual |   3.647 ns | 0.0076 ns | 0.0071 ns |   3.638 ns |   3.657 ns |  1.00 |    0.00 |      - |     - |     - |         - |
| ExpressionTreeFetcher |   4.728 ns | 0.1222 ns | 0.1143 ns |   4.503 ns |   4.838 ns |  1.30 |    0.03 |      - |     - |     - |         - |
|           EmitFetcher |   4.765 ns | 0.1213 ns | 0.1075 ns |   4.393 ns |   4.803 ns |  1.31 |    0.03 |      - |     - |     - |         - |
|       DelegateFetcher |  10.804 ns | 0.2419 ns | 0.2971 ns |   9.875 ns |  11.082 ns |  2.97 |    0.09 |      - |     - |     - |         - |
|            Reflection | 190.189 ns | 1.3800 ns | 1.2908 ns | 188.956 ns | 193.633 ns | 52.23 |    0.36 | 0.0038 |     - |     - |      64 B |

#### Private Property / Setter / Value Type
|                Method |       Mean |     Error |    StdDev |        Min |        Max | Ratio | RatioSD |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------- |-----------:|----------:|----------:|-----------:|-----------:|------:|--------:|-------:|------:|------:|----------:|
|                Direct |         NA |        NA |        NA |         NA |         NA |     ? |       ? |      - |     - |     - |         - |
|     DuckTypeInterface |   3.686 ns | 0.0158 ns | 0.0148 ns |   3.660 ns |   3.708 ns |  1.02 |    0.01 |      - |     - |     - |         - |
|      DuckTypeAbstract |   3.612 ns | 0.0239 ns | 0.0224 ns |   3.589 ns |   3.652 ns |  1.00 |    0.00 |      - |     - |     - |         - |
|       DuckTypeVirtual |   3.628 ns | 0.0100 ns | 0.0078 ns |   3.614 ns |   3.639 ns |  1.00 |    0.01 |      - |     - |     - |         - |
| ExpressionTreeFetcher |  11.522 ns | 0.3087 ns | 0.9103 ns |  10.283 ns |  13.249 ns |  3.02 |    0.20 | 0.0014 |     - |     - |      24 B |
|           EmitFetcher |  10.236 ns | 0.2166 ns | 0.4015 ns |   9.821 ns |  11.229 ns |  2.89 |    0.10 | 0.0014 |     - |     - |      24 B |
|       DelegateFetcher |  14.316 ns | 0.1558 ns | 0.1457 ns |  13.995 ns |  14.607 ns |  3.96 |    0.05 | 0.0014 |     - |     - |      24 B |
|            Reflection | 207.938 ns | 1.0840 ns | 1.0139 ns | 206.318 ns | 210.046 ns | 57.57 |    0.50 | 0.0052 |     - |     - |      88 B |

#### Private Field / Getter / Object Type
|                Method |      Mean |     Error |    StdDev |       Min |       Max | Ratio | RatioSD | Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------- |----------:|----------:|----------:|----------:|----------:|------:|--------:|------:|------:|------:|----------:|
|                Direct |        NA |        NA |        NA |        NA |        NA |     ? |       ? |     - |     - |     - |         - |
|     DuckTypeInterface |  3.147 ns | 0.0216 ns | 0.0191 ns |  3.125 ns |  3.180 ns |  1.30 |    0.01 |     - |     - |     - |         - |
|      DuckTypeAbstract |  2.420 ns | 0.0050 ns | 0.0045 ns |  2.408 ns |  2.425 ns |  1.00 |    0.00 |     - |     - |     - |         - |
|       DuckTypeVirtual |  2.407 ns | 0.0151 ns | 0.0126 ns |  2.387 ns |  2.432 ns |  0.99 |    0.01 |     - |     - |     - |         - |
| ExpressionTreeFetcher |  4.480 ns | 0.0549 ns | 0.0487 ns |  4.415 ns |  4.588 ns |  1.85 |    0.02 |     - |     - |     - |         - |
|           EmitFetcher |  4.401 ns | 0.0182 ns | 0.0162 ns |  4.366 ns |  4.428 ns |  1.82 |    0.01 |     - |     - |     - |         - |
|       DelegateFetcher |        NA |        NA |        NA |        NA |        NA |     ? |       ? |     - |     - |     - |         - |
|            Reflection | 49.517 ns | 0.2291 ns | 0.2143 ns | 49.099 ns | 49.789 ns | 20.48 |    0.09 |     - |     - |     - |         - |

#### Private Field / Getter / Value Type
|                Method |      Mean |     Error |    StdDev |       Min |       Max | Ratio | RatioSD |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------- |----------:|----------:|----------:|----------:|----------:|------:|--------:|-------:|------:|------:|----------:|
|                Direct |        NA |        NA |        NA |        NA |        NA |     ? |       ? |      - |     - |     - |         - |
|     DuckTypeInterface |  2.847 ns | 0.0154 ns | 0.0137 ns |  2.814 ns |  2.864 ns |  1.20 |    0.01 |      - |     - |     - |         - |
|      DuckTypeAbstract |  2.381 ns | 0.0164 ns | 0.0137 ns |  2.358 ns |  2.402 ns |  1.00 |    0.00 |      - |     - |     - |         - |
|       DuckTypeVirtual |  2.396 ns | 0.0074 ns | 0.0069 ns |  2.382 ns |  2.408 ns |  1.01 |    0.01 |      - |     - |     - |         - |
| ExpressionTreeFetcher | 11.441 ns | 0.2568 ns | 0.6584 ns | 10.388 ns | 13.203 ns |  4.99 |    0.27 | 0.0014 |     - |     - |      24 B |
|           EmitFetcher | 12.052 ns | 0.2697 ns | 0.4652 ns | 10.618 ns | 12.944 ns |  5.12 |    0.26 | 0.0014 |     - |     - |      24 B |
|       DelegateFetcher |        NA |        NA |        NA |        NA |        NA |     ? |       ? |      - |     - |     - |         - |
|            Reflection | 81.354 ns | 0.9989 ns | 0.8855 ns | 79.770 ns | 82.538 ns | 34.14 |    0.50 | 0.0014 |     - |     - |      24 B |

#### Private Field / Setter / Object Type
|                Method |      Mean |     Error |    StdDev |       Min |       Max | Ratio | RatioSD | Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------- |----------:|----------:|----------:|----------:|----------:|------:|--------:|------:|------:|------:|----------:|
|                Direct |        NA |        NA |        NA |        NA |        NA |     ? |       ? |     - |     - |     - |         - |
|     DuckTypeInterface |  3.605 ns | 0.0140 ns | 0.0117 ns |  3.578 ns |  3.619 ns |  0.99 |    0.00 |     - |     - |     - |         - |
|      DuckTypeAbstract |  3.649 ns | 0.0149 ns | 0.0140 ns |  3.625 ns |  3.675 ns |  1.00 |    0.00 |     - |     - |     - |         - |
|       DuckTypeVirtual |  3.656 ns | 0.0169 ns | 0.0158 ns |  3.629 ns |  3.676 ns |  1.00 |    0.00 |     - |     - |     - |         - |
| ExpressionTreeFetcher |  4.772 ns | 0.0397 ns | 0.0371 ns |  4.715 ns |  4.816 ns |  1.31 |    0.01 |     - |     - |     - |         - |
|           EmitFetcher |  4.822 ns | 0.0209 ns | 0.0174 ns |  4.784 ns |  4.858 ns |  1.32 |    0.01 |     - |     - |     - |         - |
|       DelegateFetcher |        NA |        NA |        NA |        NA |        NA |     ? |       ? |     - |     - |     - |         - |
|            Reflection | 61.724 ns | 0.2886 ns | 0.2559 ns | 61.262 ns | 62.284 ns | 16.91 |    0.10 |     - |     - |     - |         - |

#### Private Field / Setter / Value Type
|                Method |      Mean |     Error |    StdDev |       Min |       Max | Ratio | RatioSD |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------- |----------:|----------:|----------:|----------:|----------:|------:|--------:|-------:|------:|------:|----------:|
|                Direct |        NA |        NA |        NA |        NA |        NA |     ? |       ? |      - |     - |     - |         - |
|     DuckTypeInterface |  3.701 ns | 0.0248 ns | 0.0207 ns |  3.673 ns |  3.729 ns |  1.02 |    0.01 |      - |     - |     - |         - |
|      DuckTypeAbstract |  3.633 ns | 0.0218 ns | 0.0182 ns |  3.605 ns |  3.663 ns |  1.00 |    0.00 |      - |     - |     - |         - |
|       DuckTypeVirtual |  3.635 ns | 0.0262 ns | 0.0232 ns |  3.587 ns |  3.666 ns |  1.00 |    0.01 |      - |     - |     - |         - |
| ExpressionTreeFetcher | 10.347 ns | 0.1572 ns | 0.1393 ns | 10.118 ns | 10.510 ns |  2.85 |    0.04 | 0.0014 |     - |     - |      24 B |
|           EmitFetcher | 10.958 ns | 0.2460 ns | 0.5188 ns | 10.207 ns | 12.344 ns |  3.00 |    0.15 | 0.0014 |     - |     - |      24 B |
|       DelegateFetcher |        NA |        NA |        NA |        NA |        NA |     ? |       ? |      - |     - |     - |         - |
|            Reflection | 72.224 ns | 0.4226 ns | 0.3953 ns | 71.470 ns | 72.778 ns | 19.87 |    0.19 | 0.0014 |     - |     - |      24 B |

#### Private Method / Invoker
|                Method |      Mean |    Error |   StdDev |       Min |       Max | Ratio | RatioSD |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------- |----------:|---------:|---------:|----------:|----------:|------:|--------:|-------:|------:|------:|----------:|
|                Direct |        NA |       NA |       NA |        NA |        NA |     ? |       ? |      - |     - |     - |         - |
|     DuckTypeInterface |  59.46 ns | 0.536 ns | 0.476 ns |  58.59 ns |  60.31 ns |  1.03 |    0.01 | 0.0067 |     - |     - |     112 B |
|      DuckTypeAbstract |  57.49 ns | 0.568 ns | 0.531 ns |  56.77 ns |  58.35 ns |  1.00 |    0.00 | 0.0067 |     - |     - |     112 B |
|       DuckTypeVirtual |  56.76 ns | 0.176 ns | 0.164 ns |  56.50 ns |  56.99 ns |  0.99 |    0.01 | 0.0067 |     - |     - |     112 B |
| ExpressionTreeFetcher |  48.86 ns | 0.301 ns | 0.281 ns |  48.43 ns |  49.32 ns |  0.85 |    0.01 | 0.0067 |     - |     - |     112 B |
|           EmitFetcher |  49.22 ns | 0.565 ns | 0.472 ns |  48.42 ns |  49.90 ns |  0.86 |    0.01 | 0.0067 |     - |     - |     112 B |
|       DelegateFetcher |        NA |       NA |       NA |        NA |        NA |     ? |       ? |      - |     - |     - |         - |
|            Reflection | 355.14 ns | 2.225 ns | 2.082 ns | 351.81 ns | 358.70 ns |  6.18 |    0.06 | 0.0091 |     - |     - |     152 B |

## Powered By
<img src="https://raw.githubusercontent.com/tonyredondo/TWCore2/master/doc/rider.jpg" alt="Rider" width="50px" height="50px" /><img src="https://raw.githubusercontent.com/tonyredondo/TWCore2/master/doc/dotTrace.png" alt="dotTrace" width="50px" height="50px" /><img src="https://raw.githubusercontent.com/tonyredondo/TWCore2/master/doc/dotMemory.png" alt="dotMemory" width="50px" height="50px" />

Thanks to @jetbrains for helping on this development with the licenses for Rider, dotTrace and dotMemory
