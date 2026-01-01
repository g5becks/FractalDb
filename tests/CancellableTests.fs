module FractalDb.Tests.CancellableTests

// Suppress linter warnings for test code
// fsharplint:disable FL0072

open System
open System.Threading
open System.Threading.Tasks
open Xunit
open FsUnit.Xunit
open IcedTasks
open FractalDb.Types
open FractalDb.Errors
open FractalDb.Schema
open FractalDb.Operators
open FractalDb.Collection
open FractalDb.Database
open FractalDb.Options

/// <summary>
/// Tests for FractalDb.Cancellable module.
/// Verifies cancellable versions of all Collection operations work correctly
/// and properly respect CancellationToken.
/// </summary>

// ═══════════════════════════════════════════════════════════════════════════
// TEST TYPES AND FIXTURES
// ═══════════════════════════════════════════════════════════════════════════

type TestUser =
    { Name: string
      Email: string
      Age: int
      Active: bool }

let testUserSchema: SchemaDef<TestUser> =
    { Fields =
        [ { Name = "name"
            Path = None
            SqlType = SqliteType.Text
            Indexed = true
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
            Indexed = true
            Unique = false
            Nullable = false }
          { Name = "active"
            Path = None
            SqlType = SqliteType.Integer
            Indexed = false
            Unique = false
            Nullable = false } ]
      Indexes = []
      Timestamps = true
      Validate = None }

/// Test fixture providing shared in-memory database and collection
type CancellableTestFixture() =
    let db = FractalDb.InMemory()
    let users = db.Collection<TestUser>("cancellable_test_users", testUserSchema)

    member _.Db = db
    member _.Users = users

    interface IDisposable with
        member _.Dispose() = db.Close()

/// Custom error type for testing error transformation
type MapErrorTestError =
    | TestNotFound of string
    | TestQueryError of string
    | TestUnknown

/// Maps FractalError to MapErrorTestError
let toMapErrorTestError (err: FractalError) : MapErrorTestError =
    match err with
    | FractalError.NotFound id -> MapErrorTestError.TestNotFound id
    | FractalError.Query(msg, _) -> MapErrorTestError.TestQueryError msg
    | _ -> MapErrorTestError.TestUnknown

