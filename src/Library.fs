namespace FractalDb

/// <summary>
/// Public API for FractalDb - an embedded document database for F# with MongoDB-like API.
/// </summary>
///
/// <remarks>
/// This module re-exports all public types and functions from FractalDb for convenient access.
/// Import the entire API with: <c>open FractalDb</c>
///
/// FractalDb provides:
/// - Type-safe document storage with F# records
/// - MongoDB-inspired query API
/// - SQLite backend for durability
/// - ACID transactions
/// - Computation expressions for ergonomic queries
/// - Automatic schema management
/// </remarks>
///
/// <example>
/// <code>
/// open FractalDb
///
/// type User = { Name: string; Age: int; Active: bool }
///
/// // Open database
/// let db = FractalDb.Open("mydb.db", DbOptions.Default)
///
/// // Get collection with schema
/// let users = db.Collection&lt;User&gt;("users", SchemaDef.infer)
///
/// // Query with computation expression
/// let myQuery = query {
///     where (Query.field "age" (Query.gte 18))
///     where (Query.field "active" (Query.eq true))
/// }
///
/// // Execute query
/// let! results = users |> Collection.find myQuery None
/// </code>
/// </example>

// =============================================================================
// Core Types (from Types.fs)
// =============================================================================

/// <summary>
/// Metadata for a document including ID, version, and timestamps.
/// </summary>
///
/// <remarks>
/// DocumentMeta is automatically managed by FractalDb. All documents have:
/// - A unique ID (ULID format)
/// - A version number for optimistic concurrency
/// - Creation and update timestamps (Unix milliseconds)
/// </remarks>
///
/// <example>
/// <code>
/// let meta = {
///     Id = "01ARZ3NDEKTSV4RRFFQ69G5FAV"
///     Version = 1L
///     CreatedAt = 1704067200000L
///     UpdatedAt = Some 1704153600000L
/// }
/// </code>
/// </example>
type DocumentMeta = FractalDb.Types.DocumentMeta

/// <summary>
/// A document wrapper that combines user data with metadata.
/// </summary>
///
/// <typeparam name="'T">The type of user data stored in the document.</typeparam>
///
/// <remarks>
/// Document&lt;'T&gt; wraps user data with automatic metadata management.
/// The Meta field contains ID, version, and timestamps managed by the database.
/// </remarks>
///
/// <example>
/// <code>
/// type User = { Name: string; Age: int }
///
/// let doc: Document&lt;User&gt; = {
///     Meta = { Id = "..."; Version = 1L; CreatedAt = ...; UpdatedAt = None }
///     Data = { Name = "Alice"; Age = 30 }
/// }
/// </code>
/// </example>
type Document<'T> = FractalDb.Types.Document<'T>

/// <summary>
/// Functions for generating and validating ULIDs (Universally Unique Lexicographically Sortable Identifiers).
/// </summary>
///
/// <remarks>
/// ULIDs are 26-character strings that are:
/// - Lexicographically sortable
/// - Timestamp-based (first 48 bits)
/// - URL-safe (Base32 encoded)
/// - Compatible with UUID storage
///
/// Format: TTTTTTTTTTRRRRRRRRRRRRRRRR (10 timestamp + 16 random chars)
/// </remarks>
///
/// <example>
/// <code>
/// // Generate new ID
/// let id = IdGenerator.generate()
/// // Example: "01ARZ3NDEKTSV4RRFFQ69G5FAV"
///
/// // Validate ID format
/// let isValid = IdGenerator.isValid "01ARZ3NDEKTSV4RRFFQ69G5FAV"  // true
/// </code>
/// </example>
module IdGenerator = FractalDb.Types.IdGenerator

/// <summary>
/// Functions for working with Unix timestamps (milliseconds since epoch).
/// </summary>
///
/// <remarks>
/// Timestamps are stored as Int64 representing milliseconds since Unix epoch (1970-01-01 00:00:00 UTC).
/// This format is:
/// - Efficient for storage and comparison
/// - Compatible with JavaScript/TypeScript Date.now()
/// - Sortable as integers
/// </remarks>
///
/// <example>
/// <code>
/// // Get current timestamp
/// let now = Timestamp.now()
/// // Example: 1704067200000L
///
/// // Convert to DateTime
/// let dt = Timestamp.toDateTime now
///
/// // Convert from DateTime
/// let ts = Timestamp.fromDateTime System.DateTime.UtcNow
/// </code>
/// </example>
module Timestamp = FractalDb.Types.Timestamp

