# What is StrataDB?

StrataDB is a **type-safe embedded document database** for the [Bun](https://bun.sh) runtime. It runs in-process with your application - no separate database server required.

::: warning Bun Only
StrataDB requires the Bun runtime. It will **not work** with Node.js or Deno as it depends on `bun:sqlite`.
:::

## Why Embedded?

Unlike MongoDB or PostgreSQL, StrataDB runs **inside your application process**:

- **Zero setup** - No database server to install, configure, or manage
- **Zero latency** - Direct memory access, no network round-trips
- **Single file** - Your entire database is one portable `.db` file
- **Serverless ready** - Perfect for edge functions, CLI tools, Electron apps

## Why StrataDB?

- **Type Safety First**: Every query, filter, and result is fully typed. Catch errors at compile time.
- **Familiar API**: If you know MongoDB, you know StrataDB. Same methods, better types.
- **SQLite Reliability**: Battle-tested SQLite storage with JSONB for flexible documents.
- **Portable**: Single file database - easy to backup, deploy, and version control.

## How It Works

StrataDB stores documents as JSONB in SQLite while creating **generated columns** for indexed fields. This gives you:

- Full document flexibility (store any JSON structure)
- Fast indexed queries (B-tree indexes on generated columns)
- Type-safe filters (TypeScript knows your schema)

```typescript
import { Strata, createSchema, type Document } from 'stratadb'

// Define your document type
type User = Document<{
  name: string
  email: string
  age: number
}>

// Create a schema with indexed fields
const userSchema = createSchema<User>()
  .field('name', { type: 'TEXT', indexed: true })
  .field('email', { type: 'TEXT', indexed: true, unique: true })
  .field('age', { type: 'INTEGER', indexed: true })
  .build()

// Open database and create collection
const db = new Strata({ database: 'app.db' })
const users = db.collection('users', userSchema)

// Type-safe operations
await users.insertOne({ name: 'Alice', email: 'alice@example.com', age: 30 })
const adults = await users.find({ age: { $gte: 18 } })
```

## Next Steps

- [Getting Started](/guide/getting-started) - Install and create your first database
- [Documents](/guide/documents) - Learn about the Document type
- [Schemas](/guide/schemas) - Define schemas with indexes and validation
- [Queries](/guide/queries) - Write type-safe queries
- [Performance](/guide/performance) - Optimization and query caching
