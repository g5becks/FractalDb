# FractalDb Completion Plan

**Date:** 2025-12-30
**Status:** Comprehensive Implementation Plan
**Goal:** Complete all remaining work to consider FractalDb production-ready

---

## Executive Summary

FractalDb is approximately **95% complete**. This document outlines all remaining work to finalize the library, including:

1. **Return Type Migration** - Change `Task<list<>>` to `taskSeq<>` for streaming results
2. **Query Composition** - Fix the `<+>` operator and add comprehensive tests
3. **Unimplemented Features** - Complete `ArrayOp.ElemMatch` and `ArrayOp.Index`
4. **Test Coverage** - Add missing tests and improve existing ones
5. **Documentation** - Update all docs to reflect final API
6. **API Polish** - Ensure consistency across the entire public surface

**Estimated Total Effort:** 25-35 hours

---

## Part 1: Return Type Migration (`list<>` → `taskSeq<>`)

### Background

Currently, all query operations return `Task<list<Document<'T>>>`. This is problematic because:
- All results are loaded into memory at once
- No streaming/lazy evaluation
- Large result sets cause memory pressure
- Not idiomatic for database APIs

### Solution

Use `FSharp.Control.TaskSeq` to provide `IAsyncEnumerable<'T>` support with F# idioms.

### Changes Required

#### 1.1 Add TaskSeq Dependency

**File:** `src/FractalDb.fsproj`

```xml
<PackageReference Include="FSharp.Control.TaskSeq" Version="0.4.*"/>
```

#### 1.2 Collection.fs - Module Functions

**Current Signatures to Change:**

| Line | Function | Current | New |
|------|----------|---------|-----|
| 679 | `find` | `Task<list<Document<'T>>>` | `taskSeq<Document<'T>>` |
| 749 | `findWith` | `Task<list<Document<'T>>>` | `taskSeq<Document<'T>>` |
| 833 | `distinct` | `Task<FractalResult<list<'V>>>` | `Task<FractalResult<taskSeq<'V>>>` |
| 1036 | `search` | `Task<list<Document<'T>>>` | `taskSeq<Document<'T>>` |
| 1123 | `searchWith` | `Task<list<Document<'T>>>` | `taskSeq<Document<'T>>` |

#### 1.3 Collection.fs - Instance Methods

| Line | Method | Current | New |
|------|--------|---------|-----|
| 3317 | `Find` | `Task<list<Document<'T>>>` | `taskSeq<Document<'T>>` |
| 3323 | `Find` (with options) | `Task<list<Document<'T>>>` | `taskSeq<Document<'T>>` |
| 3343 | `Search` | `Task<list<Document<'T>>>` | `taskSeq<Document<'T>>` |
| 3351 | `Search` (with options) | `Task<list<Document<'T>>>` | `taskSeq<Document<'T>>` |

#### 1.4 Collection.fs - TranslatedQuery Extension

| Line | Method | Current | New |
|------|--------|---------|-----|
| 3521 | `exec` | `Task<list<Document<'T>>>` | `taskSeq<Document<'T>>` |

#### 1.5 Internal Implementation Changes

The internal query execution uses Donald's `Db.query` which returns results as a list. Need to:

1. Create a streaming reader that yields documents one at a time
2. Use `taskSeq { ... }` computation expression
3. Ensure proper resource cleanup (connection/reader disposal)

**Key implementation pattern:**

```fsharp
let find (filter: Query<'T>) (collection: Collection<'T>) : taskSeq<Document<'T>> =
    taskSeq {
        use! reader = openReader collection filter
        while! reader.ReadAsync() do
            yield parseDocument reader
    }
```

#### 1.6 Add Convenience Methods for Eager Loading

For users who want lists, add explicit methods:

```fsharp
// On Collection<'T>
member this.FindToList(filter) : Task<list<Document<'T>>> =
    this.Find(filter) |> TaskSeq.toListAsync

// Module function
let findToList filter collection =
    find filter collection |> TaskSeq.toListAsync
```

### Migration Tasks

