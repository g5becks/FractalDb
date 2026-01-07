[stratadb](../index.md) / DeleteResult

# Type Alias: DeleteResult

```ts
type DeleteResult = object;
```

Defined in: [src/collection-types.ts:97](https://github.com/g5becks/StrataDb/blob/56b93c15dc2c602cd539356668e05ed574e9a8c7/src/collection-types.ts#L97)

Result of a delete operation.

## Remarks

Provides the count of documents deleted by the operation.
Since SQLite is ACID and local, if this function returns without throwing,
the operation succeeded.

## Example

```typescript
const result = await users.deleteMany({ status: 'inactive' });
console.log(`Deleted ${result.deletedCount} users`);
```

## Properties

### deletedCount

```ts
readonly deletedCount: number;
```

Defined in: [src/collection-types.ts:99](https://github.com/g5becks/StrataDb/blob/56b93c15dc2c602cd539356668e05ed574e9a8c7/src/collection-types.ts#L99)

Number of documents deleted
