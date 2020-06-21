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
|                Direct |  0.0078 ns | 0.0103 ns | 0.0091 ns |  0.0062 ns |  0.0000 ns |  0.0272 ns |  0.005 |    0.01 |     - |     - |     - |         - |
|     DuckTypeInterface |  1.7273 ns | 0.0079 ns | 0.0074 ns |  1.7276 ns |  1.7169 ns |  1.7417 ns |  1.162 |    0.01 |     - |     - |     - |         - |
|      DuckTypeAbstract |  1.4863 ns | 0.0059 ns | 0.0056 ns |  1.4866 ns |  1.4769 ns |  1.4992 ns |  1.000 |    0.00 |     - |     - |     - |         - |
|       DuckTypeVirtual |  1.4930 ns | 0.0103 ns | 0.0096 ns |  1.4904 ns |  1.4819 ns |  1.5116 ns |  1.005 |    0.01 |     - |     - |     - |         - |
| ExpressionTreeFetcher |  3.6071 ns | 0.0817 ns | 0.0764 ns |  3.6240 ns |  3.4560 ns |  3.7141 ns |  2.427 |    0.05 |     - |     - |     - |         - |
|           EmitFetcher |  3.6236 ns | 0.0254 ns | 0.0225 ns |  3.6228 ns |  3.5846 ns |  3.6626 ns |  2.439 |    0.02 |     - |     - |     - |         - |
|       DelegateFetcher |  4.8949 ns | 0.0787 ns | 0.0736 ns |  4.8772 ns |  4.7865 ns |  5.0085 ns |  3.293 |    0.05 |     - |     - |     - |         - |
|            Reflection | 97.2909 ns | 0.4876 ns | 0.4323 ns | 97.1846 ns | 96.7490 ns | 98.0800 ns | 65.480 |    0.37 |     - |     - |     - |         - |

#### Public Property / Getter / Value Type
|                Method |        Mean |     Error |    StdDev |      Median |         Min |         Max |  Ratio | RatioSD |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------- |------------:|----------:|----------:|------------:|------------:|------------:|-------:|--------:|-------:|------:|------:|----------:|
|                Direct |   0.0011 ns | 0.0019 ns | 0.0016 ns |   0.0002 ns |   0.0000 ns |   0.0047 ns |  0.001 |    0.00 |      - |     - |     - |         - |
|     DuckTypeInterface |   1.7318 ns | 0.0113 ns | 0.0106 ns |   1.7339 ns |   1.7108 ns |   1.7485 ns |  1.159 |    0.01 |      - |     - |     - |         - |
|      DuckTypeAbstract |   1.4935 ns | 0.0101 ns | 0.0089 ns |   1.4916 ns |   1.4828 ns |   1.5109 ns |  1.000 |    0.00 |      - |     - |     - |         - |
|       DuckTypeVirtual |   1.7445 ns | 0.0072 ns | 0.0067 ns |   1.7431 ns |   1.7321 ns |   1.7540 ns |  1.168 |    0.01 |      - |     - |     - |         - |
| ExpressionTreeFetcher |   7.3436 ns | 0.0412 ns | 0.0386 ns |   7.3348 ns |   7.2873 ns |   7.4166 ns |  4.915 |    0.04 | 0.0057 |     - |     - |      24 B |
|           EmitFetcher |   7.4185 ns | 0.0997 ns | 0.0884 ns |   7.4367 ns |   7.2267 ns |   7.5268 ns |  4.968 |    0.08 | 0.0057 |     - |     - |      24 B |
|       DelegateFetcher |   7.7182 ns | 0.1043 ns | 0.0975 ns |   7.7528 ns |   7.5012 ns |   7.8365 ns |  5.173 |    0.06 | 0.0057 |     - |     - |      24 B |
|            Reflection | 116.2243 ns | 0.4081 ns | 0.3408 ns | 116.0811 ns | 115.9170 ns | 117.1458 ns | 77.788 |    0.57 | 0.0057 |     - |     - |      24 B |

