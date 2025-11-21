---
id: task-46
title: Write integration tests for StrataDB database lifecycle
status: To Do
assignee: []
created_date: '2025-11-21 01:52'
updated_date: '2025-11-21 02:07'
labels:
  - testing
  - database
  - lifecycle
dependencies:
  - task-29
  - task-34
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Test StrataDB database initialization, connection management, and cleanup with various configuration options.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Create test/database/lifecycle.test.ts file
- [ ] #2 Test constructor with string path creates database file
- [ ] #3 Test constructor with factory function allows custom pragma configuration
- [ ] #4 Test custom idGenerator is used for document IDs
- [ ] #5 Test debug mode logs SQL queries when enabled
- [ ] #6 Test close() calls onClose hook before closing connection
- [ ] #7 Test Symbol.dispose automatically closes database
- [ ] #8 Test using declaration properly cleans up resources
- [ ] #9 Test database operations fail after close()
- [ ] #10 All tests pass with bun test
- [ ] #11 Tests compile with strict mode
- [ ] #12 Tests clean up temporary database files
<!-- AC:END -->
