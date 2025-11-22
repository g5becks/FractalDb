[stratadb](../index.md) / UpdateResult

# Type Alias: UpdateResult

```ts
type UpdateResult = object;
```

Defined in: [src/collection-types.ts:47](https://github.com/g5becks/StrataDB/blob/89bee4bbe54bb52f1f1308d5950da4d385abbe16/src/collection-types.ts#L47)

Result of an update operation.

## Remarks

Provides statistics about documents matched and modified by the update.

## Properties

### acknowledged

```ts
readonly acknowledged: true;
```

Defined in: [src/collection-types.ts:55](https://github.com/g5becks/StrataDB/blob/89bee4bbe54bb52f1f1308d5950da4d385abbe16/src/collection-types.ts#L55)

Whether the update operation succeeded

***

### matchedCount

```ts
readonly matchedCount: number;
```

Defined in: [src/collection-types.ts:49](https://github.com/g5becks/StrataDB/blob/89bee4bbe54bb52f1f1308d5950da4d385abbe16/src/collection-types.ts#L49)

Number of documents that matched the filter

***

### modifiedCount

```ts
readonly modifiedCount: number;
```

Defined in: [src/collection-types.ts:52](https://github.com/g5becks/StrataDB/blob/89bee4bbe54bb52f1f1308d5950da4d385abbe16/src/collection-types.ts#L52)

Number of documents that were actually modified
