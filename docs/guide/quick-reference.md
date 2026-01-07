# Quick Reference

This quick reference provides a comprehensive overview of StrataDB's API for quick lookup.

## Installation

```bash
bun add stratadb
```

## Basic Setup

```typescript
import { Strata, createSchema, type Document } from 'stratadb'

// Define document type
type User = Document<{
  name: string
  email: string
  age: number
  active: boolean
}>

// Create schema with indexing options
const userSchema = createSchema<User>()
  .field('name', { type: 'TEXT', indexed: true })
  .field('email', { type: 'TEXT', indexed: true, unique: true })
  .field('age', { type: 'INTEGER', indexed: true })
  .field('active', { type: 'INTEGER', indexed: true }) // INTEGER for boolean in SQLite
  .timestamps(true) // Enable createdAt/updatedAt
  .build()

// Initialize database
const db = new Strata({
  database: ':memory:'  // or 'path/to/file.db'
})

// Create collection
const users = db.collection('users', userSchema)
```

## Document Operations

### Create Documents

```typescript
// Insert single document
const result = await users.insertOne({
  name: 'Alice',
  email: 'alice@example.com',
  age: 30,
  active: true
})
// Returns: { acknowledged: true, document: { id, name, email, age, active, createdAt, updatedAt } }

// Insert multiple documents
const results = await users.insertMany([
  { name: 'Bob', email: 'bob@example.com', age: 25, active: true },
  { name: 'Charlie', email: 'charlie@example.com', age: 35, active: false }
])
// Returns: { acknowledged: true, insertedCount: number, insertedIds: string[] }
```

### Read Documents

```typescript
// Find by ID
const user = await users.findById('user-id')
// Returns: User | null

// Find single document
const user = await users.findOne({ email: 'alice@example.com' })
// Returns: User | null

// Find multiple documents
const activeUsers = await users.find({ active: true })
// Returns: User[]

// Count documents
const count = await users.count({ age: { $gte: 18 } })
// Returns: number

// Find with options (sort, limit, skip)
const recentUsers = await users.find(
  { active: true },
  {
    sort: { createdAt: -1 },  // Sort by creation date, newest first
    limit: 10,                // Limit to 10 results
    skip: 20                  // Skip first 20 results
  }
)

// Search across multiple fields (v0.3.0+)
const results = await users.search('alice', ['name', 'email'])
const filtered = await users.search('admin', ['role', 'bio'], {
  filter: { active: true },
  sort: { createdAt: -1 },
  limit: 10
})

// Field projection (v0.3.0+)
const names = await users.find({}, { select: ['name', 'email'] })  // Include only these fields
const safe = await users.find({}, { omit: ['password', 'ssn'] })   // Exclude these fields
```

### Update Documents

```typescript
// Update single document by ID
const result = await users.updateOne('user-id', {
  name: 'Alice Updated',
  age: 31
})
// Returns: { matchedCount: number, modifiedCount: number }

// Update with full replacement of specified fields
// Note: No MongoDB-style operators like $inc, $set, etc.
const result = await users.updateOne('user-id', {
  name: 'Alice Updated',      // Update specific fields
  lastLogin: Date.now()       // Add new fields or replace existing ones
})

// Update with condition
const result = await users.updateOne(
  { email: 'alice@example.com' },  // Find condition
  { active: false }                // Update data
)

```

### Delete Documents

```typescript
// Delete single document by ID
const deleted = await users.deleteOne('user-id')
// Returns: boolean (true if deleted, false if not found)

// Delete with condition
const deleteResult = await users.deleteOne({ age: { $lt: 18 } })
// Returns: { deletedCount: number }

// Delete multiple documents
const deleteResult = await users.deleteMany({ active: false })
// Returns: { deletedCount: number }
```

## Query Filters

### Comparison Operators

```typescript
// Equality (implicit)
await users.find({ age: 30 })

// Explicit comparison operators
await users.find({ age: { $eq: 30 } })   // Equal to
await users.find({ age: { $ne: 30 } })   // Not equal to
await users.find({ age: { $gt: 18 } })   // Greater than
await users.find({ age: { $gte: 18 } })  // Greater than or equal
await users.find({ age: { $lt: 65 } })   // Less than
await users.find({ age: { $lte: 65 } })  // Less than or equal
```

### Array Operators

```typescript
// $in - value in array
await users.find({ status: { $in: ['active', 'pending'] } })

// $nin - value not in array
await users.find({ status: { $nin: ['suspended', 'banned'] } })

// $all - array contains all values (for array fields)
await users.find({ tags: { $all: ['admin', 'verified'] } })

// $size - array has specific length
await posts.find({ comments: { $size: 0 } })  // No comments

// $elemMatch - array has element matching condition
await users.find({
  hobbies: {
    $elemMatch: {
      category: 'sports',
      level: { $gte: 5 }
    }
  }
})
```

