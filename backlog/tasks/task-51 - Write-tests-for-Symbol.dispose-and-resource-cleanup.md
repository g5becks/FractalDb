---
id: task-51
title: Write tests for Symbol.dispose and resource cleanup
status: To Do
assignee: []
created_date: '2025-11-21 03:00'
labels:
  - testing
dependencies: []
priority: medium
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Create tests verifying automatic resource cleanup using Symbol.dispose for databases and transactions. These tests ensure resources are properly released in all scenarios including error conditions.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Tests verify database Symbol.dispose closes connection and calls onClose hook
- [ ] #2 Tests verify database cleanup occurs even when errors thrown in scope
- [ ] #3 Tests verify transaction Symbol.dispose rolls back uncommitted changes
- [ ] #4 Tests verify transaction cleanup occurs when callback throws error
- [ ] #5 Tests verify multiple using statements dispose in reverse order
- [ ] #6 Tests verify SuppressedError aggregation when disposal fails
- [ ] #7 All tests pass when running test suite
- [ ] #8 Complete test descriptions documenting resource cleanup behavior
<!-- AC:END -->
