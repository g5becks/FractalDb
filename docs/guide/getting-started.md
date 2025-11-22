# Getting Started

## Prerequisites

StrataDB requires [Bun](https://bun.sh). Install it with:

```bash
curl -fsSL https://bun.sh/install | bash
```

## Installation

```bash
bun add stratadb
```

## Basic Usage

```typescript
import { StrataDBClass, createSchema, type Document } from 'stratadb'

// 1. Define your document type
type User = Document<{
  name: string
  email: string
  age: number
}>

// 2. Create a schema with indexed fields
const userSchema = createSchema<User>()
  .field('name', { type: 'TEXT', indexed: true })
  .field('email', { type: 'TEXT', indexed: true, unique: true })
  .field('age', { type: 'INTEGER', indexed: true })
  .timestamps(true)  // adds createdAt/updatedAt
  .build()

// 3. Open database and create collection
using db = new StrataDBClass({ database: 'app.db' })
const users = db.collection('users', userSchema)

// 4. Insert
const user = await users.insertOne({
  name: 'Alice',
  email: 'alice@example.com',
  age: 30
})
console.log(user._id)  // auto-generated UUID

// 5. Query
const adults = await users.find({ age: { $gte: 18 } })
const alice = await users.findOne({ email: 'alice@example.com' })

// 6. Update
await users.updateOne(user._id, { age: 31 })

// 7. Delete
await users.deleteOne(user._id)
```

The `using` keyword automatically closes the database when the scope exits.

## In-Memory Database

For testing:

```typescript
using db = new StrataDBClass({ database: ':memory:' })
```

## Configuration Options

```typescript
const db = new StrataDBClass({
  database: 'app.db',

  // Custom ID generator (default: crypto.randomUUID())
  idGenerator: () => nanoid(),

  // Query caching for repeated patterns (default: false)
  enableCache: true
})
```

## Next Steps

- [Collections](/guide/collections) - CRUD operations and atomic methods
- [Queries](/guide/queries) - Query operators and filtering
- [Schemas](/guide/schemas) - Indexes, validation, and configuration
