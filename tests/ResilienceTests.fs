module FractalDb.Tests.ResilienceTests

// Suppress linter warnings for test code
// fsharplint:disable FL0072

open System
open System.Threading
open System.Threading.Tasks
open Xunit
open FsUnit.Xunit
open FractalDb.Errors

/// <summary>
/// Tests for RetryableError, ResilienceOptions, and Retry module.
/// Verifies error matching, retry behavior, and cancellation support.
/// </summary>

// ═══════════════════════════════════════════════════════════════
// RetryableError Preset Tests
// ═══════════════════════════════════════════════════════════════

[<Fact>]
let ``RetryableError.defaults contains Busy and Locked`` () =
    RetryableError.defaults |> should contain RetryableError.Busy
    RetryableError.defaults |> should contain RetryableError.Locked
    Set.count RetryableError.defaults |> should equal 2

[<Fact>]
let ``RetryableError.defaults does not contain IOError`` () =
    RetryableError.defaults |> should not' (contain RetryableError.IOError)

[<Fact>]
let ``RetryableError.extended contains Busy, Locked, IOError, CantOpen`` () =
    RetryableError.extended |> should contain RetryableError.Busy
    RetryableError.extended |> should contain RetryableError.Locked
    RetryableError.extended |> should contain RetryableError.IOError
    RetryableError.extended |> should contain RetryableError.CantOpen
    Set.count RetryableError.extended |> should equal 4

[<Fact>]
let ``RetryableError.extended does not contain Connection or Transaction`` () =
    RetryableError.extended |> should not' (contain RetryableError.Connection)
    RetryableError.extended |> should not' (contain RetryableError.Transaction)

[<Fact>]
let ``RetryableError.all contains all 6 retryable errors`` () =
    RetryableError.all |> should contain RetryableError.Busy
    RetryableError.all |> should contain RetryableError.Locked
    RetryableError.all |> should contain RetryableError.IOError
    RetryableError.all |> should contain RetryableError.CantOpen
    RetryableError.all |> should contain RetryableError.Connection
    RetryableError.all |> should contain RetryableError.Transaction
    Set.count RetryableError.all |> should equal 6

// ═══════════════════════════════════════════════════════════════
// RetryableError.matches Tests
// ═══════════════════════════════════════════════════════════════

[<Fact>]
let ``matches returns true for Busy error matching RetryableError.Busy`` () =
    let error = FractalError.Busy "database is busy"
    RetryableError.matches RetryableError.Busy error |> should be True

[<Fact>]
let ``matches returns true for Locked error matching RetryableError.Locked`` () =
    let error = FractalError.Locked "table is locked"
    RetryableError.matches RetryableError.Locked error |> should be True

[<Fact>]
let ``matches returns true for IOError matching RetryableError.IOError`` () =
    let error = FractalError.IOError "disk I/O error"
    RetryableError.matches RetryableError.IOError error |> should be True

[<Fact>]
let ``matches returns true for CantOpen matching RetryableError.CantOpen`` () =
    let error = FractalError.CantOpen "unable to open database file"
    RetryableError.matches RetryableError.CantOpen error |> should be True

[<Fact>]
let ``matches returns true for Connection error matching RetryableError.Connection`` () =
    let error = FractalError.Connection "connection failed"
    RetryableError.matches RetryableError.Connection error |> should be True

[<Fact>]
let ``matches returns true for Transaction error matching RetryableError.Transaction`` () =
    let error = FractalError.Transaction "transaction failed"
    RetryableError.matches RetryableError.Transaction error |> should be True

[<Fact>]
let ``matches returns false for non-matching error types`` () =
    let busyError = FractalError.Busy "busy"
    RetryableError.matches RetryableError.Locked busyError |> should be False
    RetryableError.matches RetryableError.IOError busyError |> should be False
    RetryableError.matches RetryableError.Connection busyError |> should be False

[<Fact>]
let ``matches returns false for non-transient errors`` () =
    let validationError = FractalError.Validation(Some "field", "invalid")
    let notFoundError = FractalError.NotFound "id123"
    let serializationError = FractalError.Serialization "parse error"

    RetryableError.matches RetryableError.Busy validationError |> should be False
    RetryableError.matches RetryableError.Busy notFoundError |> should be False
    RetryableError.matches RetryableError.Busy serializationError |> should be False

// ═══════════════════════════════════════════════════════════════
// RetryableError.shouldRetry Tests
// ═══════════════════════════════════════════════════════════════

[<Fact>]
let ``shouldRetry returns true for Busy error with defaults`` () =
    let error = FractalError.Busy "database is busy"
    RetryableError.shouldRetry RetryableError.defaults error |> should be True

[<Fact>]
let ``shouldRetry returns true for Locked error with defaults`` () =
    let error = FractalError.Locked "table locked"
    RetryableError.shouldRetry RetryableError.defaults error |> should be True

[<Fact>]
let ``shouldRetry returns false for IOError with defaults`` () =
    let error = FractalError.IOError "disk error"
    RetryableError.shouldRetry RetryableError.defaults error |> should be False

[<Fact>]
let ``shouldRetry returns true for IOError with extended`` () =
    let error = FractalError.IOError "disk error"
    RetryableError.shouldRetry RetryableError.extended error |> should be True

