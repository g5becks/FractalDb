# Getting Started

This guide will help you set up StrataDB in your Bun project.

## Prerequisites

::: danger Required: Bun Runtime
StrataDB **only works with Bun**. Make sure you have [Bun](https://bun.sh) installed:

```bash
curl -fsSL https://bun.sh/install | bash
```
:::

## Installation

```bash
bun add stratadb
```

## Quick Start

### 1. Define Your Document Type

```typescript
import type { Document } from 'stratadb'

type User = Document<{
  name: string
  email: string
  age: number
  role: 'admin' | 'user'
}>
```

The `Document` type adds `id`, `createdAt`, and `updatedAt` fields automatically.

### 2. Create a Schema

```typescript
import { createSchema } from 'stratadb'

const userSchema = createSchema<User>()
  .field('name', { type: 'TEXT', indexed: true })
  .field('email', { type: 'TEXT', indexed: true, unique: true })
  .field('age', { type: 'INTEGER', indexed: true })
  .field('role', { type: 'TEXT' })
  .build()
```

### 3. Open Database and Create Collection

```typescript
import { StrataDBClass } from 'stratadb'

const db = new StrataDBClass({ database: 'myapp.db' })
const users = db.collection('users', userSchema)
```

### 4. CRUD Operations

```typescript
// Insert
const result = await users.insertOne({
  name: 'Alice',
  email: 'alice@example.com',
  age: 30,
  role: 'admin'
})
console.log(result.document.id) // Auto-generated UUID

// Find
const admins = await users.find({ role: 'admin' })
const alice = await users.findOne({ email: 'alice@example.com' })

// Update
await users.updateOne(result.document.id, { age: 31 })

// Delete
await users.deleteOne(result.document.id)
```

### 5. Close Database

```typescript
db.close()

// Or use Symbol.dispose for automatic cleanup
using db = new StrataDBClass({ database: 'myapp.db' })
// Database closes automatically when scope exits
```

## In-Memory Database

For testing or temporary data, use an in-memory database:

```typescript
const db = new StrataDBClass({ database: ':memory:' })
```

## Database Configuration

The database constructor accepts several configuration options:

```typescript
const db = new StrataDBClass({
  // Database path or ':memory:'
  database: 'myapp.db',

  // Custom ID generator (default: crypto.randomUUID())
  idGenerator: () => `custom-${Date.now()}`,

  // Cleanup callback when database closes
  onClose: () => console.log('Database closed'),

  // Enable query caching for performance (default: false)
  enableCache: true
})
```

### Query Caching

Query caching improves performance for repeated query patterns:

```typescript
// Enable caching globally for all collections
const db = new StrataDBClass({
  database: 'app.db',
  enableCache: true  // 23-70% faster for repeated queries
})

// Or enable per collection
const users = db.collection('users', userSchema, { enableCache: true })
```

::: tip Performance
Caching provides significant performance improvements (23-70% faster) for repeated queries, but uses memory (up to 500 cached templates per collection). See the [Performance Guide](/guide/performance) for details.
:::

## Next Steps

- [Documents](/guide/documents) - Understand document structure
- [Schemas](/guide/schemas) - Advanced schema configuration
- [Queries](/guide/queries) - Query operators and filters
- [Performance](/guide/performance) - Optimization and caching
