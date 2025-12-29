module FractalDb.Options

/// <summary>
/// Sort direction for query result ordering.
/// </summary>
///
/// <remarks>
/// SortDirection specifies the order in which query results should be sorted.
/// Used in combination with field names to create sort specifications.
///
/// Cases:
/// - Ascending: Sort from smallest to largest (A-Z, 0-9, oldest-newest)
/// - Descending: Sort from largest to smallest (Z-A, 9-0, newest-oldest)
///
/// The RequireQualifiedAccess attribute forces usage as SortDirection.Ascending
/// or SortDirection.Descending, preventing namespace pollution and improving
/// code clarity.
///
/// Sorting behavior:
/// - Text: Lexicographic (dictionary) order, case-sensitive by default
/// - Numbers: Numeric comparison (2 &lt; 10, not "10" &lt; "2")
/// - Timestamps: Chronological order
/// - NULL values: Always sort first (SQLite behavior)
/// </remarks>
///
/// <example>
/// <code>
/// // Sort by name ascending
/// let sortByName = ("name", SortDirection.Ascending)
///
/// // Sort by createdAt descending (newest first)
/// let sortByNewest = ("createdAt", SortDirection.Descending)
///
/// // Multi-field sort: email asc, then age desc
/// let multiSort = [
///     ("email", SortDirection.Ascending)
///     ("age", SortDirection.Descending)
/// ]
///
/// // Usage in QueryOptions
/// let options = {
///     Sort = [("name", SortDirection.Ascending)]
///     Limit = Some 10
///     Skip = None
///     Cursor = None
///     Search = None
/// }
/// </code>
/// </example>
[<RequireQualifiedAccess>]
type SortDirection =
    /// <summary>
    /// Sort from smallest to largest.
    /// </summary>
    /// <remarks>
    /// Ascending order:
    /// - Text: A → Z (case-sensitive, uppercase before lowercase)
    /// - Numbers: 0 → 9, -100 → 100
    /// - Timestamps: Oldest → Newest
    /// - Booleans: false (0) → true (1)
    /// - NULL: Always first
    ///
    /// SQL equivalent: ORDER BY field ASC
    /// </remarks>
    /// <example>
    /// <code>
    /// // Sort users by name alphabetically
    /// ("name", SortDirection.Ascending)
    /// // Result: ["Alice", "Bob", "Charlie"]
    ///
    /// // Sort by age, youngest first
    /// ("age", SortDirection.Ascending)
    /// // Result: [18, 25, 30, 45]
    /// </code>
    /// </example>
    | Ascending
    
    /// <summary>
    /// Sort from largest to smallest.
    /// </summary>
    /// <remarks>
    /// Descending order:
    /// - Text: Z → A (case-sensitive, lowercase before uppercase)
    /// - Numbers: 100 → -100, 9 → 0
    /// - Timestamps: Newest → Oldest
    /// - Booleans: true (1) → false (0)
    /// - NULL: Always first (even in descending)
    ///
    /// SQL equivalent: ORDER BY field DESC
    ///
    /// Note: NULL values sort first regardless of direction (SQLite default).
    /// </remarks>
    /// <example>
    /// <code>
    /// // Sort posts by createdAt, newest first
    /// ("createdAt", SortDirection.Descending)
    /// // Result: [2024-01-03, 2024-01-02, 2024-01-01]
    ///
    /// // Sort by score, highest first
    /// ("score", SortDirection.Descending)
    /// // Result: [100, 85, 72, 50]
    /// </code>
    /// </example>
    | Descending

