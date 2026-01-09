/**
 * Collection event types and payloads for StrataDB.
 *
 * @remarks
 * This module provides type-safe event definitions for collection operations.
 * Events are emitted after successful operations and can be used for:
 * - Audit logging
 * - Cache invalidation
 * - Real-time notifications
 * - Data synchronization
 *
 * @packageDocumentation
 */

import type { Document } from "./core-types.js"
import type { QueryFilter } from "./query-types.js"

/**
 * Event payload for single document insert operations.
 *
 * @typeParam T - The document type extending Document
 *
 * @remarks
 * Emitted after `insertOne()` successfully creates a document.
 * The payload includes the complete document with its generated `_id`.
 *
 * @example
 * ```typescript
 * collection.on('insert', (event) => {
 *   console.log(`Document inserted: ${event.document._id}`)
 * })
 * ```
 */
export type InsertEvent<T extends Document> = {
  /** The inserted document with generated _id */
  readonly document: T
}

/**
 * Event payload for batch insert operations.
 *
 * @typeParam T - The document type extending Document
 *
 * @remarks
 * Emitted after `insertMany()` completes. This includes all successfully
 * inserted documents, even if some documents failed (in unordered mode).
 *
 * @example
 * ```typescript
 * collection.on('insertMany', (event) => {
 *   console.log(`${event.insertedCount} documents inserted`)
 * })
 * ```
 */
export type InsertManyEvent<T extends Document> = {
  /** Array of inserted documents */
  readonly documents: readonly T[]
  /** Number of documents successfully inserted */
  readonly insertedCount: number
}

/**
 * Event payload for single document update operations.
 *
 * @typeParam T - The document type extending Document
 *
 * @remarks
 * Emitted after `updateOne()` successfully updates or upserts a document.
 * The `document` field contains the updated document, or `null` if no match
 * was found and upsert was not enabled. The `upserted` flag indicates whether
 * a new document was created via upsert.
 *
 * @example
 * ```typescript
 * collection.on('update', (event) => {
 *   if (event.upserted) {
 *     console.log('Document was upserted')
 *   } else if (event.document) {
 *     console.log('Document was updated')
 *   }
 * })
 * ```
 */
export type UpdateEvent<T extends Document> = {
  /** The filter used to find the document (string ID or QueryFilter) */
  readonly filter: string | QueryFilter<T>
  /** The update that was applied */
  readonly update: Partial<T>
  /** The document after update, or null if not found (without upsert) */
  readonly document: T | null
  /** Whether this was an upsert (document was created) */
  readonly upserted: boolean
}

/**
 * Event payload for batch update operations.
 *
 * @typeParam T - The document type extending Document
 *
 * @remarks
 * Emitted after `updateMany()` updates multiple documents.
 * The `matchedCount` indicates how many documents matched the filter,
 * while `modifiedCount` indicates how many were actually changed.
 * These counts may differ if the update didn't change field values.
 *
 * @example
 * ```typescript
 * collection.on('updateMany', (event) => {
 *   console.log(`Updated ${event.modifiedCount} of ${event.matchedCount} matched documents`)
 * })
 * ```
 */
export type UpdateManyEvent<T extends Document> = {
  /** The filter used to find documents */
  readonly filter: QueryFilter<T>
  /** The update that was applied */
  readonly update: Partial<T>
  /** Number of documents that matched the filter */
  readonly matchedCount: number
  /** Number of documents that were modified */
  readonly modifiedCount: number
}

/**
 * Event payload for replace operations.
 *
 * @typeParam T - The document type extending Document
 *
 * @remarks
 * Emitted after `replaceOne()` successfully replaces a document.
 * Unlike updates, replace operations replace the entire document
 * (except for `_id`, `createdAt`, and `updatedAt`).
 *
 * @example
 * ```typescript
 * collection.on('replace', (event) => {
 *   if (event.document) {
 *     console.log(`Document ${event.document._id} was replaced`)
 *   }
 * })
 * ```
 */
export type ReplaceEvent<T extends Document> = {
  /** The filter used to find the document */
  readonly filter: string | QueryFilter<T>
  /** The document after replacement, or null if not found */
  readonly document: T | null
}

/**
 * Event payload for single document delete operations.
 *
 * @typeParam T - The document type extending Document
 *
 * @remarks
 * Emitted after `deleteOne()` attempts to delete a document.
 * The `deleted` flag indicates whether a document was actually deleted.
 *
 * @example
 * ```typescript
 * collection.on('delete', (event) => {
 *   if (event.deleted) {
 *     console.log('Document was deleted')
 *   }
 * })
 * ```
 */
