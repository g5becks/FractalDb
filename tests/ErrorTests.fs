module FractalDb.Tests.ErrorTests

open System
open Xunit
open FsUnit.Xunit
open Donald
open Microsoft.Data.Sqlite
open FractalDb.Errors

/// <summary>
/// Tests for FractalError types and Donald exception mapping.
/// Verifies error messages, categories, and exception-to-error conversions.
/// </summary>

// ═══════════════════════════════════════════════════════════════
// FractalError.Message Tests
// ═══════════════════════════════════════════════════════════════

[<Fact>]
let ``FractalError.Validation with field includes field name in message`` () =
    let error = FractalError.Validation(Some "email", "Invalid email format")

    error.Message
    |> should equal "Validation failed for 'email': Invalid email format"

[<Fact>]
let ``FractalError.Validation without field has generic message`` () =
    let error = FractalError.Validation(None, "Document validation failed")

    error.Message |> should equal "Validation failed: Document validation failed"

[<Fact>]
let ``FractalError.UniqueConstraint includes field and value in message`` () =
    let error = FractalError.UniqueConstraint("email", box "alice@example.com")

    error.Message.Contains("email") |> should be True
    error.Message.Contains("alice@example.com") |> should be True

[<Fact>]
let ``FractalError.Query with SQL includes SQL in message`` () =
    let sql = "SELECT * FROM users WHERE id = ?"
    let error = FractalError.Query("Syntax error", Some sql)

    error.Message.Contains("Syntax error") |> should be True
    error.Message.Contains(sql) |> should be True

[<Fact>]
let ``FractalError.Query without SQL has simple message`` () =
    let error = FractalError.Query("Query timeout", None)

    error.Message |> should equal "Query error: Query timeout"
    error.Message.Contains("SQL:") |> should be False

[<Fact>]
let ``FractalError.NotFound includes document ID in message`` () =
    let id = "01234567-89ab-cdef-0123-456789abcdef"
    let error = FractalError.NotFound id

    error.Message.Contains(id) |> should be True

[<Fact>]
let ``FractalError.Connection includes connection details`` () =
    let error = FractalError.Connection "Failed to open database"

    error.Message |> should equal "Connection error: Failed to open database"

[<Fact>]
let ``FractalError.Transaction includes transaction details`` () =
    let error = FractalError.Transaction "Cannot commit - already rolled back"

    error.Message
    |> should equal "Transaction error: Cannot commit - already rolled back"

[<Fact>]
let ``FractalError.Serialization includes serialization details`` () =
    let error =
        FractalError.Serialization "Cannot deserialize field 'age' - expected number"

    error.Message
    |> should equal "Serialization error: Cannot deserialize field 'age' - expected number"

[<Fact>]
let ``FractalError.InvalidOperation includes operation details`` () =
    let error = FractalError.InvalidOperation "Cannot delete from read-only collection"

    error.Message
    |> should equal "Invalid operation: Cannot delete from read-only collection"

// ═══════════════════════════════════════════════════════════════
// FractalError.Category Tests
// ═══════════════════════════════════════════════════════════════

[<Fact>]
let ``FractalError.Validation has category 'validation'`` () =
    let error = FractalError.Validation(None, "test")
    error.Category |> should equal "validation"

[<Fact>]
let ``FractalError.UniqueConstraint has category 'database'`` () =
    let error = FractalError.UniqueConstraint("field", box "value")
    error.Category |> should equal "database"

[<Fact>]
let ``FractalError.Query has category 'query'`` () =
    let error = FractalError.Query("test", None)
    error.Category |> should equal "query"

[<Fact>]
let ``FractalError.Connection has category 'database'`` () =
    let error = FractalError.Connection "test"
    error.Category |> should equal "database"

[<Fact>]
let ``FractalError.Transaction has category 'transaction'`` () =
    let error = FractalError.Transaction "test"
    error.Category |> should equal "transaction"

