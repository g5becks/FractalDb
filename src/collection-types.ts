import type { Document } from "./core-types.js"
import type { QueryOptions } from "./query-options-types.js"
import type { QueryFilter } from "./query-types.js"
import type { SchemaDefinition } from "./schema-types.js"

/**
 * Result of inserting a single document into a collection.
 *
 * @typeParam T - The document type
 *
 * @remarks
 * Contains the inserted document with its generated ID and metadata.
 */
export type InsertOneResult<T extends Document> = {
  /** The inserted document with its generated ID */
  readonly document: T

  /** Whether the insert operation succeeded */
  readonly acknowledged: true
}

/**
 * Result of inserting multiple documents into a collection.
 *
 * @typeParam T - The document type
 *
 * @remarks
 * Contains all successfully inserted documents with their generated IDs.
 */
export type InsertManyResult<T extends Document> = {
  /** Array of inserted documents with their generated IDs */
  readonly documents: readonly T[]

  /** Number of documents successfully inserted */
  readonly insertedCount: number

  /** Whether the insert operation succeeded */
  readonly acknowledged: true
}

/**
 * Result of an update operation.
 *
 * @remarks
 * Provides statistics about documents matched and modified by the update.
 */
export type UpdateResult = {
  /** Number of documents that matched the filter */
  readonly matchedCount: number

  /** Number of documents that were actually modified */
  readonly modifiedCount: number

  /** Whether the update operation succeeded */
  readonly acknowledged: true
}

/**
 * Result of a delete operation.
 *
 * @remarks
 * Provides the count of documents deleted by the operation.
 */
export type DeleteResult = {
  /** Number of documents deleted */
  readonly deletedCount: number

  /** Whether the delete operation succeeded */
  readonly acknowledged: true
}

/**
 * MongoDB-like collection interface for type-safe document operations.
 *
 * @typeParam T - The document type, must extend Document
 *
 * @remarks
 * Collection provides a complete CRUD API with full type safety. It wraps a SQLite
 * table and provides MongoDB-like methods for querying and manipulating documents.
 *
 * **Key Features:**
 * - Type-safe query filters with auto-completion
 * - Generated columns for indexed fields (optimized queries)
 * - JSONB storage for flexible document structure
 * - Built-in validation using Standard Schema validators
 * - Batch operations for performance
 * - Transaction support through database instance
 *
 * **Document Storage:**
 * - All documents stored in a single `data` JSONB column
 * - Indexed fields have generated columns with underscore prefix (e.g., `_age`)
 * - Auto-generated `id` and `createdAt` fields
 * - Optional `updatedAt` tracking
 *
 * **Comparison to MongoDB:**
 * - Similar API surface for easy migration
 * - Uses SQLite instead of MongoDB protocol
 * - Type-safe at compile time (MongoDB client isn't)
 * - Generated columns optimize common queries
 * - Standard Schema validation (not MongoDB JSON Schema)
 *
 * @example
 * ```typescript
 * import { StrataDB, createSchema, type Document } from 'stratadb';
 * import { z } from 'zod';
 *
 * // Define document type
 * type User = Document<{
 *   name: string;
 *   email: string;
 *   age: number;
 *   role: 'admin' | 'user';
 * }>;
 *
 * // Create schema with validation
 * const userSchema = createSchema<User>()
 *   .field('name', { type: 'TEXT', indexed: true })
 *   .field('email', { type: 'TEXT', indexed: true, unique: true })
 *   .field('age', { type: 'INTEGER', indexed: true })
 *   .field('role', { type: 'TEXT', indexed: false })
 *   .validator(z.object({
 *     name: z.string().min(1),
 *     email: z.string().email(),
 *     age: z.number().int().min(0),
 *     role: z.enum(['admin', 'user'])
 *   }))
 *   .build();
 *
 * // Open database and get collection
 * const db = new StrataDB('myapp.db');
 * const users = db.collection('users', userSchema);
 *
 * // Insert documents
 * const result = await users.insertOne({
 *   name: 'Alice',
 *   email: 'alice@example.com',
 *   age: 30,
 *   role: 'admin'
 * });
 * console.log(result.document.id); // Auto-generated UUID
 *
 * // Query documents
 * const admins = await users.find({ role: 'admin' });
 * const adults = await users.find({ age: { $gte: 18 } });
 * const alice = await users.findOne({ email: 'alice@example.com' });
 *
 * // Update documents
 * await users.updateOne(
 *   { email: 'alice@example.com' },
 *   { $set: { role: 'user' } }
 * );
 *
 * // Delete documents
 * await users.deleteMany({ age: { $lt: 18 } });
 * ```
 */
