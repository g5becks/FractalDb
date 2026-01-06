# StrataDB Enhancement Proposal: AbortSignal & Retry Configuration

This document describes the design and implementation plan for adding two features learned from the F# port:

1. **AbortSignal Support** - Cancel operations via standard `AbortSignal`
2. **Retry Configuration** - Automatic retry with exponential backoff using `p-retry`

---

## 1. AbortSignal Support

### Overview

Every async collection method should accept an optional `AbortSignal` to allow callers to cancel long-running or unwanted operations. This aligns with the standard JavaScript cancellation pattern used by `fetch`, `EventTarget`, and Node.js APIs.

### Motivation

- **Resource cleanup**: Cancel in-flight operations when a component unmounts or request times out
- **Server-side integration**: Wire up to HTTP request signals (e.g., `req.signal` in Node.js)
- **User cancellation**: Allow users to abort slow queries
- **Timeout support**: Compose with `AbortSignal.timeout()` for deadline-based cancellation

### API Design

#### Collection Methods

All async `Collection` methods will accept an optional `signal` parameter:

```typescript
interface Collection<T extends Document> {
  // Read operations
  findById(id: string, options?: { signal?: AbortSignal }): Promise<T | null>

  find(
    filter: QueryFilter<T>,
    options?: QueryOptions<T> & { signal?: AbortSignal }
  ): Promise<readonly T[]>

  findOne(
    filter: string | QueryFilter<T>,
    options?: QueryOptions<T> & { signal?: AbortSignal }
  ): Promise<T | null>

  count(
    filter: QueryFilter<T>,
    options?: { signal?: AbortSignal }
  ): Promise<number>

  search(
    text: string,
    fields: readonly (keyof T | string)[],
    options?: QueryOptions<T> & { signal?: AbortSignal }
  ): Promise<readonly T[]>

  distinct<K extends keyof T>(
    field: K,
    filter?: QueryFilter<T>,
    options?: { signal?: AbortSignal }
  ): Promise<T[K][]>

  estimatedDocumentCount(options?: { signal?: AbortSignal }): Promise<number>

  // Single write operations
  insertOne(
    doc: DocumentInput<T>,
    options?: { signal?: AbortSignal }
  ): Promise<T>

  updateOne(
    filter: string | QueryFilter<T>,
    update: DocumentUpdate<T>,
    options?: { signal?: AbortSignal; upsert?: boolean }
  ): Promise<T | null>

  replaceOne(
    filter: string | QueryFilter<T>,
    doc: DocumentInput<T>,
    options?: { signal?: AbortSignal }
  ): Promise<T | null>

  deleteOne(
    filter: string | QueryFilter<T>,
    options?: { signal?: AbortSignal }
  ): Promise<boolean>

  // Atomic find-and-modify operations
  findOneAndUpdate(
    filter: string | QueryFilter<T>,
    update: DocumentUpdate<T>,
    options?: { signal?: AbortSignal; sort?: SortSpec<T>; returnDocument?: "before" | "after"; upsert?: boolean }
  ): Promise<T | null>

  findOneAndReplace(
    filter: string | QueryFilter<T>,
    replacement: DocumentInput<T>,
    options?: { signal?: AbortSignal; sort?: SortSpec<T>; returnDocument?: "before" | "after"; upsert?: boolean }
  ): Promise<T | null>

  findOneAndDelete(
    filter: string | QueryFilter<T>,
    options?: { signal?: AbortSignal; sort?: SortSpec<T> }
  ): Promise<T | null>

  // Batch write operations
  insertMany(
    docs: readonly DocumentInput<T>[],
    options?: { signal?: AbortSignal }
  ): Promise<InsertManyResult<T>>

  updateMany(
    filter: QueryFilter<T>,
    update: DocumentUpdate<T>,
    options?: { signal?: AbortSignal }
  ): Promise<UpdateResult>

  deleteMany(
    filter: QueryFilter<T>,
    options?: { signal?: AbortSignal }
  ): Promise<DeleteResult>

  // Utility operations
  drop(options?: { signal?: AbortSignal }): Promise<void>

  validate(doc: unknown, options?: { signal?: AbortSignal }): Promise<T>

  // Note: validateSync is synchronous and does not support signal/retry
}
```

