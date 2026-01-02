---
id: task-185
title: Add Distinct field to TranslatedQuery type
status: Done
assignee:
  - '@claude'
created_date: '2026-01-01 21:58'
updated_date: '2026-01-01 22:44'
labels: []
dependencies:
  - task-184
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Extend the TranslatedQuery<'T> record type in QueryExpr.fs to include a Distinct: bool field. Update all places that create TranslatedQuery to initialize Distinct = false.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 Distinct: bool field added to TranslatedQuery type
- [x] #2 Default value is false
- [x] #3 All existing TranslatedQuery creations updated
- [x] #4 XML documentation added for Distinct field
<!-- AC:END -->

## Implementation Plan

<!-- SECTION:PLAN:BEGIN -->
1. Add Distinct: bool field to TranslatedQuery type at line 362
2. Update compose method to merge Distinct (OR logic)
3. Update empty initialization to set Distinct = false
4. Add XML documentation for the field
<!-- SECTION:PLAN:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Added Distinct: bool field to TranslatedQuery<'T> type:
- Field added at line 377 with full XML documentation
- Default value is false
- compose method updated to merge Distinct with OR logic (d1 || d2)
- Empty initialization updated in translate function

All 642 tests pass.
<!-- SECTION:NOTES:END -->
