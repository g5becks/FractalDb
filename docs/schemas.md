---
title: Schemas
category: Guides
categoryindex: 2
index: 4
---

# Schemas

Schemas define how FractalDb stores and indexes your documents. Use the `schema<'T> { }` computation expression to specify indexed fields, uniqueness constraints, and validation rules.

## Why Schemas Matter

Schemas provide:

1. **Performance** - Indexed fields enable fast queries (28x speedup)
2. **Data Integrity** - Unique constraints prevent duplicates
3. **Validation** - Enforce business rules before storing data
4. **Type Safety** - F# types map to SQLite storage types

## Schema Builder

Use the `schema<'T> { }` computation expression to define schemas:

```fsharp
open FractalDb.Schema

type User = { Name: string; Email: string; Age: int; Active: bool }

let userSchema = schema<User> {
    field "name" SqliteType.Text              // Basic field
    unique "email" SqliteType.Text            // Unique indexed field
    indexed "age" SqliteType.Integer          // Indexed field
    timestamps                                 // Enable CreatedAt/UpdatedAt
    validate (fun user ->
        if user.Age < 0 then Error "Age must be positive"
        elif not (user.Email.Contains("@")) then Error "Invalid email"
        else Ok user
    )
}
```

## Schema Operations

### Basic Field

Define a field that will be extracted from the JSON document:

```fsharp
let schema = schema<MyType> {
    field "name" SqliteType.Text
    field "count" SqliteType.Integer
    field "price" SqliteType.Real
    field "active" SqliteType.Boolean
}
```

### Indexed Field

Create an index for fast queries on this field:

```fsharp
let schema = schema<User> {
    indexed "age" SqliteType.Integer      // Fast queries on age
    indexed "status" SqliteType.Text      // Fast queries on status
}
```

Indexed fields provide **28x faster queries** compared to non-indexed fields.

### Unique Field

Enforce uniqueness and create an index:

```fsharp
let schema = schema<User> {
    unique "email" SqliteType.Text        // Must be unique, fast lookups
    unique "username" SqliteType.Text     // Must be unique, fast lookups
}
```

Attempting to insert duplicates returns `FractalError.Duplicate`.

### Composite Index

Create multi-field indexes for queries that filter on multiple fields together:

```fsharp
let postSchema = schema<Post> {
    field "authorId" SqliteType.Text
    field "status" SqliteType.Text
    field "publishedAt" SqliteType.Integer
    compoundIndex "idx_author_status" ["authorId"; "status"]
    compoundIndex "idx_author_date" ["authorId"; "publishedAt"]
}
```

Composite indexes work for queries using the **leftmost prefix** of fields.

### Timestamps

Enable automatic timestamp tracking:

```fsharp
let schema = schema<MyType> {
    timestamps    // Adds CreatedAt and UpdatedAt fields
}
```

Documents automatically get:
- `CreatedAt: int64` - Unix timestamp (milliseconds) when created
- `UpdatedAt: int64` - Unix timestamp (milliseconds) when last modified

### Validation

Add custom validation that runs before insert/update:

```fsharp
let userSchema = schema<User> {
    field "name" SqliteType.Text
    unique "email" SqliteType.Text
    validate (fun user ->
        if String.IsNullOrWhiteSpace user.Name then
            Error "Name is required"
        elif user.Age < 0 then
            Error "Age must be positive"
        elif user.Age > 150 then
            Error "Age must be realistic"
        elif not (user.Email.Contains("@")) then
            Error "Invalid email format"
        else
            Ok user
    )
}
```

Validation errors are returned as `FractalError.Validation`.

## SQLite Types

| Type | F# Types | Description |
|------|----------|-------------|
| `SqliteType.Text` | `string` | Strings, serialized JSON |
| `SqliteType.Integer` | `int`, `int64` | 64-bit integers, timestamps |
| `SqliteType.Real` | `float`, `decimal` | Floating-point numbers |
| `SqliteType.Boolean` | `bool` | Stored as INTEGER (0/1) |
| `SqliteType.Blob` | `byte[]` | Binary data |