[<Fact>]
let ``FractalError.NotFound has category 'query'`` () =
    let error = FractalError.NotFound "id"
    error.Category |> should equal "query"

[<Fact>]
let ``FractalError.Serialization has category 'serialization'`` () =
    let error = FractalError.Serialization "test"
    error.Category |> should equal "serialization"

[<Fact>]
let ``FractalError.InvalidOperation has category 'operation'`` () =
    let error = FractalError.InvalidOperation "test"
    error.Category |> should equal "operation"

// ═══════════════════════════════════════════════════════════════
// DonaldExceptions.parseUniqueConstraintField Tests
// ═══════════════════════════════════════════════════════════════

[<Fact>]
let ``parseUniqueConstraintField extracts field from simple format`` () =
    let msg = "UNIQUE constraint failed: users.email"
    let field = DonaldExceptions.parseUniqueConstraintField msg

    field |> should equal "email"

[<Fact>]
let ``parseUniqueConstraintField extracts field from quoted format`` () =
    let msg = "UNIQUE constraint failed: 'users'.'_email'"
    let field = DonaldExceptions.parseUniqueConstraintField msg

    field |> should equal "email"

[<Fact>]
let ``parseUniqueConstraintField handles field with leading underscore`` () =
    let msg = "UNIQUE constraint failed: users._name"
    let field = DonaldExceptions.parseUniqueConstraintField msg

    field |> should equal "name"

[<Fact>]
let ``parseUniqueConstraintField returns 'unknown' for non-constraint message`` () =
    let msg = "Some other error"
    let field = DonaldExceptions.parseUniqueConstraintField msg

    field |> should equal "unknown"

// ═══════════════════════════════════════════════════════════════
// DonaldExceptions.mapDonaldException Tests
// ═══════════════════════════════════════════════════════════════
// Note: Donald exception types cannot be constructed directly in tests.
// These tests cover the catch-all case and parseUniqueConstraintField logic.
// Full Donald exception mapping is integration-tested via Collection operations.

[<Fact>]
let ``mapDonaldException maps other exceptions to Query error`` () =
    let ex = new InvalidOperationException("Unexpected error")

    let error = DonaldExceptions.mapDonaldException ex

    match error with
    | FractalError.Query(msg, sql) ->
        msg.Contains("Unexpected database error") |> should be True
        msg.Contains("Unexpected error") |> should be True
        sql |> should equal None
    | _ -> failwith "Expected Query error"

// ═══════════════════════════════════════════════════════════════
// FractalResult Helper Functions Tests
// ═══════════════════════════════════════════════════════════════

[<Fact>]
let ``FractalResult.ofOption returns Ok for Some`` () =
    let opt = Some 42
    let result = FractalResult.ofOption "test-id" opt

    match result with
    | Ok v -> v |> should equal 42
    | Error e -> failwith $"Expected Ok but got Error: {e.Message}"

[<Fact>]
let ``FractalResult.ofOption returns NotFound error for None`` () =
    let opt: option<int> = None
    let result = FractalResult.ofOption "test-id" opt

    match result with
    | Error(FractalError.NotFound id) -> id |> should equal "test-id"
    | _ -> failwith "Expected NotFound error"

[<Fact>]
let ``FractalResult.toOption returns Some for Ok`` () =
    let result: FractalResult<int> = Ok 42
    let opt = FractalResult.toOption result

    opt |> should equal (Some 42)

[<Fact>]
let ``FractalResult.toOption returns None for Error`` () =
    let result: FractalResult<int> = Error(FractalError.NotFound "id")
    let opt = FractalResult.toOption result

    opt |> should equal None

[<Fact>]
let ``FractalResult.sequence returns Ok list when all results are Ok`` () =
    let results = [ Ok 1; Ok 2; Ok 3 ]
    let sequenced = FractalResult.sequence results

    match sequenced with
    | Ok values -> values |> should equal [ 1; 2; 3 ]
    | Error e -> failwith $"Expected Ok but got Error: {e.Message}"

