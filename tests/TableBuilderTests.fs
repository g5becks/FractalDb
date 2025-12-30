module FractalDb.Tests.TableBuilderTests

/// <summary>
/// Tests for TableBuilder module DDL generation.
/// Verifies correct SQL generation for tables, indexes, and constraints.
/// </summary>

open System
open System.Data
open Xunit
open FsUnit.Xunit
open Microsoft.Data.Sqlite
open FractalDb.Schema
open FractalDb.TableBuilder
open FractalDb.Database

// =============================================================================
// SqliteType Mapping Tests
// =============================================================================

[<Fact>]
let ``mapSqliteType returns TEXT for Text type``() =
    mapSqliteType SqliteType.Text |> should equal "TEXT"

[<Fact>]
let ``mapSqliteType returns INTEGER for Integer type``() =
    mapSqliteType SqliteType.Integer |> should equal "INTEGER"

[<Fact>]
let ``mapSqliteType returns REAL for Real type``() =
    mapSqliteType SqliteType.Real |> should equal "REAL"

[<Fact>]
let ``mapSqliteType returns BLOB for Blob type``() =
    mapSqliteType SqliteType.Blob |> should equal "BLOB"

[<Fact>]
let ``mapSqliteType returns NUMERIC for Numeric type``() =
    mapSqliteType SqliteType.Numeric |> should equal "NUMERIC"

[<Fact>]
let ``mapSqliteType returns INTEGER for Boolean type``() =
    // SQLite stores booleans as INTEGER (0/1)
    mapSqliteType SqliteType.Boolean |> should equal "INTEGER"

// =============================================================================
// CREATE TABLE SQL Generation Tests
// =============================================================================

[<Fact>]
let ``createTableSql generates basic table with no indexed fields``() =
    let schema: SchemaDef<obj> =
        { Fields = []
          Indexes = []
          Timestamps = true
          Validate = None }
    
    let sql = createTableSql "users" schema
    
    // Should contain base columns
    sql |> should haveSubstring "_id TEXT PRIMARY KEY"
    sql |> should haveSubstring "body BLOB NOT NULL"
    sql |> should haveSubstring "createdAt INTEGER NOT NULL"
    sql |> should haveSubstring "updatedAt INTEGER NOT NULL"
    sql |> should haveSubstring "CREATE TABLE IF NOT EXISTS users"

[<Fact>]
let ``createTableSql generates table with single indexed TEXT field``() =
    let schema: SchemaDef<obj> =
        { Fields =
            [ { Name = "email"
                Path = None
                SqlType = SqliteType.Text
                Indexed = true
                Unique = false
                Nullable = false } ]
          Indexes = []
          Timestamps = true
          Validate = None }
    
    let sql = createTableSql "users" schema
    
    // Should contain generated column for indexed field
    sql |> should haveSubstring "_email TEXT GENERATED ALWAYS AS (jsonb_extract(body, '$.email')) VIRTUAL"

[<Fact>]
let ``createTableSql generates table with INTEGER field``() =
    let schema: SchemaDef<obj> =
        { Fields =
            [ { Name = "age"
                Path = None
                SqlType = SqliteType.Integer
                Indexed = true
                Unique = false
                Nullable = false } ]
          Indexes = []
          Timestamps = true
          Validate = None }
    
    let sql = createTableSql "users" schema
    
    sql |> should haveSubstring "_age INTEGER GENERATED ALWAYS AS (jsonb_extract(body, '$.age')) VIRTUAL"

[<Fact>]
let ``createTableSql generates table with REAL field``() =
    let schema: SchemaDef<obj> =
        { Fields =
            [ { Name = "score"
                Path = None
                SqlType = SqliteType.Real
                Indexed = true
                Unique = false
                Nullable = false } ]
          Indexes = []
          Timestamps = true
          Validate = None }
    
    let sql = createTableSql "scores" schema
    
    sql |> should haveSubstring "_score REAL GENERATED ALWAYS AS (jsonb_extract(body, '$.score')) VIRTUAL"

