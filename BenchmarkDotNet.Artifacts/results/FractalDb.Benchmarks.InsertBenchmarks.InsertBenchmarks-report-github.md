```

BenchmarkDotNet v0.14.0, macOS 26.1 (25B78) [Darwin 25.1.0]
Apple M3 Pro, 1 CPU, 11 logical and 11 physical cores
.NET SDK 10.0.100
  [Host]     : .NET 10.0.0 (10.0.25.52411), Arm64 RyuJIT AdvSIMD DEBUG
  Job-TIGPFI : .NET 10.0.0 (10.0.25.52411), Arm64 RyuJIT AdvSIMD
  Dry        : .NET 10.0.0 (10.0.25.52411), Arm64 RyuJIT AdvSIMD

UnrollFactor=1  

```
| Method               | Job        | InvocationCount | IterationCount | LaunchCount | RunStrategy | WarmupCount | Mean        | Error    | StdDev    | Allocated |
|--------------------- |----------- |---------------- |--------------- |------------ |------------ |------------ |------------:|---------:|----------:|----------:|
| InsertSingleDocument | Job-TIGPFI | 1               | 5              | Default     | Default     | 3           |    663.7 μs | 788.5 μs | 204.78 μs |         - |
| InsertSingleDocument | Dry        | Default         | 1              | 1           | ColdStart   | 1           | 45,340.2 μs |       NA |   0.00 μs |         - |
