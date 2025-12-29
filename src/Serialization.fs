module FractalDb.Serialization

open System.Text.Json
open System.Text.Json.Serialization

/// <summary>
/// Default JSON serializer options configured for F# types.
/// </summary>
///
/// <remarks>
/// Provides pre-configured JsonSerializerOptions optimized for FractalDb's F# types.
/// Uses FSharp.SystemTextJson for native F# support (records, DUs, options, etc.).
///
/// Configuration:
/// - JsonFSharpConverter: Handles F# records, discriminated unions, options, tuples
/// - PropertyNamingPolicy: CamelCase (converts PascalCase to camelCase in JSON)
/// - WriteIndented: false (compact JSON output)
///
/// The converter handles:
/// - Record types: { Name = "Alice"; Age = 30 } → {"name":"Alice","age":30}
/// - Discriminated unions: Error case → {"Case":"Error","Fields":[...]}
/// - Option types: Some "value" → "value", None → null
/// - List/Array types: Native JSON arrays
/// - Tuples: JSON arrays
///
/// Usage:
/// - Document serialization: JsonSerializer.Serialize(doc, defaultOptions)
/// - Document deserialization: JsonSerializer.Deserialize&lt;T&gt;(json, defaultOptions)
/// - SQLite storage: Serialize documents before INSERT, deserialize on SELECT
/// </remarks>
///
/// <example>
/// <code>
/// open System.Text.Json
/// open FractalDb.Serialization
///
/// // Serialize F# record to JSON
/// type User = { Name: string; Age: int; Email: option&lt;string&gt; }
/// let user = { Name = "Alice"; Age = 30; Email = Some "alice@example.com" }
/// let json = JsonSerializer.Serialize(user, defaultOptions)
/// // Result: {"name":"Alice","age":30,"email":"alice@example.com"}
///
/// // Deserialize JSON to F# record
/// let json2 = """{"name":"Bob","age":25,"email":null}"""
/// let user2 = JsonSerializer.Deserialize&lt;User&gt;(json2, defaultOptions)
/// // Result: { Name = "Bob"; Age = 25; Email = None }
///
/// // Serialize discriminated union
/// type Status = Active | Inactive | Pending
/// let status = Active
/// let statusJson = JsonSerializer.Serialize(status, defaultOptions)
/// // Result: "Active"
///
/// // Serialize complex Document type
/// type Document&lt;'T&gt; = { Meta: DocumentMeta; Data: 'T }
/// let doc = { Meta = { Id = "123"; CreatedAt = 1234567890L; UpdatedAt = 1234567890L }
///             Data = user }
/// let docJson = JsonSerializer.Serialize(doc, defaultOptions)
/// // Result: {"meta":{"id":"123","createdAt":1234567890,"updatedAt":1234567890},"data":{...}}
/// </code>
/// </example>
let defaultOptions =
    let options = JsonSerializerOptions()
    options.Converters.Add(JsonFSharpConverter())
    options.PropertyNamingPolicy <- JsonNamingPolicy.CamelCase
    options

/// <summary>
/// Serializes an F# value to a JSON string.
/// </summary>
///
/// <param name="value">The value to serialize.</param>
///
/// <returns>A JSON string representation of the value.</returns>
///
/// <remarks>
/// Uses the configured defaultOptions with camelCase naming and F# type support.
/// Handles records, discriminated unions, options, lists, and tuples automatically.
/// Output is compact (not indented) for efficient storage.
/// </remarks>
///
/// <example>
/// <code>
/// type User = { Name: string; Age: int }
/// let user = { Name = "Alice"; Age = 30 }
/// let json = serialize user
/// // Result: """{"name":"Alice","age":30}"""
/// </code>
/// </example>
let serialize<'T> (value: 'T) : string =
    JsonSerializer.Serialize(value, defaultOptions)

/// <summary>
/// Deserializes a JSON string to an F# value.
/// </summary>
///
/// <param name="json">The JSON string to deserialize.</param>
///
/// <returns>The deserialized F# value of type 'T.</returns>
///
/// <remarks>
/// Uses the configured defaultOptions with camelCase naming and F# type support.
/// Automatically handles F# records, DUs, options (null → None), lists, and tuples.
/// Throws JsonException if the JSON is invalid or doesn't match the expected type.
/// </remarks>
///
/// <example>
/// <code>
/// type User = { Name: string; Age: int }
/// let json = """{"name":"Bob","age":25}"""
/// let user = deserialize&lt;User&gt; json
/// // Result: { Name = "Bob"; Age = 25 }
/// </code>
/// </example>
let deserialize<'T> (json: string) : 'T =
    JsonSerializer.Deserialize<'T>(json, defaultOptions)

/// <summary>
/// Serializes an F# value to a UTF-8 byte array (JSONB format for SQLite).
/// </summary>
///
/// <param name="value">The value to serialize.</param>
///
/// <returns>A UTF-8 encoded byte array containing the JSON representation.</returns>
///
/// <remarks>
/// More efficient than serialize for SQLite BLOB storage (avoids string allocation).
/// SQLite stores this as JSONB (binary JSON) which is compact and queryable.
/// Use with SQLite's json_extract() and jsonb() functions for efficient queries.
/// </remarks>
///
/// <example>
/// <code>
/// type User = { Name: string; Age: int }
/// let user = { Name = "Alice"; Age = 30 }
/// let bytes = serializeToBytes user
/// // Use with SQLite: INSERT INTO docs (data) VALUES (@bytes)
/// </code>
/// </example>
let serializeToBytes<'T> (value: 'T) : array<byte> =
    JsonSerializer.SerializeToUtf8Bytes(value, defaultOptions)

/// <summary>
/// Deserializes a UTF-8 byte array (JSONB format) to an F# value.
/// </summary>
///
/// <param name="bytes">The UTF-8 encoded JSON byte array.</param>
///
/// <returns>The deserialized F# value of type 'T.</returns>
///
/// <remarks>
/// More efficient than deserialize for SQLite BLOB retrieval (avoids string allocation).
/// Directly reads UTF-8 bytes without intermediate string conversion.
/// Use when reading JSONB data from SQLite BLOB columns.
/// Throws JsonException if the bytes are invalid JSON or don't match the expected type.
/// </remarks>
///
/// <example>
/// <code>
/// type User = { Name: string; Age: int }
/// // bytes retrieved from SQLite BLOB column
/// let user = deserializeFromBytes&lt;User&gt; bytes
/// // Result: { Name = "Alice"; Age = 30 }
/// </code>
/// </example>
let deserializeFromBytes<'T> (bytes: array<byte>) : 'T =
    JsonSerializer.Deserialize<'T>(System.ReadOnlySpan(bytes), defaultOptions)