/// <summary>
/// Functions for creating and manipulating Document&lt;'T&gt; values.
/// </summary>
///
/// <remarks>
/// The Document module provides helpers for:
/// - Creating new documents with auto-generated metadata
/// - Updating document metadata (version, timestamps)
/// - Extracting user data from documents
/// </remarks>
///
/// <example>
/// <code>
/// type User = { Name: string; Age: int }
///
/// // Create new document
/// let doc = Document.create { Name = "Alice"; Age = 30 }
/// // Meta is auto-generated (ID, version=1, timestamps)
///
/// // Update document (increments version, updates timestamp)
/// let updated = Document.update doc { Name = "Alice"; Age = 31 }
/// </code>
/// </example>
module Document = FractalDb.Types.Document

// =============================================================================
// Query Operators (from Operators.fs)
// =============================================================================

/// <summary>
/// Comparison operators for querying document fields by value.
/// </summary>
///
/// <typeparam name="'T">The type of value being compared.</typeparam>
///
/// <remarks>
/// CompareOp provides type-safe comparison operations including:
/// - Equality/inequality (Eq, Ne)
/// - Ordered comparisons (Gt, Gte, Lt, Lte)
/// - Set membership (In, NotIn)
///
/// The type parameter ensures compile-time type safety for comparisons.
/// </remarks>
///
/// <example>
/// <code>
/// // Equality
/// CompareOp.Eq 5
///
/// // Range check
/// CompareOp.Gte 18
///
/// // Set membership
/// CompareOp.In ["active"; "pending"]
/// </code>
/// </example>
type CompareOp<'T> = FractalDb.Operators.CompareOp<'T>

/// <summary>
/// String pattern matching operators for querying text fields.
/// </summary>
///
/// <remarks>
/// StringOp provides SQL-like pattern matching including:
/// - LIKE/ILIKE patterns with wildcards (%, _)
/// - Convenience operators (Contains, StartsWith, EndsWith)
///
/// LIKE is case-sensitive, ILIKE is case-insensitive.
/// </remarks>
///
/// <example>
/// <code>
/// // Pattern matching
/// StringOp.Like "admin%"
///
/// // Case-insensitive
/// StringOp.ILike "smith"
///
/// // Convenience operators
/// StringOp.Contains "test"
/// StringOp.EndsWith ".com"
/// </code>
/// </example>
type StringOp = FractalDb.Operators.StringOp

/// <summary>
/// Array-specific operators for querying list/array fields.
/// </summary>
///
/// <typeparam name="'T">The type of elements in the array.</typeparam>
///
/// <remarks>
/// ArrayOp provides operations for array fields including:
/// - Contains all values (All)
/// - Array size check (Size)
/// - Element matching (ElemMatch, Index)
///
/// Only valid for array-typed fields.
/// </remarks>
///
/// <example>
/// <code>
/// // Array contains all specified values
/// ArrayOp.All ["featured"; "public"]
///
/// // Array has exact size
/// ArrayOp.Size 3
///
/// // Element matches query
/// ArrayOp.ElemMatch someQuery
/// </code>
/// </example>
type ArrayOp<'T> = FractalDb.Operators.ArrayOp<'T>

/// <summary>
/// Field existence check operator.
/// </summary>
///
/// <remarks>
/// ExistsOp checks whether a field is present in a document, independent of its value.
/// This is distinct from null checks - a field can exist with a null value,
/// or be completely absent from the JSON document.
/// </remarks>
///
/// <example>
/// <code>
/// // Field must exist (even if null)
/// ExistsOp.Exists true
///
/// // Field must not exist
/// ExistsOp.Exists false
/// </code>
/// </example>
type ExistsOp = FractalDb.Operators.ExistsOp

