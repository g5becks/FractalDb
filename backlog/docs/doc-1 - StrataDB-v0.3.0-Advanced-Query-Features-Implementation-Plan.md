---
id: doc-1
title: StrataDB v0.3.0 - Advanced Query Features Implementation Plan
type: other
created_date: '2025-11-23 07:19'
---
# StrataDB v0.3.0 - Advanced Query Features Implementation Plan

## Overview

This document outlines the implementation plan for adding advanced query features to StrataDB v0.3.0. These features simplify complex queries commonly needed for UI-driven applications (like ag-grid), dashboards, and search interfaces.

## Features to Implement

### Phase 1: String Operator Enhancements (Easy)
1. `$ilike` - Case-insensitive LIKE
2. `$contains` - Sugar for `$like: '%value%'`

### Phase 2: Projection Helpers (Easy)
3. `select` - Include specific fields
4. `omit` - Exclude specific fields

### Phase 3: Text Search (Medium)
5. Multi-field text search option

### Phase 4: Cursor Pagination (Medium)
6. Cursor-based pagination with `after`/`before` cursors

### Phase 5: Aggregation (Hard - Future)
7. `aggregate()` method with `count`, `groupBy`, `sum`, `avg` (deferred to v0.4.0)

### Phase 6: Faceted Search (Hard - Future)
8. `facets()` method for filter UI building (deferred to v0.4.0)

---

## Phase 1: String Operator Enhancements

### Feature 1.1: `$ilike` Operator

**Purpose:** Case-insensitive LIKE pattern matching

**Usage Example:**
```typescript
// Find users with name containing 'alice' (case-insensitive)
await users.find({ name: { $ilike: '%alice%' } })

// Matches: 'Alice', 'ALICE', 'alice', 'AlIcE'
```

**Files to Modify:**

#### 1. `src/query-types.ts`
```typescript
// Add to StringOperator type (line ~151)
export type StringOperator = {
  /** SQL-style LIKE pattern matching (case-sensitive) */
  readonly $like?: string

  /** SQL-style LIKE pattern matching (case-insensitive) */
  readonly $ilike?: string

  /** String starts with the specified prefix */
  readonly $startsWith?: string

  /** String ends with the specified suffix */
  readonly $endsWith?: string

  /** String contains the specified substring (sugar for $like: '%value%') */
  readonly $contains?: string
}
```

#### 2. `src/sqlite-query-translator.ts`
```typescript
// Add to translateSingleOperator method (after $like case, ~line 639)
case "$ilike":
  params.push(this.toBindValue(value))
  return `${fieldSql} LIKE ? COLLATE NOCASE`
```

### Feature 1.2: `$contains` Operator

**Purpose:** Sugar for substring matching without writing `%value%`

**Usage Example:**
```typescript
// Instead of: { name: { $like: '%smith%' } }
await users.find({ name: { $contains: 'smith' } })
```

**Files to Modify:**

#### 1. `src/query-types.ts`
Already covered in 1.1 above.

#### 2. `src/sqlite-query-translator.ts`
```typescript
// Add to translateSingleOperator method (after $ilike case)
case "$contains": {
  const pattern = `%${this.toBindValue(value)}%`
  params.push(pattern)
  return `${fieldSql} LIKE ?`
}
```

### Tests for Phase 1

#### File: `test/unit/string-operators.test.ts` (new)
```typescript
import { describe, expect, it } from "bun:test"
import { SQLiteQueryTranslator } from "../../src/sqlite-query-translator.js"
import { createSchema, type Document } from "../../src/index.js"

type TestDoc = Document<{ name: string; email: string }>

describe("String Operators", () => {
  const schema = createSchema<Omit<TestDoc, "_id" | "createdAt" | "updatedAt">>()
    .field("name", { type: "TEXT", indexed: true })
    .field("email", { type: "TEXT", indexed: true })
    .build()

  const translator = new SQLiteQueryTranslator(schema)

  describe("$ilike operator", () => {
    it("should generate case-insensitive LIKE query", () => {
      const result = translator.translate({ name: { $ilike: "%alice%" } })
      expect(result.sql).toBe("(_name LIKE ? COLLATE NOCASE)")
      expect(result.params).toEqual(["%alice%"])
    })

    it("should match case-insensitively", () => {
      // Integration test with actual DB would verify:
      // 'Alice', 'ALICE', 'alice' all match
    })
  })

  describe("$contains operator", () => {
    it("should wrap value with % wildcards", () => {
      const result = translator.translate({ name: { $contains: "smith" } })
      expect(result.sql).toBe("(_name LIKE ?)")
      expect(result.params).toEqual(["%smith%"])
    })

    it("should handle special characters", () => {
      const result = translator.translate({ email: { $contains: "@example" } })
      expect(result.sql).toBe("(_email LIKE ?)")
      expect(result.params).toEqual(["%@example%"])
    })
  })
})
```

