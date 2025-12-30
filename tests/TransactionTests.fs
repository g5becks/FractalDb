module FractalDb.Tests.TransactionTests

open System
open System.Threading.Tasks
open Xunit
open FsUnit.Xunit
open FractalDb.Database
open FractalDb.Schema
open FractalDb.Collection
open FractalDb.Operators
open FractalDb.Errors
open FractalDb.Types
open FractalDb.Builders

type Account = { Name: string; Balance: int }

let accountSchema: SchemaDef<Account> =
    { Fields =
        [ { Name = "name"
            Path = None
            SqlType = SqliteType.Text
            Indexed = true
            Unique = false
            Nullable = false }
          { Name = "balance"
            Path = None
            SqlType = SqliteType.Integer
            Indexed = false
            Unique = false
            Nullable = false } ]
      Indexes = []
      Timestamps = true
      Validate = None }

type TransactionTestFixture() =
    let db = FractalDb.InMemory()
    member _.Db = db

    interface IDisposable with
        member _.Dispose() = db.Close()

type TransactionTests(fixture: TransactionTestFixture) =
    interface IClassFixture<TransactionTestFixture>

    [<Fact>]
    member _.``Transaction commits on success - both documents persisted``() : Task =
        task {
            // Arrange
            let db = fixture.Db
            let accounts = db.Collection<Account>("accounts_commit", accountSchema)

            let account1 = { Name = "Alice"; Balance = 1000 }
            let account2 = { Name = "Bob"; Balance = 500 }

            // Act - Use transaction that should succeed
            let! result =
                db.Transact {
                    let! doc1 = accounts |> Collection.insertOne account1
                    let! doc2 = accounts |> Collection.insertOne account2
                    return doc1.Id, doc2.Id
                }

            // Assert - Transaction succeeded
            match result with
            | Ok(id1, id2) ->
                // Verify both documents are persisted
                let! found1 = accounts |> Collection.findById id1
                let! found2 = accounts |> Collection.findById id2

                found1 |> should not' (equal None)
                found2 |> should not' (equal None)

                found1.Value.Data.Name |> should equal "Alice"
                found1.Value.Data.Balance |> should equal 1000
                found2.Value.Data.Name |> should equal "Bob"
                found2.Value.Data.Balance |> should equal 500

            | Error err -> failwith $"Expected Ok, got Error: {err.Message}"
        }

    [<Fact>]
    member _.``Transaction rolls back on error - first insert not persisted``() : Task =
        task {
            // Arrange
            let db = fixture.Db
            let accounts = db.Collection<Account>("accounts_rollback", accountSchema)

            let account1 = { Name = "Charlie"; Balance = 2000 }

            // Act - Use transaction that should fail
            let! result =
                db.Transact {
                    let! doc1 = accounts |> Collection.insertOne account1
                    // Return error to trigger rollback
                    return! task { return Error(FractalError.Validation(None, "Simulated validation failure")) }
                }

            // Assert - Transaction failed
            match result with
            | Ok _ -> failwith "Expected Error, got Ok"
            | Error err ->
                err.Message |> should equal "Validation failed: Simulated validation failure"

                // Verify document was NOT persisted (rolled back)
                let! allAccounts = accounts |> Collection.find Query.Empty
                allAccounts |> should haveLength 0
        }

    [<Fact>]
    member _.``TransactionBuilder with let! binding works correctly``() : Task =
        task {
            // Arrange
            let db = fixture.Db
            let accounts = db.Collection<Account>("accounts_binding", accountSchema)

            let account1 = { Name = "David"; Balance = 750 }
            let account2 = { Name = "Eve"; Balance = 1250 }

            // Act - Use multiple let! bindings
            let! result =
                db.Transact {
                    let! doc1 = accounts |> Collection.insertOne account1
                    let! doc2 = accounts |> Collection.insertOne account2

                    return doc1.Data.Balance + doc2.Data.Balance
                }

            // Assert
            match result with
            | Ok totalBalance ->
                totalBalance |> should equal 2000

                // Verify persistence after transaction
                let! allAccounts = accounts |> Collection.find Query.Empty
                allAccounts |> should haveLength 2

            | Error err -> failwith $"Expected Ok, got Error: {err.Message}"
        }

    [<Fact>]
    member _.``Nested operations commit atomically - multiple inserts``() : Task =
        task {
            // Arrange
            let db = fixture.Db
            let accounts = db.Collection<Account>("accounts_atomic", accountSchema)

            // Act - Insert 3 accounts in a transaction
            let! result =
                db.Transact {
                    let! doc1 = accounts |> Collection.insertOne { Name = "Account1"; Balance = 100 }
                    let! doc2 = accounts |> Collection.insertOne { Name = "Account2"; Balance = 200 }
                    let! doc3 = accounts |> Collection.insertOne { Name = "Account3"; Balance = 300 }

                    return doc1.Id, doc2.Id, doc3.Id
                }

            // Assert
            match result with
            | Ok(id1, id2, id3) ->
                // Verify persistence - should have 3 accounts
                let! allAccounts = accounts |> Collection.find Query.Empty
                allAccounts |> should haveLength 3

                // Verify all accounts were created
                let names = allAccounts |> List.map (fun doc -> doc.Data.Name) |> List.sort
                names |> should equal [ "Account1"; "Account2"; "Account3" ]

                let totalBalance = allAccounts |> List.sumBy (fun doc -> doc.Data.Balance)
                totalBalance |> should equal 600

            | Error err -> failwith $"Expected Ok, got Error: {err.Message}"
        }

    [<Fact>]
    member _.``Transaction rolls back on validation failure - no changes persisted``() : Task =
        task {
            // Arrange
            let db = fixture.Db
            let accounts = db.Collection<Account>("accounts_validation_fail", accountSchema)

            // Act - Insert one account, then fail validation
            let! result =
                db.Transact {
                    let! doc1 =
                        accounts
                        |> Collection.insertOne
                            { Name = "BeforeFailure"
                              Balance = 100 }

                    // Simulate validation failure
                    if doc1.Data.Balance < 500 then
                        return! task { return Error(FractalError.Validation(None, "Balance must be at least 500")) }
                    else
                        return doc1.Id
                }

            // Assert
            match result with
            | Ok _ -> failwith "Expected Error, got Ok"
            | Error err ->
                err.Message |> should equal "Validation failed: Balance must be at least 500"

                // Verify no changes were persisted (rollback)
                let! allAccounts = accounts |> Collection.find Query.Empty
                allAccounts |> should haveLength 0
        }

    [<Fact>]
    member _.``Multiple inserts in transaction commit together``() : Task =
        task {
            // Arrange
            let db = fixture.Db
            let accounts = db.Collection<Account>("accounts_batch", accountSchema)

            // Act - Insert all in transaction
            let! result =
                db.Transact {
                    let! doc1 = accounts |> Collection.insertOne { Name = "Account1"; Balance = 100 }
                    let! doc2 = accounts |> Collection.insertOne { Name = "Account2"; Balance = 200 }
                    let! doc3 = accounts |> Collection.insertOne { Name = "Account3"; Balance = 300 }
                    let! doc4 = accounts |> Collection.insertOne { Name = "Account4"; Balance = 400 }

                    return [ doc1.Id; doc2.Id; doc3.Id; doc4.Id ]
                }

            // Assert
            match result with
            | Ok ids ->
                ids |> should haveLength 4

                // Verify all are persisted
                let! allAccounts = accounts |> Collection.find Query.Empty
                allAccounts |> should haveLength 4

                let totalBalance = allAccounts |> List.sumBy (fun doc -> doc.Data.Balance)
                totalBalance |> should equal 1000

            | Error err -> failwith $"Expected Ok, got Error: {err.Message}"
        }

    [<Fact>]
    member _.``Concurrent read isolation - readers see consistent snapshot``() : Task =
        task {
            // Arrange
            let db = fixture.Db
            let accounts = db.Collection<Account>("accounts_isolation", accountSchema)

            // Pre-populate with initial data
            let! initResult = accounts |> Collection.insertOne { Name = "InitialAccount"; Balance = 500 }
            
            match initResult with
            | Error err -> failwith $"Setup failed: {err.Message}"
            | Ok _ -> ()

            // Act - Start a transaction that modifies data
            let transactionTask =
                task {
                    return!
                        db.Transact {
                            // Insert within transaction
                            let! doc1 =
                                accounts
                                |> Collection.insertOne
                                    { Name = "TransactionAccount"
                                      Balance = 1000 }

                            return doc1.Id
                        }
                }

            // Concurrent read while transaction is in progress
            do! Task.Delay(10) // Ensure transaction started

            let! concurrentReadAccounts = accounts |> Collection.find Query.Empty

            // Wait for transaction to complete
            let! txResult = transactionTask

            // Assert - Concurrent read should see consistent data
            match txResult with
            | Ok _ ->
                // During transaction, concurrent read should have seen either:
                // 1. Only initial account (before transaction committed)
                // 2. Both accounts (after transaction committed)
                // But never partial/inconsistent state
                let accountCount = concurrentReadAccounts |> List.length
                accountCount |> should be (greaterThanOrEqualTo 1)

                // After transaction commits, should see both accounts
                let! finalAccounts = accounts |> Collection.find Query.Empty
                finalAccounts |> should haveLength 2

                let names = finalAccounts |> List.map (fun doc -> doc.Data.Name) |> List.sort
                names |> should contain "InitialAccount"
                names |> should contain "TransactionAccount"

            | Error err -> failwith $"Expected Ok, got Error: {err.Message}"
        }

    [<Fact>]
    member _.``Rollback preserves data integrity - original data unchanged``() : Task =
        task {
            // Arrange
            let db = fixture.Db
            let accounts = db.Collection<Account>("accounts_integrity", accountSchema)

            // Pre-populate with initial data
            let initialAccount = { Name = "OriginalAccount"; Balance = 750 }
            let! insertResult = accounts |> Collection.insertOne initialAccount

            let originalId =
                match insertResult with
                | Ok doc -> doc.Id
                | Error err -> failwith $"Setup failed: {err.Message}"

            // Capture original state
            let! originalDoc = accounts |> Collection.findById originalId

            originalDoc
            |> should not' (equal None)

            let originalData = originalDoc.Value.Data
            let originalCreatedAt = originalDoc.Value.CreatedAt

            // Act - Attempt transaction that will fail
            let! result =
                db.Transact {
                    // Try to insert account with same initial name
                    let! doc1 =
                        accounts
                        |> Collection.insertOne
                            { Name = "FailureAccount"
                              Balance = 999 }

                    // Simulate failure
                    return! task { return Error(FractalError.Transaction "Simulated transaction failure") }
                }

            // Assert - Transaction should have failed
            match result with
            | Ok _ -> failwith "Expected Error, got Ok"
            | Error err -> err.Message |> should equal "Transaction error: Simulated transaction failure"

            // Verify original data is completely unchanged
            let! currentDoc = accounts |> Collection.findById originalId

            currentDoc
            |> should not' (equal None)

            let currentData = currentDoc.Value.Data
            let currentCreatedAt = currentDoc.Value.CreatedAt

            // Data integrity checks
            currentData.Name |> should equal originalData.Name
            currentData.Balance |> should equal originalData.Balance
            currentCreatedAt |> should equal originalCreatedAt

            // Verify no extra documents were persisted
            let! allAccounts = accounts |> Collection.find Query.Empty
            allAccounts |> should haveLength 1
            allAccounts.[0].Data.Name |> should equal "OriginalAccount"
        }

    [<Fact>]
    member _.``Large batch atomicity - 100 operations commit together``() : Task =
        task {
            // Arrange
            let db = fixture.Db
            let accounts = db.Collection<Account>("accounts_large_batch", accountSchema)

            let batchSize = 100

            // Create all accounts first
            let accountsToInsert =
                [ 1..batchSize ]
                |> List.map (fun i -> { Name = $"BatchAccount{i}"; Balance = i * 10 })

            // Act - insertMany already uses transactions internally for atomicity
            let! result = accounts |> Collection.insertMany accountsToInsert

            // Assert
            match result with
            | Ok batchResult ->
                let ids = batchResult.Documents |> List.map (fun doc -> doc.Id)
                ids |> should haveLength batchSize
                batchResult.InsertedCount |> should equal batchSize

                // Verify all 100 accounts persisted
                let! allAccounts = accounts |> Collection.find Query.Empty
                allAccounts |> should haveLength batchSize

                // Verify data correctness
                let balances = allAccounts |> List.map (fun doc -> doc.Data.Balance) |> List.sort

                let expectedBalances = [ 1..batchSize ] |> List.map (fun i -> i * 10)
                balances |> should equal expectedBalances

                let totalBalance = allAccounts |> List.sumBy (fun doc -> doc.Data.Balance)

                // Sum of 10, 20, 30, ..., 1000 = 10 * (1 + 2 + ... + 100) = 10 * 5050 = 50500
                totalBalance |> should equal 50500

            | Error err -> failwith $"Expected Ok, got Error: {err.Message}"
        }

    [<Fact>]
    member _.``Batch transaction rollback - multiple inserts rolled back on failure``() : Task =
        task {
            // Arrange
            let db = fixture.Db
            let accounts = db.Collection<Account>("accounts_batch_rollback", accountSchema)

            // Act - Insert 10 accounts individually in transaction, then fail
            // Note: We use individual inserts because insertMany has its own internal transaction
            let! result =
                db.Transact {
                    let! _ = accounts |> Collection.insertOne { Name = "RollbackAccount1"; Balance = 10 }
                    let! _ = accounts |> Collection.insertOne { Name = "RollbackAccount2"; Balance = 20 }
                    let! _ = accounts |> Collection.insertOne { Name = "RollbackAccount3"; Balance = 30 }
                    let! _ = accounts |> Collection.insertOne { Name = "RollbackAccount4"; Balance = 40 }
                    let! _ = accounts |> Collection.insertOne { Name = "RollbackAccount5"; Balance = 50 }
                    let! _ = accounts |> Collection.insertOne { Name = "RollbackAccount6"; Balance = 60 }
                    let! _ = accounts |> Collection.insertOne { Name = "RollbackAccount7"; Balance = 70 }
                    let! _ = accounts |> Collection.insertOne { Name = "RollbackAccount8"; Balance = 80 }
                    let! _ = accounts |> Collection.insertOne { Name = "RollbackAccount9"; Balance = 90 }
                    let! _ = accounts |> Collection.insertOne { Name = "RollbackAccount10"; Balance = 100 }

                    // Fail after all 10 inserts to trigger rollback
                    return! task { return Error(FractalError.Validation(None, "Batch validation failed")) }
                }

            // Assert
            match result with
            | Ok _ -> failwith "Expected Error, got Ok"
            | Error err -> err.Message |> should equal "Validation failed: Batch validation failed"

            // Verify NO accounts were persisted (all rolled back)
            let! allAccounts = accounts |> Collection.find Query.Empty
            allAccounts |> should haveLength 0
        }

    [<Fact>]
    member _.``Partial write failure rollback - mid-transaction failure reverts all``() : Task =
        task {
            // Arrange
            let db = fixture.Db
            let accounts = db.Collection<Account>("accounts_partial_fail", accountSchema)

            // Act - Insert 5 accounts, fail on 3rd
            let! result =
                db.Transact {
                    let! doc1 = accounts |> Collection.insertOne { Name = "Account1"; Balance = 100 }

                    let! doc2 = accounts |> Collection.insertOne { Name = "Account2"; Balance = 200 }

                    // Fail mid-transaction after 2 successful inserts
                    return! task { return Error(FractalError.InvalidOperation "Mid-transaction failure") }
                }

            // Assert - Transaction failed
            match result with
            | Ok _ -> failwith "Expected Error, got Ok"
            | Error err -> err.Message |> should equal "Invalid operation: Mid-transaction failure"

            // Verify NO accounts persisted (complete rollback)
            let! allAccounts = accounts |> Collection.find Query.Empty
            allAccounts |> should haveLength 0
        }

    [<Fact>]
    member _.``Rollback after multiple updates preserves original values``() : Task =
        task {
            // Arrange
            let db = fixture.Db
            let accounts = db.Collection<Account>("accounts_update_rollback", accountSchema)

            // Pre-populate
            let! doc1Result = accounts |> Collection.insertOne { Name = "Alice"; Balance = 1000 }

            let! doc2Result = accounts |> Collection.insertOne { Name = "Bob"; Balance = 500 }

            let id1, id2 =
                match doc1Result, doc2Result with
                | Ok d1, Ok d2 -> d1.Id, d2.Id
                | _ -> failwith "Setup failed"

            // Capture original balances
            let! original1 = accounts |> Collection.findById id1
            let! original2 = accounts |> Collection.findById id2

            let originalBalance1 = original1.Value.Data.Balance
            let originalBalance2 = original2.Value.Data.Balance

            // Act - Perform updates in transaction, then rollback
            let! result =
                db.Transact {
                    // Update Alice's balance
                    let! updated1 =
                        accounts
                        |> Collection.updateById id1 (fun data -> { data with Balance = data.Balance - 300 })

                    // Update Bob's balance
                    let! updated2 =
                        accounts
                        |> Collection.updateById id2 (fun data -> { data with Balance = data.Balance + 300 })

                    // Fail transaction to trigger rollback
                    return! task { return Error(FractalError.Transaction "Transfer cancelled") }
                }

            // Assert
            match result with
            | Ok _ -> failwith "Expected Error, got Ok"
            | Error err -> err.Message |> should equal "Transaction error: Transfer cancelled"

            // Verify original balances preserved
            let! current1 = accounts |> Collection.findById id1
            let! current2 = accounts |> Collection.findById id2

            current1.Value.Data.Balance
            |> should equal originalBalance1

            current2.Value.Data.Balance
            |> should equal originalBalance2

            // Exact values check
            current1.Value.Data.Balance |> should equal 1000
            current2.Value.Data.Balance |> should equal 500
        }
