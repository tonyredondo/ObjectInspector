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
|                Direct |  0.0003 ns | 0.0006 ns | 0.0006 ns |  0.0000 ns |  0.0000 ns |  0.0015 ns |  0.000 |    0.00 |     - |     - |     - |         - |
|              DuckType |  2.2143 ns | 0.0055 ns | 0.0049 ns |  2.2155 ns |  2.2038 ns |  2.2195 ns |  1.000 |    0.00 |     - |     - |     - |         - |
| ExpressionTreeFetcher |  3.4703 ns | 0.0477 ns | 0.0446 ns |  3.4697 ns |  3.3974 ns |  3.5489 ns |  1.567 |    0.02 |     - |     - |     - |         - |
|           EmitFetcher |  3.4944 ns | 0.0780 ns | 0.0729 ns |  3.5002 ns |  3.3965 ns |  3.6691 ns |  1.577 |    0.03 |     - |     - |     - |         - |
|       DelegateFetcher |  5.0758 ns | 0.0636 ns | 0.0595 ns |  5.0816 ns |  4.9652 ns |  5.1827 ns |  2.292 |    0.03 |     - |     - |     - |         - |
|            Reflection | 90.5687 ns | 0.1199 ns | 0.1002 ns | 90.5632 ns | 90.4181 ns | 90.7815 ns | 40.887 |    0.09 |     - |     - |     - |         - |

#### Public Property / Getter / Value Type
|                Method |        Mean |     Error |    StdDev |      Median |         Min |         Max |  Ratio | RatioSD |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------- |------------:|----------:|----------:|------------:|------------:|------------:|-------:|--------:|-------:|------:|------:|----------:|
|                Direct |   0.0005 ns | 0.0009 ns | 0.0008 ns |   0.0000 ns |   0.0000 ns |   0.0029 ns |  0.000 |    0.00 |      - |     - |     - |         - |
|              DuckType |   1.7137 ns | 0.0023 ns | 0.0021 ns |   1.7140 ns |   1.7093 ns |   1.7166 ns |  1.000 |    0.00 |      - |     - |     - |         - |
| ExpressionTreeFetcher |   7.7485 ns | 0.0898 ns | 0.0840 ns |   7.7053 ns |   7.6594 ns |   7.8777 ns |  4.524 |    0.05 | 0.0057 |     - |     - |      24 B |
|           EmitFetcher |   7.3599 ns | 0.0209 ns | 0.0175 ns |   7.3622 ns |   7.3238 ns |   7.3919 ns |  4.295 |    0.01 | 0.0057 |     - |     - |      24 B |
|       DelegateFetcher |   7.5484 ns | 0.1571 ns | 0.1392 ns |   7.4919 ns |   7.4229 ns |   7.8198 ns |  4.405 |    0.08 | 0.0057 |     - |     - |      24 B |
|            Reflection | 123.3160 ns | 0.4250 ns | 0.3975 ns | 123.3559 ns | 122.1645 ns | 123.9097 ns | 71.954 |    0.24 | 0.0057 |     - |     - |      24 B |

#### Public Property / Setter / Object Type
|                Method |       Mean |     Error |    StdDev |        Min |        Max | Ratio | RatioSD |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------- |-----------:|----------:|----------:|-----------:|-----------:|------:|--------:|-------:|------:|------:|----------:|
|                Direct |   1.480 ns | 0.0042 ns | 0.0040 ns |   1.472 ns |   1.487 ns |  0.54 |    0.00 |      - |     - |     - |         - |
|              DuckType |   2.716 ns | 0.0063 ns | 0.0056 ns |   2.707 ns |   2.725 ns |  1.00 |    0.00 |      - |     - |     - |         - |
| ExpressionTreeFetcher |   4.930 ns | 0.0636 ns | 0.0595 ns |   4.782 ns |   5.035 ns |  1.82 |    0.02 |      - |     - |     - |         - |
|           EmitFetcher |   4.905 ns | 0.0706 ns | 0.0661 ns |   4.786 ns |   5.003 ns |  1.80 |    0.02 |      - |     - |     - |         - |
|       DelegateFetcher |   5.848 ns | 0.0651 ns | 0.0577 ns |   5.764 ns |   5.978 ns |  2.15 |    0.02 |      - |     - |     - |         - |
|            Reflection | 153.585 ns | 1.0538 ns | 0.9857 ns | 152.453 ns | 155.419 ns | 56.57 |    0.34 | 0.0153 |     - |     - |      64 B |

