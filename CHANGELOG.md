# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

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
