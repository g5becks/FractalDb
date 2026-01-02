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

// Execute with fluent API
let! results = myQuery.exec(collection)
```

### Simple Filter

```fsharp
// Find all active users
let activeQuery = query {
    for user in users do
    where (user.Active = true)
}

// Execute
let! results = activeQuery.exec(users)
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

## Pattern Matching with LIKE

Use `Sql.like` and `Sql.ilike` for SQL LIKE pattern matching with wildcards:

```fsharp
open FractalDb

// Case-sensitive LIKE pattern
let gmailUsers = query {
    for user in users do
    where (Sql.like "%@gmail.com" user.Email)
}

// Case-insensitive LIKE pattern (ilike)
let searchName = query {
    for user in users do
    where (Sql.ilike "%john%" user.Name)
}
```

### LIKE Pattern Syntax

| Pattern | Matches |
|---------|---------|
| `%` | Any sequence of characters (including empty) |
| `_` | Any single character |
| `%text` | Ends with "text" |
| `text%` | Starts with "text" |
| `%text%` | Contains "text" |
| `a_b` | "a" + any char + "b" (e.g., "aXb", "a1b") |

### LIKE Examples

```fsharp
// Find users with Gmail addresses
let gmailQuery = query {
    for user in users do
    where (Sql.like "%@gmail.com" user.Email)
}

// Case-insensitive search for names containing "smith"
let smithQuery = query {
    for user in users do
    where (Sql.ilike "%smith%" user.Name)
}

// Match specific pattern: 3-letter extension
let shortExtension = query {
    for file in files do
    where (Sql.like "%.___" file.Name)  // e.g., "file.txt", "doc.pdf"
}
```

**Note**: `Sql.like` is case-sensitive, while `Sql.ilike` is case-insensitive. The pattern is always the first argument, followed by the field.

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

### Multi-Level Sorting

Use `thenBy` and `thenByDescending` for secondary sort criteria:

```fsharp
// Sort by department, then by name within each department
let organized = query {
    for user in users do
    sortBy user.Department
    thenBy user.Name
}

// Sort by status ascending, then by date descending
let prioritized = query {
    for task in tasks do
    sortBy task.Status
    thenByDescending task.DueDate
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

## Nullable Sorting

When sorting fields that may contain NULL values, use nullable sort operators to control NULL placement:

### sortByNullable / sortByNullableDescending

These operators sort with NULL values appearing last:

```fsharp
// Sort by price, NULLs appear last
let productsByPrice = query {
    for product in products do
    sortByNullable product.Price
}

// Sort by rating descending, NULLs appear last
let topRated = query {
    for product in products do
    sortByNullableDescending product.Rating
}
```

### thenByNullable / thenByNullableDescending

Use for secondary nullable sorting:

```fsharp
// Primary sort by category, secondary sort by price with NULLs last
let organized = query {
    for product in products do
    sortBy product.Category
    thenByNullable product.Price
}

// Sort by status, then by optional priority descending
let tasks = query {
    for task in tasks do
    sortBy task.Status
    thenByNullableDescending task.Priority
}
```

### SQL Generated

Nullable sorting generates SQL that ensures NULLs appear last:

```sql
-- sortByNullable generates:
ORDER BY field IS NULL, field ASC

-- sortByNullableDescending generates:
ORDER BY field IS NULL, field DESC
```

### When to Use Nullable Sorting

| Scenario | Use |
|----------|-----|
| Field always has values | `sortBy` / `sortByDescending` |
| Field may be NULL, want NULLs last | `sortByNullable` / `sortByNullableDescending` |
| Secondary sort on nullable field | `thenByNullable` / `thenByNullableDescending` |

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

## Distinct Results

Use `distinct` to remove duplicate documents from results:

```fsharp
// Get unique categories
let uniqueCategories = query {
    for product in products do
    select product.Category
    distinct
}

// Distinct with filters
let activeStatuses = query {
    for order in orders do
    where (order.Year = 2024)
    select order.Status
    distinct
}
```

**Note**: `distinct` removes duplicate rows from the entire result set. When used with projections, it deduplicates the projected values.

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

## Aggregate Operators

FractalDb supports aggregate operations that compute a single value from matching documents.

### minBy / maxBy

Find minimum or maximum values:

```fsharp
// Find minimum age
let minAgeQuery = query {
    for user in users do
    minBy user.Age
}
let! minAge = users |> Collection.execAggregate minAgeQuery

