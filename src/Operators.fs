module FractalDb.Operators

/// <summary>
/// Comparison operators for querying document fields by value.
/// </summary>
///
/// <typeparam name="'T">The type of value being compared.</typeparam>
///
/// <remarks>
/// CompareOp provides type-safe comparison operations for queries.
/// Each operator is generic over the value type, ensuring type safety at compile time.
/// These operators correspond to common database comparison operations:
///
/// - Eq/Ne: Equality/inequality (works with all types)
/// - Gt/Gte/Lt/Lte: Ordered comparisons (for numbers, dates, strings)
/// - In/NotIn: Set membership (for lists of values)
///
/// The type parameter allows the compiler to verify that comparisons are type-correct.
/// For example, you cannot compare a string field using Gt without a type error.
/// </remarks>
///
/// <example>
/// <code>
/// // Equality comparison
/// let equalToFive = CompareOp.Eq 5
///
/// // Greater than comparison
/// let olderThan18 = CompareOp.Gt 18
///
/// // List membership
/// let inStatusList = CompareOp.In ["active"; "pending"]
///
/// // Type safety prevents invalid comparisons
/// let valid: CompareOp&lt;int&gt; = CompareOp.Gt 10
/// // let invalid: CompareOp&lt;string&gt; = CompareOp.Gt 10  // Compiler error!
/// </code>
/// </example>
[<RequireQualifiedAccess>]
type CompareOp<'T> =
    /// <summary>
    /// Equal to (==). Matches values that are equal to the specified value.
    /// </summary>
    /// <param name="value">The value to compare against.</param>
    /// <example>
    /// <code>
    /// // Match documents where age equals 30
    /// CompareOp.Eq 30
    ///
    /// // Match documents where status equals "active"
    /// CompareOp.Eq "active"
    /// </code>
    /// </example>
    | Eq of 'T
    
    /// <summary>
    /// Not equal to (!=). Matches values that are not equal to the specified value.
    /// </summary>
    /// <param name="value">The value to compare against.</param>
    /// <example>
    /// <code>
    /// // Match documents where status is not "deleted"
    /// CompareOp.Ne "deleted"
    ///
    /// // Match documents where count is not zero
    /// CompareOp.Ne 0
    /// </code>
    /// </example>
    | Ne of 'T
    
    /// <summary>
    /// Greater than (&gt;). Matches values that are greater than the specified value.
    /// </summary>
    /// <param name="value">The value to compare against.</param>
    /// <remarks>
    /// Only valid for orderable types (numbers, dates, comparable strings).
    /// The comparison uses the type's natural ordering.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Match documents where age is greater than 18
    /// CompareOp.Gt 18
    ///
    /// // Match documents created after a specific timestamp
    /// CompareOp.Gt 1704067200000L
    /// </code>
    /// </example>
    | Gt of 'T
    
    /// <summary>
    /// Greater than or equal to (&gt;=). Matches values greater than or equal to the specified value.
    /// </summary>
    /// <param name="value">The value to compare against.</param>
    /// <remarks>
    /// Only valid for orderable types (numbers, dates, comparable strings).
    /// </remarks>
    /// <example>
    /// <code>
    /// // Match documents where age is 18 or older
    /// CompareOp.Gte 18
    ///
    /// // Match documents with non-negative balance
    /// CompareOp.Gte 0.0
    /// </code>
    /// </example>
    | Gte of 'T
    
    /// <summary>
    /// Less than (&lt;). Matches values that are less than the specified value.
    /// </summary>
    /// <param name="value">The value to compare against.</param>
    /// <remarks>
    /// Only valid for orderable types (numbers, dates, comparable strings).
    /// </remarks>
    /// <example>
    /// <code>
    /// // Match documents where age is less than 18
    /// CompareOp.Lt 18
    ///
    /// // Match documents with balance below threshold
    /// CompareOp.Lt 100.0
    /// </code>
    /// </example>
    | Lt of 'T
    
    /// <summary>
    /// Less than or equal to (&lt;=). Matches values less than or equal to the specified value.
    /// </summary>
    /// <param name="value">The value to compare against.</param>
    /// <remarks>
    /// Only valid for orderable types (numbers, dates, comparable strings).
    /// </remarks>
    /// <example>
    /// <code>
    /// // Match documents where age is 65 or younger
    /// CompareOp.Lte 65
    ///
    /// // Match documents updated before or on a specific date
    /// CompareOp.Lte 1704153600000L
    /// </code>
    /// </example>
    | Lte of 'T
    
    /// <summary>
    /// In list. Matches values that are present in the specified list.
    /// </summary>
    /// <param name="values">The list of acceptable values.</param>
    /// <remarks>
    /// Equivalent to SQL IN operator. An empty list will match no documents.
    /// The order of values in the list does not matter.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Match documents where status is "active", "pending", or "approved"
    /// CompareOp.In ["active"; "pending"; "approved"]
    ///
    /// // Match documents where priority is 1, 2, or 3
    /// CompareOp.In [1; 2; 3]
    ///
    /// // Empty list matches nothing
    /// CompareOp.In []  // No documents match
    /// </code>
    /// </example>
    | In of list<'T>
    
    /// <summary>
    /// Not in list. Matches values that are not present in the specified list.
    /// </summary>
    /// <param name="values">The list of excluded values.</param>
    /// <remarks>
    /// Equivalent to SQL NOT IN operator. An empty list will match all documents.
    /// Useful for excluding specific values from results.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Match documents where status is not "deleted" or "archived"
    /// CompareOp.NotIn ["deleted"; "archived"]
    ///
    /// // Match documents where category is not in blocked list
    /// CompareOp.NotIn ["spam"; "blocked"; "flagged"]
    ///
    /// // Empty list matches everything
    /// CompareOp.NotIn []  // All documents match
    /// </code>
    /// </example>
    | NotIn of list<'T>

