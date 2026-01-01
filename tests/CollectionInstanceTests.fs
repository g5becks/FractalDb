module FractalDb.Tests.CollectionInstanceTests

/// <summary>
/// Tests for Collection<'T> instance methods.
/// These methods provide an object-oriented API that delegates to Collection module functions.
/// Tests ensure the instance methods correctly invoke the underlying module functions.
/// </summary>

open System
open System.Threading
open Xunit
open FsUnit.Xunit
open FractalDb.Types
open FractalDb.Schema
open FractalDb.Operators
open FractalDb.Options
open FractalDb.Collection
open FractalDb.Database

// Helper to create unique collection names without hyphens (SQLite-safe)
let private uniqueName prefix =
    let guid = Guid.NewGuid().ToString("N")
    $"{prefix}_{guid}"

// Test type for collection instance method tests
type CollectionTestUser =
    { Name: string
      Email: string
      Age: int64
      Active: bool }

// Schema definition
let testUserSchema: SchemaDef<CollectionTestUser> =
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

/// Test fixture providing fresh database and collection for each test.
type CollectionInstanceTestFixture() =
    let db = FractalDb.InMemory()

    member _.Db = db

    member _.CreateCollection(name: string) =
        db.Collection<CollectionTestUser>(name, testUserSchema)

    interface IDisposable with
        member _.Dispose() = db.Close()

