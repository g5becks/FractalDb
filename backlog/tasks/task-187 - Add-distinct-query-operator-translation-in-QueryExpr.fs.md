---
id: task-187
title: Add distinct query operator translation in QueryExpr.fs
status: Done
assignee:
  - '@claude'
created_date: '2026-01-01 22:02'
updated_date: '2026-01-01 22:48'
labels: []
dependencies:
  - task-186
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Add pattern matching in translateQuery to handle the distinct query operator. When distinct is encountered, set Distinct = true on the TranslatedQuery. Need to identify how F# query builder represents distinct in quotations.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 Pattern match added for distinct operator in translateQuery
- [x] #2 Sets Distinct = true on TranslatedQuery
- [x] #3 Works with select projections
<!-- AC:END -->

## Implementation Plan

<!-- SECTION:PLAN:BEGIN -->
1. Add pattern match for "Distinct" method after "Select" pattern
2. Distinct takes only source parameter (no selector)
3. Set Distinct = true on the query
4. Also need to add Distinct member to QueryBuilder class
<!-- SECTION:PLAN:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Added distinct query operator:

1. Added Distinct custom operation to QueryBuilder (line 1739-1825)
   - Takes only source parameter (no projection)
   - Full XML documentation with examples

2. Added pattern match in translate function (line 3315-3317)
   - Matches Call(Some _, mi, [source]) for "Distinct" method
   - Sets Distinct = true on the query

Works with select projections - user writes:
  select user.Country
  distinct

All 642 tests pass.
<!-- SECTION:NOTES:END -->