#### Public Property / Setter / Value Type
|                Method |        Mean |     Error |    StdDev |      Median |         Min |         Max |  Ratio | RatioSD |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------- |------------:|----------:|----------:|------------:|------------:|------------:|-------:|--------:|-------:|------:|------:|----------:|
|                Direct |   0.0003 ns | 0.0005 ns | 0.0005 ns |   0.0000 ns |   0.0000 ns |   0.0014 ns |  0.000 |    0.00 |      - |     - |     - |         - |
|              DuckType |   3.8308 ns | 0.0072 ns | 0.0064 ns |   3.8298 ns |   3.8209 ns |   3.8399 ns |  1.000 |    0.00 |      - |     - |     - |         - |
| ExpressionTreeFetcher |   6.8111 ns | 0.0428 ns | 0.0357 ns |   6.8229 ns |   6.7378 ns |   6.8552 ns |  1.778 |    0.01 | 0.0057 |     - |     - |      24 B |
|           EmitFetcher |   7.0808 ns | 0.0598 ns | 0.0499 ns |   7.0902 ns |   6.9320 ns |   7.1448 ns |  1.849 |    0.01 | 0.0057 |     - |     - |      24 B |
|       DelegateFetcher |   7.1382 ns | 0.0598 ns | 0.0499 ns |   7.1398 ns |   7.0337 ns |   7.2245 ns |  1.864 |    0.01 | 0.0057 |     - |     - |      24 B |
|            Reflection | 172.1306 ns | 0.9076 ns | 0.8490 ns | 172.1455 ns | 170.6720 ns | 173.6074 ns | 44.929 |    0.26 | 0.0210 |     - |     - |      88 B |

#### Public Field / Getter / Object Type
|                Method |       Mean |     Error |    StdDev |     Median |        Min |        Max |  Ratio | RatioSD | Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------- |-----------:|----------:|----------:|-----------:|-----------:|-----------:|-------:|--------:|------:|------:|------:|----------:|
|                Direct |  0.0012 ns | 0.0008 ns | 0.0006 ns |  0.0014 ns |  0.0000 ns |  0.0021 ns |  0.001 |    0.00 |     - |     - |     - |         - |
|              DuckType |  2.2139 ns | 0.0039 ns | 0.0036 ns |  2.2141 ns |  2.2071 ns |  2.2195 ns |  1.000 |    0.00 |     - |     - |     - |         - |
| ExpressionTreeFetcher |  3.5530 ns | 0.0252 ns | 0.0236 ns |  3.5564 ns |  3.5147 ns |  3.6064 ns |  1.605 |    0.01 |     - |     - |     - |         - |
|           EmitFetcher |  3.5054 ns | 0.0653 ns | 0.0578 ns |  3.4973 ns |  3.4188 ns |  3.6080 ns |  1.584 |    0.03 |     - |     - |     - |         - |
|       DelegateFetcher |         NA |        NA |        NA |         NA |         NA |         NA |      ? |       ? |     - |     - |     - |         - |
|            Reflection | 37.2901 ns | 0.0964 ns | 0.0855 ns | 37.2896 ns | 37.0987 ns | 37.4011 ns | 16.846 |    0.04 |     - |     - |     - |         - |

#### Public Field / Getter / Value Type
|                Method |       Mean |     Error |    StdDev |     Median |        Min |        Max |  Ratio | RatioSD |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------- |-----------:|----------:|----------:|-----------:|-----------:|-----------:|-------:|--------:|-------:|------:|------:|----------:|
|                Direct |  0.0007 ns | 0.0012 ns | 0.0011 ns |  0.0000 ns |  0.0000 ns |  0.0027 ns |  0.000 |    0.00 |      - |     - |     - |         - |
|              DuckType |  1.7205 ns | 0.0031 ns | 0.0026 ns |  1.7202 ns |  1.7178 ns |  1.7272 ns |  1.000 |    0.00 |      - |     - |     - |         - |
| ExpressionTreeFetcher |  6.4570 ns | 0.0198 ns | 0.0155 ns |  6.4561 ns |  6.4388 ns |  6.4891 ns |  3.753 |    0.01 | 0.0057 |     - |     - |      24 B |
|           EmitFetcher |  7.1592 ns | 0.0396 ns | 0.0370 ns |  7.1509 ns |  7.1062 ns |  7.2304 ns |  4.164 |    0.02 | 0.0057 |     - |     - |      24 B |
|       DelegateFetcher |         NA |        NA |        NA |         NA |         NA |         NA |      ? |       ? |      - |     - |     - |         - |
|            Reflection | 51.3539 ns | 0.1701 ns | 0.2747 ns | 51.3883 ns | 50.9071 ns | 51.8365 ns | 29.856 |    0.16 | 0.0057 |     - |     - |      24 B |

