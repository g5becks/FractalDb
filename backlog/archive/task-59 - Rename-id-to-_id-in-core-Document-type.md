---
id: task-59
title: Rename id to _id in core Document type
status: Done
assignee:
  - '@claude'
created_date: '2025-11-22 06:25'
updated_date: '2025-11-22 06:47'
labels:
  - mongodb-compat
  - breaking-change
dependencies: []
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Rename the `id` field to `_id` in the core Document type to align with MongoDB conventions. This is the foundational change that all other tasks depend on.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 Change `readonly id: string` to `readonly _id: string` in src/core-types.ts Document type
- [x] #2 Update DocumentInput type to use `_id` instead of `id`
- [x] #3 Update DocumentUpdate type to exclude `_id` instead of `id`
- [x] #4 Update BulkWriteResult type references from `id` to `_id`
- [x] #5 Run `bun run typecheck` - expect errors (will be fixed in subsequent tasks)
- [x] #6 Run `bun run lint` - no linter errors in modified files
<!-- AC:END -->

## Implementation Plan

<!-- SECTION:PLAN:BEGIN -->
Implementation plan:

1. **Document type (line 62)**: Change `readonly id: string` to `readonly _id: string`
2. **DocumentInput type (line 94)**: Update `Except<T, "id">` to `Except<T, "_id">` and `{ id?: string }` to `{ _id?: string }`
3. **DocumentUpdate type (line 128)**: Update `Except<T, "id">` to `Except<T, "_id">`
4. **BulkWriteResult type**: No changes needed - uses generic T parameter which will automatically pick up the _id change

This is a straightforward type-only change that will break the build temporarily, but subsequent tasks will fix all the references throughout the codebase.

Files to modify:
- src/core-types.ts (4 changes total)

After this change, we expect TypeScript errors in all files that reference `.id`, which will be fixed by subsequent tasks (60-70).
<!-- SECTION:PLAN:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Successfully implemented core type changes for MongoDB compatibility:

1. ✅ Changed Document type `readonly id: string` to `readonly _id: string` (line 62)
2. ✅ Updated DocumentInput type: `Except<T, "id">` to `Except<T, "_id">` and `{ id?: string }` to `{ _id?: string }` (line 94)
3. ✅ Updated DocumentUpdate type: `Except<T, "id">` to `Except<T, "_id">` (line 128)
4. ✅ BulkWriteResult type: No changes needed (uses generic T parameter)
5. ✅ Ran `bun run typecheck` - confirmed expected TypeScript errors in files that reference old `id` field
6. ✅ Ran `bun run lint` - no linter errors in modified core-types.ts file

All type changes completed successfully. This foundational change breaks the build as expected, and subsequent tasks (60-70) will fix all references throughout the codebase to use `_id` instead of `id`.
<!-- SECTION:NOTES:END -->