/// <summary>
/// Type-erased wrapper for field operations.
/// </summary>
///
/// <remarks>
/// FieldOp boxes generic operators to enable storage in a uniform query structure.
/// Contains:
/// - Compare: Boxed CompareOp&lt;'T&gt;
/// - String: StringOp
/// - Array: Boxed ArrayOp&lt;'T&gt;
/// - Exist: ExistsOp
///
/// Type safety is enforced at query construction time.
/// </remarks>
///
/// <example>
/// <code>
/// // Comparison
/// FieldOp.Compare (box (CompareOp.Gt 18))
///
/// // String matching
/// FieldOp.String (StringOp.Contains "admin")
///
/// // Array operation
/// FieldOp.Array (box (ArrayOp.Size 3))
///
/// // Existence check
/// FieldOp.Exist (ExistsOp.Exists true)
/// </code>
/// </example>
type FieldOp = FractalDb.Operators.FieldOp

/// <summary>
/// Complete query structure for building complex document queries.
/// </summary>
///
/// <typeparam name="'T">The document type being queried.</typeparam>
///
/// <remarks>
/// Query&lt;'T&gt; is the central type for expressing document queries.
/// Supports:
/// - Field-level operations
/// - Logical combinators (And, Or, Nor, Not)
/// - Empty query (matches all)
///
/// Queries are immutable and composable. Use Query module helpers or
/// QueryBuilder computation expression for ergonomic construction.
/// </remarks>
///
/// <example>
/// <code>
/// // Single field
/// Query.Field ("age", FieldOp.Compare (box (CompareOp.Gt 18)))
///
/// // Logical AND
/// Query.And [
///     Query.Field ("status", FieldOp.Compare (box (CompareOp.Eq "active")))
///     Query.Field ("age", FieldOp.Compare (box (CompareOp.Gte 18)))
/// ]
///
/// // Using computation expression
/// let q = query {
///     where (Query.field "age" (Query.gte 18))
///     where (Query.field "active" (Query.eq true))
/// }
/// </code>
/// </example>
type Query<'T> = FractalDb.Operators.Query<'T>

// =============================================================================
// Query Helpers (from Query.fs)
// =============================================================================

/// <summary>
/// Helper functions for constructing Query&lt;'T&gt; values ergonomically.
/// </summary>
///
/// <remarks>
/// The Query module provides a functional API for building queries with less verbose syntax.
/// Functions include:
/// - Comparison operators (eq, ne, gt, gte, lt, lte, in_, notIn)
/// - String operators (like, ilike, contains, startsWith, endsWith)
/// - Array operators (all, size, elemMatch, index)
/// - Logical combinators (and_, or_, nor, not_)
/// - Field operations (field, exists)
///
/// These are particularly useful with QueryBuilder computation expressions.
/// </remarks>
///
/// <example>
/// <code>
/// // Build query with helpers
/// let q = Query.field "age" (Query.gte 18)
///
/// // Combine with logical operators
/// let complex = Query.and_ [
///     Query.field "status" (Query.eq "active")
///     Query.field "age" (Query.gte 18)
/// ]
///
/// // Use in computation expression
/// let ce = query {
///     where (Query.field "name" (Query.contains "Alice"))
/// }
/// </code>
/// </example>
module Query = FractalDb.Query.Query

// =============================================================================
// Schema Types (from Schema.fs)
// =============================================================================

/// <summary>
/// SQLite column types for schema definitions.
/// </summary>
///
/// <remarks>
/// SqliteType maps F# types to SQLite storage classes:
/// - Integer: 64-bit signed integers
/// - Real: 64-bit IEEE floating point
/// - Text: UTF-8 text strings
/// - Blob: Binary data
/// - Numeric: Mixed numeric types
///
/// Used in FieldDef for explicit schema definitions.
/// </remarks>
///
/// <example>
/// <code>
/// SqliteType.Integer  // For int, int64, bool
/// SqliteType.Real     // For float, double
/// SqliteType.Text     // For string
/// SqliteType.Blob     // For byte[]
/// </code>
/// </example>
type SqliteType = FractalDb.Schema.SqliteType

