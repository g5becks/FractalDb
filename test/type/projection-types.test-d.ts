// biome-ignore-all lint/nursery/noUnusedExpressions: Type tests intentionally use expressions to verify TypeScript errors on projected types
// biome-ignore-all lint/complexity/noVoid: Using void to suppress unused function warnings in type tests
import {
  expectAssignable,
  expectError,
  expectNotAssignable,
  expectType,
} from "tsd"
import type {
  Collection,
  Document,
  OmitSpec,
  ProjectedDocument,
  QueryOptions,
  QueryOptionsBase,
  QueryOptionsWithOmit,
  QueryOptionsWithoutProjection,
  QueryOptionsWithSelect,
  SelectSpec,
} from "../../dist/index.js"

// ============================================================================
// TEST TYPES
// ============================================================================

/**
 * User document type for testing projections.
 * Contains various field types to verify projection behavior.
 */
type User = Document<{
  name: string
  email: string
  password: string
  age: number
  active: boolean
  role: "admin" | "user" | "guest"
  tags: string[]
  profile: {
    bio: string
    avatar: string
  }
  createdAt: number
  updatedAt: number
}>

// ============================================================================
// SELECT SPEC TYPE TESTS
// ============================================================================

/**
 * SelectSpec should only accept valid field names from the document type.
 */

// ✅ Valid SelectSpec with known fields
expectAssignable<SelectSpec<User>>(["name", "email"])
expectAssignable<SelectSpec<User>>(["name", "email", "age", "active"])
expectAssignable<SelectSpec<User>>(["_id", "name"])
expectAssignable<SelectSpec<User>>([])

// ❌ Invalid SelectSpec with unknown fields
expectError<SelectSpec<User>>(["name", "nonexistent"])
expectError<SelectSpec<User>>(["unknownField"])

// ============================================================================
// OMIT SPEC TYPE TESTS
// ============================================================================

/**
 * OmitSpec should only accept valid field names from the document type.
 */

// ✅ Valid OmitSpec with known fields
expectAssignable<OmitSpec<User>>(["password"])
expectAssignable<OmitSpec<User>>(["password", "email"])
expectAssignable<OmitSpec<User>>([])

// ❌ Invalid OmitSpec with unknown fields
expectError<OmitSpec<User>>(["password", "nonexistent"])
expectError<OmitSpec<User>>(["unknownField"])

// ============================================================================
// QUERY OPTIONS WITH SELECT TYPE TESTS
// ============================================================================

/**
 * QueryOptionsWithSelect should enforce select array and exclude omit/projection.
 */

// ✅ Valid QueryOptionsWithSelect
declare const selectOptions1: QueryOptionsWithSelect<User, "name" | "email">
expectType<readonly ("name" | "email")[]>(selectOptions1.select)
expectType<undefined>(selectOptions1.omit)
expectType<undefined>(selectOptions1.projection)

// ✅ Can include sort, limit, skip with select
expectAssignable<QueryOptionsWithSelect<User, "name" | "age">>({
  select: ["name", "age"] as const,
  sort: { name: 1 },
  limit: 10,
  skip: 0,
})

// ❌ Cannot mix select with omit
expectNotAssignable<QueryOptionsWithSelect<User, "name">>({
  select: ["name"] as const,
  omit: ["password"],
})

// ============================================================================
// QUERY OPTIONS WITH OMIT TYPE TESTS
// ============================================================================

/**
 * QueryOptionsWithOmit should enforce omit array and exclude select/projection.
 */

// ✅ Valid QueryOptionsWithOmit
declare const omitOptions1: QueryOptionsWithOmit<User, "password">
expectType<readonly "password"[]>(omitOptions1.omit)
expectType<undefined>(omitOptions1.select)
expectType<undefined>(omitOptions1.projection)

// ✅ Can include sort, limit, skip with omit
expectAssignable<QueryOptionsWithOmit<User, "password" | "email">>({
  omit: ["password", "email"] as const,
  sort: { name: 1 },
  limit: 10,
})

// ❌ Cannot mix omit with select
expectNotAssignable<QueryOptionsWithOmit<User, "password">>({
  omit: ["password"] as const,
  select: ["name"],
})

// ============================================================================
// QUERY OPTIONS WITHOUT PROJECTION TYPE TESTS
// ============================================================================

/**
 * QueryOptionsWithoutProjection should exclude all projection options.
 */

// ✅ Valid QueryOptionsWithoutProjection
declare const noProjection: QueryOptionsWithoutProjection<User>
expectType<undefined>(noProjection.select)
expectType<undefined>(noProjection.omit)
expectType<undefined>(noProjection.projection)

// ✅ Can include sort, limit, skip
expectAssignable<QueryOptionsWithoutProjection<User>>({
  sort: { createdAt: -1 },
  limit: 20,
  skip: 10,
})

// ============================================================================
// PROJECTED DOCUMENT TYPE TESTS
// ============================================================================

/**
 * ProjectedDocument should correctly compute the resulting type based on projection.
 */

