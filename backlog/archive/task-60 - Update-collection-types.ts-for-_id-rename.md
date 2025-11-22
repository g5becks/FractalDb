---
id: task-60
title: Update collection-types.ts for _id rename
status: Done
assignee:
  - '@claude'
created_date: '2025-11-22 06:25'
updated_date: '2025-11-22 06:52'
labels:
  - mongodb-compat
  - breaking-change
dependencies:
  - task-59
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Update all type signatures in collection-types.ts to use `_id` instead of `id`. This includes InsertOneResult, InsertManyResult, and all method signatures in the Collection interface.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 Update InsertOneResult to reference `_id` in JSDoc examples
- [x] #2 Update InsertManyResult to reference `_id` in JSDoc examples
- [x] #3 Update all Omit types from `Omit<T, 'id' | ...>` to `Omit<T, '_id' | ...>`
- [x] #4 Update all JSDoc examples that reference `.id` to use `._id`
- [x] #5 Update Collection interface JSDoc examples to use `._id`
- [x] #6 Run `bun run typecheck` - expect errors (will be fixed in subsequent tasks)
- [x] #7 Run `bun run lint` - no linter errors in modified files
<!-- AC:END -->

## Implementation Plan

<!-- SECTION:PLAN:BEGIN -->
Implementation plan for collection-types.ts:

1. **JSDoc Examples (line 140)**: Change `result.document.id` to `result.document._id`
2. **Documentation section (line 92)**: Change "Auto-generated `id`" to "Auto-generated `_id`"
3. **InsertOne method Omit type (line 336)**: Change `Omit<T, "id" | "createdAt" | "updatedAt">` to `Omit<T, "_id" | "createdAt" | "updatedAt">`
4. **InsertOne JSDoc example (line 327)**: Change `result.document.id` to `result.document._id`
5. **updateOne method Omit type (line 375)**: Change `Omit<Partial<T>, "id" | "createdAt" | "updatedAt">` to `Omit<Partial<T>, "_id" | "createdAt" | "updatedAt">`
6. **updateOne JSDoc example (line 368)**: Change `{ id: userId }` to `{ _id: userId }`
7. **replaceOne method Omit type (line 412)**: Change `Omit<T, "id" | "createdAt" | "updatedAt">` to `Omit<T, "_id" | "createdAt" | "updatedAt">`
8. **replaceOne JSDoc example (line 400)**: Change `{ id: userId }` to `{ _id: userId }`
9. **deleteOne JSDoc example (line 427)**: Change `{ id: userId }` to `{ _id: userId }`
10. **insertMany method Omit type (line 467)**: Change `Omit<T, "id" | "createdAt" | "updatedAt">[]` to `Omit<T, "_id" | "createdAt" | "updatedAt">[]`
11. **updateMany method Omit type (line 500)**: Change `Omit<Partial<T>, "id" | "createdAt" | "updatedAt">` to `Omit<Partial<T>, "_id" | "createdAt" | "updatedAt">`

This updates all method signatures and documentation to use MongoDB-style `_id` instead of `id`.
<!-- SECTION:PLAN:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Successfully updated collection-types.ts for _id rename:

✅ **JSDoc Examples Updated:**
- Line 140: `result.document.id` → `result.document._id`
- Line 92: "Auto-generated `id`" → "Auto-generated `_id`"
- Line 327: `result.document.id` → `result.document._id`
- All examples using `{ id: userId }` → `{ _id: userId }`

✅ **Type Signatures Updated:**
- InsertOne method: `Omit<T, "id" | "createdAt" | "updatedAt">` → `Omit<T, "_id" | "createdAt" | "updatedAt">`
- UpdateOne method: `Omit<Partial<T>, "id" | "createdAt" | "updatedAt">` → `Omit<Partial<T>, "_id" | "createdAt" | "updatedAt">`
- ReplaceOne method: `Omit<T, "id" | "createdAt" | "updatedAt">` → `Omit<T, "_id" | "createdAt" | "updatedAt">`
- InsertMany method: `Omit<T, "id" | "createdAt" | "updatedAt">[]` → `Omit<T, "_id" | "createdAt" | "updatedAt">[]`
- UpdateMany method: `Omit<Partial<T>, "id" | "createdAt" | "updatedAt">` → `Omit<Partial<T>, "_id" | "createdAt" | "updatedAt">`

✅ **Verification:**
- Typecheck shows expected errors where SQLiteCollection implementation still uses `id` while interface now uses `_id`
- These will be fixed by task-61 (sqlite-collection.ts update)
- Lint shows no errors in modified collection-types.ts file

All type signatures and documentation updated to use MongoDB-style `_id` convention.
<!-- SECTION:NOTES:END -->
