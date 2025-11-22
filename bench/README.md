# StrataDB Performance Benchmarks

Benchmarks for measuring StrataDB performance characteristics using [mitata](https://github.com/evanwashere/mitata).

## Running Benchmarks

```bash
# Query translation benchmarks (fastest, no I/O)
bun run bench

# CRUD operations benchmarks
bun run bench:crud

# Batch throughput benchmarks
bun run bench:batch

# Run all benchmarks
bun run bench:all
```

Or run individual benchmark files directly:

```bash
bun bench/query-translation.bench.ts
bun bench/crud-operations.bench.ts
bun bench/batch-throughput.bench.ts
```

## Benchmark Categories

### Query Translation (`query-translation.bench.ts`)

Measures the overhead of converting StrataDB queries to SQL. These benchmarks run in-memory without database I/O, isolating the translation logic.

**What's measured:**
- Simple queries (equality, null checks)
- Comparison operators ($gt, $gte, $lt, $lte, $in, $nin)
- String operators ($startsWith, $endsWith, $like, $regex)
- Array operators ($all, $size, $elemMatch)
- Logical operators ($and, $or, $not, $nor)
- Complex nested queries
- Query options (sort, limit, skip)

**Key metrics:**
- Sub-microsecond translation for simple queries
- Linear scaling with query complexity
- Negligible overhead for empty filters

### CRUD Operations (`crud-operations.bench.ts`)

Measures end-to-end performance including serialization, SQLite execution, and deserialization.

**What's measured:**
- Insert operations (single and batch)
- Find operations (by ID, filters, pagination)
- Update operations (single and multiple fields)
- Delete operations
- Transaction overhead
- Complex query execution

### Batch Throughput (`batch-throughput.bench.ts`)

Measures bulk operation performance to identify optimal batch sizes.

**What's measured:**
- `insertMany` vs individual `insertOne` calls
- Transaction vs non-transaction inserts
- Batch update throughput
- Batch delete throughput
- Large dataset query performance (10,000+ docs)

## Interpreting Results

mitata outputs include:
- **avg**: Average time per operation
- **min/max**: Range of execution times
- **p75/p99**: Percentile latencies
- **Memory**: Allocation statistics

### Performance Expectations

| Operation | Expected Range |
|-----------|---------------|
| Simple query translation | 100-300 ns |
| Complex query translation | 1-5 µs |
| insertOne | 50-200 µs |
| findById | 20-100 µs |
| find (100 docs) | 200-500 µs |
| insertMany (100 docs) | 2-10 ms |

**Note:** Actual performance varies by hardware. These benchmarks use in-memory SQLite (`:memory:`) for consistency.

## Writing Good Benchmarks

1. **Isolate what you're measuring** - Query translation benchmarks don't touch the database
2. **Use realistic data** - Document shapes match real-world usage
3. **Avoid dead code elimination** - mitata warns if benchmarks are optimized away
4. **Fresh state per iteration** - Create new database instances to avoid caching effects

## Continuous Improvement

These benchmarks help identify:
- Regression in query translation performance
- Inefficient SQL generation patterns
- Serialization/deserialization bottlenecks
- Optimal batch sizes for bulk operations

Run benchmarks before and after changes to measure impact.

## Baseline Results (Apple M3 Pro)

Recorded on 2025-01-21 with Bun 1.3.1.

### Query Translation

| Operation | Average | Min | Max |
|-----------|---------|-----|-----|
| Empty filter | 17 ns | 7 ns | 1.6 µs |
| Direct equality (indexed) | 287 ns | 41 ns | 1.2 ms |
| Direct equality (non-indexed) | 109 ns | 66 ns | 917 ns |
| Null check | 115 ns | 73 ns | 723 ns |
| $eq operator | 429 ns | 83 ns | 597 µs |
| $gt operator | 256 ns | 130 ns | 1.1 µs |
| $gte + $lt range | 355 ns | 239 ns | 1.3 µs |
| $in (3 values) | 274 ns | 183 ns | 804 ns |
| $in (10 values) | 410 ns | 260 ns | 1.7 µs |
| $startsWith | 285 ns | 133 ns | 1.9 µs |
| $like | 251 ns | 129 ns | 1.1 µs |
| $regex (RegExp) | 470 ns | 351 ns | 1.4 µs |
| $all (2 values) | 360 ns | 235 ns | 1.3 µs |
| $size | 213 ns | 126 ns | 722 ns |
| $and (2 conditions) | 578 ns | 378 ns | 1.5 µs |
| $and (5 conditions) | 1.26 µs | 869 ns | 2.3 µs |
| $or (3 conditions) | 571 ns | 407 ns | 1.3 µs |
| Nested $and + $or | 801 ns | 575 ns | 1.7 µs |
| Deeply nested (3 levels) | 2.00 µs | 1.2 µs | 3.5 µs |
| Sort single field | 138 ns | 87 ns | 1.2 µs |
| Sort + pagination | 355 ns | 265 ns | 865 ns |
| Limit only | 40 ns | 24 ns | 374 ns |

### CRUD Operations

| Operation | Average | Min | Max |
|-----------|---------|-----|-----|
| insertOne | 199 µs | 116 µs | 4.4 ms |
| insertMany (10 docs) | 264 µs | 171 µs | 2.5 ms |
| insertMany (100 docs) | 1.00 ms | 691 µs | 3.4 ms |
| findById | 11 µs | 3.6 µs | 1.8 ms |
| findOne (indexed) | 10 µs | 4.7 µs | 1.2 ms |
| findOne (non-indexed) | 9.2 µs | 4.8 µs | 1.5 ms |
| find (100 docs) | 114 µs | 79 µs | 1.7 ms |
| find (indexed range) | 31 µs | 21 µs | 979 µs |
| find (with limit) | 18 µs | 14 µs | 24 µs |
| count (all) | 7.4 µs | 3.1 µs | 1.7 ms |
| count (filtered) | 5.5 µs | 4.1 µs | 7.7 µs |
| updateOne | 228 µs | 121 µs | 5.4 ms |
| updateMany (10 docs) | 422 µs | 256 µs | 3.5 ms |
| deleteOne | 205 µs | 119 µs | 2.7 ms |
| deleteMany (10 docs) | 366 µs | 190 µs | 2.5 ms |

### Batch Throughput

| Operation | Average | Min | Max |
|-----------|---------|-----|-----|
| 100 individual inserts | 2.20 ms | 1.2 ms | 5.5 ms |
| insertMany (100 docs) | 1.26 ms | 705 µs | 4.5 ms |
| 100 inserts (no tx) | 2.03 ms | 1.2 ms | 5.6 ms |
| 100 inserts (explicit tx) | 1.72 ms | 938 µs | 5.1 ms |
| updateMany (1000 docs) | 26 ms | 17 ms | 35 ms |
| deleteMany (1000 docs) | 15 ms | 9.3 ms | 22 ms |

### Large Dataset (10,000 docs)

| Operation | Average | Min | Max |
|-----------|---------|-----|-----|
| count (all) | 76 µs | 57 µs | 1.3 ms |
| count (indexed filter) | 42 µs | 26 µs | 2.9 ms |
| find (limit 100) | 140 µs | 78 µs | 1.6 ms |
| find (indexed range + limit) | 167 µs | 96 µs | 2.7 ms |
| find (complex + pagination) | 100 µs | 74 µs | 1.3 ms |

### Key Insights

1. **Query translation is negligible**: 100-500 ns for typical queries vs 10-200 µs for database I/O
2. **Use `insertMany` for bulk inserts**: 43% faster than individual inserts (1.26ms vs 2.20ms for 100 docs)
3. **Transactions matter**: Explicit transactions are 15% faster than auto-commit for multiple operations
4. **Indexed queries perform well**: Even on 10,000 docs, filtered counts take only 42 µs
5. **Pagination is efficient**: Complex queries with skip/limit on 10,000 docs: 100 µs

## Query Cache Performance

The SQLiteQueryTranslator includes an in-memory cache for query SQL templates. When repeated queries have the same structure (same fields and operators), only value extraction is performed, skipping SQL generation.

### Cache Comparison (Cold vs Warm)

| Query Type | Cold Cache | Warm Cache | Improvement |
|------------|------------|------------|-------------|
| Simple equality | 759 ns | 480 ns | **37% faster** |
| Range query ($gte + $lt) | 3.32 µs | 1.15 µs | **65% faster** |
| Complex ($and + $or) | 15.96 µs | 3.89 µs | **76% faster** |

### Hot Path Performance (100 iterations, all cached)

| Query Type | Average per 100 queries | Per-query average |
|------------|------------------------|-------------------|
| Simple equality | 109 µs | ~1.09 µs |
| $in operator | 177 µs | ~1.77 µs |
| $and with 2 conditions | 252 µs | ~2.52 µs |

### Cache Limitations

The following operators bypass the cache due to complex value extraction:
- `$regex` with RegExp objects
- `$elemMatch`
- `$index`
- `$all`

These operators still work correctly but require full translation on each call.