#### Public Property / Setter / Object Type
|                Method |       Mean |     Error |    StdDev |        Min |        Max | Ratio | RatioSD |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------- |-----------:|----------:|----------:|-----------:|-----------:|------:|--------:|-------:|------:|------:|----------:|
|                Direct |   1.263 ns | 0.0080 ns | 0.0075 ns |   1.249 ns |   1.274 ns |  0.42 |    0.00 |      - |     - |     - |         - |
|     DuckTypeInterface |   3.481 ns | 0.0218 ns | 0.0204 ns |   3.448 ns |   3.510 ns |  1.16 |    0.01 |      - |     - |     - |         - |
|      DuckTypeAbstract |   2.993 ns | 0.0112 ns | 0.0094 ns |   2.978 ns |   3.009 ns |  1.00 |    0.00 |      - |     - |     - |         - |
|       DuckTypeVirtual |   2.994 ns | 0.0516 ns | 0.0483 ns |   2.945 ns |   3.065 ns |  1.00 |    0.02 |      - |     - |     - |         - |
| ExpressionTreeFetcher |   5.296 ns | 0.0710 ns | 0.0665 ns |   5.160 ns |   5.413 ns |  1.77 |    0.02 |      - |     - |     - |         - |
|           EmitFetcher |   5.257 ns | 0.0916 ns | 0.0857 ns |   5.034 ns |   5.394 ns |  1.76 |    0.03 |      - |     - |     - |         - |
|       DelegateFetcher |   5.940 ns | 0.0310 ns | 0.0290 ns |   5.895 ns |   5.982 ns |  1.99 |    0.01 |      - |     - |     - |         - |
|            Reflection | 167.436 ns | 0.8458 ns | 0.7498 ns | 166.444 ns | 169.315 ns | 55.95 |    0.27 | 0.0153 |     - |     - |      64 B |

#### Public Property / Setter / Value Type
|                Method |        Mean |     Error |    StdDev |      Median |         Min |         Max |  Ratio | RatioSD |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------- |------------:|----------:|----------:|------------:|------------:|------------:|-------:|--------:|-------:|------:|------:|----------:|
|                Direct |   0.0042 ns | 0.0072 ns | 0.0056 ns |   0.0022 ns |   0.0000 ns |   0.0182 ns |  0.001 |    0.00 |      - |     - |     - |         - |
|     DuckTypeInterface |   3.9039 ns | 0.0220 ns | 0.0206 ns |   3.9049 ns |   3.8678 ns |   3.9318 ns |  0.903 |    0.00 |      - |     - |     - |         - |
|      DuckTypeAbstract |   4.3205 ns | 0.0151 ns | 0.0134 ns |   4.3185 ns |   4.3004 ns |   4.3532 ns |  1.000 |    0.00 |      - |     - |     - |         - |
|       DuckTypeVirtual |   4.3243 ns | 0.0118 ns | 0.0105 ns |   4.3230 ns |   4.3110 ns |   4.3419 ns |  1.001 |    0.00 |      - |     - |     - |         - |
| ExpressionTreeFetcher |   7.1401 ns | 0.1088 ns | 0.1018 ns |   7.1606 ns |   6.8110 ns |   7.2287 ns |  1.652 |    0.02 | 0.0057 |     - |     - |      24 B |
|           EmitFetcher |   7.1727 ns | 0.1091 ns | 0.0967 ns |   7.2079 ns |   6.9038 ns |   7.2434 ns |  1.660 |    0.03 | 0.0057 |     - |     - |      24 B |
|       DelegateFetcher |   7.2926 ns | 0.0919 ns | 0.0860 ns |   7.2879 ns |   7.1416 ns |   7.4527 ns |  1.689 |    0.02 | 0.0057 |     - |     - |      24 B |
|            Reflection | 180.5746 ns | 1.9102 ns | 1.7868 ns | 180.2398 ns | 177.9484 ns | 183.3530 ns | 41.795 |    0.45 | 0.0210 |     - |     - |      88 B |