/// <summary>
/// String-specific query operators for pattern matching and substring operations.
/// </summary>
///
/// <remarks>
/// StringOp provides operations that only make sense for string fields:
///
/// - Like/ILike: SQL LIKE pattern matching with wildcards (%, _)
/// - Contains/StartsWith/EndsWith: Common substring matching operations
///
/// These operators are only valid for string-typed fields. Using them on
/// non-string fields will result in a runtime query error.
///
/// Pattern matching notes:
/// - LIKE is case-sensitive
/// - ILIKE is case-insensitive
/// - Use % for zero or more characters, _ for exactly one character
/// - Contains/StartsWith/EndsWith are syntactic sugar for LIKE patterns
/// </remarks>
///
/// <example>
/// <code>
/// // Pattern matching with wildcards
/// StringOp.Like "alice%"       // Starts with "alice"
/// StringOp.Like "%@gmail.com"  // Ends with "@gmail.com"
/// StringOp.Like "%admin%"      // Contains "admin"
///
/// // Case-insensitive matching
/// StringOp.ILike "SMITH"       // Matches "smith", "Smith", "SMITH", etc.
///
/// // Convenience operators (sugar for LIKE patterns)
/// StringOp.Contains "test"     // Equivalent to Like "%test%"
/// StringOp.StartsWith "user_"  // Equivalent to Like "user_%"
/// StringOp.EndsWith ".com"     // Equivalent to Like "%.com"
/// </code>
/// </example>
[<RequireQualifiedAccess>]
type StringOp =
    /// <summary>
    /// SQL LIKE pattern matching (case-sensitive).
    /// </summary>
    /// <param name="pattern">
    /// The pattern to match. Use % for zero or more characters, _ for exactly one character.
    /// </param>
    /// <remarks>
    /// LIKE performs case-sensitive pattern matching in most SQLite configurations.
    /// For case-insensitive matching, use ILike instead.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Match names starting with "A"
    /// StringOp.Like "A%"
    ///
    /// // Match emails from a specific domain
    /// StringOp.Like "%@example.com"
    ///
    /// // Match codes with specific format (e.g., "ABC-123")
    /// StringOp.Like "___-___"  // Three chars, hyphen, three chars
    /// </code>
    /// </example>
    | Like of pattern: string
    
    /// <summary>
    /// SQL LIKE pattern matching (case-insensitive).
    /// </summary>
    /// <param name="pattern">
    /// The pattern to match. Use % for zero or more characters, _ for exactly one character.
    /// </param>
    /// <remarks>
    /// ILike performs case-insensitive pattern matching by adding the COLLATE NOCASE clause.
    /// This is useful for user-facing searches where case should be ignored.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Match "smith", "Smith", "SMITH", etc.
    /// StringOp.ILike "smith"
    ///
    /// // Case-insensitive prefix match
    /// StringOp.ILike "admin%"  // Matches "admin", "Admin", "ADMIN", "Administrator", etc.
    /// </code>
    /// </example>
    | ILike of pattern: string
    
    /// <summary>
    /// Matches strings containing the specified substring anywhere within them.
    /// </summary>
    /// <param name="substring">The substring to search for.</param>
    /// <remarks>
    /// Contains is syntactic sugar for the LIKE pattern '%substring%'.
    /// The matching is case-sensitive by default.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Match any name containing "john"
    /// StringOp.Contains "john"  // Matches "john", "johnson", "John Smith", etc.
    ///
    /// // Match descriptions mentioning "urgent"
    /// StringOp.Contains "urgent"
    /// </code>
    /// </example>
    | Contains of substring: string
    
    /// <summary>
    /// Matches strings that start with the specified prefix.
    /// </summary>
    /// <param name="prefix">The prefix to match.</param>
    /// <remarks>
    /// StartsWith is syntactic sugar for the LIKE pattern 'prefix%'.
    /// The matching is case-sensitive by default.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Match usernames starting with "admin"
    /// StringOp.StartsWith "admin"  // Matches "admin", "admin123", "administrator", etc.
    ///
    /// // Match file paths in a directory
    /// StringOp.StartsWith "/home/user/"
    /// </code>
    /// </example>
    | StartsWith of prefix: string
    
    /// <summary>
    /// Matches strings that end with the specified suffix.
    /// </summary>
    /// <param name="suffix">The suffix to match.</param>
    /// <remarks>
    /// EndsWith is syntactic sugar for the LIKE pattern '%suffix'.
    /// The matching is case-sensitive by default.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Match email addresses from a specific domain
    /// StringOp.EndsWith "@company.com"
    ///
    /// // Match image files
    /// StringOp.EndsWith ".jpg"  // Or .png, .gif, etc.
    ///
    /// // Match URLs with specific paths
    /// StringOp.EndsWith "/api/users"
    /// </code>
    /// </example>
    | EndsWith of suffix: string

