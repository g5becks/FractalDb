/// <summary>
/// Internal module for generating SQLite DDL statements (CREATE TABLE, CREATE INDEX).
/// Used by Collection to ensure tables and indexes exist with proper schema.
/// </summary>
/// <remarks>
/// This module handles:
/// - Mapping FractalDb SqliteType to SQLite SQL type strings
/// - Generating CREATE TABLE statements with generated columns for indexed fields
/// - Generating CREATE INDEX statements for single and composite indexes
/// - Ensuring tables exist with proper schema before operations
///
/// Key design decisions:
/// - Uses jsonb_extract for indexed fields with GENERATED columns
/// - Supports VIRTUAL generated columns (computed on read)
/// - Handles unique constraints on both fields and indexes
/// - Internal visibility - not exposed in public API
/// </remarks>
module FractalDb.TableBuilder

open System.Data
open Donald
open FractalDb.Schema

/// <summary>
/// Maps a FractalDb SqliteType discriminated union case to its corresponding
/// SQLite SQL type string for use in CREATE TABLE statements.
/// </summary>
/// <param name="sqlType">The SqliteType discriminated union case to map.</param>
/// <returns>
/// The SQLite SQL type string:
/// - Text → "TEXT"
/// - Integer → "INTEGER"
/// - Real → "REAL"
/// - Blob → "BLOB"
/// - Numeric → "NUMERIC"
/// - Boolean → "INTEGER" (SQLite convention: 0 = false, 1 = true)
/// </returns>
/// <remarks>
/// SQLite has a dynamic type system with five storage classes:
/// NULL, INTEGER, REAL, TEXT, and BLOB. The NUMERIC type uses
/// type affinity rules to determine storage. Boolean values are
/// conventionally stored as INTEGER (0/1) in SQLite.
///
/// This function is internal and used by createTableSql to generate
/// the appropriate column type definitions.
/// </remarks>
/// <example>
/// <code>
/// // String field
/// mapSqliteType SqliteType.Text  // Returns "TEXT"
///
/// // Numeric field
/// mapSqliteType SqliteType.Integer  // Returns "INTEGER"
///
/// // Boolean field (stored as integer)
/// mapSqliteType SqliteType.Boolean  // Returns "INTEGER"
///
/// // In CREATE TABLE context:
/// let fieldType = mapSqliteType field.SqlType
/// let sql = $"_{field.Name} {fieldType} GENERATED ALWAYS AS ..."
/// </code>
/// </example>
let mapSqliteType (sqlType: SqliteType) : string =
    match sqlType with
    | SqliteType.Text -> "TEXT"
    | SqliteType.Integer -> "INTEGER"
    | SqliteType.Real -> "REAL"
    | SqliteType.Blob -> "BLOB"
    | SqliteType.Numeric -> "NUMERIC"
    | SqliteType.Boolean -> "INTEGER"  // SQLite convention: 0 = false, 1 = true

