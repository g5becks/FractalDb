---
title: Query Expressions
category: Guides
categoryindex: 2
index: 2
---

# Query Expressions

FractalDb provides LINQ-style query expressions using the `query { }` computation expression. This is the primary API for querying documents with type-safe, composable syntax.

## Basic Queries

### Query Structure

Every query starts with `for ... in ... do`:

```fsharp
open FractalDb.QueryExpr
open FractalDb.Collection

let myQuery = query {
    for doc in collection do
    where (doc.Field = value)
}

let! results = collection |> Collection.executeQuery myQuery
```

### Simple Filter

```fsharp
// Find all active users
let activeQuery = query {
    for user in users do
    where (user.Active = true)
}
```

### Multiple Filters (AND)

Multiple `where` clauses are combined with AND:

```fsharp
// Find active adults
let activeAdults = query {
    for user in users do
    where (user.Age >= 18)
    where (user.Active = true)
}
```

## Comparison Operators

Use standard F# comparison operators in `where` clauses:

```fsharp
// Equal
where (user.Status = "active")

// Not equal
where (user.Status <> "deleted")

// Greater than
where (user.Age > 18)

// Greater than or equal
where (user.Age >= 18)

// Less than
where (user.Age < 65)

// Less than or equal
where (user.Age <= 65)
```

### Comparison Examples

```fsharp
// Users between 18 and 65
let workingAge = query {
    for user in users do
    where (user.Age >= 18)
    where (user.Age < 65)
}

// High-value orders
let highValue = query {
    for order in orders do
    where (order.Total > 1000.0)
}
```

## Logical Operators

### AND Conditions

Use `&&` or multiple `where` clauses:

```fsharp
// Using &&
let admins = query {
    for user in users do
    where (user.Role = "admin" && user.Active = true)
}

// Using multiple where (equivalent)
let admins = query {
    for user in users do
    where (user.Role = "admin")
    where (user.Active = true)
}
```

### OR Conditions

Use `||` for OR logic:

```fsharp
// Admin or moderator
let privileged = query {
    for user in users do
    where (user.Role = "admin" || user.Role = "moderator")
}

// Multiple conditions
let featured = query {
    for post in posts do
    where (post.Featured = true || post.Views > 10000)
}
```

### NOT Conditions

Use `not` for negation:

```fsharp
// Not deleted
let active = query {
    for user in users do
    where (not (user.Status = "deleted"))
}
```

### Complex Logic

Combine operators for complex conditions:

```fsharp
// Active admins or any superuser
let privileged = query {
    for user in users do
    where ((user.Role = "admin" && user.Active = true) || user.Superuser = true)
}
```

## String Operations

Use string methods in `where` clauses:

```fsharp
// Contains substring
let gmail = query {
    for user in users do
    where (user.Email.Contains("@gmail.com"))
}

// Starts with prefix
let aNames = query {
    for user in users do
    where (user.Name.StartsWith("A"))
}

// Ends with suffix
let dotCom = query {
    for user in users do
    where (user.Email.EndsWith(".com"))
}
```

### String Examples

```fsharp
// Search by name prefix
let searchByPrefix prefix = query {
    for user in users do
    where (user.Name.StartsWith(prefix))
    sortBy user.Name
}

// Find users with company emails
let companyUsers = query {
    for user in users do
    where (user.Email.EndsWith("@mycompany.com"))
}
```

## Sorting

### Ascending Sort

```fsharp
let alphabetical = query {
    for user in users do
    sortBy user.Name
}
```

### Descending Sort

```fsharp
let newest = query {
    for post in posts do
    sortByDescending post.CreatedAt
}
```

### Combined with Filters

```fsharp
// Active users sorted by name
let activeByName = query {
    for user in users do
    where (user.Active = true)
    sortBy user.Name
}

// Top posts by views
let topPosts = query {
    for post in posts do
    where (post.Published = true)
    sortByDescending post.Views
    take 10
}
```

