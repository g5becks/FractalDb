module FractalDb.Tests.DatabaseLifecycleTests

/// <summary>
/// Tests for Database.fs lifecycle edge cases including isolation, disposal, and caching.
/// </summary>

open System
open System.Threading.Tasks
open Xunit
open FsUnit.Xunit
open FractalDb.Types
open FractalDb.Schema
open FractalDb.Operators
open FractalDb.Collection
open FractalDb.Database

// =============================================================================
// Test Data
// =============================================================================

type SimpleDoc = {
    Value: string
}

let simpleSchema: SchemaDef<SimpleDoc> =
    { Fields =
        [ { Name = "value"
            Path = None
            SqlType = SqliteType.Text
            Indexed = false
            Unique = false
            Nullable = false } ]
      Indexes = []
      Timestamps = true
      Validate = None }

// =============================================================================
// InMemory Database Isolation Tests
// =============================================================================

[<Fact>]
let ``InMemory databases are isolated from each other``() : Task =
    task {
        let db1 = FractalDb.InMemory()
        let db2 = FractalDb.InMemory()
        
        let coll1 = db1.Collection<SimpleDoc>("items", simpleSchema)
        let coll2 = db2.Collection<SimpleDoc>("items", simpleSchema)
        
        // Insert into db1
        let! result1 = coll1 |> Collection.insertOne { Value = "db1-data" }
        match result1 with
        | Ok _ -> ()
        | Error err -> failwith $"Expected Ok, got Error: {err.Message}"
        
        // db2 should be empty
        let! items2 = coll2 |> Collection.find Query.Empty
        items2 |> should be Empty
        
        // Insert into db2
        let! result2 = coll2 |> Collection.insertOne { Value = "db2-data" }
        match result2 with
        | Ok _ -> ()
        | Error err -> failwith $"Expected Ok, got Error: {err.Message}"
        
        // Verify db1 still has only its data
        let! items1 = coll1 |> Collection.find Query.Empty
        items1 |> should haveLength 1
        items1.[0].Data.Value |> should equal "db1-data"
        
        db1.Close()
        db2.Close()
    }

[<Fact>]
let ``InMemory database survives across multiple operations``() : Task =
    task {
        let db = FractalDb.InMemory()
        let coll = db.Collection<SimpleDoc>("items", simpleSchema)
        
        // Multiple inserts
        for i in 1..10 do
            let! result = coll |> Collection.insertOne { Value = $"item-{i}" }
            match result with
            | Ok _ -> ()
            | Error err -> failwith $"Expected Ok, got Error: {err.Message}"
        
        // Verify all data persists
        let! items = coll |> Collection.find Query.Empty
        items |> should haveLength 10
        
        db.Close()
    }

// =============================================================================
// Database Close Tests
// =============================================================================

[<Fact>]
let ``Close is idempotent - double close is safe``() =
    let db = FractalDb.InMemory()
    let coll = db.Collection<SimpleDoc>("items", simpleSchema)
    
    // First close
    db.Close()
    
    // Second close should not throw
    db.Close()
    
    // Third close should also be safe
    db.Close()

[<Fact>]
let ``IDisposable.Dispose calls Close``() =
    use db = FractalDb.InMemory()
    let coll = db.Collection<SimpleDoc>("items", simpleSchema)
    
    // Dispose will be called automatically at end of use block
    // Just verify no exception is thrown
    ()

[<Fact>]
let ``Multiple dispose calls are safe``() =
    let db = FractalDb.InMemory()
    
    // First dispose
    (db :> IDisposable).Dispose()
    
    // Second dispose should be safe
    (db :> IDisposable).Dispose()
    
    // Third dispose
    (db :> IDisposable).Dispose()

// =============================================================================
// Collection Caching Tests
// =============================================================================

[<Fact>]
let ``Collection returns same instance for same name and type``() =
    let db = FractalDb.InMemory()
    
    let coll1 = db.Collection<SimpleDoc>("items", simpleSchema)
    let coll2 = db.Collection<SimpleDoc>("items", simpleSchema)
    
    // Should return the same cached instance
    Object.ReferenceEquals(coll1, coll2) |> should equal true
    
    db.Close()

