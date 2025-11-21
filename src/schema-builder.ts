import type { Document } from "./core-types.js"
import type { JsonPath } from "./path-types.js"
import type { SchemaBuilder } from "./schema-builder-types.js"
import type {
  CompoundIndex,
  SchemaDefinition,
  SchemaField,
  TimestampConfig,
  TypeScriptToSQLite,
} from "./schema-types.js"

/**
 * Concrete implementation of the SchemaBuilder interface.
 *
 * @typeParam T - The document type extending Document
 *
 * @remarks
 * This class provides a fluent API for building schemas through method chaining.
 * It accumulates configuration in private fields and returns an immutable
 * SchemaDefinition when build() is called.
 *
 * The implementation ensures:
 * - Type safety at every step
 * - Proper path defaulting for top-level fields
 * - Immutable final schema definition
 * - Clean developer experience
 *
 * @internal
 * This class is not exported directly. Use createSchema<T>() instead.
 *
 * @example
 * ```typescript
 * // Don't instantiate directly - use createSchema<T>()
 * const schema = createSchema<User>()
 *   .field('name', { type: 'TEXT', indexed: true })
 *   .build();
 * ```
 */
class SchemaBuilderImpl<T extends Document> implements SchemaBuilder<T> {
  /** Accumulated field definitions */
  private readonly fields: SchemaField<T, keyof T>[] = []

  /** Accumulated compound indexes */
  private readonly compoundIndexes: CompoundIndex<T>[] = []

  /** Timestamp configuration */
  private timestampConfig?: TimestampConfig

  /** Validation function */
  private validator?: (doc: unknown) => doc is T

  /**
   * Define an indexed field with type checking.
   *
   * @param name - The field name from the document type
   * @param options - Field configuration options
   * @returns This builder instance for method chaining
   *
   * @remarks
   * If no path is provided, it defaults to `$.{name}` for top-level field access.
   * This is a convenience feature that reduces boilerplate for common cases.
   *
   * @example
   * ```typescript
   * builder
   *   .field('name', { type: 'TEXT', indexed: true })
   *   .field('email', { type: 'TEXT', indexed: true, unique: true })
   *   .field('bio', { path: '$.profile.bio', type: 'TEXT', indexed: true })
   * ```
   */
  field<K extends keyof T>(
    name: K,
    options: {
      readonly path?: JsonPath
      readonly type: TypeScriptToSQLite<T[K]>
      readonly nullable?: boolean
      readonly indexed?: boolean
      readonly unique?: boolean
      readonly default?: T[K]
    }
  ): SchemaBuilder<T> {
    // Default path to $.{fieldName} if not provided
    const path = options.path ?? (`$.${String(name)}` as JsonPath)

    // Build field object conditionally to avoid undefined values
    const field: SchemaField<T, K> = {
      name,
      path,
      type: options.type,
      ...(options.nullable !== undefined && { nullable: options.nullable }),
      ...(options.indexed !== undefined && { indexed: options.indexed }),
      ...(options.unique !== undefined && { unique: options.unique }),
      ...(options.default !== undefined && { default: options.default }),
    }

    this.fields.push(field)

    return this
  }

  /**
   * Define a compound index spanning multiple fields.
   *
   * @param name - Unique name for the index
   * @param fields - Array of field names (order matters)
   * @param options - Index options (unique constraint)
   * @returns This builder instance for method chaining
   *
   * @example
   * ```typescript
   * builder
   *   .compoundIndex('age_status', ['age', 'status'])
   *   .compoundIndex('email_tenant', ['email', 'tenantId'], { unique: true })
   * ```
   */
  compoundIndex(
    name: string,
    fields: readonly (keyof T)[],
    options?: { readonly unique?: boolean }
  ): SchemaBuilder<T> {
    const index: CompoundIndex<T> = {
      name,
      fields,
      ...(options?.unique !== undefined && { unique: options.unique }),
    }

    this.compoundIndexes.push(index)

    return this
  }

  /**
   * Enable automatic timestamp management.
   *
   * @param enabled - Whether to auto-manage createdAt/updatedAt (default: true)
   * @returns This builder instance for method chaining
   *
   * @example
   * ```typescript
   * builder.timestamps(true)
   * ```
   */
  timestamps(enabled = true): SchemaBuilder<T> {
    this.timestampConfig = enabled
    return this
  }

  /**
   * Add validation function using a type predicate.
   *
   * @param validator - Function that validates and narrows the document type
   * @returns This builder instance for method chaining
   *
   * @example
   * ```typescript
   * builder.validate((doc): doc is User => {
   *   return typeof doc === 'object' &&
   *          doc !== null &&
   *          'name' in doc &&
   *          typeof doc.name === 'string';
   * })
   * ```
   */
  validate(validator: (doc: unknown) => doc is T): SchemaBuilder<T> {
    this.validator = validator
    return this
  }

  /**
   * Build and return the complete, immutable schema definition.
   *
   * @returns The immutable schema definition
   *
   * @remarks
   * Freezes the schema definition to prevent accidental modification.
   * The returned object is completely immutable at all levels.
   *
   * @example
   * ```typescript
   * const schema = builder.build();
   * // schema is now immutable
   * ```
   */
  build(): SchemaDefinition<T> {
    const definition: SchemaDefinition<T> = {
      fields: Object.freeze([...this.fields]),
      ...(this.compoundIndexes.length > 0 && {
        compoundIndexes: Object.freeze([...this.compoundIndexes]),
      }),
      ...(this.timestampConfig !== undefined && {
        timestamps: this.timestampConfig,
      }),
      ...(this.validator !== undefined && {
        validate: this.validator,
      }),
    }

    return Object.freeze(definition)
  }
}

/**
 * Creates a new schema builder for defining collection schemas.
 *
 * @typeParam T - The document type extending Document
 * @returns A new SchemaBuilder instance
 *
 * @remarks
 * This factory function creates a new `SchemaBuilder<T>` that provides
 * a fluent API for constructing schemas. The builder ensures type safety
 * at every step and returns an immutable schema definition when built.
 *
 * @example
 * ```typescript
 * import { createSchema, type Document } from 'stratadb';
 *
 * type User = Document<{
 *   name: string;
 *   email: string;
 *   age: number;
 *   status: 'active' | 'inactive';
 * }>;
 *
 * // ✅ Create and build schema
 * const userSchema = createSchema<User>()
 *   .field('name', { type: 'TEXT', indexed: true })
 *   .field('email', { type: 'TEXT', indexed: true, unique: true })
 *   .field('age', { type: 'INTEGER', indexed: true })
 *   .field('status', { type: 'TEXT', indexed: true })
 *   .compoundIndex('age_status', ['age', 'status'])
 *   .timestamps(true)
 *   .validate((doc): doc is User => {
 *     return typeof doc === 'object' &&
 *            doc !== null &&
 *            'name' in doc &&
 *            typeof doc.name === 'string' &&
 *            'email' in doc &&
 *            typeof doc.email === 'string';
 *   })
 *   .build();
 *
 * // ✅ Use with database
 * const users = db.collection<User>('users', userSchema);
 * ```
 */
export function createSchema<T extends Document>(): SchemaBuilder<T> {
  return new SchemaBuilderImpl<T>()
}
