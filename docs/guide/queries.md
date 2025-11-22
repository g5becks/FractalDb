# Queries

StrataDB provides type-safe MongoDB-style query operators.

## Basic Queries

### Find by ID

```typescript
const user = await users.findById('user-123')
if (user) {
  console.log(user.name)
}
```

### Find One

```typescript
const admin = await users.findOne({ role: 'admin' })
```

### Find Many

```typescript
const activeUsers = await users.find({ status: 'active' })
```

### Count

```typescript
const count = await users.count({ role: 'admin' })
```

## Comparison Operators

```typescript
// Equal (implicit)
await users.find({ age: 30 })

// Explicit operators
await users.find({ age: { $eq: 30 } })   // Equal
await users.find({ age: { $ne: 30 } })   // Not equal
await users.find({ age: { $gt: 18 } })   // Greater than
await users.find({ age: { $gte: 18 } })  // Greater than or equal
await users.find({ age: { $lt: 65 } })   // Less than
await users.find({ age: { $lte: 65 } })  // Less than or equal

// Range query
await users.find({ age: { $gte: 18, $lt: 65 } })
```

## Array Operators

```typescript
// Value in array
await users.find({ role: { $in: ['admin', 'moderator'] } })

// Value not in array
await users.find({ status: { $nin: ['banned', 'deleted'] } })
```

## String Operators

```typescript
// Starts with
await users.find({ name: { $startsWith: 'A' } })

// Ends with
await users.find({ email: { $endsWith: '@example.com' } })

// Contains
await users.find({ bio: { $contains: 'developer' } })

// LIKE pattern (SQL LIKE syntax)
await users.find({ name: { $like: 'J%n' } })  // John, Jan, Jason
```

## Logical Operators

```typescript
// AND (implicit - all conditions must match)
await users.find({ role: 'admin', status: 'active' })

// AND (explicit)
await users.find({
  $and: [
    { age: { $gte: 18 } },
    { age: { $lt: 65 } }
  ]
})

// OR
await users.find({
  $or: [
    { role: 'admin' },
    { role: 'moderator' }
  ]
})

// NOT
await users.find({
  $not: { status: 'banned' }
})
```

## Null Checks

```typescript
// Field is null
await users.find({ deletedAt: null })

// Field exists (is not null)
await users.find({ deletedAt: { $ne: null } })
```

## Query Options

### Sorting

```typescript
// Ascending (1) or descending (-1)
await users.find({}, {
  sort: { createdAt: -1 }  // Newest first
})

// Multiple sort fields
await users.find({}, {
  sort: { role: 1, name: 1 }  // By role, then by name
})
```

### Pagination

```typescript
// Limit results
await users.find({}, { limit: 10 })

// Skip results (for pagination)
await users.find({}, { skip: 20, limit: 10 })  // Page 3

// Paginated query helper
const page = 3
const pageSize = 10
await users.find({}, {
  skip: (page - 1) * pageSize,
  limit: pageSize,
  sort: { createdAt: -1 }
})
```

## Complex Queries

```typescript
// Find active adult admins, sorted by name
const results = await users.find(
  {
    $and: [
      { status: 'active' },
      { age: { $gte: 18 } },
      {
        $or: [
          { role: 'admin' },
          { permissions: { $contains: 'manage_users' } }
        ]
      }
    ]
  },
  {
    sort: { name: 1 },
    limit: 50
  }
)
```

## Type Safety

All queries are fully typed. TypeScript will error on invalid field names or types:

```typescript
// ✅ Valid - 'age' exists and is a number
await users.find({ age: { $gte: 18 } })

// ❌ Error - 'agee' doesn't exist
await users.find({ agee: { $gte: 18 } })

// ❌ Error - 'age' is number, not string
await users.find({ age: { $startsWith: '1' } })
```

## Query Debugging

Query results include a `toString()` method for inspecting generated SQL and parameters. This is useful for debugging and when reporting issues:

```typescript
import { SQLiteQueryTranslator } from 'stratadb'

const translator = new SQLiteQueryTranslator(userSchema)

// Translate a query to see the generated SQL
const result = translator.translate({
  age: { $gte: 18, $lt: 65 }
})

console.log(result.toString())
// Output:
// SQL: (_age >= ? AND _age < ?)
// Parameters: [18, 65]

// Also works with query options
const options = translator.translateOptions({
  sort: { createdAt: -1 },
  limit: 10,
  skip: 20
})

console.log(options.toString())
// Output:
// SQL: ORDER BY _createdAt DESC LIMIT ? OFFSET ?
// Parameters: [10, 20]
```

This introspection helps you understand:
- How your queries are translated to SQL
- Which fields use indexed columns vs JSON extraction
- The exact parameter values being bound