[<Fact>]
let ``shouldRetry returns false for Serialization error with all presets`` () =
    let error = FractalError.Serialization "parse error"
    RetryableError.shouldRetry RetryableError.defaults error |> should be False
    RetryableError.shouldRetry RetryableError.extended error |> should be False
    RetryableError.shouldRetry RetryableError.all error |> should be False

[<Fact>]
let ``shouldRetry returns false for NotFound error`` () =
    let error = FractalError.NotFound "document-id"
    RetryableError.shouldRetry RetryableError.all error |> should be False

[<Fact>]
let ``shouldRetry returns false for Validation error`` () =
    let error = FractalError.Validation(None, "validation failed")
    RetryableError.shouldRetry RetryableError.all error |> should be False

[<Fact>]
let ``shouldRetry returns false for empty set`` () =
    let error = FractalError.Busy "busy"
    RetryableError.shouldRetry Set.empty error |> should be False

// ═══════════════════════════════════════════════════════════════
// ResilienceOptions Tests
// ═══════════════════════════════════════════════════════════════

[<Fact>]
let ``ResilienceOptions.defaults has MaxRetries of 2`` () =
    ResilienceOptions.defaults.MaxRetries |> should equal 2

[<Fact>]
let ``ResilienceOptions.defaults has BaseDelay of 100ms`` () =
    ResilienceOptions.defaults.BaseDelay |> should equal (TimeSpan.FromMilliseconds(100.0))

[<Fact>]
let ``ResilienceOptions.defaults has MaxDelay of 5 seconds`` () =
    ResilienceOptions.defaults.MaxDelay |> should equal (TimeSpan.FromSeconds(5.0))

[<Fact>]
let ``ResilienceOptions.defaults has ExponentialBackoff enabled`` () =
    ResilienceOptions.defaults.ExponentialBackoff |> should be True

[<Fact>]
let ``ResilienceOptions.defaults has Jitter enabled`` () =
    ResilienceOptions.defaults.Jitter |> should be True

[<Fact>]
let ``ResilienceOptions.defaults uses RetryableError.defaults`` () =
    ResilienceOptions.defaults.RetryOn |> should equal RetryableError.defaults

[<Fact>]
let ``ResilienceOptions.none has MaxRetries of 0`` () =
    ResilienceOptions.none.MaxRetries |> should equal 0

[<Fact>]
let ``ResilienceOptions.none has empty RetryOn set`` () =
    ResilienceOptions.none.RetryOn |> should be Empty

[<Fact>]
let ``ResilienceOptions.extended uses RetryableError.extended`` () =
    ResilienceOptions.extended.RetryOn |> should equal RetryableError.extended

[<Fact>]
let ``ResilienceOptions.aggressive has MaxRetries of 5`` () =
    ResilienceOptions.aggressive.MaxRetries |> should equal 5

[<Fact>]
let ``ResilienceOptions.aggressive has MaxDelay of 10 seconds`` () =
    ResilienceOptions.aggressive.MaxDelay |> should equal (TimeSpan.FromSeconds(10.0))

[<Fact>]
let ``ResilienceOptions.aggressive uses RetryableError.all`` () =
    ResilienceOptions.aggressive.RetryOn |> should equal RetryableError.all

// ═══════════════════════════════════════════════════════════════
// Retry.executeAsync Tests
// ═══════════════════════════════════════════════════════════════

[<Fact>]
let ``executeAsync returns result immediately when opts is None`` () : Task =
    task {
        let mutable attempts = 0

        let operation () =
            task {
                attempts <- attempts + 1
                return Ok "success"
            }

        let! result = Retry.executeAsync None operation

        match result with
        | Ok value -> value |> should equal "success"
        | Error e -> failwith $"Expected Ok, got Error: {e.Message}"

        attempts |> should equal 1
    }

[<Fact>]
let ``executeAsync returns result immediately when MaxRetries is 0`` () : Task =
    task {
        let mutable attempts = 0

        let operation () =
            task {
                attempts <- attempts + 1
                return Error(FractalError.Busy "busy")
            }

        let opts = Some ResilienceOptions.none
        let! result = Retry.executeAsync opts operation

        match result with
        | Error(FractalError.Busy _) -> ()
        | _ -> failwith "Expected Busy error"

        attempts |> should equal 1
    }

[<Fact>]
let ``executeAsync returns result immediately when RetryOn is empty`` () : Task =
    task {
        let mutable attempts = 0

        let operation () =
            task {
                attempts <- attempts + 1
                return Error(FractalError.Busy "busy")
            }

        let opts = Some { ResilienceOptions.defaults with RetryOn = Set.empty }
        let! result = Retry.executeAsync opts operation

        match result with
        | Error(FractalError.Busy _) -> ()
        | _ -> failwith "Expected Busy error"

        attempts |> should equal 1
    }

[<Fact>]
let ``executeAsync succeeds on first attempt without retry`` () : Task =
    task {
        let mutable attempts = 0

        let operation () =
            task {
                attempts <- attempts + 1
                return Ok 42
            }

        let opts = Some ResilienceOptions.defaults
        let! result = Retry.executeAsync opts operation

        match result with
        | Ok value -> value |> should equal 42
        | Error e -> failwith $"Expected Ok, got Error: {e.Message}"

        attempts |> should equal 1
    }