// ✅ ProjectedDocument with no keys returns full type
expectType<User>({} as ProjectedDocument<User>)
expectType<User>({} as ProjectedDocument<User, never>)

// ✅ ProjectedDocument with keys returns Pick including _id
type NameEmailProjection = ProjectedDocument<User, "name" | "email">
declare const projected1: NameEmailProjection
expectType<string>(projected1._id)
expectType<string>(projected1.name)
expectType<string>(projected1.email)

// ============================================================================
// COLLECTION FIND METHOD OVERLOAD TESTS
// ============================================================================

/**
 * Collection.find() should return correctly typed results based on projection options.
 */

declare const users: Collection<User>

// ✅ find() without projection returns full User[]
async function testFindNoProjection() {
  const results = await users.find({ active: true })
  expectType<readonly User[]>(results)

  // All fields should be accessible
  const user = results[0]
  expectType<string>(user._id)
  expectType<string>(user.name)
  expectType<string>(user.email)
  expectType<string>(user.password)
  expectType<number>(user.age)
}

// ✅ find() with select returns Pick<User, K | '_id'>[]
async function testFindWithSelect() {
  const results = await users.find(
    { active: true },
    { select: ["name", "email"] as const }
  )

  // Should return Pick<User, '_id' | 'name' | 'email'>[]
  const user = results[0]
  expectType<string>(user._id)
  expectType<string>(user.name)
  expectType<string>(user.email)

  // These fields should NOT exist on the projected type
  // @ts-expect-error - password not in projection
  user.password
  // @ts-expect-error - age not in projection
  user.age
  // @ts-expect-error - active not in projection
  user.active
}

// ✅ find() with omit returns Omit<User, K>[]
async function testFindWithOmit() {
  const results = await users.find(
    { active: true },
    { omit: ["password", "email"] as const }
  )

  // Should return Omit<User, 'password' | 'email'>[]
  const user = results[0]
  expectType<string>(user._id)
  expectType<string>(user.name)
  expectType<number>(user.age)
  expectType<boolean>(user.active)

  // These fields should NOT exist on the projected type
  // @ts-expect-error - password was omitted
  user.password
  // @ts-expect-error - email was omitted
  user.email
}

// ============================================================================
// COLLECTION FINDONE METHOD OVERLOAD TESTS
// ============================================================================

/**
 * Collection.findOne() should return correctly typed results based on projection options.
 */

// ✅ findOne() without projection returns User | null
async function testFindOneNoProjection() {
  const result = await users.findOne({ active: true })
  expectType<User | null>(result)

  if (result) {
    expectType<string>(result._id)
    expectType<string>(result.name)
    expectType<string>(result.password)
  }
}

// ✅ findOne() with select returns Pick<User, K | '_id'> | null
async function testFindOneWithSelect() {
  const result = await users.findOne(
    { active: true },
    { select: ["name", "age"] as const }
  )

  if (result) {
    expectType<string>(result._id)
    expectType<string>(result.name)
    expectType<number>(result.age)

    // These fields should NOT exist on the projected type
    // @ts-expect-error - password not in projection
    result.password
    // @ts-expect-error - email not in projection
    result.email
  }
}

// ✅ findOne() with omit returns Omit<User, K> | null
async function testFindOneWithOmit() {
  const result = await users.findOne(
    { active: true },
    { omit: ["password"] as const }
  )

  if (result) {
    expectType<string>(result._id)
    expectType<string>(result.name)
    expectType<string>(result.email)
    expectType<number>(result.age)

    // password should NOT exist on the projected type
    // @ts-expect-error - password was omitted
    result.password
  }
}

// ✅ findOne() by ID with select
async function testFindOneByIdWithSelect() {
  const result = await users.findOne("user-123", {
    select: ["name", "email"] as const,
  })

  if (result) {
    expectType<string>(result._id)
    expectType<string>(result.name)
    expectType<string>(result.email)

    // @ts-expect-error - password not in projection
    result.password
  }
}

// ============================================================================
// COLLECTION SEARCH METHOD OVERLOAD TESTS
// ============================================================================

/**
 * Collection.search() should return correctly typed results based on projection options.
 */

// ✅ search() without projection returns User[]
async function testSearchNoProjection() {
  const results = await users.search("alice", ["name", "email"])
  expectType<readonly User[]>(results)

  const user = results[0]
  expectType<string>(user._id)
  expectType<string>(user.name)
  expectType<string>(user.password)
}

// ✅ search() with select returns Pick<User, K | '_id'>[]
async function testSearchWithSelect() {
  const results = await users.search("alice", ["name", "email"], {
    select: ["name", "age"] as const,
  })

  const user = results[0]
  expectType<string>(user._id)
  expectType<string>(user.name)
  expectType<number>(user.age)

  // @ts-expect-error - password not in projection
  user.password
  // @ts-expect-error - email not in projection
  user.email
}

