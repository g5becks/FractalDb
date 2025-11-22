[stratadb](../index.md) / QueryOptions

# Type Alias: QueryOptions\<T\>

```ts
type QueryOptions<T> = object;
```

Defined in: [src/query-options-types.ts:214](https://github.com/g5becks/StrataDB/blob/7791c9d2c0eca8b064c87359859d54870cd83af8/src/query-options-types.ts#L214)

Complete query options for controlling result set behavior.

## Remarks

Combines all query result control options:
- `sort`: Order results by one or more fields
- `limit`: Maximum number of results to return
- `skip`: Number of results to skip (for pagination)
- `projection`: Which fields to include/exclude

These options work together to enable:
- Pagination (limit + skip)
- Sorting (sort)
- Field selection (projection)
- Performance optimization (limit + projection)

## Example

```typescript
type User = Document<{
  name: string;
  email: string;
  age: number;
  status: 'active' | 'inactive';
  createdAt: Date;
}>;

// ✅ Pagination with sorting
const page2: QueryOptions<User> = {
  sort: { createdAt: -1 },  // Newest first
  limit: 20,                // 20 per page
  skip: 20                  // Skip first page
};

// ✅ Sorted results with field selection
const publicUsers: QueryOptions<User> = {
  sort: { name: 1 },
  projection: {
    name: 1,
    email: 1
    // Excludes age, status, createdAt
  }
};

// ✅ Top 10 most recent
const recent: QueryOptions<User> = {
  sort: { createdAt: -1 },
  limit: 10
};

// ✅ Complex pagination pattern
function getPage(pageNum: number, pageSize: number): QueryOptions<User> {
  return {
    sort: { createdAt: -1, name: 1 },
    limit: pageSize,
    skip: (pageNum - 1) * pageSize,
    projection: { name: 1, email: 1, status: 1 }
  };
}

// ✅ Performance optimization
const optimized: QueryOptions<User> = {
  limit: 100,              // Limit results
  projection: {            // Only needed fields
    name: 1,
    status: 1
  }
};

// Usage with collection
const results = await users.find(
  { status: 'active' },
  {
    sort: { createdAt: -1 },
    limit: 20,
    skip: 0,
    projection: { name: 1, email: 1 }
  }
);

// Pagination helper
async function fetchPage(page: number, perPage: number = 20) {
  return await users.find(
    {},
    {
      sort: { createdAt: -1 },
      limit: perPage,
      skip: (page - 1) * perPage
    }
  );
}
```

## Type Parameters

### T

`T`

The document type being queried

## Properties

### limit?

```ts
readonly optional limit: number;
```

Defined in: [src/query-options-types.ts:219](https://github.com/g5becks/StrataDB/blob/7791c9d2c0eca8b064c87359859d54870cd83af8/src/query-options-types.ts#L219)

Maximum number of documents to return

***

### projection?

```ts
readonly optional projection: ProjectionSpec<T>;
```

Defined in: [src/query-options-types.ts:225](https://github.com/g5becks/StrataDB/blob/7791c9d2c0eca8b064c87359859d54870cd83af8/src/query-options-types.ts#L225)

Which fields to include (1) or exclude (0) from results

***

### skip?

```ts
readonly optional skip: number;
```

Defined in: [src/query-options-types.ts:222](https://github.com/g5becks/StrataDB/blob/7791c9d2c0eca8b064c87359859d54870cd83af8/src/query-options-types.ts#L222)

Number of documents to skip (for pagination)

***

### sort?

```ts
readonly optional sort: SortSpec<T>;
```

Defined in: [src/query-options-types.ts:216](https://github.com/g5becks/StrataDB/blob/7791c9d2c0eca8b064c87359859d54870cd83af8/src/query-options-types.ts#L216)

Sort order for results (MongoDB-style: 1 = asc, -1 = desc)
