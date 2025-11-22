# Frequently Asked Questions (FAQ)

This section addresses common questions and concerns about using StrataDB.

## General Questions

### Q: Why does StrataDB require Bun specifically?

A: StrataDB is built on `bun:sqlite`, Bun's native SQLite binding. This provides several advantages:
- **Performance**: Direct native bindings without FFI overhead
- **Type Safety**: Bun's TypeScript-first approach aligns with StrataDB's type safety goals
- **Simplicity**: No need for additional SQLite packages or compilation steps

StrataDB will not work with Node.js or Deno because it depends on Bun's specific SQLite API.

### Q: How does StrataDB compare to MongoDB?

A: StrataDB provides a similar API to MongoDB with some key differences:

| Feature | MongoDB | StrataDB |
|---------|---------|----------|
| Type Safety | Runtime validation | Compile-time validation |
| Storage | MongoDB storage engine | SQLite with JSONB |
| Deployment | Separate database server | Embedded in application |
| Dependencies | Server + client | Just the library |
| Transaction Support | Multi-document ACID | Full ACID compliance |
| Performance | Complex query optimization | Optimized for simple queries |

StrataDB is ideal for applications that need the familiarity of MongoDB's API with the simplicity of SQLite.

### Q: How does StrataDB store documents?

A: StrataDB stores documents using a hybrid approach:

1. **JSONB storage**: Documents are stored in a `body` BLOB column as JSONB
2. **Generated columns**: Indexed fields are extracted to generated columns for fast queries
3. **Metadata**: Each document has `id`, `createdAt`, and `updatedAt` fields stored separately

Example table structure:
```sql
CREATE TABLE users (
  id TEXT PRIMARY KEY,
  body BLOB NOT NULL,                    -- JSONB document
  createdAt INTEGER NOT NULL,
  updatedAt INTEGER NOT NULL,
  _email TEXT GENERATED ALWAYS AS (jsonb_extract(body, '$.email')) VIRTUAL,  -- indexed field
  _age INTEGER GENERATED ALWAYS AS (jsonb_extract(body, '$.age')) VIRTUAL     -- indexed field
);
```

## Schema and Type Safety

### Q: What's the difference between `Document`, `DocumentInput`, and `DocumentUpdate`?

A: These types serve different purposes in the document lifecycle:

- **`Document<T>`**: The complete document type with `id`, `createdAt`, `updatedAt`
```typescript
type User = Document<{ name: string, email: string }>
// Results in: { id: string, name: string, email: string, createdAt: number, updatedAt: number }
```

- **`DocumentInput<T>`**: Type for creating new documents (id is auto-generated)
```typescript
const input: DocumentInput<User> = { name: "Alice", email: "alice@example.com" }
// Cannot include id, createdAt, updatedAt
```

- **`DocumentUpdate<T>`**: Type for updating documents (partial fields, cannot change id)
```typescript
const update: DocumentUpdate<User> = { name: "Alice Smith", email: undefined } // Set to undefined to delete
```

### Q: How do I handle nested object schemas?

A: StrataDB supports nested objects with proper type safety:

```typescript
type User = Document<{
  name: string
  profile: {
    bio: string
    preferences: {
      theme: 'light' | 'dark'
      notifications: boolean
    }
  }
}>

// You can index nested fields using custom paths
const userSchema = createSchema<User>()
  .field('name', { type: 'TEXT', indexed: true })
  .field('profile', { 
    path: '$.profile.bio',  // Custom path to nested property
    type: 'TEXT',
    indexed: true 
  })
  .build()
```

## Performance and Caching

### Q: When should I enable query caching?

A: Enable query caching when you have repetitive query patterns:

**Good candidates for caching:**
- Repeated queries with the same structure but different parameters
- Application code that performs similar queries in loops
- Frequently accessed data with stable query patterns

**Avoid caching when:**
- Queries are rarely repeated
- Memory usage is a concern (each collection can cache up to 500 templates)
- Query structures vary significantly

```typescript
// Enable caching for frequently repeated queries
const db = new StrataDBClass({ 
  database: 'app.db',
  enableCache: true  // Caching enabled globally
})

// Or for specific collections
const frequentQueries = db.collection('frequent', schema, { enableCache: true })
const oneOffQueries = db.collection('oneoff', schema, { enableCache: false })
```

### Q: How do I handle large documents or collections?

A: StrataDB is optimized for moderate document sizes. For large documents:

- Keep individual documents under 1-2MB for optimal performance
- Use references to other collections for large related data
- Consider document normalization for frequently queried data

```typescript
// Instead of embedding large data
type Post = Document<{
  title: string
  content: string  // Large text content
  comments: Comment[]  // Large nested array
}>

// Consider separating concerns
type Post = Document<{ 
  title: string
  contentRef: string  // Reference to content document
  commentCount: number
}>

type Comment = Document<{
  postId: string  // Reference back to post
  content: string
  authorId: string
}>
```

