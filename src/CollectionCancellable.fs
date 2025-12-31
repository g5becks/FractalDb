/// <summary>
/// Module providing cancellable versions of all Collection operations.
/// </summary>
/// <remarks>
/// This module mirrors FractalDb.Collection but returns CancellableTask&lt;'T&gt; instead of Task&lt;'T&gt;.
/// CancellableTask is an alias for CancellationToken -&gt; Task&lt;'T&gt;, providing:
/// - Seamless integration with IcedTasks cancellableTask computation expression
/// - Implicit CancellationToken propagation
/// - Cancellation checks before each operation
///
/// Usage with IcedTasks:
/// <code>
/// open IcedTasks
/// open FractalDb.Cancellable
///
/// let operation = cancellableTask {
///     let! doc = Cancellable.findById "id123" collection
///     let! updated = Cancellable.updateById "id123" (fun d -> { d with Name = "New" }) collection
///     return updated
/// }
///
/// // Execute with cancellation token
/// let! result = operation ct
/// </code>
///
/// All operations check for cancellation before executing database operations.
/// </remarks>
[<RequireQualifiedAccess>]
module FractalDb.Cancellable

open System
open System.Threading
open System.Threading.Tasks
open IcedTasks
open FractalDb.Types
open FractalDb.Errors
open FractalDb.Options
open FractalDb.Operators
open FractalDb.Collection

// ═══════════════════════════════════════════════════════════════════════════
// INTERNAL HELPERS
// ═══════════════════════════════════════════════════════════════════════════

/// Checks cancellation and throws if requested
let inline private checkCancellation (ct: CancellationToken) = ct.ThrowIfCancellationRequested()

