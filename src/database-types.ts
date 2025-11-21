import type { Database as SQLiteDatabase } from "bun:sqlite"
import type { CollectionBuilder } from "./collection-builder.js"
import type { Collection } from "./collection-types.js"
import type { Document } from "./core-types.js"
import type { SchemaDefinition } from "./schema-types.js"

/**
 * Options for creating a StrataDB instance.
 *
 * @example
 * ```typescript
 * const options: DatabaseOptions = {
 *   database: ':memory:',
 *   idGenerator: () => crypto.randomUUID(),
 *   onClose: () => console.log('Database closed'),
 *   debug: true
 * };
 * const db = new StrataDB(options);
 * ```
 */
export type DatabaseOptions = {
  /**
   * SQLite database path or ':memory:' for in-memory database.
   * Can also be an existing bun:sqlite Database instance.
   */
  readonly database: string | SQLiteDatabase

  /**
   * Custom ID generator function. Defaults to crypto.randomUUID().
   */
  readonly idGenerator?: () => string

  /**
   * Callback invoked when database is closed.
   */
  readonly onClose?: () => void

  /**
   * Enable debug logging.
   */
  readonly debug?: boolean
}

/**
 * Transaction interface for atomic multi-operation execution.
 *
 * @remarks
 * Transactions ensure that multiple operations either all succeed or all fail.
 * Uses SQLite's transaction support underneath. Implements Symbol.dispose for
 * automatic rollback if commit is not called.
 *
 * @example
 * ```typescript
 * // Using try/finally pattern
 * const tx = db.transaction();
 * try {
 *   const users = tx.collection('users', userSchema);
 *   await users.insertOne({ name: 'Alice' });
 *   await users.insertOne({ name: 'Bob' });
 *   tx.commit();
 * } catch (error) {
 *   tx.rollback();
 *   throw error;
 * }
 *
 * // Using Symbol.dispose (automatic rollback on scope exit)
 * using tx = db.transaction();
 * const users = tx.collection('users', userSchema);
 * await users.insertOne({ name: 'Alice' });
 * tx.commit(); // Must call commit, otherwise rollback on scope exit
 * ```
 */
export type Transaction = {
  /**
   * Gets a collection within this transaction.
   *
   * @param name - Collection name
   * @param schema - Schema definition for the collection
   * @returns Collection bound to this transaction
   */
  collection<T extends Document>(
    name: string,
    schema: SchemaDefinition<T>
  ): Collection<T>

  /**
   * Commits all changes made within this transaction.
   */
  commit(): void

  /**
   * Rolls back all changes made within this transaction.
   */
  rollback(): void

  /**
   * Disposes the transaction (rolls back if not committed).
   * Enables `using tx = db.transaction()` syntax.
   */
  [Symbol.dispose](): void
}

/**
 * Main StrataDB interface for database operations.
 *
 * @remarks
 * StrataDB provides a MongoDB-like API backed by SQLite. It manages collections,
 * transactions, and database lifecycle. Implements Symbol.dispose for automatic cleanup.
 *
 * @example
 * ```typescript
 * // Create database
 * const db = new StrataDB({ database: 'myapp.db' });
 *
 * // Get or create a collection
 * const users = db.collection('users', userSchema);
 *
 * // Insert documents
 * await users.insertOne({ name: 'Alice', email: 'alice@example.com' });
 *
 * // Query documents
 * const adults = await users.find({ age: { $gte: 18 } });
 *
 * // Close database when done
 * db.close();
 * ```
 */
export type StrataDB = {
  /**
   * Direct access to the underlying SQLite database.
   * Use for advanced operations not covered by StrataDB API.
   */
  readonly sqliteDb: SQLiteDatabase

  /**
   * Generates a new unique ID using the configured ID generator.
   *
   * @returns A new unique identifier string
   */
  generateId(): string

  /**
   * Gets or creates a collection with a pre-built schema.
   *
   * @param name - Collection name (table name in SQLite)
   * @param schema - Schema definition for type safety and validation
   * @returns Collection instance for the specified type
   *
   * @example
   * ```typescript
   * const users = db.collection('users', userSchema);
   * ```
   */
  collection<T extends Document>(
    name: string,
    schema: SchemaDefinition<T>
  ): Collection<T>

  /**
   * Creates a collection builder for fluent schema definition.
   *
   * @param name - Collection name (table name in SQLite)
   * @returns CollectionBuilder for defining schema inline
   *
   * @example
   * ```typescript
   * const users = db.collection<User>('users')
   *   .field('name', { type: 'TEXT', indexed: true })
   *   .field('email', { type: 'TEXT', indexed: true, unique: true })
   *   .build();
   * ```
   */
  collection<T extends Document>(name: string): CollectionBuilder<T>

  /**
   * Creates a new transaction for atomic operations.
   *
   * @returns Transaction instance
   *
   * @example
   * ```typescript
   * using tx = db.transaction();
   * // operations...
   * tx.commit();
   * ```
   */
  transaction(): Transaction

  /**
   * Executes a function within a transaction.
   *
   * @param fn - Function to execute within transaction
   * @returns Result of the function
   *
   * @remarks
   * Automatically commits on success, rolls back on error.
   *
   * @example
   * ```typescript
   * await db.execute(async (tx) => {
   *   const users = tx.collection('users', userSchema);
   *   await users.insertOne({ name: 'Alice' });
   *   await users.insertOne({ name: 'Bob' });
   * });
   * ```
   */
  execute<R>(fn: (tx: Transaction) => R | Promise<R>): Promise<R>

  /**
   * Closes the database connection.
   */
  close(): void

  /**
   * Disposes the database (closes connection).
   * Enables `using db = new StrataDB(...)` syntax.
   */
  [Symbol.dispose](): void
}