## Error Handling and Troubleshooting

### Q: What should I do when I get a UniqueConstraintError?

A: `UniqueConstraintError` occurs when you try to insert a document that violates a unique index:

```typescript
try {
  await users.insertOne({ email: 'duplicate@example.com' })
} catch (error) {
  if (error instanceof UniqueConstraintError) {
    console.log(`Field: ${error.field}`)  // 'email'
    console.log(`Value: ${error.value}`)  // 'duplicate@example.com'
    
    // Option 1: Update existing document
    // To update by field value, you'd first need to find the ID
    const user = await users.findOne({ email: 'duplicate@example.com' })
    if (user) {
      await users.updateOne(user.id, { name: 'New Name' })
    }
    
    // Option 2: Check existence first
    const existing = await users.findOne({ email: 'duplicate@example.com' })
    if (!existing) {
      await users.insertOne({ email: 'new@example.com', name: 'New User' })
    }
  }
}
```

### Q: How do I handle validation errors?

A: Validation errors provide detailed information about what went wrong:

```typescript
try {
  await users.insertOne({ name: 123 })  // name should be string
} catch (error) {
  if (error instanceof ValidationError) {
    console.log(`Field: ${error.field}`)    // 'name'
    console.log(`Value: ${error.value}`)    // 123
    console.log(`Message: ${error.message}`) // Detailed error message
    
    // Handle specific validation failures
    throw new Error(`Invalid ${error.field}: ${error.message}`)
  }
}
```

## Transactions and Concurrency

### Q: How do transactions work in StrataDB?

A: StrataDB provides full ACID transaction support:

```typescript
// Using the transaction helper
const result = await db.execute(async (tx) => {
  const usersTx = tx.collection('users', userSchema)
  const accountsTx = tx.collection('accounts', accountSchema)
  
  await usersTx.insertOne({ name: 'Alice', email: 'alice@example.com' })
  await accountsTx.insertOne({ userId: 'auto-generated-id', balance: 1000 })
  
  // If any operation fails, everything is rolled back
  return { success: true }
})

// Manual transaction management
const tx = db.transaction()
try {
  const usersTx = tx.collection('users', userSchema)
  // Perform operations
  tx.commit()
} catch (error) {
  tx.rollback()  // Optional: automatic with Symbol.dispose
}
```

### Q: Can I use StrataDB in multi-threaded environments?

A: StrataDB is designed to work with Bun's single-threaded event loop model. For concurrent access:

- Each Bun thread/process should create its own database instance
- Use connection pooling patterns if needed (though usually unnecessary with SQLite)
- Transactions are thread-safe within the same database instance

## Validation and Schema Evolution

### Q: How do I validate documents with Zod, Valibot, or other validators?

A: StrataDB supports the Standard Schema specification:

```typescript
import { z } from 'zod'
import { createSchema, wrapStandardSchema } from 'stratadb'

const UserZodSchema = z.object({
  name: z.string().min(1),
  email: z.string().email(),
  age: z.number().int().min(0)
})

const userSchema = createSchema<User>()
  .field('name', { type: 'TEXT', indexed: true })
  .field('email', { type: 'TEXT', indexed: true, unique: true })
  .validate(wrapStandardSchema(UserZodSchema))
  .build()
```

### Q: How do I handle schema changes over time?

A: StrataDB schemas are immutable definitions, but you can handle evolution:

```typescript
// 1. Create new schema with updated types
type UserV2 = Document<{
  name: string
  email: string
  age: number
  createdAt: number  // New field
}>

// 2. Handle migration in your application
async function migrateUser() {
  const oldUsers = await oldCollection.find({})
  for (const user of oldUsers) {
    // Transform and insert into new schema
    await newCollection.insertOne({
      ...user,
      createdAt: user.createdAt || Date.now()  // Provide default for new field
    })
  }
}
```

## Best Practices

### Q: What are common anti-patterns to avoid?

A: Common mistakes when using StrataDB:

1. **Blocking operations in loops**:
```typescript
// ❌ Don't do this - creates many separate queries
for (const userId of userIds) {
  await users.findById(userId)  // Many individual queries
}

// ✅ Do this instead - single query
const usersFound = await users.find({ id: { $in: userIds } })
```

2. **Not using indexes for frequently queried fields**:
```typescript
// ❌ Slow query without index
await users.find({ email: 'user@example.com' })

// ✅ Fast query with index 
const userSchema = createSchema<User>()
  .field('email', { type: 'TEXT', indexed: true, unique: true })
  .build()
```

3. **Storing binary data in documents**:
```typescript
// ❌ Don't store large binary data in JSONB
type User = Document<{
  profilePicture: Uint8Array  // Large binary data
}>

// ✅ Store file paths instead
type User = Document<{
  profilePicturePath: string  // Path to file in filesystem
}>
```