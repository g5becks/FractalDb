import type { Database as SQLiteDatabase, SQLQueryBindings } from "bun:sqlite"
import stringify from "fast-safe-stringify"
import type {
  Collection,
  DeleteResult,
  InsertManyResult,
  InsertOneResult,
  UpdateResult,
} from "./collection-types.js"
import type { Document } from "./core-types.js"
import { UniqueConstraintError, ValidationError } from "./errors.js"
import type { QueryOptions } from "./query-options-types.js"
import type { QueryFilter } from "./query-types.js"
import type { SchemaDefinition } from "./schema-types.js"
import { SQLiteQueryTranslator } from "./sqlite-query-translator.js"

/** Regex to extract field name from SQLite unique constraint error */
const UNIQUE_CONSTRAINT_REGEX = /UNIQUE constraint failed: \w+\.(\w+)/

/** Regex to remove underscore prefix from generated column names */
const UNDERSCORE_PREFIX_REGEX = /^_/

/**
 * SQLite-based implementation of the Collection interface.
 *
 * @typeParam T - The document type, must extend Document
 *
 * @remarks
 * SQLiteCollection provides a MongoDB-like API backed by SQLite with JSONB storage.
 * It uses generated columns for indexed fields to enable efficient querying while
 * maintaining document flexibility.
 *
 * **Table Structure:**
 * - `id`: TEXT PRIMARY KEY - Document identifier
 * - `body`: BLOB - JSONB document storage
 * - `createdAt`: INTEGER - Unix timestamp (if in schema)
 * - `updatedAt`: INTEGER - Unix timestamp (if in schema)
 * - Generated columns for indexed fields (e.g., `_age`, `_email`)
 *
 * **Generated Columns:**
 * Indexed fields are extracted from the JSONB `body` column into generated columns
 * for efficient querying. For example:
 * - Field `age` with `indexed: true` creates column `_age GENERATED ALWAYS AS (jsonb_extract(body, '$.age'))`
 * - Indexes are created on these generated columns
 * - Queries use generated columns for WHERE clauses (fast)
 * - Non-indexed fields use jsonb_extract directly (slower)
 *
 * **Performance Benefits:**
 * - Indexed field queries use B-tree indexes (O(log n))
 * - Non-indexed field queries scan JSONB (O(n))
 * - Generated columns have zero storage overhead (VIRTUAL)
 * - Full JSONB flexibility with SQL query performance
 *
 * @example
 * ```typescript
 * import { Database } from 'bun:sqlite';
 * import { SQLiteCollection, createSchema, type Document } from 'stratadb';
 *
 * type User = Document<{
 *   name: string;
 *   email: string;
 *   age: number;
 * }>;
 *
 * const schema = createSchema<User>()
 *   .field('name', { type: 'TEXT', indexed: true })
 *   .field('email', { type: 'TEXT', indexed: true, unique: true })
 *   .field('age', { type: 'INTEGER', indexed: true })
 *   .build();
 *
 * const db = new Database('myapp.db');
 * const users = new SQLiteCollection(db, 'users', schema, () => Bun.randomUUIDv7());
 *
 * // Table is created automatically with:
 * // - id, body, createdAt, updatedAt columns
 * // - Generated columns: _name, _email, _age
 * // - Indexes on _name, _age
 * // - Unique index on _email
 * ```
 */
export class SQLiteCollection<T extends Document> implements Collection<T> {
  /** Collection name (table name in SQLite) */
  readonly name: string

  /** Schema definition for this collection */
  readonly schema: SchemaDefinition<T>

  /** SQLite database instance */
  private readonly db: SQLiteDatabase

  /** Query translator for converting filters to SQL */
  private readonly translator: SQLiteQueryTranslator<T>

  /** ID generator function for creating document IDs */
  private readonly idGenerator: () => string

  /**
   * Creates a new SQLite collection.
   *
   * @param db - SQLite database instance
   * @param name - Collection name (table name)
   * @param schema - Schema definition for the document type
   * @param idGenerator - Function to generate unique document IDs
   *
   * @remarks
   * The constructor automatically creates the table and indexes if they don't exist.
   */
  constructor(
    db: SQLiteDatabase,
    name: string,
    schema: SchemaDefinition<T>,
    idGenerator: () => string
  ) {
    this.db = db
    this.name = name
    this.schema = schema
    this.idGenerator = idGenerator
    this.translator = new SQLiteQueryTranslator(schema)

    // Initialize table and indexes
    this.createTable()
  }

