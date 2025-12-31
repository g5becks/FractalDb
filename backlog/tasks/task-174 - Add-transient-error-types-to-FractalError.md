---
id: task-174
title: Add transient error types to FractalError
status: Done
assignee:
  - '@claude'
created_date: '2025-12-31 19:03'
updated_date: '2025-12-31 19:15'
labels: []
dependencies: []
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Add Busy, Locked, IOError, CantOpen, and DiskFull cases to FractalError discriminated union in Errors.fs
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 Add FractalError.Busy case for SQLITE_BUSY (5)
- [x] #2 Add FractalError.Locked case for SQLITE_LOCKED (6)
- [x] #3 Add FractalError.IOError case for SQLITE_IOERR (10)
- [x] #4 Add FractalError.CantOpen case for SQLITE_CANTOPEN (14)
- [x] #5 Add FractalError.DiskFull case for SQLITE_FULL (13)
- [x] #6 Update mapDonaldException to map SQLite error codes to new types
<!-- AC:END -->

## Implementation Plan

<!-- SECTION:PLAN:BEGIN -->
1. Add new FractalError cases (Busy, Locked, IOError, CantOpen, DiskFull) after InvalidOperation
2. Update Message member to handle new cases
3. Update Category member - new cases should be "transient"
4. Update mapDonaldException to map SQLite error codes 5,6,10,13,14 to new types
<!-- SECTION:PLAN:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Added 5 new transient FractalError cases:
- Busy (SQLITE_BUSY, code 5)
- Locked (SQLITE_LOCKED, code 6)
- IOError (SQLITE_IOERR, code 10)
- CantOpen (SQLITE_CANTOPEN, code 14)
- DiskFull (SQLITE_FULL, code 13)

Updated mapDonaldException to map these SQLite error codes.
Added "transient" category for these error types.
Fixed pattern match warnings in tests/Assertions.fs.
<!-- SECTION:NOTES:END -->