| Task ID | Description | Priority | Effort |
|---------|-------------|----------|--------|
| M-1 | Add FSharp.Control.TaskSeq package reference | High | 5 min |
| M-2 | Create streaming document reader helper | High | 2 hrs |
| M-3 | Update `Collection.find` to return taskSeq | High | 1 hr |
| M-4 | Update `Collection.findWith` to return taskSeq | High | 1 hr |
| M-5 | Update `Collection.search` to return taskSeq | High | 1 hr |
| M-6 | Update `Collection.searchWith` to return taskSeq | High | 1 hr |
| M-7 | Update `Collection.distinct` to return taskSeq | Medium | 1 hr |
| M-8 | Update all instance methods on Collection<'T> | High | 2 hrs |
| M-9 | Update `TranslatedQuery.exec` extension | High | 30 min |
| M-10 | Add `*ToList` convenience methods | Medium | 1 hr |
| M-11 | Update all tests to handle taskSeq | High | 3 hrs |

---

## Part 2: Query Composition API

### Background

The query composition feature allows combining query expressions:

```fsharp
let filters = query { for u in users do where (u.Active = true) }
let sorting = query { for u in users do sortBy u.Name }
let paging = query { for u in users do take 10 }

let combined = filters <+> sorting <+> paging
```

### Current Issues

1. **`<+>` Operator** - Recently added but needs testing
2. **`.compose()` Method** - Implementation complete
3. **Tests** - 7 new composition tests added but need verification
4. **Type Inference** - Some queries require explicit type annotations

### Composition Rules

When two `TranslatedQuery<'T>` values are composed:

| Component | Merge Strategy |
|-----------|----------------|
| Where | AND together (both must match) |
| OrderBy | Append (left's sorts first, then right's) |
| Skip | Right wins if set, else left |
| Take | Right wins if set, else left |
| Projection | Right wins if non-default |

### Required Changes

#### 2.1 Verify `<+>` Operator Works

**File:** `src/QueryExpr.fs` (lines 509-524)

The operator is defined as:
```fsharp
static member (<+>) (left: TranslatedQuery<'T>, right: TranslatedQuery<'T>) =
    left.compose(right)
```

Need to verify this compiles and works correctly with F# operator resolution.

#### 2.2 Add Module-Level Compose Function

For pipeline-style composition:

```fsharp
module QueryOps =
    let compose (right: TranslatedQuery<'T>) (left: TranslatedQuery<'T>) =
        left.compose(right)
```

Usage:
```fsharp
filters |> QueryOps.compose sorting |> QueryOps.compose paging
```

#### 2.3 Consider Alternative Operators

If `<+>` has issues, alternatives:
- `<&>` - Resembles "and" combination
- `+++` - Common in FParsec and similar
- `|>>` - Pipeline-ish
- Named function only (no operator)

### Composition Tasks

| Task ID | Description | Priority | Effort |
|---------|-------------|----------|--------|
| C-1 | Verify `<+>` operator compiles and works | High | 30 min |
| C-2 | Add `QueryOps.compose` pipeline function | Medium | 15 min |
| C-3 | Add tests for two-query composition | High | 30 min |
| C-4 | Add tests for three+ query composition | High | 30 min |
| C-5 | Add tests for Where clause merging | High | 30 min |
| C-6 | Add tests for OrderBy appending | High | 30 min |
| C-7 | Add tests for Skip/Take precedence | High | 30 min |
| C-8 | Add tests for Projection handling | Medium | 30 min |
| C-9 | Document composition in query-expressions.md | Medium | 30 min |

---

## Part 3: Unimplemented Features

### 3.1 ArrayOp.ElemMatch

**Location:** `src/SqlTranslator.fs` lines 631, 665

**Current State:** Raises `NotImplementedException`

**What it should do:** Match array elements against a sub-query.

```fsharp
// Example: Find users with an address in New York
Query.Field("addresses", FieldOp.Array(box (ArrayOp.ElemMatch(
    Query.Field("city", FieldOp.Compare(box (CompareOp.Eq "New York")))
))))
```

