# Troubleshooting

This guide helps you diagnose and resolve common issues with StrataDB.

## Common Installation Issues

### Bun Runtime Required

**Problem**: Error during installation or runtime that mentions bun:sqlite

**Solution**: StrataDB only works with Bun. Make sure you're using Bun and not Node.js:

```bash
# Check if you're using Bun
bun --version

# Install using Bun
bun add stratadb

# Run using Bun
bun run your-script.ts
```

### Module Resolution Issues

**Problem**: Cannot resolve imports like `bun:sqlite` or `stratadb`

**Solution**: Ensure you're running with Bun and using the correct import syntax:

```typescript
// ✅ Correct import
import { StrataDBClass, createSchema } from 'stratadb'
import type { Document } from 'stratadb'
```

## Database Connection Issues

### File Permission Errors

**Problem**: Error like "SQLITE_CANTOPEN: unable to open database file"

**Solutions**:
1. Check that the directory exists and is writable:
```bash
mkdir -p ./data
chmod 755 ./data
```

2. Use absolute paths if relative paths fail:
```typescript
const db = new StrataDBClass({ 
  database: '/absolute/path/to/your/app.db' 
})
```

3. For temporary databases, use `:memory:`:
```typescript
const db = new StrataDBClass({ database: ':memory:' })
```

### Database Locked Errors

**Problem**: Error like "SQLITE_BUSY: database is locked"

**Solution**: This typically occurs with concurrent writes. StrataDB handles most cases automatically, but if you encounter this:

```typescript
// Use transactions for multiple operations
await db.execute(async (tx) => {
  const users = tx.collection('users', userSchema)
  // Perform multiple operations in one transaction
  await users.insertOne(data1)
  await users.insertOne(data2)
})
```

## Schema and Query Issues

### Invalid Query Operator Errors

**Problem**: Error like "Query error with operator '$invalidOp': Operator not recognized"

**Solution**: Check the supported operators. Common mistakes:

```typescript
// ❌ Invalid - no $contains operator
await users.find({ tags: { $contains: 'admin' } })

// ✅ Correct - use $in for array inclusion
await users.find({ tags: { $in: ['admin'] } })

// ✅ For string contains, use $like with wildcards
await users.find({ name: { $like: '%admin%' } })
```

### Type Safety Issues

**Problem**: TypeScript errors with query filters

**Solution**: Ensure your schema field types match your query types:

```typescript
// ❌ Type mismatch - querying number field with string
type User = Document<{ age: number }>
await users.find({ age: 'thirty' }) // TypeScript error

// ✅ Correct - matching types
await users.find({ age: 30 })
await users.find({ age: { $gte: 18 } })
```

### Schema Path Validation

**Problem**: Field not found in query when using nested properties

**Solution**: Make sure your schema defines the indexed field with correct path:

```typescript
type User = Document<{
  profile: {
    bio: string
  }
}>

// ❌ This won't work for indexed queries on nested fields
const userSchema = createSchema<User>()
  .field('profile', { type: 'TEXT', indexed: true }) // Indexes the whole object

// ✅ Correct - specify the exact path for indexing
const userSchema = createSchema<User>()
  .field('profileBio', { 
    path: '$.profile.bio', // Path to nested property
    type: 'TEXT', 
    indexed: true 
  })
  .build()

// Now this works:
await users.find({ 'profile.bio': 'software engineer' }) // Won't work with custom path
// Instead query what you indexed:
await users.find({ profileBio: 'software engineer' })
```

## Performance Issues

### Slow Query Performance

**Problem**: Queries are taking longer than expected

**Solutions**:

1. **Check indexing**: Ensure frequently queried fields are indexed:
```typescript
// ❌ Unindexed query - slow
const users = await users.find({ email: 'user@example.com' })

// ✅ Indexed query - fast
const userSchema = createSchema<User>()
  .field('email', { type: 'TEXT', indexed: true })
  .build()
```

2. **Use query caching** for repeated patterns:
```typescript
// Enable caching for frequently repeated queries
const db = new StrataDBClass({ 
  database: 'app.db', 
  enableCache: true 
})
```

3. **Optimize complex queries**:
```typescript
// ❌ Multiple OR conditions can be slow
await users.find({ 
  $or: [
    { status: 'active' },
    { status: 'pending' },
    { status: 'trial' }
  ] 
})

// ✅ Single $in query is more efficient
await users.find({ status: { $in: ['active', 'pending', 'trial'] } })
```

### Memory Usage Issues

**Problem**: High memory usage with query caching enabled

**Solution**: Monitor and limit cache size:

```typescript
// Disable caching for collections with varied queries
const oneOffQueries = db.collection('temp', schema, { enableCache: false })

// Or limit the cache size by disabling global cache and enabling only where needed
const db = new StrataDBClass({ 
  database: 'app.db',
  enableCache: false  // Disable globally
})

const frequentlyUsed = db.collection('users', userSchema, { enableCache: true })
```

## Validation and Data Issues

### Unique Constraint Violations

**Problem**: `UniqueConstraintError` when inserting documents

**Solutions**:

1. **Check for duplicates before inserting**:
```typescript
const existing = await users.findOne({ email: newEmail })
if (!existing) {
  await users.insertOne(newUser)
}
```

