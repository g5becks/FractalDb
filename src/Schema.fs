module FractalDb.Schema

/// <summary>
/// SQLite column type mapping for document fields.
/// </summary>
///
/// <remarks>
/// SqliteType represents the SQLite storage type for extracted JSON fields.
/// FractalDb creates generated columns in SQLite for indexed/unique fields,
/// and these columns must have explicit SQLite types.
///
/// Type Mapping:
/// - Text: TEXT columns (strings, serialized JSON)
/// - Integer: INTEGER columns (64-bit integers, booleans as 0/1)
/// - Real: REAL columns (floating-point numbers)
/// - Blob: BLOB columns (binary data)
/// - Numeric: NUMERIC columns (flexible numeric storage)
/// - Boolean: Special case, stored as INTEGER (0=false, 1=true)
///
/// SQLite's dynamic typing means the actual stored values can vary, but
/// generated columns use these explicit type declarations for optimization.
/// </remarks>
///
/// <example>
/// <code>
/// // Define a string field as TEXT
/// let nameField = { Name = "name"; SqlType = SqliteType.Text; ... }
///
/// // Define a number field as INTEGER
/// let ageField = { Name = "age"; SqlType = SqliteType.Integer; ... }
///
/// // Define a boolean field (stored as 0/1)
/// let activeField = { Name = "active"; SqlType = SqliteType.Boolean; ... }
///
/// // Define a floating-point field
/// let scoreField = { Name = "score"; SqlType = SqliteType.Real; ... }
/// </code>
/// </example>
[<RequireQualifiedAccess>]
type SqliteType =
    /// <summary>
    /// TEXT storage class for string values.
    /// </summary>
    /// <remarks>
    /// Used for string fields, serialized JSON, dates as ISO strings, etc.
    /// SQLite stores TEXT as UTF-8, UTF-16BE, or UTF-16LE based on encoding.
    /// </remarks>
    /// <example>
    /// <code>
    /// // String field
    /// SqliteType.Text  // Maps to "TEXT" in SQL
    ///
    /// // Email field
    /// { Name = "email"; SqlType = SqliteType.Text; Unique = true }
    /// </code>
    /// </example>
    | Text
    
    /// <summary>
    /// INTEGER storage class for signed 64-bit integer values.
    /// </summary>
    /// <remarks>
    /// Used for integer fields, counts, IDs, Unix timestamps, etc.
    /// SQLite integers can be 1, 2, 3, 4, 6, or 8 bytes depending on magnitude.
    /// Boolean values should use SqliteType.Boolean instead (also stored as INTEGER).
    /// </remarks>
    /// <example>
    /// <code>
    /// // Age field
    /// SqliteType.Integer  // Maps to "INTEGER" in SQL
    ///
    /// // Timestamp field
    /// { Name = "createdAt"; SqlType = SqliteType.Integer }
    /// </code>
    /// </example>
    | Integer
    
    /// <summary>
    /// REAL storage class for floating-point values.
    /// </summary>
    /// <remarks>
    /// Used for floating-point numbers (IEEE 754 double-precision).
    /// SQLite stores REAL as 8-byte IEEE floating point numbers.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Price field
    /// SqliteType.Real  // Maps to "REAL" in SQL
    ///
    /// // Rating field
    /// { Name = "rating"; SqlType = SqliteType.Real }
    /// </code>
    /// </example>
    | Real
    
    /// <summary>
    /// BLOB storage class for binary data.
    /// </summary>
    /// <remarks>
    /// Used for binary data stored exactly as input.
    /// Useful for images, encrypted data, serialized objects, etc.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Binary data field
    /// SqliteType.Blob  // Maps to "BLOB" in SQL
    ///
    /// // File attachment field
    /// { Name = "attachment"; SqlType = SqliteType.Blob }
    /// </code>
    /// </example>
    | Blob
    
    /// <summary>
    /// NUMERIC storage class for flexible numeric values.
    /// </summary>
    /// <remarks>
    /// SQLite NUMERIC affinity attempts to convert values to INTEGER or REAL
    /// when possible, otherwise stores as TEXT. Useful for decimal numbers
    /// that should maintain precision.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Monetary amount (preserves precision)
    /// SqliteType.Numeric  // Maps to "NUMERIC" in SQL
    ///
    /// // Decimal field
    /// { Name = "amount"; SqlType = SqliteType.Numeric }
    /// </code>
    /// </example>
    | Numeric
    
    /// <summary>
    /// Boolean values stored as INTEGER (0=false, 1=true).
    /// </summary>
    /// <remarks>
    /// SQLite doesn't have a native boolean type. Boolean fields are stored
    /// as INTEGER with 0 representing false and 1 representing true.
    /// The SQL translation layer handles the conversion automatically.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Active flag
    /// SqliteType.Boolean  // Stored as INTEGER (0/1)
    ///
    /// // Published field
    /// { Name = "published"; SqlType = SqliteType.Boolean }
    ///
    /// // Query: WHERE published = 1
    /// </code>
    /// </example>
    | Boolean