// Find maximum score with filter
let maxScoreQuery = query {
    for user in users do
    where (user.Active = true)
    maxBy user.Score
}
let! maxScore = users |> Collection.execAggregate maxScoreQuery
```

### sumBy / averageBy

Calculate sums and averages:

```fsharp
// Sum of order totals
let totalQuery = query {
    for order in orders do
    where (order.Status = "completed")
    sumBy order.Total
}
let! total = orders |> Collection.execAggregate totalQuery

// Average rating
let avgQuery = query {
    for product in products do
    averageBy product.Rating
}
let! avgRating = products |> Collection.execAggregate avgQuery
```

### Nullable Aggregates

For fields that may contain NULL values, use nullable variants that return `Option`:

```fsharp
// minByNullable / maxByNullable
let minPriceQuery = query {
    for product in products do
    minByNullable product.Price
}
let! minPrice = products |> Collection.execAggregateNullable minPriceQuery
// Returns: int option (None if all values are NULL or no matches)

// sumByNullable / averageByNullable
let avgDiscountQuery = query {
    for product in products do
    averageByNullable product.Discount
}
let! avgDiscount = products |> Collection.execAggregateNullable avgDiscountQuery
```

### Aggregate Execution

| Query Type | Execution Function | Return Type |
|------------|-------------------|-------------|
| `minBy` / `maxBy` / `sumBy` / `averageBy` | `Collection.execAggregate` | `'T` |
| `minByNullable` / `maxByNullable` / `sumByNullable` / `averageByNullable` | `Collection.execAggregateNullable` | `'T option` |

## Element Operators

Element operators retrieve specific documents from the result set.

### head / headOrDefault

Get the first matching document:

```fsharp
// Get first (throws if empty)
let firstQuery = query {
    for user in users do
    where (user.Active = true)
    sortBy user.Name
    head
}
let! first = users |> Collection.execHead firstQuery

// Get first or None
let maybeFirst = query {
    for user in users do
    where (user.Role = "admin")
    headOrDefault
}
let! result = users |> Collection.execHeadOrDefault maybeFirst
```

### last / lastOrDefault

Get the last matching document (requires sorting):

```fsharp
// Get last (throws if empty)
let lastQuery = query {
    for user in users do
    sortBy user.CreatedAt
    last
}
let! lastUser = users |> Collection.execLast lastQuery

// Get last or None
let maybeLast = query {
    for user in users do
    sortByDescending user.Score
    lastOrDefault
}
let! result = users |> Collection.execLastOrDefault maybeLast
```

### exactlyOne / exactlyOneOrDefault

Get exactly one matching document:

```fsharp
// Exactly one (throws if 0 or >1 matches)
let singleQuery = query {
    for user in users do
    where (user.Email = "alice@example.com")
    exactlyOne
}
let! user = users |> Collection.execExactlyOne singleQuery

// Exactly one or None (throws if >1 matches)
let maybeSingle = query {
    for user in users do
    where (user.Id = userId)
    exactlyOneOrDefault
}
let! result = users |> Collection.execExactlyOneOrDefault maybeSingle
```

### nth

Get the nth document (0-indexed):

```fsharp
// Get 5th user (index 4)
let fifthQuery = query {
    for user in users do
    sortBy user.Name
    nth 4
}
let! fifthUser = users |> Collection.execNth fifthQuery
```

### find

Find the first document matching a predicate (throws if none found):

```fsharp
let findQuery = query {
    for user in users do
    find (user.Email = "bob@example.com")
}
let! found = users |> Collection.exec findQuery
```

### all

Check if all documents match a predicate:

```fsharp
let allActiveQuery = query {
    for user in users do
    all (user.Active = true)
}
let! allActive = users |> Collection.execAll allActiveQuery
// Returns: bool
```

### Element Operator Execution Summary

