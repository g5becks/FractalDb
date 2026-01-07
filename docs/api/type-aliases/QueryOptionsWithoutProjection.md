[stratadb](../index.md) / QueryOptionsWithoutProjection

# Type Alias: QueryOptionsWithoutProjection&lt;T&gt;

```ts
type QueryOptionsWithoutProjection<T> = QueryOptionsBase<T> & object;
```

Defined in: [src/query-options-types.ts:641](https://github.com/g5becks/StrataDb/blob/56b93c15dc2c602cd539356668e05ed574e9a8c7/src/query-options-types.ts#L641)

Query options without any projection (returns full document type).

## Type Declaration

### omit?

```ts
readonly optional omit: never;
```

### projection?

```ts
readonly optional projection: never;
```

### select?

```ts
readonly optional select: never;
```

## Type Parameters

### T

`T`

The document type being queried
