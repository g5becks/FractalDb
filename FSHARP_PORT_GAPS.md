# FractalDb F# Port - Gaps and Implementation Plan

This document details all gaps between the F# port and the TypeScript original, plus improvements for robustness and usability.

---

## 1. Query API - F# Query Expressions

### Goal

Replace the current verbose query API with idiomatic F# query expressions. This becomes the **single, unified query API** - all other query construction methods will be removed.

### Current Problem

```fsharp
// Current - verbose, string-based, not type-safe
let q = query {
    field "status" (Query.eq "active")
    field "age" (Query.gte 18)
}
```

### Solution: Quotation-Based Query Expressions

Following the approach from [Tomas Petricek's query translation article](https://tomasp.net/blog/2015/query-translation/), we implement a computation expression builder with a `Quote` member that captures the query as a quotation, then translates it to SQL.

```fsharp
type User = { Name: string; Email: string; Age: int; Status: string; Role: string }

let users = db.Collection<User>("users", userSchema)

// Type-safe, idiomatic F# query expressions
let activeAdults =
    query {
        for user in users do
        where (user.Status = "active")
        where (user.Age >= 18)
        select user
    }

// OR conditions
let adminsOrMods =
    query {
        for user in users do
        where (user.Role = "admin" || user.Role = "moderator")
        select user
    }

// Sorting and pagination
let topUsers =
    query {
        for user in users do
        where (user.Status = "active")
        sortByDescending user.Age
        thenBy user.Name
        take 10
        select user
    }

// String operations
let gmailUsers =
    query {
        for user in users do
        where (user.Email.Contains("@gmail.com"))
        where (user.Name.StartsWith("John"))
        select user
    }

// Projections
let userEmails =
    query {
        for user in users do
        where (user.Status = "active")
        select user.Email
    }

// Anonymous type projection
let summaries =
    query {
        for user in users do
        select {| Name = user.Name; IsAdmin = (user.Role = "admin") |}
    }

// Aggregations
let activeCount =
    query {
        for user in users do
        where (user.Status = "active")
        count
    }
```

### Benefits

1. **Single API**: One way to write queries - no confusion
2. **Type Safety**: Compiler verifies field names and types
3. **IntelliSense**: Full autocomplete for record fields
4. **Refactoring**: Rename refactoring works automatically
5. **Familiar Syntax**: Standard F# query expression syntax
6. **No String Field Names**: No more `"status"` - use `user.Status`

### Implementation

Based on the quotation-translation pattern, the builder captures the query as a quotation via the `Quote` member, then `Run` translates and executes it.

```fsharp
// src/QueryExpr.fs
module FractalDb.QueryExpr

open System
open Microsoft.FSharp.Quotations
open Microsoft.FSharp.Quotations.Patterns
open Microsoft.FSharp.Quotations.DerivedPatterns
open FractalDb.Operators

// ============================================================
// QUERY DOMAIN MODEL
// ============================================================

/// Represents a translated query ready for execution
type TranslatedQuery<'T> = {
    Source: Collection<'T>
    Where: Query<'T>
    OrderBy: (string * SortDirection) list
    Skip: int option
    Take: int option
    Projection: Projection option
}

and Projection =
    | SelectAll
    | SelectFields of string list
    | SelectSingle of string

// ============================================================
// QUERY BUILDER
// ============================================================

type QueryBuilder() =

    /// Required: Enables `for x in source do` syntax
    member _.For(source: Collection<'T>, f: 'T -> TranslatedQuery<'T>) : TranslatedQuery<'T> =
        Unchecked.defaultof<_>  // Never executed - just for type inference

    /// Required: Enables `select x` syntax
    member _.Yield(v: 'T) : TranslatedQuery<'T> =
        Unchecked.defaultof<_>

    /// Required: Captures the query as a quotation
    member _.Quote(e: Expr<TranslatedQuery<'T>>) : Expr<TranslatedQuery<'T>> = e

    /// Required: Translates and executes the captured quotation
    member _.Run(q: Expr<TranslatedQuery<'T>>) : Task<'T seq> =
        let translated = QueryTranslator.translate q
        QueryExecutor.execute translated

    // ----- Custom Operations -----

    [<CustomOperation("where", MaintainsVariableSpace = true)>]
    member _.Where(
        source: TranslatedQuery<'T>,
        [<ProjectionParameter>] predicate: 'T -> bool
    ) : TranslatedQuery<'T> =
        Unchecked.defaultof<_>

    [<CustomOperation("sortBy", MaintainsVariableSpace = true)>]
    member _.SortBy(
        source: TranslatedQuery<'T>,
        [<ProjectionParameter>] keySelector: 'T -> 'Key
    ) : TranslatedQuery<'T> =
        Unchecked.defaultof<_>

    [<CustomOperation("sortByDescending", MaintainsVariableSpace = true)>]
    member _.SortByDescending(
        source: TranslatedQuery<'T>,
        [<ProjectionParameter>] keySelector: 'T -> 'Key
    ) : TranslatedQuery<'T> =
        Unchecked.defaultof<_>

    [<CustomOperation("thenBy", MaintainsVariableSpace = true)>]
    member _.ThenBy(
        source: TranslatedQuery<'T>,
        [<ProjectionParameter>] keySelector: 'T -> 'Key
    ) : TranslatedQuery<'T> =
        Unchecked.defaultof<_>

    [<CustomOperation("thenByDescending", MaintainsVariableSpace = true)>]
    member _.ThenByDescending(
        source: TranslatedQuery<'T>,
        [<ProjectionParameter>] keySelector: 'T -> 'Key
    ) : TranslatedQuery<'T> =
        Unchecked.defaultof<_>

    [<CustomOperation("take", MaintainsVariableSpace = true)>]
    member _.Take(source: TranslatedQuery<'T>, count: int) : TranslatedQuery<'T> =
        Unchecked.defaultof<_>

    [<CustomOperation("skip", MaintainsVariableSpace = true)>]
    member _.Skip(source: TranslatedQuery<'T>, count: int) : TranslatedQuery<'T> =
        Unchecked.defaultof<_>

    [<CustomOperation("select", AllowIntoPattern = true)>]
    member _.Select(
        source: TranslatedQuery<'T>,
        [<ProjectionParameter>] projection: 'T -> 'R
    ) : TranslatedQuery<'R> =
        Unchecked.defaultof<_>

    [<CustomOperation("count")>]
    member _.Count(source: TranslatedQuery<'T>) : int =
        Unchecked.defaultof<_>

    [<CustomOperation("exists", MaintainsVariableSpace = true)>]
    member _.Exists(
        source: TranslatedQuery<'T>,
        [<ProjectionParameter>] predicate: 'T -> bool
    ) : bool =
        Unchecked.defaultof<_>

    [<CustomOperation("head")>]
    member _.Head(source: TranslatedQuery<'T>) : 'T =
        Unchecked.defaultof<_>

    [<CustomOperation("headOrDefault")>]
    member _.HeadOrDefault(source: TranslatedQuery<'T>) : 'T option =
        Unchecked.defaultof<_>


// ============================================================
// QUOTATION TRANSLATOR
// ============================================================

module QueryTranslator =

    /// Active pattern: Match specific method calls
    let (|CallTo|_|) (methodName: string) (expr: Expr) =
        match expr with
        | SpecificCall <@ Unchecked.defaultof<QueryBuilder>.Where @> (_, _, args)
            when methodName = "where" -> Some args
        | SpecificCall <@ Unchecked.defaultof<QueryBuilder>.SortBy @> (_, _, args)
            when methodName = "sortBy" -> Some args
        | _ -> None

    /// Extract property name from expression
    let rec extractPropertyName (expr: Expr) : string =
        match expr with
        | PropertyGet (_, prop, _) -> prop.Name |> toCamelCase
        | Lambda (_, body) -> extractPropertyName body
        | _ -> failwith $"Cannot extract property from: {expr}"

    and toCamelCase (s: string) =
        if String.IsNullOrEmpty(s) then s
        else Char.ToLowerInvariant(s.[0]).ToString() + s.Substring(1)

    /// Translate a where predicate to Query<'T>
    let rec translatePredicate<'T> (expr: Expr) : Query<'T> =
        match expr with
        | Lambda (_, body) ->
            translatePredicate body

        // Property = Value
        | SpecificCall <@ (=) @> (_, _, [left; right]) ->
            let field = extractPropertyName left
            let value = evaluateExpr right
            Query.Field(field, FieldOp.Compare(box(CompareOp.Eq value)))

        // Property <> Value
        | SpecificCall <@ (<>) @> (_, _, [left; right]) ->
            let field = extractPropertyName left
            let value = evaluateExpr right
            Query.Field(field, FieldOp.Compare(box(CompareOp.Ne value)))

        // Property > Value
        | SpecificCall <@ (>) @> (_, _, [left; right]) ->
            let field = extractPropertyName left
            let value = evaluateExpr right
            Query.Field(field, FieldOp.Compare(box(CompareOp.Gt value)))

        // Property >= Value
        | SpecificCall <@ (>=) @> (_, _, [left; right]) ->
            let field = extractPropertyName left
            let value = evaluateExpr right
            Query.Field(field, FieldOp.Compare(box(CompareOp.Gte value)))

        // Property < Value
        | SpecificCall <@ (<) @> (_, _, [left; right]) ->
            let field = extractPropertyName left
            let value = evaluateExpr right
            Query.Field(field, FieldOp.Compare(box(CompareOp.Lt value)))

        // Property <= Value
        | SpecificCall <@ (<=) @> (_, _, [left; right]) ->
            let field = extractPropertyName left
            let value = evaluateExpr right
            Query.Field(field, FieldOp.Compare(box(CompareOp.Lte value)))

        // expr1 && expr2
        | SpecificCall <@ (&&) @> (_, _, [left; right]) ->
            Query.And [translatePredicate left; translatePredicate right]

        // expr1 || expr2
        | SpecificCall <@ (||) @> (_, _, [left; right]) ->
            Query.Or [translatePredicate left; translatePredicate right]

        // not expr
        | SpecificCall <@ not @> (_, _, [inner]) ->
            Query.Not (translatePredicate inner)

        // String.Contains
        | Call (Some target, method', [arg]) when method'.Name = "Contains" ->
            let field = extractPropertyName target
            let value = evaluateExpr arg :?> string
            Query.Field(field, FieldOp.String(StringOp.Contains value))

        // String.StartsWith
        | Call (Some target, method', [arg]) when method'.Name = "StartsWith" ->
            let field = extractPropertyName target
            let value = evaluateExpr arg :?> string
            Query.Field(field, FieldOp.String(StringOp.StartsWith value))

        // String.EndsWith
        | Call (Some target, method', [arg]) when method'.Name = "EndsWith" ->
            let field = extractPropertyName target
            let value = evaluateExpr arg :?> string
            Query.Field(field, FieldOp.String(StringOp.EndsWith value))

        | _ ->
            failwith $"Unsupported predicate: {expr}"

    and evaluateExpr (expr: Expr) : obj =
        Microsoft.FSharp.Linq.RuntimeHelpers.LeafExpressionConverter
            .EvaluateQuotation expr

    /// Main translation: quotation -> TranslatedQuery
    let translate<'T> (expr: Expr<TranslatedQuery<'T>>) : TranslatedQuery<'T> =
        let rec loop expr query =
            match expr with
            // For source in collection do ...
            | SpecificCall <@ Unchecked.defaultof<QueryBuilder>.For @>
                (_, _, [source; _]) ->
                let collection = evaluateExpr source :?> Collection<'T>
                { query with Source = collection }

            // where (predicate)
            | SpecificCall <@ Unchecked.defaultof<QueryBuilder>.Where @>
                (_, _, [source; predicate]) ->
                let q = loop source query
                let condition = translatePredicate predicate
                { q with Where = Query.And [q.Where; condition] |> simplify }

            // sortBy field
            | SpecificCall <@ Unchecked.defaultof<QueryBuilder>.SortBy @>
                (_, _, [source; selector]) ->
                let q = loop source query
                let field = extractPropertyName selector
                { q with OrderBy = q.OrderBy @ [(field, Ascending)] }

            // sortByDescending field
            | SpecificCall <@ Unchecked.defaultof<QueryBuilder>.SortByDescending @>
                (_, _, [source; selector]) ->
                let q = loop source query
                let field = extractPropertyName selector
                { q with OrderBy = q.OrderBy @ [(field, Descending)] }

            // take n
            | SpecificCall <@ Unchecked.defaultof<QueryBuilder>.Take @>
                (_, _, [source; Value (count, _)]) ->
                let q = loop source query
                { q with Take = Some (count :?> int) }

            // skip n
            | SpecificCall <@ Unchecked.defaultof<QueryBuilder>.Skip @>
                (_, _, [source; Value (count, _)]) ->
                let q = loop source query
                { q with Skip = Some (count :?> int) }

            // select ...
            | SpecificCall <@ Unchecked.defaultof<QueryBuilder>.Select @>
                (_, _, [source; projection]) ->
                let q = loop source query
                { q with Projection = Some (translateProjection projection) }

            | _ ->
                query

        let empty = {
            Source = Unchecked.defaultof<_>
            Where = Query.Empty
            OrderBy = []
            Skip = None
            Take = None
            Projection = None
        }

        loop (expr :> Expr) empty

    and simplify<'T> (q: Query<'T>) : Query<'T> =
        match q with
        | Query.And queries ->
            queries
            |> List.filter (fun q -> q <> Query.Empty)
            |> function
                | [] -> Query.Empty
                | [single] -> single
                | filtered -> Query.And filtered
        | other -> other

    and translateProjection (expr: Expr) : Projection =
        match expr with
        | Lambda (_, PropertyGet (_, prop, _)) ->
            SelectSingle (prop.Name |> toCamelCase)
        | Lambda (_, NewRecord (_, exprs)) ->
            let fields = exprs |> List.map extractPropertyName
            SelectFields fields
        | _ ->
            SelectAll


// ============================================================
// QUERY EXECUTOR
// ============================================================

module QueryExecutor =

    open FractalDb.Collection
    open FractalDb.Options

    let execute<'T> (query: TranslatedQuery<'T>) : Task<'T seq> =
        task {
            let options =
                QueryOptions.empty
                |> fun o ->
                    query.OrderBy
                    |> List.fold (fun opts (field, dir) ->
                        QueryOptions.sortBy field dir opts) o
                |> fun o ->
                    match query.Skip with
                    | Some n -> QueryOptions.skip n o
                    | None -> o
                |> fun o ->
                    match query.Take with
                    | Some n -> QueryOptions.limit n o
                    | None -> o

            let! results = query.Source |> Collection.findWith query.Where options
            return results |> Seq.map (fun doc -> doc.Data)
        }


// ============================================================
// GLOBAL BUILDER INSTANCE
// ============================================================

[<AutoOpen>]
module QueryBuilderInstance =
    let query = QueryBuilder()
```

### Supported Operators

| Operator | Example |
|----------|---------|
| `where` | `where (user.Age >= 18)` |
| `select` | `select user` or `select user.Email` |
| `sortBy` | `sortBy user.Name` |
| `sortByDescending` | `sortByDescending user.Age` |
| `thenBy` | `thenBy user.Email` |
| `thenByDescending` | `thenByDescending user.Name` |
| `take` | `take 10` |
| `skip` | `skip 20` |
| `count` | `count` |
| `exists` | `exists (user.Role = "admin")` |
| `head` | `head` (first element, throws if empty) |
| `headOrDefault` | `headOrDefault` (first or None) |

### Supported Predicates

| Syntax | SQL Translation |
|--------|-----------------|
| `user.Age = 18` | `age = 18` |
| `user.Age <> 18` | `age != 18` |
| `user.Age > 18` | `age > 18` |
| `user.Age >= 18` | `age >= 18` |
| `user.Age < 65` | `age < 65` |
| `user.Age <= 65` | `age <= 65` |
| `p1 && p2` | `(p1) AND (p2)` |
| `p1 \|\| p2` | `(p1) OR (p2)` |
| `not p` | `NOT (p)` |
| `user.Name.Contains("x")` | `name LIKE '%x%'` |
| `user.Name.StartsWith("x")` | `name LIKE 'x%'` |
| `user.Name.EndsWith("x")` | `name LIKE '%x'` |

### Full Example

```fsharp
open FractalDb
open FractalDb.QueryExpr

type User = {
    Name: string
    Email: string
    Age: int
    Status: string
    Role: string
    Country: string
}

let users = db.Collection<User>("users", userSchema)

// Simple query
let! activeUsers =
    query {
        for user in users do
        where (user.Status = "active")
        select user
    }

// Multiple conditions (AND)
let! eligibleVoters =
    query {
        for user in users do
        where (user.Status = "active")
        where (user.Age >= 18)
        where (user.Country = "US")
        select user
    }

// OR conditions
let! specialUsers =
    query {
        for user in users do
        where (user.Role = "admin" || user.Role = "moderator")
        where (user.Status = "active")
        select user
    }

// String operations
let! searchResults =
    query {
        for user in users do
        where (user.Email.Contains("@company.com"))
        where (user.Name.StartsWith("John"))
        select user
    }

// Sorting and pagination
let! pagedResults =
    query {
        for user in users do
        where (user.Status = "active")
        sortByDescending user.Age
        thenBy user.Name
        skip 40
        take 20
        select user
    }

// Projection - single field
let! emails =
    query {
        for user in users do
        where (user.Status = "active")
        select user.Email
    }

// Projection - anonymous record
let! summaries =
    query {
        for user in users do
        select {| Name = user.Name; IsAdmin = (user.Role = "admin") |}
    }

// Count
let! activeCount =
    query {
        for user in users do
        where (user.Status = "active")
        count
    }

// Existence check
let! hasAdmins =
    query {
        for user in users do
        exists (user.Role = "admin")
    }

// First or default
let! maybeOldest =
    query {
        for user in users do
        sortByDescending user.Age
        headOrDefault
    }
```

### Implementation Phases

**Phase 1: Implement Query Expressions (Keep Existing APIs)**
1. Create `src/QueryExpr.fs` with the quotation-based builder
2. Implement `QueryTranslator` module for quotation → Query translation
3. Implement all operators: `where`, `select`, `sortBy`, `take`, `skip`, `count`, `exists`, `head`
4. Keep all existing APIs (`Query.eq`, `QueryBuilder`, etc.) unchanged

**Phase 2: Comprehensive Testing**
Create thorough tests before considering any API removal:

```fsharp
// tests/QueryExprTests.fs - Must have 100% coverage of:

// Basic predicates
[<Test>] let ``where with equality`` () = ...
[<Test>] let ``where with inequality`` () = ...
[<Test>] let ``where with greater than`` () = ...
[<Test>] let ``where with greater than or equal`` () = ...
[<Test>] let ``where with less than`` () = ...
[<Test>] let ``where with less than or equal`` () = ...

// Logical operators
[<Test>] let ``where with AND`` () = ...
[<Test>] let ``where with OR`` () = ...
[<Test>] let ``where with NOT`` () = ...
[<Test>] let ``where with nested AND/OR`` () = ...
[<Test>] let ``multiple where clauses combine with AND`` () = ...

// String operations
[<Test>] let ``where with Contains`` () = ...
[<Test>] let ``where with StartsWith`` () = ...
[<Test>] let ``where with EndsWith`` () = ...
[<Test>] let ``string operations are case-sensitive`` () = ...

// Sorting
[<Test>] let ``sortBy ascending`` () = ...
[<Test>] let ``sortByDescending`` () = ...
[<Test>] let ``thenBy secondary sort`` () = ...
[<Test>] let ``thenByDescending secondary sort`` () = ...
[<Test>] let ``multiple sort fields`` () = ...

// Pagination
[<Test>] let ``take limits results`` () = ...
[<Test>] let ``skip offsets results`` () = ...
[<Test>] let ``skip and take together`` () = ...
[<Test>] let ``take zero returns empty`` () = ...
[<Test>] let ``skip beyond count returns empty`` () = ...

// Projections
[<Test>] let ``select single field`` () = ...
[<Test>] let ``select multiple fields as anonymous record`` () = ...
[<Test>] let ``select entire entity`` () = ...
[<Test>] let ``select with computed field`` () = ...

// Aggregations
[<Test>] let ``count all`` () = ...
[<Test>] let ``count with where`` () = ...
[<Test>] let ``exists returns true when match found`` () = ...
[<Test>] let ``exists returns false when no match`` () = ...
[<Test>] let ``head returns first element`` () = ...
[<Test>] let ``head throws on empty`` () = ...
[<Test>] let ``headOrDefault returns Some on match`` () = ...
[<Test>] let ``headOrDefault returns None on empty`` () = ...

// Edge cases
[<Test>] let ``empty collection returns empty results`` () = ...
[<Test>] let ``null string handling`` () = ...
[<Test>] let ``special characters in string values`` () = ...
[<Test>] let ``unicode in string values`` () = ...
[<Test>] let ``very large result sets`` () = ...
[<Test>] let ``deeply nested boolean expressions`` () = ...

// Nested fields
[<Test>] let ``where on nested property`` () = ...
[<Test>] let ``sortBy nested property`` () = ...
[<Test>] let ``select nested property`` () = ...

// Integration with existing system
[<Test>] let ``query expression produces same SQL as Query.field`` () = ...
[<Test>] let ``query expression works with transactions`` () = ...
[<Test>] let ``query expression works with schema validation`` () = ...

// Error handling
[<Test>] let ``unsupported predicate throws clear error`` () = ...
[<Test>] let ``invalid property access throws clear error`` () = ...
```

**Phase 3: Parallel Testing Period**
- Run both APIs side-by-side for a release cycle
- Verify query expressions produce identical results to existing API
- Gather feedback on edge cases and missing features
- Document any behavioral differences

**Phase 4: Deprecation (Only After Phase 2-3 Complete)**
Once query expressions are proven reliable:
1. Mark existing query APIs as `[<Obsolete>]`
2. Update documentation to recommend query expressions
3. Keep deprecated APIs for at least one major version
4. Remove deprecated APIs only in a future major version

### Current Status

| Phase | Status |
|-------|--------|
| Phase 1: Implementation | Not Started |
| Phase 2: Testing | Not Started |
| Phase 3: Parallel Period | Not Started |
| Phase 4: Deprecation | Not Started |

**The existing `Query.eq`, `QueryBuilder`, etc. APIs remain fully supported until Phase 4.**

---

## 2. Missing API Features

### 2.1 Simplified DbOptions (Donald-Style)

Following Donald's philosophy of working generically with ADO.NET, we should keep DbOptions minimal. The `onClose` callback from TypeScript is **unnecessary** in F#:

- If using `FromConnection`: User already holds the connection reference
- If using `Open`/`InMemory`: User can use `use` binding and do cleanup after

**Simplified Design:**

```fsharp
// In Database.fs
type DbOptions = {
    IdGenerator: unit -> string  // For document IDs (ULID, UUID, etc.)
    EnableCache: bool            // Query result caching
}

module DbOptions =
    let defaults = {
        IdGenerator = IdGenerator.generate  // Default: ULID
        EnableCache = false
    }

    let withIdGenerator gen opts = { opts with IdGenerator = gen }
    let withCache enabled opts = { opts with EnableCache = enabled }
```

**Usage patterns:**

```fsharp
// Pattern 1: FractalDb manages connection
use db = FractalDb.Open("mydb.db")
// ... use db ...
// Auto-disposed, connection closed

// Pattern 2: User manages connection (Donald-style)
use conn = new SqliteConnection("Data Source=mydb.db;Mode=ReadWriteCreate")
conn.Open()

// Configure SQLite however you want
conn |> Db.newCommand "PRAGMA journal_mode=WAL" |> Db.exec
conn |> Db.newCommand "PRAGMA synchronous=NORMAL" |> Db.exec

// Wrap with FractalDb for document abstraction
let db = FractalDb.FromConnection(conn)
// ... use db for document operations ...
// ... use conn directly with Donald for custom queries ...

db.Close()  // Does NOT close conn (we don't own it)
// conn still usable, closed when it goes out of scope
```

### 2.2 FromConnection Factory Method

**TypeScript has:**
```typescript
database: string | SQLiteDatabase  // Can pass existing connection
```

**Implementation:**

```fsharp
// In Database.fs - add ownsConnection flag
type FractalDb private (connection: IDbConnection, options: DbOptions, ownsConnection: bool) =
    let mutable disposed = false

    /// The underlying connection (for advanced Donald usage)
    member _.Connection = connection

    /// Database options
    member _.Options = options

    /// Whether this instance owns (and will dispose) the connection
    member _.OwnsConnection = ownsConnection

    // Factory methods
    static member Open(path: string, ?options: DbOptions) : FractalDb =
        let opts = defaultArg options DbOptions.defaults
        let conn = new SqliteConnection($"Data Source={path}")
        conn.Open()
        new FractalDb(conn, opts, true)  // owns the connection

    static member InMemory(?options: DbOptions) : FractalDb =
        FractalDb.Open(":memory:", ?options = options)

    /// Create from an existing open connection (caller manages connection lifecycle)
    /// Works with any IDbConnection for Donald compatibility
    static member FromConnection(connection: IDbConnection, ?options: DbOptions) : FractalDb =
        let opts = defaultArg options DbOptions.defaults
        new FractalDb(connection, opts, false)  // does NOT own the connection

    member this.Close() =
        if not disposed then
            if ownsConnection then
                connection.Close()
                connection.Dispose()
            disposed <- true

    interface IDisposable with
        member this.Dispose() = this.Close()
```

**Key Design Decisions:**

1. **`IDbConnection` not `SqliteConnection`** - Following Donald's philosophy, accept any ADO.NET connection. This makes FractalDb more flexible.

2. **`OwnsConnection` flag** - Only dispose connections we created. User-provided connections are their responsibility.

3. **Expose `Connection` property** - Allows advanced users to use Donald directly for custom SQL while using FractalDb for document operations.

### 2.3 CollectionOptions

**TypeScript has:**
```typescript
type CollectionOptions = {
  enableCache?: boolean  // Override database-level cache setting
}
```

**Implementation:**

```fsharp
// In Database.fs or Options.fs
type CollectionOptions = {
    EnableCache: bool option  // None = inherit from database
}

module CollectionOptions =
    let defaults = { EnableCache = None }

// Update FractalDb.Collection
member this.Collection<'T>(name: string, schema: SchemaDef<'T>, ?options: CollectionOptions) =
    let collOpts = defaultArg options CollectionOptions.defaults
    let enableCache =
        collOpts.EnableCache
        |> Option.defaultValue this.Options.EnableCache
    // ... rest of implementation using enableCache
```

### 2.4 Missing Collection Methods

**TypeScript has but F# is missing:**

```fsharp
// In Collection.fs - add these functions

/// Get distinct values for a field
let distinct<'T, 'V> (field: string) (filter: Query<'T>) (collection: Collection<'T>) : Task<FractalResult<list<'V>>> =
    task {
        let whereSql, whereParams = collection.Translator.TranslateQuery(filter)
        let sql = $"SELECT DISTINCT json_extract(data, '$.{field}') as value FROM {collection.Name} WHERE {whereSql}"
        // Execute and deserialize results
    }

/// Get estimated document count (faster than count for large collections)
let estimatedCount<'T> (collection: Collection<'T>) : Task<int> =
    task {
        let sql = $"SELECT COUNT(*) FROM {collection.Name}"
        // Execute and return count
    }

/// Validate a document against the schema without inserting
let validate<'T> (data: 'T) (collection: Collection<'T>) : FractalResult<'T> =
    match collection.Schema.Validate with
    | Some validator -> validator data
    | None -> Ok data

/// Drop the entire collection (delete table)
let drop<'T> (collection: Collection<'T>) : Task<FractalResult<unit>> =
    task {
        let sql = $"DROP TABLE IF EXISTS {collection.Name}"
        // Execute DDL
    }

/// Dedicated text search method (not just via QueryOptions)
let search<'T> (searchText: string) (fields: list<string>) (collection: Collection<'T>) : Task<FractalResult<list<Document<'T>>>> =
    let options =
        QueryOptions.empty
        |> QueryOptions.search searchText fields
    find Query.empty options collection
```

### 2.5 Standard Schema Validator Integration

**TypeScript has:**
```typescript
// Wrap Zod, Valibot, ArkType validators
const schema = wrapStandardSchema(zodSchema)
```

**Implementation:**
```fsharp
// In Validation.fs - interop with .NET validators

/// Wrap a function that throws on validation failure
let wrapThrowingValidator<'T> (validate: 'T -> unit) : 'T -> FractalResult<'T> =
    fun data ->
        try
            validate data
            Ok data
        with
        | ex -> Error (FractalError.Validation (None, ex.Message))

/// Wrap FluentValidation validator
let wrapFluentValidator<'T> (validator: AbstractValidator<'T>) : 'T -> FractalResult<'T> =
    fun data ->
        let result = validator.Validate(data)
        if result.IsValid then
            Ok data
        else
            let errors = result.Errors |> Seq.map (fun e -> e.ErrorMessage) |> String.concat "; "
            Error (FractalError.Validation (None, errors))
```

### 2.6 Timestamp Configuration

**TypeScript has:**
```typescript
type TimestampConfig = {
  createdAt?: boolean | string  // Field name or disable
  updatedAt?: boolean | string  // Field name or disable
}
```

**Implementation:**
```fsharp
// In Schema.fs or Options.fs
type TimestampConfig = {
    CreatedAtField: string option  // None = disabled, Some name = custom field
    UpdatedAtField: string option
    Enabled: bool
}

module TimestampConfig =
    let defaults = {
        CreatedAtField = Some "createdAt"
        UpdatedAtField = Some "updatedAt"
        Enabled = true
    }

    let disabled = {
        CreatedAtField = None
        UpdatedAtField = None
        Enabled = false
    }

// Update SchemaDef
type SchemaDef<'T> = {
    Fields: list<FieldDef>
    Indexes: list<IndexDef>
    Timestamps: TimestampConfig  // Changed from bool
    Validate: option<'T -> Result<'T, string>>
}
```

### 2.7 Query Caching Implementation

**TypeScript has:**
```typescript
type DatabaseOptions = {
  enableCache?: boolean  // Query result caching
}
```

**Implementation:**
```fsharp
// In Cache.fs (new file)
module FractalDb.Cache

open System.Collections.Concurrent

type QueryCache<'T>() =
    let cache = ConcurrentDictionary<string, obj>()
    let maxEntries = 1000

    member _.TryGet(key: string) : option<'T> =
        match cache.TryGetValue(key) with
        | true, value -> Some (value :?> 'T)
        | false, _ -> None

    member _.Set(key: string, value: 'T) =
        if cache.Count >= maxEntries then
            // Simple eviction: clear oldest half
            let keys = cache.Keys |> Seq.take (maxEntries / 2) |> Seq.toList
            for k in keys do cache.TryRemove(k) |> ignore
        cache.[key] <- box value

    member _.Invalidate(pattern: string option) =
        match pattern with
        | None -> cache.Clear()
        | Some p ->
            let keysToRemove =
                cache.Keys
                |> Seq.filter (fun k -> k.Contains(p))
                |> Seq.toList
            for k in keysToRemove do cache.TryRemove(k) |> ignore

// DbOptions with cache configuration
type DbOptions = {
    IdGenerator: unit -> string
    EnableCache: bool
    CacheMaxEntries: int option  // Default: 1000
}

module DbOptions =
    let defaults = {
        IdGenerator = IdGenerator.generate
        EnableCache = false
        CacheMaxEntries = None  // Uses default of 1000
    }
```

---

## 3. Missing Test Coverage

### 3.1 Core Types Tests (HIGH PRIORITY)

Create `tests/TypesTests.fs`:

```fsharp
module FractalDb.Tests.TypesTests

// IdGenerator tests
[<Test>]
let ``IdGenerator.generate returns non-empty string`` () = ...

[<Test>]
let ``IdGenerator.generate returns valid GUID format`` () = ...

[<Test>]
let ``IdGenerator.isEmptyOrDefault returns true for empty string`` () = ...

[<Test>]
let ``IdGenerator.isEmptyOrDefault returns true for Guid.Empty`` () = ...

[<Test>]
let ``IdGenerator.isValid returns true for valid GUID`` () = ...

[<Test>]
let ``IdGenerator.isValid returns false for invalid string`` () = ...

// Timestamp tests
[<Test>]
let ``Timestamp.now returns value greater than 0`` () = ...

[<Test>]
let ``Timestamp.toDateTimeOffset and fromDateTimeOffset roundtrip`` () = ...

[<Test>]
let ``Timestamp.isInRange returns true for timestamp in range`` () = ...

[<Test>]
let ``Timestamp.isInRange returns false for timestamp out of range`` () = ...

// Document tests
[<Test>]
let ``Document.create generates non-empty Id`` () = ...

[<Test>]
let ``Document.create sets CreatedAt and UpdatedAt`` () = ...

[<Test>]
let ``Document.update preserves Id and CreatedAt`` () = ...

[<Test>]
let ``Document.update updates UpdatedAt`` () = ...

[<Test>]
let ``Document.map transforms Data correctly`` () = ...

[<Test>]
let ``Document.map preserves all metadata`` () = ...
```

### 3.2 Nested Fields & Path Tests (HIGH PRIORITY)

Create `tests/NestedFieldsTests.fs`:

```fsharp
module FractalDb.Tests.NestedFieldsTests

type Profile = { Bio: string; Website: string option }
type Address = { City: string; ZipCode: string; Country: string }
type Settings = { Theme: string; Notifications: bool }
type Person = { Name: string; Address: Address; Profile: Profile; Settings: Settings; Tags: list<string> }

// Query operations
[<Test>]
let ``Query nested field with dot notation`` () = ...

[<Test>]
let ``Query deeply nested field (3+ levels)`` () = ...

[<Test>]
let ``Multiple nested field conditions`` () = ...

// CRUD operations
[<Test>]
let ``Update nested field preserves other fields`` () = ...

[<Test>]
let ``Insert document with nested objects`` () = ...

// Sorting
[<Test>]
let ``Sort by nested field`` () = ...

// Indexing
[<Test>]
let ``Index on nested field works correctly`` () = ...

// Edge cases
[<Test>]
let ``Null handling in nested field`` () = ...

[<Test>]
let ``Option<'T> field within nested object`` () = ...

// Projections
[<Test>]
let ``Select projection includes nested field`` () = ...

[<Test>]
let ``Omit projection excludes nested field`` () = ...
```

### 3.3 Projection Tests (HIGH PRIORITY)

Create `tests/ProjectionTests.fs`:

```fsharp
module FractalDb.Tests.ProjectionTests

[<Test>]
let ``Select specific fields returns only those fields`` () = ...

[<Test>]
let ``Select always includes _id field`` () = ...

[<Test>]
let ``Omit specific fields excludes them from result`` () = ...

[<Test>]
let ``Omit does not affect _id field`` () = ...

[<Test>]
let ``Projection with nested fields`` () = ...

[<Test>]
let ``Empty select returns all fields`` () = ...

[<Test>]
let ``Invalid field name in select is ignored`` () = ...
```

### 3.4 Cursor Pagination Tests (HIGH PRIORITY)

Create `tests/CursorPaginationTests.fs`:

```fsharp
module FractalDb.Tests.CursorPaginationTests

[<Test>]
let ``cursorAfter returns documents after specified ID`` () = ...

[<Test>]
let ``cursorBefore returns documents before specified ID`` () = ...

[<Test>]
let ``Cursor pagination with sorting`` () = ...

[<Test>]
let ``Cursor with non-existent ID returns empty`` () = ...

[<Test>]
let ``Cursor pagination is stable across modifications`` () = ...

[<Test>]
let ``First page has no cursor requirement`` () = ...

[<Test>]
let ``Last page cursorAfter returns empty`` () = ...
```

### 3.5 Text Search Tests (MEDIUM PRIORITY)

Create `tests/TextSearchTests.fs`:

```fsharp
module FractalDb.Tests.TextSearchTests

[<Test>]
let ``Search finds documents containing term`` () = ...

[<Test>]
let ``Search across multiple fields`` () = ...

[<Test>]
let ``Search is case-insensitive by default`` () = ...

[<Test>]
let ``Search with no matches returns empty`` () = ...

[<Test>]
let ``Search combined with filter`` () = ...

[<Test>]
let ``Search with special characters`` () = ...
```

### 3.6 Error Handling & Donald Exception Tests (MEDIUM PRIORITY)

Create `tests/ErrorTests.fs`:

```fsharp
module FractalDb.Tests.ErrorTests

// FractalError tests
[<Test>]
let ``Validation error includes field name`` () = ...

[<Test>]
let ``UniqueConstraint error includes field and value`` () = ...

[<Test>]
let ``Query error includes SQL when available`` () = ...

[<Test>]
let ``NotFound error includes document ID`` () = ...

[<Test>]
let ``Serialization error includes helpful message`` () = ...

[<Test>]
let ``Connection error is distinguishable from other errors`` () = ...

[<Test>]
let ``Transaction error on rollback failure`` () = ...

// Donald exception mapping tests
[<Test>]
let ``DbConnectionException maps to FractalError.Connection with connection string`` () = ...

[<Test>]
let ``DbExecutionException maps to FractalError.Query with SQL statement`` () = ...

[<Test>]
let ``DbExecutionException with SQLite error 19 maps to UniqueConstraint`` () = ...

[<Test>]
let ``DbExecutionException with SQLite error 5 maps to Connection (database locked)`` () = ...

[<Test>]
let ``DbReaderException maps to FractalError.Serialization with field name`` () = ...

[<Test>]
let ``DbTransactionException maps to FractalError.Transaction with step`` () = ...

[<Test>]
let ``Unknown exception maps to generic Query error`` () = ...
```

### 3.7 Edge Case Tests (MEDIUM PRIORITY)

Create `tests/EdgeCaseTests.fs`:

```fsharp
module FractalDb.Tests.EdgeCaseTests

// String edge cases
[<Test>]
let ``Empty string field value`` () = ...

[<Test>]
let ``Unicode characters in field values`` () = ...

[<Test>]
let ``Very long string (10000+ chars)`` () = ...

[<Test>]
let ``Special SQL characters are escaped`` () = ...

// Numeric edge cases
[<Test>]
let ``Int64.MaxValue is stored correctly`` () = ...

[<Test>]
let ``Int64.MinValue is stored correctly`` () = ...

[<Test>]
let ``Floating point precision is maintained`` () = ...

// Collection edge cases
[<Test>]
let ``Empty array field`` () = ...

[<Test>]
let ``Large array (1000+ elements)`` () = ...

[<Test>]
let ``Deeply nested object (10+ levels)`` () = ...

// Query edge cases
[<Test>]
let ``IN operator with empty list`` () = ...

[<Test>]
let ``IN operator with 1000+ values`` () = ...

[<Test>]
let ``Query with only Empty conditions`` () = ...

[<Test>]
let ``Deeply nested AND/OR (10+ levels)`` () = ...
```

### 3.8 Concurrency Tests (MEDIUM PRIORITY)

Create `tests/ConcurrencyTests.fs`:

```fsharp
module FractalDb.Tests.ConcurrencyTests

[<Test>]
let ``Concurrent inserts do not lose data`` () = ...

[<Test>]
let ``Concurrent updates with transactions are isolated`` () = ...

[<Test>]
let ``Read during write returns consistent data`` () = ...

[<Test>]
let ``Multiple transactions on same document`` () = ...

[<Test>]
let ``Deadlock prevention with multiple collections`` () = ...
```

### 3.9 Resource Cleanup Tests (LOW PRIORITY)

Create `tests/DisposableTests.fs`:

```fsharp
module FractalDb.Tests.DisposableTests

[<Test>]
let ``Database.Close releases file lock`` () = ...

[<Test>]
let ``Database.Dispose is idempotent`` () = ...

[<Test>]
let ``Transaction.Dispose rolls back uncommitted changes`` () = ...

[<Test>]
let ``Using block properly disposes database`` () = ...

[<Test>]
let ``FromConnection does not dispose user-provided connection`` () = ...
```

### 3.10 Cache Tests (LOW PRIORITY)

Create `tests/CacheTests.fs`:

```fsharp
module FractalDb.Tests.CacheTests

[<Test>]
let ``Cache enabled improves repeated query performance`` () = ...

[<Test>]
let ``Cache disabled does not store queries`` () = ...

[<Test>]
let ``Collection-level cache overrides database setting`` () = ...

[<Test>]
let ``Cache eviction when limit reached`` () = ...
```

### 3.11 Index Management Tests (HIGH PRIORITY)

Create `tests/IndexTests.fs`:

```fsharp
module FractalDb.Tests.IndexTests

[<Test>]
let ``createIndex creates single-field index`` () = ...

[<Test>]
let ``createIndex creates compound index`` () = ...

[<Test>]
let ``createIndex with unique constraint enforces uniqueness`` () = ...

[<Test>]
let ``Query on indexed field uses index`` () = ...

[<Test>]
let ``Query on non-indexed field still works`` () = ...

[<Test>]
let ``Duplicate index creation is idempotent`` () = ...

[<Test>]
let ``Schema-defined indexes are created on collection init`` () = ...
```

### 3.12 Array Operator Tests (HIGH PRIORITY)

Create `tests/ArrayOperatorTests.fs`:

```fsharp
module FractalDb.Tests.ArrayOperatorTests

type TaggedItem = { Name: string; Tags: list<string>; Scores: list<int> }

[<Test>]
let ``Array.all matches documents with all values`` () = ...

[<Test>]
let ``Array.all with empty list matches all documents`` () = ...

[<Test>]
let ``Array.size matches exact array length`` () = ...

[<Test>]
let ``Array.size with zero matches empty arrays`` () = ...

[<Test>]
let ``Array.elemMatch finds matching element`` () = ...

[<Test>]
let ``Array.index matches specific element`` () = ...

[<Test>]
let ``isIn operator with array values`` () = ...

[<Test>]
let ``notIn operator with array values`` () = ...

[<Test>]
let ``Complex array query with AND/OR`` () = ...
```

### 3.13 Multi-Collection Transaction Tests (MEDIUM PRIORITY)

Create `tests/MultiCollectionTests.fs`:

```fsharp
module FractalDb.Tests.MultiCollectionTests

type User = { Name: string; Email: string }
type Order = { UserId: string; Amount: decimal }

[<Test>]
let ``Transaction across two collections commits both`` () = ...

[<Test>]
let ``Transaction across two collections rolls back both on error`` () = ...

[<Test>]
let ``Insert in collection A, query in collection B within transaction`` () = ...

[<Test>]
let ``Delete from one collection, update another in same transaction`` () = ...

[<Test>]
let ``Nested transactions are not supported (expected error)`` () = ...
```

### 3.14 Performance and Large Dataset Tests (LOW PRIORITY)

Create `tests/PerformanceTests.fs`:

```fsharp
module FractalDb.Tests.PerformanceTests

[<Test>]
let ``Insert 10000 documents completes in reasonable time`` () = ...

[<Test>]
let ``Query with index is faster than without`` () = ...

[<Test>]
let ``Batch insert is faster than individual inserts`` () = ...

[<Test>]
let ``Cursor pagination scales better than offset for large datasets`` () = ...

[<Test>]
let ``Memory usage stays bounded with large result sets`` () = ...
```

---

## 4. Test Coverage Summary

### Current State

| Category | Tests | Coverage |
|----------|-------|----------|
| CRUD Operations | 6 | Good |
| Validation | 9 | Good |
| Builders | 24 | Good |
| Query Construction | 24 | Good |
| Query Execution | 10 | Partial |
| Transactions | 7 | Partial |
| Atomic Operations | 8 | Good |
| Batch Operations | 6 | Good |
| SQL Translation | 44 | Good |
| Serialization | 9 | Good |
| Unique Constraints | 1 | Minimal |
| **Core Types** | **0** | **Missing** |
| **Index Management** | **0** | **Missing** |
| **Array Operators** | **0** | **Missing** |
| **Nested Fields/Paths** | **0** | **Missing** |
| **Projections (Select/Omit execution)** | **0** | **Missing** |
| **Cursor Pagination** | **0** | **Missing** |
| **Text Search** | **0** | **Missing** |
| **Error Messages** | **0** | **Missing** |
| **Edge Cases** | **0** | **Missing** |
| **Concurrency** | **0** | **Missing** |
| **Resource Cleanup** | **0** | **Missing** |
| **Multi-Collection Transactions** | **0** | **Missing** |
| **Large Dataset/Performance** | **0** | **Missing** |

**Current Total: 142 tests (16 test modules)**
**Target: ~250+ tests**

---

## 5. Additional Improvements

### 5.1 Donald Exception Handling (HIGH PRIORITY)

**Current state:** Only `DbExecutionException` is caught, and only for UNIQUE constraint violations.

**Problem:** Donald throws four distinct exception types with rich metadata that we're not utilizing:

```fsharp
// Donald's exception types
DbConnectionException    // Connection failures, includes ConnectionString
DbExecutionException     // Command execution failures, includes Statement, Step
DbReaderException        // Field access/casting failures, includes FieldName
DbTransactionException   // Commit/rollback failures, includes Step
```

**Implementation:**

```fsharp
// In a new Exceptions.fs or update Errors.fs

/// Convert Donald exceptions to FractalError with full context
let mapDonaldException (ex: exn) : FractalError =
    match ex with
    | :? DbConnectionException as e ->
        let connStr = e.ConnectionString |> Option.defaultValue "<unknown>"
        FractalError.Connection $"{e.Message} (connection: {connStr})"

    | :? DbExecutionException as e ->
        let sql = e.Statement |> Option.defaultValue "<no statement>"
        let step = e.Step |> Option.map string |> Option.defaultValue ""
        // Check for specific SQLite errors
        match e.InnerException with
        | :? SqliteException as sqlEx when sqlEx.SqliteErrorCode = 19 ->
            // UNIQUE constraint - parse field name
            let field = parseUniqueConstraintField sqlEx.Message
            FractalError.UniqueConstraint (field, box "<value>")
        | :? SqliteException as sqlEx when sqlEx.SqliteErrorCode = 5 ->
            // SQLITE_BUSY - database is locked
            FractalError.Connection $"Database is locked: {sqlEx.Message}"
        | _ ->
            FractalError.Query ($"{e.Message} {step}".Trim(), Some sql)

    | :? DbReaderException as e ->
        let field = e.FieldName |> Option.defaultValue "<unknown field>"
        FractalError.Serialization $"Failed to read field '{field}': {e.Message}"

    | :? DbTransactionException as e ->
        let step = e.Step |> string
        FractalError.Transaction $"{step}: {e.Message}"

    | _ ->
        FractalError.Query (ex.Message, None)

/// Wrapper for database operations with proper exception handling
let tryDbOperation<'T> (operation: unit -> 'T) : FractalResult<'T> =
    try
        Ok (operation())
    with
    | :? DbConnectionException
    | :? DbExecutionException
    | :? DbReaderException
    | :? DbTransactionException as ex ->
        Error (mapDonaldException ex)
    | ex ->
        Error (FractalError.Query (ex.Message, None))

/// Async version
let tryDbOperationAsync<'T> (operation: unit -> Task<'T>) : Task<FractalResult<'T>> =
    task {
        try
            let! result = operation()
            return Ok result
        with
        | :? DbConnectionException
        | :? DbExecutionException
        | :? DbReaderException
        | :? DbTransactionException as ex ->
            return Error (mapDonaldException ex)
        | ex ->
            return Error (FractalError.Query (ex.Message, None))
    }
```

**Benefits:**
1. Rich error context from Donald metadata (SQL statement, field name, transaction step)
2. Proper SQLite error code handling (locked, constraint violations, etc.)
3. Consistent error wrapping across all database operations

### 5.2 CommandBehavior Configuration

**Current state:** No configuration for CommandBehavior. Donald defaults to `SequentialAccess`.

**Problem:** Some use cases may need different behavior:
- `SequentialAccess` (default) - Best performance, forward-only reading
- `Default` - Random access, more memory but flexible column access order

**Implementation:**

```fsharp
// Add to DbOptions
type DbOptions = {
    IdGenerator: unit -> string
    EnableCache: bool
    CacheMaxEntries: int option
    CommandBehavior: CommandBehavior  // NEW
}

module DbOptions =
    let defaults = {
        IdGenerator = IdGenerator.generate
        EnableCache = false
        CacheMaxEntries = None
        CommandBehavior = CommandBehavior.SequentialAccess  // Best performance
    }

    let withCommandBehavior behavior opts = { opts with CommandBehavior = behavior }

// Usage in queries
conn
|> Db.newCommand sql
|> Db.setCommandBehavior collection.Options.CommandBehavior
|> Db.query deserialize
```

**When to use `CommandBehavior.Default`:**
- Column order in SELECT doesn't match read order (rare in generated SQL)
- Need to read columns multiple times
- Working with non-standard data readers

### 5.3 Better Error Messages

Improve error context throughout the codebase:

```fsharp
// Instead of generic errors
Error (FractalError.Query "Invalid query")

// Provide context
Error (FractalError.Query ($"Invalid query on field '{field}': {reason}", Some sql))
```

### 5.2 Query Validation

Add compile-time and runtime query validation:

```fsharp
// In Query.fs - add validation
let validateField (fieldName: string) (schema: SchemaDef<'T>) : Result<unit, string> =
    let fieldNames = schema.Fields |> List.map (fun f -> f.Name)
    if List.contains fieldName fieldNames || fieldName = "_id" then
        Ok ()
    else
        Error $"Field '{fieldName}' not found in schema. Valid fields: {String.concat ", " fieldNames}"
```

### 5.3 Logging/Diagnostics

Add optional query logging for debugging:

```fsharp
type DbOptions = {
    // ... existing fields ...
    QueryLogger: (string -> string list -> unit) option  // SQL, params
}

// Usage
let loggedOptions = {
    DbOptions.defaults with
        QueryLogger = Some (fun sql params ->
            printfn "SQL: %s" sql
            printfn "Params: %A" params)
}
```

### 5.4 Connection Resilience

Add automatic retry for transient failures:

```fsharp
// In Collection.fs
let private executeWithRetry (maxRetries: int) (delay: int) (operation: unit -> Task<'T>) =
    let rec loop attempt =
        task {
            try
                return! operation()
            with
            | :? SqliteException as ex when isTransient ex && attempt < maxRetries ->
                do! Task.Delay(delay * attempt)
                return! loop (attempt + 1)
            | ex ->
                return raise ex
        }
    loop 1
```

### 5.5 Bulk Operation Optimization

Optimize batch operations for large datasets:

```fsharp
// In Collection.fs - chunked batch insert
let insertManyChunked<'T> (chunkSize: int) (documents: list<'T>) (collection: Collection<'T>) =
    task {
        let chunks = documents |> List.chunkBySize chunkSize
        let mutable totalInserted = 0
        let mutable allDocs = []

        for chunk in chunks do
            let! result = insertMany chunk collection
            match result with
            | Ok r ->
                totalInserted <- totalInserted + r.InsertedCount
                allDocs <- allDocs @ r.Documents
            | Error e ->
                return Error e

        return Ok { Documents = allDocs; InsertedCount = totalInserted }
    }
```

---

## 6. Implementation Priority

### Phase 1: Query Expressions & Core Tests
1. Implement QueryExpr.fs with quotation-based builder
2. QueryExprTests.fs - comprehensive translation tests
3. TypesTests.fs - core type coverage
4. IndexTests.fs - index management tests

### Phase 2: API Completeness & Donald Integration
1. FromConnection factory method with ownsConnection flag
2. Expose Connection property for Donald interop
3. Proper Donald exception handling (all 4 exception types)
4. CommandBehavior configuration option
5. CollectionOptions with per-collection cache
6. distinct, estimatedCount, validate, search methods
7. TimestampConfig for customizable timestamps

### Phase 3: Missing Test Coverage
1. NestedFieldsTests.fs - nested objects, path queries, projections
2. ArrayOperatorTests.fs - array query operators
3. ProjectionTests.fs - select/omit execution
4. CursorPaginationTests.fs - keyset pagination
5. TextSearchTests.fs - full-text search
6. MultiCollectionTests.fs - cross-collection transactions

### Phase 4: Edge Cases & Robustness
1. EdgeCaseTests.fs - boundary conditions
2. ErrorTests.fs - error message quality
3. ConcurrencyTests.fs - parallel access
4. DisposableTests.fs - resource cleanup
5. CacheTests.fs - query caching behavior
6. PerformanceTests.fs - large dataset tests

### Phase 5: Query Expression Parallel Period
1. Run both query APIs side-by-side
2. Verify query expressions produce identical results
3. Document any behavioral differences
4. Gather feedback on edge cases

### Phase 6: Deprecation (Future)
1. Mark existing query APIs as `[<Obsolete>]`
2. Update documentation
3. Keep deprecated APIs for one major version
4. Remove in future major version

---

## 7. Files to Create/Modify

### New Source Files
- `src/QueryExpr.fs` - Quotation-based query expression builder
- `src/Cache.fs` - Query caching implementation
- `src/Validation.fs` - Validator wrapper utilities (FluentValidation, etc.)

### New Test Files
- `tests/TypesTests.fs` - Core types (IdGenerator, Timestamp, Document)
- `tests/NestedFieldsTests.fs` - Nested object handling, path queries, projections
- `tests/ProjectionTests.fs` - Select/Omit execution tests
- `tests/CursorPaginationTests.fs` - Keyset pagination tests
- `tests/TextSearchTests.fs` - Full-text search tests
- `tests/ErrorTests.fs` - Error message quality tests
- `tests/EdgeCaseTests.fs` - Boundary conditions and special values
- `tests/ConcurrencyTests.fs` - Parallel access and race conditions
- `tests/DisposableTests.fs` - Resource cleanup and lifecycle
- `tests/CacheTests.fs` - Query caching behavior
- `tests/IndexTests.fs` - Index creation and usage
- `tests/ArrayOperatorTests.fs` - Array query operators
- `tests/MultiCollectionTests.fs` - Cross-collection transactions
- `tests/PerformanceTests.fs` - Large dataset and timing tests
- `tests/QueryExprTests.fs` - Query expression translation tests

### Modified Files
- `src/Builders.fs` - Add new QueryBuilder custom operations
- `src/Database.fs` - Add FromConnection, ownsConnection flag, CollectionOptions
- `src/Collection.fs` - Add distinct, estimatedCount, validate, drop, search
- `src/Schema.fs` - Add TimestampConfig type
- `src/Options.fs` - Add CacheMaxEntries to DbOptions
- `src/Errors.fs` - Improve error context
- `tests/FractalDb.Tests.fsproj` - Add new test files

---

## 8. Acceptance Criteria for Completion

### Query Expressions (Phase 1-4)
- [ ] Query expressions work with `for x in collection do` syntax
- [ ] All operators translate correctly: where, select, sortBy, take, skip, count, exists, head
- [ ] String predicates work: Contains, StartsWith, EndsWith
- [ ] Logical operators work: &&, ||, not
- [ ] Nested property access works in predicates
- [ ] All 40+ query expression tests pass

### API Features
- [ ] FromConnection accepts pre-configured IDbConnection (Donald-compatible)
- [ ] FractalDb only disposes connections it owns (ownsConnection flag)
- [ ] Connection property exposed for advanced Donald usage
- [ ] All 4 Donald exception types handled with rich error context
- [ ] CommandBehavior configurable (SequentialAccess vs Default)
- [ ] CollectionOptions allows per-collection cache override
- [ ] TimestampConfig allows customizing timestamp field names
- [ ] Query caching implementation with configurable max entries
- [ ] `distinct()`, `estimatedCount()`, `validate()`, `search()` methods implemented

### Test Coverage
- [ ] All 15 new test files created with passing tests
- [ ] Total test count exceeds 250
- [ ] Core types tests (IdGenerator, Timestamp, Document) - 15+ tests
- [ ] Index management tests - 7+ tests
- [ ] Array operator tests - 9+ tests
- [ ] Nested fields tests (paths, projections) - 12+ tests
- [ ] Multi-collection transaction tests - 5+ tests

### Build & Quality
- [ ] `task build` passes with 0 errors
- [ ] `task lint` passes with acceptable warnings only
- [ ] `task test` passes with 100% success rate
- [ ] No regressions in existing functionality
