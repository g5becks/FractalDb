---
id: task-20
title: Implement SQLite query translator for existence operator
status: To Do
assignee: []
created_date: '2025-11-21 02:56'
labels:
  - query
dependencies: []
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Add existence operator translation to SQLite query translator. The exists operator checks whether a field is present in the document JSON, using json_type to distinguish between missing fields and null values.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 exists true translates to json_type IS NOT NULL check
- [ ] #2 exists false translates to json_type IS NULL check
- [ ] #3 Existence checks work correctly for both top-level and nested properties
- [ ] #4 Translator correctly uses JSONB path syntax for field access
- [ ] #5 Generated SQL properly distinguishes null values from missing fields
- [ ] #6 TypeScript type checking passes with zero errors
- [ ] #7 No any types used in implementation
- [ ] #8 Complete TypeDoc comments with examples showing SQL generation and null vs undefined behavior
<!-- AC:END -->