**SQL Translation:**
```sql
-- Using JSON_EACH to iterate array elements
EXISTS (
    SELECT 1 FROM json_each(body->'addresses') AS elem
    WHERE json_extract(elem.value, '$.city') = 'New York'
)
```

### 3.2 ArrayOp.Index

**Location:** `src/SqlTranslator.fs` lines 634, 668

**Current State:** Raises `NotImplementedException`

**What it should do:** Access array element at specific index.

```fsharp
// Example: Check first tag
Query.Field("tags", FieldOp.Array(box (ArrayOp.Index(0,
    FieldOp.Compare(box (CompareOp.Eq "featured"))
))))
```

**SQL Translation:**
```sql
json_extract(body, '$.tags[0]') = 'featured'
```

### Array Operations Tasks

| Task ID | Description | Priority | Effort |
|---------|-------------|----------|--------|
| A-1 | Implement `ArrayOp.ElemMatch` SQL translation | Medium | 2 hrs |
| A-2 | Implement `ArrayOp.Index` SQL translation | Medium | 1 hr |
| A-3 | Add ElemMatch tests for simple conditions | Medium | 30 min |
| A-4 | Add ElemMatch tests for complex nested queries | Medium | 30 min |
| A-5 | Add Index tests for various positions | Medium | 30 min |
| A-6 | Add tests for Index with nested objects | Medium | 30 min |
| A-7 | Update ArrayOperatorTests.fs | Medium | 1 hr |
| A-8 | Document array operations in docs | Low | 30 min |

---

## Part 4: Test Coverage

### Current State

- **Total Tests:** 417
- **Passing:** 416
- **Failing:** 1 (flaky timestamp test)
- **Coverage:** Good for core operations, gaps in edge cases

### Missing Test Categories

#### 4.1 Streaming Tests (After taskSeq migration)

```fsharp
[<Fact>]
let ``find streams results without loading all into memory`` () = ...

[<Fact>]
let ``cancellation token stops streaming`` () = ...

[<Fact>]
let ``taskSeq disposes resources properly`` () = ...
```

#### 4.2 Composition Edge Cases

```fsharp
[<Fact>]
let ``composing empty queries returns empty query`` () = ...

[<Fact>]
let ``composing with conflicting projections takes later`` () = ...

[<Fact>]
let ``deeply nested composition (10+ levels) works`` () = ...
```

#### 4.3 Error Handling Edge Cases

```fsharp
[<Fact>]
let ``concurrent modification during streaming handled gracefully`` () = ...

[<Fact>]
let ``connection loss during query returns proper error`` () = ...
```

#### 4.4 Property-Based Tests (FsCheck)

```fsharp
[<Property>]
let ``composition is associative`` (q1, q2, q3) =
    (q1 <+> q2) <+> q3 = q1 <+> (q2 <+> q3)

[<Property>]
let ``empty query is identity for composition`` (q) =
    q <+> emptyQuery = q
```

### Test Tasks

| Task ID | Description | Priority | Effort |
|---------|-------------|----------|--------|
| T-1 | Fix flaky timestamp test (CrudTests.fs:237) | High | 30 min |
| T-2 | Add streaming behavior tests | High | 2 hrs |
| T-3 | Add cancellation token tests | Medium | 1 hr |
| T-4 | Add resource disposal tests | Medium | 1 hr |
| T-5 | Add composition edge case tests | Medium | 1 hr |
| T-6 | Add FsCheck property tests for composition | Low | 2 hrs |
| T-7 | Add concurrent access tests | Low | 1 hr |
| T-8 | Verify all 417 tests pass after changes | High | 30 min |

---

## Part 5: Documentation Updates

### Files Requiring Updates

#### 5.1 README.md

- Update return type examples (list → taskSeq)
- Add streaming usage examples
- Update query composition examples with `<+>`
- Add "Migrating to taskSeq" section if needed

#### 5.2 docs/query-expressions.md

- Update "Executing Queries" section for taskSeq
- Add "Composing Query Expressions" section
- Add `<+>` operator documentation
- Add streaming/enumeration examples