#### Public Field / Getter / Object Type
|                Method |       Mean |     Error |    StdDev |     Median |        Min |        Max |  Ratio | RatioSD | Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------- |-----------:|----------:|----------:|-----------:|-----------:|-----------:|-------:|--------:|------:|------:|------:|----------:|
|                Direct |  0.0006 ns | 0.0012 ns | 0.0010 ns |  0.0000 ns |  0.0000 ns |  0.0029 ns |  0.000 |    0.00 |     - |     - |     - |         - |
|     DuckTypeInterface |  2.2185 ns | 0.0062 ns | 0.0055 ns |  2.2180 ns |  2.2084 ns |  2.2284 ns |  1.476 |    0.00 |     - |     - |     - |         - |
|      DuckTypeAbstract |  1.5031 ns | 0.0038 ns | 0.0035 ns |  1.5025 ns |  1.4979 ns |  1.5088 ns |  1.000 |    0.00 |     - |     - |     - |         - |
|       DuckTypeVirtual |  1.4923 ns | 0.0099 ns | 0.0088 ns |  1.4896 ns |  1.4816 ns |  1.5128 ns |  0.993 |    0.01 |     - |     - |     - |         - |
| ExpressionTreeFetcher |  3.2576 ns | 0.0201 ns | 0.0188 ns |  3.2563 ns |  3.2284 ns |  3.2984 ns |  2.167 |    0.01 |     - |     - |     - |         - |
|           EmitFetcher |  3.5869 ns | 0.0292 ns | 0.0273 ns |  3.5820 ns |  3.5440 ns |  3.6293 ns |  2.386 |    0.02 |     - |     - |     - |         - |
|       DelegateFetcher |         NA |        NA |        NA |         NA |         NA |         NA |      ? |       ? |     - |     - |     - |         - |
|            Reflection | 40.4338 ns | 0.1681 ns | 0.1404 ns | 40.4565 ns | 40.1215 ns | 40.6264 ns | 26.895 |    0.11 |     - |     - |     - |         - |

#### Public Field / Getter / Value Type
|                Method |       Mean |     Error |    StdDev |     Median |        Min |        Max |  Ratio | RatioSD |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------- |-----------:|----------:|----------:|-----------:|-----------:|-----------:|-------:|--------:|-------:|------:|------:|----------:|
|                Direct |  0.0060 ns | 0.0059 ns | 0.0055 ns |  0.0038 ns |  0.0000 ns |  0.0158 ns |  0.005 |    0.00 |      - |     - |     - |         - |
|     DuckTypeInterface |  1.7469 ns | 0.0058 ns | 0.0054 ns |  1.7462 ns |  1.7359 ns |  1.7560 ns |  1.399 |    0.01 |      - |     - |     - |         - |
|      DuckTypeAbstract |  1.2482 ns | 0.0048 ns | 0.0043 ns |  1.2492 ns |  1.2388 ns |  1.2553 ns |  1.000 |    0.00 |      - |     - |     - |         - |
|       DuckTypeVirtual |  1.2390 ns | 0.0066 ns | 0.0061 ns |  1.2405 ns |  1.2312 ns |  1.2529 ns |  0.993 |    0.01 |      - |     - |     - |         - |
| ExpressionTreeFetcher |  6.6109 ns | 0.1605 ns | 0.3015 ns |  6.4496 ns |  6.2930 ns |  7.2161 ns |  5.289 |    0.28 | 0.0057 |     - |     - |      24 B |
|           EmitFetcher |  6.2776 ns | 0.1217 ns | 0.1249 ns |  6.2334 ns |  6.1397 ns |  6.5742 ns |  5.045 |    0.10 | 0.0057 |     - |     - |      24 B |
|       DelegateFetcher |         NA |        NA |        NA |         NA |         NA |         NA |      ? |       ? |      - |     - |     - |         - |
|            Reflection | 49.3719 ns | 0.6044 ns | 0.5654 ns | 49.0962 ns | 48.6510 ns | 50.5601 ns | 39.516 |    0.48 | 0.0057 |     - |     - |      24 B |