/// <summary>
/// Field definition for schema specification.
/// </summary>
///
/// <remarks>
/// FieldDef describes a field in a document schema including:
/// - Field name (dot notation for nested fields)
/// - SQLite column type
/// - Optional constraints (unique, not null)
///
/// Used in SchemaDef to define document structure.
/// </remarks>
///
/// <example>
/// <code>
/// {
///     Name = "email"
///     Type = SqliteType.Text
///     Unique = true
///     NotNull = true
/// }
/// </code>
/// </example>
type FieldDef = FractalDb.Schema.FieldDef

/// <summary>
/// Index definition for optimizing queries.
/// </summary>
///
/// <remarks>
/// IndexDef creates an index on one or more fields to improve query performance.
/// Supports:
/// - Single or multi-column indexes
/// - Unique constraints
/// - Custom index names
/// </remarks>
///
/// <example>
/// <code>
/// {
///     Name = Some "idx_user_email"
///     Fields = ["email"]
///     Unique = true
/// }
/// </code>
/// </example>
type IndexDef = FractalDb.Schema.IndexDef

/// <summary>
/// Complete schema definition for a collection.
/// </summary>
///
/// <typeparam name="'T">The document type for this schema.</typeparam>
///
/// <remarks>
/// SchemaDef describes the structure of documents in a collection including:
/// - Field definitions (optional)
/// - Indexes for query optimization
///
/// Can be inferred from F# type or explicitly defined.
/// </remarks>
///
/// <example>
/// <code>
/// // Infer from type
/// let schema = SchemaDef.infer&lt;User&gt;
///
/// // Define explicitly
/// let schema = {
///     Fields = Some [
///         { Name = "email"; Type = SqliteType.Text; Unique = true; NotNull = true }
///         { Name = "age"; Type = SqliteType.Integer; Unique = false; NotNull = false }
///     ]
///     Indexes = [
///         { Name = Some "idx_email"; Fields = ["email"]; Unique = true }
///     ]
/// }
/// </code>
/// </example>
type SchemaDef<'T> = FractalDb.Schema.SchemaDef<'T>

// =============================================================================
// Query Options (from Options.fs)
// =============================================================================

/// <summary>
/// Sort direction for query results.
/// </summary>
///
/// <remarks>
/// Specifies ascending or descending order for sort operations.
/// Used in QueryOptions.Sort field.
/// </remarks>
///
/// <example>
/// <code>
/// SortDirection.Asc   // Sort ascending (A-Z, 0-9)
/// SortDirection.Desc  // Sort descending (Z-A, 9-0)
/// </code>
/// </example>
type SortDirection = FractalDb.Options.SortDirection

/// <summary>
/// Cursor specification for pagination.
/// </summary>
///
/// <remarks>
/// CursorSpec enables cursor-based pagination by specifying:
/// - Field to use as cursor (must be sortable and indexed)
/// - Last value seen in previous page
///
/// More efficient than offset-based pagination for large datasets.
/// </remarks>
///
/// <example>
/// <code>
/// {
///     Field = "createdAt"
///     LastValue = box 1704067200000L
/// }
/// </code>
/// </example>
type CursorSpec = FractalDb.Options.CursorSpec

/// <summary>
/// Text search specification for full-text queries.
/// </summary>
///
/// <remarks>
/// TextSearchSpec defines full-text search parameters including:
/// - Search terms
/// - Fields to search
/// - Case sensitivity
///
/// Requires FTS5 extension and appropriate indexes.
/// </remarks>
///
/// <example>
/// <code>
/// {
///     Terms = "alice developer"
///     Fields = ["name"; "bio"; "skills"]
///     CaseSensitive = false
/// }
/// </code>
/// </example>
type TextSearchSpec = FractalDb.Options.TextSearchSpec

/// <summary>
/// Query options for controlling query execution and result formatting.
/// </summary>
///
/// <typeparam name="'T">The document type being queried.</typeparam>
///
/// <remarks>
/// QueryOptions controls:
/// - Pagination (limit, skip, cursor)
/// - Sorting (field and direction)
/// - Projection (field selection)
/// - Text search (FTS5)
///
/// All fields are optional with sensible defaults.
/// </remarks>
///
/// <example>
/// <code>
/// {
///     Limit = Some 10
///     Skip = Some 0
///     Sort = [("createdAt", SortDirection.Desc)]
///     Projection = Some ["name"; "email"]
///     Cursor = None
///     Search = None
/// }
/// </code>
/// </example>
type QueryOptions<'T> = FractalDb.Options.QueryOptions<'T>

