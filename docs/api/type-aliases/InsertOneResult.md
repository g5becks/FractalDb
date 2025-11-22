[stratadb](../index.md) / InsertOneResult

# Type Alias: InsertOneResult\<T\>

```ts
type InsertOneResult<T> = T;
```

Defined in: [src/collection-types.ts:22](https://github.com/g5becks/StrataDB/blob/7791c9d2c0eca8b064c87359859d54870cd83af8/src/collection-types.ts#L22)

Result of inserting a single document into a collection.

## Type Parameters

### T

`T` *extends* [`Document`](Document.md)

The document type

## Remarks

Represents the inserted document with its generated ID.
Since SQLite is ACID and local, if this function returns without throwing,
the operation succeeded - no acknowledgment flag needed.

## Example

```typescript
const result = await users.insertOne({ name: 'Alice', email: 'alice@example.com' });
console.log(result._id); // Auto-generated UUID
```
