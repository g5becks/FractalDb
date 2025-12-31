# FractalDb Resilience Support Design

## Goal

Add automatic retry for transient database errors. Configuration at database or collection level. Users specify which `FractalError` types should trigger retries.

## Approach

1. **Extend `FractalError`** with specific transient error types (Busy, Locked, IOError)
2. **Add `RetryableError` type** - a well-defined set of errors that can trigger retries
3. **Configuration via `ResilienceOptions`** - specify which errors to retry and retry behavior
4. **Invisible to users** - operations work exactly the same, retry happens automatically

## Error Module Changes

### New Transient Error Types

```fsharp
[<RequireQualifiedAccess>]
type FractalError =
    // ... existing cases ...
    
    /// Database is busy (another connection has a lock) - SQLITE_BUSY (5)
    | Busy of message: string
    
    /// Table or row is locked - SQLITE_LOCKED (6)
    | Locked of message: string
    
    /// I/O error occurred - SQLITE_IOERR (10)
    | IOError of message: string
    
    /// Database file could not be opened - SQLITE_CANTOPEN (14)
    | CantOpen of message: string
    
    /// Database disk is full - SQLITE_FULL (13)
    | DiskFull of message: string
```

### RetryableError Type

```fsharp
/// Errors that can be configured to trigger automatic retry
[<RequireQualifiedAccess>]
type RetryableError =
    /// Database is busy (SQLITE_BUSY) - another connection holds a lock
    | Busy
    /// Table/row is locked (SQLITE_LOCKED) - write conflict
    | Locked  
    /// I/O error (SQLITE_IOERR) - transient disk/network issues
    | IOError
    /// Cannot open database (SQLITE_CANTOPEN) - file temporarily unavailable
    | CantOpen
    /// Connection error - transient connection issues
    | Connection
    /// Transaction error - deadlock or timeout
    | Transaction

module RetryableError =
    /// Default set of errors to retry (most common transient errors)
    let defaults = set [ Busy; Locked ]
    
    /// Extended set including I/O errors (for network drives, etc.)
    let extended = set [ Busy; Locked; IOError; CantOpen ]
    
    /// All retryable errors
    let all = set [ Busy; Locked; IOError; CantOpen; Connection; Transaction ]
    
    /// Check if a FractalError matches a RetryableError
    let matches (retryable: RetryableError) (error: FractalError) : bool =
        match retryable, error with
        | Busy, FractalError.Busy _ -> true
        | Locked, FractalError.Locked _ -> true
        | IOError, FractalError.IOError _ -> true
        | CantOpen, FractalError.CantOpen _ -> true
        | Connection, FractalError.Connection _ -> true
        | Transaction, FractalError.Transaction _ -> true
        | _ -> false
    
    /// Check if a FractalError should be retried given a set of retryable errors
    let shouldRetry (retryableSet: Set<RetryableError>) (error: FractalError) : bool =
        retryableSet |> Set.exists (fun r -> matches r error)
```

### Updated DonaldExceptions Mapping

```fsharp
let mapDonaldException (ex: exn) : FractalError =
    match ex with
    | :? DbConnectionException as e ->
        FractalError.Connection $"{e.Message}"

    | :? DbExecutionException as e ->
        let sql = e.Statement |> Option.defaultValue "<no statement>"
        
        match e.InnerException with
        | :? SqliteException as sqlEx ->
            match sqlEx.SqliteErrorCode with
            | 5  -> FractalError.Busy sqlEx.Message        // SQLITE_BUSY
            | 6  -> FractalError.Locked sqlEx.Message      // SQLITE_LOCKED
            | 10 -> FractalError.IOError sqlEx.Message     // SQLITE_IOERR
            | 13 -> FractalError.DiskFull sqlEx.Message    // SQLITE_FULL
            | 14 -> FractalError.CantOpen sqlEx.Message    // SQLITE_CANTOPEN
            | 19 -> 
                let field = parseUniqueConstraintField sqlEx.Message
                FractalError.UniqueConstraint(field, box "<value>")
            | _ -> FractalError.Query(e.Message, Some sql)
        | _ -> FractalError.Query(e.Message, Some sql)

    | :? DbReaderException as e ->
        let field = e.FieldName |> Option.defaultValue "<unknown>"
        FractalError.Serialization $"Failed to read field '{field}': {e.Message}"

    | :? DbTransactionException as e ->
        let step = e.Step |> Option.map string |> Option.defaultValue "Unknown"
        FractalError.Transaction $"{step}: {e.Message}"

    | _ -> FractalError.Query($"Unexpected error: {ex.Message}", None)
```