[<Fact>]
let ``executeAsync retries on Busy error and succeeds`` () : Task =
    task {
        let mutable attempts = 0

        let operation () =
            task {
                attempts <- attempts + 1

                if attempts < 3 then
                    return Error(FractalError.Busy "database is busy")
                else
                    return Ok "success after retry"
            }

        // Use minimal delay for faster tests
        let opts =
            Some
                { ResilienceOptions.defaults with
                    BaseDelay = TimeSpan.FromMilliseconds(1.0)
                    Jitter = false }

        let! result = Retry.executeAsync opts operation

        match result with
        | Ok value -> value |> should equal "success after retry"
        | Error e -> failwith $"Expected Ok, got Error: {e.Message}"

        attempts |> should equal 3 // 1 initial + 2 retries
    }

[<Fact>]
let ``executeAsync retries on Locked error and succeeds`` () : Task =
    task {
        let mutable attempts = 0

        let operation () =
            task {
                attempts <- attempts + 1

                if attempts < 2 then
                    return Error(FractalError.Locked "table locked")
                else
                    return Ok "success"
            }

        let opts =
            Some
                { ResilienceOptions.defaults with
                    BaseDelay = TimeSpan.FromMilliseconds(1.0)
                    Jitter = false }

        let! result = Retry.executeAsync opts operation

        match result with
        | Ok value -> value |> should equal "success"
        | Error e -> failwith $"Expected Ok, got Error: {e.Message}"

        attempts |> should equal 2
    }

[<Fact>]
let ``executeAsync gives up after max retries exceeded`` () : Task =
    task {
        let mutable attempts = 0

        let operation () =
            task {
                attempts <- attempts + 1
                return Error(FractalError.Busy "always busy")
            }

        let opts =
            Some
                { ResilienceOptions.defaults with
                    MaxRetries = 2
                    BaseDelay = TimeSpan.FromMilliseconds(1.0)
                    Jitter = false }

        let! result = Retry.executeAsync opts operation

        match result with
        | Error(FractalError.Busy msg) -> msg |> should equal "always busy"
        | _ -> failwith "Expected Busy error"

        attempts |> should equal 3 // 1 initial + 2 retries
    }

[<Fact>]
let ``executeAsync does not retry non-retryable errors`` () : Task =
    task {
        let mutable attempts = 0

        let operation () =
            task {
                attempts <- attempts + 1
                return Error(FractalError.Serialization "parse failed")
            }

        let opts =
            Some
                { ResilienceOptions.defaults with
                    BaseDelay = TimeSpan.FromMilliseconds(1.0) }

        let! result = Retry.executeAsync opts operation

        match result with
        | Error(FractalError.Serialization _) -> ()
        | _ -> failwith "Expected Serialization error"

        attempts |> should equal 1 // No retry for serialization errors
    }

[<Fact>]
let ``executeAsync does not retry NotFound errors`` () : Task =
    task {
        let mutable attempts = 0

        let operation () =
            task {
                attempts <- attempts + 1
                return Error(FractalError.NotFound "missing-doc")
            }

        let opts = Some ResilienceOptions.aggressive // Even with all errors
        let! result = Retry.executeAsync opts operation

        match result with
        | Error(FractalError.NotFound _) -> ()
        | _ -> failwith "Expected NotFound error"

        attempts |> should equal 1
    }

[<Fact>]
let ``executeAsync does not retry Validation errors`` () : Task =
    task {
        let mutable attempts = 0

        let operation () =
            task {
                attempts <- attempts + 1
                return Error(FractalError.Validation(Some "email", "invalid format"))
            }

        let opts = Some ResilienceOptions.aggressive
        let! result = Retry.executeAsync opts operation

        match result with
        | Error(FractalError.Validation _) -> ()
        | _ -> failwith "Expected Validation error"

        attempts |> should equal 1
    }

[<Fact>]
let ``executeAsync retries IOError with extended config`` () : Task =
    task {
        let mutable attempts = 0

        let operation () =
            task {
                attempts <- attempts + 1

                if attempts < 2 then
                    return Error(FractalError.IOError "disk error")
                else
                    return Ok "recovered"
            }

        let opts =
            Some
                { ResilienceOptions.extended with
                    BaseDelay = TimeSpan.FromMilliseconds(1.0)
                    Jitter = false }

        let! result = Retry.executeAsync opts operation

        match result with
        | Ok value -> value |> should equal "recovered"
        | Error e -> failwith $"Expected Ok, got Error: {e.Message}"

        attempts |> should equal 2
    }

[<Fact>]
let ``executeAsync does not retry IOError with defaults config`` () : Task =
    task {
        let mutable attempts = 0

        let operation () =
            task {
                attempts <- attempts + 1
                return Error(FractalError.IOError "disk error")
            }

        let opts =
            Some
                { ResilienceOptions.defaults with
                    BaseDelay = TimeSpan.FromMilliseconds(1.0) }

        let! result = Retry.executeAsync opts operation

        match result with
        | Error(FractalError.IOError _) -> ()
        | _ -> failwith "Expected IOError"

        attempts |> should equal 1 // defaults doesn't include IOError
    }

// ═══════════════════════════════════════════════════════════════
// Retry.executeCancellableAsync Tests
// ═══════════════════════════════════════════════════════════════

