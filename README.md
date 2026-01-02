# FractalDb

A lightweight **F# document database** built on SQLite with LINQ-style query expressions.

[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-9.0%2B-512BD4)](https://dotnet.microsoft.com/)

## Overview

FractalDb is an embedded document database that runs in-process with your .NET application. Store F# records as JSON documents, query them with LINQ-style `query { }` expressions, and leverage SQLite's battle-tested reliability.

**Key Features:**

- **Document Storage** - Store F# records as JSON with automatic serialization
- **Query Expressions** - LINQ-style `query { for x in collection do where ... }` syntax
- **Schema Builder** - Declarative `schema<'T> { field ... indexed ... }` expressions
- **ACID Transactions** - Full transaction support with `db.Transact { }` builder
- **Dual API** - Both instance methods (`users.InsertOne(doc)`) and module functions (`users |> Collection.insertOne doc`)
- **Indexed Queries** - Single and composite indexes (28x speedup)
- **Validation** - Custom validation rules in schema definitions
- **Automatic Timestamps** - Built-in CreatedAt/UpdatedAt tracking
- **Type Safety** - F# type system ensures compile-time correctness
- **Result-Based Errors** - All operations return `Result<'T, FractalError>`

## Quick Start

```fsharp
open FractalDb
open FractalDb.Schema
open FractalDb.Collection
open FractalDb.QueryExpr

// Define your document type
type User = { Name: string; Email: string; Age: int; Active: bool }

// Define schema using computation expression
let userSchema = schema<User> {
    field "name" SqliteType.Text
    unique "email" SqliteType.Text        // Unique index
    indexed "age" SqliteType.Integer      // Regular index
    timestamps                            // Auto CreatedAt/UpdatedAt
    validate (fun user ->
        if user.Age < 0 then Error "Age must be positive"
        else Ok user
    )
}

// Open database and get collection
let db = FractalDb.Open("app.db")
let users = db.Collection<User>("users", userSchema)

// Insert documents - Instance method style
task {
    let! result = users.InsertOne {
        Name = "Alice"; Email = "alice@example.com"; Age = 30; Active = true
    }
    match result with
    | Ok doc -> printfn "Created: %s" doc.Id
    | Error err -> printfn "Error: %s" err.Message
}

// Or use module function style (for pipelines)
task {
    let! result = users |> Collection.insertOne {
        Name = "Bob"; Email = "bob@example.com"; Age = 25; Active = true
    }
    // ...
}

// Query with filters (using Query module)
task {
    let queryFilter = Query.And [
        Query.Field("age", FieldOp.Compare(box (CompareOp.Gte 18)))
        Query.Field("active", FieldOp.Compare(box (CompareOp.Eq true)))
    ]
    
    let! adults = users |> Collection.find queryFilter
    for user in adults do
        printfn "%s (%d)" user.Data.Name user.Data.Age
}

// Or use LINQ-style query { } expressions with fluent API
task {
    let adultQuery = query {
        for user in users do
        where (user.Age >= 18)
        where (user.Active = true)
        sortBy user.Name
        take 10
    }

    // Execute with fluent .exec() method
    let! results = adultQuery.exec(users)
    // or: let! results = users |> Collection.exec adultQuery  // Module function still works
    
    for user in results do
        printfn "%s (%d)" user.Data.Name user.Data.Age
}

// Query Composition - build queries progressively using three styles

// Option 1: <+> Operator (combines queries)
task {
    // Define reusable query parts
    let filters = query {
        for user in users do
        where (user.Age >= 18)
        where (user.Active = true)
    }
    
    let sorting = query {
        for user in users do
        sortBy user.Name
    }
    
    let paging = query {
        for user in users do
        skip 10
        take 20
    }
    
    // Compose using <+> operator
    let composedQuery = filters <+> sorting <+> paging
    let! results = composedQuery.exec(users)
    
    for user in results do
        printfn "%s (%d)" user.Data.Name user.Data.Age
}

// Option 2: Fluent API (method chaining)
task {
    let baseQuery = query { for user in users do where (user.Age >= 18) }

    let! results =
        baseQuery
            .where(Query.Field("active", FieldOp.Compare (box (CompareOp.Eq true))))
            .orderBy("name", SortDirection.Asc)
            .skip(10)
            .limit(20)
            .exec(users)

    for user in results do
        printfn "%s (%d)" user.Data.Name user.Data.Age
}

// Option 3: Pipeline API (|> operators) - F# idiomatic
task {
    let baseQuery = query { for user in users do where (user.Age >= 18) }

    let composedQuery =
        baseQuery
        |> QueryOps.where (Query.Field("active", FieldOp.Compare (box (CompareOp.Eq true))))
        |> QueryOps.orderBy "name" SortDirection.Asc
        |> QueryOps.skip 10
        |> QueryOps.limit 20

    let! results = composedQuery.exec(users)

    for user in results do
        printfn "%s (%d)" user.Data.Name user.Data.Age
}

// Transactions
task {
    let! result = db.Transact {
        let! user1 = users |> Collection.insertOne { Name = "Bob"; Email = "bob@test.com"; Age = 25; Active = true }
        let! user2 = users |> Collection.insertOne { Name = "Carol"; Email = "carol@test.com"; Age = 35; Active = true }
        return (user1.Id, user2.Id)
    }
    match result with
    | Ok (id1, id2) -> printfn "Created: %s, %s" id1 id2
    | Error err -> printfn "Rolled back: %s" err.Message
}

db.Close()
```

## Schema Builder

Define schemas declaratively using the `schema<'T> { }` computation expression:

```fsharp
open FractalDb.Schema

type Product = { Name: string; Price: decimal; Category: string; InStock: bool }

let productSchema = schema<Product> {
    // Basic field (no index)
    field "name" SqliteType.Text

    // Indexed field for fast queries
    indexed "price" SqliteType.Real

    // Unique indexed field
    unique "sku" SqliteType.Text

    // Compound index for multi-field queries
    compoundIndex "idx_category_price" ["category"; "price"]

    // Enable automatic timestamps
    timestamps

    // Validation function
    validate (fun product ->
        if product.Price < 0m then Error "Price must be positive"
        elif String.IsNullOrWhiteSpace product.Name then Error "Name required"
        else Ok product
    )
}
```

### Schema Operations

| Operation | Description |
|-----------|-------------|
| `field "name" SqliteType.Text` | Basic field (no index) |
| `indexed "name" SqliteType.Text` | Field with index |
| `unique "name" SqliteType.Text` | Field with unique constraint |
| `compoundIndex "name" ["f1"; "f2"]` | Multi-field composite index |
| `timestamps` | Enable CreatedAt/UpdatedAt |
| `validate (fun x -> ...)` | Custom validation |

## Collection API

FractalDb provides two equivalent API styles for collection operations:

### Instance Method Style (Object-Oriented)

```fsharp
// More intuitive, better IntelliSense
let! doc = users.FindById("some-id")
let! result = users.InsertOne { Name = "Alice"; Email = "a@test.com"; Age = 30; Active = true }
let! updated = users.UpdateById("id", fun u -> { u with Age = u.Age + 1 })
let! deleted = users.DeleteById("some-id")
let! count = users.Count(Query.Empty)
```

### Module Function Style (Functional)

```fsharp
// Better for pipelines and composition
let! doc = users |> Collection.findById "some-id"
let! result = users |> Collection.insertOne { Name = "Alice"; Email = "a@test.com"; Age = 30; Active = true }
let! updated = users |> Collection.updateById "id" (fun u -> { u with Age = u.Age + 1 })
let! deleted = users |> Collection.deleteById "some-id"
let! count = users |> Collection.count Query.Empty
```

### Available Operations

| Operation | Instance Method | Module Function |
|-----------|-----------------|-----------------|
| Insert one | `users.InsertOne(doc)` | `Collection.insertOne doc users` |
| Insert many | `users.InsertMany(docs)` | `Collection.insertMany docs users` |
| Find by ID | `users.FindById(id)` | `Collection.findById id users` |
| Find one | `users.FindOne(filter)` | `Collection.findOne filter users` |
| Find all | `users.Find(filter)` | `Collection.find filter users` |
| Count | `users.Count(filter)` | `Collection.count filter users` |
| Update by ID | `users.UpdateById(id, fn)` | `Collection.updateById id fn users` |
| Update one | `users.UpdateOne(filter, fn)` | `Collection.updateOne filter fn users` |
| Update many | `users.UpdateMany(filter, fn)` | `Collection.updateMany filter fn users` |
| Replace one | `users.ReplaceOne(filter, doc)` | `Collection.replaceOne filter doc users` |
| Delete by ID | `users.DeleteById(id)` | `Collection.deleteById id users` |
| Delete one | `users.DeleteOne(filter)` | `Collection.deleteOne filter users` |
| Delete many | `users.DeleteMany(filter)` | `Collection.deleteMany filter users` |
| Search | `users.Search(text, fields)` | `Collection.search text fields users` |
| Distinct | `users.Distinct(field, filter)` | `Collection.distinct field filter users` |
| Drop | `users.Drop()` | `Collection.drop users` |
| Validate | `users.Validate(doc)` | `Collection.validate doc users` |

## Query Expressions

Build type-safe queries using F#'s `query { }` computation expression:

```fsharp
open FractalDb.QueryExpr

// Basic query
let activeUsers = query {
    for user in users do
    where (user.Active = true)
}

// Multiple conditions (AND)
let workingAgeActive = query {
    for user in users do
    where (user.Age >= 18)
    where (user.Age <= 65)
    where (user.Active = true)
}

// Logical operators
let privileged = query {
    for user in users do
    where (user.Role = "admin" || user.Role = "moderator")
}

// String operations
let gmailUsers = query {
    for user in users do
    where (user.Email.Contains("@gmail.com"))
}

let prefixMatch = query {
    for user in users do
    where (user.Name.StartsWith("A"))
}

// Sorting and pagination
let topUsers = query {
    for user in users do
    where (user.Active = true)
    sortByDescending user.Score
    skip 20
    take 10
}

// Nested field access
let nycUsers = query {
    for user in users do
    where (user.Address.City = "New York")
}

// Projection (select specific fields)
let emails = query {
    for user in users do
    where (user.Active = true)
    select user.Email
}
```

### Query Operations

| Operation | Description |
|-----------|-------------|
| `for x in collection do` | Start query on collection |
| `where (predicate)` | Filter documents |
| `sortBy x.Field` | Sort ascending |
| `sortByDescending x.Field` | Sort descending |
| `sortByNullable x.Field` | Sort ascending, NULLs last |
| `thenBy x.Field` | Secondary sort |
| `take n` | Limit to n results |
| `skip n` | Skip first n results |
| `select x.Field` | Project to specific field |
| `distinct` | Remove duplicate results |
| `head` / `headOrDefault` | Get first element |
| `last` / `lastOrDefault` | Get last element |
| `exactlyOne` / `exactlyOneOrDefault` | Get single element |
| `nth n` | Get nth element (0-indexed) |
| `minBy` / `maxBy` | Aggregate min/max |
| `sumBy` / `averageBy` | Aggregate sum/average |
| `groupBy x.Field` | Group by field |
| `all (predicate)` | Check all match |
| `find (predicate)` | Find first matching |

### Where Clause Operators

| Syntax | Description |
|--------|-------------|
| `x.Field = value` | Equality |
| `x.Field <> value` | Not equal |
| `x.Field > value` | Greater than |
| `x.Field >= value` | Greater or equal |
| `x.Field < value` | Less than |
| `x.Field <= value` | Less or equal |
| `pred1 && pred2` | Logical AND |
| `pred1 \|\| pred2` | Logical OR |
| `not pred` | Logical NOT |
| `x.Field.Contains("text")` | String contains |
| `x.Field.StartsWith("pre")` | String starts with |
| `x.Field.EndsWith("suf")` | String ends with |
| `Sql.like "%pattern%" x.Field` | SQL LIKE (case-sensitive) |
| `Sql.ilike "%pattern%" x.Field` | SQL LIKE (case-insensitive) |

## Query Composition

FractalDb supports three styles for composing queries progressively:

### Style 1: `<+>` Operator (Declarative)

The `<+>` operator combines separate query expressions into a single query. This is useful for defining reusable query parts:

```fsharp
// Define reusable query components
let activeFilter = query {
    for user in users do
    where (user.Active = true)
}

let adultFilter = query {
    for user in users do
    where (user.Age >= 18)
}

let nameSorting = query {
    for user in users do
    sortBy user.Name
}

let pagination = query {
    for user in users do
    skip 10
    take 20
}

// Combine using <+> operator
let fullQuery = activeFilter <+> adultFilter <+> nameSorting <+> pagination
let! results = fullQuery.exec(users)
```

**When to use:** Building queries from reusable components, composing filters from different sources.

### Style 2: Fluent API (Method Chaining)

Use fluent methods on `TranslatedQuery` for progressive query building:

```fsharp
let! results =
    query { for user in users do where (user.Age >= 18) }
        .where(Query.Field("active", FieldOp.Compare (box (CompareOp.Eq true))))
        .orderBy("name", SortDirection.Asc)
        .skip(10)
        .limit(20)
        .exec(users)
```

**When to use:** Building queries inline, method chaining style.

### Style 3: QueryOps Pipeline (F# Idiomatic)

Use the `QueryOps` module with F# pipe operators for functional composition:

```fsharp
open FractalDb.QueryOps

let query =
    query { for user in users do where (user.Age >= 18) }
    |> QueryOps.where (Query.Field("active", FieldOp.Compare (box (CompareOp.Eq true))))
    |> QueryOps.orderBy "name" SortDirection.Asc
    |> QueryOps.skip 10
    |> QueryOps.limit 20

let! results = query.exec(users)
```

**When to use:** Functional pipelines, F# idiomatic code, building query transformations.

### QueryOps Functions

| Function | Signature | Description |
|----------|-----------|-------------|
| `where` | `Query<'T> -> TranslatedQuery<'T> -> TranslatedQuery<'T>` | Add filter (AND) |
| `orderBy` | `string -> SortDirection -> TranslatedQuery<'T> -> TranslatedQuery<'T>` | Add sorting |
| `skip` | `int -> TranslatedQuery<'T> -> TranslatedQuery<'T>` | Set skip count |
| `limit` | `int -> TranslatedQuery<'T> -> TranslatedQuery<'T>` | Set result limit |

### Composition Rules

- **Where clauses** are combined with AND
- **OrderBy clauses** are appended (for secondary sorting)
- **Skip/Take** - the last one wins
- **Projection** - the last one wins
- All three styles can be mixed in the same codebase

## Advanced Query Features

FractalDb supports aggregates, grouping, element operators, and pattern matching:

```fsharp
// Aggregates
let sumQuery = query {
    for order in orders do
    where (order.Status = "completed")
    sumBy order.Total
}
let! total = orders |> Collection.execAggregate sumQuery

// Grouping
let groupQuery = query {
    for user in users do
    groupBy user.Department
}
let! groups = users |> Collection.execGroupBy groupQuery
for (dept, deptUsers) in groups do
    printfn "%s: %d users" dept (List.length deptUsers)

// Element operators
let lastQuery = query {
    for user in users do
    sortBy user.CreatedAt
    last
}
let! lastUser = users |> Collection.execLast lastQuery

// Pattern matching with LIKE
let likeQuery = query {
    for user in users do
    where (Sql.ilike "%@gmail.com" user.Email)  // Case-insensitive
}
```

See [Query Expressions documentation](docs/query-expressions.md) for complete details.

## Transactions

```fsharp
let! result = db.Transact {
    let! doc1 = users |> Collection.insertOne user1
    let! doc2 = users |> Collection.insertOne user2
    return (doc1.Id, doc2.Id)
}

match result with
| Ok ids -> printfn "Committed"
| Error err -> printfn "Rolled back: %s" err.Message
```

- Automatic commit on success
- Automatic rollback on error or exception
- 4x performance improvement for batched operations

## Resilience & Automatic Retry

FractalDb supports automatic retry for transient database errors like `SQLITE_BUSY` and `SQLITE_LOCKED`. Enable resilience at the database level and all collection operations will automatically retry on configured errors.

### Basic Usage

```fsharp
open FractalDb
open FractalDb.Errors

// Enable default resilience (retry on Busy/Locked, 2 retries)
let options = { DbOptions.defaults with Resilience = Some ResilienceOptions.defaults }
use db = FractalDb.Open("app.db", options)

// All operations now automatically retry on transient errors
let users = db.Collection<User>("users")
let! result = users |> Collection.insertOne newUser  // Retries automatically if busy
```

### Configuration Presets

| Preset | Errors Retried | Max Retries | Use Case |
|--------|---------------|-------------|----------|
| `ResilienceOptions.defaults` | Busy, Locked | 2 | Standard applications |
| `ResilienceOptions.extended` | Busy, Locked, IOError, CantOpen | 2 | Network drives |
| `ResilienceOptions.aggressive` | All transient errors | 5 | High-contention scenarios |
| `ResilienceOptions.none` | None | 0 | Disable retry |

### Custom Configuration

```fsharp
// Custom resilience settings
let customResilience = {
    ResilienceOptions.defaults with
        RetryOn = RetryableError.extended           // Busy, Locked, IOError, CantOpen
        MaxRetries = 5                               // More attempts
        BaseDelay = TimeSpan.FromMilliseconds(200.0) // Longer initial delay
        MaxDelay = TimeSpan.FromSeconds(10.0)        // Higher cap
}

let options = { DbOptions.defaults with Resilience = Some customResilience }
use db = FractalDb.Open("network.db", options)
```

### Retryable Error Types

| Error | SQLite Code | Description |
|-------|-------------|-------------|
| `Busy` | SQLITE_BUSY (5) | Another connection holds a lock |
| `Locked` | SQLITE_LOCKED (6) | Table or row write conflict |
| `IOError` | SQLITE_IOERR (10) | Transient disk/network I/O |
| `CantOpen` | SQLITE_CANTOPEN (14) | File temporarily unavailable |
| `Connection` | - | Transient connection issues |
| `Transaction` | - | Deadlock or timeout |

### Error Sets

```fsharp
// Pre-defined error sets
RetryableError.defaults   // set [ Busy; Locked ]
RetryableError.extended   // set [ Busy; Locked; IOError; CantOpen ]
RetryableError.all        // All 6 retryable error types

// Custom set
let myErrors = set [ RetryableError.Busy; RetryableError.IOError ]
```

### Retry Behavior

- **Exponential backoff**: Delays double each retry (100ms -> 200ms -> 400ms)
- **Jitter**: Random variation prevents thundering herd
- **Capped delays**: Never exceeds `MaxDelay` setting
- **Cancellation aware**: Respects `CancellationToken` during delays

### Which Operations Are Retried?

All write operations in `Collection` module are wrapped with retry logic:
- `insertOne`, `insertMany`
- `updateById`, `updateOne`, `updateMany`
- `replaceOne`
- `deleteById`, `deleteOne`, `deleteMany`

Read operations (`find`, `findById`, `count`, etc.) do not retry by default since they are typically idempotent and fast.

## Performance

| Operation | Throughput |
|-----------|------------|
| Single insert | ~2,700 ops/sec |
| Batch insert (1000 docs) | ~71,400 docs/sec |
| Indexed query | ~72,000 queries/sec |
| Non-indexed query | ~2,500 queries/sec |

**Key tips:**
- Use `insertMany` for bulk operations (6x faster)
- Use transactions for multiple writes (4x faster)
- Add indexes for frequently queried fields (28x faster)

## Installation

```bash
git clone https://github.com/takinprofit/FractalDb.git
cd FractalDb
dotnet build FractalDb.sln
```

**Requirements:** .NET 9.0 or higher

## License

MIT License - see [LICENSE](LICENSE) file for details.
