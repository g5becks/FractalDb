module rec FractalDb.QueryExpr

open System
open FractalDb.Operators
open FSharp.Quotations
open FSharp.Quotations.Patterns
open FSharp.Quotations.DerivedPatterns

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
    member _.For(source: 'Collection, body: 'T -> TranslatedQuery<'T>) : TranslatedQuery<'T> = Unchecked.defaultof<_>

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
    member _.Yield(value: 'T) : TranslatedQuery<'T> = Unchecked.defaultof<_>

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
    member _.Quote
        (expr: Microsoft.FSharp.Quotations.Expr<TranslatedQuery<'T>>)
        : Microsoft.FSharp.Quotations.Expr<TranslatedQuery<'T>> =
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
    /// 4. **Return TranslatedQuery**: Ready for execution via Collection methods
    ///
    /// <para><strong>Implementation:</strong></para>
    ///
    /// Calls QueryTranslator.translate to convert the quotation into a
    /// TranslatedQuery record containing all query components (Source, Where,
    /// OrderBy, Skip, Take, Projection). The returned TranslatedQuery can then
    /// be passed to Collection.Find or other query execution methods.
    ///
    /// <para><strong>Note on module rec:</strong></para>
    ///
    /// This module uses F#'s recursive module feature (module rec) to allow
    /// QueryBuilder.Run to call QueryTranslator.translate, which is defined
    /// later in the same file but needs to reference QueryBuilder for its
    /// SpecificCall pattern matching.
    /// </remarks>
    ///
    /// <example>
    /// <code>
    /// // Query expression usage:
    /// let translatedQuery =
    ///     query {
    ///         for user in usersCollection do
    ///         where (user.Age >= 18)
    ///         sortBy user.Name
    ///         take 10
    ///     }
    /// // Returns: TranslatedQuery with:
    /// //   Source = "users"
    /// //   Where = Some (Query.Field("age", FieldOp.Compare(box (CompareOp.Gte 18))))
    /// //   OrderBy = [("name", SortDirection.Asc)]
    /// //   Skip = None
    /// //   Take = Some 10
    /// //   Projection = SelectAll
    /// </code>
    /// </example>
    member _.Run(expr: Microsoft.FSharp.Quotations.Expr<TranslatedQuery<'T>>) : TranslatedQuery<'T> =
        QueryTranslator.translate expr

    // ============================================================
    // CUSTOM OPERATIONS
    // ============================================================

    /// <summary>
    /// Enables 'where (predicate)' filtering syntax in query expressions.
    /// </summary>
    ///
    /// <param name="source">The source sequence (from the 'for..in' clause).</param>
    /// <param name="predicate">
    /// A boolean predicate function that filters documents.
    /// The lambda expression (fun x -> x.Field op value) is analyzed in the quotation.
    /// </param>
    ///
    /// <typeparam name="'T">The document type being filtered.</typeparam>
    ///
    /// <returns>
    /// Unchecked.defaultof (never returns, only for type inference).
    /// </returns>
    ///
    /// <remarks>
    /// This member is NEVER EXECUTED. The predicate is captured in the quotation
    /// and translated by QueryTranslator.translatePredicate (tasks 112-114).
    ///
    /// <para><strong>MaintainsVariableSpace = true:</strong></para>
    ///
    /// This attribute keeps the iteration variable (e.g., 'user') in scope after
    /// the where clause, allowing subsequent operations to reference it:
    /// <code>
    /// query {
    ///     for user in users do
    ///     where (user.Age > 18)    // 'user' variable defined here
    ///     sortBy user.Name         // 'user' still in scope
    /// }
    /// </code>
    ///
    /// <para><strong>ProjectionParameter:</strong></para>
    ///
    /// This attribute enables lambda syntax for the predicate. The compiler
    /// captures the lambda as a quotation expression tree, which is then
    /// analyzed to extract field names, operators, and values.
    ///
    /// <para><strong>Supported Predicate Syntax:</strong></para>
    ///
    /// **Comparison operators** (task-112):
    /// - Equality: x.Field = value
    /// - Inequality: x.Field &lt;&gt; value
    /// - Greater than: x.Field &gt; value
    /// - Greater or equal: x.Field &gt;= value
    /// - Less than: x.Field &lt; value
    /// - Less or equal: x.Field &lt;= value
    ///
    /// **Logical operators** (task-113):
    /// - AND: predicate1 &amp;&amp; predicate2
    /// - OR: predicate1 || predicate2
    /// - NOT: not predicate
    ///
    /// **String methods** (task-114):
    /// - Contains: x.Field.Contains("text")
    /// - StartsWith: x.Field.StartsWith("prefix")
    /// - EndsWith: x.Field.EndsWith("suffix")
    ///
    /// **Nested fields:**
    /// - Dot notation: x.Address.City = "Seattle"
    /// - Deep nesting: x.User.Profile.Email.Contains("@gmail.com")
    ///
    /// <para><strong>Multiple Where Clauses:</strong></para>
    ///
    /// Multiple where clauses are combined with AND logic by QueryTranslator:
    /// <code>
    /// query {
    ///     for user in users do
    ///     where (user.Age >= 18)
    ///     where (user.Status = "active")
    /// }
    /// // Equivalent to: WHERE age >= 18 AND status = 'active'
    /// </code>
    ///
    /// <para><strong>Translation Example:</strong></para>
    ///
    /// User writes:
    /// <code>
    /// where (user.Age >= 18 &amp;&amp; user.Email.Contains("@gmail.com"))
    /// </code>
    ///
    /// Captured quotation (simplified):
    /// <code>
    /// Call(op_BooleanAnd,
    ///   [Call(op_GreaterThanOrEqual, [PropertyGet(user, "Age"), Value(18)]);
    ///    Call(PropertyGet(user, "Email"), "Contains", [Value("@gmail.com")])])
    /// </code>
    ///
    /// Translated to:
    /// <code>
    /// Query.And [
    ///     Query.Field("age", FieldOp.Compare(box (CompareOp.Gte 18)))
    ///     Query.Field("email", FieldOp.String(StringOp.Contains "@gmail.com"))
    /// ]
    /// </code>
    /// </remarks>
    ///
    /// <example>
    /// <code>
    /// // Simple comparison
    /// query {
    ///     for user in users do
    ///     where (user.Age >= 18)
    /// }
    ///
    /// // Logical operators
    /// query {
    ///     for user in users do
    ///     where (user.Age >= 18 &amp;&amp; user.Age &lt;= 65)
    /// }
    ///
    /// // String methods
    /// query {
    ///     for user in users do
    ///     where (user.Email.Contains("@gmail.com"))
    /// }
    ///
    /// // Nested fields
    /// query {
    ///     for user in users do
    ///     where (user.Address.City = "Seattle")
    /// }
    ///
    /// // Multiple where clauses (AND)
    /// query {
    ///     for product in products do
    ///     where (product.Category = "electronics")
    ///     where (product.Price &lt; 1000.0)
    ///     where (product.InStock = true)
    /// }
    /// // Equivalent to: category = 'electronics' AND price < 1000 AND inStock = true
    ///
    /// // Complex nested logic
    /// query {
    ///     for user in users do
    ///     where ((user.Age >= 18 &amp;&amp; user.Age &lt;= 65) || user.IsAdmin = true)
    /// }
    /// </code>
    /// </example>
    [<CustomOperation("where", MaintainsVariableSpace = true)>]
    member _.Where(source: TranslatedQuery<'T>, [<ProjectionParameter>] predicate: 'T -> bool) : TranslatedQuery<'T> =
        Unchecked.defaultof<_>

    /// <summary>
    /// Enables 'sortBy field' syntax for ascending sort order.
    /// </summary>
    ///
    /// <param name="source">The source sequence from previous operations.</param>
    /// <param name="keySelector">
    /// Lambda expression selecting the field to sort by (e.g., fun x -> x.Name).
    /// Analyzed in quotation to extract field name.
    /// </param>
    ///
    /// <typeparam name="'T">The document type being sorted.</typeparam>
    /// <typeparam name="'Key">The type of the sort key field.</typeparam>
    ///
    /// <returns>
    /// Unchecked.defaultof (never returns, only for type inference).
    /// </returns>
    ///
    /// <remarks>
    /// This member is NEVER EXECUTED. The keySelector is captured in the quotation
    /// and translated by QueryTranslator.extractPropertyName to get the field name,
    /// which is added to the OrderBy list with SortDirection.Asc.
    ///
    /// <para><strong>Sort Precedence:</strong></para>
    ///
    /// sortBy establishes the primary sort order. Use thenBy/thenByDescending for
    /// secondary sorts when primary sort values are equal.
    ///
    /// <para><strong>Sort Behavior:</strong></para>
    /// - Text: Alphabetical (A-Z), case-sensitive
    /// - Numbers: Numeric order (1, 2, 10, 20)
    /// - Dates: Chronological (oldest to newest)
    /// - NULL: Appears first (SQLite default)
    ///
    /// <para><strong>Field Name Extraction:</strong></para>
    ///
    /// The keySelector lambda (fun x -> x.Field) is captured as a quotation:
    /// - Lambda(x, PropertyGet(x, "Field"))
    /// - Extracted field name: "field" (camelCase)
    /// - Added to OrderBy: [("field", SortDirection.Asc)]
    /// </remarks>
    ///
    /// <example>
    /// <code>
    /// // Sort by name ascending
    /// query {
    ///     for user in users do
    ///     sortBy user.Name
    /// }
    ///
    /// // Sort with secondary field (use thenBy)
    /// query {
    ///     for user in users do
    ///     sortBy user.LastName
    ///     thenBy user.FirstName
    /// }
    /// // Sort order: LastName asc, then FirstName asc for ties
    ///
    /// // Sort nested field
    /// query {
    ///     for user in users do
    ///     sortBy user.Address.City
    /// }
    /// </code>
    /// </example>
    [<CustomOperation("sortBy", MaintainsVariableSpace = true)>]
    member _.SortBy
        (source: TranslatedQuery<'T>, [<ProjectionParameter>] keySelector: 'T -> 'Key)
        : TranslatedQuery<'T> =
        Unchecked.defaultof<_>

    /// <summary>
    /// Enables 'sortByDescending field' syntax for descending sort order.
    /// </summary>
    ///
    /// <param name="source">The source sequence from previous operations.</param>
    /// <param name="keySelector">
    /// Lambda expression selecting the field to sort by (e.g., fun x -> x.CreatedAt).
    /// Analyzed in quotation to extract field name.
    /// </param>
    ///
    /// <typeparam name="'T">The document type being sorted.</typeparam>
    /// <typeparam name="'Key">The type of the sort key field.</typeparam>
    ///
    /// <returns>
    /// Unchecked.defaultof (never returns, only for type inference).
    /// </returns>
    ///
    /// <remarks>
    /// This member is NEVER EXECUTED. Same as sortBy but produces SortDirection.Desc.
    ///
    /// <para><strong>Sort Behavior:</strong></para>
    /// - Text: Reverse alphabetical (Z-A)
    /// - Numbers: Descending (20, 10, 2, 1)
    /// - Dates: Reverse chronological (newest to oldest)
    /// - NULL: Still appears first (SQLite default, regardless of direction)
    ///
    /// Common use cases:
    /// - Recent items first: sortByDescending x.CreatedAt
    /// - Highest scores: sortByDescending x.Score
    /// - Most expensive: sortByDescending x.Price
    /// </remarks>
    ///
    /// <example>
    /// <code>
    /// // Recent posts first
    /// query {
    ///     for post in posts do
    ///     sortByDescending post.CreatedAt
    ///     take 10
    /// }
    ///
    /// // Highest rated products
    /// query {
    ///     for product in products do
    ///     where (product.InStock = true)
    ///     sortByDescending product.Rating
    ///     thenBy product.Price
    /// }
    /// // Order: Rating desc, then Price asc for ties
    /// </code>
    /// </example>
    [<CustomOperation("sortByDescending", MaintainsVariableSpace = true)>]
    member _.SortByDescending
        (source: TranslatedQuery<'T>, [<ProjectionParameter>] keySelector: 'T -> 'Key)
        : TranslatedQuery<'T> =
        Unchecked.defaultof<_>

    /// <summary>
    /// Enables 'thenBy field' syntax for secondary ascending sort.
    /// </summary>
    ///
    /// <param name="source">The source sequence with existing sort order.</param>
    /// <param name="keySelector">
    /// Lambda expression selecting the secondary sort field.
    /// </param>
    ///
    /// <typeparam name="'T">The document type being sorted.</typeparam>
    /// <typeparam name="'Key">The type of the sort key field.</typeparam>
    ///
    /// <returns>
    /// Unchecked.defaultof (never returns, only for type inference).
    /// </returns>
    ///
    /// <remarks>
    /// This member is NEVER EXECUTED. Adds a secondary sort field to break ties
    /// in the primary sort.
    ///
    /// <para><strong>Multi-Level Sorting:</strong></para>
    ///
    /// Sort order is determined by the sequence of sort operations:
    /// 1. sortBy/sortByDescending: Primary sort
    /// 2. thenBy/thenByDescending: Secondary sort (for ties)
    /// 3. Additional thenBy/thenByDescending: Tertiary sort, etc.
    ///
    /// <para><strong>SQL Translation:</strong></para>
    ///
    /// <code>
    /// sortBy user.LastName
    /// thenBy user.FirstName
    /// thenByDescending user.Age
    /// </code>
    ///
    /// Translates to:
    /// <code>
    /// ORDER BY lastName ASC, firstName ASC, age DESC
    /// </code>
    ///
    /// <para><strong>Important:</strong></para>
    ///
    /// Must follow a sortBy or sortByDescending. Cannot be the first sort operation.
    /// </remarks>
    ///
    /// <example>
    /// <code>
    /// // Sort by last name, then first name for ties
    /// query {
    ///     for user in users do
    ///     sortBy user.LastName
    ///     thenBy user.FirstName
    /// }
    ///
    /// // Complex multi-level sort
    /// query {
    ///     for product in products do
    ///     sortBy product.Category
    ///     thenByDescending product.Rating
    ///     thenBy product.Price
    ///     thenBy product.Name
    /// }
    /// // Order: Category asc, Rating desc, Price asc, Name asc
    /// </code>
    /// </example>
    [<CustomOperation("thenBy", MaintainsVariableSpace = true)>]
    member _.ThenBy
        (source: TranslatedQuery<'T>, [<ProjectionParameter>] keySelector: 'T -> 'Key)
        : TranslatedQuery<'T> =
        Unchecked.defaultof<_>

    /// <summary>
    /// Enables 'thenByDescending field' syntax for secondary descending sort.
    /// </summary>
    ///
    /// <param name="source">The source sequence with existing sort order.</param>
    /// <param name="keySelector">
    /// Lambda expression selecting the secondary sort field.
    /// </param>
    ///
    /// <typeparam name="'T">The document type being sorted.</typeparam>
    /// <typeparam name="'Key">The type of the sort key field.</typeparam>
    ///
    /// <returns>
    /// Unchecked.defaultof (never returns, only for type inference).
    /// </returns>
    ///
    /// <remarks>
    /// This member is NEVER EXECUTED. Same as thenBy but produces descending order
    /// for the secondary sort field.
    ///
    /// Common use case: Primary sort ascending, but ties broken by most recent:
    /// <code>
    /// sortBy user.Status          // "active", "inactive", "pending"
    /// thenByDescending user.LastLogin  // Most recent first within each status
    /// </code>
    ///
    /// Must follow a sortBy or sortByDescending operation.
    /// </remarks>
    ///
    /// <example>
    /// <code>
    /// // Products by category, highest rated first within category
    /// query {
    ///     for product in products do
    ///     sortBy product.Category
    ///     thenByDescending product.Rating
    /// }
    ///
    /// // Users by department, most recent hires first
    /// query {
    ///     for user in users do
    ///     sortBy user.Department
    ///     thenByDescending user.HireDate
    ///     thenBy user.LastName
    /// }
    /// </code>
    /// </example>
    [<CustomOperation("thenByDescending", MaintainsVariableSpace = true)>]
    member _.ThenByDescending
        (source: TranslatedQuery<'T>, [<ProjectionParameter>] keySelector: 'T -> 'Key)
        : TranslatedQuery<'T> =
        Unchecked.defaultof<_>

    /// <summary>
    /// Enables 'take n' syntax to limit the number of results returned.
    /// </summary>
    ///
    /// <param name="source">The source sequence from previous operations.</param>
    /// <param name="count">Maximum number of results to return.</param>
    ///
    /// <typeparam name="'T">The document type.</typeparam>
    ///
    /// <returns>
    /// Unchecked.defaultof (never returns, only for type inference).
    /// </returns>
    ///
    /// <remarks>
    /// This member is NEVER EXECUTED. The count value is extracted from the quotation
    /// and stored in TranslatedQuery.Take.
    ///
    /// <para><strong>Execution Order:</strong></para>
    ///
    /// Take is applied AFTER:
    /// - Filtering (where)
    /// - Sorting (sortBy/sortByDescending)
    /// - Skipping (skip)
    ///
    /// So: WHERE ... ORDER BY ... OFFSET skip LIMIT take
    ///
    /// <para><strong>Pagination Pattern:</strong></para>
    ///
    /// Combine skip and take for offset-based pagination:
    /// <code>
    /// let pageSize = 20
    /// let pageNumber = 3
    /// skip (pageNumber * pageSize)  // Skip 60 items (pages 1-3)
    /// take pageSize                  // Return 20 items (page 4)
    /// </code>
    ///
    /// <para><strong>Performance:</strong></para>
    ///
    /// Always use take to limit result sets. Without it, queries may return
    /// thousands of documents, consuming excessive memory and bandwidth.
    ///
    /// Typical values:
    /// - UI pagination: 10-100
    /// - API responses: 50-200
    /// - "Find one": 1
    /// </remarks>
    ///
    /// <example>
    /// <code>
    /// // Top 10 results
    /// query {
    ///     for user in users do
    ///     sortByDescending user.Score
    ///     take 10
    /// }
    ///
    /// // Pagination: page 3, 20 items per page
    /// query {
    ///     for product in products do
    ///     where (product.Category = "electronics")
    ///     sortBy product.Price
    ///     skip 40
    ///     take 20
    /// }
    ///
    /// // Find first matching result
    /// query {
    ///     for user in users do
    ///     where (user.Email = "alice@example.com")
    ///     take 1
    /// }
    /// </code>
    /// </example>
    [<CustomOperation("take", MaintainsVariableSpace = true)>]
    member _.Take(source: TranslatedQuery<'T>, count: int) : TranslatedQuery<'T> = Unchecked.defaultof<_>

    /// <summary>
    /// Enables 'skip n' syntax to skip the first n results (offset pagination).
    /// </summary>
    ///
    /// <param name="source">The source sequence from previous operations.</param>
    /// <param name="count">Number of results to skip.</param>
    ///
    /// <typeparam name="'T">The document type.</typeparam>
    ///
    /// <returns>
    /// Unchecked.defaultof (never returns, only for type inference).
    /// </returns>
    ///
    /// <remarks>
    /// This member is NEVER EXECUTED. The count value is extracted from the quotation
    /// and stored in TranslatedQuery.Skip.
    ///
    /// <para><strong>Execution Order:</strong></para>
    ///
    /// Skip is applied AFTER filtering and sorting, BEFORE take:
    /// WHERE ... ORDER BY ... OFFSET skip LIMIT take
    ///
    /// <para><strong>Pagination Calculation:</strong></para>
    ///
    /// To get page N (1-indexed) with page size P:
    /// <code>
    /// skip ((N - 1) * P)
    /// take P
    /// </code>
    ///
    /// Examples:
    /// - Page 1: skip 0, take 20
    /// - Page 2: skip 20, take 20
    /// - Page 3: skip 40, take 20
    ///
    /// <para><strong>Performance Warning:</strong></para>
    ///
    /// Skip uses OFFSET in SQL, which scans through the first N rows even though
    /// they're discarded. This is O(n) - slow for large offsets.
    ///
    /// For large offsets (N > 1000):
    /// - Consider cursor-based pagination instead
    /// - Or use "where (id > lastSeenId)" keyset pagination
    ///
    /// <para><strong>Page Drift Issue:</strong></para>
    ///
    /// If data changes between requests (inserts/deletes), offset pagination
    /// may skip items or show duplicates. Cursor pagination avoids this.
    /// </remarks>
    ///
    /// <example>
    /// <code>
    /// // Page 1: First 20 results
    /// query {
    ///     for user in users do
    ///     sortBy user.Name
    ///     take 20
    /// }
    ///
    /// // Page 2: Results 21-40
    /// query {
    ///     for user in users do
    ///     sortBy user.Name
    ///     skip 20
    ///     take 20
    /// }
    ///
    /// // Page 3: Results 41-60
    /// query {
    ///     for user in users do
    ///     sortBy user.Name
    ///     skip 40
    ///     take 20
    /// }
    ///
    /// // Skip first 100, get next 50
    /// query {
    ///     for product in products do
    ///     where (product.InStock = true)
    ///     sortByDescending product.CreatedAt
    ///     skip 100
    ///     take 50
    /// }
    /// </code>
    /// </example>
    [<CustomOperation("skip", MaintainsVariableSpace = true)>]
    member _.Skip(source: TranslatedQuery<'T>, count: int) : TranslatedQuery<'T> = Unchecked.defaultof<_>

    /// <summary>
    /// Enables 'select projection' syntax for field projection and result transformation.
    /// </summary>
    ///
    /// <param name="source">The source sequence from previous operations.</param>
    /// <param name="projection">
    /// Lambda expression defining what fields/values to project (e.g., fun x -> x.Email
    /// or fun x -> (x.Name, x.Age)). Analyzed in quotation to determine projection type.
    /// </param>
    ///
    /// <typeparam name="'T">The source document type being projected from.</typeparam>
    /// <typeparam name="'R">The result type being projected to (may differ from 'T).</typeparam>
    ///
    /// <returns>
    /// TranslatedQuery&lt;'R&gt; - Result type may differ from source type.
    /// Returns Unchecked.defaultof (never executes, only for type inference).
    /// </returns>
    ///
    /// <remarks>
    /// This member is NEVER EXECUTED. The projection lambda is captured in the quotation
    /// and translated by QueryTranslator.translateProjection to determine the projection
    /// type (SelectAll, SelectFields, SelectSingle) and field names.
    ///
    /// <para><strong>AllowIntoPattern = true:</strong></para>
    ///
    /// This attribute enables type transformation from 'T to 'R. Without it, the query
    /// expression would require all operations to maintain the same type parameter.
    ///
    /// With AllowIntoPattern, you can write:
    /// <code>
    /// query {
    ///     for user in users do
    ///     where (user.Age > 18)
    ///     select user.Email  // Changes from TranslatedQuery&lt;User&gt; to TranslatedQuery&lt;string&gt;
    /// }
    /// </code>
    ///
    /// <para><strong>Projection Patterns Supported:</strong></para>
    ///
    /// 1. <strong>Identity</strong> - Select entire document:
    /// <code>select user</code>
    /// Maps to: Projection.SelectAll (returns complete Document&lt;User&gt;)
    ///
    /// 2. <strong>Single Field</strong> - Select one field:
    /// <code>select user.Email</code>
    /// Maps to: Projection.SelectSingle "email" (returns string)
    ///
    /// 3. <strong>Tuple</strong> - Select multiple fields as tuple:
    /// <code>select (user.Name, user.Email, user.Age)</code>
    /// Maps to: Projection.SelectFields ["name"; "email"; "age"] (returns tuple)
    ///
    /// 4. <strong>Anonymous Record</strong> - Select multiple fields with names:
    /// <code>select {| Name = user.Name; Email = user.Email |}</code>
    /// Maps to: Projection.SelectFields ["name"; "email"] (returns anonymous record)
    ///
    /// 5. <strong>Nested Fields</strong> - Project nested object fields:
    /// <code>select user.Address.City</code>
    /// Maps to: Projection.SelectSingle "address.city" (dot notation)
    ///
    /// 6. <strong>Computed Values</strong> - Transform/combine fields:
    /// <code>select {| FullName = user.FirstName + " " + user.LastName |}</code>
    /// Maps to: Projection.SelectFields ["firstName"; "lastName"] with client-side transform
    ///
    /// <para><strong>Performance Benefits:</strong></para>
    ///
    /// Field projection reduces:
    /// - SQL SELECT clause size (only named fields queried)
    /// - Network transfer size (less JSON data)
    /// - Deserialization cost (fewer fields to parse)
    /// - Memory usage (smaller result objects)
    ///
    /// Example: Selecting user.Email for 10,000 records sends only emails, not
    /// complete user documents (name, address, phone, etc.).
    ///
    /// <para><strong>Type Safety:</strong></para>
    ///
    /// The F# type system ensures projection expressions are well-typed:
    /// - Field names are checked at compile time
    /// - Result type 'R is inferred from projection expression
    /// - Type mismatches cause compile errors, not runtime errors
    ///
    /// <para><strong>Quotation Translation:</strong></para>
    ///
    /// The projection lambda is captured as a quotation expression tree:
    /// - Identity: Lambda(x, Var(x)) → SelectAll
    /// - Single field: Lambda(x, PropertyGet(x, "Email")) → SelectSingle "email"
    /// - Tuple: Lambda(x, NewTuple([PropertyGet(...), ...])) → SelectFields [...]
    /// - Anonymous record: Lambda(x, NewRecord(...)) → SelectFields [...]
    ///
    /// Field names are extracted and converted to camelCase for JSON compatibility.
    /// </remarks>
    ///
    /// <example>
    /// <code>
    /// // Identity - complete documents
    /// query {
    ///     for user in users do
    ///     where (user.Age > 18)
    ///     select user
    /// }
    /// // Returns: Document&lt;User&gt; seq (all fields)
    ///
    /// // Single field - just emails
    /// query {
    ///     for user in users do
    ///     where (user.IsActive = true)
    ///     select user.Email
    /// }
    /// // Returns: string seq (only email field)
    ///
    /// // Tuple - multiple fields
    /// query {
    ///     for user in users do
    ///     select (user.Name, user.Email, user.Age)
    /// }
    /// // Returns: (string * string * int) seq
    ///
    /// // Anonymous record - named fields
    /// query {
    ///     for user in users do
    ///     where (user.Age >= 18)
    ///     select {| Name = user.Name; IsAdult = true |}
    /// }
    /// // Returns: {| Name: string; IsAdult: bool |} seq
    ///
    /// // Nested fields - dot notation
    /// query {
    ///     for user in users do
    ///     where (user.Address.Country = "USA")
    ///     select user.Address.City
    /// }
    /// // Returns: string seq (city field from nested address)
    ///
    /// // Computed values - transformations
    /// query {
    ///     for user in users do
    ///     select {| FullName = user.FirstName + " " + user.LastName; Age = user.Age |}
    /// }
    /// // Returns: {| FullName: string; Age: int |} seq
    /// // Note: String concatenation done client-side after fetching firstName, lastName
    /// </code>
    /// </example>
    [<CustomOperation("select", AllowIntoPattern = true)>]
    member _.Select(source: TranslatedQuery<'T>, [<ProjectionParameter>] projection: 'T -> 'R) : TranslatedQuery<'R> =
        Unchecked.defaultof<_>

    /// <summary>
    /// Enables 'count' syntax to count matching documents.
    /// </summary>
    ///
    /// <param name="source">The source sequence from previous operations.</param>
    ///
    /// <typeparam name="'T">The document type being counted.</typeparam>
    ///
    /// <returns>
    /// int - Count of matching documents.
    /// Returns Unchecked.defaultof (never executes, only for type inference).
    /// </returns>
    ///
    /// <remarks>
    /// This member is NEVER EXECUTED. The count operation is captured in the quotation
    /// and translated to an optimized SQL COUNT(*) query by QueryExecutor.
    ///
    /// <para><strong>SQL Translation:</strong></para>
    ///
    /// count is translated to SQL COUNT(*) aggregate:
    /// <code>
    /// query {
    ///     for user in users do
    ///     where (user.Status = "active")
    ///     count
    /// }
    /// // Translates to: SELECT COUNT(*) FROM users WHERE data->>'status' = 'active'
    /// </code>
    ///
    /// <para><strong>Performance Optimization:</strong></para>
    ///
    /// COUNT(*) is highly optimized:
    /// - No document deserialization (SQLite just counts rows)
    /// - Uses indexes if available (e.g., WHERE clause with indexed field)
    /// - Returns single integer, not full documents
    /// - O(n) worst case, but much faster than fetching all documents
    ///
    /// Counting 1 million records:
    /// - COUNT(*): ~50ms (just count)
    /// - Fetch all: ~5000ms (deserialize all documents)
    ///
    /// <para><strong>vs. exists:</strong></para>
    ///
    /// Use 'exists' instead of 'count' when you only need to know IF documents exist:
    /// - count: SELECT COUNT(*) (scans all matching rows)
    /// - exists: SELECT 1 FROM ... LIMIT 1 (stops after first match)
    ///
    /// For existence checks, 'exists' is faster (especially with many matches).
    ///
    /// <para><strong>Use Cases:</strong></para>
    ///
    /// - Dashboards: "Total active users", "Orders this month"
    /// - Analytics: "Documents by category", "Failed requests count"
    /// - Pagination: Total count for page calculation (Page 3 of 10)
    /// - Validation: "Maximum capacity reached?" (count >= limit)
    /// </remarks>
    ///
    /// <example>
    /// <code>
    /// // Count all users
    /// query {
    ///     for user in users do
    ///     count
    /// }
    /// // Returns: int (e.g., 1523)
    ///
    /// // Count active users
    /// query {
    ///     for user in users do
    ///     where (user.Status = "active")
    ///     count
    /// }
    /// // SQL: SELECT COUNT(*) FROM users WHERE data->>'status' = 'active'
    ///
    /// // Count with complex filter
    /// query {
    ///     for order in orders do
    ///     where (order.Status = "completed" &amp;&amp; order.Total > 100.0)
    ///     count
    /// }
    /// // Returns: int count of high-value completed orders
    ///
    /// // Pagination total
    /// let totalUsers = query { for user in users do count }
    /// let totalPages = (totalUsers + pageSize - 1) / pageSize
    /// </code>
    /// </example>
    [<CustomOperation("count")>]
    member _.Count(source: TranslatedQuery<'T>) : int = Unchecked.defaultof<_>

    /// <summary>
    /// Enables 'exists' syntax to check if any documents match a predicate.
    /// </summary>
    ///
    /// <param name="source">The source sequence from previous operations.</param>
    /// <param name="predicate">
    /// Lambda expression defining the existence test (e.g., fun x -> x.Email = email).
    /// Analyzed in quotation to extract filter condition.
    /// </param>
    ///
    /// <typeparam name="'T">The document type being tested.</typeparam>
    ///
    /// <returns>
    /// bool - true if any matching documents exist, false otherwise.
    /// Returns Unchecked.defaultof (never executes, only for type inference).
    /// </returns>
    ///
    /// <remarks>
    /// This member is NEVER EXECUTED. The predicate is captured in the quotation and
    /// translated to an optimized SQL EXISTS query by QueryExecutor.
    ///
    /// <para><strong>SQL Translation:</strong></para>
    ///
    /// exists is translated to SQL with LIMIT 1 for early termination:
    /// <code>
    /// query {
    ///     for user in users do
    ///     where (user.Email = "test@example.com")
    ///     exists
    /// }
    /// // Translates to: SELECT 1 FROM users WHERE data->>'email' = 'test@example.com' LIMIT 1
    /// // Returns: true if any row found, false if none
    /// </code>
    ///
    /// <para><strong>Performance Optimization:</strong></para>
    ///
    /// exists is optimized for early termination:
    /// - Stops after finding first match (LIMIT 1)
    /// - No document deserialization (just checks row existence)
    /// - Uses indexes if available (much faster than full scan)
    /// - Returns immediately on first match
    ///
    /// Checking existence in 1 million records:
    /// - exists with index: ~0.1ms (instant lookup)
    /// - count: ~50ms (must count all matches)
    /// - Fetch + check: ~5000ms (deserialize all)
    ///
    /// <para><strong>vs. count > 0:</strong></para>
    ///
    /// Always prefer 'exists' over 'count > 0' for existence checks:
    /// - count: SELECT COUNT(*) (scans ALL matching rows)
    /// - exists: SELECT 1 ... LIMIT 1 (stops after FIRST match)
    ///
    /// Example: Checking if email exists among 1 million users:
    /// - count > 0: Scans all rows with that email (slow)
    /// - exists: Stops after finding first match (fast)
    ///
    /// <para><strong>Use Cases:</strong></para>
    ///
    /// - Uniqueness validation: "Email already registered?"
    /// - Authorization: "User has permission?"
    /// - Existence checks: "Product in stock?", "Category exists?"
    /// - Conditional logic: "if exists then ... else ..."
    /// </remarks>
    ///
    /// <example>
    /// <code>
    /// // Check if email exists
    /// let emailExists =
    ///     query {
    ///         for user in users do
    ///         where (user.Email = "test@example.com")
    ///         exists
    ///     }
    /// // Returns: bool (true if found, false if not)
    /// // SQL: SELECT 1 FROM users WHERE data->>'email' = '...' LIMIT 1
    ///
    /// // Validation in registration
    /// if query { for user in users do where (user.Email = newEmail) exists } then
    ///     Error "Email already registered"
    /// else
    ///     // Proceed with registration
    ///     Ok ()
    ///
    /// // Check complex condition
    /// query {
    ///     for order in orders do
    ///     where (order.UserId = userId &amp;&amp; order.Status = "pending")
    ///     exists
    /// }
    /// // Returns: bool - does user have pending orders?
    ///
    /// // Combined with other operations
    /// query {
    ///     for product in products do
    ///     where (product.Category = "electronics" &amp;&amp; product.Price &lt; 500.0)
    ///     exists
    /// }
    /// // Returns: bool - any affordable electronics available?
    /// </code>
    /// </example>
    [<CustomOperation("exists", MaintainsVariableSpace = true)>]
    member _.Exists(source: TranslatedQuery<'T>, [<ProjectionParameter>] predicate: 'T -> bool) : bool =
        Unchecked.defaultof<_>

    /// <summary>
    /// Enables 'head' syntax to retrieve the first matching document.
    /// </summary>
    ///
    /// <param name="source">The source sequence from previous operations.</param>
    ///
    /// <typeparam name="'T">The document type being retrieved.</typeparam>
    ///
    /// <returns>
    /// 'T - The first matching document (Document&lt;'T&gt;).
    /// Throws exception if no documents match.
    /// Returns Unchecked.defaultof (never executes, only for type inference).
    /// </returns>
    ///
    /// <remarks>
    /// This member is NEVER EXECUTED. The head operation is captured in the quotation
    /// and translated to SQL with LIMIT 1 by QueryExecutor. If no results, throws exception.
    ///
    /// <para><strong>SQL Translation:</strong></para>
    ///
    /// head is translated to SQL with LIMIT 1:
    /// <code>
    /// query {
    ///     for user in users do
    ///     where (user.Id = userId)
    ///     head
    /// }
    /// // Translates to: SELECT * FROM users WHERE data->>'id' = '...' LIMIT 1
    /// // Throws: InvalidOperationException if no rows returned
    /// </code>
    ///
    /// <para><strong>Exception Behavior:</strong></para>
    ///
    /// head throws InvalidOperationException if no documents match:
    /// <code>
    /// try
    ///     let user = query { for user in users do where (user.Id = id) head }
    ///     processUser user
    /// with
    /// | :? InvalidOperationException -> printfn "User not found"
    /// </code>
    ///
    /// This is the F# idiomatic behavior matching Seq.head, List.head.
    ///
    /// <para><strong>vs. headOrDefault:</strong></para>
    ///
    /// Choose based on whether missing document is exceptional:
    ///
    /// Use 'head' when:
    /// - Document MUST exist (e.g., lookup by primary key)
    /// - Missing document is an error condition
    /// - You want exception-based control flow
    ///
    /// Use 'headOrDefault' when:
    /// - Document MAY exist (e.g., optional lookups)
    /// - Missing document is a valid case
    /// - You prefer option-based control flow (idiomatic F#)
    ///
    /// <para><strong>Performance:</strong></para>
    ///
    /// head is optimized with LIMIT 1:
    /// - Stops after finding first match
    /// - Deserializes only one document
    /// - Uses indexes if available
    /// - O(1) with index, O(n) without
    ///
    /// <para><strong>Use Cases:</strong></para>
    ///
    /// - Lookup by primary key: Get user by ID (must exist)
    /// - Required references: Get category for product (must exist)
    /// - Authentication: Get user by session token (must be valid)
    /// - Configuration: Get setting by key (must be configured)
    /// </remarks>
    ///
    /// <example>
    /// <code>
    /// // Get user by ID (throws if not found)
    /// let user =
    ///     query {
    ///         for user in users do
    ///         where (user.Id = userId)
    ///         head
    ///     }
    /// // Returns: Document&lt;User&gt; (or throws InvalidOperationException)
    /// // SQL: SELECT * FROM users WHERE data->>'id' = '...' LIMIT 1
    ///
    /// // With exception handling
    /// try
    ///     let user = query { for user in users do where (user.Email = email) head }
    ///     Ok user
    /// with
    /// | :? InvalidOperationException -> Error "User not found"
    ///
    /// // Get first result with sorting
    /// query {
    ///     for product in products do
    ///     where (product.Category = "electronics")
    ///     sortByDescending product.Rating
    ///     head
    /// }
    /// // Returns: Highest-rated electronics product
    ///
    /// // Required lookup
    /// let category =
    ///     query {
    ///         for cat in categories do
    ///         where (cat.Id = product.CategoryId)
    ///         head
    ///     }
    /// // Throws if category doesn't exist (data integrity issue)
    /// </code>
    /// </example>
    [<CustomOperation("head")>]
    member _.Head(source: TranslatedQuery<'T>) : 'T = Unchecked.defaultof<_>

    /// <summary>
    /// Enables 'headOrDefault' syntax to safely retrieve the first matching document.
    /// </summary>
    ///
    /// <param name="source">The source sequence from previous operations.</param>
    ///
    /// <typeparam name="'T">The document type being retrieved.</typeparam>
    ///
    /// <returns>
    /// 'T option - Some(document) if found, None if no matches.
    /// Returns Unchecked.defaultof (never executes, only for type inference).
    /// </returns>
    ///
    /// <remarks>
    /// This member is NEVER EXECUTED. The headOrDefault operation is captured in the
    /// quotation and translated to SQL with LIMIT 1 by QueryExecutor. Returns None if
    /// no results instead of throwing.
    ///
    /// <para><strong>SQL Translation:</strong></para>
    ///
    /// headOrDefault is translated to SQL with LIMIT 1:
    /// <code>
    /// query {
    ///     for user in users do
    ///     where (user.Email = email)
    ///     headOrDefault
    /// }
    /// // Translates to: SELECT * FROM users WHERE data->>'email' = '...' LIMIT 1
    /// // Returns: Some(document) if found, None if no rows
    /// </code>
    ///
    /// <para><strong>Option Type Behavior:</strong></para>
    ///
    /// headOrDefault returns F# option type for safe handling:
    /// <code>
    /// match query { for user in users do where (user.Email = email) headOrDefault } with
    /// | Some user -> processUser user
    /// | None -> printfn "User not found"
    /// </code>
    ///
    /// This is idiomatic F# - prefer option types over exceptions for expected cases.
    ///
    /// <para><strong>vs. head:</strong></para>
    ///
    /// Choose based on whether missing document is exceptional:
    ///
    /// Use 'headOrDefault' when:
    /// - Document MAY exist (e.g., optional lookups, search results)
    /// - Missing document is a valid case
    /// - You prefer option-based control flow (idiomatic F#)
    ///
    /// Use 'head' when:
    /// - Document MUST exist (e.g., primary key lookup)
    /// - Missing document is an error condition
    /// - You want exception-based control flow
    ///
    /// <para><strong>Performance:</strong></para>
    ///
    /// headOrDefault is optimized with LIMIT 1:
    /// - Stops after finding first match
    /// - Deserializes only one document
    /// - Uses indexes if available
    /// - O(1) with index, O(n) without
    /// - Same performance as 'head' (just safer return type)
    ///
    /// <para><strong>Use Cases:</strong></para>
    ///
    /// - Optional lookups: Find user by email (may not exist)
    /// - Search results: Get first match (may have no results)
    /// - Cache lookups: Get cached value (may be missing)
    /// - Conditional operations: Get setting by key (use default if missing)
    /// </remarks>
    ///
    /// <example>
    /// <code>
    /// // Safe user lookup
    /// let userOpt =
    ///     query {
    ///         for user in users do
    ///         where (user.Email = email)
    ///         headOrDefault
    ///     }
    /// // Returns: Document&lt;User&gt; option (Some or None)
    /// // SQL: SELECT * FROM users WHERE data->>'email' = '...' LIMIT 1
    ///
    /// // Pattern matching
    /// match query { for user in users do where (user.Email = email) headOrDefault } with
    /// | Some user -> Ok user
    /// | None -> Error "User not found"
    ///
    /// // With Option.map
    /// query { for user in users do where (user.Id = id) headOrDefault }
    /// |> Option.map (fun user -> user.Data.Name)
    /// |> Option.defaultValue "Unknown"
    ///
    /// // Search with fallback
    /// let firstMatch =
    ///     query {
    ///         for product in products do
    ///         where (product.Name.Contains(searchTerm))
    ///         sortByDescending product.Popularity
    ///         headOrDefault
    ///     }
    /// // Returns: Some(product) if found, None if no matches
    ///
    /// // Cache pattern
    /// match query { for item in cache do where (item.Key = key) headOrDefault } with
    /// | Some cached -> cached.Value
    /// | None ->
    ///     let value = computeExpensiveValue()
    ///     saveToCache key value
    ///     value
    /// </code>
    /// </example>
    [<CustomOperation("headOrDefault")>]
    member _.HeadOrDefault(source: TranslatedQuery<'T>) : option<'T> = Unchecked.defaultof<_>


// ============================================================
// QUOTATION TRANSLATOR
// ============================================================

/// <summary>
/// Internal module for translating F# quotation expressions to Query&lt;'T&gt; filters.
/// </summary>
///
/// <remarks>
/// This module analyzes captured quotation expressions from query computation expressions
/// and translates them to FractalDb's Query&lt;'T&gt; AST, which is then converted to SQL.
///
/// <para><strong>Translation Pipeline:</strong></para>
///
/// 1. User writes: query { for user in users do where (user.Age > 18) }
/// 2. QueryBuilder captures quotation: Lambda(user, Call(>, PropertyGet(user, Age), Value(18)))
/// 3. QueryTranslator.translatePredicate → Query.Field("age", FieldOp.Compare(CompareOp.Gt 18))
/// 4. SqlTranslator → SQL: WHERE data->>'age' > 18
///
/// <para><strong>Key Functions:</strong></para>
///
/// - extractPropertyName: Extracts field names from PropertyGet quotations
/// - toCamelCase: Converts PascalCase property names to camelCase JSON field names
/// - translatePredicate: Converts predicate quotations to Query&lt;'T&gt; filters (tasks 112-114)
/// - translate: Main translation function orchestrating the full conversion (task 115)
/// - translateProjection: Converts projection quotations to Projection DU (task 116)
/// </remarks>
module internal QueryTranslator =

    /// <summary>
    /// Converts PascalCase property names to camelCase for JSON field names.
    /// </summary>
    ///
    /// <param name="s">The PascalCase string to convert.</param>
    ///
    /// <returns>
    /// The camelCase version of the input string.
    /// </returns>
    ///
    /// <remarks>
    /// JSON serialization in FractalDb uses camelCase field names by convention.
    /// F# property names use PascalCase by convention.
    ///
    /// This function bridges the gap:
    /// - "CreatedAt" → "createdAt"
    /// - "FirstName" → "firstName"
    /// - "ID" → "iD" (keeps remaining capitals)
    ///
    /// <para><strong>Edge Cases:</strong></para>
    ///
    /// - Empty string: Returns empty string
    /// - Single char: Lowercases it ("A" → "a")
    /// - Already camelCase: Returns unchanged ("email" → "email")
    /// - All caps: Only lowercases first char ("URL" → "uRL")
    /// </remarks>
    ///
    /// <example>
    /// <code>
    /// toCamelCase "CreatedAt"    // "createdAt"
    /// toCamelCase "FirstName"    // "firstName"
    /// toCamelCase "Age"          // "age"
    /// toCamelCase "email"        // "email" (unchanged)
    /// toCamelCase ""             // ""
    /// </code>
    /// </example>
    let private toCamelCase (s: string) : string =
        if String.IsNullOrEmpty(s) then
            s
        else
            string (Char.ToLowerInvariant(s.[0])) + s.Substring(1)

    /// <summary>
    /// Extracts property name from F# quotation expression, converting to camelCase.
    /// </summary>
    ///
    /// <param name="expr">
    /// F# quotation expression representing property access (e.g., user.Email, user.Address.City).
    /// </param>
    ///
    /// <returns>
    /// String representing the property path in camelCase with dot notation for nested fields.
    /// Returns empty string for identity (Var) references.
    /// </returns>
    ///
    /// <exception cref="System.ArgumentException">
    /// Thrown when expression is not a valid property access pattern.
    /// </exception>
    ///
    /// <remarks>
    /// This function recursively analyzes quotation expressions to extract property names
    /// from various patterns supported by F# query expressions.
    ///
    /// <para><strong>Supported Patterns:</strong></para>
    ///
    /// 1. <strong>Lambda wrapping</strong>: Lambda(x, body)
    ///    - Strips outer lambda, processes body
    ///    - Example: fun user -> user.Email → processes user.Email
    ///
    /// 2. <strong>PropertyGet with receiver</strong>: PropertyGet(Some receiver, propInfo, [])
    ///    - Extracts property chain with dot notation
    ///    - Example: user.Address.City → "address.city"
    ///    - Recursively processes receiver for nested properties
    ///
    /// 3. <strong>Static PropertyGet</strong>: PropertyGet(None, propInfo, [])
    ///    - Extracts static property names
    ///    - Example: DateTime.Now → "now"
    ///
    /// 4. <strong>Var reference</strong>: Var(varInfo)
    ///    - Identity projection (select entire object)
    ///    - Returns empty string to signal SelectAll
    ///    - Example: select user → ""
    ///
    /// <para><strong>Property Name Conversion:</strong></para>
    ///
    /// All property names are converted from PascalCase (F# convention) to camelCase
    /// (JSON convention) using the toCamelCase helper function.
    ///
    /// - User.CreatedAt → "createdAt"
    /// - User.FirstName → "firstName"
    /// - User.Address.City → "address.city"
    ///
    /// <para><strong>Nested Property Handling:</strong></para>
    ///
    /// Nested properties use dot notation:
    /// <code>
    /// user.Address.City
    /// // Quotation: PropertyGet(PropertyGet(Var(user), Address), City)
    /// // Result: "address.city"
    /// // SQL: data->'address'->>'city'
    /// </code>
    ///
    /// The recursion processes inside-out:
    /// 1. Outer PropertyGet(_, City) → "city"
    /// 2. Recursive call on receiver PropertyGet(_, Address) → "address"
    /// 3. Combine: "address.city"
    ///
    /// <para><strong>Error Handling:</strong></para>
    ///
    /// Throws ArgumentException for unsupported expression patterns:
    /// - Method calls: user.ToString()
    /// - Computed values: user.Age + 10
    /// - Complex expressions: if user.IsActive then user.Email else ""
    ///
    /// Only simple property access chains are supported.
    /// </remarks>
    ///
    /// <example>
    /// <code>
    /// // Simple property
    /// extractPropertyName &lt;@ fun (u: User) -> u.Email @&gt;
    /// // Returns: "email"
    ///
    /// // Nested property
    /// extractPropertyName &lt;@ fun (u: User) -> u.Address.City @&gt;
    /// // Returns: "address.city"
    ///
    /// // Identity
    /// extractPropertyName &lt;@ fun (u: User) -> u @&gt;
    /// // Returns: ""
    ///
    /// // From query expression (captured quotation)
    /// query {
    ///     for user in users do
    ///     where (user.Email = "test@example.com")
    ///     // Captured: Lambda(user, Call(=, PropertyGet(user, Email), Value("test@example.com")))
    ///     // extractPropertyName on left operand → "email"
    /// }
    /// </code>
    /// </example>
    let rec extractPropertyName (expr: Expr) : string =
        match expr with
        // Lambda wrapper: fun x -> x.Property
        // Strip lambda, process body recursively
        | Lambda(_, body) -> extractPropertyName body

        // Property access: receiver.Property
        // Example: user.Email, user.Address.City
        | PropertyGet(Some receiver, propInfo, []) ->
            let receiverPath = extractPropertyName receiver
            let propName = propInfo.Name |> toCamelCase

            // Build dot-notation path for nested properties
            if String.IsNullOrEmpty(receiverPath) then
                propName
            else
                receiverPath + "." + propName

        // Static property: Type.Property
        // Example: DateTime.Now
        | PropertyGet(None, propInfo, []) -> propInfo.Name |> toCamelCase

        // Variable reference: just the variable itself
        // Example: select user (identity projection)
        // Return empty string to signal SelectAll
        | Var _ -> ""

        // Unsupported pattern
        | _ -> raise (ArgumentException($"Cannot extract property name from expression: {expr}"))

    /// <summary>
    /// Evaluates a quotation expression to extract its runtime value.
    /// </summary>
    ///
    /// <param name="expr">
    /// F# quotation expression to evaluate (e.g., Value(42), captured variables, computed expressions).
    /// </param>
    ///
    /// <returns>
    /// Boxed object representing the runtime value of the expression.
    /// </returns>
    ///
    /// <remarks>
    /// This helper function bridges F# compile-time quotations with runtime values needed
    /// for SQL query generation.
    ///
    /// <para><strong>Use Case:</strong></para>
    ///
    /// In query expressions, the right-hand side of comparisons may be:
    /// - Constants: where (user.Age > 18)
    /// - Captured variables: where (user.Status = targetStatus)
    /// - Computed expressions: where (user.Price &lt; maxPrice * discountRate)
    ///
    /// All these need to be evaluated to get actual runtime values for SQL parameters.
    ///
    /// <para><strong>Implementation:</strong></para>
    ///
    /// Uses Microsoft.FSharp.Linq.RuntimeHelpers.LeafExpressionConverter.EvaluateQuotation
    /// to safely evaluate the expression tree and extract the runtime value.
    ///
    /// This handles:
    /// - Value nodes: Direct constants
    /// - Variable captures: Closure variables
    /// - Complex expressions: Arithmetic, function calls, etc.
    ///
    /// <para><strong>Type Handling:</strong></para>
    ///
    /// Returns obj (boxed) because:
    /// - SQL parameters are type-erased
    /// - Query&lt;'T&gt; stores values as obj
    /// - Caller can downcast if needed (e.g., :?> string)
    ///
    /// <para><strong>Performance:</strong></para>
    ///
    /// Evaluation happens once at query construction time, not per-row:
    /// <code>
    /// let maxAge = 65
    /// query { for user in users do where (user.Age &lt;= maxAge) }
    /// // maxAge evaluated once → SQL: WHERE data->>'age' &lt;= 65
    /// // Not evaluated per row
    /// </code>
    /// </remarks>
    ///
    /// <example>
    /// <code>
    /// // Constants
    /// evaluateExpr &lt;@ 42 @&gt;          // Returns: box 42
    /// evaluateExpr &lt;@ "text" @&gt;      // Returns: box "text"
    ///
    /// // Captured variables
    /// let age = 18
    /// evaluateExpr &lt;@ age @&gt;        // Returns: box 18
    ///
    /// // Computed expressions
    /// let max = 100
    /// evaluateExpr &lt;@ max * 2 @&gt;   // Returns: box 200
    ///
    /// // In translatePredicate context
    /// // where (user.Age > 18)
    /// // Captured quotation: Call(op_GreaterThan, PropertyGet(user, Age), Value(18))
    /// let field = extractPropertyName left   // "age"
    /// let value = evaluateExpr right         // 18
    /// // Result: Query.Field("age", FieldOp.Compare(CompareOp.Gt 18))
    /// </code>
    /// </example>
    let private evaluateExpr (expr: Expr) : obj =
        Microsoft.FSharp.Linq.RuntimeHelpers.LeafExpressionConverter.EvaluateQuotation expr

    /// <summary>
    /// Translates F# quotation predicate expressions to Query&lt;'T&gt; AST.
    /// </summary>
    ///
    /// <param name="expr">
    /// F# quotation expression representing a boolean predicate (e.g., user.Age > 18).
    /// </param>
    ///
    /// <typeparam name="'T">
    /// Document type being queried (preserved for type safety).
    /// </typeparam>
    ///
    /// <returns>
    /// Query&lt;'T&gt; AST node representing the translated predicate.
    /// </returns>
    ///
    /// <exception cref="System.Exception">
    /// Thrown when expression contains unsupported patterns (e.g., method calls, complex logic).
    /// </exception>
    ///
    /// <remarks>
    /// This function is the core of query expression translation. It recursively walks the
    /// quotation tree and converts F# operators and expressions into FractalDb's Query&lt;'T&gt; AST,
    /// which is then converted to SQL by SqlTranslator.
    ///
    /// <para><strong>Translation Pipeline:</strong></para>
    ///
    /// 1. User writes: where (user.Age > 18)
    /// 2. Captured quotation: Call(op_GreaterThan, PropertyGet(user, Age), Value(18))
    /// 3. translatePredicate → Query.Field("age", FieldOp.Compare(CompareOp.Gt 18))
    /// 4. SqlTranslator → SQL: WHERE data->>'age' > 18
    ///
    /// <para><strong>Currently Supported Patterns (Task 112):</strong></para>
    ///
    /// 1. <strong>Lambda wrappers</strong>: Lambda(x, body)
    ///    - Strips outer lambda from 'where' clause
    ///    - Recursively processes body
    ///
    /// 2. <strong>Equality</strong>: Property = Value
    ///    - SpecificCall &lt;@ (=) @&gt;
    ///    - Maps to: CompareOp.Eq
    ///    - Example: user.Status = "active"
    ///
    /// 3. <strong>Inequality</strong>: Property &lt;> Value
    ///    - SpecificCall &lt;@ (&lt;>) @&gt;
    ///    - Maps to: CompareOp.Ne
    ///    - Example: user.Id &lt;> excludedId
    ///
    /// 4. <strong>Greater Than</strong>: Property > Value
    ///    - SpecificCall &lt;@ (>) @&gt;
    ///    - Maps to: CompareOp.Gt
    ///    - Example: user.Age > 18
    ///
    /// 5. <strong>Greater Than or Equal</strong>: Property >= Value
    ///    - SpecificCall &lt;@ (>=) @&gt;
    ///    - Maps to: CompareOp.Gte
    ///    - Example: product.Price >= 100.0
    ///
    /// 6. <strong>Less Than</strong>: Property &lt; Value
    ///    - SpecificCall &lt;@ (&lt;) @&gt;
    ///    - Maps to: CompareOp.Lt
    ///    - Example: user.Age &lt; 65
    ///
    /// 7. <strong>Less Than or Equal</strong>: Property &lt;= Value
    ///    - SpecificCall &lt;@ (&lt;=) @&gt;
    ///    - Maps to: CompareOp.Lte
    ///    - Example: order.Total &lt;= budget
    ///
    /// <para><strong>Future Extensions:</strong></para>
    ///
    /// - Task 113: Logical operators (&&, ||, not)
    /// - Task 114: String methods (Contains, StartsWith, EndsWith)
    /// - Future: Array operations (Any, All, Size)
    /// - Future: Null checks (IsNull, IsNotNull)
    ///
    /// <para><strong>Pattern Matching Strategy:</strong></para>
    ///
    /// Uses SpecificCall active pattern to match against specific operator quotations:
    /// <code>
    /// SpecificCall &lt;@ (>) @&gt; (_, _, [left; right])
    /// // Matches: left > right
    /// // Extracts: left expression, right expression
    /// </code>
    ///
    /// Then:
    /// 1. extractPropertyName left → field name (e.g., "age")
    /// 2. evaluateExpr right → runtime value (e.g., 18)
    /// 3. Map operator → CompareOp case (e.g., CompareOp.Gt)
    /// 4. Build AST → Query.Field("age", FieldOp.Compare(box (CompareOp.Gt 18)))
    ///
    /// <para><strong>Value Boxing:</strong></para>
    ///
    /// Values are boxed because Query&lt;'T&gt; is type-erased - it doesn't know if the value
    /// is int, string, float, etc. The box is preserved through SQL generation and becomes
    /// a SQL parameter.
    ///
    /// <para><strong>Type Safety:</strong></para>
    ///
    /// The 'T type parameter is preserved through translation for downstream type safety,
    /// even though the Query&lt;'T&gt; cases use obj for value storage.
    /// </remarks>
    ///
    /// <example>
    /// <code>
    /// // Simple comparison
    /// translatePredicate &lt;@ fun (user: User) -> user.Age > 18 @&gt;
    /// // Returns: Query.Field("age", FieldOp.Compare(box (CompareOp.Gt 18)))
    /// // SQL: WHERE data->>'age' > 18
    ///
    /// // Equality
    /// translatePredicate &lt;@ fun (user: User) -> user.Status = "active" @&gt;
    /// // Returns: Query.Field("status", FieldOp.Compare(box (CompareOp.Eq "active")))
    /// // SQL: WHERE data->>'status' = 'active'
    ///
    /// // Inequality
    /// translatePredicate &lt;@ fun (product: Product) -> product.Stock &lt;> 0 @&gt;
    /// // Returns: Query.Field("stock", FieldOp.Compare(box (CompareOp.Ne 0)))
    /// // SQL: WHERE data->>'stock' != 0
    ///
    /// // Less than or equal
    /// translatePredicate &lt;@ fun (order: Order) -> order.Total &lt;= 1000.0 @&gt;
    /// // Returns: Query.Field("total", FieldOp.Compare(box (CompareOp.Lte 1000.0)))
    /// // SQL: WHERE data->>'total' &lt;= 1000.0
    ///
    /// // Nested property
    /// translatePredicate &lt;@ fun (user: User) -> user.Address.City = "Seattle" @&gt;
    /// // Returns: Query.Field("address.city", FieldOp.Compare(box (CompareOp.Eq "Seattle")))
    /// // SQL: WHERE data->'address'->>'city' = 'Seattle'
    /// </code>
    /// </example>
    let rec translatePredicate<'T> (expr: Expr) : Query<'T> =
        match expr with
        // Lambda wrapper: fun x -> body
        // Strip lambda and process body
        | Lambda(_, body) -> translatePredicate body

        // Property = Value (Equality)
        | SpecificCall <@ (=) @> (_, _, [ left; right ]) ->
            let field = extractPropertyName left
            let value = evaluateExpr right
            Query.Field(field, FieldOp.Compare(box (CompareOp.Eq value)))

        // Property <> Value (Inequality)
        | SpecificCall <@ (<>) @> (_, _, [ left; right ]) ->
            let field = extractPropertyName left
            let value = evaluateExpr right
            Query.Field(field, FieldOp.Compare(box (CompareOp.Ne value)))

        // Property > Value (Greater Than)
        | SpecificCall <@ (>) @> (_, _, [ left; right ]) ->
            let field = extractPropertyName left
            let value = evaluateExpr right
            Query.Field(field, FieldOp.Compare(box (CompareOp.Gt value)))

        // Property >= Value (Greater Than or Equal)
        | SpecificCall <@ (>=) @> (_, _, [ left; right ]) ->
            let field = extractPropertyName left
            let value = evaluateExpr right
            Query.Field(field, FieldOp.Compare(box (CompareOp.Gte value)))

        // Property < Value (Less Than)
        | SpecificCall <@ (<) @> (_, _, [ left; right ]) ->
            let field = extractPropertyName left
            let value = evaluateExpr right
            Query.Field(field, FieldOp.Compare(box (CompareOp.Lt value)))

        // Property <= Value (Less Than or Equal)
        | SpecificCall <@ (<=) @> (_, _, [ left; right ]) ->
            let field = extractPropertyName left
            let value = evaluateExpr right
            Query.Field(field, FieldOp.Compare(box (CompareOp.Lte value)))

        // Logical AND: expr1 && expr2
        | SpecificCall <@ (&&) @> (_, _, [ left; right ]) ->
            Query.And [ translatePredicate left; translatePredicate right ]

        // Logical OR: expr1 || expr2
        | SpecificCall <@ (||) @> (_, _, [ left; right ]) ->
            Query.Or [ translatePredicate left; translatePredicate right ]

        // Logical NOT: not expr
        | SpecificCall <@ not @> (_, _, [ inner ]) -> Query.Not(translatePredicate inner)

        // String.Contains: field.Contains(substring)
        | Call(Some receiver, methodInfo, [ arg ]) when methodInfo.Name = "Contains" ->
            let field = extractPropertyName receiver
            let value = evaluateExpr arg :?> string
            Query.Field(field, FieldOp.String(StringOp.Contains value))

        // String.StartsWith: field.StartsWith(prefix)
        | Call(Some receiver, methodInfo, [ arg ]) when methodInfo.Name = "StartsWith" ->
            let field = extractPropertyName receiver
            let value = evaluateExpr arg :?> string
            Query.Field(field, FieldOp.String(StringOp.StartsWith value))

        // String.EndsWith: field.EndsWith(suffix)
        | Call(Some receiver, methodInfo, [ arg ]) when methodInfo.Name = "EndsWith" ->
            let field = extractPropertyName receiver
            let value = evaluateExpr arg :?> string
            Query.Field(field, FieldOp.String(StringOp.EndsWith value))

        // Unsupported pattern
        | _ -> failwith $"Unsupported predicate expression: {expr}"

    /// <summary>
    /// Simplifies Query&lt;'T&gt; structures by optimizing nested And/Or operations.
    /// </summary>
    ///
    /// <param name="q">Query structure to simplify.</param>
    ///
    /// <typeparam name="'T">Document type being queried.</typeparam>
    ///
    /// <returns>
    /// Optimized Query&lt;'T&gt; with flattened And/Or operations and empty queries removed.
    /// </returns>
    ///
    /// <remarks>
    /// This function optimizes query structures created by combining multiple where clauses
    /// to reduce nesting depth and improve SQL generation efficiency.
    ///
    /// <para><strong>Optimizations Applied:</strong></para>
    ///
    /// 1. <strong>Remove Empty queries</strong>: Filter out Query.Empty from And/Or lists
    /// 2. <strong>Unwrap single-element lists</strong>: And [q] → q, Or [q] → q
    /// 3. <strong>Collapse empty lists</strong>: And [] → Empty, Or [] → Empty
    ///
    /// <para><strong>Why This Matters:</strong></para>
    ///
    /// Multiple where clauses create nested And structures:
    /// <code>
    /// query {
    ///     for user in users do
    ///     where (user.Age >= 18)
    ///     where (user.Status = "active")
    ///     where (user.Country = "USA")
    /// }
    /// // Without simplify: And [And [And [Empty; q1]; q2]; q3]
    /// // With simplify: And [q1; q2; q3]
    /// </code>
    ///
    /// <para><strong>Implementation Strategy:</strong></para>
    ///
    /// Pattern matches on Query cases:
    /// - Query.And: Filters empty, unwraps single, collapses empty list
    /// - Query.Or: Same optimizations as And
    /// - Other cases: Returned unchanged
    ///
    /// <para><strong>Performance Impact:</strong></para>
    ///
    /// - Reduces AST depth → faster SQL translation
    /// - Cleaner generated SQL (fewer nested parentheses)
    /// - Easier debugging (simpler structure to inspect)
    /// </remarks>
    ///
    /// <example>
    /// <code>
    /// // Remove Empty queries
    /// simplify (Query.And [Query.Empty; q1; Query.Empty; q2])
    /// // Returns: Query.And [q1; q2]
    ///
    /// // Unwrap single element
    /// simplify (Query.And [q1])
    /// // Returns: q1
    ///
    /// // Collapse empty list
    /// simplify (Query.And [])
    /// // Returns: Query.Empty
    ///
    /// // Practical example from query expressions
    /// // Three where clauses create nested structure:
    /// let q1 = Query.Field("age", FieldOp.Compare(box (CompareOp.Gte 18)))
    /// let q2 = Query.Field("status", FieldOp.Compare(box (CompareOp.Eq "active")))
    /// let q3 = Query.Field("country", FieldOp.Compare(box (CompareOp.Eq "USA")))
    ///
    /// // Before simplify (nested):
    /// Query.And [Query.And [Query.And [Query.Empty; q1]; q2]; q3]
    ///
    /// // After simplify (flat):
    /// Query.And [q1; q2; q3]
    /// </code>
    /// </example>
    let rec simplify<'T> (q: Query<'T>) : Query<'T> =
        match q with
        | Query.And queries ->
            queries
            |> List.filter (fun q -> q <> Query.Empty)
            |> function
                | [] -> Query.Empty
                | [ single ] -> single
                | filtered -> Query.And filtered
        | Query.Or queries ->
            queries
            |> List.filter (fun q -> q <> Query.Empty)
            |> function
                | [] -> Query.Empty
                | [ single ] -> single
                | filtered -> Query.Or filtered
        | other -> other

    /// <summary>
    /// Analyzes projection expression to determine projection type and fields.
    /// </summary>
    ///
    /// <param name="expr">
    /// F# quotation expression from select clause representing the projection.
    /// </param>
    ///
    /// <returns>
    /// Projection discriminated union: SelectAll, SelectSingle, or SelectFields.
    /// </returns>
    ///
    /// <remarks>
    /// This function translates the select clause of query expressions to determine what
    /// fields to return from the query. It analyzes the quotation pattern to distinguish
    /// between:
    /// - Identity projections: select user (return complete documents)
    /// - Single field: select user.Email (return one field value)
    /// - Multiple fields: select (user.Name, user.Email) or {| Name=...; Email=... |}
    ///
    /// <para><strong>Projection Types:</strong></para>
    ///
    /// 1. <strong>SelectAll</strong> - Return complete documents
    ///    - Pattern: Lambda(_, Var(_))
    ///    - Example: select user
    ///    - SQL: SELECT * FROM ...
    ///    - Result type: Document&lt;'T&gt; seq
    ///
    /// 2. <strong>SelectSingle field</strong> - Return single field value
    ///    - Pattern: Lambda(_, PropertyGet(...))
    ///    - Example: select user.Email
    ///    - SQL: SELECT data->>'email' FROM ...
    ///    - Result type: string seq (or appropriate field type)
    ///    - Nested: select user.Profile.Email → "profile.email"
    ///
    /// 3. <strong>SelectFields list</strong> - Return multiple fields
    ///    - Pattern: Lambda(_, NewTuple(...)) or Lambda(_, NewRecord(...))
    ///    - Examples:
    ///      - Tuple: select (user.Name, user.Email)
    ///      - Anonymous record: select {| Name = user.Name; Email = user.Email |}
    ///    - SQL: SELECT data->>'name', data->>'email' FROM ...
    ///    - Result type: (string * string) seq or {| Name: string; Email: string |} seq
    ///
    /// <para><strong>Pattern Matching Strategy:</strong></para>
    ///
    /// Patterns are checked in specificity order:
    /// 1. Var (identity) → SelectAll
    /// 2. PropertyGet (single field) → SelectSingle
    /// 3. NewTuple (tuple projection) → SelectFields
    /// 4. NewRecord (anonymous record) → SelectFields
    /// 5. Fallback (unknown pattern) → SelectAll (safe default)
    ///
    /// <para><strong>Field Name Extraction:</strong></para>
    ///
    /// Uses extractPropertyName for consistency:
    /// - PascalCase properties → camelCase JSON fields
    /// - Nested properties → dot notation (e.g., "address.city")
    /// - Type-safe: compile-time property name validation
    ///
    /// <para><strong>Performance Impact:</strong></para>
    ///
    /// Field projection reduces:
    /// - SQL SELECT clause size (only specified fields)
    /// - Network transfer (less JSON data)
    /// - Deserialization cost (fewer fields to parse)
    /// - Memory usage (smaller result objects)
    ///
    /// Example: Selecting user.Email from 10,000 documents transfers only emails,
    /// not complete user documents (name, address, phone, etc.).
    ///
    /// <para><strong>Type Safety:</strong></para>
    ///
    /// F# type inference ensures projection expressions are well-typed:
    /// - Field names validated at compile time
    /// - Result type matches projection expression
    /// - Type mismatches cause compiler errors
    ///
    /// <para><strong>Fallback Behavior:</strong></para>
    ///
    /// Unknown or complex expressions default to SelectAll (return complete documents).
    /// This is conservative: better to return more data than fail.
    ///
    /// Complex expressions not supported:
    /// - Computed values: select (user.FirstName + " " + user.LastName)
    /// - Method calls: select user.ToString()
    /// - Conditionals: select (if user.IsActive then user.Email else "N/A")
    ///
    /// For these cases, query returns complete documents, then apply transformation
    /// client-side after retrieval.
    /// </remarks>
    ///
    /// <example>
    /// <code>
    /// // Identity projection - complete documents
    /// translateProjection &lt;@ fun (user: User) -> user @&gt;
    /// // Returns: Projection.SelectAll
    ///
    /// // Single field - unwrapped value
    /// translateProjection &lt;@ fun (user: User) -> user.Email @&gt;
    /// // Returns: Projection.SelectSingle "email"
    ///
    /// // Nested field - dot notation
    /// translateProjection &lt;@ fun (user: User) -> user.Profile.Email @&gt;
    /// // Returns: Projection.SelectSingle "profile.email"
    ///
    /// // Tuple projection - multiple fields
    /// translateProjection &lt;@ fun (user: User) -> (user.Name, user.Email, user.Age) @&gt;
    /// // Returns: Projection.SelectFields ["name"; "email"; "age"]
    ///
    /// // Anonymous record - named fields
    /// translateProjection &lt;@ fun (user: User) -> {| Name = user.Name; Email = user.Email |} @&gt;
    /// // Returns: Projection.SelectFields ["name"; "email"]
    ///
    /// // Complex expression - falls back to SelectAll
    /// translateProjection &lt;@ fun (user: User) -> user.FirstName + " " + user.LastName @&gt;
    /// // Returns: Projection.SelectAll (safe fallback)
    /// </code>
    /// </example>
    let translateProjection (expr: Expr) : Projection =
        match expr with
        // Identity: fun x -> x (select entire document)
        | Lambda(_, Var _) -> Projection.SelectAll

        // Single field: fun x -> x.Property
        | Lambda(_, PropertyGet _) as lambda ->
            let field = extractPropertyName lambda

            if String.IsNullOrEmpty(field) then
                Projection.SelectAll
            else
                Projection.SelectSingle field

        // Tuple: fun x -> (x.A, x.B, x.C)
        | Lambda(_, NewTuple fields) ->
            let fieldNames = fields |> List.map extractPropertyName
            Projection.SelectFields fieldNames

        // Anonymous record: fun x -> {| A = x.A; B = x.B |}
        | Lambda(_, NewRecord(_, fields)) ->
            let fieldNames = fields |> List.map extractPropertyName
            Projection.SelectFields fieldNames

        // Unknown pattern - safe fallback to complete documents
        | _ -> Projection.SelectAll

    /// <summary>
    /// Main translation function: converts full query expression quotation to TranslatedQuery&lt;'T&gt;.
    /// </summary>
    ///
    /// <param name="expr">
    /// F# quotation expression representing complete query computation expression.
    /// Type is Expr&lt;TranslatedQuery&lt;'T&gt;&gt; but cast to Expr for pattern matching.
    /// </param>
    ///
    /// <typeparam name="'T">Document type being queried.</typeparam>
    ///
    /// <returns>
    /// TranslatedQuery&lt;'T&gt; record containing all query components ready for execution.
    /// </returns>
    ///
    /// <remarks>
    /// This is the orchestration function for the entire query translation pipeline. It walks
    /// the captured quotation expression tree from QueryBuilder computation expressions and
    /// extracts all query components into a TranslatedQuery&lt;'T&gt; structure.
    ///
    /// <para><strong>Translation Architecture:</strong></para>
    ///
    /// Uses a recursive loop with an accumulator pattern:
    /// <code>
    /// let rec loop (expr: Expr) (query: TranslatedQuery&lt;'T&gt;) : TranslatedQuery&lt;'T&gt; =
    ///     match expr with
    ///     | SpecificCall QueryBuilderMethod -> extract + recurse
    ///     | _ -> return accumulator
    /// </code>
    ///
    /// <para><strong>Quotation Structure:</strong></para>
    ///
    /// Query expressions are represented as nested method calls:
    /// <code>
    /// query {
    ///     for user in users do
    ///     where (user.Age >= 18)
    ///     sortBy user.Name
    ///     take 10
    /// }
    ///
    /// // Captured quotation structure:
    /// Call(Take,
    ///   Call(SortBy,
    ///     Call(Where,
    ///       Call(For, users, lambda),
    ///       wherePredicate),
    ///     sortSelector),
    ///   10)
    /// </code>
    ///
    /// The translate function recursively walks this tree, extracting components at each level.
    ///
    /// <para><strong>Handled Operations:</strong></para>
    ///
    /// 1. <strong>For</strong>: QueryBuilder.For
    ///    - Extracts Collection&lt;'T&gt; source
    ///    - Evaluates to get runtime collection instance
    ///    - Updates TranslatedQuery.Source
    ///
    /// 2. <strong>Where</strong>: QueryBuilder.Where
    ///    - Translates predicate using translatePredicate&lt;'T&gt;
    ///    - Combines with existing where using Query.And
    ///    - Applies simplify to optimize structure
    ///    - Multiple where clauses supported (AND logic)
    ///
    /// 3. <strong>SortBy</strong>: QueryBuilder.SortBy
    ///    - Extracts property name using extractPropertyName
    ///    - Adds (field, SortDirection.Asc) to OrderBy list
    ///    - Appends to existing sorts (for ThenBy support)
    ///
    /// 4. <strong>SortByDescending</strong>: QueryBuilder.SortByDescending
    ///    - Adds (field, SortDirection.Desc) to OrderBy list
    ///
    /// 5. <strong>Take</strong>: QueryBuilder.Take
    ///    - Extracts int count from Value node
    ///    - Sets TranslatedQuery.Take to Some(count)
    ///
    /// 6. <strong>Skip</strong>: QueryBuilder.Skip
    ///    - Extracts int count from Value node
    ///    - Sets TranslatedQuery.Skip to Some(count)
    ///
    /// 7. <strong>Select</strong>: QueryBuilder.Select
    ///    - Placeholder for task-116 (translateProjection)
    ///    - Currently sets Projection.SelectAll
    ///
    /// <para><strong>Default Values (empty query):</strong></para>
    ///
    /// - Source: Unchecked.defaultof (will be set by For)
    /// - Where: Query.Empty (no filter)
    /// - OrderBy: [] (no sorting)
    /// - Skip: None (start from beginning)
    /// - Take: None (return all)
    /// - Projection: Projection.SelectAll (complete documents)
    ///
    /// <para><strong>Combining Multiple Where Clauses:</strong></para>
    ///
    /// Multiple where clauses use AND logic:
    /// <code>
    /// query {
    ///     for user in users do
    ///     where (user.Age >= 18)
    ///     where (user.Status = "active")
    ///     where (user.Country = "USA")
    /// }
    /// // Translates to:
    /// Where = Query.And [
    ///     Query.Field("age", FieldOp.Compare(box (CompareOp.Gte 18)));
    ///     Query.Field("status", FieldOp.Compare(box (CompareOp.Eq "active")));
    ///     Query.Field("country", FieldOp.Compare(box (CompareOp.Eq "USA")))
    /// ]
    /// </code>
    ///
    /// The simplify helper optimizes the And structure.
    ///
    /// <para><strong>Order of Operations:</strong></para>
    ///
    /// Quotation tree is walked inside-out (innermost calls first):
    /// 1. For → extract source collection
    /// 2. Where → accumulate predicates
    /// 3. SortBy/SortByDescending → add primary sort
    /// 4. ThenBy/ThenByDescending → add secondary sorts
    /// 5. Skip → set offset
    /// 6. Take → set limit
    /// 7. Select → set projection
    ///
    /// <para><strong>Error Handling:</strong></para>
    ///
    /// - Unsupported expressions: Silently ignored (default case returns accumulator)
    /// - This allows partial query structures during development
    /// - Production queries should use all standard operations
    /// </remarks>
    ///
    /// <example>
    /// <code>
    /// // Simple query
    /// let q1 = query {
    ///     for user in usersCollection do
    ///     where (user.Age >= 18)
    /// }
    ///
    /// let result1 = translate &lt;@ q1 @&gt;
    /// // Returns: {
    /// //   Source = usersCollection
    /// //   Where = Query.Field("age", FieldOp.Compare(box (CompareOp.Gte 18)))
    /// //   OrderBy = []
    /// //   Skip = None
    /// //   Take = None
    /// //   Projection = SelectAll
    /// // }
    ///
    /// // Complex query with all features
    /// let q2 = query {
    ///     for user in usersCollection do
    ///     where (user.Age >= 18 &amp;&amp; user.Status = "active")
    ///     where (user.Country = "USA")
    ///     sortByDescending user.CreatedAt
    ///     thenBy user.Name
    ///     skip 20
    ///     take 10
    ///     select (user.Name, user.Email)
    /// }
    ///
    /// let result2 = translate &lt;@ q2 @&gt;
    /// // Returns: {
    /// //   Source = usersCollection
    /// //   Where = Query.And [
    /// //     Query.And [
    /// //       Query.Field("age", ...);
    /// //       Query.Field("status", ...)
    /// //     ];
    /// //     Query.Field("country", ...)
    /// //   ] |> simplify
    /// //   OrderBy = [("createdAt", Desc); ("name", Asc)]
    /// //   Skip = Some 20
    /// //   Take = Some 10
    /// //   Projection = SelectAll  // TODO: task-116
    /// // }
    /// </code>
    /// </example>
    let translate<'T> (expr: Expr<TranslatedQuery<'T>>) : TranslatedQuery<'T> =
        // Helper to check if a method belongs to QueryBuilder
        let isQueryBuilderMethod (mi: System.Reflection.MethodInfo) (name: string) =
            mi.Name = name && mi.DeclaringType.Name = "QueryBuilder"

        let rec loop (expr: Expr) (query: TranslatedQuery<'T>) : TranslatedQuery<'T> =
            match expr with
            // For source in collection do ...
            | Call(Some _, mi, [ source; _ ]) when isQueryBuilderMethod mi "For" ->
                let collection = evaluateExpr source

                let collectionType = collection.GetType()
                let nameProp = collectionType.GetProperty("Name")

                let collectionName =
                    if isNull nameProp then
                        // Try to get Name from all properties including non-public
                        let allProps =
                            collectionType.GetProperties(
                                System.Reflection.BindingFlags.Instance
                                ||| System.Reflection.BindingFlags.Public
                                ||| System.Reflection.BindingFlags.NonPublic
                            )

                        let nameField = allProps |> Array.tryFind (fun p -> p.Name = "Name")

                        match nameField with
                        | Some prop -> prop.GetValue(collection) :?> string
                        | None ->
                            let propNames = allProps |> Array.map (fun p -> p.Name) |> String.concat ", "
                            failwith $"Collection type '{collectionType.FullName}' does not have a Name property. Available: {propNames}"
                    else
                        nameProp.GetValue(collection) :?> string

                { query with Source = collectionName }

            // where (predicate)
            | Call(Some _, mi, [ source; predicate ]) when isQueryBuilderMethod mi "Where" ->
                let q = loop source query
                let condition = translatePredicate predicate

                let combined =
                    match q.Where with
                    | None -> condition
                    | Some existing -> Query.And [ existing; condition ]

                { q with
                    Where = Some(simplify combined) }

            // sortBy field
            | Call(Some _, mi, [ source; selector ]) when isQueryBuilderMethod mi "SortBy" ->
                let q = loop source query
                let field = extractPropertyName selector

                { q with
                    OrderBy = q.OrderBy @ [ (field, SortDirection.Asc) ] }

            // sortByDescending field
            | Call(Some _, mi, [ source; selector ]) when isQueryBuilderMethod mi "SortByDescending" ->
                let q = loop source query
                let field = extractPropertyName selector

                { q with
                    OrderBy = q.OrderBy @ [ (field, SortDirection.Desc) ] }

            // take n
            | Call(Some _, mi, [ source; Value(count, _) ]) when isQueryBuilderMethod mi "Take" ->
                let q = loop source query
                { q with Take = Some(count :?> int) }

            // skip n
            | Call(Some _, mi, [ source; Value(count, _) ]) when isQueryBuilderMethod mi "Skip" ->
                let q = loop source query
                { q with Skip = Some(count :?> int) }

            // select ... (translate projection)
            | Call(Some _, mi, [ source; projection ]) when isQueryBuilderMethod mi "Select" ->
                let q = loop source query
                let proj = translateProjection projection
                { q with Projection = proj }

            // Unhandled pattern - return accumulator unchanged
            | _ -> query

        // Start with empty query structure
        let empty =
            { Source = ""
              Where = None
              OrderBy = []
              Skip = None
              Take = None
              Projection = Projection.SelectAll }

        loop (expr :> Expr) empty

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
