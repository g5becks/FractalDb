[stratadb](../index.md) / DeleteResult

# Type Alias: DeleteResult

```ts
type DeleteResult = object;
```

Defined in: [src/collection-types.ts:64](https://github.com/g5becks/StrataDB/blob/89bee4bbe54bb52f1f1308d5950da4d385abbe16/src/collection-types.ts#L64)

Result of a delete operation.

## Remarks

Provides the count of documents deleted by the operation.

## Properties

### acknowledged

```ts
readonly acknowledged: true;
```

Defined in: [src/collection-types.ts:69](https://github.com/g5becks/StrataDB/blob/89bee4bbe54bb52f1f1308d5950da4d385abbe16/src/collection-types.ts#L69)

Whether the delete operation succeeded

***

### deletedCount

```ts
readonly deletedCount: number;
```

Defined in: [src/collection-types.ts:66](https://github.com/g5becks/StrataDB/blob/89bee4bbe54bb52f1f1308d5950da4d385abbe16/src/collection-types.ts#L66)

Number of documents deleted
