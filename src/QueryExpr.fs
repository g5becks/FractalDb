module FractalDb.QueryExpr

open FractalDb.Operators

/// <summary>
/// Sort direction for query result ordering.
/// </summary>
///
/// <remarks>
/// Specifies whether query results should be sorted in ascending or descending order.
/// Used in conjunction with field names to define sort order in TranslatedQuery.
/// </remarks>
///
/// <example>
/// <code>
/// // Sort by age ascending
/// ("age", SortDirection.Asc)
///
/// // Sort by createdAt descending
/// ("createdAt", SortDirection.Desc)
///
/// // Multi-field sort: first by status ascending, then by createdAt descending
/// [("status", SortDirection.Asc); ("createdAt", SortDirection.Desc)]
/// </code>
/// </example>
[<RequireQualifiedAccess>]
type SortDirection =
    /// <summary>
    /// Ascending order (smallest to largest, A-Z, oldest to newest).
    /// </summary>
    | Asc

    /// <summary>
    /// Descending order (largest to smallest, Z-A, newest to oldest).
    /// </summary>
    | Desc

/// <summary>
/// Projection configuration for controlling which fields are returned in query results.
/// </summary>
///
/// <remarks>
/// Projection allows you to control the shape of query results:
///
/// - SelectAll: Returns complete documents with all fields
/// - SelectFields: Returns only specified fields (similar to SQL SELECT field1, field2)
/// - SelectSingle: Returns only a single field value (unwrapped from document structure)
///
/// Projections improve performance by reducing data transfer and serialization overhead.
/// The _id field behavior:
/// - SelectAll: _id is always included
/// - SelectFields: _id is included unless explicitly omitted
/// - SelectSingle: _id is not included (only the selected field)
/// </remarks>
///
/// <example>
/// <code>
/// // Return all fields
/// SelectAll
/// // Result: { _id: "123", name: "Alice", email: "alice@example.com", age: 30 }
///
/// // Return specific fields only
/// SelectFields ["name"; "email"]
/// // Result: { _id: "123", name: "Alice", email: "alice@example.com" }
///
/// // Return single field value (unwrapped)
/// SelectSingle "email"
/// // Result: "alice@example.com" (not wrapped in an object)
/// </code>
/// </example>
type Projection =
    /// <summary>
    /// Returns complete documents with all fields included.
    /// </summary>
    ///
    /// <remarks>
    /// This is the default behavior when no projection is specified.
    /// All document fields are returned, including metadata fields (_id, _createdAt, _updatedAt).
    ///
    /// Use SelectAll when:
    /// - You need the complete document
    /// - The document is small
    /// - You're storing the document for later use
    /// </remarks>
    ///
    /// <example>
    /// <code>
    /// // Query expression with no select clause defaults to SelectAll
    /// query {
    ///     for user in usersCollection do
    ///     where (user.Age >= 18)
    /// }
    /// </code>
    /// </example>
    | SelectAll

    /// <summary>
    /// Returns only the specified fields (whitelist projection).
    /// </summary>
    ///
    /// <param name="fields">List of field names to include in results.</param>
    ///
    /// <remarks>
    /// Only the specified fields are returned, plus _id by default.
    /// All other fields are excluded from the result documents.
    ///
    /// Field names:
    /// - Simple fields: "name", "email", "age"
    /// - Nested fields: "address.city", "user.profile.avatar"
    /// - Array fields: "tags", "items"
    ///
    /// Performance benefits:
    /// - Reduces memory usage by excluding unused fields
    /// - Reduces network transfer size
    /// - Reduces JSON serialization/deserialization overhead
    ///
    /// Use SelectFields when:
    /// - You only need specific fields from large documents
    /// - Building API responses with partial data
    /// - Excluding sensitive fields (use Omit for blacklist projection)
    /// </remarks>
    ///
    /// <example>
    /// <code>
    /// // Select name and email only
    /// SelectFields ["name"; "email"]
    /// // Result: { _id: "123", name: "Alice", email: "alice@example.com" }
    ///
    /// // Select nested fields
    /// SelectFields ["user.name"; "user.email"; "order.total"]
    /// // Result: { _id: "123", user: { name: "Alice", email: "..." }, order: { total: 99.99 } }
    ///
    /// // Query expression syntax
    /// query {
    ///     for user in usersCollection do
    ///     select (user.Name, user.Email)
    /// }
    /// // Translates to: SelectFields ["name"; "email"]
    /// </code>
    /// </example>
    | SelectFields of list<string>

    /// <summary>
    /// Returns a single field value, unwrapped from the document structure.
    /// </summary>
    ///
    /// <param name="field">The field name to extract.</param>
    ///
    /// <remarks>
    /// Instead of returning an object with the field, returns just the field value.
    /// This is useful when you need a list of values rather than a list of objects.
    ///
    /// The result type changes:
    /// - Without SelectSingle: seq&lt;{ _id: string; field: 'T }&gt;
    /// - With SelectSingle: seq&lt;'T&gt;
    ///
    /// Use SelectSingle when:
    /// - Building a list of IDs: ["id1"; "id2"; "id3"]
    /// - Building a list of names: ["Alice"; "Bob"; "Charlie"]
    /// - Extracting a single computed value
    ///
    /// Note: If the field doesn't exist on a document, the value will be null/default.
    /// </remarks>
    ///
    /// <example>
    /// <code>
    /// // Select just email addresses
    /// SelectSingle "email"
    /// // Result: seq ["alice@example.com"; "bob@example.com"; "charlie@example.com"]
    ///
    /// // Select just IDs
    /// SelectSingle "_id"
    /// // Result: seq ["123"; "456"; "789"]
    ///
    /// // Query expression syntax
    /// query {
    ///     for user in usersCollection do
    ///     select user.Email
    /// }
    /// // Translates to: SelectSingle "email"
    /// </code>
    /// </example>
    | SelectSingle of string

