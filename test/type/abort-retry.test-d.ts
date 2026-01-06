import { expectAssignable, expectError, expectType } from "tsd"
import type {
  AbortedError,
  Collection,
  Document,
  RetryContext,
  RetryOptions,
  StrataDBError,
} from "../../dist/index.js"

// ============================================================================
// TEST TYPES
// ============================================================================

type TestDoc = Document<{
  name: string
  value: number
}>

// Mock collection for testing
declare const collection: Collection<TestDoc>

// ============================================================================
// RetryOptions Type Structure
// ============================================================================

// RetryOptions should accept all valid properties
expectAssignable<RetryOptions>({
  retries: 3,
})

expectAssignable<RetryOptions>({
  retries: 3,
  minTimeout: 100,
})

expectAssignable<RetryOptions>({
  retries: 3,
  minTimeout: 100,
  maxRetryTime: 5000,
})

expectAssignable<RetryOptions>({
  retries: 3,
  onFailedAttempt: (context: RetryContext) => {
    expectType<number>(context.attemptNumber)
    expectType<number>(context.retriesLeft)
    expectType<Error>(context.error)
  },
})

expectAssignable<RetryOptions>({
  retries: 3,
  shouldRetry: (_error: Error) => true,
})

expectAssignable<RetryOptions>({
  retries: 3,
  shouldConsumeRetry: (_error: Error) => true,
})

// RetryContext should have correct structure
expectAssignable<RetryContext>({
  attemptNumber: 1,
  retriesLeft: 2,
  error: new Error("test"),
})

// ============================================================================
// AbortedError Type
// ============================================================================

// AbortedError should extend StrataDBError
declare const abortedError: AbortedError
expectAssignable<StrataDBError>(abortedError)

// AbortedError should have correct properties
expectType<string>(abortedError.message)
expectType<string>(abortedError.code)
expectType<string>(abortedError.category)
expectType<unknown>(abortedError.reason)

// ============================================================================
// Signal Option on Collection Methods
// ============================================================================

// Read operations
expectAssignable<Promise<TestDoc[]>>(
  collection.find({}, { signal: new AbortController().signal })
)

expectAssignable<Promise<TestDoc | null>>(
  collection.findOne({}, { signal: new AbortController().signal })
)

expectAssignable<Promise<TestDoc | null>>(
  collection.findById("id", { signal: new AbortController().signal })
)

expectAssignable<Promise<number>>(
  collection.count({}, { signal: new AbortController().signal })
)

expectAssignable<Promise<TestDoc[]>>(
  collection.search("query", ["name"], { signal: new AbortController().signal })
)

expectAssignable<Promise<unknown[]>>(
  collection.distinct("name", {}, { signal: new AbortController().signal })
)

expectAssignable<Promise<number>>(
  collection.estimatedDocumentCount({ signal: new AbortController().signal })
)

// Write operations
expectAssignable<Promise<TestDoc>>(
  collection.insertOne(
    { name: "test", value: 1 },
    { signal: new AbortController().signal }
  )
)

expectAssignable<Promise<TestDoc | null>>(
  collection.replaceOne(
    "id",
    { name: "test", value: 1 },
    { signal: new AbortController().signal }
  )
)

expectAssignable<Promise<boolean>>(
  collection.deleteOne("id", { signal: new AbortController().signal })
)

// Atomic operations
expectAssignable<Promise<TestDoc | null>>(
  collection.findOneAndDelete({}, { signal: new AbortController().signal })
)

expectAssignable<Promise<TestDoc | null>>(
  collection.findOneAndUpdate(
    {},
    { value: 2 },
    { signal: new AbortController().signal }
  )
)

expectAssignable<Promise<TestDoc | null>>(
  collection.findOneAndReplace(
    {},
    { name: "test", value: 2 },
    { signal: new AbortController().signal }
  )
)

// Utility operations
expectAssignable<Promise<void>>(
  collection.drop({ signal: new AbortController().signal })
)

