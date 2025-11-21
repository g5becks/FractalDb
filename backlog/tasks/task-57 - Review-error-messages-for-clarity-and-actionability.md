---
id: task-57
title: Review error messages for clarity and actionability
status: To Do
assignee: []
created_date: '2025-11-21 03:01'
labels:
  - core
dependencies: []
priority: low
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Review all error messages across the library to ensure they provide clear, actionable guidance. Good error messages significantly improve developer experience by explaining what went wrong and how to fix it.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 ValidationError messages include field name, expected type, and actual value
- [ ] #2 QueryError messages explain which operator or query pattern failed and why
- [ ] #3 TypeMismatchError messages suggest correct operators for the field type
- [ ] #4 UniqueConstraintError messages include suggestion to use upsert when appropriate
- [ ] #5 DatabaseError messages include SQLite error codes and helpful context
- [ ] #6 All error messages follow consistent format and tone
- [ ] #7 Error messages provide next steps or suggestions for resolution
- [ ] #8 Complete documentation of error message standards and examples
<!-- AC:END -->
