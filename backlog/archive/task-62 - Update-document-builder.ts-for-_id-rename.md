---
id: task-62
title: Update document-builder.ts for _id rename
status: Done
assignee:
  - '@claude'
created_date: '2025-11-22 06:25'
updated_date: '2025-11-22 07:18'
labels:
  - mongodb-compat
  - breaking-change
dependencies:
  - task-61
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Update the document builder utility to use `_id` instead of `id` when constructing complete documents.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 Update buildCompleteDocument to use `_id` property
- [x] #2 Update any type references from `id` to `_id`
- [x] #3 Update JSDoc examples to use `._id`
- [x] #4 Run `bun run typecheck` - expect errors (will be fixed in subsequent tasks)
- [x] #5 Run `bun run lint` - no linter errors in modified files
<!-- AC:END -->

## Implementation Plan

<!-- SECTION:PLAN:BEGIN -->
Implementation plan for document-builder.ts:

This is a focused update to change the buildCompleteDocument function from using `id` to `_id`:

1. **Function constraint (line 45)**: Change `T extends { id: string }` to `T extends { _id: string }`

2. **Type signatures (lines 46-48)**: 
   - Change `Partial<Omit<T, "id" | "createdAt" | "updatedAt">>` to `Partial<Omit<T, "_id" | "createdAt" | "updatedAt">>`
   - Change `Omit<T, "id" | "createdAt" | "updatedAt">` to `Omit<T, "_id" | "createdAt" | "updatedAt">`

3. **Metadata parameter (line 49)**: Change `{ id: string; createdAt: number; updatedAt: number }` to `{ _id: string; createdAt: number; updatedAt: number }`

4. **JSDoc examples (lines 30-42)**: Update all examples to use `_id` instead of `id`

5. **Documentation comments**: Update references to "id" field to "_id" field

This is the key change that will resolve the type constraint errors we saw in task-61, making the entire type system consistent with MongoDB-style `_id`.
<!-- SECTION:PLAN:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Successfully updated document-builder.ts for _id rename:

✅ **Function Signature Updated:**
- Line 45: `T extends { id: string }` → `T extends { _id: string }`
- Lines 46-48: All type constraints updated from `"id"` to `"_id"`
- Line 49: `{ id: string; createdAt: number; updatedAt: number }` → `{ _id: string; createdAt: number; updatedAt: number }`

✅ **JSDoc Documentation Updated:**
- Line 7: Updated parameter documentation to reference `_id`
- Lines 30-42: Updated all examples to use `_id` instead of `id`
- Updated expected output value to show `_id` field

✅ **Implementation Verified:**
- Function now correctly builds documents with `_id` field
- Type signature matches MongoDB-style document structure
- All type constraints use `_id` consistently

✅ **Type System Impact:**
- Resolved buildCompleteDocument constraint errors from task-61
- sqlite-collection.ts now compiles correctly with buildCompleteDocument calls
- Remaining type errors are in other utility functions (mergeDocumentUpdate) for separate tasks

✅ **Quality Assurance:**
- Lint shows no errors in modified document-builder.ts file
- All changes follow MongoDB naming conventions
- Function maintains same API surface with updated field names

This key update completes the core document building infrastructure for MongoDB-style `_id` support.
<!-- SECTION:NOTES:END -->
