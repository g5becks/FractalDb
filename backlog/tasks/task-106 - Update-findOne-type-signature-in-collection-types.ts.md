---
id: task-106
title: Update findOne type signature in collection-types.ts
status: Done
assignee: []
created_date: '2025-11-22 19:21'
updated_date: '2025-11-22 21:00'
labels: []
dependencies:
  - task-104
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Change findOne filter parameter to string | QueryFilter<T> and update JSDoc with examples in src/collection-types.ts
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 Type compiles without errors
- [x] #2 Linting passes
- [x] #3 Signature accepts string | QueryFilter<T>
- [x] #4 JSDoc shows both string and filter examples
- [x] #5 Full typedocs included
<!-- AC:END -->
