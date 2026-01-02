# Query Operators Expansion Design Document

## Overview

This document outlines the implementation plan for expanding FractalDb's query expression support to include additional F# query operators. FractalDb is a document database - joins are explicitly out of scope as data should be embedded/denormalized.

## Current State (v1.3.0)

### Supported Operators
- `for...in...do` - Collection iteration
- `select` - Projection (SelectAll, SelectFields, SelectSingle)
- `where` - Filtering with predicates
- `sortBy` / `sortByDescending` - Primary sorting
- `thenBy` / `thenByDescending` - Secondary sorting
- `take` / `skip` - Pagination
- `count` - Count documents
- `head` / `headOrDefault` - First element
- `exists` - Check if any match predicate
- `String.Contains/StartsWith/EndsWith` - String matching
- `List.contains` / `Array.contains` - IN operator
- `&&` / `||` / `not` - Logical operators

### Existing Infrastructure
- `StringOp.Like` / `StringOp.ILike` already exist in `Operators.fs`
- SQL LIKE translation already exists in `SqlTranslator.fs`
- Query AST supports extensible `FieldOp` and `Query<'T>` types

---

## Phase 1: Pattern Matching & Simple Operators

### 1.1 SQL LIKE Pattern Matching

**Priority:** HIGH  
**Complexity:** LOW  
**Files:** `src/QueryExpr.fs`, `src/Library.fs`, `tests/QueryExprTests.fs`

#### Problem
Users cannot use SQL LIKE wildcards (`%`, `_`, `[abc]`, `[^abc]`) in query expressions. The `String.Contains/StartsWith/EndsWith` methods only cover simple cases.

#### Solution
Create a `Sql` module with `like` and `ilike` marker functions that get translated in query expressions.

#### API Design
```fsharp
// New module in Library.fs
module Sql =
    /// SQL LIKE pattern matching (case-sensitive)
    /// Wildcards: % (any chars), _ (single char), [abc] (char set), [^abc] (negated set)
    let like (pattern: string) (field: string) : bool =
        raise (InvalidOperationException("Sql.like can only be used in query expressions"))
    
    /// SQL LIKE pattern matching (case-insensitive)
    let ilike (pattern: string) (field: string) : bool =
        raise (InvalidOperationException("Sql.ilike can only be used in query expressions"))
```

#### Usage
```fsharp
open FractalDb

// Case-sensitive LIKE
query {
    for user in users do
    where (Sql.like "_e%" user.Name)  // Second char is 'e'
    select user
}

// Case-insensitive LIKE
query {
    for user in users do
    where (Sql.ilike "admin%" user.Email)  // Starts with 'admin' (any case)
    select user
}

// Pattern matching examples
where (Sql.like "[abc]%" user.Name)   // Starts with a, b, or c
where (Sql.like "[^0-9]%" user.Code)  // Doesn't start with digit
where (Sql.like "___-___" user.Zip)   // Format: 3 chars, hyphen, 3 chars
```

#### Implementation
```fsharp
// In QueryExpr.fs translatePredicate, add before unsupported pattern:

// Sql.like: Sql.like pattern field
| Call(None, methodInfo, [ patternExpr; fieldExpr ]) 
    when methodInfo.DeclaringType <> null 
      && methodInfo.DeclaringType.Name = "Sql" 
      && methodInfo.Name = "like" ->
    let field = extractPropertyName fieldExpr
    let pattern = evaluateExpr patternExpr :?> string
    Query.Field(field, FieldOp.String(StringOp.Like pattern))

// Sql.ilike: Sql.ilike pattern field
| Call(None, methodInfo, [ patternExpr; fieldExpr ]) 
    when methodInfo.DeclaringType <> null 
      && methodInfo.DeclaringType.Name = "Sql" 
      && methodInfo.Name = "ilike" ->
    let field = extractPropertyName fieldExpr
    let pattern = evaluateExpr patternExpr :?> string
    Query.Field(field, FieldOp.String(StringOp.ILike pattern))
```

#### Tests
- `Sql.like with % wildcard generates correct SQL`
- `Sql.like with _ wildcard generates correct SQL`
- `Sql.like with character set [abc] generates correct SQL`
- `Sql.ilike generates case-insensitive SQL`
- `Integration: Sql.like returns matching documents`
- `Integration: Sql.ilike is case-insensitive`

---

### 1.2 `distinct` Operator

**Priority:** HIGH  
**Complexity:** LOW  
**Files:** `src/QueryExpr.fs`, `src/SqlTranslator.fs`, `tests/QueryExprTests.fs`

