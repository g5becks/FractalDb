---
id: task-183
title: Add unit tests for Sql.like and Sql.ilike translation
status: Done
assignee:
  - '@claude'
created_date: '2026-01-01 21:54'
updated_date: '2026-01-01 22:39'
labels: []
dependencies:
  - task-182
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Add unit tests in QueryExprTests.fs to verify that Sql.like and Sql.ilike calls in query expressions are correctly translated to Query.Field with StringOp.Like/ILike.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 Test: Sql.like with % wildcard generates StringOp.Like
- [x] #2 Test: Sql.like with _ wildcard generates StringOp.Like
- [x] #3 Test: Sql.ilike generates StringOp.ILike
- [x] #4 Test: Sql.like with character set pattern [abc] works
<!-- AC:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Added 4 unit tests to QueryExprTests.fs: 3 for Sql.like (% wildcard, _ wildcard, [abc] character set) and 1 for Sql.ilike. All tests verify correct translation to Query.Field with StringOp.Like/ILike.
<!-- SECTION:NOTES:END -->