#### Public Field / Setter / Object Type
|                Method |      Mean |     Error |    StdDev |       Min |       Max | Ratio | RatioSD | Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------- |----------:|----------:|----------:|----------:|----------:|------:|--------:|------:|------:|------:|----------:|
|                Direct |  1.260 ns | 0.0081 ns | 0.0075 ns |  1.250 ns |  1.276 ns |  0.50 |    0.00 |     - |     - |     - |         - |
|     DuckTypeInterface |  2.656 ns | 0.0319 ns | 0.0298 ns |  2.612 ns |  2.717 ns |  1.06 |    0.01 |     - |     - |     - |         - |
|      DuckTypeAbstract |  2.495 ns | 0.0096 ns | 0.0080 ns |  2.480 ns |  2.508 ns |  1.00 |    0.00 |     - |     - |     - |         - |
|       DuckTypeVirtual |  2.352 ns | 0.0297 ns | 0.0232 ns |  2.299 ns |  2.373 ns |  0.94 |    0.01 |     - |     - |     - |         - |
| ExpressionTreeFetcher |  4.945 ns | 0.0189 ns | 0.0158 ns |  4.925 ns |  4.977 ns |  1.98 |    0.01 |     - |     - |     - |         - |
|           EmitFetcher |  4.947 ns | 0.0292 ns | 0.0259 ns |  4.918 ns |  5.013 ns |  1.98 |    0.01 |     - |     - |     - |         - |
|       DelegateFetcher |        NA |        NA |        NA |        NA |        NA |     ? |       ? |     - |     - |     - |         - |
|            Reflection | 58.896 ns | 0.1331 ns | 0.1179 ns | 58.677 ns | 59.054 ns | 23.61 |    0.10 |     - |     - |     - |         - |

#### Public Field / Setter / Value Type
|                Method |       Mean |     Error |    StdDev |     Median |        Min |        Max |  Ratio | RatioSD |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------- |-----------:|----------:|----------:|-----------:|-----------:|-----------:|-------:|--------:|-------:|------:|------:|----------:|
|                Direct |  0.0026 ns | 0.0043 ns | 0.0040 ns |  0.0000 ns |  0.0000 ns |  0.0112 ns |  0.001 |    0.00 |      - |     - |     - |         - |
|     DuckTypeInterface |  3.6817 ns | 0.1898 ns | 0.5595 ns |  3.8977 ns |  1.7354 ns |  3.9805 ns |  0.689 |    0.19 |      - |     - |     - |         - |
|      DuckTypeAbstract |  4.3436 ns | 0.0152 ns | 0.0142 ns |  4.3435 ns |  4.3197 ns |  4.3666 ns |  1.000 |    0.00 |      - |     - |     - |         - |
|       DuckTypeVirtual |  4.3346 ns | 0.0129 ns | 0.0121 ns |  4.3370 ns |  4.3098 ns |  4.3515 ns |  0.998 |    0.00 |      - |     - |     - |         - |
| ExpressionTreeFetcher |  6.7729 ns | 0.0255 ns | 0.0226 ns |  6.7764 ns |  6.7228 ns |  6.8041 ns |  1.559 |    0.01 | 0.0057 |     - |     - |      24 B |
|           EmitFetcher |  6.8498 ns | 0.0437 ns | 0.0387 ns |  6.8462 ns |  6.8000 ns |  6.9299 ns |  1.577 |    0.01 | 0.0057 |     - |     - |      24 B |
|       DelegateFetcher |         NA |        NA |        NA |         NA |         NA |         NA |      ? |       ? |      - |     - |     - |         - |
|            Reflection | 68.4736 ns | 0.2032 ns | 0.1801 ns | 68.4711 ns | 68.1403 ns | 68.7215 ns | 15.761 |    0.08 | 0.0057 |     - |     - |      24 B |

#### Public Method / Invoker
|                Method |        Mean |     Error |    StdDev |         Min |         Max |   Ratio | RatioSD |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------- |------------:|----------:|----------:|------------:|------------:|--------:|--------:|-------:|------:|------:|----------:|
|                Direct |   0.0000 ns | 0.0000 ns | 0.0000 ns |   0.0000 ns |   0.0000 ns |   0.000 |    0.00 |      - |     - |     - |         - |
|     DuckTypeInterface |   2.2393 ns | 0.0179 ns | 0.0158 ns |   2.2163 ns |   2.2742 ns |   1.476 |    0.02 |      - |     - |     - |         - |
|      DuckTypeAbstract |   1.5172 ns | 0.0116 ns | 0.0103 ns |   1.5012 ns |   1.5366 ns |   1.000 |    0.00 |      - |     - |     - |         - |
|       DuckTypeVirtual |   1.5123 ns | 0.0080 ns | 0.0075 ns |   1.5025 ns |   1.5289 ns |   0.997 |    0.01 |      - |     - |     - |         - |
| ExpressionTreeFetcher |  29.5894 ns | 0.5326 ns | 0.4982 ns |  28.3402 ns |  30.2799 ns |  19.539 |    0.35 | 0.0268 |     - |     - |     112 B |
|           EmitFetcher |  26.8521 ns | 0.1879 ns | 0.1666 ns |  26.7144 ns |  27.2641 ns |  17.699 |    0.15 | 0.0268 |     - |     - |     112 B |
|       DelegateFetcher |          NA |        NA |        NA |          NA |          NA |       ? |       ? |      - |     - |     - |         - |
|            Reflection | 240.0391 ns | 0.9571 ns | 0.8485 ns | 238.8628 ns | 241.4760 ns | 158.222 |    1.40 | 0.0362 |     - |     - |     152 B |

