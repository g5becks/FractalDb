```

BenchmarkDotNet v0.14.0, macOS 26.1 (25B78) [Darwin 25.1.0]
Apple M3 Pro 2.40GHz, 1 CPU, 11 logical and 11 physical cores
.NET SDK 10.0.100
  [Host]     : .NET 10.0.0 (10.0.25.52411), Arm64 RyuJIT AdvSIMD DEBUG
  Job-KDGGIQ : .NET 10.0.0 (10.0.25.52411), Arm64 RyuJIT AdvSIMD

IterationCount=5  WarmupCount=3  

```
| Method                         | Mean        | Error      | StdDev    | Gen0     | Gen1    | Allocated  |
|------------------------------- |------------:|-----------:|----------:|---------:|--------:|-----------:|
| Query_IndexedField_Equality    |    13.84 μs |   0.961 μs |  0.149 μs |   0.4730 |       - |    3.97 KB |
| Query_NonIndexedField_Equality |   390.82 μs |  40.654 μs |  6.291 μs |        - |       - |     4.2 KB |
| Query_Range                    | 1,068.93 μs | 305.721 μs | 79.395 μs |  31.2500 |  7.8125 |  263.78 KB |
| Query_Complex_Nested           | 1,008.09 μs |  73.274 μs | 19.029 μs |  27.3438 |  5.8594 |  233.87 KB |
| Query_Count                    |   395.86 μs |  28.425 μs |  4.399 μs |        - |       - |    2.26 KB |
| Query_WithOptions_Limit        |    39.62 μs |   1.387 μs |  0.360 μs |   1.9531 |       - |   16.08 KB |
| Query_WithOptions_Sort         | 1,185.15 μs |  16.303 μs |  4.234 μs |  46.8750 | 13.6719 |  394.24 KB |
| Query_FindAll                  | 2,538.14 μs | 268.754 μs | 41.590 μs | 140.6250 | 70.3125 | 1177.96 KB |
