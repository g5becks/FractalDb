---
id: task-52
title: Write edge case tests for null and undefined handling
status: To Do
assignee: []
created_date: '2025-11-21 01:53'
updated_date: '2025-11-21 02:07'
labels:
  - testing
  - edge-cases
dependencies:
  - task-19
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Test the documented behavior for null values, undefined fields, and field existence queries.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Create test/edge-cases/null-undefined.test.ts file
- [ ] #2 Test null values are stored and retrieved correctly as JSON null
- [ ] #3 Test undefined values cause field to be omitted from document
- [ ] #4 Test { field: null } query matches documents where field is null
- [ ] #5 Test { field: { : false } } matches documents without field
- [ ] #6 Test { field: { : true } } matches documents with field even if null
- [ ] #7 Test distinction between null and missing field is preserved
- [ ] #8 All tests pass with bun test
- [ ] #9 Tests compile with strict mode
<!-- AC:END -->
