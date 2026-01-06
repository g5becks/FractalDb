# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.4.0] - 2026-01-06

### Added

- **Operation Cancellation** - AbortSignal support for canceling long-running operations:
  ```typescript
  const controller = new AbortController()
  setTimeout(() => controller.abort(), 5000)
  
  const results = await users.find(
    { age: { $gte: 18 } },
    { signal: controller.signal }
  )
  
  // Or use AbortSignal.timeout()
  const results = await users.find({}, { signal: AbortSignal.timeout(5000) })
  ```

- **Automatic Retries** - Configurable retry logic for transient failures:
  ```typescript
  // Database-level configuration
  const db = new Strata({
    database: 'app.db',
    retry: { retries: 3, minTimeout: 100 }
  })
  
  // Collection-level configuration
  const users = db.collection('users', schema, {
    retry: { retries: 5, minTimeout: 50 }
  })
  
  // Operation-level configuration
  await users.insertOne(doc, { retry: { retries: 2 } })
  
  // Disable retries for specific operation
  await users.find({}, { retry: false })
  ```

- **New error type**: `AbortedError` - Thrown when operations are cancelled via AbortSignal
- **New retry types**: `RetryOptions`, `RetryContext` - Configure retry behavior with exponential backoff
- **Retry utilities**: `withRetry`, `defaultShouldRetry`, `mergeRetryOptions` - Exported for custom retry logic

### Changed

- All collection methods now accept `signal?: AbortSignal` option for cancellation
- All collection methods now accept `retry?: RetryOptions | false` option for retry configuration
- Retry configuration follows precedence: operation > collection > database

## [0.3.2] - 2025-11-23

### Added

- **Type-safe projections** - `select` and `omit` options now narrow the TypeScript return type:
  ```typescript
  // TypeScript knows this returns Pick<User, '_id' | 'name' | 'email'>[]
  const users = await collection.find(
    { status: 'active' },
    { select: ['name', 'email'] as const }
  )
  users[0].name      // ✅ TypeScript knows this exists
  users[0].password  // ❌ TypeScript error: Property 'password' does not exist

  // With omit, TypeScript returns Omit<User, 'password'>[]
  const safeUsers = await collection.find(
    { status: 'active' },
    { omit: ['password'] as const }
  )
  ```

- **New exported types for type-safe projections**:
  - `QueryOptionsBase<T>` - Base query options without projection
  - `QueryOptionsWithSelect<T, K>` - Query options with select projection
  - `QueryOptionsWithOmit<T, K>` - Query options with omit projection
  - `QueryOptionsWithoutProjection<T>` - Query options explicitly without projection
  - `ProjectedDocument<T, K>` - Helper type for projected document results

### Notes

- Use `as const` with select/omit arrays for best type inference
- Method overloads added for `find()`, `findOne()`, and `search()` to support type-safe projections
- Comprehensive type tests added for projection type safety

## [0.3.1] - 2024-11-23

### Fixed

- Build now uses tsdown to bundle into single file instead of tsc multi-file output
- Package exports updated to use bundled .mjs files

## [0.3.0] - 2024-11-23

### Added

- **Dedicated `search()` method** - Clean API for text search across multiple fields:
  ```typescript
  const results = await collection.search('typescript', ['title', 'content'])
  ```

- **String operators** - New query operators for flexible string matching:
  - `$ilike` - Case-insensitive LIKE matching
  - `$contains` - Substring matching (shorthand for `$like: '%value%'`)

- **Field projection helpers** - Cleaner alternatives to `projection`:
  - `select` - Include only specified fields: `{ select: ['name', 'email'] }`
  - `omit` - Exclude specified fields: `{ omit: ['password'] }`

- **Text search option** - Multi-field text search via `find()`:
  ```typescript
  await collection.find({}, {
    search: { text: 'query', fields: ['title', 'content'] }
  })
  ```

- **Cursor pagination** - Efficient pagination for large datasets:
  ```typescript
  const page1 = await collection.find({}, { sort: { createdAt: -1 }, limit: 20 })
  const page2 = await collection.find({}, {
    sort: { createdAt: -1 },
    limit: 20,
    cursor: { after: page1.at(-1)?._id }
  })
  ```

- **New exported types**:
  - `SelectSpec<T>` - Type for select field arrays
  - `OmitSpec<T>` - Type for omit field arrays
  - `TextSearchSpec<T>` - Type for search configuration
  - `CursorSpec` - Type for cursor pagination

### Notes

- All changes are backward compatible
- Existing `projection` option continues to work alongside new `select`/`omit` helpers
- The `search` option in `find()` remains available for complex queries requiring filters