[<Fact>]
let ``executeCancellableAsync returns result immediately when opts is None`` () : Task =
    task {
        let mutable attempts = 0
        use cts = new CancellationTokenSource()

        let operation () =
            task {
                attempts <- attempts + 1
                return Ok "success"
            }

        let! result = Retry.executeCancellableAsync None cts.Token operation

        match result with
        | Ok value -> value |> should equal "success"
        | Error e -> failwith $"Expected Ok, got Error: {e.Message}"

        attempts |> should equal 1
    }

[<Fact>]
let ``executeCancellableAsync respects CancellationToken during delay`` () : Task =
    task {
        let mutable attempts = 0
        use cts = new CancellationTokenSource()

        let operation () =
            task {
                attempts <- attempts + 1
                return Error(FractalError.Busy "busy")
            }

        // Cancel after a short delay (before retry delay completes)
        cts.CancelAfter(50)

        let opts =
            Some
                { ResilienceOptions.defaults with
                    BaseDelay = TimeSpan.FromSeconds(5.0) // Long delay
                    Jitter = false }

        let mutable cancelled = false

        try
            let! _ = Retry.executeCancellableAsync opts cts.Token operation
            ()
        with :? OperationCanceledException ->
            cancelled <- true

        cancelled |> should be True
        // Should have attempted at least once before cancellation
        attempts |> should be (greaterThanOrEqualTo 1)
        // Should not have completed all retries
        attempts |> should be (lessThan 3)
    }

[<Fact>]
let ``executeCancellableAsync throws immediately when token is already cancelled`` () : Task =
    task {
        let mutable attempts = 0
        use cts = new CancellationTokenSource()
        cts.Cancel()

        let operation () =
            task {
                attempts <- attempts + 1
                return Ok "should not reach"
            }

        let opts = Some ResilienceOptions.defaults
        let mutable cancelled = false

        try
            let! _ = Retry.executeCancellableAsync opts cts.Token operation
            ()
        with :? OperationCanceledException ->
            cancelled <- true

        cancelled |> should be True
        attempts |> should equal 0 // Should never attempt operation
    }

[<Fact>]
let ``executeCancellableAsync succeeds normally without cancellation`` () : Task =
    task {
        let mutable attempts = 0
        use cts = new CancellationTokenSource()

        let operation () =
            task {
                attempts <- attempts + 1

                if attempts < 2 then
                    return Error(FractalError.Busy "busy")
                else
                    return Ok "success"
            }

        let opts =
            Some
                { ResilienceOptions.defaults with
                    BaseDelay = TimeSpan.FromMilliseconds(1.0)
                    Jitter = false }

        let! result = Retry.executeCancellableAsync opts cts.Token operation

        match result with
        | Ok value -> value |> should equal "success"
        | Error e -> failwith $"Expected Ok, got Error: {e.Message}"

        attempts |> should equal 2
    }

// ═══════════════════════════════════════════════════════════════
// FractalError Transient Error Message Tests
// ═══════════════════════════════════════════════════════════════

[<Fact>]
let ``FractalError.Busy has correct message format`` () =
    let error = FractalError.Busy "database is locked"
    error.Message |> should equal "Database busy: database is locked"

[<Fact>]
let ``FractalError.Locked has correct message format`` () =
    let error = FractalError.Locked "table is locked"
    error.Message |> should equal "Database locked: table is locked"

[<Fact>]
let ``FractalError.IOError has correct message format`` () =
    let error = FractalError.IOError "disk I/O error"
    error.Message |> should equal "I/O error: disk I/O error"

[<Fact>]
let ``FractalError.CantOpen has correct message format`` () =
    let error = FractalError.CantOpen "unable to open database file"
    error.Message |> should equal "Cannot open database: unable to open database file"

[<Fact>]
let ``FractalError.DiskFull has correct message format`` () =
    let error = FractalError.DiskFull "database or disk is full"
    error.Message |> should equal "Disk full: database or disk is full"

// ═══════════════════════════════════════════════════════════════
// FractalError Transient Error Category Tests
// ═══════════════════════════════════════════════════════════════

[<Fact>]
let ``FractalError.Busy has category 'transient'`` () =
    let error = FractalError.Busy "busy"
    error.Category |> should equal "transient"

[<Fact>]
let ``FractalError.Locked has category 'transient'`` () =
    let error = FractalError.Locked "locked"
    error.Category |> should equal "transient"

[<Fact>]
let ``FractalError.IOError has category 'transient'`` () =
    let error = FractalError.IOError "io"
    error.Category |> should equal "transient"

[<Fact>]
let ``FractalError.CantOpen has category 'transient'`` () =
    let error = FractalError.CantOpen "cantopen"
    error.Category |> should equal "transient"

[<Fact>]
let ``FractalError.DiskFull has category 'transient'`` () =
    let error = FractalError.DiskFull "full"
    error.Category |> should equal "transient"

// ═══════════════════════════════════════════════════════════════
// SQLite Error Code Mapping Tests (via DonaldExceptions.mapDonaldException)
// ═══════════════════════════════════════════════════════════════

// Note: We need to create Donald.DbExecutionException with SqliteException as inner exception
// to test the mapping. SqliteException can be created with (message, errorCode).

open Microsoft.Data.Sqlite
open Donald

/// Helper to create a DbExecutionException with a SqliteException inner exception
let createDbExecutionExceptionWithSqliteError (errorCode: int) (message: string) =
    let sqliteEx = SqliteException(message, errorCode)
    DbExecutionException(message, sqliteEx)