## Pagination

### Limit Results

```fsharp
// Get first 10
let first10 = query {
    for user in users do
    take 10
}
```

### Skip Results

```fsharp
// Skip first 20
let afterFirst20 = query {
    for user in users do
    skip 20
}
```

### Combined Pagination

```fsharp
// Page 3 (10 items per page)
let page3 = query {
    for user in users do
    sortBy user.Name
    skip 20
    take 10
}
```

## Nested Fields

Query nested object properties using dot notation:

```fsharp
type Address = { City: string; Country: string; ZipCode: string }
type User = { Name: string; Address: Address }

// Query nested field
let nycUsers = query {
    for user in users do
    where (user.Address.City = "New York")
}

// Multiple nested conditions
let usUsers = query {
    for user in users do
    where (user.Address.Country = "USA")
    where (user.Address.City = "New York")
}
```

## Projections

Use `select` to project specific fields:

```fsharp
// Select single field
let names = query {
    for user in users do
    select user.Name
}

// Select nested field
let cities = query {
    for user in users do
    select user.Address.City
}
```

## Executing Queries

### Get All Matching Documents

```fsharp
task {
    let myQuery = query {
        for user in users do
        where (user.Active = true)
        sortBy user.Name
    }
    let! results = users |> Collection.executeQuery myQuery
    for doc in results do
        printfn "%s" doc.Data.Name
}
```

### Count Matching Documents

```fsharp
task {
    let activeQuery = query {
        for user in users do
        where (user.Active = true)
    }
    let! count = users |> Collection.count activeQuery
    printfn "Active users: %d" count
}
```

### Find One Document

```fsharp
task {
    let emailQuery = query {
        for user in users do
        where (user.Email = "alice@example.com")
    }
    let! user = users |> Collection.findOne emailQuery
    match user with
    | Some doc -> printfn "Found: %s" doc.Data.Name
    | None -> printfn "Not found"
}
```

## Complete Examples

### User Search

```fsharp
let searchUsers namePrefix minAge maxAge = query {
    for user in users do
    where (user.Name.StartsWith(namePrefix))
    where (user.Age >= minAge)
    where (user.Age <= maxAge)
    where (user.Active = true)
    sortBy user.Name
    take 50
}
```

### Recent Posts

```fsharp
let recentPublished authorId = query {
    for post in posts do
    where (post.AuthorId = authorId)
    where (post.Published = true)
    sortByDescending post.CreatedAt
    take 10
}
```

### E-commerce Orders

```fsharp
let highValuePending = query {
    for order in orders do
    where (order.Total > 100.0)
    where (order.Status = "pending")
    sortByDescending order.Total
}
```

### Users by Location

```fsharp
let usersInCity city = query {
    for user in users do
    where (user.Address.City = city)
    where (user.Active = true)
    sortBy user.Name
}
```

## Query Expression Reference

| Operation | Syntax | Description |
|-----------|--------|-------------|
| Start | `for x in collection do` | Begin query |
| Filter | `where (predicate)` | Filter documents |
| Sort Ascending | `sortBy x.Field` | Sort ascending |
| Sort Descending | `sortByDescending x.Field` | Sort descending |
| Limit | `take n` | Limit results |
| Skip | `skip n` | Skip results |
| Project | `select x.Field` | Select field |

## Predicate Reference

| Operation | Syntax |
|-----------|--------|
| Equal | `x.Field = value` |
| Not Equal | `x.Field <> value` |
| Greater Than | `x.Field > value` |
| Greater or Equal | `x.Field >= value` |
| Less Than | `x.Field < value` |
| Less or Equal | `x.Field <= value` |
| AND | `pred1 && pred2` |
| OR | `pred1 \|\| pred2` |
| NOT | `not pred` |
| Contains | `x.Field.Contains("text")` |
| Starts With | `x.Field.StartsWith("prefix")` |
| Ends With | `x.Field.EndsWith("suffix")` |