/// <summary>
/// Field definition for a document field to be indexed or constrained.
/// </summary>
///
/// <remarks>
/// FieldDef describes a field from the document that should be extracted as a
/// generated column in SQLite. Generated columns enable efficient querying on
/// JSON fields without parsing the entire document.
///
/// Fields:
/// - Name: The field name (used in queries and column names)
/// - Path: Optional JSON path (defaults to $.{Name} if None)
/// - SqlType: SQLite storage type for the generated column
/// - Indexed: Whether to create an index on this field
/// - Unique: Whether to enforce uniqueness (implies Indexed)
/// - Nullable: Whether the field can be NULL
///
/// Only fields that need indexing, uniqueness, or type constraints should be
/// defined explicitly. All other fields remain in the JSON body and are
/// queried via jsonb_extract.
/// </remarks>
///
/// <example>
/// <code>
/// // Simple indexed string field
/// let nameField = {
///     Name = "name"
///     Path = None  // Defaults to "$.name"
///     SqlType = SqliteType.Text
///     Indexed = true
///     Unique = false
///     Nullable = false
/// }
///
/// // Unique email field with custom path
/// let emailField = {
///     Name = "email"
///     Path = Some "$.user.email"
///     SqlType = SqliteType.Text
///     Indexed = true  // Automatically indexed due to Unique
///     Unique = true
///     Nullable = false
/// }
///
/// // Optional age field (nullable)
/// let ageField = {
///     Name = "age"
///     Path = None
///     SqlType = SqliteType.Integer
///     Indexed = true
///     Unique = false
///     Nullable = true
/// }
/// </code>
/// </example>
type FieldDef = {
    /// <summary>
    /// Field name used in queries and for generated column naming.
    /// </summary>
    /// <remarks>
    /// The column name will be prefixed with underscore (e.g., "_name").
    /// Must be a valid SQLite identifier.
    /// </remarks>
    Name: string
    
    /// <summary>
    /// JSON path for field extraction. Defaults to $.{Name} if None.
    /// </summary>
    /// <remarks>
    /// Uses SQLite json_extract syntax. Examples:
    /// - None → "$.name"
    /// - Some "$.user.email" → custom nested path
    /// - Some "$.tags[0]" → array element access
    /// </remarks>
    Path: option<string>
    
    /// <summary>
    /// SQLite type for the generated column.
    /// </summary>
    /// <remarks>
    /// Determines the column type in the CREATE TABLE statement.
    /// Must match the actual data type in JSON for correct comparisons.
    /// </remarks>
    SqlType: SqliteType
    
    /// <summary>
    /// Whether to create an index on this field.
    /// </summary>
    /// <remarks>
    /// Indexed fields enable faster queries but increase write overhead.
    /// Automatically true if Unique is true.
    /// </remarks>
    Indexed: bool
    
    /// <summary>
    /// Whether to enforce uniqueness constraint.
    /// </summary>
    /// <remarks>
    /// Unique fields automatically have Indexed = true.
    /// Creates a UNIQUE constraint on the generated column.
    /// Prevents duplicate values across all documents.
    /// </remarks>
    Unique: bool
    
    /// <summary>
    /// Whether the field can be NULL.
    /// </summary>
    /// <remarks>
    /// If false, adds NOT NULL constraint to the column.
    /// NULL means the field is absent from the JSON document.
    /// </remarks>
    Nullable: bool
}