export type DeleteEvent<T extends Document> = {
  /** The filter used to find the document */
  readonly filter: string | QueryFilter<T>
  /** Whether a document was deleted */
  readonly deleted: boolean
}

/**
 * Event payload for batch delete operations.
 *
 * @typeParam T - The document type extending Document
 *
 * @remarks
 * Emitted after `deleteMany()` deletes multiple documents.
 * The `deletedCount` indicates how many documents were removed.
 *
 * @example
 * ```typescript
 * collection.on('deleteMany', (event) => {
 *   console.log(`${event.deletedCount} documents deleted`)
 * })
 * ```
 */
export type DeleteManyEvent<T extends Document> = {
  /** The filter used to find documents */
  readonly filter: QueryFilter<T>
  /** Number of documents deleted */
  readonly deletedCount: number
}

/**
 * Event payload for findOneAndDelete operations.
 *
 * @typeParam T - The document type extending Document
 *
 * @remarks
 * Emitted after `findOneAndDelete()` finds and deletes a document.
 * The `document` field contains the deleted document, or `null` if
 * no matching document was found.
 *
 * @example
 * ```typescript
 * collection.on('findOneAndDelete', (event) => {
 *   if (event.document) {
 *     console.log(`Deleted document: ${event.document._id}`)
 *   }
 * })
 * ```
 */
export type FindOneAndDeleteEvent<T extends Document> = {
  /** The filter used to find the document */
  readonly filter: string | QueryFilter<T>
  /** The deleted document, or null if not found */
  readonly document: T | null
}

/**
 * Event payload for findOneAndUpdate operations.
 *
 * @typeParam T - The document type extending Document
 *
 * @remarks
 * Emitted after `findOneAndUpdate()` finds and updates a document.
 * The `document` field contains either the document before or after
 * the update, depending on the `returnDocument` option. The `upserted`
 * flag indicates whether a new document was created via upsert.
 *
 * @example
 * ```typescript
 * collection.on('findOneAndUpdate', (event) => {
 *   if (event.upserted) {
 *     console.log('Document was upserted')
 *   } else if (event.document) {
 *     console.log(`Document ${event.document._id} was updated`)
 *   }
 * })
 * ```
 */
export type FindOneAndUpdateEvent<T extends Document> = {
  /** The filter used to find the document */
  readonly filter: string | QueryFilter<T>
  /** The update that was applied */
  readonly update: Partial<T>
  /** The document (before or after based on options), or null if not found */
  readonly document: T | null
  /** Whether this was an upsert (document was created) */
  readonly upserted: boolean
}

/**
 * Event payload for findOneAndReplace operations.
 *
 * @typeParam T - The document type extending Document
 *
 * @remarks
 * Emitted after `findOneAndReplace()` finds and replaces a document.
 * The `document` field contains either the document before or after
 * the replacement, depending on the `returnDocument` option. The `upserted`
 * flag indicates whether a new document was created via upsert.
 *
 * @example
 * ```typescript
 * collection.on('findOneAndReplace', (event) => {
 *   if (event.upserted) {
 *     console.log('Document was upserted')
 *   } else if (event.document) {
 *     console.log(`Document ${event.document._id} was replaced`)
 *   }
 * })
 * ```
 */
export type FindOneAndReplaceEvent<T extends Document> = {
  /** The filter used to find the document */
  readonly filter: string | QueryFilter<T>
  /** The document (before or after based on options), or null if not found */
  readonly document: T | null
  /** Whether this was an upsert (document was created) */
  readonly upserted: boolean
}

/**
 * Event payload for drop operations.
 *
 * @remarks
 * Emitted after `drop()` successfully drops a collection.
 * This is a collection-level event that includes the collection name.
 *
 * @example
 * ```typescript
 * collection.on('drop', (event) => {
 *   console.log(`Collection ${event.name} was dropped`)
 * })
 * ```
 */
export type DropEvent = {
  /** The collection name that was dropped */
  readonly name: string
}

/**
 * Event payload for error events.
 *
 * @remarks
 * Emitted when an operation encounters an error. This allows for
 * centralized error handling and logging. The `context` field may
 * contain additional information about the operation that failed.
 *
 * @example
 * ```typescript
 * collection.on('error', (event) => {
 *   console.error(`Operation ${event.operation} failed:`, event.error)
 *   if (event.context) {
 *     console.error('Context:', event.context)
 *   }
 * })
 * ```
 */
export type ErrorEvent = {
  /** The operation that failed */
  readonly operation: string
  /** The error that occurred */
  readonly error: Error
  /** Additional context about the operation */
  readonly context?: unknown
}