#### 5.3 docs/getting-started.md

- Update CRUD examples for taskSeq
- Add note about eager loading with `TaskSeq.toListAsync`

#### 5.4 LLMS.txt

- Update all return type signatures
- Add taskSeq patterns
- Add composition operator documentation

#### 5.5 API Documentation (XML Comments)

- Update all changed function signatures
- Add remarks about streaming behavior
- Add examples showing taskSeq usage

### Documentation Tasks

| Task ID | Description | Priority | Effort |
|---------|-------------|----------|--------|
| D-1 | Update README.md with taskSeq examples | High | 1 hr |
| D-2 | Update docs/query-expressions.md | High | 1 hr |
| D-3 | Update docs/getting-started.md | Medium | 30 min |
| D-4 | Update LLMS.txt with new API | Medium | 1 hr |
| D-5 | Add "Streaming Results" documentation page | Medium | 1 hr |
| D-6 | Update all XML doc comments | Medium | 2 hrs |
| D-7 | Review and update API reference docs | Low | 1 hr |

---

## Part 6: API Consistency & Polish

### 6.1 Naming Consistency

Verify all methods follow consistent naming:

| Pattern | Example | Status |
|---------|---------|--------|
| `findX` → finds and returns | `findById`, `findOne` | ✅ |
| `insertX` → creates new | `insertOne`, `insertMany` | ✅ |
| `updateX` → modifies existing | `updateOne`, `updateMany` | ✅ |
| `deleteX` → removes | `deleteOne`, `deleteMany` | ✅ |
| `XToList` → eager loading variant | `findToList` | ❌ Need to add |

### 6.2 Parameter Order Consistency

All module functions should follow: `operation params → collection → result`

```fsharp
// Correct pattern
let find filter collection = ...
let insertOne doc collection = ...
let updateById id update collection = ...
```

Verify all functions follow this pattern.

### 6.3 Instance Method Parity

Every module function should have a corresponding instance method:

| Module Function | Instance Method | Status |
|-----------------|-----------------|--------|
| `Collection.find` | `collection.Find()` | ✅ |
| `Collection.findOne` | `collection.FindOne()` | ✅ |
| `Collection.insertOne` | `collection.InsertOne()` | ✅ |
| `Collection.exec` | `query.exec(collection)` | ✅ |
| `Collection.findToList` | `collection.FindToList()` | ❌ Need |

### 6.4 Error Message Quality

Review all error messages for clarity:

```fsharp
// Bad
Error "Failed"

// Good
Error $"Failed to insert document: unique constraint violated on field 'email' with value '{email}'"
```

### Polish Tasks

| Task ID | Description | Priority | Effort |
|---------|-------------|----------|--------|
| P-1 | Add `*ToList` eager loading methods | High | 1 hr |
| P-2 | Verify parameter order consistency | Medium | 30 min |
| P-3 | Ensure instance method parity | Medium | 1 hr |
| P-4 | Review and improve error messages | Low | 1 hr |
| P-5 | Add `[<Obsolete>]` to any deprecated APIs | Low | 30 min |

---

## Part 7: Library.fs Public API Surface

### Required Updates to Library.fs

After all changes, Library.fs needs to export:

```fsharp
// New types to export
type taskSeq<'T> = FSharp.Control.TaskSeq.taskSeq<'T>  // Or re-export module

// New modules to export
module QueryOps = FractalDb.QueryExpr.QueryOps
module TaskSeq = FSharp.Control.TaskSeq  // Optional convenience
```

### Library.fs Tasks

| Task ID | Description | Priority | Effort |
|---------|-------------|----------|--------|
| L-1 | Export QueryOps module | High | 15 min |
| L-2 | Review and update all type exports | Medium | 30 min |
| L-3 | Update XML documentation | Medium | 30 min |
| L-4 | Verify all public API is accessible via `open FractalDb` | High | 15 min |

---

## Part 8: Build & Project Configuration

### 8.1 Project File Updates

