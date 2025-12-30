module FractalDb.Benchmarks.InsertBenchmarks

open BenchmarkDotNet.Attributes
open FractalDb.Types
open FractalDb.Schema
open FractalDb.Operators
open FractalDb.Collection
open FractalDb.Database
open System.IO

type TestDocument = { 
    id: string
    name: string
    age: int
    email: string
    active: bool
}

[<MemoryDiagnoser>]
[<SimpleJob(warmupCount = 3, iterationCount = 5)>]
type InsertBenchmarks() =
    let mutable db: FractalDb = Unchecked.defaultof<_>
    let mutable collection: Collection<TestDocument> = Unchecked.defaultof<_>
    
    let testSchema: SchemaDef<TestDocument> = 
        { Fields =
            [ { Name = "name"
                Path = None
                SqlType = SqliteType.Text
                Indexed = true
                Unique = false
                Nullable = false } ]
          Indexes = []
          Timestamps = true
          Validate = None }
    
    let createDocument i = 
        { id = $"doc-{i}"
          name = $"User {i}"
          age = 20 + (i % 50)
          email = $"user{i}@example.com"
          active = i % 2 = 0 }
    
    [<GlobalSetup>]
    member _.Setup() =
        let dbPath = Path.Combine(Path.GetTempPath(), "fractaldb_bench_insert.db")
        if File.Exists(dbPath) then File.Delete(dbPath)
        db <- FractalDb.Open(dbPath)
        collection <- db.Collection<TestDocument>("test_documents", testSchema)
    
    [<GlobalCleanup>]
    member _.Cleanup() =
        db.Close()
        let dbPath = Path.Combine(Path.GetTempPath(), "fractaldb_bench_insert.db")
        if File.Exists(dbPath) then File.Delete(dbPath)
    
    [<IterationSetup>]
    member _.IterationSetup() =
        // Clear collection before each iteration
        (collection |> Collection.deleteMany Query.Empty).Wait()
    
    [<Benchmark>]
    member _.InsertSingleDocument() =
        let doc = createDocument 1
        (collection |> Collection.insertOne doc).Result
    
    [<Benchmark>]
    member _.InsertMany_10_Documents() =
        let docs = [ for i in 1..10 -> createDocument i ]
        (collection |> Collection.insertMany docs).Result
    
    [<Benchmark>]
    member _.InsertMany_100_Documents() =
        let docs = [ for i in 1..100 -> createDocument i ]
        (collection |> Collection.insertMany docs).Result
    
    [<Benchmark>]
    member _.InsertMany_1000_Documents() =
        let docs = [ for i in 1..1000 -> createDocument i ]
        (collection |> Collection.insertMany docs).Result
    
    [<Benchmark>]
    member _.Insert_10_Sequential() =
        for i in 1..10 do
            (collection |> Collection.insertOne (createDocument i)).Wait()