## Configuration Types

```fsharp
/// Resilience configuration for automatic retry
type ResilienceOptions = {
    /// Which errors should trigger a retry
    RetryOn: Set<RetryableError>
    
    /// Maximum retry attempts (default: 2)
    MaxRetries: int
    
    /// Initial delay between retries (default: 100ms)
    BaseDelay: TimeSpan
    
    /// Maximum delay cap (default: 5s)
    MaxDelay: TimeSpan
    
    /// Use exponential backoff (default: true)
    ExponentialBackoff: bool
    
    /// Add random jitter to delays (default: true)
    Jitter: bool
}

module ResilienceOptions =
    /// Default: retry Busy and Locked errors
    let defaults = {
        RetryOn = RetryableError.defaults
        MaxRetries = 2
        BaseDelay = TimeSpan.FromMilliseconds(100.0)
        MaxDelay = TimeSpan.FromSeconds(5.0)
        ExponentialBackoff = true
        Jitter = true
    }
    
    /// No retry
    let none = { defaults with MaxRetries = 0; RetryOn = Set.empty }
    
    /// Extended: also retry I/O errors
    let extended = { defaults with RetryOn = RetryableError.extended }
    
    /// Aggressive: retry all transient errors with more attempts
    let aggressive = { 
        defaults with 
            RetryOn = RetryableError.all
            MaxRetries = 5
            MaxDelay = TimeSpan.FromSeconds(10.0)
    }
```

## User Experience

```fsharp
// Database-level: all collections inherit these settings
let options = { DbOptions.defaults with Resilience = Some ResilienceOptions.defaults }
use db = FractalDb.Open("app.db", options)

// Custom retry configuration
let customResilience = {
    ResilienceOptions.defaults with
        RetryOn = set [ RetryableError.Busy; RetryableError.Locked; RetryableError.IOError ]
        MaxRetries = 5
}
let options2 = { DbOptions.defaults with Resilience = Some customResilience }
use db2 = FractalDb.Open("network.db", options2)

// Usage is identical - retry is invisible
let! result = users |> Collection.insertOne newUser
let! docs = users |> Collection.find (Query.eq "status" "active")
```

## Implementation

### Internal Retry Module

```fsharp
module internal Retry =
    open System
    open System.Threading.Tasks
    
    let private random = Random()
    
    let private calculateDelay (opts: ResilienceOptions) (attempt: int) =
        let baseMs = opts.BaseDelay.TotalMilliseconds
        
        let delayMs = 
            if opts.ExponentialBackoff then
                baseMs * (pown 2.0 attempt)
            else
                baseMs
        
        let withJitter =
            if opts.Jitter then
                let jitter = random.NextDouble() * 0.2 * delayMs
                delayMs + jitter
            else
                delayMs
        
        TimeSpan.FromMilliseconds(min withJitter opts.MaxDelay.TotalMilliseconds)
    
    let executeAsync (opts: ResilienceOptions option) (operation: unit -> Task<'T>) : Task<'T> =
        task {
            match opts with
            | None -> return! operation()
            | Some res when res.MaxRetries <= 0 || Set.isEmpty res.RetryOn -> 
                return! operation()
            | Some res ->
                let mutable attempt = 0
                let mutable lastError = Unchecked.defaultof<FractalError>
                let mutable result = Unchecked.defaultof<'T>
                let mutable success = false
                
                while not success && attempt <= res.MaxRetries do
                    try
                        result <- operation() |> Async.AwaitTask |> Async.RunSynchronously
                        success <- true
                    with ex ->
                        let error = DonaldExceptions.mapDonaldException ex
                        
                        if attempt < res.MaxRetries && RetryableError.shouldRetry res.RetryOn error then
                            lastError <- error
                            let delay = calculateDelay res attempt
                            do! Task.Delay(delay)
                            attempt <- attempt + 1
                        else
                            // Non-retryable error or max retries exceeded
                            return raise ex
                
                if success then
                    return result
                else
                    return raise (Exception(lastError.Message))
        }
```

