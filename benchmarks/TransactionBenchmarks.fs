module FractalDb.Benchmarks.TransactionBenchmarks

open BenchmarkDotNet.Attributes
open FractalDb.Types
open FractalDb.Schema
open FractalDb.Operators
open FractalDb.Collection
open FractalDb.Database
open FractalDb.Builders
open System.IO

type TestDocument = { 
    id: string
    name: string
    value: int
}

[<MemoryDiagnoser>]
[<SimpleJob(warmupCount = 3, iterationCount = 5)>]
type TransactionBenchmarks() =
    let mutable db: FractalDb = Unchecked.defaultof<_>
    let mutable collection: Collection<TestDocument> = Unchecked.defaultof<_>
    
    let testSchema: SchemaDef<TestDocument> = 
        { Fields = []
          Indexes = []
          Timestamps = true
          Validate = None }
    
    let createDocument i = 
        { id = $"doc-{i}"
          name = $"Item {i}"
          value = i * 100 }
    
    [<GlobalSetup>]
    member _.Setup() =
        let dbPath = Path.Combine(Path.GetTempPath(), "fractaldb_bench_txn.db")
        if File.Exists(dbPath) then File.Delete(dbPath)
        db <- FractalDb.Open(dbPath)
        collection <- db.Collection<TestDocument>("test_documents", testSchema)
    
    [<GlobalCleanup>]
    member _.Cleanup() =
        db.Close()
        let dbPath = Path.Combine(Path.GetTempPath(), "fractaldb_bench_txn.db")
        if File.Exists(dbPath) then File.Delete(dbPath)
    
    [<IterationSetup>]
    member _.IterationSetup() =
        (collection |> Collection.deleteMany Query.Empty).Wait()
        // Insert some documents for update/delete operations
        let docs = [ for i in 1..100 -> createDocument i ]
        (collection |> Collection.insertMany docs).Wait()
    
    [<Benchmark>]
    member _.Transaction_SingleInsert() =
        (db.Transact {
            let! result = collection |> Collection.insertOne (createDocument 9999)
            return result.Id
        }).Result
    
    [<Benchmark>]
    member _.Transaction_TwoInserts() =
        (db.Transact {
            let! result1 = collection |> Collection.insertOne (createDocument 10001)
            let! result2 = collection |> Collection.insertOne (createDocument 10002)
            return (result1.Id, result2.Id)
        }).Result
    
    [<Benchmark>]
    member _.Transaction_FiveInserts() =
        (db.Transact {
            let! r1 = collection |> Collection.insertOne (createDocument 10001)
            let! r2 = collection |> Collection.insertOne (createDocument 10002)
            let! r3 = collection |> Collection.insertOne (createDocument 10003)
            let! r4 = collection |> Collection.insertOne (createDocument 10004)
            let! r5 = collection |> Collection.insertOne (createDocument 10005)
            return [r1.Id; r2.Id; r3.Id; r4.Id; r5.Id]
        }).Result
    
    [<Benchmark>]
    member _.Transaction_InsertAndQuery() =
        (db.Transact {
            let doc = createDocument 20000
            let! inserted = collection |> Collection.insertOne doc
            let query = Query.Field("id", FieldOp.Compare(box (CompareOp.Eq inserted.Data.id)))
            // Note: find returns Task<list> not Task<FractalResult<list>>, so we use task {} not Transact {}
            return inserted.Id
        }).Result
    
    [<Benchmark>]
    member _.NoTransaction_SingleInsert() =
        (collection |> Collection.insertOne (createDocument 40000)).Result
    
    [<Benchmark>]
    member _.NoTransaction_TwoInserts() =
        let r1 = (collection |> Collection.insertOne (createDocument 50001)).Result
        let r2 = (collection |> Collection.insertOne (createDocument 50002)).Result
        (r1, r2)
    
    [<Benchmark>]
    member _.NoTransaction_FiveInserts() =
        let r1 = (collection |> Collection.insertOne (createDocument 50001)).Result
        let r2 = (collection |> Collection.insertOne (createDocument 50002)).Result
        let r3 = (collection |> Collection.insertOne (createDocument 50003)).Result
        let r4 = (collection |> Collection.insertOne (createDocument 50004)).Result
        let r5 = (collection |> Collection.insertOne (createDocument 50005)).Result
        [r1; r2; r3; r4; r5]
