# StrataDB

Type-safe document database built on SQLite with MongoDB-like queries. Powered by Bun.

## Features

- **Full Type Safety** - Compile-time validation of queries, schemas, and SQLite types
- **MongoDB-like API** - Familiar operators: `$eq`, `$gt`, `$in`, `$and`, `$or`, etc.
- **High Performance** - JSONB storage with generated columns for indexed fields
- **Flexible Validation** - Use any Standard Schema validator (Zod, ArkType, Valibot)
- **Minimal Dependencies** - Only `@standard-schema/spec` and `fast-safe-stringify`

## Installation

```bash
bun add stratadb
```

Requires [Bun](https://bun.sh) runtime.

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

See the [documentation](https://g5becks.github.io/stratadb/) for complete guides:

- [Getting Started](https://g5becks.github.io/stratadb/guide/getting-started)
- [Collections](https://g5becks.github.io/stratadb/guide/collections)
- [Queries](https://g5becks.github.io/stratadb/guide/queries)
- [Schemas](https://g5becks.github.io/stratadb/guide/schemas)
- [Transactions](https://g5becks.github.io/stratadb/guide/transactions)

## Development

```bash
bun install      # Install dependencies
bun test         # Run tests
bun run build    # Build
bun run typecheck # Type check
```

## License

MIT
