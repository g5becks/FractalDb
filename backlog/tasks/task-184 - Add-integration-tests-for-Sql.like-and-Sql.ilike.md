---
id: task-184
title: Add integration tests for Sql.like and Sql.ilike
status: Done
assignee:
  - '@claude'
created_date: '2026-01-01 21:56'
updated_date: '2026-01-01 22:42'
labels: []
dependencies:
  - task-183
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Add integration tests that execute actual queries using Sql.like and Sql.ilike against a real SQLite database to verify end-to-end functionality.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 Test: Sql.like returns documents matching pattern
- [x] #2 Test: Sql.ilike is case-insensitive
- [x] #3 Test: Sql.like with complex pattern works end-to-end
<!-- AC:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Added 5 integration tests for Sql.like and Sql.ilike:
- Sql.like returns documents matching pattern (% wildcard)
- Sql.like with underscore wildcard works
- Sql.like with complex pattern works end-to-end
- Sql.ilike is case-insensitive
- Sql.ilike with mixed case pattern matches

Fixed type inference issue by adding explicit TranslatedQuery<TestUser> annotations.
All 9 tests (4 unit + 5 integration) pass.
<!-- SECTION:NOTES:END -->