/// <summary>
/// Cursor specification for cursor-based pagination.
/// </summary>
///
/// <remarks>
/// CursorSpec enables efficient keyset pagination by using opaque cursor tokens
/// instead of OFFSET/LIMIT. Cursors are more efficient for large datasets and
/// provide consistent results even when data changes between requests.
///
/// Fields:
/// - After: Retrieve results after this cursor (for forward pagination)
/// - Before: Retrieve results before this cursor (for backward pagination)
///
/// Cursor Behavior:
/// - Cursors are opaque strings (typically base64-encoded document IDs)
/// - Only one of After or Before should be set (not both)
/// - Combine with Limit to control page size
/// - Sort order determines pagination direction
///
/// Advantages over offset-based pagination:
/// 1. Consistent results when data changes
/// 2. O(1) seek performance (vs O(n) for OFFSET)
/// 3. Works with real-time data updates
/// 4. No "page drift" when items are inserted/deleted
///
/// Implementation:
/// - After: WHERE id &gt; cursor ORDER BY id ASC LIMIT n
/// - Before: WHERE id &lt; cursor ORDER BY id DESC LIMIT n (then reverse)
/// </remarks>
///
/// <example>
/// <code>
/// // No cursor - get first page
/// let firstPage = { After = None; Before = None }
///
/// // Forward pagination - get next page after cursor
/// let nextPage = { After = Some "eyJpZCI6ImFiYzEyMyJ9"; Before = None }
///
/// // Backward pagination - get previous page before cursor
/// let prevPage = { After = None; Before = Some "eyJpZCI6ImFiYzEyMyJ9" }
///
/// // Usage in QueryOptions
/// let options = {
///     Sort = [("_id", SortDirection.Ascending)]
///     Limit = Some 20
///     Skip = None
///     Cursor = Some { After = Some "cursor_token"; Before = None }
///     Search = None
/// }
///
/// // Typical pagination flow:
/// // 1. Request page with Limit = 20, Cursor = { After = None; Before = None }
/// // 2. Response includes nextCursor = "token123"
/// // 3. Request next page with Cursor = { After = Some "token123"; Before = None }
/// // 4. Continue until nextCursor is None (end of results)
/// </code>
/// </example>
type CursorSpec = {
    /// <summary>
    /// Cursor token for forward pagination (results after this cursor).
    /// </summary>
    /// <remarks>
    /// When set, returns documents that come after the specified cursor
    /// in the sort order. Typically used for "next page" navigation.
    ///
    /// The cursor is an opaque string (usually base64-encoded) that
    /// encodes the position in the result set. It should be treated as
    /// an opaque token and not parsed or constructed manually.
    ///
    /// Behavior:
    /// - None: No lower bound (start from beginning)
    /// - Some cursor: Start results after this position
    /// - Mutually exclusive with Before (one or none, not both)
    ///
    /// SQL translation: WHERE id &gt; decode(cursor)
    /// </remarks>
    After: option<string>
    
    /// <summary>
    /// Cursor token for backward pagination (results before this cursor).
    /// </summary>
    /// <remarks>
    /// When set, returns documents that come before the specified cursor
    /// in the sort order. Typically used for "previous page" navigation.
    ///
    /// The cursor is an opaque string (usually base64-encoded) that
    /// encodes the position in the result set. Results are fetched in
    /// reverse order and then flipped to maintain correct ordering.
    ///
    /// Behavior:
    /// - None: No upper bound (end at last result)
    /// - Some cursor: End results before this position
    /// - Mutually exclusive with After (one or none, not both)
    ///
    /// SQL translation: WHERE id &lt; decode(cursor) ORDER BY id DESC
    /// (then reverse the results)
    /// </remarks>
    Before: option<string>
}

/// <summary>
/// Full-text search specification for searching across multiple fields.
/// </summary>
///
/// <remarks>
/// Simple full-text search using SQLite LIKE operator. Substring matches across specified fields.
/// Multiple fields use OR logic. Wildcards (%, _) are escaped and treated literally.
/// </remarks>
///
/// <example>
/// <code>
/// { Text = "database"; Fields = ["title"; "description"]; CaseSensitive = false }
/// </code>
/// </example>
type TextSearchSpec = {
    /// <summary>Search query text (substring match, wildcards escaped).</summary>
    Text: string
    
    /// <summary>List of field names to search within (OR logic).</summary>
    Fields: list<string>
    
    /// <summary>Whether the search should be case-sensitive.</summary>
    CaseSensitive: bool
}

