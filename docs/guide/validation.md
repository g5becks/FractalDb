# Validation

StrataDB supports document validation using any [Standard Schema](https://github.com/standard-schema/standard-schema) compatible library, including **Zod**, **ArkType**, **Valibot**, and others.

## Using Zod

[Zod](https://zod.dev) is a popular TypeScript-first schema validation library.

```bash
bun add zod
```

```typescript
import { z } from 'zod'
import { createSchema, Strata, type Document } from 'stratadb'

// Define Zod schema
const UserSchema = z.object({
  name: z.string().min(1, 'Name is required'),
  email: z.string().email('Invalid email address'),
  age: z.number().int().min(0).max(150),
  role: z.enum(['admin', 'user']),
})

// Infer TypeScript type from Zod
type UserData = z.infer<typeof UserSchema>
type User = Document<UserData>

// Create StrataDB schema with Zod validation
const userSchema = createSchema<User>()
  .field('name', { type: 'TEXT', indexed: true })
  .field('email', { type: 'TEXT', indexed: true, unique: true })
  .field('age', { type: 'INTEGER', indexed: true })
  .field('role', { type: 'TEXT' })
  .validate((doc): doc is User => {
    const result = UserSchema.safeParse(doc)
    return result.success
  })
  .build()

const db = new Strata({ database: ':memory:' })
const users = db.collection('users', userSchema)

// Valid document - succeeds
await users.insertOne({
  name: 'Alice',
  email: 'alice@example.com',
  age: 30,
  role: 'admin'
})

// Invalid document - throws error
await users.insertOne({
  name: '',           // Too short
  email: 'not-email', // Invalid email
  age: -5,            // Negative age
  role: 'superuser'   // Invalid role
})
// Error: Document validation failed
```

## Using ArkType

[ArkType](https://arktype.io) provides excellent TypeScript integration with runtime validation.

```bash
bun add arktype
```

```typescript
import { type } from 'arktype'
import { createSchema, Strata, type Document } from 'stratadb'

// Define ArkType schema
const userType = type({
  name: 'string>0',           // Non-empty string
  email: 'string.email',      // Valid email
  age: 'integer>=0&<=150',    // Integer 0-150
  role: "'admin'|'user'",     // Literal union
})

// Infer TypeScript type from ArkType
type UserData = typeof userType.infer
type User = Document<UserData>

// Create StrataDB schema with ArkType validation
const userSchema = createSchema<User>()
  .field('name', { type: 'TEXT', indexed: true })
  .field('email', { type: 'TEXT', indexed: true, unique: true })
  .field('age', { type: 'INTEGER', indexed: true })
  .field('role', { type: 'TEXT' })
  .validate((doc): doc is User => {
    const result = userType(doc)
    return !(result instanceof type.errors)
  })
  .build()

const db = new Strata({ database: ':memory:' })
const users = db.collection('users', userSchema)

// Insert with validation
await users.insertOne({
  name: 'Bob',
  email: 'bob@example.com',
  age: 25,
  role: 'user'
})
```

## Using Valibot

[Valibot](https://valibot.dev) is a lightweight alternative to Zod.

```bash
bun add valibot
```

```typescript
import * as v from 'valibot'
import { createSchema, Strata, type Document } from 'stratadb'

// Define Valibot schema
const UserSchema = v.object({
  name: v.pipe(v.string(), v.minLength(1)),
  email: v.pipe(v.string(), v.email()),
  age: v.pipe(v.number(), v.integer(), v.minValue(0), v.maxValue(150)),
  role: v.picklist(['admin', 'user']),
})

type UserData = v.InferOutput<typeof UserSchema>
type User = Document<UserData>

const userSchema = createSchema<User>()
  .field('name', { type: 'TEXT', indexed: true })
  .field('email', { type: 'TEXT', indexed: true, unique: true })
  .field('age', { type: 'INTEGER', indexed: true })
  .field('role', { type: 'TEXT' })
  .validate((doc): doc is User => {
    const result = v.safeParse(UserSchema, doc)
    return result.success
  })
  .build()
```

## Custom Validation

You can also write custom validation without external libraries:

```typescript
const userSchema = createSchema<User>()
  .field('email', { type: 'TEXT', indexed: true, unique: true })
  .validate((doc): doc is User => {
    if (typeof doc !== 'object' || doc === null) return false

    const { name, email, age, role } = doc as Record<string, unknown>

    // Validate name
    if (typeof name !== 'string' || name.length === 0) return false

    // Validate email
    if (typeof email !== 'string' || !email.includes('@')) return false

    // Validate age
    if (typeof age !== 'number' || age < 0 || age > 150) return false

    // Validate role
    if (role !== 'admin' && role !== 'user') return false

    return true
  })
  .build()
```

## Validation on Update

Validation also runs on updates:

```typescript
// This will fail validation if the resulting document is invalid
await users.updateOne(userId, { age: -1 }) // Error: validation failed

// replaceOne validates the entire replacement document
await users.replaceOne(userId, {
  name: '',  // Invalid - will throw
  email: 'test@example.com',
  age: 30,
  role: 'user'
})
```

## Manual Validation

You can validate documents manually without inserting:

```typescript
try {
  const validUser = users.validateSync(unknownData)
  console.log('Valid:', validUser.name)
} catch (err) {
  if (err instanceof ValidationError) {
    console.error('Invalid document:', err.message)
  }
}

// Async version
const validUser = await users.validate(unknownData)
```