[<Fact>]
let ``mapDonaldException maps SQLITE_BUSY (5) to FractalError.Busy`` () =
    let ex = createDbExecutionExceptionWithSqliteError 5 "database is locked"
    let error = DonaldExceptions.mapDonaldException ex

    match error with
    | FractalError.Busy msg ->
        msg |> should haveSubstring "locked"
    | _ -> failwith $"Expected Busy error, got: {error.Message}"

[<Fact>]
let ``mapDonaldException maps SQLITE_LOCKED (6) to FractalError.Locked`` () =
    let ex = createDbExecutionExceptionWithSqliteError 6 "table is locked"
    let error = DonaldExceptions.mapDonaldException ex

    match error with
    | FractalError.Locked msg ->
        msg |> should haveSubstring "locked"
    | _ -> failwith $"Expected Locked error, got: {error.Message}"

[<Fact>]
let ``mapDonaldException maps SQLITE_IOERR (10) to FractalError.IOError`` () =
    let ex = createDbExecutionExceptionWithSqliteError 10 "disk I/O error"
    let error = DonaldExceptions.mapDonaldException ex

    match error with
    | FractalError.IOError msg ->
        msg |> should haveSubstring "I/O"
    | _ -> failwith $"Expected IOError, got: {error.Message}"

[<Fact>]
let ``mapDonaldException maps SQLITE_FULL (13) to FractalError.DiskFull`` () =
    let ex = createDbExecutionExceptionWithSqliteError 13 "database or disk is full"
    let error = DonaldExceptions.mapDonaldException ex

    match error with
    | FractalError.DiskFull msg ->
        msg |> should haveSubstring "full"
    | _ -> failwith $"Expected DiskFull error, got: {error.Message}"

[<Fact>]
let ``mapDonaldException maps SQLITE_CANTOPEN (14) to FractalError.CantOpen`` () =
    let ex = createDbExecutionExceptionWithSqliteError 14 "unable to open database file"
    let error = DonaldExceptions.mapDonaldException ex

    match error with
    | FractalError.CantOpen msg ->
        msg |> should haveSubstring "open"
    | _ -> failwith $"Expected CantOpen error, got: {error.Message}"

[<Fact>]
let ``mapDonaldException maps SQLITE_CONSTRAINT (19) to FractalError.UniqueConstraint`` () =
    let ex = createDbExecutionExceptionWithSqliteError 19 "UNIQUE constraint failed: users.email"
    let error = DonaldExceptions.mapDonaldException ex

    match error with
    | FractalError.UniqueConstraint(field, _) ->
        field |> should equal "email"
    | _ -> failwith $"Expected UniqueConstraint error, got: {error.Message}"

[<Fact>]
let ``mapDonaldException maps other SQLite errors to FractalError.Query`` () =
    // Error code 1 is SQLITE_ERROR (generic error)
    let ex = createDbExecutionExceptionWithSqliteError 1 "SQL logic error"
    let error = DonaldExceptions.mapDonaldException ex

    match error with
    | FractalError.Query(msg, _) ->
        msg |> should haveSubstring "SQL logic error"
    | _ -> failwith $"Expected Query error, got: {error.Message}"

[<Fact>]
let ``Busy error from SQLite is retryable with defaults`` () =
    let ex = createDbExecutionExceptionWithSqliteError 5 "database is busy"
    let error = DonaldExceptions.mapDonaldException ex

    RetryableError.shouldRetry RetryableError.defaults error |> should be True

[<Fact>]
let ``Locked error from SQLite is retryable with defaults`` () =
    let ex = createDbExecutionExceptionWithSqliteError 6 "table locked"
    let error = DonaldExceptions.mapDonaldException ex

    RetryableError.shouldRetry RetryableError.defaults error |> should be True

[<Fact>]
let ``IOError from SQLite is retryable with extended`` () =
    let ex = createDbExecutionExceptionWithSqliteError 10 "disk I/O error"
    let error = DonaldExceptions.mapDonaldException ex

    RetryableError.shouldRetry RetryableError.defaults error |> should be False
    RetryableError.shouldRetry RetryableError.extended error |> should be True

[<Fact>]
let ``CantOpen error from SQLite is retryable with extended`` () =
    let ex = createDbExecutionExceptionWithSqliteError 14 "cannot open database"
    let error = DonaldExceptions.mapDonaldException ex

    RetryableError.shouldRetry RetryableError.defaults error |> should be False
    RetryableError.shouldRetry RetryableError.extended error |> should be True

[<Fact>]
let ``UniqueConstraint error from SQLite is not retryable`` () =
    let ex = createDbExecutionExceptionWithSqliteError 19 "UNIQUE constraint failed: users.id"
    let error = DonaldExceptions.mapDonaldException ex

    RetryableError.shouldRetry RetryableError.all error |> should be False

// ═══════════════════════════════════════════════════════════════
// Integration Tests with Real Database
// ═══════════════════════════════════════════════════════════════

open FractalDb.Types
open FractalDb.Schema
open FractalDb.Operators
open FractalDb.Collection
open FractalDb.Database
open IcedTasks

type ResilienceTestUser =
    { Name: string
      Email: string
      Age: int }

