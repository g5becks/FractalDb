---
id: task-112
title: Update replaceOne type signature in collection-types.ts
status: Done
assignee:
  - '@droid'
created_date: '2025-11-22 19:33'
updated_date: '2025-11-22 21:42'
labels: []
dependencies:
  - task-104
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Change replaceOne parameter to filter: string | QueryFilter<T> and update JSDoc in src/collection-types.ts
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 Type compiles without errors
- [x] #2 Linting passes
- [x] #3 Parameter type is string | QueryFilter<T>
- [x] #4 JSDoc shows both usage patterns
- [x] #5 Full typedocs included
<!-- AC:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Updated replaceOne type signature in collection-types.ts to accept string | QueryFilter<T>.

Changes made:
- Changed parameter from id: string to filter: string | QueryFilter<T>
- Updated JSDoc to document both usage patterns (string ID and filter)
- Added comprehensive examples showing ID-based and filter-based replacement
- Clarified difference between updateOne (merge) and replaceOne (full replacement)
- Updated field naming to use _id consistently

All acceptance criteria met:
- Type checking passes
- Linting passes
- Parameter type is string | QueryFilter<T>
- JSDoc shows both usage patterns with examples
- Full TypeDoc documentation included
<!-- SECTION:NOTES:END -->