  /**
   * Creates the SQLite table with JSONB storage and generated columns.
   *
   * @remarks
   * Generates and executes SQL statements to create:
   * 1. Main table with id, body, createdAt, updatedAt columns
   * 2. Generated columns for indexed fields
   * 3. Indexes on generated columns
   * 4. Unique constraints where specified
   *
   * Uses CREATE TABLE IF NOT EXISTS for idempotency.
   *
   * **Generated SQL Example:**
   * ```sql
   * CREATE TABLE IF NOT EXISTS users (
   *   id TEXT PRIMARY KEY,
   *   body BLOB NOT NULL,
   *   createdAt INTEGER NOT NULL,
   *   updatedAt INTEGER NOT NULL,
   *   _name TEXT GENERATED ALWAYS AS (jsonb_extract(body, '$.name')) VIRTUAL,
   *   _email TEXT GENERATED ALWAYS AS (jsonb_extract(body, '$.email')) VIRTUAL,
   *   _age INTEGER GENERATED ALWAYS AS (jsonb_extract(body, '$.age')) VIRTUAL
   * );
   * CREATE INDEX IF NOT EXISTS idx_users_name ON users(_name);
   * CREATE UNIQUE INDEX IF NOT EXISTS idx_users_email ON users(_email);
   * CREATE INDEX IF NOT EXISTS idx_users_age ON users(_age);
   * ```
   *
   * @internal
   */
  // biome-ignore lint/correctness/noUnusedPrivateClassMembers: called in constructor
  private createTable(): void {
    const columns: string[] = [
      "id TEXT PRIMARY KEY",
      "body BLOB NOT NULL",
      "createdAt INTEGER NOT NULL",
      "updatedAt INTEGER NOT NULL",
    ]

    // Add generated columns for indexed fields
    for (const field of this.schema.fields) {
      if (field.indexed) {
        const columnName = `_${String(field.name)}`
        const sqlType = this.mapTypeToSQLite(field.type)
        const path = `$.${String(field.name)}`
        columns.push(
          `${columnName} ${sqlType} GENERATED ALWAYS AS (jsonb_extract(body, '${path}')) VIRTUAL`
        )
      }
    }

    // Create table
    const createTableSQL = `
      CREATE TABLE IF NOT EXISTS ${this.name} (
        ${columns.join(",\n        ")}
      )
    `
    this.db.run(createTableSQL)

    // Create indexes
    this.createIndexes()
  }

  /**
   * Creates indexes on generated columns for indexed fields.
   *
   * @remarks
   * For each indexed field, creates an index on its generated column.
   * Unique fields get UNIQUE indexes, others get regular indexes.
   *
   * Index names follow the pattern: `idx_{table}_{field}`
   *
   * @internal
   */
  private createIndexes(): void {
    for (const field of this.schema.fields) {
      if (field.indexed) {
        const columnName = `_${String(field.name)}`
        const indexName = `idx_${this.name}_${String(field.name)}`
        const uniqueKeyword = field.unique ? "UNIQUE" : ""

        const createIndexSQL = `
          CREATE ${uniqueKeyword} INDEX IF NOT EXISTS ${indexName}
          ON ${this.name}(${columnName})
        `
        this.db.run(createIndexSQL)
      }
    }
  }

  /**
   * Maps schema field types to SQLite column types for generated columns.
   *
   * @param type - Schema field type
   * @returns SQLite column type
   *
   * @remarks
   * Maps StrataDB schema types to actual SQLite column types:
   * - TEXT → TEXT
   * - INTEGER → INTEGER
   * - REAL → REAL
   * - BLOB → BLOB
   * - BOOLEAN → INTEGER (stored as 0/1)
   * - NUMERIC → NUMERIC
   *
   * @internal
   */
  private mapTypeToSQLite(
    type: "TEXT" | "INTEGER" | "REAL" | "BLOB" | "BOOLEAN" | "NUMERIC"
  ): "TEXT" | "INTEGER" | "REAL" | "BLOB" | "NUMERIC" {
    switch (type) {
      case "TEXT":
        return "TEXT"
      case "INTEGER":
        return "INTEGER"
      case "REAL":
        return "REAL"
      case "BLOB":
        return "BLOB"
      case "BOOLEAN":
        return "INTEGER" // SQLite doesn't have BOOLEAN, use INTEGER (0/1)
      case "NUMERIC":
        return "NUMERIC"
      default:
        return "TEXT" // Fallback to TEXT for unknown types
    }
  }

