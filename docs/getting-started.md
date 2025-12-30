---
title: Getting Started
category: Guides
categoryindex: 2
index: 1
---

# Getting Started with FractalDb

This guide walks you through installing FractalDb, creating your first database, and performing basic CRUD operations.

## Installation

### Requirements

- **.NET 9.0 or higher**
- **F# 9.0+**

### Build from Source

```bash
git clone https://github.com/takinprofit/FractalDb.git
cd FractalDb
dotnet build FractalDb.sln
```

## Creating a Database

FractalDb supports both file-based and in-memory databases.

### File-Based Database

```fsharp
open FractalDb

// Open or create a database file
let db = FractalDb.Open("myapp.db")

// Use the database...

// Close when done
db.Close()
```

### In-Memory Database

Perfect for testing or temporary data:

```fsharp
// Create an in-memory database
let db = FractalDb.InMemory()

// All data is lost when db is disposed
```

## Defining Document Types

Define your document types as F# records:

```fsharp
type User = {
    Name: string
    Email: string
    Age: int
    Active: bool
}

type Post = {
    Title: string
    Content: string
    AuthorId: string
    Tags: list<string>
}
```

## Defining Schemas

Use the `schema<'T> { }` computation expression to define which fields should be indexed:

```fsharp
open FractalDb.Schema

let userSchema = schema<User> {
    field "name" SqliteType.Text              // Basic field
    unique "email" SqliteType.Text            // Unique indexed field
    indexed "age" SqliteType.Integer          // Indexed field
    timestamps                                 // Enable CreatedAt/UpdatedAt
    validate (fun user ->
        if user.Age < 0 then Error "Age must be positive"
        else Ok user
    )
}
```

### Schema Operations

| Operation | Description |
|-----------|-------------|
| `field "name" SqliteType.Text` | Basic field |
| `indexed "name" SqliteType.Text` | Field with index (fast queries) |
| `unique "name" SqliteType.Text` | Unique indexed field |
| `compoundIndex "idx" ["f1"; "f2"]` | Composite index |
| `timestamps` | Enable auto timestamps |
| `validate (fun x -> ...)` | Validation function |

### SQLite Types

- `SqliteType.Text` - Strings
- `SqliteType.Integer` - 64-bit integers
- `SqliteType.Real` - Floating-point
- `SqliteType.Boolean` - Boolean (stored as 0/1)
- `SqliteType.Blob` - Binary data

## Getting a Collection

```fsharp
let users = db.Collection<User>("users", userSchema)
```

## Basic CRUD Operations

FractalDb provides two equivalent API styles:
- **Instance methods**: `users.InsertOne(doc)` - more intuitive, better IntelliSense
- **Module functions**: `users |> Collection.insertOne doc` - better for pipelines

### Insert Documents

```fsharp
open FractalDb.Collection

// Insert using instance method
task {
    let! result = users.InsertOne {
        Name = "Alice"
        Email = "alice@example.com"
        Age = 30
        Active = true
    }

    match result with
    | Ok doc ->
        printfn "Created document with ID: %s" doc.Id
        printfn "Created at: %d" doc.CreatedAt
    | Error err ->
        printfn "Error: %s" err.Message
}

// Or use module function style (for pipelines)
task {
    let! result = users |> Collection.insertOne {
        Name = "Bob"; Email = "bob@example.com"; Age = 25; Active = true
    }
    // ...
}

// Insert multiple documents
task {
    let! result = users.InsertMany [
        { Name = "Bob"; Email = "bob@example.com"; Age = 25; Active = true }
        { Name = "Charlie"; Email = "charlie@example.com"; Age = 35; Active = false }
    ]

    match result with
    | Ok batch ->
        printfn "Inserted %d documents" batch.InsertedCount
    | Error err ->
        printfn "Error: %s" err.Message
}
```

### Query Documents with Query Expressions

Use the `query { }` computation expression for type-safe, LINQ-style queries:

```fsharp
open FractalDb.QueryExpr

// Find adults
task {
    let adultQuery = query {
        for user in users do
        where (user.Age >= 18)
    }
    let! adults = users |> Collection.executeQuery adultQuery
    printfn "Found %d adults" (List.length adults)
}

// Find active adults sorted by name
task {
    let q = query {
        for user in users do
        where (user.Age >= 18)
        where (user.Active = true)
        sortBy user.Name
        take 10
    }
    let! results = users |> Collection.executeQuery q
    for user in results do
        printfn "%s (%d)" user.Data.Name user.Data.Age
}
```

### Find by ID

```fsharp
task {
    let! doc = users.FindById("some-id")
    // or: let! doc = users |> Collection.findById "some-id"
    match doc with
    | Some user -> printfn "Found: %s" user.Data.Name
    | None -> printfn "Not found"
}
```

### Update Documents

```fsharp
// Update by ID with a function
task {
    let! updated = users.UpdateById("some-id", fun user ->
        { user with Age = user.Age + 1 }
    )
    // or: let! updated = users |> Collection.updateById "some-id" (fun user -> ...)

    match updated with
    | Some doc -> printfn "Updated: %s is now %d" doc.Data.Name doc.Data.Age
    | None -> printfn "Document not found"
}

// Replace entire document
task {
    let! replaced = users |> Collection.replaceById "some-id" {
        Name = "Alice Smith"
        Email = "alice.smith@example.com"
        Age = 31
        Active = true
    }

    match replaced with
    | Some doc -> printfn "Replaced: %s" doc.Data.Name
    | None -> printfn "Document not found"
}
```

### Delete Documents

```fsharp
// Delete by ID
task {
    let! count = users |> Collection.deleteById "some-id"
    printfn "Deleted %d document(s)" count
}

// Delete matching query
task {
    let inactiveQuery = query {
        for user in users do
        where (user.Active = false)
    }
    let! count = users |> Collection.deleteMany inactiveQuery
    printfn "Deleted %d inactive users" count
}
```

## Error Handling

All operations return `Result<'T, FractalError>` or `Task<Result<'T, FractalError>>`:

```fsharp
task {
    let! result = users |> Collection.insertOne newUser

    match result with
    | Ok doc ->
        // Success - use the document
        printfn "Success: %s" doc.Id
    | Error err ->
        // Handle specific error types
        match err with
        | FractalError.Validation (field, msg) ->
            printfn "Validation error on %A: %s" field msg
        | FractalError.Duplicate (field, value) ->
            printfn "Duplicate %s: %s" field value
        | FractalError.NotFound id ->
            printfn "Not found: %s" id
        | _ ->
            printfn "Error: %s" err.Message
}
```

## Next Steps

- [Query Expressions](query-expressions.html) - Learn the full query syntax
- [Transactions](transactions.html) - Group operations in ACID transactions
- [Schemas](schemas.html) - Advanced schema configuration
- [Indexes](indexes.html) - Optimize query performance