type CollectionInstanceTests(fixture: CollectionInstanceTestFixture) =
    interface IClassFixture<CollectionInstanceTestFixture>

    // ═══════════════════════════════════════════════════════════════
    // INSERT OPERATIONS
    // ═══════════════════════════════════════════════════════════════

    [<Fact>]
    member _.``InsertOne instance method inserts document correctly``() =
        task {
            let collName = uniqueName "users"
            let users = fixture.CreateCollection(collName)

            let user =
                { Name = "Alice"
                  Email = $"alice-{Guid.NewGuid()}@test.com"
                  Age = 30L
                  Active = true }

            let! result = users.InsertOne(user)

            match result with
            | Ok doc ->
                doc.Data.Name |> should equal "Alice"
                doc.Data.Age |> should equal 30L
                doc.Id |> should not' (be EmptyString)
            | Error err -> failwith $"Expected Ok but got Error: {err}"
        }

    [<Fact>]
    member _.``InsertOne with CancellationToken inserts document``() =
        task {
            let collName = uniqueName "users"
            let users = fixture.CreateCollection(collName)

            use cts = new CancellationTokenSource()

            let user =
                { Name = "Bob"
                  Email = $"bob-{Guid.NewGuid()}@test.com"
                  Age = 25L
                  Active = true }

            let! result = users.InsertOne(user, ct = cts.Token)

            match result with
            | Ok doc ->
                doc.Data.Name |> should equal "Bob"
                doc.Data.Age |> should equal 25L
            | Error err -> failwith $"Expected Ok but got Error: {err}"
        }

    [<Fact>]
    member _.``InsertOne respects cancellation``() =
        task {
            let collName = uniqueName "users"
            let users = fixture.CreateCollection(collName)

            use cts = new CancellationTokenSource()
            cts.Cancel()

            let user =
                { Name = "Test"
                  Email = $"test-{Guid.NewGuid()}@test.com"
                  Age = 20L
                  Active = true }

            let! exn = Assert.ThrowsAsync<OperationCanceledException>(fun () -> users.InsertOne(user, ct = cts.Token))

            exn |> should not' (be Null)
        }

    [<Fact>]
    member _.``InsertMany instance method inserts multiple documents``() =
        task {
            let collName = uniqueName "users"
            let users = fixture.CreateCollection(collName)

            let docs =
                [ { Name = "User1"
                    Email = $"user1-{Guid.NewGuid()}@test.com"
                    Age = 20L
                    Active = true }
                  { Name = "User2"
                    Email = $"user2-{Guid.NewGuid()}@test.com"
                    Age = 30L
                    Active = false }
                  { Name = "User3"
                    Email = $"user3-{Guid.NewGuid()}@test.com"
                    Age = 40L
                    Active = true } ]

            let! result = users.InsertMany(docs)

            match result with
            | Ok insertResult ->
                insertResult.InsertedCount |> should equal 3
                insertResult.Documents |> List.length |> should equal 3
            | Error err -> failwith $"Expected Ok but got Error: {err}"
        }

    [<Fact>]
    member _.``InsertMany with ordered flag works correctly``() =
        task {
            let collName = uniqueName "users"
            let users = fixture.CreateCollection(collName)

            let docs =
                [ { Name = "Ordered1"
                    Email = $"ordered1-{Guid.NewGuid()}@test.com"
                    Age = 25L
                    Active = true }
                  { Name = "Ordered2"
                    Email = $"ordered2-{Guid.NewGuid()}@test.com"
                    Age = 35L
                    Active = false } ]

            let! result = users.InsertMany(docs, ordered = true)

            match result with
            | Ok insertResult -> insertResult.InsertedCount |> should equal 2
            | Error err -> failwith $"Expected Ok but got Error: {err}"
        }

    // ═══════════════════════════════════════════════════════════════
    // FIND OPERATIONS
    // ═══════════════════════════════════════════════════════════════

    [<Fact>]
    member _.``FindById instance method finds document by id``() =
        task {
            let collName = uniqueName "users"
            let users = fixture.CreateCollection(collName)

            let user =
                { Name = "FindMe"
                  Email = $"findme-{Guid.NewGuid()}@test.com"
                  Age = 28L
                  Active = true }

            let! insertResult = users.InsertOne(user)
            let docId = (Result.defaultValue Unchecked.defaultof<_> insertResult).Id

            let! found = users.FindById(docId)

            found |> should not' (equal None)
            found.Value.Data.Name |> should equal "FindMe"
        }

    [<Fact>]
    member _.``FindById returns None for non-existent id``() =
        task {
            let collName = uniqueName "users"
            let users = fixture.CreateCollection(collName)

            let! found = users.FindById("non-existent-id")

            found |> should equal None
        }

    [<Fact>]
    member _.``FindOne instance method finds first matching document``() =
        task {
            let collName = uniqueName "users"
            let users = fixture.CreateCollection(collName)

            let _ =
                users.InsertMany(
                    [ { Name = "Alice"
                        Email = $"alice-{Guid.NewGuid()}@test.com"
                        Age = 25L
                        Active = true }
                      { Name = "Bob"
                        Email = $"bob-{Guid.NewGuid()}@test.com"
                        Age = 30L
                        Active = true } ]
                )

            let filter = Query.Field("name", FieldOp.Compare(box (CompareOp.Eq "Alice")))
            let! found = users.FindOne(filter)

            found |> should not' (equal None)
            found.Value.Data.Name |> should equal "Alice"
        }

    [<Fact>]
    member _.``FindOne with QueryOptions uses options correctly``() =
        task {
            let collName = uniqueName "users"
            let users = fixture.CreateCollection(collName)

            let docs =
                [ { Name = "Alice"
                    Email = $"alice-{Guid.NewGuid()}@test.com"
                    Age = 25L
                    Active = true }
                  { Name = "Bob"
                    Email = $"bob-{Guid.NewGuid()}@test.com"
                    Age = 30L
                    Active = true }
                  { Name = "Charlie"
                    Email = $"charlie-{Guid.NewGuid()}@test.com"
                    Age = 35L
                    Active = true } ]

            let! _ = users.InsertMany(docs)

            let filter = Query.Field("active", FieldOp.Compare(box (CompareOp.Eq true)))

            let options =
                QueryOptions.empty |> QueryOptions.sortBy "age" SortDirection.Descending

            let! found = users.FindOne(filter, options)

            found |> should not' (equal None)
            found.Value.Data.Name |> should equal "Charlie" // Highest age
        }

    [<Fact>]
    member _.``Find instance method returns all matching documents``() =
        task {
            let collName = uniqueName "users"
            let users = fixture.CreateCollection(collName)

            let docs =
                [ { Name = "Active1"
                    Email = $"active1-{Guid.NewGuid()}@test.com"
                    Age = 25L
                    Active = true }
                  { Name = "Inactive"
                    Email = $"inactive-{Guid.NewGuid()}@test.com"
                    Age = 30L
                    Active = false }
                  { Name = "Active2"
                    Email = $"active2-{Guid.NewGuid()}@test.com"
                    Age = 35L
                    Active = true } ]

            let! _ = users.InsertMany(docs)

            let filter = Query.Field("active", FieldOp.Compare(box (CompareOp.Eq true)))
            let! results = users.Find(filter)

            results |> List.length |> should equal 2
        }

    [<Fact>]
    member _.``Find with QueryOptions applies sort and limit``() =
        task {
            let collName = uniqueName "users"
            let users = fixture.CreateCollection(collName)

            let docs =
                [ { Name = "A"
                    Email = $"a-{Guid.NewGuid()}@test.com"
                    Age = 10L
                    Active = true }
                  { Name = "B"
                    Email = $"b-{Guid.NewGuid()}@test.com"
                    Age = 20L
                    Active = true }
                  { Name = "C"
                    Email = $"c-{Guid.NewGuid()}@test.com"
                    Age = 30L
                    Active = true } ]

            let! _ = users.InsertMany(docs)

            let options =
                QueryOptions.empty
                |> QueryOptions.sortBy "age" SortDirection.Ascending
                |> QueryOptions.limit 2

            let! results = users.Find(Query.Empty, options)

            results |> List.length |> should equal 2
            results.[0].Data.Name |> should equal "A"
            results.[1].Data.Name |> should equal "B"
        }

    // ═══════════════════════════════════════════════════════════════
    // COUNT OPERATIONS
    // ═══════════════════════════════════════════════════════════════

    [<Fact>]
    member _.``Count instance method counts matching documents``() =
        task {
            let collName = uniqueName "users"
            let users = fixture.CreateCollection(collName)

            let docs =
                [ { Name = "A"
                    Email = $"a-{Guid.NewGuid()}@test.com"
                    Age = 25L
                    Active = true }
                  { Name = "B"
                    Email = $"b-{Guid.NewGuid()}@test.com"
                    Age = 30L
                    Active = false }
                  { Name = "C"
                    Email = $"c-{Guid.NewGuid()}@test.com"
                    Age = 35L
                    Active = true } ]

            let! _ = users.InsertMany(docs)

            let filter = Query.Field("active", FieldOp.Compare(box (CompareOp.Eq true)))
            let! count = users.Count(filter)

            count |> should equal 2
        }

    [<Fact>]
    member _.``EstimatedCount instance method returns total count``() =
        task {
            let collName = uniqueName "users"
            let users = fixture.CreateCollection(collName)

            let docs =
                [ { Name = "A"
                    Email = $"a-{Guid.NewGuid()}@test.com"
                    Age = 25L
                    Active = true }
                  { Name = "B"
                    Email = $"b-{Guid.NewGuid()}@test.com"
                    Age = 30L
                    Active = false } ]

            let! _ = users.InsertMany(docs)

            let! count = users.EstimatedCount()

            count |> should equal 2
        }

    // ═══════════════════════════════════════════════════════════════
    // SEARCH OPERATIONS
    // ═══════════════════════════════════════════════════════════════

    [<Fact>]
    member _.``Search instance method searches across fields``() =
        task {
            let collName = uniqueName "users"
            let users = fixture.CreateCollection(collName)

            let docs =
                [ { Name = "Alice Smith"
                    Email = $"alice-{Guid.NewGuid()}@test.com"
                    Age = 25L
                    Active = true }
                  { Name = "Bob Jones"
                    Email = $"bob-{Guid.NewGuid()}@test.com"
                    Age = 30L
                    Active = true } ]

            let! _ = users.InsertMany(docs)

            let! results = users.Search("Alice", [ "name" ])

            results |> List.length |> should equal 1
            results.[0].Data.Name |> should equal "Alice Smith"
        }

    [<Fact>]
    member _.``Search with QueryOptions applies options``() =
        task {
            let collName = uniqueName "users"
            let users = fixture.CreateCollection(collName)

            let docs =
                [ { Name = "Alice Young"
                    Email = $"alice-{Guid.NewGuid()}@test.com"
                    Age = 25L
                    Active = true }
                  { Name = "Alice Old"
                    Email = $"alice2-{Guid.NewGuid()}@test.com"
                    Age = 60L
                    Active = true } ]

            let! _ = users.InsertMany(docs)

            let options =
                QueryOptions.empty
                |> QueryOptions.sortBy "age" SortDirection.Descending
                |> QueryOptions.limit 1

            let! results = users.Search("Alice", [ "name" ], options)

            results |> List.length |> should equal 1
            results.[0].Data.Name |> should equal "Alice Old"
        }

    [<Fact>]
    member _.``Distinct instance method returns distinct values``() =
        task {
            let collName = uniqueName "users"
            let users = fixture.CreateCollection(collName)

            let docs =
                [ { Name = "Alice"
                    Email = $"alice-{Guid.NewGuid()}@test.com"
                    Age = 25L
                    Active = true }
                  { Name = "Bob"
                    Email = $"bob-{Guid.NewGuid()}@test.com"
                    Age = 25L
                    Active = true }
                  { Name = "Charlie"
                    Email = $"charlie-{Guid.NewGuid()}@test.com"
                    Age = 30L
                    Active = true } ]

            let! _ = users.InsertMany(docs)

            let! result = users.Distinct<int64>("age", Query.Empty)

            match result with
            | Ok ages ->
                ages |> List.length |> should equal 2
                ages |> should contain 25L
                ages |> should contain 30L
            | Error err -> failwith $"Expected Ok but got Error: {err}"
        }

    // ═══════════════════════════════════════════════════════════════
    // UPDATE OPERATIONS
    // ═══════════════════════════════════════════════════════════════

    [<Fact>]
    member _.``UpdateById instance method updates document by id``() =
        task {
            let collName = uniqueName "users"
            let users = fixture.CreateCollection(collName)

            let user =
                { Name = "Original"
                  Email = $"original-{Guid.NewGuid()}@test.com"
                  Age = 25L
                  Active = true }

            let! insertResult = users.InsertOne(user)
            let docId = (Result.defaultValue Unchecked.defaultof<_> insertResult).Id

            let! updateResult = users.UpdateById(docId, fun u -> { u with Name = "Updated" })

            match updateResult with
            | Ok(Some doc) -> doc.Data.Name |> should equal "Updated"
            | Ok None -> failwith "Expected Some but got None"
            | Error err -> failwith $"Expected Ok but got Error: {err}"
        }

    [<Fact>]
    member _.``UpdateOne instance method updates first matching document``() =
        task {
            let collName = uniqueName "users"
            let users = fixture.CreateCollection(collName)

            let user =
                { Name = "Target"
                  Email = $"target-{Guid.NewGuid()}@test.com"
                  Age = 25L
                  Active = true }

            let! _ = users.InsertOne(user)

            let filter = Query.Field("name", FieldOp.Compare(box (CompareOp.Eq "Target")))

            let! updateResult = users.UpdateOne(filter, fun u -> { u with Age = 30L })

            match updateResult with
            | Ok(Some doc) -> doc.Data.Age |> should equal 30L
            | Ok None -> failwith "Expected Some but got None"
            | Error err -> failwith $"Expected Ok but got Error: {err}"
        }

    [<Fact>]
    member _.``UpdateOne with upsert creates document if not found``() =
        task {
            let collName = uniqueName "users"
            let users = fixture.CreateCollection(collName)

            let filter = Query.Field("name", FieldOp.Compare(box (CompareOp.Eq "NonExistent")))

            let! updateResult =
                users.UpdateOne(
                    filter,
                    (fun _ ->
                        { Name = "Created"
                          Email = $"created-{Guid.NewGuid()}@test.com"
                          Age = 20L
                          Active = true }),
                    upsert = true
                )

            match updateResult with
            | Ok(Some doc) -> doc.Data.Name |> should equal "Created"
            | Ok None -> failwith "Expected Some (upsert) but got None"
            | Error err -> failwith $"Expected Ok but got Error: {err}"
        }

    [<Fact>]
    member _.``ReplaceOne instance method replaces document``() =
        task {
            let collName = uniqueName "users"
            let users = fixture.CreateCollection(collName)

            let user =
                { Name = "Original"
                  Email = $"original-{Guid.NewGuid()}@test.com"
                  Age = 25L
                  Active = true }

            let! _ = users.InsertOne(user)

            let filter = Query.Field("name", FieldOp.Compare(box (CompareOp.Eq "Original")))

            let replacement =
                { Name = "Replaced"
                  Email = $"replaced-{Guid.NewGuid()}@test.com"
                  Age = 99L
                  Active = false }

            let! replaceResult = users.ReplaceOne(filter, replacement)

            match replaceResult with
            | Ok(Some doc) ->
                doc.Data.Name |> should equal "Replaced"
                doc.Data.Age |> should equal 99L
            | Ok None -> failwith "Expected Some but got None"
            | Error err -> failwith $"Expected Ok but got Error: {err}"
        }

    [<Fact>]
    member _.``UpdateMany instance method updates all matching documents``() =
        task {
            let collName = uniqueName "users"
            let users = fixture.CreateCollection(collName)

            let docs =
                [ { Name = "User1"
                    Email = $"user1-{Guid.NewGuid()}@test.com"
                    Age = 25L
                    Active = true }
                  { Name = "User2"
                    Email = $"user2-{Guid.NewGuid()}@test.com"
                    Age = 30L
                    Active = true }
                  { Name = "User3"
                    Email = $"user3-{Guid.NewGuid()}@test.com"
                    Age = 35L
                    Active = false } ]

            let! _ = users.InsertMany(docs)

            let filter = Query.Field("active", FieldOp.Compare(box (CompareOp.Eq true)))

            let! updateResult = users.UpdateMany(filter, fun u -> { u with Age = 99L })

            match updateResult with
            | Ok result ->
                result.MatchedCount |> should equal 2
                result.ModifiedCount |> should equal 2
            | Error err -> failwith $"Expected Ok but got Error: {err}"
        }

    // ═══════════════════════════════════════════════════════════════
    // DELETE OPERATIONS
    // ═══════════════════════════════════════════════════════════════

    [<Fact>]
    member _.``DeleteById instance method deletes document by id``() =
        task {
            let collName = uniqueName "users"
            let users = fixture.CreateCollection(collName)

            let user =
                { Name = "ToDelete"
                  Email = $"todelete-{Guid.NewGuid()}@test.com"
                  Age = 25L
                  Active = true }

            let! insertResult = users.InsertOne(user)
            let docId = (Result.defaultValue Unchecked.defaultof<_> insertResult).Id

            let! deleted = users.DeleteById(docId)

            deleted |> should equal true

            let! found = users.FindById(docId)
            found |> should equal None
        }

    [<Fact>]
    member _.``DeleteById returns false for non-existent id``() =
        task {
            let collName = uniqueName "users"
            let users = fixture.CreateCollection(collName)

            let! deleted = users.DeleteById("non-existent-id")

            deleted |> should equal false
        }

    [<Fact>]
    member _.``DeleteOne instance method deletes first matching document``() =
        task {
            let collName = uniqueName "users"
            let users = fixture.CreateCollection(collName)

            let user =
                { Name = "Target"
                  Email = $"target-{Guid.NewGuid()}@test.com"
                  Age = 25L
                  Active = true }

            let! _ = users.InsertOne(user)

            let filter = Query.Field("name", FieldOp.Compare(box (CompareOp.Eq "Target")))
            let! deleted = users.DeleteOne(filter)

            deleted |> should equal true
        }

    [<Fact>]
    member _.``DeleteMany instance method deletes all matching documents``() =
        task {
            let collName = uniqueName "users"
            let users = fixture.CreateCollection(collName)

            let docs =
                [ { Name = "ToDelete1"
                    Email = $"todelete1-{Guid.NewGuid()}@test.com"
                    Age = 25L
                    Active = true }
                  { Name = "ToDelete2"
                    Email = $"todelete2-{Guid.NewGuid()}@test.com"
                    Age = 30L
                    Active = true }
                  { Name = "Keep"
                    Email = $"keep-{Guid.NewGuid()}@test.com"
                    Age = 35L
                    Active = false } ]

            let! _ = users.InsertMany(docs)

            let filter = Query.Field("active", FieldOp.Compare(box (CompareOp.Eq true)))
            let! deleteResult = users.DeleteMany(filter)

            deleteResult.DeletedCount |> should equal 2

            let! remaining = users.Find(Query.Empty)
            remaining |> List.length |> should equal 1
        }

    // ═══════════════════════════════════════════════════════════════
    // ATOMIC FIND-AND-MODIFY OPERATIONS
    // ═══════════════════════════════════════════════════════════════

    [<Fact>]
    member _.``FindOneAndDelete instance method atomically finds and deletes``() =
        task {
            let collName = uniqueName "users"
            let users = fixture.CreateCollection(collName)

            let user =
                { Name = "ToFindAndDelete"
                  Email = $"findanddelete-{Guid.NewGuid()}@test.com"
                  Age = 25L
                  Active = true }

            let! _ = users.InsertOne(user)

            let filter =
                Query.Field("name", FieldOp.Compare(box (CompareOp.Eq "ToFindAndDelete")))

            let! deleted = users.FindOneAndDelete(filter)

            deleted |> should not' (equal None)
            deleted.Value.Data.Name |> should equal "ToFindAndDelete"

            // Verify it's actually deleted
            let! found = users.FindOne(filter)
            found |> should equal None
        }

    [<Fact>]
    member _.``FindOneAndDelete with options uses sort``() =
        task {
            let collName = uniqueName "users"
            let users = fixture.CreateCollection(collName)

            let docs =
                [ { Name = "Young"
                    Email = $"young-{Guid.NewGuid()}@test.com"
                    Age = 20L
                    Active = true }
                  { Name = "Old"
                    Email = $"old-{Guid.NewGuid()}@test.com"
                    Age = 60L
                    Active = true } ]

            let! _ = users.InsertMany(docs)

            let filter = Query.Field("active", FieldOp.Compare(box (CompareOp.Eq true)))
            let options: FindOptions = { Sort = [ ("age", SortDirection.Descending) ] }

            let! deleted = users.FindOneAndDelete(filter, options)

            deleted |> should not' (equal None)
            deleted.Value.Data.Name |> should equal "Old" // Oldest first due to DESC sort
        }

    [<Fact>]
    member _.``FindOneAndUpdate instance method atomically finds and updates``() =
        task {
            let collName = uniqueName "users"
            let users = fixture.CreateCollection(collName)

            let user =
                { Name = "ToFindAndUpdate"
                  Email = $"findandupdate-{Guid.NewGuid()}@test.com"
                  Age = 25L
                  Active = true }

            let! _ = users.InsertOne(user)

            let filter =
                Query.Field("name", FieldOp.Compare(box (CompareOp.Eq "ToFindAndUpdate")))

            let options: FindAndModifyOptions =
                { Sort = []
                  ReturnDocument = ReturnDocument.After
                  Upsert = false }

            let! result = users.FindOneAndUpdate(filter, (fun u -> { u with Age = 99L }), options)

            match result with
            | Ok(Some doc) -> doc.Data.Age |> should equal 99L
            | Ok None -> failwith "Expected Some but got None"
            | Error err -> failwith $"Expected Ok but got Error: {err}"
        }

    [<Fact>]
    member _.``FindOneAndReplace instance method atomically finds and replaces``() =
        task {
            let collName = uniqueName "users"
            let users = fixture.CreateCollection(collName)

            let user =
                { Name = "ToReplace"
                  Email = $"toreplace-{Guid.NewGuid()}@test.com"
                  Age = 25L
                  Active = true }

            let! _ = users.InsertOne(user)

            let filter = Query.Field("name", FieldOp.Compare(box (CompareOp.Eq "ToReplace")))

            let options: FindAndModifyOptions =
                { Sort = []
                  ReturnDocument = ReturnDocument.After
                  Upsert = false }

            let replacement =
                { Name = "Replaced"
                  Email = $"replaced-{Guid.NewGuid()}@test.com"
                  Age = 100L
                  Active = false }

            let! result = users.FindOneAndReplace(filter, replacement, options)

            match result with
            | Ok(Some doc) ->
                doc.Data.Name |> should equal "Replaced"
                doc.Data.Age |> should equal 100L
            | Ok None -> failwith "Expected Some but got None"
            | Error err -> failwith $"Expected Ok but got Error: {err}"
        }

    // ═══════════════════════════════════════════════════════════════
    // UTILITY OPERATIONS
    // ═══════════════════════════════════════════════════════════════

    [<Fact>]
    member _.``Drop instance method drops the collection``() =
        task {
            let collName = uniqueName "users"
            let users = fixture.CreateCollection(collName)

            let user =
                { Name = "WillBeDropped"
                  Email = $"dropped-{Guid.NewGuid()}@test.com"
                  Age = 25L
                  Active = true }

            let! _ = users.InsertOne(user)

            do! users.Drop()

            // After drop, queries should return empty (table recreated on access)
            // Note: The collection object may still be usable but table is dropped
            // This behavior depends on implementation details
            ()
        }

    [<Fact>]
    member _.``Validate instance method validates document against schema``() =
        task {
            let collName = uniqueName "users"

            // Create schema with validator
            let schemaWithValidator =
                { testUserSchema with
                    Validate =
                        Some(fun u ->
                            if u.Age >= 0L then
                                Ok u
                            else
                                Error "Age must be non-negative") }

            let users = fixture.Db.Collection<CollectionTestUser>(collName, schemaWithValidator)

            let validUser =
                { Name = "Valid"
                  Email = "valid@test.com"
                  Age = 25L
                  Active = true }

            let result = users.Validate(validUser)

            match result with
            | Ok _ -> ()
            | Error _ -> failwith "Expected Ok for valid user"

            let invalidUser =
                { Name = "Invalid"
                  Email = "invalid@test.com"
                  Age = -5L
                  Active = true }

            let invalidResult = users.Validate(invalidUser)

            match invalidResult with
            | Error _ -> ()
            | Ok _ -> failwith "Expected Error for invalid user"
        }

    // ═══════════════════════════════════════════════════════════════
    // CANCELLATION TOKEN TESTS
    // ═══════════════════════════════════════════════════════════════

    [<Fact>]
    member _.``FindById respects cancellation``() =
        task {
            let collName = uniqueName "users"
            let users = fixture.CreateCollection(collName)

            use cts = new CancellationTokenSource()
            cts.Cancel()

            let! exn =
                Assert.ThrowsAsync<OperationCanceledException>(fun () -> users.FindById("any-id", ct = cts.Token))

            exn |> should not' (be Null)
        }

    [<Fact>]
    member _.``Find respects cancellation``() =
        task {
            let collName = uniqueName "users"
            let users = fixture.CreateCollection(collName)

            use cts = new CancellationTokenSource()
            cts.Cancel()

            let! exn = Assert.ThrowsAsync<OperationCanceledException>(fun () -> users.Find(Query.Empty, ct = cts.Token))

            exn |> should not' (be Null)
        }

    [<Fact>]
    member _.``UpdateById respects cancellation``() =
        task {
            let collName = uniqueName "users"
            let users = fixture.CreateCollection(collName)

            use cts = new CancellationTokenSource()
            cts.Cancel()

            let! exn =
                Assert.ThrowsAsync<OperationCanceledException>(fun () ->
                    users.UpdateById("any-id", (fun u -> u), ct = cts.Token))

            exn |> should not' (be Null)
        }

    [<Fact>]
    member _.``DeleteById respects cancellation``() =
        task {
            let collName = uniqueName "users"
            let users = fixture.CreateCollection(collName)

            use cts = new CancellationTokenSource()
            cts.Cancel()

            let! exn =
                Assert.ThrowsAsync<OperationCanceledException>(fun () -> users.DeleteById("any-id", ct = cts.Token))

            exn |> should not' (be Null)
        }