/// <summary>
/// Existence check operator for determining whether a field exists in a document.
/// </summary>
///
/// <remarks>
/// ExistsOp is used to query documents based on field presence, independent of the field's value.
/// This is useful for:
///
/// - Finding documents that have optional fields populated
/// - Finding documents missing certain fields (sparse indexes)
/// - Schema validation queries
///
/// In JSON documents stored in SQLite, fields can be genuinely absent (not just null).
/// Exists(true) matches documents where the field is present (even if null).
/// Exists(false) matches documents where the field is completely absent from the JSON.
/// </remarks>
///
/// <example>
/// <code>
/// // Match documents that have an "email" field (even if null)
/// ExistsOp.Exists true
///
/// // Match documents that don't have a "deletedAt" field
/// ExistsOp.Exists false
///
/// // Combine with other queries to find documents with optional fields populated
/// // Query.And [
/// //     Query.Field ("email", FieldOp.Exist (ExistsOp.Exists true))
/// //     Query.Field ("email", FieldOp.Compare (box (CompareOp.Ne null)))
/// // ]
/// </code>
/// </example>
and [<RequireQualifiedAccess>] ExistsOp =
    /// <summary>
    /// Checks whether a field exists in the document.
    /// </summary>
    /// <param name="shouldExist">
    /// If true, matches documents where the field exists (is present in the JSON).
    /// If false, matches documents where the field does not exist (is absent from the JSON).
    /// </param>
    /// <remarks>
    /// This is distinct from null checks. A field can exist with a null value,
    /// or it can be completely absent from the document. Exists checks presence/absence,
    /// not the value.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Field is present (regardless of value, including null)
    /// ExistsOp.Exists true
    ///
    /// // Field is absent (not in the JSON at all)
    /// ExistsOp.Exists false
    /// </code>
    /// </example>
    | Exists of bool