#### File: `test/integration/string-operators.test.ts` (new)
```typescript
import { afterEach, beforeEach, describe, expect, it } from "bun:test"
import { Database } from "bun:sqlite"
import { Strata, createSchema, type Document } from "../../src/index.js"

type User = Document<{ name: string; email: string }>

describe("String Operators Integration", () => {
  let db: Strata
  let users: ReturnType<typeof db.collection<User>>

  beforeEach(() => {
    db = new Strata({ database: ":memory:" })
    const schema = createSchema<Omit<User, "_id" | "createdAt" | "updatedAt">>()
      .field("name", { type: "TEXT", indexed: true })
      .field("email", { type: "TEXT", indexed: true })
      .build()
    users = db.collection("users", schema)

    // Seed test data
    users.insertMany([
      { name: "Alice Smith", email: "alice@example.com" },
      { name: "ALICE JONES", email: "ALICE@EXAMPLE.COM" },
      { name: "Bob Smith", email: "bob@example.com" },
      { name: "Charlie Brown", email: "charlie@test.org" },
    ])
  })

  afterEach(() => {
    db.close()
  })

  describe("$ilike", () => {
    it("should find case-insensitive matches", async () => {
      const results = await users.find({ name: { $ilike: "%alice%" } })
      expect(results).toHaveLength(2)
      expect(results.map(u => u.name)).toContain("Alice Smith")
      expect(results.map(u => u.name)).toContain("ALICE JONES")
    })

    it("should work with $startsWith pattern", async () => {
      const results = await users.find({ name: { $ilike: "alice%" } })
      expect(results).toHaveLength(2)
    })
  })

  describe("$contains", () => {
    it("should find substring matches", async () => {
      const results = await users.find({ name: { $contains: "Smith" } })
      expect(results).toHaveLength(2)
    })

    it("should find email domain matches", async () => {
      const results = await users.find({ email: { $contains: "@example" } })
      expect(results).toHaveLength(3) // alice, ALICE, bob
    })
  })
})
```

---

## Phase 2: Projection Helpers

### Feature 2.1: `select` Option

**Purpose:** Cleaner syntax for including specific fields

**Usage Example:**
```typescript
// Instead of: { projection: { name: 1, email: 1 } }
await users.find({}, { select: ['name', 'email'] })
```

### Feature 2.2: `omit` Option

**Purpose:** Cleaner syntax for excluding specific fields

**Usage Example:**
```typescript
// Instead of: { projection: { password: 0, ssn: 0 } }
await users.find({}, { omit: ['password', 'ssn'] })
```

**Files to Modify:**

#### 1. `src/query-options-types.ts`
```typescript
// Add new types after ProjectionSpec (line ~118)

/**
 * Field selection for query results using array syntax.
 *
 * @typeParam T - The document type being queried
 *
 * @example
 * ```typescript
 * // Include only name and email
 * const options: QueryOptions<User> = {
 *   select: ['name', 'email']
 * }
 * ```
 */
export type SelectSpec<T> = readonly (keyof T)[]

/**
 * Field exclusion for query results using array syntax.
 *
 * @typeParam T - The document type being queried
 *
 * @example
 * ```typescript
 * // Exclude password and ssn
 * const options: QueryOptions<User> = {
 *   omit: ['password', 'ssn']
 * }
 * ```
 */
export type OmitSpec<T> = readonly (keyof T)[]

// Update QueryOptions type to add select and omit
export type QueryOptions<T> = {
  /** Sort order for results (MongoDB-style: 1 = asc, -1 = desc) */
  readonly sort?: SortSpec<T>

  /** Maximum number of documents to return */
  readonly limit?: number

  /** Number of documents to skip (for pagination) */
  readonly skip?: number

  /** Which fields to include (1) or exclude (0) from results */
  readonly projection?: ProjectionSpec<T>

  /** Fields to include in results (alternative to projection) */
  readonly select?: SelectSpec<T>

  /** Fields to exclude from results (alternative to projection) */
  readonly omit?: OmitSpec<T>
}
```

