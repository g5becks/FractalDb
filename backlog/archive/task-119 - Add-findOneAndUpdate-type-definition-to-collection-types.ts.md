---
id: task-119
title: Add findOneAndUpdate type definition to collection-types.ts
status: Done
assignee:
  - '@droid'
created_date: '2025-11-22 19:48'
updated_date: '2025-11-22 22:13'
labels: []
dependencies:
  - task-115
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Add findOneAndUpdate method to Collection interface with full JSDoc, all options (sort, returnDocument, upsert), and examples in src/collection-types.ts
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 Type compiles without errors
- [x] #2 Linting passes
- [x] #3 Method signature includes filter: string | QueryFilter<T>
- [x] #4 Options include sort, returnDocument, upsert
- [x] #5 returnDocument type is 'before' | 'after'
- [x] #6 Full JSDoc with examples included
<!-- AC:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Added findOneAndUpdate type definition.

Changes:
- Method signature with filter: string | QueryFilter<T>
- Options include sort, returnDocument (before|after), upsert
- Comprehensive JSDoc with 4 examples
- Proper TypeScript types
<!-- SECTION:NOTES:END -->