  // ===== Collection Interface Implementation =====
  // The following methods will be implemented in subsequent tasks

  /**
   * Retrieves a document by its unique identifier.
   *
   * @param id - The document ID to search for
   * @returns Promise resolving to the document if found, or null if not found
   *
   * @remarks
   * This method uses a prepared statement for efficient ID lookups.
   * The JSONB body is converted to a JavaScript object using SQLite's json() function.
   *
   * @example
   * ```typescript
   * const user = await users.findById('user-123');
   * if (user) {
   *   console.log(user.name);
   * } else {
   *   console.log('User not found');
   * }
   * ```
   */
  findById(id: string): Promise<T | null> {
    const stmt = this.db.prepare<{ id: string; body: string }, [string]>(
      `SELECT id, json(body) as body FROM ${this.name} WHERE id = ?`
    )
    const row = stmt.get(id)

    if (!row) {
      return Promise.resolve(null)
    }

    const doc = JSON.parse(row.body) as Omit<T, "id">
    return Promise.resolve({ id: row.id, ...doc } as T)
  }

  /**
   * Finds all documents matching the given filter with optional query options.
   *
   * @param filter - Query filter to match documents
   * @param options - Optional query options (sort, limit, skip)
   * @returns Promise resolving to array of matching documents
   *
   * @remarks
   * Uses the query translator to convert type-safe filters to SQL WHERE clauses.
   * Supports sorting, pagination (limit/skip), and projection.
   *
   * @example
   * ```typescript
   * // Find active users over 18, sorted by name
   * const users = await collection.find(
   *   { status: 'active', age: { $gte: 18 } },
   *   { sort: { name: 1 }, limit: 10 }
   * );
   *
   * // Find with complex filter
   * const results = await collection.find({
   *   $or: [
   *     { role: 'admin' },
   *     { permissions: { $in: ['write', 'delete'] } }
   *   ]
   * });
   * ```
   */
  find(
    filter: QueryFilter<T>,
    options?: QueryOptions<T>
  ): Promise<readonly T[]> {
    const { sql: whereClause, params } = this.translator.translate(filter)

    let sql = `SELECT id, json(body) as body FROM ${this.name}`
    if (whereClause) {
      sql += ` WHERE ${whereClause}`
    }

    // Apply sort
    if (options?.sort) {
      const sortClauses: string[] = []
      for (const [field, direction] of Object.entries(options.sort)) {
        const order = direction === 1 ? "ASC" : "DESC"
        // Use generated column name for indexed fields, otherwise jsonb_extract
        const indexedField = this.schema.fields.find(
          (f) => String(f.name) === field && f.indexed
        )
        const column = indexedField
          ? `_${field}`
          : `jsonb_extract(body, '$.${field}')`
        sortClauses.push(`${column} ${order}`)
      }
      if (sortClauses.length > 0) {
        sql += ` ORDER BY ${sortClauses.join(", ")}`
      }
    }

    // Apply limit and skip
    if (options?.limit !== undefined) {
      sql += ` LIMIT ${options.limit}`
    }
    if (options?.skip !== undefined) {
      sql += ` OFFSET ${options.skip}`
    }

    const stmt = this.db.prepare<
      { id: string; body: string },
      SQLQueryBindings[]
    >(sql)
    const rows = stmt.all(...params)

    const results = rows.map((row) => {
      const doc = JSON.parse(row.body) as Omit<T, "id">
      return { id: row.id, ...doc } as T
    })

    return Promise.resolve(results)
  }

  /**
   * Finds the first document matching the given filter.
   *
   * @param filter - Query filter to match documents
   * @param options - Optional query options (sort only, no limit/skip)
   * @returns Promise resolving to the first matching document or null
   *
   * @example
   * ```typescript
   * const admin = await users.findOne({ role: 'admin' });
   * if (admin) {
   *   console.log(`Found admin: ${admin.name}`);
   * }
   * ```
   */
  async findOne(
    filter: QueryFilter<T>,
    options?: Omit<QueryOptions<T>, "limit" | "skip">
  ): Promise<T | null> {
    const results = await this.find(filter, { ...options, limit: 1 })
    return results[0] ?? null
  }

