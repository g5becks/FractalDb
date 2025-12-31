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

type SimpleDoc = { Value: string }

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
let ``InMemory databases are isolated from each other`` () : Task =
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
let ``InMemory database survives across multiple operations`` () : Task =
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
let ``Close is idempotent - double close is safe`` () =
    let db = FractalDb.InMemory()
    let coll = db.Collection<SimpleDoc>("items", simpleSchema)

    // First close
    db.Close()

    // Second close should not throw
    db.Close()

    // Third close should also be safe
    db.Close()

[<Fact>]
let ``IDisposable.Dispose calls Close`` () =
    use db = FractalDb.InMemory()
    let coll = db.Collection<SimpleDoc>("items", simpleSchema)

    // Dispose will be called automatically at end of use block
    // Just verify no exception is thrown
    ()

[<Fact>]
let ``Multiple dispose calls are safe`` () =
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
let ``Collection returns same instance for same name and type`` () =
    let db = FractalDb.InMemory()

    let coll1 = db.Collection<SimpleDoc>("items", simpleSchema)
    let coll2 = db.Collection<SimpleDoc>("items", simpleSchema)

    // Should return the same cached instance
    Object.ReferenceEquals(coll1, coll2) |> should equal true

    db.Close()

[<Fact>]
let ``Collection returns different instances for different names`` () =
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
let ``DbOptions.defaults provides sensible defaults`` () =
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
let ``Empty database can be closed safely`` () =
    let db = FractalDb.InMemory()

    // Close without any operations
    db.Close()

[<Fact>]
let ``Database with empty collections can be closed`` () =
    let db = FractalDb.InMemory()
    let coll = db.Collection<SimpleDoc>("items", simpleSchema)

    // Get collection but don't insert anything
    db.Close()

[<Fact>]
let ``Multiple collections can be created before any operations`` () =
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
let ``Collection creation is thread-safe`` () : Task =
    task {
        let db = FractalDb.InMemory()

        // Create collections concurrently
        let tasks =
            [ for i in 1..10 -> Task.Run(fun () -> db.Collection<SimpleDoc>($"items{i}", simpleSchema)) ]

        let! results = Task.WhenAll(tasks)

        // All collections should be created successfully
        results |> should haveLength 10
        results |> Array.forall (fun c -> not (isNull (box c))) |> should equal true

        db.Close()
    }

[<Fact>]
let ``InMemory database has valid connection`` () =
    let db = FractalDb.InMemory()

    // Connection should not be null
    db.Connection |> should not' (be Null)

    db.Close()

// =============================================================================
// Database State Tests
// =============================================================================

[<Fact>]
let ``Database operations work after collection creation`` () : Task =
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
let ``Database supports multiple concurrent operations`` () : Task =
    task {
        let db = FractalDb.InMemory()
        let coll = db.Collection<SimpleDoc>("items", simpleSchema)

        // Perform multiple inserts concurrently
        let insertTasks =
            [ for i in 1..20 -> coll |> Collection.insertOne { Value = $"item-{i}" } ]

        let! results = Task.WhenAll(insertTasks)

        // All inserts should succeed
        results
        |> Array.forall (function
            | Ok _ -> true
            | Error _ -> false)
        |> should equal true

        // Verify all items were inserted
        let! items = coll |> Collection.find Query.Empty
        items |> should haveLength 20

        db.Close()
    }

// =============================================================================
// FromConnection Tests
// =============================================================================

[<Fact>]
let ``FromConnection uses provided connection`` () : Task =
    task {
        // Arrange - create external connection
        use conn = new Microsoft.Data.Sqlite.SqliteConnection("Data Source=:memory:")
        conn.Open()

        // Act
        use db = FractalDb.FromConnection(conn)

        // Assert - connection should be the same
        db.Connection |> should equal (conn :> System.Data.IDbConnection)

        // Database should be usable
        let coll = db.Collection<SimpleDoc>("items", simpleSchema)
        let! result = coll |> Collection.insertOne { Value = "test" }

        match result with
        | Ok _ -> ()
        | Error err -> failwith $"Expected Ok, got Error: {err.Message}"
    }

[<Fact>]
let ``FromConnection does not own connection - Close does not close connection`` () =
    // Arrange - create external connection
    use conn = new Microsoft.Data.Sqlite.SqliteConnection("Data Source=:memory:")
    conn.Open()

    // Act - create FractalDb and close it
    let db = FractalDb.FromConnection(conn)
    db.OwnsConnection |> should equal false
    db.Close()

    // Assert - connection should still be open
    conn.State |> should equal System.Data.ConnectionState.Open

    // We can still use the connection after FractalDb is closed
    conn.Close()

[<Fact>]
let ``FromConnection with custom options preserves options`` () =
    // Arrange
    use conn = new Microsoft.Data.Sqlite.SqliteConnection("Data Source=:memory:")
    conn.Open()

    let customOptions =
        { DbOptions.defaults with
            EnableCache = true
            IdGenerator = fun () -> "custom-id" }

    // Act
    use db = FractalDb.FromConnection(conn, customOptions)

    // Assert
    db.Options.EnableCache |> should equal true
    db.Options.IdGenerator() |> should equal "custom-id"

[<Fact>]
let ``Open owns connection - Close disposes connection`` () =
    // Arrange & Act
    let db = FractalDb.InMemory()

    // Assert
    db.OwnsConnection |> should equal true

    // Get connection before closing
    let conn = db.Connection
    conn.State |> should equal System.Data.ConnectionState.Open

    // Close should close the connection
    db.Close()

    // Connection should be closed (cannot verify state directly as it's disposed)
    db.IsDisposed |> should equal true

