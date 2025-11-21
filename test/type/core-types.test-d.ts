import {
  expectAssignable,
  expectError,
  expectNotAssignable,
  expectType,
} from "tsd"
import type {
  ArrayOperator,
  BulkWriteResult,
  ComparisonOperator,
  Document,
  DocumentInput,
  DocumentUpdate,
  FieldOperator,
  QueryFilter,
  StringOperator,
} from "../../dist/index.js"

// ============================================================================
// TEST TYPES
// ============================================================================

/**
 * Basic user document type for testing.
 * Represents a simple user with required fields and optional nested data.
 */
type User = Document<{
  name: string
  email: string
  age: number
  active: boolean
  tags: string[]
  profile?: {
    bio: string
    settings: {
      theme: "light" | "dark"
      notifications: boolean
    }
  }
  createdAt: Date
}>

/**
 * Product document type for testing nested objects and arrays.
 * Used to verify complex nested path type extraction.
 */
type Product = Document<{
  name: string
  price: number
  inventory: {
    stock: number
    warehouse: string
  }
  categories: string[]
  variants: Array<{
    id: string
    price: number
    features: string[]
  }>
}>

// Use Product type in a test to avoid unused error
declare const _productFilter: QueryFilter<Product>
expectType<QueryFilter<Product>>(_productFilter)

// ============================================================================
// DOCUMENTINPUT TYPE TESTS
// ============================================================================

/**
 * DocumentInput should make the 'id' field optional while requiring all other fields.
 * This tests that input types correctly enforce required fields for insertion.
 */

// ✅ Valid DocumentInput usage - id is optional, all other fields required
declare const validUserInput: DocumentInput<User>
expectType<string | undefined>(validUserInput.id)
expectType<string>(validUserInput.name)
expectType<string>(validUserInput.email)
expectType<number>(validUserInput.age)
expectType<boolean>(validUserInput.active)

// ✅ Value assignability tests
expectAssignable<DocumentInput<User>>({
  name: "Alice",
  email: "alice@example.com",
  age: 30,
  active: true,
  tags: ["developer"],
  createdAt: new Date(),
} as const)

// ✅ Valid DocumentInput usage - id can be explicitly provided
expectAssignable<DocumentInput<User>>({
  id: "user-123",
  name: "Bob",
  email: "bob@example.com",
  age: 25,
  active: true,
  tags: ["designer"],
  profile: {
    bio: "UI/UX Designer",
    settings: {
      theme: "dark",
      notifications: true,
    },
  },
  createdAt: new Date(),
} as const)

// ❌ Invalid DocumentInput usage - missing required fields should cause compilation error
expectError<DocumentInput<User>>({
  id: "user-123",
  // Missing: name, email, age, active, tags, createdAt
})

// ============================================================================
// DOCUMENTUPDATE TYPE TESTS
// ============================================================================

/**
 * DocumentUpdate should make all fields optional (deep partial) and exclude 'id'.
 * This tests that update types correctly allow partial updates while preventing ID modification.
 */

// ✅ Valid DocumentUpdate usage - all fields optional
declare const validUpdate: DocumentUpdate<User>
expectType<string | undefined>(validUpdate.name)
expectType<number | undefined>(validUpdate.age)

// ✅ Value assignability for partial updates
expectAssignable<DocumentUpdate<User>>({
  name: "Alice Smith",
  age: 31,
})

expectAssignable<DocumentUpdate<User>>({
  profile: {
    bio: "Updated bio",
  },
})

expectAssignable<DocumentUpdate<User>>({})

// ❌ Invalid DocumentUpdate usage - id field should be excluded from updates
expectNotAssignable<DocumentUpdate<User>>({
  id: "new-id",
  name: "Alice",
})

// ============================================================================
// COMPARISON OPERATOR TYPE TESTS
// ============================================================================

/**
 * Comparison operators should reject invalid type combinations.
 * Numbers and Dates support ordering operators, strings only support basic comparisons.
 */

// ✅ Valid comparison operators for numbers - check individual properties
declare const numberOps: ComparisonOperator<number>
expectType<number | undefined>(numberOps.$eq)
expectType<number | undefined>(numberOps.$ne)
expectType<number | undefined>(numberOps.$gt)
expectType<number | undefined>(numberOps.$gte)
expectType<number | undefined>(numberOps.$lt)
expectType<number | undefined>(numberOps.$lte)
expectType<readonly number[] | undefined>(numberOps.$in)
expectType<readonly number[] | undefined>(numberOps.$nin)

// ✅ Valid comparison operators for dates
declare const dateOps: ComparisonOperator<Date>
expectType<Date | undefined>(dateOps.$eq)
expectType<Date | undefined>(dateOps.$gt)
expectType<readonly Date[] | undefined>(dateOps.$in)

// ✅ Valid comparison operators for strings (no ordering operators)
declare const stringOps: ComparisonOperator<string>
expectType<string | undefined>(stringOps.$eq)
expectType<string | undefined>(stringOps.$ne)
expectType<readonly string[] | undefined>(stringOps.$in)

