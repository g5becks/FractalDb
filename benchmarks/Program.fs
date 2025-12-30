module FractalDb.Benchmarks.Program

open BenchmarkDotNet.Running
open BenchmarkDotNet.Configs

[<EntryPoint>]
let main args =
    // Configure BenchmarkRunner with default settings
    let config = DefaultConfig.Instance
    
    // Run all benchmarks in the assembly (use a type from our assembly)
    let summary = BenchmarkRunner.Run(typeof<InsertBenchmarks.InsertBenchmarks>.Assembly, config, args)
    
    // Return 0 for success
    0