/// <summary>
/// Helper functions for creating QueryOptions values.
/// </summary>
///
/// <remarks>
/// Provides builders and combinators for QueryOptions including:
/// - Default empty options
/// - Pagination helpers (limit, skip, cursor)
/// - Sort helpers
/// - Projection helpers
/// - Text search helpers
/// </remarks>
///
/// <example>
/// <code>
/// // Default options
/// let opts = QueryOptions.empty
///
/// // With pagination
/// let opts = QueryOptions.empty |> QueryOptions.limit 10 |> QueryOptions.skip 20
///
/// // With sorting
/// let opts = QueryOptions.empty |> QueryOptions.sortBy "createdAt" SortDirection.Desc
/// </code>
/// </example>
module QueryOptions = FractalDb.Options.QueryOptions

// =============================================================================
// Errors (from Errors.fs)
// =============================================================================

/// <summary>
/// Error types that can occur during FractalDb operations.
/// </summary>
///
/// <remarks>
/// FractalError represents all possible error conditions including:
/// - NotFound: Document not found by ID
/// - ValidationError: Schema or constraint violation
/// - UniqueConstraintViolation: Unique index violation
/// - SerializationError: JSON serialization failure
/// - DatabaseError: SQLite error
/// - TransactionError: Transaction failure
/// - InvalidQuery: Query syntax or semantic error
///
/// All FractalDb operations return FractalResult&lt;'T&gt; wrapping either
/// success or FractalError.
/// </remarks>
///
/// <example>
/// <code>
/// match result with
/// | Ok value -> printfn "Success: %A" value
/// | Error (FractalError.NotFound id) -> printfn "Document %s not found" id
/// | Error (FractalError.ValidationError (field, msg)) -> printfn "Validation failed: %s - %s" field msg
/// | Error (FractalError.DatabaseError (op, msg)) -> printfn "DB error in %s: %s" op msg
/// | Error _ -> printfn "Other error"
/// </code>
/// </example>
type FractalError = FractalDb.Errors.FractalError

/// <summary>
/// Result type alias for FractalDb operations.
/// </summary>
///
/// <typeparam name="'T">The success value type.</typeparam>
///
/// <remarks>
/// FractalResult&lt;'T&gt; is an alias for Result&lt;'T, FractalError&gt;.
/// All FractalDb operations that can fail return this type instead of throwing exceptions.
/// This enables:
/// - Explicit error handling
/// - Composable error handling with Result combinators
/// - Exhaustive pattern matching
/// </remarks>
///
/// <example>
/// <code>
/// // Function returning FractalResult
/// let findUser (id: string) : Task&lt;FractalResult&lt;User option&gt;&gt; = ...
///
/// // Pattern match on result
/// match! findUser "123" with
/// | Ok (Some user) -> printfn "Found: %A" user
/// | Ok None -> printfn "User not found"
/// | Error err -> printfn "Error: %A" err
/// </code>
/// </example>
type FractalResult<'T> = FractalDb.Errors.FractalResult<'T>

/// <summary>
/// Helper functions for working with FractalResult values.
/// </summary>
///
/// <remarks>
/// Provides functional combinators for FractalResult including:
/// - map: Transform success value
/// - bind: Chain operations that return FractalResult
/// - traverse: Transform lists with FractalResult-returning functions
/// - combine: Combine multiple FractalResult values
///
/// Useful for composing error-handling logic.
/// </remarks>
///
/// <example>
/// <code>
/// // Map over success
/// let result = FractalResult.map (fun user -> user.Name) userResult
///
/// // Chain operations
/// let result =
///     findUser "123"
///     |> FractalResult.bind validateUser
///     |> FractalResult.bind saveUser
///
/// // Traverse list
/// let results = FractalResult.traverse findUser ["id1"; "id2"; "id3"]
/// </code>
/// </example>
module FractalResult = FractalDb.Errors.FractalResult

// =============================================================================
// Collection and Storage (from Collection.fs)
// =============================================================================

