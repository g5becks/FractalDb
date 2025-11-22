[stratadb](../index.md) / UpdateResult

# Type Alias: UpdateResult

```ts
type UpdateResult = object;
```

Defined in: [src/collection-types.ts:69](https://github.com/g5becks/StrataDB/blob/7791c9d2c0eca8b064c87359859d54870cd83af8/src/collection-types.ts#L69)

Result of an update operation.

## Remarks

Provides statistics about documents matched and modified by the update.
Since SQLite is ACID and local, if this function returns without throwing,
the operation succeeded.

## Example

```typescript
const result = await users.updateMany(
  { status: 'inactive' },
  { $set: { status: 'deleted' } }
);
console.log(`Matched: ${result.matchedCount}, Modified: ${result.modifiedCount}`);
```

## Properties

### matchedCount

```ts
readonly matchedCount: number;
```

Defined in: [src/collection-types.ts:71](https://github.com/g5becks/StrataDB/blob/7791c9d2c0eca8b064c87359859d54870cd83af8/src/collection-types.ts#L71)

Number of documents that matched the filter

***

### modifiedCount

```ts
readonly modifiedCount: number;
```

Defined in: [src/collection-types.ts:74](https://github.com/g5becks/StrataDB/blob/7791c9d2c0eca8b064c87359859d54870cd83af8/src/collection-types.ts#L74)

Number of documents that were actually modified