[<Fact>]
let ``FractalResult.sequence returns first Error when any result is Error`` () =
    let results = [ Ok 1; Error(FractalError.NotFound "id1"); Ok 3 ]
    let sequenced = FractalResult.sequence results

    match sequenced with
    | Error(FractalError.NotFound id) -> id |> should equal "id1"
    | _ -> failwith "Expected NotFound error"

// ═══════════════════════════════════════════════════════════════
// FractalResult.map Tests
// ═══════════════════════════════════════════════════════════════

[<Fact>]
let ``FractalResult.map transforms Ok value`` () =
    let result: FractalResult<int> = Ok 21
    let mapped = FractalResult.map (fun x -> x * 2) result

    match mapped with
    | Ok v -> v |> should equal 42
    | Error e -> failwith $"Expected Ok but got Error: {e.Message}"

[<Fact>]
let ``FractalResult.map preserves Error unchanged`` () =
    let result: FractalResult<int> = Error(FractalError.NotFound "id")
    let mapped = FractalResult.map (fun x -> x * 2) result

    match mapped with
    | Error(FractalError.NotFound id) -> id |> should equal "id"
    | _ -> failwith "Expected NotFound error"

// ═══════════════════════════════════════════════════════════════
// FractalResult.bind Tests
// ═══════════════════════════════════════════════════════════════

[<Fact>]
let ``FractalResult.bind chains successful operations`` () =
    let validate x =
        if x > 0 then
            Ok(x * 2)
        else
            Error(FractalError.Validation(Some "x", "Must be positive"))

    let result: FractalResult<int> = Ok 21
    let bound = FractalResult.bind validate result

    match bound with
    | Ok v -> v |> should equal 42
    | Error e -> failwith $"Expected Ok but got Error: {e.Message}"

[<Fact>]
let ``FractalResult.bind short-circuits on initial Error`` () =
    let validate x = Ok(x * 2) // Should never be called

    let result: FractalResult<int> = Error(FractalError.NotFound "id")
    let bound = FractalResult.bind validate result

    match bound with
    | Error(FractalError.NotFound id) -> id |> should equal "id"
    | _ -> failwith "Expected NotFound error"

[<Fact>]
let ``FractalResult.bind propagates Error from function`` () =
    let validate x =
        if x > 0 then
            Ok(x * 2)
        else
            Error(FractalError.Validation(Some "x", "Must be positive"))

    let result: FractalResult<int> = Ok(-5)
    let bound = FractalResult.bind validate result

    match bound with
    | Error(FractalError.Validation(field, msg)) ->
        field |> should equal (Some "x")
        msg |> should equal "Must be positive"
    | _ -> failwith "Expected Validation error"

// ═══════════════════════════════════════════════════════════════
// FractalResult.mapError Tests
// ═══════════════════════════════════════════════════════════════

[<Fact>]
let ``FractalResult.mapError transforms Error value`` () =
    let result: FractalResult<int> = Error(FractalError.NotFound "id")

    let mapped =
        FractalResult.mapError (fun _ -> FractalError.InvalidOperation "Wrapped") result

    match mapped with
    | Error(FractalError.InvalidOperation msg) -> msg |> should equal "Wrapped"
    | _ -> failwith "Expected InvalidOperation error"

[<Fact>]
let ``FractalResult.mapError preserves Ok unchanged`` () =
    let result: FractalResult<int> = Ok 42

    let mapped =
        FractalResult.mapError (fun _ -> FractalError.InvalidOperation "Wrapped") result

    match mapped with
    | Ok v -> v |> should equal 42
    | Error e -> failwith $"Expected Ok but got Error: {e.Message}"

// ═══════════════════════════════════════════════════════════════
// FractalResult.getOrRaise Tests
// ═══════════════════════════════════════════════════════════════

