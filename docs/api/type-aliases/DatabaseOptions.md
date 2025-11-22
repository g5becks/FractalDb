[stratadb](../index.md) / DatabaseOptions

# Type Alias: DatabaseOptions

```ts
type DatabaseOptions = object;
```

Defined in: src/database-types.ts:21

Options for creating a StrataDB instance.

## Example

```typescript
const options: DatabaseOptions = {
  database: ':memory:',
  idGenerator: () => crypto.randomUUID(),
  onClose: () => console.log('Database closed'),
  debug: true
};
const db = new StrataDB(options);
```

## Properties

### database

```ts
readonly database: string | SQLiteDatabase;
```

Defined in: src/database-types.ts:26

SQLite database path or ':memory:' for in-memory database.
Can also be an existing bun:sqlite Database instance.

***

### debug?

```ts
readonly optional debug: boolean;
```

Defined in: src/database-types.ts:41

Enable debug logging.

***

### idGenerator()?

```ts
readonly optional idGenerator: () => string;
```

Defined in: src/database-types.ts:31

Custom ID generator function. Defaults to crypto.randomUUID().

#### Returns

`string`

***

### onClose()?

```ts
readonly optional onClose: () => void;
```

Defined in: src/database-types.ts:36

Callback invoked when database is closed.

#### Returns

`void`
