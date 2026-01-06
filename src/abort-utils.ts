import { AbortedError } from "./errors.js"

/**
 * Throws AbortedError if the signal is already aborted.
 *
 * @param signal - Optional AbortSignal to check
 * @throws {AbortedError} When signal is aborted
 *
 * @remarks
 * Should be called at the start of async operations to fail fast if the
 * operation has already been cancelled. Does nothing if signal is undefined
 * or not aborted.
 *
 * @example
 * ```typescript
 * async function findUsers(filter: QueryFilter, signal?: AbortSignal) {
 *   throwIfAborted(signal);
 *   // Proceed with operation...
 * }
 * ```
 */
export function throwIfAborted(signal?: AbortSignal): void {
  if (signal?.aborted) {
    const message =
      signal.reason instanceof Error
        ? signal.reason.message
        : "Operation aborted"
    throw new AbortedError(message, signal.reason)
  }
}

/**
 * Creates a promise that rejects when the signal is aborted.
 *
 * @param signal - Optional AbortSignal to monitor
 * @returns Object with promise that rejects on abort and cleanup function
 *
 * @remarks
 * Use with Promise.race() for operations that don't natively support signals.
 *
 * **IMPORTANT:** The returned cleanup function MUST be called to prevent memory leaks.
 * The cleanup function removes the event listener from the signal.
 *
 * @example
 * ```typescript
 * const { promise: abortPromise, cleanup } = createAbortPromise(signal);
 * try {
 *   const result = await Promise.race([
 *     longRunningOperation(),
 *     abortPromise
 *   ]);
 *   return result;
 * } finally {
 *   cleanup();
 * }
 * ```
 */
export function createAbortPromise(signal?: AbortSignal): {
  promise: Promise<never>
  cleanup: () => void
} {
  if (!signal) {
    return {
      promise: new Promise(() => {
        // Never resolves - no signal to monitor
      }),
      cleanup: () => {
        // No cleanup needed when no signal
      },
    }
  }

  if (signal.aborted) {
    const message =
      signal.reason instanceof Error
        ? signal.reason.message
        : "Operation aborted"
    return {
      promise: Promise.reject(new AbortedError(message, signal.reason)),
      cleanup: () => {
        // No cleanup needed for already-aborted signal
      },
    }
  }

  let onAbort: (() => void) | undefined

  const promise = new Promise<never>((_, reject) => {
    onAbort = () => {
      const message =
        signal.reason instanceof Error
          ? signal.reason.message
          : "Operation aborted"
      reject(new AbortedError(message, signal.reason))
    }
    signal.addEventListener("abort", onAbort, { once: true })
  })

  return {
    promise,
    cleanup: () => {
      if (onAbort) {
        signal.removeEventListener("abort", onAbort)
      }
    },
  }
}
