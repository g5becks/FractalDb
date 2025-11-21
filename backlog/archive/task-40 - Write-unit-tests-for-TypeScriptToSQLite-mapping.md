---
id: task-40
title: Write unit tests for TypeScriptToSQLite mapping
status: To Do
assignee: []
created_date: '2025-11-21 01:50'
updated_date: '2025-11-21 02:07'
labels:
  - testing
  - types
  - schema
dependencies:
  - task-3
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Test that TypeScriptToSQLite conditional type correctly maps TypeScript types to valid SQLite types and prevents invalid mappings.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Create test/types/schema.test.ts file
- [ ] #2 Test string maps to TEXT or BLOB only
- [ ] #3 Test number maps to INTEGER, REAL, or NUMERIC only
- [ ] #4 Test boolean maps to BOOLEAN or INTEGER only
- [ ] #5 Test Date maps to INTEGER, TEXT, or REAL
- [ ] #6 Test Uint8Array and ArrayBuffer map to BLOB
- [ ] #7 Test Array<T> maps to TEXT or BLOB
- [ ] #8 Test object types map to TEXT or BLOB
- [ ] #9 Use type-level assertions to verify invalid mappings cause compile errors
- [ ] #10 All tests pass with bun test
- [ ] #11 Tests compile with strict mode
<!-- AC:END -->