  /**
   * Counts documents matching the given filter.
   *
   * @param filter - Query filter to match documents
   * @returns Promise resolving to the count of matching documents
   *
   * @example
   * ```typescript
   * const activeCount = await users.count({ status: 'active' });
   * console.log(`Active users: ${activeCount}`);
   * ```
   */
  count(filter: QueryFilter<T>): Promise<number> {
    const { sql: whereClause, params } = this.translator.translate(filter)

    let sql = `SELECT COUNT(*) as count FROM ${this.name}`
    if (whereClause) {
      sql += ` WHERE ${whereClause}`
    }

    const stmt = this.db.prepare<{ count: number }, SQLQueryBindings[]>(sql)
    const row = stmt.get(...params)
    return Promise.resolve(row?.count ?? 0)
  }

  /**
   * Inserts a single document into the collection.
   *
   * @param doc - Document to insert (id is optional, timestamps auto-generated)
   * @returns Promise resolving to the inserted document with generated id
   *
   * @throws UniqueConstraintError if document violates a unique constraint
   * @throws ValidationError if document fails schema validation
   *
   * @example
   * ```typescript
   * // Insert with auto-generated ID
   * const result = await users.insertOne({
   *   name: 'Alice',
   *   email: 'alice@example.com',
   *   age: 30
   * });
   * console.log(result.document.id); // Auto-generated ID
   *
   * // Insert with custom ID
   * const result2 = await users.insertOne({
   *   id: 'custom-id',
   *   name: 'Bob',
   *   email: 'bob@example.com',
   *   age: 25
   * });
   * ```
   */
  insertOne(
    doc: Omit<T, "id" | "createdAt" | "updatedAt"> & { id?: string }
  ): Promise<InsertOneResult<T>> {
    const now = Date.now()
    const id = doc.id ?? this.idGenerator()

    // Build the full document
    const fullDoc = {
      ...doc,
      id,
      createdAt: now,
      updatedAt: now,
    } as unknown as T

    // Validate using schema validator if provided
    if (this.schema.validate && !this.schema.validate(fullDoc)) {
      throw new Error("Document validation failed")
    }

    // Serialize body (excludes id which is stored separately)
    const { id: _id, ...bodyData } = fullDoc
    const body = stringify(bodyData)

    try {
      const stmt = this.db.prepare(
        `INSERT INTO ${this.name} (id, body, createdAt, updatedAt) VALUES (?, jsonb(?), ?, ?)`
      )
      stmt.run(id, body, now, now)

      return Promise.resolve({
        document: fullDoc,
        acknowledged: true,
      })
    } catch (error) {
      // Handle unique constraint violations
      if (
        error instanceof Error &&
        error.message.includes("UNIQUE constraint failed")
      ) {
        // Extract field name from error message
        const match = error.message.match(UNIQUE_CONSTRAINT_REGEX)
        const field = match?.[1]?.replace(UNDERSCORE_PREFIX_REGEX, "") // Remove underscore prefix from generated columns
        throw new UniqueConstraintError(
          `Duplicate value for unique field '${field}'`,
          field,
          undefined
        )
      }
      throw error
    }
  }

