module FractalDb.Types

open System

/// <summary>
/// Metadata added to all documents in the database.
/// </summary>
///
/// <remarks>
/// This type contains auto-generated fields that FractalDb manages automatically:
/// - Id: A unique identifier (UUID v7) generated when the document is created
/// - CreatedAt: Unix timestamp in milliseconds when the document was first inserted
/// - UpdatedAt: Unix timestamp in milliseconds when the document was last modified
/// </remarks>
///
/// <example>
/// <code>
/// let meta = {
///     Id = "01234567-89ab-cdef-0123-456789abcdef"
///     CreatedAt = 1704067200000L
///     UpdatedAt = 1704153600000L
/// }
/// </code>
/// </example>
type DocumentMeta = {
    /// <summary>Unique identifier for the document (UUID v7 format).</summary>
    Id: string
    
    /// <summary>Unix timestamp in milliseconds when the document was created.</summary>
    CreatedAt: int64
    
    /// <summary>Unix timestamp in milliseconds when the document was last updated.</summary>
    UpdatedAt: int64
}

/// <summary>
/// A document wraps user data with auto-generated metadata.
/// </summary>
///
/// <typeparam name="'T">The type of user data stored in the document.</typeparam>
///
/// <remarks>
/// Documents are the primary storage unit in FractalDb. Each document contains:
/// - Id: Auto-generated UUID v7 identifier
/// - Data: The user's data of type 'T
/// - CreatedAt: Timestamp when the document was first inserted
/// - UpdatedAt: Timestamp when the document was last modified
///
/// FractalDb automatically manages the metadata fields, while users work with the Data field.
/// </remarks>
///
/// <example>
/// <code>
/// type User = { Name: string; Email: string; Age: int }
///
/// let doc: Document&lt;User&gt; = {
///     Id = "01234567-89ab-cdef-0123-456789abcdef"
///     Data = { Name = "Alice"; Email = "alice@example.com"; Age = 30 }
///     CreatedAt = 1704067200000L
///     UpdatedAt = 1704153600000L
/// }
/// </code>
/// </example>
type Document<'T> = {
    /// <summary>Unique identifier for the document (UUID v7 format).</summary>
    Id: string
    
    /// <summary>The user data stored in this document.</summary>
    Data: 'T
    
    /// <summary>Unix timestamp in milliseconds when the document was created.</summary>
    CreatedAt: int64
    
    /// <summary>Unix timestamp in milliseconds when the document was last updated.</summary>
    UpdatedAt: int64
}

