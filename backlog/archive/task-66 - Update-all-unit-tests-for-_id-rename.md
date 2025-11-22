---
id: task-66
title: Update all unit tests for _id rename
status: Done
assignee:
  - '@claude'
created_date: '2025-11-22 06:26'
updated_date: '2025-11-22 07:46'
labels:
  - mongodb-compat
  - tests
dependencies:
  - task-65
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Update all unit test files to use `_id` instead of `id` in assertions and test data.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Update test/unit/schema-builder.test.ts - all `.id` to `._id`
- [ ] #2 Update test/unit/document-builder.test.ts - all `.id` to `._id`
- [ ] #3 Update test/unit/deep-merge.test.ts - all `.id` to `._id`
- [ ] #4 Update test/unit/id-generator.test.ts - all `.id` to `._id`
- [ ] #5 Update test/unit/timestamps.test.ts - all `.id` to `._id`
- [ ] #6 Update test/unit/collection-builder.test.ts - all `.id` to `._id`
- [ ] #7 Update test/unit/validator.test.ts - all `.id` to `._id`
- [ ] #8 Update test/unit/error-messages.test.ts - all `.id` to `._id`
- [ ] #9 Update test/unit/query-cache.test.ts - all `.id` to `._id`
- [ ] #10 Update test/unit/sqlite-query-translator-simple.test.ts - all `.id` to `._id`
- [ ] #11 Run `bun run typecheck` - should pass
- [ ] #12 Run `bun run lint` - no linter errors
<!-- AC:END -->

## Implementation Plan

<!-- SECTION:PLAN:BEGIN -->
Implementation plan for unit tests:

**Scope Analysis:**
- 66 total id references across all unit test files
- 4 instances of `.id` field access
- 7 instances of "id" strings in test data
- Multiple file-specific idGenerator references (correct, should keep)

**Files to Update:**
1. document-builder.test.ts (has multiple .id and "id" references)
2. deep-merge.test.ts (has .id field access)
3. collection-builder.test.ts (idGenerator functions - KEEP as-is)
4. Other files to check individually

**Strategy:**
- Use targeted find-and-replace for `.id` → `._id`
- Use targeted find-and-replace for `"id"` → `"_id"` (in test data)
- Keep idGenerator function references unchanged
- Run typecheck after changes to catch any issues

**Changes Required:**
1. Update field access: `.id` → `._id`
2. Update test data: `"id"` → `"_id"`
3. Update type definitions where needed
4. Keep idGenerator references unchanged
<!-- SECTION:PLAN:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Updated all unit test files for _id rename including document-builder.test.ts and deep-merge.test.ts. Fixed all type references, test data, and assertions. Updated mergeDocumentUpdate function signature and JSDoc. All unit tests pass and typecheck succeeds.
<!-- SECTION:NOTES:END -->
