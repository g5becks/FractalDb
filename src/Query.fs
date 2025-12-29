module FractalDb.Query

open FractalDb.Operators

/// <summary>
/// Helper functions for constructing Query&lt;'T&gt; values ergonomically.
/// </summary>
///
/// <remarks>
/// The Query module provides a functional API for building queries without verbose syntax.
/// Most functions create Query.Field values with an empty field name placeholder (""),
/// which is intended to be used with field binding operations or computation expressions
/// that will fill in the actual field name.
///
/// These helpers are particularly useful when combined with:
/// - QueryBuilder computation expression (provides automatic field binding)
/// - Pipeline-style query construction
/// - Type-safe query building
///
/// Note: The empty string ("") as field name is a placeholder. In actual usage,
/// you'll typically use Query.Field(fieldName, op) directly or use computation expressions.
/// </remarks>
[<RequireQualifiedAccess>]
module Query =
    /// <summary>
    /// Creates an empty query that matches all documents.
    /// </summary>
    ///
    /// <typeparam name="'T">The document type being queried.</typeparam>
    ///
    /// <returns>A Query&lt;'T&gt; that matches all documents.</returns>
    ///
    /// <remarks>
    /// The empty query is useful as a starting point for query composition or when
    /// you want to retrieve all documents without filtering. In SQL, this translates
    /// to omitting the WHERE clause or using "WHERE 1=1".
    /// </remarks>
    ///
    /// <example>
    /// <code>
    /// // Retrieve all documents from a collection
    /// let allUsers = Query.empty&lt;User&gt;
    ///
    /// // Use as a base for conditional query building
    /// let query =
    ///     if hasFilter then
    ///         Query.Field ("status", FieldOp.Compare (box (CompareOp.Eq "active")))
    ///     else
    ///         Query.empty
    /// </code>
    /// </example>
    let empty<'T> : Query<'T> = Query.Empty

    // ===== Comparison Operators =====

    /// <summary>
    /// Creates an equality comparison query.
    /// </summary>
    ///
    /// <param name="value">The value to compare against.</param>
    ///
    /// <typeparam name="'T">The type of the value being compared.</typeparam>
    ///
    /// <returns>A Query that matches documents where the field equals the specified value.</returns>
    ///
    /// <remarks>
    /// This function creates a field query with an empty field name placeholder.
    /// Use it with field binding operations or computation expressions that provide the field name.
    /// The inline keyword ensures type inference works correctly across generic contexts.
    /// </remarks>
    ///
    /// <example>
    /// <code>
    /// // Direct usage (requires field name)
    /// Query.Field ("age", FieldOp.Compare (box (CompareOp.Eq 30)))
    ///
    /// // With computation expression (field name provided by CE)
    /// // query {
    /// //     where (_.age, Query.eq 30)
    /// // }
    /// </code>
    /// </example>
    let inline eq value =
        Query.Field("", FieldOp.Compare(box (CompareOp.Eq value)))

    /// <summary>
    /// Creates a not-equal comparison query.
    /// </summary>
    ///
    /// <param name="value">The value to compare against.</param>
    ///
    /// <typeparam name="'T">The type of the value being compared.</typeparam>
    ///
    /// <returns>A Query that matches documents where the field does not equal the specified value.</returns>
    ///
    /// <remarks>
    /// Creates a field query with an empty field name placeholder for use with field binding.
    /// </remarks>
    ///
    /// <example>
    /// <code>
    /// // Match documents where status is not "deleted"
    /// Query.Field ("status", FieldOp.Compare (box (CompareOp.Ne "deleted")))
    /// </code>
    /// </example>
    let inline ne value =
        Query.Field("", FieldOp.Compare(box (CompareOp.Ne value)))

    /// <summary>
    /// Creates a greater-than comparison query.
    /// </summary>
    ///
    /// <param name="value">The value to compare against.</param>
    ///
    /// <typeparam name="'T">The type of the value being compared (must be orderable).</typeparam>
    ///
    /// <returns>A Query that matches documents where the field is greater than the specified value.</returns>
    ///
    /// <remarks>
    /// Only valid for orderable types (numbers, dates, comparable strings).
    /// Creates a field query with an empty field name placeholder.
    /// </remarks>
    ///
    /// <example>
    /// <code>
    /// // Match documents where age is greater than 18
    /// Query.Field ("age", FieldOp.Compare (box (CompareOp.Gt 18)))
    ///
    /// // Match documents with high scores
    /// Query.Field ("score", FieldOp.Compare (box (CompareOp.Gt 90.0)))
    /// </code>
    /// </example>
    let inline gt value =
        Query.Field("", FieldOp.Compare(box (CompareOp.Gt value)))

    /// <summary>
    /// Creates a greater-than-or-equal comparison query.
    /// </summary>
    ///
    /// <param name="value">The value to compare against.</param>
    ///
    /// <typeparam name="'T">The type of the value being compared (must be orderable).</typeparam>
    ///
    /// <returns>
    /// A Query that matches documents where the field is greater than or equal to the specified value.
    /// </returns>
    ///
    /// <remarks>
    /// Only valid for orderable types. Creates a field query with an empty field name placeholder.
    /// </remarks>
    ///
    /// <example>
    /// <code>
    /// // Match documents where age is 18 or older
    /// Query.Field ("age", FieldOp.Compare (box (CompareOp.Gte 18)))
    /// </code>
    /// </example>
    let inline gte value =
        Query.Field("", FieldOp.Compare(box (CompareOp.Gte value)))

    /// <summary>
    /// Creates a less-than comparison query.
    /// </summary>
    ///
    /// <param name="value">The value to compare against.</param>
    ///
    /// <typeparam name="'T">The type of the value being compared (must be orderable).</typeparam>
    ///
    /// <returns>A Query that matches documents where the field is less than the specified value.</returns>
    ///
    /// <remarks>
    /// Only valid for orderable types. Creates a field query with an empty field name placeholder.
    /// </remarks>
    ///
    /// <example>
    /// <code>
    /// // Match documents where age is less than 18
    /// Query.Field ("age", FieldOp.Compare (box (CompareOp.Lt 18)))
    /// </code>
    /// </example>
    let inline lt value =
        Query.Field("", FieldOp.Compare(box (CompareOp.Lt value)))

    /// <summary>
    /// Creates a less-than-or-equal comparison query.
    /// </summary>
    ///
    /// <param name="value">The value to compare against.</param>
    ///
    /// <typeparam name="'T">The type of the value being compared (must be orderable).</typeparam>
    ///
    /// <returns>A Query that matches documents where the field is less than or equal to the specified value.</returns>
    ///
    /// <remarks>
    /// Only valid for orderable types. Creates a field query with an empty field name placeholder.
    /// </remarks>
    ///
    /// <example>
    /// <code>
    /// // Match documents where age is 65 or younger
    /// Query.Field ("age", FieldOp.Compare (box (CompareOp.Lte 65)))
    /// </code>
    /// </example>
    let inline lte value =
        Query.Field("", FieldOp.Compare(box (CompareOp.Lte value)))

    /// <summary>
    /// Creates an IN list membership query.
    /// </summary>
    ///
    /// <param name="values">The list of acceptable values.</param>
    ///
    /// <typeparam name="'T">The type of the values in the list.</typeparam>
    ///
    /// <returns>A Query that matches documents where the field value is in the specified list.</returns>
    ///
    /// <remarks>
    /// Equivalent to SQL IN operator. An empty list will match no documents.
    /// Creates a field query with an empty field name placeholder.
    /// </remarks>
    ///
    /// <example>
    /// <code>
    /// // Match documents with specific statuses
    /// Query.Field ("status", FieldOp.Compare (box (CompareOp.In ["active"; "pending"; "approved"])))
    ///
    /// // Match documents with priority 1, 2, or 3
    /// Query.Field ("priority", FieldOp.Compare (box (CompareOp.In [1; 2; 3])))
    /// </code>
    /// </example>
    let inline isIn values =
        Query.Field("", FieldOp.Compare(box (CompareOp.In values)))

    /// <summary>
    /// Creates a NOT IN list exclusion query.
    /// </summary>
    ///
    /// <param name="values">The list of values to exclude.</param>
    ///
    /// <typeparam name="'T">The type of the values in the list.</typeparam>
    ///
    /// <returns>A Query that matches documents where the field value is not in the specified list.</returns>
    ///
    /// <remarks>
    /// Equivalent to SQL NOT IN operator. An empty list will match all documents.
    /// Creates a field query with an empty field name placeholder.
    /// </remarks>
    ///
    /// <example>
    /// <code>
    /// // Exclude documents with specific statuses
    /// Query.Field ("status", FieldOp.Compare (box (CompareOp.NotIn ["deleted"; "archived"])))
    ///
    /// // Exclude blocked categories
    /// Query.Field ("category", FieldOp.Compare (box (CompareOp.NotIn ["spam"; "blocked"])))
    /// </code>
    /// </example>
    let inline notIn values =
        Query.Field("", FieldOp.Compare(box (CompareOp.NotIn values)))

    // ===== String Operators =====

    /// <summary>
    /// Creates a SQL LIKE pattern matching query (case-sensitive).
    /// </summary>
    ///
    /// <param name="pattern">
    /// The pattern to match. Use % for zero or more characters, _ for exactly one character.
    /// </param>
    ///
    /// <returns>A Query that matches documents where the string field matches the pattern.</returns>
    ///
    /// <remarks>
    /// LIKE performs case-sensitive pattern matching in most SQLite configurations.
    /// For case-insensitive matching, use ilike instead.
    /// Creates a field query with an empty field name placeholder.
    /// </remarks>
    ///
    /// <example>
    /// <code>
    /// // Match names starting with "A"
    /// Query.Field ("name", FieldOp.String (StringOp.Like "A%"))
    ///
    /// // Match emails from specific domain
    /// Query.Field ("email", FieldOp.String (StringOp.Like "%@example.com"))
    ///
    /// // Match codes with specific format (e.g., "ABC-123")
    /// Query.Field ("code", FieldOp.String (StringOp.Like "___-___"))
    /// </code>
    /// </example>
    let like pattern =
        Query.Field("", FieldOp.String(StringOp.Like pattern))

    /// <summary>
    /// Creates a SQL LIKE pattern matching query (case-insensitive).
    /// </summary>
    ///
    /// <param name="pattern">
    /// The pattern to match. Use % for zero or more characters, _ for exactly one character.
    /// </param>
    ///
    /// <returns>A Query that matches documents where the string field matches the pattern (case-insensitive).</returns>
    ///
    /// <remarks>
    /// ILIKE performs case-insensitive pattern matching by adding the COLLATE NOCASE clause.
    /// This is useful for user-facing searches where case should be ignored.
    /// Creates a field query with an empty field name placeholder.
    /// </remarks>
    ///
    /// <example>
    /// <code>
    /// // Match "smith", "Smith", "SMITH", etc.
    /// Query.Field ("lastName", FieldOp.String (StringOp.ILike "smith"))
    ///
    /// // Case-insensitive prefix match
    /// Query.Field ("role", FieldOp.String (StringOp.ILike "admin%"))
    /// </code>
    /// </example>
    let ilike pattern =
        Query.Field("", FieldOp.String(StringOp.ILike pattern))

    /// <summary>
    /// Creates a substring matching query.
    /// </summary>
    ///
    /// <param name="substring">The substring to search for.</param>
    ///
    /// <returns>A Query that matches documents where the string field contains the substring.</returns>
    ///
    /// <remarks>
    /// Contains is syntactic sugar for the LIKE pattern '%substring%'.
    /// The matching is case-sensitive by default.
    /// Creates a field query with an empty field name placeholder.
    /// </remarks>
    ///
    /// <example>
    /// <code>
    /// // Match any name containing "john"
    /// Query.Field ("name", FieldOp.String (StringOp.Contains "john"))
    ///
    /// // Match descriptions mentioning "urgent"
    /// Query.Field ("description", FieldOp.String (StringOp.Contains "urgent"))
    /// </code>
    /// </example>
    let contains substring =
        Query.Field("", FieldOp.String(StringOp.Contains substring))

    /// <summary>
    /// Creates a prefix matching query.
    /// </summary>
    ///
    /// <param name="prefix">The prefix to match.</param>
    ///
    /// <returns>A Query that matches documents where the string field starts with the prefix.</returns>
    ///
    /// <remarks>
    /// StartsWith is syntactic sugar for the LIKE pattern 'prefix%'.
    /// The matching is case-sensitive by default.
    /// Creates a field query with an empty field name placeholder.
    /// </remarks>
    ///
    /// <example>
    /// <code>
    /// // Match usernames starting with "admin"
    /// Query.Field ("username", FieldOp.String (StringOp.StartsWith "admin"))
    ///
    /// // Match file paths in a directory
    /// Query.Field ("path", FieldOp.String (StringOp.StartsWith "/home/user/"))
    /// </code>
    /// </example>
    let startsWith prefix =
        Query.Field("", FieldOp.String(StringOp.StartsWith prefix))

    /// <summary>
    /// Creates a suffix matching query.
    /// </summary>
    ///
    /// <param name="suffix">The suffix to match.</param>
    ///
    /// <returns>A Query that matches documents where the string field ends with the suffix.</returns>
    ///
    /// <remarks>
    /// EndsWith is syntactic sugar for the LIKE pattern '%suffix'.
    /// The matching is case-sensitive by default.
    /// Creates a field query with an empty field name placeholder.
    /// </remarks>
    ///
    /// <example>
    /// <code>
    /// // Match email addresses from a specific domain
    /// Query.Field ("email", FieldOp.String (StringOp.EndsWith "@company.com"))
    ///
    /// // Match image files
    /// Query.Field ("filename", FieldOp.String (StringOp.EndsWith ".jpg"))
    ///
    /// // Match URLs with specific paths
    /// Query.Field ("url", FieldOp.String (StringOp.EndsWith "/api/users"))
    /// </code>
    /// </example>
    let endsWith suffix =
        Query.Field("", FieldOp.String(StringOp.EndsWith suffix))

    // ===== Array Operators =====

    /// <summary>
    /// Creates an array containment query that matches arrays containing all specified values.
    /// </summary>
    ///
    /// <param name="values">The list of values that must all be present in the array.</param>
    ///
    /// <typeparam name="'T">The type of elements in the array.</typeparam>
    ///
    /// <returns>A Query that matches documents where the array field contains all specified values.</returns>
    ///
    /// <remarks>
    /// Order does not matter - the array can contain the values in any order and can
    /// contain additional values beyond those specified.
    /// Creates a field query with an empty field name placeholder.
    /// </remarks>
    ///
    /// <example>
    /// <code>
    /// // Match documents where tags array contains both "featured" and "public"
    /// Query.Field ("tags", FieldOp.Array (box (ArrayOp.All ["featured"; "public"])))
    ///
    /// // Match documents where scores array contains 100, 95, and 90
    /// Query.Field ("scores", FieldOp.Array (box (ArrayOp.All [100; 95; 90])))
    /// </code>
    /// </example>
    let all<'T> values =
        Query.Field("", FieldOp.Array(box (ArrayOp.All values)))

    /// <summary>
    /// Creates an array size query that matches arrays with an exact number of elements.
    /// </summary>
    ///
    /// <param name="n">The exact number of elements the array must contain.</param>
    ///
    /// <returns>A Query that matches documents where the array field has exactly n elements.</returns>
    ///
    /// <remarks>
    /// Use this to find arrays with a specific length, including empty arrays (size 0).
    /// Creates a field query with an empty field name placeholder.
    /// </remarks>
    ///
    /// <example>
    /// <code>
    /// // Match documents where tags array has exactly 3 elements
    /// Query.Field ("tags", FieldOp.Array (box (ArrayOp.Size 3)))
    ///
    /// // Match documents with empty arrays
    /// Query.Field ("items", FieldOp.Array (box (ArrayOp.Size 0)))
    ///
    /// // Match documents with single-element arrays
    /// Query.Field ("categories", FieldOp.Array (box (ArrayOp.Size 1)))
    /// </code>
    /// </example>
    let size n =
        Query.Field("", FieldOp.Array(box (ArrayOp.Size n)))

    // ===== Existence Operators =====

    /// <summary>
    /// Creates a field existence query that matches documents where the field is present.
    /// </summary>
    ///
    /// <returns>A Query that matches documents where the field exists (is present in the JSON).</returns>
    ///
    /// <remarks>
    /// This checks field presence, not value. A field can exist with a null value.
    /// Creates a field query with an empty field name placeholder.
    /// Useful for finding documents with optional fields populated.
    /// </remarks>
    ///
    /// <example>
    /// <code>
    /// // Match documents that have an "email" field (even if null)
    /// Query.Field ("email", FieldOp.Exist (ExistsOp.Exists true))
    ///
    /// // Find users with phone numbers
    /// Query.Field ("phoneNumber", FieldOp.Exist (ExistsOp.Exists true))
    /// </code>
    /// </example>
    let exists =
        Query.Field("", FieldOp.Exist(ExistsOp.Exists true))

    /// <summary>
    /// Creates a field non-existence query that matches documents where the field is absent.
    /// </summary>
    ///
    /// <returns>A Query that matches documents where the field does not exist (is absent from the JSON).</returns>
    ///
    /// <remarks>
    /// This checks for field absence. A field with a null value is still considered to exist.
    /// Creates a field query with an empty field name placeholder.
    /// Useful for finding documents missing certain fields.
    /// </remarks>
    ///
    /// <example>
    /// <code>
    /// // Match documents that don't have a "deletedAt" field
    /// Query.Field ("deletedAt", FieldOp.Exist (ExistsOp.Exists false))
    ///
    /// // Find documents without optional metadata
    /// Query.Field ("metadata", FieldOp.Exist (ExistsOp.Exists false))
    /// </code>
    /// </example>
    let notExists =
        Query.Field("", FieldOp.Exist(ExistsOp.Exists false))

    // ===== Field Binding =====

    /// <summary>
    /// Attaches a field name to a query operator, binding the placeholder to an actual field path.
    /// </summary>
    ///
    /// <param name="name">The field name or path (supports dot notation for nested fields).</param>
    /// <param name="op">The query containing a field operation with empty field name placeholder.</param>
    ///
    /// <typeparam name="'T">The document type being queried.</typeparam>
    ///
    /// <returns>A Query with the field name bound to the operation.</returns>
    ///
    /// <remarks>
    /// This function extracts the FieldOp from a Query.Field and rebinds it with the actual field name.
    /// If the query is not a Field query, it returns the query unchanged.
    /// This enables a pipeline-style API where operators are composed first, then bound to field names.
    /// </remarks>
    ///
    /// <example>
    /// <code>
    /// // Bind field name to a comparison operator
    /// Query.eq 30 |> Query.field "age"
    /// // Equivalent to: Query.Field ("age", FieldOp.Compare (box (CompareOp.Eq 30)))
    ///
    /// // Bind field name to a string operator
    /// Query.like "admin%" |> Query.field "username"
    ///
    /// // Bind field name to an array operator
    /// Query.all ["featured"; "public"] |> Query.field "tags"
    ///
    /// // Nested field access with dot notation
    /// Query.eq "active" |> Query.field "user.status"
    /// </code>
    /// </example>
    let field (name: string) (op: Query<'T>) : Query<'T> =
        match op with
        | Query.Field(_, innerOp) -> Query.Field(name, innerOp)
        | other -> other

    // ===== Logical Combinators =====

    /// <summary>
    /// Creates a logical AND query where all sub-queries must match.
    /// </summary>
    ///
    /// <param name="queries">List of queries that must all match.</param>
    ///
    /// <typeparam name="'T">The document type being queried.</typeparam>
    ///
    /// <returns>A Query that matches documents satisfying all sub-queries.</returns>
    ///
    /// <remarks>
    /// An empty list is treated as Empty (matches all documents).
    /// A single-element list can be simplified to the contained query.
    /// Named with a prime (all') to avoid conflict with the array helper function.
    /// </remarks>
    ///
    /// <example>
    /// <code>
    /// // Multiple conditions must all be true
    /// Query.all' [
    ///     Query.Field ("age", FieldOp.Compare (box (CompareOp.Gte 18)))
    ///     Query.Field ("age", FieldOp.Compare (box (CompareOp.Lt 65)))
    ///     Query.Field ("status", FieldOp.Compare (box (CompareOp.Eq "active")))
    /// ]
    ///
    /// // Using pipeline style
    /// [
    ///     Query.eq 18 |> Query.field "age"
    ///     Query.eq "active" |> Query.field "status"
    /// ] |> Query.all'
    /// </code>
    /// </example>
    let all' (queries: list<Query<'T>>) : Query<'T> =
        Query.And queries

    /// <summary>
    /// Creates a logical OR query where at least one sub-query must match.
    /// </summary>
    ///
    /// <param name="queries">List of queries where at least one must match.</param>
    ///
    /// <typeparam name="'T">The document type being queried.</typeparam>
    ///
    /// <returns>A Query that matches documents satisfying at least one sub-query.</returns>
    ///
    /// <remarks>
    /// An empty list matches no documents.
    /// A single-element list can be simplified to the contained query.
    /// </remarks>
    ///
    /// <example>
    /// <code>
    /// // Any of these conditions can be true
    /// Query.any [
    ///     Query.Field ("role", FieldOp.Compare (box (CompareOp.Eq "admin")))
    ///     Query.Field ("role", FieldOp.Compare (box (CompareOp.Eq "moderator")))
    ///     Query.Field ("permissions", FieldOp.Array (box (ArrayOp.All ["manage_users"])))
    /// ]
    ///
    /// // Using pipeline style
    /// [
    ///     Query.eq "admin" |> Query.field "role"
    ///     Query.eq "moderator" |> Query.field "role"
    /// ] |> Query.any
    /// </code>
    /// </example>
    let any (queries: list<Query<'T>>) : Query<'T> =
        Query.Or queries

    /// <summary>
    /// Creates a logical NOR query where none of the sub-queries must match.
    /// </summary>
    ///
    /// <param name="queries">List of queries where none must match.</param>
    ///
    /// <typeparam name="'T">The document type being queried.</typeparam>
    ///
    /// <returns>A Query that matches documents satisfying none of the sub-queries.</returns>
    ///
    /// <remarks>
    /// NOR is equivalent to NOT (OR ...). Useful for exclusion patterns.
    /// An empty list matches all documents (nothing to exclude).
    /// </remarks>
    ///
    /// <example>
    /// <code>
    /// // Exclude documents matching any of these conditions
    /// Query.none [
    ///     Query.Field ("status", FieldOp.Compare (box (CompareOp.Eq "deleted")))
    ///     Query.Field ("status", FieldOp.Compare (box (CompareOp.Eq "archived")))
    ///     Query.Field ("banned", FieldOp.Compare (box (CompareOp.Eq true)))
    /// ]
    ///
    /// // Using pipeline style
    /// [
    ///     Query.eq "deleted" |> Query.field "status"
    ///     Query.eq "archived" |> Query.field "status"
    /// ] |> Query.none
    /// </code>
    /// </example>
    let none (queries: list<Query<'T>>) : Query<'T> =
        Query.Nor queries

    /// <summary>
    /// Creates a logical NOT query that negates the sub-query.
    /// </summary>
    ///
    /// <param name="query">The query to negate.</param>
    ///
    /// <typeparam name="'T">The document type being queried.</typeparam>
    ///
    /// <returns>A Query that matches documents NOT satisfying the sub-query.</returns>
    ///
    /// <remarks>
    /// Matches documents that do NOT match the sub-query.
    /// Double negation (not' (not' q)) can be simplified to q.
    /// Named with a prime (not') to avoid conflict with F#'s built-in 'not' keyword.
    /// </remarks>
    ///
    /// <example>
    /// <code>
    /// // Documents that are NOT deleted
    /// Query.not' (Query.Field ("deleted", FieldOp.Exist (ExistsOp.Exists true)))
    ///
    /// // Documents where age is NOT greater than 65
    /// Query.not' (Query.Field ("age", FieldOp.Compare (box (CompareOp.Gt 65))))
    ///
    /// // Using pipeline style
    /// Query.exists |> Query.field "deleted" |> Query.not'
    /// </code>
    /// </example>
    let not' (query: Query<'T>) : Query<'T> =
        Query.Not query