/// <summary>
/// Index definition for creating composite or custom indexes.
/// </summary>
///
/// <remarks>
/// IndexDef allows creation of multi-column indexes beyond the single-field
/// indexes created by FieldDef. Useful for:
/// - Composite indexes on multiple fields
/// - Custom naming for indexes
/// - Unique constraints across multiple fields
///
/// Fields:
/// - Name: Index name (must be unique within the database)
/// - Fields: List of field names to include in the index
/// - Unique: Whether this is a UNIQUE index
///
/// The fields must reference existing FieldDef names or valid JSON paths.
/// </remarks>
///
/// <example>
/// <code>
/// // Composite index on email and status
/// let emailStatusIndex = {
///     Name = "idx_email_status"
///     Fields = ["email"; "status"]
///     Unique = false
/// }
///
/// // Unique compound index
/// let uniqueUserRole = {
///     Name = "idx_unique_user_role"
///     Fields = ["userId"; "roleId"]
///     Unique = true  // Enforces uniqueness of the combination
/// }
///
/// // Single-field custom index (alternative to FieldDef.Indexed)
/// let customNameIndex = {
///     Name = "idx_name_custom"
///     Fields = ["name"]
///     Unique = false
/// }
/// </code>
/// </example>
type IndexDef = {
    /// <summary>
    /// Index name used in CREATE INDEX statement.
    /// </summary>
    /// <remarks>
    /// Must be a valid SQLite identifier and unique within the database.
    /// Convention: prefix with "idx_" for regular indexes.
    /// </remarks>
    Name: string
    
    /// <summary>
    /// List of field names to include in the index.
    /// </summary>
    /// <remarks>
    /// Fields are included in the index in the order specified.
    /// Order matters for composite indexes - queries benefit most when
    /// they filter/sort on leftmost fields first.
    /// Each field must reference a defined FieldDef or valid JSON path.
    /// </remarks>
    Fields: list<string>
    
    /// <summary>
    /// Whether this is a UNIQUE index.
    /// </summary>
    /// <remarks>
    /// UNIQUE indexes enforce uniqueness constraint on the combination of fields.
    /// For single-field uniqueness, consider using FieldDef.Unique instead.
    /// </remarks>
    Unique: bool
}

