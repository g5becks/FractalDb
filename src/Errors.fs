module FractalDb.Errors

open System
open System.Threading.Tasks
open Donald
open Microsoft.Data.Sqlite

/// <summary>
/// Represents all possible errors that can occur during FractalDb operations.
/// </summary>
///
/// <remarks>
/// FractalDb uses a discriminated union for error handling, providing type-safe,
/// composable error handling without exceptions. All FractalDb operations return
/// <c>Result&lt;'T, FractalError&gt;</c>, allowing errors to be handled explicitly.
///
/// Error categories:
/// - Validation: Schema or data validation failures
/// - UniqueConstraint: Duplicate values in unique fields
/// - Query: SQL query construction or execution errors
/// - Connection: Database connection errors
/// - Transaction: Transaction management errors
/// - NotFound: Document not found errors
/// - Serialization: JSON serialization/deserialization errors
/// - InvalidOperation: Operation precondition failures
/// </remarks>
///
/// <example>
/// <code>
/// // Handling errors with pattern matching
/// match result with
/// | Ok doc -> printfn "Success: %A" doc
/// | Error (FractalError.NotFound id) -> printfn "Document %s not found" id
/// | Error (FractalError.Validation (Some field, msg)) -> printfn "Field %s: %s" field msg
/// | Error e -> printfn "Error: %s" e.Message
/// </code>
/// </example>
[<RequireQualifiedAccess>]
type FractalError =
    /// <summary>
    /// A validation error occurred during schema validation or data validation.
    /// </summary>
    /// <param name="field">The field that failed validation (None if validation is document-level).</param>
    /// <param name="message">A description of the validation failure.</param>
    /// <example>
    /// <code>
    /// FractalError.Validation (Some "email", "Email format is invalid")
    /// FractalError.Validation (None, "Document must have at least one field")
    /// </code>
    /// </example>
    | Validation of field: option<string> * message: string

    /// <summary>
    /// A unique constraint violation occurred when attempting to insert or update a document.
    /// </summary>
    /// <param name="field">The field with the unique constraint.</param>
    /// <param name="value">The duplicate value that caused the violation.</param>
    /// <example>
    /// <code>
    /// FractalError.UniqueConstraint ("email", box "alice@example.com")
    /// </code>
    /// </example>
    | UniqueConstraint of field: string * value: obj

    /// <summary>
    /// An error occurred during query construction or execution.
    /// </summary>
    /// <param name="message">A description of the query error.</param>
    /// <param name="sql">The SQL query that caused the error (if available).</param>
    /// <example>
    /// <code>
    /// FractalError.Query ("Invalid field reference", Some "SELECT * FROM users WHERE invalid_field = ?")
    /// FractalError.Query ("Query timeout", None)
    /// </code>
    /// </example>
    | Query of message: string * sql: option<string>

    /// <summary>
    /// A database connection error occurred.
    /// </summary>
    /// <param name="message">A description of the connection error.</param>
    /// <example>
    /// <code>
    /// FractalError.Connection "Failed to open database file"
    /// FractalError.Connection "Database is locked"
    /// </code>
    /// </example>
    | Connection of message: string

    /// <summary>
    /// A transaction management error occurred.
    /// </summary>
    /// <param name="message">A description of the transaction error.</param>
    /// <example>
    /// <code>
    /// FractalError.Transaction "Cannot commit - transaction already rolled back"
    /// FractalError.Transaction "Deadlock detected"
    /// </code>
    /// </example>
    | Transaction of message: string

    /// <summary>
    /// A document with the specified ID was not found.
    /// </summary>
    /// <param name="id">The ID of the document that was not found.</param>
    /// <example>
    /// <code>
    /// FractalError.NotFound "01234567-89ab-7def-8123-456789abcdef"
    /// </code>
    /// </example>
    | NotFound of id: string

    /// <summary>
    /// A JSON serialization or deserialization error occurred.
    /// </summary>
    /// <param name="message">A description of the serialization error.</param>
    /// <example>
    /// <code>
    /// FractalError.Serialization "Cannot deserialize field 'age' - expected number but got string"
    /// FractalError.Serialization "JSON document is too deeply nested"
    /// </code>
    /// </example>
    | Serialization of message: string

    /// <summary>
    /// An operation was attempted that violates a precondition or invariant.
    /// </summary>
    /// <param name="message">A description of why the operation is invalid.</param>
    /// <example>
    /// <code>
    /// FractalError.InvalidOperation "Cannot delete document from read-only collection"
    /// FractalError.InvalidOperation "Cannot update document with empty ID"
    /// </code>
    /// </example>
    | InvalidOperation of message: string

    /// <summary>
    /// Gets a human-readable error message describing the error.
    /// </summary>
    ///
    /// <returns>A formatted string describing the error and its context.</returns>
    ///
    /// <example>
    /// <code>
    /// let error = FractalError.NotFound "abc123"
    /// printfn "%s" error.Message
    /// // Output: "Document not found: abc123"
    ///
    /// let validationError = FractalError.Validation (Some "email", "Invalid format")
    /// printfn "%s" validationError.Message
    /// // Output: "Validation failed for 'email': Invalid format"
    /// </code>
    /// </example>
    member this.Message =
        match this with
        | Validation(Some field, msg) -> $"Validation failed for '{field}': {msg}"
        | Validation(None, msg) -> $"Validation failed: {msg}"
        | UniqueConstraint(field, value) -> $"Duplicate value for unique field '{field}': {value}"
        | Query(msg, Some sql) -> $"Query error: {msg}. SQL: {sql}"
        | Query(msg, None) -> $"Query error: {msg}"
        | Connection msg -> $"Connection error: {msg}"
        | Transaction msg -> $"Transaction error: {msg}"
        | NotFound id -> $"Document not found: {id}"
        | Serialization msg -> $"Serialization error: {msg}"
        | InvalidOperation msg -> $"Invalid operation: {msg}"

    /// <summary>
    /// Gets the category of the error for grouping and filtering.
    /// </summary>
    ///
    /// <returns>
    /// A string representing the error category:
    /// "validation", "database", "query", "transaction", "serialization", or "operation".
    /// </returns>
    ///
    /// <remarks>
    /// Error categories are useful for:
    /// - Logging and metrics aggregation
    /// - Error handling strategies (e.g., retry transient database errors)
    /// - User-facing error messages (e.g., group validation errors together)
    /// </remarks>
    ///
    /// <example>
    /// <code>
    /// let error = FractalError.NotFound "abc123"
    /// printfn "Category: %s" error.Category  // "query"
    ///
    /// let validationError = FractalError.Validation (None, "Invalid data")
    /// printfn "Category: %s" validationError.Category  // "validation"
    ///
    /// // Group errors by category
    /// let errors = [
    ///     FractalError.NotFound "id1"
    ///     FractalError.Validation (Some "email", "Invalid")
    ///     FractalError.Connection "Timeout"
    /// ]
    /// errors
    /// |> List.groupBy (fun e -> e.Category)
    /// |> List.iter (fun (cat, errs) -> printfn "%s: %d errors" cat (List.length errs))
    /// </code>
    /// </example>
    member this.Category =
        match this with
        | Validation _ -> "validation"
        | UniqueConstraint _ -> "database"
        | Query _ -> "query"
        | Connection _ -> "database"
        | Transaction _ -> "transaction"
        | NotFound _ -> "query"
        | Serialization _ -> "serialization"
        | InvalidOperation _ -> "operation"