### Private Class

#### Private Property / Getter / Object Type
|                Method |      Mean |     Error |    StdDev |       Min |       Max | Ratio | RatioSD | Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------- |----------:|----------:|----------:|----------:|----------:|------:|--------:|------:|------:|------:|----------:|
|                Direct |        NA |        NA |        NA |        NA |        NA |     ? |       ? |     - |     - |     - |         - |
|     DuckTypeInterface |  3.004 ns | 0.0764 ns | 0.0715 ns |  2.941 ns |  3.146 ns |  1.11 |    0.03 |     - |     - |     - |         - |
|      DuckTypeAbstract |  2.712 ns | 0.0111 ns | 0.0104 ns |  2.699 ns |  2.737 ns |  1.00 |    0.00 |     - |     - |     - |         - |
|       DuckTypeVirtual |  2.473 ns | 0.0147 ns | 0.0137 ns |  2.457 ns |  2.498 ns |  0.91 |    0.01 |     - |     - |     - |         - |
| ExpressionTreeFetcher |  3.631 ns | 0.0423 ns | 0.0396 ns |  3.564 ns |  3.698 ns |  1.34 |    0.02 |     - |     - |     - |         - |
|           EmitFetcher |  3.675 ns | 0.0370 ns | 0.0346 ns |  3.592 ns |  3.727 ns |  1.36 |    0.01 |     - |     - |     - |         - |
|       DelegateFetcher |  4.831 ns | 0.0615 ns | 0.0546 ns |  4.737 ns |  4.925 ns |  1.78 |    0.02 |     - |     - |     - |         - |
|            Reflection | 96.302 ns | 0.7098 ns | 0.6640 ns | 95.377 ns | 97.575 ns | 35.51 |    0.30 |     - |     - |     - |         - |

#### Private Property / Getter / Value Type
|                Method |       Mean |     Error |    StdDev |        Min |        Max | Ratio | RatioSD |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------- |-----------:|----------:|----------:|-----------:|-----------:|------:|--------:|-------:|------:|------:|----------:|
|                Direct |         NA |        NA |        NA |         NA |         NA |     ? |       ? |      - |     - |     - |         - |
|     DuckTypeInterface |   2.752 ns | 0.0178 ns | 0.0167 ns |   2.717 ns |   2.777 ns |  1.23 |    0.03 |      - |     - |     - |         - |
|      DuckTypeAbstract |   2.243 ns | 0.0593 ns | 0.0555 ns |   2.197 ns |   2.348 ns |  1.00 |    0.00 |      - |     - |     - |         - |
|       DuckTypeVirtual |   2.235 ns | 0.0114 ns | 0.0107 ns |   2.222 ns |   2.258 ns |  1.00 |    0.02 |      - |     - |     - |         - |
| ExpressionTreeFetcher |   7.366 ns | 0.0441 ns | 0.0391 ns |   7.291 ns |   7.442 ns |  3.28 |    0.08 | 0.0057 |     - |     - |      24 B |
|           EmitFetcher |   7.342 ns | 0.0472 ns | 0.0419 ns |   7.269 ns |   7.411 ns |  3.27 |    0.09 | 0.0057 |     - |     - |      24 B |
|       DelegateFetcher |   7.720 ns | 0.0380 ns | 0.0356 ns |   7.685 ns |   7.784 ns |  3.44 |    0.09 | 0.0057 |     - |     - |      24 B |
|            Reflection | 120.083 ns | 0.3154 ns | 0.2950 ns | 119.549 ns | 120.511 ns | 53.57 |    1.23 | 0.0057 |     - |     - |      24 B |

