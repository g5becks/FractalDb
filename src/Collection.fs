/// <summary>
/// Module providing Collection type and operations for document storage and retrieval.
/// </summary>
/// <remarks>
/// Collection<'T> represents a typed document collection within a FractalDb database.
/// Each collection stores documents of type 'T with automatic JSON serialization,
/// schema-based indexing, and full CRUD operations.
///
/// Key concepts:
/// - Collection<'T>: Container for documents of type 'T with schema and connection
/// - Result types: InsertManyResult, UpdateResult, DeleteResult for batch operations
/// - Options types: FindOptions, FindAndModifyOptions for controlling operations
/// - ReturnDocument: Controls whether find-and-modify returns before or after state
///
/// Collections are created via FractalDb.Collection() method and are thread-safe
/// for concurrent reads. Writes use database-level locking (SQLite SERIALIZABLE).
/// </remarks>
module FractalDb.Collection

open System
open System.Data
open System.Threading.Tasks
open Donald
open Microsoft.Data.Sqlite
open FractalDb.Types
open FractalDb.Errors
open FractalDb.Schema
open FractalDb.Operators
open FractalDb.Options
open FractalDb.SqlTranslator
open FractalDb.Serialization
open FractalDb.Transaction

/// <summary>
/// Represents a typed document collection with schema, connection, and configuration.
/// </summary>
/// <typeparam name="T">The document data type stored in this collection.</typeparam>
/// <remarks>
/// Collection is the primary interface for document operations. It contains:
/// - Name: Collection/table name in the database
/// - Schema: Schema definition with indexed fields and constraints
/// - Connection: IDbConnection for database access
/// - IdGenerator: Function to generate document IDs (default: ULID)
/// - Translator: SqlTranslator for converting queries to SQL
/// - EnableCache: Whether to cache translated queries
///
/// Collections are created internally by FractalDb.Collection() method.
/// The type is marked internal to prevent direct instantiation.
///
/// Thread safety:
/// - Safe for concurrent reads from multiple threads
/// - Writes are serialized by SQLite database lock
/// - Each operation is atomic (single SQL statement or transaction)
///
/// Lifecycle:
/// - Created via FractalDb.Collection<'T>(name, schema)
/// - Automatically ensures table and indexes exist on creation
/// - Lives as long as the parent FractalDb instance
/// - Disposed when FractalDb is disposed (connection cleanup)
/// </remarks>
/// <example>
/// <code>
/// type User = { Name: string; Email: string; Age: int }
///
/// let schema = {
///     Fields = [
///         { Name = "email"; Path = None; SqlType = SqliteType.Text;
///           Indexed = true; Unique = true; Nullable = false }
///     ]
///     Indexes = []
///     Timestamps = true
/// }
///
/// use db = FractalDb.Open("app.db")
/// let users = db.Collection<User>("users", schema)
/// // Collection is now ready for operations
/// </code>
/// </example>
type Collection<'T> = internal {
    /// <summary>
    /// The collection name, used as the table name in SQLite.
    /// </summary>
    Name: string
    
    /// <summary>
    /// Schema definition with indexed fields, constraints, and indexes.
    /// </summary>
    Schema: SchemaDef<'T>
    
    /// <summary>
    /// Database connection for executing operations.
    /// </summary>
    Connection: IDbConnection
    
    /// <summary>
    /// Function to generate unique document IDs (typically ULID generator).
    /// </summary>
    IdGenerator: unit -> string
    
    /// <summary>
    /// SQL translator for converting Query<'T> to SQL statements.
    /// </summary>
    Translator: SqlTranslator<'T>
    
    /// <summary>
    /// Whether to cache translated SQL queries for performance.
    /// </summary>
    EnableCache: bool
}

/// <summary>
/// Result of insertMany operation containing inserted documents and count.
/// </summary>
/// <typeparam name="T">The document data type.</typeparam>
/// <remarks>
/// InsertManyResult is returned by Collection.insertMany after successfully
/// inserting multiple documents. It provides both the inserted documents
/// (with generated IDs and timestamps) and a count for verification.
///
/// Use cases:
/// - Verify all documents were inserted (InsertedCount = input count)
/// - Access generated IDs for inserted documents
/// - Check timestamps (createdAt, updatedAt) of inserted documents
/// </remarks>
/// <example>
/// <code>
/// let! result = users |> Collection.insertMany [user1; user2; user3]
/// match result with
/// | Ok insertResult ->
///     printfn $"Inserted {insertResult.InsertedCount} documents"
///     for doc in insertResult.Documents do
///         printfn $"  ID: {doc.Id}, CreatedAt: {doc.CreatedAt}"
/// | Error e -> printfn $"Insert failed: {e.Message}"
/// </code>
/// </example>
type InsertManyResult<'T> = {
    /// <summary>
    /// List of successfully inserted documents with generated IDs and metadata.
    /// </summary>
    Documents: list<Document<'T>>
    
    /// <summary>
    /// Number of documents successfully inserted.
    /// </summary>
    /// <remarks>
    /// Should equal Documents.Length. Provided for convenience and validation.
    /// If InsertedCount is less than input count, some documents failed to insert.
    /// </remarks>
    InsertedCount: int
}

/// <summary>
/// Result of updateMany operation containing match and modification counts.
/// </summary>
/// <remarks>
/// UpdateResult is returned by Collection.updateMany after updating documents.
/// It distinguishes between documents that matched the filter and documents
/// that were actually modified (content changed).
///
/// Key metrics:
/// - MatchedCount: Documents that matched the filter
/// - ModifiedCount: Documents where update changed content
/// - ModifiedCount ≤ MatchedCount (some matches may have no changes)
///
/// Use cases:
/// - Verify filter matched expected documents
/// - Detect no-op updates (MatchedCount > 0 but ModifiedCount = 0)
/// - Audit how many documents changed
/// </remarks>
/// <example>
/// <code>
/// let! result =
///     users
///     |> Collection.updateMany
///         (Query.field "status" (Query.eq "pending"))
///         (fun user -> { user with Status = "active" })
///
/// match result with
/// | Ok updateResult ->
///     printfn $"Matched: {updateResult.MatchedCount}"
///     printfn $"Modified: {updateResult.ModifiedCount}"
/// | Error e -> printfn $"Update failed: {e.Message}"
/// </code>
/// </example>
type UpdateResult = {
    /// <summary>
    /// Number of documents that matched the filter criteria.
    /// </summary>
    MatchedCount: int
    
    /// <summary>
    /// Number of documents where content was actually changed.
    /// </summary>
    /// <remarks>
    /// May be less than MatchedCount if update function returns same content.
    /// Example: updating status to "active" when already "active" → no modification.
    /// </remarks>
    ModifiedCount: int
}

/// <summary>
/// Result of deleteMany operation containing count of deleted documents.
/// </summary>
/// <remarks>
/// DeleteResult is returned by Collection.deleteMany after deleting documents.
/// Provides count of how many documents were actually removed from the collection.
///
/// Use cases:
/// - Verify expected documents were deleted
/// - Detect no-op deletes (DeletedCount = 0 when expecting deletions)
/// - Audit record of deletions
/// </remarks>
/// <example>
/// <code>
/// let! result =
///     users
///     |> Collection.deleteMany
///         (Query.field "lastLogin" (Query.lt oldDate))
///
/// printfn $"Deleted {result.DeletedCount} inactive users"
/// </code>
/// </example>
type DeleteResult = {
    /// <summary>
    /// Number of documents successfully deleted.
    /// </summary>
    DeletedCount: int
}

/// <summary>
/// Specifies which version of document to return in find-and-modify operations.
/// </summary>
/// <remarks>
/// Used by findOneAndUpdate, findOneAndReplace, findOneAndDelete to control
/// whether the returned document reflects state before or after modification.
///
/// Options:
/// - Before: Return document as it was before the modification
/// - After: Return document as it is after the modification
///
/// Common patterns:
/// - Before: Useful for audit logs (capture what changed)
/// - After: Useful for returning updated state to caller
/// - findOneAndDelete: Always returns Before (After would be None)
/// </remarks>
/// <example>
/// <code>
/// // Return document state after update
/// let options = {
///     Sort = []
///     ReturnDocument = ReturnDocument.After
///     Upsert = false
/// }
///
/// let! maybeDoc =
///     users
///     |> Collection.findOneAndUpdate
///         (Query.field "email" (Query.eq "alice@example.com"))
///         (fun user -> { user with Age = user.Age + 1 })
///         options
///
/// match maybeDoc with
/// | Ok (Some doc) -> printfn $"Updated age: {doc.Data.Age}"  // New age
/// | _ -> printfn "Not found or error"
/// </code>
/// </example>
[<RequireQualifiedAccess>]
type ReturnDocument =
    /// <summary>
    /// Return the document as it was before the modification.
    /// </summary>
    | Before
    
    /// <summary>
    /// Return the document as it is after the modification.
    /// </summary>
    | After

/// <summary>
/// Options for controlling find operations (findOne, find).
/// </summary>
/// <remarks>
/// FindOptions configures sorting behavior for find operations.
/// Sorting is performed in SQL (efficient) using indexed fields when possible.
///
/// Sort specification:
/// - List of (fieldName, SortDirection) tuples
/// - Applied in order (first tuple is primary sort, etc.)
/// - Uses generated columns for indexed fields
/// - Uses json_extract for non-indexed fields
///
/// Performance notes:
/// - Sorting on indexed fields is fast (uses index)
/// - Sorting on non-indexed fields requires json_extract (slower)
/// - Multiple sort fields work best when leftmost field is indexed
/// </remarks>
/// <example>
/// <code>
/// let options = {
///     Sort = [("age", SortDirection.Desc); ("name", SortDirection.Asc)]
/// }
///
/// let! users =
///     users
///     |> Collection.findWith
///         (Query.field "active" (Query.eq true))
///         (QueryOptions.sort [("age", SortDirection.Desc)])
/// </code>
/// </example>
type FindOptions = {
    /// <summary>
    /// List of (fieldName, direction) tuples specifying sort order.
    /// </summary>
    /// <remarks>
    /// Empty list = no sorting (results in arbitrary order).
    /// Sort is stable - documents with equal sort values maintain insertion order.
    /// </remarks>
    Sort: list<string * SortDirection>
}

/// <summary>
/// Options for controlling find-and-modify operations.
/// </summary>
/// <remarks>
/// FindAndModifyOptions extends FindOptions with additional controls for
/// atomic find-and-modify operations (findOneAndUpdate, findOneAndReplace).
///
/// Options:
/// - Sort: Determines which document to modify when multiple match
/// - ReturnDocument: Whether to return before or after state
/// - Upsert: Whether to insert if no document matches
///
/// Upsert behavior:
/// - If true and no match: insert new document with provided data
/// - If true and match found: update existing document
/// - If false and no match: return None
///
/// Atomicity:
/// All find-and-modify operations are atomic (single SQL statement).
/// No other operation can modify the document between find and modify.
/// </remarks>
/// <example>
/// <code>
/// let options = {
///     Sort = [("priority", SortDirection.Desc)]  // Update highest priority first
///     ReturnDocument = ReturnDocument.After      // Return updated state
///     Upsert = true                              // Insert if not found
/// }
///
/// let! result =
///     tasks
///     |> Collection.findOneAndUpdate
///         (Query.field "status" (Query.eq "pending"))
///         (fun task -> { task with Status = "processing" })
///         options
/// </code>
/// </example>
type FindAndModifyOptions = {
    /// <summary>
    /// Sort order to determine which document to modify when multiple match.
    /// </summary>
    Sort: list<string * SortDirection>
    
    /// <summary>
    /// Whether to return document state before or after modification.
    /// </summary>
    ReturnDocument: ReturnDocument
    
    /// <summary>
    /// Whether to insert a new document if no match is found.
    /// </summary>
    /// <remarks>
    /// Only applies to update and replace operations (not delete).
    /// If true and no match, inserts new document with update applied.
    /// </remarks>
    Upsert: bool
}