2. **Use upsert operations**:
```typescript
await users.updateOne(
  { email: 'user@example.com' },  // Search criteria
  { name: 'Updated Name' },       // Update data
  { upsert: true }                // Insert if not found
)
```

3. **Handle the error gracefully**:
```typescript
try {
  await users.insertOne(newUser)
} catch (error) {
  if (error instanceof UniqueConstraintError) {
    if (error.field === 'email') {
      // Handle duplicate email
      throw new Error('Email already registered')
    }
  }
  throw error // Re-throw other errors
}
```

### Validation Errors

**Problem**: `ValidationError` when inserting documents

**Solutions**:

1. **Validate before insertion**:
```typescript
try {
  const validatedUser = userSchema.validate(userData)
  if (validatedUser) {
    await users.insertOne(userData)
  }
} catch (error) {
  if (error instanceof ValidationError) {
    console.log(`Validation failed: ${error.message}`)
  }
}
```

2. **Debug validation issues**:
```typescript
// Use validateSync for synchronous validation
try {
  const validated = userSchema.validateSync(userData)
  await users.insertOne(validated)
} catch (error) {
  console.log('Validation error:', error)
  console.log('Problematic field:', error.field)
  console.log('Value that failed:', error.value)
}
```

## Transaction Issues

### Transaction Rollback Issues

**Problem**: Transaction doesn't rollback when expected

**Solution**: Make sure to properly handle errors in transactions:

```typescript
// ✅ Using execute helper (recommended)
const result = await db.execute(async (tx) => {
  const usersTx = tx.collection('users', userSchema)
  // If any operation throws, transaction automatically rolls back
  await usersTx.insertOne(userData)
  return { success: true }
})

// ✅ Manual transaction with proper error handling
const tx = db.transaction()
try {
  const users = tx.collection('users', userSchema)
  await users.insertOne(userData)
  await users.updateOne(someId, updateData)
  tx.commit()
} catch (error) {
  // tx.rollback() is called automatically by Symbol.dispose if not committed
  throw error
}
```

### Nested Transaction Issues

**Problem**: Issues with nested or concurrent transactions

**Solution**: StrataDB doesn't support nested transactions. Instead, pass transaction context:

```typescript
// ❌ Don't create nested transactions
async function outerFunction() {
  return await db.execute(async (tx1) => {
    await innerFunction() // This might try to create another transaction
  })
}

async function innerFunction() {
  return await db.execute(async (tx2) => { // ❌ Creates nested transaction
    // operations
  })
}

// ✅ Pass transaction to inner functions
async function outerFunction() {
  return await db.execute(async (tx) => {
    await innerFunction(tx) // Pass the existing transaction
  })
}

async function innerFunction(tx: Transaction) {
  const users = tx.collection('users', userSchema)
  // Use the passed transaction
  return await users.find({})
}
```

## Advanced Troubleshooting

### Debugging Slow Queries

Enable query logging to see the actual SQL being executed:

```typescript
// Add logging middleware (not built-in, but you can create one)
const originalFind = users.find
users.find = async function(...args) {
  console.time('find operation')
  const result = await originalFind.apply(this, args)
  console.timeEnd('find operation')
  return result
}
```

### Memory Leaks

If experiencing memory issues:

1. **Close databases properly**:
```typescript
// Use Symbol.dispose for automatic cleanup
using db = new StrataDBClass({ database: 'app.db' })
// db.close() called automatically when scope exits

// Or manually close
const db = new StrataDBClass({ database: 'app.db' })
// ... use db ...
db.close()
```

2. **Monitor collection instances** - avoid creating many collection instances:

```typescript
// ❌ Don't create collections repeatedly
function processUser(userId: string) {
  const users = db.collection('users', userSchema)  // New instance each time
  return users.findById(userId)
}

// ✅ Create collection once and reuse
const users = db.collection('users', userSchema)
function processUser(userId: string) {
  return users.findById(userId)  // Reuse same instance
}
```

### Testing Issues

**Problem**: Testing code that uses StrataDB

**Solution**: Use in-memory databases for tests:

```typescript
import { describe, test, expect, beforeEach, afterEach } from 'bun:test'

describe('User Service', () => {
  let db: StrataDBClass
  let users: Collection<User>

  beforeEach(() => {
    // Use in-memory database for tests
    db = new StrataDBClass({ database: ':memory:' })
    users = db.collection('users', userSchema)
  })

  afterEach(() => {
    db.close()
  })

  test('should create user', async () => {
    const result = await users.insertOne(userData)
    expect(result.document.id).toBeDefined()
  })
})
```

## Getting Help

If you encounter issues not covered here:

1. **Check the API documentation** for method signatures and options
2. **Review the examples** in the guide documentation
3. **Search for similar issues** in the repository (if open source)
4. **Create a minimal reproduction** to isolate the problem

For debugging, consider creating a minimal example that reproduces your issue:

```typescript
// Example of a minimal reproduction
import { StrataDBClass, createSchema, type Document } from 'stratadb'

type TestDoc = Document<{ name: string }>

const schema = createSchema<TestDoc>()
  .field('name', { type: 'TEXT', indexed: true })
  .build()

const db = new StrataDBClass({ database: ':memory:' })
const collection = db.collection('test', schema)

try {
  await collection.insertOne({ name: 'test' })
  console.log('Success!')
} catch (error) {
  console.error('Error:', error)
} finally {
  db.close()
}
```