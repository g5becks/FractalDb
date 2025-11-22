---
id: task-108
title: Update updateOne type signature in collection-types.ts
status: Done
assignee: []
created_date: '2025-11-22 19:25'
updated_date: '2025-11-22 21:11'
labels: []
dependencies:
  - task-104
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Change updateOne parameter from id to filter with type string | QueryFilter<T> and update JSDoc in src/collection-types.ts
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 Type compiles without errors
- [x] #2 Linting passes
- [x] #3 Parameter renamed to filter
- [x] #4 Type is string | QueryFilter<T>
- [x] #5 JSDoc shows both usage patterns
- [x] #6 Full typedocs included
<!-- AC:END -->
