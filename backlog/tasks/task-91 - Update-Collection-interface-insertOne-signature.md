---
id: task-91
title: Update Collection interface insertOne signature
status: Done
assignee:
  - '@claude'
created_date: '2025-11-22 19:04'
updated_date: '2025-11-22 20:06'
labels: []
dependencies: []
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Change insertOne return type from InsertOneResult<T> to T in Collection interface in src/collection-types.ts
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 Type compiles without errors
- [x] #2 Linting passes
- [x] #3 Return type changed to T
- [x] #4 JSDoc examples show direct document access
- [x] #5 Full typedocs included
<!-- AC:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Updated Collection interface insertOne signature from Promise<InsertOneResult<T>> to Promise<T>. Updated JSDoc to reflect direct document access - changed example from result.document._id to user._id and added user.name access. Updated parameter description to reference _id instead of id. Type checking passes cleanly. Implementation now aligns with the updated sqlite-collection.ts which returns fullDoc directly.
<!-- SECTION:NOTES:END -->
