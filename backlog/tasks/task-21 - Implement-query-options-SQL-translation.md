---
id: task-21
title: Implement query options SQL translation
status: In Progress
assignee:
  - '@claude'
created_date: '2025-11-21 02:56'
updated_date: '2025-11-21 06:44'
labels:
  - query
dependencies: []
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Add translation for query options (sort, limit, skip, projection) to complete SQL query generation. These options control result ordering, pagination, and field selection in the final SQL statement.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Sort specification translates to ORDER BY clause with field names and ASC/DESC
- [ ] #2 Limit translates to SQL LIMIT clause with parameterized value
- [ ] #3 Skip translates to SQL OFFSET clause with parameterized value
- [ ] #4 Projection includes/excludes fields in SELECT clause appropriately
- [ ] #5 Sort uses generated column names for indexed fields, jsonb_extract for non-indexed
- [ ] #6 Multiple sort fields generate comma-separated ORDER BY expressions
- [ ] #7 TypeScript type checking passes with zero errors
- [ ] #8 No any types used in implementation
- [ ] #9 Complete TypeDoc comments with examples showing complete SELECT query generation
<!-- AC:END -->

## Implementation Plan

<!-- SECTION:PLAN:BEGIN -->
1. Review QueryOptions type (sort, limit, skip, projection)
2. Implement translateOptions method replacing placeholder
3. Handle sort specification: convert to ORDER BY clause
   - Use resolveFieldName for indexed vs non-indexed fields
   - Map 1 to ASC, -1 to DESC
   - Join multiple sort fields with commas
4. Handle limit: add LIMIT clause with parameterized value
5. Handle skip: add OFFSET clause with parameterized value
6. Note: Projection handled by collection layer, not translator
7. Add comprehensive TypeDoc documentation
8. Verify TypeScript compilation and linting
<!-- SECTION:PLAN:END -->
