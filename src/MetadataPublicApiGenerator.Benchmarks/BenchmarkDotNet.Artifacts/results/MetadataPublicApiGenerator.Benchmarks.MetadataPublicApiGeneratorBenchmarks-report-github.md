``` ini

BenchmarkDotNet=v0.11.5, OS=Windows 10.0.18362
Intel Core i7-8550U CPU 1.80GHz (Kaby Lake R), 1 CPU, 8 logical and 4 physical cores
.NET Core SDK=3.0.100-preview6-012264
  [Host] : .NET Core 2.2.6 (CoreCLR 4.6.27817.03, CoreFX 4.6.27818.02), 64bit RyuJIT
  Core   : .NET Core 2.2.6 (CoreCLR 4.6.27817.03, CoreFX 4.6.27818.02), 64bit RyuJIT

Job=Core  Runtime=Core  

```
|               Method |     Mean |     Error |   StdDev |   Median |      Gen 0 |     Gen 1 | Gen 2 | Allocated |
|--------------------- |---------:|----------:|---------:|---------:|-----------:|----------:|------:|----------:|
| MetadataApiGenerator | 602.9 ms | 21.315 ms | 57.99 ms | 585.1 ms | 21000.0000 | 7000.0000 |     - | 129.97 MB |
|   PublicApiGenerator | 246.3 ms |  9.624 ms | 25.69 ms | 238.7 ms |  4000.0000 | 1000.0000 |     - |   36.5 MB |
