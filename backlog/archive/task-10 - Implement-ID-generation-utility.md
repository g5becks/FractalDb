---
id: task-10
title: Implement ID generation utility
status: To Do
assignee: []
created_date: '2025-11-21 01:44'
updated_date: '2025-11-21 02:02'
labels:
  - utils
  - core
dependencies:
  - task-1
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Create the default ID generator and utilities for managing document IDs. Supports custom ID generators via DatabaseOptions.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Create src/utils/id.ts file
- [ ] #2 Implement IdGenerator type as () => string
- [ ] #3 Implement defaultIdGenerator() function generating unique IDs (use crypto.randomUUID() or nanoid-style)
- [ ] #4 Implement createIdGenerator() accepting optional custom IdGenerator
- [ ] #5 Return defaultIdGenerator if no custom generator provided
- [ ] #6 Add TypeDoc comments explaining ID generation strategy
- [ ] #7 Export ID utilities from src/utils/index.ts
- [ ] #8 All code compiles with strict mode
- [ ] #9 No use of any type
<!-- AC:END -->
