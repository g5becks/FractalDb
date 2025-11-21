import type { Database as SQLiteDatabase } from "bun:sqlite"
import type { Collection } from "./collection-types.js"
import type { Document } from "./core-types.js"
import type { JsonPath } from "./path-types.js"
import type {
  CompoundIndex,
  SchemaDefinition,
  SchemaField,
  TypeScriptToSQLite,
} from "./schema-types.js"
import { SQLiteCollection } from "./sqlite-collection.js"

/**
 * Fluent builder for creating collections with inline schema definition.
 *
 * @typeParam T - The document type extending Document
 *
 * @remarks
 * CollectionBuilder provides a fluent API for defining collection schemas inline
 * while creating the collection. This is an alternative to using createSchema()
 * separately.
 *
 * @example
 * ```typescript
 * // Fluent collection creation
 * const users = db.collection<User>('users')
 *   .field('name', { type: 'TEXT', indexed: true })
 *   .field('email', { type: 'TEXT', indexed: true, unique: true })
 *   .timestamps(true)
 *   .build();
 * ```
 */
export type CollectionBuilder<T extends Document> = {
  /**
   * Define an indexed field with type checking.
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
  ): CollectionBuilder<T>

  /**
   * Define a compound index spanning multiple fields.
   */
  compoundIndex(
    name: string,
    fields: readonly (keyof T)[],
    options?: { readonly unique?: boolean }
  ): CollectionBuilder<T>

  /**
   * Enable automatic timestamp management.
   */
  timestamps(enabled?: boolean): CollectionBuilder<T>

  /**
   * Add validation function using a type predicate.
   */
  validate(validator: (doc: unknown) => doc is T): CollectionBuilder<T>

  /**
   * Build and return the collection with the defined schema.
   *
   * @returns The collection instance ready for operations
   */
  build(): Collection<T>
}

/**
 * Implementation of CollectionBuilder.
 * @internal
 */
export class CollectionBuilderImpl<T extends Document>
  implements CollectionBuilder<T>
{
  private readonly db: SQLiteDatabase
  private readonly collectionName: string
  private readonly idGenerator: () => string
  private readonly fieldDefs: SchemaField<T, keyof T>[] = []
  private readonly compoundIndexDefs: CompoundIndex<T>[] = []
  // biome-ignore lint/correctness/noUnusedPrivateClassMembers: used in timestamps() and build()
  private enableTimestamps = false
  private validatorFn: ((doc: unknown) => doc is T) | undefined

  constructor(
    db: SQLiteDatabase,
    collectionName: string,
    idGenerator: () => string
  ) {
    this.db = db
    this.collectionName = collectionName
    this.idGenerator = idGenerator
  }

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
  ): CollectionBuilder<T> {
    // Build field object, only adding defined properties
    const field = {
      name,
      type: options.type as TypeScriptToSQLite<T[keyof T]>,
    } as SchemaField<T, keyof T>

    if (options.path !== undefined) {
      ;(field as { path?: JsonPath }).path = options.path
    }
    if (options.nullable !== undefined) {
      ;(field as { nullable?: boolean }).nullable = options.nullable
    }
    if (options.indexed !== undefined) {
      ;(field as { indexed?: boolean }).indexed = options.indexed
    }
    if (options.unique !== undefined) {
      ;(field as { unique?: boolean }).unique = options.unique
    }
    if (options.default !== undefined) {
      ;(field as { default?: T[keyof T] }).default =
        options.default as T[keyof T]
    }

    this.fieldDefs.push(field)
    return this
  }

  compoundIndex(
    name: string,
    fields: readonly (keyof T)[],
    options?: { readonly unique?: boolean }
  ): CollectionBuilder<T> {
    const index = { name, fields } as CompoundIndex<T>
    if (options?.unique !== undefined) {
      ;(index as { unique?: boolean }).unique = options.unique
    }
    this.compoundIndexDefs.push(index)
    return this
  }

  timestamps(enabled = true): CollectionBuilder<T> {
    this.enableTimestamps = enabled
    return this
  }

  validate(validator: (doc: unknown) => doc is T): CollectionBuilder<T> {
    this.validatorFn = validator
    return this
  }

  build(): Collection<T> {
    const schema = {
      fields: this.fieldDefs,
      timestamps: this.enableTimestamps,
    } as SchemaDefinition<T>

    if (this.compoundIndexDefs.length > 0) {
      ;(
        schema as { compoundIndexes?: readonly CompoundIndex<T>[] }
      ).compoundIndexes = this.compoundIndexDefs
    }
    if (this.validatorFn !== undefined) {
      ;(schema as { validate?: (doc: unknown) => doc is T }).validate =
        this.validatorFn
    }

    return new SQLiteCollection<T>(
      this.db,
      this.collectionName,
      schema,
      this.idGenerator
    )
  }
}
