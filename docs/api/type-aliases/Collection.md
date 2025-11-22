[stratadb](../index.md) / Collection

# Type Alias: Collection\<T\>

```ts
type Collection<T> = object;
```

Defined in: [src/collection-types.ts:157](https://github.com/g5becks/StrataDB/blob/89bee4bbe54bb52f1f1308d5950da4d385abbe16/src/collection-types.ts#L157)

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
- Auto-generated `id` and `createdAt` fields
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
console.log(result.document.id); // Auto-generated UUID

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

Defined in: [src/collection-types.ts:161](https://github.com/g5becks/StrataDB/blob/89bee4bbe54bb52f1f1308d5950da4d385abbe16/src/collection-types.ts#L161)

Collection name (table name in SQLite).

***

### schema

```ts
readonly schema: SchemaDefinition<T>;
```

Defined in: [src/collection-types.ts:166](https://github.com/g5becks/StrataDB/blob/89bee4bbe54bb52f1f1308d5950da4d385abbe16/src/collection-types.ts#L166)

Schema definition for this collection.

## Methods

### count()

```ts
count(filter): Promise<number>;
```

Defined in: [src/collection-types.ts:295](https://github.com/g5becks/StrataDB/blob/89bee4bbe54bb52f1f1308d5950da4d385abbe16/src/collection-types.ts#L295)

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

Defined in: [src/collection-types.ts:527](https://github.com/g5becks/StrataDB/blob/89bee4bbe54bb52f1f1308d5950da4d385abbe16/src/collection-types.ts#L527)

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
deleteOne(id): Promise<boolean>;
```

Defined in: [src/collection-types.ts:435](https://github.com/g5becks/StrataDB/blob/89bee4bbe54bb52f1f1308d5950da4d385abbe16/src/collection-types.ts#L435)

Delete a single document matching the query filter.

#### Parameters

##### id

`string`

The document ID to delete

#### Returns

`Promise`\<`boolean`\>

Promise resolving to delete result with statistics

#### Remarks

Deletes the first document that matches the filter.
If no document matches, returns `{ deletedCount: 0 }`.

#### Example

```typescript
const result = await users.deleteOne({ id: userId });
if (result.deletedCount > 0) {
  console.log('User deleted successfully');
} else {
  console.log('User not found');
}
```

***

### find()

```ts
find(filter, options?): Promise<readonly T[]>;
```

Defined in: [src/collection-types.ts:234](https://github.com/g5becks/StrataDB/blob/89bee4bbe54bb52f1f1308d5950da4d385abbe16/src/collection-types.ts#L234)

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

Defined in: [src/collection-types.ts:188](https://github.com/g5becks/StrataDB/blob/89bee4bbe54bb52f1f1308d5950da4d385abbe16/src/collection-types.ts#L188)

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

Defined in: [src/collection-types.ts:265](https://github.com/g5becks/StrataDB/blob/89bee4bbe54bb52f1f1308d5950da4d385abbe16/src/collection-types.ts#L265)

Find the first document matching the query filter.

#### Parameters

##### filter

[`QueryFilter`](QueryFilter.md)\<`T`\>

Query filter to match documents

##### options?

`Omit`\<[`QueryOptions`](QueryOptions.md)\<`T`\>, `"limit"` \| `"skip"`\>

Query options for sorting and projection

#### Returns

`Promise`\<`T` \| `null`\>

Promise resolving to the first matching document, or null if none found

#### Remarks

Equivalent to `find(filter, { ...options, limit: 1 })[0]` but more efficient
since it stops after finding the first match.

#### Example

```typescript
// Find any admin user
const admin = await users.findOne({ role: 'admin' });

// Find most recent user
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

### insertMany()

```ts
insertMany(docs): Promise<InsertManyResult<T>>;
```

Defined in: [src/collection-types.ts:466](https://github.com/g5becks/StrataDB/blob/89bee4bbe54bb52f1f1308d5950da4d385abbe16/src/collection-types.ts#L466)

Insert multiple documents into the collection.

#### Parameters

##### docs

readonly `Omit`\<`T`, `"id"` \| `"createdAt"` \| `"updatedAt"`\>[]

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
insertOne(doc): Promise<InsertOneResult<T>>;
```

Defined in: [src/collection-types.ts:335](https://github.com/g5becks/StrataDB/blob/89bee4bbe54bb52f1f1308d5950da4d385abbe16/src/collection-types.ts#L335)

Insert a single document into the collection.

#### Parameters

##### doc

`Omit`\<`T`, `"id"` \| `"createdAt"` \| `"updatedAt"`\>

Document to insert (without id, createdAt, updatedAt)

#### Returns

`Promise`\<[`InsertOneResult`](InsertOneResult.md)\<`T`\>\>

Promise resolving to insert result with the new document

#### Remarks

Validates the document against the schema before inserting.
Automatically generates:
- `id`: UUID v4
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
  const result = await users.insertOne({
    name: 'Bob',
    email: 'bob@example.com',
    age: 25,
    role: 'user'
  });
  console.log(`Inserted user with ID: ${result.document.id}`);
} catch (err) {
  if (err instanceof ValidationError) {
    console.error('Invalid user:', err.message);
  }
}
```

***

### replaceOne()

```ts
replaceOne(id, doc): Promise<T | null>;
```

Defined in: [src/collection-types.ts:410](https://github.com/g5becks/StrataDB/blob/89bee4bbe54bb52f1f1308d5950da4d385abbe16/src/collection-types.ts#L410)

Replace a single document matching the query filter.

#### Parameters

##### id

`string`

The document ID to replace

##### doc

`Omit`\<`T`, `"id"` \| `"createdAt"` \| `"updatedAt"`\>

New document to replace with (without id, createdAt)

#### Returns

`Promise`\<`T` \| `null`\>

Promise resolving to update result with statistics

#### Remarks

Completely replaces the matched document with the new document.
Preserves `id` and `createdAt`, updates `updatedAt`.
Validates the new document against the schema.

**Difference from updateOne:**
- `updateOne`: Modifies specific fields
- `replaceOne`: Replaces entire document

#### Throws

If new document fails schema validation

#### Example

```typescript
await users.replaceOne(
  { id: userId },
  {
    name: 'Alice Smith',
    email: 'alice.smith@example.com',
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

Defined in: [src/collection-types.ts:498](https://github.com/g5becks/StrataDB/blob/89bee4bbe54bb52f1f1308d5950da4d385abbe16/src/collection-types.ts#L498)

Update multiple documents matching the query filter.

#### Parameters

##### filter

[`QueryFilter`](QueryFilter.md)\<`T`\>

Query filter to find documents to update

##### update

`Omit`\<`Partial`\<`T`\>, `"id"` \| `"createdAt"` \| `"updatedAt"`\>

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
   id, 
   update, 
options?): Promise<T | null>;
```

Defined in: [src/collection-types.ts:373](https://github.com/g5becks/StrataDB/blob/89bee4bbe54bb52f1f1308d5950da4d385abbe16/src/collection-types.ts#L373)

Update a single document matching the query filter.

#### Parameters

##### id

`string`

The document ID to update

##### update

`Omit`\<`Partial`\<`T`\>, `"id"` \| `"createdAt"` \| `"updatedAt"`\>

Update operations to apply

##### options?

###### upsert?

`boolean`

#### Returns

`Promise`\<`T` \| `null`\>

Promise resolving to update result with statistics

#### Remarks

Updates the first document that matches the filter.
Uses MongoDB-style update operators like `$set`, `$inc`, etc.
Automatically updates `updatedAt` if present in schema.

**Update Operators:**
- `$set`: Set field values
- `$inc`: Increment numeric fields
- `$unset`: Remove fields
- `$push`: Add to arrays
- `$pull`: Remove from arrays

#### Example

```typescript
// Set field values
await users.updateOne(
  { email: 'alice@example.com' },
  { $set: { role: 'admin', verified: true } }
);

// Increment counter
await users.updateOne(
  { id: userId },
  { $inc: { loginCount: 1 } }
);
```

***

### validate()

```ts
validate(doc): Promise<T>;
```

Defined in: [src/collection-types.ts:561](https://github.com/g5becks/StrataDB/blob/89bee4bbe54bb52f1f1308d5950da4d385abbe16/src/collection-types.ts#L561)

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

Defined in: [src/collection-types.ts:586](https://github.com/g5becks/StrataDB/blob/89bee4bbe54bb52f1f1308d5950da4d385abbe16/src/collection-types.ts#L586)

Validate a document against the schema (sync).

#### Parameters

##### doc

`unknown`

Document to validate

#### Returns

`T`

The validated document typed as T

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
  const user = users.validateSync({ name: 'Alice', email: 'alice@example.com', age: 30, role: 'admin' });
  console.log('Document is valid:', user.name);
} catch (err) {
  console.error('Validation failed:', err.message);
}
```