// =============================================================================
// Execute Tests
// =============================================================================

[<Fact>]
let ``Execute commits transaction on successful completion`` () : Task =
    task {
        // Arrange
        let db = FractalDb.InMemory()
        let coll = db.Collection<SimpleDoc>("items", simpleSchema)

        // Act - Execute should commit automatically on success
        let! insertedId =
            db.Execute(fun _tx ->
                task {
                    let! result = coll |> Collection.insertOne { Value = "test-value" }

                    match result with
                    | Ok doc -> return doc.Id
                    | Error err -> return failwith $"Insert failed: {err.Message}"
                })

        // Assert - Data should be persisted after Execute completes
        let! found = coll |> Collection.findById insertedId
        found.IsSome |> should equal true
        found.Value.Data.Value |> should equal "test-value"

        db.Close()
    }

[<Fact>]
let ``Execute rolls back transaction on exception`` () : Task =
    task {
        // Arrange
        let db = FractalDb.InMemory()
        let coll = db.Collection<SimpleDoc>("items", simpleSchema)

        // Act - Execute should rollback on exception
        let mutable exceptionThrown = false

        try
            let! _ =
                db.Execute(fun _tx ->
                    task {
                        let! result = coll |> Collection.insertOne { Value = "should-rollback" }

                        match result with
                        | Ok _ -> ()
                        | Error err -> failwith $"Insert failed: {err.Message}"

                        // Throw exception after insert to trigger rollback
                        return failwith "Intentional exception"
                    })

            ()
        with _ ->
            exceptionThrown <- true

        // Assert
        exceptionThrown |> should equal true

        // Data should NOT be persisted (rolled back)
        let! items = coll |> Collection.find Query.Empty
        items |> should be Empty

        db.Close()
    }

[<Fact>]
let ``Execute returns result from function`` () : Task =
    task {
        // Arrange
        let db = FractalDb.InMemory()
        let coll = db.Collection<SimpleDoc>("items", simpleSchema)

        // Act
        let! result =
            db.Execute(fun _tx ->
                task {
                    let! r1 = coll |> Collection.insertOne { Value = "item1" }
                    let! r2 = coll |> Collection.insertOne { Value = "item2" }

                    match r1, r2 with
                    | Ok d1, Ok d2 -> return (d1.Id, d2.Id)
                    | _ -> return failwith "Insert failed"
                })

        // Assert - Should return tuple of IDs
        let (id1, id2) = result
        id1 |> should not' (equal id2)

        // Both should be persisted
        let! found1 = coll |> Collection.findById id1
        let! found2 = coll |> Collection.findById id2
        found1.IsSome |> should equal true
        found2.IsSome |> should equal true

        db.Close()
    }

// =============================================================================
// ExecuteTransaction Tests
// =============================================================================

[<Fact>]
let ``ExecuteTransaction commits on Ok result`` () : Task =
    task {
        // Arrange
        let db = FractalDb.InMemory()
        let coll = db.Collection<SimpleDoc>("items", simpleSchema)

        // Act
        let! result =
            db.ExecuteTransaction(fun _tx ->
                task {
                    let! insertResult = coll |> Collection.insertOne { Value = "ok-value" }
                    return insertResult |> Result.map (fun doc -> doc.Id)
                })

        // Assert
        match result with
        | Ok id ->
            let! found = coll |> Collection.findById id
            found.IsSome |> should equal true
            found.Value.Data.Value |> should equal "ok-value"
        | Error err -> failwith $"Expected Ok, got Error: {err.Message}"

        db.Close()
    }

[<Fact>]
let ``ExecuteTransaction rolls back on Error result`` () : Task =
    task {
        // Arrange
        let db = FractalDb.InMemory()
        let coll = db.Collection<SimpleDoc>("items", simpleSchema)

        // Act
        let! result =
            db.ExecuteTransaction(fun _tx ->
                task {
                    // First insert
                    let! _ = coll |> Collection.insertOne { Value = "should-rollback" }

                    // Return error to trigger rollback
                    return Error(FractalDb.Errors.FractalError.Validation(None, "Intentional error"))
                })

        // Assert - Should return error
        match result with
        | Ok _ -> failwith "Expected Error, got Ok"
        | Error err -> err.Message |> should equal "Validation failed: Intentional error"

        // Data should NOT be persisted (rolled back)
        let! items = coll |> Collection.find Query.Empty
        items |> should be Empty

        db.Close()
    }

[<Fact>]
let ``ExecuteTransaction wraps exceptions in Transaction error`` () : Task =
    task {
        // Arrange
        let db = FractalDb.InMemory()
        let coll = db.Collection<SimpleDoc>("items", simpleSchema)

        // Act
        let! result =
            db.ExecuteTransaction(fun _tx ->
                task {
                    let! _ = coll |> Collection.insertOne { Value = "should-rollback" }
                    return failwith "Intentional exception"
                })

        // Assert - Should return Transaction error
        match result with
        | Ok _ -> failwith "Expected Error, got Ok"
        | Error err ->
            match err with
            | FractalDb.Errors.FractalError.Transaction msg -> msg |> should equal "Intentional exception"
            | _ -> failwith $"Expected Transaction error, got {err}"

        // Data should NOT be persisted (rolled back)
        let! items = coll |> Collection.find Query.Empty
        items |> should be Empty

        db.Close()
    }
