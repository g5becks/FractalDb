---
id: task-181
title: Add Sql.like translation in QueryExpr.fs translatePredicate
status: Done
assignee:
  - '@claude'
created_date: '2026-01-01 21:50'
updated_date: '2026-01-01 22:37'
labels: []
dependencies:
  - task-180
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Add pattern matching in translatePredicate function to recognize Sql.like calls and translate them to Query.Field with StringOp.Like. Must match Call(None, methodInfo, [patternExpr; fieldExpr]) where methodInfo.DeclaringType.Name = 'Sql' and methodInfo.Name = 'like'.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 Pattern match added for Sql.like in translatePredicate
- [x] #2 Extracts field name using extractPropertyName
- [x] #3 Extracts pattern value using evaluateExpr
- [x] #4 Returns Query.Field(field, FieldOp.String(StringOp.Like pattern))
<!-- AC:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Added pattern match for Sql.like in translatePredicate. Matches Call(None, methodInfo, [patternExpr; fieldExpr]) where DeclaringType.Name = 'Sql' and Name = 'like'. Extracts field name and pattern, returns Query.Field with StringOp.Like.
<!-- SECTION:NOTES:END -->
