---
id: task-81
title: Add TimestampConfig type for customizable timestamps
status: To Do
assignee: []
created_date: '2025-12-29 05:48'
updated_date: '2025-12-29 06:20'
labels:
  - foundation
  - schema
dependencies: []
priority: medium
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Add TimestampConfig type to allow customizing createdAt/updatedAt field names or disabling timestamps entirely.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 TimestampConfig type defined with CreatedAtField, UpdatedAtField, and Enabled fields
- [ ] #2 TimestampConfig.defaults provides standard field names
- [ ] #3 TimestampConfig.disabled helper for disabling timestamps
- [ ] #4 SchemaDef uses TimestampConfig instead of bool for Timestamps field
- [ ] #5 Code builds with no errors or warnings
- [ ] #6 All existing tests pass

- [ ] #7 XML doc comments on TimestampConfig type and all helper functions with examples
<!-- AC:END -->
