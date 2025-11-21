---
id: task-27
title: Implement Collection interface - validation methods
status: To Do
assignee: []
created_date: '2025-11-21 01:48'
updated_date: '2025-11-21 02:06'
labels:
  - collection
  - validation
dependencies:
  - task-19
  - task-3
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Implement validate() and validateSync() methods for explicit document validation outside of insert/update operations.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Extend Collection class in src/collection/collection.ts
- [ ] #2 Implement validate(doc): Promise<T> async method
- [ ] #3 Call schema.validate() type predicate function
- [ ] #4 Return validated document cast to type T if validation passes
- [ ] #5 Throw SchemaValidationError with detailed message if validation fails
- [ ] #6 Implement validateSync(doc): T synchronous method
- [ ] #7 Identical behavior to validate() but synchronous
- [ ] #8 Add TypeDoc comments explaining validation behavior and use cases
- [ ] #9 All code compiles with strict mode
- [ ] #10 No use of any type except for narrowing unknown to T after validation
<!-- AC:END -->
