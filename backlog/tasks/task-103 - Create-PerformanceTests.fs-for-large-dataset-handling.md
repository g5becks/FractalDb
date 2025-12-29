---
id: task-103
title: Create PerformanceTests.fs for large dataset handling
status: To Do
assignee: []
created_date: '2025-12-29 06:07'
labels:
  - tests
  - performance
dependencies: []
priority: low
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Create performance-focused tests for large datasets. These tests verify the system handles scale appropriately.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Test file created at tests/PerformanceTests.fs
- [ ] #2 Insert 10000 documents completes in reasonable time
- [ ] #3 Batch insert is faster than individual inserts
- [ ] #4 Cursor pagination scales better than offset for large datasets
- [ ] #5 Test file added to fsproj
- [ ] #6 All tests pass
<!-- AC:END -->