## Nested Field Indexes

Index fields inside nested objects by specifying the JSON path:

```fsharp
type Address = { City: string; Country: string; ZipCode: string }
type User = { Name: string; Address: Address }

let userSchema = schema<User> {
    field "name" SqliteType.Text
    indexed "address.city" SqliteType.Text      // Nested field
    indexed "address.country" SqliteType.Text   // Nested field
    timestamps
}
```

Query using dot notation:

```fsharp
let nycUsers = query {
    for user in users do
    where (user.Address.City = "New York")
}
```

## Handling Errors

### Validation Errors

```fsharp
task {
    let! result = users |> Collection.insertOne { Name = ""; Email = "invalid"; Age = -5 }

    match result with
    | Ok doc -> printfn "Created: %s" doc.Id
    | Error (FractalError.Validation (field, msg)) ->
        printfn "Validation failed: %s" msg
    | Error err ->
        printfn "Error: %s" err.Message
}
```

### Duplicate Errors

```fsharp
task {
    let! result = users |> Collection.insertOne { Email = "existing@example.com"; ... }

    match result with
    | Error (FractalError.Duplicate (field, value)) ->
        printfn "Duplicate %s: %s" field value
    | _ -> ()
}
```

## Schema Examples

### User with Email Uniqueness

```fsharp
type User = { Name: string; Email: string; Role: string; Age: int }

let userSchema = schema<User> {
    field "name" SqliteType.Text
    unique "email" SqliteType.Text
    indexed "role" SqliteType.Text
    indexed "age" SqliteType.Integer
    timestamps
    validate (fun u ->
        if String.IsNullOrWhiteSpace u.Email then Error "Email required"
        elif not (u.Email.Contains("@")) then Error "Invalid email"
        else Ok u
    )
}
```

### Blog Post with Composite Index

```fsharp
type Post = { Title: string; AuthorId: string; PublishedAt: int64; Status: string }

let postSchema = schema<Post> {
    field "title" SqliteType.Text
    field "authorId" SqliteType.Text
    indexed "status" SqliteType.Text
    field "publishedAt" SqliteType.Integer
    compoundIndex "idx_author_status" ["authorId"; "status"]
    compoundIndex "idx_author_date" ["authorId"; "publishedAt"]
    timestamps
}
```

### E-commerce Product

```fsharp
type Product = { Name: string; Sku: string; Price: float; Category: string; InStock: bool }

let productSchema = schema<Product> {
    field "name" SqliteType.Text
    unique "sku" SqliteType.Text
    indexed "category" SqliteType.Text
    indexed "price" SqliteType.Real
    indexed "inStock" SqliteType.Boolean
    timestamps
    validate (fun p ->
        if p.Price < 0.0 then Error "Price cannot be negative"
        elif String.IsNullOrWhiteSpace p.Sku then Error "SKU required"
        else Ok p
    )
}
```

### Nested Address Index

```fsharp
type Location = { City: string; Country: string }
type Profile = { Bio: string; Location: Location }

let profileSchema = schema<Profile> {
    field "bio" SqliteType.Text
    indexed "location.city" SqliteType.Text
    indexed "location.country" SqliteType.Text
    timestamps
}
```

## Minimal Schema

For simple use cases without indexes or validation:

```fsharp
let simpleSchema = schema<MyType> {
    timestamps    // Just enable timestamps
}
```

## Schema Operation Reference

| Operation | Syntax | Description |
|-----------|--------|-------------|
| Basic field | `field "name" SqliteType.Text` | Extract field from JSON |
| Indexed field | `indexed "name" SqliteType.Text` | Field with index |
| Unique field | `unique "name" SqliteType.Text` | Unique indexed field |
| Composite index | `compoundIndex "idx" ["f1"; "f2"]` | Multi-field index |
| Timestamps | `timestamps` | Enable auto timestamps |
| Validation | `validate (fun x -> Result)` | Custom validation |