#### 2. `src/sqlite-collection.ts`
```typescript
// Add helper method to convert select/omit to projection
private normalizeProjection(options?: QueryOptions<T>): ProjectionSpec<T> | undefined {
  if (options?.projection) {
    return options.projection
  }

  if (options?.select) {
    const projection: Record<string, 1> = {}
    for (const field of options.select) {
      projection[field as string] = 1
    }
    return projection as ProjectionSpec<T>
  }

  if (options?.omit) {
    const projection: Record<string, 0> = {}
    for (const field of options.omit) {
      projection[field as string] = 0
    }
    return projection as ProjectionSpec<T>
  }

  return undefined
}

// Update find method to use normalizeProjection
// Add projection filtering after fetching results
find(filter: QueryFilter<T>, options?: QueryOptions<T>): Promise<readonly T[]> {
  // ... existing code ...

  const results = rows.map((row) => {
    const doc = JSON.parse(row.body) as Omit<T, "_id">
    return { _id: row._id, ...doc } as T
  })

  // Apply projection
  const projection = this.normalizeProjection(options)
  if (projection) {
    return Promise.resolve(this.applyProjection(results, projection))
  }

  return Promise.resolve(results)
}

// Add projection application helper
private applyProjection(docs: readonly T[], projection: ProjectionSpec<T>): readonly T[] {
  const includeMode = Object.values(projection).some(v => v === 1)

  return docs.map(doc => {
    const result: Record<string, unknown> = {}

    if (includeMode) {
      // Include only specified fields (plus _id by default)
      result._id = doc._id
      for (const [key, value] of Object.entries(projection)) {
        if (value === 1 && key in doc) {
          result[key] = (doc as Record<string, unknown>)[key]
        }
      }
    } else {
      // Exclude specified fields
      for (const [key, value] of Object.entries(doc as Record<string, unknown>)) {
        const exclude = projection[key as keyof T] === 0
        if (!exclude) {
          result[key] = value
        }
      }
    }

    return result as T
  })
}
```

### Tests for Phase 2

#### File: `test/unit/projection-helpers.test.ts` (new)
```typescript
import { describe, expect, it, beforeEach, afterEach } from "bun:test"
import { Strata, createSchema, type Document } from "../../src/index.js"

type User = Document<{
  name: string
  email: string
  password: string
  age: number
}>

describe("Projection Helpers", () => {
  let db: Strata
  let users: ReturnType<typeof db.collection<User>>

  beforeEach(async () => {
    db = new Strata({ database: ":memory:" })
    const schema = createSchema<Omit<User, "_id" | "createdAt" | "updatedAt">>()
      .field("name", { type: "TEXT", indexed: true })
      .field("email", { type: "TEXT", indexed: true })
      .field("password", { type: "TEXT", indexed: false })
      .field("age", { type: "INTEGER", indexed: true })
      .build()
    users = db.collection("users", schema)

    await users.insertOne({
      name: "Alice",
      email: "alice@example.com",
      password: "secret123",
      age: 30
    })
  })

  afterEach(() => {
    db.close()
  })

  describe("select option", () => {
    it("should include only specified fields", async () => {
      const results = await users.find({}, { select: ["name", "email"] })
      expect(results).toHaveLength(1)
      expect(results[0]).toHaveProperty("_id")
      expect(results[0]).toHaveProperty("name", "Alice")
      expect(results[0]).toHaveProperty("email", "alice@example.com")
      expect(results[0]).not.toHaveProperty("password")
      expect(results[0]).not.toHaveProperty("age")
    })

    it("should always include _id", async () => {
      const results = await users.find({}, { select: ["name"] })
      expect(results[0]).toHaveProperty("_id")
    })
  })

  describe("omit option", () => {
    it("should exclude specified fields", async () => {
      const results = await users.find({}, { omit: ["password"] })
      expect(results[0]).toHaveProperty("name")
      expect(results[0]).toHaveProperty("email")
      expect(results[0]).toHaveProperty("age")
      expect(results[0]).not.toHaveProperty("password")
    })

    it("should exclude multiple fields", async () => {
      const results = await users.find({}, { omit: ["password", "age"] })
      expect(results[0]).not.toHaveProperty("password")
      expect(results[0]).not.toHaveProperty("age")
    })
  })

  describe("projection precedence", () => {
    it("should prefer projection over select", async () => {
      const results = await users.find({}, {
        projection: { name: 1 },
        select: ["email"]  // Should be ignored
      })
      expect(results[0]).toHaveProperty("name")
      expect(results[0]).not.toHaveProperty("email")
    })
  })
})
```

