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

// Query with LINQ-style expressions
task {
    let adultQuery = query {
        for user in users do
        where (user.Age >= 18)
        where (user.Active = true)
        sortBy user.Name
        take 10
    }

    // Execute the query
    let! results = users |> Collection.executeQuery adultQuery
    for user in results do
        printfn "%s (%d)" user.Data.Name user.Data.Age
}

// Complex queries
let searchQuery = query {
    for user in users do
    where (user.Email.Contains("@gmail.com") || user.Email.EndsWith("@company.com"))
    where (user.Age >= 21 && user.Age <= 65)
    sortByDescending user.Age
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
| `take n` | Limit to n results |
| `skip n` | Skip first n results |
| `select x.Field` | Project to specific field |

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
