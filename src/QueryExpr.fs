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

/// <summary>
/// Computation expression builder for LINQ-style queries using F# quotations.
/// </summary>
///
/// <remarks>
/// QueryBuilder provides a computation expression syntax for building type-safe queries
/// that are translated at runtime from F# quotations into TranslatedQuery values.
///
/// <para><strong>How It Works:</strong></para>
///
/// Unlike typical computation expressions that execute their members, QueryBuilder uses
/// F# quotations to capture the entire query structure as an abstract syntax tree (AST).
/// The Quote member captures the expression, and the Run member translates it.
///
/// All builder members (For, Yield, Where, etc.) return Unchecked.defaultof and are
/// NEVER EXECUTED. They exist only for type checking and AST structure. The actual
/// query logic is extracted from the quotation during translation.
///
/// <para><strong>Query Translation Pipeline:</strong></para>
///
/// 1. **User writes query**: query { for user in users do where (user.Age > 18) }
/// 2. **F# compiler captures quotation**: Quote member receives Expr&lt;TranslatedQuery&lt;'T&gt;&gt;
/// 3. **Run calls QueryTranslator.translate**: Walks AST, extracts predicates/sorts/projections
/// 4. **Translation produces TranslatedQuery**: Structured query object
/// 5. **QueryExecutor.execute runs query**: Converts to SQL and executes against database
/// 6. **Results returned**: seq&lt;'T&gt; or projected type
///
/// <para><strong>Supported Query Syntax:</strong></para>
///
/// - **for x in source do**: Specifies collection to query
/// - **where (predicate)**: Filters documents (can be used multiple times, combined with AND)
/// - **sortBy field**: Sort ascending by field
/// - **sortByDescending field**: Sort descending by field  
/// - **thenBy field**: Secondary sort ascending
/// - **thenByDescending field**: Secondary sort descending
/// - **skip n**: Skip first n results (pagination)
/// - **take n**: Limit to n results (pagination)
/// - **select expr**: Project results (all fields, specific fields, or single field)
///
/// <para><strong>Example Queries:</strong></para>
///
/// Simple filter:
/// <code>
/// query {
///     for user in usersCollection do
///     where (user.Age >= 18)
/// }
/// </code>
///
/// Complex query with sorting and pagination:
/// <code>
/// query {
///     for product in productsCollection do
///     where (product.Category = "electronics")
///     where (product.Price &lt; 1000.0)
///     sortBy product.Price
///     thenBy product.Name
///     skip 20
///     take 10
///     select (product.Name, product.Price)
/// }
/// </code>
///
/// Projection to single field:
/// <code>
/// query {
///     for user in usersCollection do
///     where (user.Subscribed = true)
///     select user.Email
/// }
/// </code>
///
/// <para><strong>Implementation Status:</strong></para>
///
/// - task-105: For, Yield, Quote, Run members (foundation)
/// - task-106: where CustomOperation
/// - task-107: sortBy, sortByDescending CustomOperations
/// - task-108: take, skip CustomOperations
/// - task-109: select CustomOperation
/// - task-110: count, exists, head, headOrDefault CustomOperations
/// - task-111-116: QueryTranslator implementation
/// - task-117: QueryExecutor implementation
/// - task-118: Wire Run member to translator/executor
/// </remarks>
///
/// <example>
/// <code>
/// // Basic usage with global 'query' instance
/// let adults =
///     query {
///         for user in usersCollection do
///         where (user.Age >= 18)
///     }
///
/// // Execute query
/// let! results = adults  // Returns Task&lt;seq&lt;User&gt;&gt;
///
/// // Complex query
/// let topProducts =
///     query {
///         for product in productsCollection do
///         where (product.InStock = true)
///         where (product.Rating >= 4.0)
///         sortByDescending product.Sales
///         take 10
///         select product.Name
///     }
/// </code>
/// </example>
type QueryBuilder() =
    
    /// <summary>
    /// Enables 'for x in source do' syntax in query expressions.
    /// </summary>
    ///
    /// <param name="source">The collection to query (Collection&lt;'T&gt; instance).</param>
    /// <param name="body">
    /// The body of the query expression (never executed, only analyzed in quotation).
    /// </param>
    ///
    /// <typeparam name="'T">The document type being queried.</typeparam>
    ///
    /// <returns>
    /// Unchecked.defaultof (never returns, only for type inference).
    /// </returns>
    ///
    /// <remarks>
    /// This member is NEVER EXECUTED. It exists only to enable the 'for..in..do' syntax
    /// and provide type information to the F# compiler.
    ///
    /// The actual collection source is extracted from the quotation during translation
    /// by QueryTranslator.translate. The quotation contains the collection instance,
    /// which is evaluated at runtime to get the collection name and configuration.
    ///
    /// The body function parameter is also never called - instead, its quotation
    /// representation is analyzed to extract query operations (where, sortBy, etc.).
    /// </remarks>
    ///
    /// <example>
    /// <code>
    /// // The 'for user in usersCollection do' part uses this member
    /// query {
    ///     for user in usersCollection do
    ///     where (user.Age >= 18)
    /// }
    ///
    /// // Multiple queries on different collections
    /// let userQuery = query { for user in usersCollection do where (user.Active = true) }
    /// let productQuery = query { for product in productsCollection do where (product.InStock = true) }
    /// </code>
    /// </example>
    member _.For(source: 'Collection, body: 'T -> TranslatedQuery<'T>) : TranslatedQuery<'T> =
        Unchecked.defaultof<_>
    
    /// <summary>
    /// Enables 'yield' and implicit select syntax in query expressions.
    /// </summary>
    ///
    /// <param name="value">
    /// The value to yield (never used, only for type inference).
    /// </param>
    ///
    /// <typeparam name="'T">The document type being yielded.</typeparam>
    ///
    /// <returns>
    /// Unchecked.defaultof (never returns, only for type inference).
    /// </returns>
    ///
    /// <remarks>
    /// This member is NEVER EXECUTED. It enables computation expression syntax
    /// where a query without explicit transformations returns documents as-is.
    ///
    /// In practice, most queries use the 'select' CustomOperation instead of
    /// relying on Yield, but Yield is required by the computation expression
    /// specification for proper type inference.
    ///
    /// When no 'select' is specified, the query returns complete documents.
    /// The Projection field in TranslatedQuery will be SelectAll.
    /// </remarks>
    ///
    /// <example>
    /// <code>
    /// // Implicitly uses Yield (no explicit select)
    /// query {
    ///     for user in usersCollection do
    ///     where (user.Age >= 18)
    /// }
    /// // Returns: seq&lt;User&gt; with all fields
    ///
    /// // Equivalent to:
    /// query {
    ///     for user in usersCollection do
    ///     where (user.Age >= 18)
    ///     select user  // Explicit but redundant
    /// }
    /// </code>
    /// </example>
    member _.Yield(value: 'T) : TranslatedQuery<'T> =
        Unchecked.defaultof<_>
    
    /// <summary>
    /// Captures the query expression as an F# quotation for analysis.
    /// </summary>
    ///
    /// <param name="expr">
    /// The quotation representing the entire query expression AST.
    /// </param>
    ///
    /// <typeparam name="'T">The document type being queried.</typeparam>
    ///
    /// <returns>
    /// The same quotation, unmodified (passed through to Run).
    /// </returns>
    ///
    /// <remarks>
    /// This is the KEY member that enables quotation-based query translation.
    ///
    /// When the F# compiler sees a computation expression with a Quote member,
    /// it captures the entire expression as Expr&lt;TranslatedQuery&lt;'T&gt;&gt; instead
    /// of executing the members. This quotation contains the full AST including:
    /// - Collection source
    /// - Where predicates
    /// - Sort specifications
    /// - Skip/Take values
    /// - Select projections
    ///
    /// The quotation is then passed to Run, which calls QueryTranslator.translate
    /// to walk the AST and extract query components into a TranslatedQuery record.
    ///
    /// Without Quote, computation expression members would execute normally
    /// (returning Unchecked.defaultof, which would fail). Quote prevents execution
    /// and enables compile-time AST capture with runtime analysis.
    /// </remarks>
    ///
    /// <example>
    /// <code>
    /// // User writes this query
    /// query {
    ///     for user in usersCollection do
    ///     where (user.Age >= 18)
    ///     select user.Email
    /// }
    ///
    /// // Quote captures it as (simplified):
    /// Expr&lt;TranslatedQuery&lt;User&gt;&gt; containing:
    ///   Call(For,
    ///     [usersCollection;
    ///      Lambda(user, Call(Where,
    ///        [Lambda(user, Call(op_GreaterThanOrEqual, [user.Age; 18]));
    ///         Call(Select, [Lambda(user, user.Email)])]))])
    ///
    /// // This quotation is passed to Run for translation
    /// </code>
    /// </example>
    member _.Quote(expr: Microsoft.FSharp.Quotations.Expr<TranslatedQuery<'T>>) : Microsoft.FSharp.Quotations.Expr<TranslatedQuery<'T>> =
        expr
    
    /// <summary>
    /// Translates the captured quotation into a TranslatedQuery.
    /// </summary>
    ///
    /// <param name="expr">
    /// The quotation captured by Quote, containing the query AST.
    /// </param>
    ///
    /// <typeparam name="'T">The document type being queried.</typeparam>
    ///
    /// <returns>
    /// A TranslatedQuery record ready for execution.
    /// </returns>
    ///
    /// <remarks>
    /// This is the final step in the computation expression pipeline and the
    /// entry point for query translation.
    ///
    /// <para><strong>Translation Process:</strong></para>
    ///
    /// 1. **Receive quotation from Quote**: Full AST of query expression
    /// 2. **Call QueryTranslator.translate**: Walk AST and extract components
    /// 3. **Build TranslatedQuery**: Structured query with Source, Where, OrderBy, etc.
    /// 4. **Return TranslatedQuery**: Ready for QueryExecutor.execute
    ///
    /// <para><strong>Current Implementation (task-105):</strong></para>
    ///
    /// Returns Unchecked.defaultof as a placeholder. The actual implementation
    /// will be added in task-118 after QueryTranslator and QueryExecutor are
    /// implemented (tasks 111-117).
    ///
    /// <para><strong>Final Implementation (task-118):</strong></para>
    ///
    /// Will call QueryTranslator.translate to convert the quotation into a
    /// TranslatedQuery, then return that query for execution.
    ///
    /// The result type will change from TranslatedQuery&lt;'T&gt; to Task&lt;seq&lt;'T&gt;&gt;
    /// once QueryExecutor.execute is integrated.
    /// </remarks>
    ///
    /// <example>
    /// <code>
    /// // User code (task-105, current):
    /// let translatedQuery =
    ///     query {
    ///         for user in usersCollection do
    ///         where (user.Age >= 18)
    ///     }
    /// // Returns: TranslatedQuery&lt;User&gt; (placeholder, not functional yet)
    ///
    /// // After task-118 (final):
    /// let translatedQuery =
    ///     query {
    ///         for user in usersCollection do
    ///         where (user.Age >= 18)
    ///     }
    /// // Returns: TranslatedQuery with:
    /// //   Source = "users"
    /// //   Where = Some (Query.Field("age", FieldOp.Compare(box (CompareOp.Gte 18))))
    /// //   OrderBy = []
    /// //   Skip = None
    /// //   Take = None
    /// //   Projection = SelectAll
    /// </code>
    /// </example>
    member _.Run(expr: Microsoft.FSharp.Quotations.Expr<TranslatedQuery<'T>>) : TranslatedQuery<'T> =
        raise (System.NotImplementedException("QueryBuilder.Run will be implemented in task-118"))

/// <summary>
/// Module providing the global 'query' computation expression instance.
/// </summary>
///
/// <remarks>
/// This module is marked with [&lt;AutoOpen&gt;] so the 'query' instance is automatically
/// available whenever FractalDb.QueryExpr is opened.
///
/// Users can write:
/// <code>
/// open FractalDb.QueryExpr
///
/// let myQuery = query { for user in users do where (user.Age > 18) }
/// </code>
///
/// Without needing to instantiate QueryBuilder() themselves.
///
/// The global instance approach is standard for F# computation expressions:
/// - async { } uses global 'async' instance
/// - seq { } uses global 'seq' instance  
/// - task { } uses global 'task' instance
/// - query { } uses this global 'query' instance
/// </remarks>
[<AutoOpen>]
module QueryBuilderInstance =
    /// <summary>
    /// Global instance of QueryBuilder for use in query expressions.
    /// </summary>
    ///
    /// <remarks>
    /// This is the entry point for all LINQ-style queries in FractalDb.
    ///
    /// Usage:
    /// <code>
    /// query { for x in collection do ... }
    /// </code>
    ///
    /// The 'query' keyword is bound to this instance. When you write a query
    /// expression, the F# compiler translates it into method calls on this builder
    /// instance, wrapped in quotations due to the Quote member.
    /// </remarks>
    ///
    /// <example>
    /// <code>
    /// open FractalDb
    /// open FractalDb.QueryExpr
    ///
    /// task {
    ///     use! db = FractalDb.Open("data.db")
    ///     let users = db.Collection&lt;User&gt;("users")
    ///
    ///     // Use global 'query' instance
    ///     let translatedQuery =
    ///         query {
    ///             for user in users do
    ///             where (user.Age >= 18)
    ///             where (user.Active = true)
    ///             sortBy user.Name
    ///             take 10
    ///         }
    ///
    ///     return translatedQuery
    /// }
    /// </code>
    /// </example>
    let query = QueryBuilder()
