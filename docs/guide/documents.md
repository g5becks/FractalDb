# Documents

Documents are the core data structure in StrataDB. Every document has a unique ID and optional timestamps.

## The Document Type

```typescript
import type { Document } from 'stratadb'

// Your data fields
type UserData = {
  name: string
  email: string
  age: number
}

// Document adds readonly _id
type User = Document<UserData>

// Equivalent to:
type User = {
  readonly _id: string
  readonly name: string
  readonly email: string
  readonly age: number
}
// Note: createdAt/updatedAt are added when you enable timestamps in the schema
```

## Auto-Generated Fields

When you insert a document, StrataDB automatically generates:

| Field | Type | Description |
|-------|------|-------------|
| `_id` | `string` | UUID v4 (or custom if provided) |
| `createdAt` | `number` | Unix timestamp at insertion (if timestamps enabled) |
| `updatedAt` | `number` | Unix timestamp, updated on modifications (if timestamps enabled) |

```typescript
const user = await users.insertOne({
  name: 'Alice',
  email: 'alice@example.com',
  age: 30
})

console.log(user)
// {
//   _id: '550e8400-e29b-41d4-a716-446655440000',
//   name: 'Alice',
//   email: 'alice@example.com',
//   age: 30,
//   createdAt: 1703001234567,  // if timestamps enabled
//   updatedAt: 1703001234567   // if timestamps enabled
// }
```

## Custom IDs

You can provide your own ID:

```typescript
await users.insertOne({
  _id: 'custom-id-123',
  name: 'Bob',
  email: 'bob@example.com',
  age: 25
})
```

Or configure a custom ID generator for the entire database:

```typescript
const db = new Strata({
  database: 'app.db',
  idGenerator: () => `user_${Date.now()}_${Math.random().toString(36).slice(2)}`
})
```

### Using nanoid

[nanoid](https://github.com/ai/nanoid) is a popular choice for compact, URL-safe IDs:

```bash
bun add nanoid
```

```typescript
import { nanoid } from 'nanoid'
import { Strata } from 'stratadb'

const db = new Strata({
  database: 'app.db',
  idGenerator: () => nanoid()  // e.g., "V1StGXR8_Z5jdHi6B-myT"
})

// Or with custom length
const db = new Strata({
  database: 'app.db',
  idGenerator: () => nanoid(10)  // e.g., "IRFa-VaY2b"
})
```

### Using ULID

[ULID](https://github.com/ulid/spec) provides sortable, unique IDs:

```bash
bun add ulid
```

```typescript
import { ulid } from 'ulid'
import { Strata } from 'stratadb'

const db = new Strata({
  database: 'app.db',
  idGenerator: () => ulid()  // e.g., "01ARZ3NDEKTSV4RRFFQ69G5FAV"
})
```

## Nested Objects

Documents can contain nested objects and arrays:

```typescript
type Post = Document<{
  title: string
  content: string
  author: {
    name: string
    avatar: string
  }
  tags: string[]
  metadata: {
    views: number
    likes: number
  }
}>
```

::: tip Indexing Nested Fields
To index nested fields, use JSON path syntax in your schema:

```typescript
.field('authorName', {
  path: '$.author.name',
  type: 'TEXT',
  indexed: true
})
```
:::

## Storage Format

Internally, documents are stored as JSONB in SQLite:

- `_id` column: TEXT PRIMARY KEY
- `body` column: BLOB (JSONB containing all document fields)
- `createdAt` column: INTEGER (if timestamps enabled)
- `updatedAt` column: INTEGER (if timestamps enabled)
- Generated columns for indexed fields (e.g., `_name`, `_email`)

This gives you the flexibility of document storage with the query performance of relational indexes.