  /**
   * Updates a single document by ID with partial update support.
   *
   * @param id - Document ID to update
   * @param update - Partial document with fields to update
   * @param options - Update options (upsert: create if not exists)
   * @returns Promise resolving to updated document or null if not found
   *
   * @example
   * ```typescript
   * // Partial update
   * const updated = await users.updateOne('user-123', { age: 31 });
   *
   * // Upsert - creates if not found
   * const result = await users.updateOne(
   *   'new-user',
   *   { name: 'New User', email: 'new@example.com' },
   *   { upsert: true }
   * );
   * ```
   */
  updateOne(
    id: string,
    update: Omit<Partial<T>, "id" | "createdAt" | "updatedAt">,
    options?: { upsert?: boolean }
  ): Promise<T | null> {
    const existing = this.db
      .prepare(`SELECT id, json(body) as body FROM ${this.name} WHERE id = ?`)
      .get(id) as { id: string; body: string } | null

    const now = Date.now()

    if (!existing) {
      if (options?.upsert) {
        // Create new document
        const newDoc = {
          ...update,
          id,
          createdAt: now,
          updatedAt: now,
        } as unknown as T

        if (this.schema.validate && !this.schema.validate(newDoc)) {
          throw new Error("Document validation failed")
        }

        const { id: _newId, ...newBodyData } = newDoc
        const newBody = stringify(newBodyData)
        this.db
          .prepare(
            `INSERT INTO ${this.name} (id, body, createdAt, updatedAt) VALUES (?, jsonb(?), ?, ?)`
          )
          .run(id, newBody, now, now)

        return Promise.resolve(newDoc)
      }
      return Promise.resolve(null)
    }

    // Merge existing with update
    const existingDoc = JSON.parse(existing.body) as Omit<T, "id">
    const mergedDoc = {
      ...existingDoc,
      ...update,
      id: existing.id,
      updatedAt: now,
    } as unknown as T

    if (this.schema.validate && !this.schema.validate(mergedDoc)) {
      throw new Error("Document validation failed")
    }

    const { id: _mergedId, ...mergedBodyData } = mergedDoc
    const mergedBody = stringify(mergedBodyData)
    this.db
      .prepare(
        `UPDATE ${this.name} SET body = jsonb(?), updatedAt = ? WHERE id = ?`
      )
      .run(mergedBody, now, id)

    return Promise.resolve(mergedDoc)
  }

  /**
   * Replaces an entire document by ID (no merge, complete replacement).
   *
   * @param id - Document ID to replace
   * @param doc - Complete new document (id excluded via type)
   * @returns Promise resolving to replaced document or null if not found
   *
   * @remarks
   * Unlike updateOne which merges fields, replaceOne completely replaces
   * the document body while preserving the original ID and createdAt timestamp.
   *
   * @example
   * ```typescript
   * // Complete document replacement
   * const replaced = await users.replaceOne('user-123', {
   *   name: 'Alice Smith',
   *   email: 'alice.new@example.com',
   *   age: 31
   * });
   *
   * // Difference from updateOne:
   * // - updateOne({ age: 31 }) -> merges age into existing doc
   * // - replaceOne({ age: 31 }) -> doc now ONLY has age field
   * ```
   */
  replaceOne(
    id: string,
    doc: Omit<T, "id" | "createdAt" | "updatedAt">
  ): Promise<T | null> {
    const existing = this.db
      .prepare(`SELECT id, json(body) as body FROM ${this.name} WHERE id = ?`)
      .get(id) as { id: string; body: string } | null

    if (!existing) {
      return Promise.resolve(null)
    }

    const existingDoc = JSON.parse(existing.body) as T
    const now = Date.now()

    // Build full document with preserved id and createdAt
    const fullDoc = {
      ...doc,
      id: existing.id,
      createdAt: existingDoc.createdAt,
      updatedAt: now,
    } as unknown as T

    if (this.schema.validate && !this.schema.validate(fullDoc)) {
      throw new Error("Document validation failed")
    }

    const { id: _docId, ...bodyData } = fullDoc
    const body = stringify(bodyData)
    this.db
      .prepare(
        `UPDATE ${this.name} SET body = jsonb(?), updatedAt = ? WHERE id = ?`
      )
      .run(body, now, id)

    return Promise.resolve(fullDoc)
  }

  /**
   * Deletes a single document by ID.
   *
   * @param id - Document ID to delete
   * @returns Promise resolving to true if deleted, false if not found
   *
   * @example
   * ```typescript
   * const deleted = await users.deleteOne('user-123');
   * if (deleted) {
   *   console.log('User deleted');
   * } else {
   *   console.log('User not found');
   * }
   * ```
   */
  deleteOne(id: string): Promise<boolean> {
    const stmt = this.db.prepare(`DELETE FROM ${this.name} WHERE id = ?`)
    const result = stmt.run(id)
    return Promise.resolve(result.changes > 0)
  }

