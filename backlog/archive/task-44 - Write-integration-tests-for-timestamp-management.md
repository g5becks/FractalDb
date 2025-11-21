---
id: task-44
title: Write integration tests for timestamp management
status: To Do
assignee: []
created_date: '2025-11-21 01:51'
updated_date: '2025-11-21 02:07'
labels:
  - testing
  - collection
  - timestamps
dependencies:
  - task-20
  - task-22
priority: medium
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Test automatic createdAt and updatedAt timestamp generation and management across insert and update operations.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Create test/collection/timestamps.test.ts file
- [ ] #2 Test insertOne adds createdAt when timestamps enabled
- [ ] #3 Test insertOne adds updatedAt equal to createdAt on creation
- [ ] #4 Test updateOne updates updatedAt but preserves createdAt
- [ ] #5 Test replaceOne updates updatedAt but preserves createdAt
- [ ] #6 Test timestamps are Date objects with correct values
- [ ] #7 Test timestamps are not added when schema.timestamps is false
- [ ] #8 All tests pass with bun test
- [ ] #9 Tests compile with strict mode
<!-- AC:END -->
