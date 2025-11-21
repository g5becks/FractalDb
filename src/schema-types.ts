import type { JsonPath } from "./path-types.js"

/**
 * SQLite column data types for generated columns.
 *
 * @remarks
 * These types are used when creating generated columns from JSON paths.
 * SQLite will store the extracted value using the specified type for indexing.
 *
 * @example
 * ```typescript
 * // Correct type mappings
 * const textField: SQLiteType = 'TEXT';      // For strings
 * const numberField: SQLiteType = 'INTEGER';  // For whole numbers
 * const realField: SQLiteType = 'REAL';       // For floating point numbers
 * const boolField: SQLiteType = 'BOOLEAN';    // For true/false values
 * const dataField: SQLiteType = 'BLOB';       // For binary data
 * ```
 */
export type SQLiteType =
  | "TEXT" // String values
  | "INTEGER" // Whole numbers
  | "REAL" // Floating point numbers
  | "BOOLEAN" // True/false values (stored as 0/1)
  | "NUMERIC" // Numeric values with flexible precision
  | "BLOB" // Binary data

/**
 * Maps TypeScript types to their corresponding SQLite types.
 *
 * @remarks
 * This ensures compile-time type safety between TypeScript document fields
 * and SQLite column types. Prevents runtime errors from type mismatches.
 *
 * @example
 * ```typescript
 * type User = Document<{
 *   name: string;        // TypeScript: string
 *   age: number;         // TypeScript: number
 *   active: boolean;     // TypeScript: boolean
 *   data: Uint8Array;    // TypeScript: binary data
 * }>;
 *
 * // ✅ Correct - types align
 * type NameType = TypeScriptToSQLite<User['name']>;     // 'TEXT' | 'BLOB'
 * type AgeType = TypeScriptToSQLite<User['age']>;      // 'INTEGER' | 'REAL' | 'NUMERIC'
 * type ActiveType = TypeScriptToSQLite<User['active']>; // 'BOOLEAN' | 'INTEGER'
 *
 * // ❌ Compiler error - type mismatch
 * type Invalid = TypeScriptToSQLite<User['name']>;     // Won't accept 'INTEGER'
 * ```
 */
export type TypeScriptToSQLite<T> = T extends string
  ? "TEXT" | "BLOB"
  : T extends number
    ? "INTEGER" | "REAL" | "NUMERIC"
    : T extends boolean
      ? "BOOLEAN" | "INTEGER"
      : T extends Date
        ? "INTEGER" | "TEXT" | "REAL"
        : T extends Uint8Array | ArrayBuffer
          ? "BLOB"
          : T extends unknown[]
            ? "TEXT" | "BLOB"
            : T extends object
              ? "TEXT" | "BLOB"
              : SQLiteType // Fallback for complex types

/**
 * Schema field definition for document properties.
 *
 * @typeParam T - The document type
 * @typeParam K - The field key within the document type
 *
 * @remarks
 * Defines how a document field is stored and indexed in SQLite.
 * The `path` is optional - if omitted, it defaults to `$.{fieldName}` for top-level fields.
 * Only specify `path` explicitly when:
 * 1. Accessing nested properties (e.g., `$.profile.bio`)
 * 2. Using a shorter field name for a nested property (e.g., field name `theme` with path `$.profile.settings.theme`)
 *
 * @example
 * ```typescript
 * // Top-level field - path is optional (defaults to $.name)
 * .field('name', {
 *   type: 'TEXT',
 *   indexed: true
 * })
 *
 * // Nested property with explicit path
 * .field('bio', {
 *   path: '$.profile.bio',
 *   type: 'TEXT',
 *   indexed: true
 * })
 *
 * // Unique constraint with default value
 * .field('email', {
 *   type: 'TEXT',
 *   indexed: true,
 *   unique: true,
 *   nullable: false
 * })
 *
 * // ❌ Compiler error - type mismatch
 * .field('name', {
 *   type: 'INTEGER',  // Error: string field cannot use INTEGER
 * })
 * ```
 */
export type SchemaField<T, K extends keyof T> = {
  /** The field name as it appears in the TypeScript type */
  readonly name: K

  /**
   * JSON path for extracting the value from the document.
   * Defaults to `$.{name}` if omitted (top-level field).
   * Only specify for nested properties or custom mappings.
   */
  readonly path?: JsonPath

  /**
   * SQLite column type for the generated column.
   * Must be compatible with the TypeScript type of the field.
   * Compile-time type checking ensures type safety.
   */
  readonly type: TypeScriptToSQLite<T[K]>

  /** Whether the field can be null (default: true for optional fields) */
  readonly nullable?: boolean

  /** Whether to create an index on this field for faster queries */
  readonly indexed?: boolean

  /** Whether to enforce uniqueness constraint on this field */
  readonly unique?: boolean

  /** Default value when field is not provided on insert */
  readonly default?: T[K]
}