#### Public Field / Setter / Object Type
|                Method |      Mean |     Error |    StdDev |       Min |       Max | Ratio | RatioSD | Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------- |----------:|----------:|----------:|----------:|----------:|------:|--------:|------:|------:|------:|----------:|
|                Direct |  1.228 ns | 0.0028 ns | 0.0025 ns |  1.225 ns |  1.234 ns |  0.50 |    0.00 |     - |     - |     - |         - |
|              DuckType |  2.460 ns | 0.0057 ns | 0.0047 ns |  2.452 ns |  2.468 ns |  1.00 |    0.00 |     - |     - |     - |         - |
| ExpressionTreeFetcher |  4.736 ns | 0.0616 ns | 0.0515 ns |  4.637 ns |  4.829 ns |  1.93 |    0.02 |     - |     - |     - |         - |
|           EmitFetcher |  4.767 ns | 0.0513 ns | 0.0429 ns |  4.708 ns |  4.873 ns |  1.94 |    0.02 |     - |     - |     - |         - |
|       DelegateFetcher |        NA |        NA |        NA |        NA |        NA |     ? |       ? |     - |     - |     - |         - |
|            Reflection | 60.060 ns | 0.0957 ns | 0.0848 ns | 59.865 ns | 60.173 ns | 24.42 |    0.07 |     - |     - |     - |         - |

#### Public Field / Setter / Value Type
|                Method |       Mean |     Error |    StdDev |     Median |        Min |        Max |  Ratio | RatioSD |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------- |-----------:|----------:|----------:|-----------:|-----------:|-----------:|-------:|--------:|-------:|------:|------:|----------:|
|                Direct |  0.0000 ns | 0.0002 ns | 0.0001 ns |  0.0000 ns |  0.0000 ns |  0.0004 ns |  0.000 |    0.00 |      - |     - |     - |         - |
|              DuckType |  3.8203 ns | 0.0028 ns | 0.0024 ns |  3.8205 ns |  3.8141 ns |  3.8232 ns |  1.000 |    0.00 |      - |     - |     - |         - |
| ExpressionTreeFetcher |  6.7743 ns | 0.0845 ns | 0.1649 ns |  6.6911 ns |  6.6281 ns |  7.2628 ns |  1.807 |    0.05 | 0.0057 |     - |     - |      24 B |
|           EmitFetcher |  6.9073 ns | 0.0919 ns | 0.0814 ns |  6.9135 ns |  6.7838 ns |  7.0579 ns |  1.808 |    0.02 | 0.0057 |     - |     - |      24 B |
|       DelegateFetcher |         NA |        NA |        NA |         NA |         NA |         NA |      ? |       ? |      - |     - |     - |         - |
|            Reflection | 67.7958 ns | 0.3516 ns | 0.3289 ns | 67.7573 ns | 67.3496 ns | 68.4705 ns | 17.750 |    0.09 | 0.0057 |     - |     - |      24 B |


#### Public Method / Invoker
|                Method |        Mean |     Error |    StdDev |      Median |         Min |         Max |   Ratio | RatioSD |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------- |------------:|----------:|----------:|------------:|------------:|------------:|--------:|--------:|-------:|------:|------:|----------:|
|                Direct |   0.0007 ns | 0.0012 ns | 0.0009 ns |   0.0000 ns |   0.0000 ns |   0.0026 ns |   0.000 |    0.00 |      - |     - |     - |         - |
|              DuckType |   2.2199 ns | 0.0052 ns | 0.0046 ns |   2.2196 ns |   2.2120 ns |   2.2257 ns |   1.000 |    0.00 |      - |     - |     - |         - |
| ExpressionTreeFetcher |  28.4919 ns | 0.2601 ns | 0.2306 ns |  28.4825 ns |  28.2234 ns |  28.9457 ns |  12.835 |    0.10 | 0.0268 |     - |     - |     112 B |
|           EmitFetcher |  26.9525 ns | 0.1893 ns | 0.1771 ns |  26.8833 ns |  26.7087 ns |  27.2355 ns |  12.136 |    0.08 | 0.0268 |     - |     - |     112 B |
|       DelegateFetcher |          NA |        NA |        NA |          NA |          NA |          NA |       ? |       ? |      - |     - |     - |         - |
|            Reflection | 238.9645 ns | 2.8059 ns | 2.6246 ns | 238.4335 ns | 235.1679 ns | 243.5034 ns | 107.688 |    1.33 | 0.0362 |     - |     - |     152 B |

