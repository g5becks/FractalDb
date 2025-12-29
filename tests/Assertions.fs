module FractalDb.Tests.Assertions

/// <summary>
/// Custom assertions for FractalDb testing using FsUnit-style ergonomics.
/// </summary>
///
/// <remarks>
/// This module provides assertion helpers for common FractalDb patterns including:
/// - Result<'T, FractalError> assertions (shouldBeOk, shouldBeError, etc.)
/// - Option<'T> assertions (shouldBeSome, shouldBeNone, etc.)
/// - String assertions (shouldNotBeEmpty)
///
/// These assertions provide clear, concise error messages when tests fail,
/// making it easier to diagnose issues.
/// </remarks>

open System
open FractalDb.Errors

// =============================================================================
// Result Assertions
// =============================================================================

/// <summary>
/// Asserts that a FractalResult is Ok (success).
/// </summary>
///
/// <param name="result">The FractalResult to check.</param>
///
/// <remarks>
/// Fails the test if the result is Error, displaying the error message.
/// Use this when you only care that the operation succeeded, not about the value.
/// </remarks>
///
/// <example>
/// <code>
/// let result = someOperation()
/// result |> shouldBeOk  // Passes if Ok, fails if Error
/// </code>
/// </example>
let shouldBeOk (result: FractalResult<'T>) : unit =
    match result with
    | Ok _ -> ()
    | Error (FractalError.NotFound id) ->
        failwith $"Expected Ok but got NotFound: {id}"
    | Error (FractalError.Validation (field, message)) ->
        match field with
        | Some f -> failwith $"Expected Ok but got Validation error for field '{f}': {message}"
        | None -> failwith $"Expected Ok but got Validation error: {message}"
    | Error (FractalError.UniqueConstraint (field, value)) ->
        failwith $"Expected Ok but got UniqueConstraint: field='{field}', value='{value}'"
    | Error (FractalError.Serialization message) ->
        failwith $"Expected Ok but got Serialization error: {message}"
    | Error (FractalError.Query (message, sql)) ->
        match sql with
        | Some s -> failwith $"Expected Ok but got Query error: {message}\nSQL: {s}"
        | None -> failwith $"Expected Ok but got Query error: {message}"
    | Error (FractalError.Connection message) ->
        failwith $"Expected Ok but got Connection error: {message}"
    | Error (FractalError.Transaction message) ->
        failwith $"Expected Ok but got Transaction error: {message}"
    | Error (FractalError.InvalidOperation message) ->
        failwith $"Expected Ok but got InvalidOperation: {message}"

/// <summary>
/// Asserts that a FractalResult is Ok and applies a function to the value.
/// </summary>
///
/// <param name="f">Function to apply to the Ok value (typically contains assertions).</param>
/// <param name="result">The FractalResult to check.</param>
///
/// <remarks>
/// Extracts the value from Ok and passes it to the provided function for further assertions.
/// Fails the test if the result is Error.
/// </remarks>
///
/// <example>
/// <code>
/// let result = findUser "123"
/// result |> shouldBeOkWith (fun user ->
///     user.Name |> should equal "Alice"
///     user.Age |> should equal 30
/// )
/// </code>
/// </example>
let shouldBeOkWith (f: 'T -> unit) (result: FractalResult<'T>) : unit =
    match result with
    | Ok value -> f value
    | Error (FractalError.NotFound id) ->
        failwith $"Expected Ok but got NotFound: {id}"
    | Error (FractalError.Validation (field, message)) ->
        match field with
        | Some f -> failwith $"Expected Ok but got Validation error for field '{f}': {message}"
        | None -> failwith $"Expected Ok but got Validation error: {message}"
    | Error (FractalError.UniqueConstraint (field, value)) ->
        failwith $"Expected Ok but got UniqueConstraint: field='{field}', value='{value}'"
    | Error (FractalError.Serialization message) ->
        failwith $"Expected Ok but got Serialization error: {message}"
    | Error (FractalError.Query (message, sql)) ->
        match sql with
        | Some s -> failwith $"Expected Ok but got Query error: {message}\nSQL: {s}"
        | None -> failwith $"Expected Ok but got Query error: {message}"
    | Error (FractalError.Connection message) ->
        failwith $"Expected Ok but got Connection error: {message}"
    | Error (FractalError.Transaction message) ->
        failwith $"Expected Ok but got Transaction error: {message}"
    | Error (FractalError.InvalidOperation message) ->
        failwith $"Expected Ok but got InvalidOperation: {message}"

/// <summary>
/// Asserts that a FractalResult is Error (failure).
/// </summary>
///
/// <param name="result">The FractalResult to check.</param>
///
/// <remarks>
/// Fails the test if the result is Ok.
/// Use this when you want to verify an operation failed, but don't care about the specific error.
/// </remarks>
///
/// <example>
/// <code>
/// let result = insertDuplicateUser()
/// result |> shouldBeError  // Passes if Error, fails if Ok
/// </code>
/// </example>
let shouldBeError (result: FractalResult<'T>) : unit =
    match result with
    | Error _ -> ()
    | Ok value -> failwith $"Expected Error but got Ok: {value}"

/// <summary>
/// Asserts that a FractalResult is a specific Error type.
/// </summary>
///
/// <param name="expected">The expected FractalError value.</param>
/// <param name="result">The FractalResult to check.</param>
///
/// <remarks>
/// Fails the test if:
/// - The result is Ok
/// - The result is Error but doesn't match the expected error
///
/// Useful for verifying exact error conditions.
/// </remarks>
///
/// <example>
/// <code>
/// let result = findUser "nonexistent"
/// result |> shouldBeErrorOf (FractalError.NotFound "nonexistent")
/// </code>
/// </example>
let shouldBeErrorOf (expected: FractalError) (result: FractalResult<'T>) : unit =
    match result with
    | Error actual when actual = expected -> ()
    | Error actual -> failwith $"Expected error {expected} but got {actual}"
    | Ok value -> failwith $"Expected Error but got Ok: {value}"

// =============================================================================
// Option Assertions
// =============================================================================

/// <summary>
/// Asserts that an Option is Some.
/// </summary>
///
/// <param name="opt">The Option to check.</param>
///
/// <remarks>
/// Fails the test if the option is None.
/// Use this when you only care that a value is present, not what the value is.
/// </remarks>
///
/// <example>
/// <code>
/// let maybeUser = tryFindUser "123"
/// maybeUser |> shouldBeSome  // Passes if Some, fails if None
/// </code>
/// </example>
let shouldBeSome (opt: option<'T>) : unit =
    match opt with
    | Some _ -> ()
    | None -> failwith "Expected Some but got None"

/// <summary>
/// Asserts that an Option is Some and applies a function to the value.
/// </summary>
///
/// <param name="f">Function to apply to the Some value (typically contains assertions).</param>
/// <param name="opt">The Option to check.</param>
///
/// <remarks>
/// Extracts the value from Some and passes it to the provided function for further assertions.
/// Fails the test if the option is None.
/// </remarks>
///
/// <example>
/// <code>
/// let maybeUser = tryFindUser "123"
/// maybeUser |> shouldBeSomeWith (fun user ->
///     user.Name |> should equal "Alice"
///     user.Active |> should equal true
/// )
/// </code>
/// </example>
let shouldBeSomeWith (f: 'T -> unit) (opt: option<'T>) : unit =
    match opt with
    | Some value -> f value
    | None -> failwith "Expected Some but got None"

/// <summary>
/// Asserts that an Option is None.
/// </summary>
///
/// <param name="opt">The Option to check.</param>
///
/// <remarks>
/// Fails the test if the option is Some, displaying the unexpected value.
/// Use this to verify that an operation correctly returned no value.
/// </remarks>
///
/// <example>
/// <code>
/// let maybeUser = tryFindUser "nonexistent"
/// maybeUser |> shouldBeNone  // Passes if None, fails if Some
/// </code>
/// </example>
let shouldBeNone (opt: option<'T>) : unit =
    match opt with
    | None -> ()
    | Some value -> failwith $"Expected None but got Some: {value}"

// =============================================================================
// String Assertions
// =============================================================================

/// <summary>
/// Asserts that a string is not null or empty.
/// </summary>
///
/// <param name="s">The string to check.</param>
///
/// <remarks>
/// Fails the test if the string is null, empty, or whitespace-only.
/// Useful for verifying that IDs, names, and other required strings are populated.
/// </remarks>
///
/// <example>
/// <code>
/// let doc = createDocument()
/// doc.Id |> shouldNotBeEmpty  // Passes if ID is non-empty
/// </code>
/// </example>
let shouldNotBeEmpty (s: string) : unit =
    if String.IsNullOrWhiteSpace s then
        failwith "Expected non-empty string but got null/empty/whitespace"
