[stratadb](../index.md) / DatabaseOptions

# Type Alias: DatabaseOptions

```ts
type DatabaseOptions = object;
```

Defined in: [src/database-types.ts:20](https://github.com/g5becks/StrataDB/blob/7791c9d2c0eca8b064c87359859d54870cd83af8/src/database-types.ts#L20)

Options for creating a StrataDB instance.

## Example

```typescript
const options: DatabaseOptions = {
  database: ':memory:',
  idGenerator: () => crypto.randomUUID(),
  onClose: () => console.log('Database closed')
};
const db = new StrataDB(options);
```

## Properties

### database

```ts
readonly database: string | SQLiteDatabase;
```

Defined in: [src/database-types.ts:25](https://github.com/g5becks/StrataDB/blob/7791c9d2c0eca8b064c87359859d54870cd83af8/src/database-types.ts#L25)

SQLite database path or ':memory:' for in-memory database.
Can also be an existing bun:sqlite Database instance.

***

### enableCache?

```ts
readonly optional enableCache: boolean;
```

Defined in: [src/database-types.ts:75](https://github.com/g5becks/StrataDB/blob/7791c9d2c0eca8b064c87359859d54870cd83af8/src/database-types.ts#L75)

Enable query caching for all collections.

#### Remarks

When enabled, the query translator caches SQL templates for queries with the
same structure, improving performance for repeated queries at the cost of
memory usage (up to 500 cached query templates per collection).

**Performance improvements with cache enabled:**
- Simple queries: ~23% faster
- Range queries: ~70% faster
- Complex queries: ~55% faster

**Memory considerations:**
- Each collection maintains its own cache (up to 500 entries)
- Cache stores SQL strings and value extraction paths
- Use FIFO eviction when cache is full

**When to enable:**
- Applications with repeated query patterns
- High-throughput read operations
- When performance is more critical than memory usage

Individual collections can override this setting.

#### Default Value

```ts
false
```

#### See

CollectionOptions.enableCache for per-collection override

#### Example

```typescript
// Enable caching for all collections (opt-in for performance)
const db = new StrataDB({ database: ':memory:', enableCache: true });

// Disabled by default
const db = new StrataDB({ database: ':memory:' });
```

***

### idGenerator()?

```ts
readonly optional idGenerator: () => string;
```

Defined in: [src/database-types.ts:30](https://github.com/g5becks/StrataDB/blob/7791c9d2c0eca8b064c87359859d54870cd83af8/src/database-types.ts#L30)

Custom ID generator function. Defaults to crypto.randomUUID().

#### Returns

`string`

***

### onClose()?

```ts
readonly optional onClose: () => void;
```

Defined in: [src/database-types.ts:35](https://github.com/g5becks/StrataDB/blob/7791c9d2c0eca8b064c87359859d54870cd83af8/src/database-types.ts#L35)

Callback invoked when database is closed.

#### Returns

`void`