**Methods NOT affected** (synchronous, no signal/retry needed):
- `validateSync` - Synchronous validation
```

#### Database Methods

```typescript
interface StrataDB {
  execute<R>(
    fn: (tx: Transaction) => R | Promise<R>,
    options?: { signal?: AbortSignal }
  ): Promise<R>
}
```

### New Error Type

Add a new error class for aborted operations:

```typescript
/**
 * Error thrown when an operation is aborted via AbortSignal.
 *
 * @remarks
 * This error is thrown when an AbortSignal is triggered during an operation.
 * The original abort reason is preserved in the `reason` property.
 *
 * When used with retry, p-retry will automatically stop retrying when this error is thrown.
 *
 * @example
 * ```typescript
 * const controller = new AbortController();
 * setTimeout(() => controller.abort(), 100);
 *
 * try {
 *   await users.find({}, { signal: controller.signal });
 * } catch (error) {
 *   if (error instanceof AbortedError) {
 *     console.log('Operation aborted:', error.reason);
 *   }
 * }
 * ```
 */
export class AbortedError extends StrataDBError {
  readonly category = "database" as const
  readonly code = "OPERATION_ABORTED"

  /** The abort reason from the AbortSignal */
  readonly reason?: unknown

  constructor(message: string, reason?: unknown) {
    super(message)
    this.reason = reason
  }
}
```

### Implementation Details

#### Signal Checking Helper

Create a utility function to check and throw on abort:

```typescript
// src/abort-utils.ts
import { AbortedError } from "./errors.js"

/**
 * Throws AbortedError if the signal is already aborted.
 * Should be called at the start of async operations.
 */
export function throwIfAborted(signal?: AbortSignal): void {
  if (signal?.aborted) {
    throw new AbortedError(
      signal.reason?.message ?? "Operation aborted",
      signal.reason
    )
  }
}

/**
 * Creates a promise that rejects when the signal is aborted.
 * Use with Promise.race() for operations that don't natively support signals.
 *
 * IMPORTANT: The returned cleanup function MUST be called to prevent memory leaks.
 */
export function createAbortPromise(signal?: AbortSignal): {
  promise: Promise<never>
  cleanup: () => void
} {
  if (!signal) {
    return {
      promise: new Promise(() => {}), // Never resolves
      cleanup: () => {}
    }
  }

  if (signal.aborted) {
    return {
      promise: Promise.reject(
        new AbortedError(
          signal.reason?.message ?? "Operation aborted",
          signal.reason
        )
      ),
      cleanup: () => {}
    }
  }

  let onAbort: (() => void) | undefined

  const promise = new Promise<never>((_, reject) => {
    onAbort = () => {
      reject(
        new AbortedError(
          signal.reason?.message ?? "Operation aborted",
          signal.reason
        )
      )
    }
    signal.addEventListener("abort", onAbort, { once: true })
  })

  return {
    promise,
    cleanup: () => {
      if (onAbort) {
        signal.removeEventListener("abort", onAbort)
      }
    }
  }
}
```

#### Integration in Collection Methods

In `sqlite-collection.ts`, integrate abort checking:

```typescript
async find(
  filter: QueryFilter<T>,
  options?: QueryOptions<T> & { signal?: AbortSignal }
): Promise<readonly T[]> {
  // Check abort at start
  throwIfAborted(options?.signal)

  const { sql, params } = this.translator.translate(filter, options)

  // Check again before expensive operation
  throwIfAborted(options?.signal)

  const stmt = this.db.prepare(sql)
  const rows = stmt.all(...params)

  // Check before deserialization of large result sets
  throwIfAborted(options?.signal)

  return rows.map(row => this.deserialize(row))
}
```

#### SQLite-Specific Considerations

Since Bun's SQLite is synchronous, true mid-query cancellation isn't possible. However:

1. **Pre-flight check**: Abort before starting the query
2. **Batch processing**: For large result sets, check between batches
3. **Transaction boundaries**: Check at transaction start and before each statement

---

## 2. Retry Configuration

### Overview

Add database-level and collection-level retry configuration for automatic retry of transient failures with exponential backoff. Uses the `p-retry` library for battle-tested retry logic.

### Motivation

- **Transient failure resilience**: SQLite `SQLITE_BUSY` and lock contention
- **Simplified application code**: Move retry logic out of business code
- **Configurable behavior**: Different retry strategies per collection
- **Observable retries**: Hooks for logging and monitoring

### Dependencies

```bash
bun add p-retry
```

The library provides:
- Exponential backoff with jitter
- Configurable retry counts and timeouts
- AbortSignal integration
- Custom retry predicates

### Type Re-exports

Re-export `RetryContext` from p-retry for users who need to type custom `shouldRetry` or `onFailedAttempt` callbacks:

```typescript
// src/retry-types.ts
export type { RetryContext } from "p-retry"
```

This allows users to import everything from stratadb:

```typescript
import { type RetryContext, type RetryOptions } from "stratadb"

