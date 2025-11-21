---
id: task-17
title: 'Implement query options translator (sort, limit, skip)'
status: To Do
assignee: []
created_date: '2025-11-21 01:46'
updated_date: '2025-11-21 02:03'
labels:
  - query
  - translator
dependencies:
  - task-12
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Extend the query translator to handle QueryOptions including sort, limit (LIMIT), and skip (OFFSET) clauses. These options control result ordering and pagination.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Extend QueryTranslator in src/query/translator.ts
- [ ] #2 Implement translateSort() method accepting SortSpec<T>
- [ ] #3 Generate ORDER BY clause with field names and ASC/DESC based on 1/-1 values
- [ ] #4 Use generated column names for indexed fields in ORDER BY
- [ ] #5 Use jsonb_extract() for non-indexed fields in ORDER BY
- [ ] #6 Support multiple sort fields with comma separation
- [ ] #7 Implement translateLimit() generating LIMIT N clause
- [ ] #8 Implement translateSkip() generating OFFSET N clause
- [ ] #9 Combine sort, limit, skip into complete SQL query suffix
- [ ] #10 Add TypeDoc comments explaining query option translation
- [ ] #11 All code compiles with strict mode
- [ ] #12 No use of any type
<!-- AC:END -->