---

## Phase 3: Multi-Field Text Search

### Feature 3.1: `search` Query Option

**Purpose:** Search text across multiple fields without building complex `$or` queries

**Usage Example:**
```typescript
// Search for 'AAPL' in name, ticker, and description fields
const results = await alerts.find({}, {
  search: {
    text: 'AAPL',
    fields: ['name', 'watchlistItem.ticker', 'description'],
    caseSensitive: false  // default: false
  }
})

// Equivalent to:
const results = await alerts.find({
  $or: [
    { name: { $ilike: '%AAPL%' } },
    { 'watchlistItem.ticker': { $ilike: '%AAPL%' } },
    { description: { $ilike: '%AAPL%' } }
  ]
})
```

**Files to Modify:**

#### 1. `src/query-options-types.ts`
```typescript
/**
 * Text search specification for multi-field searching.
 *
 * @typeParam T - The document type being queried
 *
 * @example
 * ```typescript
 * const options: QueryOptions<Alert> = {
 *   search: {
 *     text: 'AAPL',
 *     fields: ['name', 'watchlistItem.ticker'],
 *     caseSensitive: false
 *   }
 * }
 * ```
 */
export type TextSearchSpec<T> = {
  /** The text to search for */
  readonly text: string

  /** Fields to search in (supports dot notation for nested fields) */
  readonly fields: readonly (keyof T | string)[]

  /** Whether search is case-sensitive (default: false) */
  readonly caseSensitive?: boolean
}

// Update QueryOptions
export type QueryOptions<T> = {
  // ... existing fields ...

  /** Multi-field text search */
  readonly search?: TextSearchSpec<T>
}
```

#### 2. `src/sqlite-collection.ts`
```typescript
// Add method to build search filter
private buildSearchFilter(search: TextSearchSpec<T>): QueryFilter<T> {
  const operator = search.caseSensitive ? '$like' : '$ilike'
  const pattern = `%${search.text}%`

  const conditions = search.fields.map(field => ({
    [field]: { [operator]: pattern }
  }))

  return { $or: conditions } as QueryFilter<T>
}

// Update find method
async find(filter: QueryFilter<T>, options?: QueryOptions<T>): Promise<readonly T[]> {
  // Merge search into filter if provided
  let effectiveFilter = filter

  if (options?.search) {
    const searchFilter = this.buildSearchFilter(options.search)
    if (Object.keys(filter).length > 0) {
      effectiveFilter = { $and: [filter, searchFilter] } as QueryFilter<T>
    } else {
      effectiveFilter = searchFilter
    }
  }

  // ... rest of existing find implementation using effectiveFilter ...
}
```

### Tests for Phase 3

