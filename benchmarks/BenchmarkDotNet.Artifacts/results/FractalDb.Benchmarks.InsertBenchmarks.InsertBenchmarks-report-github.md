```

BenchmarkDotNet v0.14.0, macOS 26.1 (25B78) [Darwin 25.1.0]
Apple M3 Pro 2.40GHz, 1 CPU, 11 logical and 11 physical cores
.NET SDK 10.0.100
  [Host]     : .NET 10.0.0 (10.0.25.52411), Arm64 RyuJIT AdvSIMD DEBUG
  Job-NPGNZK : .NET 10.0.0 (10.0.25.52411), Arm64 RyuJIT AdvSIMD

InvocationCount=1  IterationCount=5  UnrollFactor=1  
WarmupCount=3  

```
| Method                    | Mean        | Error    | StdDev    | Allocated |
|-------------------------- |------------:|---------:|----------:|----------:|
| InsertSingleDocument      |    373.4 μs | 217.5 μs |  33.66 μs |         - |
| InsertMany_10_Documents   |    571.2 μs | 365.2 μs |  94.85 μs |   51848 B |
| InsertMany_100_Documents  |  1,787.9 μs | 256.2 μs |  66.54 μs |  498256 B |
| InsertMany_1000_Documents | 13,964.8 μs | 447.5 μs | 116.20 μs | 4969464 B |
| Insert_10_Sequential      |  3,497.2 μs | 355.7 μs |  55.05 μs |   49800 B |
