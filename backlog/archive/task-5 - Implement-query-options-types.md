---
id: task-5
title: Implement query options types
status: To Do
assignee: []
created_date: '2025-11-21 01:43'
updated_date: '2025-11-21 02:02'
labels:
  - types
  - query
dependencies:
  - task-2
  - task-4
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Implement types for query options including sorting, pagination, and field projection. These types control how query results are ordered, limited, and shaped.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Create query options types in src/types/query.ts
- [ ] #2 Implement SortSpec<T> type with fields mapped to 1 (ascending) or -1 (descending)
- [ ] #3 Implement ProjectionSpec<T> type with fields mapped to 1 (include) or 0 (exclude)
- [ ] #4 Implement QueryOptions<T extends Document> with sort, limit, skip, and projection properties
- [ ] #5 Add TypeDoc comments explaining each option's behavior
- [ ] #6 Export QueryOptions from src/types/index.ts
- [ ] #7 Types compile with strict mode
- [ ] #8 No use of any type
<!-- AC:END -->