#### File: `test/integration/text-search.test.ts` (new)
```typescript
import { afterEach, beforeEach, describe, expect, it } from "bun:test"
import { Strata, createSchema, type Document } from "../../src/index.js"

type Alert = Document<{
  name: string
  description: string
  watchlistItem?: {
    ticker: string
    symbol: string
  }
}>

describe("Multi-Field Text Search", () => {
  let db: Strata
  let alerts: ReturnType<typeof db.collection<Alert>>

  beforeEach(async () => {
    db = new Strata({ database: ":memory:" })
    const schema = createSchema<Omit<Alert, "_id" | "createdAt" | "updatedAt">>()
      .field("name", { type: "TEXT", indexed: true })
      .field("description", { type: "TEXT", indexed: false })
      .field("watchlistItem.ticker", { type: "TEXT", indexed: true })
      .field("watchlistItem.symbol", { type: "TEXT", indexed: true })
      .build()
    alerts = db.collection("alerts", schema)

    await alerts.insertMany([
      {
        name: "AAPL Price Alert",
        description: "Alert when Apple stock moves",
        watchlistItem: { ticker: "AAPL", symbol: "Apple Inc" }
      },
      {
        name: "Tech Sector Alert",
        description: "Monitor AAPL and MSFT",
        watchlistItem: { ticker: "QQQ", symbol: "Nasdaq ETF" }
      },
      {
        name: "Dividend Alert",
        description: "High yield stocks",
        watchlistItem: { ticker: "VYM", symbol: "Vanguard Dividend" }
      }
    ])
  })

  afterEach(() => {
    db.close()
  })

  it("should search across multiple fields", async () => {
    const results = await alerts.find({}, {
      search: {
        text: "AAPL",
        fields: ["name", "description", "watchlistItem.ticker"]
      }
    })

    expect(results).toHaveLength(2)
    expect(results.map(a => a.name)).toContain("AAPL Price Alert")
    expect(results.map(a => a.name)).toContain("Tech Sector Alert")
  })

  it("should be case-insensitive by default", async () => {
    const results = await alerts.find({}, {
      search: {
        text: "aapl",
        fields: ["name", "watchlistItem.ticker"]
      }
    })

    expect(results).toHaveLength(2)
  })

  it("should support case-sensitive search", async () => {
    const results = await alerts.find({}, {
      search: {
        text: "aapl",
        fields: ["name", "watchlistItem.ticker"],
        caseSensitive: true
      }
    })

    expect(results).toHaveLength(0)
  })

  it("should combine with existing filters", async () => {
    const results = await alerts.find(
      { "watchlistItem.ticker": "AAPL" },
      {
        search: {
          text: "Price",
          fields: ["name", "description"]
        }
      }
    )

    expect(results).toHaveLength(1)
    expect(results[0].name).toBe("AAPL Price Alert")
  })
})
```

---

## Phase 4: Cursor-Based Pagination

### Feature 4.1: Cursor Pagination

**Purpose:** More efficient pagination for large datasets (better than skip/limit)

**Usage Example:**
```typescript
// First page
const page1 = await users.find({}, {
  sort: { createdAt: -1 },
  limit: 20
})

// Get cursor from last item
const lastItem = page1[page1.length - 1]

// Next page using cursor
const page2 = await users.find({}, {
  sort: { createdAt: -1 },
  limit: 20,
  cursor: {
    after: lastItem._id,
    sortField: 'createdAt',
    sortValue: lastItem.createdAt
  }
})
```

**Files to Modify:**

#### 1. `src/query-options-types.ts`
```typescript
/**
 * Cursor specification for cursor-based pagination.
 *
 * @remarks
 * Cursor-based pagination is more efficient than skip/limit for large datasets
 * because it doesn't require scanning skipped rows.
 *
 * @example
 * ```typescript
 * // After getting first page, use last item as cursor
 * const nextPage = await users.find({}, {
 *   sort: { createdAt: -1 },
 *   limit: 20,
 *   cursor: {
 *     after: lastItem._id,
 *     sortField: 'createdAt',
 *     sortValue: lastItem.createdAt
 *   }
 * })
 * ```
 */
export type CursorSpec = {
  /** Document ID to start after (forward pagination) */
  readonly after?: string

  /** Document ID to start before (backward pagination) */
  readonly before?: string

  /** The sort field being used for cursor comparison */
  readonly sortField: string

  /** The value of the sort field at the cursor position */
  readonly sortValue: unknown
}

// Update QueryOptions
export type QueryOptions<T> = {
  // ... existing fields ...

  /** Cursor for cursor-based pagination (alternative to skip) */
  readonly cursor?: CursorSpec
}
```

