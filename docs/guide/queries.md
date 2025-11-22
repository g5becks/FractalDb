# Queries

StrataDB provides type-safe MongoDB-style query operators.

## Comparison Operators

```typescript
// Implicit equality
await users.find({ status: 'active' })

// Explicit operators
await users.find({ age: { $eq: 30 } })   // equal
await users.find({ age: { $ne: 30 } })   // not equal
await users.find({ age: { $gt: 18 } })   // greater than
await users.find({ age: { $gte: 18 } })  // greater than or equal
await users.find({ age: { $lt: 65 } })   // less than
await users.find({ age: { $lte: 65 } })  // less than or equal

// Range query (combine operators)
await users.find({ age: { $gte: 18, $lt: 65 } })

// Membership
await users.find({ role: { $in: ['admin', 'moderator'] } })
await users.find({ status: { $nin: ['banned', 'deleted'] } })
```

## String Operators

```typescript
await users.find({ name: { $startsWith: 'A' } })
await users.find({ email: { $endsWith: '@example.com' } })
await users.find({ name: { $like: 'J%n' } })      // John, Jan, Jason
await users.find({ bio: { $like: '%engineer%' } })  // contains
```

The `$like` operator uses SQL LIKE syntax: `%` matches any sequence, `_` matches one character.

## Array Operators

For querying array fields:

```typescript
type User = Document<{
  tags: string[]
  scores: number[]
}>

// Contains all specified values
await users.find({ tags: { $all: ['typescript', 'react'] } })

// Exact array length
await users.find({ tags: { $size: 3 } })

// Element at index matches value
await users.find({ scores: { $index: 0 } })  // first element

// At least one element matches filter
await users.find({
  scores: { $elemMatch: { $gte: 90 } }
})
```

## Logical Operators

```typescript
// Implicit AND (all conditions must match)
await users.find({ role: 'admin', status: 'active' })

// Explicit AND
await users.find({
  $and: [
    { age: { $gte: 18 } },
    { age: { $lt: 65 } }
  ]
})

// OR (at least one must match)
await users.find({
  $or: [
    { role: 'admin' },
    { role: 'moderator' }
  ]
})

// NOR (none must match)
await users.find({
  $nor: [
    { status: 'banned' },
    { status: 'deleted' }
  ]
})

// NOT (negate condition)
await users.find({
  $not: { status: 'inactive' }
})
```

## Existence Operator

```typescript
// Field exists (even if null)
await users.find({ deletedAt: { $exists: true } })

// Field does not exist
await users.find({ phone: { $exists: false } })

// Field is null
await users.find({ deletedAt: null })

// Field exists and is not null
await users.find({ email: { $exists: true, $ne: null } })
```

## Query Options

```typescript
await users.find(
  { status: 'active' },
  {
    sort: { createdAt: -1, name: 1 },  // -1 desc, 1 asc
    limit: 20,
    skip: 40  // for pagination
  }
)
```

## Nested Fields

Query nested objects using dot notation:

```typescript
type User = Document<{
  profile: {
    bio: string
    settings: { theme: string }
  }
}>

await users.find({ 'profile.bio': { $like: '%developer%' } })
await users.find({ 'profile.settings.theme': 'dark' })
```

## Type Safety

Queries are fully typed. TypeScript catches invalid field names and type mismatches:

```typescript
// ✅ Valid
await users.find({ age: { $gte: 18 } })

// ❌ Compile error: 'agee' doesn't exist
await users.find({ agee: { $gte: 18 } })

// ❌ Compile error: $startsWith only works on strings
await users.find({ age: { $startsWith: '1' } })

// ❌ Compile error: $gt only works on numbers/dates
await users.find({ name: { $gt: 'A' } })
```

## Complex Example

```typescript
const results = await users.find(
  {
    $and: [
      { status: 'active' },
      { age: { $gte: 18 } },
      {
        $or: [
          { role: 'admin' },
          { permissions: { $like: '%manage_users%' } }
        ]
      }
    ]
  },
  {
    sort: { createdAt: -1 },
    limit: 50
  }
)
```
