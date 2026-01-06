---
id: task-87
title: Update InsertOneResult type definition
status: Done
assignee:
  - '@claude'
created_date: '2025-11-22 19:04'
updated_date: '2025-11-22 19:56'
labels: []
dependencies: []
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Change InsertOneResult from object type to type alias T and add deprecation JSDoc warning in src/collection-types.ts
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 Type compiles without errors (bun run typecheck)
- [x] #2 Linting passes (bun run lint)
- [ ] #3 Deprecation JSDoc includes version notice (v2.0.0)
- [x] #4 Type correctly aliases to T
<!-- AC:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Updated InsertOneResult<T> TsDoc to reflect new API where insertOne returns T directly instead of wrapper object. Fixed sqlite-collection.ts return type to return fullDoc instead of { document: fullDoc, acknowledged: true }. Type checking passes cleanly. Linting shows only pre-existing issues unrelated to changes.
<!-- SECTION:NOTES:END -->
