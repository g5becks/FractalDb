[stratadb](../index.md) / UpdateResult

# Type Alias: UpdateResult

```ts
type UpdateResult = object;
```

Defined in: [src/collection-types.ts:75](https://github.com/g5becks/StrataDb/blob/56b93c15dc2c602cd539356668e05ed574e9a8c7/src/collection-types.ts#L75)

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

Defined in: [src/collection-types.ts:77](https://github.com/g5becks/StrataDb/blob/56b93c15dc2c602cd539356668e05ed574e9a8c7/src/collection-types.ts#L77)

Number of documents that matched the filter

***

### modifiedCount

```ts
readonly modifiedCount: number;
```

Defined in: [src/collection-types.ts:80](https://github.com/g5becks/StrataDb/blob/56b93c15dc2c602cd539356668e05ed574e9a8c7/src/collection-types.ts#L80)

Number of documents that were actually modified
