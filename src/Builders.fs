/// <summary>
/// Computation expression builders for FractalDb schema, options, and transaction construction.
/// Provides F#-idiomatic DSL for building schemas and query options.
/// </summary>
/// <remarks>
/// This module provides computation expression builders that make it easy to construct
/// FractalDb schemas and options using a fluent, type-safe syntax. The builders
/// are designed to work seamlessly with F#'s computation expression syntax.
///
/// All builders are auto-opened for convenience, making them available without explicit
/// module qualification.
/// </remarks>
module FractalDb.Builders

open System.Threading.Tasks
open FractalDb.Types
open FractalDb.Errors
open FractalDb.Operators
open FractalDb.Schema
open FractalDb.Options
open FractalDb.Database

/// <summary>
/// Computation expression builder for constructing type-safe schema definitions.
/// Provides custom operations for defining fields, indexes, timestamps, and validation.
/// </summary>
/// <remarks>
/// The SchemaBuilder enables declarative schema definition using F# computation expression
/// syntax. It builds SchemaDef&lt;'T&gt; values that describe how document types map to
/// SQLite columns and indexes.
///
/// <para>Fields are added in order and the builder accumulates them in the schema state.
/// Multiple custom operations are provided for common patterns (field, indexed, unique).</para>
///
/// <para>Thread Safety: This type is immutable and thread-safe. Multiple threads can use
/// the same SchemaBuilder instance concurrently.</para>
///
/// <para>Performance: Schema construction is lightweight. Schemas are typically defined
/// once at startup and reused throughout the application lifetime.</para>
/// </remarks>
/// <example>
/// <code>
/// type User = { Name: string; Email: string; Age: int; Active: bool }
///
/// let userSchema = schema&lt;User&gt; {
///     field "name" SqliteType.Text
///     unique "email" SqliteType.Text
///     indexed "age" SqliteType.Integer
///     field "active" SqliteType.Integer
///     timestamps
/// }
/// </code>
/// </example>
type SchemaBuilder<'T>() =
    /// <summary>
    /// Yield operation for computation expression (returns empty schema definition).
    /// </summary>
    /// <param name="x">The unit value (ignored).</param>
    /// <returns>An empty schema definition with no fields, indexes, or validation.</returns>
    member _.Yield(_) =
        { Fields = []
          Indexes = []
          Timestamps = false
          Validate = None }

    /// <summary>
    /// Zero operation for computation expression (returns empty schema definition).
    /// </summary>
    /// <returns>An empty schema definition with no fields, indexes, or validation.</returns>
    member _.Zero() =
        { Fields = []
          Indexes = []
          Timestamps = false
          Validate = None }

    /// <summary>
    /// Delay operation for computation expression (defers evaluation).
    /// </summary>
    /// <param name="f">The delayed computation function.</param>
    /// <returns>The result of invoking the delayed computation.</returns>
    member _.Delay(f) = f ()

    /// <summary>
    /// Define a field in the schema.
    /// Creates a FieldDef with the specified name and type, with optional parameters.
    /// </summary>
    /// <param name="state">The current schema state.</param>
    /// <param name="name">The field name (used for column naming and queries).</param>
    /// <param name="sqlType">The SQLite type for this field.</param>
    /// <param name="indexed">Whether to create an index (default: false).</param>
    /// <param name="unique">Whether the field must be unique (default: false).</param>
    /// <param name="nullable">Whether NULL values are allowed (default: false).</param>
    /// <param name="path">Custom JSON path (default: None, uses $.name).</param>
    /// <returns>A new schema state with the field added.</returns>
    /// <remarks>
    /// The 'field' operation is the primary way to define schema fields. It provides
    /// full control over all field properties through optional parameters.
    ///
    /// <para>Fields are added to the schema's Fields list in the order they are defined.
    /// Field order matters for some operations and affects the generated SQL.</para>
    ///
    /// <para>Note: Setting unique=true automatically creates an index. Use 'unique'
    /// operation for a shorthand that sets both indexed and unique to true.</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Simple field
    /// let schema1 = schema&lt;User&gt; {
    ///     field "name" SqliteType.Text
    /// }
    ///
    /// // Field with options
    /// let schema2 = schema&lt;User&gt; {
    ///     field "email" SqliteType.Text (indexed=true, unique=true, nullable=false)
    /// }
    ///
    /// // Nullable field
    /// let schema3 = schema&lt;User&gt; {
    ///     field "middleName" SqliteType.Text (nullable=true)
    /// }
    ///
    /// // Custom JSON path
    /// let schema4 = schema&lt;User&gt; {
    ///     field "zip" SqliteType.Text (path="$.address.zipCode")
    /// }
    /// </code>
    /// </example>
    [<CustomOperation("field")>]
    member _.Field
        (
            state: SchemaDef<'T>,
            name: string,
            sqlType: SqliteType,
            ?indexed: bool,
            ?unique: bool,
            ?nullable: bool,
            ?path: string
        ) =
        let field =
            { Name = name
              Path = path
              SqlType = sqlType
              Indexed = defaultArg indexed false
              Unique = defaultArg unique false
              Nullable = defaultArg nullable false }

        { state with
            Fields = state.Fields @ [ field ] }

    /// <summary>
    /// Define an indexed field (shorthand for field with indexed=true).
    /// </summary>
    /// <param name="state">The current schema state.</param>
    /// <param name="name">The field name.</param>
    /// <param name="sqlType">The SQLite type for this field.</param>
    /// <param name="unique">Whether the field must be unique (default: false).</param>
    /// <param name="nullable">Whether NULL values are allowed (default: false).</param>
    /// <param name="path">Custom JSON path (default: None, uses $.name).</param>
    /// <returns>A new schema state with the indexed field added.</returns>
    /// <remarks>
    /// The 'indexed' operation is a convenience shorthand for defining fields that
    /// need an index but are not unique. It automatically sets indexed=true.
    ///
    /// <para>Indexes improve query performance for fields frequently used in WHERE
    /// clauses, ORDER BY, or JOIN conditions.</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// let userSchema = schema&lt;User&gt; {
    ///     field "name" SqliteType.Text
    ///     indexed "age" SqliteType.Integer  // Will be indexed for fast queries
    ///     indexed "createdAt" SqliteType.Integer
    /// }
    ///
    /// // Indexed field with unique constraint
    /// let accountSchema = schema&lt;Account&gt; {
    ///     indexed "username" SqliteType.Text (unique=true)
    /// }
    /// </code>
    /// </example>
    [<CustomOperation("indexed")>]
    member _.Indexed
        (state: SchemaDef<'T>, name: string, sqlType: SqliteType, ?unique: bool, ?nullable: bool, ?path: string)
        =
        let field =
            { Name = name
              Path = path
              SqlType = sqlType
              Indexed = true
              Unique = defaultArg unique false
              Nullable = defaultArg nullable false }

        { state with
            Fields = state.Fields @ [ field ] }

    /// <summary>
    /// Define a unique indexed field (shorthand for field with indexed=true, unique=true).
    /// </summary>
    /// <param name="state">The current schema state.</param>
    /// <param name="name">The field name.</param>
    /// <param name="sqlType">The SQLite type for this field.</param>
    /// <param name="nullable">Whether NULL values are allowed (default: false).</param>
    /// <param name="path">Custom JSON path (default: None, uses $.name).</param>
    /// <returns>A new schema state with the unique indexed field added.</returns>
    /// <remarks>
    /// The 'unique' operation is a convenience shorthand for defining fields that
    /// must be unique. It automatically creates a unique index, ensuring no two
    /// documents can have the same value for this field.
    ///
    /// <para>Unique constraints are enforced by SQLite at insert/update time.
    /// Attempts to insert duplicate values will fail with a UniqueConstraint error.</para>
    ///
    /// <para>Note: NULL values are considered distinct in SQLite unique constraints,
    /// so multiple NULL values are allowed even in unique fields (unless nullable=false).</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// let userSchema = schema&lt;User&gt; {
    ///     field "name" SqliteType.Text
    ///     unique "email" SqliteType.Text  // No two users can have the same email
    ///     unique "username" SqliteType.Text
    /// }
    ///
    /// // Unique but nullable (multiple NULLs allowed)
    /// let productSchema = schema&lt;Product&gt; {
    ///     unique "sku" SqliteType.Text (nullable=true)
    /// }
    /// </code>
    /// </example>
    [<CustomOperation("unique")>]
    member _.Unique(state: SchemaDef<'T>, name: string, sqlType: SqliteType, ?nullable: bool, ?path: string) =
        let field =
            { Name = name
              Path = path
              SqlType = sqlType
              Indexed = true
              Unique = true
              Nullable = defaultArg nullable false }

        { state with
            Fields = state.Fields @ [ field ] }

    /// <summary>
    /// Enable automatic timestamp management for the schema.
    /// Adds createdAt and updatedAt timestamps that are managed automatically.
    /// </summary>
    /// <param name="state">The current schema state.</param>
    /// <returns>A new schema state with timestamps enabled.</returns>
    /// <remarks>
    /// When timestamps are enabled, FractalDb automatically manages:
    /// <list type="bullet">
    /// <item>createdAt: Set to current time on document insertion (never changes)</item>
    /// <item>updatedAt: Set to current time on insertion, updated on every modification</item>
    /// </list>
    ///
    /// <para>Timestamps are stored as Unix milliseconds (INTEGER type) and are
    /// accessible through the document's metadata.</para>
    ///
    /// <para>Timestamp fields are automatically indexed for efficient time-based queries.</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// let userSchema = schema&lt;User&gt; {
    ///     field "name" SqliteType.Text
    ///     unique "email" SqliteType.Text
    ///     timestamps  // Enable automatic timestamp tracking
    /// }
    ///
    /// // Later, query by timestamps
    /// let recentUsers = query {
    ///     field "createdAt" (Query.gte (DateTimeOffset.UtcNow.AddDays(-7.0).ToUnixTimeMilliseconds()))
    /// }
    /// </code>
    /// </example>
    [<CustomOperation("timestamps")>]
    member _.Timestamps(state: SchemaDef<'T>) = { state with Timestamps = true }

    /// <summary>
    /// Define a compound index spanning multiple fields.
    /// Creates an index that includes multiple columns for efficient multi-field queries.
    /// </summary>
    /// <param name="state">The current schema state.</param>
    /// <param name="name">The index name (must be unique within the collection).</param>
    /// <param name="fields">The list of field names to include in the index (in order).</param>
    /// <param name="unique">Whether the combination of fields must be unique (default: false).</param>
    /// <returns>A new schema state with the compound index added.</returns>
    /// <remarks>
    /// Compound indexes are useful for queries that filter or sort by multiple fields.
    /// The order of fields in the index matters: queries using leftmost fields can
    /// benefit from the index, but queries skipping the first field cannot.
    ///
    /// <para>Example: An index on (category, price) helps queries filtering by category,
    /// or by both category and price, but not queries filtering only by price.</para>
    ///
    /// <para>Unique compound indexes enforce that the combination of field values is
    /// unique, but individual fields can have duplicates.</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// let productSchema = schema&lt;Product&gt; {
    ///     field "category" SqliteType.Text
    ///     field "name" SqliteType.Text
    ///     field "price" SqliteType.Integer
    ///
    ///     // Efficient queries by category + name
    ///     compoundIndex "idx_category_name" ["category"; "name"]
    ///
    ///     // Unique combination of category + SKU
    ///     compoundIndex "idx_category_sku" ["category"; "sku"] (unique=true)
    ///     timestamps
    /// }
    ///
    /// // This query benefits from the compound index
    /// let electronics = query {
    ///     field "category" (Query.eq "Electronics")
    ///     field "name" (Query.contains "Phone")
    /// }
    /// </code>
    /// </example>
    [<CustomOperation("compoundIndex")>]
    member _.CompoundIndex(state: SchemaDef<'T>, name: string, fields: list<string>, ?unique: bool) =
        let index =
            { Name = name
              Fields = fields
              Unique = defaultArg unique false }

        { state with
            Indexes = state.Indexes @ [ index ] }

    /// <summary>
    /// Add a validation function to the schema.
    /// The function is called on every insert/update to validate documents.
    /// </summary>
    /// <param name="state">The current schema state.</param>
    /// <param name="validator">A function that validates documents and returns Result.</param>
    /// <returns>A new schema state with the validation function added.</returns>
    /// <remarks>
    /// The validation function receives a document value and must return:
    /// <list type="bullet">
    /// <item>Ok document - Validation passed (can return modified document)</item>
    /// <item>Error message - Validation failed with error message</item>
    /// </list>
    ///
    /// <para>Validation is performed before serialization and database operations.
    /// Failed validation prevents the operation and returns a ValidationFailed error.</para>
    ///
    /// <para>Only one validation function can be set per schema. Calling validate
    /// multiple times replaces the previous validator.</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// type User = { Name: string; Email: string; Age: int }
    ///
    /// let userSchema = schema&lt;User&gt; {
    ///     field "name" SqliteType.Text
    ///     unique "email" SqliteType.Text
    ///     indexed "age" SqliteType.Integer
    ///
    ///     validate (fun user -&gt;
    ///         if user.Age &lt; 0 then
    ///             Error "Age must be non-negative"
    ///         elif user.Age &gt; 150 then
    ///             Error "Age must be realistic"
    ///         elif not (user.Email.Contains("@")) then
    ///             Error "Email must contain @"
    ///         elif String.IsNullOrWhiteSpace(user.Name) then
    ///             Error "Name cannot be empty"
    ///         else
    ///             Ok user
    ///     )
    /// }
    ///
    /// // Validation with document transformation
    /// let normalizedSchema = schema&lt;User&gt; {
    ///     field "name" SqliteType.Text
    ///     unique "email" SqliteType.Text
    ///
    ///     validate (fun user -&gt;
    ///         // Normalize email to lowercase
    ///         Ok { user with Email = user.Email.ToLowerInvariant() }
    ///     )
    /// }
    /// </code>
    /// </example>
    [<CustomOperation("validate")>]
    member _.Validate(state: SchemaDef<'T>, validator: 'T -> Result<'T, string>) =
        { state with Validate = Some validator }

/// <summary>
/// Auto-opened module providing global schema builder instances.
/// Makes schema builder available without explicit module qualification.
/// </summary>
/// <remarks>
/// This module is automatically opened when you open FractalDb.Builders or FractalDb,
/// making the 'schema&lt;'T&gt;' builder available for immediate use in computation expressions.
/// </remarks>
[<AutoOpen>]
module SchemaBuilderInstance =
    /// <summary>
    /// Global SchemaBuilder instance for computation expressions.
    /// Use this in 'schema { }' computation expressions to build type-safe schemas.
    /// </summary>
    /// <remarks>
    /// This is a generic function that creates a SchemaBuilder&lt;'T&gt; instance for
    /// the specified type. The type parameter is typically inferred from usage context.
    ///
    /// <para>Schema builders are stateless and thread-safe.</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// type User = { Name: string; Email: string; Age: int }
    ///
    /// // Type parameter inferred from context
    /// let userSchema = schema&lt;User&gt; {
    ///     field "name" SqliteType.Text
    ///     unique "email" SqliteType.Text
    ///     indexed "age" SqliteType.Integer
    ///     timestamps
    /// }
    ///
    /// // Use with collection
    /// let users = db.Collection("users", userSchema)
    /// </code>
    /// </example>
    let schema<'T> = SchemaBuilder<'T>()

/// <summary>
/// Computation expression builder for constructing query options.
/// Provides custom operations for sorting, pagination, field projection, and search.
/// </summary>
/// <remarks>
/// The OptionsBuilder enables declarative construction of QueryOptions&lt;'T&gt; values
/// using F# computation expression syntax. It wraps the QueryOptions module functions
/// in a fluent, composable builder API.
///
/// <para>Operations can be chained in any order within the computation expression.
/// The builder internally uses the QueryOptions pipeline functions.</para>
///
/// <para>Thread Safety: This type is immutable and thread-safe. Multiple threads can use
/// the same OptionsBuilder instance concurrently.</para>
///
/// <para>Performance: Options construction is lightweight and creates immutable
/// QueryOptions records.</para>
/// </remarks>
/// <example>
/// <code>
/// type User = { Name: string; Email: string; Age: int }
///
/// let userOptions = options&lt;User&gt; {
///     sortDesc "createdAt"
///     sortAsc "name"
///     limit 20
///     skip 40
///     select ["name"; "email"]
/// }
/// </code>
/// </example>
type OptionsBuilder<'T>() =
    /// <summary>
    /// Yield operation for computation expression (returns empty options).
    /// </summary>
    /// <param name="x">The unit value (ignored).</param>
    /// <returns>Empty query options.</returns>
    member _.Yield(_) = QueryOptions.empty<'T>

    /// <summary>
    /// Zero operation for computation expression (returns empty options).
    /// </summary>
    /// <returns>Empty query options.</returns>
    member _.Zero() = QueryOptions.empty<'T>

    /// <summary>
    /// Sort by a field with specified direction.
    /// </summary>
    /// <param name="state">The current options state.</param>
    /// <param name="field">The field name to sort by.</param>
    /// <param name="dir">The sort direction (Ascending or Descending).</param>
    /// <returns>New options with the sort specification added.</returns>
    /// <remarks>
    /// Multiple sortBy operations can be chained to create multi-level sorting.
    /// Sorts are applied in the order they are defined.
    /// </remarks>
    /// <example>
    /// <code>
    /// let opts = options&lt;Product&gt; {
    ///     sortBy "category" SortDirection.Ascending
    ///     sortBy "price" SortDirection.Descending
    /// }
    /// // Sorts by category (asc), then by price (desc) within each category
    /// </code>
    /// </example>
    [<CustomOperation("sortBy")>]
    member _.SortBy(state, field, dir) = QueryOptions.sortBy field dir state

    /// <summary>
    /// Sort by a field in ascending order (shorthand for sortBy with Ascending).
    /// </summary>
    /// <param name="state">The current options state.</param>
    /// <param name="field">The field name to sort by.</param>
    /// <returns>New options with ascending sort added.</returns>
    /// <remarks>
    /// This is a convenience operation equivalent to sortBy field SortDirection.Ascending.
    /// Multiple sort operations can be chained for multi-level sorting.
    /// </remarks>
    /// <example>
    /// <code>
    /// let opts = options&lt;User&gt; {
    ///     sortAsc "lastName"
    ///     sortAsc "firstName"
    /// }
    /// // Results sorted by lastName, then firstName (both ascending)
    /// </code>
    /// </example>
    [<CustomOperation("sortAsc")>]
    member _.SortAsc(state, field) = QueryOptions.sortAsc field state

    /// <summary>
    /// Sort by a field in descending order (shorthand for sortBy with Descending).
    /// </summary>
    /// <param name="state">The current options state.</param>
    /// <param name="field">The field name to sort by.</param>
    /// <returns>New options with descending sort added.</returns>
    /// <remarks>
    /// This is a convenience operation equivalent to sortBy field SortDirection.Descending.
    /// Commonly used for time-based fields to get newest items first.
    /// </remarks>
    /// <example>
    /// <code>
    /// let recentUsers = options&lt;User&gt; {
    ///     sortDesc "createdAt"
    ///     limit 10
    /// }
    /// // Returns 10 most recently created users
    /// </code>
    /// </example>
    [<CustomOperation("sortDesc")>]
    member _.SortDesc(state, field) = QueryOptions.sortDesc field state

    /// <summary>
    /// Limit the number of results returned.
    /// </summary>
    /// <param name="state">The current options state.</param>
    /// <param name="n">The maximum number of results to return.</param>
    /// <returns>New options with limit set.</returns>
    /// <remarks>
    /// Use limit for pagination or to restrict result set size. Combine with skip
    /// for offset-based pagination.
    ///
    /// <para>Note: Limit is applied after sorting and filtering.</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // First page (items 0-19)
    /// let page1 = options&lt;User&gt; {
    ///     sortAsc "name"
    ///     limit 20
    /// }
    ///
    /// // Second page (items 20-39)
    /// let page2 = options&lt;User&gt; {
    ///     sortAsc "name"
    ///     skip 20
    ///     limit 20
    /// }
    /// </code>
    /// </example>
    [<CustomOperation("limit")>]
    member _.Limit(state, n) = QueryOptions.limit n state

    /// <summary>
    /// Skip a number of results (offset pagination).
    /// </summary>
    /// <param name="state">The current options state.</param>
    /// <param name="n">The number of results to skip.</param>
    /// <returns>New options with skip offset set.</returns>
    /// <remarks>
    /// Use skip with limit for offset-based pagination. Skip is applied after
    /// sorting and filtering but before limit.
    ///
    /// <para>Note: For large offsets, cursor-based pagination (cursorAfter/cursorBefore)
    /// is more efficient than skip.</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Page 3 of results (skip first 40, take next 20)
    /// let page3 = options&lt;Product&gt; {
    ///     sortDesc "popularity"
    ///     skip 40
    ///     limit 20
    /// }
    /// </code>
    /// </example>
    [<CustomOperation("skip")>]
    member _.Skip(state, n) = QueryOptions.skip n state

    /// <summary>
    /// Select only specific fields to include in results (projection).
    /// </summary>
    /// <param name="state">The current options state.</param>
    /// <param name="fields">List of field names to include.</param>
    /// <returns>New options with field projection set.</returns>
    /// <remarks>
    /// Use select to reduce payload size by returning only needed fields.
    /// The id field is always included regardless of select.
    ///
    /// <para>Note: select and omit are mutually exclusive. The last one specified wins.</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Return only name and email fields
    /// let opts = options&lt;User&gt; {
    ///     select ["name"; "email"]
    ///     sortAsc "name"
    /// }
    /// // Result documents will only contain id, name, and email
    /// </code>
    /// </example>
    [<CustomOperation("select")>]
    member _.Select(state, fields) = QueryOptions.select fields state

    /// <summary>
    /// Omit specific fields from results (inverse projection).
    /// </summary>
    /// <param name="state">The current options state.</param>
    /// <param name="fields">List of field names to exclude.</param>
    /// <returns>New options with field omission set.</returns>
    /// <remarks>
    /// Use omit to exclude specific fields while returning all others.
    /// Useful when you want most fields but need to exclude sensitive data.
    ///
    /// <para>Note: select and omit are mutually exclusive. The last one specified wins.</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Return all fields except password and internalNotes
    /// let opts = options&lt;User&gt; {
    ///     omit ["password"; "internalNotes"]
    ///     sortAsc "name"
    /// }
    /// </code>
    /// </example>
    [<CustomOperation("omit")>]
    member _.Omit(state, fields) = QueryOptions.omit fields state

    /// <summary>
    /// Enable full-text search across specified fields.
    /// </summary>
    /// <param name="state">The current options state.</param>
    /// <param name="text">The search text/query.</param>
    /// <param name="fields">List of field names to search.</param>
    /// <returns>New options with search specification set.</returns>
    /// <remarks>
    /// Search performs case-insensitive text matching across the specified fields.
    /// Results are filtered to only include documents where at least one field
    /// contains the search text.
    ///
    /// <para>Note: Search is applied as a filter, so it works in combination with
    /// regular query conditions.</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Search for "Smith" in name and email fields
    /// let opts = options&lt;User&gt; {
    ///     search "Smith" ["name"; "email"]
    ///     sortAsc "name"
    ///     limit 50
    /// }
    ///
    /// // Combine with query
    /// let activeSmithUsers = query {
    ///     field "active" (Query.eq true)
    /// }
    /// let opts = options&lt;User&gt; {
    ///     search "Smith" ["name"; "email"]
    /// }
    /// // Returns active users with "Smith" in name or email
    /// </code>
    /// </example>
    [<CustomOperation("search")>]
    member _.Search(state, text, fields) = QueryOptions.search text fields state

    /// <summary>
    /// Set cursor for forward pagination (results after specified ID).
    /// </summary>
    /// <param name="state">The current options state.</param>
    /// <param name="id">The document ID to start after.</param>
    /// <returns>New options with cursor set for forward pagination.</returns>
    /// <remarks>
    /// Cursor-based pagination is more efficient than skip for large offsets.
    /// Use cursorAfter to get results after a specific document ID.
    ///
    /// <para>Cursors work best with sorted results and are more stable than
    /// offset-based pagination when data is frequently modified.</para>
    ///
    /// <para>Note: cursorAfter and cursorBefore are mutually exclusive.</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // First page
    /// let page1 = options&lt;User&gt; {
    ///     sortAsc "name"
    ///     limit 20
    /// }
    ///
    /// // Next page (after last ID from page1)
    /// let page2 = options&lt;User&gt; {
    ///     sortAsc "name"
    ///     cursorAfter lastIdFromPage1
    ///     limit 20
    /// }
    /// </code>
    /// </example>
    [<CustomOperation("cursorAfter")>]
    member _.CursorAfter(state, id) = QueryOptions.cursorAfter id state

    /// <summary>
    /// Set cursor for backward pagination (results before specified ID).
    /// </summary>
    /// <param name="state">The current options state.</param>
    /// <param name="id">The document ID to start before.</param>
    /// <returns>New options with cursor set for backward pagination.</returns>
    /// <remarks>
    /// Use cursorBefore to get results before a specific document ID.
    /// Useful for implementing "previous page" functionality.
    ///
    /// <para>Note: cursorAfter and cursorBefore are mutually exclusive.</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Previous page (before first ID from current page)
    /// let prevPage = options&lt;User&gt; {
    ///     sortAsc "name"
    ///     cursorBefore firstIdFromCurrentPage
    ///     limit 20
    /// }
    /// </code>
    /// </example>
    [<CustomOperation("cursorBefore")>]
    member _.CursorBefore(state, id) = QueryOptions.cursorBefore id state

/// <summary>
/// Auto-opened module providing global options builder instances.
/// Makes options builder available without explicit module qualification.
/// </summary>
/// <remarks>
/// This module is automatically opened when you open FractalDb.Builders or FractalDb,
/// making the 'options&lt;'T&gt;' builder available for immediate use in computation expressions.
/// </remarks>
[<AutoOpen>]
module OptionsBuilderInstance =
    /// <summary>
    /// Global OptionsBuilder instance for computation expressions.
    /// Use this in 'options { }' computation expressions to build query options.
    /// </summary>
    /// <remarks>
    /// This is a generic function that creates an OptionsBuilder&lt;'T&gt; instance for
    /// the specified type. The type parameter is typically inferred from usage context.
    ///
    /// <para>Options builders are stateless and thread-safe.</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// type Product = { Name: string; Category: string; Price: int }
    ///
    /// // Type parameter inferred from context
    /// let productOptions = options&lt;Product&gt; {
    ///     sortDesc "price"
    ///     limit 10
    ///     select ["name"; "price"]
    /// }
    ///
    /// // Use with collection operations
    /// let! topProducts = collection |&gt; Collection.find Query.Empty productOptions
    /// </code>
    /// </example>
    let options<'T> = OptionsBuilder<'T>()

/// <summary>
/// Result-aware computation expression builder for database transactions.
/// Provides automatic commit/rollback based on Result values within a transaction scope.
/// </summary>
/// <remarks>
/// The TransactionBuilder enables writing transactional database operations using
/// F# computation expression syntax. It automatically handles Result-based error
/// propagation and transaction lifecycle management.
///
/// <para>Key features:</para>
/// <list type="bullet">
/// <item>Automatic commit on Ok results</item>
/// <item>Automatic rollback on Error results</item>
/// <item>Short-circuit evaluation on first error</item>
/// <item>Support for both async (Task) and sync (Result) operations</item>
/// <item>Exception handling with try/with and try/finally</item>
/// </list>
///
/// <para>The builder is created via the FractalDb.Transact property, which provides
/// a computation expression scope that wraps operations in a transaction.</para>
///
/// <para>Thread Safety: Each transaction builder instance is tied to a specific
/// database instance. Transactions are not thread-safe and should be used from
/// a single thread.</para>
///
/// <para>Performance: Transactions add overhead but ensure ACID properties.
/// Use transactions when atomicity is required across multiple operations.</para>
/// </remarks>
/// <example>
/// <code>
/// type User = { Name: string; Email: string; Balance: int }
///
/// // Transaction with automatic commit/rollback
/// let! result = db.Transact {
///     let users = db.Collection&lt;User&gt;("users", userSchema)
///
///     // All operations within transaction scope
///     let! user1 = users |&gt; Collection.findById "user1"
///     let! user2 = users |&gt; Collection.findById "user2"
///
///     // Validation (returns Error on failure, causing rollback)
///     if user1.Balance &lt; 100 then
///         return Error (FractalError.ValidationFailed "Insufficient balance")
///
///     // Updates (both succeed or both rolled back)
///     let! _ = users |&gt; Collection.updateById "user1" { user1 with Balance = user1.Balance - 100 }
///     let! _ = users |&gt; Collection.updateById "user2" { user2 with Balance = user2.Balance + 100 }
///
///     return Ok ()
/// }
/// </code>
/// </example>
type TransactionBuilder(db: FractalDb) =

    /// <summary>
    /// Bind operation for chaining Task&lt;FractalResult&lt;'T&gt;&gt; computations.
    /// Implements monadic bind for Result within Task context.
    /// </summary>
    /// <param name="taskValue">The task returning a FractalResult to bind.</param>
    /// <param name="f">The continuation function to apply on success.</param>
    /// <returns>A task with the result of the continuation or propagated error.</returns>
    /// <remarks>
    /// This overload handles async operations that return Task&lt;FractalResult&lt;'T&gt;&gt;.
    /// If the task result is Ok, the continuation is applied. If Error, the error
    /// is propagated without executing the continuation (short-circuit evaluation).
    /// </remarks>
    member _.Bind(taskValue: Task<FractalResult<'T>>, f: 'T -> Task<FractalResult<'U>>) : Task<FractalResult<'U>> =
        task {
            match! taskValue with
            | Ok value -> return! f value
            | Error e -> return Error e
        }

    /// <summary>
    /// Bind operation for chaining FractalResult&lt;'T&gt; computations.
    /// Implements monadic bind for synchronous Result values.
    /// </summary>
    /// <param name="result">The FractalResult to bind.</param>
    /// <param name="f">The continuation function to apply on success.</param>
    /// <returns>A task with the result of the continuation or propagated error.</returns>
    /// <remarks>
    /// This overload handles synchronous Result values within the transaction.
    /// Useful for mixing sync validation logic with async database operations.
    /// </remarks>
    member _.Bind(result: FractalResult<'T>, f: 'T -> Task<FractalResult<'U>>) : Task<FractalResult<'U>> =
        task {
            match result with
            | Ok value -> return! f value
            | Error e -> return Error e
        }

    /// <summary>
    /// Return operation wrapping a value in a successful Result within a Task.
    /// </summary>
    /// <param name="value">The value to wrap.</param>
    /// <returns>A completed task with Ok result containing the value.</returns>
    /// <remarks>
    /// Use 'return' in computation expressions to wrap values in the Result context.
    /// </remarks>
    member _.Return(value: 'T) : Task<FractalResult<'T>> = Task.FromResult(Ok value)

    /// <summary>
    /// ReturnFrom operation for returning an existing Task&lt;FractalResult&lt;'T&gt;&gt;.
    /// </summary>
    /// <param name="taskValue">The task to return directly.</param>
    /// <returns>The task unchanged.</returns>
    /// <remarks>
    /// Use 'return!' in computation expressions to return an existing task result.
    /// </remarks>
    member _.ReturnFrom(taskValue: Task<FractalResult<'T>>) : Task<FractalResult<'T>> = taskValue

    /// <summary>
    /// Zero operation returning an empty success result.
    /// </summary>
    /// <returns>A completed task with Ok unit result.</returns>
    /// <remarks>
    /// Used by the computation expression framework for empty branches.
    /// </remarks>
    member _.Zero() : Task<FractalResult<unit>> = Task.FromResult(Ok())

    /// <summary>
    /// Delay operation deferring computation execution.
    /// </summary>
    /// <param name="f">The delayed computation function.</param>
    /// <returns>The delayed computation function unchanged.</returns>
    /// <remarks>
    /// Delays computation until Run is called, allowing proper transaction setup.
    /// </remarks>
    member _.Delay(f: unit -> Task<FractalResult<'T>>) = f

    /// <summary>
    /// Run operation executing the computation within a transaction scope.
    /// Automatically commits on Ok result or rolls back on Error result.
    /// </summary>
    /// <param name="f">The delayed computation to execute.</param>
    /// <returns>A task with the computation result.</returns>
    /// <remarks>
    /// This is the key operation that wraps the entire computation in a transaction.
    /// It delegates to FractalDb.ExecuteTransaction which handles the transaction
    /// lifecycle (begin, commit, rollback) based on the Result value.
    ///
    /// <para>Commit happens when the computation returns Ok.</para>
    /// <para>Rollback happens when the computation returns Error or throws an exception.</para>
    /// </remarks>
    member _.Run(f: unit -> Task<FractalResult<'T>>) : Task<FractalResult<'T>> = db.ExecuteTransaction(fun _tx -> f ())

    /// <summary>
    /// TryWith operation for exception handling within transactions.
    /// </summary>
    /// <param name="computation">The computation that may throw.</param>
    /// <param name="handler">The exception handler function.</param>
    /// <returns>A delayed computation with exception handling.</returns>
    /// <remarks>
    /// Enables try/with blocks in transaction computation expressions.
    /// The handler can convert exceptions to Error results or re-throw.
    /// </remarks>
    member _.TryWith(computation: unit -> Task<FractalResult<'T>>, handler: exn -> Task<FractalResult<'T>>) =
        fun () ->
            task {
                try
                    return! computation ()
                with ex ->
                    return! handler ex
            }

    /// <summary>
    /// TryFinally operation for cleanup within transactions.
    /// </summary>
    /// <param name="computation">The computation to execute.</param>
    /// <param name="compensation">The cleanup function to run regardless of success/failure.</param>
    /// <returns>A delayed computation with cleanup logic.</returns>
    /// <remarks>
    /// Enables try/finally blocks in transaction computation expressions.
    /// The compensation function always executes, even if the computation fails.
    /// </remarks>
    member _.TryFinally(computation: unit -> Task<FractalResult<'T>>, compensation: unit -> unit) =
        fun () ->
            task {
                try
                    return! computation ()
                finally
                    compensation ()
            }

/// <summary>
/// Extension methods for FractalDb to provide builder instances.
/// </summary>
type FractalDb with
    /// <summary>
    /// Creates a TransactionBuilder for Result-aware transactional operations.
    /// </summary>
    /// <returns>A TransactionBuilder instance bound to this database.</returns>
    /// <remarks>
    /// Use this property to create transaction computation expressions:
    ///
    /// <para>The transaction automatically commits if all operations succeed (return Ok)
    /// or rolls back if any operation fails (returns Error) or throws an exception.</para>
    ///
    /// <para>All database operations within the transaction scope are executed on the
    /// same underlying SQLite transaction, ensuring ACID properties.</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Transfer balance between users atomically
    /// let! result = db.Transact {
    ///     let users = db.Collection&lt;User&gt;("users", userSchema)
    ///
    ///     let! sender = users |&gt; Collection.findById senderId
    ///     let! receiver = users |&gt; Collection.findById receiverId
    ///
    ///     // Validation
    ///     if sender.Balance &lt; amount then
    ///         return Error (FractalError.ValidationFailed "Insufficient funds")
    ///
    ///     // Both updates succeed or both fail
    ///     let! _ = users |&gt; Collection.updateById senderId
    ///         { sender with Balance = sender.Balance - amount }
    ///     let! _ = users |&gt; Collection.updateById receiverId
    ///         { receiver with Balance = receiver.Balance + amount }
    ///
    ///     return Ok amount
    /// }
    ///
    /// match result with
    /// | Ok amount -&gt; printfn "Transferred %d" amount
    /// | Error err -&gt; printfn "Transaction failed: %s" err.Message
    /// </code>
    /// </example>
    member this.Transact = TransactionBuilder(this)