// ✅ search() with omit returns Omit<User, K>[]
async function testSearchWithOmit() {
  const results = await users.search("alice", ["name", "email"], {
    omit: ["password", "profile"] as const,
  })

  const user = results[0]
  expectType<string>(user._id)
  expectType<string>(user.name)
  expectType<string>(user.email)

  // @ts-expect-error - password was omitted
  user.password
  // @ts-expect-error - profile was omitted
  user.profile
}

// ✅ search() with select and filter
async function testSearchWithSelectAndFilter() {
  const results = await users.search("alice", ["name", "email"], {
    select: ["name", "role"] as const,
    filter: { active: true },
    sort: { name: 1 },
    limit: 10,
  })

  const user = results[0]
  expectType<string>(user._id)
  expectType<string>(user.name)
  expectType<"admin" | "user" | "guest">(user.role)

  // @ts-expect-error - email not in select
  user.email
}

// ============================================================================
// COMPLEX PROJECTION SCENARIOS
// ============================================================================

/**
 * Test complex scenarios with multiple fields and nested types.
 */

// ✅ Select with many fields
async function testSelectManyFields() {
  const results = await users.find(
    {},
    { select: ["name", "email", "age", "active", "role"] as const }
  )

  const user = results[0]
  expectType<string>(user._id)
  expectType<string>(user.name)
  expectType<string>(user.email)
  expectType<number>(user.age)
  expectType<boolean>(user.active)
  expectType<"admin" | "user" | "guest">(user.role)

  // @ts-expect-error - password not selected
  user.password
  // @ts-expect-error - tags not selected
  user.tags
  // @ts-expect-error - profile not selected
  user.profile
}

// ✅ Omit with many fields
async function testOmitManyFields() {
  const results = await users.find(
    {},
    { omit: ["password", "profile", "tags", "updatedAt"] as const }
  )

  const user = results[0]
  expectType<string>(user._id)
  expectType<string>(user.name)
  expectType<string>(user.email)
  expectType<number>(user.age)
  expectType<boolean>(user.active)

  // @ts-expect-error - password omitted
  user.password
  // @ts-expect-error - profile omitted
  user.profile
  // @ts-expect-error - tags omitted
  user.tags
  // @ts-expect-error - updatedAt omitted
  user.updatedAt
}

// ✅ Select with array field
async function testSelectArrayField() {
  const results = await users.find({}, { select: ["name", "tags"] as const })

  const user = results[0]
  expectType<string>(user._id)
  expectType<string>(user.name)
  expectType<string[]>(user.tags)

  // @ts-expect-error - email not selected
  user.email
}

// ✅ Select with nested object field
async function testSelectNestedField() {
  const results = await users.find({}, { select: ["name", "profile"] as const })

  const user = results[0]
  expectType<string>(user._id)
  expectType<string>(user.name)
  expectType<{ bio: string; avatar: string }>(user.profile)

  // @ts-expect-error - email not selected
  user.email
}

// ============================================================================
// QUERY OPTIONS BASE TYPE TESTS
// ============================================================================

/**
 * QueryOptionsBase should contain common options without projection fields.
 */

// ✅ QueryOptionsBase has sort, limit, skip, search, cursor
declare const baseOptions: QueryOptionsBase<User>
expectType<{ readonly [K in keyof User]?: 1 | -1 } | undefined>(
  baseOptions.sort
)
expectType<number | undefined>(baseOptions.limit)
expectType<number | undefined>(baseOptions.skip)

// ============================================================================
// INTEGRATION WITH QUERY OPTIONS TYPE
// ============================================================================

/**
 * Regular QueryOptions should still work for backwards compatibility.
 */

// ✅ QueryOptions accepts select as SelectSpec
expectAssignable<QueryOptions<User>>({
  select: ["name", "email"],
  sort: { name: 1 },
})

// ✅ QueryOptions accepts omit as OmitSpec
expectAssignable<QueryOptions<User>>({
  omit: ["password"],
  limit: 10,
})

// ✅ QueryOptions accepts projection
expectAssignable<QueryOptions<User>>({
  projection: { name: 1, email: 1 },
})

// ============================================================================
// ENSURE TYPE SAFETY IS ENFORCED
// ============================================================================

/**
 * These tests verify that invalid projections cause type errors.
 */

// ❌ Cannot select fields that don't exist
async function testInvalidSelect() {
  // @ts-expect-error - 'nonexistent' is not a valid field
  await users.find({}, { select: ["name", "nonexistent"] as const })
}

// ❌ Cannot omit fields that don't exist
async function testInvalidOmit() {
  // @ts-expect-error - 'nonexistent' is not a valid field
  await users.find({}, { omit: ["password", "nonexistent"] as const })
}

// Suppress unused function warnings
void testFindNoProjection
void testFindWithSelect
void testFindWithOmit
void testFindOneNoProjection
void testFindOneWithSelect
void testFindOneWithOmit
void testFindOneByIdWithSelect
void testSearchNoProjection
void testSearchWithSelect
void testSearchWithOmit
void testSearchWithSelectAndFilter
void testSelectManyFields
void testOmitManyFields
void testSelectArrayField
void testSelectNestedField
void testInvalidSelect
void testInvalidOmit
