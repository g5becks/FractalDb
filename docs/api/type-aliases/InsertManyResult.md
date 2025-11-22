[stratadb](../index.md) / InsertManyResult

# Type Alias: InsertManyResult\<T\>

```ts
type InsertManyResult<T> = object;
```

Defined in: [src/collection-types.ts:30](https://github.com/g5becks/StrataDB/blob/89bee4bbe54bb52f1f1308d5950da4d385abbe16/src/collection-types.ts#L30)

Result of inserting multiple documents into a collection.

## Remarks

Contains all successfully inserted documents with their generated IDs.

## Type Parameters

### T

`T` *extends* [`Document`](Document.md)

The document type

## Properties

### acknowledged

```ts
readonly acknowledged: true;
```

Defined in: [src/collection-types.ts:38](https://github.com/g5becks/StrataDB/blob/89bee4bbe54bb52f1f1308d5950da4d385abbe16/src/collection-types.ts#L38)

Whether the insert operation succeeded

***

### documents

```ts
readonly documents: readonly T[];
```

Defined in: [src/collection-types.ts:32](https://github.com/g5becks/StrataDB/blob/89bee4bbe54bb52f1f1308d5950da4d385abbe16/src/collection-types.ts#L32)

Array of inserted documents with their generated IDs

***

### insertedCount

```ts
readonly insertedCount: number;
```

Defined in: [src/collection-types.ts:35](https://github.com/g5becks/StrataDB/blob/89bee4bbe54bb52f1f1308d5950da4d385abbe16/src/collection-types.ts#L35)

Number of documents successfully inserted
