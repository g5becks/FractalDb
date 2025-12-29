---
id: task-34
title: Implement SqlTranslator Class - Field Resolution
status: In Progress
assignee:
  - '@assistant'
created_date: '2025-12-28 06:35'
updated_date: '2025-12-28 18:22'
labels:
  - phase-2
  - storage
  - translator
dependencies:
  - task-33
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Create SqlTranslator class with field resolution logic in src/SqlTranslator.fs. Reference: FSHARP_PORT_DESIGN.md lines 1606-1624.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Add private fieldMap: Map<string, FieldDef> from schema.Fields
- [ ] #2 Add private method ResolveField(fieldName: string) : string
- [ ] #3 ResolveField returns: '_id', 'createdAt', 'updatedAt' unchanged; indexed fields as '_fieldName'; non-indexed as 'jsonb_extract(body, \'$.fieldName\')'
- [ ] #4 Add public method Translate(query: Query<'T>) : TranslatorResult (stub returning TranslatorResult.empty for now)
- [ ] #5 Run 'dotnet build' - build succeeds
- [ ] #6 All public types and functions have XML doc comments with <summary>, and public functions include <param>, <returns>, and <example> (see doc-2 for standards)

- [ ] #7 Run 'task lint' - no errors or warnings

- [ ] #8 In src/SqlTranslator.fs, add 'type SqlTranslator<'T>(schema: SchemaDef<'T>, enableCache: bool)'
<!-- AC:END -->

## Implementation Plan

<!-- SECTION:PLAN:BEGIN -->
1. Read design spec lines 1606-1624 for SqlTranslator class structure
2. Add SqlTranslator<'T> class constructor with schema and enableCache parameters
3. Add private fieldMap built from schema.Fields
4. Add private ResolveField method:
   - Returns metadata fields (_id, createdAt, updatedAt) unchanged
   - Returns indexed fields as _fieldName
   - Returns non-indexed fields as jsonb_extract(body, '$.fieldName')
5. Add public Translate method (stub returning TranslatorResult.empty)
6. Add comprehensive XML documentation
7. Run dotnet build to verify
8. Run task lint to verify
9. Mark all ACs complete
<!-- SECTION:PLAN:END -->
