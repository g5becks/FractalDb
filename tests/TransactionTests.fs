module FractalDb.Tests.TransactionTests

open System
open System.Threading.Tasks
open Xunit
open FsUnit.Xunit
open FractalDb.Database
open FractalDb.Schema
open FractalDb.Collection
open FractalDb.Query
open FractalDb.Errors
open FractalDb.Types
open FractalDb.Builders

type Account = {
    Name: string
    Balance: int
}

let accountSchema : SchemaDef<Account> = {
    Fields = [
        { Name = "name"; Path = None; SqlType = SqliteType.Text; Indexed = true; Unique = false; Nullable = false }
        { Name = "balance"; Path = None; SqlType = SqliteType.Integer; Indexed = false; Unique = false; Nullable = false }
    ]
    Indexes = []
    Timestamps = true
    Validate = None
}

type TransactionTestFixture() =
    let db = FractalDb.InMemory()
    member _.Db = db
    interface IDisposable with
        member _.Dispose() = db.Close()

type TransactionTests(fixture: TransactionTestFixture) =
    interface IClassFixture<TransactionTestFixture>

    [<Fact>]
    member _.``Transaction commits on success - both documents persisted`` () : Task = task {
        // Arrange
        let db = fixture.Db
        let accounts = db.Collection<Account>("accounts_commit", accountSchema)
        
        let account1 = { Name = "Alice"; Balance = 1000 }
        let account2 = { Name = "Bob"; Balance = 500 }
        
        // Act - Use transaction that should succeed
        let! result = db.Transact {
            let! doc1 = accounts |> Collection.insertOne account1
            let! doc2 = accounts |> Collection.insertOne account2
            return doc1.Id, doc2.Id
        }
        
        // Assert - Transaction succeeded
        match result with
        | Ok (id1, id2) ->
            // Verify both documents are persisted
            let! found1 = accounts |> Collection.findById id1
            let! found2 = accounts |> Collection.findById id2
            
            found1 |> should not' (equal None)
            found2 |> should not' (equal None)
            
            found1.Value.Data.Name |> should equal "Alice"
            found1.Value.Data.Balance |> should equal 1000
            found2.Value.Data.Name |> should equal "Bob"
            found2.Value.Data.Balance |> should equal 500
            
        | Error err -> 
            failwith $"Expected Ok, got Error: {err.Message}"
    }

    [<Fact>]
    member _.``Transaction rolls back on error - first insert not persisted`` () : Task = task {
        // Arrange
        let db = fixture.Db
        let accounts = db.Collection<Account>("accounts_rollback", accountSchema)
        
        let account1 = { Name = "Charlie"; Balance = 2000 }
        
        // Act - Use transaction that should fail
        let! result = db.Transact {
            let! doc1 = accounts |> Collection.insertOne account1
            // Return error to trigger rollback
            return! task { return Error (FractalError.Validation (None, "Simulated validation failure")) }
        }
        
        // Assert - Transaction failed
        match result with
        | Ok _ -> 
            failwith "Expected Error, got Ok"
        | Error err ->
            err.Message |> should equal "Validation failed: Simulated validation failure"
            
            // Verify document was NOT persisted (rolled back)
            let! allAccounts = accounts |> Collection.find Query.empty
            allAccounts |> should haveLength 0
    }

    [<Fact>]
    member _.``TransactionBuilder with let! binding works correctly`` () : Task = task {
        // Arrange
        let db = fixture.Db
        let accounts = db.Collection<Account>("accounts_binding", accountSchema)
        
        let account1 = { Name = "David"; Balance = 750 }
        let account2 = { Name = "Eve"; Balance = 1250 }
        
        // Act - Use multiple let! bindings
        let! result = db.Transact {
            let! doc1 = accounts |> Collection.insertOne account1
            let! doc2 = accounts |> Collection.insertOne account2
            
            return doc1.Data.Balance + doc2.Data.Balance
        }
        
        // Assert
        match result with
        | Ok totalBalance ->
            totalBalance |> should equal 2000
            
            // Verify persistence after transaction
            let! allAccounts = accounts |> Collection.find Query.empty
            allAccounts |> should haveLength 2
            
        | Error err ->
            failwith $"Expected Ok, got Error: {err.Message}"
    }

    [<Fact>]
    member _.``Nested operations commit atomically - multiple inserts`` () : Task = task {
        // Arrange
        let db = fixture.Db
        let accounts = db.Collection<Account>("accounts_atomic", accountSchema)
        
        // Act - Insert 3 accounts in a transaction
        let! result = db.Transact {
            let! doc1 = accounts |> Collection.insertOne { Name = "Account1"; Balance = 100 }
            let! doc2 = accounts |> Collection.insertOne { Name = "Account2"; Balance = 200 }
            let! doc3 = accounts |> Collection.insertOne { Name = "Account3"; Balance = 300 }
            
            return doc1.Id, doc2.Id, doc3.Id
        }
        
        // Assert
        match result with
        | Ok (id1, id2, id3) ->
            // Verify persistence - should have 3 accounts
            let! allAccounts = accounts |> Collection.find Query.empty
            allAccounts |> should haveLength 3
            
            // Verify all accounts were created
            let names = allAccounts |> List.map (fun doc -> doc.Data.Name) |> List.sort
            names |> should equal ["Account1"; "Account2"; "Account3"]
            
            let totalBalance = allAccounts |> List.sumBy (fun doc -> doc.Data.Balance)
            totalBalance |> should equal 600
            
        | Error err ->
            failwith $"Expected Ok, got Error: {err.Message}"
    }

    [<Fact>]
    member _.``Transaction rolls back on validation failure - no changes persisted`` () : Task = task {
        // Arrange
        let db = fixture.Db
        let accounts = db.Collection<Account>("accounts_validation_fail", accountSchema)
        
        // Act - Insert one account, then fail validation
        let! result = db.Transact {
            let! doc1 = accounts |> Collection.insertOne { Name = "BeforeFailure"; Balance = 100 }
            
            // Simulate validation failure
            if doc1.Data.Balance < 500 then
                return! task { return Error (FractalError.Validation (None, "Balance must be at least 500")) }
            else
                return doc1.Id
        }
        
        // Assert
        match result with
        | Ok _ ->
            failwith "Expected Error, got Ok"
        | Error err ->
            err.Message |> should equal "Validation failed: Balance must be at least 500"
            
            // Verify no changes were persisted (rollback)
            let! allAccounts = accounts |> Collection.find Query.empty
            allAccounts |> should haveLength 0
    }

    [<Fact>]
    member _.``Multiple inserts in transaction commit together`` () : Task = task {
        // Arrange
        let db = fixture.Db
        let accounts = db.Collection<Account>("accounts_batch", accountSchema)
        
        // Act - Insert all in transaction
        let! result = db.Transact {
            let! doc1 = accounts |> Collection.insertOne { Name = "Account1"; Balance = 100 }
            let! doc2 = accounts |> Collection.insertOne { Name = "Account2"; Balance = 200 }
            let! doc3 = accounts |> Collection.insertOne { Name = "Account3"; Balance = 300 }
            let! doc4 = accounts |> Collection.insertOne { Name = "Account4"; Balance = 400 }
            
            return [doc1.Id; doc2.Id; doc3.Id; doc4.Id]
        }
        
        // Assert
        match result with
        | Ok ids ->
            ids |> should haveLength 4
            
            // Verify all are persisted
            let! allAccounts = accounts |> Collection.find Query.empty
            allAccounts |> should haveLength 4
            
            let totalBalance = allAccounts |> List.sumBy (fun doc -> doc.Data.Balance)
            totalBalance |> should equal 1000
            
        | Error err ->
            failwith $"Expected Ok, got Error: {err.Message}"
    }