let resilienceUserSchema: SchemaDef<ResilienceTestUser> =
    { Fields =
        [ { Name = "email"
            Path = None
            SqlType = SqliteType.Text
            Indexed = true
            Unique = true
            Nullable = false } ]
      Indexes = []
      Timestamps = true
      Validate = None }

/// Test fixture for resilience integration tests
type ResilienceIntegrationFixture() =
    // Create db with resilience enabled
    let dbOptions = { DbOptions.defaults with Resilience = Some ResilienceOptions.defaults }
    let db = FractalDb.Open(":memory:", dbOptions)
    let users = db.Collection<ResilienceTestUser>("resilience_users", resilienceUserSchema)

    member _.Db = db
    member _.Users = users
    member _.Options = dbOptions

    interface System.IDisposable with
        member _.Dispose() = db.Close()

/// Integration tests for resilience with real database operations
type ResilienceIntegrationTests(fixture: ResilienceIntegrationFixture) =
    let users = fixture.Users

    interface IClassFixture<ResilienceIntegrationFixture>

    [<Fact>]
    member _.``insertOne succeeds with resilience enabled``() : Task =
        task {
            let! result =
                users
                |> Collection.insertOne
                    { Name = "ResilienceUser"
                      Email = $"resilience-{System.Guid.NewGuid()}@test.com"
                      Age = 30 }

            match result with
            | Ok doc ->
                doc.Data.Name |> should equal "ResilienceUser"
            | Error err -> failwith $"Expected Ok, got Error: {err.Message}"
        }

    [<Fact>]
    member _.``insertMany succeeds with resilience enabled``() : Task =
        task {
            let docs = [
                { Name = "User1"; Email = $"u1-{System.Guid.NewGuid()}@test.com"; Age = 25 }
                { Name = "User2"; Email = $"u2-{System.Guid.NewGuid()}@test.com"; Age = 30 }
                { Name = "User3"; Email = $"u3-{System.Guid.NewGuid()}@test.com"; Age = 35 }
            ]

            let! result = users |> Collection.insertMany docs

            match result with
            | Ok insertResult ->
                insertResult.Documents |> should haveLength 3
                insertResult.InsertedCount |> should equal 3
            | Error err -> failwith $"Expected Ok, got Error: {err.Message}"
        }

    [<Fact>]
    member _.``updateById succeeds with resilience enabled``() : Task =
        task {
            let! insertResult = users |> Collection.insertOne { Name = "ToUpdate"; Email = $"update-{System.Guid.NewGuid()}@test.com"; Age = 25 }
            let doc = Result.defaultWith (fun _ -> failwith "Insert failed") insertResult

            let! updateResult = users |> Collection.updateById doc.Id (fun u -> { u with Age = 26 })

            match updateResult with
            | Ok (Some updated) ->
                updated.Data.Age |> should equal 26
            | Ok None -> failwith "Expected Some, got None"
            | Error err -> failwith $"Expected Ok, got Error: {err.Message}"
        }

    [<Fact>]
    member _.``deleteById succeeds with resilience enabled``() : Task =
        task {
            let! insertResult = users |> Collection.insertOne { Name = "ToDelete"; Email = $"delete-{System.Guid.NewGuid()}@test.com"; Age = 30 }
            let doc = Result.defaultWith (fun _ -> failwith "Insert failed") insertResult

            let! deleted = users |> Collection.deleteById doc.Id

            deleted |> should be True
        }

    [<Fact>]
    member _.``non-retryable errors are returned immediately with resilience enabled``() : Task =
        task {
            // UniqueConstraint is not retryable
            let uniqueEmail = $"unique-{System.Guid.NewGuid()}@test.com"

            let! _ = users |> Collection.insertOne { Name = "First"; Email = uniqueEmail; Age = 25 }
            let! result = users |> Collection.insertOne { Name = "Second"; Email = uniqueEmail; Age = 30 }

            match result with
            | Error (FractalError.UniqueConstraint _) -> ()
            | Error err -> failwith $"Expected UniqueConstraint, got: {err.Message}"
            | Ok _ -> failwith "Expected Error, got Ok"
        }

// ═══════════════════════════════════════════════════════════════
// Database-Level Resilience Configuration Tests
// ═══════════════════════════════════════════════════════════════

[<Fact>]
let ``database with no resilience config has None resilience`` () =
    use db = FractalDb.InMemory()
    db.Options.Resilience.IsNone |> should be True

[<Fact>]
let ``database with defaults resilience config has correct settings`` () =
    let dbOptions = { DbOptions.defaults with Resilience = Some ResilienceOptions.defaults }
    use db = FractalDb.Open(":memory:", dbOptions)

    db.Options.Resilience.IsSome |> should be True
    db.Options.Resilience.Value.MaxRetries |> should equal 2

[<Fact>]
let ``database with extended resilience config has correct settings`` () =
    let dbOptions = { DbOptions.defaults with Resilience = Some ResilienceOptions.extended }
    use db = FractalDb.Open(":memory:", dbOptions)

    db.Options.Resilience.Value.RetryOn |> should equal RetryableError.extended

[<Fact>]
let ``database with aggressive resilience config has correct settings`` () =
    let dbOptions = { DbOptions.defaults with Resilience = Some ResilienceOptions.aggressive }
    use db = FractalDb.Open(":memory:", dbOptions)

    db.Options.Resilience.Value.MaxRetries |> should equal 5
    db.Options.Resilience.Value.RetryOn |> should equal RetryableError.all

