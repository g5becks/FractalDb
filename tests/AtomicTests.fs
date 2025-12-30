module FractalDb.Tests.AtomicTests

open System.Threading.Tasks
open Xunit
open FsUnit.Xunit
open FractalDb.Database
open FractalDb.Schema
open FractalDb.Collection
open FractalDb.Operators
open FractalDb.Options
open FractalDb.Types

type TodoTask =
    { Name: string
      Status: string
      Priority: int }

let taskSchema: SchemaDef<TodoTask> =
    { Fields =
        [ { Name = "name"
            Path = None
            SqlType = SqliteType.Text
            Indexed = true
            Unique = false
            Nullable = false }
          { Name = "status"
            Path = None
            SqlType = SqliteType.Text
            Indexed = true
            Unique = false
            Nullable = false }
          { Name = "priority"
            Path = None
            SqlType = SqliteType.Integer
            Indexed = true
            Unique = false
            Nullable = false } ]
      Indexes = []
      Timestamps = true
      Validate = None }

type AtomicTestFixture() =
    let db = FractalDb.InMemory()
    member _.Db = db

    interface System.IDisposable with
        member _.Dispose() = db.Close()

type AtomicTests(fixture: AtomicTestFixture) =
    interface IClassFixture<AtomicTestFixture>

    [<Fact>]
    member _.``findOneAndDelete returns deleted document``() : Task =
        task {
            // Arrange
            let db = fixture.Db
            let tasks = db.Collection<TodoTask>("tasks_findAndDelete", taskSchema)

            let tasksToInsert =
                [ { Name = "Task1"
                    Status = "pending"
                    Priority = 1 }
                  { Name = "Task2"
                    Status = "pending"
                    Priority = 2 }
                  { Name = "Task3"
                    Status = "completed"
                    Priority = 3 } ]

            let! _ = tasks |> Collection.insertMany tasksToInsert

            // Act - Delete first pending task
            let query = Query.Field("status", FieldOp.Compare(box (CompareOp.Eq "pending")))
            let! deletedDoc = tasks |> Collection.findOneAndDelete query

            // Assert
            deletedDoc |> should not' (equal None)
            deletedDoc.Value.Data.Status |> should equal "pending"

            // Verify document was actually deleted
            let! remaining = tasks |> Collection.find Query.Empty
            remaining |> should haveLength 2

            // Verify the deleted document is not in remaining
            let foundDeleted = remaining |> List.tryFind (fun d -> d.Id = deletedDoc.Value.Id)
            foundDeleted |> should equal None
        }

    [<Fact>]
    member _.``findOneAndDelete returns None if not found``() : Task =
        task {
            // Arrange
            let db = fixture.Db
            let tasks = db.Collection<TodoTask>("tasks_deleteNotFound", taskSchema)

            let tasksToInsert =
                [ { Name = "Task1"
                    Status = "completed"
                    Priority = 1 }
                  { Name = "Task2"
                    Status = "completed"
                    Priority = 2 } ]

            let! _ = tasks |> Collection.insertMany tasksToInsert

            // Act - Try to delete a pending task (none exist)
            let query = Query.Field("status", FieldOp.Compare(box (CompareOp.Eq "pending")))
            let! deletedDoc = tasks |> Collection.findOneAndDelete query

            // Assert
            deletedDoc |> should equal None

            // Verify all documents remain
            let! remaining = tasks |> Collection.find Query.Empty
            remaining |> should haveLength 2
        }

    [<Fact>]
    member _.``findOneAndUpdate with ReturnDocument.Before returns original``() : Task =
        task {
            // Arrange
            let db = fixture.Db
            let tasks = db.Collection<TodoTask>("tasks_updateBefore", taskSchema)

            let initialTask =
                { Name = "UpdateTest"
                  Status = "pending"
                  Priority = 5 }

            let! insertResult = tasks |> Collection.insertOne initialTask

            let insertedDoc =
                match insertResult with
                | Ok doc -> doc
                | Error e -> failwith e.Message

            // Act - Update and return Before state
            let options =
                { Sort = []
                  ReturnDocument = ReturnDocument.Before
                  Upsert = false }

            let query = Query.Field("name", FieldOp.Compare(box (CompareOp.Eq "UpdateTest")))

            let! result =
                tasks
                |> Collection.findOneAndUpdate
                    query
                    (fun t ->
                        { t with
                            Status = "in-progress"
                            Priority = 10 })
                    options

            // Assert
            match result with
            | Ok(Some doc) ->
                // Should return the BEFORE state
                doc.Data.Status |> should equal "pending"
                doc.Data.Priority |> should equal 5

                // Verify document was actually updated in database
                let! updated = tasks |> Collection.findById insertedDoc.Id
                updated.Value.Data.Status |> should equal "in-progress"
                updated.Value.Data.Priority |> should equal 10

            | Ok None -> failwith "Expected Some document, got None"
            | Error err -> failwith $"Expected Ok, got Error: {err.Message}"
        }

    [<Fact>]
    member _.``findOneAndUpdate with ReturnDocument.After returns modified``() : Task =
        task {
            // Arrange
            let db = fixture.Db
            let tasks = db.Collection<TodoTask>("tasks_updateAfter", taskSchema)

            let initialTask =
                { Name = "UpdateTest2"
                  Status = "pending"
                  Priority = 5 }

            let! insertResult = tasks |> Collection.insertOne initialTask

            let insertedDoc =
                match insertResult with
                | Ok doc -> doc
                | Error e -> failwith e.Message

            // Act - Update and return After state
            let options =
                { Sort = []
                  ReturnDocument = ReturnDocument.After
                  Upsert = false }

            let query = Query.Field("name", FieldOp.Compare(box (CompareOp.Eq "UpdateTest2")))

            let! result =
                tasks
                |> Collection.findOneAndUpdate
                    query
                    (fun t ->
                        { t with
                            Status = "completed"
                            Priority = 20 })
                    options

            // Assert
            match result with
            | Ok(Some doc) ->
                // Should return the AFTER state
                doc.Data.Status |> should equal "completed"
                doc.Data.Priority |> should equal 20

                // Verify it's the same document
                doc.Id |> should equal insertedDoc.Id

            | Ok None -> failwith "Expected Some document, got None"
            | Error err -> failwith $"Expected Ok, got Error: {err.Message}"
        }

    [<Fact>]
    member _.``findOneAndUpdate with sort selects correct document``() : Task =
        task {
            // Arrange
            let db = fixture.Db
            let tasks = db.Collection<TodoTask>("tasks_updateSort", taskSchema)

            let tasksToInsert =
                [ { Name = "Task1"
                    Status = "pending"
                    Priority = 3 }
                  { Name = "Task2"
                    Status = "pending"
                    Priority = 1 } // Lowest priority
                  { Name = "Task3"
                    Status = "pending"
                    Priority = 5 } ]

            let! _ = tasks |> Collection.insertMany tasksToInsert

            // Act - Update the pending task with lowest priority
            let options =
                { Sort = [ ("priority", SortDirection.Ascending) ] // Sort by priority ascending
                  ReturnDocument = ReturnDocument.After
                  Upsert = false }

            let query = Query.Field("status", FieldOp.Compare(box (CompareOp.Eq "pending")))

            let! result =
                tasks
                |> Collection.findOneAndUpdate query (fun t -> { t with Status = "in-progress" }) options

            // Assert
            match result with
            | Ok(Some doc) ->
                // Should have updated Task2 (lowest priority)
                doc.Data.Name |> should equal "Task2"
                doc.Data.Priority |> should equal 1
                doc.Data.Status |> should equal "in-progress"

            | Ok None -> failwith "Expected Some document, got None"
            | Error err -> failwith $"Expected Ok, got Error: {err.Message}"
        }

    [<Fact>]
    member _.``findOneAndReplace replaces document body``() : Task =
        task {
            // Arrange
            let db = fixture.Db
            let tasks = db.Collection<TodoTask>("tasks_replace", taskSchema)

            let originalTask =
                { Name = "OriginalTask"
                  Status = "pending"
                  Priority = 5 }

            let! insertResult = tasks |> Collection.insertOne originalTask

            let insertedDoc =
                match insertResult with
                | Ok doc -> doc
                | Error e -> failwith e.Message

            // Act - Replace entire document
            let replacementTask =
                { Name = "ReplacedTask"
                  Status = "completed"
                  Priority = 10 }

            let options =
                { Sort = []
                  ReturnDocument = ReturnDocument.After
                  Upsert = false }

            let query = Query.Field("name", FieldOp.Compare(box (CompareOp.Eq "OriginalTask")))
            let! result = tasks |> Collection.findOneAndReplace query replacementTask options

            // Assert
            match result with
            | Ok(Some doc) ->
                // Should return the replaced document
                doc.Data.Name |> should equal "ReplacedTask"
                doc.Data.Status |> should equal "completed"
                doc.Data.Priority |> should equal 10

                // Verify same document ID (replace, not insert)
                doc.Id |> should equal insertedDoc.Id

                // Verify in database
                let! found = tasks |> Collection.findById insertedDoc.Id
                found.Value.Data.Name |> should equal "ReplacedTask"

            | Ok None -> failwith "Expected Some document, got None"
            | Error err -> failwith $"Expected Ok, got Error: {err.Message}"
        }

    [<Fact>]
    member _.``findOneAndReplace with Before returns original``() : Task =
        task {
            // Arrange
            let db = fixture.Db
            let tasks = db.Collection<TodoTask>("tasks_replaceBefore", taskSchema)

            let originalTask =
                { Name = "BeforeTest"
                  Status = "pending"
                  Priority = 3 }

            let! insertResult = tasks |> Collection.insertOne originalTask

            let insertedDoc =
                match insertResult with
                | Ok doc -> doc
                | Error e -> failwith e.Message

            // Act - Replace and return Before state
            let replacementTask =
                { Name = "AfterTest"
                  Status = "completed"
                  Priority = 7 }

            let options =
                { Sort = []
                  ReturnDocument = ReturnDocument.Before
                  Upsert = false }

            let query = Query.Field("name", FieldOp.Compare(box (CompareOp.Eq "BeforeTest")))
            let! result = tasks |> Collection.findOneAndReplace query replacementTask options

            // Assert
            match result with
            | Ok(Some doc) ->
                // Should return the BEFORE state (original)
                doc.Data.Name |> should equal "BeforeTest"
                doc.Data.Status |> should equal "pending"
                doc.Data.Priority |> should equal 3

                // But document should be replaced in database
                let! found = tasks |> Collection.findById insertedDoc.Id
                found.Value.Data.Name |> should equal "AfterTest"
                found.Value.Data.Status |> should equal "completed"
                found.Value.Data.Priority |> should equal 7

            | Ok None -> failwith "Expected Some document, got None"
            | Error err -> failwith $"Expected Ok, got Error: {err.Message}"
        }

    [<Fact>]
    member _.``atomic operations are truly atomic``() : Task =
        task {
            // Arrange
            let db = fixture.Db
            let tasks = db.Collection<TodoTask>("tasks_atomic", taskSchema)

            let initialTasks =
                [ { Name = "Task1"
                    Status = "pending"
                    Priority = 1 }
                  { Name = "Task2"
                    Status = "pending"
                    Priority = 2 } ]

            let! _ = tasks |> Collection.insertMany initialTasks

            // Act - Perform multiple atomic operations
            let query = Query.Field("status", FieldOp.Compare(box (CompareOp.Eq "pending")))

            // Delete one atomically
            let! deleted = tasks |> Collection.findOneAndDelete query

            // Verify count decreased immediately
            let! afterDelete = tasks |> Collection.find Query.Empty
            afterDelete |> should haveLength 1

            // Update the remaining one atomically
            let options =
                { Sort = []
                  ReturnDocument = ReturnDocument.After
                  Upsert = false }

            let! updated =
                tasks
                |> Collection.findOneAndUpdate query (fun t -> { t with Status = "completed" }) options

            // Assert
            deleted |> should not' (equal None)

            match updated with
            | Ok(Some doc) ->
                doc.Data.Status |> should equal "completed"

                // Verify no pending tasks remain
                let pendingQuery =
                    Query.Field("status", FieldOp.Compare(box (CompareOp.Eq "pending")))

                let! pendingTasks = tasks |> Collection.find pendingQuery
                pendingTasks |> should haveLength 0

            | Ok None -> failwith "Expected Some document, got None"
            | Error err -> failwith $"Expected Ok, got Error: {err.Message}"
        }
