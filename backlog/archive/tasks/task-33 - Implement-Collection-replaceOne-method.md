---
id: task-33
title: Implement Collection replaceOne method
status: Done
assignee: []
created_date: '2025-11-21 02:57'
updated_date: '2025-11-21 20:00'
labels:
  - collection
dependencies: []
priority: medium
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Implement full document replacement maintaining ID. Unlike updateOne which merges, replaceOne completely replaces the document body while keeping the same ID, useful for document version updates or complete rewrites.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 replaceOne accepts id string and Except<T, 'id'> (full document without id)
- [ ] #2 Method prevents id modification through Except type excluding id field
- [ ] #3 Method validates replacement document before updating
- [ ] #4 Method completely replaces document body in database (no merge)
- [ ] #5 Method preserves existing id field in returned document
- [ ] #6 Method returns Promise<T | null> with replaced document or null if not found
- [ ] #7 TypeScript type checking passes with zero errors
- [ ] #8 No any types used in implementation
- [ ] #9 Complete TypeDoc comments with examples showing difference from updateOne
<!-- AC:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Implementation already exists in codebase - verified working with integration tests
<!-- SECTION:NOTES:END -->