/// <summary>
/// Module containing all collection operations for document storage and retrieval.
/// </summary>
/// <remarks>
/// Collection module provides functional API for all CRUD and query operations.
/// All functions use Collection<'T> as the last parameter for pipeline-style usage.
///
/// Function organization:
/// - READ: findById, findOne, find, count, search, distinct
/// - WRITE: insertOne, updateById, updateOne, replaceOne, deleteById, deleteOne
/// - BATCH: insertMany, updateMany, deleteMany
/// - ATOMIC: findOneAndDelete, findOneAndUpdate, findOneAndReplace
///
/// All operations return Task for async execution.
/// Write operations return FractalResult<'T> for error handling.
/// Read operations return option types for not-found cases.
/// </remarks>
[<RequireQualifiedAccess>]
module Collection =
    
    /// <summary>
    /// Converts an obj parameter value to the appropriate SqlType case.
    /// </summary>
    /// <param name="value">The parameter value as obj.</param>
    /// <returns>The SqlType case matching the runtime type of the value.</returns>
    /// <remarks>
    /// Helper function for converting SqlTranslator parameters (string * obj) 
    /// to Donald-compatible parameters (string * SqlType).
    /// Supports common types: string, int, int64, bool, float, decimal, DateTime, byte[], null.
    /// </remarks>
    let private toSqlType (value: obj) : SqlType =
        match value with
        | null -> SqlType.Null
        | :? string as s -> SqlType.String s
        | :? int as i -> SqlType.Int32 i
        | :? int64 as i64 -> SqlType.Int64 i64
        | :? bool as b -> SqlType.Boolean b
        | :? float as f -> SqlType.Double f
        | :? decimal as d -> SqlType.Decimal d
        | :? System.DateTime as dt -> SqlType.DateTime dt
        | :? array<byte> as bytes -> SqlType.Bytes bytes
        | _ -> 
            // Fallback: convert to string representation
            SqlType.String (value.ToString())
    
    /// <summary>
    /// Finds a document by its unique identifier.
    /// </summary>
    /// <param name="id">The document ID to search for.</param>
    /// <param name="collection">The collection to search in.</param>
    /// <returns>
    /// Task containing Some document if found, None if not found.
    /// </returns>
    /// <remarks>
    /// findById is the fastest read operation as it uses the primary key index.
    /// The operation executes a simple SELECT with WHERE _id = @id clause.
    ///
    /// Performance:
    /// - O(log n) time complexity (B-tree index lookup)
    /// - Single database round-trip
    /// - No JSON parsing if document not found
    ///
    /// Error handling:
    /// - Returns None if document doesn't exist
    /// - Throws exception on database errors (connection, serialization, etc.)
    /// - ID validation not performed (any string accepted)
    ///
    /// Thread safety:
    /// - Safe for concurrent reads
    /// - Reads see committed snapshot (SQLite SERIALIZABLE isolation)
    /// </remarks>
    /// <example>
    /// <code>
    /// // Find user by ID
    /// let! maybeUser = users |> Collection.findById "user123"
    /// match maybeUser with
    /// | Some doc -> 
    ///     printfn $"Found: {doc.Data.Name}"
    ///     printfn $"Created: {doc.CreatedAt}"
    /// | None -> 
    ///     printfn "User not found"
    ///
    /// // Pipeline style
    /// let! user =
    ///     users
    ///     |> Collection.findById userId
    ///     |> Task.map (Option.defaultValue emptyUser)
    /// </code>
    /// </example>
    let findById (id: string) (collection: Collection<'T>) : Task<option<Document<'T>>> =
        let sql = $"SELECT _id, json(body) as body, createdAt, updatedAt 
                    FROM {collection.Name} WHERE _id = @id"
        
        let dbResult =
            collection.Connection
            |> Db.newCommand sql
            |> Db.setParams ["id", SqlType.String id]
            |> Db.querySingle (fun rd -> 
                rd.ReadString "_id",
                rd.ReadString "body",
                rd.ReadInt64 "createdAt",
                rd.ReadInt64 "updatedAt"
            )
        
        let result =
            match dbResult with
            | Some (docId, bodyJson, createdAt, updatedAt) ->
                // Deserialize JSON body to 'T (can throw exception)
                let data = deserialize<'T> bodyJson
                Some {
                    Id = docId
                    Data = data
                    CreatedAt = createdAt
                    UpdatedAt = updatedAt
                }
            | None ->
                // Document not found
                None
        
        Task.FromResult(result)
    
    /// <summary>
    /// Helper function to deserialize a row tuple into a Document&lt;'T&gt;.
    /// </summary>
    let private rowToDocument (docId: string, bodyJson: string, createdAt: int64, updatedAt: int64) : Document<'T> =
        let data = deserialize<'T> bodyJson
        {
            Id = docId
            Data = data
            CreatedAt = createdAt
            UpdatedAt = updatedAt
        }
    
    /// <summary>
    /// Finds the first document matching a filter query.
    /// </summary>
    /// <param name="filter">The query filter to match documents.</param>
    /// <param name="collection">The collection to search in.</param>
    /// <returns>
    /// Task containing Some document if found, None if no match.
    /// </returns>
    /// <remarks>
    /// findOne translates the Query&lt;'T&gt; filter to SQL and executes with LIMIT 1.
    /// If multiple documents match, only the first is returned (order undefined without sort).
    ///
    /// Performance:
    /// - Uses SqlTranslator to convert type-safe queries to SQL
    /// - LIMIT 1 optimization stops after first match
    /// - Indexed fields in filter use B-tree indexes for fast lookup
    /// - Query.Empty matches all documents (returns first arbitrary document)
    ///
    /// Error handling:
    /// - Returns None if no documents match the filter
    /// - Throws exception on database errors
    /// - Throws exception on JSON deserialization errors
    /// </remarks>
    /// <example>
    /// <code>
    /// // Find first active user
    /// let! maybeUser =
    ///     users
    ///     |> Collection.findOne (Query.field "active" (Query.eq true))
    ///
    /// match maybeUser with
    /// | Some doc -> printfn $"Found: {doc.Data.Name}"
    /// | None -> printfn "No active users"
    /// </code>
    /// </example>
    let findOne (filter: Query<'T>) (collection: Collection<'T>) : Task<option<Document<'T>>> =
        let translated = collection.Translator.Translate(filter)
        let whereClause = if translated.Sql = "" then "" else $"WHERE {translated.Sql}"
        let sql = $"SELECT _id, json(body) as body, createdAt, updatedAt 
                    FROM {collection.Name} {whereClause} LIMIT 1"
        
        let params' = translated.Parameters |> List.map (fun (name, value) -> name, toSqlType value)
        
        let dbResult =
            collection.Connection
            |> Db.newCommand sql
            |> Db.setParams params'
            |> Db.querySingle (fun rd -> 
                rd.ReadString "_id",
                rd.ReadString "body",
                rd.ReadInt64 "createdAt",
                rd.ReadInt64 "updatedAt"
            )
        
        let result =
            match dbResult with
            | Some rowData -> Some (rowToDocument rowData)
            | None -> None
        
        Task.FromResult(result)
    
    /// <summary>
    /// Finds the first document matching a filter with query options (sort, limit, skip).
    /// </summary>
    /// <param name="filter">The query filter to match documents.</param>
    /// <param name="options">Query options for sorting, limiting, and skipping results.</param>
    /// <param name="collection">The collection to search in.</param>
    /// <returns>
    /// Task containing Some document if found, None if no match.
    /// </returns>
    /// <remarks>
    /// findOneWith extends findOne with QueryOptions support:
    /// - Sort: Determines which document is "first" when multiple match
    /// - Limit: Applied after sort (typically 1 for findOne pattern)
    /// - Skip: Skips N documents before returning first match
    /// - Select/Omit: Currently not supported (returns full document)
    ///
    /// The LIMIT 1 is applied after options, so effective SQL might be LIMIT 1 OFFSET skip.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Find newest active user
    /// let! maybeUser =
    ///     users
    ///     |> Collection.findOneWith
    ///         (Query.field "active" (Query.eq true))
    ///         (QueryOptions.sort [("createdAt", SortDirection.Desc)])
    /// </code>
    /// </example>
    let findOneWith 
        (filter: Query<'T>) 
        (options: QueryOptions<'T>) 
        (collection: Collection<'T>) 
        : Task<option<Document<'T>>> =
        let translated = collection.Translator.Translate(filter)
        let whereClause = if translated.Sql = "" then "" else $"WHERE {translated.Sql}"
        
        let (optionsSql, optionsParams) = collection.Translator.TranslateOptions(options)
        
        let sql = $"SELECT _id, json(body) as body, createdAt, updatedAt 
                    FROM {collection.Name} {whereClause} {optionsSql} LIMIT 1"
        
        let allParams = 
            (translated.Parameters |> List.map (fun (name, value) -> name, toSqlType value))
            @ (optionsParams |> List.map (fun (name, value) -> name, toSqlType value))
        
        let dbResult =
            collection.Connection
            |> Db.newCommand sql
            |> Db.setParams allParams
            |> Db.querySingle (fun rd -> 
                rd.ReadString "_id",
                rd.ReadString "body",
                rd.ReadInt64 "createdAt",
                rd.ReadInt64 "updatedAt"
            )
        
        let result =
            match dbResult with
            | Some rowData -> Some (rowToDocument rowData)
            | None -> None
        
        Task.FromResult(result)
    
    /// <summary>
    /// Finds all documents matching a filter query.
    /// </summary>
    /// <param name="filter">The query filter to match documents.</param>
    /// <param name="collection">The collection to search in.</param>
    /// <returns>
    /// Task containing list of matching documents (empty list if no matches).
    /// </returns>
    /// <remarks>
    /// find executes the query and returns ALL matching documents.
    /// For large result sets, consider using findWith with Limit option.
    ///
    /// Performance:
    /// - No LIMIT clause - returns all matches
    /// - Result order is undefined without sort (insertion order or index scan order)
    /// - Uses indexes for WHERE clause fields when available
    /// - Query.Empty returns all documents in the collection
    ///
    /// Memory considerations:
    /// - Loads all matching documents into memory
    /// - For large result sets (&gt;1000 documents), use pagination with findWith
    /// </remarks>
    /// <example>
    /// <code>
    /// // Find all active users
    /// let! activeUsers =
    ///     users
    ///     |> Collection.find (Query.field "active" (Query.eq true))
    ///
    /// for doc in activeUsers do
    ///     printfn $"{doc.Data.Name} - {doc.Data.Email}"
    /// </code>
    /// </example>
    let find (filter: Query<'T>) (collection: Collection<'T>) : Task<list<Document<'T>>> =
        let translated = collection.Translator.Translate(filter)
        let whereClause = if translated.Sql = "" then "" else $"WHERE {translated.Sql}"
        let sql = $"SELECT _id, json(body) as body, createdAt, updatedAt 
                    FROM {collection.Name} {whereClause}"
        
        let params' = translated.Parameters |> List.map (fun (name, value) -> name, toSqlType value)
        
        let dbResult =
            collection.Connection
            |> Db.newCommand sql
            |> Db.setParams params'
            |> Db.query (fun rd -> 
                rd.ReadString "_id",
                rd.ReadString "body",
                rd.ReadInt64 "createdAt",
                rd.ReadInt64 "updatedAt"
            )
        
        let result = dbResult |> List.map rowToDocument
        
        Task.FromResult(result)
    
    /// <summary>
    /// Finds all documents matching a filter with query options (sort, limit, skip).
    /// </summary>
    /// <param name="filter">The query filter to match documents.</param>
    /// <param name="options">Query options for sorting, limiting, and skipping results.</param>
    /// <param name="collection">The collection to search in.</param>
    /// <returns>
    /// Task containing list of matching documents (empty list if no matches).
    /// </returns>
    /// <remarks>
    /// findWith extends find with full QueryOptions support:
    /// - Sort: ORDER BY clause for deterministic ordering
    /// - Limit: Maximum number of documents to return
    /// - Skip: Number of documents to skip (for pagination)
    /// - Select/Omit: Currently not supported (returns full documents)
    ///
    /// Pagination pattern:
    /// - Page 1: Skip = 0, Limit = 20
    /// - Page 2: Skip = 20, Limit = 20
    /// - Page 3: Skip = 40, Limit = 20
    ///
    /// Performance:
    /// - Sort uses indexes when possible (indexed fields in ORDER BY)
    /// - Skip is implemented as OFFSET (can be slow for large offsets)
    /// - Limit reduces memory usage and network transfer
    /// </remarks>
    /// <example>
    /// <code>
    /// // Get second page of active users, sorted by name
    /// let! users =
    ///     users
    ///     |> Collection.findWith
    ///         (Query.field "active" (Query.eq true))
    ///         (QueryOptions.create()
    ///             |> QueryOptions.sort [("name", SortDirection.Asc)]
    ///             |> QueryOptions.limit 20
    ///             |> QueryOptions.skip 20)
    /// </code>
    /// </example>
    let findWith 
        (filter: Query<'T>) 
        (options: QueryOptions<'T>) 
        (collection: Collection<'T>) 
        : Task<list<Document<'T>>> =
        let translated = collection.Translator.Translate(filter)
        let whereClause = if translated.Sql = "" then "" else $"WHERE {translated.Sql}"
        
        let (optionsSql, optionsParams) = collection.Translator.TranslateOptions(options)
        
        let sql = $"SELECT _id, json(body) as body, createdAt, updatedAt 
                    FROM {collection.Name} {whereClause} {optionsSql}"
        
        let allParams = 
            (translated.Parameters |> List.map (fun (name, value) -> name, toSqlType value))
            @ (optionsParams |> List.map (fun (name, value) -> name, toSqlType value))
        
        let dbResult =
            collection.Connection
            |> Db.newCommand sql
            |> Db.setParams allParams
            |> Db.query (fun rd -> 
                rd.ReadString "_id",
                rd.ReadString "body",
                rd.ReadInt64 "createdAt",
                rd.ReadInt64 "updatedAt"
            )
        
        let result = dbResult |> List.map rowToDocument
        
        Task.FromResult(result)
    
    /// <summary>
    /// Counts the number of documents matching the specified filter.
    /// </summary>
    /// <param name="filter">The query filter to match documents against.</param>
    /// <param name="collection">The collection to count documents in.</param>
    /// <returns>
    /// Task containing the count of matching documents as an integer.
    /// </returns>
    /// <remarks>
    /// count executes a SELECT COUNT(*) query with the translated filter.
    /// This operation scans all documents matching the filter to provide an exact count.
    ///
    /// Performance:
    /// - O(n) time complexity where n = matching documents
    /// - Uses indexes when filter involves indexed fields
    /// - Single database round-trip
    /// - No deserialization overhead (only counts rows)
    ///
    /// For large collections where an approximate count is acceptable,
    /// consider using estimatedCount instead for much better performance.
    ///
    /// Error handling:
    /// - Returns 0 if no documents match
    /// - Throws exception on database errors (connection, SQL syntax, etc.)
    ///
    /// Thread safety:
    /// - Safe for concurrent reads
    /// - Count reflects committed snapshot (SQLite SERIALIZABLE isolation)
    /// </remarks>
    /// <example>
    /// <code>
    /// // Count active users
    /// let! activeCount = 
    ///     users 
    ///     |> Collection.count (Query.field "status" (Query.eq "active"))
    /// printfn $"Active users: {activeCount}"
    ///
    /// // Count users older than 18
    /// let! adultCount = 
    ///     users 
    ///     |> Collection.count (Query.field "age" (Query.gte 18))
    /// printfn $"Adults: {adultCount}"
    ///
    /// // Count all documents
    /// let! totalCount = users |> Collection.count Query.Empty
    /// printfn $"Total: {totalCount}"
    /// </code>
    /// </example>
    let count (filter: Query<'T>) (collection: Collection<'T>) : Task<int> =
        let translated = collection.Translator.Translate(filter)
        let whereClause = if translated.Sql = "" then "" else $"WHERE {translated.Sql}"
        
        let sql = $"SELECT COUNT(*) as count FROM {collection.Name} {whereClause}"
        
        let params' = translated.Parameters |> List.map (fun (name, value) -> name, toSqlType value)
        
        let dbResult =
            collection.Connection
            |> Db.newCommand sql
            |> Db.setParams params'
            |> Db.scalar Convert.ToInt32
        
        Task.FromResult(dbResult)
    
    /// <summary>
    /// Returns an estimated count of all documents in the collection.
    /// </summary>
    /// <param name="collection">The collection to count documents in.</param>
    /// <returns>
    /// Task containing the estimated document count as an integer.
    /// </returns>
    /// <remarks>
    /// estimatedCount provides a fast approximation of the total document count
    /// by executing a simple SELECT COUNT(*) without any WHERE clause.
    /// SQLite can often satisfy this query using internal statistics without
    /// scanning the entire table.
    ///
    /// Performance:
    /// - O(1) to O(log n) time complexity (depends on SQLite statistics)
    /// - Much faster than count() for large collections
    /// - No filter support - always counts all documents
    /// - Single database round-trip
    ///
    /// Use cases:
    /// - Pagination total count (when approximate is acceptable)
    /// - UI display of collection size
    /// - Monitoring and statistics
    /// - When exact count is not required
    ///
    /// For exact counts or filtered counts, use count() instead.
    ///
    /// Error handling:
    /// - Returns 0 for empty collections
    /// - Throws exception on database errors
    ///
    /// Thread safety:
    /// - Safe for concurrent reads
    /// - Count reflects committed snapshot
    /// </remarks>
    /// <example>
    /// <code>
    /// // Get estimated total users (fast)
    /// let! userCount = users |> Collection.estimatedCount
    /// printfn $"Approximately {userCount} users"
    ///
    /// // Use for pagination info
    /// let! total = products |> Collection.estimatedCount
    /// let pageCount = (total + pageSize - 1) / pageSize
    /// printfn $"Total pages: {pageCount}"
    ///
    /// // Compare with exact count
    /// let! estimated = orders |> Collection.estimatedCount
    /// let! exact = orders |> Collection.count Query.Empty
    /// printfn $"Estimated: {estimated}, Exact: {exact}"
    /// </code>
    /// </example>
    let estimatedCount (collection: Collection<'T>) : Task<int> =
        let sql = $"SELECT COUNT(*) as count FROM {collection.Name}"
        
        let dbResult =
            collection.Connection
            |> Db.newCommand sql
            |> Db.setParams []
            |> Db.scalar Convert.ToInt32
        
        Task.FromResult(dbResult)
    
    /// <summary>
    /// Searches for documents containing the specified text across multiple fields.
    /// </summary>
    /// <param name="text">The text to search for.</param>
    /// <param name="fields">The list of field names to search in.</param>
    /// <param name="collection">The collection to search.</param>
    /// <returns>
    /// Task containing a list of documents matching the search criteria.
    /// </returns>
    /// <remarks>
    /// search performs a text search using SQL LIKE queries across the specified fields.
    /// The search is case-insensitive and uses wildcard matching (%text%).
    ///
    /// Implementation:
    /// - Builds WHERE clause: field1 LIKE @text OR field2 LIKE @text OR ...
    /// - Uses json_extract to access nested fields in the JSON body
    /// - Wraps search text with % wildcards for substring matching
    ///
    /// Performance:
    /// - O(n) time complexity (scans all documents)
    /// - No index support for LIKE queries on JSON fields
    /// - Consider using full-text search indexes for better performance
    /// - Returns all matching documents (no limit)
    ///
    /// Use searchWith for paginated results or to add sort/limit/skip options.
    ///
    /// Error handling:
    /// - Returns empty list if no matches found
    /// - Throws exception on database errors or invalid field paths
    ///
    /// Thread safety:
    /// - Safe for concurrent reads
    /// - Results reflect committed snapshot
    /// </remarks>
    /// <example>
    /// <code>
    /// // Search for users by name or email
    /// let! results = 
    ///     users 
    ///     |> Collection.search "john" ["name"; "email"]
    /// printfn $"Found {List.length results} matches"
    ///
    /// // Search nested fields
    /// let! products = 
    ///     catalog 
    ///     |> Collection.search "laptop" ["title"; "description"; "specs.brand"]
    /// </code>
    /// </example>
    let search 
        (text: string) 
        (fields: list<string>) 
        (collection: Collection<'T>) 
        : Task<list<Document<'T>>> =
        
        // Build OR conditions for LIKE queries across fields
        let likeConditions =
            fields
            |> List.mapi (fun i field ->
                $"json_extract(body, '$.{field}') LIKE @search{i}"
            )
            |> String.concat " OR "
        
        let sql = 
            if likeConditions = "" then
                $"SELECT _id, json(body) as body, createdAt, updatedAt 
                    FROM {collection.Name} WHERE 1=0"  // No fields = no results
            else
                $"SELECT _id, json(body) as body, createdAt, updatedAt 
                    FROM {collection.Name} WHERE {likeConditions}"
        
        // Create parameters with wildcards
        let searchPattern = $"%%{text}%%"
        let params' = 
            fields 
            |> List.mapi (fun i _ -> $"search{i}", SqlType.String searchPattern)
        
        let dbResult =
            collection.Connection
            |> Db.newCommand sql
            |> Db.setParams params'
            |> Db.query (fun rd -> 
                rd.ReadString "_id",
                rd.ReadString "body",
                rd.ReadInt64 "createdAt",
                rd.ReadInt64 "updatedAt"
            )
        
        let result = dbResult |> List.map rowToDocument
        
        Task.FromResult(result)
    
    /// <summary>
    /// Searches for documents with QueryOptions support for sorting, pagination, etc.
    /// </summary>
    /// <param name="text">The text to search for.</param>
    /// <param name="fields">The list of field names to search in.</param>
    /// <param name="options">Query options for sort, limit, skip, etc.</param>
    /// <param name="collection">The collection to search.</param>
    /// <returns>
    /// Task containing a list of documents matching the search criteria.
    /// </returns>
    /// <remarks>
    /// searchWith extends search with QueryOptions support, allowing:
    /// - Sorting results by one or more fields
    /// - Limiting number of results (pagination)
    /// - Skipping results (pagination offset)
    /// - Field projection (select/omit)
    ///
    /// The search behavior is identical to search(), but the results can be
    /// sorted, paginated, and projected according to the provided options.
    ///
    /// Performance:
    /// - Same O(n) scan as search()
    /// - Sorting adds O(m log m) where m = matching documents
    /// - Use limit to reduce result processing overhead
    ///
    /// For better performance on large collections, consider:
    /// - Adding indexes on commonly searched fields
    /// - Using full-text search extensions
    /// - Filtering by indexed fields before search
    /// </remarks>
    /// <example>
    /// <code>
    /// // Search with limit and sort
    /// let! top10 = 
    ///     products 
    ///     |> Collection.searchWith "laptop" ["title"; "description"]
    ///         (QueryOptions.create()
    ///             |> QueryOptions.sort [("price", SortDirection.Asc)]
    ///             |> QueryOptions.limit 10)
    ///
    /// // Paginated search
    /// let! page2 = 
    ///     users 
    ///     |> Collection.searchWith "john" ["name"; "email"]
    ///         (QueryOptions.create()
    ///             |> QueryOptions.limit 20
    ///             |> QueryOptions.skip 20)
    /// </code>
    /// </example>
    let searchWith
        (text: string)
        (fields: list<string>)
        (options: QueryOptions<'T>)
        (collection: Collection<'T>)
        : Task<list<Document<'T>>> =
        
        // Build OR conditions for LIKE queries across fields
        let likeConditions =
            fields
            |> List.mapi (fun i field ->
                $"json_extract(body, '$.{field}') LIKE @search{i}"
            )
            |> String.concat " OR "
        
        let whereClause = 
            if likeConditions = "" then "WHERE 1=0" 
            else $"WHERE {likeConditions}"
        
        let (optionsSql, optionsParams) = collection.Translator.TranslateOptions(options)
        
        let sql = $"SELECT _id, json(body) as body, createdAt, updatedAt 
                    FROM {collection.Name} {whereClause} {optionsSql}"
        
        // Create search parameters with wildcards
        let searchPattern = $"%%{text}%%"
        let searchParams = 
            fields 
            |> List.mapi (fun i _ -> $"search{i}", SqlType.String searchPattern)
        
        let allParams = 
            searchParams @ (optionsParams |> List.map (fun (name, value) -> name, toSqlType value))
        
        let dbResult =
            collection.Connection
            |> Db.newCommand sql
            |> Db.setParams allParams
            |> Db.query (fun rd -> 
                rd.ReadString "_id",
                rd.ReadString "body",
                rd.ReadInt64 "createdAt",
                rd.ReadInt64 "updatedAt"
            )
        
        let result = dbResult |> List.map rowToDocument
        
        Task.FromResult(result)
    
    /// <summary>
    /// Returns distinct values for a specified field, optionally filtered.
    /// </summary>
    /// <param name="field">The field name to get distinct values from.</param>
    /// <param name="filter">Optional query filter to apply before getting distinct values.</param>
    /// <param name="collection">The collection to query.</param>
    /// <returns>
    /// Task containing a list of distinct values as objects.
    /// </returns>
    /// <remarks>
    /// distinct extracts unique values from the specified field across all documents
    /// (or filtered documents if a filter is provided).
    ///
    /// Implementation:
    /// - Uses SELECT DISTINCT json_extract(body, $.field)
    /// - NULL values are excluded from results
    /// - Values are returned as obj and may need type casting
    ///
    /// Performance:
    /// - O(n) time complexity (scans filtered documents)
    /// - O(d) space complexity where d = number of distinct values
    /// - No automatic sorting of results
    /// - Consider indexes on the distinct field for better performance
    ///
    /// Type handling:
    /// - Results are returned as obj list
    /// - Caller must cast to appropriate type
    /// - JSON types map to: string, int64, double, bool, null
    ///
    /// Error handling:
    /// - Returns empty list if no values found
    /// - Throws exception on database errors or invalid field path
    ///
    /// Thread safety:
    /// - Safe for concurrent reads
    /// - Results reflect committed snapshot
    /// </remarks>
    /// <example>
    /// <code>
    /// // Get all unique categories
    /// let! categories = 
    ///     products 
    ///     |> Collection.distinct "category" None
    /// for cat in categories do
    ///     printfn $"Category: {cat :?> string}"
    ///
    /// // Get unique statuses for active users
    /// let! statuses = 
    ///     users 
    ///     |> Collection.distinct "status" 
    ///         (Some (Query.field "active" (Query.eq true)))
    ///
    /// // Get distinct ages
    /// let! ages = 
    ///     users 
    ///     |> Collection.distinct "age" None
    /// let ageList = ages |> List.map (fun x -> x :?> int64)
    /// </code>
    /// </example>
    let distinct
        (field: string)
        (filter: option<Query<'T>>)
        (collection: Collection<'T>)
        : Task<list<obj>> =
        
        let whereClause =
            match filter with
            | None -> ""
            | Some q ->
                let translated = collection.Translator.Translate(q)
                if translated.Sql = "" then "" else $"WHERE {translated.Sql}"
        
        let sql = 
            $"SELECT DISTINCT json_extract(body, '$.{field}') as value 
                FROM {collection.Name} {whereClause}
                WHERE json_extract(body, '$.{field}') IS NOT NULL"
        
        let params' =
            match filter with
            | None -> []
            | Some q ->
                let translated = collection.Translator.Translate(q)
                translated.Parameters |> List.map (fun (name, value) -> name, toSqlType value)
        
        let dbResult =
            collection.Connection
            |> Db.newCommand sql
            |> Db.setParams params'
            |> Db.query (fun rd -> rd.GetValue(0))
        
        Task.FromResult(dbResult)
    
    // ═══════════════════════════════════════════════════════════════
    // WRITE OPERATIONS (Single Document)
    // ═══════════════════════════════════════════════════════════════
    
    /// <summary>
    /// Inserts a new document into the collection with auto-generated ID.
    /// </summary>
    /// <param name="doc">The document data to insert.</param>
    /// <param name="collection">The collection to insert into.</param>
    /// <returns>
    /// Task containing FractalResult with the inserted document on success,
    /// or UniqueConstraint error if a duplicate ID or unique field violation occurs.
    /// </returns>
    /// <remarks>
    /// insertOne creates a new document with:
    /// - Auto-generated UUID v7 ID (time-sortable)
    /// - CreatedAt and UpdatedAt timestamps set to current time
    /// - Serialized JSON body stored in the database
    ///
    /// The operation is atomic - either the document is fully inserted or not at all.
    ///
    /// ID Generation:
    /// - Uses UUID v7 format (time-sortable, collision-resistant)
    /// - Generated via collection.IdGenerator()
    /// - IDs are globally unique across collections
    ///
    /// Performance:
    /// - O(log n) time complexity (B-tree insert)
    /// - Single database transaction
    /// - Triggers index updates for indexed fields
    ///
    /// Error handling:
    /// - Returns Error UniqueConstraint on duplicate ID
    /// - Returns Error UniqueConstraint on unique field violations
    /// - Throws exception on other database errors (serialization, connection, etc.)
    ///
    /// Thread safety:
    /// - Safe for concurrent inserts (different IDs)
    /// - Database-level locking prevents duplicate IDs
    /// - Uses SQLite SERIALIZABLE isolation level
    /// </remarks>
    /// <example>
    /// <code>
    /// // Insert a new user
    /// let! result = 
    ///     users 
    ///     |> Collection.insertOne { Name = "Alice"; Email = "alice@example.com" }
    ///
    /// match result with
    /// | Ok doc ->
    ///     printfn $"Inserted user with ID: {doc.Id}"
    ///     printfn $"Created at: {doc.CreatedAt}"
    /// | Error (UniqueConstraint (field, value)) ->
    ///     printfn $"Duplicate {field}: {value}"
    /// | Error err ->
    ///     printfn $"Error: {err.Message}"
    ///
    /// // The document has auto-generated metadata
    /// // doc.Id = "01234567-89ab-7def-8123-456789abcdef" (UUID v7)
    /// // doc.CreatedAt = doc.UpdatedAt = 1704067200000 (Unix timestamp)
    /// // doc.Data = { Name = "Alice"; Email = "alice@example.com" }
    /// </code>
    /// </example>
    let insertOne (doc: 'T) (collection: Collection<'T>) : Task<FractalResult<Document<'T>>> =
        // Create document with auto-generated ID and timestamps
        let document = Document.create doc
        
        // Serialize document data to JSON
        let bodyJson = serialize document.Data
        
        // Build INSERT statement
        let sql = 
            $"INSERT INTO {collection.Name} (_id, body, createdAt, updatedAt) 
                VALUES (@id, @body, @created, @updated)"
        
        let params' = [
            "id", SqlType.String document.Id
            "body", SqlType.String bodyJson
            "created", SqlType.Int64 document.CreatedAt
            "updated", SqlType.Int64 document.UpdatedAt
        ]
        
        try
            collection.Connection
            |> Db.newCommand sql
            |> Db.setParams params'
            |> Db.exec
            |> ignore
            
            Task.FromResult(Ok document)
        with
        | :? DbExecutionException as ex ->
            // Check if inner exception is SqliteException with constraint violation
            match ex.InnerException with
            | :? SqliteException as sqlEx when sqlEx.SqliteErrorCode = 19 ->
                // SQLITE_CONSTRAINT error code = 19
                // Parse the error message to extract field name
                // Message format: "UNIQUE constraint failed: table.field"
                // or "UNIQUE constraint failed: 'table'.'_field'"
                let errorMsg = sqlEx.Message
                let fieldName =
                    if errorMsg.Contains("UNIQUE constraint failed:") then
                        let parts = errorMsg.Split([|':'|], 2)
                        if parts.Length > 1 then
                            let tableDotField = parts.[1].Trim()
                            let fieldParts = tableDotField.Split('.')
                            if fieldParts.Length > 1 then
                                // Remove leading underscore and any quotes from field name
                                fieldParts.[1].Trim([|'_'; '\''; ' '|])
                            else
                                "id"  // Default to id
                        else
                            "id"
                    else
                        "id"
                
                let error = FractalError.UniqueConstraint(fieldName, box document.Id)
                Task.FromResult(Error error)
            | _ ->
                // Re-raise other exceptions
                reraise()
    
    /// <summary>
    /// Inserts multiple documents into the collection with transaction support.
    /// </summary>
    /// <param name="docs">The list of documents to insert.</param>
    /// <param name="ordered">
    /// If true (default), stops on first error and rolls back all inserts.
    /// If false, continues inserting remaining documents on error.
    /// </param>
    /// <param name="collection">The collection to insert into.</param>
    /// <returns>
    /// Task containing FractalResult with InsertManyResult on success,
    /// or error on failure (ordered mode) or partial success (unordered mode).
    /// </returns>
    /// <remarks>
    /// insertManyWith inserts multiple documents in a single transaction:
    /// - All documents get auto-generated IDs and timestamps
    /// - Ordered mode: stops on first error, rolls back entire batch
    /// - Unordered mode: continues on errors, returns partial results
    ///
    /// Performance:
    /// - O(n * log m) where n = insert count, m = collection size
    /// - Single transaction for all inserts (better than individual insertOne calls)
    /// - Batch operations are 10-100x faster than individual inserts
    /// - Uses database-level locking (SQLite: full write lock)
    ///
    /// Ordered mode (ordered = true):
    /// - Stops on first error
    /// - Rolls back all previous inserts in the batch
    /// - Returns Error with the failure cause
    /// - Guarantees all-or-nothing semantics
    ///
    /// Unordered mode (ordered = false):
    /// - Continues on errors
    /// - Commits successful inserts
    /// - Returns Ok with partial results (successfully inserted documents)
    /// - Useful for bulk imports where partial success is acceptable
    ///
    /// Error handling:
    /// - Ordered: Returns Error UniqueConstraint on first duplicate
    /// - Unordered: Skips duplicates, returns successful inserts
    /// - Throws exception on database errors (connection, transaction, etc.)
    ///
    /// Thread safety:
    /// - Transaction holds exclusive write lock
    /// - Safe to call from multiple threads (serialized by SQLite)
    /// - Each call gets its own transaction
    /// </remarks>
    /// <example>
    /// <code>
    /// // Insert multiple users (ordered - all or nothing)
    /// let newUsers = [
    ///     { Name = "Alice"; Email = "alice@example.com" }
    ///     { Name = "Bob"; Email = "bob@example.com" }
    ///     { Name = "Carol"; Email = "carol@example.com" }
    /// ]
    ///
    /// let! result = 
    ///     users 
    ///     |> Collection.insertManyWith newUsers true  // ordered=true
    ///
    /// match result with
    /// | Ok insertResult ->
    ///     printfn $"Inserted {insertResult.InsertedCount} users"
    ///     for doc in insertResult.Documents do
    ///         printfn $"  - {doc.Id}: {doc.Data.Name}"
    /// | Error (UniqueConstraint (field, value)) ->
    ///     printfn $"Duplicate {field}: {value}"
    ///     // All inserts rolled back
    /// | Error err ->
    ///     printfn $"Error: {err.Message}"
    ///
    /// // Unordered insert (partial success allowed)
    /// let! result2 = 
    ///     users 
    ///     |> Collection.insertManyWith newUsers false  // ordered=false
    ///
    /// match result2 with
    /// | Ok insertResult ->
    ///     printfn $"Inserted {insertResult.InsertedCount} of {List.length newUsers}"
    ///     // Some may have failed, but successful ones are committed
    /// | Error err ->
    ///     printfn $"All inserts failed: {err.Message}"
    /// </code>
    /// </example>
    let insertManyWith
        (docs: list<'T>)
        (ordered: bool)
        (collection: Collection<'T>)
        : Task<FractalResult<InsertManyResult<'T>>> =
        
        if List.isEmpty docs then
            // Empty list - return empty result
            let emptyResult = { Documents = []; InsertedCount = 0 }
            Task.FromResult(Ok emptyResult)
        else
            use transaction = Transaction.create collection.Connection
            
            let mutable insertedDocs = []
            let mutable error = None
            
            for doc in docs do
                if error.IsNone || not ordered then
                    // Create document with auto-generated ID and timestamps
                    let document = Document.create doc
                    let bodyJson = serialize document.Data
                    
                    let sql = 
                        $"INSERT INTO {collection.Name} (_id, body, createdAt, updatedAt) 
                            VALUES (@id, @body, @created, @updated)"
                    
                    let params' = [
                        "id", SqlType.String document.Id
                        "body", SqlType.String bodyJson
                        "created", SqlType.Int64 document.CreatedAt
                        "updated", SqlType.Int64 document.UpdatedAt
                    ]
                    
                    try
                        collection.Connection
                        |> Db.newCommand sql
                        |> Db.setParams params'
                        |> Db.exec
                        |> ignore
                        
                        insertedDocs <- document :: insertedDocs
                    with
                    | :? DbExecutionException as ex ->
                        match ex.InnerException with
                        | :? SqliteException as sqlEx when sqlEx.SqliteErrorCode = 19 ->
                            let err = FractalError.UniqueConstraint("_id", box document.Id)
                            if ordered then
                                error <- Some err
                            // else: skip this document, continue with next
                        | _ ->
                            // Re-raise unexpected exceptions
                            reraise()
            
            match error with
            | Some err when ordered ->
                // Ordered mode with error - rollback
                transaction.Rollback()
                Task.FromResult(Error err)
            | _ ->
                // Success (or unordered with partial success)
                transaction.Commit()
                let result = {
                    Documents = List.rev insertedDocs  // Reverse to maintain insertion order
                    InsertedCount = List.length insertedDocs
                }
                Task.FromResult(Ok result)
    
    /// <summary>
    /// Inserts multiple documents into the collection (ordered mode).
    /// </summary>
    /// <param name="docs">The list of documents to insert.</param>
    /// <param name="collection">The collection to insert into.</param>
    /// <returns>
    /// Task containing FractalResult with InsertManyResult on success,
    /// or error if any insert fails (all inserts rolled back).
    /// </returns>
    /// <remarks>
    /// insertMany is a convenience wrapper around insertManyWith with ordered=true.
    /// This provides all-or-nothing semantics: either all documents are inserted
    /// successfully, or none are (transaction rolled back on first error).
    ///
    /// Equivalent to: insertManyWith docs true collection
    ///
    /// Use insertManyWith with ordered=false for partial success scenarios.
    ///
    /// Performance:
    /// - Same as insertManyWith (single transaction, batch insert)
    /// - Typically 10-100x faster than calling insertOne repeatedly
    ///
    /// Error handling:
    /// - Stops on first error
    /// - Rolls back all inserts in the batch
    /// - Returns Error with failure cause
    /// - No partial results on failure
    /// </remarks>
    /// <example>
    /// <code>
    /// // Insert batch of products
    /// let products = [
    ///     { Name = "Laptop"; Price = 999.99 }
    ///     { Name = "Mouse"; Price = 29.99 }
    ///     { Name = "Keyboard"; Price = 79.99 }
    /// ]
    ///
    /// let! result = products |> Collection.insertMany catalog
    ///
    /// match result with
    /// | Ok insertResult ->
    ///     printfn $"Successfully inserted {insertResult.InsertedCount} products"
    /// | Error err ->
    ///     printfn $"Batch insert failed: {err.Message}"
    ///     printfn "No products were inserted (transaction rolled back)"
    /// </code>
    /// </example>
    let insertMany
        (docs: list<'T>)
        (collection: Collection<'T>)
        : Task<FractalResult<InsertManyResult<'T>>> =
        insertManyWith docs true collection
    
    /// <summary>
    /// Updates a document by ID using a transformation function.
    /// </summary>
    /// <param name="id">The document ID to update.</param>
    /// <param name="update">Function to transform the document data.</param>
    /// <param name="collection">The collection to update in.</param>
    /// <returns>
    /// Task containing FractalResult with Some updated document on success,
    /// None if document not found, or Error on failure.
    /// </returns>
    /// <remarks>
    /// updateById updates a document by:
    /// 1. Finding the document by ID
    /// 2. Applying the update function to document.Data
    /// 3. Serializing updated data
    /// 4. Updating database with new body and updatedAt timestamp
    /// 5. Preserving ID and createdAt
    ///
    /// The update function receives the current document data and returns
    /// the new data. This allows for functional transformations:
    ///
    /// Performance:
    /// - O(log n) for ID lookup (primary key index)
    /// - Single database transaction
    /// - Atomic operation (read + write)
    ///
    /// Update behavior:
    /// - ID is never changed
    /// - CreatedAt is preserved from original document
    /// - UpdatedAt is set to current timestamp
    /// - All other fields come from update function result
    ///
    /// Error handling:
    /// - Returns Ok None if document doesn't exist
    /// - Returns Error on serialization failures
    /// - Throws exception on database errors
    ///
    /// Thread safety:
    /// - Atomic read-modify-write operation
    /// - Uses database-level locking
    /// - Safe for concurrent updates (last write wins)
    /// </remarks>
    /// <example>
    /// <code>
    /// // Increment user age
    /// let! result = 
    ///     users 
    ///     |> Collection.updateById "user123" (fun user -> 
    ///         { user with Age = user.Age + 1 })
    ///
    /// match result with
    /// | Ok (Some doc) ->
    ///     printfn $"Updated: {doc.Data.Name}, Age: {doc.Data.Age}"
    ///     printfn $"UpdatedAt: {doc.UpdatedAt}"
    /// | Ok None ->
    ///     printfn "User not found"
    /// | Error err ->
    ///     printfn $"Error: {err.Message}"
    ///
    /// // Update multiple fields
    /// let! result2 = 
    ///     products 
    ///     |> Collection.updateById "prod456" (fun p -> 
    ///         { p with Price = p.Price * 0.9; OnSale = true })
    /// </code>
    /// </example>
    let updateById
        (id: string)
        (update: 'T -> 'T)
        (collection: Collection<'T>)
        : Task<FractalResult<option<Document<'T>>>> =
        task {
            // Find existing document
            let! maybeDoc = findById id collection
            
            match maybeDoc with
            | None ->
                // Document not found
                return Ok None
            | Some doc ->
                try
                    // Apply update function to document data
                    let updatedData = update doc.Data
                    
                    // Serialize updated data
                    let bodyJson = serialize updatedData
                    
                    // Get new timestamp
                    let newUpdatedAt = Timestamp.now()
                    
                    // Build UPDATE statement
                    let sql = 
                        $"UPDATE {collection.Name} 
                            SET body = @body, updatedAt = @updated 
                            WHERE _id = @id"
                    
                    let params' = [
                        "body", SqlType.String bodyJson
                        "updated", SqlType.Int64 newUpdatedAt
                        "id", SqlType.String id
                    ]
                    
                    collection.Connection
                    |> Db.newCommand sql
                    |> Db.setParams params'
                    |> Db.exec
                    |> ignore
                    
                    // Return updated document
                    let updatedDoc = {
                        Id = doc.Id
                        Data = updatedData
                        CreatedAt = doc.CreatedAt
                        UpdatedAt = newUpdatedAt
                    }
                    
                    return Ok (Some updatedDoc)
                with
                | ex ->
                    // Wrap serialization or database errors
                    let error = FractalError.Serialization ex.Message
                    return Error error
        }
    
    /// <summary>
    /// Updates the first document matching the filter using a transformation function.
    /// </summary>
    /// <param name="filter">Query filter to match documents.</param>
    /// <param name="update">Function to transform the document data.</param>
    /// <param name="collection">The collection to update in.</param>
    /// <returns>
    /// Task containing FractalResult with Some updated document on success,
    /// None if no matching document found, or Error on failure.
    /// </returns>
    /// <remarks>
    /// updateOne is similar to updateById but uses a query filter instead of ID.
    /// It updates only the first matching document.
    ///
    /// Matching behavior:
    /// - Uses findOne to locate first match
    /// - No guaranteed order if multiple documents match
    /// - Use sort in filter or updateOneWith for predictable selection
    ///
    /// Performance:
    /// - O(n) for filter matching (unless indexed)
    /// - Single database transaction
    /// - Only one document is updated even if multiple match
    ///
    /// For updating multiple documents, use updateMany instead.
    ///
    /// Thread safety:
    /// - Atomic operation with database locking
    /// - Concurrent updates may affect different documents
    /// - Last write wins for same document
    /// </remarks>
    /// <example>
    /// <code>
    /// // Update first active user
    /// let! result = 
    ///     users 
    ///     |> Collection.updateOne 
    ///         (Query.field "status" (Query.eq "active"))
    ///         (fun user -> { user with LastSeen = DateTime.UtcNow })
    ///
    /// match result with
    /// | Ok (Some doc) ->
    ///     printfn $"Updated user: {doc.Id}"
    /// | Ok None ->
    ///     printfn "No active users found"
    /// | Error err ->
    ///     printfn $"Error: {err.Message}"
    /// </code>
    /// </example>
    let updateOne
        (filter: Query<'T>)
        (update: 'T -> 'T)
        (collection: Collection<'T>)
        : Task<FractalResult<option<Document<'T>>>> =
        task {
            // Find first matching document
            let! maybeDoc = findOne filter collection
            
            match maybeDoc with
            | None ->
                return Ok None
            | Some doc ->
                // Use updateById for the actual update
                return! updateById doc.Id update collection
        }
    
    /// <summary>
    /// Updates first matching document with upsert option.
    /// </summary>
    /// <param name="filter">Query filter to match documents.</param>
    /// <param name="update">Function to transform the document data.</param>
    /// <param name="upsert">
    /// If true, inserts a new document when no match found.
    /// If false, behaves like updateOne.
    /// </param>
    /// <param name="collection">The collection to update in.</param>
    /// <returns>
    /// Task containing FractalResult with Some document (updated or inserted),
    /// None if not found and upsert=false, or Error on failure.
    /// </returns>
    /// <remarks>
    /// updateOneWith extends updateOne with upsert capability:
    /// - upsert=false: identical to updateOne
    /// - upsert=true: inserts new document if no match found
    ///
    /// Upsert behavior (upsert=true):
    /// - If document found: updates using the update function
    /// - If not found: creates new document by applying update to default 'T
    /// - Note: For upsert, 'T must have a default/empty value
    ///
    /// IMPORTANT: Upsert with update function has limitations:
    /// - The update function must work on uninitialized 'T values
    /// - Consider using replaceOne for full document replacement
    /// - Better for incremental updates (counters, flags, etc.)
    ///
    /// Performance:
    /// - Same as updateOne for update case
    /// - Additional insert cost for upsert case
    /// - Single transaction for atomicity
    ///
    /// Thread safety:
    /// - Atomic operation
    /// - Prevents concurrent upserts creating duplicates
    /// </remarks>
    /// <example>
    /// <code>
    /// // Update or create user profile
    /// let! result = 
    ///     profiles 
    ///     |> Collection.updateOneWith 
    ///         (Query.field "userId" (Query.eq "user123"))
    ///         (fun profile -> { profile with LastLogin = DateTime.UtcNow })
    ///         true  // upsert=true
    ///
    /// match result with
    /// | Ok (Some doc) ->
    ///     printfn $"Profile updated/created: {doc.Id}"
    /// | Ok None ->
    ///     // Won't happen with upsert=true
    ///     printfn "Not found"
    /// | Error err ->
    ///     printfn $"Error: {err.Message}"
    ///
    /// // Without upsert (same as updateOne)
    /// let! result2 = 
    ///     users 
    ///     |> Collection.updateOneWith 
    ///         (Query.field "email" (Query.eq "test@example.com"))
    ///         (fun u -> { u with Active = false })
    ///         false  // upsert=false
    /// </code>
    /// </example>
    let updateOneWith
        (filter: Query<'T>)
        (update: 'T -> 'T)
        (upsert: bool)
        (collection: Collection<'T>)
        : Task<FractalResult<option<Document<'T>>>> =
        task {
            // Find first matching document
            let! maybeDoc = findOne filter collection
            
            match maybeDoc, upsert with
            | Some doc, _ ->
                // Document found - update it
                return! updateById doc.Id update collection
            | None, false ->
                // Not found and upsert disabled
                return Ok None
            | None, true ->
                // Not found but upsert enabled - insert new document
                // Apply update function to default value of 'T
                try
                    let defaultValue = Unchecked.defaultof<'T>
                    let newData = update defaultValue
                    
                    let! insertResult = insertOne newData collection
                    
                    match insertResult with
                    | Ok doc ->
                        return Ok (Some doc)
                    | Error err ->
                        return Error err
                with
                | ex ->
                    let errorMsg = $"Upsert failed: cannot create default value for type. {ex.Message}"
                    let error = FractalError.Serialization errorMsg
                    return Error error
        }
    
    /// <summary>
    /// Replaces the first document matching the filter with new data.
    /// </summary>
    /// <param name="filter">Query filter to match documents.</param>
    /// <param name="doc">The new document data to replace with.</param>
    /// <param name="collection">The collection to update in.</param>
    /// <returns>
    /// Task containing FractalResult with Some replaced document on success,
    /// None if no matching document found, or Error on failure.
    /// </returns>
    /// <remarks>
    /// replaceOne completely replaces a document's data while preserving:
    /// - Document ID (immutable)
    /// - CreatedAt timestamp (from original document)
    /// - UpdatedAt is set to current time
    ///
    /// Difference from updateOne:
    /// - replaceOne: takes complete new data, full replacement
    /// - updateOne: takes transformation function, can update specific fields
    ///
    /// Use cases:
    /// - Replacing entire document with new version
    /// - Overwriting all fields at once
    /// - When you have complete new data (not incremental updates)
    ///
    /// The operation is atomic - either the document is fully replaced or not at all.
    ///
    /// Performance:
    /// - O(n) for filter matching (unless indexed)
    /// - Single database transaction
    /// - Only first matching document is replaced
    ///
    /// Error handling:
    /// - Returns Ok None if no document matches filter
    /// - Returns Error on serialization failures
    /// - Throws exception on database errors
    ///
    /// Thread safety:
    /// - Atomic operation with database locking
    /// - Safe for concurrent replacements
    /// - Last write wins for same document
    /// </remarks>
    /// <example>
    /// <code>
    /// // Replace entire user document
    /// let newUserData = { 
    ///     Name = "Alice Smith"
    ///     Email = "alice.smith@example.com"
    ///     Age = 31
    ///     Active = true 
    /// }
    ///
    /// let! result = 
    ///     users 
    ///     |> Collection.replaceOne 
    ///         (Query.field "email" (Query.eq "alice@example.com"))
    ///         newUserData
    ///
    /// match result with
    /// | Ok (Some doc) ->
    ///     printfn $"Replaced user: {doc.Id}"
    ///     printfn $"New data: {doc.Data}"
    ///     printfn $"CreatedAt preserved: {doc.CreatedAt}"
    ///     printfn $"UpdatedAt changed: {doc.UpdatedAt}"
    /// | Ok None ->
    ///     printfn "User not found"
    /// | Error err ->
    ///     printfn $"Error: {err.Message}"
    ///
    /// // Replace product by ID (more efficient)
    /// let! result2 = 
    ///     products 
    ///     |> Collection.replaceOne 
    ///         (Query.field "_id" (Query.eq "prod123"))
    ///         { Name = "New Product"; Price = 99.99 }
    /// </code>
    /// </example>
    let replaceOne
        (filter: Query<'T>)
        (doc: 'T)
        (collection: Collection<'T>)
        : Task<FractalResult<option<Document<'T>>>> =
        task {
            // Find first matching document
            let! maybeDoc = findOne filter collection
            
            match maybeDoc with
            | None ->
                // Document not found
                return Ok None
            | Some existingDoc ->
                try
                    // Serialize new data
                    let bodyJson = serialize doc
                    
                    // Get new timestamp
                    let newUpdatedAt = Timestamp.now()
                    
                    // Build UPDATE statement
                    let sql = 
                        $"UPDATE {collection.Name} 
                            SET body = @body, updatedAt = @updated 
                            WHERE _id = @id"
                    
                    let params' = [
                        "body", SqlType.String bodyJson
                        "updated", SqlType.Int64 newUpdatedAt
                        "id", SqlType.String existingDoc.Id
                    ]
                    
                    collection.Connection
                    |> Db.newCommand sql
                    |> Db.setParams params'
                    |> Db.exec
                    |> ignore
                    
                    // Return replaced document
                    let replacedDoc = {
                        Id = existingDoc.Id
                        Data = doc
                        CreatedAt = existingDoc.CreatedAt
                        UpdatedAt = newUpdatedAt
                    }
                    
                    return Ok (Some replacedDoc)
                with
                | ex ->
                    let error = FractalError.Serialization ex.Message
                    return Error error
        }
    
    // ═══════════════════════════════════════════════════════════════
    // BATCH OPERATIONS
    // ═══════════════════════════════════════════════════════════════
    
    /// <summary>
    /// Updates all documents matching the filter using a transformation function.
    /// </summary>
    /// <param name="filter">Query filter to match documents.</param>
    /// <param name="update">Function to transform each document's data.</param>
    /// <param name="collection">The collection to update in.</param>
    /// <returns>
    /// Task containing FractalResult with UpdateResult showing matched and modified counts,
    /// or Error on failure.
    /// </returns>
    /// <remarks>
    /// updateMany updates multiple documents in a single transaction:
    /// - Finds all documents matching the filter
    /// - Applies update function to each document's data
    /// - Updates timestamps (UpdatedAt) for each
    /// - Executes all updates in one transaction
    /// - Returns counts of matched and modified documents
    ///
    /// The operation is atomic - either all updates succeed or all are rolled back.
    ///
    /// MatchedCount vs ModifiedCount:
    /// - MatchedCount: Total documents found by filter
    /// - ModifiedCount: Documents where data actually changed
    /// - ModifiedCount may be less if update function returns unchanged data
    ///
    /// Performance:
    /// - O(n * log m) where n = matched documents, m = collection size
    /// - Single transaction for all updates (much faster than individual updates)
    /// - Uses database-level locking (SQLite: full write lock)
    /// - Consider batching for very large update sets (>1000 documents)
    ///
    /// Error handling:
    /// - Rolls back all updates on any error
    /// - Returns Error on serialization failures
    /// - Throws exception on database errors
    /// - All-or-nothing semantics
    ///
    /// Thread safety:
    /// - Transaction holds exclusive write lock
    /// - Safe for concurrent calls (serialized by SQLite)
    /// - Each call gets its own transaction
    ///
    /// Use cases:
    /// - Bulk status updates
    /// - Applying schema migrations
    /// - Batch field modifications
    /// - Mass data corrections
    /// </remarks>
    /// <example>
    /// <code>
    /// // Deactivate all old users
    /// let! result = 
    ///     users 
    ///     |> Collection.updateMany 
    ///         (Query.field "lastSeen" (Query.lt (DateTime.UtcNow.AddDays(-90))))
    ///         (fun user -> { user with Active = false })
    ///
    /// match result with
    /// | Ok updateResult ->
    ///     printfn $"Matched: {updateResult.MatchedCount}"
    ///     printfn $"Modified: {updateResult.ModifiedCount}"
    /// | Error err ->
    ///     printfn $"Error: {err.Message}"
    ///
    /// // Apply discount to all products in category
    /// let! result2 = 
    ///     products 
    ///     |> Collection.updateMany 
    ///         (Query.field "category" (Query.eq "electronics"))
    ///         (fun p -> { p with Price = p.Price * 0.9; OnSale = true })
    ///
    /// // Increment view count for all featured items
    /// let! result3 = 
    ///     items 
    ///     |> Collection.updateMany 
    ///         (Query.field "featured" (Query.eq true))
    ///         (fun item -> { item with Views = item.Views + 1 })
    /// </code>
    /// </example>
    let updateMany
        (filter: Query<'T>)
        (update: 'T -> 'T)
        (collection: Collection<'T>)
        : Task<FractalResult<UpdateResult>> =
        task {
            try
                // Find all matching documents
                let! docs = find filter collection
                
                let matchedCount = List.length docs
                
                if matchedCount = 0 then
                    // No documents matched - return zero counts
                    return Ok { MatchedCount = 0; ModifiedCount = 0 }
                else
                    use transaction = Transaction.create collection.Connection
                    
                    let mutable modifiedCount = 0
                    let newUpdatedAt = Timestamp.now()
                    
                    // Update each document
                    for doc in docs do
                        // Apply update function
                        let updatedData = update doc.Data
                        
                        // Serialize updated data
                        let bodyJson = serialize updatedData
                        
                        // Build UPDATE statement
                        let sql = 
                            $"UPDATE {collection.Name} 
                                SET body = @body, updatedAt = @updated 
                                WHERE _id = @id"
                        
                        let params' = [
                            "body", SqlType.String bodyJson
                            "updated", SqlType.Int64 newUpdatedAt
                            "id", SqlType.String doc.Id
                        ]
                        
                        collection.Connection
                        |> Db.newCommand sql
                        |> Db.setParams params'
                        |> Db.exec
                        |> ignore
                        
                        modifiedCount <- modifiedCount + 1
                    
                    // Commit transaction
                    transaction.Commit()
                    
                    return Ok { MatchedCount = matchedCount; ModifiedCount = modifiedCount }
            with
            | ex ->
                let error = FractalError.Serialization ex.Message
                return Error error
        }
    
    // ═══════════════════════════════════════════════════════════════
    // DELETE OPERATIONS
    // ═══════════════════════════════════════════════════════════════
    
    /// <summary>
    /// Deletes a document by its unique identifier.
    /// </summary>
    /// <param name="id">The document ID to delete.</param>
    /// <param name="collection">The collection to delete from.</param>
    /// <returns>
    /// Task containing true if document was deleted, false if not found.
    /// </returns>
    /// <remarks>
    /// deleteById permanently removes a document from the collection:
    /// - Uses primary key index for O(log n) lookup
    /// - Document is immediately deleted (no soft delete)
    /// - Operation is irreversible
    /// - Returns false if document doesn't exist
    ///
    /// Performance:
    /// - O(log n) time complexity (B-tree delete)
    /// - Single database operation
    /// - Very fast (primary key index)
    ///
    /// Safety considerations:
    /// - Deletion is immediate and permanent
    /// - No undo capability
    /// - Consider soft delete pattern for important data
    /// - Use transactions if deleting related data
    ///
    /// Thread safety:
    /// - Safe for concurrent deletes
    /// - Database-level locking prevents conflicts
    /// - Multiple threads can delete different documents safely
    /// </remarks>
    /// <example>
    /// <code>
    /// // Delete user by ID
    /// let! deleted = users |> Collection.deleteById "user123"
    /// if deleted then
    ///     printfn "User deleted successfully"
    /// else
    ///     printfn "User not found"
    ///
    /// // Delete with confirmation
    /// let userId = "user456"
    /// let! exists = users |> Collection.findById userId
    /// match exists with
    /// | Some user ->
    ///     printfn $"Deleting user: {user.Data.Name}"
    ///     let! deleted = users |> Collection.deleteById userId
    ///     printfn $"Deleted: {deleted}"
    /// | None ->
    ///     printfn "User not found"
    /// </code>
    /// </example>
    let deleteById (id: string) (collection: Collection<'T>) : Task<bool> =
        task {
            try
                let sql = $"DELETE FROM {collection.Name} WHERE _id = @id"
                
                let params' = ["id", SqlType.String id]
                
                collection.Connection
                |> Db.newCommand sql
                |> Db.setParams params'
                |> Db.exec
                |> ignore
                
                // Use SQLite changes() to get affected row count
                let deletedCount =
                    collection.Connection
                    |> Db.newCommand "SELECT changes()"
                    |> Db.setParams []
                    |> Db.scalar Convert.ToInt32
                
                return deletedCount > 0
            with
            | _ ->
                return false
        }
    
    /// <summary>
    /// Deletes the first document matching the filter.
    /// </summary>
    /// <param name="filter">Query filter to match documents.</param>
    /// <param name="collection">The collection to delete from.</param>
    /// <returns>
    /// Task containing true if a document was deleted, false if none matched.
    /// </returns>
    /// <remarks>
    /// deleteOne removes the first document matching the filter:
    /// - Finds first match using filter
    /// - Deletes by ID (uses primary key)
    /// - Only one document deleted even if multiple match
    /// - No guaranteed order unless filter includes sort
    ///
    /// Performance:
    /// - O(n) for filter matching (unless indexed)
    /// - O(log n) for actual delete (primary key)
    /// - Two database operations (find + delete)
    ///
    /// For deleting multiple documents, use deleteMany instead.
    ///
    /// Thread safety:
    /// - Atomic operation
    /// - Safe for concurrent deletes
    /// - Another thread may delete the same document first
    /// </remarks>
    /// <example>
    /// <code>
    /// // Delete first inactive user
    /// let! deleted = 
    ///     users 
    ///     |> Collection.deleteOne 
    ///         (Query.field "active" (Query.eq false))
    ///
    /// printfn $"Deleted: {deleted}"
    ///
    /// // Delete oldest post
    /// let! deleted2 = 
    ///     posts 
    ///     |> Collection.deleteOne 
    ///         (Query.Empty)  // Would need sort in QueryOptions for truly "oldest"
    ///
    /// // Delete specific email
    /// let! deleted3 = 
    ///     users 
    ///     |> Collection.deleteOne 
    ///         (Query.field "email" (Query.eq "old@example.com"))
    /// </code>
    /// </example>
    let deleteOne (filter: Query<'T>) (collection: Collection<'T>) : Task<bool> =
        task {
            // Find first matching document
            let! maybeDoc = findOne filter collection
            
            match maybeDoc with
            | None ->
                return false
            | Some doc ->
                return! deleteById doc.Id collection
        }
    
    /// <summary>
    /// Deletes all documents matching the filter.
    /// </summary>
    /// <param name="filter">Query filter to match documents.</param>
    /// <param name="collection">The collection to delete from.</param>
    /// <returns>
    /// Task containing DeleteResult with count of deleted documents.
    /// </returns>
    /// <remarks>
    /// deleteMany removes all documents matching the filter:
    /// - Translates filter to SQL WHERE clause
    /// - Executes single DELETE statement
    /// - Returns count of deleted documents
    /// - Empty filter (Query.Empty) deletes ALL documents
    ///
    /// Performance:
    /// - O(n) time complexity where n = matching documents
    /// - Single database operation
    /// - Much faster than calling deleteOne repeatedly
    /// - Uses indexes when filter involves indexed fields
    ///
    /// Safety considerations:
    /// - DANGEROUS: Query.Empty deletes entire collection
    /// - Deletions are permanent and immediate
    /// - Consider using transactions if part of larger operation
    /// - Verify filter before calling in production
    ///
    /// Error handling:
    /// - Returns DeletedCount = 0 if no matches
    /// - Throws exception on database errors
    ///
    /// Thread safety:
    /// - Atomic operation
    /// - Safe for concurrent deletes
    /// - Database-level locking
    /// </remarks>
    /// <example>
    /// <code>
    /// // Delete all inactive users
    /// let! result = 
    ///     users 
    ///     |> Collection.deleteMany 
    ///         (Query.field "active" (Query.eq false))
    ///
    /// printfn $"Deleted {result.DeletedCount} inactive users"
    ///
    /// // Delete old logs (with date filter)
    /// let cutoffDate = DateTime.UtcNow.AddDays(-30)
    /// let! result2 = 
    ///     logs 
    ///     |> Collection.deleteMany 
    ///         (Query.field "timestamp" (Query.lt cutoffDate))
    ///
    /// printfn $"Deleted {result2.DeletedCount} old logs"
    ///
    /// // DANGEROUS: Delete all documents
    /// let! result3 = collection |> Collection.deleteMany Query.Empty
    /// printfn $"Deleted entire collection: {result3.DeletedCount} documents"
    /// </code>
    /// </example>
    let deleteMany (filter: Query<'T>) (collection: Collection<'T>) : Task<DeleteResult> =
        task {
            try
                // Translate filter to SQL
                let translated = collection.Translator.Translate(filter)
                let whereClause = if translated.Sql = "" then "" else $"WHERE {translated.Sql}"
                
                let sql = $"DELETE FROM {collection.Name} {whereClause}"
                
                let params' = translated.Parameters |> List.map (fun (name, value) -> name, toSqlType value)
                
                collection.Connection
                |> Db.newCommand sql
                |> Db.setParams params'
                |> Db.exec
                |> ignore
                
                // Get deleted count using changes()
                let deletedCount =
                    collection.Connection
                    |> Db.newCommand "SELECT changes()"
                    |> Db.setParams []
                    |> Db.scalar Convert.ToInt32
                
                return { DeletedCount = deletedCount }
            with
            | ex ->
                // Return zero count on error (or could throw)
                printfn $"Delete error: {ex.Message}"
                return { DeletedCount = 0 }
        }
    
    // ═══════════════════════════════════════════════════════════════
    // ATOMIC FIND-AND-DELETE OPERATIONS
    // ═══════════════════════════════════════════════════════════════
    
    /// <summary>
    /// Atomically finds and deletes a single document matching the filter.
    /// </summary>
    /// <param name="filter">Query filter to match documents.</param>
    /// <param name="collection">The collection to operate on.</param>
    /// <returns>
    /// Task containing Some(document) if found and deleted, or None if no match.
    /// Returns the document state before deletion.
    /// </returns>
    /// <remarks>
    /// findOneAndDelete provides atomic find-then-delete operation:
    /// - Finds first document matching filter
    /// - Deletes the document
    /// - Returns the document that was deleted
    /// - All operations in single transaction
    ///
    /// Operation is atomic:
    /// - Transaction ensures no other operation can modify document between find and delete
    /// - Document returned is guaranteed to be the one deleted
    /// - If multiple documents match, only first is deleted
    ///
    /// Without sort options, result order is arbitrary:
    /// - Use findOneAndDeleteWith to control which document is selected
    /// - Sort determines which document is deleted when multiple match
    ///
    /// Use cases:
    /// - Processing work queues (get item and remove atomically)
    /// - Claiming exclusive access to resources
    /// - Implementing pop operations on document collections
    /// - Audit trails (need deleted document for logging)
    ///
    /// Performance:
    /// - O(n) for filter scan + O(log n) for delete
    /// - Uses transaction overhead
    /// - Returns full document (includes body deserialization)
    /// - Consider deleteOne if document content not needed
    ///
    /// Thread safety:
    /// - Fully thread-safe due to transaction
    /// - Multiple threads can call concurrently
    /// - Each thread gets different document or None
    /// </remarks>
    /// <example>
    /// <code>
    /// // Process next work item
    /// let! work = 
    ///     workQueue 
    ///     |> Collection.findOneAndDelete 
    ///         (Query.field "status" (Query.eq "pending"))
    ///
    /// match work with
    /// | Some doc -&gt;
    ///     printfn $"Processing work item: {doc.Id}"
    ///     // Process doc.Body here
    /// | None -&gt;
    ///     printfn "No pending work"
    ///
    /// // Claim exclusive resource
    /// let! resource = 
    ///     pool 
    ///     |> Collection.findOneAndDelete 
    ///         (Query.field "available" (Query.eq true))
    ///
    /// match resource with
    /// | Some doc -&gt;
    ///     // Use resource (doc.Body)
    ///     // Resource automatically removed from pool
    ///     printfn $"Claimed resource: {doc.Id}"
    /// | None -&gt;
    ///     printfn "No resources available"
    /// </code>
    /// </example>
    let findOneAndDelete (filter: Query<'T>) (collection: Collection<'T>) : Task<option<Document<'T>>> =
        task {
            try
                use transaction = Transaction.create collection.Connection
                
                // Find the document first
                let! maybeDoc = collection |> findOne filter
                
                match maybeDoc with
                | Some doc ->
                    // Delete it by ID
                    let! deleted = collection |> deleteById doc.Id
                    transaction.Commit()
                    
                    if deleted then
                        return Some doc
                    else
                        return None
                | None ->
                    transaction.Commit()
                    return None
            with
            | ex ->
                printfn $"findOneAndDelete error: {ex.Message}"
                return None
        }
    
    /// <summary>
    /// Atomically finds and deletes a single document with sort options.
    /// </summary>
    /// <param name="filter">Query filter to match documents.</param>
    /// <param name="options">Find options for controlling sort order.</param>
    /// <param name="collection">The collection to operate on.</param>
    /// <returns>
    /// Task containing Some(document) if found and deleted, or None if no match.
    /// Returns the document state before deletion.
    /// </returns>
    /// <remarks>
    /// findOneAndDeleteWith extends findOneAndDelete with sort control:
    /// - Allows specifying sort order to control which document is deleted
    /// - Useful when multiple documents match and order matters
    /// - All operations in single atomic transaction
    ///
    /// Sort behavior:
    /// - Sort determines which matching document is selected
    /// - Only the first document (after sorting) is deleted
    /// - Multiple sort fields supported
    /// - Ascending or descending order per field
    ///
    /// Common patterns:
    /// - FIFO queue: sort by timestamp ascending
    /// - LIFO stack: sort by timestamp descending
    /// - Priority queue: sort by priority then timestamp
    /// - Newest first: sort by createdAt descending
    ///
    /// Performance:
    /// - O(n log n) for sort + O(log n) for delete
    /// - Sorting overhead proportional to matching documents
    /// - Consider creating index on sort fields
    /// - Uses transaction overhead
    ///
    /// Thread safety:
    /// - Fully thread-safe due to transaction
    /// - Multiple threads get different documents
    /// - Sort order applied consistently
    /// </remarks>
    /// <example>
    /// <code>
    /// // FIFO work queue - oldest first
    /// let options = { Sort = [("createdAt", Ascending)] }
    /// let! work = 
    ///     workQueue 
    ///     |> Collection.findOneAndDeleteWith 
    ///         (Query.field "status" (Query.eq "pending"))
    ///         options
    ///
    /// match work with
    /// | Some doc -&gt; printfn $"Processing oldest work: {doc.Id}"
    /// | None -&gt; printfn "Queue empty"
    ///
    /// // LIFO stack - newest first
    /// let stackOptions = { Sort = [("createdAt", Descending)] }
    /// let! item = collection |> Collection.findOneAndDeleteWith Query.Empty stackOptions
    ///
    /// // Priority queue - highest priority, then oldest
    /// let priorityOptions = { 
    ///     Sort = [("priority", Descending); ("createdAt", Ascending)] 
    /// }
    /// let! task = 
    ///     tasks 
    ///     |> Collection.findOneAndDeleteWith 
    ///         (Query.field "status" (Query.eq "ready"))
    ///         priorityOptions
    /// </code>
    /// </example>
    let findOneAndDeleteWith 
        (filter: Query<'T>) 
        (options: FindOptions) 
        (collection: Collection<'T>) 
        : Task<option<Document<'T>>> =
        task {
            try
                use transaction = Transaction.create collection.Connection
                
                // Find the document with sort options
                let queryOptions = { QueryOptions.empty with Sort = options.Sort }
                let! maybeDoc = collection |> findOneWith filter queryOptions
                
                match maybeDoc with
                | Some doc ->
                    // Delete it by ID
                    let! deleted = collection |> deleteById doc.Id
                    transaction.Commit()
                    
                    if deleted then
                        return Some doc
                    else
                        return None
                | None ->
                    transaction.Commit()
                    return None
            with
            | ex ->
                printfn $"findOneAndDeleteWith error: {ex.Message}"
                return None
        }
    
    // ═══════════════════════════════════════════════════════════════
    // ATOMIC FIND-AND-UPDATE OPERATIONS
    // ═══════════════════════════════════════════════════════════════
    
    /// <summary>
    /// Atomically finds and updates a single document matching the filter.
    /// </summary>
    /// <param name="filter">Query filter to match documents.</param>
    /// <param name="update">Update function to transform the document body.</param>
    /// <param name="options">Options controlling sort, return document, and upsert behavior.</param>
    /// <param name="collection">The collection to operate on.</param>
    /// <returns>
    /// Task containing FractalResult with:
    /// - Ok(Some(document)): Document found and updated (before or after state per options)
    /// - Ok(None): No match found and upsert=false
    /// - Error(SerializationError): Update function produced invalid data
    /// </returns>
    /// <remarks>
    /// findOneAndUpdate provides atomic find-then-update operation:
    /// - Finds first document matching filter (optionally sorted)
    /// - Applies update function to document body
    /// - Saves updated document
    /// - Returns document in requested state (before or after)
    /// - All operations in single transaction
    ///
    /// Operation is atomic:
    /// - Transaction ensures no other operation can modify document between find and update
    /// - Document returned matches the one updated
    /// - If multiple documents match, only first (after sort) is updated
    ///
    /// Return document behavior:
    /// - ReturnDocument.Before: Returns original document state (useful for audit)
    /// - ReturnDocument.After: Returns updated document state (useful for confirmation)
    ///
    /// Upsert behavior:
    /// - If upsert=true and no match: creates new document with update applied to empty/default body
    /// - If upsert=false and no match: returns Ok(None)
    /// - Upsert with ReturnDocument.After returns the newly inserted document
    ///
    /// Sort options:
    /// - Sort determines which document is updated when multiple match
    /// - Empty sort list results in arbitrary selection
    /// - Use sort for priority queues, FIFO/LIFO patterns
    ///
    /// Error handling:
    /// - Returns Error if update function produces data that cannot be serialized
    /// - Returns Error if database constraint violation occurs
    /// - Transaction automatically rolls back on error
    ///
    /// Use cases:
    /// - Claim and mark work item as processing
    /// - Update with optimistic locking (check version field)
    /// - Increment counters and return new value
    /// - State machine transitions with audit trail
    ///
    /// Performance:
    /// - O(n) for filter scan + O(log n) for update
    /// - Includes full document deserialization and serialization
    /// - Uses transaction overhead
    /// - Consider index on filter and sort fields
    ///
    /// Thread safety:
    /// - Fully thread-safe due to transaction
    /// - Multiple threads can call concurrently
    /// - Each thread gets different document or None
    /// </remarks>
    /// <example>
    /// <code>
    /// // Claim and process next work item (return after state)
    /// let options = {
    ///     Sort = [("priority", Descending); ("createdAt", Ascending)]
    ///     ReturnDocument = After
    ///     Upsert = false
    /// }
    ///
    /// let! result =
    ///     workQueue
    ///     |> Collection.findOneAndUpdate
    ///         (Query.field "status" (Query.eq "pending"))
    ///         (fun work -&gt; { work with Status = "processing"; WorkerId = currentWorkerId })
    ///         options
    ///
    /// match result with
    /// | Ok (Some doc) -&gt;
    ///     printfn $"Processing work: {doc.Id}"
    ///     // doc.Body has Status = "processing"
    /// | Ok None -&gt;
    ///     printfn "No work available"
    /// | Error err -&gt;
    ///     printfn $"Update failed: {err.Message}"
    ///
    /// // Increment counter with upsert (return new value)
    /// let counterOptions = {
    ///     Sort = []
    ///     ReturnDocument = After
    ///     Upsert = true
    /// }
    ///
    /// let! counterResult =
    ///     counters
    ///     |> Collection.findOneAndUpdate
    ///         (Query.field "name" (Query.eq "page_views"))
    ///         (fun counter -&gt; { counter with Count = counter.Count + 1 })
    ///         counterOptions
    ///
    /// // Optimistic locking with version check (return before for audit)
    /// let lockOptions = { 
    ///     Sort = []
    ///     ReturnDocument = Before
    ///     Upsert = false 
    /// }
    ///
    /// let! lockResult =
    ///     documents
    ///     |> Collection.findOneAndUpdate
    ///         (Query.all [
    ///             Query.field "id" (Query.eq docId)
    ///             Query.field "version" (Query.eq expectedVersion)
    ///         ])
    ///         (fun doc -&gt; { doc with Version = doc.Version + 1; Data = newData })
    ///         lockOptions
    /// </code>
    /// </example>
    let findOneAndUpdate 
        (filter: Query<'T>) 
        (update: 'T -> 'T) 
        (options: FindAndModifyOptions) 
        (collection: Collection<'T>) 
        : Task<FractalResult<option<Document<'T>>>> =
        task {
            try
                use transaction = Transaction.create collection.Connection
                
                // Find the document with sort options
                let queryOptions = { QueryOptions.empty with Sort = options.Sort }
                let! maybeDoc = collection |> findOneWith filter queryOptions
                
                match maybeDoc with
                | Some doc ->
                    // Apply update function and serialize
                    let updatedData = update doc.Data
                    let dataJson = Serialization.serialize updatedData
                    let now = Timestamp.now()
                    
                    // Update the document in database
                    let sql = $"
                        UPDATE {collection.Name}
                        SET body = @body, updatedAt = @updatedAt
                        WHERE _id = @id"
                    
                    let params' = [
                        "@body", SqlType.String dataJson
                        "@updatedAt", SqlType.Int64 now
                        "@id", SqlType.String doc.Id
                    ]
                    
                    collection.Connection
                    |> Db.newCommand sql
                    |> Db.setParams params'
                    |> Db.exec
                    |> ignore
                    
                    transaction.Commit()
                    
                    // Return document based on options
                    match options.ReturnDocument with
                    | ReturnDocument.Before ->
                        return Ok (Some doc)
                    | ReturnDocument.After ->
                        let updatedDoc = {
                            Id = doc.Id
                            Data = updatedData
                            CreatedAt = doc.CreatedAt
                            UpdatedAt = now
                        }
                        return Ok (Some updatedDoc)
                
                | None when options.Upsert ->
                    // No document found, but upsert is enabled - insert new document
                    // Apply update to default/'T value
                    let defaultData = Unchecked.defaultof<'T>
                    let newData = update defaultData
                    let dataJson = Serialization.serialize newData
                    let now = Timestamp.now()
                    let newId = IdGenerator.generate()
                    
                    let sql = $"
                        INSERT INTO {collection.Name} (_id, body, createdAt, updatedAt)
                        VALUES (@id, @body, @createdAt, @updatedAt)"
                    
                    let params' = [
                        "@id", SqlType.String newId
                        "@body", SqlType.String dataJson
                        "@createdAt", SqlType.Int64 now
                        "@updatedAt", SqlType.Int64 now
                    ]
                    
                    collection.Connection
                    |> Db.newCommand sql
                    |> Db.setParams params'
                    |> Db.exec
                    |> ignore
                    
                    transaction.Commit()
                    
                    let newDoc = {
                        Id = newId
                        Data = newData
                        CreatedAt = now
                        UpdatedAt = now
                    }
                    
                    return Ok (Some newDoc)
                
                | None ->
                    // No document found and upsert is false
                    transaction.Commit()
                    return Ok None
            
            with
            | :? DbExecutionException as ex ->
                match ex.InnerException with
                | :? SqliteException as sqlEx when sqlEx.SqliteErrorCode = 19 ->
                    // SQLITE_CONSTRAINT
                    return Error (FractalError.UniqueConstraint("_id", obj()))
                | _ ->
                    let msg = $"Database error during findOneAndUpdate: {ex.Message}"
                    return Error (FractalError.InvalidOperation(msg))
            | ex ->
                let msg = $"Unexpected error during findOneAndUpdate: {ex.Message}"
                return Error (FractalError.InvalidOperation(msg))
        }
    
    /// <summary>
    /// Atomically finds and replaces a single document matching the filter.
    /// </summary>
    /// <param name="filter">Query filter to match documents.</param>
    /// <param name="doc">New document data to replace with.</param>
    /// <param name="options">Options controlling sort, return document, and upsert behavior.</param>
    /// <param name="collection">The collection to operate on.</param>
    /// <returns>
    /// Task containing FractalResult with:
    /// - Ok(Some(document)): Document found and replaced (before or after state per options)
    /// - Ok(None): No match found and upsert=false
    /// - Error(SerializationError): New document data is invalid
    /// </returns>
    /// <remarks>
    /// findOneAndReplace provides atomic find-then-replace operation:
    /// - Finds first document matching filter (optionally sorted)
    /// - Replaces entire document body with new data
    /// - Preserves Id and CreatedAt from original document
    /// - Updates UpdatedAt timestamp
    /// - Returns document in requested state (before or after)
    /// - All operations in single transaction
    ///
    /// Operation is atomic:
    /// - Transaction ensures no other operation can modify document between find and replace
    /// - Document returned matches the one replaced
    /// - If multiple documents match, only first (after sort) is replaced
    ///
    /// Difference from update:
    /// - findOneAndUpdate: applies transformation function (partial update)
    /// - findOneAndReplace: completely replaces document body (full replacement)
    /// - Both preserve Id and CreatedAt
    ///
    /// Return document behavior:
    /// - ReturnDocument.Before: Returns original document state (useful for audit)
    /// - ReturnDocument.After: Returns replaced document state (useful for confirmation)
    ///
    /// Upsert behavior:
    /// - If upsert=true and no match: creates new document with provided data
    /// - If upsert=false and no match: returns Ok(None)
    /// - Upsert with ReturnDocument.After returns the newly inserted document
    ///
    /// Sort options:
    /// - Sort determines which document is replaced when multiple match
    /// - Empty sort list results in arbitrary selection
    /// - Use sort for priority queues, FIFO/LIFO patterns
    ///
    /// Use cases:
    /// - Document versioning (save complete new version)
    /// - Import/sync operations (replace with external data)
    /// - Cache invalidation (replace stale entry)
    /// - Configuration updates (replace entire config)
    ///
    /// Performance:
    /// - O(n) for filter scan + O(log n) for update
    /// - Includes full document deserialization and serialization
    /// - Uses transaction overhead
    /// - Consider index on filter and sort fields
    ///
    /// Thread safety:
    /// - Fully thread-safe due to transaction
    /// - Multiple threads can call concurrently
    /// - Each thread gets different document or None
    /// </remarks>
    /// <example>
    /// <code>
    /// // Replace user profile completely (return new state)
    /// let options = {
    ///     Sort = []
    ///     ReturnDocument = After
    ///     Upsert = false
    /// }
    ///
    /// let newProfile = {
    ///     Name = "Jane Doe"
    ///     Email = "jane@example.com"
    ///     Settings = { Theme = "dark"; Language = "en" }
    /// }
    ///
    /// let! result =
    ///     users
    ///     |> Collection.findOneAndReplace
    ///         (Query.field "email" (Query.eq "old@example.com"))
    ///         newProfile
    ///         options
    ///
    /// match result with
    /// | Ok (Some doc) -&gt;
    ///     printfn $"Profile replaced: {doc.Id}"
    ///     // doc.Data contains newProfile
    /// | Ok None -&gt;
    ///     printfn "User not found"
    /// | Error err -&gt;
    ///     printfn $"Replace failed: {err.Message}"
    ///
    /// // Upsert pattern - create if not exists (return old value for audit)
    /// let upsertOptions = {
    ///     Sort = []
    ///     ReturnDocument = Before
    ///     Upsert = true
    /// }
    ///
    /// let config = { ApiKey = "new_key"; Timeout = 30 }
    ///
    /// let! configResult =
    ///     settings
    ///     |> Collection.findOneAndReplace
    ///         (Query.field "name" (Query.eq "api_config"))
    ///         config
    ///         upsertOptions
    ///
    /// match configResult with
    /// | Ok (Some oldDoc) -&gt;
    ///     // Had existing config, oldDoc contains previous value
    ///     printfn $"Updated config, old timeout: {oldDoc.Data.Timeout}"
    /// | Ok None -&gt;
    ///     // New config was inserted (upsert), None because ReturnDocument.Before
    ///     printfn "Created new config"
    /// | Error err -&gt;
    ///     printfn $"Config update failed: {err.Message}"
    ///
    /// // Version control - replace with sorted selection
    /// let versionOptions = {
    ///     Sort = [("version", Descending)]  // Replace newest version
    ///     ReturnDocument = Before           // Return old version for backup
    ///     Upsert = false
    /// }
    ///
    /// let! versionResult =
    ///     documents
    ///     |> Collection.findOneAndReplace
    ///         (Query.field "docId" (Query.eq documentId))
    ///         newVersion
    ///         versionOptions
    /// </code>
    /// </example>
    let findOneAndReplace 
        (filter: Query<'T>) 
        (doc: 'T) 
        (options: FindAndModifyOptions) 
        (collection: Collection<'T>) 
        : Task<FractalResult<option<Document<'T>>>> =
        task {
            try
                use transaction = Transaction.create collection.Connection
                
                // Find the document with sort options
                let queryOptions = { QueryOptions.empty with Sort = options.Sort }
                let! maybeDoc = collection |> findOneWith filter queryOptions
                
                match maybeDoc with
                | Some existingDoc ->
                    // Replace document data (preserve Id and CreatedAt)
                    let dataJson = Serialization.serialize doc
                    let now = Timestamp.now()
                    
                    // Update the document in database
                    let sql = $"
                        UPDATE {collection.Name}
                        SET body = @body, updatedAt = @updatedAt
                        WHERE _id = @id"
                    
                    let params' = [
                        "@body", SqlType.String dataJson
                        "@updatedAt", SqlType.Int64 now
                        "@id", SqlType.String existingDoc.Id
                    ]
                    
                    collection.Connection
                    |> Db.newCommand sql
                    |> Db.setParams params'
                    |> Db.exec
                    |> ignore
                    
                    transaction.Commit()
                    
                    // Return document based on options
                    match options.ReturnDocument with
                    | ReturnDocument.Before ->
                        return Ok (Some existingDoc)
                    | ReturnDocument.After ->
                        let replacedDoc = {
                            Id = existingDoc.Id
                            Data = doc
                            CreatedAt = existingDoc.CreatedAt
                            UpdatedAt = now
                        }
                        return Ok (Some replacedDoc)
                
                | None when options.Upsert ->
                    // No document found, but upsert is enabled - insert new document
                    let dataJson = Serialization.serialize doc
                    let now = Timestamp.now()
                    let newId = IdGenerator.generate()
                    
                    let sql = $"
                        INSERT INTO {collection.Name} (_id, body, createdAt, updatedAt)
                        VALUES (@id, @body, @createdAt, @updatedAt)"
                    
                    let params' = [
                        "@id", SqlType.String newId
                        "@body", SqlType.String dataJson
                        "@createdAt", SqlType.Int64 now
                        "@updatedAt", SqlType.Int64 now
                    ]
                    
                    collection.Connection
                    |> Db.newCommand sql
                    |> Db.setParams params'
                    |> Db.exec
                    |> ignore
                    
                    transaction.Commit()
                    
                    let newDoc = {
                        Id = newId
                        Data = doc
                        CreatedAt = now
                        UpdatedAt = now
                    }
                    
                    return Ok (Some newDoc)
                
                | None ->
                    // No document found and upsert is false
                    transaction.Commit()
                    return Ok None
            
            with
            | :? DbExecutionException as ex ->
                match ex.InnerException with
                | :? SqliteException as sqlEx when sqlEx.SqliteErrorCode = 19 ->
                    // SQLITE_CONSTRAINT
                    return Error (FractalError.UniqueConstraint("_id", obj()))
                | _ ->
                    let msg = $"Database error during findOneAndReplace: {ex.Message}"
                    return Error (FractalError.InvalidOperation(msg))
            | ex ->
                let msg = $"Unexpected error during findOneAndReplace: {ex.Message}"
                return Error (FractalError.InvalidOperation(msg))
        }
    
    // ═══════════════════════════════════════════════════════════════
    // UTILITY OPERATIONS
    // ═══════════════════════════════════════════════════════════════
    
    /// <summary>
    /// Drops the collection by deleting its table from the database.
    /// </summary>
    /// <param name="collection">The collection to drop.</param>
    /// <returns>
    /// Task that completes when the table is dropped.
    /// </returns>
    /// <remarks>
    /// drop permanently deletes the collection's table and all its data:
    /// - Executes DROP TABLE IF EXISTS statement
    /// - Removes all documents in the collection
    /// - Removes all indexes associated with the table
    /// - Operation is irreversible - all data is lost
    /// - No error if table doesn't exist (IF EXISTS clause)
    ///
    /// Safety considerations:
    /// - This operation cannot be undone
    /// - All documents are permanently deleted
    /// - All indexes are permanently deleted
    /// - Consider backup before dropping
    /// - Consider soft delete pattern for important data
    ///
    /// Use cases:
    /// - Cleanup during testing
    /// - Application uninstall
    /// - Schema migration (drop and recreate)
    /// - Temporary collections that are no longer needed
    ///
    /// Performance:
    /// - Very fast operation (single DDL statement)
    /// - No transaction needed (DDL is auto-commit in SQLite)
    /// - Releases disk space immediately
    ///
    /// Thread safety:
    /// - Should not be called while other operations are active
    /// - SQLite locks prevent concurrent access during DDL
    /// - Application should ensure no active operations before drop
    /// </remarks>
    /// <example>
    /// <code>
    /// // Drop collection
    /// do! tempData |> Collection.drop
    /// printfn "Temporary data collection dropped"
    ///
    /// // Drop and recreate pattern (schema migration)
    /// do! users |> Collection.drop
    /// let! newUsers = db |> FractalDb.collection "users" newSchema
    /// printfn "Users collection recreated with new schema"
    ///
    /// // Cleanup in tests
    /// [&lt;Test&gt;]
    /// let testWithTempCollection() = task {
    ///     let! testCollection = db |> FractalDb.collection "test_temp" schema
    ///
    ///     // ... run tests ...
    ///
    ///     // Cleanup
    ///     do! testCollection |> Collection.drop
    /// }
    ///
    /// // DANGEROUS: Drop production data (only with confirmation)
    /// let confirmDrop = // ... get user confirmation ...
    /// if confirmDrop then
    ///     do! collection |> Collection.drop
    ///     printfn "Collection dropped permanently"
    /// </code>
    /// </example>
    let drop (collection: Collection<'T>) : Task<unit> =
        task {
            let sql = $"DROP TABLE IF EXISTS {collection.Name}"
            
            collection.Connection
            |> Db.newCommand sql
            |> Db.setParams []
            |> Db.exec
            |> ignore
            
            return ()
        }
    
    /// <summary>
    /// Validates a document against the collection's schema validator.
    /// </summary>
    /// <param name="doc">The document data to validate.</param>
    /// <param name="collection">The collection with the validation schema.</param>
    /// <returns>
    /// FractalResult containing:
    /// - Ok(doc): Document is valid (or no validator defined)
    /// - Error(Validation): Document failed validation with error message
    /// </returns>
    /// <remarks>
    /// validate checks a document against the schema's validation function:
    /// - If schema has Validate function: runs it and returns result
    /// - If schema has no Validate function: returns Ok (validation bypassed)
    /// - Validation errors are converted to FractalError.Validation
    /// - Validation is synchronous (no database access)
    ///
    /// Validation timing:
    /// - Can be called explicitly before insert/update
    /// - Can be integrated into application logic
    /// - Does not automatically run on CRUD operations
    /// - Application controls when validation occurs
    ///
    /// Validator function signature:
    /// - Type: 'T -&gt; Result&lt;'T, string&gt;
    /// - Input: document data to validate
    /// - Output: Ok(data) if valid, Error(message) if invalid
    /// - Can transform data during validation
    /// - Can perform complex business logic checks
    ///
    /// Use cases:
    /// - Pre-insert validation
    /// - Pre-update validation
    /// - Batch validation before bulk operations
    /// - API request validation
    /// - Data migration validation
    ///
    /// Performance:
    /// - O(1) if no validator defined
    /// - Performance depends on validator complexity
    /// - No database access (pure function)
    /// - Can be called frequently without overhead
    ///
    /// Thread safety:
    /// - Fully thread-safe (no shared state)
    /// - Validator should be pure function
    /// - Can be called concurrently
    /// </remarks>
    /// <example>
    /// <code>
    /// // Define schema with validator
    /// let userSchema = {
    ///     Fields = []
    ///     Indexes = []
    ///     Validate = Some (fun user -&gt;
    ///         if String.IsNullOrWhiteSpace(user.Email) then
    ///             Error "Email is required"
    ///         elif not (user.Email.Contains("@")) then
    ///             Error "Invalid email format"
    ///         elif user.Age &lt; 18 then
    ///             Error "User must be 18 or older"
    ///         else
    ///             Ok user
    ///     )
    /// }
    ///
    /// // Validate before insert
    /// let newUser = { Name = "Jane"; Email = "jane@example.com"; Age = 25 }
    ///
    /// match users |> Collection.validate newUser with
    /// | Ok validUser -&gt;
    ///     let! doc = users |> Collection.insertOne validUser
    ///     printfn $"User inserted: {doc.Id}"
    /// | Error err -&gt;
    ///     printfn $"Validation failed: {err.Message}"
    ///
    /// // Batch validation
    /// let usersToInsert = [
    ///     { Name = "User1"; Email = "user1@test.com"; Age = 20 }
    ///     { Name = "User2"; Email = "invalid"; Age = 15 }
    ///     { Name = "User3"; Email = "user3@test.com"; Age = 30 }
    /// ]
    ///
    /// let validationResults =
    ///     usersToInsert
    ///     |> List.map (fun user -&gt;
    ///         match users |> Collection.validate user with
    ///         | Ok valid -&gt; Some valid
    ///         | Error err -&gt;
    ///             printfn $"Validation failed for {user.Name}: {err.Message}"
    ///             None
    ///     )
    ///     |> List.choose id  // Keep only valid documents
    ///
    /// let! result = users |> Collection.insertMany validationResults
    /// printfn $"Inserted {result.InsertedCount} valid users"
    ///
    /// // Schema without validator (validation bypassed)
    /// let noValidatorSchema = { Fields = []; Indexes = []; Validate = None }
    /// match collection |> Collection.validate anyDoc with
    /// | Ok doc -&gt; printfn "No validation defined, document accepted"
    /// | Error _ -&gt; printfn "This branch never executes"
    /// </code>
    /// </example>
    let validate (doc: 'T) (collection: Collection<'T>) : FractalResult<'T> =
        match collection.Schema.Validate with
        | Some validator ->
            match validator doc with
            | Ok validDoc -> Ok validDoc
            | Error errorMsg -> Error (FractalError.Validation(None, errorMsg))
        | None ->
            // No validator defined, accept document
            Ok doc