### String Operators

```typescript
// $like - SQL LIKE pattern matching
await users.find({ email: { $like: '%@gmail.com' } })

// $ilike - Case-insensitive LIKE (v0.3.0+)
await users.find({ name: { $ilike: 'john' } })  // Matches John, JOHN, john

// $contains - Substring matching (v0.3.0+)
await users.find({ bio: { $contains: 'developer' } })  // Shorthand for $like: '%developer%'

// $startsWith - string starts with
await users.find({ name: { $startsWith: 'John' } })

// $endsWith - string ends with
await users.find({ email: { $endsWith: '@company.com' } })
```

> **Note**: Regular expressions (`$regex`) are not supported. Use `$like`, `$ilike`, `$contains`, `$startsWith`, and `$endsWith` for pattern matching.

### Logical Operators

```typescript
// $and - all conditions must be true
await users.find({
  $and: [
    { age: { $gte: 18 } },
    { active: true }
  ]
})

// $or - any condition can be true
await users.find({
  $or: [
    { role: 'admin' },
    { role: 'moderator' }
  ]
})

// $not - negate condition
await users.find({
  $not: { active: false }
})

// $nor - none of the conditions are true
await users.find({
  $nor: [
    { age: { $lt: 18 } },
    { active: false }
  ]
})

// Implicit and explicit combinations
await users.find({
  age: { $gte: 18 },           // Implicit AND
  $or: [
    { role: 'admin' },
    { verified: true }
  ]
})
```

## Advanced Features

### Schema Validation

```typescript
import { z } from 'zod'
import { wrapStandardSchema } from 'stratadb'

// Define Zod schema
const UserZodSchema = z.object({
  id: z.string(),
  name: z.string().min(1).max(100),
  email: z.string().email(),
  age: z.number().int().min(0).max(150),
  createdAt: z.number(),
  updatedAt: z.number()
})

// Add validation to schema
const userSchema = createSchema<User>()
  .field('name', { type: 'TEXT', indexed: true })
  .field('email', { type: 'TEXT', indexed: true, unique: true })
  .validate(wrapStandardSchema<User>(UserZodSchema))
  .build()
```

### Compound Indexes

```typescript
const userSchema = createSchema<User>()
  .field('email', { type: 'TEXT', indexed: true })
  .field('tenantId', { type: 'TEXT', indexed: true })
  .compoundIndex('email_tenant', ['email', 'tenantId'], { unique: true })
  .build()
```

### Transactions

```typescript
// Transaction helper (recommended)
const result = await db.execute(async (tx) => {
  const usersTx = tx.collection('users', userSchema)
  const accountsTx = tx.collection('accounts', accountSchema)

  const user = await usersTx.insertOne(userData)
  await accountsTx.insertOne({ userId: user.document.id, balance: 0 })

  return { userId: user.document.id }
})

// Manual transaction management
const tx = db.transaction()
try {
  const usersTx = tx.collection('users', userSchema)
  // Perform operations
  await usersTx.insertOne(userData)
  tx.commit()
} catch (error) {
  tx.rollback()
  throw error
} finally {
  tx[Symbol.dispose]() // Cleanup if not committed
}
```

### Query Caching

```typescript
// Enable for entire database
const db = new Strata({
  database: 'app.db',
  enableCache: true  // Cache for all collections
})

// Enable for specific collection
const users = db.collection('users', userSchema, { enableCache: true })

// Disable for specific collection when global cache is enabled
const tempCollection = db.collection('temp', tempSchema, { enableCache: false })
```

### Cancellation & Retries (v0.4.0+)

