[stratadb](../index.md) / Document

# Type Alias: Document\<T\>

```ts
type Document<T> = object & ReadonlyDeep<T>;
```

Defined in: [src/core-types.ts:60](https://github.com/g5becks/StrataDB/blob/7791c9d2c0eca8b064c87359859d54870cd83af8/src/core-types.ts#L60)

Document type with immutable ID.

## Type Declaration

### \_id

```ts
readonly _id: string;
```

Unique identifier for the document (immutable)

## Type Parameters

### T

`T` = `Record`\<`string`, `unknown`\>

The document shape (your custom fields)

## Remarks

Generic type that combines your custom document fields with an immutable ID.
The ID is automatically managed by StrataDB and cannot be updated after creation.
If not provided during insert, a unique ID will be auto-generated.

This design allows for clean, composable document types:
- `type User = Document<{ name: string; email: string; }>`
- No need for `extends` or `&` intersections
- Fully type-safe with IntelliSense support

## Example

```typescript
// Simple document type
type User = Document<{
  name: string;
  email: string;
  age: number;
}>;

// Nested document structure
type Product = Document<{
  name: string;
  price: number;
  inventory: {
    stock: number;
    warehouse: string;
  };
}>;

// With arrays and optional fields
type Post = Document<{
  title: string;
  content: string;
  published: boolean;
  tags: string[];
  author?: {
    name: string;
    email: string;
  };
}>;

// All types work seamlessly with collections
using db = new StrataDB({ database: './app.db' });
const users = db.collection<User>('users', schema);
```
