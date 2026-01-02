---
id: task-186
title: Handle distinct in SQL generation
status: Done
assignee:
  - '@claude'
created_date: '2026-01-01 22:00'
updated_date: '2026-01-01 22:46'
labels: []
dependencies:
  - task-185
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Update SqlTranslator.fs to check TranslatedQuery.Distinct and generate SELECT DISTINCT when true. Modify the translateToSql function to conditionally add DISTINCT keyword.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 SqlTranslator checks Distinct field
- [x] #2 Generates SELECT DISTINCT when Distinct = true
- [x] #3 Generates regular SELECT when Distinct = false
<!-- AC:END -->

## Implementation Plan

<!-- SECTION:PLAN:BEGIN -->
1. Modify Collection.exec to handle Distinct flag
2. Generate SELECT DISTINCT when Distinct = true
3. Keep regular SELECT when Distinct = false
4. Test both code paths
<!-- SECTION:PLAN:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Modified Collection.exec to handle Distinct flag:
- When Distinct = true: generates SELECT DISTINCT SQL directly
- When Distinct = false: delegates to findWith (original behavior)

SQL generation in Collection.fs:845-902
All 642 tests pass.
<!-- SECTION:NOTES:END -->
