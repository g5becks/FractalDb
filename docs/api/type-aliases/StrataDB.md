[stratadb](../index.md) / StrataDB

# Type Alias: StrataDB

```ts
type StrataDB = object;
```

Defined in: src/database-types.ts:128

Main StrataDB interface for database operations.

## Remarks

StrataDB provides a MongoDB-like API backed by SQLite. It manages collections,
transactions, and database lifecycle. Implements Symbol.dispose for automatic cleanup.

## Example

```typescript
// Create database
const db = new StrataDB({ database: 'myapp.db' });

// Get or create a collection
const users = db.collection('users', userSchema);

// Insert documents
await users.insertOne({ name: 'Alice', email: 'alice@example.com' });

// Query documents
const adults = await users.find({ age: { $gte: 18 } });

// Close database when done
db.close();
```

## Properties

### sqliteDb

```ts
readonly sqliteDb: SQLiteDatabase;
```

Defined in: src/database-types.ts:133

Direct access to the underlying SQLite database.
Use for advanced operations not covered by StrataDB API.

## Methods

### \[dispose\]()

```ts
dispose: void;
```

Defined in: src/database-types.ts:218

Disposes the database (closes connection).
Enables `using db = new StrataDB(...)` syntax.

#### Returns

`void`

***

### close()

```ts
close(): void;
```

Defined in: src/database-types.ts:212

Closes the database connection.

#### Returns

`void`

***

### collection()

#### Call Signature

```ts
collection<T>(name, schema): Collection<T>;
```

Defined in: src/database-types.ts:154

Gets or creates a collection with a pre-built schema.

##### Type Parameters

###### T

`T` *extends* [`Document`](Document.md)

##### Parameters

###### name

`string`

Collection name (table name in SQLite)

###### schema

[`SchemaDefinition`](SchemaDefinition.md)\<`T`\>

Schema definition for type safety and validation

##### Returns

[`Collection`](Collection.md)\<`T`\>

Collection instance for the specified type

##### Example

```typescript
const users = db.collection('users', userSchema);
```

#### Call Signature

```ts
collection<T>(name): CollectionBuilder<T>;
```

Defined in: src/database-types.ts:173

Creates a collection builder for fluent schema definition.

##### Type Parameters

###### T

`T` *extends* [`Document`](Document.md)

##### Parameters

###### name

`string`

Collection name (table name in SQLite)

##### Returns

[`CollectionBuilder`](CollectionBuilder.md)\<`T`\>

CollectionBuilder for defining schema inline

##### Example

```typescript
const users = db.collection<User>('users')
  .field('name', { type: 'TEXT', indexed: true })
  .field('email', { type: 'TEXT', indexed: true, unique: true })
  .build();
```

***

### execute()

```ts
execute<R>(fn): Promise<R>;
```

Defined in: src/database-types.ts:207

Executes a function within a transaction.

#### Type Parameters

##### R

`R`

#### Parameters

##### fn

(`tx`) => `R` \| `Promise`\<`R`\>

Function to execute within transaction

#### Returns

`Promise`\<`R`\>

Result of the function

#### Remarks

Automatically commits on success, rolls back on error.

#### Example

```typescript
await db.execute(async (tx) => {
  const users = tx.collection('users', userSchema);
  await users.insertOne({ name: 'Alice' });
  await users.insertOne({ name: 'Bob' });
});
```

***

### generateId()

```ts
generateId(): string;
```

Defined in: src/database-types.ts:140

Generates a new unique ID using the configured ID generator.

#### Returns

`string`

A new unique identifier string

***

### transaction()

```ts
transaction(): Transaction;
```

Defined in: src/database-types.ts:187

Creates a new transaction for atomic operations.

#### Returns

[`Transaction`](Transaction.md)

Transaction instance

#### Example

```typescript
using tx = db.transaction();
// operations...
tx.commit();
```
