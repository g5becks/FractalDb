---
id: task-126
title: Create CacheTests.fs for query caching
status: To Do
assignee: []
created_date: '2025-12-29 06:14'
updated_date: '2025-12-29 06:15'
labels:
  - tests
  - cache
dependencies:
  - task-125
priority: low
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Create tests for query caching functionality if cache is implemented. Verify cache hits/misses and eviction behavior.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Test file created at tests/CacheTests.fs
- [ ] #2 Test that cache disabled does not store queries
- [ ] #3 Test that cache enabled returns cached results
- [ ] #4 Test that cache eviction works when limit reached
- [ ] #5 Test that collection-level cache overrides database setting
- [ ] #6 Test file added to fsproj
- [ ] #7 All tests pass
<!-- AC:END -->
