---
id: task-29
title: Implement Collection insertOne method
status: To Do
assignee: []
created_date: '2025-11-21 02:57'
labels:
  - collection
dependencies: []
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Implement single document insertion with validation and ID generation. This method validates the document, generates an ID if needed, serializes to JSONB, and inserts into SQLite while handling unique constraint violations.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 insertOne accepts DocumentInput<T> parameter and returns Promise<T>
- [ ] #2 Method validates document using schema validator before insertion
- [ ] #3 Method generates ID using ID generator when not provided in input
- [ ] #4 Method prevents ID update by using DocumentInput type (id is optional)
- [ ] #5 Document serialized to JSONB using fast-safe-stringify before storage
- [ ] #6 Unique constraint violations throw UniqueConstraintError with field and value context
- [ ] #7 Returned document includes generated or provided id
- [ ] #8 TypeScript type checking passes with zero errors
- [ ] #9 No any types used in implementation
- [ ] #10 Complete TypeDoc comments with examples showing auto-ID generation and validation
<!-- AC:END -->