[<Fact>]
let ``FractalResult.getOrRaise extracts Ok value`` () =
    let result: FractalResult<int> = Ok 42
    let value = FractalResult.getOrRaise result

    value |> should equal 42

[<Fact>]
let ``FractalResult.getOrRaise throws on Error`` () =
    let result: FractalResult<int> = Error(FractalError.NotFound "test-id")

    let action = fun () -> FractalResult.getOrRaise result |> ignore

    action |> should throw typeof<System.Exception>

// ═══════════════════════════════════════════════════════════════
// FractalResult.traverse Tests
// ═══════════════════════════════════════════════════════════════

[<Fact>]
let ``FractalResult.traverse applies function to all elements when all succeed`` () =
    let validate x =
        if x >= 0 then
            Ok(x * 2)
        else
            Error(FractalError.Validation(None, $"Invalid: {x}"))

    let result = FractalResult.traverse validate [ 1; 2; 3 ]

    match result with
    | Ok values -> values |> should equal [ 2; 4; 6 ]
    | Error e -> failwith $"Expected Ok but got Error: {e.Message}"

[<Fact>]
let ``FractalResult.traverse stops at first Error`` () =
    let mutable callCount = 0

    let validate x =
        callCount <- callCount + 1

        if x >= 0 then
            Ok(x * 2)
        else
            Error(FractalError.Validation(None, $"Invalid: {x}"))

    let result = FractalResult.traverse validate [ 1; -2; 3; 4 ]

    match result with
    | Error(FractalError.Validation(_, msg)) ->
        msg |> should equal "Invalid: -2"
        callCount |> should equal 2 // Only called for 1 and -2, not 3 and 4
    | _ -> failwith "Expected Validation error"

[<Fact>]
let ``FractalResult.traverse returns empty list for empty input`` () =
    let validate x = Ok(x * 2)
    let result = FractalResult.traverse validate []

    match result with
    | Ok values -> values |> should be Empty
    | Error e -> failwith $"Expected Ok but got Error: {e.Message}"

// ═══════════════════════════════════════════════════════════════
// FractalResult.combine Tests
// ═══════════════════════════════════════════════════════════════

[<Fact>]
let ``FractalResult.combine returns tuple when both Ok`` () =
    let r1: FractalResult<int> = Ok 42
    let r2: FractalResult<string> = Ok "hello"

    let combined = FractalResult.combine r1 r2

    match combined with
    | Ok(v1, v2) ->
        v1 |> should equal 42
        v2 |> should equal "hello"
    | Error e -> failwith $"Expected Ok but got Error: {e.Message}"

[<Fact>]
let ``FractalResult.combine returns first Error when first fails`` () =
    let r1: FractalResult<int> = Error(FractalError.NotFound "id1")
    let r2: FractalResult<string> = Ok "hello"

    let combined = FractalResult.combine r1 r2

    match combined with
    | Error(FractalError.NotFound id) -> id |> should equal "id1"
    | _ -> failwith "Expected NotFound error"

[<Fact>]
let ``FractalResult.combine returns second Error when first succeeds and second fails`` () =
    let r1: FractalResult<int> = Ok 42
    let r2: FractalResult<string> = Error(FractalError.NotFound "id2")

    let combined = FractalResult.combine r1 r2

    match combined with
    | Error(FractalError.NotFound id) -> id |> should equal "id2"
    | _ -> failwith "Expected NotFound error"

[<Fact>]
let ``FractalResult.combine returns first Error when both fail`` () =
    let r1: FractalResult<int> = Error(FractalError.NotFound "id1")
    let r2: FractalResult<string> = Error(FractalError.NotFound "id2")

    let combined = FractalResult.combine r1 r2

    match combined with
    | Error(FractalError.NotFound id) -> id |> should equal "id1" // First error wins
    | _ -> failwith "Expected NotFound error"
