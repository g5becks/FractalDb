# StrataDB Final Sprint - Complete Implementation Plan

**Version:** 2.0 - Fully Uniform API
**Status:** Ready for implementation
**Estimated Total Changes:** ~950 locations across 70+ files
**Estimated Implementation Time:** 3-4 days for experienced developer

---

## Table of Contents

1. [Overview](#overview)
2. [API Design Principles](#api-design-principles)
3. [Part 1: Remove Wasteful Wrappers](#part-1-remove-wasteful-wrappers)
4. [Part 2: Uniform Filter Support](#part-2-uniform-filter-support)
5. [Part 3: New MongoDB-Compatible Methods](#part-3-new-mongodb-compatible-methods)
6. [Part 4: Utility Methods](#part-4-utility-methods)
7. [Part 5: Documentation](#part-5-documentation)
8. [Part 6: Benchmark Migrations](#part-6-benchmark-migrations)
9. [Implementation Order](#implementation-order)
10. [Testing Strategy](#testing-strategy)
11. [Success Criteria](#success-criteria)

---

## Overview

### Goals

1. **Simplify API** - Remove wasteful MongoDB-style wrappers that provide no value for SQLite
2. **Uniform Filter Support** - All "One" methods accept `string | QueryFilter<T>` for maximum flexibility
3. **Add Useful MongoDB Methods** - Implement `findOneAnd*` variants and utility methods
4. **Document Differences** - Clearly explain where StrataDB differs from MongoDB

### Philosophy

**StrataDB is NOT MongoDB.** It's SQLite with a MongoDB-inspired API. We:
- ✅ Add useful MongoDB patterns that work well with SQLite
- ✅ Simplify return types to leverage ACID guarantees
- ✅ Create predictable, uniform APIs
- ✅ Document differences honestly
- ❌ Don't pretend to be MongoDB
- ❌ Don't add features that don't map to SQLite

### Key Decisions

1. **API Uniformity:** ALL "One" methods accept `string | QueryFilter<T>` - no exceptions
2. **insertMany syntax:** Keep array parameter (MongoDB standard)
3. **Index methods:** Skip - indexes are schema-defined
4. **Return types:** Remove `acknowledged` fields, simplify `insertOne` to return `T` directly

---

## API Design Principles

### The Uniform Pattern

**✅ CONSISTENT PATTERN FOR ALL SINGLE-DOCUMENT OPERATIONS:**

```typescript
// Pattern: "One" methods accept string ID OR filter
method(filter: string | QueryFilter<T>, ...otherParams)
```

**This applies to ALL of these methods:**
- `findOne(string | QueryFilter<T>)`
- `updateOne(string | QueryFilter<T>, update, options?)`
- `deleteOne(string | QueryFilter<T>)`
- `replaceOne(string | QueryFilter<T>, replacement)`
- `findOneAndUpdate(string | QueryFilter<T>, update, options?)`
- `findOneAndDelete(string | QueryFilter<T>, options?)`
- `findOneAndReplace(string | QueryFilter<T>, replacement, options?)`

**Why this is better:**
- ✅ Zero cognitive load - all "One" methods work the same way
- ✅ Maximum convenience - pass string when you have ID, filter when you don't
- ✅ Predictable - if method name has "One", it accepts both
- ✅ Backwards compatible - existing string IDs keep working

### Complete API Signature Reference

```typescript
// ===== Read Operations =====

/**
 * Find document by ID (optimized path).
 * @param id - Document ID
 */
findById(id: string): Promise<T | null>

/**
 * Find first document matching ID or filter.
 * @param filter - Document ID (string) or query filter
 */
findOne(
  filter: string | QueryFilter<T>,  // ✅ UNIFORM
  options?: QueryOptions<T>
): Promise<T | null>

/**
 * Find all documents matching filter.
 * @param filter - Query filter (required)
 */
find(
  filter: QueryFilter<T>,
  options?: QueryOptions<T>
): Promise<T[]>

/**
 * Count documents matching filter.
 * @param filter - Query filter (required)
 */
count(filter: QueryFilter<T>): Promise<number>

// ===== Single-Document Write Operations =====

/**
 * Insert a single document.
 * @param doc - Document to insert
 */
insertOne(
  doc: Omit<T, "_id" | "createdAt" | "updatedAt">
): Promise<T>  // ✅ Returns document directly

/**
 * Update a single document by ID or filter.
 * @param filter - Document ID (string) or query filter
 */
updateOne(
  filter: string | QueryFilter<T>,  // ✅ UNIFORM
  update: Partial<T>,
  options?: { upsert?: boolean }
): Promise<T | null>

/**
 * Delete a single document by ID or filter.
 * @param filter - Document ID (string) or query filter
 */
deleteOne(
  filter: string | QueryFilter<T>  // ✅ UNIFORM
): Promise<boolean>

/**
 * Replace a single document by ID or filter.
 * @param filter - Document ID (string) or query filter
 */
replaceOne(
  filter: string | QueryFilter<T>,  // ✅ UNIFORM
  replacement: Omit<T, "_id" | "createdAt" | "updatedAt">
): Promise<T | null>

// ===== Atomic Operations =====

/**
 * Atomically find and update a document.
 * @param filter - Document ID (string) or query filter
 */
findOneAndUpdate(
  filter: string | QueryFilter<T>,  // ✅ UNIFORM
  update: Partial<T>,
  options?: {
    sort?: SortSpec<T>
    returnDocument?: 'before' | 'after'
    upsert?: boolean
  }
): Promise<T | null>

/**
 * Atomically find and delete a document.
 * @param filter - Document ID (string) or query filter
 */
findOneAndDelete(
  filter: string | QueryFilter<T>,  // ✅ UNIFORM
  options?: { sort?: SortSpec<T> }
): Promise<T | null>

/**
 * Atomically find and replace a document.
 * @param filter - Document ID (string) or query filter
 */
findOneAndReplace(
  filter: string | QueryFilter<T>,  // ✅ UNIFORM
  replacement: Omit<T, "_id" | "createdAt" | "updatedAt">,
  options?: {
    sort?: SortSpec<T>
    returnDocument?: 'before' | 'after'
    upsert?: boolean
  }
): Promise<T | null>

// ===== Batch Operations =====

/**
 * Insert multiple documents.
 * @param docs - Array of documents to insert
 */
insertMany(
  docs: readonly Omit<T, "_id" | "createdAt" | "updatedAt">[]
): Promise<InsertManyResult<T>>  // ✅ No 'acknowledged' field

/**
 * Update multiple documents matching filter.
 * @param filter - Query filter (required)
 */
updateMany(
  filter: QueryFilter<T>,
  update: Partial<T>
): Promise<UpdateResult>  // ✅ No 'acknowledged' field

/**
 * Delete multiple documents matching filter.
 * @param filter - Query filter (required)
 */
deleteMany(
  filter: QueryFilter<T>
): Promise<DeleteResult>  // ✅ No 'acknowledged' field

// ===== Utility Methods =====

/**
 * Get distinct values for a field.
 * @param field - Field name
 * @param filter - Optional query filter
 */
distinct<K extends keyof T>(
  field: K,
  filter?: QueryFilter<T>
): Promise<Array<T[K]>>

/**
 * Get total document count (fast, no filter).
 */
estimatedDocumentCount(): Promise<number>

/**
 * Drop the entire collection (⚠️ irreversible).
 */
drop(): Promise<void>
```

---

## Part 1: Remove Wasteful Wrappers

### The Problem

MongoDB returns `{ acknowledged: true }` because operations can fail to reach the server (distributed, network failures). SQLite is local and ACID - if a function returns without throwing, it succeeded. The `acknowledged` field provides ZERO information.

Similarly, `insertOne` returns `{ document: T, acknowledged: true }` when it could just return `T`.

### Changes Summary

| Type | Before | After |
|------|--------|-------|
| `InsertOneResult<T>` | `{ document: T, acknowledged: true }` | `T` (just the type alias) |
| `InsertManyResult<T>` | `{ documents: T[], insertedCount: number, acknowledged: true }` | `{ documents: T[], insertedCount: number }` |
| `UpdateResult` | `{ matchedCount: number, modifiedCount: number, acknowledged: true }` | `{ matchedCount: number, modifiedCount: number }` |
| `DeleteResult` | `{ deletedCount: number, acknowledged: true }` | `{ deletedCount: number }` |

---

### 1.1 Type Definitions

**File:** `src/collection-types.ts` (Lines 6-70)

**Changes:**

```typescript
// BEFORE (Line 14-20)
export type InsertOneResult<T extends Document> = {
  readonly document: T
  readonly acknowledged: true
}

// AFTER
/**
 * @deprecated InsertOneResult is now just T. This type alias remains for backwards compatibility.
 * Will be removed in v2.0.0.
 */
export type InsertOneResult<T extends Document> = T
```

```typescript
// BEFORE (Line 30-39)
export type InsertManyResult<T extends Document> = {
  readonly documents: readonly T[]
  readonly insertedCount: number
  readonly acknowledged: true
}

// AFTER
export type InsertManyResult<T extends Document> = {
  readonly documents: readonly T[]
  readonly insertedCount: number
  // ✅ No acknowledged field
}
```

```typescript
// BEFORE (Line 47-56)
export type UpdateResult = {
  readonly matchedCount: number
  readonly modifiedCount: number
  readonly acknowledged: true
}

// AFTER
export type UpdateResult = {
  readonly matchedCount: number
  readonly modifiedCount: number
  // ✅ No acknowledged field
}
```

```typescript
// BEFORE (Line 64-70)
export type DeleteResult = {
  readonly deletedCount: number
  readonly acknowledged: true
}

// AFTER
export type DeleteResult = {
  readonly deletedCount: number
  // ✅ No acknowledged field
}
```

**Update Collection interface signature (Line 335-337):**

```typescript
// BEFORE
insertOne(doc: Omit<T, "_id" | "createdAt" | "updatedAt">): Promise<InsertOneResult<T>>

// AFTER
insertOne(doc: Omit<T, "_id" | "createdAt" | "updatedAt">): Promise<T>
```

**JSDoc Examples to Update in collection-types.ts:**

- Line 140: `console.log(result.document._id)` → `console.log(result._id)`
- Line 327: `console.log(\`Inserted user with ID: ${result.document._id}\`)` → `console.log(\`Inserted user with ID: ${result._id}\`)`
- All other JSDoc examples referencing `.acknowledged` should note it's removed

---

### 1.2 Implementation Changes

**File:** `src/sqlite-collection.ts`

**Line ~450-480 - insertOne method:**

```typescript
// BEFORE
async insertOne(
  doc: Omit<T, "_id" | "createdAt" | "updatedAt">
): Promise<InsertOneResult<T>> {
  // ... validation and insertion logic ...
  return Promise.resolve({
    document: fullDoc,
    acknowledged: true,
  })
}

// AFTER
async insertOne(
  doc: Omit<T, "_id" | "createdAt" | "updatedAt">
): Promise<T> {
  // ... validation and insertion logic ...
  return Promise.resolve(fullDoc)  // ✅ Direct return
}
```

**Line ~510-580 - insertMany method:**

```typescript
// BEFORE
return Promise.resolve({
  documents: insertedDocs,
  insertedCount: insertedDocs.length,
  acknowledged: true,
})

// AFTER
return Promise.resolve({
  documents: insertedDocs,
  insertedCount: insertedDocs.length,
  // ✅ No acknowledged field
})
```

**Line ~650-720 - updateMany method:**

```typescript
// BEFORE
return Promise.resolve({
  matchedCount,
  modifiedCount: updatedDocs.length,
  acknowledged: true,
})

// AFTER
return Promise.resolve({
  matchedCount,
  modifiedCount: updatedDocs.length,
  // ✅ No acknowledged field
})
```

**Line ~750-780 - deleteMany method:**

```typescript
// BEFORE
return Promise.resolve({
  deletedCount: result.changes,
  acknowledged: true,
})

// AFTER
return Promise.resolve({
  deletedCount: result.changes,
  // ✅ No acknowledged field
})
```

---

### 1.3 Test File Changes

**Occurrences found:** 63 instances of `.document._id` and `.document.field` patterns across test files

#### File: `test/integration/collection-crud.test.ts` (16 occurrences)

**Pattern to find:** `result.document._id` → `result._id`

**Example changes:**

```typescript
// BEFORE
const result = await users.insertOne({ name: 'Alice', age: 30 })
expect(result.document._id).toBeDefined()
expect(typeof result.document._id).toBe('string')
expect(result.document.name).toBe('Alice')

// AFTER
const user = await users.insertOne({ name: 'Alice', age: 30 })
expect(user._id).toBeDefined()
expect(typeof user._id).toBe('string')
expect(user.name).toBe('Alice')
```

#### File: `test/integration/transactions.test.ts` (9 occurrences)

Same pattern - replace `.document.` with direct access.

#### File: `test/integration/standard-schema-validators.test.ts` (12 occurrences)

Same pattern - replace `.document.` with direct access.

#### File: `test/integration/symbol-dispose.test.ts` (1 occurrence)

Same pattern - replace `.document.` with direct access.

#### File: `test/integration/batch-operations.test.ts` (3 `.acknowledged` occurrences)

```typescript
// BEFORE
const result = await users.insertMany([...])
expect(result.acknowledged).toBe(true)

// AFTER
const result = await users.insertMany([...])
// Remove the .acknowledged assertion entirely
```

---

## Part 2: Uniform Filter Support

### Goal

Make **ALL** single-document operations accept `string | QueryFilter<T>` for maximum API uniformity and predictability.

### Methods to Update

1. ✅ `findOne` - Already accepts QueryFilter, add string support
2. ✅ `updateOne` - Add `string | QueryFilter<T>` support
3. ✅ `deleteOne` - Add `string | QueryFilter<T>` support
4. ✅ `replaceOne` - Add `string | QueryFilter<T>` support
5. ✅ `findOneAndUpdate` - Add string support (was filter-only in draft)
6. ✅ `findOneAndDelete` - Add string support (was filter-only in draft)
7. ✅ `findOneAndReplace` - Add string support (was filter-only in draft)

---

### 2.1 Type Changes

**File:** `src/collection-types.ts`

**Update ALL "One" method signatures:**

```typescript
// BEFORE (Line 265-268)
findOne(
  filter: QueryFilter<T>,
  options?: Omit<QueryOptions<T>, "limit" | "skip">
): Promise<T | null>

// AFTER
findOne(
  filter: string | QueryFilter<T>,  // ✅ UNIFORM
  options?: Omit<QueryOptions<T>, "limit" | "skip">
): Promise<T | null>
```

```typescript
// BEFORE (Line 373-377)
updateOne(
  id: string,
  update: Omit<Partial<T>, "_id" | "createdAt" | "updatedAt">,
  options?: { upsert?: boolean }
): Promise<T | null>

// AFTER
updateOne(
  filter: string | QueryFilter<T>,  // ✅ UNIFORM (renamed param)
  update: Omit<Partial<T>, "_id" | "createdAt" | "updatedAt">,
  options?: { upsert?: boolean }
): Promise<T | null>
```

```typescript
// BEFORE (Line 410-413)
replaceOne(
  id: string,
  doc: Omit<T, "_id" | "createdAt" | "updatedAt">
): Promise<T | null>

// AFTER
replaceOne(
  filter: string | QueryFilter<T>,  // ✅ UNIFORM (renamed param)
  doc: Omit<T, "_id" | "createdAt" | "updatedAt">
): Promise<T | null>
```

```typescript
// BEFORE (Line 435)
deleteOne(id: string): Promise<boolean>

// AFTER
deleteOne(filter: string | QueryFilter<T>): Promise<boolean>  // ✅ UNIFORM
```

**JSDoc Updates:**

Update parameter descriptions and add examples showing both string and filter usage:

```typescript
/**
 * Find the first document matching ID or filter.
 *
 * @param filter - Document ID (string) or query filter
 * @param options - Query options for sorting and projection
 * @returns Promise resolving to the first matching document, or null if none found
 *
 * @remarks
 * This method accepts either a string ID or a query filter for maximum flexibility.
 * When you have the document ID, pass it directly as a string for convenience.
 * When you need to query by other fields, pass a filter object.
 *
 * Equivalent to `find(filter, { ...options, limit: 1 })[0]` but more efficient.
 *
 * @example
 * ```typescript
 * // Find by ID (string)
 * const user = await users.findOne('user-123')
 *
 * // Find by filter
 * const admin = await users.findOne({ role: 'admin' })
 * const alice = await users.findOne({ email: 'alice@example.com' })
 *
 * // Find with sort
 * const latest = await users.findOne(
 *   { status: 'active' },
 *   { sort: { createdAt: -1 } }
 * )
 * ```
 */
```

Similar JSDoc updates for `updateOne`, `deleteOne`, `replaceOne`.

---

### 2.2 Implementation Changes

**File:** `src/sqlite-collection.ts`

**Add private helper method (~Line 200):**

```typescript
/**
 * Normalize filter parameter to QueryFilter.
 * Accepts either a string ID or a QueryFilter object for backwards compatibility.
 *
 * @internal
 */
private normalizeFilter(filter: string | QueryFilter<T>): QueryFilter<T> {
  if (typeof filter === 'string') {
    return { _id: filter } as QueryFilter<T>
  }
  return filter
}
```

**Update findOne method (~Line 340):**

```typescript
// BEFORE
findOne(
  filter: QueryFilter<T>,
  options?: Omit<QueryOptions<T>, "limit" | "skip">
): Promise<T | null> {
  return this.find(filter, { ...options, limit: 1 }).then(docs => docs[0] ?? null)
}

// AFTER
findOne(
  filter: string | QueryFilter<T>,  // ✅ UNIFORM
  options?: Omit<QueryOptions<T>, "limit" | "skip">
): Promise<T | null> {
  const normalizedFilter = this.normalizeFilter(filter)
  return this.find(normalizedFilter, { ...options, limit: 1 }).then(docs => docs[0] ?? null)
}
```

**Update updateOne method (~Line 590):**

```typescript
// BEFORE
async updateOne(
  id: string,
  update: Omit<Partial<T>, "_id" | "createdAt" | "updatedAt">,
  options?: { upsert?: boolean }
): Promise<T | null> {
  const existing = await this.findById(id)
  if (!existing) {
    if (options?.upsert) {
      return this.insertOne({ ...update, _id: id } as any)
    }
    return null
  }
  // ... rest of update logic
}

// AFTER
async updateOne(
  filter: string | QueryFilter<T>,  // ✅ UNIFORM
  update: Omit<Partial<T>, "_id" | "createdAt" | "updatedAt">,
  options?: { upsert?: boolean }
): Promise<T | null> {
  const normalizedFilter = this.normalizeFilter(filter)
  const existing = await this.findOne(normalizedFilter)

  if (!existing) {
    if (options?.upsert) {
      // Merge filter fields into document for upsert
      const filterFields = typeof filter === 'string' ? { _id: filter } : filter
      return this.insertOne({ ...filterFields, ...update } as any)
    }
    return null
  }

  // Rest of update logic using existing._id
  // ... (keep existing merge logic)
}
```

**Update replaceOne method (~Line 640):**

```typescript
// BEFORE
async replaceOne(
  id: string,
  doc: Omit<T, "_id" | "createdAt" | "updatedAt">
): Promise<T | null> {
  const existing = await this.findById(id)
  if (!existing) return null
  // ... replacement logic
}

// AFTER
async replaceOne(
  filter: string | QueryFilter<T>,  // ✅ UNIFORM
  doc: Omit<T, "_id" | "createdAt" | "updatedAt">
): Promise<T | null> {
  const normalizedFilter = this.normalizeFilter(filter)
  const existing = await this.findOne(normalizedFilter)
  if (!existing) return null

  // Rest of replacement logic using existing._id
  // ... (keep existing logic)
}
```

**Update deleteOne method (~Line 730):**

```typescript
// BEFORE
deleteOne(id: string): Promise<boolean> {
  const stmt = this.db.prepare(`DELETE FROM ${this.name} WHERE _id = ?`)
  const result = stmt.run(id)
  return Promise.resolve(result.changes > 0)
}

// AFTER
deleteOne(filter: string | QueryFilter<T>): Promise<boolean> {  // ✅ UNIFORM
  const normalizedFilter = this.normalizeFilter(filter)

  // Optimization: If filter is just { _id: 'xxx' }, use direct delete
  if ('_id' in normalizedFilter && Object.keys(normalizedFilter).length === 1) {
    const stmt = this.db.prepare(`DELETE FROM ${this.name} WHERE _id = ?`)
    const result = stmt.run(normalizedFilter._id)
    return Promise.resolve(result.changes > 0)
  }

  // For complex filters: find first match, then delete by ID
  const { sql: whereClause, params } = this.translator.translate(normalizedFilter)
  let findSql = `SELECT _id FROM ${this.name}`
  if (whereClause && whereClause !== '1=1') {
    findSql += ` WHERE ${whereClause}`
  }
  findSql += ` LIMIT 1`

  const row = this.db.prepare<{ _id: string }, SQLQueryBindings[]>(findSql).get(...params)
  if (!row) return Promise.resolve(false)

  const stmt = this.db.prepare(`DELETE FROM ${this.name} WHERE _id = ?`)
  const result = stmt.run(row._id)
  return Promise.resolve(result.changes > 0)
}
```

---

### 2.3 Test Additions

**File:** `test/integration/collection-crud.test.ts`

**Add new test section (~Line 500):**

```typescript
describe("Uniform filter support for all 'One' methods", () => {
  describe("findOne", () => {
    it("should accept string ID", async () => {
      const inserted = await users.insertOne({ name: 'Alice', age: 30 })
      const found = await users.findOne(inserted._id)
      expect(found?._id).toBe(inserted._id)
    })

    it("should accept query filter", async () => {
      await users.insertOne({ name: 'Alice', email: 'alice@example.com', age: 30 })
      const found = await users.findOne({ email: 'alice@example.com' })
      expect(found?.name).toBe('Alice')
    })
  })

  describe("updateOne", () => {
    it("should accept string ID (backwards compatible)", async () => {
      const user = await users.insertOne({ name: 'Alice', age: 30 })
      const updated = await users.updateOne(user._id, { age: 31 })
      expect(updated?.age).toBe(31)
    })

    it("should accept query filter", async () => {
      await users.insertOne({ name: 'Alice', email: 'alice@example.com', age: 30 })
      const updated = await users.updateOne(
        { email: 'alice@example.com' },
        { age: 31 }
      )
      expect(updated?.age).toBe(31)
    })
  })

  describe("deleteOne", () => {
    it("should accept string ID (backwards compatible)", async () => {
      const user = await users.insertOne({ name: 'Alice', age: 30 })
      const deleted = await users.deleteOne(user._id)
      expect(deleted).toBe(true)
    })

    it("should accept query filter", async () => {
      await users.insertOne({ name: 'Alice', email: 'alice@example.com', age: 30 })
      const deleted = await users.deleteOne({ email: 'alice@example.com' })
      expect(deleted).toBe(true)
    })
  })

  describe("replaceOne", () => {
    it("should accept string ID (backwards compatible)", async () => {
      const user = await users.insertOne({ name: 'Alice', age: 30 })
      const replaced = await users.replaceOne(user._id, { name: 'Alicia', age: 31 })
      expect(replaced?.name).toBe('Alicia')
    })

    it("should accept query filter", async () => {
      await users.insertOne({ name: 'Alice', email: 'alice@example.com', age: 30 })
      const replaced = await users.replaceOne(
        { email: 'alice@example.com' },
        { name: 'Alicia', email: 'alice@example.com', age: 31 }
      )
      expect(replaced?.name).toBe('Alicia')
    })
  })

  describe("Edge cases", () => {
    it("should handle _id in filter object", async () => {
      const user = await users.insertOne({ name: 'Alice', age: 30 })
      const found = await users.findOne({ _id: user._id })
      expect(found?._id).toBe(user._id)
    })

    it("should update first match when multiple documents match filter", async () => {
      await users.insertMany([
        { name: 'Alice', status: 'inactive', age: 30 },
        { name: 'Bob', status: 'inactive', age: 25 }
      ])
      await users.updateOne({ status: 'inactive' }, { status: 'active' })
      const activeCount = await users.count({ status: 'active' })
      expect(activeCount).toBe(1)  // Only one updated
    })

    it("should delete first match when multiple documents match filter", async () => {
      await users.insertMany([
        { name: 'Alice', status: 'inactive', age: 30 },
        { name: 'Bob', status: 'inactive', age: 25 }
      ])
      const deleted = await users.deleteOne({ status: 'inactive' })
      expect(deleted).toBe(true)
      const remaining = await users.count({ status: 'inactive' })
      expect(remaining).toBe(1)  // Only one deleted
    })
  })
})
```

---

## Part 3: New MongoDB-Compatible Methods

Add three atomic find-and-modify methods with UNIFORM `string | QueryFilter<T>` support.

---

### 3.1 findOneAndDelete

**Purpose:** Atomically find and delete a document, returning the deleted document.

#### Type Definition

**File:** `src/collection-types.ts` (Add after `deleteOne` ~Line 445)

```typescript
/**
 * Find and delete a single document atomically.
 *
 * @param filter - Document ID (string) or query filter
 * @param options - Query options (sort)
 * @returns The deleted document, or null if not found
 *
 * @remarks
 * This operation is atomic - it finds and deletes in a single operation.
 * Useful when you need the deleted document's data (e.g., for logging, undo operations).
 *
 * Accepts either a string ID or a query filter for maximum flexibility.
 *
 * @example
 * ```typescript
 * // Delete by ID
 * const deleted = await users.findOneAndDelete('user-123')
 *
 * // Delete by filter with sort
 * const deleted = await users.findOneAndDelete(
 *   { status: 'inactive' },
 *   { sort: { createdAt: 1 } }  // Delete oldest first
 * )
 * if (deleted) {
 *   console.log(`Deleted user: ${deleted.name}`)
 *   await logDeletion(deleted)
 * }
 * ```
 */
findOneAndDelete(
  filter: string | QueryFilter<T>,  // ✅ UNIFORM
  options?: { sort?: SortSpec<T> }
): Promise<T | null>
```

#### Implementation

**File:** `src/sqlite-collection.ts` (Add after `deleteMany` ~Line 780)

```typescript
async findOneAndDelete(
  filter: string | QueryFilter<T>,  // ✅ UNIFORM
  options?: { sort?: SortSpec<T> }
): Promise<T | null> {
  // Normalize and find the document first
  const normalizedFilter = this.normalizeFilter(filter)
  const doc = await this.findOne(normalizedFilter, options)
  if (!doc) return null

  // Delete by ID
  const stmt = this.db.prepare(`DELETE FROM ${this.name} WHERE _id = ?`)
  stmt.run(doc._id)

  return doc
}
```

**Estimated lines:** ~13 lines

#### Tests

**File:** `test/integration/collection-crud.test.ts` (Add new section ~Line 600)

```typescript
describe("findOneAndDelete", () => {
  it("should accept string ID", async () => {
    const user = await users.insertOne({ name: 'Alice', age: 30 })
    const deleted = await users.findOneAndDelete(user._id)

    expect(deleted?._id).toBe(user._id)
    expect(deleted?.name).toBe('Alice')
    expect(await users.findById(user._id)).toBeNull()
  })

  it("should accept query filter", async () => {
    await users.insertOne({ name: 'Alice', status: 'inactive', age: 30 })
    const deleted = await users.findOneAndDelete({ status: 'inactive' })

    expect(deleted).toBeDefined()
    expect(deleted?.name).toBe('Alice')
    expect(await users.count({ status: 'inactive' })).toBe(0)
  })

  it("should return null if no document matches", async () => {
    const deleted = await users.findOneAndDelete({ status: 'nonexistent' })
    expect(deleted).toBeNull()
  })

  it("should respect sort option when multiple matches exist", async () => {
    await users.insertMany([
      { name: 'Alice', status: 'inactive', age: 30 },
      { name: 'Bob', status: 'inactive', age: 25 },
      { name: 'Charlie', status: 'inactive', age: 35 }
    ])

    const deleted = await users.findOneAndDelete(
      { status: 'inactive' },
      { sort: { age: 1 } }  // Youngest first
    )

    expect(deleted?.name).toBe('Bob')  // Age 25
    expect(await users.count({ status: 'inactive' })).toBe(2)
  })

  it("should work with _id filter object", async () => {
    const user = await users.insertOne({ name: 'Alice', age: 30 })
    const deleted = await users.findOneAndDelete({ _id: user._id })
    expect(deleted?._id).toBe(user._id)
  })
})
```

---

### 3.2 findOneAndUpdate

**Purpose:** Atomically find and update a document, returning before or after state.

#### Type Definition

**File:** `src/collection-types.ts` (Add after `findOneAndDelete` ~Line 480)

```typescript
/**
 * Find and update a single document atomically.
 *
 * @param filter - Document ID (string) or query filter
 * @param update - Partial document with fields to update
 * @param options - Update options (sort, returnDocument, upsert)
 * @returns The document before or after update, or null if not found
 *
 * @remarks
 * This operation is atomic - it finds and updates in a single logical operation.
 * Use `returnDocument` to control whether you get the document state before or after the update.
 *
 * Accepts either a string ID or a query filter for maximum flexibility.
 *
 * @example
 * ```typescript
 * // Update by ID
 * const updated = await users.findOneAndUpdate(
 *   'user-123',
 *   { loginCount: 5 },
 *   { returnDocument: 'after' }
 * )
 *
 * // Update by filter
 * const updated = await users.findOneAndUpdate(
 *   { email: 'alice@example.com' },
 *   { loginCount: 5 },
 *   { returnDocument: 'after' }
 * )
 * console.log(`New login count: ${updated?.loginCount}`)
 *
 * // Get previous state before update
 * const before = await users.findOneAndUpdate(
 *   { _id: userId },
 *   { status: 'archived' },
 *   { returnDocument: 'before' }
 * )
 * await logStatusChange(before, 'archived')
 * ```
 */
findOneAndUpdate(
  filter: string | QueryFilter<T>,  // ✅ UNIFORM
  update: Omit<Partial<T>, "_id" | "createdAt" | "updatedAt">,
  options?: {
    sort?: SortSpec<T>
    returnDocument?: 'before' | 'after'  // default: 'after'
    upsert?: boolean
  }
): Promise<T | null>
```

#### Implementation

**File:** `src/sqlite-collection.ts` (Add after `findOneAndDelete` ~Line 810)

```typescript
async findOneAndUpdate(
  filter: string | QueryFilter<T>,  // ✅ UNIFORM
  update: Omit<Partial<T>, "_id" | "createdAt" | "updatedAt">,
  options?: {
    sort?: SortSpec<T>
    returnDocument?: 'before' | 'after'
    upsert?: boolean
  }
): Promise<T | null> {
  const returnDoc = options?.returnDocument ?? 'after'
  const normalizedFilter = this.normalizeFilter(filter)
  const existing = await this.findOne(normalizedFilter, { sort: options?.sort })

  // Handle not found case
  if (!existing) {
    if (options?.upsert) {
      // Insert new document with merged filter + update
      const filterFields = typeof filter === 'string' ? { _id: filter } : filter
      const merged = { ...filterFields, ...update } as any
      const inserted = await this.insertOne(merged)
      return returnDoc === 'after' ? inserted : null
    }
    return null
  }

  // Document exists - update it
  const before = existing
  const after = await this.updateOne(existing._id, update)

  return returnDoc === 'before' ? before : after
}
```

**Estimated lines:** ~28 lines

#### Tests

**File:** `test/integration/collection-crud.test.ts` (Add after `findOneAndDelete` tests ~Line 650)

```typescript
describe("findOneAndUpdate", () => {
  it("should accept string ID", async () => {
    const user = await users.insertOne({ name: 'Alice', age: 30 })
    const updated = await users.findOneAndUpdate(user._id, { age: 31 })
    expect(updated?.age).toBe(31)
  })

  it("should accept query filter", async () => {
    await users.insertOne({ name: 'Alice', email: 'alice@example.com', age: 30 })
    const updated = await users.findOneAndUpdate(
      { email: 'alice@example.com' },
      { age: 31 }
    )
    expect(updated?.age).toBe(31)
  })

  it("should return updated document by default", async () => {
    await users.insertOne({ name: 'Alice', age: 30 })
    const updated = await users.findOneAndUpdate(
      { name: 'Alice' },
      { age: 31 }
    )
    expect(updated?.age).toBe(31)  // New value
  })

  it("should return document before update when specified", async () => {
    await users.insertOne({ name: 'Alice', age: 30 })
    const before = await users.findOneAndUpdate(
      { name: 'Alice' },
      { age: 31 },
      { returnDocument: 'before' }
    )
    expect(before?.age).toBe(30)  // Old value
  })

  it("should return document after update when specified", async () => {
    await users.insertOne({ name: 'Alice', age: 30 })
    const after = await users.findOneAndUpdate(
      { name: 'Alice' },
      { age: 31 },
      { returnDocument: 'after' }
    )
    expect(after?.age).toBe(31)  // New value
  })

  it("should return null if no document matches", async () => {
    const updated = await users.findOneAndUpdate(
      { name: 'Nonexistent' },
      { age: 31 }
    )
    expect(updated).toBeNull()
  })

  it("should upsert when option is true and document not found", async () => {
    const result = await users.findOneAndUpdate(
      { email: 'new@example.com' },
      { name: 'New User', age: 25 },
      { upsert: true, returnDocument: 'after' }
    )

    expect(result).toBeDefined()
    expect(result?.email).toBe('new@example.com')
    expect(result?.name).toBe('New User')
  })

  it("should return null on upsert with returnDocument: before", async () => {
    const result = await users.findOneAndUpdate(
      { email: 'new@example.com' },
      { name: 'New User', age: 25 },
      { upsert: true, returnDocument: 'before' }
    )
    expect(result).toBeNull()  // No "before" state for new doc
  })

  it("should respect sort option when multiple matches exist", async () => {
    await users.insertMany([
      { name: 'User', age: 30, score: 100 },
      { name: 'User', age: 25, score: 200 },
      { name: 'User', age: 35, score: 150 }
    ])

    const updated = await users.findOneAndUpdate(
      { name: 'User' },
      { status: 'updated' },
      { sort: { score: -1 } }  // Highest score first
    )

    expect(updated?.age).toBe(25)  // Score 200
  })

  it("should work with _id in filter object", async () => {
    const user = await users.insertOne({ name: 'Alice', age: 30 })
    const updated = await users.findOneAndUpdate(
      { _id: user._id },
      { age: 31 }
    )
    expect(updated?.age).toBe(31)
  })
})
```

---

### 3.3 findOneAndReplace

**Purpose:** Atomically find and replace an entire document.

#### Type Definition

**File:** `src/collection-types.ts` (Add after `findOneAndUpdate` ~Line 545)

```typescript
/**
 * Find and replace a single document atomically.
 *
 * @param filter - Document ID (string) or query filter
 * @param replacement - Complete replacement document (without _id, createdAt, updatedAt)
 * @param options - Replace options (sort, returnDocument, upsert)
 * @returns The document before or after replacement, or null if not found
 *
 * @remarks
 * This operation replaces the ENTIRE document (except _id, createdAt).
 * Unlike `findOneAndUpdate` which merges fields, this replaces everything.
 * The replacement document is validated against the schema.
 *
 * Accepts either a string ID or a query filter for maximum flexibility.
 *
 * @example
 * ```typescript
 * // Replace by ID
 * const replaced = await users.findOneAndReplace(
 *   'user-123',
 *   {
 *     name: 'New Name',
 *     email: 'new@example.com',
 *     age: 30,
 *     status: 'active'
 *   },
 *   { returnDocument: 'after' }
 * )
 *
 * // Replace by filter
 * const replaced = await users.findOneAndReplace(
 *   { email: 'old@example.com' },
 *   {
 *     name: 'New Name',
 *     email: 'new@example.com',
 *     age: 30,
 *     status: 'active'
 *   }
 * )
 * ```
 */
findOneAndReplace(
  filter: string | QueryFilter<T>,  // ✅ UNIFORM
  replacement: Omit<T, '_id' | 'createdAt' | 'updatedAt'>,
  options?: {
    sort?: SortSpec<T>
    returnDocument?: 'before' | 'after'
    upsert?: boolean
  }
): Promise<T | null>
```

#### Implementation

**File:** `src/sqlite-collection.ts` (Add after `findOneAndUpdate` ~Line 865)

```typescript
async findOneAndReplace(
  filter: string | QueryFilter<T>,  // ✅ UNIFORM
  replacement: Omit<T, '_id' | 'createdAt' | 'updatedAt'>,
  options?: {
    sort?: SortSpec<T>
    returnDocument?: 'before' | 'after'
    upsert?: boolean
  }
): Promise<T | null> {
  const returnDoc = options?.returnDocument ?? 'after'
  const normalizedFilter = this.normalizeFilter(filter)
  const existing = await this.findOne(normalizedFilter, { sort: options?.sort })

  // Handle not found case
  if (!existing) {
    if (options?.upsert) {
      const inserted = await this.insertOne(replacement)
      return returnDoc === 'after' ? inserted : null
    }
    return null
  }

  // Document exists - replace it
  const before = existing
  const after = await this.replaceOne(existing._id, replacement)

  return returnDoc === 'before' ? before : after
}
```

**Estimated lines:** ~26 lines

#### Tests

**File:** `test/integration/collection-crud.test.ts` (Add after `findOneAndUpdate` tests ~Line 760)

```typescript
describe("findOneAndReplace", () => {
  it("should accept string ID", async () => {
    const user = await users.insertOne({ name: 'Alice', email: 'alice@example.com', age: 30 })
    const replaced = await users.findOneAndReplace(
      user._id,
      { name: 'Alicia', email: 'alicia@example.com', age: 31, status: 'inactive' }
    )
    expect(replaced?.name).toBe('Alicia')
  })

  it("should accept query filter", async () => {
    await users.insertOne({ name: 'Alice', email: 'alice@example.com', age: 30 })
    const replaced = await users.findOneAndReplace(
      { email: 'alice@example.com' },
      { name: 'Alicia', email: 'alicia@example.com', age: 31, status: 'inactive' }
    )
    expect(replaced?.name).toBe('Alicia')
  })

  it("should return replaced document by default", async () => {
    await users.insertOne({ name: 'Alice', email: 'alice@example.com', age: 30 })
    const replaced = await users.findOneAndReplace(
      { email: 'alice@example.com' },
      { name: 'Alicia', email: 'alicia@example.com', age: 31, status: 'inactive' }
    )
    expect(replaced?.name).toBe('Alicia')  // New name
    expect(replaced?.age).toBe(31)         // New age
  })

  it("should return document before replacement when specified", async () => {
    await users.insertOne({ name: 'Alice', email: 'alice@example.com', age: 30 })
    const before = await users.findOneAndReplace(
      { email: 'alice@example.com' },
      { name: 'Alicia', email: 'alicia@example.com', age: 31, status: 'inactive' },
      { returnDocument: 'before' }
    )
    expect(before?.name).toBe('Alice')  // Old name
    expect(before?.age).toBe(30)        // Old age
  })

  it("should return null if no document matches", async () => {
    const replaced = await users.findOneAndReplace(
      { email: 'nonexistent@example.com' },
      { name: 'New', email: 'new@example.com', age: 25, status: 'active' }
    )
    expect(replaced).toBeNull()
  })

  it("should upsert when option is true and document not found", async () => {
    const result = await users.findOneAndReplace(
      { email: 'new@example.com' },
      { name: 'New User', email: 'new@example.com', age: 25, status: 'active' },
      { upsert: true, returnDocument: 'after' }
    )

    expect(result).toBeDefined()
    expect(result?.name).toBe('New User')
  })

  it("should preserve _id and createdAt after replacement", async () => {
    const original = await users.insertOne({ name: 'Alice', email: 'alice@example.com', age: 30 })

    const replaced = await users.findOneAndReplace(
      { _id: original._id },
      { name: 'Alicia', email: 'alicia@example.com', age: 31, status: 'inactive' }
    )

    expect(replaced?._id).toBe(original._id)                 // Same ID
    expect(replaced?.createdAt).toBe(original.createdAt)     // Same createdAt
  })

  it("should work with _id in filter object", async () => {
    const user = await users.insertOne({ name: 'Alice', email: 'alice@example.com', age: 30 })
    const replaced = await users.findOneAndReplace(
      { _id: user._id },
      { name: 'Alicia', email: 'alicia@example.com', age: 31, status: 'inactive' }
    )
    expect(replaced?.name).toBe('Alicia')
  })
})
```

---

## Part 4: Utility Methods

Add three simple utility methods that are universally useful.

---

### 4.1 distinct

**Purpose:** Get all unique values for a field.

#### Type Definition

**File:** `src/collection-types.ts` (Add after `count` ~Line 305)

```typescript
/**
 * Get distinct values for a field across all documents.
 *
 * @param field - Field name to get distinct values for
 * @param filter - Optional query filter to narrow results
 * @returns Array of unique values for the field
 *
 * @remarks
 * Returns all unique values for the specified field.
 * Useful for getting lists of categories, tags, statuses, etc.
 *
 * @example
 * ```typescript
 * // Get all unique roles
 * const roles = await users.distinct('role')  // ['admin', 'user', 'moderator']
 *
 * // Get roles for active users only
 * const activeRoles = await users.distinct('role', { status: 'active' })
 *
 * // Get all unique tags
 * const tags = await users.distinct('tags')
 * ```
 */
distinct<K extends keyof Omit<T, "_id" | "createdAt" | "updatedAt">>(
  field: K,
  filter?: QueryFilter<T>
): Promise<Array<T[K]>>
```

#### Implementation

**File:** `src/sqlite-collection.ts` (Add after `count` ~Line 380)

```typescript
async distinct<K extends keyof Omit<T, "_id" | "createdAt" | "updatedAt">>(
  field: K,
  filter?: QueryFilter<T>
): Promise<Array<T[K]>> {
  const fieldName = field as string
  const isIndexed = this.schema.fields.find(f => f.name === fieldName)?.indexed

  let sql: string
  let params: SQLQueryBindings[] = []

  if (filter && Object.keys(filter).length > 0) {
    const { sql: whereClause, params: whereParams } = this.translator.translate(filter)
    params = whereParams

    if (isIndexed) {
      // Use generated column for indexed fields
      sql = `SELECT DISTINCT _${fieldName} AS value FROM ${this.name}`
      if (whereClause && whereClause !== '1=1') {
        sql += ` WHERE ${whereClause}`
      }
    } else {
      // Use json_extract for non-indexed fields
      sql = `SELECT DISTINCT json_extract(data, '$.${fieldName}') AS value FROM ${this.name}`
      if (whereClause && whereClause !== '1=1') {
        sql += ` WHERE ${whereClause}`
      }
    }
  } else {
    // No filter
    if (isIndexed) {
      sql = `SELECT DISTINCT _${fieldName} AS value FROM ${this.name}`
    } else {
      sql = `SELECT DISTINCT json_extract(data, '$.${fieldName}') AS value FROM ${this.name}`
    }
  }

  sql += ` ORDER BY value`  // Consistent ordering

  const results = this.db.prepare<{ value: T[K] }, SQLQueryBindings[]>(sql).all(...params)
  return results.map(row => row.value).filter(v => v !== null && v !== undefined)
}
```

**Estimated lines:** ~40 lines

#### Tests

**File:** `test/integration/collection-crud.test.ts` (Add new section ~Line 870)

```typescript
describe("distinct", () => {
  it("should return all unique values for a field", async () => {
    await users.insertMany([
      { name: 'Alice', role: 'admin', age: 30 },
      { name: 'Bob', role: 'user', age: 25 },
      { name: 'Charlie', role: 'admin', age: 35 },
      { name: 'Diana', role: 'moderator', age: 28 }
    ])

    const roles = await users.distinct('role')
    expect(roles).toHaveLength(3)
    expect(roles).toContain('admin')
    expect(roles).toContain('user')
    expect(roles).toContain('moderator')
  })

  it("should return distinct values with filter", async () => {
    await users.insertMany([
      { name: 'Alice', role: 'admin', status: 'active', age: 30 },
      { name: 'Bob', role: 'user', status: 'active', age: 25 },
      { name: 'Charlie', role: 'admin', status: 'inactive', age: 35 },
      { name: 'Diana', role: 'moderator', status: 'active', age: 28 }
    ])

    const activeRoles = await users.distinct('role', { status: 'active' })
    expect(activeRoles).toHaveLength(3)  // admin, user, moderator (all active)
  })

  it("should return empty array if no documents exist", async () => {
    const roles = await users.distinct('role')
    expect(roles).toEqual([])
  })

  it("should work with indexed fields", async () => {
    await users.insertMany([
      { name: 'Alice', age: 30 },
      { name: 'Bob', age: 25 },
      { name: 'Charlie', age: 30 },
      { name: 'Diana', age: 28 }
    ])

    const ages = await users.distinct('age')
    expect(ages).toHaveLength(3)  // 25, 28, 30
    expect(ages).toContain(25)
    expect(ages).toContain(28)
    expect(ages).toContain(30)
  })

  it("should work with non-indexed fields", async () => {
    await users.insertMany([
      { name: 'Alice', status: 'active', age: 30 },
      { name: 'Bob', status: 'inactive', age: 25 },
      { name: 'Charlie', status: 'active', age: 35 }
    ])

    const statuses = await users.distinct('status')
    expect(statuses).toHaveLength(2)
    expect(statuses).toContain('active')
    expect(statuses).toContain('inactive')
  })
})
```

---

### 4.2 estimatedDocumentCount

**Purpose:** Fast total count without filter (uses COUNT(*)).

#### Type Definition

**File:** `src/collection-types.ts` (Add after `count` ~Line 310)

```typescript
/**
 * Get estimated total count of documents in the collection.
 *
 * @returns Total number of documents
 *
 * @remarks
 * This is faster than `count({})` because it uses `COUNT(*)` without filters.
 * Use this for statistics, pagination totals, etc. when you need the total count.
 *
 * For filtered counts, use `count(filter)` instead.
 *
 * @example
 * ```typescript
 * // Get total users
 * const total = await users.estimatedDocumentCount()
 * console.log(`Total users: ${total}`)
 *
 * // Pagination
 * const pageSize = 20
 * const totalPages = Math.ceil(await users.estimatedDocumentCount() / pageSize)
 * ```
 */
estimatedDocumentCount(): Promise<number>
```

#### Implementation

**File:** `src/sqlite-collection.ts` (Add after `distinct` ~Line 425)

```typescript
estimatedDocumentCount(): Promise<number> {
  const sql = `SELECT COUNT(*) as count FROM ${this.name}`
  const result = this.db.prepare<{ count: number }, []>(sql).get()
  return Promise.resolve(result?.count ?? 0)
}
```

**Estimated lines:** ~5 lines

#### Tests

**File:** `test/integration/collection-crud.test.ts` (Add to utility methods section ~Line 940)

```typescript
describe("estimatedDocumentCount", () => {
  it("should return total document count", async () => {
    await users.insertMany([
      { name: 'Alice', age: 30 },
      { name: 'Bob', age: 25 },
      { name: 'Charlie', age: 35 }
    ])

    const total = await users.estimatedDocumentCount()
    expect(total).toBe(3)
  })

  it("should return 0 for empty collection", async () => {
    const total = await users.estimatedDocumentCount()
    expect(total).toBe(0)
  })

  it("should be faster than count({}) for large collections", async () => {
    // Insert 1000 documents
    const docs = Array.from({ length: 1000 }, (_, i) => ({
      name: `User${i}`,
      age: 20 + (i % 50)
    }))
    await users.insertMany(docs)

    const start1 = performance.now()
    await users.estimatedDocumentCount()
    const time1 = performance.now() - start1

    const start2 = performance.now()
    await users.count({})
    const time2 = performance.now() - start2

    // Both should be fast, but estimatedDocumentCount should be <=
    expect(time1).toBeLessThanOrEqual(time2 * 1.5)
  })
})
```

---

### 4.3 drop

**Purpose:** Drop the entire collection (delete table).

#### Type Definition

**File:** `src/collection-types.ts` (Add at end of Collection interface ~Line 595)

```typescript
/**
 * Drop the entire collection (delete the table).
 *
 * @returns Promise that resolves when the collection is dropped
 *
 * @remarks
 * **⚠️ WARNING:** This operation is irreversible. All data in the collection will be permanently deleted.
 *
 * Use this for:
 * - Test cleanup
 * - Development/staging environments
 * - Migration scripts
 *
 * **DO NOT** use in production without proper backups.
 *
 * @example
 * ```typescript
 * // Drop collection in test cleanup
 * afterAll(async () => {
 *   await users.drop()
 * })
 *
 * // Drop and recreate collection
 * await oldUsers.drop()
 * const newUsers = db.collection('users', newSchema)
 * ```
 */
drop(): Promise<void>
```

#### Implementation

**File:** `src/sqlite-collection.ts` (Add at end of class ~Line 910)

```typescript
drop(): Promise<void> {
  const sql = `DROP TABLE IF EXISTS ${this.name}`
  this.db.prepare(sql).run()
  return Promise.resolve()
}
```

**Estimated lines:** ~5 lines

#### Tests

**File:** `test/integration/collection-crud.test.ts` (Add to utility methods section ~Line 985)

```typescript
describe("drop", () => {
  it("should drop the collection table", async () => {
    // Create collection and insert data
    const testDb = new StrataDBClass({ database: ':memory:' })
    const testUsers = testDb.collection('test_users', userSchema)

    await testUsers.insertMany([
      { name: 'Alice', age: 30 },
      { name: 'Bob', age: 25 }
    ])

    expect(await testUsers.count({})).toBe(2)

    // Drop collection
    await testUsers.drop()

    // Verify table no longer exists by trying to query it
    expect(() => {
      testDb.db.prepare('SELECT * FROM test_users').all()
    }).toThrow()

    testDb.close()
  })

  it("should not throw if collection already dropped", async () => {
    const testDb = new StrataDBClass({ database: ':memory:' })
    const testUsers = testDb.collection('test_users', userSchema)

    await testUsers.drop()
    await expect(testUsers.drop()).resolves.not.toThrow()

    testDb.close()
  })
})
```

---

## Part 5: Documentation

### 5.1 Create MongoDB Differences Guide

**File:** `docs/guide/mongodb-differences.md` (NEW FILE)

```markdown
# Differences from MongoDB

StrataDB provides a MongoDB-inspired API for SQLite. While the API feels familiar to MongoDB developers, there are important differences you should know about.

---

## Philosophy

**StrataDB is NOT MongoDB.** It's a SQLite-backed document database with MongoDB-like patterns. We implement what makes sense for SQLite and skip what doesn't.

---

## ✅ What's the Same

- Document model with `_id` field
- Query filter syntax (`$gt`, `$gte`, `$lt`, `$lte`, `$in`, `$and`, `$or`, etc.)
- Collection-based API (`find`, `findOne`, `insertOne`, `insertMany`, etc.)
- Batch operations (`updateMany`, `deleteMany`)
- Transaction support
- Schema validation (using Standard Schema instead of JSON Schema)
- Atomic find-and-modify operations (`findOneAndUpdate`, `findOneAndDelete`, `findOneAndReplace`)

---

## 🔄 What's Different

### 1. Simpler Return Types

**MongoDB** returns acknowledgment metadata because operations can fail to reach the server:

\`\`\`typescript
// MongoDB
const result = await users.insertOne({ name: 'Alice' })
console.log(result.acknowledged)  // Did server receive it?
console.log(result.insertedId)    // What ID was assigned?
\`\`\`

**StrataDB** uses local SQLite with ACID guarantees. If a function returns, it succeeded:

\`\`\`typescript
// StrataDB
const user = await users.insertOne({ name: 'Alice' })
console.log(user._id)  // Direct access, no wrapper
// If we got here, it succeeded - no need for .acknowledged
\`\`\`

**Result type differences:**

| Operation | MongoDB | StrataDB |
|-----------|---------|----------|
| insertOne | `{ acknowledged, insertedId }` | Returns document directly (`T`) |
| insertMany | `{ acknowledged, insertedCount, insertedIds }` | `{ documents, insertedCount }` |
| updateMany | `{ acknowledged, matchedCount, modifiedCount }` | `{ matchedCount, modifiedCount }` |
| deleteMany | `{ acknowledged, deletedCount }` | `{ deletedCount }` |

---

### 2. Uniform Single-Document Operations

**StrataDB** has a simple, predictable pattern: ALL "One" methods accept either a string ID or a query filter.

\`\`\`typescript
// All these methods work the same way:
await users.findOne('user-123')                         // By ID
await users.findOne({ email: 'alice@example.com' })     // By filter

await users.updateOne('user-123', { age: 31 })          // By ID
await users.updateOne({ email: 'alice@example.com' }, { age: 31 })  // By filter

await users.deleteOne('user-123')                       // By ID
await users.deleteOne({ status: 'inactive' })           // By filter

await users.findOneAndUpdate('user-123', { age: 31 })   // By ID
await users.findOneAndUpdate({ email: 'alice@example.com' }, { age: 31 })  // By filter
\`\`\`

**Pattern:** If the method name contains "One", it accepts `string | QueryFilter<T>`.

**MongoDB** requires different approaches or methods for ID vs field-based lookups.

---

### 3. No Update Operators

**MongoDB** has `$set`, `$inc`, `$push`, `$unset`, `$pull`, etc.

**StrataDB** uses direct field updates:

\`\`\`typescript
// StrataDB - Direct update
await users.updateOne(userId, {
  age: 31,
  status: 'active'
})

// MongoDB syntax NOT supported:
// ❌ { $set: { age: 31 } }
// ❌ { $inc: { loginCount: 1 } }
// ❌ { $push: { tags: 'new-tag' } }
\`\`\`

**Rationale:** These operators add complexity with little benefit for SQLite. For complex atomic updates, use transactions:

\`\`\`typescript
await db.execute(async (tx) => {
  const users = tx.collection('users', userSchema)
  const user = await users.findById(userId)
  await users.updateOne(userId, {
    loginCount: user.loginCount + 1,
    lastLogin: Date.now()
  })
})
\`\`\`

---

### 4. String Pattern Matching (No Regex)

**MongoDB** supports `$regex` with full regex syntax.

**StrataDB** provides SQL-based string operators:

\`\`\`typescript
// Use these instead of $regex:
await users.find({ name: { $like: '%john%' } })           // Contains
await users.find({ email: { $startsWith: 'admin@' } })    // Starts with
await users.find({ domain: { $endsWith: '.com' } })       // Ends with
\`\`\`

**Rationale:** SQLite's `REGEXP` requires native code. String operators cover 95% of use cases and work out of the box.

---

### 5. Schema-Defined Indexes

**StrataDB** indexes are defined in the schema at collection creation:

\`\`\`typescript
const userSchema = createSchema<User>()
  .field('email', { type: 'TEXT', indexed: true, unique: true })
  .field('age', { type: 'INTEGER', indexed: true })
  .build()
\`\`\`

**MongoDB** supports runtime index creation with `createIndex()`.

**Rationale:** Schema-first approach ensures indexes exist from the start and prevents schema drift.

---

## ❌ What's Not Supported

These MongoDB features are explicitly **NOT** implemented:

### Complex Update Operators
- `$inc`, `$mul`, `$min`, `$max` - Use direct updates or transactions
- `$unset` - Use `updateOne` with `undefined` value
- `$push`, `$pull`, `$addToSet` - Manage arrays in application code
- `$bit`, `$currentDate` - Use application logic

### Aggregation Pipeline
- `$group`, `$lookup`, `$unwind`, etc. - Use SQLite's SQL for complex queries
- Use `db.exec()` for raw SQL when needed

### Advanced Query Operators
- `$regex` - Use `$like`, `$startsWith`, `$endsWith` instead
- `$text` - Use SQLite FTS5 extension if needed
- `$where` - Use query filters or raw SQL

### Distributed Database Features
- Change streams (`watch()`) - Not applicable to local database
- Read/write concerns - Not applicable to SQLite
- Replica sets - Not applicable to single file database
- Sharding - Not applicable to SQLite

### Other Features
- GridFS - Use filesystem or blob storage
- Geospatial queries - Use PostGIS/SpatiaLite if needed
- Capped collections - Not applicable to SQLite
- Server-side JavaScript - Not applicable
- Atlas Search - Not applicable

---

## 🎯 Migration Tips

### Coming from MongoDB?

1. **Remove `$set`** - Just pass the fields directly:
   \`\`\`typescript
   // MongoDB: { $set: { age: 31 } }
   // StrataDB: { age: 31 }
   \`\`\`

2. **Remove `.acknowledged` checks** - If the function returns, it succeeded

3. **Access inserted documents directly**:
   \`\`\`typescript
   // MongoDB: result.insertedId
   // StrataDB: result._id
   \`\`\`

4. **Replace regex with string operators**:
   \`\`\`typescript
   // MongoDB: { name: { $regex: /^john/i } }
   // StrataDB: { name: { $startsWith: 'john' } }
   \`\`\`

5. **Define indexes in schema** - Not at runtime

6. **Use flexible "One" methods**:
   \`\`\`typescript
   // Both work in StrataDB:
   await users.updateOne('user-123', { age: 31 })
   await users.updateOne({ email: 'alice@example.com' }, { age: 31 })
   \`\`\`

---

## 🚀 When to Use StrataDB vs MongoDB

### Use StrataDB when:
- ✅ Building local-first applications
- ✅ Embedding database in desktop/mobile apps
- ✅ Single-user or single-server applications
- ✅ You want MongoDB-like API with SQLite performance
- ✅ You need full ACID guarantees without network latency

### Use MongoDB when:
- ✅ Building distributed systems across multiple servers
- ✅ Need horizontal scaling / sharding
- ✅ Need advanced aggregation pipeline
- ✅ Need change streams for reactive applications
- ✅ Managing massive datasets (100+ GB)

---

## Questions?

See the [FAQ](./faq.md) or [Quick Reference](./quick-reference.md) for more details.
```

---

### 5.2 Update Existing Documentation

#### File: `docs/guide/collections.md`

**Add section: "API Uniformity Pattern"**

```markdown
## API Uniformity Pattern

StrataDB follows a simple, predictable pattern for single-document operations:

### The Rule: All "One" Methods Are Flexible

**Every method with "One" in its name accepts EITHER:**
- A string document ID, OR
- A query filter object

This applies to:
- `findOne(string | QueryFilter<T>)`
- `updateOne(string | QueryFilter<T>, ...)`
- `deleteOne(string | QueryFilter<T>)`
- `replaceOne(string | QueryFilter<T>, ...)`
- `findOneAndUpdate(string | QueryFilter<T>, ...)`
- `findOneAndDelete(string | QueryFilter<T>)`
- `findOneAndReplace(string | QueryFilter<T>, ...)`

### Examples

\`\`\`typescript
// By ID (convenient when you have the ID)
await users.findOne('user-123')
await users.updateOne('user-123', { age: 31 })
await users.deleteOne('user-123')

// By filter (when you need to query by other fields)
await users.findOne({ email: 'alice@example.com' })
await users.updateOne({ email: 'alice@example.com' }, { age: 31 })
await users.deleteOne({ status: 'inactive' })

// Both syntaxes work identically
\`\`\`

### When to Use Each

- **String ID:** When you already have the document ID (most common)
- **Query filter:** When you need to find by other fields

### Pattern Benefits

✅ **Predictable:** All "One" methods work the same way
✅ **Convenient:** Use whichever form makes sense
✅ **Backwards compatible:** Existing code keeps working
✅ **Type-safe:** TypeScript ensures correct usage

---

## Return Types

StrataDB return types are simpler than MongoDB's because SQLite is local and ACID:

| Method | Return Type | Notes |
|--------|-------------|-------|
| `insertOne` | `T` | Returns the document directly |
| `insertMany` | `{ documents: T[], insertedCount: number }` | No `acknowledged` field |
| `updateOne` | `T \| null` | Returns updated document |
| `updateMany` | `{ matchedCount: number, modifiedCount: number }` | No `acknowledged` field |
| `deleteOne` | `boolean` | True if deleted |
| `deleteMany` | `{ deletedCount: number }` | No `acknowledged` field |

**Why no `acknowledged` field?**

MongoDB needs this because operations can fail to reach the server. SQLite is local - if a function returns without throwing, the operation succeeded. The field provides no information.

See [MongoDB Differences](./mongodb-differences.md) for details.
```

---

#### File: `docs/guide/getting-started.md`

**Update quick-start example:**

```typescript
// Insert a user - returns document directly
const user = await users.insertOne({
  name: 'Alice',
  email: 'alice@example.com',
  age: 30,
  role: 'admin'
})
console.log('Inserted user:', user._id)  // ✅ Direct access

// Find users
const admins = await users.find({ role: 'admin' })
const alice = await users.findOne({ email: 'alice@example.com' })

// Update - accepts ID or filter
await users.updateOne(user._id, { age: 31 })  // By ID
await users.updateOne({ email: 'alice@example.com' }, { age: 31 })  // By filter

// Delete - accepts ID or filter
await users.deleteOne(user._id)  // Returns boolean
await users.deleteOne({ status: 'inactive' })  // By filter

// Atomic operations
const updated = await users.findOneAndUpdate(
  { email: 'alice@example.com' },
  { loginCount: 5 },
  { returnDocument: 'after' }
)

// Utilities
const roles = await users.distinct('role')
const total = await users.estimatedDocumentCount()
```

---

#### File: `docs/guide/quick-reference.md`

**Add section: "Atomic Find-and-Modify Operations"**

```markdown
## Atomic Find-and-Modify Operations

All atomic methods accept either string ID or query filter.

\`\`\`typescript
// Find and delete, return deleted doc
const deleted = await users.findOneAndDelete('user-123')
const deleted = await users.findOneAndDelete(
  { status: 'inactive' },
  { sort: { createdAt: 1 } }
)

// Find and update, return before/after
const updated = await users.findOneAndUpdate(
  'user-123',
  { loginCount: 5 },
  { returnDocument: 'after' }
)
const updated = await users.findOneAndUpdate(
  { email: 'alice@example.com' },
  { loginCount: 5 },
  { returnDocument: 'after' }
)

// Find and replace entire document
const replaced = await users.findOneAndReplace(
  'user-123',
  { name: 'New Name', email: 'new@example.com', age: 30 }
)
const replaced = await users.findOneAndReplace(
  { email: 'old@example.com' },
  { name: 'New Name', email: 'new@example.com', age: 30 }
)
\`\`\`

## Utility Methods

\`\`\`typescript
// Get unique values for a field
const roles = await users.distinct('role')  // ['admin', 'user', 'moderator']
const activeRoles = await users.distinct('role', { status: 'active' })

// Fast total count (no filter)
const total = await users.estimatedDocumentCount()

// Drop collection (⚠️ irreversible!)
await users.drop()
\`\`\`
```

---

#### File: `README.md`

**Update features section:**

```markdown
## Features

- 🎯 **MongoDB-like API** - Familiar query syntax with improvements
- 🔄 **Uniform "One" Methods** - All single-document operations accept ID or filter
- 🔒 **Type-safe** - Full TypeScript support with type inference
- ⚡ **Fast** - SQLite performance with query caching (23-70% faster)
- 🎨 **Flexible Schema** - Optional validation with Standard Schema
- 🔍 **Rich Queries** - Filters, sorting, pagination, projections
- 🔄 **Transactions** - Full ACID guarantees
- 📦 **Atomic Operations** - findOneAndUpdate, findOneAndDelete, findOneAndReplace
- 🛠️ **Utilities** - distinct(), estimatedDocumentCount(), drop()
- 🌐 **Works Everywhere** - Node.js, Bun, Deno (via Bun's SQLite)

**Not MongoDB:** See [differences](./docs/guide/mongodb-differences.md)
```

**Update quick example:**

```typescript
import { StrataDB, createSchema, type Document } from 'stratadb'

// Define your document type
type User = Document<{
  name: string
  email: string
  age: number
}>

// Create schema
const userSchema = createSchema<User>()
  .field('name', { type: 'TEXT', indexed: true })
  .field('email', { type: 'TEXT', indexed: true, unique: true })
  .field('age', { type: 'INTEGER' })
  .build()

// Open database
const db = new StrataDB('myapp.db')
const users = db.collection('users', userSchema)

// Insert - returns document directly
const user = await users.insertOne({
  name: 'Alice',
  email: 'alice@example.com',
  age: 30
})
console.log('Inserted:', user._id)

// Query
const admins = await users.find({ role: 'admin' })

// ALL "One" methods accept ID or filter
await users.findOne('user-123')  // By ID
await users.findOne({ email: 'alice@example.com' })  // By filter

await users.updateOne('user-123', { age: 31 })  // By ID
await users.updateOne({ email: 'alice@example.com' }, { age: 31 })  // By filter

await users.deleteOne('user-123')  // By ID
await users.deleteOne({ status: 'inactive' })  // By filter

// Atomic operations
const updated = await users.findOneAndUpdate(
  { email: 'alice@example.com' },
  { age: 31 },
  { returnDocument: 'after' }
)

// Utilities
const roles = await users.distinct('role')
const total = await users.estimatedDocumentCount()

// Cleanup
db.close()
```

---

## Part 6: Benchmark Migrations

### File Changes Required

#### 6.1 File: `bench/crud-operations.bench.ts`

**Current issues:**
1. Line 78: Uses `.document._id` pattern (needs update)
2. No benchmarks for new methods
3. Should add benchmarks for uniform filter support

**Required changes:**

**Change 1: Fix insertOne usage (Line 70-80):**

```typescript
// BEFORE (Line 70-80)
async function seedDatabase(
  db: StrataDBClass,
  count: number
): Promise<string[]> {
  const users = db.collection("users", userSchema)
  const ids: string[] = []
  for (let i = 0; i < count; i += 1) {
    const result = await users.insertOne(generateUser())
    ids.push(result.document._id)  // ❌ OLD API
  }
  return ids
}

// AFTER
async function seedDatabase(
  db: StrataDBClass,
  count: number
): Promise<string[]> {
  const users = db.collection("users", userSchema)
  const ids: string[] = []
  for (let i = 0; i < count; i += 1) {
    const user = await users.insertOne(generateUser())
    ids.push(user._id)  // ✅ NEW API
  }
  return ids
}
```

**Change 2: Add benchmarks for uniform filter support (Add after Line 200):**

```typescript
// ============================================================================
// UNIFORM FILTER SUPPORT - String vs Filter Performance
// ============================================================================

group("Uniform Filter Support - Performance Comparison", () => {
  bench("findOne - by string ID", async () => {
    await smallUsers.findOne(smallIds[50])
  })

  bench("findOne - by _id filter object", async () => {
    await smallUsers.findOne({ _id: smallIds[50] })
  })

  bench("findOne - by indexed field filter", async () => {
    await smallUsers.findOne({ role: "admin" })
  })

  bench("updateOne - by string ID", async () => {
    const db = createTestDb()
    const users = db.collection("users", userSchema)
    const user = await users.insertOne(generateUser())
    await users.updateOne(user._id, { score: 999 })
    db.close()
  })

  bench("updateOne - by filter", async () => {
    const db = createTestDb()
    const users = db.collection("users", userSchema)
    await users.insertOne(generateUser())
    await users.updateOne({ role: "admin" }, { score: 999 })
    db.close()
  })

  bench("deleteOne - by string ID", async () => {
    const db = createTestDb()
    const users = db.collection("users", userSchema)
    const user = await users.insertOne(generateUser())
    await users.deleteOne(user._id)
    db.close()
  })

  bench("deleteOne - by filter", async () => {
    const db = createTestDb()
    const users = db.collection("users", userSchema)
    await users.insertOne(generateUser())
    await users.deleteOne({ role: "admin" })
    db.close()
  })
})
```

**Change 3: Add benchmarks for new atomic methods (Add after Line 270):**

```typescript
// ============================================================================
// ATOMIC FIND-AND-MODIFY OPERATIONS
// ============================================================================

group("Atomic Operations", () => {
  bench("findOneAndDelete - by string ID", async () => {
    const db = createTestDb()
    const users = db.collection("users", userSchema)
    const user = await users.insertOne(generateUser())
    await users.findOneAndDelete(user._id)
    db.close()
  })

  bench("findOneAndDelete - by filter", async () => {
    const db = createTestDb()
    const users = db.collection("users", userSchema)
    await users.insertOne(generateUser())
    await users.findOneAndDelete({ role: "admin" })
    db.close()
  })

  bench("findOneAndUpdate - returnDocument: after", async () => {
    const db = createTestDb()
    const users = db.collection("users", userSchema)
    const user = await users.insertOne(generateUser())
    await users.findOneAndUpdate(
      user._id,
      { score: 999 },
      { returnDocument: 'after' }
    )
    db.close()
  })

  bench("findOneAndUpdate - returnDocument: before", async () => {
    const db = createTestDb()
    const users = db.collection("users", userSchema)
    const user = await users.insertOne(generateUser())
    await users.findOneAndUpdate(
      user._id,
      { score: 999 },
      { returnDocument: 'before' }
    )
    db.close()
  })

  bench("findOneAndReplace", async () => {
    const db = createTestDb()
    const users = db.collection("users", userSchema)
    const user = await users.insertOne(generateUser())
    await users.findOneAndReplace(user._id, generateUser())
    db.close()
  })
})
```

**Change 4: Add benchmarks for utility methods (Add after Line 340):**

```typescript
// ============================================================================
// UTILITY METHODS
// ============================================================================

group("Utility Methods - Small Dataset (100 docs)", () => {
  bench("distinct - indexed field", async () => {
    await smallUsers.distinct('role')
  })

  bench("distinct - non-indexed field", async () => {
    await smallUsers.distinct('status')
  })

  bench("distinct - with filter", async () => {
    await smallUsers.distinct('role', { status: 'active' })
  })

  bench("estimatedDocumentCount", async () => {
    await smallUsers.estimatedDocumentCount()
  })

  bench("count({}) vs estimatedDocumentCount", async () => {
    await smallUsers.count({})
  })
})

group("Utility Methods - Medium Dataset (1000 docs)", () => {
  bench("distinct - indexed field", async () => {
    await mediumUsers.distinct('role')
  })

  bench("distinct - with complex filter", async () => {
    await mediumUsers.distinct('role', {
      $and: [
        { status: 'active' },
        { age: { $gte: 30 } }
      ]
    })
  })

  bench("estimatedDocumentCount", async () => {
    await mediumUsers.estimatedDocumentCount()
  })
})
```

**Summary of bench/crud-operations.bench.ts changes:**
- Line 78: Fix `.document._id` → `._id`
- ~Line 200: Add uniform filter support benchmarks (~50 lines)
- ~Line 270: Add atomic operations benchmarks (~55 lines)
- ~Line 340: Add utility methods benchmarks (~45 lines)

**Total benchmark additions:** ~150 lines

---

### 6.2 Create New Benchmark File (Optional)

**File:** `bench/api-uniformity.bench.ts` (NEW FILE - Optional)

This would be a dedicated benchmark comparing string ID vs filter performance across all operations.

```typescript
/**
 * API Uniformity Benchmarks
 *
 * Compares performance of string ID vs filter for all "One" methods.
 * Validates that the uniform API doesn't introduce performance penalties.
 *
 * Run with: bun bench/api-uniformity.bench.ts
 */

import { bench, group, run } from "mitata"
import type { Document } from "../src/core-types.js"
import { createSchema } from "../src/schema-builder.js"
import { StrataDBClass } from "../src/stratadb.js"

type User = Document<{
  name: string
  email: string
  age: number
  role: string
}>

const userSchema = createSchema<User>()
  .field("name", { type: "TEXT", indexed: true })
  .field("email", { type: "TEXT", indexed: true, unique: true })
  .field("age", { type: "INTEGER", indexed: true })
  .field("role", { type: "TEXT", indexed: true })
  .build()

// Setup
const db = new StrataDBClass({ database: ":memory:" })
const users = db.collection("users", userSchema)
let testUserId: string

// Seed database
const user = await users.insertOne({
  name: "Test User",
  email: "test@example.com",
  age: 30,
  role: "admin"
})
testUserId = user._id

// ============================================================================
// BENCHMARKS
// ============================================================================

group("findOne: String ID vs Filter", () => {
  bench("findOne(string)", async () => {
    await users.findOne(testUserId)
  })

  bench("findOne({ _id: string })", async () => {
    await users.findOne({ _id: testUserId })
  })

  bench("findOne({ indexedField: value })", async () => {
    await users.findOne({ email: "test@example.com" })
  })
})

group("updateOne: String ID vs Filter", () => {
  bench("updateOne(string, update)", async () => {
    await users.updateOne(testUserId, { age: 31 })
  })

  bench("updateOne({ _id: string }, update)", async () => {
    await users.updateOne({ _id: testUserId }, { age: 31 })
  })

  bench("updateOne({ indexedField: value }, update)", async () => {
    await users.updateOne({ email: "test@example.com" }, { age: 31 })
  })
})

group("deleteOne: String ID vs Filter", () => {
  bench("deleteOne(string) - setup & teardown", async () => {
    const temp = await users.insertOne({ name: "Temp", email: `temp-${Date.now()}@example.com`, age: 25, role: "user" })
    await users.deleteOne(temp._id)
  })

  bench("deleteOne({ _id: string }) - setup & teardown", async () => {
    const temp = await users.insertOne({ name: "Temp", email: `temp-${Date.now()}@example.com`, age: 25, role: "user" })
    await users.deleteOne({ _id: temp._id })
  })

  bench("deleteOne({ indexedField: value }) - setup & teardown", async () => {
    const email = `temp-${Date.now()}@example.com`
    await users.insertOne({ name: "Temp", email, age: 25, role: "user" })
    await users.deleteOne({ email })
  })
})

await run({
  avg: true,
  json: false,
  colors: true,
  min_max: true,
  percentiles: true,
})

// Cleanup
db.close()
```

---

## Implementation Order

### Phase 1: Type Simplification (Day 1 - Morning)
**Priority:** Critical - Foundation for everything else

1. Update type definitions in `src/collection-types.ts`
   - Deprecate `InsertOneResult<T>`, make it type alias to `T`
   - Remove `acknowledged` from `InsertManyResult`, `UpdateResult`, `DeleteResult`
   - Update all JSDoc examples

2. Update `src/sqlite-collection.ts` implementation
   - Change `insertOne` to return `T` directly
   - Remove `acknowledged` from other methods

3. Run TypeScript compiler to find all type errors
   - Fix compilation errors in source files

**Estimated time:** 3-4 hours

---

### Phase 2: Test Updates (Day 1 - Afternoon)
**Priority:** Critical - Ensure correctness

1. Update test files in order:
   - `test/integration/collection-crud.test.ts` (16 occurrences)
   - `test/integration/transactions.test.ts` (9 occurrences)
   - `test/integration/standard-schema-validators.test.ts` (12 occurrences)
   - `test/integration/symbol-dispose.test.ts` (1 occurrence)
   - `test/integration/batch-operations.test.ts` (3 `.acknowledged`)

2. Update benchmarks:
   - `bench/crud-operations.bench.ts` (Line 78 fix)

3. Run full test suite: `bun test`
4. Run benchmarks: `bun run bench/crud-operations.bench.ts`

**Estimated time:** 4-5 hours

---

### Phase 3: Uniform Filter Support (Day 2 - Morning)
**Priority:** High - Core API improvement

1. Add `normalizeFilter()` helper to `src/sqlite-collection.ts`
2. Update signatures in `src/collection-types.ts` (all "One" methods)
3. Update implementations:
   - `findOne`
   - `updateOne`
   - `deleteOne`
   - `replaceOne`
4. Add new tests for uniform filter support
5. Update JSDoc examples
6. Run tests

**Estimated time:** 3-4 hours

---

### Phase 4: New MongoDB Methods (Day 2 - Afternoon)
**Priority:** High - Expected features

1. Implement `findOneAndDelete`:
   - Type definition
   - Implementation
   - Tests
   - JSDoc

2. Implement `findOneAndUpdate`:
   - Type definition
   - Implementation
   - Tests
   - JSDoc

3. Implement `findOneAndReplace`:
   - Type definition
   - Implementation
   - Tests
   - JSDoc

4. Run full test suite

**Estimated time:** 4-5 hours

---

### Phase 5: Utility Methods (Day 3 - Morning)
**Priority:** Medium - Nice to have

1. Implement `distinct()`
2. Implement `estimatedDocumentCount()`
3. Implement `drop()`
4. Add tests for all three
5. Run tests

**Estimated time:** 3-4 hours

---

### Phase 6: Benchmarks (Day 3 - Afternoon)
**Priority:** Medium - Performance validation

1. Update `bench/crud-operations.bench.ts`:
   - Fix `.document._id` usage
   - Add uniform filter benchmarks
   - Add atomic operations benchmarks
   - Add utility methods benchmarks

2. Optionally create `bench/api-uniformity.bench.ts`

3. Run all benchmarks and verify no regressions

**Estimated time:** 2-3 hours

---

### Phase 7: Documentation (Day 3-4)
**Priority:** High - Critical for release

1. Create `docs/guide/mongodb-differences.md`
2. Update `docs/guide/collections.md`
3. Update `docs/guide/getting-started.md`
4. Update `docs/guide/quick-reference.md`
5. Update `README.md`
6. Regenerate API docs: `npx typedoc`
7. Review all documentation for consistency

**Estimated time:** 5-6 hours

---

### Phase 8: Final Testing & Cleanup (Day 4)
**Priority:** Critical - Release readiness

1. Run full test suite: `bun test`
2. Run all benchmarks
3. Test against example applications
4. Review all changed files
5. Check for console.log / debugging code
6. Verify no TypeScript errors
7. Final documentation review
8. Create git commit with clear message

**Estimated time:** 3-4 hours

---

## Testing Strategy

### Unit Tests
- All existing unit tests should pass without changes
- No unit test changes needed (no implementation details changed)

### Integration Tests
**Files requiring updates:**
- `test/integration/collection-crud.test.ts` - Major updates + new tests
- `test/integration/batch-operations.test.ts` - Minor updates
- `test/integration/transactions.test.ts` - Minor updates
- `test/integration/standard-schema-validators.test.ts` - Minor updates
- `test/integration/symbol-dispose.test.ts` - Minor updates

**New test sections to add:**
- Uniform filter support (~140 lines)
- findOneAndDelete (~60 lines)
- findOneAndUpdate (~110 lines)
- findOneAndReplace (~85 lines)
- distinct (~75 lines)
- estimatedDocumentCount (~35 lines)
- drop (~30 lines)

**Total new test code:** ~535 lines

### Benchmarks
**Files requiring updates:**
- `bench/crud-operations.bench.ts` - Fix + additions (~155 lines)
- `bench/api-uniformity.bench.ts` - Optional new file (~80 lines)

---

## Success Criteria

### Code Quality
- ✅ All TypeScript compilation passes with no errors
- ✅ All tests pass (existing + new)
- ✅ Code coverage maintained or improved
- ✅ No eslint/biome warnings
- ✅ All JSDoc complete and accurate

### Functionality
- ✅ `insertOne` returns `T` directly (not wrapped)
- ✅ No `acknowledged` fields in any result types
- ✅ ALL "One" methods accept `string | QueryFilter<T>` uniformly
- ✅ All existing code continues to work (backwards compatible)
- ✅ New `findOneAnd*` methods work correctly with both string and filter
- ✅ Utility methods (`distinct`, `estimatedDocumentCount`, `drop`) work correctly

### API Uniformity
- ✅ All seven "One" methods have identical filter parameter signature
- ✅ `normalizeFilter()` helper works correctly
- ✅ String IDs are optimized (direct lookup when possible)
- ✅ Filter objects work correctly (translate to SQL)

### Documentation
- ✅ All API docs regenerated and accurate
- ✅ MongoDB differences guide complete and clear
- ✅ All examples updated to show uniform API
- ✅ README reflects new capabilities
- ✅ Migration guide included

### Performance
- ✅ Benchmarks show no regression
- ✅ String ID path is optimized
- ✅ Filter path performs well
- ✅ New methods perform well

---

## File Change Summary

### Source Files Changed: 2
- `src/collection-types.ts` - Type definitions, all "One" method signatures, JSDoc
- `src/sqlite-collection.ts` - All implementations + `normalizeFilter()` helper

### Test Files Changed: 5
- `test/integration/collection-crud.test.ts` - Major changes + ~535 lines new tests
- `test/integration/batch-operations.test.ts` - Minor updates
- `test/integration/transactions.test.ts` - Minor updates
- `test/integration/standard-schema-validators.test.ts` - Minor updates
- `test/integration/symbol-dispose.test.ts` - Minor updates

### Benchmark Files Changed: 1-2
- `bench/crud-operations.bench.ts` - Fix + ~155 lines additions
- `bench/api-uniformity.bench.ts` - Optional new file (~80 lines)

### Documentation Files Changed: 6
- `docs/guide/mongodb-differences.md` - NEW FILE (~350 lines)
- `docs/guide/collections.md` - Major additions
- `docs/guide/getting-started.md` - Updates
- `docs/guide/quick-reference.md` - Updates
- `docs/api/index.md` - Updates
- `README.md` - Major updates

### Auto-Generated Files: ~10
- All files in `docs/api/` will be regenerated by TypeDoc

**Total files touched:** ~26 files
**Total estimated changes:** ~950 locations

---

## Risk Assessment

### Low Risk
- Type changes (TypeScript will catch errors)
- Adding new methods (pure additions)
- Test updates (verification only)
- Documentation (no code impact)
- Benchmark updates (no runtime impact)

### Medium Risk
- Changing `insertOne` return type (breaking, but TypeScript catches it)
- Removing `acknowledged` fields (could break code that checks it)
- Adding `string | QueryFilter<T>` support (runtime branching)

### Mitigation
- Comprehensive test coverage (existing + new ~535 lines)
- TypeScript compilation will catch all breaking changes
- `normalizeFilter()` helper is simple and testable
- Clear migration guide for users
- Deprecation warnings in types
- Performance benchmarks validate no regressions

---

## Post-Implementation Checklist

Before marking as complete:

- [ ] All TypeScript compiles with no errors
- [ ] `bun test` passes 100%
- [ ] All benchmarks complete successfully
- [ ] `npx typedoc` generates docs without errors
- [ ] All new methods have complete JSDoc with examples
- [ ] MongoDB differences guide is complete and accurate
- [ ] README updated with uniform API pattern
- [ ] All examples in docs show both string and filter usage
- [ ] No debug code (console.log, etc.) remains
- [ ] Git commit messages are clear
- [ ] CHANGELOG.md updated (if exists)
- [ ] Performance benchmarks show no regression
- [ ] All "One" methods tested with both string and filter

---

## Release Notes Template

```markdown
# StrataDB v1.0.0

## 🎉 Major Changes

### 1. Simplified Return Types

**`insertOne` now returns document directly:**
\`\`\`typescript
// Before
const result = await users.insertOne({ name: 'Alice' })
console.log(result.document._id)

// After
const user = await users.insertOne({ name: 'Alice' })
console.log(user._id)
\`\`\`

**Removed `acknowledged` field from all results:**
- `InsertManyResult` - Now `{ documents, insertedCount }`
- `UpdateResult` - Now `{ matchedCount, modifiedCount }`
- `DeleteResult` - Now `{ deletedCount }`

**Why?** SQLite is local and ACID. If a function returns, the operation succeeded. The `acknowledged` field provided zero information.

---

### 2. Uniform API for Single-Document Operations

**ALL "One" methods now accept either string ID or query filter:**

\`\`\`typescript
// Every "One" method works the same way:
await users.findOne('user-123')                         // By ID
await users.findOne({ email: 'alice@example.com' })     // By filter

await users.updateOne('user-123', { age: 31 })          // By ID
await users.updateOne({ email: 'alice@example.com' }, { age: 31 })  // By filter

await users.deleteOne('user-123')                       // By ID
await users.deleteOne({ status: 'inactive' })           // By filter

await users.findOneAndUpdate('user-123', { age: 31 })   // By ID
await users.findOneAndUpdate({ email: 'alice@example.com' }, { age: 31 })  // By filter
\`\`\`

**Benefits:**
- ✅ Zero cognitive load - all "One" methods work identically
- ✅ Maximum convenience - use whichever form makes sense
- ✅ Fully backwards compatible - existing code keeps working
- ✅ Type-safe - TypeScript ensures correct usage

---

## 🚀 New Features

### Atomic Find-and-Modify Operations

All accept either string ID or query filter:

\`\`\`typescript
// Find and delete, return deleted doc
const deleted = await users.findOneAndDelete('user-123')
const deleted = await users.findOneAndDelete(
  { status: 'inactive' },
  { sort: { createdAt: 1 } }
)

// Find and update, return before/after state
const updated = await users.findOneAndUpdate(
  'user-123',
  { loginCount: 5 },
  { returnDocument: 'after' }
)

// Find and replace entire document
const replaced = await users.findOneAndReplace(
  { email: 'old@example.com' },
  { name: 'New Name', email: 'new@example.com', age: 30 }
)
\`\`\`

### Utility Methods
- `distinct(field, filter?)` - Get unique values for a field
- `estimatedDocumentCount()` - Fast total count without filter
- `drop()` - Drop entire collection

---

## 📚 Documentation

- New [MongoDB Differences Guide](./docs/guide/mongodb-differences.md)
- New [API Uniformity Pattern Guide](./docs/guide/collections.md#api-uniformity-pattern)
- Updated all examples to show string and filter usage
- Complete API reference

---

## 🔧 Under the Hood

- Query caching improvements (23-70% faster repeated queries)
- Optimized SQL generation for string ID lookups
- Better TypeScript inference with union types
- Comprehensive benchmarks for uniform API

---

## 📦 Migration Guide

### Update insertOne calls:
\`\`\`typescript
// Before
const result = await users.insertOne({ name: 'Alice' })
const id = result.document._id

// After
const user = await users.insertOne({ name: 'Alice' })
const id = user._id
\`\`\`

### Remove .acknowledged checks:
\`\`\`typescript
// Before
const result = await users.insertMany([...])
if (result.acknowledged) { ... }

// After
const result = await users.insertMany([...])
// If we got here, it succeeded!
\`\`\`

### Use flexible "One" methods:
\`\`\`typescript
// Both work!
await users.updateOne('user-123', { age: 31 })
await users.updateOne({ email: 'alice@example.com' }, { age: 31 })
\`\`\`

---

See [CHANGELOG.md](./CHANGELOG.md) for complete details.
\`\`\`

---

**END OF IMPLEMENTATION PLAN - VERSION 2.0 (FULLY UNIFORM API)**
