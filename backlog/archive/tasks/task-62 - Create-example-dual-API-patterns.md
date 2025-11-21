---
id: task-62
title: 'Create example: dual API patterns'
status: To Do
assignee: []
created_date: '2025-11-21 01:55'
labels:
  - examples
  - documentation
  - api
dependencies: []
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Create an example showing both fluent and declarative API patterns side-by-side to help users choose their preferred style.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Create examples/dual-api-patterns.ts file
- [ ] #2 Define User type once for both patterns
- [ ] #3 Show fluent API: db.collection<User>('users').field(...).build()
- [ ] #4 Show declarative API: createSchema<User>() then db.collection<User>('users', schema)
- [ ] #5 Demonstrate that both produce equivalent collections
- [ ] #6 Explain when to use each pattern in comments
- [ ] #7 Show operations work identically with both
- [ ] #8 Example runs successfully
- [ ] #9 Example compiles with strict mode
<!-- AC:END -->