#### 2. `src/sqlite-collection.ts`
```typescript
// Add method to build cursor filter
private buildCursorFilter(cursor: CursorSpec, sort: SortSpec<T>): QueryFilter<T> {
  const sortField = cursor.sortField as keyof T
  const sortDirection = sort[sortField] ?? 1

  // For ascending sort, get items AFTER the cursor (greater than)
  // For descending sort, get items AFTER the cursor (less than)
  const operator = sortDirection === 1 ? '$gt' : '$lt'

  // Use compound condition: (sortField > cursorValue) OR (sortField = cursorValue AND _id > cursorId)
  // This handles ties in the sort field
  if (cursor.after) {
    return {
      $or: [
        { [sortField]: { [operator]: cursor.sortValue } },
        {
          $and: [
            { [sortField]: cursor.sortValue },
            { _id: { [sortDirection === 1 ? '$gt' : '$lt']: cursor.after } }
          ]
        }
      ]
    } as QueryFilter<T>
  }

  if (cursor.before) {
    const reverseOp = sortDirection === 1 ? '$lt' : '$gt'
    return {
      $or: [
        { [sortField]: { [reverseOp]: cursor.sortValue } },
        {
          $and: [
            { [sortField]: cursor.sortValue },
            { _id: { [sortDirection === 1 ? '$lt' : '$gt']: cursor.before } }
          ]
        }
      ]
    } as QueryFilter<T>
  }

  return {} as QueryFilter<T>
}

// Update find method to handle cursor
async find(filter: QueryFilter<T>, options?: QueryOptions<T>): Promise<readonly T[]> {
  let effectiveFilter = filter

  // Apply cursor if provided
  if (options?.cursor && options.sort) {
    const cursorFilter = this.buildCursorFilter(options.cursor, options.sort)
    if (Object.keys(filter).length > 0) {
      effectiveFilter = { $and: [filter, cursorFilter] } as QueryFilter<T>
    } else {
      effectiveFilter = cursorFilter
    }
  }

  // ... rest of implementation ...
}
```

### Tests for Phase 4

#### File: `test/integration/cursor-pagination.test.ts` (new)
```typescript
import { afterEach, beforeEach, describe, expect, it } from "bun:test"
import { Strata, createSchema, type Document } from "../../src/index.js"

type User = Document<{
  name: string
  score: number
}>

describe("Cursor-Based Pagination", () => {
  let db: Strata
  let users: ReturnType<typeof db.collection<User>>

  beforeEach(async () => {
    db = new Strata({ database: ":memory:" })
    const schema = createSchema<Omit<User, "_id" | "createdAt" | "updatedAt">>()
      .field("name", { type: "TEXT", indexed: true })
      .field("score", { type: "INTEGER", indexed: true })
      .build()
    users = db.collection("users", schema)

    // Insert 50 users with sequential scores
    const docs = Array.from({ length: 50 }, (_, i) => ({
      name: `User ${i + 1}`,
      score: (i + 1) * 10
    }))
    await users.insertMany(docs)
  })

  afterEach(() => {
    db.close()
  })

  describe("forward pagination", () => {
    it("should paginate through results using cursor", async () => {
      // First page
      const page1 = await users.find({}, {
        sort: { score: 1 },
        limit: 10
      })

      expect(page1).toHaveLength(10)
      expect(page1[0].score).toBe(10)
      expect(page1[9].score).toBe(100)

      // Second page using cursor
      const lastItem = page1[page1.length - 1]
      const page2 = await users.find({}, {
        sort: { score: 1 },
        limit: 10,
        cursor: {
          after: lastItem._id,
          sortField: 'score',
          sortValue: lastItem.score
        }
      })

      expect(page2).toHaveLength(10)
      expect(page2[0].score).toBe(110)
      expect(page2[9].score).toBe(200)
    })
  })

  describe("backward pagination", () => {
    it("should paginate backwards using before cursor", async () => {
      // Get middle of dataset
      const middle = await users.find({}, {
        sort: { score: 1 },
        limit: 1,
        skip: 25
      })

      const cursor = middle[0]

      // Get previous page
      const prevPage = await users.find({}, {
        sort: { score: 1 },
        limit: 10,
        cursor: {
          before: cursor._id,
          sortField: 'score',
          sortValue: cursor.score
        }
      })

      expect(prevPage).toHaveLength(10)
      // Should get items before the cursor
    })
  })

  describe("with descending sort", () => {
    it("should handle descending sort correctly", async () => {
      const page1 = await users.find({}, {
        sort: { score: -1 },
        limit: 10
      })

      expect(page1[0].score).toBe(500)  // Highest first
      expect(page1[9].score).toBe(410)

      const lastItem = page1[page1.length - 1]
      const page2 = await users.find({}, {
        sort: { score: -1 },
        limit: 10,
        cursor: {
          after: lastItem._id,
          sortField: 'score',
          sortValue: lastItem.score
        }
      })

      expect(page2[0].score).toBe(400)
      expect(page2[9].score).toBe(310)
    })
  })

  describe("with filters", () => {
    it("should combine cursor with filters", async () => {
      // Get high scores only
      const page1 = await users.find(
        { score: { $gte: 300 } },
        { sort: { score: 1 }, limit: 5 }
      )

      const lastItem = page1[page1.length - 1]
      const page2 = await users.find(
        { score: { $gte: 300 } },
        {
          sort: { score: 1 },
          limit: 5,
          cursor: {
            after: lastItem._id,
            sortField: 'score',
            sortValue: lastItem.score
          }
        }
      )

      // All results should still be >= 300
      for (const user of page2) {
        expect(user.score).toBeGreaterThanOrEqual(300)
      }
    })
  })
})
```