const options: RetryOptions = {
  retries: 3,
  shouldRetry: (context: RetryContext) => {
    return context.retriesLeft > 0
  }
}
```

### API Design

#### Retry Options Type

```typescript
// src/retry-types.ts
import type { RetryContext } from "p-retry"

/**
 * Configuration for automatic retry of database operations.
 */
export interface RetryOptions {
  /**
   * Maximum number of retry attempts.
   * @default 0 (no retries)
   */
  readonly retries?: number

  /**
   * Exponential backoff factor.
   * @default 2
   */
  readonly factor?: number

  /**
   * Minimum delay between retries in milliseconds.
   * @default 1000
   */
  readonly minTimeout?: number

  /**
   * Maximum delay between retries in milliseconds.
   * @default 30000
   */
  readonly maxTimeout?: number

  /**
   * Add randomization to retry delays.
   * @default true
   */
  readonly randomize?: boolean

  /**
   * Maximum total time for all retries in milliseconds.
   * @default Infinity
   */
  readonly maxRetryTime?: number

  /**
   * Predicate to determine if an error should trigger a retry.
   * Return true to retry, false to abort immediately.
   * @default Retries on SQLITE_BUSY, SQLITE_LOCKED, and connection errors
   */
  readonly shouldRetry?: (context: RetryContext) => boolean | Promise<boolean>

  /**
   * Callback invoked on each failed attempt.
   * Useful for logging or metrics.
   */
  readonly onFailedAttempt?: (context: RetryContext) => void | Promise<void>

  /**
   * Decide if this failure should consume a retry from the retries budget.
   * Useful for rate limiting scenarios where you don't want to decrement retries.
   * @default All failures consume a retry
   */
  readonly shouldConsumeRetry?: (context: RetryContext) => boolean | Promise<boolean>
}
```

#### Database Options Extension

```typescript
// Updated database-types.ts
export interface DatabaseOptions {
  readonly database: string | SQLiteDatabase
  readonly idGenerator?: () => string
  readonly onClose?: () => void
  readonly enableCache?: boolean

  /**
   * Default retry configuration for all collections.
   * Can be overridden at the collection level.
   */
  readonly retry?: RetryOptions
}
```

#### Collection Options Extension

```typescript
export interface CollectionOptions {
  readonly enableCache?: boolean

  /**
   * Retry configuration for this collection.
   * Overrides database-level retry settings.
   * Set to `false` to disable retries for this collection.
   */
  readonly retry?: RetryOptions | false
}
```

#### Per-Operation Override

All async collection methods accept both `signal` and `retry` options:

```typescript
interface OperationOptions {
  signal?: AbortSignal
  retry?: RetryOptions | false
}

// Methods that support operation-level retry override:
// - findById, find, findOne, count, search, distinct, estimatedDocumentCount
// - insertOne, updateOne, replaceOne, deleteOne
// - findOneAndUpdate, findOneAndReplace, findOneAndDelete
// - insertMany, updateMany, deleteMany
// - drop, validate