/// <summary>
/// Provides functions for generating and validating document identifiers.
/// </summary>
///
/// <remarks>
/// FractalDb uses UUID v7 for document IDs, which provides:
/// - Time-sortable: First 48 bits encode Unix timestamp in milliseconds
/// - Globally unique: Remaining bits are random
/// - Lexicographically orderable: String comparison preserves time order
/// - Low collision probability: 2^-74 collision probability
///
/// UUID v7 is available in .NET 9+ via Guid.CreateVersion7().
/// </remarks>
module IdGenerator =
    
    /// <summary>
    /// Generates a new time-sortable UUID v7 identifier.
    /// </summary>
    ///
    /// <returns>A string representation of a UUID v7 in standard format (lowercase with hyphens).</returns>
    ///
    /// <remarks>
    /// UUID v7 encodes the current timestamp in its first 48 bits, making IDs naturally sortable by creation time.
    /// This is ideal for document databases where temporal ordering is important.
    /// </remarks>
    ///
    /// <example>
    /// <code>
    /// let id1 = IdGenerator.generate()
    /// // id1 = "01234567-89ab-7def-8123-456789abcdef"
    ///
    /// let id2 = IdGenerator.generate()
    /// // id2 > id1 (lexicographically, since id2 was generated later)
    /// </code>
    /// </example>
    let generate () : string =
        Guid.CreateVersion7().ToString()
    
    /// <summary>
    /// Checks if an ID string is empty, null, or represents an empty GUID.
    /// </summary>
    ///
    /// <param name="id">The ID string to check.</param>
    ///
    /// <returns>
    /// <c>true</c> if the ID is null, empty, or equals "00000000-0000-0000-0000-000000000000"; 
    /// otherwise <c>false</c>.
    /// </returns>
    ///
    /// <remarks>
    /// Use this function to determine if an ID should be auto-generated.
    /// Documents with empty or default IDs will have new UUIDs generated automatically.
    /// </remarks>
    ///
    /// <example>
    /// <code>
    /// IdGenerator.isEmptyOrDefault ""     // true
    /// IdGenerator.isEmptyOrDefault null   // true
    /// IdGenerator.isEmptyOrDefault "00000000-0000-0000-0000-000000000000"  // true
    /// IdGenerator.isEmptyOrDefault "01234567-89ab-7def-8123-456789abcdef"  // false
    /// </code>
    /// </example>
    let isEmptyOrDefault (id: string) : bool =
        String.IsNullOrEmpty id || id = Guid.Empty.ToString()
    
    /// <summary>
    /// Validates whether a string is a properly formatted GUID.
    /// </summary>
    ///
    /// <param name="id">The string to validate.</param>
    ///
    /// <returns><c>true</c> if the string is a valid GUID format; otherwise <c>false</c>.</returns>
    ///
    /// <remarks>
    /// This function accepts any valid GUID format, including:
    /// - Standard format with hyphens: "01234567-89ab-cdef-0123-456789abcdef"
    /// - Format without hyphens: "0123456789abcdef0123456789abcdef"
    /// - Format with braces: "{01234567-89ab-cdef-0123-456789abcdef}"
    /// - Format with parentheses: "(01234567-89ab-cdef-0123-456789abcdef)"
    /// </remarks>
    ///
    /// <example>
    /// <code>
    /// IdGenerator.isValid "01234567-89ab-cdef-0123-456789abcdef"  // true
    /// IdGenerator.isValid "0123456789abcdef0123456789abcdef"      // true
    /// IdGenerator.isValid "not-a-guid"                            // false
    /// IdGenerator.isValid ""                                      // false
    /// </code>
    /// </example>
    let isValid (id: string) : bool =
        match Guid.TryParse(id) with
        | true, _ -> true
        | false, _ -> false