```typescript
import { AbortedError, defaultShouldRetry } from 'stratadb'

// ============================================
// CANCELLATION
// ============================================

// Simple timeout
const results = await users.find({}, { signal: AbortSignal.timeout(5000) })

// Manual control with AbortController
const controller = new AbortController()
setTimeout(() => controller.abort(), 5000)
await users.find({}, { signal: controller.signal })

// Cancel transactions
await db.execute(async (tx) => {
  await tx.collection('users', userSchema).insertOne(doc)
}, { signal: controller.signal })

// Handle cancellation errors
try {
  await users.find({}, { signal: AbortSignal.timeout(100) })
} catch (error) {
  if (error instanceof AbortedError) {
    console.log('Cancelled:', error.reason)
  }
}

// ============================================
// RETRY CONFIGURATION LEVELS
// ============================================

// Database level (applies to all operations)
const db = new Strata({
  database: 'app.db',
  retry: { retries: 3, minTimeout: 100, maxTimeout: 5000 }
})

// Collection level (overrides database)
const users = db.collection('users', userSchema, {
  retry: { retries: 5, minTimeout: 50 }
})

// Disable retries for a collection
const logs = db.collection('logs', logSchema, { retry: false })

// Operation level (overrides collection)
await users.insertOne(doc, { retry: { retries: 10 } })

// Disable retries for specific operation
await users.find({}, { retry: false })

// ============================================
// FULL RETRY OPTIONS
// ============================================

await users.insertOne(doc, {
  retry: {
    retries: 5,              // Max retry attempts
    factor: 2,               // Exponential backoff multiplier
    minTimeout: 100,         // Initial delay (ms)
    maxTimeout: 10000,       // Max delay between retries (ms)
    maxRetryTime: 30000,     // Overall timeout for all retries (ms)
    randomize: true,         // Add jitter

    // Logging callback
    onFailedAttempt: (ctx) => {
      console.log(`Attempt ${ctx.attemptNumber} failed: ${ctx.error.message}`)
      console.log(`Retrying in ${ctx.delay}ms (${ctx.retriesLeft} left)`)
    },

    // Custom retry decision
    shouldRetry: (ctx) => {
      return defaultShouldRetry(ctx) && ctx.elapsedTime < 10000
    },

    // Control retry budget
    shouldConsumeRetry: (ctx) => {
      return !ctx.error.message.includes('rate limit')
    }
  }
})

// ============================================
// COMBINING CANCELLATION + RETRIES
// ============================================

const controller = new AbortController()
setTimeout(() => controller.abort(), 10000) // 10s overall timeout

await users.insertOne(doc, {
  signal: controller.signal,
  retry: { retries: 5, minTimeout: 500 }
})
```

## Error Handling

```typescript
import {
  ValidationError,
  UniqueConstraintError,
  QueryError,
  TransactionError,
  AbortedError
} from 'stratadb'

try {
  await users.insertOne(invalidData, { signal: AbortSignal.timeout(5000) })
} catch (error) {
  if (error instanceof AbortedError) {
    console.log('Operation cancelled or timed out')
    console.log('Reason:', error.reason)
  } else if (error instanceof ValidationError) {
    console.log(`Validation error on field: ${error.field}`)
    console.log(`Value that failed: ${error.value}`)
    console.log(`Error message: ${error.message}`)
  } else if (error instanceof UniqueConstraintError) {
    console.log(`Unique constraint violation on field: ${error.field}`)
    console.log(`Duplicate value: ${error.value}`)
  } else if (error instanceof QueryError) {
    console.log(`Query error: ${error.message}`)
  } else {
    console.log(`Unexpected error: ${error}`)
  }
}
```

## Utility Functions

### ID Generation

```typescript
import { generateId } from 'stratadb'

const newId = generateId()  // Generates UUID v7 string
```

### Timestamps

```typescript
import {
  nowTimestamp,
  timestampToDate,
  dateToTimestamp,
  isTimestampInRange,
  timestampDiff
} from 'stratadb'

const now = nowTimestamp()                    // Current timestamp
const date = timestampToDate(now)             // Convert to Date
const timestamp = dateToTimestamp(new Date()) // Convert Date to timestamp
const inRange = isTimestampInRange(now, start, end)  // Check range
const diff = timestampDiff(timestamp1, timestamp2)   // Calculate difference
```

## Common Patterns

### Upsert Operations

```typescript
// Update or insert a document with a specific ID
const result = await users.updateOne(
  'specific-id',              // Explicit ID to update or insert
  { name: 'New Name' },       // Update data
  { upsert: true }            // Create new document if ID doesn't exist
)

// Note: Unlike MongoDB's update(findCriteria, updateData, {upsert:true}),
// StrataDB's updateOne requires an explicit ID. If upsert=true and the ID doesn't exist,
// it creates a new document with that specific ID.
```

### Pagination

```typescript
// Offset-based pagination
const getPaginatedUsers = async (page: number, limit: number, filter: any = {}) => {
  const skip = (page - 1) * limit

  const data = await users.find(filter, {
    sort: { createdAt: -1 },
    skip,
    limit
  })

  const total = await users.count(filter)

  return {
    data,
    pagination: {
      page,
      limit,
      total,
      pages: Math.ceil(total / limit)
    }
  }
}

// Cursor-based pagination (v0.3.0+) - more efficient for large datasets
const page1 = await users.find({}, { sort: { createdAt: -1 }, limit: 20 })
const page2 = await users.find({}, {
  sort: { createdAt: -1 },
  limit: 20,
  cursor: { after: page1.at(-1)?._id }
})
// Go back: cursor: { before: page2[0]._id }
```

### Bulk Operations

```typescript
// Bulk insert with error handling
const bulkInsert = async (usersData: User[]) => {
  try {
    const result = await users.insertMany(usersData, { ordered: false }) // Continue on error
    return {
      success: true,
      inserted: result.insertedCount,
      errors: result.errors
    }
  } catch (error) {
    return {
      success: false,
      error: error.message
    }
  }
}
```

This reference provides a comprehensive overview of StrataDB's API for quick lookup during development.