#### Private Property / Setter / Object Type
|                Method |       Mean |     Error |    StdDev |        Min |        Max | Ratio | RatioSD |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------- |-----------:|----------:|----------:|-----------:|-----------:|------:|--------:|-------:|------:|------:|----------:|
|                Direct |         NA |        NA |        NA |         NA |         NA |     ? |       ? |      - |     - |     - |         - |
|     DuckTypeInterface |   3.865 ns | 0.0149 ns | 0.0139 ns |   3.847 ns |   3.893 ns |  1.30 |    0.01 |      - |     - |     - |         - |
|      DuckTypeAbstract |   2.976 ns | 0.0116 ns | 0.0103 ns |   2.963 ns |   3.000 ns |  1.00 |    0.00 |      - |     - |     - |         - |
|       DuckTypeVirtual |   2.979 ns | 0.0055 ns | 0.0048 ns |   2.969 ns |   2.988 ns |  1.00 |    0.00 |      - |     - |     - |         - |
| ExpressionTreeFetcher |   4.971 ns | 0.0341 ns | 0.0319 ns |   4.927 ns |   5.026 ns |  1.67 |    0.01 |      - |     - |     - |         - |
|           EmitFetcher |   5.281 ns | 0.1012 ns | 0.0946 ns |   5.080 ns |   5.405 ns |  1.77 |    0.03 |      - |     - |     - |         - |
|       DelegateFetcher |   6.035 ns | 0.0907 ns | 0.0848 ns |   5.891 ns |   6.161 ns |  2.03 |    0.03 |      - |     - |     - |         - |
|            Reflection | 176.616 ns | 0.8415 ns | 0.7459 ns | 175.338 ns | 178.082 ns | 59.35 |    0.30 | 0.0153 |     - |     - |      64 B |

#### Private Property / Setter / Value Type
|                Method |       Mean |     Error |    StdDev |        Min |        Max | Ratio | RatioSD |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------- |-----------:|----------:|----------:|-----------:|-----------:|------:|--------:|-------:|------:|------:|----------:|
|                Direct |         NA |        NA |        NA |         NA |         NA |     ? |       ? |      - |     - |     - |         - |
|     DuckTypeInterface |   2.464 ns | 0.0120 ns | 0.0094 ns |   2.449 ns |   2.487 ns |  0.57 |    0.00 |      - |     - |     - |         - |
|      DuckTypeAbstract |   4.313 ns | 0.0232 ns | 0.0217 ns |   4.275 ns |   4.343 ns |  1.00 |    0.00 |      - |     - |     - |         - |
|       DuckTypeVirtual |   4.338 ns | 0.0348 ns | 0.0326 ns |   4.278 ns |   4.387 ns |  1.01 |    0.01 |      - |     - |     - |         - |
| ExpressionTreeFetcher |   6.858 ns | 0.0451 ns | 0.0422 ns |   6.796 ns |   6.942 ns |  1.59 |    0.02 | 0.0057 |     - |     - |      24 B |
|           EmitFetcher |   7.082 ns | 0.1568 ns | 0.1467 ns |   6.922 ns |   7.276 ns |  1.64 |    0.03 | 0.0057 |     - |     - |      24 B |
|       DelegateFetcher |   7.186 ns | 0.0639 ns | 0.0598 ns |   7.115 ns |   7.326 ns |  1.67 |    0.02 | 0.0057 |     - |     - |      24 B |
|            Reflection | 184.552 ns | 0.6198 ns | 0.5798 ns | 183.149 ns | 185.484 ns | 42.79 |    0.22 | 0.0210 |     - |     - |      88 B |