/// Tests for Cancellable module functions
type CancellableTests(fixture: CancellableTestFixture) =
    let users = fixture.Users

    interface IClassFixture<CancellableTestFixture>

    // ═══════════════════════════════════════════════════════════════════════
    // INSERT OPERATIONS
    // ═══════════════════════════════════════════════════════════════════════

    [<Fact>]
    member _.``insertOne creates document with cancellation token``() : Task =
        task {
            use cts = new CancellationTokenSource()

            let operation =
                FractalDb.Cancellable.insertOne
                    { Name = "CancellableAlice"
                      Email = $"alice-{Guid.NewGuid()}@test.com"
                      Age = 30
                      Active = true }
                    users

            let! result = operation cts.Token

            match result with
            | Ok doc ->
                doc.Id |> should not' (be EmptyString)
                doc.Data.Name |> should equal "CancellableAlice"
            | Error err -> failwith $"Expected Ok, got Error: {err.Message}"
        }

    [<Fact>]
    member _.``insertOne throws when token is cancelled before operation``() : Task =
        task {
            use cts = new CancellationTokenSource()
            cts.Cancel()

            let operation =
                FractalDb.Cancellable.insertOne
                    { Name = "ShouldFail"
                      Email = "fail@test.com"
                      Age = 25
                      Active = true }
                    users

            let mutable cancelled = false

            try
                let! _ = operation cts.Token
                ()
            with :? OperationCanceledException ->
                cancelled <- true

            cancelled |> should be True
        }

    [<Fact>]
    member _.``insertMany creates multiple documents with cancellation token``() : Task =
        task {
            use cts = new CancellationTokenSource()

            let docs =
                [ { Name = "User1"
                    Email = $"user1-{Guid.NewGuid()}@test.com"
                    Age = 25
                    Active = true }
                  { Name = "User2"
                    Email = $"user2-{Guid.NewGuid()}@test.com"
                    Age = 30
                    Active = false } ]

            let operation = FractalDb.Cancellable.insertMany docs users
            let! result = operation cts.Token

            match result with
            | Ok insertResult ->
                insertResult.Documents |> should haveLength 2
                insertResult.InsertedCount |> should equal 2
            | Error err -> failwith $"Expected Ok, got Error: {err.Message}"
        }

    // ═══════════════════════════════════════════════════════════════════════
    // FIND OPERATIONS
    // ═══════════════════════════════════════════════════════════════════════

    [<Fact>]
    member _.``findById returns Some for existing document with cancellation token``() : Task =
        task {
            use cts = new CancellationTokenSource()

            // First insert a document
            let! insertResult =
                users
                |> Collection.insertOne
                    { Name = "FindMe"
                      Email = $"findme-{Guid.NewGuid()}@test.com"
                      Age = 35
                      Active = true }

            let doc = Result.defaultWith (fun _ -> failwith "Insert failed") insertResult

            // Now find it with cancellable operation
            let operation = FractalDb.Cancellable.findById doc.Id users
            let! found = operation cts.Token

            found.IsSome |> should be True
            found.Value.Data.Name |> should equal "FindMe"
        }

    [<Fact>]
    member _.``findById returns None for non-existent document``() : Task =
        task {
            use cts = new CancellationTokenSource()

            let operation = FractalDb.Cancellable.findById "non-existent-id" users
            let! found = operation cts.Token

            found.IsNone |> should be True
        }

    [<Fact>]
    member _.``findOne returns matching document with cancellation token``() : Task =
        task {
            use cts = new CancellationTokenSource()

            let uniqueEmail = $"findone-{Guid.NewGuid()}@test.com"

            let! _ =
                users
                |> Collection.insertOne
                    { Name = "FindOneUser"
                      Email = uniqueEmail
                      Age = 40
                      Active = true }

            let operation =
                FractalDb.Cancellable.findOne
                    (Query.Field("email", FieldOp.Compare(box (CompareOp.Eq uniqueEmail))))
                    users

            let! found = operation cts.Token

            found.IsSome |> should be True
            found.Value.Data.Name |> should equal "FindOneUser"
        }

    [<Fact>]
    member _.``find returns all matching documents with cancellation token``() : Task =
        task {
            use cts = new CancellationTokenSource()

            // Insert some test users
            let uniquePrefix = Guid.NewGuid().ToString()

            let! _ =
                users
                |> Collection.insertOne
                    { Name = $"Active-{uniquePrefix}"
                      Email = $"active1-{uniquePrefix}@test.com"
                      Age = 25
                      Active = true }

            let! _ =
                users
                |> Collection.insertOne
                    { Name = $"Active-{uniquePrefix}"
                      Email = $"active2-{uniquePrefix}@test.com"
                      Age = 30
                      Active = true }

            let operation =
                FractalDb.Cancellable.find
                    (Query.Field("name", FieldOp.Compare(box (CompareOp.Eq $"Active-{uniquePrefix}"))))
                    users

            let! results = operation cts.Token

            results |> should haveLength 2
        }

    [<Fact>]
    member _.``count returns correct count with cancellation token``() : Task =
        task {
            use cts = new CancellationTokenSource()

            let uniqueName = $"CountTest-{Guid.NewGuid()}"

            let! _ =
                users
                |> Collection.insertOne
                    { Name = uniqueName
                      Email = $"count1-{Guid.NewGuid()}@test.com"
                      Age = 25
                      Active = true }

            let! _ =
                users
                |> Collection.insertOne
                    { Name = uniqueName
                      Email = $"count2-{Guid.NewGuid()}@test.com"
                      Age = 30
                      Active = true }

            let! _ =
                users
                |> Collection.insertOne
                    { Name = uniqueName
                      Email = $"count3-{Guid.NewGuid()}@test.com"
                      Age = 35
                      Active = true }

            let operation =
                FractalDb.Cancellable.count (Query.Field("name", FieldOp.Compare(box (CompareOp.Eq uniqueName)))) users

            let! count = operation cts.Token

            count |> should equal 3
        }

    [<Fact>]
    member _.``estimatedCount returns total count with cancellation token``() : Task =
        task {
            use cts = new CancellationTokenSource()

            let operation = FractalDb.Cancellable.estimatedCount users
            let! count = operation cts.Token

            count |> should be (greaterThanOrEqualTo 0)
        }

    // ═══════════════════════════════════════════════════════════════════════
    // UPDATE OPERATIONS
    // ═══════════════════════════════════════════════════════════════════════

    [<Fact>]
    member _.``updateById updates document with cancellation token``() : Task =
        task {
            use cts = new CancellationTokenSource()

            let! insertResult =
                users
                |> Collection.insertOne
                    { Name = "UpdateMe"
                      Email = $"update-{Guid.NewGuid()}@test.com"
                      Age = 25
                      Active = true }

            let doc = Result.defaultWith (fun _ -> failwith "Insert failed") insertResult

            let operation =
                FractalDb.Cancellable.updateById doc.Id (fun u -> { u with Age = 26 }) users

            let! updateResult = operation cts.Token

            match updateResult with
            | Ok(Some updated) -> updated.Data.Age |> should equal 26
            | Ok None -> failwith "Expected Some, got None"
            | Error err -> failwith $"Expected Ok, got Error: {err.Message}"
        }

    [<Fact>]
    member _.``updateOne updates first matching document with cancellation token``() : Task =
        task {
            use cts = new CancellationTokenSource()

            let uniqueEmail = $"updateone-{Guid.NewGuid()}@test.com"

            let! _ =
                users
                |> Collection.insertOne
                    { Name = "UpdateOneUser"
                      Email = uniqueEmail
                      Age = 30
                      Active = true }

            let operation =
                FractalDb.Cancellable.updateOne
                    (Query.Field("email", FieldOp.Compare(box (CompareOp.Eq uniqueEmail))))
                    (fun u -> { u with Active = false })
                    users

            let! updateResult = operation cts.Token

            match updateResult with
            | Ok(Some updated) -> updated.Data.Active |> should be False
            | Ok None -> failwith "Expected Some, got None"
            | Error err -> failwith $"Expected Ok, got Error: {err.Message}"
        }

    [<Fact>]
    member _.``updateMany updates all matching documents with cancellation token``() : Task =
        task {
            use cts = new CancellationTokenSource()

            let uniqueName = $"UpdateMany-{Guid.NewGuid()}"

            let! _ =
                users
                |> Collection.insertOne
                    { Name = uniqueName
                      Email = $"um1-{Guid.NewGuid()}@test.com"
                      Age = 20
                      Active = true }

            let! _ =
                users
                |> Collection.insertOne
                    { Name = uniqueName
                      Email = $"um2-{Guid.NewGuid()}@test.com"
                      Age = 25
                      Active = true }

            let operation =
                FractalDb.Cancellable.updateMany
                    (Query.Field("name", FieldOp.Compare(box (CompareOp.Eq uniqueName))))
                    (fun u -> { u with Active = false })
                    users

            let! updateResult = operation cts.Token

            match updateResult with
            | Ok result ->
                result.MatchedCount |> should equal 2
                result.ModifiedCount |> should equal 2
            | Error err -> failwith $"Expected Ok, got Error: {err.Message}"
        }

    [<Fact>]
    member _.``replaceOne replaces document with cancellation token``() : Task =
        task {
            use cts = new CancellationTokenSource()

            let uniqueEmail = $"replace-{Guid.NewGuid()}@test.com"

            let! _ =
                users
                |> Collection.insertOne
                    { Name = "ReplaceMe"
                      Email = uniqueEmail
                      Age = 30
                      Active = true }

            let operation =
                FractalDb.Cancellable.replaceOne
                    (Query.Field("email", FieldOp.Compare(box (CompareOp.Eq uniqueEmail))))
                    { Name = "Replaced"
                      Email = uniqueEmail
                      Age = 35
                      Active = false }
                    users

            let! replaceResult = operation cts.Token

            match replaceResult with
            | Ok(Some replaced) ->
                replaced.Data.Name |> should equal "Replaced"
                replaced.Data.Age |> should equal 35
            | Ok None -> failwith "Expected Some, got None"
            | Error err -> failwith $"Expected Ok, got Error: {err.Message}"
        }

    // ═══════════════════════════════════════════════════════════════════════
    // DELETE OPERATIONS
    // ═══════════════════════════════════════════════════════════════════════

    [<Fact>]
    member _.``deleteById deletes document with cancellation token``() : Task =
        task {
            use cts = new CancellationTokenSource()

            let! insertResult =
                users
                |> Collection.insertOne
                    { Name = "DeleteMe"
                      Email = $"delete-{Guid.NewGuid()}@test.com"
                      Age = 25
                      Active = true }

            let doc = Result.defaultWith (fun _ -> failwith "Insert failed") insertResult

            let operation = FractalDb.Cancellable.deleteById doc.Id users
            let! deleted = operation cts.Token

            deleted |> should be True

            // Verify it's gone
            let! found = users |> Collection.findById doc.Id
            found.IsNone |> should be True
        }

    [<Fact>]
    member _.``deleteOne deletes first matching document with cancellation token``() : Task =
        task {
            use cts = new CancellationTokenSource()

            let uniqueEmail = $"deleteone-{Guid.NewGuid()}@test.com"

            let! _ =
                users
                |> Collection.insertOne
                    { Name = "DeleteOneUser"
                      Email = uniqueEmail
                      Age = 30
                      Active = true }

            let operation =
                FractalDb.Cancellable.deleteOne
                    (Query.Field("email", FieldOp.Compare(box (CompareOp.Eq uniqueEmail))))
                    users

            let! deleted = operation cts.Token

            deleted |> should be True
        }

    [<Fact>]
    member _.``deleteMany deletes all matching documents with cancellation token``() : Task =
        task {
            use cts = new CancellationTokenSource()

            let uniqueName = $"DeleteMany-{Guid.NewGuid()}"

            let! _ =
                users
                |> Collection.insertOne
                    { Name = uniqueName
                      Email = $"dm1-{Guid.NewGuid()}@test.com"
                      Age = 20
                      Active = true }

            let! _ =
                users
                |> Collection.insertOne
                    { Name = uniqueName
                      Email = $"dm2-{Guid.NewGuid()}@test.com"
                      Age = 25
                      Active = true }

            let! _ =
                users
                |> Collection.insertOne
                    { Name = uniqueName
                      Email = $"dm3-{Guid.NewGuid()}@test.com"
                      Age = 30
                      Active = true }

            let operation =
                FractalDb.Cancellable.deleteMany
                    (Query.Field("name", FieldOp.Compare(box (CompareOp.Eq uniqueName))))
                    users

            let! deleteResult = operation cts.Token

            deleteResult.DeletedCount |> should equal 3
        }

    // ═══════════════════════════════════════════════════════════════════════
    // ATOMIC FIND-AND-MODIFY OPERATIONS
    // ═══════════════════════════════════════════════════════════════════════

    [<Fact>]
    member _.``findOneAndDelete atomically finds and deletes with cancellation token``() : Task =
        task {
            use cts = new CancellationTokenSource()

            let uniqueEmail = $"finddelete-{Guid.NewGuid()}@test.com"

            let! _ =
                users
                |> Collection.insertOne
                    { Name = "FindAndDelete"
                      Email = uniqueEmail
                      Age = 30
                      Active = true }

            let operation =
                FractalDb.Cancellable.findOneAndDelete
                    (Query.Field("email", FieldOp.Compare(box (CompareOp.Eq uniqueEmail))))
                    users

            let! result = operation cts.Token

            result.IsSome |> should be True
            result.Value.Data.Name |> should equal "FindAndDelete"

            // Verify it's gone using regular Collection.findOne
            let! foundResult =
                users
                |> Collection.findOne (Query.Field("email", FieldOp.Compare(box (CompareOp.Eq uniqueEmail))))

            foundResult.IsNone |> should be True
        }

    [<Fact>]
    member _.``findOneAndUpdate atomically finds and updates with cancellation token``() : Task =
        task {
            use cts = new CancellationTokenSource()

            let uniqueEmail = $"findupdate-{Guid.NewGuid()}@test.com"

            let! _ =
                users
                |> Collection.insertOne
                    { Name = "FindAndUpdate"
                      Email = uniqueEmail
                      Age = 30
                      Active = true }

            let options: FindAndModifyOptions =
                { Sort = []
                  Upsert = false
                  ReturnDocument = ReturnDocument.After }

            let operation =
                FractalDb.Cancellable.findOneAndUpdate
                    (Query.Field("email", FieldOp.Compare(box (CompareOp.Eq uniqueEmail))))
                    (fun u -> { u with Age = 31 })
                    options
                    users

            let! result = operation cts.Token

            match result with
            | Ok(Some doc) -> doc.Data.Age |> should equal 31
            | Ok None -> failwith "Expected Some, got None"
            | Error err -> failwith $"Expected Ok, got Error: {err.Message}"
        }

    [<Fact>]
    member _.``findOneAndReplace atomically finds and replaces with cancellation token``() : Task =
        task {
            use cts = new CancellationTokenSource()

            let uniqueEmail = $"findreplace-{Guid.NewGuid()}@test.com"

            let! _ =
                users
                |> Collection.insertOne
                    { Name = "FindAndReplace"
                      Email = uniqueEmail
                      Age = 30
                      Active = true }

            let options: FindAndModifyOptions =
                { Sort = []
                  Upsert = false
                  ReturnDocument = ReturnDocument.After }

            let operation =
                FractalDb.Cancellable.findOneAndReplace
                    (Query.Field("email", FieldOp.Compare(box (CompareOp.Eq uniqueEmail))))
                    { Name = "Replaced"
                      Email = uniqueEmail
                      Age = 99
                      Active = false }
                    options
                    users

            let! result = operation cts.Token

            match result with
            | Ok(Some doc) ->
                doc.Data.Name |> should equal "Replaced"
                doc.Data.Age |> should equal 99
            | Ok None -> failwith "Expected Some, got None"
            | Error err -> failwith $"Expected Ok, got Error: {err.Message}"
        }

    // ═══════════════════════════════════════════════════════════════════════
    // SEARCH AND DISTINCT OPERATIONS
    // ═══════════════════════════════════════════════════════════════════════

    [<Fact>]
    member _.``search finds documents by text with cancellation token``() : Task =
        task {
            use cts = new CancellationTokenSource()

            let uniqueSearchName = $"SearchableUser-{Guid.NewGuid()}"

            let! _ =
                users
                |> Collection.insertOne
                    { Name = uniqueSearchName
                      Email = $"search-{Guid.NewGuid()}@test.com"
                      Age = 30
                      Active = true }

            let operation = FractalDb.Cancellable.search uniqueSearchName [ "name" ] users
            let! results = operation cts.Token

            results |> List.isEmpty |> should be False
            results |> List.head |> (fun d -> d.Data.Name |> should equal uniqueSearchName)
        }

    [<Fact>]
    member _.``distinct returns unique values with cancellation token``() : Task =
        task {
            use cts = new CancellationTokenSource()

            let uniquePrefix = Guid.NewGuid().ToString()

            let! _ =
                users
                |> Collection.insertOne
                    { Name = $"Dist-{uniquePrefix}"
                      Email = $"d1-{Guid.NewGuid()}@test.com"
                      Age = 25
                      Active = true }

            let! _ =
                users
                |> Collection.insertOne
                    { Name = $"Dist-{uniquePrefix}"
                      Email = $"d2-{Guid.NewGuid()}@test.com"
                      Age = 30
                      Active = true }

            let! _ =
                users
                |> Collection.insertOne
                    { Name = $"Dist-{uniquePrefix}"
                      Email = $"d3-{Guid.NewGuid()}@test.com"
                      Age = 25
                      Active = true }

            let operation =
                FractalDb.Cancellable.distinct<TestUser, int>
                    "age"
                    (Query.Field("name", FieldOp.Compare(box (CompareOp.Eq $"Dist-{uniquePrefix}"))))
                    users

            let! result = operation cts.Token

            match result with
            | Ok ages ->
                ages |> should contain 25
                ages |> should contain 30
                ages |> should haveLength 2
            | Error err -> failwith $"Expected Ok, got Error: {err.Message}"
        }

    // ═══════════════════════════════════════════════════════════════════════
    // CANCELLATION BEHAVIOR TESTS
    // ═══════════════════════════════════════════════════════════════════════

    [<Fact>]
    member _.``all read operations respect pre-cancelled token``() : Task =
        task {
            use cts = new CancellationTokenSource()
            cts.Cancel()

            // findById
            let mutable cancelled = false

            try
                let! _ = (FractalDb.Cancellable.findById "id" users) cts.Token
                ()
            with :? OperationCanceledException ->
                cancelled <- true

            cancelled |> should be True

            // findOne
            cancelled <- false

            try
                let! _ = (FractalDb.Cancellable.findOne Query.Empty users) cts.Token
                ()
            with :? OperationCanceledException ->
                cancelled <- true

            cancelled |> should be True

            // find
            cancelled <- false

            try
                let! _ = (FractalDb.Cancellable.find Query.Empty users) cts.Token
                ()
            with :? OperationCanceledException ->
                cancelled <- true

            cancelled |> should be True

            // count
            cancelled <- false

            try
                let! _ = (FractalDb.Cancellable.count Query.Empty users) cts.Token
                ()
            with :? OperationCanceledException ->
                cancelled <- true

            cancelled |> should be True
        }

    [<Fact>]
    member _.``all write operations respect pre-cancelled token``() : Task =
        task {
            use cts = new CancellationTokenSource()
            cts.Cancel()

            // insertOne
            let mutable cancelled = false

            try
                let! _ =
                    (FractalDb.Cancellable.insertOne
                        { Name = "Test"
                          Email = "test@test.com"
                          Age = 25
                          Active = true }
                        users)
                        cts.Token

                ()
            with :? OperationCanceledException ->
                cancelled <- true

            cancelled |> should be True

            // updateById
            cancelled <- false

            try
                let! _ = (FractalDb.Cancellable.updateById "id" id users) cts.Token
                ()
            with :? OperationCanceledException ->
                cancelled <- true

            cancelled |> should be True

            // deleteById
            cancelled <- false

            try
                let! _ = (FractalDb.Cancellable.deleteById "id" users) cts.Token
                ()
            with :? OperationCanceledException ->
                cancelled <- true

            cancelled |> should be True
        }

    // ═══════════════════════════════════════════════════════════════════════
    // ADDITIONAL MODULE FUNCTION TESTS
    // ═══════════════════════════════════════════════════════════════════════

    [<Fact>]
    member _.``insertOne via module function with explicit token passing``() : Task =
        task {
            use cts = new CancellationTokenSource()

            // Module function returns CancellableTask, invoke with token
            let! result =
                (FractalDb.Cancellable.insertOne
                    { Name = "ModuleUser"
                      Email = $"mod-{Guid.NewGuid()}@test.com"
                      Age = 25
                      Active = true }
                    users)
                    cts.Token

            match result with
            | Ok doc -> doc.Data.Name |> should equal "ModuleUser"
            | Error err -> failwith $"Expected Ok, got Error: {err.Message}"
        }

    [<Fact>]
    member _.``findById via module function with explicit token passing``() : Task =
        task {
            use cts = new CancellationTokenSource()

            // First insert
            let! insertResult =
                (FractalDb.Cancellable.insertOne
                    { Name = "FindByIdMod"
                      Email = $"findbyidmod-{Guid.NewGuid()}@test.com"
                      Age = 30
                      Active = true }
                    users)
                    cts.Token

            let doc = Result.defaultWith (fun _ -> failwith "Insert failed") insertResult

            // Find using module function
            let! found = (FractalDb.Cancellable.findById doc.Id users) cts.Token

            found.IsSome |> should be True
            found.Value.Data.Name |> should equal "FindByIdMod"
        }

    [<Fact>]
    member _.``updateById via module function with explicit token passing``() : Task =
        task {
            use cts = new CancellationTokenSource()

            let! insertResult =
                (FractalDb.Cancellable.insertOne
                    { Name = "UpdateMod"
                      Email = $"updatemod-{Guid.NewGuid()}@test.com"
                      Age = 25
                      Active = true }
                    users)
                    cts.Token

            let doc = Result.defaultWith (fun _ -> failwith "Insert failed") insertResult

            let! updateResult = (FractalDb.Cancellable.updateById doc.Id (fun u -> { u with Age = 26 }) users) cts.Token

            match updateResult with
            | Ok(Some updated) -> updated.Data.Age |> should equal 26
            | Ok None -> failwith "Expected Some, got None"
            | Error err -> failwith $"Expected Ok, got Error: {err.Message}"
        }

    [<Fact>]
    member _.``deleteById via module function with explicit token passing``() : Task =
        task {
            use cts = new CancellationTokenSource()

            let! insertResult =
                (FractalDb.Cancellable.insertOne
                    { Name = "DeleteMod"
                      Email = $"deletemod-{Guid.NewGuid()}@test.com"
                      Age = 25
                      Active = true }
                    users)
                    cts.Token

            let doc = Result.defaultWith (fun _ -> failwith "Insert failed") insertResult

            let! deleted = (FractalDb.Cancellable.deleteById doc.Id users) cts.Token

            deleted |> should be True
        }

    [<Fact>]
    member _.``count via module function with explicit token passing``() : Task =
        task {
            use cts = new CancellationTokenSource()

            let! count = (FractalDb.Cancellable.count Query.Empty users) cts.Token

            count |> should be (greaterThanOrEqualTo 0)
        }

    // ═══════════════════════════════════════════════════════════════════════
    // ICEDTASKS INTEGRATION TESTS
    // ═══════════════════════════════════════════════════════════════════════

    [<Fact>]
    member _.``cancellableTask CE works with Cancellable module functions``() : Task =
        task {
            use cts = new CancellationTokenSource()

            let workflow =
                cancellableTask {
                    let! insertResult =
                        FractalDb.Cancellable.insertOne
                            { Name = "CEUser"
                              Email = $"ce-{Guid.NewGuid()}@test.com"
                              Age = 30
                              Active = true }
                            users

                    match insertResult with
                    | Ok doc ->
                        let! found = FractalDb.Cancellable.findById doc.Id users
                        return found
                    | Error _ -> return None
                }

            let! result = workflow cts.Token

            result.IsSome |> should be True
            result.Value.Data.Name |> should equal "CEUser"
        }

    [<Fact>]
    member _.``cancellableTask CE propagates cancellation automatically``() : Task =
        task {
            use cts = new CancellationTokenSource()
            cts.Cancel()

            let workflow =
                cancellableTask {
                    let! _ =
                        FractalDb.Cancellable.insertOne
                            { Name = "ShouldNotRun"
                              Email = "norun@test.com"
                              Age = 25
                              Active = true }
                            users

                    return "completed"
                }

            let mutable cancelled = false

            try
                let! _ = workflow cts.Token
                ()
            with :? OperationCanceledException ->
                cancelled <- true

            cancelled |> should be True
        }

    [<Fact>]
    member _.``cancellableTask CE works with module functions``() : Task =
        task {
            use cts = new CancellationTokenSource()

            let workflow =
                cancellableTask {
                    let! insertResult =
                        FractalDb.Cancellable.insertOne
                            { Name = "ExtCEUser"
                              Email = $"extce-{Guid.NewGuid()}@test.com"
                              Age = 35
                              Active = true }
                            users

                    match insertResult with
                    | Ok doc ->
                        let! updated = FractalDb.Cancellable.updateById doc.Id (fun u -> { u with Age = 36 }) users
                        return updated
                    | Error err -> return Error err
                }

            let! result = workflow cts.Token

            match result with
            | Ok(Some doc) -> doc.Data.Age |> should equal 36
            | Ok None -> failwith "Expected Some, got None"
            | Error err -> failwith $"Expected Ok, got Error: {err.Message}"
        }

    [<Fact>]
    member _.``complex cancellableTask workflow with multiple operations``() : Task =
        task {
            use cts = new CancellationTokenSource()

            let workflow =
                cancellableTask {
                    // Insert multiple users
                    let! r1 =
                        FractalDb.Cancellable.insertOne
                            { Name = "Workflow1"
                              Email = $"wf1-{Guid.NewGuid()}@test.com"
                              Age = 20
                              Active = true }
                            users

                    let! r2 =
                        FractalDb.Cancellable.insertOne
                            { Name = "Workflow2"
                              Email = $"wf2-{Guid.NewGuid()}@test.com"
                              Age = 25
                              Active = true }
                            users

                    let! r3 =
                        FractalDb.Cancellable.insertOne
                            { Name = "Workflow3"
                              Email = $"wf3-{Guid.NewGuid()}@test.com"
                              Age = 30
                              Active = true }
                            users

                    match r1, r2, r3 with
                    | Ok d1, Ok d2, Ok d3 ->
                        // Update one
                        let! _ = FractalDb.Cancellable.updateById d2.Id (fun u -> { u with Age = 26 }) users

                        // Delete one
                        let! _ = FractalDb.Cancellable.deleteById d3.Id users

                        // Find remaining
                        let! found = FractalDb.Cancellable.findById d1.Id users
                        let! count = FractalDb.Cancellable.count Query.Empty users

                        return (found, count)
                    | _ -> return (None, 0)
                }

            let! (found, count) = workflow cts.Token

            found.IsSome |> should be True
            count |> should be (greaterThan 0)
        }

    // ═══════════════════════════════════════════════════════════════════════
    // CANCELLABLE TASK RESULT UTILITY TESTS
    // ═══════════════════════════════════════════════════════════════════════

    [<Fact>]
    member _.``CancellableTaskResult.mapError transforms error type on Error``() : Task =
        task {
            use cts = new CancellationTokenSource()

            // Create a CancellableTask that returns an error
            let errorTask: CancellationToken -> Task<Result<int, FractalError>> =
                fun _ -> Task.FromResult(Error(FractalError.NotFound "test-id"))

            let mapped =
                FractalDb.Cancellable.CancellableTaskResult.mapError toMapErrorTestError errorTask

            let! result = mapped cts.Token

            match result with
            | Error(MapErrorTestError.TestNotFound id) -> id |> should equal "test-id"
            | Error other -> failwith $"Expected MapErrorTestError.TestNotFound, got {other}"
            | Ok _ -> failwith "Expected Error, got Ok"
        }

    [<Fact>]
    member _.``CancellableTaskResult.mapError preserves Ok value unchanged``() : Task =
        task {
            use cts = new CancellationTokenSource()

            // Create a CancellableTask that returns Ok
            let okTask: CancellationToken -> Task<Result<string, FractalError>> =
                fun _ -> Task.FromResult(Ok "success-value")

            let mapped =
                FractalDb.Cancellable.CancellableTaskResult.mapError toMapErrorTestError okTask

            let! result = mapped cts.Token

            match result with
            | Ok value -> value |> should equal "success-value"
            | Error _ -> failwith "Expected Ok, got Error"
        }

    [<Fact>]
    member _.``CancellableTaskResult.mapError works with real FractalDb operations``() : Task =
        task {
            use cts = new CancellationTokenSource()

            // Insert a document, then map the error type
            let operation =
                FractalDb.Cancellable.insertOne
                    { Name = "MapErrorTest"
                      Email = $"maperror-{Guid.NewGuid()}@test.com"
                      Age = 25
                      Active = true }
                    users
                |> FractalDb.Cancellable.CancellableTaskResult.mapError toMapErrorTestError

            let! result = operation cts.Token

            match result with
            | Ok doc -> doc.Data.Name |> should equal "MapErrorTest"
            | Error err -> failwith $"Expected Ok, got Error: {err}"
        }

    [<Fact>]
    member _.``CancellableTaskResult.bimap transforms both Ok and Error``() : Task =
        task {
            use cts = new CancellationTokenSource()

            // Test with Ok value
            let okTask: CancellationToken -> Task<Result<int, FractalError>> =
                fun _ -> Task.FromResult(Ok 42)

            let mappedOk =
                FractalDb.Cancellable.CancellableTaskResult.bimap (fun x -> x * 2) toMapErrorTestError okTask

            let! okResult = mappedOk cts.Token

            match okResult with
            | Ok value -> value |> should equal 84
            | Error _ -> failwith "Expected Ok, got Error"

            // Test with Error value
            let errorTask: CancellationToken -> Task<Result<int, FractalError>> =
                fun _ -> Task.FromResult(Error(FractalError.Query("db error", None)))

            let mappedError =
                FractalDb.Cancellable.CancellableTaskResult.bimap (fun x -> x * 2) toMapErrorTestError errorTask

            let! errorResult = mappedError cts.Token

            match errorResult with
            | Error(MapErrorTestError.TestQueryError msg) -> msg |> should equal "db error"
            | Error other -> failwith $"Expected MapErrorTestError.TestQueryError, got {other}"
            | Ok _ -> failwith "Expected Error, got Ok"
        }

    [<Fact>]
    member _.``CancellableTaskResult.mapError works in cancellableTask CE``() : Task =
        task {
            use cts = new CancellationTokenSource()

            let workflow =
                cancellableTask {
                    let! result =
                        FractalDb.Cancellable.insertOne
                            { Name = "CEMapError"
                              Email = $"cemaperror-{Guid.NewGuid()}@test.com"
                              Age = 30
                              Active = true }
                            users
                        |> FractalDb.Cancellable.CancellableTaskResult.mapError toMapErrorTestError

                    return result
                }

            let! result = workflow cts.Token

            match result with
            | Ok doc -> doc.Data.Name |> should equal "CEMapError"
            | Error err -> failwith $"Expected Ok, got Error: {err}"
        }

    [<Fact>]
    member _.``CancellableTaskResult.mapError respects cancellation``() : Task =
        task {
            use cts = new CancellationTokenSource()
            cts.Cancel()

            let operation =
                FractalDb.Cancellable.insertOne
                    { Name = "ShouldNotRun"
                      Email = "shouldnotrun@test.com"
                      Age = 25
                      Active = true }
                    users
                |> FractalDb.Cancellable.CancellableTaskResult.mapError toMapErrorTestError

            let mutable cancelled = false

            try
                let! _ = operation cts.Token
                ()
            with :? OperationCanceledException ->
                cancelled <- true

            cancelled |> should be True
        }
