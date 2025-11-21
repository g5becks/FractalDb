# StrataDB

⚡ Type-safe document database built on SQLite with MongoDB-like queries. Zero runtime overhead, maximum type safety, powered by Bun.

## Features

- 🔒 **Full Type Safety** - End-to-end type checking with TypeScript, including SQLite type validation
- 🚀 **MongoDB-like Queries** - Familiar query syntax with compile-time validation
- ⚡ **High Performance** - Built on SQLite JSONB with generated columns
- 🎯 **Minimal Dependencies** - Only `@standard-schema/spec` and `fast-safe-stringify` at runtime
- 🔧 **Flexible Validation** - Use any Standard Schema validator (Zod, ArkType, Valibot, etc.)
- 📦 **Clean API** - Both fluent and declarative styles supported with full IntelliSense

## Quick Start

```typescript
import { StrataDB, type Document } from 'stratadb';

// Define your document type
type User = Document<{
  name: string;
  email: string;
  age: number;
}>;

// Create database and collection with fluent API
using db = new StrataDB({ database: './app.db' });

const users = db.collection<User>('users')
  .field('name', { type: 'TEXT', indexed: true })
  .field('email', { type: 'TEXT', indexed: true, unique: true })
  .field('age', { type: 'INTEGER', indexed: true })
  .timestamps(true)
  .build();

// Insert documents
await users.insertOne({
  name: 'Alice',
  email: 'alice@example.com',
  age: 30
});

// Query with full type safety
const results = await users.find({
  age: { $gte: 18 },
  email: { $regex: /@example\.com$/ }
});
```

## Type Safety

StrataDB ensures compile-time type safety between TypeScript types and SQLite types:

```typescript
type User = Document<{
  name: string;
  age: number;
  active: boolean;
}>;

// ✅ Correct - types align
db.collection<User>('users')
  .field('name', { type: 'TEXT' })      // string → TEXT ✓
  .field('age', { type: 'INTEGER' })    // number → INTEGER ✓
  .field('active', { type: 'BOOLEAN' }) // boolean → BOOLEAN ✓
  .build();

// ❌ Compiler error - type mismatch
db.collection<User>('users')
  .field('name', { type: 'INTEGER' })   // Error: string cannot use INTEGER
  .field('age', { type: 'TEXT' })       // Error: number cannot use TEXT
  .build();
```

## API Styles

StrataDB supports both fluent and declarative API styles:

### Fluent API (Inline)
```typescript
const users = db.collection<User>('users')
  .field('name', { type: 'TEXT', indexed: true })
  .field('email', { type: 'TEXT', indexed: true, unique: true })
  .build();
```

### Declarative API (Separate Schema)
```typescript
import { createSchema } from 'stratadb';

const userSchema = createSchema<User>()
  .field('name', { type: 'TEXT', indexed: true })
  .field('email', { type: 'TEXT', indexed: true, unique: true })
  .build();

const users = db.collection<User>('users', userSchema);
```

## Installation

```bash
bun add stratadb
```

## Documentation

See [DESIGN.md](./DESIGN.md) for comprehensive documentation and API reference.

## Development

```bash
# Install dependencies
bun install

# Run tests
bun test

# Build
bun run build

# Type check
bun run typecheck

# Lint & format
bun run lint:fix
```

## License

MIT