/// Wraps a Task-returning function with cancellation check
let inline private wrapWithCancellation (operation: unit -> Task<'T>) : CancellableTask<'T> =
    fun ct ->
        task {
            checkCancellation ct
            return! operation ()
        }

// ═══════════════════════════════════════════════════════════════════════════
// READ OPERATIONS
// ═══════════════════════════════════════════════════════════════════════════

/// <summary>
/// Finds a document by its unique identifier with cancellation support.
/// </summary>
/// <param name="id">The document ID to search for.</param>
/// <param name="collection">The collection to search in.</param>
/// <returns>CancellableTask containing Some document if found, None if not found.</returns>
let findById (id: string) (collection: Collection<'T>) : CancellableTask<option<Document<'T>>> =
    wrapWithCancellation (fun () -> Collection.findById id collection)

/// <summary>
/// Finds the first document matching the filter with cancellation support.
/// </summary>
/// <param name="filter">Query filter to match documents.</param>
/// <param name="collection">The collection to search in.</param>
/// <returns>CancellableTask containing Some document if found, None if no match.</returns>
let findOne (filter: Query<'T>) (collection: Collection<'T>) : CancellableTask<option<Document<'T>>> =
    wrapWithCancellation (fun () -> Collection.findOne filter collection)

/// <summary>
/// Finds the first document matching the filter with options and cancellation support.
/// </summary>
/// <param name="filter">Query filter to match documents.</param>
/// <param name="options">Query options for sorting, limiting, etc.</param>
/// <param name="collection">The collection to search in.</param>
/// <returns>CancellableTask containing Some document if found, None if no match.</returns>
let findOneWith
    (filter: Query<'T>)
    (options: QueryOptions<'T>)
    (collection: Collection<'T>)
    : CancellableTask<option<Document<'T>>> =
    wrapWithCancellation (fun () -> Collection.findOneWith filter options collection)

/// <summary>
/// Finds all documents matching the filter with cancellation support.
/// </summary>
/// <param name="filter">Query filter to match documents.</param>
/// <param name="collection">The collection to search in.</param>
/// <returns>CancellableTask containing list of matching documents.</returns>
let find (filter: Query<'T>) (collection: Collection<'T>) : CancellableTask<list<Document<'T>>> =
    wrapWithCancellation (fun () -> Collection.find filter collection)

/// <summary>
/// Finds all documents matching the filter with options and cancellation support.
/// </summary>
/// <param name="filter">Query filter to match documents.</param>
/// <param name="options">Query options for sorting, limiting, skipping, etc.</param>
/// <param name="collection">The collection to search in.</param>
/// <returns>CancellableTask containing list of matching documents.</returns>
let findWith
    (filter: Query<'T>)
    (options: QueryOptions<'T>)
    (collection: Collection<'T>)
    : CancellableTask<list<Document<'T>>> =
    wrapWithCancellation (fun () -> Collection.findWith filter options collection)

/// <summary>
/// Counts documents matching the filter with cancellation support.
/// </summary>
/// <param name="filter">Query filter to match documents.</param>
/// <param name="collection">The collection to count in.</param>
/// <returns>CancellableTask containing count of matching documents.</returns>
let count (filter: Query<'T>) (collection: Collection<'T>) : CancellableTask<int> =
    wrapWithCancellation (fun () -> Collection.count filter collection)

/// <summary>
/// Gets estimated total document count with cancellation support.
/// </summary>
/// <param name="collection">The collection to count.</param>
/// <returns>CancellableTask containing estimated count.</returns>
let estimatedCount (collection: Collection<'T>) : CancellableTask<int> =
    wrapWithCancellation (fun () -> Collection.estimatedCount collection)

/// <summary>
/// Full-text search across specified fields with cancellation support.
/// </summary>
/// <param name="text">Search text.</param>
/// <param name="fields">Fields to search in.</param>
/// <param name="collection">The collection to search.</param>
/// <returns>CancellableTask containing list of matching documents.</returns>
let search (text: string) (fields: list<string>) (collection: Collection<'T>) : CancellableTask<list<Document<'T>>> =
    wrapWithCancellation (fun () -> Collection.search text fields collection)

/// <summary>
/// Full-text search with options and cancellation support.
/// </summary>
/// <param name="text">Search text.</param>
/// <param name="fields">Fields to search in.</param>
/// <param name="options">Query options.</param>
/// <param name="collection">The collection to search.</param>
/// <returns>CancellableTask containing list of matching documents.</returns>
let searchWith
    (text: string)
    (fields: list<string>)
    (options: QueryOptions<'T>)
    (collection: Collection<'T>)
    : CancellableTask<list<Document<'T>>> =
    wrapWithCancellation (fun () -> Collection.searchWith text fields options collection)

/// <summary>
/// Gets distinct values for a field with cancellation support.
/// </summary>
/// <param name="field">Field name.</param>
/// <param name="filter">Query filter.</param>
/// <param name="collection">The collection to query.</param>
/// <returns>CancellableTask with Result containing list of distinct values.</returns>
let distinct<'T, 'V>
    (field: string)
    (filter: Query<'T>)
    (collection: Collection<'T>)
    : CancellableTask<FractalResult<list<'V>>> =
    wrapWithCancellation (fun () -> Collection.distinct<'T, 'V> field filter collection)

/// <summary>
/// Executes a translated query with cancellation support.
/// </summary>
/// <param name="query">The translated query to execute.</param>
/// <param name="collection">The collection to execute against.</param>
/// <returns>CancellableTask containing list of matching documents.</returns>
let exec
    (query: FractalDb.QueryExpr.TranslatedQuery<'T>)
    (collection: Collection<'T>)
    : CancellableTask<list<Document<'T>>> =
    wrapWithCancellation (fun () -> Collection.exec query collection)

// ═══════════════════════════════════════════════════════════════════════════
// INSERT OPERATIONS
// ═══════════════════════════════════════════════════════════════════════════

/// <summary>
/// Inserts a single document with cancellation support.
/// </summary>
/// <param name="doc">The document data to insert.</param>
/// <param name="collection">The collection to insert into.</param>
/// <returns>CancellableTask with Result containing the inserted Document or error.</returns>
let insertOne (doc: 'T) (collection: Collection<'T>) : CancellableTask<FractalResult<Document<'T>>> =
    wrapWithCancellation (fun () -> Collection.insertOne doc collection)

/// <summary>
/// Inserts multiple documents with cancellation support.
/// </summary>
/// <param name="docs">List of documents to insert.</param>
/// <param name="collection">The collection to insert into.</param>
/// <returns>CancellableTask with Result containing InsertManyResult or error.</returns>
let insertMany (docs: list<'T>) (collection: Collection<'T>) : CancellableTask<FractalResult<InsertManyResult<'T>>> =
    wrapWithCancellation (fun () -> Collection.insertMany docs collection)

/// <summary>
/// Inserts multiple documents with ordering control and cancellation support.
/// </summary>
/// <param name="docs">List of documents to insert.</param>
/// <param name="ordered">If true, stops on first error; if false, continues.</param>
/// <param name="collection">The collection to insert into.</param>
/// <returns>CancellableTask with Result containing InsertManyResult or error.</returns>
let insertManyWith
    (docs: list<'T>)
    (ordered: bool)
    (collection: Collection<'T>)
    : CancellableTask<FractalResult<InsertManyResult<'T>>> =
    wrapWithCancellation (fun () -> Collection.insertManyWith docs ordered collection)

// ═══════════════════════════════════════════════════════════════════════════
// UPDATE OPERATIONS
// ═══════════════════════════════════════════════════════════════════════════

/// <summary>
/// Updates a document by ID with cancellation support.
/// </summary>
/// <param name="id">Document ID.</param>
/// <param name="update">Update function.</param>
/// <param name="collection">The collection to update in.</param>
/// <returns>CancellableTask with Result containing Some updated document if found, None otherwise.</returns>
let updateById
    (id: string)
    (update: 'T -> 'T)
    (collection: Collection<'T>)
    : CancellableTask<FractalResult<option<Document<'T>>>> =
    wrapWithCancellation (fun () -> Collection.updateById id update collection)

/// <summary>
/// Updates the first document matching the filter with cancellation support.
/// </summary>
/// <param name="filter">Query filter.</param>
/// <param name="update">Update function.</param>
/// <param name="collection">The collection to update in.</param>
/// <returns>CancellableTask with Result containing Some updated document if found, None otherwise.</returns>
let updateOne
    (filter: Query<'T>)
    (update: 'T -> 'T)
    (collection: Collection<'T>)
    : CancellableTask<FractalResult<option<Document<'T>>>> =
    wrapWithCancellation (fun () -> Collection.updateOne filter update collection)

/// <summary>
/// Updates the first document matching the filter with upsert option and cancellation support.
/// </summary>
/// <param name="filter">Query filter.</param>
/// <param name="update">Update function.</param>
/// <param name="upsert">If true, inserts if no match found.</param>
/// <param name="collection">The collection to update in.</param>
/// <returns>CancellableTask with Result containing Some document if found/created, None otherwise.</returns>
let updateOneWith
    (filter: Query<'T>)
    (update: 'T -> 'T)
    (upsert: bool)
    (collection: Collection<'T>)
    : CancellableTask<FractalResult<option<Document<'T>>>> =
    wrapWithCancellation (fun () -> Collection.updateOneWith filter update upsert collection)

/// <summary>
/// Replaces the first document matching the filter with cancellation support.
/// </summary>
/// <param name="filter">Query filter.</param>
/// <param name="doc">Replacement document.</param>
/// <param name="collection">The collection to update in.</param>
/// <returns>CancellableTask with Result containing Some replaced document if found, None otherwise.</returns>
let replaceOne
    (filter: Query<'T>)
    (doc: 'T)
    (collection: Collection<'T>)
    : CancellableTask<FractalResult<option<Document<'T>>>> =
    wrapWithCancellation (fun () -> Collection.replaceOne filter doc collection)

/// <summary>
/// Updates all documents matching the filter with cancellation support.
/// </summary>
/// <param name="filter">Query filter.</param>
/// <param name="update">Update function.</param>
/// <param name="collection">The collection to update in.</param>
/// <returns>CancellableTask with Result containing UpdateResult with match and modify counts.</returns>
let updateMany
    (filter: Query<'T>)
    (update: 'T -> 'T)
    (collection: Collection<'T>)
    : CancellableTask<FractalResult<UpdateResult>> =
    wrapWithCancellation (fun () -> Collection.updateMany filter update collection)

// ═══════════════════════════════════════════════════════════════════════════
// DELETE OPERATIONS
// ═══════════════════════════════════════════════════════════════════════════

/// <summary>
/// Deletes a document by ID with cancellation support.
/// </summary>
/// <param name="id">Document ID.</param>
/// <param name="collection">The collection to delete from.</param>
/// <returns>CancellableTask containing true if deleted, false if not found.</returns>
let deleteById (id: string) (collection: Collection<'T>) : CancellableTask<bool> =
    wrapWithCancellation (fun () -> Collection.deleteById id collection)

/// <summary>
/// Deletes the first document matching the filter with cancellation support.
/// </summary>
/// <param name="filter">Query filter.</param>
/// <param name="collection">The collection to delete from.</param>
/// <returns>CancellableTask containing true if deleted, false if not found.</returns>
let deleteOne (filter: Query<'T>) (collection: Collection<'T>) : CancellableTask<bool> =
    wrapWithCancellation (fun () -> Collection.deleteOne filter collection)

/// <summary>
/// Deletes all documents matching the filter with cancellation support.
/// </summary>
/// <param name="filter">Query filter.</param>
/// <param name="collection">The collection to delete from.</param>
/// <returns>CancellableTask containing DeleteResult with deleted count.</returns>
let deleteMany (filter: Query<'T>) (collection: Collection<'T>) : CancellableTask<DeleteResult> =
    wrapWithCancellation (fun () -> Collection.deleteMany filter collection)

// ═══════════════════════════════════════════════════════════════════════════
// ATOMIC FIND-AND-MODIFY OPERATIONS
// ═══════════════════════════════════════════════════════════════════════════

/// <summary>
/// Atomically finds and deletes a document with cancellation support.
/// </summary>
/// <param name="filter">Query filter.</param>
/// <param name="collection">The collection to operate on.</param>
/// <returns>CancellableTask containing Some deleted document if found, None otherwise.</returns>
let findOneAndDelete (filter: Query<'T>) (collection: Collection<'T>) : CancellableTask<option<Document<'T>>> =
    wrapWithCancellation (fun () -> Collection.findOneAndDelete filter collection)

/// <summary>
/// Atomically finds and deletes a document with options and cancellation support.
/// </summary>
/// <param name="filter">Query filter.</param>
/// <param name="options">Find options (sort).</param>
/// <param name="collection">The collection to operate on.</param>
/// <returns>CancellableTask containing Some deleted document if found, None otherwise.</returns>
let findOneAndDeleteWith
    (filter: Query<'T>)
    (options: FindOptions)
    (collection: Collection<'T>)
    : CancellableTask<option<Document<'T>>> =
    wrapWithCancellation (fun () -> Collection.findOneAndDeleteWith filter options collection)

/// <summary>
/// Atomically finds and updates a document with cancellation support.
/// </summary>
/// <param name="filter">Query filter.</param>
/// <param name="update">Update function.</param>
/// <param name="options">Find and modify options.</param>
/// <param name="collection">The collection to operate on.</param>
/// <returns>CancellableTask with Result containing Some document if found, None otherwise.</returns>
let findOneAndUpdate
    (filter: Query<'T>)
    (update: 'T -> 'T)
    (options: FindAndModifyOptions)
    (collection: Collection<'T>)
    : CancellableTask<FractalResult<option<Document<'T>>>> =
    wrapWithCancellation (fun () -> Collection.findOneAndUpdate filter update options collection)

/// <summary>
/// Atomically finds and replaces a document with cancellation support.
/// </summary>
/// <param name="filter">Query filter.</param>
/// <param name="doc">Replacement document.</param>
/// <param name="options">Find and modify options.</param>
/// <param name="collection">The collection to operate on.</param>
/// <returns>CancellableTask with Result containing Some document if found, None otherwise.</returns>
let findOneAndReplace
    (filter: Query<'T>)
    (doc: 'T)
    (options: FindAndModifyOptions)
    (collection: Collection<'T>)
    : CancellableTask<FractalResult<option<Document<'T>>>> =
    wrapWithCancellation (fun () -> Collection.findOneAndReplace filter doc options collection)

// ═══════════════════════════════════════════════════════════════════════════
// UTILITY OPERATIONS
// ═══════════════════════════════════════════════════════════════════════════

/// <summary>
/// Drops the collection with cancellation support.
/// </summary>
/// <param name="collection">The collection to drop.</param>
/// <returns>CancellableTask that completes when collection is dropped.</returns>
let drop (collection: Collection<'T>) : CancellableTask<unit> =
    wrapWithCancellation (fun () -> Collection.drop collection)

// ═══════════════════════════════════════════════════════════════════════════
// COLLECTION TYPE EXTENSION - CancellableTask-returning instance methods
// ═══════════════════════════════════════════════════════════════════════════

/// <summary>
/// Extension methods for Collection&lt;'T&gt; that return CancellableTask for
/// seamless integration with IcedTasks cancellableTask computation expression.
/// </summary>
/// <remarks>
/// These methods provide automatic CancellationToken propagation when used
/// inside cancellableTask blocks:
/// <code>
/// cancellableTask {
///     let! result = collection.InsertOneAsync(doc)  // CT auto-propagated
///     let! found = collection.FindByIdAsync(id)     // CT auto-propagated
///     return result
/// }
/// </code>
/// Methods are suffixed with "Async" to distinguish from Task-returning methods.
/// </remarks>
type Collection<'T> with

    // ════════════════════════════════════════════════════════════════════════
    // INSERT OPERATIONS
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>Inserts a document with automatic cancellation propagation.</summary>
    member this.InsertOneAsync(doc: 'T) : CancellableTask<FractalResult<Document<'T>>> = insertOne doc this

    /// <summary>Inserts multiple documents with automatic cancellation propagation.</summary>
    member this.InsertManyAsync(docs: list<'T>) : CancellableTask<FractalResult<InsertManyResult<'T>>> =
        insertMany docs this

    /// <summary>Inserts multiple documents with ordering control and automatic cancellation propagation.</summary>
    member this.InsertManyAsync(docs: list<'T>, ordered: bool) : CancellableTask<FractalResult<InsertManyResult<'T>>> =
        insertManyWith docs ordered this

    // ════════════════════════════════════════════════════════════════════════
    // FIND OPERATIONS
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>Finds a document by ID with automatic cancellation propagation.</summary>
    member this.FindByIdAsync(id: string) : CancellableTask<option<Document<'T>>> = findById id this

    /// <summary>Finds the first matching document with automatic cancellation propagation.</summary>
    member this.FindOneAsync(filter: Query<'T>) : CancellableTask<option<Document<'T>>> = findOne filter this

    /// <summary>Finds the first matching document with options and automatic cancellation propagation.</summary>
    member this.FindOneAsync(filter: Query<'T>, options: QueryOptions<'T>) : CancellableTask<option<Document<'T>>> =
        findOneWith filter options this

    /// <summary>Finds all matching documents with automatic cancellation propagation.</summary>
    member this.FindAsync(filter: Query<'T>) : CancellableTask<list<Document<'T>>> = find filter this

    /// <summary>Finds all matching documents with options and automatic cancellation propagation.</summary>
    member this.FindAsync(filter: Query<'T>, options: QueryOptions<'T>) : CancellableTask<list<Document<'T>>> =
        findWith filter options this

    /// <summary>Counts matching documents with automatic cancellation propagation.</summary>
    member this.CountAsync(filter: Query<'T>) : CancellableTask<int> = count filter this

    /// <summary>Gets estimated document count with automatic cancellation propagation.</summary>
    member this.EstimatedCountAsync() : CancellableTask<int> = estimatedCount this

    // ════════════════════════════════════════════════════════════════════════
    // SEARCH OPERATIONS
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>Full-text search with automatic cancellation propagation.</summary>
    member this.SearchAsync(text: string, fields: list<string>) : CancellableTask<list<Document<'T>>> =
        search text fields this

    /// <summary>Full-text search with options and automatic cancellation propagation.</summary>
    member this.SearchAsync
        (text: string, fields: list<string>, options: QueryOptions<'T>)
        : CancellableTask<list<Document<'T>>> =
        searchWith text fields options this

    /// <summary>Gets distinct field values with automatic cancellation propagation.</summary>
    member this.DistinctAsync<'V>(field: string, filter: Query<'T>) : CancellableTask<FractalResult<list<'V>>> =
        distinct<'T, 'V> field filter this

    // ════════════════════════════════════════════════════════════════════════
    // UPDATE OPERATIONS
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>Updates a document by ID with automatic cancellation propagation.</summary>
    member this.UpdateByIdAsync(id: string, update: 'T -> 'T) : CancellableTask<FractalResult<option<Document<'T>>>> =
        updateById id update this

    /// <summary>Updates the first matching document with automatic cancellation propagation.</summary>
    member this.UpdateOneAsync
        (filter: Query<'T>, update: 'T -> 'T)
        : CancellableTask<FractalResult<option<Document<'T>>>> =
        updateOne filter update this

    /// <summary>Updates the first matching document with upsert and automatic cancellation propagation.</summary>
    member this.UpdateOneAsync
        (filter: Query<'T>, update: 'T -> 'T, upsert: bool)
        : CancellableTask<FractalResult<option<Document<'T>>>> =
        updateOneWith filter update upsert this

    /// <summary>Replaces the first matching document with automatic cancellation propagation.</summary>
    member this.ReplaceOneAsync(filter: Query<'T>, doc: 'T) : CancellableTask<FractalResult<option<Document<'T>>>> =
        replaceOne filter doc this

    /// <summary>Updates all matching documents with automatic cancellation propagation.</summary>
    member this.UpdateManyAsync(filter: Query<'T>, update: 'T -> 'T) : CancellableTask<FractalResult<UpdateResult>> =
        updateMany filter update this

    // ════════════════════════════════════════════════════════════════════════
    // DELETE OPERATIONS
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>Deletes a document by ID with automatic cancellation propagation.</summary>
    member this.DeleteByIdAsync(id: string) : CancellableTask<bool> = deleteById id this

    /// <summary>Deletes the first matching document with automatic cancellation propagation.</summary>
    member this.DeleteOneAsync(filter: Query<'T>) : CancellableTask<bool> = deleteOne filter this

    /// <summary>Deletes all matching documents with automatic cancellation propagation.</summary>
    member this.DeleteManyAsync(filter: Query<'T>) : CancellableTask<DeleteResult> = deleteMany filter this

    // ════════════════════════════════════════════════════════════════════════
    // ATOMIC FIND-AND-MODIFY OPERATIONS
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>Atomically finds and deletes with automatic cancellation propagation.</summary>
    member this.FindOneAndDeleteAsync(filter: Query<'T>) : CancellableTask<option<Document<'T>>> =
        findOneAndDelete filter this

    /// <summary>Atomically finds and deletes with options and automatic cancellation propagation.</summary>
    member this.FindOneAndDeleteAsync(filter: Query<'T>, options: FindOptions) : CancellableTask<option<Document<'T>>> =
        findOneAndDeleteWith filter options this

    /// <summary>Atomically finds and updates with automatic cancellation propagation.</summary>
    member this.FindOneAndUpdateAsync
        (filter: Query<'T>, update: 'T -> 'T, options: FindAndModifyOptions)
        : CancellableTask<FractalResult<option<Document<'T>>>> =
        findOneAndUpdate filter update options this

    /// <summary>Atomically finds and replaces with automatic cancellation propagation.</summary>
    member this.FindOneAndReplaceAsync
        (filter: Query<'T>, doc: 'T, options: FindAndModifyOptions)
        : CancellableTask<FractalResult<option<Document<'T>>>> =
        findOneAndReplace filter doc options this

    // ════════════════════════════════════════════════════════════════════════
    // UTILITY OPERATIONS
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>Drops the collection with automatic cancellation propagation.</summary>
    member this.DropAsync() : CancellableTask<unit> = drop this

    /// <summary>Executes a translated query with automatic cancellation propagation.</summary>
    member this.ExecAsync(query: FractalDb.QueryExpr.TranslatedQuery<'T>) : CancellableTask<list<Document<'T>>> =
        exec query this