export type Collection<T extends Document> = {
  /**
   * Collection name (table name in SQLite).
   */
  readonly name: string

  /**
   * Schema definition for this collection.
   */
  readonly schema: SchemaDefinition<T>

  // ===== Read Operations =====

  /**
   * Find a single document by its ID.
   *
   * @param id - The document ID to search for
   * @returns Promise resolving to the document, or null if not found
   *
   * @remarks
   * This is the fastest way to retrieve a specific document since it uses
   * the primary key index.
   *
   * @example
   * ```typescript
   * const user = await users.findById('123e4567-e89b-12d3-a456-426614174000');
   * if (user) {
   *   console.log(user.name);
   * }
   * ```
   */
  findById(id: string): Promise<T | null>

  /**
   * Find all documents matching the query filter.
   *
   * @param filter - Query filter to match documents
   * @param options - Query options for sorting, pagination, and projection
   * @returns Promise resolving to array of matching documents
   *
   * @remarks
   * Returns all documents that match the filter. For large result sets,
   * use `limit` and `skip` options for pagination.
   *
   * **Performance:**
   * - Indexed fields use generated columns (fast)
   * - Non-indexed fields use jsonb_extract (slower)
   * - Consider adding indexes for frequently queried fields
   *
   * @example
   * ```typescript
   * // Find all active admins
   * const admins = await users.find({
   *   role: 'admin',
   *   status: 'active'
   * });
   *
   * // Find with sorting and pagination
   * const page = await users.find(
   *   { age: { $gte: 18 } },
   *   { sort: { createdAt: -1 }, limit: 20, skip: 40 }
   * );
   *
   * // Find with complex query
   * const results = await users.find({
   *   $and: [
   *     { age: { $gte: 18, $lt: 65 } },
   *     {
   *       $or: [
   *         { role: 'admin' },
   *         { verified: true }
   *       ]
   *     }
   *   ]
   * });
   * ```
   */
  find(filter: QueryFilter<T>, options?: QueryOptions<T>): Promise<readonly T[]>

  /**
   * Find the first document matching the query filter.
   *
   * @param filter - Query filter to match documents
   * @param options - Query options for sorting and projection
   * @returns Promise resolving to the first matching document, or null if none found
   *
   * @remarks
   * Equivalent to `find(filter, { ...options, limit: 1 })[0]` but more efficient
   * since it stops after finding the first match.
   *
   * @example
   * ```typescript
   * // Find any admin user
   * const admin = await users.findOne({ role: 'admin' });
   *
   * // Find most recent user
   * const latest = await users.findOne(
   *   {},
   *   { sort: { createdAt: -1 } }
   * );
   *
   * // Check if email exists
   * const exists = await users.findOne({ email: 'alice@example.com' });
   * if (exists) {
   *   console.log('Email already registered');
   * }
   * ```
   */
  findOne(
    filter: QueryFilter<T>,
    options?: Omit<QueryOptions<T>, "limit" | "skip">
  ): Promise<T | null>

  /**
   * Count documents matching the query filter.
   *
   * @param filter - Query filter to match documents
   * @returns Promise resolving to the count of matching documents
   *
   * @remarks
   * More efficient than `find(filter).length` since it only counts without
   * retrieving document data.
   *
   * @example
   * ```typescript
   * // Count all users
   * const total = await users.count({});
   *
   * // Count active admins
   * const adminCount = await users.count({
   *   role: 'admin',
   *   status: 'active'
   * });
   *
   * // Count adults
   * const adultCount = await users.count({ age: { $gte: 18 } });
   * ```
   */
  count(filter: QueryFilter<T>): Promise<number>

  // ===== Single Write Operations =====

  /**
   * Insert a single document into the collection.
   *
   * @param doc - Document to insert (without id, createdAt, updatedAt)
   * @returns Promise resolving to insert result with the new document
   *
   * @remarks
   * Validates the document against the schema before inserting.
   * Automatically generates:
   * - `id`: UUID v4
   * - `createdAt`: Current timestamp
   * - `updatedAt`: Current timestamp (if schema has this field)
   *
   * **Validation:**
   * If validation fails, throws `ValidationError` with details.
   *
   * @throws {ValidationError} If document fails schema validation
   * @throws {ConstraintError} If unique constraint is violated
   *
   * @example
   * ```typescript
   * try {
   *   const result = await users.insertOne({
   *     name: 'Bob',
   *     email: 'bob@example.com',
   *     age: 25,
   *     role: 'user'
   *   });
   *   console.log(`Inserted user with ID: ${result.document.id}`);
   * } catch (err) {
   *   if (err instanceof ValidationError) {
   *     console.error('Invalid user:', err.message);
   *   }
   * }
   * ```
   */
  insertOne(
    doc: Omit<T, "id" | "createdAt" | "updatedAt">
  ): Promise<InsertOneResult<T>>

  /**
   * Update a single document matching the query filter.
   *
   * @param filter - Query filter to find the document to update
   * @param update - Update operations to apply
   * @returns Promise resolving to update result with statistics
   *
   * @remarks
   * Updates the first document that matches the filter.
   * Uses MongoDB-style update operators like `$set`, `$inc`, etc.
   * Automatically updates `updatedAt` if present in schema.
   *
   * **Update Operators:**
   * - `$set`: Set field values
   * - `$inc`: Increment numeric fields
   * - `$unset`: Remove fields
   * - `$push`: Add to arrays
   * - `$pull`: Remove from arrays
   *
   * @example
   * ```typescript
   * // Set field values
   * await users.updateOne(
   *   { email: 'alice@example.com' },
   *   { $set: { role: 'admin', verified: true } }
   * );
   *
   * // Increment counter
   * await users.updateOne(
   *   { id: userId },
   *   { $inc: { loginCount: 1 } }
   * );
   * ```
   */
  updateOne(
    id: string,
    update: Omit<Partial<T>, "id" | "createdAt" | "updatedAt">,
    options?: { upsert?: boolean }
  ): Promise<T | null>

  /**
   * Replace a single document matching the query filter.
   *
   * @param filter - Query filter to find the document to replace
   * @param doc - New document to replace with (without id, createdAt)
   * @returns Promise resolving to update result with statistics
   *
   * @remarks
   * Completely replaces the matched document with the new document.
   * Preserves `id` and `createdAt`, updates `updatedAt`.
   * Validates the new document against the schema.
   *
   * **Difference from updateOne:**
   * - `updateOne`: Modifies specific fields
   * - `replaceOne`: Replaces entire document
   *
   * @throws {ValidationError} If new document fails schema validation
   *
   * @example
   * ```typescript
   * await users.replaceOne(
   *   { id: userId },
   *   {
   *     name: 'Alice Smith',
   *     email: 'alice.smith@example.com',
   *     age: 31,
   *     role: 'admin'
   *   }
   * );
   * ```
   */
  replaceOne(
    id: string,
    doc: Omit<T, "id" | "createdAt" | "updatedAt">
  ): Promise<T | null>

  /**
   * Delete a single document matching the query filter.
   *
   * @param filter - Query filter to find the document to delete
   * @returns Promise resolving to delete result with statistics
   *
   * @remarks
   * Deletes the first document that matches the filter.
   * If no document matches, returns `{ deletedCount: 0 }`.
   *
   * @example
   * ```typescript
   * const result = await users.deleteOne({ id: userId });
   * if (result.deletedCount > 0) {
   *   console.log('User deleted successfully');
   * } else {
   *   console.log('User not found');
   * }
   * ```
   */
  deleteOne(id: string): Promise<boolean>

  // ===== Batch Write Operations =====

  /**
   * Insert multiple documents into the collection.
   *
   * @param docs - Array of documents to insert
   * @returns Promise resolving to insert result with all new documents
   *
   * @remarks
   * Inserts all documents in a single transaction for performance.
   * If any document fails validation, the entire operation rolls back.
   * Automatically generates `id`, `createdAt`, and `updatedAt` for each document.
   *
   * **Performance:**
   * Much faster than multiple `insertOne` calls since it uses a single transaction.
   *
   * @throws {ValidationError} If any document fails schema validation
   * @throws {ConstraintError} If any unique constraint is violated
   *
   * @example
   * ```typescript
   * const result = await users.insertMany([
   *   { name: 'Alice', email: 'alice@example.com', age: 30, role: 'admin' },
   *   { name: 'Bob', email: 'bob@example.com', age: 25, role: 'user' },
   *   { name: 'Charlie', email: 'charlie@example.com', age: 35, role: 'user' }
   * ]);
   * console.log(`Inserted ${result.insertedCount} users`);
   * ```
   */
  insertMany(
    docs: readonly Omit<T, "id" | "createdAt" | "updatedAt">[]
  ): Promise<InsertManyResult<T>>

  /**
   * Update multiple documents matching the query filter.
   *
   * @param filter - Query filter to find documents to update
   * @param update - Update operations to apply
   * @returns Promise resolving to update result with statistics
   *
   * @remarks
   * Updates all documents that match the filter.
   * Uses MongoDB-style update operators.
   * Automatically updates `updatedAt` for each modified document.
   *
   * @example
   * ```typescript
   * // Set all inactive users to deleted status
   * const result = await users.updateMany(
   *   { status: 'inactive' },
   *   { $set: { status: 'deleted' } }
   * );
   * console.log(`Updated ${result.modifiedCount} users`);
   *
   * // Give all admins a badge
   * await users.updateMany(
   *   { role: 'admin' },
   *   { $set: { badge: 'admin' } }
   * );
   * ```
   */
  updateMany(
    filter: QueryFilter<T>,
    update: Omit<Partial<T>, "id" | "createdAt" | "updatedAt">
  ): Promise<UpdateResult>

  /**
   * Delete multiple documents matching the query filter.
   *
   * @param filter - Query filter to find documents to delete
   * @returns Promise resolving to delete result with statistics
   *
   * @remarks
   * Deletes all documents that match the filter.
   * Use an empty filter `{}` to delete all documents in the collection.
   *
   * **Warning:**
   * This operation cannot be undone. Consider backing up data before
   * bulk delete operations.
   *
   * @example
   * ```typescript
   * // Delete all inactive users
   * const result = await users.deleteMany({ status: 'inactive' });
   * console.log(`Deleted ${result.deletedCount} users`);
   *
   * // Delete all documents (use with caution!)
   * await users.deleteMany({});
   * ```
   */
  deleteMany(filter: QueryFilter<T>): Promise<DeleteResult>

  // ===== Validation =====

  /**
   * Validate a document against the schema (async).
   *
   * @param doc - Document to validate
   * @returns Promise resolving to true if valid
   *
   * @remarks
   * Validates the document against the schema validator.
   * Supports both synchronous and asynchronous validators.
   * Throws `ValidationError` if validation fails.
   *
   * @throws {ValidationError} If document fails validation
   *
   * @example
   * ```typescript
   * try {
   *   await users.validate({
   *     name: 'Alice',
   *     email: 'invalid-email',
   *     age: -5,
   *     role: 'admin'
   *   });
   * } catch (err) {
   *   if (err instanceof ValidationError) {
   *     console.error(`Invalid field: ${err.field}`);
   *     console.error(`Error: ${err.message}`);
   *   }
   * }
   * ```
   */
  validate(doc: unknown): Promise<T>

  /**
   * Validate a document against the schema (sync).
   *
   * @param doc - Document to validate
   * @returns True if valid
   *
   * @remarks
   * Synchronous version of `validate()`. Only works with synchronous validators.
   * Throws error if the schema uses async validators.
   *
   * @throws {ValidationError} If document fails validation
   * @throws {TypeError} If schema uses async validators
   *
   * @example
   * ```typescript
   * try {
   *   users.validateSync({ name: 'Alice', email: 'alice@example.com', age: 30, role: 'admin' });
   *   console.log('Document is valid');
   * } catch (err) {
   *   console.error('Validation failed:', err.message);
   * }
   * ```
   */
  validateSync(doc: unknown): T
}