// Example: disable retry for a specific operation
await users.insertOne(data, { retry: false })

// Example: custom retry for a specific operation
await users.find({}, {
  retry: {
    retries: 5,
    shouldRetry: ({ error }) => error.code === "SQLITE_BUSY"
  }
})

// Example: combined signal and retry
await users.find({}, {
  signal: AbortSignal.timeout(5000),
  retry: { retries: 3 }
})
```

### Default Retry Predicate

```typescript
// src/retry-utils.ts
import type { RetryContext } from "p-retry"
import {
  ConnectionError,
  DatabaseError,
  TransactionError
} from "./errors.js"

/**
 * SQLite error codes that indicate transient failures worth retrying.
 */
const RETRYABLE_SQLITE_CODES = new Set([
  5,   // SQLITE_BUSY - database is locked
  6,   // SQLITE_LOCKED - table is locked
  7,   // SQLITE_NOMEM - out of memory (might recover)
  10,  // SQLITE_IOERR - I/O error (might be transient)
])

/**
 * Default predicate for determining if an error should be retried.
 */
export function defaultShouldRetry(context: RetryContext): boolean {
  const { error } = context

  // Never retry validation errors - they won't succeed on retry
  if (error instanceof ValidationError || error instanceof SchemaValidationError) {
    return false
  }

  // Never retry unique constraint violations
  if (error instanceof UniqueConstraintError) {
    return false
  }

  // Never retry other constraint errors
  if (error instanceof ConstraintError) {
    return false
  }

  // Retry connection errors (might be transient)
  if (error instanceof ConnectionError) {
    return true
  }

  // Retry specific SQLite codes
  if (error instanceof DatabaseError && error.sqliteCode !== undefined) {
    return RETRYABLE_SQLITE_CODES.has(error.sqliteCode)
  }

  // Retry transaction errors (might be deadlock)
  if (error instanceof TransactionError) {
    return true
  }

  return false
}
```

### Implementation Details

#### Retry Wrapper

```typescript
// src/retry-utils.ts
import pRetry from "p-retry"

export interface RetryableOptions extends RetryOptions {
  signal?: AbortSignal
}

/**
 * Wraps an async operation with retry logic.
 */
export async function withRetry<T>(
  operation: (attemptNumber: number) => Promise<T>,
  options: RetryableOptions = {}
): Promise<T> {
  const {
    retries = 0,
    factor = 2,
    minTimeout = 1000,
    maxTimeout = 30000,
    randomize = true,
    maxRetryTime,
    shouldRetry = defaultShouldRetry,
    shouldConsumeRetry,
    onFailedAttempt,
    signal,
  } = options

  // If no retries configured, just run the operation
  if (retries === 0) {
    return operation(1)
  }

  return pRetry(operation, {
    retries,
    factor,
    minTimeout,
    maxTimeout,
    randomize,
    maxRetryTime,
    signal,
    shouldRetry,
    shouldConsumeRetry,
    onFailedAttempt,
  })
}
```

#### Merging Retry Options

```typescript
// src/retry-utils.ts

/**
 * Merges retry options from database, collection, and operation levels.
 * Returns undefined if retry is explicitly disabled.
 */
export function mergeRetryOptions(
  databaseOptions?: RetryOptions,
  collectionOptions?: RetryOptions | false,
  operationOptions?: RetryOptions | false
): RetryOptions | undefined {
  // Operation-level false disables retry
  if (operationOptions === false) {
    return undefined
  }

  // Collection-level false disables retry
  if (collectionOptions === false) {
    return undefined
  }

  // Merge with later options taking precedence
  return {
    ...databaseOptions,
    ...collectionOptions,
    ...operationOptions,
  }
}
```

#### Integration in SQLiteCollection

```typescript
// sqlite-collection.ts
class SQLiteCollection<T extends Document> implements Collection<T> {
  private readonly retryOptions?: RetryOptions

