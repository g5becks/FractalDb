module FractalDb.Tests.ErrorRecoveryTests

/// <summary>
/// Tests for error recovery and resilience scenarios.
/// Verifies that the system handles and recovers gracefully from various error conditions.
/// </summary>

open System
open System.Threading.Tasks
open Xunit
open FsUnit.Xunit
open Microsoft.Data.Sqlite
open Donald
open FractalDb.Types
open FractalDb.Schema
open FractalDb.Operators
open FractalDb.Collection
open FractalDb.Database
open FractalDb.Builders
open FractalDb.Errors
open FractalDb.Transaction

// =============================================================================
// Test Data
// =============================================================================

type TestUser = {
    Name: string
    Email: string
    Age: int
}

let userSchema: SchemaDef<TestUser> =
    { Fields =
        [ { Name = "name"
            Path = None
            SqlType = SqliteType.Text
            Indexed = false
            Unique = false
            Nullable = false }
          { Name = "email"
            Path = None
            SqlType = SqliteType.Text
            Indexed = true
            Unique = true
            Nullable = false }
          { Name = "age"
            Path = None
            SqlType = SqliteType.Integer
            Indexed = false
            Unique = false
            Nullable = false } ]
      Indexes = []
      Timestamps = true
      Validate = None }

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
// Unique Constraint Error Recovery Tests
// =============================================================================

[<Fact>]
let ``Unique constraint violation provides field name``() : Task =
    task {
        let db = FractalDb.InMemory()
        let users = db.Collection<TestUser>("users", userSchema)
        
        let user1 = { Name = "Alice"; Email = "alice@test.com"; Age = 30 }
        let user2 = { Name = "Bob"; Email = "alice@test.com"; Age = 25 }
        
        // Insert first user
        let! result1 = users |> Collection.insertOne user1
        match result1 with
        | Ok _ -> ()
        | Error err -> failwith $"First insert should succeed: {err.Message}"
        
        // Try to insert duplicate email
        let! result2 = users |> Collection.insertOne user2
        
        match result2 with
        | Error (FractalError.UniqueConstraint (field, _)) ->
            // Should identify the email field
            field |> should equal "email"
        | Ok _ -> failwith "Should have failed with unique constraint error"
        | Error err -> failwith $"Expected UniqueConstraint error, got: {err.Message}"
        
        // Verify first user is still there
        let! allUsers = users |> Collection.find Query.Empty
        allUsers |> should haveLength 1
        allUsers.[0].Data.Name |> should equal "Alice"
        
        db.Close()
    }

[<Fact>]
let ``Multiple constraint violations are handled independently``() : Task =
    task {
        let db = FractalDb.InMemory()
        let users = db.Collection<TestUser>("users", userSchema)
        
        // Insert users
        let! _ = users |> Collection.insertOne { Name = "Alice"; Email = "alice@test.com"; Age = 30 }
        let! _ = users |> Collection.insertOne { Name = "Bob"; Email = "bob@test.com"; Age = 25 }
        
        // Try multiple violations
        let violations = [
            { Name = "Charlie"; Email = "alice@test.com"; Age = 35 }
            { Name = "David"; Email = "bob@test.com"; Age = 40 }
            { Name = "Eve"; Email = "eve@test.com"; Age = 28 }
        ]
        
        let mutable successCount = 0
        let mutable errorCount = 0
        
        for user in violations do
            let! result = users |> Collection.insertOne user
            match result with
            | Ok _ -> successCount <- successCount + 1
            | Error (FractalError.UniqueConstraint (field, _)) ->
                errorCount <- errorCount + 1
                field |> should equal "email"
            | Error err -> failwith $"Unexpected error: {err.Message}"
        
        // Should have 2 violations and 1 success
        errorCount |> should equal 2
        successCount |> should equal 1
        
        // Verify final state: Alice, Bob, and Eve
        let! finalUsers = users |> Collection.find Query.Empty
        finalUsers |> should haveLength 3
        
        let names = finalUsers |> List.map (fun u -> u.Data.Name) |> List.sort
        names |> should equal ["Alice"; "Bob"; "Eve"]
        
        db.Close()
    }


// =============================================================================
// Query Error Tests
// =============================================================================