/// <summary>
/// Array-specific operators for querying array/list fields.
/// </summary>
///
/// <typeparam name="'T">The type of elements in the array.</typeparam>
///
/// <remarks>
/// ArrayOp provides operations that only make sense for array/list fields:
///
/// - All: Array contains all specified values (order doesn't matter)
/// - Size: Array has an exact length
/// - ElemMatch: At least one array element matches a complex query
/// - Index: Element at a specific index matches a query
///
/// These operators are only valid for array-typed fields. Using them on
/// non-array fields will result in a runtime query error.
///
/// The type parameter ensures type safety - you can only query arrays of the correct element type.
/// </remarks>
///
/// <example>
/// <code>
/// // Array contains all specified tags
/// ArrayOp.All ["typescript"; "database"]
///
/// // Array has exactly 3 elements
/// ArrayOp.Size 3
///
/// // At least one element matches criteria
/// ArrayOp.ElemMatch (Query.Field ("status", FieldOp.Compare (box (CompareOp.Eq "active"))))
///
/// // Element at index 0 matches criteria
/// ArrayOp.Index (0, Query.Field ("name", FieldOp.Compare (box (CompareOp.Eq "first"))))
/// </code>
/// </example>
and [<RequireQualifiedAccess>] ArrayOp<'T> =
    /// <summary>
    /// Matches arrays that contain all of the specified values.
    /// </summary>
    /// <param name="values">The list of values that must all be present in the array.</param>
    /// <remarks>
    /// Order does not matter - the array can contain the values in any order and can
    /// contain additional values beyond those specified. This operator only checks that
    /// all specified values are present somewhere in the array.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Match documents where tags array contains both "featured" and "public"
    /// ArrayOp.All ["featured"; "public"]
    /// // Would match: ["featured"; "public"], ["public"; "featured"; "archived"]
    /// // Would NOT match: ["featured"], ["public"], []
    ///
    /// // Match documents where scores array contains 100, 95, and 90
    /// ArrayOp.All [100; 95; 90]
    /// </code>
    /// </example>
    | All of list<'T>
    
    /// <summary>
    /// Matches arrays that have exactly the specified number of elements.
    /// </summary>
    /// <param name="length">The exact number of elements the array must contain.</param>
    /// <remarks>
    /// This operator checks the array's length. Use it to find arrays with a specific size,
    /// including empty arrays (Size 0).
    /// </remarks>
    /// <example>
    /// <code>
    /// // Match documents where tags array has exactly 3 elements
    /// ArrayOp.Size 3
    ///
    /// // Match documents with empty arrays
    /// ArrayOp.Size 0
    ///
    /// // Match documents with single-element arrays
    /// ArrayOp.Size 1
    /// </code>
    /// </example>
    | Size of int
    
    /// <summary>
    /// Matches arrays where at least one element satisfies the specified query.
    /// </summary>
    /// <param name="query">The query that at least one array element must match.</param>
    /// <remarks>
    /// ElemMatch allows complex queries on array elements. It's particularly useful for
    /// arrays of objects where you need to match multiple fields within the same element.
    /// The query operates on individual array elements.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Match documents where at least one comment has status "approved"
    /// ArrayOp.ElemMatch (Query.Field ("status", FieldOp.Compare (box (CompareOp.Eq "approved"))))
    ///
    /// // Match documents where at least one item has quantity > 10 AND price &lt; 100
    /// ArrayOp.ElemMatch (Query.And [
    ///     Query.Field ("quantity", FieldOp.Compare (box (CompareOp.Gt 10)))
    ///     Query.Field ("price", FieldOp.Compare (box (CompareOp.Lt 100.0)))
    /// ])
    /// </code>
    /// </example>
    | ElemMatch of Query<'T>
    
    /// <summary>
    /// Matches arrays where the element at the specified index satisfies the query.
    /// </summary>
    /// <param name="index">The zero-based index of the array element to check.</param>
    /// <param name="query">The query that the element at the specified index must match.</param>
    /// <remarks>
    /// Index allows querying specific positions in an array. The index is zero-based.
    /// If the array doesn't have an element at the specified index, the query fails to match.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Match documents where first tag (index 0) equals "featured"
    /// ArrayOp.Index (0, Query.Field ("value", FieldOp.Compare (box (CompareOp.Eq "featured"))))
    ///
    /// // Match documents where second score (index 1) is greater than 90
    /// ArrayOp.Index (1, Query.Field ("score", FieldOp.Compare (box (CompareOp.Gt 90))))
    /// </code>
    /// </example>
    | Index of index: int * Query<'T>