  constructor(
    private readonly db: SQLiteDatabase,
    private readonly name: string,
    private readonly schema: SchemaDefinition<T>,
    private readonly collectionOptions?: CollectionOptions,
    private readonly databaseRetryOptions?: RetryOptions,
  ) {
    this.retryOptions = mergeRetryOptions(
      databaseRetryOptions,
      collectionOptions?.retry
    )
  }

  async find(
    filter: QueryFilter<T>,
    options?: QueryOptions<T> & { signal?: AbortSignal; retry?: RetryOptions | false }
  ): Promise<readonly T[]> {
    const effectiveRetry = mergeRetryOptions(
      this.retryOptions,
      undefined,
      options?.retry
    )

    return withRetry(
      async (attemptNumber) => {
        throwIfAborted(options?.signal)

        const { sql, params } = this.translator.translate(filter, options)
        const stmt = this.db.prepare(sql)
        const rows = stmt.all(...params)

        return rows.map(row => this.deserialize(row))
      },
      { ...effectiveRetry, signal: options?.signal }
    )
  }

  // ... other methods follow the same pattern
}
```

---

## 3. Files to Modify

### New Files

| File | Description |
|------|-------------|
| `src/abort-utils.ts` | AbortSignal utilities (`throwIfAborted`, `createAbortPromise`) |
| `src/retry-types.ts` | `RetryOptions` interface and related types |
| `src/retry-utils.ts` | `withRetry` wrapper, `mergeRetryOptions`, `defaultShouldRetry` |

### Modified Files

| File | Changes |
|------|---------|
| `src/errors.ts` | Add `AbortedError` class |
| `src/database-types.ts` | Add `retry?: RetryOptions` to `DatabaseOptions` and `CollectionOptions` |
| `src/collection-types.ts` | Add `signal?: AbortSignal` and `retry?: RetryOptions \| false` to all method options |
| `src/sqlite-collection.ts` | Integrate abort checking and retry wrapper in all async methods |
| `src/stratadb.ts` | Pass retry options to collections, add signal to `execute()` |
| `src/index.ts` | Export new types (`RetryOptions`, `RetryContext`, `AbortedError`, `throwIfAborted`, `defaultShouldRetry`) |
| `package.json` | Add `p-retry` dependency |

---

## 4. Implementation Order

### Phase 1: AbortSignal Support

1. Create `src/abort-utils.ts` with signal checking utilities
2. Add `AbortedError` to `src/errors.ts`
3. Update `src/collection-types.ts` with signal options on all methods
4. Update `src/sqlite-collection.ts` to check signals
5. Update `src/database-types.ts` with signal on `execute()`
6. Export new types from `src/index.ts`
7. Add tests for abort behavior

### Phase 2: Retry Configuration

1. Add `p-retry` dependency
2. Create `src/retry-types.ts` with `RetryOptions` interface
3. Create `src/retry-utils.ts` with retry wrapper and utilities
4. Update `src/database-types.ts` with retry options
5. Update `src/collection-types.ts` with operation-level retry options
6. Update `src/sqlite-collection.ts` to use retry wrapper
7. Update `src/stratadb.ts` to pass retry options
8. Export new types from `src/index.ts`
9. Add tests for retry behavior

---

## 5. Important Design Decisions

### Why AbortedError?

While p-retry handles `AbortSignal` natively, we still need `AbortedError` for:
1. **Pre-flight checks**: Throwing before operations start (when signal is already aborted)
2. **Consistent error handling**: All StrataDB errors extend `StrataDBError`
3. **Additional context**: Preserving the abort reason for debugging

### Why Default retries = 0?

Unlike p-retry's default of 10 retries, StrataDB defaults to 0 because:
1. **Explicit opt-in**: Retries can mask bugs; users should consciously enable them
2. **Predictable behavior**: No surprises for users expecting immediate failures
3. **Performance**: No overhead for applications that don't need retries

### Retry vs Transaction Semantics

Retries apply to individual operations, not entire transactions. This is because:
1. **Granular control**: Different operations may need different retry strategies
2. **Partial progress**: Some operations in a transaction may succeed before one fails
3. **User control**: Users can disable retry per-operation for atomic behavior

---

## 6. Transaction Retry Behavior

### Database-Level Transaction Retry

When retry is configured at the database level, it applies to individual collection operations within transactions, not the entire transaction:

```typescript
const db = new Strata({
  database: "app.db",
  retry: { retries: 3 }
})