  /**
   * Handles insertion errors and pushes to errors array.
   * @internal
   */
  private handleInsertError(
    error: unknown,
    index: number,
    doc: Omit<T, "id" | "createdAt" | "updatedAt"> & { id?: string },
    errors: { index: number; error: Error; document: typeof doc }[]
  ): Error {
    const err = error instanceof Error ? error : new Error(String(error))

    if (err.message.includes("UNIQUE constraint failed")) {
      const match = err.message.match(UNIQUE_CONSTRAINT_REGEX)
      const field = match?.[1]?.replace(UNDERSCORE_PREFIX_REGEX, "")
      const uniqueErr = new UniqueConstraintError(
        `Duplicate value for unique field '${field}'`,
        field,
        undefined
      )
      errors.push({ index, error: uniqueErr, document: doc })
      return uniqueErr
    }

    errors.push({ index, error: err, document: doc })
    return err
  }

  /**
   * Inserts multiple documents into the collection.
   *
   * @param docs - Array of documents to insert
   * @param options - Insertion options (ordered: stop on first error if true)
   * @returns Promise resolving to bulk write result with success/failure details
   *
   * @example
   * ```typescript
   * // Ordered insertion (default) - stops on first error
   * const result = await users.insertMany([
   *   { name: 'Alice', email: 'alice@example.com' },
   *   { name: 'Bob', email: 'bob@example.com' }
   * ]);
   *
   * // Unordered - continues despite errors
   * const result2 = await users.insertMany(
   *   [{ name: 'A' }, { name: 'B' }, { name: 'C' }],
   *   { ordered: false }
   * );
   * console.log(`Inserted: ${result2.insertedCount}, Errors: ${result2.errors.length}`);
   * ```
   */
  insertMany(
    docs: readonly (Omit<T, "id" | "createdAt" | "updatedAt"> & {
      id?: string
    })[],
    options?: { ordered?: boolean }
  ): Promise<InsertManyResult<T>> {
    const ordered = options?.ordered ?? true
    const now = Date.now()
    const insertedDocs: T[] = []
    const insertedIds: string[] = []
    const errors: {
      index: number
      error: Error
      document: Omit<T, "id" | "createdAt" | "updatedAt"> & { id?: string }
    }[] = []

    const stmt = this.db.prepare(
      `INSERT INTO ${this.name} (id, body, createdAt, updatedAt) VALUES (?, jsonb(?), ?, ?)`
    )

    for (let i = 0; i < docs.length; i++) {
      const doc = docs[i]
      if (!doc) {
        continue
      }

      try {
        const id = doc.id ?? this.idGenerator()
        const fullDoc = {
          ...doc,
          id,
          createdAt: now,
          updatedAt: now,
        } as unknown as T

        // Validate if validator is provided
        if (this.schema.validate && !this.schema.validate(fullDoc)) {
          throw new Error("Document validation failed")
        }

        const { id: _id, ...bodyData } = fullDoc
        const body = stringify(bodyData)
        stmt.run(id, body, now, now)

        insertedDocs.push(fullDoc)
        insertedIds.push(id)
      } catch (error) {
        const err = this.handleInsertError(error, i, doc, errors)
        if (ordered && err) {
          break
        }
      }
    }

    return Promise.resolve({
      documents: insertedDocs,
      insertedCount: insertedDocs.length,
      acknowledged: true,
    })
  }