/// <summary>
/// Type-erased wrapper for field operations, allowing heterogeneous storage of different operator types.
/// </summary>
///
/// <remarks>
/// FieldOp boxes generic operators (CompareOp&lt;'T&gt;, ArrayOp&lt;'T&gt;) as 'obj' to enable
/// storage in a uniform query structure. This type erasure is necessary because Query&lt;'T&gt; needs
/// to store operations on fields of different types within the same document.
///
/// Type safety is enforced at query construction time through helper functions, while the
/// SQL translator handles unboxing and validation at execution time.
///
/// Cases:
/// - Compare: Boxed CompareOp&lt;'T&gt; for value comparisons
/// - String: StringOp for string pattern matching
/// - Array: Boxed ArrayOp&lt;'T&gt; for array operations
/// - Exist: ExistsOp for field existence checks
/// </remarks>
///
/// <example>
/// <code>
/// // Wrap a comparison operator
/// let ageOp = FieldOp.Compare (box (CompareOp.Gt 18))
///
/// // Wrap a string operator
/// let emailOp = FieldOp.String (StringOp.EndsWith "@example.com")
///
/// // Wrap an array operator
/// let tagsOp = FieldOp.Array (box (ArrayOp.All ["featured"; "public"]))
///
/// // Wrap an existence check
/// let hasEmailOp = FieldOp.Exist (ExistsOp.Exists true)
/// </code>
/// </example>
and [<RequireQualifiedAccess>] FieldOp =
    /// <summary>
    /// Boxed comparison operator for type-erased storage.
    /// </summary>
    /// <param name="op">A boxed CompareOp&lt;'T&gt; value.</param>
    /// <remarks>
    /// The generic CompareOp&lt;'T&gt; is boxed to 'obj' to allow storage alongside
    /// operations for fields of different types. The SQL translator will unbox and
    /// validate the type at query execution time.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Box an integer comparison
    /// FieldOp.Compare (box (CompareOp.Eq 42))
    ///
    /// // Box a string comparison
    /// FieldOp.Compare (box (CompareOp.In ["active"; "pending"]))
    /// </code>
    /// </example>
    | Compare of obj
    
    /// <summary>
    /// String pattern matching operator.
    /// </summary>
    /// <param name="op">A StringOp value for pattern matching.</param>
    /// <remarks>
    /// StringOp doesn't require boxing since it's not generic.
    /// Only valid for string-typed fields.
    /// </remarks>
    /// <example>
    /// <code>
    /// // String contains check
    /// FieldOp.String (StringOp.Contains "admin")
    ///
    /// // Pattern matching
    /// FieldOp.String (StringOp.Like "%@gmail.com")
    /// </code>
    /// </example>
    | String of StringOp
    
    /// <summary>
    /// Boxed array operator for type-erased storage.
    /// </summary>
    /// <param name="op">A boxed ArrayOp&lt;'T&gt; value.</param>
    /// <remarks>
    /// The generic ArrayOp&lt;'T&gt; is boxed to 'obj' to allow storage alongside
    /// operations for fields of different types. Only valid for array-typed fields.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Box an array containment check
    /// FieldOp.Array (box (ArrayOp.All ["featured"; "public"]))
    ///
    /// // Box an array size check
    /// FieldOp.Array (box (ArrayOp.Size 3))
    /// </code>
    /// </example>
    | Array of obj
    
    /// <summary>
    /// Field existence check.
    /// </summary>
    /// <param name="op">An ExistsOp value for existence checking.</param>
    /// <remarks>
    /// ExistsOp doesn't require boxing since it's not generic.
    /// Checks whether a field is present in the document, independent of its value.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Check field exists
    /// FieldOp.Exist (ExistsOp.Exists true)
    ///
    /// // Check field doesn't exist
    /// FieldOp.Exist (ExistsOp.Exists false)
    /// </code>
    /// </example>
    | Exist of ExistsOp

