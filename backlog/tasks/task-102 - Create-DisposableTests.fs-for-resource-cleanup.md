---
id: task-102
title: Create DisposableTests.fs for resource cleanup
status: To Do
assignee: []
created_date: '2025-12-29 06:07'
updated_date: '2025-12-29 06:15'
labels:
  - tests
  - disposable
  - lifecycle
dependencies:
  - task-84
  - task-85
priority: medium
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Create tests for IDisposable implementation and resource cleanup. Verify connections are properly released and FromConnection behavior.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Test file created at tests/DisposableTests.fs
- [ ] #2 Database.Close releases file lock
- [ ] #3 Database.Dispose is idempotent (multiple calls don't throw)
- [ ] #4 Transaction.Dispose rolls back uncommitted changes
- [ ] #5 Using block properly disposes database
- [ ] #6 FromConnection does not dispose user-provided connection
- [ ] #7 Test file added to fsproj
- [ ] #8 All tests pass
<!-- AC:END -->
