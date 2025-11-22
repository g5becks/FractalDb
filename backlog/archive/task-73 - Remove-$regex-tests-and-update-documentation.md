---
id: task-73
title: Remove $regex tests and update documentation
status: Done
assignee:
  - '@claude'
created_date: '2025-11-22 06:27'
updated_date: '2025-11-22 15:02'
labels:
  - mongodb-compat
  - docs
  - tests
dependencies:
  - task-72
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Remove any tests that use $regex and update documentation to clarify that regex is not supported.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Remove $regex test cases from test/integration/complex-queries.test.ts if present
- [ ] #2 Remove $regex test cases from test/unit/sqlite-query-translator-simple.test.ts if present
- [ ] #3 Remove $regex benchmarks from bench/query-translation.bench.ts
- [ ] #4 Update docs/guide/queries.md to remove $regex examples and note it's not supported
- [ ] #5 Run `bun run typecheck` - should pass
- [ ] #6 Run `bun run lint` - no linter errors
- [ ] #7 Run `bun test` - all tests must pass
<!-- AC:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Successfully removed all  tests and updated documentation throughout the codebase. Changes include:\n\n1. **Test Files Updated:**\n   - Removed skipped  test from test/integration/complex-queries.test.ts\n   - Removed 4  test cases and regex constants from test/unit/sqlite-query-translator-simple.test.ts\n   - Removed 2  benchmark cases and constants from bench/query-translation.bench.ts\n   - Updated query cache tests to use  instead of \n   - Updated error message tests to expect new string operators\n\n2. **Documentation Updated:**\n   - Removed  example from docs/guide/quick-reference.md and added note about alternatives\n   - Updated  examples to use  in docs/api/type-aliases/QueryFilter.md\n   - Updated  examples in docs/api/type-aliases/LogicalOperator.md and FieldOperator.md\n   - Updated operator suggestions in src/error-messages.ts\n\n3. **Verification:**\n   - All 347 tests pass (0 failures)\n   - Type checking passes with no errors\n   - Lint shows only pre-existing style issues unrelated to changes\n\nThe  operator has been completely removed from tests, documentation, and error messages, providing clear guidance to users about supported alternatives (, , ).
<!-- SECTION:NOTES:END -->
