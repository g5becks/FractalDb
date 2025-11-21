/**
 * StrataDB - A type-safe document database built on SQLite
 *
 * @packageDocumentation
 *
 * @remarks
 * StrataDB provides a MongoDB-like API with full TypeScript type safety,
 * backed by SQLite for reliability and performance. It uses JSONB storage
 * with generated columns for indexed fields.
 *
 * @example
 * ```typescript
 * import { StrataDBClass, createSchema, type Document } from 'stratadb';
 *
 * type User = Document<{
 *   name: string;
 *   email: string;
 *   age: number;
 * }>;
 *
 * const userSchema = createSchema<User>()
 *   .field('name', { type: 'TEXT', indexed: true })
 *   .field('email', { type: 'TEXT', indexed: true, unique: true })
 *   .field('age', { type: 'INTEGER', indexed: true })
 *   .build();
 *
 * const db = new StrataDBClass({ database: 'app.db' });
 * const users = db.collection('users', userSchema);
 *
 * await users.insertOne({ name: 'Alice', email: 'alice@example.com', age: 30 });
 * const adults = await users.find({ age: { $gte: 18 } });
 * ```
 */

export type { CollectionBuilder } from "./collection-builder.js"
// Collection types
export type {
  Collection,
  DeleteResult,
  InsertManyResult,
  InsertOneResult,
  UpdateResult,
} from "./collection-types.js"
// Core document types
export type {
  BulkWriteResult,
  Document,
  DocumentInput,
  DocumentUpdate,
} from "./core-types.js"
// Database types
export type {
  DatabaseOptions,
  StrataDB,
  Transaction,
} from "./database-types.js"
// Error types
// biome-ignore lint/performance/noBarrelFile: library entry point requires re-exports
export {
  ConnectionError,
  DocDBError,
  QueryError,
  TransactionError,
  UniqueConstraintError,
  ValidationError,
} from "./errors.js"
// Path types for advanced schema definitions
export type { DocumentPath, JsonPath, PathValue } from "./path-types.js"
export type {
  ProjectionSpec,
  QueryOptions,
  SortSpec,
} from "./query-options-types.js"
// Query types
export type {
  ArrayOperator,
  ComparisonOperator,
  EqualityOperators,
  ExistenceOperator,
  FieldOperator,
  LogicalOperator,
  MembershipOperators,
  OrderingOperators,
  QueryFilter,
  StringOperator,
} from "./query-types.js"
export { createSchema } from "./schema-builder.js"
export type { SchemaBuilder } from "./schema-builder-types.js"
// Schema types and builder
export type {
  CompoundIndex,
  SchemaDefinition,
  SchemaField,
  SQLiteType,
  TimestampConfig,
  TypeScriptToSQLite,
} from "./schema-types.js"
// Main database class
export { StrataDBClass } from "./stratadb.js"