/// <summary>
/// Complete query structure with logical operators for building complex document queries.
/// </summary>
///
/// <typeparam name="'T">The document type being queried.</typeparam>
///
/// <remarks>
/// Query&lt;'T&gt; is the central type for expressing document queries in FractalDb.
/// It supports:
///
/// - Field-level operations (comparisons, string matching, array operations, existence checks)
/// - Logical combinators (And, Or, Nor, Not)
/// - Empty query (matches all documents)
///
/// Queries are immutable and composable. Use the Query module helper functions or
/// QueryBuilder computation expression to construct queries ergonomically.
///
/// The type parameter 'T represents the document type, enabling type-safe field access
/// when used with computation expressions.
/// </remarks>
///
/// <example>
/// <code>
/// // Match all documents
/// Query.Empty
///
/// // Single field query
/// Query.Field ("age", FieldOp.Compare (box (CompareOp.Gt 18)))
///
/// // Logical AND
/// Query.And [
///     Query.Field ("status", FieldOp.Compare (box (CompareOp.Eq "active")))
///     Query.Field ("age", FieldOp.Compare (box (CompareOp.Gte 18)))
/// ]
///
/// // Logical OR
/// Query.Or [
///     Query.Field ("role", FieldOp.Compare (box (CompareOp.Eq "admin")))
///     Query.Field ("role", FieldOp.Compare (box (CompareOp.Eq "moderator")))
/// ]
///
/// // Negation
/// Query.Not (Query.Field ("deleted", FieldOp.Exist (ExistsOp.Exists true)))
/// </code>
/// </example>
and [<RequireQualifiedAccess>] Query<'T> =
    /// <summary>
    /// Empty query that matches all documents.
    /// </summary>
    /// <remarks>
    /// Translates to SQL "WHERE 1=1" or is omitted entirely.
    /// Useful as a base case for query composition.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Retrieve all documents
    /// Query.Empty
    /// </code>
    /// </example>
    | Empty
    
    /// <summary>
    /// Single field operation query.
    /// </summary>
    /// <param name="fieldName">The JSON field path (supports dot notation for nested fields).</param>
    /// <param name="op">The field operation to perform.</param>
    /// <remarks>
    /// Queries a single field using the specified operation. Field paths use dot notation
    /// for nested JSON fields (e.g., "user.profile.email").
    /// </remarks>
    /// <example>
    /// <code>
    /// // Simple equality
    /// Query.Field ("status", FieldOp.Compare (box (CompareOp.Eq "active")))
    ///
    /// // Nested field access
    /// Query.Field ("user.email", FieldOp.String (StringOp.EndsWith "@company.com"))
    ///
    /// // Array field query
    /// Query.Field ("tags", FieldOp.Array (box (ArrayOp.All ["featured"; "public"])))
    /// </code>
    /// </example>
    | Field of fieldName: string * FieldOp
    
    /// <summary>
    /// Logical AND - all sub-queries must match.
    /// </summary>
    /// <param name="queries">List of queries that must all match.</param>
    /// <remarks>
    /// An empty list is treated as Empty (matches all documents).
    /// A single-element list can be unwrapped to the contained query.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Multiple conditions must all be true
    /// Query.And [
    ///     Query.Field ("age", FieldOp.Compare (box (CompareOp.Gte 18)))
    ///     Query.Field ("age", FieldOp.Compare (box (CompareOp.Lt 65)))
    ///     Query.Field ("status", FieldOp.Compare (box (CompareOp.Eq "active")))
    /// ]
    /// </code>
    /// </example>
    | And of list<Query<'T>>
    
    /// <summary>
    /// Logical OR - at least one sub-query must match.
    /// </summary>
    /// <param name="queries">List of queries where at least one must match.</param>
    /// <remarks>
    /// An empty list matches no documents.
    /// A single-element list can be unwrapped to the contained query.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Any of these conditions can be true
    /// Query.Or [
    ///     Query.Field ("role", FieldOp.Compare (box (CompareOp.Eq "admin")))
    ///     Query.Field ("role", FieldOp.Compare (box (CompareOp.Eq "moderator")))
    ///     Query.Field ("permissions", FieldOp.Array (box (ArrayOp.All ["manage_users"])))
    /// ]
    /// </code>
    /// </example>
    | Or of list<Query<'T>>
    
    /// <summary>
    /// Logical NOR - none of the sub-queries must match.
    /// </summary>
    /// <param name="queries">List of queries where none must match.</param>
    /// <remarks>
    /// NOR is equivalent to NOT (OR ...). Useful for exclusion patterns.
    /// An empty list matches all documents (nothing to exclude).
    /// </remarks>
    /// <example>
    /// <code>
    /// // Exclude documents matching any of these conditions
    /// Query.Nor [
    ///     Query.Field ("status", FieldOp.Compare (box (CompareOp.Eq "deleted")))
    ///     Query.Field ("status", FieldOp.Compare (box (CompareOp.Eq "archived")))
    ///     Query.Field ("banned", FieldOp.Compare (box (CompareOp.Eq true)))
    /// ]
    /// </code>
    /// </example>
    | Nor of list<Query<'T>>
    
    /// <summary>
    /// Logical NOT - negates the sub-query.
    /// </summary>
    /// <param name="query">The query to negate.</param>
    /// <remarks>
    /// Matches documents that do NOT match the sub-query.
    /// Double negation (Not (Not q)) can be simplified to q.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Documents that are NOT deleted
    /// Query.Not (Query.Field ("deleted", FieldOp.Exist (ExistsOp.Exists true)))
    ///
    /// // Documents where age is NOT greater than 65
    /// Query.Not (Query.Field ("age", FieldOp.Compare (box (CompareOp.Gt 65))))
    /// </code>
    /// </example>
    | Not of Query<'T>

