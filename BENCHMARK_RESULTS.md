# FractalDb Benchmark Results

**Date:** 2025-12-29
**Environment:** Apple M3 Pro 2.40GHz, 11 cores, .NET 10.0, macOS 26.1

---

## Insert Benchmarks

| Method | Mean | Throughput | Memory |
|--------|------|------------|--------|
| InsertSingleDocument | 373 μs | ~2,680 docs/sec | 0 B |
| InsertMany_10_Documents | 571 μs | ~17,500 docs/sec | 51 KB |
| InsertMany_100_Documents | 1.79 ms | ~55,900 docs/sec | 498 KB |
| InsertMany_1000_Documents | 14.0 ms | ~71,400 docs/sec | 4.97 MB |
| Insert_10_Sequential | 3.50 ms | ~2,860 docs/sec | 50 KB |

### Key Insights

- **Batch inserts scale excellently**: 1000 docs in 14ms = **71,400 docs/sec**
- **`insertMany` is 6x faster** than sequential inserts (571μs vs 3.5ms for 10 docs)
- Memory scales linearly (~5KB per document in batch)

---

## Query Benchmarks

Tested against a collection of 1000 documents with an index on the `name` field.

| Method | Mean | Memory | Analysis |
|--------|------|--------|----------|
| Query_IndexedField_Equality | 13.8 μs | 4 KB | Excellent - index utilized |
| Query_NonIndexedField_Equality | 391 μs | 4 KB | Full table scan |
| Query_Range | 1.07 ms | 264 KB | Returns many rows |
| Query_Complex_Nested | 1.01 ms | 234 KB | Complex but efficient |
| Query_Count | 396 μs | 2 KB | Aggregation overhead |
| Query_WithOptions_Limit | 39.6 μs | 16 KB | Limit optimization works |
| Query_WithOptions_Sort | 1.19 ms | 394 KB | Sort overhead expected |
| Query_FindAll | 2.54 ms | 1.2 MB | All 1000 docs returned |

### Key Insights

- **Indexed queries are 28x faster** (14μs vs 391μs)
- **Limit optimization works well** - early termination is effective
- Memory usage correlates directly with result set size

---

## Transaction Benchmarks

| Method | Mean | Memory |
|--------|------|--------|
| Transaction_SingleInsert | 496 μs | 0 B |
| Transaction_TwoInserts | 438 μs | 12 KB |
| Transaction_FiveInserts | 442 μs | 25 KB |
| Transaction_InsertAndQuery | 439 μs | 0 B |
| NoTransaction_SingleInsert | 409 μs | 0 B |
| NoTransaction_TwoInserts | 756 μs | 10 KB |
| NoTransaction_FiveInserts | 1.76 ms | 24 KB |

### Key Insights

- **Transactions amortize cost beautifully**: 5 inserts in 442μs (transaction) vs 1.76ms (no transaction)
- **4x speedup** when batching multiple operations in a transaction
- Transaction overhead is ~87μs per transaction (~22% for single insert)

---

## Performance Summary

| Metric | Performance |
|--------|-------------|
| Single insert | ~2,700 ops/sec |
| Batch insert (1000 docs) | ~71,400 docs/sec |
| Indexed query | ~72,000 queries/sec |
| Non-indexed query | ~2,500 queries/sec |
| Transaction overhead | ~87 μs |

---

## Recommendations

1. **Always use `insertMany` for bulk operations** - 6x faster than sequential inserts
2. **Use transactions for multiple writes** - 4x faster than individual operations
3. **Add indexes for frequently queried fields** - 28x faster lookups
4. **Use `Limit` option when you don't need all results** - significant performance improvement

---

## Running Benchmarks

```bash
cd benchmarks
dotnet run -c Release -- --filter "*"
```

Results are saved to `benchmarks/BenchmarkDotNet.Artifacts/results/`.
