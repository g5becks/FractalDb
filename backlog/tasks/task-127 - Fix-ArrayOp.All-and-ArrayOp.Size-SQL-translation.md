---
id: task-127
title: Fix ArrayOp.All and ArrayOp.Size SQL translation
status: To Do
assignee: []
created_date: '2025-12-29 07:55'
labels:
  - bugfix
  - sql
dependencies: []
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
ArrayOperatorTests revealed these operators match ALL documents instead of filtering. Fix SQL generation to use json_each/json_array_length properly.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 ArrayOp.All generates correct SQL
- [ ] #2 ArrayOp.Size generates correct SQL
- [ ] #3 All 13 ArrayOperatorTests pass
<!-- AC:END -->
