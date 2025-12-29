module FractalDb.Tests.IndexTests

open System
open System.Data
open System.Threading.Tasks
open Xunit
open FsUnit.Xunit
open Microsoft.Data.Sqlite
open FractalDb
open FractalDb.Types
open FractalDb.Schema
open FractalDb.Database
open FractalDb.Errors
open FractalDb.Collection

/// <summary>
/// Unit tests for index creation and management.
/// Tests that indexes are created correctly, enforce uniqueness, and are idempotent.
/// </summary>

type TestUser = {
    Name: string
    Email: string
    Age: int
    Status: string
}

// Helper to create in-memory database with collection
let createTestDb() =
    let db = FractalDb.InMemory()
    db

// Helper to check if an index exists in SQLite
let indexExists (db: FractalDb) (indexName: string) : bool =
    let sql = "SELECT COUNT(*) FROM sqlite_master WHERE type='index' AND name=@name"
    let result =
        db.Connection
        |> Donald.Db.newCommand sql
        |> Donald.Db.setParams [ "name", Donald.SqlType.String indexName ]
        |> Donald.Db.scalar Convert.ToInt32
    result > 0

// Helper to get index definition
let getIndexSql (db: FractalDb) (indexName: string) : option<string> =
    let sql = "SELECT sql FROM sqlite_master WHERE type='index' AND name=@name"
    try
        let result =
            db.Connection
            |> Donald.Db.newCommand sql
            |> Donald.Db.setParams [ "name", Donald.SqlType.String indexName ]
            |> Donald.Db.querySingle (fun (rd: IDataReader) -> rd.GetString(0))
        result
    with
    | _ -> None

// ═══════════════════════════════════════════════════════════════
// Single-Field Index Tests
// ═══════════════════════════════════════════════════════════════

[<Fact>]
let ``Single-field index is created from FieldDef.Indexed`` () =
    task {
        let db = createTestDb()
        
        let schema = {
            Fields = [
                { Name = "email"; Path = None; SqlType = SqliteType.Text
                  Indexed = true; Unique = false; Nullable = false }
            ]
            Indexes = []
            Timestamps = false
            Validate = None
        }
        
        let collection = db.Collection<TestUser>("users", schema)
        
        // Check that index was created
        let exists = indexExists db "idx_users_email"
        exists |> should be True
    }

[<Fact>]
let ``Single-field unique index is created from FieldDef.Unique`` () =
    task {
        let db = createTestDb()
        
        let schema = {
            Fields = [
                { Name = "email"; Path = None; SqlType = SqliteType.Text
                  Indexed = true; Unique = true; Nullable = false }
            ]
            Indexes = []
            Timestamps = false
            Validate = None
        }
        
        let collection = db.Collection<TestUser>("users", schema)
        
        // Check that unique index was created
        let exists = indexExists db "idx_users_email"
        exists |> should be True
        
        // Verify it's a UNIQUE index by checking SQL
        let sqlOpt = getIndexSql db "idx_users_email"
        match sqlOpt with
        | Some sql -> 
            sql |> should haveSubstring "UNIQUE"
        | None ->
            failwith "Index SQL not found"
    }

[<Fact>]
let ``Unique index enforces uniqueness constraint`` () =
    task {
        let db = createTestDb()
        
        let schema = {
            Fields = [
                { Name = "email"; Path = None; SqlType = SqliteType.Text
                  Indexed = true; Unique = true; Nullable = false }
            ]
            Indexes = []
            Timestamps = false
            Validate = None
        }
        
        let collection = db.Collection<TestUser>("users", schema)
        
        // Insert first user
        let user1 = { Name = "Alice"; Email = "alice@example.com"; Age = 30; Status = "active" }
        let! result1 = collection |> Collection.insertOne user1
        
        match result1 with
        | Ok _ -> ()
        | Error err -> failwith $"First insert should succeed, got: {err.Message}"
        
        // Try to insert second user with same email (should fail)
        let user2 = { Name = "Bob"; Email = "alice@example.com"; Age = 25; Status = "active" }
        let! result2 = collection |> Collection.insertOne user2
        
        match result2 with
        | Ok _ -> failwith "Expected unique constraint violation"
        | Error err ->
            match err with
            | FractalError.UniqueConstraint (field, _) ->
                field |> should equal "email"
            | _ -> failwith $"Expected UniqueConstraint error, got: {err}"
    }

[<Fact>]
let ``Multiple single-field indexes are created`` () =
    task {
        let db = createTestDb()
        
        let schema = {
            Fields = [
                { Name = "email"; Path = None; SqlType = SqliteType.Text
                  Indexed = true; Unique = true; Nullable = false }
                { Name = "age"; Path = None; SqlType = SqliteType.Integer
                  Indexed = true; Unique = false; Nullable = false }
                { Name = "status"; Path = None; SqlType = SqliteType.Text
                  Indexed = true; Unique = false; Nullable = false }
            ]
            Indexes = []
            Timestamps = false
            Validate = None
        }
        
        let collection = db.Collection<TestUser>("users", schema)
        
        // Check all indexes were created
        let emailExists = indexExists db "idx_users_email"
        let ageExists = indexExists db "idx_users_age"
        let statusExists = indexExists db "idx_users_status"
        
        emailExists |> should be True
        ageExists |> should be True
        statusExists |> should be True
    }

