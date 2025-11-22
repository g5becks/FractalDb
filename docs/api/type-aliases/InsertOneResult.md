[stratadb](../index.md) / InsertOneResult

# Type Alias: InsertOneResult\<T\>

```ts
type InsertOneResult<T> = object;
```

Defined in: [src/collection-types.ts:14](https://github.com/g5becks/StrataDB/blob/89bee4bbe54bb52f1f1308d5950da4d385abbe16/src/collection-types.ts#L14)

Result of inserting a single document into a collection.

## Remarks

Contains the inserted document with its generated ID and metadata.

## Type Parameters

### T

`T` *extends* [`Document`](Document.md)

The document type

## Properties

### acknowledged

```ts
readonly acknowledged: true;
```

Defined in: [src/collection-types.ts:19](https://github.com/g5becks/StrataDB/blob/89bee4bbe54bb52f1f1308d5950da4d385abbe16/src/collection-types.ts#L19)

Whether the insert operation succeeded

***

### document

```ts
readonly document: T;
```

Defined in: [src/collection-types.ts:16](https://github.com/g5becks/StrataDB/blob/89bee4bbe54bb52f1f1308d5950da4d385abbe16/src/collection-types.ts#L16)

The inserted document with its generated ID
