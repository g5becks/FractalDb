[stratadb](../index.md) / InsertManyResult

# Type Alias: InsertManyResult\<T\>

```ts
type InsertManyResult<T> = object;
```

Defined in: [src/collection-types.ts:44](https://github.com/g5becks/StrataDB/blob/7791c9d2c0eca8b064c87359859d54870cd83af8/src/collection-types.ts#L44)

Result of inserting multiple documents into a collection.

## Remarks

Contains all successfully inserted documents with their generated IDs and count.
Since SQLite is ACID and local, if this function returns without throwing,
all inserts succeeded.

## Example

```typescript
const result = await users.insertMany([
  { name: 'Alice', email: 'alice@example.com' },
  { name: 'Bob', email: 'bob@example.com' }
]);
console.log(`Inserted ${result.insertedCount} users`);
console.log('User IDs:', result.documents.map(d => d._id));
```

## Type Parameters

### T

`T` *extends* [`Document`](Document.md)

The document type

## Properties

### documents

```ts
readonly documents: readonly T[];
```

Defined in: [src/collection-types.ts:46](https://github.com/g5becks/StrataDB/blob/7791c9d2c0eca8b064c87359859d54870cd83af8/src/collection-types.ts#L46)

Array of inserted documents with their generated IDs

***

### insertedCount

```ts
readonly insertedCount: number;
```

Defined in: [src/collection-types.ts:49](https://github.com/g5becks/StrataDB/blob/7791c9d2c0eca8b064c87359859d54870cd83af8/src/collection-types.ts#L49)

Number of documents successfully inserted
