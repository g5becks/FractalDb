---
id: task-192
title: Handle aggregates in SQL generation
status: Done
assignee:
  - '@claude'
created_date: '2026-01-01 22:13'
updated_date: '2026-01-01 22:56'
labels: []
dependencies:
  - task-191
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Update SqlTranslator.fs to check TranslatedQuery.Aggregate and generate appropriate SQL with MIN/MAX/SUM/AVG functions. When Aggregate is Some, generate scalar aggregate query instead of document selection.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 SqlTranslator checks Aggregate field
- [x] #2 Generates SELECT MIN(json_extract(...)) for Min
- [x] #3 Generates SELECT MAX(json_extract(...)) for Max
- [x] #4 Generates SELECT SUM(json_extract(...)) for Sum
- [x] #5 Generates SELECT AVG(json_extract(...)) for Avg
<!-- AC:END -->

## Implementation Plan

<!-- SECTION:PLAN:BEGIN -->
1. Check where SQL generation happens (Collection.fs exec function)
2. Add handling for Aggregate in exec or create separate aggregate execution path
3. Generate SELECT MIN/MAX/SUM/AVG(json_extract(body, ...)) queries
<!-- SECTION:PLAN:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Added execAggregate function in Collection.fs (lines 1036-1104):

- Generates SELECT MIN/MAX/SUM/AVG/COUNT SQL
- Uses json_extract for field access in aggregate functions
- For COUNT: returns int via Convert.ToInt32
- For Min/Max/Sum/Avg: returns raw value via identity converter
- Handles WHERE clause from filter

SQL patterns:
- SELECT MIN(json_extract(body, \"$.field\")) FROM table WHERE ...
- SELECT MAX(json_extract(body, \"$.field\")) FROM table WHERE ...
- SELECT SUM(json_extract(body, \"$.field\")) FROM table WHERE ...
- SELECT AVG(json_extract(body, \"$.field\")) FROM table WHERE ...
- SELECT COUNT(*) FROM table WHERE ...

All 648 tests pass.
<!-- SECTION:NOTES:END -->