```xml
<!-- src/FractalDb.fsproj -->
<ItemGroup>
    <PackageReference Include="FSharp.Control.TaskSeq" Version="0.4.*"/>
</ItemGroup>
```

### 8.2 Test Project Updates

```xml
<!-- tests/FractalDb.Tests.fsproj -->
<ItemGroup>
    <PackageReference Include="FSharp.Control.TaskSeq" Version="0.4.*"/>
</ItemGroup>
```

### 8.3 Version Bump

Consider versioning strategy for breaking changes:
- Current: Pre-release (no version specified)
- After taskSeq: Could be 1.0.0 or 0.1.0

### Config Tasks

| Task ID | Description | Priority | Effort |
|---------|-------------|----------|--------|
| B-1 | Add TaskSeq package to src project | High | 5 min |
| B-2 | Add TaskSeq package to test project | High | 5 min |
| B-3 | Update version if needed | Low | 5 min |
| B-4 | Verify clean build with all changes | High | 10 min |

---

## Implementation Order (Recommended)

### Phase 1: Foundation (Day 1)
1. B-1, B-2: Add TaskSeq dependencies
2. M-1: Verify package works
3. T-1: Fix flaky timestamp test
4. C-1: Verify `<+>` operator works

### Phase 2: Core Migration (Days 2-3)
1. M-2: Create streaming document reader
2. M-3 to M-6: Update module functions
3. M-8: Update instance methods
4. M-9: Update TranslatedQuery.exec

### Phase 3: Convenience & Polish (Day 4)
1. M-10: Add ToList convenience methods
2. P-1 to P-3: API consistency
3. L-1 to L-4: Library.fs updates

### Phase 4: Tests (Day 5)
1. M-11: Update existing tests for taskSeq
2. T-2 to T-5: Add new tests
3. T-8: Verify all tests pass

### Phase 5: Array Operations (Day 6)
1. A-1, A-2: Implement ElemMatch and Index
2. A-3 to A-7: Add tests

### Phase 6: Composition (Day 7)
1. C-2 to C-8: Composition tests and polish

### Phase 7: Documentation (Day 8)
1. D-1 to D-7: Update all documentation

### Phase 8: Final Verification
1. Full test suite pass
2. Build verification
3. Documentation review
4. Clean up temporary files

---

## Complete Task Checklist

### Migration Tasks (M)
- [ ] M-1: Add FSharp.Control.TaskSeq package reference
- [ ] M-2: Create streaming document reader helper
- [ ] M-3: Update `Collection.find` to return taskSeq
- [ ] M-4: Update `Collection.findWith` to return taskSeq
- [ ] M-5: Update `Collection.search` to return taskSeq
- [ ] M-6: Update `Collection.searchWith` to return taskSeq
- [ ] M-7: Update `Collection.distinct` to return taskSeq
- [ ] M-8: Update all instance methods on Collection<'T>
- [ ] M-9: Update `TranslatedQuery.exec` extension
- [ ] M-10: Add `*ToList` convenience methods
- [ ] M-11: Update all tests to handle taskSeq

### Composition Tasks (C)
- [ ] C-1: Verify `<+>` operator compiles and works
- [ ] C-2: Add `QueryOps.compose` pipeline function
- [ ] C-3: Add tests for two-query composition
- [ ] C-4: Add tests for three+ query composition
- [ ] C-5: Add tests for Where clause merging
- [ ] C-6: Add tests for OrderBy appending
- [ ] C-7: Add tests for Skip/Take precedence
- [ ] C-8: Add tests for Projection handling
- [ ] C-9: Document composition in query-expressions.md

### Array Operation Tasks (A)
- [ ] A-1: Implement `ArrayOp.ElemMatch` SQL translation
- [ ] A-2: Implement `ArrayOp.Index` SQL translation
- [ ] A-3: Add ElemMatch tests for simple conditions
- [ ] A-4: Add ElemMatch tests for complex nested queries
- [ ] A-5: Add Index tests for various positions
- [ ] A-6: Add tests for Index with nested objects
- [ ] A-7: Update ArrayOperatorTests.fs
- [ ] A-8: Document array operations in docs