#### Private Field / Getter / Object Type
|                Method |      Mean |     Error |    StdDev |       Min |       Max | Ratio | RatioSD | Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------- |----------:|----------:|----------:|----------:|----------:|------:|--------:|------:|------:|------:|----------:|
|                Direct |        NA |        NA |        NA |        NA |        NA |     ? |       ? |     - |     - |     - |         - |
|     DuckTypeInterface |  2.958 ns | 0.0126 ns | 0.0118 ns |  2.943 ns |  2.985 ns |  1.10 |    0.01 |     - |     - |     - |         - |
|      DuckTypeAbstract |  2.679 ns | 0.0193 ns | 0.0161 ns |  2.653 ns |  2.716 ns |  1.00 |    0.00 |     - |     - |     - |         - |
|       DuckTypeVirtual |  2.721 ns | 0.0121 ns | 0.0113 ns |  2.707 ns |  2.743 ns |  1.02 |    0.01 |     - |     - |     - |         - |
| ExpressionTreeFetcher |  3.570 ns | 0.0238 ns | 0.0222 ns |  3.532 ns |  3.616 ns |  1.33 |    0.01 |     - |     - |     - |         - |
|           EmitFetcher |  3.529 ns | 0.0185 ns | 0.0154 ns |  3.504 ns |  3.554 ns |  1.32 |    0.01 |     - |     - |     - |         - |
|       DelegateFetcher |        NA |        NA |        NA |        NA |        NA |     ? |       ? |     - |     - |     - |         - |
|            Reflection | 37.977 ns | 0.0736 ns | 0.0575 ns | 37.900 ns | 38.070 ns | 14.17 |    0.09 |     - |     - |     - |         - |

#### Private Field / Getter / Value Type
|                Method |      Mean |     Error |    StdDev |    Median |       Min |       Max | Ratio | RatioSD |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------- |----------:|----------:|----------:|----------:|----------:|----------:|------:|--------:|-------:|------:|------:|----------:|
|                Direct |        NA |        NA |        NA |        NA |        NA |        NA |     ? |       ? |      - |     - |     - |         - |
|     DuckTypeInterface |  3.235 ns | 0.0133 ns | 0.0125 ns |  3.232 ns |  3.216 ns |  3.262 ns |  1.29 |    0.01 |      - |     - |     - |         - |
|      DuckTypeAbstract |  2.500 ns | 0.0101 ns | 0.0090 ns |  2.501 ns |  2.482 ns |  2.513 ns |  1.00 |    0.00 |      - |     - |     - |         - |
|       DuckTypeVirtual |  2.492 ns | 0.0248 ns | 0.0232 ns |  2.479 ns |  2.469 ns |  2.537 ns |  1.00 |    0.01 |      - |     - |     - |         - |
| ExpressionTreeFetcher |  6.899 ns | 0.0442 ns | 0.0370 ns |  6.889 ns |  6.833 ns |  6.953 ns |  2.76 |    0.02 | 0.0057 |     - |     - |      24 B |
|           EmitFetcher |  6.722 ns | 0.1639 ns | 0.2502 ns |  6.587 ns |  6.407 ns |  7.140 ns |  2.76 |    0.08 | 0.0057 |     - |     - |      24 B |
|       DelegateFetcher |        NA |        NA |        NA |        NA |        NA |        NA |     ? |       ? |      - |     - |     - |         - |
|            Reflection | 48.613 ns | 0.2391 ns | 0.2236 ns | 48.518 ns | 48.283 ns | 49.044 ns | 19.44 |    0.11 | 0.0057 |     - |     - |      24 B |

#### Private Field / Setter / Object Type
|                Method |      Mean |     Error |    StdDev |       Min |       Max | Ratio | RatioSD | Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------- |----------:|----------:|----------:|----------:|----------:|------:|--------:|------:|------:|------:|----------:|
|                Direct |        NA |        NA |        NA |        NA |        NA |     ? |       ? |     - |     - |     - |         - |
|     DuckTypeInterface |  3.671 ns | 0.0988 ns | 0.0924 ns |  3.558 ns |  3.836 ns |  1.21 |    0.02 |     - |     - |     - |         - |
|      DuckTypeAbstract |  3.035 ns | 0.0366 ns | 0.0306 ns |  2.997 ns |  3.099 ns |  1.00 |    0.00 |     - |     - |     - |         - |
|       DuckTypeVirtual |  3.029 ns | 0.0195 ns | 0.0173 ns |  3.000 ns |  3.065 ns |  1.00 |    0.01 |     - |     - |     - |         - |
| ExpressionTreeFetcher |  4.913 ns | 0.0269 ns | 0.0252 ns |  4.846 ns |  4.951 ns |  1.62 |    0.02 |     - |     - |     - |         - |
|           EmitFetcher |  4.900 ns | 0.0284 ns | 0.0266 ns |  4.846 ns |  4.931 ns |  1.61 |    0.02 |     - |     - |     - |         - |
|       DelegateFetcher |        NA |        NA |        NA |        NA |        NA |     ? |       ? |     - |     - |     - |         - |
|            Reflection | 54.727 ns | 0.1541 ns | 0.1441 ns | 54.378 ns | 55.007 ns | 18.03 |    0.18 |     - |     - |     - |         - |