  /**
   * Updates all documents matching the filter with a partial update.
   *
   * @param filter - Query filter to find documents to update
   * @param update - Partial document with fields to update
   * @returns Promise resolving to update result with modifiedCount
   *
   * @remarks
   * Uses a transaction to ensure all-or-nothing semantics. All matching documents
   * are retrieved, merged with the update, validated, and then persisted atomically.
   *
   * @example
   * ```typescript
   * // Set all inactive users to deleted status
   * const result = await users.updateMany(
   *   { status: 'inactive' },
   *   { status: 'deleted' }
   * );
   * console.log(`Updated ${result.modifiedCount} users`);
   *
   * // Give all admins a badge
   * await users.updateMany(
   *   { role: 'admin' },
   *   { badge: 'admin' }
   * );
   * ```
   */
  updateMany(
    filter: QueryFilter<T>,
    update: Omit<Partial<T>, "id" | "createdAt" | "updatedAt">
  ): Promise<UpdateResult> {
    const { sql: whereClause, params } = this.translator.translate(filter)

    // Find all matching documents
    let selectSql = `SELECT id, json(body) as body FROM ${this.name}`
    if (whereClause) {
      selectSql += ` WHERE ${whereClause}`
    }

    const stmt = this.db.prepare<
      { id: string; body: string },
      SQLQueryBindings[]
    >(selectSql)
    const rows = stmt.all(...params)
    const matchedCount = rows.length

    if (matchedCount === 0) {
      return Promise.resolve({
        matchedCount: 0,
        modifiedCount: 0,
        acknowledged: true,
      })
    }

    const now = Date.now()
    const updatedDocs: T[] = []

    // Prepare all updated documents and validate before committing
    for (const row of rows) {
      const existingDoc = JSON.parse(row.body) as Omit<T, "id">
      const mergedDoc = {
        ...existingDoc,
        ...update,
        id: row.id,
        updatedAt: now,
      } as unknown as T

      if (this.schema.validate && !this.schema.validate(mergedDoc)) {
        throw new Error(`Document validation failed for id: ${row.id}`)
      }

      updatedDocs.push(mergedDoc)
    }

    // Use transaction for atomic update
    const updateStmt = this.db.prepare(
      `UPDATE ${this.name} SET body = jsonb(?), updatedAt = ? WHERE id = ?`
    )

    this.db.exec("BEGIN TRANSACTION")
    try {
      for (const doc of updatedDocs) {
        const { id, ...bodyData } = doc
        const body = stringify(bodyData)
        updateStmt.run(body, now, id)
      }
      this.db.exec("COMMIT")
    } catch (error) {
      this.db.exec("ROLLBACK")
      throw error
    }

    return Promise.resolve({
      matchedCount,
      modifiedCount: updatedDocs.length,
      acknowledged: true,
    })
  }

  /**
   * Deletes all documents matching the filter.
   *
   * @param filter - Query filter to match documents to delete
   * @returns Promise resolving to delete result with deletedCount
   *
   * @remarks
   * Uses a transaction to ensure atomic deletion of all matching documents.
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
  deleteMany(filter: QueryFilter<T>): Promise<DeleteResult> {
    const { sql: whereClause, params } = this.translator.translate(filter)

    let sql = `DELETE FROM ${this.name}`
    if (whereClause) {
      sql += ` WHERE ${whereClause}`
    }

    this.db.exec("BEGIN TRANSACTION")
    try {
      const stmt = this.db.prepare(sql)
      const result = stmt.run(...params)
      this.db.exec("COMMIT")

      return Promise.resolve({
        deletedCount: result.changes,
        acknowledged: true,
      })
    } catch (error) {
      this.db.exec("ROLLBACK")
      throw error
    }
  }

  /**
   * Validates a document against the schema asynchronously.
   *
   * @param doc - Document to validate
   * @returns Promise resolving to the validated document typed as T
   * @throws ValidationError if document fails validation
   *
   * @example
   * ```typescript
   * try {
   *   const user = await users.validate(unknownData);
   *   console.log('Valid user:', user.name);
   * } catch (err) {
   *   if (err instanceof ValidationError) {
   *     console.error(`Field ${err.field} failed:`, err.message);
   *   }
   * }
   * ```
   */
  validate(doc: unknown): Promise<T> {
    if (!this.schema.validate) {
      // No validator configured, trust the document
      return Promise.resolve(doc as T)
    }

    if (this.schema.validate(doc)) {
      return Promise.resolve(doc as T)
    }

    throw new ValidationError("Document validation failed", undefined, doc)
  }

  /**
   * Validates a document against the schema synchronously.
   *
   * @param doc - Document to validate
   * @returns The validated document typed as T
   * @throws ValidationError if document fails validation
   *
   * @example
   * ```typescript
   * try {
   *   const user = users.validateSync(unknownData);
   *   console.log('Valid user:', user.name);
   * } catch (err) {
   *   if (err instanceof ValidationError) {
   *     console.error(`Field ${err.field} failed:`, err.message);
   *   }
   * }
   * ```
   */
  validateSync(doc: unknown): T {
    if (!this.schema.validate) {
      // No validator configured, trust the document
      return doc as T
    }

    if (this.schema.validate(doc)) {
      return doc as T
    }

    throw new ValidationError("Document validation failed", undefined, doc)
  }
}