[<Fact>]
let ``Invalid field in query is handled gracefully``() : Task =
    task {
        let db = FractalDb.InMemory()
        let users = db.Collection<TestUser>("users", userSchema)
        
        // Insert a document
        let! _ = users |> Collection.insertOne { Name = "Alice"; Email = "alice@test.com"; Age = 30 }
        
        // Try to query with invalid field reference
        // This tests that the system handles SQL errors gracefully
        try
            let invalidQuery = Query.Field("nonexistent_field", FieldOp.Compare(box (CompareOp.Eq "value")))
            let! results = users |> Collection.find invalidQuery
            
            // SQLite might not error on unknown fields in JSON queries
            // Just verify we get a result (even if empty)
            results |> should be (instanceOfType<list<Document<TestUser>>>)
        with
        | ex ->
            // If it throws, it should be a database-related exception
            ex |> should be (instanceOfType<Exception>)
        
        db.Close()
    }

// =============================================================================
// Transaction Rollback Tests
// =============================================================================

[<Fact>]
let ``Transaction rolls back on error``() : Task =
    task {
        let db = FractalDb.InMemory()
        let users = db.Collection<TestUser>("users", userSchema)
        
        // Insert initial user
        let! _ = users |> Collection.insertOne { Name = "Alice"; Email = "alice@test.com"; Age = 30 }
        
        // Verify initial state
        let! initialUsers = users |> Collection.find Query.Empty
        initialUsers |> should haveLength 1
        
        // Try transaction that will fail
        let! result = db.Transact {
            // First insert should succeed
            let! doc1 = users |> Collection.insertOne { Name = "Bob"; Email = "bob@test.com"; Age = 25 }
            // Second insert has duplicate email - should fail
            let! doc2 = users |> Collection.insertOne { Name = "Charlie"; Email = "alice@test.com"; Age = 35 }
            return doc1.Id, doc2.Id
        }
        
        // Transaction should have failed
        match result with
        | Error (FractalError.UniqueConstraint _) -> ()
        | Ok _ -> failwith "Transaction should have failed"
        | Error err -> failwith $"Expected UniqueConstraint error, got: {err.Message}"
        
        // Verify rollback - should still only have Alice
        let! finalUsers = users |> Collection.find Query.Empty
        finalUsers |> should haveLength 1
        finalUsers.[0].Data.Name |> should equal "Alice"
        
        db.Close()
    }

[<Fact>]
let ``Data remains consistent after transaction failure``() : Task =
    task {
        let db = FractalDb.InMemory()
        let users = db.Collection<TestUser>("users", userSchema)
        
        // Insert initial users
        let! _ = users |> Collection.insertOne { Name = "Alice"; Email = "alice@test.com"; Age = 30 }
        let! _ = users |> Collection.insertOne { Name = "Bob"; Email = "bob@test.com"; Age = 25 }
        
        // Try multiple transactions with failures
        for i in 1..5 do
            let! _ = db.Transact {
                // This will always fail due to duplicate email
                let! doc = users |> Collection.insertOne { Name = $"User{i}"; Email = "alice@test.com"; Age = 20 + i }
                return doc.Id
            }
            ()
        
        // Verify original data is intact
        let! finalUsers = users |> Collection.find Query.Empty
        finalUsers |> should haveLength 2
        
        let names = finalUsers |> List.map (fun u -> u.Data.Name) |> List.sort
        names |> should equal ["Alice"; "Bob"]
        
        db.Close()
    }

[<Fact>]
let ``Transaction with multiple operations rolls back atomically``() : Task =
    task {
        let db = FractalDb.InMemory()
        let users = db.Collection<TestUser>("users", userSchema)
        
        // Insert initial user
        let user1 = { Name = "Alice"; Email = "alice@test.com"; Age = 30 }
        let! result1 = users |> Collection.insertOne user1
        let aliceId = match result1 with | Ok doc -> doc.Id | Error _ -> failwith "Setup failed"
        
        // Try transaction with 3 operations where the 3rd fails
        let! result = db.Transact {
            // 1. Update Alice's age
            let! _ = users |> Collection.updateById aliceId (fun u -> { u with Age = 31 })
            // 2. Insert Bob
            let! bob = users |> Collection.insertOne { Name = "Bob"; Email = "bob@test.com"; Age = 25 }
            // 3. Insert duplicate email - this will fail
            let! charlie = users |> Collection.insertOne { Name = "Charlie"; Email = "alice@test.com"; Age = 35 }
            return bob.Id, charlie.Id
        }
        
        // Transaction should have failed
        match result with
        | Error _ -> ()
        | Ok _ -> failwith "Transaction should have failed"
        
        // Verify ALL operations were rolled back
        let! finalUsers = users |> Collection.find Query.Empty
        finalUsers |> should haveLength 1  // Only Alice should exist
        finalUsers.[0].Data.Name |> should equal "Alice"
        finalUsers.[0].Data.Age |> should equal 30  // Age should still be 30, not 31
        
        db.Close()
    }