#### Problem
Cannot remove duplicate results from queries.

#### Solution
Add `Distinct` flag to `TranslatedQuery` and handle in SQL generation.

#### API Design
```fsharp
query {
    for user in users do
    select user.Country
    distinct
}
// Returns: ["USA"; "Canada"; "UK"] (no duplicates)
```

#### Implementation

1. **Extend TranslatedQuery:**
```fsharp
// In QueryExpr.fs
type TranslatedQuery<'T> = {
    Query: Query<'T>
    Sort: list<string * SortDirection>
    Projection: Projection
    Skip: int option
    Take: int option
    Distinct: bool  // NEW
}
```

2. **Handle in translateQuery:**
```fsharp
// Add pattern for distinct in translateQuery
| SpecificCall <@@ Linq.QueryBuilder.Distinct @@> (_, _, [source]) ->
    let inner = translateQueryInner source
    { inner with Distinct = true }
```

3. **Generate SQL:**
```fsharp
// In SqlTranslator.fs
let selectClause = 
    if query.Distinct then "SELECT DISTINCT" else "SELECT"
```

#### Tests
- `distinct generates SELECT DISTINCT SQL`
- `distinct with projection selects distinct projected fields`
- `Integration: distinct removes duplicate values`

---

### 1.3 `all` Operator

**Priority:** HIGH  
**Complexity:** LOW  
**Files:** `src/QueryExpr.fs`, `src/Collection.fs`, `tests/QueryExprTests.fs`

#### Problem
Cannot check if ALL documents match a predicate.

#### Solution
Implement as `NOT EXISTS (... WHERE NOT predicate)` or count-based check.

#### API Design
```fsharp
query {
    for user in users do
    all (user.Age >= 18)
}
// Returns: true if ALL users are 18+, false otherwise
```

#### Implementation Strategy
Two approaches:

**Option A: SQL-based (preferred for large datasets)**
```sql
SELECT NOT EXISTS (
    SELECT 1 FROM collection WHERE NOT (predicate)
) as result
```

**Option B: Count-based**
```sql
SELECT COUNT(*) = (SELECT COUNT(*) FROM collection WHERE predicate) as result
```

#### Tests
- `all returns true when all documents match`
- `all returns false when any document doesn't match`
- `all with empty collection returns true`

---

### 1.4 `find` Operator

**Priority:** MEDIUM  
**Complexity:** LOW  
**Files:** `src/QueryExpr.fs`, `tests/QueryExprTests.fs`

#### Problem
No direct way to find first element matching a predicate (throws if not found).

#### Solution
Combine `where` + `head` semantics.

#### API Design
```fsharp
query {
    for user in users do
    find (user.Name = "Alice")
}
// Returns: User (throws if not found)
```

#### Implementation
```fsharp
// Translate to: where predicate -> head
| SpecificCall <@@ Linq.QueryBuilder.Find @@> (_, _, [source; predicate]) ->
    let inner = translateQueryInner source
    let whereClause = translatePredicate predicate
    { inner with 
        Query = Query.And [inner.Query; whereClause]
        Take = Some 1 }
    // Execution layer throws if no results
```

---

## Phase 2: Aggregate Functions

### 2.1 Core Aggregates: `minBy`, `maxBy`, `sumBy`, `averageBy`

**Priority:** HIGH  
**Complexity:** MEDIUM  
**Files:** `src/QueryExpr.fs`, `src/SqlTranslator.fs`, `src/Types.fs`, `tests/QueryExprTests.fs`

#### Problem
Cannot compute aggregate values (min, max, sum, average) over document fields.

#### Solution
Add `AggregateOp` type and handle in SQL translation.

#### API Design
```fsharp
// Minimum value
query {
    for product in products do
    minBy product.Price
}
// Returns: 9.99m (decimal)

// Maximum value
query {
    for user in users do
    maxBy user.Age
}
// Returns: 65L (int64)

// Sum
query {
    for order in orders do
    sumBy order.Total
}
// Returns: 15420.50m (decimal)

// Average
query {
    for sensor in sensors do
    averageBy sensor.Reading
}
// Returns: 23.5 (float)
```

#### Implementation

1. **Add AggregateOp type:**
```fsharp
// In Types.fs or QueryExpr.fs
[<RequireQualifiedAccess>]
type AggregateOp =
    | Min of field: string
    | Max of field: string
    | Sum of field: string
    | Avg of field: string
    | Count
```

2. **Extend TranslatedQuery:**
```fsharp
type TranslatedQuery<'T> = {
    // ... existing fields
    Aggregate: AggregateOp option
}
```