/// <summary>
/// Represents a translated query ready for execution against the database.
/// </summary>
///
/// <typeparam name="'T">The document type being queried.</typeparam>
///
/// <remarks>
/// TranslatedQuery is an intermediate representation created by translating
/// F# query expressions (quotations) into a structured query object that can
/// be executed against the database.
///
/// <para><strong>Query Execution Pipeline:</strong></para>
///
/// 1. **User writes query expression**: Using query { } syntax
/// 2. **F# compiler captures quotation**: AST representation of the query
/// 3. **QueryTranslator.translate**: Converts quotation to TranslatedQuery
/// 4. **QueryExecutor.execute**: Executes TranslatedQuery against database
/// 5. **Results returned**: seq&lt;'T&gt; or projected type
///
/// <para><strong>Field Semantics:</strong></para>
///
/// - **Source**: The collection name to query (e.g., "users")
/// - **Where**: Optional filter predicate (Query&lt;'T&gt;), None means no filtering
/// - **OrderBy**: Sort specifications, applied in list order (primary, secondary, ...)
/// - **Skip**: Offset for pagination (0-based), None means start from beginning
/// - **Take**: Limit result count, None means return all matching documents
/// - **Projection**: Field selection, SelectAll means return complete documents
///
/// <para><strong>Query Composition:</strong></para>
///
/// Multiple where clauses are combined with AND logic.
/// Sort order precedence matches list order.
/// Skip/Take enable offset-based pagination.
///
/// <para><strong>Execution Model:</strong></para>
///
/// TranslatedQuery is immutable and can be:
/// - Executed multiple times
/// - Modified and re-executed
/// - Cached for reuse
/// - Serialized for logging/debugging
/// </remarks>
///
/// <example>
/// <code>
/// // Simple query: all adult users
/// {
///     Source = "users"
///     Where = Some (Query.Field("age", FieldOp.Compare(box (CompareOp.Gte 18))))
///     OrderBy = []
///     Skip = None
///     Take = None
///     Projection = SelectAll
/// }
///
/// // Complex query: paginated, sorted, filtered
/// {
///     Source = "products"
///     Where = Some (Query.And [
///         Query.Field("category", FieldOp.Compare(box (CompareOp.Eq "electronics")))
///         Query.Field("price", FieldOp.Compare(box (CompareOp.Lt 1000.0)))
///     ])
///     OrderBy = [("price", SortDirection.Asc); ("name", SortDirection.Asc)]
///     Skip = Some 20
///     Take = Some 10
///     Projection = SelectFields ["name"; "price"; "imageUrl"]
/// }
/// // Returns: 10 electronics under $1000, sorted by price then name, page 3 (items 21-30)
///
/// // Projection query: email list
/// {
///     Source = "users"
///     Where = Some (Query.Field("subscribed", FieldOp.Compare(box (CompareOp.Eq true))))
///     OrderBy = []
///     Skip = None
///     Take = None
///     Projection = SelectSingle "email"
/// }
/// // Returns: seq&lt;string&gt; of email addresses
/// </code>
/// </example>
type TranslatedQuery<'T> =
    {
        /// <summary>
        /// The name of the collection to query.
        /// </summary>
        ///
        /// <remarks>
        /// Extracted from the 'for x in collection do' clause of the query expression.
        /// Must match an existing collection name in the database.
        ///
        /// Examples: "users", "products", "orders"
        /// </remarks>
        Source: string

        /// <summary>
        /// Optional filter predicate to apply to documents.
        /// </summary>
        ///
        /// <remarks>
        /// Represents the WHERE clause of the query. Built from query expression predicates.
        /// None means no filtering (return all documents).
        ///
        /// Multiple where clauses in the query expression are combined with Query.And.
        /// Supports:
        /// - Comparison operators: =, &lt;&gt;, &gt;, &gt;=, &lt;, &lt;=
        /// - Logical operators: &amp;&amp;, ||, not
        /// - String methods: Contains, StartsWith, EndsWith
        /// - Array operators: All, Any, Size
        /// </remarks>
        Where: option<Query<'T>>

        /// <summary>
        /// List of sort specifications in precedence order.
        /// </summary>
        ///
        /// <remarks>
        /// Each tuple is (fieldName, direction). Sort order:
        /// 1. First tuple: primary sort
        /// 2. Second tuple: secondary sort (for ties in primary)
        /// 3. And so on...
        ///
        /// Empty list means no sorting (database order, typically insertion order).
        ///
        /// Corresponds to sortBy/sortByDescending/thenBy/thenByDescending in query expressions.
        /// </remarks>
        OrderBy: list<string * SortDirection>

        /// <summary>
        /// Number of documents to skip for pagination.
        /// </summary>
        ///
        /// <remarks>
        /// Offset-based pagination. None means start from the beginning.
        /// Some n skips the first n documents after filtering and sorting.
        ///
        /// Performance note: Skip is O(n) - the database must scan through
        /// skipped documents. For large offsets, consider cursor-based pagination.
        ///
        /// Page calculation: Skip = pageNumber * pageSize
        /// </remarks>
        Skip: option<int>

        /// <summary>
        /// Maximum number of documents to return.
        /// </summary>
        ///
        /// <remarks>
        /// Limits the result set size. None means return all matching documents.
        /// Some n returns at most n documents.
        ///
        /// Applied after Skip, so Take = 10 with Skip = 20 returns items 21-30.
        ///
        /// Best practice: Always set Take to prevent unbounded result sets.
        /// Typical values: 10-100 for UI pagination, 1 for "find one" operations.
        /// </remarks>
        Take: option<int>

        /// <summary>
        /// Field projection configuration.
        /// </summary>
        ///
        /// <remarks>
        /// Controls which fields are returned in the results.
        /// Corresponds to the 'select' clause in query expressions.
        ///
        /// - SelectAll: Return complete documents (default)
        /// - SelectFields: Return only specified fields
        /// - SelectSingle: Return unwrapped single field values
        ///
        /// Projections reduce memory usage and improve performance by excluding unused fields.
        /// </remarks>
        Projection: Projection
    }