### Private Class

#### Private Property / Getter / Object Type
|                Method |      Mean |     Error |    StdDev |       Min |       Max | Ratio | RatioSD | Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------- |----------:|----------:|----------:|----------:|----------:|------:|--------:|------:|------:|------:|----------:|
|                Direct |        NA |        NA |        NA |        NA |        NA |     ? |       ? |     - |     - |     - |         - |
|              DuckType |  2.960 ns | 0.0195 ns | 0.0163 ns |  2.943 ns |  3.003 ns |  1.00 |    0.00 |     - |     - |     - |         - |
| ExpressionTreeFetcher |  3.705 ns | 0.0546 ns | 0.0511 ns |  3.606 ns |  3.786 ns |  1.25 |    0.02 |     - |     - |     - |         - |
|           EmitFetcher |  3.502 ns | 0.0177 ns | 0.0157 ns |  3.476 ns |  3.526 ns |  1.18 |    0.01 |     - |     - |     - |         - |
|       DelegateFetcher |  5.161 ns | 0.1267 ns | 0.1408 ns |  4.938 ns |  5.391 ns |  1.75 |    0.06 |     - |     - |     - |         - |
|            Reflection | 93.862 ns | 0.2188 ns | 0.2046 ns | 93.512 ns | 94.119 ns | 31.71 |    0.18 |     - |     - |     - |         - |

#### Private Property / Getter / Value Type
|                Method |       Mean |     Error |    StdDev |        Min |        Max | Ratio | RatioSD |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------- |-----------:|----------:|----------:|-----------:|-----------:|------:|--------:|-------:|------:|------:|----------:|
|                Direct |         NA |        NA |        NA |         NA |         NA |     ? |       ? |      - |     - |     - |         - |
|              DuckType |   2.684 ns | 0.0174 ns | 0.0162 ns |   2.665 ns |   2.715 ns |  1.00 |    0.00 |      - |     - |     - |         - |
| ExpressionTreeFetcher |   6.959 ns | 0.1213 ns | 0.1135 ns |   6.698 ns |   7.036 ns |  2.59 |    0.04 | 0.0057 |     - |     - |      24 B |
|           EmitFetcher |   7.321 ns | 0.0571 ns | 0.0534 ns |   7.259 ns |   7.406 ns |  2.73 |    0.01 | 0.0057 |     - |     - |      24 B |
|       DelegateFetcher |   7.470 ns | 0.0311 ns | 0.0276 ns |   7.428 ns |   7.509 ns |  2.78 |    0.02 | 0.0057 |     - |     - |      24 B |
|            Reflection | 112.011 ns | 0.1858 ns | 0.1647 ns | 111.653 ns | 112.314 ns | 41.75 |    0.27 | 0.0057 |     - |     - |      24 B |

#### Private Property / Setter / Object Type
|                Method |       Mean |     Error |    StdDev |        Min |        Max | Ratio | RatioSD |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------- |-----------:|----------:|----------:|-----------:|-----------:|------:|--------:|-------:|------:|------:|----------:|
|                Direct |         NA |        NA |        NA |         NA |         NA |     ? |       ? |      - |     - |     - |         - |
|              DuckType |   3.934 ns | 0.0039 ns | 0.0037 ns |   3.926 ns |   3.940 ns |  1.00 |    0.00 |      - |     - |     - |         - |
| ExpressionTreeFetcher |   5.081 ns | 0.0459 ns | 0.0429 ns |   5.024 ns |   5.159 ns |  1.29 |    0.01 |      - |     - |     - |         - |
|           EmitFetcher |   4.898 ns | 0.0605 ns | 0.0565 ns |   4.807 ns |   4.987 ns |  1.25 |    0.01 |      - |     - |     - |         - |
|       DelegateFetcher |   5.939 ns | 0.0767 ns | 0.0718 ns |   5.840 ns |   6.036 ns |  1.51 |    0.02 |      - |     - |     - |         - |
|            Reflection | 161.647 ns | 0.5268 ns | 0.4670 ns | 160.533 ns | 162.355 ns | 41.09 |    0.12 | 0.0153 |     - |     - |      64 B |