3. **SQL Generation:**
```fsharp
// In SqlTranslator.fs
match query.Aggregate with
| Some (AggregateOp.Min field) ->
    $"SELECT MIN(json_extract(body, '$.{field}')) FROM {table} {whereClause}"
| Some (AggregateOp.Max field) ->
    $"SELECT MAX(json_extract(body, '$.{field}')) FROM {table} {whereClause}"
| Some (AggregateOp.Sum field) ->
    $"SELECT SUM(json_extract(body, '$.{field}')) FROM {table} {whereClause}"
| Some (AggregateOp.Avg field) ->
    $"SELECT AVG(json_extract(body, '$.{field}')) FROM {table} {whereClause}"
| None -> // regular select
```

#### Tests
- `minBy generates MIN SQL`
- `maxBy generates MAX SQL`
- `sumBy generates SUM SQL`
- `averageBy generates AVG SQL`
- `Integration: minBy returns minimum value`
- `Integration: aggregates work with where clause`
- `Integration: aggregates on empty collection`

---

### 2.2 Nullable Aggregates: `minByNullable`, `maxByNullable`, `sumByNullable`, `averageByNullable`

**Priority:** MEDIUM  
**Complexity:** LOW (builds on 2.1)  
**Files:** Same as 2.1

#### Problem
Need to handle nullable fields in aggregates, ignoring null values.

#### API Design
```fsharp
query {
    for user in users do
    sumByNullable user.OptionalScore
}
// Returns: Nullable<int64> - sum of non-null scores
```

#### Implementation
SQLite naturally ignores NULL in aggregates. The difference is return type handling:
- `sumBy` returns `'T`
- `sumByNullable` returns `Nullable<'T>`

---

## Phase 3: Grouping Operations

### 3.1 `groupBy` Operator

**Priority:** MEDIUM  
**Complexity:** HIGH  
**Files:** `src/QueryExpr.fs`, `src/SqlTranslator.fs`, `src/Collection.fs`

#### Problem
Cannot group documents by field and perform aggregate operations on groups.

#### Solution
Add grouping support with aggregate functions on groups.

#### API Design
```fsharp
// Group and count
query {
    for user in users do
    groupBy user.Country into g
    select (g.Key, g.Count())
}
// Returns: [("USA", 150); ("Canada", 45); ("UK", 80)]

// Group and aggregate
query {
    for order in orders do
    groupBy order.CustomerId into g
    select (g.Key, g.Count(), query { for o in g do sumBy o.Total })
}
// Returns: [("cust1", 5, 500.0m); ("cust2", 3, 250.0m)]
```

#### Implementation Considerations

1. **SQL Generation:**
```sql
SELECT json_extract(body, '$.country') as _key, COUNT(*) as _count
FROM users
GROUP BY json_extract(body, '$.country')
```

2. **Result Mapping:**
- Group key extraction
- Aggregate result mapping
- Nested query support within groups

3. **Limitations:**
- May need to limit supported operations within groups
- Complex nested queries may require multiple SQL queries

#### Tests
- `groupBy generates GROUP BY SQL`
- `groupBy with Count() works`
- `groupBy with multiple aggregates`
- `groupBy with where clause`
- `groupBy with having (where on groups)`

---

### 3.2 `groupValBy` Operator

**Priority:** LOW  
**Complexity:** HIGH  
**Files:** Same as 3.1

#### Problem
Need to select specific values when grouping, not full documents.

#### API Design
```fsharp
query {
    for user in users do
    groupValBy user.Name user.Country into g
    select (g.Key, g.Count())
}
// Groups names by country
```

---

## Phase 4: Additional Element Operators

### 4.1 `last` / `lastOrDefault`

**Priority:** LOW  
**Complexity:** LOW  
**Files:** `src/QueryExpr.fs`, `src/SqlTranslator.fs`

#### Problem
Cannot get the last element of a query result.

#### Solution
Reverse sort order and take first, or use subquery with MAX.

#### API Design
```fsharp
query {
    for log in logs do
    sortBy log.Timestamp
    last
}
// Returns: most recent log entry
```

#### Implementation
```sql
-- Option 1: Reverse and limit
SELECT * FROM logs ORDER BY timestamp DESC LIMIT 1

-- Option 2: Subquery
SELECT * FROM logs WHERE timestamp = (SELECT MAX(timestamp) FROM logs)
```

---

### 4.2 `exactlyOne` / `exactlyOneOrDefault`

