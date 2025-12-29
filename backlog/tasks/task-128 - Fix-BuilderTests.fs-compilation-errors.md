---
id: task-128
title: Fix BuilderTests.fs compilation errors
status: To Do
assignee: []
created_date: '2025-12-29 07:55'
labels:
  - bugfix
  - tests
  - technical-debt
dependencies: []
priority: medium
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
BuilderTests.fs has approximately 75 compilation errors that prevent test execution. These are pre-existing issues unrelated to recent test additions. Errors include missing FieldOp namespace, incorrect type usage, and missing QueryOptions.Projection member.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 BuilderTests.fs compiles without errors
- [ ] #2 All BuilderTests pass
- [ ] #3 No warnings related to BuilderTests
<!-- AC:END -->