| Operator | Execution Function | Returns | Throws When |
|----------|-------------------|---------|-------------|
| `head` | `execHead` | `Doc<'T>` | Empty result |
| `headOrDefault` | `execHeadOrDefault` | `Doc<'T> option` | Never |
| `last` | `execLast` | `Doc<'T>` | Empty result |
| `lastOrDefault` | `execLastOrDefault` | `Doc<'T> option` | Never |
| `exactlyOne` | `execExactlyOne` | `Doc<'T>` | 0 or >1 results |
| `exactlyOneOrDefault` | `execExactlyOneOrDefault` | `Doc<'T> option` | >1 results |
| `nth n` | `execNth` | `Doc<'T>` | Index out of range |
| `find` | `exec` | `Doc<'T> list` | Predicate never matches |
| `all` | `execAll` | `bool` | Never |

## Grouping

Use `groupBy` to group documents by a field value:

```fsharp
// Group users by department
let byDepartment = query {
    for user in users do
    groupBy user.Department
}
let! groups = users |> Collection.execGroupBy byDepartment
// Returns: (string * Doc<User> list) list

// Process groups
for (dept, deptUsers) in groups do
    printfn "Department: %s (%d users)" dept (List.length deptUsers)
```

### Grouping with Filters

```fsharp
// Group active users by role
let activeByRole = query {
    for user in users do
    where (user.Active = true)
    groupBy user.Role
}
let! groups = users |> Collection.execGroupBy activeByRole

// Group orders by status
let ordersByStatus = query {
    for order in orders do
    where (order.Total > 100.0)
    groupBy order.Status
}
```

### GroupBy Execution

```fsharp
// Execute with Collection.execGroupBy
let! groups = collection |> Collection.execGroupBy groupQuery

// Each group is a tuple: (groupKey, documents)
for (key, docs) in groups do
    printfn "Group '%s' has %d items" (string key) (List.length docs)
```

**Note**: `groupBy` returns groups as tuples where the first element is the group key and the second is the list of documents in that group.

## Executing Queries

### Get All Matching Documents

Query expressions are executed using the fluent `.exec()` method on the query result:

```fsharp
task {
    let myQuery = query {
        for user in users do
        where (user.Active = true)
        sortBy user.Name
    }
    
    // Execute query expression with fluent API
    let! results = myQuery.exec(users)
    for doc in results do
        printfn "%s" doc.Data.Name
}
```

You can also use the module function (pipe-style):

```fsharp
let! results = users |> Collection.exec myQuery
```

### Composable Queries

Queries are composable - you can progressively build them using three different styles. All styles produce the same result but offer different ergonomics for different scenarios.

#### Style 1: `<+>` Operator (Declarative Composition)

The `<+>` operator combines separate query expressions into a single query. This is ideal for building reusable query components:

```fsharp
open FractalDb.QueryExpr

task {
    // Define reusable query parts
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
    
    for doc in results do
        printfn "%s (%d)" doc.Data.Name doc.Data.Age
}
```

**Use cases:**
- Building queries from reusable components
- Combining filters from different sources
- Modular query construction
- Conditional query building

```fsharp
// Conditional query composition
let baseQuery = query { for user in users do where (user.Active = true) }

let finalQuery =
    if includeAdults then
        baseQuery <+> query { for user in users do where (user.Age >= 18) }
    else
        baseQuery

let! results = finalQuery.exec(users)
```

#### Style 2: Fluent API (Method Chaining)

#### Style 2: Fluent API (Method Chaining)

Use fluent methods on `TranslatedQuery` for inline query building with method chaining:

```fsharp
task {
    // Start with base query
    let baseQuery = query { for user in users do where (user.Age >= 18) }

    // Build query progressively with fluent API
    let! results =
        baseQuery
            .where(Query.Field("active", FieldOp.Compare (box (CompareOp.Eq true))))
            .where(Query.Field("verified", FieldOp.Compare (box (CompareOp.Eq true))))
            .orderBy("createdAt", SortDirection.Desc)
            .skip(20)
            .limit(10)  // Page 3, 10 per page
            .exec(users)

    for doc in results do
        printfn "%s" doc.Data.Name
}
```

**Use cases:**
- Inline query building
- Object-oriented style
- Method chaining patterns
- Quick one-off queries

