---
id: task-16
title: Implement SQLite query translator for comparison operators
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
Create SQLite-specific query translation for comparison operators (eq, ne, gt, gte, lt, lte, in, nin). This translator uses generated columns for indexed fields and jsonb_extract for non-indexed fields, generating parameterized SQL to prevent injection attacks.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 SQLiteQueryTranslator class implements QueryTranslator<T> interface
- [ ] #2 Class accepts schema and table name in constructor
- [ ] #3 Translator generates SQL using generated column names for indexed fields (underscore prefix)
- [ ] #4 Translator uses jsonb_extract for non-indexed fields
- [ ] #5 All comparison operators correctly translated to SQL equivalents with placeholders
- [ ] #6 in and nin operators generate correct IN and NOT IN clauses with multiple placeholders
- [ ] #7 Generated SQL is properly parameterized with no string interpolation of values
- [ ] #8 TypeScript type checking passes with zero errors
- [ ] #9 No any types used in implementation
- [ ] #10 Complete TypeDoc comments with examples showing SQL generation for each operator
<!-- AC:END -->