#### Private Field / Setter / Value Type
|                Method |      Mean |     Error |    StdDev |       Min |       Max | Ratio | RatioSD |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------- |----------:|----------:|----------:|----------:|----------:|------:|--------:|-------:|------:|------:|----------:|
|                Direct |        NA |        NA |        NA |        NA |        NA |     ? |       ? |      - |     - |     - |         - |
|     DuckTypeInterface |  2.748 ns | 0.0185 ns | 0.0164 ns |  2.716 ns |  2.770 ns |  0.63 |    0.01 |      - |     - |     - |         - |
|      DuckTypeAbstract |  4.373 ns | 0.0911 ns | 0.0852 ns |  4.128 ns |  4.498 ns |  1.00 |    0.00 |      - |     - |     - |         - |
|       DuckTypeVirtual |  4.343 ns | 0.0110 ns | 0.0097 ns |  4.329 ns |  4.364 ns |  0.99 |    0.02 |      - |     - |     - |         - |
| ExpressionTreeFetcher |  6.876 ns | 0.0495 ns | 0.0463 ns |  6.820 ns |  6.964 ns |  1.57 |    0.03 | 0.0057 |     - |     - |      24 B |
|           EmitFetcher |  6.790 ns | 0.0417 ns | 0.0369 ns |  6.720 ns |  6.862 ns |  1.55 |    0.03 | 0.0057 |     - |     - |      24 B |
|       DelegateFetcher |        NA |        NA |        NA |        NA |        NA |     ? |       ? |      - |     - |     - |         - |
|            Reflection | 67.798 ns | 0.2852 ns | 0.2529 ns | 67.342 ns | 68.328 ns | 15.52 |    0.32 | 0.0057 |     - |     - |      24 B |

#### Private Method / Invoker
|                Method |      Mean |    Error |   StdDev |       Min |       Max | Ratio | RatioSD |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------- |----------:|---------:|---------:|----------:|----------:|------:|--------:|-------:|------:|------:|----------:|
|                Direct |        NA |       NA |       NA |        NA |        NA |     ? |       ? |      - |     - |     - |         - |
|     DuckTypeInterface |  36.18 ns | 0.495 ns | 0.463 ns |  35.50 ns |  37.14 ns |  1.04 |    0.01 | 0.0268 |     - |     - |     112 B |
|      DuckTypeAbstract |  34.88 ns | 0.166 ns | 0.147 ns |  34.60 ns |  35.12 ns |  1.00 |    0.00 | 0.0268 |     - |     - |     112 B |
|       DuckTypeVirtual |  35.09 ns | 0.121 ns | 0.101 ns |  34.93 ns |  35.26 ns |  1.01 |    0.01 | 0.0268 |     - |     - |     112 B |
| ExpressionTreeFetcher |  27.26 ns | 0.204 ns | 0.181 ns |  26.95 ns |  27.59 ns |  0.78 |    0.01 | 0.0268 |     - |     - |     112 B |
|           EmitFetcher |  28.07 ns | 0.579 ns | 0.568 ns |  27.48 ns |  29.43 ns |  0.81 |    0.02 | 0.0268 |     - |     - |     112 B |
|       DelegateFetcher |        NA |       NA |       NA |        NA |        NA |     ? |       ? |      - |     - |     - |         - |
|            Reflection | 241.25 ns | 1.535 ns | 1.361 ns | 239.59 ns | 244.10 ns |  6.92 |    0.06 | 0.0362 |     - |     - |     152 B |

## Powered By
<img src="https://raw.githubusercontent.com/tonyredondo/TWCore2/master/doc/rider.jpg" alt="Rider" width="50px" height="50px" /><img src="https://raw.githubusercontent.com/tonyredondo/TWCore2/master/doc/dotTrace.png" alt="dotTrace" width="50px" height="50px" /><img src="https://raw.githubusercontent.com/tonyredondo/TWCore2/master/doc/dotMemory.png" alt="dotMemory" width="50px" height="50px" />

Thanks to @jetbrains for helping on this development with the licenses for Rider, dotTrace and dotMemory