**Available Fluent Methods:**
- `.where(predicate)` - Add filter (ANDed with existing filters)
- `.orderBy(field, direction)` - Add sorting (can chain multiple)
- `.skip(n)` - Set pagination offset
- `.limit(n)` - Set maximum results
- `.exec(collection)` - Execute the query

#### Style 3: Pipeline API (F# Idiomatic)

For a more F#-idiomatic style, use the `QueryOps` module with pipeline operators:

```fsharp
open FractalDb.QueryExpr

task {
    // Start with base query
    let baseQuery = query { for user in users do where (user.Age >= 18) }

    // Build query with pipeline operators
    let composedQuery =
        baseQuery
        |> QueryOps.where (Query.Field("active", FieldOp.Compare (box (CompareOp.Eq true))))
        |> QueryOps.orderBy "name" SortDirection.Asc
        |> QueryOps.skip 20
        |> QueryOps.limit 10  // Page 3, 10 per page

    let! results = composedQuery.exec(users)

    for doc in results do
        printfn "%s" doc.Data.Name
}
```

**Use cases:**
- Functional pipelines
- F# idiomatic code style
- Building query transformations
- Composing with other pipeline operations

**Available Pipeline Functions:**

| Function | Signature | Description |
|----------|-----------|-------------|
| `QueryOps.where` | `Query<'T> -> TranslatedQuery<'T> -> TranslatedQuery<'T>` | Add filter predicate (AND) |
| `QueryOps.orderBy` | `string -> SortDirection -> TranslatedQuery<'T> -> TranslatedQuery<'T>` | Add sort specification |
| `QueryOps.skip` | `int -> TranslatedQuery<'T> -> TranslatedQuery<'T>` | Set skip count for pagination |
| `QueryOps.limit` | `int -> TranslatedQuery<'T> -> TranslatedQuery<'T>` | Set maximum result count |

**Pipeline Example with Custom Transformations:**

```fsharp
// Define a custom query transformation
let addAdminFilter query =
    query |> QueryOps.where (Query.Field("role", FieldOp.Compare (box (CompareOp.Eq "admin"))))

let addPagination pageNum pageSize query =
    query
    |> QueryOps.skip ((pageNum - 1) * pageSize)
    |> QueryOps.limit pageSize

// Use in pipeline
task {
    let! results =
        query { for user in users do where (user.Active = true) }
        |> addAdminFilter
        |> QueryOps.orderBy "name" SortDirection.Asc
        |> addPagination 3 10  // Page 3, 10 items per page
        |> fun q -> q.exec(users)
    
    for doc in results do
        printfn "%s" doc.Data.Name
}
```

#### Composition Rules

When composing queries, the following rules apply:

1. **Where clauses** are combined with AND logic
   ```fsharp
   let q1 = query { for u in users do where (u.Age >= 18) }
   let q2 = query { for u in users do where (u.Active = true) }
   let combined = q1 <+> q2  // WHERE age >= 18 AND active = true
   ```

2. **OrderBy clauses** are appended (for multi-level sorting)
   ```fsharp
   let q1 = query { for u in users do sortBy u.Name }
   let q2 = query { for u in users do sortByDescending u.Age }
   let combined = q1 <+> q2  // ORDER BY name ASC, age DESC
   ```

3. **Skip and Take** - the last one wins
   ```fsharp
   let q1 = query { for u in users do skip 10; take 20 }
   let q2 = query { for u in users do skip 5; take 10 }
   let combined = q1 <+> q2  // Uses: skip 5, take 10
   ```

4. **Projection** - the last one wins
   ```fsharp
   let q1 = query { for u in users do select u.Name }
   let q2 = query { for u in users do select u.Email }
   let combined = q1 <+> q2  // Projects: Email
   ```

5. **All styles can be mixed**
   ```fsharp
   let query =
       (baseQuery <+> filters)  // <+> operator
           .orderBy("name", SortDirection.Asc)  // Fluent
       |> QueryOps.skip 10  // Pipeline
       |> QueryOps.limit 20
   ```

#### Choosing a Style

| Style | Best For | Advantages |
|-------|----------|------------|
| `<+>` Operator | Reusable components, conditional composition | Declarative, modular, easy to test parts |
| Fluent API | Inline queries, method chaining | Familiar to OOP developers, good IntelliSense |
| Pipeline API | Functional style, transformations | F# idiomatic, composable with other functions |

