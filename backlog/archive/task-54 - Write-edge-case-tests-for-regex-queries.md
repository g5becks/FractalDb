---
id: task-54
title: Write edge case tests for regex queries
status: To Do
assignee: []
created_date: '2025-11-21 01:53'
updated_date: '2025-11-21 02:07'
labels:
  - testing
  - edge-cases
  - regex
dependencies:
  - task-13
priority: medium
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Test regex operator behavior including case sensitivity, special character escaping, and regex options.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Create test/edge-cases/regex.test.ts file
- [ ] #2 Test  is case-sensitive by default
- [ ] #3 Test  with off on off off off off off on off off on off on off on off off off on off off off on off off off off on on off off off off off on off on off off off off on off on off off off off off on off on on off off off on off off on off off on off off on on on off on on off on off off on off on off off off on off off on off on off off off on on off on off on off off on off off off on off off off off off off off off on off on off on on on off on on off off on off off on off off on on off on off on on off off off off off on off off on off off off off off on off off on on off on off off off off off off on off off off off on on off on off off off off off off off on on off off on off off off off off off off on off off off off off: 'i' flag for case-insensitive matching
- [ ] #4 Test regex with special characters (user must escape)
- [ ] #5 Test regex patterns work correctly with SQLite REGEXP
- [ ] #6 Test invalid regex patterns throw appropriate errors
- [ ] #7 All tests pass with bun test
- [ ] #8 Tests compile with strict mode
<!-- AC:END -->
