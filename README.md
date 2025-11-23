# StrataDB

A type-safe **embedded document database** for Bun. No separate server process, no network overhead - just import and use.

## Why Embedded?

Unlike MongoDB or other client-server databases, StrataDB runs **in-process** with your application:

- **Zero setup** - No database server to install or manage
- **Zero latency** - Direct memory access, no network round-trips
- **Single file** - Your entire database is one portable `.db` file
- **Serverless ready** - Perfect for edge functions, CLI tools, and desktop apps

## Features

- **Embedded SQLite** - Runs in-process via `bun:sqlite`, no external dependencies
- **Full Type Safety** - Compile-time validation of queries, schemas, and results
- **MongoDB-like API** - Familiar operators: `$eq`, `$gt`, `$in`, `$and`, `$or`, etc.
- **JSONB Storage** - Flexible documents with indexed generated columns
- **Portable** - Single file database, easy to backup and deploy
- **Text Search** - Multi-field search with case-insensitive matching
- **Cursor Pagination** - Efficient, stable pagination for large datasets
- **Field Projection** - Clean `select`/`omit` helpers for controlling returned fields

## Installation

```bash
bun add stratadb
```

Requires [Bun](https://bun.sh) runtime.

> **Note:** StrataDB is ESM-only. It does not provide CommonJS exports.

## Quick Start

```typescript
import { Strata, createSchema, type Document } from 'stratadb'

type User = Document<{
  name: string
  email: string
  age: number
}>

const schema = createSchema<User>()
  .field('name', { type: 'TEXT', indexed: true })
  .field('email', { type: 'TEXT', indexed: true, unique: true })
  .field('age', { type: 'INTEGER', indexed: true })
  .timestamps(true)
  .build()

using db = new Strata({ database: 'app.db' })
const users = db.collection('users', schema)

// Insert
const user = await users.insertOne({
  name: 'Alice',
  email: 'alice@example.com',
  age: 30
})

// Query
const adults = await users.find({
  age: { $gte: 18 },
  email: { $endsWith: '@example.com' }
})

// Update
await users.updateOne(user._id, { age: 31 })

// Delete
await users.deleteOne(user._id)

// Text search across fields
const results = await users.search('alice', ['name', 'email'])

// Cursor pagination for large datasets
const page1 = await users.find({}, { sort: { createdAt: -1 }, limit: 20 })
const page2 = await users.find(
  {},
  { sort: { createdAt: -1 }, limit: 20, cursor: { after: page1.at(-1)?._id } }
)

// Field projection
const names = await users.find({}, { select: ['name', 'email'] })
const safe = await users.find({}, { omit: ['password'] })
```

## Type Safety

TypeScript validates schema types against field definitions:

```typescript
// Correct - types match
db.collection<User>('users')
  .field('name', { type: 'TEXT' })      // string -> TEXT
  .field('age', { type: 'INTEGER' })    // number -> INTEGER
  .build()

// Compile error - type mismatch
db.collection<User>('users')
  .field('name', { type: 'INTEGER' })   // Error: string cannot use INTEGER
  .build()
```

## Documentation

See the [documentation](https://g5becks.github.io/StrataDB/) for complete guides:

- [Getting Started](https://g5becks.github.io/StrataDB/guide/getting-started)
- [Collections](https://g5becks.github.io/StrataDB/guide/collections)
- [Queries](https://g5becks.github.io/StrataDB/guide/queries)
- [Schemas](https://g5becks.github.io/StrataDB/guide/schemas)
- [Transactions](https://g5becks.github.io/StrataDB/guide/transactions)

## Development

```bash
bun install      # Install dependencies
bun test         # Run tests
bun run build    # Build
bun run typecheck # Type check
```

## License

MIT
