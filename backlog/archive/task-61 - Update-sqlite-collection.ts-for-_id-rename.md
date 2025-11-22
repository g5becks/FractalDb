---
id: task-61
title: Update sqlite-collection.ts for _id rename
status: Done
assignee:
  - '@claude'
created_date: '2025-11-22 06:25'
updated_date: '2025-11-22 07:12'
labels:
  - mongodb-compat
  - breaking-change
dependencies:
  - task-60
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Update all references to `id` in sqlite-collection.ts to use `_id`. This includes SQL column names, variable names, and object property access.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 Change SQL column from `id TEXT PRIMARY KEY` to `_id TEXT PRIMARY KEY` in createTable()
- [x] #2 Update all SQL queries: `WHERE id = ?` to `WHERE _id = ?`
- [x] #3 Update all SQL SELECT statements: `SELECT id,` to `SELECT _id,`
- [x] #4 Update all row access: `row.id` to `row._id`
- [x] #5 Update all object destructuring: `{ id, ...rest }` to `{ _id, ...rest }`
- [x] #6 Update all object construction: `{ id: ... }` to `{ _id: ... }`
- [x] #7 Update variable names: `const id =` to `const _id =` where appropriate
- [x] #8 Update all JSDoc examples to use `._id`
- [x] #9 Run `bun run typecheck` - expect errors (will be fixed in subsequent tasks)
- [x] #10 Run `bun run lint` - no linter errors in modified files
<!-- AC:END -->

## Implementation Plan

<!-- SECTION:PLAN:BEGIN -->
Implementation plan for sqlite-collection.ts:

This is a comprehensive update with ~80 references to change from `id` to `_id`:

**Core Changes:**
1. **SQL column creation (line 162)**: Change `"id TEXT PRIMARY KEY"` to `"_id TEXT PRIMARY KEY"`
2. **SQL SELECT statements**: Change `SELECT id,` to `SELECT _id,` in multiple locations
3. **SQL WHERE clauses**: Change `WHERE id = ?` to `WHERE _id = ?` throughout
4. **Row access**: Change `row.id` to `row._id` throughout
5. **Object construction**: Change `{ id: ... }` to `{ _id: ... }` throughout
6. **Type signatures**: Update all `Omit<T, "id" | ...>` to `Omit<T, "_id" | ...>`
7. **Variable names**: Keep `id` for local variables where it represents the parameter, but update field names
8. **JSDoc examples**: Update `result.document.id` to `result.document._id`
9. **Comments and documentation**: Update all references to `id` field

**Key Patterns:**
- Method parameters: `id: string` stays the same (function signatures)
- Field access: `row.id` → `row._id` (database field access)
- Object construction: `{ id: value }` → `{ _id: value }`
- Type constraints: `Omit<T, "id" | ...>` → `Omit<T, "_id" | ...>`
- SQL queries: All references to `id` column become `_id`

This will resolve the type errors from task-60 by making the implementation match the interface.
<!-- SECTION:PLAN:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Successfully updated sqlite-collection.ts for _id rename:

✅ **SQL Schema Changes:**
- Line 36: Updated documentation: `_id: TEXT PRIMARY KEY` 
- Line 77: Updated comment: `_id, body, createdAt, updatedAt columns`
- Line 134: Updated documentation: `_id, body, createdAt, updatedAt columns` 
- Line 144: Updated schema example: `_id TEXT PRIMARY KEY`
- Line 162: Changed SQL column definition: `"id TEXT PRIMARY KEY"` → `"_id TEXT PRIMARY KEY"`

✅ **SQL Query Updates:**
- All `SELECT id,` → `SELECT _id,` throughout
- All `WHERE id = ?` → `WHERE _id = ?` throughout
- All `INSERT INTO ... (id,` → `INSERT INTO ... (_id,` throughout

✅ **Database Access:**
- All `row.id` → `row._id` for database row access
- Updated type constraints: `{ id: string; body: string }` → `{ _id: string; body: string }`

✅ **Type Signatures:**
- All `Omit<T, "id" | ...>` → `Omit<T, "_id" | ...>` throughout
- Updated function parameter types to use `_id?` instead of `id?`

✅ **Object Construction:**
- All `{ id: value }` → `{ _id: value }` for document construction
- Updated destructuring patterns to avoid variable name conflicts
- Fixed parameter passing to SQL statements

✅ **JSDoc Examples:**
- Line 450: `result.document.id` → `result.document._id`
- Line 454: `id: 'custom-id'` → `_id: 'custom-id'`

✅ **Verification:**
- Typecheck shows expected constraint errors (buildCompleteDocument still expects `id` - will be fixed in task-62)
- Lint shows no errors in modified sqlite-collection.ts file
- All SQL schema and queries successfully updated for MongoDB compatibility

This comprehensive update resolves all type compatibility issues between the Collection interface and SQLiteCollection implementation.
<!-- SECTION:NOTES:END -->
