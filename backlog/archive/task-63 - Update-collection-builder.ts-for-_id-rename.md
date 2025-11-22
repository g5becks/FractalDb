---
id: task-63
title: Update collection-builder.ts for _id rename
status: Done
assignee:
  - '@claude'
created_date: '2025-11-22 06:25'
updated_date: '2025-11-22 07:22'
labels:
  - mongodb-compat
  - breaking-change
dependencies:
  - task-62
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Update the collection builder to use `_id` in table creation SQL and any related logic.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 Update any SQL generation to use `_id` column name
- [x] #2 Update any references to document id field
- [x] #3 Update JSDoc examples if present
- [x] #4 Run `bun run typecheck` - expect errors (will be fixed in subsequent tasks)
- [x] #5 Run `bun run lint` - no linter errors in modified files
<!-- AC:END -->

## Implementation Plan

<!-- SECTION:PLAN:BEGIN -->
Implementation plan for collection-builder.ts:

After analyzing the file, CollectionBuilder.ts primarily builds schema definitions and creates SQLiteCollection instances. The actual SQL generation happens in SQLiteCollection (already updated in task-61).

**Analysis Results:**
- No direct SQL column generation in this file
- No references to database field names like "id"
- All `id` references are either:
  - `idGenerator` function (correct name)
  - Generic type predicates (not field-specific)
  - Parameter names (appropriate)

**Required Changes:**
1. **None needed** - This file doesn't contain SQL generation logic
2. **SQL column creation** is handled in SQLiteCollection (already updated in task-61)
3. **Field name references** don't exist in this file

**Verification:**
- Run typecheck to ensure no new issues
- Run lint to confirm code quality
- This task may be a no-op given the architecture

CollectionBuilder creates schema definitions but doesn't generate SQL with field names directly.
<!-- SECTION:PLAN:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Successfully analyzed collection-builder.ts for _id rename:

✅ **Analysis Results:**
- No SQL generation logic found in collection-builder.ts
- No database field name references that need updating
- All `id` references are appropriate (idGenerator, type predicates, parameter names)
- SQL column creation is handled in SQLiteCollection (already updated in task-61)

✅ **Architecture Understanding:**
- CollectionBuilder creates schema definitions, not SQL directly
- SQLiteCollection handles all SQL generation with field names
- This separation of concerns means CollectionBuilder doesn't need field name changes

✅ **Verification Completed:**
- Typecheck shows no errors in collection-builder.ts
- Lint shows no errors in collection-builder.ts
- Remaining type errors are in other files (mergeDocumentUpdate in deep-merge.ts)

✅ **Task Assessment:**
- This task was a "no-op" due to the clean architecture separation
- SQL generation properly isolated to SQLiteCollection
- CollectionBuilder maintains schema definition responsibilities without field coupling

The clean architecture prevented unnecessary changes in this file, demonstrating good separation of concerns between schema definition (CollectionBuilder) and SQL generation (SQLiteCollection).
<!-- SECTION:NOTES:END -->
