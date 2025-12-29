---
id: task-82
title: Add Donald exception mapper function
status: Done
assignee:
  - '@assistant'
created_date: '2025-12-29 05:48'
updated_date: '2025-12-29 06:43'
labels:
  - foundation
  - errors
  - donald
dependencies: []
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Create mapDonaldException function that converts all 4 Donald exception types (DbConnectionException, DbExecutionException, DbReaderException, DbTransactionException) to FractalError with rich context including SQL statements, field names, and transaction steps.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 mapDonaldException function handles DbConnectionException with connection string
- [x] #2 mapDonaldException function handles DbExecutionException with SQL statement
- [x] #3 mapDonaldException function handles DbReaderException with field name
- [x] #4 mapDonaldException function handles DbTransactionException with step
- [x] #5 SQLite error code 19 (UNIQUE constraint) maps to UniqueConstraint error
- [x] #6 SQLite error code 5 (BUSY) maps to Connection error with locked message
- [x] #7 Code builds with no errors or warnings
- [ ] #8 All existing tests pass

- [x] #9 XML doc comments on mapDonaldException with param/returns/example tags
<!-- AC:END -->

## Implementation Plan

<!-- SECTION:PLAN:BEGIN -->
1. Research Donald exception types by checking Donald documentation or source
2. Add Donald exception imports to Errors.fs
3. Implement mapDonaldException function with all 4 exception types
4. Add parseUniqueConstraintField helper for extracting field names from SQLite errors
5. Handle SQLite error codes 19 (UNIQUE) and 5 (BUSY) specifically
6. Add comprehensive XML documentation with examples
7. Build and verify no errors/warnings
8. Check all acceptance criteria
<!-- SECTION:PLAN:END -->
