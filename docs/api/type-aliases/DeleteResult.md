[stratadb](../index.md) / DeleteResult

# Type Alias: DeleteResult

```ts
type DeleteResult = object;
```

Defined in: [src/collection-types.ts:91](https://github.com/g5becks/StrataDB/blob/7791c9d2c0eca8b064c87359859d54870cd83af8/src/collection-types.ts#L91)

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

Defined in: [src/collection-types.ts:93](https://github.com/g5becks/StrataDB/blob/7791c9d2c0eca8b064c87359859d54870cd83af8/src/collection-types.ts#L93)

Number of documents deleted
