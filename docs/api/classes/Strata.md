[stratadb](../index.md) / Strata

# Class: Strata

Defined in: [src/stratadb.ts:53](https://github.com/g5becks/StrataDB/blob/7791c9d2c0eca8b064c87359859d54870cd83af8/src/stratadb.ts#L53)

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
const db = new Strata({ database: 'myapp.db' });

// Create in-memory database
const memDb = new Strata({ database: ':memory:' });

// With custom options
const customDb = new Strata({
  database: 'app.db',
  idGenerator: () => `custom-${Date.now()}`,
  onClose: () => console.log('Database closed'),
  debug: true
});

// Using with automatic cleanup
using db = new Strata({ database: ':memory:' });
const users = db.collection('users', userSchema);
// Database automatically closes when scope exits
```

## Implements

- [`StrataDB`](../type-aliases/StrataDB.md)

## Constructors

### Constructor

```ts
new Strata(options): Strata;
```

Defined in: [src/stratadb.ts:66](https://github.com/g5becks/StrataDB/blob/7791c9d2c0eca8b064c87359859d54870cd83af8/src/stratadb.ts#L66)

Creates a new StrataDB instance.

#### Parameters

##### options

[`DatabaseOptions`](../type-aliases/DatabaseOptions.md)

Database configuration options

#### Returns

`Strata`

## Properties

### sqliteDb

```ts
readonly sqliteDb: Database;
```

Defined in: [src/stratadb.ts:54](https://github.com/g5becks/StrataDB/blob/7791c9d2c0eca8b064c87359859d54870cd83af8/src/stratadb.ts#L54)

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

Defined in: [src/stratadb.ts:206](https://github.com/g5becks/StrataDB/blob/7791c9d2c0eca8b064c87359859d54870cd83af8/src/stratadb.ts#L206)

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

Defined in: [src/stratadb.ts:198](https://github.com/g5becks/StrataDB/blob/7791c9d2c0eca8b064c87359859d54870cd83af8/src/stratadb.ts#L198)

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
collection<T>(
   name, 
   schema, 
options?): Collection<T>;
```

Defined in: [src/stratadb.ts:91](https://github.com/g5becks/StrataDB/blob/7791c9d2c0eca8b064c87359859d54870cd83af8/src/stratadb.ts#L91)

Gets or creates a collection with a pre-built schema.

##### Type Parameters

###### T

`T` *extends* [`Document`](../type-aliases/Document.md)

##### Parameters

###### name

`string`

###### schema

[`SchemaDefinition`](../type-aliases/SchemaDefinition.md)\<`T`\>

###### options?

`CollectionOptions`

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

Defined in: [src/stratadb.ts:100](https://github.com/g5becks/StrataDB/blob/7791c9d2c0eca8b064c87359859d54870cd83af8/src/stratadb.ts#L100)

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

Defined in: [src/stratadb.ts:183](https://github.com/g5becks/StrataDB/blob/7791c9d2c0eca8b064c87359859d54870cd83af8/src/stratadb.ts#L183)

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

Defined in: [src/stratadb.ts:84](https://github.com/g5becks/StrataDB/blob/7791c9d2c0eca8b064c87359859d54870cd83af8/src/stratadb.ts#L84)

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

Defined in: [src/stratadb.ts:145](https://github.com/g5becks/StrataDB/blob/7791c9d2c0eca8b064c87359859d54870cd83af8/src/stratadb.ts#L145)

Creates a new transaction.

#### Returns

[`Transaction`](../type-aliases/Transaction.md)

Transaction instance

#### Implementation of

```ts
StrataDBInterface.transaction
```