// ═══════════════════════════════════════════════════════════════
// Compound Index Tests
// ═══════════════════════════════════════════════════════════════

[<Fact>]
let ``Compound index is created from SchemaDef.Indexes`` () =
    task {
        let db = createTestDb()
        
        let schema = {
            Fields = [
                { Name = "email"; Path = None; SqlType = SqliteType.Text
                  Indexed = true; Unique = false; Nullable = false }
                { Name = "status"; Path = None; SqlType = SqliteType.Text
                  Indexed = true; Unique = false; Nullable = false }
            ]
            Indexes = [
                { Name = "idx_email_status"; Fields = ["email"; "status"]; Unique = false }
            ]
            Timestamps = false
            Validate = None
        }
        
        let collection = db.Collection<TestUser>("users", schema)
        
        // Check that compound index was created
        let exists = indexExists db "idx_email_status"
        exists |> should be True
        
        // Verify it includes both fields
        let sqlOpt = getIndexSql db "idx_email_status"
        match sqlOpt with
        | Some sql ->
            sql |> should haveSubstring "_email"
            sql |> should haveSubstring "_status"
        | None ->
            failwith "Index SQL not found"
    }

[<Fact>]
let ``Unique compound index enforces uniqueness on combination`` () =
    task {
        let db = createTestDb()
        
        let schema = {
            Fields = [
                { Name = "email"; Path = None; SqlType = SqliteType.Text
                  Indexed = true; Unique = false; Nullable = false }
                { Name = "status"; Path = None; SqlType = SqliteType.Text
                  Indexed = true; Unique = false; Nullable = false }
            ]
            Indexes = [
                { Name = "idx_email_status"; Fields = ["email"; "status"]; Unique = true }
            ]
            Timestamps = false
            Validate = None
        }
        
        let collection = db.Collection<TestUser>("users", schema)
        
        // Insert first user
        let user1 = { Name = "Alice"; Email = "alice@example.com"; Age = 30; Status = "active" }
        let! result1 = collection |> Collection.insertOne user1
        
        match result1 with
        | Ok _ -> ()
        | Error err -> failwith $"First insert should succeed, got: {err.Message}"
        
        // Insert second user with same email but different status (should succeed)
        let user2 = { Name = "Alice2"; Email = "alice@example.com"; Age = 31; Status = "inactive" }
        let! result2 = collection |> Collection.insertOne user2
        
        match result2 with
        | Ok _ -> ()
        | Error err -> failwith $"Second insert should succeed with different status, got: {err.Message}"
        
        // Try to insert third user with same email AND status (should fail)
        let user3 = { Name = "Alice3"; Email = "alice@example.com"; Age = 32; Status = "active" }
        let! result3 = collection |> Collection.insertOne user3
        
        match result3 with
        | Ok _ -> failwith "Expected unique constraint violation"
        | Error err ->
            match err with
            | FractalError.UniqueConstraint _ -> ()  // Success - got expected error
            | _ -> failwith $"Expected UniqueConstraint error, got: {err}"
    }

[<Fact>]
let ``Compound index with three fields is created`` () =
    task {
        let db = createTestDb()
        
        let schema = {
            Fields = [
                { Name = "name"; Path = None; SqlType = SqliteType.Text
                  Indexed = true; Unique = false; Nullable = false }
                { Name = "email"; Path = None; SqlType = SqliteType.Text
                  Indexed = true; Unique = false; Nullable = false }
                { Name = "status"; Path = None; SqlType = SqliteType.Text
                  Indexed = true; Unique = false; Nullable = false }
            ]
            Indexes = [
                { Name = "idx_name_email_status"; Fields = ["name"; "email"; "status"]; Unique = false }
            ]
            Timestamps = false
            Validate = None
        }
        
        let collection = db.Collection<TestUser>("users", schema)
        
        // Check that compound index was created
        let exists = indexExists db "idx_name_email_status"
        exists |> should be True
        
        // Verify it includes all three fields
        let sqlOpt = getIndexSql db "idx_name_email_status"
        match sqlOpt with
        | Some sql ->
            sql |> should haveSubstring "_name"
            sql |> should haveSubstring "_email"
            sql |> should haveSubstring "_status"
        | None ->
            failwith "Index SQL not found"
    }

// ═══════════════════════════════════════════════════════════════
// Schema-Defined Indexes on Collection Init
// ═══════════════════════════════════════════════════════════════

