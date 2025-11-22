[stratadb](../index.md) / Collection

# Type Alias: Collection\<T\>

```ts
type Collection<T> = object;
```

Defined in: [src/collection-types.ts:181](https://github.com/g5becks/StrataDB/blob/7791c9d2c0eca8b064c87359859d54870cd83af8/src/collection-types.ts#L181)

MongoDB-like collection interface for type-safe document operations.

## Remarks

Collection provides a complete CRUD API with full type safety. It wraps a SQLite
table and provides MongoDB-like methods for querying and manipulating documents.

**Key Features:**
- Type-safe query filters with auto-completion
- Generated columns for indexed fields (optimized queries)
- JSONB storage for flexible document structure
- Built-in validation using Standard Schema validators
- Batch operations for performance
- Transaction support through database instance

**Document Storage:**
- All documents stored in a single `data` JSONB column
- Indexed fields have generated columns with underscore prefix (e.g., `_age`)
- Auto-generated `_id` and `createdAt` fields
- Optional `updatedAt` tracking

**Comparison to MongoDB:**
- Similar API surface for easy migration
- Uses SQLite instead of MongoDB protocol
- Type-safe at compile time (MongoDB client isn't)
- Generated columns optimize common queries
- Standard Schema validation (not MongoDB JSON Schema)

## Example

```typescript
import { StrataDB, createSchema, type Document } from 'stratadb';
import { z } from 'zod';

// Define document type
type User = Document<{
  name: string;
  email: string;
  age: number;
  role: 'admin' | 'user';
}>;

// Create schema with validation
const userSchema = createSchema<User>()
  .field('name', { type: 'TEXT', indexed: true })
  .field('email', { type: 'TEXT', indexed: true, unique: true })
  .field('age', { type: 'INTEGER', indexed: true })
  .field('role', { type: 'TEXT', indexed: false })
  .validator(z.object({
    name: z.string().min(1),
    email: z.string().email(),
    age: z.number().int().min(0),
    role: z.enum(['admin', 'user'])
  }))
  .build();

// Open database and get collection
const db = new StrataDB('myapp.db');
const users = db.collection('users', userSchema);

// Insert documents
const result = await users.insertOne({
  name: 'Alice',
  email: 'alice@example.com',
  age: 30,
  role: 'admin'
});
console.log(result._id); // Auto-generated UUID

// Query documents
const admins = await users.find({ role: 'admin' });
const adults = await users.find({ age: { $gte: 18 } });
const alice = await users.findOne({ email: 'alice@example.com' });

// Update documents
await users.updateOne(
  { email: 'alice@example.com' },
  { $set: { role: 'user' } }
);

// Delete documents
await users.deleteMany({ age: { $lt: 18 } });
```

## Type Parameters

### T

`T` *extends* [`Document`](Document.md)

The document type, must extend Document

## Properties

### name

```ts
readonly name: string;
```

Defined in: [src/collection-types.ts:185](https://github.com/g5becks/StrataDB/blob/7791c9d2c0eca8b064c87359859d54870cd83af8/src/collection-types.ts#L185)

Collection name (table name in SQLite).

***

### schema

```ts
readonly schema: SchemaDefinition<T>;
```

Defined in: [src/collection-types.ts:190](https://github.com/g5becks/StrataDB/blob/7791c9d2c0eca8b064c87359859d54870cd83af8/src/collection-types.ts#L190)

Schema definition for this collection.

## Methods

### count()

```ts
count(filter): Promise<number>;
```

Defined in: [src/collection-types.ts:329](https://github.com/g5becks/StrataDB/blob/7791c9d2c0eca8b064c87359859d54870cd83af8/src/collection-types.ts#L329)

Count documents matching the query filter.

#### Parameters

##### filter

[`QueryFilter`](QueryFilter.md)\<`T`\>

Query filter to match documents

#### Returns

`Promise`\<`number`\>

Promise resolving to the count of matching documents

#### Remarks

More efficient than `find(filter).length` since it only counts without
retrieving document data.

#### Example

```typescript
// Count all users
const total = await users.count({});

// Count active admins
const adminCount = await users.count({
  role: 'admin',
  status: 'active'
});

// Count adults
const adultCount = await users.count({ age: { $gte: 18 } });
```

***

### deleteMany()

```ts
deleteMany(filter): Promise<DeleteResult>;
```

Defined in: [src/collection-types.ts:752](https://github.com/g5becks/StrataDB/blob/7791c9d2c0eca8b064c87359859d54870cd83af8/src/collection-types.ts#L752)

Delete multiple documents matching the query filter.

#### Parameters

##### filter

[`QueryFilter`](QueryFilter.md)\<`T`\>

Query filter to find documents to delete

#### Returns

`Promise`\<[`DeleteResult`](DeleteResult.md)\>

Promise resolving to delete result with statistics

#### Remarks

Deletes all documents that match the filter.
Use an empty filter `{}` to delete all documents in the collection.

**Warning:**
This operation cannot be undone. Consider backing up data before
bulk delete operations.

#### Example

```typescript
// Delete all inactive users
const result = await users.deleteMany({ status: 'inactive' });
console.log(`Deleted ${result.deletedCount} users`);

// Delete all documents (use with caution!)
await users.deleteMany({});
```

***

### deleteOne()

```ts
deleteOne(filter): Promise<boolean>;
```

Defined in: [src/collection-types.ts:501](https://github.com/g5becks/StrataDB/blob/7791c9d2c0eca8b064c87359859d54870cd83af8/src/collection-types.ts#L501)

Delete a single document by ID or filter.

#### Parameters

##### filter

Document ID (string) or query filter

`string` | [`QueryFilter`](QueryFilter.md)\<`T`\>

#### Returns

`Promise`\<`boolean`\>

Promise resolving to `true` if document was deleted, `false` if not found

#### Remarks

This method accepts either a string ID or a query filter for maximum flexibility.
When you have the document ID, pass it directly as a string for convenience.
When you need to delete by other fields, pass a filter object.

Deletes the first document that matches the filter.
If no document matches, returns `false`.

#### Example

```typescript
// Delete by ID (string)
const deleted = await users.deleteOne('user-123')
if (deleted) {
  console.log('User deleted successfully')
}

// Delete by filter
const deleted = await users.deleteOne({ email: 'inactive@example.com' })

// Delete with complex filter
const deleted = await users.deleteOne({
  status: 'inactive',
  lastLogin: { $lt: Date.now() - 90 * 24 * 60 * 60 * 1000 } // 90 days
})
```

***

### distinct()

```ts
distinct<K>(field, filter?): Promise<T[K][]>;
```

Defined in: [src/collection-types.ts:783](https://github.com/g5becks/StrataDB/blob/7791c9d2c0eca8b064c87359859d54870cd83af8/src/collection-types.ts#L783)

Find distinct values for a specified field across the collection.

#### Type Parameters

##### K

`K` *extends* `string` \| `number` \| `symbol`

#### Parameters

##### field

`K`

The field name to get distinct values for

##### filter?

[`QueryFilter`](QueryFilter.md)\<`T`\>

Optional query filter to narrow results

#### Returns

`Promise`\<`T`\[`K`\][]\>

Promise resolving to array of unique values for the field

#### Remarks

Returns an array of unique values for the specified field.
If a filter is provided, only documents matching the filter are considered.

The method uses SQLite's DISTINCT clause for efficient querying.
For indexed fields, uses the generated column; for non-indexed fields, uses JSON extraction.

#### Example

```typescript
// Get all unique ages
const ages = await users.distinct('age');
console.log(ages); // [25, 30, 35, 40]

// Get unique roles for active users
const activeRoles = await users.distinct('role', { active: true });

// Get unique tags (array field)
const tags = await users.distinct('tags');
```

***

### drop()

```ts
drop(): Promise<void>;
```

Defined in: [src/collection-types.ts:835](https://github.com/g5becks/StrataDB/blob/7791c9d2c0eca8b064c87359859d54870cd83af8/src/collection-types.ts#L835)

Drop the collection (delete the table).

#### Returns

`Promise`\<`void`\>

Promise resolving when the collection is dropped

#### Remarks

Permanently deletes the collection and all its documents.
This operation cannot be undone.

The table and all associated indexes are removed from the database.
Use with caution in production environments.

#### Example

```typescript
// Drop a temporary collection
await tempCollection.drop();

// Drop with confirmation
if (confirm('Really delete all users?')) {
  await users.drop();
}
```

***

### estimatedDocumentCount()

```ts
estimatedDocumentCount(): Promise<number>;
```

Defined in: [src/collection-types.ts:810](https://github.com/g5becks/StrataDB/blob/7791c9d2c0eca8b064c87359859d54870cd83af8/src/collection-types.ts#L810)

Get an estimated count of documents in the collection.

#### Returns

`Promise`\<`number`\>

Promise resolving to the estimated document count

#### Remarks

Returns a fast estimate of the document count using SQLite table statistics.
This is much faster than count() for large collections but may not be exact.

Uses SQLite's internal statistics which are updated periodically.
For exact counts, use the count() method instead.

#### Example

```typescript
const estimate = await users.estimatedDocumentCount();
console.log(`Approximately ${estimate} users`);

// Compare with exact count
const exact = await users.count({});
console.log(`Exact: ${exact}, Estimated: ${estimate}`);
```

***

### find()

```ts
find(filter, options?): Promise<readonly T[]>;
```

Defined in: [src/collection-types.ts:258](https://github.com/g5becks/StrataDB/blob/7791c9d2c0eca8b064c87359859d54870cd83af8/src/collection-types.ts#L258)

Find all documents matching the query filter.

#### Parameters

##### filter

[`QueryFilter`](QueryFilter.md)\<`T`\>

Query filter to match documents

##### options?

[`QueryOptions`](QueryOptions.md)\<`T`\>

Query options for sorting, pagination, and projection

#### Returns

`Promise`\<readonly `T`[]\>

Promise resolving to array of matching documents

#### Remarks

Returns all documents that match the filter. For large result sets,
use `limit` and `skip` options for pagination.

**Performance:**
- Indexed fields use generated columns (fast)
- Non-indexed fields use jsonb_extract (slower)
- Consider adding indexes for frequently queried fields

#### Example

```typescript
// Find all active admins
const admins = await users.find({
  role: 'admin',
  status: 'active'
});

// Find with sorting and pagination
const page = await users.find(
  { age: { $gte: 18 } },
  { sort: { createdAt: -1 }, limit: 20, skip: 40 }
);

// Find with complex query
const results = await users.find({
  $and: [
    { age: { $gte: 18, $lt: 65 } },
    {
      $or: [
        { role: 'admin' },
        { verified: true }
      ]
    }
  ]
});
```

***

### findById()

```ts
findById(id): Promise<T | null>;
```

Defined in: [src/collection-types.ts:212](https://github.com/g5becks/StrataDB/blob/7791c9d2c0eca8b064c87359859d54870cd83af8/src/collection-types.ts#L212)

Find a single document by its ID.

#### Parameters

##### id

`string`

The document ID to search for

#### Returns

`Promise`\<`T` \| `null`\>

Promise resolving to the document, or null if not found

#### Remarks

This is the fastest way to retrieve a specific document since it uses
the primary key index.

#### Example

```typescript
const user = await users.findById('123e4567-e89b-12d3-a456-426614174000');
if (user) {
  console.log(user.name);
}
```

***

### findOne()

```ts
findOne(filter, options?): Promise<T | null>;
```

Defined in: [src/collection-types.ts:299](https://github.com/g5becks/StrataDB/blob/7791c9d2c0eca8b064c87359859d54870cd83af8/src/collection-types.ts#L299)

Find the first document matching the query filter or ID.

#### Parameters

##### filter

Document ID or query filter to match documents

`string` | [`QueryFilter`](QueryFilter.md)\<`T`\>

##### options?

`Omit`\<[`QueryOptions`](QueryOptions.md)\<`T`\>, `"limit"` \| `"skip"`\>

Query options for sorting and projection

#### Returns

`Promise`\<`T` \| `null`\>

Promise resolving to the first matching document, or null if none found

#### Remarks

Equivalent to `find(filter, { ...options, limit: 1 })[0]` but more efficient
since it stops after finding the first match.

Accepts either a string ID or a full QueryFilter object for flexible querying:
- String ID: `{ _id: string }` filter is applied automatically
- QueryFilter: Full MongoDB-style query filtering

#### Example

```typescript
// Find by document ID
const user = await users.findOne('123e4567-e89b-12d3-a456-426614174000');
if (user) {
  console.log(user.name);
}

// Find by query filter
const admin = await users.findOne({ role: 'admin' });

// Find with options
const latest = await users.findOne(
  {},
  { sort: { createdAt: -1 } }
);

// Check if email exists
const exists = await users.findOne({ email: 'alice@example.com' });
if (exists) {
  console.log('Email already registered');
}
```

***

### findOneAndDelete()

```ts
findOneAndDelete(filter, options?): Promise<T | null>;
```

Defined in: [src/collection-types.ts:541](https://github.com/g5becks/StrataDB/blob/7791c9d2c0eca8b064c87359859d54870cd83af8/src/collection-types.ts#L541)

Find and delete a single document atomically.

#### Parameters

##### filter

Document ID (string) or query filter

`string` | [`QueryFilter`](QueryFilter.md)\<`T`\>

##### options?

Query options (sort)

###### sort?

[`SortSpec`](SortSpec.md)\<`T`\>

#### Returns

`Promise`\<`T` \| `null`\>

Promise resolving to the deleted document, or null if not found

#### Remarks

This method accepts either a string ID or a query filter for maximum flexibility.
When you have the document ID, pass it directly as a string for convenience.
When you need to delete by other fields, pass a filter object.

This operation is atomic - it finds and deletes in a single operation.
Useful when you need the deleted document's data (e.g., for logging, undo operations).

When multiple documents match the filter, the sort option determines which one is deleted.

#### Example

```typescript
// Delete by ID
const deleted = await users.findOneAndDelete('user-123');
if (deleted) {
  console.log(`Deleted user: ${deleted.name}`);
  await logDeletion(deleted);
}

// Delete by filter
const deleted = await users.findOneAndDelete({ email: 'old@example.com' });

// Delete with sort (delete oldest inactive user)
const deleted = await users.findOneAndDelete(
  { status: 'inactive' },
  { sort: { createdAt: 1 } }
);
```

***

### findOneAndReplace()

```ts
findOneAndReplace(
   filter, 
   replacement, 
options?): Promise<T | null>;
```

Defined in: [src/collection-types.ts:652](https://github.com/g5becks/StrataDB/blob/7791c9d2c0eca8b064c87359859d54870cd83af8/src/collection-types.ts#L652)

Find and replace a single document atomically.

#### Parameters

##### filter

Document ID (string) or query filter

`string` | [`QueryFilter`](QueryFilter.md)\<`T`\>

##### replacement

`Omit`\<`T`, `"_id"` \| `"createdAt"` \| `"updatedAt"`\>

Complete replacement document (without _id, createdAt, updatedAt)

##### options?

Replace options (sort, returnDocument, upsert)

###### returnDocument?

`"before"` \| `"after"`

###### sort?

[`SortSpec`](SortSpec.md)\<`T`\>

###### upsert?

`boolean`

#### Returns

`Promise`\<`T` \| `null`\>

Promise resolving to the document before or after replacement, or null if not found

#### Remarks

This method accepts either a string ID or a query filter for maximum flexibility.
When you have the document ID, pass it directly as a string for convenience.
When you need to replace by other fields, pass a filter object.

This operation replaces the ENTIRE document (except _id, createdAt).
Unlike `findOneAndUpdate` which merges fields, this replaces everything.
The replacement document is validated against the schema.
Default returnDocument is 'after'.

#### Example

```typescript
// Replace by ID
const replaced = await users.findOneAndReplace(
  'user-123',
  {
    name: 'New Name',
    email: 'new@example.com',
    age: 30,
    active: true,
    tags: []
  },
  { returnDocument: 'after' }
);

// Replace by filter
const replaced = await users.findOneAndReplace(
  { email: 'old@example.com' },
  {
    name: 'New Name',
    email: 'new@example.com',
    age: 30,
    active: true,
    tags: []
  }
);
```

***

### findOneAndUpdate()

```ts
findOneAndUpdate(
   filter, 
   update, 
options?): Promise<T | null>;
```

Defined in: [src/collection-types.ts:596](https://github.com/g5becks/StrataDB/blob/7791c9d2c0eca8b064c87359859d54870cd83af8/src/collection-types.ts#L596)

Find and update a single document atomically.

#### Parameters

##### filter

Document ID (string) or query filter

`string` | [`QueryFilter`](QueryFilter.md)\<`T`\>

##### update

`Omit`\<`Partial`\<`T`\>, `"_id"` \| `"createdAt"` \| `"updatedAt"`\>

Partial document with fields to update

##### options?

Update options (sort, returnDocument, upsert)

###### returnDocument?

`"before"` \| `"after"`

###### sort?

[`SortSpec`](SortSpec.md)\<`T`\>

###### upsert?

`boolean`

#### Returns

`Promise`\<`T` \| `null`\>

Promise resolving to the document before or after update, or null if not found

#### Remarks

This method accepts either a string ID or a query filter for maximum flexibility.
When you have the document ID, pass it directly as a string for convenience.
When you need to update by other fields, pass a filter object.

This operation is atomic - it finds and updates in a single logical operation.
Use `returnDocument` to control whether you get the document state before or after the update.
Default is 'after'.

#### Example

```typescript
// Update by ID
const updated = await users.findOneAndUpdate(
  'user-123',
  { loginCount: 5 },
  { returnDocument: 'after' }
);

// Update by filter
const updated = await users.findOneAndUpdate(
  { email: 'alice@example.com' },
  { loginCount: 5 },
  { returnDocument: 'after' }
);
console.log(`New login count: ${updated?.loginCount}`);

// Get previous state before update
const before = await users.findOneAndUpdate(
  { _id: userId },
  { status: 'archived' },
  { returnDocument: 'before' }
);
await logStatusChange(before, 'archived');

// Upsert with returnDocument
const result = await users.findOneAndUpdate(
  { email: 'new@example.com' },
  { name: 'New User', age: 25 },
  { upsert: true, returnDocument: 'after' }
);
```

***

### insertMany()

```ts
insertMany(docs): Promise<InsertManyResult<T>>;
```

Defined in: [src/collection-types.ts:691](https://github.com/g5becks/StrataDB/blob/7791c9d2c0eca8b064c87359859d54870cd83af8/src/collection-types.ts#L691)

Insert multiple documents into the collection.

#### Parameters

##### docs

readonly `Omit`\<`T`, `"_id"` \| `"createdAt"` \| `"updatedAt"`\>[]

Array of documents to insert

#### Returns

`Promise`\<[`InsertManyResult`](InsertManyResult.md)\<`T`\>\>

Promise resolving to insert result with all new documents

#### Remarks

Inserts all documents in a single transaction for performance.
If any document fails validation, the entire operation rolls back.
Automatically generates `id`, `createdAt`, and `updatedAt` for each document.

**Performance:**
Much faster than multiple `insertOne` calls since it uses a single transaction.

#### Throws

If any document fails schema validation

#### Throws

If any unique constraint is violated

#### Example

```typescript
const result = await users.insertMany([
  { name: 'Alice', email: 'alice@example.com', age: 30, role: 'admin' },
  { name: 'Bob', email: 'bob@example.com', age: 25, role: 'user' },
  { name: 'Charlie', email: 'charlie@example.com', age: 35, role: 'user' }
]);
console.log(`Inserted ${result.insertedCount} users`);
```

***

### insertOne()

```ts
insertOne(doc): Promise<T>;
```

Defined in: [src/collection-types.ts:370](https://github.com/g5becks/StrataDB/blob/7791c9d2c0eca8b064c87359859d54870cd83af8/src/collection-types.ts#L370)

Insert a single document into the collection.

#### Parameters

##### doc

`Omit`\<`T`, `"_id"` \| `"createdAt"` \| `"updatedAt"`\>

Document to insert (without _id, createdAt, updatedAt)

#### Returns

`Promise`\<`T`\>

Promise resolving to the inserted document with generated _id

#### Remarks

Validates the document against the schema before inserting.
Automatically generates:
- `_id`: UUID v4
- `createdAt`: Current timestamp
- `updatedAt`: Current timestamp (if schema has this field)

**Validation:**
If validation fails, throws `ValidationError` with details.

#### Throws

If document fails schema validation

#### Throws

If unique constraint is violated

#### Example

```typescript
try {
  const user = await users.insertOne({
    name: 'Bob',
    email: 'bob@example.com',
    age: 25,
    role: 'user'
  });
  console.log(`Inserted user with ID: ${user._id}`);
  console.log(`User name: ${user.name}`);
} catch (err) {
  if (err instanceof ValidationError) {
    console.error('Invalid user:', err.message);
  }
}
```

***

### replaceOne()

```ts
replaceOne(filter, doc): Promise<T | null>;
```

Defined in: [src/collection-types.ts:464](https://github.com/g5becks/StrataDB/blob/7791c9d2c0eca8b064c87359859d54870cd83af8/src/collection-types.ts#L464)

Replace a single document by ID or filter.

#### Parameters

##### filter

Document ID (string) or query filter

`string` | [`QueryFilter`](QueryFilter.md)\<`T`\>

##### doc

`Omit`\<`T`, `"_id"` \| `"createdAt"` \| `"updatedAt"`\>

New document to replace with (without _id, createdAt, updatedAt)

#### Returns

`Promise`\<`T` \| `null`\>

Promise resolving to the replaced document, or null if not found

#### Remarks

This method accepts either a string ID or a query filter for maximum flexibility.
When you have the document ID, pass it directly as a string for convenience.
When you need to replace by other fields, pass a filter object.

Completely replaces the matched document with the new document.
Preserves `_id` and `createdAt`, updates `updatedAt`.
Validates the new document against the schema.

**Difference from updateOne:**
- `updateOne`: Merges specific fields into existing document
- `replaceOne`: Replaces entire document (except _id, createdAt)

#### Throws

If new document fails schema validation

#### Example

```typescript
// Replace by ID (string)
const replaced = await users.replaceOne(
  'user-123',
  {
    name: 'Alice Smith',
    email: 'alice.smith@example.com',
    age: 31,
    role: 'admin'
  }
);

// Replace by filter
const replaced = await users.replaceOne(
  { email: 'old@example.com' },
  {
    name: 'Alice Smith',
    email: 'new@example.com',
    age: 31,
    role: 'admin'
  }
);
```

***

### updateMany()

```ts
updateMany(filter, update): Promise<UpdateResult>;
```

Defined in: [src/collection-types.ts:723](https://github.com/g5becks/StrataDB/blob/7791c9d2c0eca8b064c87359859d54870cd83af8/src/collection-types.ts#L723)

Update multiple documents matching the query filter.

#### Parameters

##### filter

[`QueryFilter`](QueryFilter.md)\<`T`\>

Query filter to find documents to update

##### update

`Omit`\<`Partial`\<`T`\>, `"_id"` \| `"createdAt"` \| `"updatedAt"`\>

Update operations to apply

#### Returns

`Promise`\<[`UpdateResult`](UpdateResult.md)\>

Promise resolving to update result with statistics

#### Remarks

Updates all documents that match the filter.
Uses MongoDB-style update operators.
Automatically updates `updatedAt` for each modified document.

#### Example

```typescript
// Set all inactive users to deleted status
const result = await users.updateMany(
  { status: 'inactive' },
  { $set: { status: 'deleted' } }
);
console.log(`Updated ${result.modifiedCount} users`);

// Give all admins a badge
await users.updateMany(
  { role: 'admin' },
  { $set: { badge: 'admin' } }
);
```

***

### updateOne()

```ts
updateOne(
   filter, 
   update, 
options?): Promise<T | null>;
```

Defined in: [src/collection-types.ts:411](https://github.com/g5becks/StrataDB/blob/7791c9d2c0eca8b064c87359859d54870cd83af8/src/collection-types.ts#L411)

Update a single document matching the query filter or ID.

#### Parameters

##### filter

Document ID or query filter to find the document to update

`string` | [`QueryFilter`](QueryFilter.md)\<`T`\>

##### update

`Omit`\<`Partial`\<`T`\>, `"_id"` \| `"createdAt"` \| `"updatedAt"`\>

Partial document with fields to update

##### options?

Optional upsert configuration

###### upsert?

`boolean`

#### Returns

`Promise`\<`T` \| `null`\>

Promise resolving to the updated document, or null if not found

#### Remarks

Updates the first document that matches the filter.
Merges the provided partial document with the existing document.
Automatically updates `updatedAt` if present in schema.

Accepts either a string ID or a full QueryFilter object for flexible targeting:
- String ID: `{ _id: string }` filter is applied automatically
- QueryFilter: Full MongoDB-style query filtering

#### Example

```typescript
// Update by document ID
const updated = await users.updateOne(
  '123e4567-e89b-12d3-a456-426614174000',
  { role: 'admin', verified: true }
);

// Update by query filter
await users.updateOne(
  { email: 'alice@example.com' },
  { role: 'admin', verified: true }
);

// Update with upsert
await users.updateOne(
  { email: 'new@example.com' },
  { role: 'user' },
  { upsert: true }
);
```

***

### validate()

```ts
validate(doc): Promise<T>;
```

Defined in: [src/collection-types.ts:869](https://github.com/g5becks/StrataDB/blob/7791c9d2c0eca8b064c87359859d54870cd83af8/src/collection-types.ts#L869)

Validate a document against the schema (async).

#### Parameters

##### doc

`unknown`

Document to validate

#### Returns

`Promise`\<`T`\>

Promise resolving to true if valid

#### Remarks

Validates the document against the schema validator.
Supports both synchronous and asynchronous validators.
Throws `ValidationError` if validation fails.

#### Throws

If document fails validation

#### Example

```typescript
try {
  await users.validate({
    name: 'Alice',
    email: 'invalid-email',
    age: -5,
    role: 'admin'
  });
} catch (err) {
  if (err instanceof ValidationError) {
    console.error(`Invalid field: ${err.field}`);
    console.error(`Error: ${err.message}`);
  }
}
```

***

### validateSync()

```ts
validateSync(doc): T;
```

Defined in: [src/collection-types.ts:894](https://github.com/g5becks/StrataDB/blob/7791c9d2c0eca8b064c87359859d54870cd83af8/src/collection-types.ts#L894)

Validate a document against the schema (sync).

#### Parameters

##### doc

`unknown`

Document to validate

#### Returns

`T`

True if valid

#### Remarks

Synchronous version of `validate()`. Only works with synchronous validators.
Throws error if the schema uses async validators.

#### Throws

If document fails validation

#### Throws

If schema uses async validators

#### Example

```typescript
try {
  users.validateSync({ name: 'Alice', email: 'alice@example.com', age: 30, role: 'admin' });
  console.log('Document is valid');
} catch (err) {
  console.error('Validation failed:', err.message);
}
```