/// <summary>
/// Schema definition for a document collection with field definitions and validation.
/// </summary>
///
/// <remarks>
/// SchemaDef is the top-level schema configuration for a document collection.
/// It specifies which fields to extract as generated columns, what indexes to create,
/// and optional validation logic for documents.
///
/// Fields:
/// - Fields: List of field definitions for generated columns (indexed/unique/constrained fields)
/// - Indexes: List of composite or custom index definitions
/// - Timestamps: Whether to automatically manage createdAt/updatedAt timestamps
/// - Validate: Optional validation function called before insert/update operations
///
/// Key Design Points:
/// 1. Only fields that need indexing, uniqueness, or constraints are defined in Fields
/// 2. All other fields remain in the JSON body and are queried dynamically
/// 3. Timestamps auto-manages DocumentMeta.CreatedAt and DocumentMeta.UpdatedAt
/// 4. Validation returns Result - Ok(transformed) or Error(message)
///
/// The schema is used by Collection to:
/// - Generate SQLite table with extracted columns
/// - Create indexes for query optimization
/// - Enforce constraints (unique, not null)
/// - Validate documents before persistence
/// </remarks>
///
/// <example>
/// <code>
/// // User schema with email uniqueness and validation
/// let userSchema: SchemaDef&lt;User&gt; = {
///     Fields = [
///         { Name = "email"; Path = None; SqlType = SqliteType.Text
///           Indexed = true; Unique = true; Nullable = false }
///         { Name = "age"; Path = None; SqlType = SqliteType.Integer
///           Indexed = true; Unique = false; Nullable = true }
///         { Name = "status"; Path = None; SqlType = SqliteType.Text
///           Indexed = true; Unique = false; Nullable = false }
///     ]
///     Indexes = [
///         { Name = "idx_email_status"; Fields = ["email"; "status"]; Unique = false }
///     ]
///     Timestamps = true  // Auto-manage createdAt/updatedAt
///     Validate = Some (fun user -&gt;
///         if user.age &lt; 0 then Error "Age must be non-negative"
///         elif String.IsNullOrWhiteSpace(user.email) then Error "Email required"
///         else Ok user
///     )
/// }
///
/// // Minimal schema with just timestamps
/// let minimalSchema: SchemaDef&lt;Document&gt; = {
///     Fields = []
///     Indexes = []
///     Timestamps = true
///     Validate = None
/// }
///
/// // Schema with custom validation and transformation
/// let productSchema: SchemaDef&lt;Product&gt; = {
///     Fields = [
///         { Name = "sku"; Path = None; SqlType = SqliteType.Text
///           Indexed = true; Unique = true; Nullable = false }
///         { Name = "price"; Path = None; SqlType = SqliteType.Real
///           Indexed = true; Unique = false; Nullable = false }
///     ]
///     Indexes = []
///     Timestamps = true
///     Validate = Some (fun product -&gt;
///         // Validation with transformation (normalize SKU)
///         let normalizedSku = product.sku.ToUpperInvariant()
///         if product.price &lt;= 0.0 then Error "Price must be positive"
///         else Ok { product with sku = normalizedSku }
///     )
/// }
/// </code>
/// </example>
type SchemaDef<'T> = {
    /// <summary>
    /// List of field definitions for generated columns.
    /// </summary>
    /// <remarks>
    /// Each FieldDef describes a field to extract from the JSON document
    /// as a generated column in SQLite. Only define fields that need:
    /// - Indexing for query performance
    /// - Unique constraints
    /// - NOT NULL constraints
    /// - Type conversion for comparisons
    ///
    /// All other fields remain in the JSON body and can be queried dynamically.
    /// </remarks>
    Fields: list<FieldDef>
    
    /// <summary>
    /// List of composite or custom index definitions.
    /// </summary>
    /// <remarks>
    /// IndexDef allows creation of multi-column indexes beyond the single-field
    /// indexes created by FieldDef.Indexed. Use this for:
    /// - Composite indexes on multiple fields (e.g., email + status)
    /// - Unique constraints across field combinations
    /// - Custom index naming
    ///
    /// Single-field indexes can be defined via FieldDef.Indexed instead.
    /// </remarks>
    Indexes: list<IndexDef>
    
    /// <summary>
    /// Whether to automatically manage createdAt and updatedAt timestamps.
    /// </summary>
    /// <remarks>
    /// If true, FractalDb automatically:
    /// - Sets DocumentMeta.CreatedAt on insert (Unix milliseconds)
    /// - Updates DocumentMeta.UpdatedAt on every update
    /// - Creates indexes on _createdAt and _updatedAt columns
    ///
    /// If false, timestamp fields are ignored and users manage timestamps manually.
    /// </remarks>
    Timestamps: bool
    
    /// <summary>
    /// Optional validation function called before insert/update operations.
    /// </summary>
    /// <remarks>
    /// Validation function signature: 'T -&gt; Result&lt;'T, string&gt;
    ///
    /// The function receives the document data before persistence and can:
    /// - Validate business rules (return Error with message)
    /// - Transform/normalize the document (return Ok with modified value)
    /// - Allow persistence unchanged (return Ok with original value)
    ///
    /// If validation returns Error, the operation fails with FractalError.ValidationError.
    /// If validation returns Ok, the (possibly transformed) value is persisted.
    ///
    /// Validation is called:
    /// - Before insertOne/insertMany (on user data before Document wrapper)
    /// - Before updateOne/updateMany/replaceOne (on updated data)
    /// - NOT called on findOneAndUpdate/Replace (user responsible)
    /// </remarks>
    Validate: option<('T -> Result<'T, string>)>
}
