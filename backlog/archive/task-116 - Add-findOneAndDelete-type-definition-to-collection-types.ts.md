---
id: task-116
title: Add findOneAndDelete type definition to collection-types.ts
status: Done
assignee:
  - '@droid'
created_date: '2025-11-22 19:42'
updated_date: '2025-11-22 22:06'
labels: []
dependencies:
  - task-115
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Add findOneAndDelete method to Collection interface with full JSDoc including examples showing both string and filter usage in src/collection-types.ts
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 Type compiles without errors
- [x] #2 Linting passes
- [x] #3 Method signature includes filter: string | QueryFilter<T>
- [x] #4 Method signature includes optional sort parameter
- [x] #5 Full JSDoc with examples included
<!-- AC:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Added findOneAndDelete type definition to Collection interface.

Changes made:
- Added method signature with filter: string | QueryFilter<T>
- Included optional sort parameter in options
- Comprehensive JSDoc documentation
- Examples showing both string ID and filter usage
- Examples showing sort option for multiple matches
- Added new section "Atomic Find-and-Modify Operations"

All acceptance criteria met:
- Type checking passes
- Linting passes
- Method accepts string | QueryFilter<T>
- Optional sort parameter included
- Full JSDoc with multiple examples
<!-- SECTION:NOTES:END -->
