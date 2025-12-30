module FractalDb.Benchmarks.QueryBenchmarks

open BenchmarkDotNet.Attributes
open FractalDb.Types
open FractalDb.Schema
open FractalDb.Operators
open FractalDb.Options
open FractalDb.Collection
open FractalDb.Database
open System.IO

type TestDocument = { 
    id: string
    name: string
    age: int
    email: string
    status: string
    score: int
}

[<MemoryDiagnoser>]
[<SimpleJob(warmupCount = 3, iterationCount = 5)>]
type QueryBenchmarks() =
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
          status = if i % 3 = 0 then "active" else "inactive"
          score = i * 10 }
    
    [<GlobalSetup>]
    member _.Setup() =
        let dbPath = Path.Combine(Path.GetTempPath(), "fractaldb_bench_query.db")
        if File.Exists(dbPath) then File.Delete(dbPath)
        db <- FractalDb.Open(dbPath)
        collection <- db.Collection<TestDocument>("test_documents", testSchema)
        
        // Insert 1000 documents for querying
        let docs = [ for i in 1..1000 -> createDocument i ]
        (collection |> Collection.insertMany docs).Wait()
    
    [<GlobalCleanup>]
    member _.Cleanup() =
        db.Close()
        let dbPath = Path.Combine(Path.GetTempPath(), "fractaldb_bench_query.db")
        if File.Exists(dbPath) then File.Delete(dbPath)
    
    [<Benchmark>]
    member _.Query_IndexedField_Equality() =
        // Query on indexed field (name)
        let query = Query.Field("name", FieldOp.Compare(box (CompareOp.Eq "User 500")))
        (collection |> Collection.find query).Result
    
    [<Benchmark>]
    member _.Query_NonIndexedField_Equality() =
        // Query on non-indexed field (email)
        let query = Query.Field("email", FieldOp.Compare(box (CompareOp.Eq "user500@example.com")))
        (collection |> Collection.find query).Result
    
    [<Benchmark>]
    member _.Query_Range() =
        // Range query on age
        let query = Query.And [
            Query.Field("age", FieldOp.Compare(box (CompareOp.Gte 30)))
            Query.Field("age", FieldOp.Compare(box (CompareOp.Lte 40)))
        ]
        (collection |> Collection.find query).Result
    
    [<Benchmark>]
    member _.Query_Complex_Nested() =
        // Complex nested query: (name = "User 500" OR age > 40) AND status = "active"
        let query = Query.And [
            Query.Or [
                Query.Field("name", FieldOp.Compare(box (CompareOp.Eq "User 500")))
                Query.Field("age", FieldOp.Compare(box (CompareOp.Gt 40)))
            ]
            Query.Field("status", FieldOp.Compare(box (CompareOp.Eq "active")))
        ]
        (collection |> Collection.find query).Result
    
    [<Benchmark>]
    member _.Query_Count() =
        let query = Query.Field("status", FieldOp.Compare(box (CompareOp.Eq "active")))
        (collection |> Collection.count query).Result
    
    [<Benchmark>]
    member _.Query_WithOptions_Limit() =
        let query = Query.Field("age", FieldOp.Compare(box (CompareOp.Gte 25)))
        let options = { QueryOptions.empty with Limit = Some 10 }
        (collection |> Collection.findWith query options).Result
    
    [<Benchmark>]
    member _.Query_WithOptions_Sort() =
        let query = Query.Field("status", FieldOp.Compare(box (CompareOp.Eq "active")))
        let options = { QueryOptions.empty with Sort = [("age", SortDirection.Ascending)] }
        (collection |> Collection.findWith query options).Result
    
    [<Benchmark>]
    member _.Query_FindAll() =
        (collection |> Collection.find Query.Empty).Result
