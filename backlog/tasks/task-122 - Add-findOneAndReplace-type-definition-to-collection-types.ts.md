---
id: task-122
title: Add findOneAndReplace type definition to collection-types.ts
status: Done
assignee: []
created_date: '2025-11-22 19:54'
updated_date: '2025-11-22 22:13'
labels: []
dependencies:
  - task-115
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Add findOneAndReplace method to Collection interface with full JSDoc, all options, and examples in src/collection-types.ts
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Type compiles without errors
- [ ] #2 Linting passes
- [ ] #3 Method signature includes filter: string | QueryFilter<T>
- [ ] #4 Options include sort, returnDocument, upsert
- [ ] #5 Full JSDoc with examples included
<!-- AC:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Added findOneAndReplace type definition.

Changes:
- Method signature with filter: string | QueryFilter<T>
- Options include sort, returnDocument (before|after), upsert
- Comprehensive JSDoc with examples
- Proper TypeScript types
<!-- SECTION:NOTES:END -->
