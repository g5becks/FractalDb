---
id: task-53
title: Write edge case tests for array queries
status: To Do
assignee: []
created_date: '2025-11-21 01:53'
updated_date: '2025-11-21 02:07'
labels:
  - testing
  - edge-cases
  - arrays
dependencies:
  - task-14
priority: medium
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Test array operator edge cases including empty arrays, null values in arrays, and array indexing.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Create test/edge-cases/arrays.test.ts file
- [ ] #2 Test { tags: [] } matches documents with empty array, not missing field
- [ ] #3 Test  operator handles null values in array correctly
- [ ] #4 Test  operator with empty array
- [ ] #5 Test  operator with various array lengths
- [ ] #6 Test array indexing with positive and negative indices
- [ ] #7 Test  with complex nested conditions
- [ ] #8 All tests pass with bun test
- [ ] #9 Tests compile with strict mode
<!-- AC:END -->