/// <summary>
/// Query options for controlling result set size, ordering, and field projection.
/// </summary>
///
/// <remarks>
/// Combines pagination, sorting, field selection, and search. Use QueryOptions.empty as starting point.
/// Mutually exclusive: Select/Omit, Skip/Cursor. Execution: filter → sort → skip/cursor → limit → projection.
/// </remarks>
///
/// <example>
/// <code>
/// { Sort = [("createdAt", SortDirection.Descending)]; Limit = Some 20; Skip = None
///   Select = None; Omit = None; Search = None; Cursor = None }
/// </code>
/// </example>
type QueryOptions<'T> = {
    /// <summary>Sort order: list of (field, direction) tuples.</summary>
    Sort: list<(string * SortDirection)>
    
    /// <summary>Maximum number of results (None = all results).</summary>
    Limit: option<int>
    
    /// <summary>Number of results to skip (offset pagination, O(n)).</summary>
    Skip: option<int>
    
    /// <summary>Include only these fields (whitelist projection).</summary>
    Select: option<list<string>>
    
    /// <summary>Exclude these fields (blacklist projection).</summary>
    Omit: option<list<string>>
    
    /// <summary>Full-text search specification.</summary>
    Search: option<TextSearchSpec>
    
    /// <summary>Cursor-based pagination (O(1), prefer over Skip).</summary>
    Cursor: option<CursorSpec>
}