// Each operation retries independently
await db.execute(async (tx) => {
  const users = tx.collection("users", userSchema)
  await users.insertOne(user1)  // Retries up to 3 times if it fails
  await users.insertOne(user2)  // Retries up to 3 times if it fails
})
```

### Disabling Retry in Transactions

For atomic all-or-nothing behavior, disable retry within transactions:

```typescript
await db.execute(async (tx) => {
  const users = tx.collection("users", userSchema)
  // No retries - fail fast
  await users.insertOne(user1, { retry: false })
  await users.insertOne(user2, { retry: false })
})
```

---

## 7. Verification Plan

### Automated Tests

The existing test infrastructure uses Bun's test runner. New tests will be added to verify:

#### Unit Tests

##### AbortSignal Unit Tests (`test/unit/abort-utils.test.ts`)

```bash
bun test test/unit/abort-utils.test.ts
```

Test cases:
- `throwIfAborted` throws `AbortedError` when signal is aborted
- `throwIfAborted` does nothing when signal is not aborted
- `throwIfAborted` does nothing when signal is undefined
- `createAbortPromise` rejects immediately when signal is already aborted
- `createAbortPromise` rejects when signal is aborted later
- `createAbortPromise` never resolves when signal is undefined
- `createAbortPromise` cleanup removes event listener (no memory leak)
- `AbortedError` preserves reason from signal
- `AbortedError` uses default message when reason has no message
- `AbortedError` has correct code and category properties

##### Retry Unit Tests (`test/unit/retry-utils.test.ts`)

```bash
bun test test/unit/retry-utils.test.ts
```

Test cases:
- `withRetry` executes operation once when retries = 0
- `withRetry` retries on retryable errors
- `withRetry` stops on non-retryable errors
- `withRetry` respects maxRetryTime
- `withRetry` calls onFailedAttempt on each failure
- `withRetry` respects shouldRetry predicate
- `withRetry` respects shouldConsumeRetry predicate
- `mergeRetryOptions` returns undefined when operation-level is false
- `mergeRetryOptions` returns undefined when collection-level is false
- `mergeRetryOptions` merges options with correct precedence
- `defaultShouldRetry` returns false for ValidationError
- `defaultShouldRetry` returns false for SchemaValidationError
- `defaultShouldRetry` returns false for UniqueConstraintError
- `defaultShouldRetry` returns false for ConstraintError
- `defaultShouldRetry` returns true for ConnectionError
- `defaultShouldRetry` returns true for SQLITE_BUSY (code 5)
- `defaultShouldRetry` returns true for SQLITE_LOCKED (code 6)
- `defaultShouldRetry` returns true for TransactionError
- `defaultShouldRetry` returns false for unknown errors

#### Integration Tests

##### AbortSignal Integration Tests (`test/integration/abort-signal.test.ts`)

```bash
bun test test/integration/abort-signal.test.ts
```

Test cases:
- `find` throws AbortedError when signal is pre-aborted
- `findOne` throws AbortedError when signal is pre-aborted
- `findById` throws AbortedError when signal is pre-aborted
- `count` throws AbortedError when signal is pre-aborted
- `insertOne` throws AbortedError when signal is pre-aborted
- `insertMany` throws AbortedError when signal is pre-aborted
- `updateOne` throws AbortedError when signal is pre-aborted
- `updateMany` throws AbortedError when signal is pre-aborted
- `deleteOne` throws AbortedError when signal is pre-aborted
- `deleteMany` throws AbortedError when signal is pre-aborted
- `findOneAndUpdate` throws AbortedError when signal is pre-aborted
- `findOneAndDelete` throws AbortedError when signal is pre-aborted
- `findOneAndReplace` throws AbortedError when signal is pre-aborted
- `search` throws AbortedError when signal is pre-aborted
- `distinct` throws AbortedError when signal is pre-aborted
- `db.execute` throws AbortedError when signal is pre-aborted
- `AbortSignal.timeout()` works correctly with operations
- Operations complete successfully when signal is not aborted

##### Retry Integration Tests (`test/integration/retry-configuration.test.ts`)

```bash
bun test test/integration/retry-configuration.test.ts
```

Test cases:
- Database-level retry options are passed to collections
- Collection-level retry options override database-level
- Operation-level retry options override collection-level
- `retry: false` at collection level disables retries
- `retry: false` at operation level disables retries
- Retries work correctly with AbortSignal
- Retries stop when signal is aborted
- `onFailedAttempt` receives correct context
- Exponential backoff is applied between retries
- `maxRetryTime` limits total retry duration

#### Type Tests (`test/type/abort-retry.test-d.ts`)

Test cases:
- Signal option is accepted on all collection methods
- Retry option is accepted on all collection methods
- `retry: false` is accepted
- RetryOptions type is correct
- AbortedError extends StrataDBError

### Running All Tests

```bash
bun test
```

### Type Checking

```bash
bun run typecheck
```

### Lint

```bash
bun run lint
```

---

## 8. Documentation Updates

### New Documentation Files

| File | Description |
|------|-------------|
| `docs/guide/abort-signal.md` | Guide for using AbortSignal with operations |
| `docs/guide/retry.md` | Guide for configuring retry behavior |

### Modified Documentation Files

| File | Changes |
|------|---------|
| `docs/guide/collections.md` | Add signal and retry options to method examples |
| `docs/guide/transactions.md` | Document signal support in `execute()` |
| `docs/guide/troubleshooting.md` | Add AbortedError and retry troubleshooting |
| `docs/guide/faq.md` | Add FAQ entries for abort and retry |
| `docs/api/classes/` | Add `AbortedError.md` |
| `docs/api/type-aliases/` | Add `RetryOptions.md`, `RetryContext.md` |
| `docs/api/functions/` | Add `throwIfAborted.md`, `withRetry.md`, `defaultShouldRetry.md` |

### API Documentation (typedoc)

The following exports need JSDoc documentation:
- `AbortedError` class
- `RetryOptions` type
- `throwIfAborted` function
- `abortPromise` function
- `withRetry` function
- `mergeRetryOptions` function
- `defaultShouldRetry` function

---

## 9. Edge Cases and Error Handling

### AbortSignal Edge Cases

| Edge Case | Expected Behavior |
|-----------|-------------------|
| Signal already aborted before call | Throw `AbortedError` immediately |
| Signal aborted with custom reason | Preserve reason in `AbortedError.reason` |
| Signal aborted with no reason | Use default message "Operation aborted" |
| Signal is `undefined` | Operation proceeds normally |
| Signal aborted during deserialization | Throw `AbortedError` (check between batches) |
| Multiple operations with same signal | All throw when signal aborts |
| Signal from `AbortSignal.timeout()` | Works correctly, throws on timeout |
| Signal from `AbortSignal.any()` | Works correctly with combined signals |

### Retry Edge Cases

| Edge Case | Expected Behavior |
|-----------|-------------------|
| `retries: 0` | No retry, execute once |
| `retries: 1` | Execute once, retry once on failure |
| All retries exhausted | Throw last error |
| `shouldRetry` returns false | Stop retrying, throw error |
| `shouldRetry` throws | Stop retrying, throw shouldRetry's error |
| `onFailedAttempt` throws | Stop retrying, throw onFailedAttempt's error |
| `maxRetryTime` exceeded | Stop retrying, throw last error |
| Signal aborted during retry wait | Stop retrying, throw abort error |
| Error without `instanceof` match | `defaultShouldRetry` returns false |
| `retry: false` with database retry configured | No retry for that operation |
| Nested retry options (db + collection + operation) | Merge with operation taking precedence |

### Combined AbortSignal + Retry Edge Cases

| Edge Case | Expected Behavior |
|-----------|-------------------|
| Signal aborted before first attempt | Throw `AbortedError`, no retries |
| Signal aborted during retry backoff | Stop retrying, throw abort error |
| Signal aborted during operation execution | Throw `AbortedError` on next check |
| `maxRetryTime` and signal timeout both set | Whichever triggers first stops execution |

### Error Propagation

| Scenario | Error Type |
|----------|------------|
| Signal aborted | `AbortedError` |
| Validation failure (no retry) | `ValidationError` |
| Unique constraint (no retry) | `UniqueConstraintError` |
| SQLITE_BUSY after all retries | `DatabaseError` with sqliteCode |
| Connection error after all retries | `ConnectionError` |
| Unknown error (no retry) | Original error |

---

## 10. Breaking Changes and Migration

### Breaking Changes

**None.** Both features are additive:
- `signal` and `retry` options are optional on all methods
- Existing code continues to work without modification
- Default behavior (no abort checking, no retries) matches current behavior

### Migration Guide

No migration required. To adopt the new features:

1. **Add AbortSignal support:**
   ```typescript
   // Before
   await users.find({ status: 'active' })

   // After (optional)
   await users.find({ status: 'active' }, { signal: controller.signal })
   ```

2. **Add retry support:**
   ```typescript
   // Before
   const db = new Strata({ database: 'app.db' })

   // After (optional)
   const db = new Strata({
     database: 'app.db',
     retry: { retries: 3 }
   })
   ```

---

## 11. Usage Examples

### AbortSignal Examples

```typescript
import { Strata, AbortedError } from "stratadb"