#### Private Property / Setter / Value Type
|                Method |       Mean |     Error |    StdDev |        Min |        Max | Ratio | RatioSD |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------- |-----------:|----------:|----------:|-----------:|-----------:|------:|--------:|-------:|------:|------:|----------:|
|                Direct |         NA |        NA |        NA |         NA |         NA |     ? |       ? |      - |     - |     - |         - |
|              DuckType |   5.804 ns | 0.0330 ns | 0.0276 ns |   5.746 ns |   5.842 ns |  1.00 |    0.00 | 0.0057 |     - |     - |      24 B |
| ExpressionTreeFetcher |   6.990 ns | 0.0924 ns | 0.0864 ns |   6.857 ns |   7.119 ns |  1.20 |    0.02 | 0.0057 |     - |     - |      24 B |
|           EmitFetcher |   6.669 ns | 0.0387 ns | 0.0362 ns |   6.612 ns |   6.742 ns |  1.15 |    0.01 | 0.0057 |     - |     - |      24 B |
|       DelegateFetcher |   7.176 ns | 0.0592 ns | 0.0554 ns |   7.072 ns |   7.276 ns |  1.24 |    0.01 | 0.0057 |     - |     - |      24 B |
|            Reflection | 176.083 ns | 2.2643 ns | 2.1180 ns | 173.121 ns | 180.202 ns | 30.36 |    0.41 | 0.0210 |     - |     - |      88 B |

#### Private Field / Getter / Object Type
|                Method |      Mean |     Error |    StdDev |       Min |       Max | Ratio | RatioSD | Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------- |----------:|----------:|----------:|----------:|----------:|------:|--------:|------:|------:|------:|----------:|
|                Direct |        NA |        NA |        NA |        NA |        NA |     ? |       ? |     - |     - |     - |         - |
|              DuckType |  2.918 ns | 0.0074 ns | 0.0058 ns |  2.908 ns |  2.928 ns |  1.00 |    0.00 |     - |     - |     - |         - |
| ExpressionTreeFetcher |  3.107 ns | 0.0136 ns | 0.0121 ns |  3.078 ns |  3.117 ns |  1.06 |    0.00 |     - |     - |     - |         - |
|           EmitFetcher |  3.140 ns | 0.0160 ns | 0.0150 ns |  3.119 ns |  3.168 ns |  1.08 |    0.01 |     - |     - |     - |         - |
|       DelegateFetcher |        NA |        NA |        NA |        NA |        NA |     ? |       ? |     - |     - |     - |         - |
|            Reflection | 38.430 ns | 0.7969 ns | 0.6655 ns | 38.169 ns | 40.626 ns | 13.17 |    0.25 |     - |     - |     - |         - |

#### Private Field / Getter / Value Type
|                Method |      Mean |     Error |    StdDev |       Min |       Max | Ratio | RatioSD |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------- |----------:|----------:|----------:|----------:|----------:|------:|--------:|-------:|------:|------:|----------:|
|                Direct |        NA |        NA |        NA |        NA |        NA |     ? |       ? |      - |     - |     - |         - |
|              DuckType |  2.715 ns | 0.0083 ns | 0.0078 ns |  2.704 ns |  2.730 ns |  1.00 |    0.00 |      - |     - |     - |         - |
| ExpressionTreeFetcher |  6.058 ns | 0.0484 ns | 0.0453 ns |  5.938 ns |  6.119 ns |  2.23 |    0.02 | 0.0057 |     - |     - |      24 B |
|           EmitFetcher |  7.129 ns | 0.0981 ns | 0.0918 ns |  7.035 ns |  7.322 ns |  2.63 |    0.03 | 0.0057 |     - |     - |      24 B |
|       DelegateFetcher |        NA |        NA |        NA |        NA |        NA |     ? |       ? |      - |     - |     - |         - |
|            Reflection | 49.546 ns | 0.6772 ns | 1.0543 ns | 48.627 ns | 53.299 ns | 18.36 |    0.46 | 0.0057 |     - |     - |      24 B |