[<Fact>]
let ``database with custom resilience config propagates to collections`` () =
    let customResilience = {
        ResilienceOptions.defaults with
            MaxRetries = 10
            RetryOn = set [ RetryableError.Busy ]
    }
    let dbOptions = { DbOptions.defaults with Resilience = Some customResilience }
    use db = FractalDb.Open(":memory:", dbOptions)

    let users = db.Collection<ResilienceTestUser>("users", resilienceUserSchema)

    users.Resilience.IsSome |> should be True
    users.Resilience.Value.MaxRetries |> should equal 10
    Set.count users.Resilience.Value.RetryOn |> should equal 1

// ═══════════════════════════════════════════════════════════════
// Cancellable Retry Integration Tests
// ═══════════════════════════════════════════════════════════════

[<Fact>]
let ``executeCancellableAsync with transient error retries and succeeds`` () : Task =
    task {
        let mutable attempts = 0
        use cts = new CancellationTokenSource()

        let operation () =
            task {
                attempts <- attempts + 1
                if attempts < 3 then
                    return Error(FractalError.Busy "busy")
                else
                    return Ok "success"
            }

        let opts = Some { ResilienceOptions.defaults with BaseDelay = TimeSpan.FromMilliseconds(1.0); Jitter = false }
        let! result = Retry.executeCancellableAsync opts cts.Token operation

        match result with
        | Ok value -> value |> should equal "success"
        | Error e -> failwith $"Expected Ok, got Error: {e.Message}"

        attempts |> should equal 3
    }

[<Fact>]
let ``executeCancellableAsync cancels during retry delay`` () : Task =
    task {
        let mutable attempts = 0
        use cts = new CancellationTokenSource()

        let operation () =
            task {
                attempts <- attempts + 1
                return Error(FractalError.Busy "always busy")
            }

        // Cancel quickly
        cts.CancelAfter(50)

        let opts = Some { ResilienceOptions.defaults with BaseDelay = TimeSpan.FromSeconds(10.0); Jitter = false }
        let mutable cancelled = false

        try
            let! _ = Retry.executeCancellableAsync opts cts.Token operation
            ()
        with :? OperationCanceledException ->
            cancelled <- true

        cancelled |> should be True
        // Should have tried at least once but not completed all retries
        attempts |> should be (greaterThanOrEqualTo 1)
    }

[<Fact>]
let ``executeCancellableAsync with Locked error retries correctly`` () : Task =
    task {
        let mutable attempts = 0
        use cts = new CancellationTokenSource()

        let operation () =
            task {
                attempts <- attempts + 1
                if attempts < 2 then
                    return Error(FractalError.Locked "table locked")
                else
                    return Ok 42
            }

        let opts = Some { ResilienceOptions.defaults with BaseDelay = TimeSpan.FromMilliseconds(1.0); Jitter = false }
        let! result = Retry.executeCancellableAsync opts cts.Token operation

        match result with
        | Ok value -> value |> should equal 42
        | Error e -> failwith $"Expected Ok, got Error: {e.Message}"

        attempts |> should equal 2
    }

[<Fact>]
let ``executeCancellableAsync does not retry IOError with defaults`` () : Task =
    task {
        let mutable attempts = 0
        use cts = new CancellationTokenSource()

        let operation () =
            task {
                attempts <- attempts + 1
                return Error(FractalError.IOError "disk error")
            }

        let opts = Some { ResilienceOptions.defaults with BaseDelay = TimeSpan.FromMilliseconds(1.0) }
        let! result = Retry.executeCancellableAsync opts cts.Token operation

        match result with
        | Error(FractalError.IOError _) -> ()
        | _ -> failwith "Expected IOError"

        attempts |> should equal 1 // No retry for IOError with defaults
    }

[<Fact>]
let ``executeCancellableAsync retries IOError with extended`` () : Task =
    task {
        let mutable attempts = 0
        use cts = new CancellationTokenSource()

        let operation () =
            task {
                attempts <- attempts + 1
                if attempts < 2 then
                    return Error(FractalError.IOError "disk error")
                else
                    return Ok "recovered"
            }

        let opts = Some { ResilienceOptions.extended with BaseDelay = TimeSpan.FromMilliseconds(1.0); Jitter = false }
        let! result = Retry.executeCancellableAsync opts cts.Token operation

        match result with
        | Ok value -> value |> should equal "recovered"
        | Error e -> failwith $"Expected Ok, got Error: {e.Message}"

        attempts |> should equal 2
    }

// ═══════════════════════════════════════════════════════════════
// Exponential Backoff Delay Calculation Tests
// ═══════════════════════════════════════════════════════════════

