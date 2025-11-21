---
id: task-11
title: Define query options types
status: To Do
assignee: []
created_date: '2025-11-21 02:31'
labels:
  - query
  - types
dependencies: []
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Implement types for query sorting, pagination, and field projection. These options control result ordering, limiting, skipping, and field selection while maintaining type safety across all operations.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 SortSpec<T> type defined mapping document fields to 1 (ascending) or -1 (descending)
- [ ] #2 ProjectionSpec<T> type defined mapping document fields to 1 (include) or 0 (exclude)
- [ ] #3 QueryOptions<T> type defined with sort, limit, skip, and projection properties
- [ ] #4 All properties appropriately marked as readonly
- [ ] #5 Types work correctly with nested document structures
- [ ] #6 TypeScript type checking passes with zero errors
- [ ] #7 No any types used in implementation
- [ ] #8 Complete TypeDoc comments with examples showing pagination and field projection patterns
<!-- AC:END -->
