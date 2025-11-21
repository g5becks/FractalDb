---
id: task-17
title: Implement SQLite query translator for string operators
status: To Do
assignee: []
created_date: '2025-11-21 02:55'
labels:
  - query
dependencies: []
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Add string operator translation (regex, like, startsWith, endsWith) to SQLite query translator. String operations use SQLite's LIKE, GLOB, and REGEXP capabilities while maintaining parameterization and type safety.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 regex operator translates to SQLite REGEXP with pattern parameter
- [ ] #2 regex options i flag correctly converts pattern to case-insensitive form
- [ ] #3 like operator translates to SQLite LIKE with pattern parameter
- [ ] #4 startsWith translates to LIKE with percent wildcard suffix
- [ ] #5 endsWith translates to LIKE with percent wildcard prefix
- [ ] #6 All string patterns properly escaped and parameterized
- [ ] #7 TypeScript type checking passes with zero errors
- [ ] #8 No any types used in implementation
- [ ] #9 Complete TypeDoc comments with examples showing SQL generation for string matching
<!-- AC:END -->