**Priority:** LOW  
**Complexity:** LOW  
**Files:** `src/QueryExpr.fs`, `src/Collection.fs`

#### Problem
Need to assert exactly one result exists.

#### Solution
Execute query with LIMIT 2, check count, return or throw.

#### API Design
```fsharp
query {
    for user in users do
    where (user.Email = "alice@example.com")
    exactlyOne
}
// Returns: User (throws if 0 or >1 results)
```

---

### 4.3 `nth`

**Priority:** LOW  
**Complexity:** LOW  
**Files:** `src/QueryExpr.fs`

#### Problem
Cannot get element at specific index.

#### Solution
Use OFFSET and LIMIT.

#### API Design
```fsharp
query {
    for user in users do
    sortBy user.Name
    nth 5
}
// Returns: 6th user (0-indexed)
```

#### Implementation
```sql
SELECT * FROM users ORDER BY name LIMIT 1 OFFSET 5
```

---

### 4.4 `skipWhile` / `takeWhile`

**Priority:** LOW  
**Complexity:** MEDIUM  
**Files:** `src/QueryExpr.fs`

#### Problem
Cannot conditionally skip/take based on predicate.

#### Note
These are challenging in SQL as they require row-by-row evaluation. May need to:
1. Fetch all and filter in memory (not ideal)
2. Use window functions (complex)
3. Document as unsupported for large datasets

#### Recommendation
Consider marking as "not recommended for large datasets" or implementing with window functions if SQLite version supports them.

---

## Phase 5: Nullable Sorting

### 5.1 Nullable Sort Operators

**Priority:** MEDIUM  
**Complexity:** LOW  
**Files:** `src/QueryExpr.fs`, `src/SqlTranslator.fs`

#### Operators
- `sortByNullable` / `sortByNullableDescending`
- `thenByNullable` / `thenByNullableDescending`

#### Problem
Need to handle NULL values explicitly in sorting (nulls first vs nulls last).

#### API Design
```fsharp
query {
    for user in users do
    sortByNullable user.MiddleName  // NULLs handled appropriately
    select user
}
```

#### Implementation
```sql
-- SQLite: NULLs sort first by default
ORDER BY middle_name ASC NULLS LAST
-- or
ORDER BY middle_name IS NULL, middle_name ASC
```

---

## Implementation Schedule

### Sprint 1: Pattern Matching (1-2 days)
- [ ] 1.1 `Sql.like` / `Sql.ilike`
- [ ] Tests and documentation

### Sprint 2: Simple Operators (1-2 days)
- [ ] 1.2 `distinct`
- [ ] 1.3 `all`
- [ ] 1.4 `find`
- [ ] Tests and documentation

### Sprint 3: Core Aggregates (2-3 days)
- [ ] 2.1 `minBy`, `maxBy`, `sumBy`, `averageBy`
- [ ] 2.2 Nullable variants
- [ ] Tests and documentation

### Sprint 4: Grouping (3-4 days)
- [ ] 3.1 `groupBy` with basic aggregates
- [ ] 3.2 `groupValBy` (if time permits)
- [ ] Tests and documentation

### Sprint 5: Remaining Operators (2-3 days)
- [ ] 4.1 `last` / `lastOrDefault`
- [ ] 4.2 `exactlyOne` / `exactlyOneOrDefault`
- [ ] 4.3 `nth`
- [ ] 5.1 Nullable sort operators
- [ ] Tests and documentation

### Deferred
- 4.4 `skipWhile` / `takeWhile` - Complex, limited SQL support

---

## Out of Scope

The following are explicitly **NOT** planned for FractalDb:

| Operator | Reason |
|----------|--------|
| `join` | Document DB - embed data instead |
| `leftOuterJoin` | Document DB - embed data instead |
| `groupJoin` | Document DB - embed data instead |
| `Union` / `Intersect` | Can be done at application level |

---

## Documentation Updates Required

After implementation:
1. Update `LLMS.txt` with new operators
2. Update `docs/query-expressions.md`
3. Update `README.md` examples
4. Add XML documentation to all new functions
5. Update `CHANGELOG.md`

---

## Breaking Changes

None anticipated. All changes are additive.

---

## Version Planning

| Version | Features |
|---------|----------|
| 1.4.0 | `Sql.like`, `Sql.ilike`, `distinct`, `all`, `find` |
| 1.5.0 | `minBy`, `maxBy`, `sumBy`, `averageBy` + nullable variants |
| 1.6.0 | `groupBy`, `groupValBy` |
| 1.7.0 | Remaining element operators, nullable sorting |
