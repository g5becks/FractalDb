---
id: task-176
title: Create queries documentation page
status: Done
assignee:
  - '@claude'
created_date: '2025-11-23 07:31'
updated_date: '2025-11-23 15:35'
labels:
  - documentation
  - phase-5
  - v0.3.0
dependencies:
  - task-175
priority: medium
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Create `docs/guide/queries.md` documenting all query features including the new v0.3.0 features: $ilike, $contains, select/omit, text search, and cursor pagination.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 Create new file `docs/guide/queries.md`
- [x] #2 Document all comparison operators ($eq, $ne, $gt, $gte, $lt, $lte, $in, $nin)
- [x] #3 Document string operators ($like, $ilike, $contains, $startsWith, $endsWith)
- [x] #4 Document logical operators ($and, $or, $nor, $not)
- [x] #5 Document array operators ($all, $size, $elemMatch, $index)
- [x] #6 Document projection options (projection, select, omit)
- [x] #7 Document text search with examples
- [x] #8 Document cursor pagination with examples
- [x] #9 Include performance tips for each feature
- [ ] #10 Add page to docs/.vitepress/config.ts sidebar
<!-- AC:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Updated existing docs/guide/queries.md with:
- $ilike and $contains string operators
- select/omit projection helpers
- Text search with examples
- Cursor pagination with examples and tips
<!-- SECTION:NOTES:END -->
