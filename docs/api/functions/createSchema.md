[stratadb](../index.md) / createSchema

# Function: createSchema()

```ts
function createSchema<T>(): SchemaBuilder<T>;
```

Defined in: [src/schema-builder.ts:246](https://github.com/g5becks/StrataDB/blob/89bee4bbe54bb52f1f1308d5950da4d385abbe16/src/schema-builder.ts#L246)

Creates a new schema builder for defining collection schemas.

## Type Parameters

### T

`T` *extends* [`Document`](../type-aliases/Document.md)

The document type extending Document

## Returns

[`SchemaBuilder`](../type-aliases/SchemaBuilder.md)\<`T`\>

A new SchemaBuilder instance

## Remarks

This factory function creates a new `SchemaBuilder<T>` that provides
a fluent API for constructing schemas. The builder ensures type safety
at every step and returns an immutable schema definition when built.

## Example

```typescript
import { createSchema, type Document } from 'stratadb';

type User = Document<{
  name: string;
  email: string;
  age: number;
  status: 'active' | 'inactive';
}>;

// ✅ Create and build schema
const userSchema = createSchema<User>()
  .field('name', { type: 'TEXT', indexed: true })
  .field('email', { type: 'TEXT', indexed: true, unique: true })
  .field('age', { type: 'INTEGER', indexed: true })
  .field('status', { type: 'TEXT', indexed: true })
  .compoundIndex('age_status', ['age', 'status'])
  .timestamps(true)
  .validate((doc): doc is User => {
    return typeof doc === 'object' &&
           doc !== null &&
           'name' in doc &&
           typeof doc.name === 'string' &&
           'email' in doc &&
           typeof doc.email === 'string';
  })
  .build();

// ✅ Use with database
const users = db.collection<User>('users', userSchema);
```
