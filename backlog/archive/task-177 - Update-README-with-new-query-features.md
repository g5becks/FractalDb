---
id: task-177
title: Update README with new query features
status: Done
assignee:
  - '@claude'
created_date: '2025-11-23 07:31'
updated_date: '2025-11-23 15:36'
labels:
  - documentation
  - phase-5
  - v0.3.0
dependencies:
  - task-176
priority: medium
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Update `README.md` to highlight the new query features in v0.3.0: case-insensitive search, text search across fields, projection helpers, and cursor pagination.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 Add $ilike and $contains to features list
- [x] #2 Add text search feature description
- [x] #3 Add select/omit projection helpers description
- [x] #4 Add cursor pagination feature description
- [x] #5 Add code example showing text search usage
- [x] #6 Add code example showing cursor pagination
- [ ] #7 Update version badge if applicable
<!-- AC:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Updated README.md with:
- Added text search, cursor pagination, field projection to features list
- Added code examples for text search, cursor pagination, and select/omit
<!-- SECTION:NOTES:END -->
