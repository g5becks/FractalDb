---
title: FractalDb
category: Overview
categoryindex: 1
index: 1
---

# FractalDb

A lightweight **F# document database** built on SQLite with LINQ-style query expressions.

## What is FractalDb?

FractalDb is an embedded document database that runs in-process with your .NET application. It combines the simplicity of document storage with the reliability of SQLite, providing type-safe query expressions and declarative schema definitions.

## Key Features

- **Query Expressions** - LINQ-style `query { for x in collection do where ... }` syntax
- **Schema Builder** - Declarative `schema<'T> { field ... indexed ... }` expressions
- **Dual API** - Both `users.InsertOne(doc)` and `users |> Collection.insertOne doc`
- **High Performance** - 71,400+ docs/sec batch inserts, 72,000+ queries/sec indexed
- **ACID Transactions** - Full transaction support with `db.Transact { }` builder
- **Indexed Queries** - Single and composite indexes (28x speedup)
- **Validation** - Custom validation rules in schema definitions
- **Automatic Timestamps** - Built-in CreatedAt/UpdatedAt tracking
- **Type Safety** - F# type system ensures compile-time correctness

## Quick Example

```fsharp
open FractalDb
open FractalDb.Schema
open FractalDb.Collection
open FractalDb.QueryExpr

// Document type
type User = { Name: string; Email: string; Age: int; Active: bool }

// Schema with computation expression
let userSchema = schema<User> {
    field "name" SqliteType.Text
    unique "email" SqliteType.Text
    indexed "age" SqliteType.Integer
    timestamps
    validate (fun u -> if u.Age < 0 then Error "Invalid age" else Ok u)
}

// Database and collection
let db = FractalDb.Open("app.db")
let users = db.Collection<User>("users", userSchema)

// Insert - using instance method
task {
    let! result = users.InsertOne {
        Name = "Alice"; Email = "alice@example.com"; Age = 30; Active = true
    }
    // Or use pipeline: users |> Collection.insertOne { ... }
    match result with
    | Ok doc -> printfn "Created: %s" doc.Id
    | Error err -> printfn "Error: %s" err.Message
}

// Query with LINQ-style expression
task {
    let adultQuery = query {
        for user in users do
        where (user.Age >= 18)
        where (user.Active = true)
        sortBy user.Name
        take 10
    }
    let! results = users |> Collection.executeQuery adultQuery
    for user in results do
        printfn "%s" user.Data.Name
}

// Transactions
task {
    let! result = db.Transact {
        let! u1 = users |> Collection.insertOne { Name = "Bob"; Email = "bob@test.com"; Age = 25; Active = true }
        let! u2 = users |> Collection.insertOne { Name = "Carol"; Email = "carol@test.com"; Age = 35; Active = true }
        return (u1.Id, u2.Id)
    }
    match result with
    | Ok _ -> printfn "Committed"
    | Error err -> printfn "Rolled back"
}

db.Close()
```

## Performance

| Operation | Throughput |
|-----------|------------|
| Single insert | ~2,700 ops/sec |
| Batch insert (1000 docs) | ~71,400 docs/sec |
| Indexed query | ~72,000 queries/sec |
| Non-indexed query | ~2,500 queries/sec |

## Documentation

- [Getting Started](getting-started.html) - Installation and first steps
- [Query Expressions](query-expressions.html) - LINQ-style query syntax
- [Schemas](schemas.html) - Schema builder and validation
- [Transactions](transactions.html) - ACID transaction patterns
- [Indexes](indexes.html) - Optimizing query performance
- [API Reference](reference/index.html) - Full API documentation
