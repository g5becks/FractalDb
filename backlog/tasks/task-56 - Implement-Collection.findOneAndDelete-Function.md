---
id: task-56
title: Implement Collection.findOneAndDelete Function
status: To Do
assignee: []
created_date: '2025-12-28 06:40'
updated_date: '2025-12-28 07:03'
labels:
  - phase-3
  - storage
  - collection
dependencies:
  - task-55
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Add atomic findOneAndDelete operation. Reference: FSHARP_PORT_DESIGN.md lines 1127-1134.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Add 'let findOneAndDelete (filter: Query<'T>) (collection: Collection<'T>) : Task<Document<'T> option>'
- [ ] #2 In single transaction: find document, delete it, return the found document
- [ ] #3 Add 'findOneAndDeleteWith' variant with FindOptions for sorting
- [ ] #4 Return None if no document matched
- [ ] #5 Run 'dotnet build' - build succeeds

- [ ] #6 All public types and functions have XML doc comments with <summary>, and public functions include <param>, <returns>, and <example> (see doc-2 for standards)
<!-- AC:END -->
