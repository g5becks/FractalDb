---
id: task-124
title: Add CollectionOptions type for per-collection configuration
status: To Do
assignee: []
created_date: '2025-12-29 06:14'
updated_date: '2025-12-29 06:20'
labels:
  - collection
  - options
dependencies: []
priority: medium
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Add CollectionOptions type that allows overriding database-level settings at the collection level, particularly EnableCache.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 CollectionOptions type defined with EnableCache field (bool option)
- [ ] #2 CollectionOptions.defaults provides None for all fields
- [ ] #3 FractalDb.Collection method accepts optional CollectionOptions
- [ ] #4 Collection uses CollectionOptions.EnableCache or falls back to DbOptions.EnableCache
- [ ] #5 Code builds with no errors or warnings
- [ ] #6 All existing tests pass

- [ ] #7 XML doc comments on CollectionOptions type and defaults with examples
<!-- AC:END -->