**Recommendation:** Use `<+>` for building reusable query components, pipeline API for functional code, and fluent API for quick inline queries.

### Working with Query Filters

For operations like `count` and `findOne`, you need to use the Query module API instead of query expressions:

```fsharp
open FractalDb

task {
    // Count with Query module filter
    let activeFilter = 
        Query.Field("active", FieldOp.Compare(box (CompareOp.Eq true)))
    let! count = users |> Collection.count activeFilter
    printfn "Active users: %d" count
    
    // FindOne with Query module filter  
    let emailFilter = 
        Query.Field("email", FieldOp.Compare(box (CompareOp.Eq "alice@example.com")))
    let! user = users |> Collection.findOne emailFilter
    match user with
    | Some doc -> printfn "Found: %s" doc.Data.Name
    | None -> printfn "Not found"
}
```

**Note**: Currently, only `Collection.exec` and `.exec()` support query expressions. Other operations like `count` and `findOne` require Query module filters.

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
| Sort Nullable Asc | `sortByNullable x.Field` | Sort ascending, NULLs last |
| Sort Nullable Desc | `sortByNullableDescending x.Field` | Sort descending, NULLs last |
| Secondary Sort | `thenBy x.Field` | Secondary sort ascending |
| Secondary Sort Desc | `thenByDescending x.Field` | Secondary sort descending |
| Secondary Nullable | `thenByNullable x.Field` | Secondary sort, NULLs last |
| Secondary Nullable Desc | `thenByNullableDescending x.Field` | Secondary desc, NULLs last |
| Limit | `take n` | Limit results |
| Skip | `skip n` | Skip results |
| Project | `select x.Field` | Select field |
| Distinct | `distinct` | Remove duplicates |
| Min | `minBy x.Field` | Minimum value |
| Max | `maxBy x.Field` | Maximum value |
| Sum | `sumBy x.Field` | Sum values |
| Average | `averageBy x.Field` | Average value |
| Min Nullable | `minByNullable x.Field` | Min with NULL handling |
| Max Nullable | `maxByNullable x.Field` | Max with NULL handling |
| Sum Nullable | `sumByNullable x.Field` | Sum with NULL handling |
| Avg Nullable | `averageByNullable x.Field` | Avg with NULL handling |
| First | `head` | First element (throws if empty) |
| First or None | `headOrDefault` | First element or None |
| Last | `last` | Last element (throws if empty) |
| Last or None | `lastOrDefault` | Last element or None |
| Single | `exactlyOne` | Exactly one (throws if 0 or >1) |
| Single or None | `exactlyOneOrDefault` | Exactly one or None |
| Nth Element | `nth n` | Element at index n |
| Find | `find (predicate)` | Find first matching |
| All Match | `all (predicate)` | Check all match |
| Group | `groupBy x.Field` | Group by field |

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
| LIKE (case-sensitive) | `Sql.like "%pattern%" x.Field` |
| LIKE (case-insensitive) | `Sql.ilike "%pattern%" x.Field` |

## Execution Functions Reference

| Function | Input Query | Return Type | Description |
|----------|-------------|-------------|-------------|
| `exec` | Standard query | `Doc<'T> list` | Get all matching documents |
| `execHead` | `head` query | `Doc<'T>` | Get first document |
| `execHeadOrDefault` | `headOrDefault` query | `Doc<'T> option` | Get first or None |
| `execLast` | `last` query | `Doc<'T>` | Get last document |
| `execLastOrDefault` | `lastOrDefault` query | `Doc<'T> option` | Get last or None |
| `execExactlyOne` | `exactlyOne` query | `Doc<'T>` | Get single document |
| `execExactlyOneOrDefault` | `exactlyOneOrDefault` query | `Doc<'T> option` | Get single or None |
| `execNth` | `nth` query | `Doc<'T>` | Get nth document |
| `execAll` | `all` query | `bool` | Check all match predicate |
| `execAggregate` | Aggregate query | `'T` | Execute aggregate |
| `execAggregateNullable` | Nullable aggregate | `'T option` | Execute nullable aggregate |
| `execGroupBy` | `groupBy` query | `('K * Doc<'T> list) list` | Get grouped documents |
