[stratadb](../index.md) / QueryOptionsWithSelect

# Type Alias: QueryOptionsWithSelect&lt;T, K&gt;

```ts
type QueryOptionsWithSelect<T, K> = QueryOptionsBase<T> & object;
```

Defined in: [src/query-options-types.ts:598](https://github.com/g5becks/StrataDb/blob/56b93c15dc2c602cd539356668e05ed574e9a8c7/src/query-options-types.ts#L598)

Query options with `select` for type-safe field inclusion.

## Type Declaration

### omit?

```ts
readonly optional omit: never;
```

### projection?

```ts
readonly optional projection: never;
```

### select

```ts
readonly select: readonly K[];
```

Array of fields to include in results (plus _id).

## Type Parameters

### T

`T`

The document type being queried

### K

`K` *extends* keyof `T`

The keys to select (inferred from the select array)

## Remarks

When using `select`, the return type is narrowed to only include the
selected fields plus `_id`. This provides compile-time type safety.

## Example

```typescript
// Returns Pick<User, '_id' | 'name' | 'email'>[]
const users = await collection.find(
  { status: 'active' },
  { select: ['name', 'email'] as const }
);
users[0].name   // ✅ TypeScript knows this exists
users[0].password // ❌ TypeScript error: property doesn't exist
```