### Integration

Wrap operations in `Collection.fs`:

```fsharp
let insertOne (doc: 'T) (collection: Collection<'T>) : Task<FractalResult<Document<'T>>> =
    Retry.executeAsync collection.Resilience (fun () ->
        // existing implementation
    )
```

## Backwards Compatibility

- Default `Resilience = None` - no behavior change
- New error types (`Busy`, `Locked`, etc.) are additions, not breaking changes
- Existing error handling code continues to work

## Summary

1. **Add transient error types** to `FractalError` (Busy, Locked, IOError, CantOpen, DiskFull)
2. **Add `RetryableError` type** with defaults, extended, and all presets
3. **Update `mapDonaldException`** to map SQLite error codes to new types  
4. **Add `ResilienceOptions`** to `DbOptions`
5. **Add internal `Retry` module** with exponential backoff
6. **Wrap Collection operations** with retry logic

## Testing Strategy

### Unit Tests for Retry Module

```fsharp
module ResilienceTests =
    open Expecto
    open FractalDb
    open FractalDb.Errors
    
    [<Tests>]
    let retryableErrorTests = testList "RetryableError" [
        test "defaults contains Busy and Locked" {
            Expect.contains RetryableError.defaults RetryableError.Busy "should contain Busy"
            Expect.contains RetryableError.defaults RetryableError.Locked "should contain Locked"
            Expect.equal (Set.count RetryableError.defaults) 2 "should have exactly 2 errors"
        }
        
        test "extended contains IO errors" {
            Expect.contains RetryableError.extended RetryableError.IOError "should contain IOError"
            Expect.contains RetryableError.extended RetryableError.CantOpen "should contain CantOpen"
        }
        
        test "shouldRetry returns true for matching error" {
            let error = FractalError.Busy "database is locked"
            Expect.isTrue (RetryableError.shouldRetry RetryableError.defaults error) "should retry Busy"
        }
        
        test "shouldRetry returns false for non-matching error" {
            let error = FractalError.Serialization "parse error"
            Expect.isFalse (RetryableError.shouldRetry RetryableError.defaults error) "should not retry Serialization"
        }
    ]
    
    [<Tests>]
    let resilienceOptionsTests = testList "ResilienceOptions" [
        test "defaults has MaxRetries = 2" {
            Expect.equal ResilienceOptions.defaults.MaxRetries 2 "default retries should be 2"
        }
        
        test "none disables retry" {
            Expect.equal ResilienceOptions.none.MaxRetries 0 "none should have 0 retries"
            Expect.isEmpty ResilienceOptions.none.RetryOn "none should have empty RetryOn"
        }
    ]
```

### Integration Tests with Simulated Failures