---

## Documentation Updates

### Files to Update:

#### 1. `docs/guide/queries.md` (new or update existing)
- Add section on String Operators (`$ilike`, `$contains`)
- Add section on Projection Helpers (`select`, `omit`)
- Add section on Text Search
- Add section on Cursor Pagination

#### 2. `docs/api/` (TypeDoc will auto-generate)
- Ensure all new types have JSDoc comments

#### 3. `README.md`
- Update feature list to include new query capabilities
- Add examples in "Quick Start" section

---

## Implementation Order

### Sprint 1 (Phase 1 & 2) - String Operators & Projection
**Estimated: 2-3 hours**

1. Update `query-types.ts` - Add `$ilike` and `$contains` to StringOperator
2. Update `sqlite-query-translator.ts` - Add translation cases
3. Update `query-options-types.ts` - Add `select` and `omit` types
4. Update `sqlite-collection.ts` - Add projection normalization and application
5. Write unit tests for string operators
6. Write unit tests for projection helpers
7. Write integration tests

### Sprint 2 (Phase 3) - Text Search
**Estimated: 2-3 hours**

1. Update `query-options-types.ts` - Add TextSearchSpec
2. Update `sqlite-collection.ts` - Add search filter building
3. Write integration tests
4. Update documentation

### Sprint 3 (Phase 4) - Cursor Pagination
**Estimated: 3-4 hours**

1. Update `query-options-types.ts` - Add CursorSpec
2. Update `sqlite-collection.ts` - Add cursor filter building
3. Write comprehensive integration tests
4. Update documentation
5. Add examples to docs

### Sprint 4 - Final
**Estimated: 1-2 hours**

1. Run full test suite
2. Run type checks
3. Update CHANGELOG.md
4. Bump version to 0.3.0
5. Publish to npm

---

## Type Exports

Add to `src/index.ts`:
```typescript
export type {
  ProjectionSpec,
  QueryOptions,
  SelectSpec,
  OmitSpec,
  TextSearchSpec,
  CursorSpec,
  SortSpec,
} from "./query-options-types.js"
```

---

## Breaking Changes

**None** - All changes are additive. Existing APIs remain unchanged.

---

## Future Work (v0.4.0)

### Aggregation Method
```typescript
const stats = await users.aggregate({
  count: true,
  groupBy: 'role',
  sum: ['age'],
  avg: ['score'],
  filter: { status: 'active' }
})
// Returns: { total: 150, groups: [...], sums: {...}, averages: {...} }
```

### Faceted Search
```typescript
const facets = await users.facets({
  fields: ['role', 'status', 'department'],
  filter: { active: true }
})
// Returns: { role: { admin: 5, user: 45 }, status: {...} }
```

These features require more complex SQL generation and new result types.

---

## Summary

| Feature | Complexity | Files Modified | New Tests |
|---------|-----------|----------------|-----------|
| `$ilike` | Easy | 2 | 2 files |
| `$contains` | Easy | 2 | 2 files |
| `select`/`omit` | Easy | 2 | 1 file |
| Text Search | Medium | 2 | 1 file |
| Cursor Pagination | Medium | 2 | 1 file |

Total new test files: 5
Total modified source files: 4
- `src/query-types.ts`
- `src/query-options-types.ts`
- `src/sqlite-query-translator.ts`
- `src/sqlite-collection.ts`
