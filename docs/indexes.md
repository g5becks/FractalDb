---
title: Indexes
category: Guides
categoryindex: 2
index: 5
---

# Indexes

Indexes are critical for query performance. A well-indexed field can be queried **28x faster** than a non-indexed field.

## Why Indexes Matter

Without indexes, queries perform **full table scans** - reading every document to find matches. Indexes create fast lookup structures that dramatically improve query speed.

### Performance Impact

Benchmark results from a collection of 1,000 documents:

| Query Type | Mean Time | Throughput | Speedup |
|------------|-----------|------------|---------|
| Indexed field | 13.8 us | 72,000 queries/sec | **28x faster** |
| Non-indexed field | 391 us | 2,500 queries/sec | Baseline |

## When to Use Indexes

**Add indexes when:**

- Field is used in query filters (`where` clauses)
- Field is used for sorting
- Field needs uniqueness constraint
- Query performance is important

**Don't index when:**

- Field is rarely queried
- Collection is very small (< 100 docs)
- Field values are not selective (e.g., boolean with 99% true)
- Write performance is more critical than read performance

## Single Field Indexes

Use the `indexed` operation in your schema:

```fsharp
open FractalDb.Schema

type User = { Name: string; Email: string; Age: int }

let userSchema = schema<User> {
    indexed "email" SqliteType.Text       // Index for fast email queries
    indexed "age" SqliteType.Integer      // Index for fast age queries
    timestamps
}
```

## Unique Indexes

Use `unique` to create an index that also enforces uniqueness:

```fsharp
let userSchema = schema<User> {
    unique "email" SqliteType.Text        // Unique + indexed
    indexed "age" SqliteType.Integer      // Just indexed
    timestamps
}
```

Unique indexes are also used for fast lookups:

```fsharp
// Fast lookup using unique index
let emailQuery = query {
    for user in users do
    where (user.Email = "alice@example.com")
}
let! user = users |> Collection.findOne emailQuery
```

Attempting to insert duplicates returns an error:

```fsharp
let! result = users |> Collection.insertOne { Email = "existing@example.com"; ... }

match result with
| Error (FractalError.Duplicate (field, value)) ->
    printfn "Email already exists: %s" value
| _ -> ()
```

## Composite Indexes

Create multi-field indexes for queries that filter on multiple fields together:

```fsharp
type Post = { AuthorId: string; Status: string; PublishedAt: int64 }

let postSchema = schema<Post> {
    field "authorId" SqliteType.Text
    field "status" SqliteType.Text
    field "publishedAt" SqliteType.Integer
    compoundIndex "idx_author_status_date" ["authorId"; "status"; "publishedAt"]
    timestamps
}
```

### Composite Index Usage

Composite indexes work for queries using **leftmost prefix** of fields:

```fsharp
// Uses idx_author_status_date (matches leftmost field)
let byAuthor = query {
    for post in posts do
    where (post.AuthorId = "user123")
}

// Uses idx_author_status_date (matches first two fields)
let byAuthorStatus = query {
    for post in posts do
    where (post.AuthorId = "user123")
    where (post.Status = "published")
}

// Uses idx_author_status_date (matches all three fields)
let byAuthorStatusDate = query {
    for post in posts do
    where (post.AuthorId = "user123")
    where (post.Status = "published")
    where (post.PublishedAt > timestamp)
}

// Does NOT use idx_author_status_date (skips authorId)
let byStatusOnly = query {
    for post in posts do
    where (post.Status = "published")
}
```

## Nested Field Indexes

Index fields inside nested objects using dot notation:

```fsharp
type Address = { City: string; Country: string; ZipCode: string }
type User = { Name: string; Address: Address }

let userSchema = schema<User> {
    field "name" SqliteType.Text
    indexed "address.city" SqliteType.Text      // Index nested field
    indexed "address.country" SqliteType.Text   // Index nested field
    timestamps
}
```

Query using the nested path:

```fsharp
// Query by indexed nested field
let nycUsers = query {
    for user in users do
    where (user.Address.City = "New York")
}
```

## How Indexes Work

FractalDb uses SQLite's generated columns for indexing:

1. **Generated Column**: For each indexed field, a virtual column is created
2. **JSON Extraction**: The column extracts the value from the JSON document
3. **Index Creation**: A B-tree index is created on the generated column

Example SQL generated:

```sql
-- Generated column
ALTER TABLE users ADD COLUMN _email TEXT
    GENERATED ALWAYS AS (json_extract(body, '$.email')) VIRTUAL;

-- Index on generated column
CREATE UNIQUE INDEX idx_users_email ON users(_email);
```

This approach is efficient because:
- Generated columns are computed once when data is written
- Indexes use SQLite's optimized B-tree structures
- Queries use the index without re-parsing JSON

## Index Best Practices

### 1. Index Fields Used in Filters

```fsharp
// If you frequently query by status, index it
let userSchema = schema<User> {
    indexed "status" SqliteType.Text
}

let activeUsers = query {
    for user in users do
    where (user.Status = "active")
}
```

### 2. Consider Query Patterns

Create composite indexes matching your common query patterns:

```fsharp
// Common query: posts by author, filtered by status
let authorPosts = query {
    for post in posts do
    where (post.AuthorId = authorId)
    where (post.Status = "published")
}

// Composite index to match
let postSchema = schema<Post> {
    field "authorId" SqliteType.Text
    field "status" SqliteType.Text
    compoundIndex "idx_author_status" ["authorId"; "status"]
}
```

### 3. Don't Over-Index

Each index:
- Increases storage space
- Slows down inserts and updates
- Must be maintained on every write

Only index fields that are frequently queried.

### 4. Use Unique for Lookups

If you frequently look up by a unique identifier:

```fsharp
let userSchema = schema<User> {
    unique "email" SqliteType.Text
    unique "username" SqliteType.Text
}
```

### 5. Order Matters in Composite Indexes

Put the most selective field first:

```fsharp
// Good: authorId is more selective than status
compoundIndex "idx" ["authorId"; "status"]

// Less optimal: status has fewer distinct values
compoundIndex "idx" ["status"; "authorId"]
```

## Checking Index Usage

Use SQLite's EXPLAIN to verify index usage:

```sql
EXPLAIN QUERY PLAN
SELECT * FROM users WHERE _email = 'test@example.com';
```

Output should show `USING INDEX idx_users_email` for indexed queries.

## Schema Examples

### User with Multiple Indexes

```fsharp
let userSchema = schema<User> {
    unique "email" SqliteType.Text        // Unique lookups
    indexed "role" SqliteType.Text        // Filter by role
    indexed "createdAt" SqliteType.Integer // Sort by date
    timestamps
}
```

### Posts with Composite Index

```fsharp
let postSchema = schema<Post> {
    field "authorId" SqliteType.Text
    indexed "status" SqliteType.Text
    field "publishedAt" SqliteType.Integer
    compoundIndex "idx_author_published" ["authorId"; "publishedAt"]
    timestamps
}
```

### Products with Category Index

```fsharp
let productSchema = schema<Product> {
    unique "sku" SqliteType.Text
    indexed "category" SqliteType.Text
    indexed "price" SqliteType.Real
    compoundIndex "idx_category_price" ["category"; "price"]
    timestamps
}
```