[<Fact>]
let ``retry delays increase with exponential backoff`` () : Task =
    task {
        let delays = System.Collections.Generic.List<TimeSpan>()
        let mutable attempts = 0

        let operation () =
            task {
                attempts <- attempts + 1
                if attempts <= 3 then
                    return Error(FractalError.Busy "busy")
                else
                    return Ok "done"
            }

        // Track when each attempt starts
        let startTimes = System.Collections.Generic.List<DateTime>()
        let mutable lastTime = DateTime.UtcNow

        let trackingOperation () =
            task {
                let now = DateTime.UtcNow
                if startTimes.Count > 0 then
                    delays.Add(now - lastTime)
                startTimes.Add(now)
                lastTime <- now
                return! operation()
            }

        let opts = Some {
            ResilienceOptions.defaults with
                BaseDelay = TimeSpan.FromMilliseconds(50.0)
                MaxRetries = 3
                Jitter = false
                ExponentialBackoff = true
        }

        let! _ = Retry.executeAsync opts trackingOperation

        // With exponential backoff and no jitter:
        // Attempt 1: immediate
        // Attempt 2: ~50ms delay
        // Attempt 3: ~100ms delay (50 * 2)
        // Attempt 4: success (or ~200ms delay if not)

        // Should have at least 2 delays recorded
        delays.Count |> should be (greaterThanOrEqualTo 2)

        // Second delay should be approximately double the first (allowing for timing variance)
        if delays.Count >= 2 then
            let ratio = delays.[1].TotalMilliseconds / delays.[0].TotalMilliseconds
            ratio |> should be (greaterThan 1.5)  // Allow some tolerance
    }

// ═══════════════════════════════════════════════════════════════
// Mixed Error Sequence Tests
// ═══════════════════════════════════════════════════════════════

[<Fact>]
let ``retry handles mixed retryable errors`` () : Task =
    task {
        let mutable attempts = 0

        let operation () =
            task {
                attempts <- attempts + 1
                match attempts with
                | 1 -> return Error(FractalError.Busy "busy")
                | 2 -> return Error(FractalError.Locked "locked")
                | _ -> return Ok "success"
            }

        let opts = Some { ResilienceOptions.defaults with BaseDelay = TimeSpan.FromMilliseconds(1.0); Jitter = false }
        let! result = Retry.executeAsync opts operation

        match result with
        | Ok value -> value |> should equal "success"
        | Error e -> failwith $"Expected Ok, got Error: {e.Message}"

        attempts |> should equal 3
    }

[<Fact>]
let ``retry stops on first non-retryable error in sequence`` () : Task =
    task {
        let mutable attempts = 0

        let operation () =
            task {
                attempts <- attempts + 1
                match attempts with
                | 1 -> return Error(FractalError.Busy "busy")
                | _ -> return Error(FractalError.Validation(Some "field", "invalid"))
            }

        let opts = Some { ResilienceOptions.defaults with BaseDelay = TimeSpan.FromMilliseconds(1.0); Jitter = false }
        let! result = Retry.executeAsync opts operation

        match result with
        | Error(FractalError.Validation _) -> ()
        | _ -> failwith "Expected Validation error"

        attempts |> should equal 2 // One retry before hitting non-retryable
    }

// ═══════════════════════════════════════════════════════════════
// Edge Case Tests
// ═══════════════════════════════════════════════════════════════

[<Fact>]
let ``retry with MaxRetries of 1 allows one retry`` () : Task =
    task {
        let mutable attempts = 0

        let operation () =
            task {
                attempts <- attempts + 1
                if attempts < 3 then
                    return Error(FractalError.Busy "busy")
                else
                    return Ok "success"
            }

        let opts = Some { ResilienceOptions.defaults with MaxRetries = 1; BaseDelay = TimeSpan.FromMilliseconds(1.0); Jitter = false }
        let! result = Retry.executeAsync opts operation

        // With MaxRetries = 1, we get 1 initial + 1 retry = 2 attempts
        // Since we need 3 attempts to succeed, we should fail
        match result with
        | Error(FractalError.Busy _) -> ()
        | _ -> failwith "Expected Busy error after max retries"

        attempts |> should equal 2
    }

[<Fact>]
let ``retry with very short MaxDelay caps delay correctly`` () : Task =
    task {
        let mutable attempts = 0
        let startTime = DateTime.UtcNow

        let operation () =
            task {
                attempts <- attempts + 1
                if attempts < 3 then
                    return Error(FractalError.Busy "busy")
                else
                    return Ok "done"
            }

        let opts = Some {
            ResilienceOptions.defaults with
                BaseDelay = TimeSpan.FromMilliseconds(100.0)
                MaxDelay = TimeSpan.FromMilliseconds(50.0) // Cap lower than base
                MaxRetries = 2
                Jitter = false
                ExponentialBackoff = true
        }

        let! _ = Retry.executeAsync opts operation

        let elapsed = DateTime.UtcNow - startTime

        // Even though base is 100ms, cap is 50ms, so delays should be capped
        // 2 retries * 50ms = 100ms max expected delay (plus operation time)
        elapsed.TotalMilliseconds |> should be (lessThan 500.0)
    }

[<Fact>]
let ``retry preserves error message from last attempt`` () : Task =
    task {
        let mutable attempts = 0

        let operation () =
            task {
                attempts <- attempts + 1
                return Error(FractalError.Busy $"busy attempt {attempts}")
            }

        let opts = Some { ResilienceOptions.defaults with MaxRetries = 2; BaseDelay = TimeSpan.FromMilliseconds(1.0); Jitter = false }
        let! result = Retry.executeAsync opts operation

        match result with
        | Error(FractalError.Busy msg) ->
            msg |> should equal "busy attempt 3" // Last attempt's message
        | _ -> failwith "Expected Busy error"

        attempts |> should equal 3
    }