/// <summary>
/// A typed collection of documents in the database.
/// </summary>
///
/// <typeparam name="'T">The type of documents stored in this collection.</typeparam>
///
/// <remarks>
/// Collection&lt;'T&gt; is the primary interface for CRUD operations on documents.
/// Operations include:
/// - Queries (find, findOne, findById, count)
/// - Inserts (insertOne, insertMany)
/// - Updates (updateOne, updateMany, replaceOne)
/// - Deletes (deleteOne, deleteMany)
/// - Atomic operations (findOneAndUpdate, findOneAndReplace, findOneAndDelete)
///
/// Collections are obtained from FractalDb.Collection() method.
/// </remarks>
///
/// <example>
/// <code>
/// type User = { Name: string; Age: int }
///
/// let users = db.Collection&lt;User&gt;("users", SchemaDef.infer)
///
/// // Query
/// let! results = users |> Collection.find (Query.field "age" (Query.gte 18)) None
///
/// // Insert
/// let! result = users |> Collection.insertOne { Name = "Alice"; Age = 30 }
/// </code>
/// </example>
type Collection<'T> = FractalDb.Collection.Collection<'T>

/// <summary>
/// Result of inserting multiple documents.
/// </summary>
///
/// <typeparam name="'T">The type of documents inserted.</typeparam>
///
/// <remarks>
/// Contains:
/// - InsertedIds: List of generated document IDs
/// - Documents: Full documents with metadata
/// </remarks>
///
/// <example>
/// <code>
/// let! result = collection |> Collection.insertMany [user1; user2; user3]
/// printfn "Inserted %d documents" (List.length result.InsertedIds)
/// </code>
/// </example>
type InsertManyResult<'T> = FractalDb.Collection.InsertManyResult<'T>

/// <summary>
/// Result of an update operation.
/// </summary>
///
/// <remarks>
/// Contains:
/// - MatchedCount: Number of documents matching the query
/// - ModifiedCount: Number of documents actually modified
///
/// ModifiedCount may be less than MatchedCount if some documents
/// already had the target values.
/// </remarks>
///
/// <example>
/// <code>
/// let! result = collection |> Collection.updateMany query update
/// printfn "Matched: %d, Modified: %d" result.MatchedCount result.ModifiedCount
/// </code>
/// </example>
type UpdateResult = FractalDb.Collection.UpdateResult

/// <summary>
/// Result of a delete operation.
/// </summary>
///
/// <remarks>
/// Contains:
/// - DeletedCount: Number of documents deleted
/// </remarks>
///
/// <example>
/// <code>
/// let! result = collection |> Collection.deleteMany query
/// printfn "Deleted %d documents" result.DeletedCount
/// </code>
/// </example>
type DeleteResult = FractalDb.Collection.DeleteResult

/// <summary>
/// Specifies whether to return the document before or after modification.
/// </summary>
///
/// <remarks>
/// Used in atomic operations (findOneAndUpdate, findOneAndReplace) to control
/// which version of the document is returned.
/// </remarks>
///
/// <example>
/// <code>
/// ReturnDocument.Before  // Return original document
/// ReturnDocument.After   // Return modified document
/// </code>
/// </example>
type ReturnDocument = FractalDb.Collection.ReturnDocument

/// <summary>
/// Options for find operations.
/// </summary>
///
/// <remarks>
/// Alias for QueryOptions&lt;'T&gt; with additional find-specific behavior.
/// </remarks>
type FindOptions = FractalDb.Collection.FindOptions

/// <summary>
/// Options for atomic find-and-modify operations.
/// </summary>
///
/// <remarks>
/// Controls behavior of findOneAndUpdate/Replace/Delete operations including:
/// - Upsert: Whether to insert if no match found
/// - ReturnDocument: Before or After modification
/// - Sort: Which document to select if multiple matches
/// </remarks>
///
/// <example>
/// <code>
/// {
///     Upsert = true
///     ReturnDocument = ReturnDocument.After
///     Sort = Some [("createdAt", SortDirection.Desc)]
/// }
/// </code>
/// </example>
type FindAndModifyOptions = FractalDb.Collection.FindAndModifyOptions

