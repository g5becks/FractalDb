---
id: task-8
title: Implement SQL Translator - Field Resolution (SqlTranslator.fs Part 1)
status: To Do
assignee: []
created_date: '2025-12-28 06:06'
labels:
  - storage
  - phase-2
dependencies:
  - task-3
  - task-4
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Create the SQL translator structure and field resolution logic in Storage/SqlTranslator.fs. This handles mapping field names to SQL column references.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 TranslatorResult record with Sql and Parameters fields
- [ ] #2 TranslatorResult module with empty and create functions
- [ ] #3 SqlTranslator<'T> class with schema and cache parameters
- [ ] #4 ResolveField private method that maps field names to SQL (indexed fields use _fieldName, non-indexed use jsonb_extract)
- [ ] #5 Translate public method signature that takes Query<'T> and returns TranslatorResult
- [ ] #6 Code compiles successfully with dotnet build
<!-- AC:END -->