expectAssignable<Promise<TestDoc>>(
  collection.validate(
    { name: "test", value: 1 },
    { signal: new AbortController().signal }
  )
)

// ============================================================================
// Retry Option on Collection Methods
// ============================================================================

// Read operations with retry
expectAssignable<Promise<TestDoc[]>>(
  collection.find({}, { retry: { retries: 3 } })
)

expectAssignable<Promise<TestDoc | null>>(
  collection.findOne({}, { retry: { retries: 3, minTimeout: 100 } })
)

expectAssignable<Promise<TestDoc | null>>(
  collection.findById("id", { retry: { retries: 3 } })
)

expectAssignable<Promise<number>>(
  collection.count({}, { retry: { retries: 3 } })
)

expectAssignable<Promise<TestDoc[]>>(
  collection.search("query", ["name"], { retry: { retries: 3 } })
)

expectAssignable<Promise<unknown[]>>(
  collection.distinct("name", {}, { retry: { retries: 3 } })
)

expectAssignable<Promise<number>>(
  collection.estimatedDocumentCount({ retry: { retries: 3 } })
)

// Write operations with retry
expectAssignable<Promise<TestDoc>>(
  collection.insertOne({ name: "test", value: 1 }, { retry: { retries: 3 } })
)

expectAssignable<Promise<TestDoc | null>>(
  collection.replaceOne(
    "id",
    { name: "test", value: 1 },
    { retry: { retries: 3 } }
  )
)

expectAssignable<Promise<boolean>>(
  collection.deleteOne("id", { retry: { retries: 3 } })
)

// Atomic operations with retry
expectAssignable<Promise<TestDoc | null>>(
  collection.findOneAndDelete({}, { retry: { retries: 3 } })
)

expectAssignable<Promise<TestDoc | null>>(
  collection.findOneAndUpdate({}, { value: 2 }, { retry: { retries: 3 } })
)

expectAssignable<Promise<TestDoc | null>>(
  collection.findOneAndReplace(
    {},
    { name: "test", value: 2 },
    { retry: { retries: 3 } }
  )
)

// Utility operations with retry
expectAssignable<Promise<void>>(collection.drop({ retry: { retries: 3 } }))

expectAssignable<Promise<TestDoc>>(
  collection.validate({ name: "test", value: 1 }, { retry: { retries: 3 } })
)

// ============================================================================
// Retry: false Option
// ============================================================================

// retry: false should be accepted
expectAssignable<Promise<TestDoc[]>>(collection.find({}, { retry: false }))

expectAssignable<Promise<TestDoc>>(
  collection.insertOne({ name: "test", value: 1 }, { retry: false })
)

expectAssignable<Promise<TestDoc | null>>(
  collection.findOneAndUpdate({}, { value: 2 }, { retry: false })
)

expectAssignable<Promise<void>>(collection.drop({ retry: false }))

// ============================================================================
// Combined Signal and Retry Options
// ============================================================================

// Both signal and retry should work together
expectAssignable<Promise<TestDoc[]>>(
  collection.find(
    {},
    {
      signal: new AbortController().signal,
      retry: { retries: 3, minTimeout: 100 },
    }
  )
)

expectAssignable<Promise<TestDoc>>(
  collection.insertOne(
    { name: "test", value: 1 },
    {
      signal: new AbortController().signal,
      retry: { retries: 3 },
    }
  )
)

expectAssignable<Promise<TestDoc | null>>(
  collection.findOneAndUpdate(
    {},
    { value: 2 },
    {
      signal: new AbortController().signal,
      retry: false,
    }
  )
)

// ============================================================================
// Invalid Retry Options Should Error
// ============================================================================

// Invalid retry properties should error
expectError<RetryOptions>({
  retries: "3", // Should be number
})

expectError<RetryOptions>({
  retries: 3,
  minTimeout: "100", // Should be number
})

expectError<RetryOptions>({
  retries: 3,
  invalidProperty: true, // Unknown property
})