[<Fact>]
let ``Collection returns different instances for different names``() =
    let db = FractalDb.InMemory()
    
    let coll1 = db.Collection<SimpleDoc>("items1", simpleSchema)
    let coll2 = db.Collection<SimpleDoc>("items2", simpleSchema)
    
    // Different collections should not be same instance
    Object.ReferenceEquals(coll1, coll2) |> should equal false
    
    db.Close()

// =============================================================================
// Database Options Tests
// =============================================================================

[<Fact>]
let ``DbOptions.defaults provides sensible defaults``() =
    let opts = DbOptions.defaults
    
    // Should have an ID generator
    let id1 = opts.IdGenerator()
    let id2 = opts.IdGenerator()
    
    // IDs should be unique
    id1 |> should not' (equal id2)
    
    // IDs should be valid GUIDs
    let mutable guid = Guid.Empty
    Guid.TryParse(id1, &guid) |> should equal true
    
    // Cache should be disabled by default
    opts.EnableCache |> should equal false

// =============================================================================
// Edge Cases Tests
// =============================================================================

[<Fact>]
let ``Empty database can be closed safely``() =
    let db = FractalDb.InMemory()
    
    // Close without any operations
    db.Close()

[<Fact>]
let ``Database with empty collections can be closed``() =
    let db = FractalDb.InMemory()
    let coll = db.Collection<SimpleDoc>("items", simpleSchema)
    
    // Get collection but don't insert anything
    db.Close()

[<Fact>]
let ``Multiple collections can be created before any operations``() =
    let db = FractalDb.InMemory()
    
    let coll1 = db.Collection<SimpleDoc>("items1", simpleSchema)
    let coll2 = db.Collection<SimpleDoc>("items2", simpleSchema)
    let coll3 = db.Collection<SimpleDoc>("items3", simpleSchema)
    
    // All collections should be created successfully
    coll1 |> should not' (be Null)
    coll2 |> should not' (be Null)
    coll3 |> should not' (be Null)
    
    db.Close()

[<Fact>]
let ``Collection creation is thread-safe``() : Task =
    task {
        let db = FractalDb.InMemory()
        
        // Create collections concurrently
        let tasks = [
            for i in 1..10 ->
                Task.Run(fun () -> db.Collection<SimpleDoc>($"items{i}", simpleSchema))
        ]
        
        let! results = Task.WhenAll(tasks)
        
        // All collections should be created successfully
        results |> should haveLength 10
        results |> Array.forall (fun c -> not (isNull (box c))) |> should equal true
        
        db.Close()
    }

[<Fact>]
let ``InMemory database has valid connection``() =
    let db = FractalDb.InMemory()
    
    // Connection should not be null
    db.Connection |> should not' (be Null)
    
    db.Close()

// =============================================================================
// Database State Tests
// =============================================================================

[<Fact>]
let ``Database operations work after collection creation``() : Task =
    task {
        let db = FractalDb.InMemory()
        let coll = db.Collection<SimpleDoc>("items", simpleSchema)
        
        // Perform operation
        let! result = coll |> Collection.insertOne { Value = "test" }
        match result with
        | Ok _ -> ()
        | Error err -> failwith $"Expected Ok, got Error: {err.Message}"
        
        // Verify operation succeeded
        let! items = coll |> Collection.find Query.Empty
        items |> should haveLength 1
        
        db.Close()
    }

[<Fact>]
let ``Database supports multiple concurrent operations``() : Task =
    task {
        let db = FractalDb.InMemory()
        let coll = db.Collection<SimpleDoc>("items", simpleSchema)
        
        // Perform multiple inserts concurrently
        let insertTasks = [
            for i in 1..20 ->
                coll |> Collection.insertOne { Value = $"item-{i}" }
        ]
        
        let! results = Task.WhenAll(insertTasks)
        
        // All inserts should succeed
        results |> Array.forall (function | Ok _ -> true | Error _ -> false)
        |> should equal true
        
        // Verify all items were inserted
        let! items = coll |> Collection.find Query.Empty
        items |> should haveLength 20
        
        db.Close()
    }