[<Fact>]
let ``All schema-defined indexes are created on collection initialization`` () =
    task {
        let db = createTestDb()
        
        let schema = {
            Fields = [
                { Name = "name"; Path = None; SqlType = SqliteType.Text
                  Indexed = true; Unique = false; Nullable = false }
                { Name = "email"; Path = None; SqlType = SqliteType.Text
                  Indexed = true; Unique = true; Nullable = false }
                { Name = "age"; Path = None; SqlType = SqliteType.Integer
                  Indexed = true; Unique = false; Nullable = false }
                { Name = "status"; Path = None; SqlType = SqliteType.Text
                  Indexed = true; Unique = false; Nullable = false }
            ]
            Indexes = [
                { Name = "idx_email_status"; Fields = ["email"; "status"]; Unique = false }
                { Name = "idx_name_age"; Fields = ["name"; "age"]; Unique = false }
            ]
            Timestamps = false
            Validate = None
        }
        
        let collection = db.Collection<TestUser>("users", schema)
        
        // Check all single-field indexes
        let nameExists = indexExists db "idx_users_name"
        let emailExists = indexExists db "idx_users_email"
        let ageExists = indexExists db "idx_users_age"
        let statusExists = indexExists db "idx_users_status"
        
        // Check all compound indexes
        let emailStatusExists = indexExists db "idx_email_status"
        let nameAgeExists = indexExists db "idx_name_age"
        
        nameExists |> should be True
        emailExists |> should be True
        ageExists |> should be True
        statusExists |> should be True
        emailStatusExists |> should be True
        nameAgeExists |> should be True
    }

// ═══════════════════════════════════════════════════════════════
// Idempotency Tests
// ═══════════════════════════════════════════════════════════════

[<Fact>]
let ``Duplicate index creation is idempotent`` () =
    task {
        let db = createTestDb()
        
        let schema = {
            Fields = [
                { Name = "email"; Path = None; SqlType = SqliteType.Text
                  Indexed = true; Unique = true; Nullable = false }
            ]
            Indexes = []
            Timestamps = false
            Validate = None
        }
        
        // Create collection first time
        let collection1 = db.Collection<TestUser>("users", schema)
        let exists1 = indexExists db "idx_users_email"
        exists1 |> should be True
        
        // Create collection again with same schema (should not error)
        let collection2 = db.Collection<TestUser>("users", schema)
        let exists2 = indexExists db "idx_users_email"
        exists2 |> should be True
        
        // Both collections should work fine
        let user = { Name = "Alice"; Email = "alice@example.com"; Age = 30; Status = "active" }
        let! result = collection2 |> Collection.insertOne user
        
        match result with
        | Ok _ -> ()
        | Error err -> failwith $"Insert should succeed, got: {err.Message}"
    }

[<Fact>]
let ``Creating same index multiple times does not error`` () =
    task {
        let db = createTestDb()
        
        let schema = {
            Fields = [
                { Name = "email"; Path = None; SqlType = SqliteType.Text
                  Indexed = true; Unique = false; Nullable = false }
            ]
            Indexes = [
                { Name = "idx_custom"; Fields = ["email"]; Unique = false }
            ]
            Timestamps = false
            Validate = None
        }
        
        // Create collection multiple times
        let collection1 = db.Collection<TestUser>("users", schema)
        let collection2 = db.Collection<TestUser>("users", schema)
        let collection3 = db.Collection<TestUser>("users", schema)
        
        // All indexes should exist
        let fieldIndexExists = indexExists db "idx_users_email"
        let compoundIndexExists = indexExists db "idx_custom"
        
        fieldIndexExists |> should be True
        compoundIndexExists |> should be True
        
        // Collection should still work
        let user = { Name = "Bob"; Email = "bob@example.com"; Age = 25; Status = "active" }
        let! result = collection3 |> Collection.insertOne user
        
        match result with
        | Ok _ -> ()
        | Error err -> failwith $"Insert should succeed, got: {err.Message}"
    }

// ═══════════════════════════════════════════════════════════════
// Index Order and Field Coverage Tests
// ═══════════════════════════════════════════════════════════════

[<Fact>]
let ``Compound index maintains field order`` () =
    task {
        let db = createTestDb()
        
        let schema = {
            Fields = [
                { Name = "email"; Path = None; SqlType = SqliteType.Text
                  Indexed = true; Unique = false; Nullable = false }
                { Name = "status"; Path = None; SqlType = SqliteType.Text
                  Indexed = true; Unique = false; Nullable = false }
            ]
            Indexes = [
                { Name = "idx_email_status"; Fields = ["email"; "status"]; Unique = false }
            ]
            Timestamps = false
            Validate = None
        }
        
        let collection = db.Collection<TestUser>("users", schema)
        
        // Get index SQL and verify field order
        let sqlOpt = getIndexSql db "idx_email_status"
        match sqlOpt with
        | Some sql ->
            // Email should appear before status in the SQL
            let emailPos = sql.IndexOf("_email")
            let statusPos = sql.IndexOf("_status")
            (emailPos < statusPos) |> should be True
        | None ->
            failwith "Index SQL not found"
    }