/**
 * Compound index definition for multi-field indexes.
 *
 * @typeParam T - The document type
 *
 * @remarks
 * Compound indexes improve query performance when filtering by multiple fields together.
 * The order of fields matters - queries must use fields from left to right for the index to be effective.
 *
 * @example
 * ```typescript
 * // Define compound index on age and status
 * .compoundIndex('age_status', ['age', 'status'])
 *
 * // Queries that can use this index:
 * { age: 30, status: 'active' }           // Uses index fully
 * { age: { $gte: 25 } }                   // Uses index partially (age only)
 *
 * // Queries that CANNOT use this index:
 * { status: 'active' }                    // Doesn't start with first field (age)
 *
 * // Unique compound constraint
 * .compoundIndex('email_tenant', ['email', 'tenantId'], { unique: true })
 * ```
 */
export type CompoundIndex<T> = {
  /** Unique name for the index (e.g., 'age_status', 'email_tenant') */
  readonly name: string

  /** Array of field names to include in the index (order matters) */
  readonly fields: ReadonlyArray<keyof T>

  /** Whether to enforce uniqueness across the combination of fields */
  readonly unique?: boolean
}

/**
 * Configuration for automatic timestamp management.
 *
 * @remarks
 * Controls whether documents automatically receive createdAt and updatedAt timestamps
 * when inserted or updated. This simplifies audit trail management and is commonly
 * used in document databases.

 * @example
 * ```typescript
 * // Enable both timestamps (default behavior)
 * const timestamps: TimestampConfig = true;
 *
 * // Explicit configuration
 * const timestamps: TimestampConfig = {
 *   createdAt: true,
 *   updatedAt: true
 * };
 *
 * // Only track creation time
 * const timestamps: TimestampConfig = {
 *   createdAt: true,
 *   updatedAt: false
 * };
 *
 * // Disable automatic timestamps
 * const timestamps: TimestampConfig = false;
 * ```
 */
export type TimestampConfig =
  | {
      /** Whether to automatically add createdAt timestamp on document creation */
      readonly createdAt?: boolean

      /** Whether to automatically update updatedAt timestamp on document modification */
      readonly updatedAt?: boolean
    }
  | boolean

/**
 * Complete schema definition for a document collection.
 *
 * @typeParam T - The document type this schema defines

 * @remarks
 * The SchemaDefinition type brings together all schema configuration elements
 * to create a complete, type-safe collection definition. It combines field definitions,
 * compound indexes, timestamp configuration, and optional validation logic.

 * This type serves as the foundation for collection creation and ensures that
 * all database operations maintain schema consistency and type safety throughout
 * the library.

 * @example
 * ```typescript
 * type User = Document<{
 *   name: string;
 *   email: string;
 *   age: number;
 *   profile: {
 *     bio: string;
 *     settings: {
 *       theme: 'light' | 'dark';
 *     };
 *   };
 *   createdAt: Date;
 *   updatedAt: Date;
 * }>;

 * const userSchema: SchemaDefinition<User> = {
 *   // Field definitions for indexing and constraints
 *   fields: [
 *     { name: 'name', type: 'TEXT', indexed: true },
 *     { name: 'email', type: 'TEXT', indexed: true, unique: true, nullable: false },
 *     { name: 'age', type: 'INTEGER', indexed: true },
 *     { name: 'profile.bio', type: 'TEXT', indexed: true },
 *     { name: 'profile.settings.theme', type: 'TEXT', indexed: true }
 *   ],
 *
 *   // Compound indexes for multi-field queries
 *   compoundIndexes: [
 *     { name: 'name_age', fields: ['name', 'age'] },
 *     { name: 'email_status', fields: ['email', 'status'], unique: true }
 *   ],
 *
 *   // Automatic timestamp management
 *   timestamps: {
 *     createdAt: true,
 *     updatedAt: true
 *   },
 *
 *   // Optional validation using Standard Schema
 *   validate: (doc: unknown): doc is User => {
 *     return typeof doc === 'object' && doc !== null &&
 *            'name' in doc && typeof doc.name === 'string' &&
 *            'email' in doc && typeof doc.email === 'string';
 *   }
 * };
 *
 * // Usage with collection
 * const users = db.collection('users', userSchema);
 * ```
 */
export type SchemaDefinition<T> = {
  /** Array of field definitions for indexing and constraints */
  readonly fields: readonly SchemaField<T, keyof T>[]

  /** Array of compound index definitions for multi-field queries */
  readonly compoundIndexes?: readonly CompoundIndex<T>[]

  /** Configuration for automatic timestamp management */
  readonly timestamps?: TimestampConfig

  /** Optional validation function using Standard Schema type predicate signature */
  readonly validate?: (doc: unknown) => doc is T
}
