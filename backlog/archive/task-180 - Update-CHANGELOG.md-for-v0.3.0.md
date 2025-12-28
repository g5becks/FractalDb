---
id: task-180
title: Update CHANGELOG.md for v0.3.0
status: Done
assignee:
  - '@claude'
created_date: '2025-11-23 07:31'
updated_date: '2025-11-23 15:41'
labels:
  - documentation
  - phase-5
  - v0.3.0
dependencies:
  - task-179
priority: medium
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Create or update CHANGELOG.md with all changes in v0.3.0 following Keep a Changelog format.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 Add v0.3.0 section with release date
- [x] #2 List all new features under 'Added' section
- [x] #3 Document $ilike operator for case-insensitive LIKE
- [x] #4 Document $contains operator for substring matching
- [x] #5 Document select option for field inclusion
- [x] #6 Document omit option for field exclusion
- [x] #7 Document search option for multi-field text search
- [x] #8 Document cursor option for cursor-based pagination
- [x] #9 Note that all changes are backward compatible
- [x] #10 No 'Changed', 'Deprecated', 'Removed', or 'Breaking' sections needed
<!-- AC:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Created CHANGELOG.md with:
- Dedicated search() method
- $ilike and $contains string operators
- select/omit projection helpers
- Text search option for find()
- Cursor pagination
- New exported types
- Note about backward compatibility
<!-- SECTION:NOTES:END -->