### Test Tasks (T)
- [ ] T-1: Fix flaky timestamp test (CrudTests.fs:237)
- [ ] T-2: Add streaming behavior tests
- [ ] T-3: Add cancellation token tests
- [ ] T-4: Add resource disposal tests
- [ ] T-5: Add composition edge case tests
- [ ] T-6: Add FsCheck property tests for composition
- [ ] T-7: Add concurrent access tests
- [ ] T-8: Verify all tests pass after changes

### Documentation Tasks (D)
- [ ] D-1: Update README.md with taskSeq examples
- [ ] D-2: Update docs/query-expressions.md
- [ ] D-3: Update docs/getting-started.md
- [ ] D-4: Update LLMS.txt with new API
- [ ] D-5: Add "Streaming Results" documentation page
- [ ] D-6: Update all XML doc comments
- [ ] D-7: Review and update API reference docs

### Polish Tasks (P)
- [ ] P-1: Add `*ToList` eager loading methods
- [ ] P-2: Verify parameter order consistency
- [ ] P-3: Ensure instance method parity
- [ ] P-4: Review and improve error messages
- [ ] P-5: Add `[<Obsolete>]` to any deprecated APIs

### Library Tasks (L)
- [ ] L-1: Export QueryOps module
- [ ] L-2: Review and update all type exports
- [ ] L-3: Update XML documentation
- [ ] L-4: Verify all public API is accessible

### Build Tasks (B)
- [ ] B-1: Add TaskSeq package to src project
- [ ] B-2: Add TaskSeq package to test project
- [ ] B-3: Update version if needed
- [ ] B-4: Verify clean build with all changes

---

## Success Criteria

The library is considered **complete** when:

1. ✅ All 60+ tasks above are completed
2. ✅ All tests pass (currently 417, expect ~450+ after additions)
3. ✅ No `NotImplementedException` in production code
4. ✅ All public APIs documented with XML comments
5. ✅ README, docs/, and LLMS.txt updated
6. ✅ Clean build with no warnings
7. ✅ taskSeq used for all streaming results
8. ✅ Query composition works with `<+>` operator
9. ✅ Both module functions and instance methods available
10. ✅ Eager loading alternatives (`*ToList`) available

---

## Appendix A: File Change Summary

| File | Changes Required |
|------|------------------|
| `src/FractalDb.fsproj` | Add TaskSeq package |
| `src/Collection.fs` | Major - return types, streaming impl |
| `src/QueryExpr.fs` | Minor - QueryOps module |
| `src/SqlTranslator.fs` | Medium - ElemMatch/Index impl |
| `src/Library.fs` | Minor - exports |
| `tests/FractalDb.Tests.fsproj` | Add TaskSeq package |
| `tests/QueryExprTests.fs` | Major - composition tests |
| `tests/CrudTests.fs` | Minor - fix flaky test |
| `tests/*.fs` | Medium - update for taskSeq |
| `README.md` | Medium - examples |
| `docs/*.md` | Medium - all guides |
| `LLMS.txt` | Medium - API reference |

---

## Appendix B: Breaking Changes

The following are **breaking changes** for existing users:

1. **Return Types**: `Task<list<Document<'T>>>` → `taskSeq<Document<'T>>`
   - **Migration**: Add `|> TaskSeq.toListAsync` to get old behavior

2. **Query Composition**: New `<+>` operator
   - **Migration**: None needed, additive feature

3. **Package Dependency**: New `FSharp.Control.TaskSeq` dependency
   - **Migration**: None needed, transitive

---

## Appendix C: Post-Completion Considerations

After completion, consider:

1. **NuGet Package Publishing** - Publish to NuGet.org
2. **CI/CD Pipeline** - GitHub Actions for automated testing
3. **Benchmarks** - Update BenchmarkDotNet project for new APIs
4. **Migration Guide** - If breaking changes affect users
5. **Version 1.0** - Official stable release

---

*This document serves as the complete roadmap for finishing FractalDb. When all tasks are checked off, the library is production-ready.*
