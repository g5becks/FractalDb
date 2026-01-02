---
id: task-180
title: Add Sql.like and Sql.ilike marker functions to Library.fs
status: Done
assignee:
  - '@claude'
created_date: '2026-01-01 21:50'
updated_date: '2026-01-01 22:37'
labels: []
dependencies: []
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Create a new Sql module in Library.fs with like and ilike marker functions that throw InvalidOperationException when called directly. These functions are only meant to be used in query expressions where they get translated to SQL LIKE patterns. The like function is case-sensitive, ilike is case-insensitive. Both take pattern first, then field (for piping).
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 Sql module exists in Library.fs
- [x] #2 Sql.like function defined with signature: pattern:string -> field:string -> bool
- [x] #3 Sql.ilike function defined with signature: pattern:string -> field:string -> bool
- [x] #4 Both functions throw InvalidOperationException with descriptive message when called directly
- [x] #5 XML documentation added for module and both functions
<!-- AC:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Added Sql module with like and ilike marker functions to Library.fs. Both functions throw InvalidOperationException when called directly - they are marker functions for query expression translation only. Full XML documentation included for module and both functions with usage examples.
<!-- SECTION:NOTES:END -->
