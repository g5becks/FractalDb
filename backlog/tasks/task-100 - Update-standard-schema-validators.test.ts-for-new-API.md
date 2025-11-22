---
id: task-100
title: Update standard-schema-validators.test.ts for new API
status: Done
assignee:
  - '@claude'
created_date: '2025-11-22 19:08'
updated_date: '2025-11-22 20:38'
labels: []
dependencies:
  - task-97
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Replace all .document. patterns with direct property access in test/integration/standard-schema-validators.test.ts (12 occurrences)
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 All tests pass
- [x] #2 Type checking passes
- [x] #3 Linting passes
- [x] #4 All .document. patterns replaced
<!-- AC:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Successfully updated standard-schema-validators.test.ts for new API. Replaced all 12 instances of .document._id with direct ._id access across result, zodResult, valibotResult, and arkResult variables. All tests pass (14/14), type checking passes, and linting shows only pre-existing issues. Tests now properly reflect the new API where insertOne returns the document directly across all validator implementations (Standard Schema, Zod, Valibot, ArkType).
<!-- SECTION:NOTES:END -->