const db = new Strata({ database: "app.db" })
const users = db.collection("users", userSchema)

// Timeout after 5 seconds
const controller = new AbortController()
const timeout = setTimeout(() => controller.abort(), 5000)

try {
  const results = await users.find({}, { signal: controller.signal })
  clearTimeout(timeout)
  return results
} catch (error) {
  if (error instanceof AbortedError) {
    console.log("Query timed out")
  }
  throw error
}

// Using AbortSignal.timeout() (Node 18+)
const results = await users.find({}, {
  signal: AbortSignal.timeout(5000)
})
```

### Retry Examples

```typescript
import { Strata } from "stratadb"

// Database-level retry (applies to all collections)
const db = new Strata({
  database: "app.db",
  retry: {
    retries: 3,
    minTimeout: 100,
    maxTimeout: 5000,
    onFailedAttempt: ({ error, attemptNumber, retriesLeft }) => {
      console.log(`Attempt ${attemptNumber} failed: ${error.message}`)
      console.log(`${retriesLeft} retries remaining`)
    }
  }
})

// Collection-level override
const users = db.collection("users", userSchema, {
  retry: {
    retries: 5,  // More retries for this collection
    shouldRetry: ({ error }) => {
      // Custom logic: only retry on specific errors
      return error.code === "SQLITE_BUSY"
    }
  }
})

// Disable retry for specific collection
const logs = db.collection("logs", logSchema, {
  retry: false
})

// Operation-level override
await users.insertOne(data, {
  retry: { retries: 10 }  // Extra retries for this operation
})

// Disable retry for specific operation
await users.find({}, { retry: false })
```

### Combined AbortSignal + Retry

```typescript
const controller = new AbortController()

// Retry will stop if signal is aborted
await users.find({}, {
  signal: controller.signal,
  retry: {
    retries: 3,
    onFailedAttempt: ({ attemptNumber }) => {
      console.log(`Retry attempt ${attemptNumber}`)
    }
  }
})

// Abort after 10 seconds (including all retries)
setTimeout(() => controller.abort(), 10000)
```

### Rate Limiting with shouldConsumeRetry

```typescript
// Don't consume retries for rate limit errors
const users = db.collection("users", userSchema, {
  retry: {
    retries: 10,
    shouldConsumeRetry: ({ error }) => {
      // Rate limits don't count against retry budget
      return !(error instanceof RateLimitError)
    },
    onFailedAttempt: ({ error, retriesLeft }) => {
      if (error instanceof RateLimitError) {
        console.log(`Rate limited, waiting... (${retriesLeft} retries still available)`)
      }
    }
  }
})
```
