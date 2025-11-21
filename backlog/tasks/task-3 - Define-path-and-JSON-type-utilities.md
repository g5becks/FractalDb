---
id: task-3
title: Define path and JSON type utilities
status: To Do
assignee: []
created_date: '2025-11-21 02:29'
labels:
  - core
  - types
dependencies: []
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Implement type-safe path utilities for nested document property access using type-fest. These types enable compile-time validation of dot-notation paths in queries, ensuring developers cannot query non-existent properties. This is critical for query type safety.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 DocumentPath<T> type defined using type-fest Paths<T> utility
- [ ] #2 PathValue<T, P> type defined using type-fest Get<T, P> utility
- [ ] #3 JsonPath type defined as template literal starting with dollar-dot prefix
- [ ] #4 Types correctly generate all valid nested paths for test document structures
- [ ] #5 PathValue correctly extracts types at nested paths
- [ ] #6 TypeScript type checking passes with zero errors
- [ ] #7 No any types used anywhere in implementation
- [ ] #8 Complete TypeDoc comments with examples demonstrating nested path extraction
<!-- AC:END -->