[<Fact>]
let ``createTableSql generates table with multiple indexed fields``() =
    let schema: SchemaDef<obj> =
        { Fields =
            [ { Name = "email"
                Path = None
                SqlType = SqliteType.Text
                Indexed = true
                Unique = false
                Nullable = false }
              { Name = "age"
                Path = None
                SqlType = SqliteType.Integer
                Indexed = true
                Unique = false
                Nullable = false }
              { Name = "score"
                Path = None
                SqlType = SqliteType.Real
                Indexed = true
                Unique = false
                Nullable = false } ]
          Indexes = []
          Timestamps = true
          Validate = None }
    
    let sql = createTableSql "users" schema
    
    // All indexed fields should have generated columns
    sql |> should haveSubstring "_email TEXT"
    sql |> should haveSubstring "_age INTEGER"
    sql |> should haveSubstring "_score REAL"

[<Fact>]
let ``createTableSql respects custom JSON path``() =
    let schema: SchemaDef<obj> =
        { Fields =
            [ { Name = "username"
                Path = Some "$.user.name"  // Nested path
                SqlType = SqliteType.Text
                Indexed = true
                Unique = false
                Nullable = false } ]
          Indexes = []
          Timestamps = true
          Validate = None }
    
    let sql = createTableSql "users" schema
    
    sql |> should haveSubstring "jsonb_extract(body, '$.user.name')"

[<Fact>]
let ``createTableSql only generates columns for indexed fields``() =
    let schema: SchemaDef<obj> =
        { Fields =
            [ { Name = "email"
                Path = None
                SqlType = SqliteType.Text
                Indexed = true   // Indexed - should have column
                Unique = false
                Nullable = false }
              { Name = "bio"
                Path = None
                SqlType = SqliteType.Text
                Indexed = false  // Not indexed - should NOT have column
                Unique = false
                Nullable = false } ]
          Indexes = []
          Timestamps = true
          Validate = None }
    
    let sql = createTableSql "users" schema
    
    sql |> should haveSubstring "_email"
    sql |> should not' (haveSubstring "_bio")

// =============================================================================
// CREATE INDEX SQL Generation Tests
// =============================================================================

[<Fact>]
let ``createIndexesSql generates no indexes for empty schema``() =
    let schema: SchemaDef<obj> =
        { Fields = []
          Indexes = []
          Timestamps = true
          Validate = None }
    
    let indexes = createIndexesSql "users" schema
    
    indexes |> should be Empty

[<Fact>]
let ``createIndexesSql generates single field index``() =
    let schema: SchemaDef<obj> =
        { Fields =
            [ { Name = "email"
                Path = None
                SqlType = SqliteType.Text
                Indexed = true
                Unique = false
                Nullable = false } ]
          Indexes = []
          Timestamps = true
          Validate = None }
    
    let indexes = createIndexesSql "users" schema
    
    indexes |> should haveLength 1
    indexes.[0] |> should equal "CREATE INDEX IF NOT EXISTS idx_users_email ON users(_email)"

[<Fact>]
let ``createIndexesSql generates UNIQUE index for unique field``() =
    let schema: SchemaDef<obj> =
        { Fields =
            [ { Name = "email"
                Path = None
                SqlType = SqliteType.Text
                Indexed = true
                Unique = true  // Unique field
                Nullable = false } ]
          Indexes = []
          Timestamps = true
          Validate = None }
    
    let indexes = createIndexesSql "users" schema
    
    indexes |> should haveLength 1
    indexes.[0] |> should equal "CREATE UNIQUE INDEX IF NOT EXISTS idx_users_email ON users(_email)"

[<Fact>]
let ``createIndexesSql generates multiple field indexes``() =
    let schema: SchemaDef<obj> =
        { Fields =
            [ { Name = "email"
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
                Nullable = false } ]
          Indexes = []
          Timestamps = true
          Validate = None }
    
    let indexes = createIndexesSql "users" schema
    
    indexes |> should haveLength 2
    indexes |> should contain "CREATE UNIQUE INDEX IF NOT EXISTS idx_users_email ON users(_email)"
    indexes |> should contain "CREATE INDEX IF NOT EXISTS idx_users_age ON users(_age)"