#### Private Field / Setter / Object Type
|                Method |      Mean |     Error |    StdDev |       Min |       Max | Ratio | RatioSD | Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------- |----------:|----------:|----------:|----------:|----------:|------:|--------:|------:|------:|------:|----------:|
|                Direct |        NA |        NA |        NA |        NA |        NA |     ? |       ? |     - |     - |     - |         - |
|              DuckType |  3.713 ns | 0.0276 ns | 0.0258 ns |  3.685 ns |  3.777 ns |  1.00 |    0.00 |     - |     - |     - |         - |
| ExpressionTreeFetcher |  4.916 ns | 0.0189 ns | 0.0168 ns |  4.879 ns |  4.942 ns |  1.33 |    0.01 |     - |     - |     - |         - |
|           EmitFetcher |  4.879 ns | 0.1120 ns | 0.1047 ns |  4.708 ns |  4.989 ns |  1.31 |    0.03 |     - |     - |     - |         - |
|       DelegateFetcher |        NA |        NA |        NA |        NA |        NA |     ? |       ? |     - |     - |     - |         - |
|            Reflection | 55.703 ns | 0.4668 ns | 0.3645 ns | 55.199 ns | 56.401 ns | 15.01 |    0.12 |     - |     - |     - |         - |

#### Private Field / Setter / Value Type
|                Method |      Mean |     Error |    StdDev |       Min |       Max | Ratio | RatioSD |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------- |----------:|----------:|----------:|----------:|----------:|------:|--------:|-------:|------:|------:|----------:|
|                Direct |        NA |        NA |        NA |        NA |        NA |     ? |       ? |      - |     - |     - |         - |
|              DuckType |  2.483 ns | 0.0123 ns | 0.0109 ns |  2.470 ns |  2.505 ns |  1.00 |    0.00 |      - |     - |     - |         - |
| ExpressionTreeFetcher |  6.842 ns | 0.0767 ns | 0.0717 ns |  6.730 ns |  6.954 ns |  2.75 |    0.03 | 0.0057 |     - |     - |      24 B |
|           EmitFetcher |  6.699 ns | 0.0518 ns | 0.0485 ns |  6.638 ns |  6.792 ns |  2.70 |    0.02 | 0.0057 |     - |     - |      24 B |
|       DelegateFetcher |        NA |        NA |        NA |        NA |        NA |     ? |       ? |      - |     - |     - |         - |
|            Reflection | 68.085 ns | 0.2334 ns | 0.2069 ns | 67.889 ns | 68.624 ns | 27.42 |    0.16 | 0.0057 |     - |     - |      24 B |

#### Private Method / Invoker
|                Method |      Mean |    Error |   StdDev |       Min |       Max | Ratio | RatioSD |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------- |----------:|---------:|---------:|----------:|----------:|------:|--------:|-------:|------:|------:|----------:|
|                Direct |        NA |       NA |       NA |        NA |        NA |     ? |       ? |      - |     - |     - |         - |
|              DuckType |  36.83 ns | 0.479 ns | 0.448 ns |  35.92 ns |  37.50 ns |  1.00 |    0.00 | 0.0268 |     - |     - |     112 B |
| ExpressionTreeFetcher |  33.70 ns | 0.641 ns | 0.600 ns |  33.02 ns |  34.78 ns |  0.92 |    0.02 | 0.0268 |     - |     - |     112 B |
|           EmitFetcher |  34.72 ns | 0.145 ns | 0.135 ns |  34.41 ns |  34.88 ns |  0.94 |    0.01 | 0.0268 |     - |     - |     112 B |
|       DelegateFetcher |        NA |       NA |       NA |        NA |        NA |     ? |       ? |      - |     - |     - |         - |
|            Reflection | 238.71 ns | 1.310 ns | 1.161 ns | 236.47 ns | 241.03 ns |  6.48 |    0.09 | 0.0362 |     - |     - |     152 B |


## Powered By
<img src="https://raw.githubusercontent.com/tonyredondo/TWCore2/master/doc/rider.jpg" alt="Rider" width="50px" height="50px" /><img src="https://raw.githubusercontent.com/tonyredondo/TWCore2/master/doc/dotTrace.png" alt="dotTrace" width="50px" height="50px" /><img src="https://raw.githubusercontent.com/tonyredondo/TWCore2/master/doc/dotMemory.png" alt="dotMemory" width="50px" height="50px" />

Thanks to @jetbrains for helping on this development with the licenses for Rider, dotTrace and dotMemory