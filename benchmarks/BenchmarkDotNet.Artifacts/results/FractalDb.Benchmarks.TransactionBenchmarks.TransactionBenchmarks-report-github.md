```

BenchmarkDotNet v0.14.0, macOS 26.1 (25B78) [Darwin 25.1.0]
Apple M3 Pro 2.40GHz, 1 CPU, 11 logical and 11 physical cores
.NET SDK 10.0.100
  [Host]     : .NET 10.0.0 (10.0.25.52411), Arm64 RyuJIT AdvSIMD DEBUG
  Job-NPGNZK : .NET 10.0.0 (10.0.25.52411), Arm64 RyuJIT AdvSIMD

InvocationCount=1  IterationCount=5  UnrollFactor=1  
WarmupCount=3  

```
| Method                     | Mean       | Error    | StdDev    | Allocated |
|--------------------------- |-----------:|---------:|----------:|----------:|
| Transaction_SingleInsert   |   495.9 μs | 561.5 μs | 145.83 μs |         - |
| Transaction_TwoInserts     |   437.9 μs | 201.7 μs |  52.39 μs |   11632 B |
| Transaction_FiveInserts    |   442.0 μs | 217.3 μs |  56.42 μs |   24792 B |
| Transaction_InsertAndQuery |   438.6 μs | 284.6 μs |  73.90 μs |         - |
| NoTransaction_SingleInsert |   409.3 μs | 280.7 μs |  72.90 μs |         - |
| NoTransaction_TwoInserts   |   756.0 μs | 581.1 μs | 150.91 μs |    9888 B |
| NoTransaction_FiveInserts  | 1,757.5 μs | 521.0 μs |  80.62 μs |   23912 B |