/// <summary>
/// Type alias for Result with FractalError as the error type.
/// </summary>
///
/// <typeparam name="'T">The success value type.</typeparam>
///
/// <remarks>
/// FractalResult is the standard return type for all FractalDb operations that can fail.
/// It provides explicit, type-safe error handling without exceptions.
/// </remarks>
///
/// <example>
/// <code>
/// let findUser (id: string) : FractalResult&lt;User&gt; =
///     if id = "" then
///         Error (FractalError.InvalidOperation "ID cannot be empty")
///     else
///         Ok { Name = "Alice"; Email = "alice@example.com" }
/// </code>
/// </example>
type FractalResult<'T> = Result<'T, FractalError>

/// <summary>
/// Provides utility functions for working with FractalResult values.
/// </summary>
///
/// <remarks>
/// This module offers composable operations for Result types, including:
/// - Mapping and binding for success values
/// - Error transformations
/// - Conversions to/from Option types
/// - Utility functions for unwrapping results
/// </remarks>
module FractalResult =

    /// <summary>
    /// Maps the success value of a FractalResult using the provided function.
    /// </summary>
    ///
    /// <param name="f">The function to apply to the success value.</param>
    /// <param name="result">The FractalResult to map.</param>
    ///
    /// <returns>
    /// <c>Ok (f value)</c> if result is <c>Ok value</c>;
    /// otherwise returns the error unchanged.
    /// </returns>
    ///
    /// <example>
    /// <code>
    /// let result: FractalResult&lt;int&gt; = Ok 42
    /// let doubled = FractalResult.map (fun x -> x * 2) result
    /// // doubled = Ok 84
    ///
    /// let error: FractalResult&lt;int&gt; = Error (FractalError.NotFound "id")
    /// let stillError = FractalResult.map (fun x -> x * 2) error
    /// // stillError = Error (FractalError.NotFound "id")
    /// </code>
    /// </example>
    let map f result = Result.map f result

    /// <summary>
    /// Applies a function that returns a FractalResult to the success value.
    /// </summary>
    ///
    /// <param name="f">The function to apply that returns a FractalResult.</param>
    /// <param name="result">The FractalResult to bind.</param>
    ///
    /// <returns>
    /// The result of applying <paramref name="f"/> if result is <c>Ok</c>;
    /// otherwise returns the error unchanged.
    /// </returns>
    ///
    /// <remarks>
    /// Bind (also known as flatMap) is used to chain operations that can fail.
    /// If any operation in the chain returns an Error, subsequent operations are skipped.
    /// </remarks>
    ///
    /// <example>
    /// <code>
    /// let parseId (s: string) : FractalResult&lt;int&gt; =
    ///     match System.Int32.TryParse(s) with
    ///     | true, n -> Ok n
    ///     | false, _ -> Error (FractalError.Validation (Some "id", "Must be a number"))
    ///
    /// let validatePositive (n: int) : FractalResult&lt;int&gt; =
    ///     if n > 0 then Ok n
    ///     else Error (FractalError.Validation (Some "id", "Must be positive"))
    ///
    /// let result =
    ///     parseId "42"
    ///     |> FractalResult.bind validatePositive
    /// // result = Ok 42
    ///
    /// let invalid =
    ///     parseId "-5"
    ///     |> FractalResult.bind validatePositive
    /// // invalid = Error (Validation (Some "id", "Must be positive"))
    /// </code>
    /// </example>
    let bind f result = Result.bind f result

    /// <summary>
    /// Maps the error value of a FractalResult using the provided function.
    /// </summary>
    ///
    /// <param name="f">The function to apply to the error value.</param>
    /// <param name="result">The FractalResult to map.</param>
    ///
    /// <returns>
    /// <c>Error (f error)</c> if result is <c>Error error</c>;
    /// otherwise returns the success value unchanged.
    /// </returns>
    ///
    /// <example>
    /// <code>
    /// let result: FractalResult&lt;int&gt; = Error (FractalError.NotFound "id")
    ///
    /// let transformed =
    ///     result
    ///     |> FractalResult.mapError (fun _ -> FractalError.InvalidOperation "Wrapped error")
    /// // transformed = Error (FractalError.InvalidOperation "Wrapped error")
    /// </code>
    /// </example>
    let mapError f result = Result.mapError f result

    /// <summary>
    /// Converts an Option to a FractalResult, using NotFound error for None.
    /// </summary>
    ///
    /// <param name="id">The document ID to include in the NotFound error message.</param>
    /// <param name="opt">The Option value to convert.</param>
    ///
    /// <returns>
    /// <c>Ok value</c> if opt is <c>Some value</c>;
    /// <c>Error (FractalError.NotFound id)</c> if opt is <c>None</c>.
    /// </returns>
    ///
    /// <remarks>
    /// This function is useful for converting database query results (which return Option)
    /// into FractalResults with appropriate error context.
    /// </remarks>
    ///
    /// <example>
    /// <code>
    /// let maybeUser: option&lt;User&gt; = Some { Name = "Alice" }
    /// let result = FractalResult.ofOption "user-123" maybeUser
    /// // result = Ok { Name = "Alice" }
    ///
    /// let notFound: option&lt;User&gt; = None
    /// let error = FractalResult.ofOption "user-456" notFound
    /// // error = Error (FractalError.NotFound "user-456")
    /// </code>
    /// </example>
    let ofOption (id: string) (opt: option<'T>) : FractalResult<'T> =
        match opt with
        | Some v -> Ok v
        | None -> Error(FractalError.NotFound id)

    /// <summary>
    /// Converts a FractalResult to an Option, discarding error information.
    /// </summary>
    ///
    /// <param name="result">The FractalResult to convert.</param>
    ///
    /// <returns>
    /// <c>Some value</c> if result is <c>Ok value</c>;
    /// <c>None</c> if result is <c>Error</c>.
    /// </returns>
    ///
    /// <remarks>
    /// Use this function when you need to work with Option types but don't need
    /// the specific error information. Error details are lost in the conversion.
    /// </remarks>
    ///
    /// <example>
    /// <code>
    /// let result: FractalResult&lt;int&gt; = Ok 42
    /// let opt = FractalResult.toOption result
    /// // opt = Some 42
    ///
    /// let error: FractalResult&lt;int&gt; = Error (FractalError.NotFound "id")
    /// let none = FractalResult.toOption error
    /// // none = None
    /// </code>
    /// </example>
    let toOption result =
        match result with
        | Ok v -> Some v
        | Error _ -> None

    /// <summary>
    /// Extracts the success value or raises an exception for errors.
    /// </summary>
    ///
    /// <param name="result">The FractalResult to unwrap.</param>
    ///
    /// <returns>The success value if result is <c>Ok</c>.</returns>
    ///
    /// <exception cref="System.Exception">Thrown if result is <c>Error</c>.</exception>
    ///
    /// <remarks>
    /// <para>
    /// <strong>WARNING:</strong> This function defeats the purpose of explicit error handling
    /// and should be used sparingly. Prefer pattern matching or other combinators.
    /// </para>
    /// <para>
    /// Valid use cases:
    /// - Testing code where failures indicate test failures
    /// - Initialization code where errors are unrecoverable
    /// - Proof-of-concept or example code
    /// </para>
    /// </remarks>
    ///
    /// <example>
    /// <code>
    /// let result: FractalResult&lt;int&gt; = Ok 42
    /// let value = FractalResult.getOrRaise result
    /// // value = 42
    ///
    /// let error: FractalResult&lt;int&gt; = Error (FractalError.NotFound "id")
    /// let throws = FractalResult.getOrRaise error
    /// // Throws exception: "Document not found: id"
    /// </code>
    /// </example>
    let getOrRaise (result: FractalResult<'T>) : 'T =
        match result with
        | Ok v -> v
        | Error e -> failwith e.Message

    /// <summary>
    /// Applies a function that returns a FractalResult to each element of a list,
    /// collecting the results. Stops at the first error.
    /// </summary>
    ///
    /// <param name="f">The function to apply to each element.</param>
    /// <param name="xs">The list of elements to process.</param>
    ///
    /// <typeparam name="'T">The input element type.</typeparam>
    /// <typeparam name="'U">The output element type.</typeparam>
    ///
    /// <returns>
    /// <c>Ok [results]</c> if all applications succeed;
    /// <c>Error e</c> with the first error encountered.
    /// </returns>
    ///
    /// <remarks>
    /// <para>
    /// Traverse is a fundamental operation for working with collections of fallible operations.
    /// It processes elements left-to-right and short-circuits on the first error.
    /// </para>
    /// <para>
    /// This is useful for:
    /// - Validating multiple documents
    /// - Processing batch operations
    /// - Collecting results from multiple database queries
    /// </para>
    /// </remarks>
    ///
    /// <example>
    /// <code>
    /// let validateAge (age: int) : FractalResult&lt;int&gt; =
    ///     if age >= 0 && age <= 150 then Ok age
    ///     else Error (FractalError.Validation (Some "age", "Age must be 0-150"))
    ///
    /// // All valid
    /// let result1 = FractalResult.traverse validateAge [25; 30; 45]
    /// // result1 = Ok [25; 30; 45]
    ///
    /// // One invalid
    /// let result2 = FractalResult.traverse validateAge [25; -5; 45]
    /// // result2 = Error (FractalError.Validation (Some "age", "Age must be 0-150"))
    /// // Note: 45 is never validated (short-circuit)
    /// </code>
    /// </example>
    let traverse (f: 'T -> FractalResult<'U>) (xs: list<'T>) : FractalResult<list<'U>> =
        let rec loop acc =
            function
            | [] -> Ok(List.rev acc)
            | x :: rest ->
                match f x with
                | Ok y -> loop (y :: acc) rest
                | Error e -> Error e

        loop [] xs

    /// <summary>
    /// Converts a list of FractalResults into a FractalResult of a list.
    /// Stops at the first error.
    /// </summary>
    ///
    /// <param name="results">The list of FractalResults to sequence.</param>
    ///
    /// <returns>
    /// <c>Ok [values]</c> if all results are <c>Ok</c>;
    /// <c>Error e</c> with the first error encountered.
    /// </returns>
    ///
    /// <remarks>
    /// Sequence is implemented as <c>traverse id</c>, processing results left-to-right
    /// and collecting all success values or returning the first error.
    /// </remarks>
    ///
    /// <example>
    /// <code>
    /// let results = [
    ///     Ok 1
    ///     Ok 2
    ///     Ok 3
    /// ]
    /// let sequenced = FractalResult.sequence results
    /// // sequenced = Ok [1; 2; 3]
    ///
    /// let withError = [
    ///     Ok 1
    ///     Error (FractalError.NotFound "id")
    ///     Ok 3
    /// ]
    /// let failed = FractalResult.sequence withError
    /// // failed = Error (FractalError.NotFound "id")
    /// // Note: Ok 3 is never evaluated (short-circuit)
    /// </code>
    /// </example>
    let sequence (results: list<FractalResult<'T>>) : FractalResult<list<'T>> = traverse id results

    /// <summary>
    /// Combines two FractalResults into a tuple.
    /// </summary>
    ///
    /// <param name="r1">The first FractalResult.</param>
    /// <param name="r2">The second FractalResult.</param>
    ///
    /// <typeparam name="'T">The success type of the first result.</typeparam>
    /// <typeparam name="'U">The success type of the second result.</typeparam>
    ///
    /// <returns>
    /// <c>Ok (v1, v2)</c> if both results are <c>Ok</c>;
    /// otherwise returns the first <c>Error</c> encountered.
    /// </returns>
    ///
    /// <remarks>
    /// Combine is useful for aggregating multiple independent operations.
    /// It evaluates both results but returns the first error if either fails.
    /// For combining more than two results, consider using sequence or traverse.
    /// </remarks>
    ///
    /// <example>
    /// <code>
    /// let result1: FractalResult&lt;int&gt; = Ok 42
    /// let result2: FractalResult&lt;string&gt; = Ok "hello"
    /// let combined = FractalResult.combine result1 result2
    /// // combined = Ok (42, "hello")
    ///
    /// let error1: FractalResult&lt;int&gt; = Error (FractalError.NotFound "id1")
    /// let error2: FractalResult&lt;string&gt; = Error (FractalError.NotFound "id2")
    /// let failed = FractalResult.combine error1 error2
    /// // failed = Error (FractalError.NotFound "id1") - first error wins
    /// </code>
    /// </example>
    let combine (r1: FractalResult<'T>) (r2: FractalResult<'U>) : FractalResult<'T * 'U> =
        match r1, r2 with
        | Ok v1, Ok v2 -> Ok(v1, v2)
        | Error e, _
        | _, Error e -> Error e

/// <summary>
/// Provides utilities for converting Donald exceptions to FractalError.
/// </summary>
///
/// <remarks>
/// Donald is the ADO.NET library used by FractalDb for database access.
/// It throws four specific exception types with rich metadata:
/// - DbConnectionException: Connection failures with connection string
/// - DbExecutionException: Command execution failures with SQL statement
/// - DbReaderException: Field reading failures with field name
/// - DbTransactionException: Transaction failures with step information
///
/// This module maps these exceptions to appropriate FractalError cases,
/// preserving the rich context for debugging and error handling.
/// </remarks>
module DonaldExceptions =

    /// <summary>
    /// Extracts the field name from a SQLite UNIQUE constraint error message.
    /// </summary>
    ///
    /// <param name="errorMsg">The SQLite error message.</param>
    ///
    /// <returns>
    /// The field name that caused the constraint violation, or "unknown" if parsing fails.
    /// </returns>
    ///
    /// <remarks>
    /// SQLite UNIQUE constraint error messages have the format:
    /// - "UNIQUE constraint failed: table.field"
    /// - "UNIQUE constraint failed: 'table'.'_field'"
    ///
    /// This function extracts the field name, removing table prefix, underscores, and quotes.
    /// FractalDb uses leading underscores for internal JSON columns (e.g., _data).
    /// </remarks>
    ///
    /// <example>
    /// <code>
    /// let msg1 = "UNIQUE constraint failed: users.email"
    /// parseUniqueConstraintField msg1  // Returns "email"
    ///
    /// let msg2 = "UNIQUE constraint failed: 'users'.'_email'"
    /// parseUniqueConstraintField msg2  // Returns "email"
    ///
    /// let msg3 = "Some other error"
    /// parseUniqueConstraintField msg3  // Returns "unknown"
    /// </code>
    /// </example>
    let parseUniqueConstraintField (errorMsg: string) : string =
        if errorMsg.Contains("UNIQUE constraint failed:") then
            let parts = errorMsg.Split([| ':' |], 2)

            if parts.Length > 1 then
                let tableDotField = parts.[1].Trim()
                let fieldParts = tableDotField.Split('.')

                if fieldParts.Length > 1 then
                    // Remove leading underscore and any quotes from field name
                    fieldParts.[1].Trim([| '_'; '\''; ' ' |])
                else
                    "unknown"
            else
                "unknown"
        else
            "unknown"

    /// <summary>
    /// Converts Donald exceptions to FractalError with rich context.
    /// </summary>
    ///
    /// <param name="ex">The exception to convert.</param>
    ///
    /// <returns>
    /// A FractalError with appropriate type and context from the Donald exception metadata.
    /// </returns>
    ///
    /// <remarks>
    /// <para>
    /// This function handles all four Donald exception types and extracts their rich metadata:
    /// </para>
    /// <list type="bullet">
    /// <item>
    /// <term>DbConnectionException</term>
    /// <description>Maps to Connection error with connection string context.</description>
    /// </item>
    /// <item>
    /// <term>DbExecutionException</term>
    /// <description>
    /// Maps to Query error with SQL statement. Special handling for SQLite errors:
    /// - Error code 19 (UNIQUE constraint) → UniqueConstraint error
    /// - Error code 5 (BUSY/locked) → Connection error
    /// </description>
    /// </item>
    /// <item>
    /// <term>DbReaderException</term>
    /// <description>Maps to Serialization error with field name context.</description>
    /// </item>
    /// <item>
    /// <term>DbTransactionException</term>
    /// <description>Maps to Transaction error with step information.</description>
    /// </item>
    /// <item>
    /// <term>Other exceptions</term>
    /// <description>Maps to Query error with generic message.</description>
    /// </item>
    /// </list>
    /// </remarks>
    ///
    /// <example>
    /// <code>
    /// // Connection failure
    /// try
    ///     let conn = new SqliteConnection("invalid")
    ///     conn.Open()
    /// with
    /// | ex ->
    ///     let error = mapDonaldException ex
    ///     // error = FractalError.Connection "Failed to open: ... (connection: invalid)"
    ///
    /// // UNIQUE constraint violation
    /// try
    ///     // Execute INSERT that violates unique constraint
    ///     Db.exec connection "INSERT INTO users ..."
    /// with
    /// | ex ->
    ///     let error = mapDonaldException ex
    ///     // error = FractalError.UniqueConstraint ("email", box "duplicate@example.com")
    ///
    /// // Database locked
    /// try
    ///     // Try to write while another connection has exclusive lock
    ///     Db.exec connection "INSERT INTO users ..."
    /// with
    /// | ex ->
    ///     let error = mapDonaldException ex
    ///     // error = FractalError.Connection "Database is locked: database is locked"
    ///
    /// // Field reading error
    /// try
    ///     // Try to read field with wrong type
    ///     Db.query connection "SELECT * FROM users" (fun rd -> rd.ReadInt32 "name")
    /// with
    /// | ex ->
    ///     let error = mapDonaldException ex
    ///     // error = FractalError.Serialization "Failed to read field 'name': ..."
    ///
    /// // Transaction error
    /// try
    ///     // Transaction fails during commit
    ///     transaction.Commit()
    /// with
    /// | ex ->
    ///     let error = mapDonaldException ex
    ///     // error = FractalError.Transaction "Commit: Transaction has already been committed or rolled back"
    /// </code>
    /// </example>
    let mapDonaldException (ex: exn) : FractalError =
        match ex with
        | :? DbConnectionException as e ->
            let connStr = e.ConnectionString |> Option.defaultValue "<unknown>"
            FractalError.Connection $"{e.Message} (connection: {connStr})"

        | :? DbExecutionException as e ->
            let sql = e.Statement |> Option.defaultValue "<no statement>"

            // Check for specific SQLite errors in InnerException
            match e.InnerException with
            | :? SqliteException as sqlEx when sqlEx.SqliteErrorCode = 19 ->
                // SQLITE_CONSTRAINT (19) - UNIQUE constraint violation
                let field = parseUniqueConstraintField sqlEx.Message
                FractalError.UniqueConstraint(field, box "<value>")

            | :? SqliteException as sqlEx when sqlEx.SqliteErrorCode = 5 ->
                // SQLITE_BUSY (5) - Database is locked
                FractalError.Connection $"Database is locked: {sqlEx.Message}"

            | _ ->
                // Generic execution error
                FractalError.Query(e.Message, Some sql)

        | :? DbReaderException as e ->
            let field = e.FieldName |> Option.defaultValue "<unknown field>"
            FractalError.Serialization $"Failed to read field '{field}': {e.Message}"

        | :? DbTransactionException as e ->
            let step = e.Step |> Option.map string |> Option.defaultValue "Unknown"
            FractalError.Transaction $"{step}: {e.Message}"

        | _ ->
            // Catch-all for any other exceptions
            FractalError.Query($"Unexpected database error: {ex.Message}", None)

    /// <summary>
    /// Wraps a synchronous database operation with consistent error handling.
    /// </summary>
    ///
    /// <param name="operation">The database operation to execute.</param>
    ///
    /// <typeparam name="'T">The return type of the operation.</typeparam>
    ///
    /// <returns>
    /// <c>Ok value</c> if the operation succeeds;
    /// <c>Error fractalError</c> if a Donald exception occurs.
    /// </returns>
    ///
    /// <remarks>
    /// <para>
    /// This function provides a standard way to wrap database operations that use Donald,
    /// automatically converting all Donald exceptions to FractalError with rich context.
    /// </para>
    /// <para>
    /// Use this wrapper for any synchronous database operation that could throw Donald exceptions.
    /// It catches all four Donald exception types plus any unexpected exceptions.
    /// </para>
    /// <para>
    /// For async operations, use <see cref="tryDbOperationAsync"/> instead.
    /// </para>
    /// </remarks>
    ///
    /// <example>
    /// <code>
    /// open FractalDb.Errors.DonaldExceptions
    ///
    /// // Wrap a Donald query operation
    /// let getUserCount (conn: IDbConnection) : FractalResult&lt;int&gt; =
    ///     tryDbOperation (fun () ->
    ///         use cmd = conn.CreateCommand()
    ///         cmd.CommandText <- "SELECT COUNT(*) FROM users"
    ///         cmd.ExecuteScalar() :?> int64 |> int
    ///     )
    ///
    /// // Wrap a Donald execution
    /// let createTable (conn: IDbConnection) : FractalResult&lt;unit&gt; =
    ///     tryDbOperation (fun () ->
    ///         Db.exec conn "CREATE TABLE IF NOT EXISTS users (id TEXT PRIMARY KEY)"
    ///     )
    ///
    /// // Usage
    /// match getUserCount connection with
    /// | Ok count -> printfn "Found %d users" count
    /// | Error (FractalError.Connection msg) -> printfn "Connection failed: %s" msg
    /// | Error (FractalError.Query (msg, sql)) -> printfn "Query failed: %s" msg
    /// | Error e -> printfn "Error: %s" e.Message
    /// </code>
    /// </example>
    let tryDbOperation<'T> (operation: unit -> 'T) : FractalResult<'T> =
        try
            Ok(operation ())
        with
        | :? DbConnectionException
        | :? DbExecutionException
        | :? DbReaderException
        | :? DbTransactionException as ex -> Error(mapDonaldException ex)
        | ex -> Error(FractalError.Query(ex.Message, None))

    /// <summary>
    /// Wraps an asynchronous database operation with consistent error handling.
    /// </summary>
    ///
    /// <param name="operation">The async database operation to execute.</param>
    ///
    /// <typeparam name="'T">The return type of the operation.</typeparam>
    ///
    /// <returns>
    /// A Task containing <c>Ok value</c> if the operation succeeds;
    /// <c>Error fractalError</c> if a Donald exception occurs.
    /// </returns>
    ///
    /// <remarks>
    /// <para>
    /// This function provides a standard way to wrap async database operations that use Donald,
    /// automatically converting all Donald exceptions to FractalError with rich context.
    /// </para>
    /// <para>
    /// Use this wrapper for any asynchronous database operation that could throw Donald exceptions.
    /// It catches all four Donald exception types plus any unexpected exceptions.
    /// </para>
    /// <para>
    /// For synchronous operations, use <see cref="tryDbOperation"/> instead.
    /// </para>
    /// </remarks>
    ///
    /// <example>
    /// <code>
    /// open System.Threading.Tasks
    /// open FractalDb.Errors.DonaldExceptions
    ///
    /// // Wrap an async Donald query
    /// let getUserCountAsync (conn: IDbConnection) : Task&lt;FractalResult&lt;int&gt;&gt; =
    ///     tryDbOperationAsync (fun () ->
    ///         task {
    ///             use cmd = conn.CreateCommand()
    ///             cmd.CommandText <- "SELECT COUNT(*) FROM users"
    ///             let! result = cmd.ExecuteScalarAsync()
    ///             return result :?> int64 |> int
    ///         }
    ///     )
    ///
    /// // Wrap Collection operations
    /// let insertUserAsync (users: Collection&lt;User&gt;) (user: User)
    ///     : Task&lt;FractalResult&lt;Document&lt;User&gt;&gt;&gt; =
    ///     tryDbOperationAsync (fun () ->
    ///         users.InsertOne(user)
    ///     )
    ///
    /// // Usage with async workflows
    /// task {
    ///     match! getUserCountAsync connection with
    ///     | Ok count -> printfn "Found %d users" count
    ///     | Error (FractalError.Connection msg) -> printfn "Connection failed: %s" msg
    ///     | Error (FractalError.Query (msg, sql)) ->
    ///         printfn "Query failed: %s" msg
    ///         sql |> Option.iter (printfn "SQL: %s")
    ///     | Error e -> printfn "Error: %s" e.Message
    /// }
    ///
    /// // Chaining operations
    /// let insertAndCount (users: Collection&lt;User&gt;) (user: User) : Task&lt;FractalResult&lt;int&gt;&gt; =
    ///     task {
    ///         match! insertUserAsync users user with
    ///         | Error e -> return Error e
    ///         | Ok _ -> return! getUserCountAsync connection
    ///     }
    /// </code>
    /// </example>
    let tryDbOperationAsync<'T> (operation: unit -> Task<'T>) : Task<FractalResult<'T>> =
        task {
            try
                let! result = operation ()
                return Ok result
            with
            | :? DbConnectionException
            | :? DbExecutionException
            | :? DbReaderException
            | :? DbTransactionException as ex -> return Error(mapDonaldException ex)
            | ex -> return Error(FractalError.Query(ex.Message, None))
        }
