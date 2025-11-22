[stratadb](../index.md) / StrataDBClass

# Class: StrataDBClass

Defined in: src/stratadb.ts:51

Main StrataDB database class.

## Remarks

StrataDB provides a MongoDB-like document database API backed by SQLite.
It manages collections, transactions, and database lifecycle.

**Features:**
- Type-safe document operations
- JSONB storage with indexed generated columns
- Transaction support with automatic rollback
- Symbol.dispose for automatic cleanup

## Example

```typescript
// Create database with path
const db = new StrataDBClass({ database: 'myapp.db' });

// Create in-memory database
const memDb = new StrataDBClass({ database: ':memory:' });

// With custom options
const customDb = new StrataDBClass({
  database: 'app.db',
  idGenerator: () => `custom-${Date.now()}`,
  onClose: () => console.log('Database closed'),
  debug: true
});

// Using with automatic cleanup
using db = new StrataDBClass({ database: ':memory:' });
const users = db.collection('users', userSchema);
// Database automatically closes when scope exits
```

## Implements

- [`StrataDB`](../type-aliases/StrataDB.md)

## Constructors

### Constructor

```ts
new StrataDBClass(options): StrataDBClass;
```

Defined in: src/stratadb.ts:64

Creates a new StrataDB instance.

#### Parameters

##### options

[`DatabaseOptions`](../type-aliases/DatabaseOptions.md)

Database configuration options

#### Returns

`StrataDBClass`

## Properties

### sqliteDb

```ts
readonly sqliteDb: Database;
```

Defined in: src/stratadb.ts:52

Direct access to the underlying SQLite database.
Use for advanced operations not covered by StrataDB API.

#### Implementation of

```ts
StrataDBInterface.sqliteDb
```

## Methods

### \[dispose\]()

```ts
dispose: void;
```

Defined in: src/stratadb.ts:213

Disposes the database (closes connection).

#### Returns

`void`

#### Implementation of

```ts
StrataDBInterface.[dispose]
```

***

### close()

```ts
close(): void;
```

Defined in: src/stratadb.ts:201

Closes the database connection.

#### Returns

`void`

#### Implementation of

```ts
StrataDBInterface.close
```

***

### collection()

Implementation of overloaded collection method.

#### Call Signature

```ts
collection<T>(name, schema): Collection<T>;
```

Defined in: src/stratadb.ts:93

Gets or creates a collection with a pre-built schema.

##### Type Parameters

###### T

`T` *extends* [`Document`](../type-aliases/Document.md)

##### Parameters

###### name

`string`

###### schema

[`SchemaDefinition`](../type-aliases/SchemaDefinition.md)\<`T`\>

##### Returns

[`Collection`](../type-aliases/Collection.md)\<`T`\>

##### Implementation of

```ts
StrataDBInterface.collection
```

#### Call Signature

```ts
collection<T>(name): CollectionBuilder<T>;
```

Defined in: src/stratadb.ts:101

Creates a collection builder for fluent schema definition.

##### Type Parameters

###### T

`T` *extends* [`Document`](../type-aliases/Document.md)

##### Parameters

###### name

`string`

##### Returns

[`CollectionBuilder`](../type-aliases/CollectionBuilder.md)\<`T`\>

##### Implementation of

```ts
StrataDBInterface.collection
```

***

### execute()

```ts
execute<R>(fn): Promise<R>;
```

Defined in: src/stratadb.ts:186

Executes a function within a transaction.

#### Type Parameters

##### R

`R`

#### Parameters

##### fn

(`tx`) => `R` \| `Promise`\<`R`\>

Function to execute

#### Returns

`Promise`\<`R`\>

Result of the function

#### Implementation of

```ts
StrataDBInterface.execute
```

***

### generateId()

```ts
generateId(): string;
```

Defined in: src/stratadb.ts:86

Generates a new unique ID using the configured ID generator.

#### Returns

`string`

A new unique identifier string

#### Implementation of

```ts
StrataDBInterface.generateId
```

***

### transaction()

```ts
transaction(): Transaction;
```

Defined in: src/stratadb.ts:138

Creates a new transaction.

#### Returns

[`Transaction`](../type-aliases/Transaction.md)

Transaction instance

#### Implementation of

```ts
StrataDBInterface.transaction
```
