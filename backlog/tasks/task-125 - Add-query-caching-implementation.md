---
id: task-125
title: Add query caching implementation
status: To Do
assignee: []
created_date: '2025-12-29 06:14'
updated_date: '2025-12-29 06:20'
labels:
  - cache
  - performance
dependencies:
  - task-80
priority: low
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Implement basic query caching using ConcurrentDictionary. Cache is keyed by SQL + params hash, with configurable max entries and LRU-style eviction.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 QueryCache type defined with TryGet, Set, Invalidate methods
- [ ] #2 Cache uses ConcurrentDictionary for thread-safety
- [ ] #3 Cache size limited by CacheMaxEntries option
- [ ] #4 Simple eviction when cache is full
- [ ] #5 Invalidate can clear all or by pattern
- [ ] #6 Code builds with no errors or warnings
- [ ] #7 All existing tests pass

- [ ] #8 XML doc comments on QueryCache type with summary, remarks, and method documentation
<!-- AC:END -->