[<Fact>]
let ``createIndexesSql generates composite index``() =
    let schema: SchemaDef<obj> =
        { Fields =
            [ { Name = "status"
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
          Indexes =
            [ { Name = "idx_users_status_priority"
                Fields = ["status"; "priority"]
                Unique = false } ]
          Timestamps = true
          Validate = None }
    
    let indexes = createIndexesSql "users" schema
    
    // Should have 2 single-field indexes + 1 composite index
    indexes |> should haveLength 3
    indexes |> should contain "CREATE INDEX IF NOT EXISTS idx_users_status_priority ON users(_status, _priority)"

[<Fact>]
let ``createIndexesSql generates UNIQUE composite index``() =
    let schema: SchemaDef<obj> =
        { Fields =
            [ { Name = "tenantId"
                Path = None
                SqlType = SqliteType.Text
                Indexed = true
                Unique = false
                Nullable = false }
              { Name = "username"
                Path = None
                SqlType = SqliteType.Text
                Indexed = true
                Unique = false
                Nullable = false } ]
          Indexes =
            [ { Name = "idx_users_tenant_username"
                Fields = ["tenantId"; "username"]
                Unique = true } ]  // Unique composite
          Timestamps = true
          Validate = None }
    
    let indexes = createIndexesSql "users" schema
    
    indexes |> should contain "CREATE UNIQUE INDEX IF NOT EXISTS idx_users_tenant_username ON users(_tenantId, _username)"

[<Fact>]
let ``createIndexesSql does not generate indexes for non-indexed fields``() =
    let schema: SchemaDef<obj> =
        { Fields =
            [ { Name = "email"
                Path = None
                SqlType = SqliteType.Text
                Indexed = true
                Unique = false
                Nullable = false }
              { Name = "bio"
                Path = None
                SqlType = SqliteType.Text
                Indexed = false  // Not indexed
                Unique = false
                Nullable = false } ]
          Indexes = []
          Timestamps = true
          Validate = None }
    
    let indexes = createIndexesSql "users" schema
    
    indexes |> should haveLength 1  // Only email
    indexes.[0] |> should not' (haveSubstring "bio")

// =============================================================================
// ensureTable Integration Tests
// =============================================================================

[<Fact>]
let ``ensureTable creates table in database``() =
    use conn = new SqliteConnection("Data Source=:memory:")
    conn.Open()
    
    let schema: SchemaDef<obj> =
        { Fields =
            [ { Name = "email"
                Path = None
                SqlType = SqliteType.Text
                Indexed = true
                Unique = true
                Nullable = false } ]
          Indexes = []
          Timestamps = true
          Validate = None }
    
    // Create table
    ensureTable conn "users" schema
    
    // Verify table exists by querying sqlite_master
    use cmd = conn.CreateCommand()
    cmd.CommandText <- "SELECT name FROM sqlite_master WHERE type='table' AND name='users'"
    let result = cmd.ExecuteScalar()
    
    result |> should not' (be Null)
    result :?> string |> should equal "users"

[<Fact>]
let ``ensureTable creates indexes in database``() =
    use conn = new SqliteConnection("Data Source=:memory:")
    conn.Open()
    
    let schema: SchemaDef<obj> =
        { Fields =
            [ { Name = "email"
                Path = None
                SqlType = SqliteType.Text
                Indexed = true
                Unique = true
                Nullable = false } ]
          Indexes = []
          Timestamps = true
          Validate = None }
    
    // Create table and indexes
    ensureTable conn "users" schema
    
    // Verify index exists
    use cmd = conn.CreateCommand()
    cmd.CommandText <- "SELECT name FROM sqlite_master WHERE type='index' AND name='idx_users_email'"
    let result = cmd.ExecuteScalar()
    
    result |> should not' (be Null)
    result :?> string |> should equal "idx_users_email"

[<Fact>]
let ``ensureTable is idempotent - can be called multiple times``() =
    use conn = new SqliteConnection("Data Source=:memory:")
    conn.Open()
    
    let schema: SchemaDef<obj> =
        { Fields =
            [ { Name = "email"
                Path = None
                SqlType = SqliteType.Text
                Indexed = true
                Unique = false
                Nullable = false } ]
          Indexes = []
          Timestamps = true
          Validate = None }
    
    // Call ensureTable multiple times - should not throw
    ensureTable conn "users" schema
    ensureTable conn "users" schema
    ensureTable conn "users" schema
    
    // Verify table still exists and is usable
    use cmd = conn.CreateCommand()
    cmd.CommandText <- "SELECT COUNT(*) FROM users"
    let count = cmd.ExecuteScalar() :?> int64
    
    count |> should equal 0L

[<Fact>]
let ``ensureTable creates all base columns``() =
    use conn = new SqliteConnection("Data Source=:memory:")
    conn.Open()
    
    let schema: SchemaDef<obj> =
        { Fields = []
          Indexes = []
          Timestamps = true
          Validate = None }
    
    ensureTable conn "users" schema
    
    // Verify all base columns exist by attempting insert
    use cmd = conn.CreateCommand()
    cmd.CommandText <- "INSERT INTO users (_id, body, createdAt, updatedAt) VALUES ('test-id', '{}', 1234567890, 1234567890)"
    cmd.ExecuteNonQuery() |> ignore
    
    // Verify inserted
    cmd.CommandText <- "SELECT COUNT(*) FROM users"
    let count = cmd.ExecuteScalar() :?> int64
    
    count |> should equal 1L

[<Fact>]
let ``ensureTable creates generated columns that can be queried``() =
    use conn = new SqliteConnection("Data Source=:memory:")
    conn.Open()
    
    let schema: SchemaDef<obj> =
        { Fields =
            [ { Name = "name"
                Path = None
                SqlType = SqliteType.Text
                Indexed = true
                Unique = false
                Nullable = false } ]
          Indexes = []
          Timestamps = true
          Validate = None }
    
    ensureTable conn "users" schema
    
    // Insert data with JSON body
    use cmd = conn.CreateCommand()
    cmd.CommandText <- "INSERT INTO users (_id, body, createdAt, updatedAt) VALUES ('id1', '{\"name\": \"Alice\"}', 1234567890, 1234567890)"
    cmd.ExecuteNonQuery() |> ignore
    
    // Query using generated column
    cmd.CommandText <- "SELECT _name FROM users WHERE _id = 'id1'"
    let name = cmd.ExecuteScalar() :?> string
    
    name |> should equal "Alice"

[<Fact>]
let ``ensureTable handles FractalDb collection workflow``() =
    // Full integration test simulating how Collection uses TableBuilder
    let db = FractalDb.InMemory()
    
    let schema: SchemaDef<{| Name: string; Age: int |}> =
        { Fields =
            [ { Name = "name"
                Path = None
                SqlType = SqliteType.Text
                Indexed = true
                Unique = false
                Nullable = false }
              { Name = "age"
                Path = None
                SqlType = SqliteType.Integer
                Indexed = true
                Unique = false
                Nullable = false } ]
          Indexes = []
          Timestamps = true
          Validate = None }
    
    // This internally calls ensureTable
    let coll = db.Collection("people", schema)
    
    // Verify we can use the collection (table was created successfully)
    let insertTask = coll |> FractalDb.Collection.Collection.insertOne {| Name = "Bob"; Age = 30 |}
    let result = insertTask.Result
    
    match result with
    | Ok doc ->
        doc.Data.Name |> should equal "Bob"
        doc.Data.Age |> should equal 30
    | Error err -> failwith $"Insert failed: {err.Message}"
    
    db.Close()
