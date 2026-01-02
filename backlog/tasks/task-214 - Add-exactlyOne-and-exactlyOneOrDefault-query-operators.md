---
id: task-214
title: Add exactlyOne and exactlyOneOrDefault query operators
status: Done
assignee:
  - '@claude'
created_date: '2026-01-01 23:37'
updated_date: '2026-01-01 23:51'
labels:
  - query
  - element
dependencies: []
priority: medium
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Implement exactlyOne/exactlyOneOrDefault operators that assert exactly one result exists. Uses LIMIT 2 query and checks count.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 Add 'exactlyOne' CustomOperation to QueryBuilder
- [x] #2 Add 'exactlyOneOrDefault' CustomOperation to QueryBuilder
- [x] #3 Add execExactlyOne function to Collection.fs
- [x] #4 exactlyOne throws if 0 or >1 results, exactlyOneOrDefault returns None for 0, throws for >1
<!-- AC:END -->

## Implementation Plan

<!-- SECTION:PLAN:BEGIN -->
1. Add exactlyOne CustomOperation to QueryBuilder in src/QueryExpr.fs
2. Add exactlyOneOrDefault CustomOperation to QueryBuilder
3. Add execExactlyOne function to src/Collection.fs (LIMIT 2, throws if 0 or >1)
4. Add execExactlyOneOrDefault function (LIMIT 2, returns None for 0, throws for >1)
5. Add integration tests
<!-- SECTION:PLAN:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Implemented exactlyOne/exactlyOneOrDefault operators:
- Added exactlyOne CustomOperation (QueryExpr.fs ~3028-3099)
- Added exactlyOneOrDefault CustomOperation (QueryExpr.fs ~3101-3178)
- Added execExactlyOne function (Collection.fs ~1702-1795) - uses LIMIT 2, throws for 0 or >1
- Added execExactlyOneOrDefault function (Collection.fs ~1797-1884) - returns None for 0, throws for >1
- Added 7 integration tests covering single match, no match, multiple matches, and Option handling
- Test count: 682 → 689
<!-- SECTION:NOTES:END -->