// ✅ Valid comparison operators for booleans
declare const boolOps: ComparisonOperator<boolean>
expectType<boolean | undefined>(boolOps.$eq)
expectType<boolean | undefined>(boolOps.$ne)

// ❌ Invalid comparison operators - wrong array types
expectError<ComparisonOperator<string>>({
  $eq: "hello",
  $in: [1, 2, 3], // Error: array elements must match field type (string)
})

// ============================================================================
// STRING OPERATOR TYPE TESTS
// ============================================================================

/**
 * String operators should only be available on string fields.
 * This ensures pattern matching operators are type-constrained to appropriate fields.
 */

// ✅ Valid string operators - check individual properties
declare const strOps: StringOperator
expectType<RegExp | string | undefined>(strOps.$regex)
expectType<"i" | undefined>(strOps.$options)
expectType<string | undefined>(strOps.$like)
expectType<string | undefined>(strOps.$startsWith)
expectType<string | undefined>(strOps.$endsWith)

// ✅ String operators work with string fields in FieldOperator
declare const stringFieldOps: FieldOperator<string>
expectType<RegExp | string | undefined>(stringFieldOps.$regex)
expectType<string | undefined>(stringFieldOps.$startsWith)

// ❌ String regex pattern must be RegExp or string
expectError<StringOperator>({
  $regex: 123, // Error: regex must be RegExp or string
})

// ============================================================================
// ARRAY OPERATOR TYPE TESTS
// ============================================================================

/**
 * Array operators should only be available on array fields.
 * This ensures array-specific operations are type-constrained to array types.
 */

// ✅ Valid array operators for string arrays - check individual properties
declare const strArrayOps: ArrayOperator<string[]>
expectType<readonly string[] | undefined>(strArrayOps.$all)
expectType<number | undefined>(strArrayOps.$size)
expectType<number | undefined>(strArrayOps.$index)

// ✅ Valid array operators for number arrays
declare const numArrayOps: ArrayOperator<number[]>
expectType<readonly number[] | undefined>(numArrayOps.$all)
expectType<number | undefined>(numArrayOps.$size)

// ✅ Array operators work with array fields in FieldOperator
declare const tagsFieldOps: FieldOperator<string[]>
expectType<readonly string[] | undefined>(tagsFieldOps.$all)
expectType<number | undefined>(tagsFieldOps.$size)

// ❌ ArrayOperator should be never for non-array types
expectType<never>({} as ArrayOperator<string>)
expectType<never>({} as ArrayOperator<number>)

// ============================================================================
// QUERY FILTER TYPE TESTS
// ============================================================================

/**
 * QueryFilter should allow field conditions and logical operators.
 */

// ✅ Valid queries with direct field matching
expectAssignable<QueryFilter<User>>({
  name: "Alice",
})

expectAssignable<QueryFilter<User>>({
  age: { $gt: 18 },
})

expectAssignable<QueryFilter<User>>({
  active: true,
})

// ✅ Valid queries with logical operators
expectAssignable<QueryFilter<User>>({
  $and: [{ age: { $gte: 18 } }],
})

expectAssignable<QueryFilter<User>>({
  $or: [{ name: "Alice" }, { name: "Bob" }],
})

// ✅ Valid nested path queries
expectAssignable<QueryFilter<User>>({
  "profile.bio": "Software engineer",
})

expectAssignable<QueryFilter<User>>({
  "profile.settings.theme": "dark",
})

// ============================================================================
// INVALID QUERY TYPE TESTS
// ============================================================================

/**
 * Invalid queries should cause compilation errors.
 */

// ❌ Invalid nested paths should cause compilation error
expectError<QueryFilter<User>>({
  "profile.nonexistent": "value",
})

// ❌ Type mismatches should cause compilation error
expectError<QueryFilter<User>>({
  age: "thirty", // Error: should be number, not string
})

// ============================================================================
// BULK WRITE RESULT TYPE TESTS
// ============================================================================

/**
 * BulkWriteResult should maintain correct types for successful and failed operations.
 */

// ✅ Valid BulkWriteResult type properties
declare const bulkResult: BulkWriteResult<User>
expectType<number>(bulkResult.insertedCount)
expectType<readonly string[]>(bulkResult.insertedIds)
expectType<readonly User[]>(bulkResult.documents)

// ============================================================================
// TYPE SAFETY VERIFICATION TESTS
// ============================================================================

/**
 * These tests verify that the type system prevents common runtime errors
 * through compile-time type checking.
 */

// ❌ Should prevent assigning wrong types to fields
expectError<DocumentInput<User>>({
  name: "Alice",
  email: "alice@example.com",
  age: "thirty", // Error: should be number, not string
  active: true,
  tags: ["developer"],
  createdAt: new Date(),
})

// ❌ Should prevent type mismatches in array elements
expectError<DocumentInput<User>>({
  name: "Alice",
  email: "alice@example.com",
  age: 30,
  active: true,
  tags: [1, 2, 3], // Error: array elements should be strings, not numbers
  createdAt: new Date(),
})
