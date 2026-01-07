# stratadb

StrataDB - A type-safe document database built on SQLite

## Remarks

StrataDB provides a MongoDB-like API with full TypeScript type safety,
backed by SQLite for reliability and performance. It uses JSONB storage
with generated columns for indexed fields.

## Example

```typescript
import { Strata, createSchema, type Document } from 'stratadb';

type User = Document<{
  name: string;
  email: string;
  age: number;
}>;

const userSchema = createSchema<User>()
  .field('name', { type: 'TEXT', indexed: true })
  .field('email', { type: 'TEXT', indexed: true, unique: true })
  .field('age', { type: 'INTEGER', indexed: true })
  .build();

const db = new Strata({ database: 'app.db' });
const users = db.collection('users', userSchema);

await users.insertOne({ name: 'Alice', email: 'alice@example.com', age: 30 });
const adults = await users.find({ age: { $gte: 18 } });
```

## Classes

- [AbortedError](classes/AbortedError.md)
- [ConnectionError](classes/ConnectionError.md)
- [QueryError](classes/QueryError.md)
- [Strata](classes/Strata.md)
- [StrataDBError](classes/StrataDBError.md)
- [TransactionError](classes/TransactionError.md)
- [UniqueConstraintError](classes/UniqueConstraintError.md)
- [ValidationError](classes/ValidationError.md)

## Type Aliases

- [ArrayOperator](type-aliases/ArrayOperator.md)
- [BulkWriteResult](type-aliases/BulkWriteResult.md)
- [Collection](type-aliases/Collection.md)
- [CollectionBuilder](type-aliases/CollectionBuilder.md)
- [ComparisonOperator](type-aliases/ComparisonOperator.md)
- [CompoundIndex](type-aliases/CompoundIndex.md)
- [DatabaseOptions](type-aliases/DatabaseOptions.md)
- [DeleteResult](type-aliases/DeleteResult.md)
- [Document](type-aliases/Document.md)
- [DocumentInput](type-aliases/DocumentInput.md)
- [DocumentPath](type-aliases/DocumentPath.md)
- [DocumentUpdate](type-aliases/DocumentUpdate.md)
- [EqualityOperators](type-aliases/EqualityOperators.md)
- [ExistenceOperator](type-aliases/ExistenceOperator.md)
- [FieldOperator](type-aliases/FieldOperator.md)
- [IdGenerator](type-aliases/IdGenerator.md)
- [InsertManyResult](type-aliases/InsertManyResult.md)
- [InsertOneResult](type-aliases/InsertOneResult.md)
- [JsonPath](type-aliases/JsonPath.md)
- [LogicalOperator](type-aliases/LogicalOperator.md)
- [MembershipOperators](type-aliases/MembershipOperators.md)
- [OrderingOperators](type-aliases/OrderingOperators.md)
- [PathValue](type-aliases/PathValue.md)
- [ProjectionSpec](type-aliases/ProjectionSpec.md)
- [QueryFilter](type-aliases/QueryFilter.md)
- [QueryOptions](type-aliases/QueryOptions.md)
- [RetryContext](type-aliases/RetryContext.md)
- [RetryOptions](type-aliases/RetryOptions.md)
- [SchemaBuilder](type-aliases/SchemaBuilder.md)
- [SchemaDefinition](type-aliases/SchemaDefinition.md)
- [SchemaField](type-aliases/SchemaField.md)
- [SortSpec](type-aliases/SortSpec.md)
- [SQLiteType](type-aliases/SQLiteType.md)
- [StrataDB](type-aliases/StrataDB.md)
- [StringOperator](type-aliases/StringOperator.md)
- [TimestampConfig](type-aliases/TimestampConfig.md)
- [Transaction](type-aliases/Transaction.md)
- [TypeScriptToSQLite](type-aliases/TypeScriptToSQLite.md)
- [UpdateResult](type-aliases/UpdateResult.md)

## Functions

- [createSchema](functions/createSchema.md)
- [dateToTimestamp](functions/dateToTimestamp.md)
- [defaultShouldRetry](functions/defaultShouldRetry.md)
- [generateId](functions/generateId.md)
- [isTimestampInRange](functions/isTimestampInRange.md)
- [isValidTimestamp](functions/isValidTimestamp.md)
- [mergeRetryOptions](functions/mergeRetryOptions.md)
- [nowTimestamp](functions/nowTimestamp.md)
- [timestampDiff](functions/timestampDiff.md)
- [timestampToDate](functions/timestampToDate.md)
- [withRetry](functions/withRetry.md)