/// <summary>
/// Provides utility functions for working with Unix timestamps.
/// </summary>
///
/// <remarks>
/// FractalDb stores all timestamps as Unix milliseconds (int64) for consistency and compactness.
/// This module provides conversions between .NET's DateTimeOffset and Unix timestamps,
/// as well as utility functions for timestamp operations.
/// </remarks>
module Timestamp =
    
    /// <summary>
    /// Gets the current Unix timestamp in milliseconds.
    /// </summary>
    ///
    /// <returns>The current UTC time as a Unix timestamp in milliseconds.</returns>
    ///
    /// <remarks>
    /// This function always returns UTC time. Unix timestamps are timezone-agnostic
    /// and represent the number of milliseconds since January 1, 1970 00:00:00 UTC.
    /// </remarks>
    ///
    /// <example>
    /// <code>
    /// let timestamp = Timestamp.now()
    /// // timestamp = 1704067200000L (example: Jan 1, 2024 00:00:00 UTC)
    /// </code>
    /// </example>
    let now () : int64 =
        DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
    
    /// <summary>
    /// Converts a Unix timestamp in milliseconds to a DateTimeOffset.
    /// </summary>
    ///
    /// <param name="timestamp">The Unix timestamp in milliseconds.</param>
    ///
    /// <returns>A DateTimeOffset representing the timestamp in UTC.</returns>
    ///
    /// <remarks>
    /// The returned DateTimeOffset will have an offset of +00:00 (UTC).
    /// To convert to a different timezone, use DateTimeOffset.ToOffset().
    /// </remarks>
    ///
    /// <example>
    /// <code>
    /// let dto = Timestamp.toDateTimeOffset 1704067200000L
    /// // dto = 2024-01-01 00:00:00.000 +00:00
    /// </code>
    /// </example>
    let toDateTimeOffset (timestamp: int64) : DateTimeOffset =
        DateTimeOffset.FromUnixTimeMilliseconds(timestamp)
    
    /// <summary>
    /// Converts a DateTimeOffset to a Unix timestamp in milliseconds.
    /// </summary>
    ///
    /// <param name="dto">The DateTimeOffset to convert.</param>
    ///
    /// <returns>A Unix timestamp in milliseconds representing the DateTimeOffset.</returns>
    ///
    /// <remarks>
    /// This conversion is timezone-aware. The function will convert the DateTimeOffset
    /// to UTC before generating the timestamp.
    /// </remarks>
    ///
    /// <example>
    /// <code>
    /// let dto = DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero)
    /// let timestamp = Timestamp.fromDateTimeOffset dto
    /// // timestamp = 1704067200000L
    /// </code>
    /// </example>
    let fromDateTimeOffset (dto: DateTimeOffset) : int64 =
        dto.ToUnixTimeMilliseconds()
    
    /// <summary>
    /// Checks if a timestamp falls within a specified range (inclusive).
    /// </summary>
    ///
    /// <param name="start">The start of the range (inclusive).</param>
    /// <param name="end'">The end of the range (inclusive).</param>
    /// <param name="timestamp">The timestamp to check.</param>
    ///
    /// <returns><c>true</c> if the timestamp is within the range; otherwise <c>false</c>.</returns>
    ///
    /// <remarks>
    /// This function performs an inclusive range check: start &lt;= timestamp &lt;= end.
    /// Useful for filtering documents by creation or update time ranges.
    /// </remarks>
    ///
    /// <example>
    /// <code>
    /// let start = 1704067200000L  // Jan 1, 2024
    /// let end' = 1704153600000L   // Jan 2, 2024
    /// let ts = 1704110400000L     // Jan 1, 2024 12:00:00
    ///
    /// Timestamp.isInRange start end' ts  // true
    /// Timestamp.isInRange start end' (start - 1L)  // false
    /// </code>
    /// </example>
    let isInRange (start: int64) (end': int64) (timestamp: int64) : bool =
        timestamp >= start && timestamp <= end'

/// <summary>
/// Provides functions for creating and manipulating documents.
/// </summary>
///
/// <remarks>
/// Documents in FractalDb wrap user data with auto-generated metadata (ID and timestamps).
/// This module provides functions to create documents with automatic ID generation,
/// update document data while preserving metadata, and transform document data.
/// </remarks>
module Document =
    
    /// <summary>
    /// Creates a new document with auto-generated ID and current timestamps.
    /// </summary>
    ///
    /// <param name="data">The user data to wrap in a document.</param>
    ///
    /// <returns>A new document with generated ID and timestamps set to the current time.</returns>
    ///
    /// <remarks>
    /// This is the primary way to create documents. The ID is generated using UUID v7,
    /// which provides time-sortable identifiers. Both CreatedAt and UpdatedAt are set
    /// to the current UTC timestamp.
    /// </remarks>
    ///
    /// <example>
    /// <code>
    /// type User = { Name: string; Email: string }
    ///
    /// let doc = Document.create { Name = "Alice"; Email = "alice@example.com" }
    /// // doc.Id is auto-generated (e.g., "01234567-89ab-7def-8123-456789abcdef")
    /// // doc.CreatedAt and doc.UpdatedAt are set to current timestamp
    /// // doc.Data = { Name = "Alice"; Email = "alice@example.com" }
    /// </code>
    /// </example>
    let create (data: 'T) : Document<'T> =
        let now = Timestamp.now()
        {
            Id = IdGenerator.generate()
            Data = data
            CreatedAt = now
            UpdatedAt = now
        }
    
    /// <summary>
    /// Creates a new document with an explicit ID and current timestamps.
    /// </summary>
    ///
    /// <param name="id">The explicit ID to use for the document.</param>
    /// <param name="data">The user data to wrap in a document.</param>
    ///
    /// <returns>A new document with the specified ID and timestamps set to the current time.</returns>
    ///
    /// <remarks>
    /// Use this function when you need to specify the document ID explicitly,
    /// such as when importing data or maintaining external ID references.
    /// The ID should be a valid UUID format.
    /// </remarks>
    ///
    /// <example>
    /// <code>
    /// type User = { Name: string; Email: string }
    ///
    /// let customId = "01234567-89ab-7def-8123-456789abcdef"
    /// let doc = Document.createWithId customId { Name = "Bob"; Email = "bob@example.com" }
    /// // doc.Id = "01234567-89ab-7def-8123-456789abcdef"
    /// // doc.CreatedAt and doc.UpdatedAt are set to current timestamp
    /// </code>
    /// </example>
    let createWithId (id: string) (data: 'T) : Document<'T> =
        let now = Timestamp.now()
        {
            Id = id
            Data = data
            CreatedAt = now
            UpdatedAt = now
        }
    
    /// <summary>
    /// Updates a document's data using a transformation function, preserving ID and CreatedAt.
    /// </summary>
    ///
    /// <param name="f">A function that transforms the document data.</param>
    /// <param name="doc">The document to update.</param>
    ///
    /// <returns>
    /// A new document with transformed data, the same ID and CreatedAt,
    /// and UpdatedAt set to the current time.
    /// </returns>
    ///
    /// <remarks>
    /// This function creates a new document with updated data while preserving
    /// the document's identity (ID) and creation time. The UpdatedAt timestamp
    /// is automatically set to the current time.
    /// </remarks>
    ///
    /// <example>
    /// <code>
    /// type User = { Name: string; Email: string; Age: int }
    ///
    /// let originalDoc = Document.create { Name = "Alice"; Email = "alice@example.com"; Age = 30 }
    ///
    /// // Increment age by 1
    /// let updatedDoc = Document.update (fun user -> { user with Age = user.Age + 1 }) originalDoc
    /// // updatedDoc.Id = originalDoc.Id
    /// // updatedDoc.Data.Age = 31
    /// // updatedDoc.CreatedAt = originalDoc.CreatedAt
    /// // updatedDoc.UpdatedAt = current timestamp (greater than originalDoc.UpdatedAt)
    /// </code>
    /// </example>
    let update (f: 'T -> 'T) (doc: Document<'T>) : Document<'T> =
        { doc with
            Data = f doc.Data
            UpdatedAt = Timestamp.now() }
    
    /// <summary>
    /// Maps a document's data to a different type while preserving all metadata.
    /// </summary>
    ///
    /// <param name="f">A function that transforms the document data from type 'T to type 'U.</param>
    /// <param name="doc">The document to map.</param>
    ///
    /// <typeparam name="'T">The original data type.</typeparam>
    /// <typeparam name="'U">The target data type.</typeparam>
    ///
    /// <returns>
    /// A new document with transformed data and the same ID, CreatedAt, and UpdatedAt.
    /// </returns>
    ///
    /// <remarks>
    /// Unlike 'update', this function preserves the UpdatedAt timestamp because
    /// it's used for data transformations (like projections) rather than actual updates.
    /// This is useful for converting between different representations of the same document.
    /// </remarks>
    ///
    /// <example>
    /// <code>
    /// type User = { Name: string; Email: string; Age: int }
    /// type UserSummary = { DisplayName: string; Contact: string }
    ///
    /// let userDoc = Document.create { Name = "Alice"; Email = "alice@example.com"; Age = 30 }
    ///
    /// let summaryDoc = Document.map 
    ///     (fun user -> { DisplayName = user.Name; Contact = user.Email })
    ///     userDoc
    /// // summaryDoc.Id = userDoc.Id
    /// // summaryDoc.Data = { DisplayName = "Alice"; Contact = "alice@example.com" }
    /// // summaryDoc.CreatedAt = userDoc.CreatedAt
    /// // summaryDoc.UpdatedAt = userDoc.UpdatedAt (preserved)
    /// </code>
    /// </example>
    let map (f: 'T -> 'U) (doc: Document<'T>) : Document<'U> =
        {
            Id = doc.Id
            Data = f doc.Data
            CreatedAt = doc.CreatedAt
            UpdatedAt = doc.UpdatedAt
        }
