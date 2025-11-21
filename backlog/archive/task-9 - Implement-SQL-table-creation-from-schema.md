---
id: task-9
title: Implement SQL table creation from schema
status: To Do
assignee: []
created_date: '2025-11-21 01:44'
updated_date: '2025-11-21 02:02'
labels:
  - schema
  - sql
dependencies:
  - task-3
  - task-7
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Create utilities for translating SchemaDefinition to SQLite CREATE TABLE statements with generated columns for indexed fields. This is the bridge between TypeScript schema and SQLite storage.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Create src/schema/sql.ts file
- [ ] #2 Implement generateTableSQL() function accepting collection name and SchemaDefinition
- [ ] #3 Generate CREATE TABLE IF NOT EXISTS with id TEXT PRIMARY KEY and body BLOB columns
- [ ] #4 Generate VIRTUAL GENERATED columns for each indexed field using jsonb_extract()
- [ ] #5 Apply correct SQLite type from SchemaField.type property
- [ ] #6 Apply NOT NULL constraint when field.nullable is false
- [ ] #7 Generate CREATE INDEX statements for indexed fields
- [ ] #8 Generate CREATE UNIQUE INDEX for unique fields
- [ ] #9 Generate CREATE INDEX for compound indexes
- [ ] #10 Handle timestamp fields (createdAt, updatedAt) when timestamps option is true
- [ ] #11 Return object with createTable and createIndexes SQL strings
- [ ] #12 Add TypeDoc comments explaining SQL generation logic
- [ ] #13 Export SQL generation functions from src/schema/index.ts
- [ ] #14 All code compiles with strict mode
- [ ] #15 No use of any or unsafe type assertions
<!-- AC:END -->
