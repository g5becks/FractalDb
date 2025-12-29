---
id: task-118
title: Wire up QueryBuilder.Run to translate and execute
status: In Progress
assignee:
  - '@assistant'
created_date: '2025-12-29 06:12'
updated_date: '2025-12-29 17:28'
labels:
  - query-expressions
  - integration
dependencies:
  - task-117
  - task-106
  - task-107
  - task-108
  - task-109
  - task-110
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Complete the QueryBuilder.Run implementation to call QueryTranslator.translate and QueryExecutor.execute. This makes query expressions functional end-to-end.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Run member calls QueryTranslator.translate
- [ ] #2 Run member calls QueryExecutor.execute
- [ ] #3 Query expressions can now execute against database
- [ ] #4 Code builds with no errors or warnings
- [ ] #5 All existing tests pass

- [ ] #6 XML doc comments on Run member explaining the quotation-to-execution flow
<!-- AC:END -->