```fsharp
    [<Tests>]
    let retryIntegrationTests = testList "Retry Integration" [
        testTask "retries on Busy error and succeeds" {
            // Use a mock or test double that fails twice then succeeds
            let mutable attempts = 0
            let operation () = task {
                attempts <- attempts + 1
                if attempts < 3 then
                    raise (createBusyException "database is busy")
                return "success"
            }
            
            let opts = Some ResilienceOptions.defaults
            let! result = Retry.executeAsync opts operation
            
            Expect.equal result "success" "should eventually succeed"
            Expect.equal attempts 3 "should have tried 3 times (1 + 2 retries)"
        }
        
        testTask "gives up after max retries" {
            let mutable attempts = 0
            let operation () = task {
                attempts <- attempts + 1
                raise (createBusyException "always busy")
                return "never"
            }
            
            let opts = Some { ResilienceOptions.defaults with MaxRetries = 2 }
            
            let! result = Expect.throwsAsyncT<FractalException> (fun () ->
                Retry.executeAsync opts operation
            )
            
            Expect.equal attempts 3 "should have tried 3 times (1 + 2 retries)"
        }
        
        testTask "does not retry non-retryable errors" {
            let mutable attempts = 0
            let operation () = task {
                attempts <- attempts + 1
                raise (createSerializationException "parse failed")
                return "never"
            }
            
            let opts = Some ResilienceOptions.defaults
            
            let! _ = Expect.throwsAsyncT<FractalException> (fun () ->
                Retry.executeAsync opts operation
            )
            
            Expect.equal attempts 1 "should not retry serialization errors"
        }
        
        testTask "respects CancellationToken during retry delays" {
            use cts = new CancellationTokenSource()
            let mutable attempts = 0
            
            let operation () = cancellableTask {
                attempts <- attempts + 1
                raise (createBusyException "busy")
                return "never"
            }
            
            // Cancel after first attempt
            cts.CancelAfter(50)
            
            let opts = Some { ResilienceOptions.defaults with BaseDelay = TimeSpan.FromSeconds(1.0) }
            
            let! _ = Expect.throwsAsyncT<OperationCanceledException> (fun () ->
                Retry.executeCancellableAsync opts cts.Token operation
            )
            
            Expect.isLessThan attempts 3 "should cancel before all retries"
        }
    ]
```

### SQLite Error Mapping Tests

```fsharp
    [<Tests>]
    let errorMappingTests = testList "SQLite Error Mapping" [
        test "maps SQLITE_BUSY (5) to FractalError.Busy" {
            let sqliteEx = createSqliteException 5 "database is locked"
            let donaldEx = DbExecutionException("exec", sqliteEx, Some "SELECT 1")
            
            let error = mapDonaldException donaldEx
            
            match error with
            | FractalError.Busy msg -> Expect.stringContains msg "locked" "should preserve message"
            | _ -> failtest "should map to Busy"
        }
        
        test "maps SQLITE_LOCKED (6) to FractalError.Locked" {
            let sqliteEx = createSqliteException 6 "table locked"
            let donaldEx = DbExecutionException("exec", sqliteEx, Some "UPDATE x")
            
            let error = mapDonaldException donaldEx
            
            match error with
            | FractalError.Locked _ -> ()
            | _ -> failtest "should map to Locked"
        }
        
        test "maps SQLITE_IOERR (10) to FractalError.IOError" {
            let sqliteEx = createSqliteException 10 "disk I/O error"
            let donaldEx = DbExecutionException("exec", sqliteEx, Some "INSERT")
            
            let error = mapDonaldException donaldEx
            
            match error with
            | FractalError.IOError _ -> ()
            | _ -> failtest "should map to IOError"
        }
    ]
```

### Test Helpers

```fsharp
module TestHelpers =
    open Microsoft.Data.Sqlite
    
    /// Create a SqliteException with specific error code (for testing)
    let createSqliteException (errorCode: int) (message: string) =
        // SqliteException constructor requires specific setup
        // Use reflection or a wrapper approach
        let ex = SqliteException(message, errorCode)
        ex
    
    let createBusyException msg =
        let inner = createSqliteException 5 msg
        DbExecutionException("exec", inner, Some "SQL")
    
    let createLockedException msg =
        let inner = createSqliteException 6 msg
        DbExecutionException("exec", inner, Some "SQL")
```

### Real Database Tests (Optional - for CI)

```fsharp
    [<Tests>]
    let realDbRetryTests = testList "Real DB Retry" [
        testTask "handles concurrent write contention" {
            use db = FractalDb.Open(":memory:", { 
                DbOptions.defaults with Resilience = Some ResilienceOptions.defaults 
            })
            
            let users = db.Collection<User>("users")
            
            // Simulate concurrent operations that might cause BUSY
            let tasks = [
                for i in 1..10 do
                    users |> Collection.insertOne { Id = i; Name = $"User{i}" }
            ]
            
            let! results = Task.WhenAll(tasks)
            
            Expect.all results Result.isOk "all inserts should succeed with retry"
        }
    ]
```

