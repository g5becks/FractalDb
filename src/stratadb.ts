import { Database as SQLiteDatabase } from "bun:sqlite"
import {
  type CollectionBuilder,
  CollectionBuilderImpl,
} from "./collection-builder.js"
import type { Collection } from "./collection-types.js"
import type { Document } from "./core-types.js"
import type {
  DatabaseOptions,
  StrataDB as StrataDBInterface,
  Transaction,
} from "./database-types.js"
import type { SchemaDefinition } from "./schema-types.js"
import { SQLiteCollection } from "./sqlite-collection.js"

/**
 * Main StrataDB database class.
 *
 * @remarks
 * StrataDB provides a MongoDB-like document database API backed by SQLite.
 * It manages collections, transactions, and database lifecycle.
 *
 * **Features:**
 * - Type-safe document operations
 * - JSONB storage with indexed generated columns
 * - Transaction support with automatic rollback
 * - Symbol.dispose for automatic cleanup
 *
 * @example
 * ```typescript
 * // Create database with path
 * const db = new StrataDBClass({ database: 'myapp.db' });
 *
 * // Create in-memory database
 * const memDb = new StrataDBClass({ database: ':memory:' });
 *
 * // With custom options
 * const customDb = new StrataDBClass({
 *   database: 'app.db',
 *   idGenerator: () => `custom-${Date.now()}`,
 *   onClose: () => console.log('Database closed'),
 *   debug: true
 * });
 *
 * // Using with automatic cleanup
 * using db = new StrataDBClass({ database: ':memory:' });
 * const users = db.collection('users', userSchema);
 * // Database automatically closes when scope exits
 * ```
 */
export class StrataDBClass implements StrataDBInterface {
  readonly sqliteDb: SQLiteDatabase

  private readonly idGeneratorFn: () => string
  private readonly onCloseCallback: (() => void) | undefined
  private readonly debug: boolean
  private readonly collections = new Map<string, Collection<Document>>()

  /**
   * Creates a new StrataDB instance.
   *
   * @param options - Database configuration options
   */
  constructor(options: DatabaseOptions) {
    // Initialize database connection
    if (typeof options.database === "string") {
      this.sqliteDb = new SQLiteDatabase(options.database)
    } else {
      this.sqliteDb = options.database
    }

    this.idGeneratorFn = options.idGenerator ?? (() => Bun.randomUUIDv7())
    this.onCloseCallback = options.onClose
    this.debug = options.debug ?? false

    if (this.debug) {
      console.log("[StrataDB] Database initialized")
    }
  }

  /**
   * Generates a new unique ID using the configured ID generator.
   *
   * @returns A new unique identifier string
   */
  generateId(): string {
    return this.idGeneratorFn()
  }

  /**
   * Gets or creates a collection with a pre-built schema.
   */
  collection<T extends Document>(
    name: string,
    schema: SchemaDefinition<T>
  ): Collection<T>

  /**
   * Creates a collection builder for fluent schema definition.
   */
  collection<T extends Document>(name: string): CollectionBuilder<T>

  /**
   * Implementation of overloaded collection method.
   */
  collection<T extends Document>(
    name: string,
    schema?: SchemaDefinition<T>
  ): Collection<T> | CollectionBuilder<T> {
    if (!schema) {
      // Return builder for fluent schema definition
      if (this.debug) {
        console.log(`[StrataDB] Creating collection builder for '${name}'`)
      }
      return new CollectionBuilderImpl<T>(
        this.sqliteDb,
        name,
        this.idGeneratorFn
      )
    }

    const existing = this.collections.get(name)
    if (existing) {
      return existing as Collection<T>
    }

    const collection = new SQLiteCollection<T>(
      this.sqliteDb,
      name,
      schema,
      this.idGeneratorFn
    )
    this.collections.set(name, collection as Collection<Document>)

    if (this.debug) {
      console.log(`[StrataDB] Collection '${name}' created`)
    }

    return collection
  }

  /**
   * Creates a new transaction.
   *
   * @returns Transaction instance
   */
  transaction(): Transaction {
    this.sqliteDb.exec("BEGIN TRANSACTION")

    if (this.debug) {
      console.log("[StrataDB] Transaction started")
    }

    let committed = false

    const tx: Transaction = {
      collection: <T extends Document>(
        name: string,
        schema: SchemaDefinition<T>
      ): Collection<T> => this.collection(name, schema),

      commit: () => {
        this.sqliteDb.exec("COMMIT")
        committed = true
        if (this.debug) {
          console.log("[StrataDB] Transaction committed")
        }
      },

      rollback: () => {
        if (!committed) {
          this.sqliteDb.exec("ROLLBACK")
          if (this.debug) {
            console.log("[StrataDB] Transaction rolled back")
          }
        }
      },

      [Symbol.dispose]: () => {
        if (!committed) {
          tx.rollback()
        }
      },
    }

    return tx
  }

  /**
   * Executes a function within a transaction.
   *
   * @param fn - Function to execute
   * @returns Result of the function
   */
  async execute<R>(fn: (tx: Transaction) => R | Promise<R>): Promise<R> {
    const tx = this.transaction()
    try {
      const result = await fn(tx)
      tx.commit()
      return result
    } catch (error) {
      tx.rollback()
      throw error
    }
  }

  /**
   * Closes the database connection.
   */
  close(): void {
    if (this.debug) {
      console.log("[StrataDB] Closing database")
    }

    this.onCloseCallback?.()
    this.sqliteDb.close()
  }

  /**
   * Disposes the database (closes connection).
   */
  [Symbol.dispose](): void {
    this.close()
  }
}
