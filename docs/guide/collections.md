# Collections

Collections are the primary way to interact with documents in StrataDB.

## Creating Collections

### With Pre-built Schema

```typescript
import { StrataDBClass, createSchema, type Document } from 'stratadb'

type User = Document<{
  name: string
  email: string
}>

const userSchema = createSchema<User>()
  .field('email', { type: 'TEXT', indexed: true, unique: true })
  .build()

const db = new StrataDBClass({ database: 'app.db' })
const users = db.collection('users', userSchema)
```

### With Inline Schema (Fluent API)

```typescript
const users = db.collection<User>('users')
  .field('email', { type: 'TEXT', indexed: true, unique: true })
  .build()
```

## CRUD Operations

### Insert

```typescript
// Insert one
const result = await users.insertOne({
  name: 'Alice',
  email: 'alice@example.com'
})
console.log(result.document.id)

// Insert many
const results = await users.insertMany([
  { name: 'Bob', email: 'bob@example.com' },
  { name: 'Carol', email: 'carol@example.com' }
])
console.log(`Inserted ${results.insertedCount} documents`)
```

### Read

```typescript
// By ID
const user = await users.findById('user-123')

// Find one matching filter
const admin = await users.findOne({ role: 'admin' })

// Find all matching filter
const activeUsers = await users.find({ status: 'active' })

// Count
const count = await users.count({ role: 'admin' })
```

### Update

```typescript
// Update one by ID (partial update)
const updated = await users.updateOne('user-123', { name: 'Alice Smith' })

// Update with upsert (create if not exists)
await users.updateOne('user-456', { name: 'New User' }, { upsert: true })

// Update many matching filter
const result = await users.updateMany(
  { status: 'inactive' },
  { status: 'archived' }
)
console.log(`Updated ${result.modifiedCount} documents`)

// Replace entire document (keeps id and createdAt)
await users.replaceOne('user-123', {
  name: 'Alice Smith',
  email: 'alice.smith@example.com'
})
```

### Delete

```typescript
// Delete one by ID
const deleted = await users.deleteOne('user-123')
if (deleted) {
  console.log('User deleted')
}

// Delete many matching filter
const result = await users.deleteMany({ status: 'archived' })
console.log(`Deleted ${result.deletedCount} documents`)
```

## Error Handling

### Unique Constraint Violations

```typescript
import { UniqueConstraintError } from 'stratadb'

try {
  await users.insertOne({ email: 'existing@example.com' })
} catch (err) {
  if (err instanceof UniqueConstraintError) {
    console.error(`Duplicate ${err.field}: ${err.message}`)
  }
}
```

### Validation Errors

```typescript
import { ValidationError } from 'stratadb'

try {
  await users.insertOne({ email: 'invalid' })
} catch (err) {
  if (err instanceof ValidationError) {
    console.error('Validation failed:', err.message)
  }
}
```

## Collection Properties

```typescript
// Collection name (table name in SQLite)
console.log(users.name)  // 'users'

// Schema definition
console.log(users.schema.fields)
```

## Manual Validation

```typescript
// Async validation
const validUser = await users.validate(unknownData)

// Sync validation
const validUser = users.validateSync(unknownData)
```