/// <summary>
/// Helper functions for constructing and modifying QueryOptions.
/// </summary>
///
/// <remarks>
/// The QueryOptions module provides a fluent API for building query options
/// using function composition. Start with 'empty' and chain modifications:
///
/// empty |&gt; limit 10 |&gt; skip 20 |&gt; sortAsc "name"
///
/// All functions are immutable - they return new QueryOptions instances
/// rather than modifying the input.
/// </remarks>
module QueryOptions =
    
    /// <summary>
    /// Creates an empty QueryOptions with all fields set to default values.
    /// </summary>
    ///
    /// <returns>
    /// A QueryOptions&lt;'T&gt; with Sort = [], and all option fields = None.
    /// </returns>
    ///
    /// <remarks>
    /// Use 'empty' as the starting point for building query options with the
    /// fluent API. All other functions modify this base configuration.
    ///
    /// Default values:
    /// - Sort: [] (no ordering, database order)
    /// - Limit: None (return all results)
    /// - Skip: None (start from beginning)
    /// - Select: None (include all fields)
    /// - Omit: None (exclude no fields)
    /// - Search: None (no text search)
    /// - Cursor: None (no cursor pagination)
    /// </remarks>
    ///
    /// <example>
    /// <code>
    /// // Empty options - return all results
    /// let opts1 = QueryOptions.empty&lt;User&gt;
    ///
    /// // Build options with fluent API
    /// let opts2 =
    ///     QueryOptions.empty&lt;User&gt;
    ///     |&gt; QueryOptions.limit 10
    ///     |&gt; QueryOptions.sortDesc "createdAt"
    ///
    /// // Equivalent to:
    /// let opts3 = {
    ///     Sort = [("createdAt", SortDirection.Descending)]
    ///     Limit = Some 10
    ///     Skip = None
    ///     Select = None
    ///     Omit = None
    ///     Search = None
    ///     Cursor = None
    /// }
    /// </code>
    /// </example>
    let empty<'T> : QueryOptions<'T> = {
        Sort = []
        Limit = None
        Skip = None
        Select = None
        Omit = None
        Search = None
        Cursor = None
    }
    
    /// <summary>
    /// Sets the maximum number of results to return.
    /// </summary>
    ///
    /// <param name="n">Maximum number of results (must be positive).</param>
    /// <param name="opts">The QueryOptions to modify.</param>
    ///
    /// <returns>
    /// A new QueryOptions with Limit set to Some n.
    /// </returns>
    ///
    /// <remarks>
    /// The limit is applied after Skip/Cursor and before projection.
    /// Always returns at most n results, even if more match the query.
    ///
    /// Best practices:
    /// - Always set a limit to prevent unbounded result sets
    /// - Typical values: 10-100 for paginated results
    /// - Use limit 1 for operations that need at most one result
    /// </remarks>
    ///
    /// <example>
    /// <code>
    /// // Limit to 10 results
    /// let opts = QueryOptions.empty |&gt; QueryOptions.limit 10
    ///
    /// // Combine with other options
    /// let pagedOpts =
    ///     QueryOptions.empty
    ///     |&gt; QueryOptions.limit 25
    ///     |&gt; QueryOptions.skip 50
    ///     |&gt; QueryOptions.sortAsc "name"
    /// // Returns results 51-75, sorted by name
    /// </code>
    /// </example>
    let limit (n: int) (opts: QueryOptions<'T>) : QueryOptions<'T> =
        { opts with Limit = Some n }
    
    /// <summary>
    /// Sets the number of results to skip (offset pagination).
    /// </summary>
    ///
    /// <param name="n">Number of results to skip (must be non-negative).</param>
    /// <param name="opts">The QueryOptions to modify.</param>
    ///
    /// <returns>
    /// A new QueryOptions with Skip set to Some n.
    /// </returns>
    ///
    /// <remarks>
    /// Skip performs offset-based pagination by skipping the first n results.
    /// Applied after filtering/sorting, before Limit.
    ///
    /// Performance warning:
    /// - Skip does a full scan of the first n results (O(n))
    /// - For large offsets (n &gt; 1000), use cursor pagination instead
    /// - Page drift: results may shift if data changes between requests
    ///
    /// Cannot be used with Cursor (mutually exclusive).
    ///
    /// Page calculation: skip = pageNumber * pageSize
    /// </remarks>
    ///
    /// <example>
    /// <code>
    /// // Skip first 20 results
    /// let opts = QueryOptions.empty |&gt; QueryOptions.skip 20
    ///
    /// // Pagination: page 3, 10 items per page (skip 20, limit 10)
    /// let page3 =
    ///     QueryOptions.empty
    ///     |&gt; QueryOptions.skip 20  // Skip pages 1 and 2
    ///     |&gt; QueryOptions.limit 10 // Get page 3
    ///
    /// // Large offset - consider cursor pagination instead
    /// let page100 =
    ///     QueryOptions.empty
    ///     |&gt; QueryOptions.skip 990  // Slow! Scans 990 results
    ///     |&gt; QueryOptions.limit 10
    /// </code>
    /// </example>
    let skip (n: int) (opts: QueryOptions<'T>) : QueryOptions<'T> =
        { opts with Skip = Some n }
    
    /// <summary>
    /// Adds a sort field with specified direction to the sort order.
    /// </summary>
    ///
    /// <param name="field">Field name to sort by.</param>
    /// <param name="dir">Sort direction (Ascending or Descending).</param>
    /// <param name="opts">The QueryOptions to modify.</param>
    ///
    /// <returns>
    /// A new QueryOptions with the field added to the beginning of the Sort list.
    /// </returns>
    ///
    /// <remarks>
    /// Adds the field to the front of the sort order. Multiple calls build up
    /// a multi-field sort in reverse order (last call = primary sort).
    ///
    /// Sort order precedence:
    /// - First field: primary sort
    /// - Second field: secondary sort (for ties in first field)
    /// - And so on...
    ///
    /// Field names:
    /// - Simple: "name", "age"
    /// - Nested: "user.email" (JSON path)
    /// - Metadata: "_id", "_createdAt", "_updatedAt"
    ///
    /// For more natural order, use sortAsc/sortDesc shortcuts or reverse the chain.
    /// </remarks>
    ///
    /// <example>
    /// <code>
    /// // Sort by name ascending
    /// let opts1 =
    ///     QueryOptions.empty
    ///     |&gt; QueryOptions.sortBy "name" SortDirection.Ascending
    ///
    /// // Multi-field sort: age desc, then name asc
    /// // Note: reverse order due to prepending
    /// let opts2 =
    ///     QueryOptions.empty
    ///     |&gt; QueryOptions.sortBy "name" SortDirection.Ascending
    ///     |&gt; QueryOptions.sortBy "age" SortDirection.Descending
    /// // Result: Sort = [("age", Desc); ("name", Asc)]
    ///
    /// // Prefer sortAsc/sortDesc for clarity
    /// let opts3 =
    ///     QueryOptions.empty
    ///     |&gt; QueryOptions.sortAsc "name"
    ///     |&gt; QueryOptions.sortDesc "age"
    /// </code>
    /// </example>
    let sortBy (field: string) (dir: SortDirection) (opts: QueryOptions<'T>) : QueryOptions<'T> =
        { opts with Sort = (field, dir) :: opts.Sort }
    
    /// <summary>
    /// Adds a field to sort in ascending order.
    /// </summary>
    ///
    /// <param name="field">Field name to sort by.</param>
    /// <param name="opts">The QueryOptions to modify.</param>
    ///
    /// <returns>
    /// A new QueryOptions with the field added to Sort in Ascending direction.
    /// </returns>
    ///
    /// <remarks>
    /// Shortcut for sortBy field SortDirection.Ascending.
    ///
    /// Ascending order: A→Z, 0→9, oldest→newest, false→true
    ///
    /// Adds to the front of the sort list (last call = primary sort).
    /// </remarks>
    ///
    /// <example>
    /// <code>
    /// // Sort by name A-Z
    /// let opts = QueryOptions.empty |&gt; QueryOptions.sortAsc "name"
    ///
    /// // Sort by email asc, then age asc
    /// let multiSort =
    ///     QueryOptions.empty
    ///     |&gt; QueryOptions.sortAsc "age"
    ///     |&gt; QueryOptions.sortAsc "email"
    /// // Result: Sort = [("email", Asc); ("age", Asc)]
    /// </code>
    /// </example>
    let sortAsc (field: string) (opts: QueryOptions<'T>) : QueryOptions<'T> =
        sortBy field SortDirection.Ascending opts
    
    /// <summary>
    /// Adds a field to sort in descending order.
    /// </summary>
    ///
    /// <param name="field">Field name to sort by.</param>
    /// <param name="opts">The QueryOptions to modify.</param>
    ///
    /// <returns>
    /// A new QueryOptions with the field added to Sort in Descending direction.
    /// </returns>
    ///
    /// <remarks>
    /// Shortcut for sortBy field SortDirection.Descending.
    ///
    /// Descending order: Z→A, 9→0, newest→oldest, true→false
    ///
    /// Adds to the front of the sort list (last call = primary sort).
    /// </remarks>
    ///
    /// <example>
    /// <code>
    /// // Sort by createdAt newest first
    /// let opts = QueryOptions.empty |&gt; QueryOptions.sortDesc "createdAt"
    ///
    /// // Sort by score desc, then name asc
    /// let leaderboard =
    ///     QueryOptions.empty
    ///     |&gt; QueryOptions.sortAsc "name"
    ///     |&gt; QueryOptions.sortDesc "score"
    /// // Result: Sort = [("score", Desc); ("name", Asc)]
    /// // Highest scores first, ties broken by name
    /// </code>
    /// </example>
    let sortDesc (field: string) (opts: QueryOptions<'T>) : QueryOptions<'T> =
        sortBy field SortDirection.Descending opts
    
    /// <summary>
    /// Sets the fields to include in results (whitelist projection).
    /// </summary>
    ///
    /// <param name="fields">List of field names to include.</param>
    /// <param name="opts">The QueryOptions to modify.</param>
    ///
    /// <returns>
    /// A new QueryOptions with Select set to Some fields.
    /// </returns>
    ///
    /// <remarks>
    /// Whitelist projection: only specified fields are returned.
    /// All other fields are excluded from the result documents.
    ///
    /// Mutually exclusive with omit - do not use both.
    ///
    /// Metadata fields (_id, _createdAt, _updatedAt) are always included
    /// unless explicitly omitted.
    ///
    /// Performance: Reduces memory usage and serialization overhead by
    /// excluding unnecessary fields.
    /// </remarks>
    ///
    /// <example>
    /// <code>
    /// // Include only email and name
    /// let opts =
    ///     QueryOptions.empty
    ///     |&gt; QueryOptions.select ["email"; "name"]
    /// // Result: { _id, email, name, _createdAt, _updatedAt }
    ///
    /// // Get minimal user info
    /// let minimalUser =
    ///     QueryOptions.empty
    ///     |&gt; QueryOptions.select ["name"]
    ///     |&gt; QueryOptions.limit 100
    /// </code>
    /// </example>
    let select (fields: list<string>) (opts: QueryOptions<'T>) : QueryOptions<'T> =
        { opts with Select = Some fields }
    
    /// <summary>
    /// Sets the fields to exclude from results (blacklist projection).
    /// </summary>
    ///
    /// <param name="fields">List of field names to exclude.</param>
    /// <param name="opts">The QueryOptions to modify.</param>
    ///
    /// <returns>
    /// A new QueryOptions with Omit set to Some fields.
    /// </returns>
    ///
    /// <remarks>
    /// Blacklist projection: specified fields are excluded from results.
    /// All other fields are included.
    ///
    /// Mutually exclusive with select - do not use both.
    ///
    /// Useful for excluding sensitive fields (passwords, tokens) without
    /// explicitly listing all other fields.
    ///
    /// Metadata fields can also be omitted if needed.
    /// </remarks>
    ///
    /// <example>
    /// <code>
    /// // Exclude sensitive fields
    /// let opts =
    ///     QueryOptions.empty
    ///     |&gt; QueryOptions.omit ["password"; "ssn"; "creditCard"]
    /// // Result: All fields except password, ssn, creditCard
    ///
    /// // Public user profile (hide internal fields)
    /// let publicProfile =
    ///     QueryOptions.empty
    ///     |&gt; QueryOptions.omit ["password"; "apiKey"; "internalId"]
    /// </code>
    /// </example>
    let omit (fields: list<string>) (opts: QueryOptions<'T>) : QueryOptions<'T> =
        { opts with Omit = Some fields }
    
    /// <summary>
    /// Sets case-insensitive text search across specified fields.
    /// </summary>
    ///
    /// <param name="text">The search query text to find.</param>
    /// <param name="fields">List of field names to search within.</param>
    /// <param name="opts">The QueryOptions to modify.</param>
    ///
    /// <returns>QueryOptions with Search set (CaseSensitive = false).</returns>
    ///
    /// <remarks>
    /// Substring match, case-insensitive. Multiple fields use OR logic.
    /// Combined with query using AND logic. May be slow without FTS indexes.
    /// </remarks>
    ///
    /// <example>
    /// <code>
    /// QueryOptions.empty |&gt; QueryOptions.search "functional" ["title"; "description"]
    /// </code>
    /// </example>
    let search (text: string) (fields: list<string>) (opts: QueryOptions<'T>) : QueryOptions<'T> =
        { opts with Search = Some { Text = text; Fields = fields; CaseSensitive = false } }
    
    /// <summary>
    /// Sets case-sensitive text search across specified fields.
    /// </summary>
    ///
    /// <param name="text">The search query text (exact case).</param>
    /// <param name="fields">List of field names to search within.</param>
    /// <param name="opts">The QueryOptions to modify.</param>
    ///
    /// <returns>QueryOptions with Search set (CaseSensitive = true).</returns>
    ///
    /// <remarks>
    /// Substring match with exact case matching. Use for code search or when case distinction matters.
    /// </remarks>
    ///
    /// <example>
    /// <code>
    /// QueryOptions.empty |&gt; QueryOptions.searchCaseSensitive "FractalDb" ["code"]
    /// </code>
    /// </example>
    let searchCaseSensitive (text: string) (fields: list<string>) (opts: QueryOptions<'T>) : QueryOptions<'T> =
        { opts with Search = Some { Text = text; Fields = fields; CaseSensitive = true } }
    
    /// <summary>
    /// Sets cursor for forward pagination (results after the cursor).
    /// </summary>
    ///
    /// <param name="id">The cursor token (opaque string).</param>
    /// <param name="opts">The QueryOptions to modify.</param>
    ///
    /// <returns>QueryOptions with Cursor.After set.</returns>
    ///
    /// <remarks>
    /// Use for "next page" navigation. Cursor tokens are opaque (don't parse manually).
    /// Requires stable sort order. Cannot be used with Skip. O(1) performance vs Skip's O(n).
    /// </remarks>
    ///
    /// <example>
    /// <code>
    /// QueryOptions.empty |&gt; QueryOptions.cursorAfter "token" |&gt; QueryOptions.limit 20
    /// </code>
    /// </example>
    let cursorAfter (id: string) (opts: QueryOptions<'T>) : QueryOptions<'T> =
        { opts with Cursor = Some { After = Some id; Before = None } }
    
    /// <summary>
    /// Sets cursor for backward pagination (results before the cursor).
    /// </summary>
    ///
    /// <param name="id">The cursor token (opaque string).</param>
    /// <param name="opts">The QueryOptions to modify.</param>
    ///
    /// <returns>QueryOptions with Cursor.Before set.</returns>
    ///
    /// <remarks>
    /// Use for "previous page" navigation. Results are fetched in reverse then flipped.
    /// Requires stable sort order. Cannot be used with Skip.
    /// </remarks>
    ///
    /// <example>
    /// <code>
    /// QueryOptions.empty |&gt; QueryOptions.cursorBefore "token" |&gt; QueryOptions.limit 20
    /// </code>
    /// </example>
    let cursorBefore (id: string) (opts: QueryOptions<'T>) : QueryOptions<'T> =
        { opts with Cursor = Some { After = None; Before = Some id } }