/// <summary>
/// Functions for collection operations.
/// </summary>
///
/// <remarks>
/// The Collection module provides all CRUD operations as functions that take
/// a Collection&lt;'T&gt; as the last parameter, enabling pipeline-style usage.
///
/// Operations:
/// - find, findOne, findById, count
/// - insertOne, insertMany
/// - updateOne, updateMany, updateById, replaceOne
/// - deleteOne, deleteMany, deleteById
/// - findOneAndUpdate, findOneAndReplace, findOneAndDelete
/// - drop, createIndex
/// </remarks>
///
/// <example>
/// <code>
/// // Pipeline style
/// let! results =
///     collection
///     |> Collection.find query (Some options)
///
/// let! user =
///     collection
///     |> Collection.findById "user123"
///
/// let! count =
///     collection
///     |> Collection.count query
/// </code>
/// </example>
module Collection = FractalDb.Collection.Collection

// =============================================================================
// Database (from Database.fs)
// =============================================================================

/// <summary>
/// Configuration options for opening a database.
/// </summary>
///
/// <remarks>
/// DbOptions controls database behavior including:
/// - AutoCheckpoint: SQLite WAL checkpoint interval
/// - BusyTimeout: Timeout for lock acquisition
/// - CacheSize: Page cache size
/// - JournalMode: SQLite journal mode (WAL recommended)
/// - Synchronous: Durability vs. performance tradeoff
///
/// Use DbOptions.Default for sensible defaults optimized for FractalDb.
/// </remarks>
///
/// <example>
/// <code>
/// // Use defaults
/// let opts = DbOptions.Default
///
/// // Custom options
/// let opts = {
///     AutoCheckpoint = Some 1000
///     BusyTimeout = Some 5000
///     CacheSize = Some 10000
///     JournalMode = Some "WAL"
///     Synchronous = Some "NORMAL"
/// }
/// </code>
/// </example>
type DbOptions = FractalDb.Database.DbOptions

/// <summary>
/// Helper functions for creating DbOptions values.
/// </summary>
///
/// <remarks>
/// Provides:
/// - Default: Recommended default options
/// - Builder functions for customization
/// </remarks>
///
/// <example>
/// <code>
/// let opts =
///     DbOptions.Default
///     |> DbOptions.withBusyTimeout 10000
///     |> DbOptions.withCacheSize 20000
/// </code>
/// </example>
module DbOptions = FractalDb.Database.DbOptions

/// <summary>
/// Main database class for FractalDb operations.
/// </summary>
///
/// <remarks>
/// FractalDb provides:
/// - Database lifecycle (Open, InMemory, Close)
/// - Collection access with automatic schema management
/// - ACID transactions
/// - Connection management
///
/// Usage pattern:
/// 1. Open database (file or in-memory)
/// 2. Get collections with schemas
/// 3. Perform operations
/// 4. Close database when done (or use 'use' for automatic disposal)
/// </remarks>
///
/// <example>
/// <code>
/// // File-based database
/// use db = FractalDb.Open("mydb.db", DbOptions.Default)
///
/// // In-memory database
/// use db = FractalDb.InMemory(DbOptions.Default)
///
/// // Get collection
/// let users = db.Collection&lt;User&gt;("users", SchemaDef.infer)
///
/// // Use transact computation expression
/// let! result = db.Transact(fun t -> task {
///     let! _ = users |> Collection.insertOne user1 |> t.Run
///     let! _ = users |> Collection.insertOne user2 |> t.Run
///     return Ok ()
/// })
///
/// // Dispose closes the connection
/// </code>
/// </example>
type FractalDb = FractalDb.Database.FractalDb

// =============================================================================
// Computation Expression Builders (from Builders.fs)
// =============================================================================

// NOTE: Computation expression builders (query, schema, options) are already available globally
// through [<AutoOpen>] modules in Builders.fs:
// - QueryBuilderInstance: provides 'query' builder
// - SchemaBuilderInstance: provides 'schema<'T>' builder
// - OptionsBuilderInstance: provides 'options<'T>' builder
// - FractalDb.Transact: provides transaction builder as extension method
//
// No re-export needed here to avoid module name conflicts.
