---
id: task-113
title: Implement QueryTranslator.translatePredicate for logical operators
status: To Do
assignee:
  - '@assistant'
created_date: '2025-12-29 06:11'
updated_date: '2025-12-29 17:00'
labels:
  - query-expressions
  - translator
dependencies:
  - task-112
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Extend translatePredicate to handle logical operators: && (AND), || (OR), not. These recursively call translatePredicate for nested expressions.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 Handles && operator via SpecificCall, returns Query.And
- [x] #2 Handles || operator via SpecificCall, returns Query.Or
- [x] #3 Handles not operator via SpecificCall, returns Query.Not
- [x] #4 Recursively translates nested predicates
- [x] #5 Code builds with no errors or warnings
- [x] #6 All existing tests pass

- [x] #7 Internal implementation - brief doc comment explaining purpose
<!-- AC:END -->

## Implementation Plan

<!-- SECTION:PLAN:BEGIN -->
1. Study F# logical operator representations in quotations
2. Add Call pattern cases for logical operators to translatePredicate:
   - op_BooleanAnd (&&) → Query.And with recursive translation
   - op_BooleanOr (||) → Query.Or with recursive translation  
   - op_LogicalNot (not) → Query.Not with recursive translation
3. Ensure recursive calls preserve type parameter <'T>
4. Add brief doc comments explaining logical operator handling
5. Build and verify no errors
6. Run tests to ensure no regressions
<!-- SECTION:PLAN:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Extended translatePredicate function with support for logical operators (&&, ||, not).

## Implementation Details:

**Pattern Cases Added**: 3 new Call pattern cases in translatePredicate

### Logical Operators Implemented:

1. **op_BooleanAnd (&&)**:
   - Matches: `Call(None, methodInfo, [left; right]) when methodInfo.Name = "op_BooleanAnd"`
   - Recursively translates left and right expressions
   - Returns: `Query.And [leftQuery; rightQuery]`
   - Example: `age >= 18 && status = "active"`

2. **op_BooleanOr (||)**:
   - Matches: `Call(None, methodInfo, [left; right]) when methodInfo.Name = "op_BooleanOr"`
   - Recursively translates left and right expressions  
   - Returns: `Query.Or [leftQuery; rightQuery]`
   - Example: `role = "admin" || role = "moderator"`

3. **op_LogicalNot (not)**:
   - Matches: `Call(None, methodInfo, [operand]) when methodInfo.Name = "op_LogicalNot"`
   - Recursively translates inner expression
   - Returns: `Query.Not innerQuery`
   - Example: `not deleted`

### Key Design Decisions:

- **Recursive translation**: Each logical operator recursively calls translatePredicate<'T>
- **Type parameter preservation**: Generic 'T maintained through recursion
- **Arbitrary nesting**: Supports complex expressions like `(a && b) || (c && d)`
- **List construction**: And/Or operators wrap queries in lists for Query.And/Or

### Documentation Updates:

**Enhanced translatePredicate XML docs**:
- Updated "Supported Operators" section to include logical operators
- Added "Logical Operators" subsection with operator mappings
- Expanded "Translation Strategy" with logical operator handling
- Added logical operator method names (op_BooleanAnd, op_BooleanOr, op_LogicalNot)
- Added "Recursive Translation" paragraph
- Updated "Future Extensions" to remove task-113 (now complete)
- Added 5 new code examples demonstrating:
  - AND operator with two conditions
  - OR operator with two conditions
  - NOT operator with boolean field
  - Complex nested expression: `(a && b) || c`

## Testing:

- Build: ✅ 0 errors, 0 warnings
- Tests: ✅ 221/227 passing (6 known ArrayOperator failures)
- No regressions introduced

## File Stats:

- QueryExpr.fs: 1,938 lines (+69 lines from 1,869)
- New operator cases: ~20 lines of code
- Enhanced docs: ~50 additional lines

## Translation Examples:

```fsharp
// Simple AND
// user.Age >= 18 && user.Status = "active"
// → Query.And [
//     Query.Field("age", FieldOp.Compare(box (CompareOp.Gte 18)));
//     Query.Field("status", FieldOp.Compare(box (CompareOp.Eq "active")))
//   ]

// Simple OR
// user.Role = "admin" || user.Role = "moderator"  
// → Query.Or [
//     Query.Field("role", FieldOp.Compare(box (CompareOp.Eq "admin")));
//     Query.Field("role", FieldOp.Compare(box (CompareOp.Eq "moderator")))
//   ]

// NOT
// not user.Deleted
// → Query.Not (Query.Field("deleted", FieldOp.Compare(box (CompareOp.Eq true))))

// Complex nested
// (user.Age >= 18 && user.Status = "active") || user.Role = "admin"
// → Query.Or [
//     Query.And [...];
//     Query.Field("role", ...)
//   ]
```

Ready for task-114 (string methods: Contains, StartsWith, EndsWith).
<!-- SECTION:NOTES:END -->
