---
id: task-182
title: Add Sql.ilike translation in QueryExpr.fs translatePredicate
status: Done
assignee:
  - '@claude'
created_date: '2026-01-01 21:52'
updated_date: '2026-01-01 22:38'
labels: []
dependencies:
  - task-181
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Add pattern matching in translatePredicate function to recognize Sql.ilike calls and translate them to Query.Field with StringOp.ILike. Similar to Sql.like but uses StringOp.ILike for case-insensitive matching.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 Pattern match added for Sql.ilike in translatePredicate
- [x] #2 Returns Query.Field(field, FieldOp.String(StringOp.ILike pattern))
<!-- AC:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Added pattern match for Sql.ilike in translatePredicate. Same structure as Sql.like but uses StringOp.ILike for case-insensitive matching.
<!-- SECTION:NOTES:END -->
