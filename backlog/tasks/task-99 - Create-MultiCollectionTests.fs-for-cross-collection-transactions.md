---
id: task-99
title: Create MultiCollectionTests.fs for cross-collection transactions
status: To Do
assignee: []
created_date: '2025-12-29 06:06'
labels:
  - tests
  - transactions
  - multi-collection
dependencies: []
priority: medium
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Create tests for transactions spanning multiple collections. Verify atomicity is maintained across collection boundaries.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Test file created at tests/MultiCollectionTests.fs
- [ ] #2 Transaction across two collections commits both
- [ ] #3 Transaction across two collections rolls back both on error
- [ ] #4 Insert in collection A, query in collection B within transaction works
- [ ] #5 Delete from one collection, update another in same transaction works
- [ ] #6 Test file added to fsproj
- [ ] #7 All tests pass
<!-- AC:END -->