/// <summary>
/// Generates a CREATE TABLE SQL statement for a FractalDb collection schema.
/// </summary>
/// <param name="name">The table name (typically the collection name).</param>
/// <param name="schema">The SchemaDef containing field definitions and indexes.</param>
/// <returns>
/// A complete CREATE TABLE IF NOT EXISTS statement with:
/// - Base columns: _id (PRIMARY KEY), body (JSON BLOB), createdAt, updatedAt
/// - Generated columns for indexed fields using jsonb_extract
/// - Proper column types via mapSqliteType
/// - VIRTUAL generated columns (computed on read, not stored)
/// </returns>
/// <remarks>
/// The generated SQL follows FractalDb's storage model:
/// - _id: TEXT PRIMARY KEY (document identifier)
/// - body: BLOB NOT NULL (JSON document storage)
/// - createdAt: INTEGER NOT NULL (Unix timestamp milliseconds)
/// - updatedAt: INTEGER NOT NULL (Unix timestamp milliseconds)
/// - _fieldName: Generated columns for indexed fields
///   (e.g., _name TEXT GENERATED ALWAYS AS (jsonb_extract(body, '$.name')) VIRTUAL)
///
/// Generated columns enable efficient querying on indexed fields without
/// duplicating data. VIRTUAL columns are computed on read and not stored.
///
/// Field paths default to "$.{fieldName}" if not explicitly set in FieldDef.Path.
/// </remarks>
/// <example>
/// <code>
/// // Example schema
/// let schema = {
///     Fields = [
///         { Name = "email"; Path = None; SqlType = SqliteType.Text; Indexed = true; Unique = true; Nullable = false }
///         { Name = "age"; Path = None; SqlType = SqliteType.Integer; Indexed = true; Unique = false; Nullable = true }
///     ]
///     Indexes = []
///     Timestamps = true
/// }
///
/// let sql = createTableSql "users" schema
/// // Returns:
/// // CREATE TABLE IF NOT EXISTS users (
/// //     _id TEXT PRIMARY KEY,
/// //     body BLOB NOT NULL,
/// //     createdAt INTEGER NOT NULL,
/// //     updatedAt INTEGER NOT NULL,
/// //     _email TEXT GENERATED ALWAYS AS (jsonb_extract(body, '$.email')) VIRTUAL,
/// //     _age INTEGER GENERATED ALWAYS AS (jsonb_extract(body, '$.age')) VIRTUAL
/// // )
/// </code>
/// </example>
let createTableSql (name: string) (schema: SchemaDef<'T>) : string =
    let columns = [
        "_id TEXT PRIMARY KEY"
        "body BLOB NOT NULL"
        "createdAt INTEGER NOT NULL"
        "updatedAt INTEGER NOT NULL"
    ]

    let generatedCols =
        schema.Fields
        |> List.filter (fun f -> f.Indexed)
        |> List.map (fun f ->
            let colName = $"_{f.Name}"
            let sqlType = mapSqliteType f.SqlType
            let path = defaultArg f.Path $"$.{f.Name}"
            $"{colName} {sqlType} GENERATED ALWAYS AS (jsonb_extract(body, '{path}')) VIRTUAL"
        )

    let allColumns = columns @ generatedCols |> String.concat ",\n        "
    $"CREATE TABLE IF NOT EXISTS {name} (\n        {allColumns}\n    )"

/// <summary>
/// Generates a list of CREATE INDEX SQL statements for a FractalDb collection schema.
/// </summary>
/// <param name="name">The table name (typically the collection name).</param>
/// <param name="schema">The SchemaDef containing field definitions and composite indexes.</param>
/// <returns>
/// A list of CREATE INDEX IF NOT EXISTS statements including:
/// - Single-field indexes for each indexed field (e.g., idx_users_email)
/// - UNIQUE keyword for fields with Unique = true
/// - Composite indexes from schema.Indexes with multiple columns
/// - All indexes use generated column names (prefixed with underscore)
/// </returns>
/// <remarks>
/// Two types of indexes are generated:
///
/// 1. Field Indexes (from FieldDef with Indexed = true):
///    - Index name: idx_{tableName}_{fieldName}
///    - Column name: _{fieldName}
///    - UNIQUE if field.Unique is true
///
/// 2. Composite Indexes (from schema.Indexes):
///    - Index name: from IndexDef.Name
///    - Columns: multiple _{fieldName} columns comma-separated
///    - UNIQUE if idx.Unique is true
///    - Order of fields matters for query optimization
///
/// All indexes use IF NOT EXISTS to avoid errors on repeated calls.
/// Indexes are created on generated columns (not directly on JSON paths).
/// </remarks>
/// <example>
/// <code>
/// // Example schema with both field and composite indexes
/// let schema = {
///     Fields = [
///         { Name = "email"; Path = None; SqlType = SqliteType.Text;
///           Indexed = true; Unique = true; Nullable = false }
///         { Name = "status"; Path = None; SqlType = SqliteType.Text;
///           Indexed = true; Unique = false; Nullable = false }
///     ]
///     Indexes = [
///         { Name = "idx_users_email_status"; Fields = ["email"; "status"]; Unique = false }
///     ]
///     Timestamps = true
/// }
///
/// let indexes = createIndexesSql "users" schema
/// // Returns list:
/// // [
/// //   "CREATE UNIQUE INDEX IF NOT EXISTS idx_users_email ON users(_email)"
/// //   "CREATE INDEX IF NOT EXISTS idx_users_status ON users(_status)"
/// //   "CREATE INDEX IF NOT EXISTS idx_users_email_status ON users(_email, _status)"
/// // ]
/// </code>
/// </example>
let createIndexesSql (name: string) (schema: SchemaDef<'T>) : list<string> =
    let fieldIndexes =
        schema.Fields
        |> List.filter (fun f -> f.Indexed)
        |> List.map (fun f ->
            let unique = if f.Unique then "UNIQUE " else ""
            let indexName = $"idx_{name}_{f.Name}"
            $"CREATE {unique}INDEX IF NOT EXISTS {indexName} ON {name}(_{f.Name})"
        )

    let compoundIndexes =
        schema.Indexes
        |> List.map (fun idx ->
            let unique = if idx.Unique then "UNIQUE " else ""
            let cols = idx.Fields |> List.map (fun f -> $"_{f}") |> String.concat ", "
            $"CREATE {unique}INDEX IF NOT EXISTS {idx.Name} ON {name}({cols})"
        )

    fieldIndexes @ compoundIndexes

/// <summary>
/// Ensures that a table and its indexes exist in the database by executing DDL statements.
/// </summary>
/// <param name="conn">The IDbConnection to execute DDL statements against.</param>
/// <param name="name">The table name to create.</param>
/// <param name="schema">The SchemaDef defining the table structure and indexes.</param>
/// <returns>Unit - throws exception if DDL execution fails.</returns>
/// <remarks>
/// This function orchestrates table creation by:
/// 1. Generating CREATE TABLE SQL via createTableSql
/// 2. Executing the table creation statement
/// 3. Generating CREATE INDEX statements via createIndexesSql
/// 4. Executing each index creation statement
///
/// Uses Donald library for ADO.NET operations:
/// - Db.newCommand: creates command from SQL string
/// - Db.exec: executes non-query command, returns Result&lt;unit, DbError&gt;
///
/// All statements use IF NOT EXISTS clauses, making this function idempotent.
/// Safe to call multiple times - will not error if table/indexes already exist.
///
/// Exception handling:
/// - Result&lt;unit, DbError&gt; from Db.exec is not explicitly handled
/// - DbError will propagate as exception if DDL fails
/// - Caller should handle connection errors, schema errors, etc.
/// </remarks>
/// <example>
/// <code>
/// open System.Data
/// open Microsoft.Data.Sqlite
///
/// // Create connection
/// let conn = new SqliteConnection("Data Source=:memory:")
/// conn.Open()
///
/// // Define schema
/// let schema = {
///     Fields = [
///         { Name = "email"; Path = None; SqlType = SqliteType.Text;
///           Indexed = true; Unique = true; Nullable = false }
///     ]
///     Indexes = []
///     Timestamps = true
/// }
///
/// // Ensure table exists (idempotent)
/// ensureTable conn "users" schema  // Creates table and indexes
/// ensureTable conn "users" schema  // No-op, already exists
/// </code>
/// </example>
let ensureTable (conn: IDbConnection) (name: string) (schema: SchemaDef<'T>) : unit =
    // Create table
    let tableSql = createTableSql name schema
    conn
    |> Db.newCommand tableSql
    |> Db.exec
    |> ignore

    // Create indexes
    let indexesSql = createIndexesSql name schema
    indexesSql
    |> List.iter (fun indexSql ->
        conn
        |> Db.newCommand indexSql
        |> Db.exec
        |> ignore
    )
