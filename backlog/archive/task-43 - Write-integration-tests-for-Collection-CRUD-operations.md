---
id: task-43
title: Write integration tests for Collection CRUD operations
status: To Do
assignee: []
created_date: '2025-11-21 01:51'
updated_date: '2025-11-21 02:07'
labels:
  - testing
  - collection
  - integration
dependencies:
  - task-19
  - task-20
  - task-21
  - task-22
  - task-23
  - task-24
  - task-25
  - task-26
  - task-27
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Test all Collection methods with real SQLite database operations to verify correct behavior end-to-end.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Create test/collection/crud.test.ts file
- [ ] #2 Setup in-memory SQLite database for each test
- [ ] #3 Test insertOne creates document with generated ID
- [ ] #4 Test insertOne validates documents and throws SchemaValidationError on invalid input
- [ ] #5 Test insertMany in ordered mode stops at first error
- [ ] #6 Test insertMany in unordered mode continues and reports all errors
- [ ] #7 Test findById returns document or null
- [ ] #8 Test find returns filtered documents with query operators
- [ ] #9 Test findOne returns first match or null
- [ ] #10 Test count returns correct document count
- [ ] #11 Test updateOne performs partial updates
- [ ] #12 Test updateOne with upsert creates document if not found
- [ ] #13 Test updateMany updates all matching documents
- [ ] #14 Test replaceOne replaces entire document except ID
- [ ] #15 Test deleteOne returns true/false based on success
- [ ] #16 Test deleteMany returns deleted count
- [ ] #17 All tests pass with bun test
- [ ] #18 Tests use strict mode and no any types
<!-- AC:END -->
