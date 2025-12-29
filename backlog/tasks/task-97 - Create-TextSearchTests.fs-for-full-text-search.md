---
id: task-97
title: Create TextSearchTests.fs for full-text search
status: To Do
assignee: []
created_date: '2025-12-29 06:04'
labels:
  - tests
  - search
dependencies: []
priority: medium
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Create tests for text search functionality. Verify LIKE-based text search works correctly across single and multiple fields.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Test file created at tests/TextSearchTests.fs
- [ ] #2 Search finds documents containing term
- [ ] #3 Search across multiple fields works
- [ ] #4 Search is case-insensitive by default
- [ ] #5 Search with no matches returns empty
- [ ] #6 Search combined with filter works
- [ ] #7 Search with special characters is escaped properly
- [ ] #8 Test file added to fsproj
- [ ] #9 All tests pass
<!-- AC:END -->
