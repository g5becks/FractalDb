import {
  ConnectionError,
  ConstraintError,
  SchemaValidationError,
  TransactionError,
  UniqueConstraintError,
  ValidationError,
} from "./errors.js"
import type { RetryContext } from "./retry-types.js"

/**
 * SQLite error codes that are safe to retry.
 * - 5: SQLITE_BUSY - database is locked
 * - 6: SQLITE_LOCKED - table is locked
 * - 7: SQLITE_NOMEM - out of memory
 * - 10: SQLITE_IOERR - disk I/O error
 */
export const RETRYABLE_SQLITE_CODES = [5, 6, 7, 10] as const

/**
 * Default retry logic for database operations.
 * Determines which errors should trigger a retry attempt.
 *
 * @param context - Retry context containing error and attempt information
 * @returns true if the operation should be retried, false otherwise
 *
 * @example
 * ```typescript
 * const options: RetryOptions = {
 *   retries: 3,
 *   shouldRetry: defaultShouldRetry
 * }
 * ```
 */
export function defaultShouldRetry(context: RetryContext): boolean {
  const { error } = context

  // Never retry validation errors
  if (error instanceof ValidationError) {
    return false
  }
  if (error instanceof SchemaValidationError) {
    return false
  }
  if (error instanceof UniqueConstraintError) {
    return false
  }
  if (error instanceof ConstraintError) {
    return false
  }

  // Always retry connection and transaction errors
  if (error instanceof ConnectionError) {
    return true
  }
  if (error instanceof TransactionError) {
    return true
  }

  // Retry specific SQLite error codes
  if ("code" in error && typeof error.code === "number") {
    return RETRYABLE_SQLITE_CODES.includes(error.code as 5 | 6 | 7 | 10)
  }

  // Don't retry unknown errors
  return false
}
