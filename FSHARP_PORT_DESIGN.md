# FractalDb - Comprehensive F# Port Design Document

**Version:** 2.0
**Status:** Design Complete
**Target Runtime:** .NET 9+ (for `Guid.CreateVersion7`)
**Original:** [StrataDB](https://github.com/user/stratadb) (TypeScript)

---

## Executive Summary

**FractalDb** is a type-safe, embedded document database for .NET 9+ that provides a MongoDB-like API backed by SQLite. It is a complete F# port of the TypeScript StrataDB library, embracing idiomatic F# patterns throughout.

### Design Principles

| Principle | Implementation |
|-----------|----------------|
| **F# Idiomatic** | Computation expressions, pipelines, pattern matching, immutability |
| **Result-Based Errors** | `Result<'T, FractalError>` everywhere - no exceptions |
| **Task-Based Async** | `Task<'T>` via `task { }` CE for .NET ecosystem alignment |
| **Full API Parity** | Every StrataDB feature has an F# equivalent |
| **Zero CLIMutable** | Pure immutable records with FSharp.SystemTextJson |
| **Type-Safe Queries** | Discriminated unions prevent invalid queries at compile time |

### Technology Stack

| Component | Choice | Rationale |
|-----------|--------|-----------|
| **SQLite Library** | [Donald](https://github.com/pimbrouwers/Donald) | Thin, F#-idiomatic, Result types, close to ADO.NET |
| **JSON Serialization** | [FSharp.SystemTextJson](https://github.com/Tarmil/FSharp.SystemTextJson) | Native AOT, handles F# records/DUs/options natively |
| **ID Generation** | `Guid.CreateVersion7().ToString()` | Built-in .NET 9, time-sortable, lexicographically orderable |
| **Async Model** | `Task<'T>` via `task { }` CE | .NET ecosystem alignment, interop with C# |
| **Testing** | xUnit + [FsUnit.Light](https://github.com/Lanayx/FsUnit.Light) | Zero-cost assertions, F#-friendly syntax |
| **Benchmarking** | BenchmarkDotNet | Industry standard for .NET performance testing |

---

## Table of Contents

1. [Architecture Overview](#1-architecture-overview)
2. [Core Type System](#2-core-type-system)
3. [Query System](#3-query-system)
4. [Computation Expressions](#4-computation-expressions)
5. [Collection API](#5-collection-api)
6. [Schema System](#6-schema-system)
7. [Storage Layer](#7-storage-layer)
8. [SQL Translation Engine](#8-sql-translation-engine)
9. [Error Handling](#9-error-handling)
10. [Testing Strategy](#10-testing-strategy)
11. [Performance Benchmarks](#11-performance-benchmarks)
12. [Implementation Roadmap](#12-implementation-roadmap)
13. [Project Configuration](#13-project-configuration)
14. [Complete Usage Examples](#14-complete-usage-examples)

---

## 1. Architecture Overview

### Project Structure

```
FractalDb/
├── src/
│   └── FractalDb/
│       ├── FractalDb.fsproj
│       ├── Core/
│       │   ├── Types.fs              # Document, ID generation, base types
│       │   ├── Query.fs              # Query discriminated unions
│       │   ├── Operators.fs          # Comparison, String, Array operators
│       │   ├── Schema.fs             # Schema definition types
│       │   ├── Options.fs            # QueryOptions, SortSpec, CursorSpec
│       │   └── Errors.fs             # FractalError discriminated union
│       ├── Storage/
│       │   ├── Database.fs           # FractalDb main class
│       │   ├── Collection.fs         # Collection<'T> implementation
│       │   ├── Transaction.fs        # Transaction handling
│       │   ├── SqlTranslator.fs      # Query → SQL translation
│       │   └── TableBuilder.fs       # DDL generation
│       ├── Builders/
│       │   ├── QueryBuilder.fs       # Query CE
│       │   ├── SchemaBuilder.fs      # Schema CE
│       │   ├── OptionsBuilder.fs     # QueryOptions CE
│       │   └── TransactionBuilder.fs # Transaction CE
│       ├── Json/
│       │   └── Serialization.fs      # FSharp.SystemTextJson configuration
│       └── Library.fs                # Public API exports
├── tests/
│   └── FractalDb.Tests/
│       ├── FractalDb.Tests.fsproj
│       ├── Assertions.fs             # Custom FsUnit.Light assertions
│       ├── TestHelpers.fs            # Shared test utilities
│       ├── Core/
│       │   ├── QueryTests.fs         # Query DU construction
│       │   ├── SchemaTests.fs        # Schema builder tests
│       │   └── ErrorTests.fs         # Error type tests
│       ├── Unit/
│       │   ├── SqlTranslatorTests.fs # Query → SQL translation
│       │   ├── IdGeneratorTests.fs   # UUID v7 generation
│       │   └── JsonTests.fs          # Serialization tests
│       ├── Integration/
│       │   ├── CrudTests.fs          # Full CRUD operations
│       │   ├── QueryTests.fs         # Complex query execution
│       │   ├── TransactionTests.fs   # Transaction commit/rollback
│       │   ├── ValidationTests.fs    # Schema validation
│       │   └── BatchTests.fs         # Batch operations
│       └── Benchmarks/
│           ├── CrudBenchmarks.fs     # CRUD performance
│           └── QueryBenchmarks.fs    # Query translation performance
├── samples/
│   └── GettingStarted/
│       └── Program.fs
├── FractalDb.sln
└── README.md
```

### Module Dependency Graph

```
Library.fs (public exports)
    ↑
Builders/
├── QueryBuilder.fs ──────┐
├── SchemaBuilder.fs ─────┼──→ Core/
├── OptionsBuilder.fs ────┤
└── TransactionBuilder.fs ┘
    ↑
Storage/
├── Database.fs ──────────┐
├── Collection.fs ────────┼──→ SqlTranslator.fs → Core/
├── Transaction.fs ───────┤
└── TableBuilder.fs ──────┘
    ↑
Json/Serialization.fs
    ↑
Core/
├── Types.fs
├── Query.fs ←── Operators.fs
├── Schema.fs
├── Options.fs
└── Errors.fs
```

---

## 2. Core Type System

### 2.1 Document Types

```fsharp
namespace FractalDb.Core

open System

/// Metadata added to all documents
type DocumentMeta = {
    Id: string
    CreatedAt: int64
    UpdatedAt: int64
}

/// A document wraps user data with metadata
type Document<'T> = {
    Id: string
    Data: 'T
    CreatedAt: int64
    UpdatedAt: int64
}

module Document =
    /// Create a new document with generated ID and timestamps
    let create (data: 'T) : Document<'T> =
        let now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        {
            Id = IdGenerator.generate()
            Data = data
            CreatedAt = now
            UpdatedAt = now
        }

    /// Create with explicit ID
    let createWithId (id: string) (data: 'T) : Document<'T> =
        let now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        {
            Id = id
            Data = data
            CreatedAt = now
            UpdatedAt = now
        }

    /// Update document data (preserves ID and createdAt, updates updatedAt)
    let update (f: 'T -> 'T) (doc: Document<'T>) : Document<'T> =
        { doc with
            Data = f doc.Data
            UpdatedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }

    /// Map over document data
    let map (f: 'T -> 'U) (doc: Document<'T>) : Document<'U> =
        {
            Id = doc.Id
            Data = f doc.Data
            CreatedAt = doc.CreatedAt
            UpdatedAt = doc.UpdatedAt
        }
```

### 2.2 ID Generation

```fsharp
module IdGenerator =
    /// Generate time-sortable UUID v7 (.NET 9+)
    /// - First 48 bits encode Unix timestamp in milliseconds
    /// - Remaining bits are random
    /// - Lexicographically sortable
    /// - 2^-74 collision probability
    let generate () : string =
        Guid.CreateVersion7().ToString()

    /// Check if ID should be auto-generated
    let isEmptyOrDefault (id: string) : bool =
        String.IsNullOrEmpty id || id = Guid.Empty.ToString()

    /// Validate ID format (UUID)
    let isValid (id: string) : bool =
        match Guid.TryParse(id) with
        | true, _ -> true
        | false, _ -> false
```

### 2.3 Timestamp Utilities

```fsharp
module Timestamp =
    /// Get current Unix timestamp in milliseconds
    let now () : int64 =
        DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()

    /// Convert timestamp to DateTimeOffset
    let toDateTimeOffset (timestamp: int64) : DateTimeOffset =
        DateTimeOffset.FromUnixTimeMilliseconds(timestamp)

    /// Convert DateTimeOffset to timestamp
    let fromDateTimeOffset (dto: DateTimeOffset) : int64 =
        dto.ToUnixTimeMilliseconds()

    /// Check if timestamp is in range
    let isInRange (start: int64) (end': int64) (timestamp: int64) : bool =
        timestamp >= start && timestamp <= end'
```

---

## 3. Query System

### 3.1 Comparison Operators

```fsharp
namespace FractalDb.Core

/// Comparison operators - type-safe per value type
[<RequireQualifiedAccess>]
type CompareOp<'T> =
    | Eq of 'T           // Equal to
    | Ne of 'T           // Not equal to
    | Gt of 'T           // Greater than (numbers/dates only)
    | Gte of 'T          // Greater than or equal
    | Lt of 'T           // Less than
    | Lte of 'T          // Less than or equal
    | In of 'T list      // Value in list
    | NotIn of 'T list   // Value not in list
```

### 3.2 String Operators

```fsharp
/// String-specific operators (only valid for string fields)
[<RequireQualifiedAccess>]
type StringOp =
    | Like of pattern: string        // SQL LIKE pattern (case-sensitive)
    | ILike of pattern: string       // SQL LIKE pattern (case-insensitive)
    | Contains of substring: string  // Sugar for LIKE '%substring%'
    | StartsWith of prefix: string   // Sugar for LIKE 'prefix%'
    | EndsWith of suffix: string     // Sugar for LIKE '%suffix'
```

### 3.3 Array Operators

```fsharp
/// Array operators (only valid for array/list fields)
[<RequireQualifiedAccess>]
type ArrayOp<'T> =
    | All of 'T list                          // Array contains all values
    | Size of int                             // Array has exact length
    | ElemMatch of Query<'T>                  // At least one element matches
    | Index of index: int * Query<'T>         // Element at index matches
```

### 3.4 Existence Operator

```fsharp
/// Existence check
[<RequireQualifiedAccess>]
type ExistsOp =
    | Exists of bool  // Field exists (true) or doesn't exist (false)
```

### 3.5 Field Operations (Type-Erased for Storage)

```fsharp
/// A single field operation (boxed for heterogeneous storage)
[<RequireQualifiedAccess>]
type FieldOp =
    | Compare of obj        // CompareOp<'T> boxed
    | String of StringOp    // String operators
    | Array of obj          // ArrayOp<'T> boxed
    | Exist of ExistsOp     // Existence check
```

### 3.6 Complete Query Structure

```fsharp
/// Complete query structure with logical operators
[<RequireQualifiedAccess>]
type Query<'T> =
    | Empty                              // Match all documents (1=1)
    | Field of fieldName: string * FieldOp
    | And of Query<'T> list              // All must match
    | Or of Query<'T> list               // At least one must match
    | Nor of Query<'T> list              // None must match
    | Not of Query<'T>                   // Negation
```

### 3.7 Query Helper Module

```fsharp
[<RequireQualifiedAccess>]
module Query =
    // ===== Constructors =====

    /// Match all documents
    let empty<'T> : Query<'T> = Query.Empty

    // ===== Comparison Operators =====

    let inline eq value =
        Query.Field("", FieldOp.Compare(box (CompareOp.Eq value)))

    let inline ne value =
        Query.Field("", FieldOp.Compare(box (CompareOp.Ne value)))

    let inline gt value =
        Query.Field("", FieldOp.Compare(box (CompareOp.Gt value)))

    let inline gte value =
        Query.Field("", FieldOp.Compare(box (CompareOp.Gte value)))

    let inline lt value =
        Query.Field("", FieldOp.Compare(box (CompareOp.Lt value)))

    let inline lte value =
        Query.Field("", FieldOp.Compare(box (CompareOp.Lte value)))

    let inline isIn values =
        Query.Field("", FieldOp.Compare(box (CompareOp.In values)))

    let inline notIn values =
        Query.Field("", FieldOp.Compare(box (CompareOp.NotIn values)))

    // ===== String Operators =====

    let like pattern =
        Query.Field("", FieldOp.String(StringOp.Like pattern))

    let ilike pattern =
        Query.Field("", FieldOp.String(StringOp.ILike pattern))

    let contains substring =
        Query.Field("", FieldOp.String(StringOp.Contains substring))

    let startsWith prefix =
        Query.Field("", FieldOp.String(StringOp.StartsWith prefix))

    let endsWith suffix =
        Query.Field("", FieldOp.String(StringOp.EndsWith suffix))

    // ===== Array Operators =====

    let all<'T> values =
        Query.Field("", FieldOp.Array(box (ArrayOp.All<'T> values)))

    let size n =
        Query.Field("", FieldOp.Array(box (ArrayOp.Size n)))

    let elemMatch<'T> (subQuery: Query<'T>) =
        Query.Field("", FieldOp.Array(box (ArrayOp.ElemMatch<'T> subQuery)))

    let index<'T> idx (subQuery: Query<'T>) =
        Query.Field("", FieldOp.Array(box (ArrayOp.Index<'T>(idx, subQuery))))

    // ===== Existence =====

    let exists = Query.Field("", FieldOp.Exist(ExistsOp.Exists true))
    let notExists = Query.Field("", FieldOp.Exist(ExistsOp.Exists false))

    // ===== Field Binding =====

    /// Attach field name to a query operator
    let field (name: string) (op: Query<'T>) : Query<'T> =
        match op with
        | Query.Field(_, innerOp) -> Query.Field(name, innerOp)
        | other -> other

    // ===== Logical Combinators =====

    /// All conditions must match (AND)
    let all' (queries: Query<'T> list) : Query<'T> =
        Query.And queries

    /// At least one condition must match (OR)
    let any (queries: Query<'T> list) : Query<'T> =
        Query.Or queries

    /// None of the conditions must match (NOR)
    let none (queries: Query<'T> list) : Query<'T> =
        Query.Nor queries

    /// Negate a condition (NOT)
    let not' (query: Query<'T>) : Query<'T> =
        Query.Not query
```

### 3.8 Operator Matrix

| Operator | String | Number | Date | Bool | Array | Description |
|----------|--------|--------|------|------|-------|-------------|
| `eq` | ✅ | ✅ | ✅ | ✅ | ✅ | Equality |
| `ne` | ✅ | ✅ | ✅ | ✅ | ✅ | Not equal |
| `gt` | ❌ | ✅ | ✅ | ❌ | ❌ | Greater than |
| `gte` | ❌ | ✅ | ✅ | ❌ | ❌ | Greater or equal |
| `lt` | ❌ | ✅ | ✅ | ❌ | ❌ | Less than |
| `lte` | ❌ | ✅ | ✅ | ❌ | ❌ | Less or equal |
| `isIn` | ✅ | ✅ | ✅ | ✅ | ✅ | In list |
| `notIn` | ✅ | ✅ | ✅ | ✅ | ✅ | Not in list |
| `like` | ✅ | ❌ | ❌ | ❌ | ❌ | SQL LIKE |
| `ilike` | ✅ | ❌ | ❌ | ❌ | ❌ | Case-insensitive LIKE |
| `contains` | ✅ | ❌ | ❌ | ❌ | ❌ | Substring match |
| `startsWith` | ✅ | ❌ | ❌ | ❌ | ❌ | Prefix match |
| `endsWith` | ✅ | ❌ | ❌ | ❌ | ❌ | Suffix match |
| `all` | ❌ | ❌ | ❌ | ❌ | ✅ | Contains all values |
| `size` | ❌ | ❌ | ❌ | ❌ | ✅ | Array length |
| `elemMatch` | ❌ | ❌ | ❌ | ❌ | ✅ | Element matches query |
| `index` | ❌ | ❌ | ❌ | ❌ | ✅ | Element at index |
| `exists` | ✅ | ✅ | ✅ | ✅ | ✅ | Field presence |

---

## 4. Computation Expressions

### 4.1 Query Builder CE

```fsharp
namespace FractalDb.Builders

open FractalDb.Core

type QueryBuilder() =
    member _.Yield(_) = Query.Empty
    member _.Zero() = Query.Empty
    member _.Delay(f) = f()

    /// Add a field condition (implicitly AND)
    [<CustomOperation("where")>]
    member _.Where(state: Query<'T>, name: string, op: Query<'T>) : Query<'T> =
        let fieldOp = Query.field name op
        match state with
        | Query.Empty -> fieldOp
        | Query.And queries -> Query.And (fieldOp :: queries)
        | other -> Query.And [fieldOp; other]

    /// Alias for 'where'
    [<CustomOperation("field")>]
    member this.Field(state, name, op) = this.Where(state, name, op)

    /// Add additional AND condition
    [<CustomOperation("andAlso")>]
    member _.AndAlso(state: Query<'T>, query: Query<'T>) : Query<'T> =
        match state with
        | Query.Empty -> query
        | Query.And queries -> Query.And (query :: queries)
        | other -> Query.And [query; other]

    /// Add OR branch
    [<CustomOperation("orElse")>]
    member _.OrElse(state: Query<'T>, queries: Query<'T> list) : Query<'T> =
        match state with
        | Query.Empty -> Query.Or queries
        | other -> Query.And [other; Query.Or queries]

    /// Add NOR branch
    [<CustomOperation("norElse")>]
    member _.NorElse(state: Query<'T>, queries: Query<'T> list) : Query<'T> =
        match state with
        | Query.Empty -> Query.Nor queries
        | other -> Query.And [other; Query.Nor queries]

    /// Negate a condition
    [<CustomOperation("not'")>]
    member _.Not(state: Query<'T>, query: Query<'T>) : Query<'T> =
        let negated = Query.Not query
        match state with
        | Query.Empty -> negated
        | Query.And queries -> Query.And (negated :: queries)
        | other -> Query.And [negated; other]

    member _.Combine(a: Query<'T>, b: Query<'T>) : Query<'T> =
        match a, b with
        | Query.Empty, x | x, Query.Empty -> x
        | Query.And xs, Query.And ys -> Query.And (xs @ ys)
        | Query.And xs, y -> Query.And (y :: xs)
        | x, Query.And ys -> Query.And (x :: ys)
        | x, y -> Query.And [x; y]

[<AutoOpen>]
module QueryBuilderInstance =
    /// Global query builder instance
    let query = QueryBuilder()
```

**Usage Examples:**

```fsharp
// Simple query
let filter = query {
    field "age" (Query.gte 18)
    field "status" (Query.eq "active")
}

// With string operators
let emailFilter = query {
    field "email" (Query.endsWith "@company.com")
    field "name" (Query.contains "Smith")
}

// Complex logical query
let complexFilter = query {
    field "active" (Query.eq true)
    field "age" (Query.gte 18)
    orElse [
        Query.field "role" (Query.eq "admin")
        Query.field "role" (Query.eq "moderator")
    ]
    not' (Query.field "banned" (Query.eq true))
}

// Array queries
let tagFilter = query {
    field "tags" (Query.all ["developer"; "senior"])
    field "skills" (Query.size 5)
}

// Function composition alternative
let filter =
    Query.all' [
        Query.field "age" (Query.gte 18)
        Query.field "status" (Query.eq "active")
        Query.any [
            Query.field "role" (Query.eq "admin")
            Query.field "role" (Query.eq "moderator")
        ]
    ]
```

### 4.2 Schema Builder CE

```fsharp
namespace FractalDb.Builders

open FractalDb.Core

[<RequireQualifiedAccess>]
type SqliteType =
    | Text
    | Integer
    | Real
    | Blob
    | Numeric
    | Boolean  // Stored as INTEGER (0/1)

type FieldDef = {
    Name: string
    Path: string option      // JSON path, defaults to $.{Name}
    SqlType: SqliteType
    Indexed: bool
    Unique: bool
    Nullable: bool
}

type IndexDef = {
    Name: string
    Fields: string list
    Unique: bool
}

type SchemaDef<'T> = {
    Fields: FieldDef list
    Indexes: IndexDef list
    Timestamps: bool
    Validate: ('T -> Result<'T, string>) option
}

type SchemaBuilder<'T>() =
    member _.Yield(_) = {
        Fields = []
        Indexes = []
        Timestamps = false
        Validate = None
    }

    /// Define a field (not indexed by default)
    [<CustomOperation("field")>]
    member _.Field(state: SchemaDef<'T>, name: string, sqlType: SqliteType,
                   ?indexed: bool, ?unique: bool, ?nullable: bool, ?path: string) =
        let field = {
            Name = name
            Path = path
            SqlType = sqlType
            Indexed = defaultArg indexed false
            Unique = defaultArg unique false
            Nullable = defaultArg nullable true
        }
        { state with Fields = field :: state.Fields }

    /// Define an indexed field (shorthand)
    [<CustomOperation("indexed")>]
    member _.Indexed(state: SchemaDef<'T>, name: string, sqlType: SqliteType,
                     ?unique: bool, ?nullable: bool, ?path: string) =
        let field = {
            Name = name
            Path = path
            SqlType = sqlType
            Indexed = true
            Unique = defaultArg unique false
            Nullable = defaultArg nullable true
        }
        { state with Fields = field :: state.Fields }

    /// Define a unique indexed field (shorthand)
    [<CustomOperation("unique")>]
    member _.Unique(state: SchemaDef<'T>, name: string, sqlType: SqliteType,
                    ?nullable: bool, ?path: string) =
        let field = {
            Name = name
            Path = path
            SqlType = sqlType
            Indexed = true
            Unique = true
            Nullable = defaultArg nullable true
        }
        { state with Fields = field :: state.Fields }

    /// Enable automatic timestamp management
    [<CustomOperation("timestamps")>]
    member _.Timestamps(state: SchemaDef<'T>) =
        { state with Timestamps = true }

    /// Define a compound index
    [<CustomOperation("compoundIndex")>]
    member _.CompoundIndex(state: SchemaDef<'T>, name: string,
                           fields: string list, ?unique: bool) =
        let index = {
            Name = name
            Fields = fields
            Unique = defaultArg unique false
        }
        { state with Indexes = index :: state.Indexes }

    /// Add validation function
    [<CustomOperation("validate")>]
    member _.Validate(state: SchemaDef<'T>, validator: 'T -> Result<'T, string>) =
        { state with Validate = Some validator }

[<AutoOpen>]
module SchemaBuilderInstance =
    /// Create a schema builder for type 'T
    let schema<'T> = SchemaBuilder<'T>()
```

**Usage Examples:**

```fsharp
type User = {
    Name: string
    Email: string
    Age: int
    Active: bool
    Tags: string list
}

// Basic schema
let userSchema = schema<User> {
    indexed "name" SqliteType.Text
    unique "email" SqliteType.Text
    indexed "age" SqliteType.Integer
    field "active" SqliteType.Integer
    field "tags" SqliteType.Text
    timestamps
}

// With compound index
let userSchemaWithIndex = schema<User> {
    indexed "name" SqliteType.Text
    unique "email" SqliteType.Text
    indexed "age" SqliteType.Integer
    timestamps
    compoundIndex "age_active" ["age"; "active"]
}

// With validation
let validatedUserSchema = schema<User> {
    indexed "name" SqliteType.Text
    unique "email" SqliteType.Text
    indexed "age" SqliteType.Integer
    timestamps
    validate (fun user ->
        if String.IsNullOrEmpty user.Email then
            Error "Email is required"
        elif not (user.Email.Contains "@") then
            Error "Invalid email format"
        elif user.Age < 0 then
            Error "Age must be non-negative"
        elif user.Age > 150 then
            Error "Age must be realistic"
        else
            Ok user
    )
}

// Nested field paths (dot notation)
let nestedSchema = schema<UserWithProfile> {
    indexed "profile.bio" SqliteType.Text (path = "$.profile.bio")
    indexed "settings.theme" SqliteType.Text (path = "$.settings.theme")
}
```

### 4.3 Options Builder CE

```fsharp
namespace FractalDb.Builders

open FractalDb.Core

[<RequireQualifiedAccess>]
type SortDirection =
    | Ascending
    | Descending

type CursorSpec = {
    After: string option
    Before: string option
}

type TextSearchSpec = {
    Text: string
    Fields: string list
    CaseSensitive: bool
}

type QueryOptions<'T> = {
    Sort: (string * SortDirection) list
    Limit: int option
    Skip: int option
    Select: string list option    // Include only these fields
    Omit: string list option      // Exclude these fields
    Search: TextSearchSpec option // Full-text search
    Cursor: CursorSpec option     // Cursor-based pagination
}

module QueryOptions =
    let empty<'T> : QueryOptions<'T> = {
        Sort = []
        Limit = None
        Skip = None
        Select = None
        Omit = None
        Search = None
        Cursor = None
    }

    let limit n opts = { opts with Limit = Some n }
    let skip n opts = { opts with Skip = Some n }
    let sortBy field dir opts = { opts with Sort = (field, dir) :: opts.Sort }
    let sortAsc field opts = sortBy field SortDirection.Ascending opts
    let sortDesc field opts = sortBy field SortDirection.Descending opts
    let select fields opts = { opts with Select = Some fields }
    let omit fields opts = { opts with Omit = Some fields }

    let search text fields opts =
        { opts with Search = Some { Text = text; Fields = fields; CaseSensitive = false } }

    let searchCaseSensitive text fields opts =
        { opts with Search = Some { Text = text; Fields = fields; CaseSensitive = true } }

    let cursorAfter id opts =
        { opts with Cursor = Some { After = Some id; Before = None } }

    let cursorBefore id opts =
        { opts with Cursor = Some { After = None; Before = Some id } }

type OptionsBuilder<'T>() =
    member _.Yield(_) = QueryOptions.empty<'T>
    member _.Zero() = QueryOptions.empty<'T>

    [<CustomOperation("sortBy")>]
    member _.SortBy(state, field, dir) = QueryOptions.sortBy field dir state

    [<CustomOperation("sortAsc")>]
    member _.SortAsc(state, field) = QueryOptions.sortAsc field state

    [<CustomOperation("sortDesc")>]
    member _.SortDesc(state, field) = QueryOptions.sortDesc field state

    [<CustomOperation("limit")>]
    member _.Limit(state, n) = QueryOptions.limit n state

    [<CustomOperation("skip")>]
    member _.Skip(state, n) = QueryOptions.skip n state

    [<CustomOperation("select")>]
    member _.Select(state, fields) = QueryOptions.select fields state

    [<CustomOperation("omit")>]
    member _.Omit(state, fields) = QueryOptions.omit fields state

    [<CustomOperation("search")>]
    member _.Search(state, text, fields) = QueryOptions.search text fields state

    [<CustomOperation("cursorAfter")>]
    member _.CursorAfter(state, id) = QueryOptions.cursorAfter id state

    [<CustomOperation("cursorBefore")>]
    member _.CursorBefore(state, id) = QueryOptions.cursorBefore id state

[<AutoOpen>]
module OptionsBuilderInstance =
    let options<'T> = OptionsBuilder<'T>()
```

**Usage Examples:**

```fsharp
// CE style
let opts = options<User> {
    sortDesc "createdAt"
    sortAsc "name"
    limit 20
    skip 40
    select ["name"; "email"; "age"]
}

// Pipeline style
let opts =
    QueryOptions.empty<User>
    |> QueryOptions.sortDesc "createdAt"
    |> QueryOptions.limit 20
    |> QueryOptions.select ["name"; "email"]

// With text search
let searchOpts = options<User> {
    search "john" ["name"; "email"; "bio"]
    sortDesc "createdAt"
    limit 10
}

// Cursor pagination
let paginatedOpts = options<User> {
    sortDesc "createdAt"
    limit 20
    cursorAfter "last-seen-id"
}
```

### 4.4 Transaction Builder CE

```fsharp
namespace FractalDb.Builders

open System.Threading.Tasks
open FractalDb.Core

/// Result-aware task builder for transactions
type TransactionBuilder(db: FractalDb) =

    member _.Bind(task: Task<FractalResult<'T>>,
                  f: 'T -> Task<FractalResult<'U>>) : Task<FractalResult<'U>> =
        task {
            match! task with
            | Ok value -> return! f value
            | Error e -> return Error e
        }

    member _.Bind(result: FractalResult<'T>,
                  f: 'T -> Task<FractalResult<'U>>) : Task<FractalResult<'U>> =
        task {
            match result with
            | Ok value -> return! f value
            | Error e -> return Error e
        }

    member _.Return(value: 'T) : Task<FractalResult<'T>> =
        Task.FromResult(Ok value)

    member _.ReturnFrom(task: Task<FractalResult<'T>>) : Task<FractalResult<'T>> =
        task

    member _.Zero() : Task<FractalResult<unit>> =
        Task.FromResult(Ok ())

    member _.Delay(f: unit -> Task<FractalResult<'T>>) = f

    member _.Run(f: unit -> Task<FractalResult<'T>>) : Task<FractalResult<'T>> =
        db.ExecuteTransaction(fun _tx -> f())

    member _.TryWith(computation: unit -> Task<FractalResult<'T>>,
                     handler: exn -> Task<FractalResult<'T>>) =
        fun () -> task {
            try
                return! computation()
            with ex ->
                return! handler ex
        }

    member _.TryFinally(computation: unit -> Task<FractalResult<'T>>,
                        compensation: unit -> unit) =
        fun () -> task {
            try
                return! computation()
            finally
                compensation()
        }

/// Extension for FractalDb
type FractalDb with
    /// Create a transaction builder for Result-aware operations
    member this.Transact = TransactionBuilder(this)
```

**Usage Examples:**

```fsharp
// Transaction with automatic commit/rollback based on Result
let! result = db.Transact {
    let users = db.Collection<User>("users", userSchema)

    // Insert first user
    let! user1 = users |> Collection.insertOne {
        Name = "Alice"
        Email = "alice@example.com"
        Age = 30
        Active = true
        Tags = []
    }

    // Insert second user
    let! user2 = users |> Collection.insertOne {
        Name = "Bob"
        Email = "bob@example.com"
        Age = 25
        Active = true
        Tags = []
    }

    return (user1, user2)
}

match result with
| Ok (u1, u2) ->
    printfn $"Transaction succeeded: {u1.Id}, {u2.Id}"
| Error e ->
    printfn $"Transaction failed (rolled back): {e.Message}"

// Transfer funds example
let! transferResult = db.Transact {
    let accounts = db.Collection<Account>("accounts", accountSchema)

    // Debit source
    let! source =
        accounts
        |> Collection.findOneAndUpdate
            (query { field "id" (Query.eq sourceId) })
            (fun acc -> { acc with Balance = acc.Balance - amount })
            { ReturnDocument = After }

    // Credit destination
    let! dest =
        accounts
        |> Collection.findOneAndUpdate
            (query { field "id" (Query.eq destId) })
            (fun acc -> { acc with Balance = acc.Balance + amount })
            { ReturnDocument = After }

    return (source, dest)
}
```

---

## 5. Collection API

### 5.1 Collection Type

```fsharp
namespace FractalDb.Storage

open System.Threading.Tasks
open FractalDb.Core
open FractalDb.Builders

type Collection<'T> = internal {
    Name: string
    Schema: SchemaDef<'T>
    Connection: IDbConnection
    IdGenerator: unit -> string
    Translator: SqlTranslator<'T>
    EnableCache: bool
}
```

### 5.2 Collection Module (All 20+ Methods)

```fsharp
[<RequireQualifiedAccess>]
module Collection =

    // ═══════════════════════════════════════════════════════════════
    // READ OPERATIONS
    // ═══════════════════════════════════════════════════════════════

    /// Find document by ID
    val findById : id: string -> collection: Collection<'T> -> Task<Document<'T> option>

    /// Find first document matching filter
    val findOne : filter: Query<'T> -> collection: Collection<'T> -> Task<Document<'T> option>

    /// Find first document matching filter with options
    val findOneWith : filter: Query<'T> -> options: QueryOptions<'T>
                    -> collection: Collection<'T> -> Task<Document<'T> option>

    /// Find all documents matching filter
    val find : filter: Query<'T> -> collection: Collection<'T> -> Task<Document<'T> list>

    /// Find all documents matching filter with options
    val findWith : filter: Query<'T> -> options: QueryOptions<'T>
                 -> collection: Collection<'T> -> Task<Document<'T> list>

    /// Count documents matching filter
    val count : filter: Query<'T> -> collection: Collection<'T> -> Task<int>

    /// Estimated document count (fast, uses SQLite stats)
    val estimatedCount : collection: Collection<'T> -> Task<int>

    /// Search across multiple fields
    val search : text: string -> fields: string list
               -> collection: Collection<'T> -> Task<Document<'T> list>

    /// Search with options
    val searchWith : text: string -> fields: string list -> options: QueryOptions<'T>
                   -> collection: Collection<'T> -> Task<Document<'T> list>

    /// Get distinct values for a field
    val distinct : field: string -> filter: Query<'T> option
                 -> collection: Collection<'T> -> Task<obj list>

    // ═══════════════════════════════════════════════════════════════
    // WRITE OPERATIONS (Single Document)
    // ═══════════════════════════════════════════════════════════════

    /// Insert a document (ID auto-generated if empty)
    val insertOne : doc: 'T -> collection: Collection<'T>
                  -> Task<FractalResult<Document<'T>>>

    /// Update document by ID using transformation function
    val updateById : id: string -> update: ('T -> 'T) -> collection: Collection<'T>
                   -> Task<FractalResult<Document<'T> option>>

    /// Update first document matching filter
    val updateOne : filter: Query<'T> -> update: ('T -> 'T) -> collection: Collection<'T>
                  -> Task<FractalResult<Document<'T> option>>

    /// Update with upsert option
    val updateOneWith : filter: Query<'T> -> update: ('T -> 'T) -> upsert: bool
                      -> collection: Collection<'T>
                      -> Task<FractalResult<Document<'T> option>>

    /// Replace document entirely (preserves ID and createdAt)
    val replaceOne : filter: Query<'T> -> doc: 'T -> collection: Collection<'T>
                   -> Task<FractalResult<Document<'T> option>>

    /// Delete document by ID
    val deleteById : id: string -> collection: Collection<'T> -> Task<bool>

    /// Delete first document matching filter
    val deleteOne : filter: Query<'T> -> collection: Collection<'T> -> Task<bool>

    // ═══════════════════════════════════════════════════════════════
    // BATCH OPERATIONS
    // ═══════════════════════════════════════════════════════════════

    /// Insert multiple documents
    val insertMany : docs: 'T list -> collection: Collection<'T>
                   -> Task<FractalResult<InsertManyResult<'T>>>

    /// Insert many with options (ordered: stop on first error)
    val insertManyWith : docs: 'T list -> ordered: bool -> collection: Collection<'T>
                       -> Task<FractalResult<InsertManyResult<'T>>>

    /// Update all documents matching filter
    val updateMany : filter: Query<'T> -> update: ('T -> 'T) -> collection: Collection<'T>
                   -> Task<FractalResult<UpdateResult>>

    /// Delete all documents matching filter
    val deleteMany : filter: Query<'T> -> collection: Collection<'T>
                   -> Task<DeleteResult>

    // ═══════════════════════════════════════════════════════════════
    // ATOMIC FIND-AND-MODIFY OPERATIONS
    // ═══════════════════════════════════════════════════════════════

    /// Find and delete atomically, returns deleted document
    val findOneAndDelete : filter: Query<'T> -> collection: Collection<'T>
                         -> Task<Document<'T> option>

    /// Find and delete with options (sort)
    val findOneAndDeleteWith : filter: Query<'T> -> options: FindOptions
                             -> collection: Collection<'T>
                             -> Task<Document<'T> option>

    /// Find and update atomically
    val findOneAndUpdate : filter: Query<'T> -> update: ('T -> 'T)
                         -> options: FindAndModifyOptions
                         -> collection: Collection<'T>
                         -> Task<FractalResult<Document<'T> option>>

    /// Find and replace atomically
    val findOneAndReplace : filter: Query<'T> -> doc: 'T
                          -> options: FindAndModifyOptions
                          -> collection: Collection<'T>
                          -> Task<FractalResult<Document<'T> option>>

    // ═══════════════════════════════════════════════════════════════
    // UTILITY OPERATIONS
    // ═══════════════════════════════════════════════════════════════

    /// Drop the collection (delete table)
    val drop : collection: Collection<'T> -> Task<unit>

    /// Validate document against schema
    val validate : doc: 'T -> collection: Collection<'T> -> FractalResult<'T>
```

### 5.3 Result Types

```fsharp
/// Result of insertMany operation
type InsertManyResult<'T> = {
    Documents: Document<'T> list
    InsertedCount: int
}

/// Result of updateMany operation
type UpdateResult = {
    MatchedCount: int
    ModifiedCount: int
}

/// Result of deleteMany operation
type DeleteResult = {
    DeletedCount: int
}

/// Options for find operations
type FindOptions = {
    Sort: (string * SortDirection) list
}

/// Options for find-and-modify operations
type FindAndModifyOptions = {
    Sort: (string * SortDirection) list
    ReturnDocument: ReturnDocument
    Upsert: bool
}

and ReturnDocument =
    | Before  // Return document before modification
    | After   // Return document after modification
```

### 5.4 Usage Examples

```fsharp
open FractalDb
open FractalDb.Core
open FractalDb.Builders
open FractalDb.Storage

// Define document type
type User = {
    Name: string
    Email: string
    Age: int
    Active: bool
    Tags: string list
}

// Define schema
let userSchema = schema<User> {
    indexed "name" SqliteType.Text
    unique "email" SqliteType.Text
    indexed "age" SqliteType.Integer
    field "active" SqliteType.Integer
    timestamps
}

// Open database
use db = FractalDb.Open("app.db")
let users = db.Collection<User>("users", userSchema)

// ===== INSERT =====
let! insertResult =
    users
    |> Collection.insertOne {
        Name = "Alice"
        Email = "alice@example.com"
        Age = 30
        Active = true
        Tags = ["developer"; "fsharp"]
    }

match insertResult with
| Ok doc -> printfn $"Inserted: {doc.Id}"
| Error e -> printfn $"Failed: {e.Message}"

// ===== FIND BY ID =====
let! maybeUser = users |> Collection.findById "some-id"
match maybeUser with
| Some doc -> printfn $"Found: {doc.Data.Name}"
| None -> printfn "Not found"

// ===== QUERY WITH CE =====
let! activeDevs =
    users
    |> Collection.findWith
        (query {
            field "active" (Query.eq true)
            field "age" (Query.gte 18)
            field "tags" (Query.contains "developer")
        })
        (options<User> {
            sortAsc "name"
            limit 10
        })

for doc in activeDevs do
    printfn $"{doc.Data.Name} ({doc.Data.Email})"

// ===== UPDATE =====
let! updateResult =
    users
    |> Collection.updateById "some-id" (fun u ->
        { u with Age = u.Age + 1 })

// ===== FIND AND UPDATE (ATOMIC) =====
let! atomicResult =
    users
    |> Collection.findOneAndUpdate
        (query { field "email" (Query.eq "alice@example.com") })
        (fun u -> { u with Active = false })
        { Sort = []; ReturnDocument = After; Upsert = false }

// ===== BATCH INSERT =====
let! batchResult =
    users
    |> Collection.insertMany [
        { Name = "Bob"; Email = "bob@example.com"; Age = 25; Active = true; Tags = [] }
        { Name = "Charlie"; Email = "charlie@example.com"; Age = 35; Active = true; Tags = [] }
    ]

match batchResult with
| Ok result -> printfn $"Inserted {result.InsertedCount} documents"
| Error e -> printfn $"Batch failed: {e.Message}"

// ===== DELETE =====
let! deleted = users |> Collection.deleteById "some-id"
printfn $"Deleted: {deleted}"

// ===== COUNT =====
let! activeCount =
    users
    |> Collection.count (query { field "active" (Query.eq true) })
printfn $"Active users: {activeCount}"

// ===== SEARCH =====
let! searchResults =
    users
    |> Collection.searchWith "john" ["name"; "email"]
        (options<User> { limit 10 })
```

---

## 6. Schema System

### 6.1 SQLite Type Mapping

| F# Type | SQLite Type | Storage |
|---------|-------------|---------|
| `string` | TEXT | As-is |
| `int`, `int64` | INTEGER | As-is |
| `float`, `decimal` | REAL | As-is |
| `bool` | INTEGER | 0 or 1 |
| `byte[]` | BLOB | As-is |
| `DateTime`, `DateTimeOffset` | INTEGER | Unix timestamp (ms) |
| `'T list`, `'T array` | TEXT | JSON array |
| Record types | TEXT | JSON object |

### 6.2 Generated Columns

Indexed fields create virtual generated columns for efficient querying:

```sql
-- Table structure for users collection
CREATE TABLE IF NOT EXISTS users (
    _id TEXT PRIMARY KEY,
    body BLOB NOT NULL,
    createdAt INTEGER NOT NULL,
    updatedAt INTEGER NOT NULL,

    -- Generated columns for indexed fields
    _name TEXT GENERATED ALWAYS AS (jsonb_extract(body, '$.name')) VIRTUAL,
    _email TEXT GENERATED ALWAYS AS (jsonb_extract(body, '$.email')) VIRTUAL,
    _age INTEGER GENERATED ALWAYS AS (jsonb_extract(body, '$.age')) VIRTUAL
);

-- Indexes on generated columns
CREATE INDEX IF NOT EXISTS idx_users_name ON users(_name);
CREATE UNIQUE INDEX IF NOT EXISTS idx_users_email ON users(_email);
CREATE INDEX IF NOT EXISTS idx_users_age ON users(_age);

-- Compound indexes
CREATE INDEX IF NOT EXISTS idx_users_age_active ON users(_age, _active);
```

### 6.3 Table Builder Module

```fsharp
module internal TableBuilder =

    /// Generate CREATE TABLE SQL for a schema
    let createTableSql (name: string) (schema: SchemaDef<'T>) : string =
        let columns = [
            "_id TEXT PRIMARY KEY"
            "body BLOB NOT NULL"
            "createdAt INTEGER NOT NULL"
            "updatedAt INTEGER NOT NULL"
        ]

        let generatedCols =
            schema.Fields
            |> List.filter (fun f -> f.Indexed)
            |> List.map (fun f ->
                let colName = $"_{f.Name}"
                let sqlType = mapSqliteType f.SqlType
                let path = defaultArg f.Path $"$.{f.Name}"
                $"{colName} {sqlType} GENERATED ALWAYS AS (jsonb_extract(body, '{path}')) VIRTUAL"
            )

        let allColumns = columns @ generatedCols |> String.concat ",\n        "
        $"CREATE TABLE IF NOT EXISTS {name} (\n        {allColumns}\n    )"

    /// Generate CREATE INDEX statements
    let createIndexesSql (name: string) (schema: SchemaDef<'T>) : string list =
        let fieldIndexes =
            schema.Fields
            |> List.filter (fun f -> f.Indexed)
            |> List.map (fun f ->
                let unique = if f.Unique then "UNIQUE " else ""
                let indexName = $"idx_{name}_{f.Name}"
                $"CREATE {unique}INDEX IF NOT EXISTS {indexName} ON {name}(_{f.Name})"
            )

        let compoundIndexes =
            schema.Indexes
            |> List.map (fun idx ->
                let unique = if idx.Unique then "UNIQUE " else ""
                let cols = idx.Fields |> List.map (fun f -> $"_{f}") |> String.concat ", "
                $"CREATE {unique}INDEX IF NOT EXISTS {idx.Name} ON {name}({cols})"
            )

        fieldIndexes @ compoundIndexes
```

---

## 7. Storage Layer

### 7.1 Database Class

```fsharp
namespace FractalDb.Storage

open System
open System.Collections.Concurrent
open System.Threading.Tasks
open Microsoft.Data.Sqlite
open FractalDb.Core
open FractalDb.Builders

type DbOptions = {
    IdGenerator: unit -> string
    EnableCache: bool
}

module DbOptions =
    let defaults = {
        IdGenerator = IdGenerator.generate
        EnableCache = false
    }

type FractalDb private (connection: SqliteConnection, options: DbOptions) =
    let collections = ConcurrentDictionary<string, obj>()
    let mutable disposed = false

    /// Open database from file path
    static member Open(path: string, ?options: DbOptions) : FractalDb =
        let opts = defaultArg options DbOptions.defaults
        let conn = new SqliteConnection($"Data Source={path}")
        conn.Open()
        new FractalDb(conn, opts)

    /// Create in-memory database
    static member InMemory(?options: DbOptions) : FractalDb =
        FractalDb.Open(":memory:", ?options = options)

    /// Get or create a collection
    member this.Collection<'T>(name: string, schema: SchemaDef<'T>) : Collection<'T> =
        collections.GetOrAdd(name, fun _ ->
            let coll = {
                Name = name
                Schema = schema
                Connection = connection
                IdGenerator = options.IdGenerator
                Translator = SqlTranslator<'T>(schema, options.EnableCache)
                EnableCache = options.EnableCache
            }
            // Ensure table and indexes exist
            TableBuilder.ensureTable connection name schema
            box coll
        ) :?> Collection<'T>

    /// Create a manual transaction
    member this.Transaction() : Transaction =
        Transaction.create connection

    /// Execute function in transaction (commits on success, rolls back on exception)
    member this.Execute<'T>(fn: Transaction -> Task<'T>) : Task<'T> =
        task {
            use tx = this.Transaction()
            try
                let! result = fn tx
                tx.Commit()
                return result
            with ex ->
                tx.Rollback()
                return raise ex
        }

    /// Execute Result-returning function in transaction
    member this.ExecuteTransaction<'T>(fn: Transaction -> Task<FractalResult<'T>>)
        : Task<FractalResult<'T>> =
        task {
            use tx = this.Transaction()
            try
                let! result = fn tx
                match result with
                | Ok _ -> tx.Commit()
                | Error _ -> tx.Rollback()
                return result
            with ex ->
                tx.Rollback()
                return Error (FractalError.Transaction ex.Message)
        }

    /// Close database connection
    member this.Close() =
        if not disposed then
            connection.Close()
            connection.Dispose()
            disposed <- true

    interface IDisposable with
        member this.Dispose() = this.Close()
```

### 7.2 Donald Integration

```fsharp
module internal DbOperations =
    open Donald

    /// Insert a document
    let insertDocument (conn: IDbConnection) (table: string)
                       (id: string) (body: string) (now: int64)
                       : Result<unit, DbError> =
        conn
        |> Db.newCommand $"INSERT INTO {table} (_id, body, createdAt, updatedAt)
                          VALUES (@id, jsonb(@body), @created, @updated)"
        |> Db.setParams [
            "id", SqlType.String id
            "body", SqlType.String body
            "created", SqlType.Int64 now
            "updated", SqlType.Int64 now
        ]
        |> Db.exec

    /// Find document by ID
    let findById (conn: IDbConnection) (table: string) (id: string)
                 : Result<(string * string * int64 * int64) option, DbError> =
        conn
        |> Db.newCommand $"SELECT _id, json(body) as body, createdAt, updatedAt
                          FROM {table} WHERE _id = @id"
        |> Db.setParams ["id", SqlType.String id]
        |> Db.querySingle (fun rd ->
            rd.ReadString "_id",
            rd.ReadString "body",
            rd.ReadInt64 "createdAt",
            rd.ReadInt64 "updatedAt"
        )

    /// Execute parameterized query
    let query (conn: IDbConnection) (sql: string)
              (parameters: (string * SqlType) list)
              : Result<(string * string * int64 * int64) list, DbError> =
        conn
        |> Db.newCommand sql
        |> Db.setParams parameters
        |> Db.query (fun rd ->
            rd.ReadString "_id",
            rd.ReadString "body",
            rd.ReadInt64 "createdAt",
            rd.ReadInt64 "updatedAt"
        )

    /// Update document
    let updateDocument (conn: IDbConnection) (table: string)
                       (id: string) (body: string) (now: int64)
                       : Result<int, DbError> =
        conn
        |> Db.newCommand $"UPDATE {table}
                          SET body = jsonb(@body), updatedAt = @updated
                          WHERE _id = @id"
        |> Db.setParams [
            "id", SqlType.String id
            "body", SqlType.String body
            "updated", SqlType.Int64 now
        ]
        |> Db.execAndReturn (fun cmd -> cmd.RecordsAffected)

    /// Delete document
    let deleteDocument (conn: IDbConnection) (table: string) (id: string)
                       : Result<int, DbError> =
        conn
        |> Db.newCommand $"DELETE FROM {table} WHERE _id = @id"
        |> Db.setParams ["id", SqlType.String id]
        |> Db.execAndReturn (fun cmd -> cmd.RecordsAffected)

    /// Count documents
    let countDocuments (conn: IDbConnection) (sql: string)
                       (parameters: (string * SqlType) list)
                       : Result<int, DbError> =
        conn
        |> Db.newCommand sql
        |> Db.setParams parameters
        |> Db.querySingle (fun rd -> rd.ReadInt32 "count")
        |> Result.map (Option.defaultValue 0)
```

---

## 8. SQL Translation Engine

### 8.1 Translator Structure

```fsharp
namespace FractalDb.Storage

open FractalDb.Core
open FractalDb.Builders

type TranslatorResult = {
    Sql: string
    Parameters: (string * obj) list
}

module TranslatorResult =
    let empty = { Sql = "1=1"; Parameters = [] }
    let create sql params' = { Sql = sql; Parameters = params' }

type SqlTranslator<'T>(schema: SchemaDef<'T>, enableCache: bool) =

    let fieldMap =
        schema.Fields
        |> List.map (fun f -> f.Name, f)
        |> Map.ofList

    let cache =
        if enableCache then Some(ConcurrentDictionary<string, CacheEntry>())
        else None

    /// Resolve field name to SQL column reference
    member private this.ResolveField(fieldName: string) : string =
        match fieldName with
        | "_id" | "createdAt" | "updatedAt" -> fieldName
        | name ->
            match Map.tryFind name fieldMap with
            | Some field when field.Indexed -> $"_{name}"
            | _ -> $"jsonb_extract(body, '$.{name}')"

    /// Translate query to SQL
    member this.Translate(query: Query<'T>) : TranslatorResult =
        let paramCounter = ref 0
        let nextParam () =
            incr paramCounter
            $"@p{!paramCounter}"

        this.TranslateQuery(query, nextParam)

    member private this.TranslateQuery(query: Query<'T>, nextParam: unit -> string)
        : TranslatorResult =
        match query with
        | Query.Empty ->
            TranslatorResult.empty

        | Query.Field(name, op) ->
            let fieldSql = this.ResolveField(name)
            this.TranslateFieldOp(fieldSql, op, nextParam)

        | Query.And queries ->
            let results = queries |> List.map (fun q -> this.TranslateQuery(q, nextParam))
            let sql = results |> List.map (fun r -> r.Sql) |> String.concat " AND "
            let params' = results |> List.collect (fun r -> r.Parameters)
            TranslatorResult.create $"({sql})" params'

        | Query.Or queries ->
            let results = queries |> List.map (fun q -> this.TranslateQuery(q, nextParam))
            let sql = results |> List.map (fun r -> r.Sql) |> String.concat " OR "
            let params' = results |> List.collect (fun r -> r.Parameters)
            TranslatorResult.create $"({sql})" params'

        | Query.Nor queries ->
            let results = queries |> List.map (fun q -> this.TranslateQuery(q, nextParam))
            let sql = results |> List.map (fun r -> r.Sql) |> String.concat " OR "
            let params' = results |> List.collect (fun r -> r.Parameters)
            TranslatorResult.create $"NOT ({sql})" params'

        | Query.Not q ->
            let result = this.TranslateQuery(q, nextParam)
            TranslatorResult.create $"NOT ({result.Sql})" result.Parameters

    member private this.TranslateFieldOp(fieldSql: string, op: FieldOp, nextParam: unit -> string)
        : TranslatorResult =
        match op with
        | FieldOp.Compare boxedOp ->
            this.TranslateCompare(fieldSql, boxedOp, nextParam)
        | FieldOp.String stringOp ->
            this.TranslateString(fieldSql, stringOp, nextParam)
        | FieldOp.Array boxedOp ->
            this.TranslateArray(fieldSql, boxedOp, nextParam)
        | FieldOp.Exist existOp ->
            this.TranslateExist(fieldSql, existOp)

    member private this.TranslateCompare(fieldSql: string, boxedOp: obj, nextParam: unit -> string)
        : TranslatorResult =
        // Pattern match on boxed CompareOp
        let translateTyped (op: CompareOp<'V>) =
            match op with
            | CompareOp.Eq v ->
                let p = nextParam()
                TranslatorResult.create $"{fieldSql} = {p}" [p, box v]
            | CompareOp.Ne v ->
                let p = nextParam()
                TranslatorResult.create $"{fieldSql} != {p}" [p, box v]
            | CompareOp.Gt v ->
                let p = nextParam()
                TranslatorResult.create $"{fieldSql} > {p}" [p, box v]
            | CompareOp.Gte v ->
                let p = nextParam()
                TranslatorResult.create $"{fieldSql} >= {p}" [p, box v]
            | CompareOp.Lt v ->
                let p = nextParam()
                TranslatorResult.create $"{fieldSql} < {p}" [p, box v]
            | CompareOp.Lte v ->
                let p = nextParam()
                TranslatorResult.create $"{fieldSql} <= {p}" [p, box v]
            | CompareOp.In values when List.isEmpty values ->
                TranslatorResult.create "0=1" []  // Empty IN matches nothing
            | CompareOp.In values ->
                let ps = values |> List.map (fun v -> nextParam(), box v)
                let placeholders = ps |> List.map fst |> String.concat ", "
                TranslatorResult.create $"{fieldSql} IN ({placeholders})" ps
            | CompareOp.NotIn values when List.isEmpty values ->
                TranslatorResult.create "1=1" []  // Empty NOT IN matches everything
            | CompareOp.NotIn values ->
                let ps = values |> List.map (fun v -> nextParam(), box v)
                let placeholders = ps |> List.map fst |> String.concat ", "
                TranslatorResult.create $"{fieldSql} NOT IN ({placeholders})" ps

        // Unbox and translate
        match boxedOp with
        | :? CompareOp<int> as op -> translateTyped op
        | :? CompareOp<int64> as op -> translateTyped op
        | :? CompareOp<float> as op -> translateTyped op
        | :? CompareOp<string> as op -> translateTyped op
        | :? CompareOp<bool> as op -> translateTyped op
        | _ -> TranslatorResult.empty

    member private this.TranslateString(fieldSql: string, op: StringOp, nextParam: unit -> string)
        : TranslatorResult =
        match op with
        | StringOp.Like pattern ->
            let p = nextParam()
            TranslatorResult.create $"{fieldSql} LIKE {p}" [p, box pattern]
        | StringOp.ILike pattern ->
            let p = nextParam()
            TranslatorResult.create $"{fieldSql} LIKE {p} COLLATE NOCASE" [p, box pattern]
        | StringOp.Contains substring ->
            let p = nextParam()
            TranslatorResult.create $"{fieldSql} LIKE {p}" [p, box $"%%{substring}%%"]
        | StringOp.StartsWith prefix ->
            let p = nextParam()
            TranslatorResult.create $"{fieldSql} LIKE {p}" [p, box $"{prefix}%%"]
        | StringOp.EndsWith suffix ->
            let p = nextParam()
            TranslatorResult.create $"{fieldSql} LIKE {p}" [p, box $"%%{suffix}"]

    member private this.TranslateArray(fieldSql: string, boxedOp: obj, nextParam: unit -> string)
        : TranslatorResult =
        match boxedOp with
        | :? ArrayOp<_> as op ->
            match op with
            | ArrayOp.All values when List.isEmpty values ->
                TranslatorResult.create "1=1" []  // Empty ALL is vacuously true
            | ArrayOp.All values ->
                let conditions =
                    values
                    |> List.map (fun v ->
                        let p = nextParam()
                        $"EXISTS (SELECT 1 FROM json_each({fieldSql}) WHERE json_each.value = {p})",
                        [p, box v]
                    )
                let sql = conditions |> List.map fst |> String.concat " AND "
                let params' = conditions |> List.collect snd
                TranslatorResult.create $"({sql})" params'
            | ArrayOp.Size n ->
                let p = nextParam()
                TranslatorResult.create $"json_array_length({fieldSql}) = {p}" [p, box n]
            | ArrayOp.ElemMatch subQuery ->
                // Translate sub-query for array elements
                let subResult = this.TranslateQuery(subQuery, nextParam)
                // Replace field references with json_each.value
                let subSql = subResult.Sql.Replace(fieldSql, "json_each.value")
                TranslatorResult.create
                    $"EXISTS (SELECT 1 FROM json_each({fieldSql}) WHERE {subSql})"
                    subResult.Parameters
            | ArrayOp.Index(idx, subQuery) ->
                let elementSql = $"json_extract({fieldSql}, '$[{idx}]')"
                let subResult = this.TranslateQuery(subQuery, nextParam)
                let subSql = subResult.Sql.Replace(fieldSql, elementSql)
                TranslatorResult.create subSql subResult.Parameters
        | _ -> TranslatorResult.empty

    member private this.TranslateExist(fieldSql: string, op: ExistsOp) : TranslatorResult =
        match op with
        | ExistsOp.Exists true ->
            TranslatorResult.create $"json_type({fieldSql}) IS NOT NULL" []
        | ExistsOp.Exists false ->
            TranslatorResult.create $"json_type({fieldSql}) IS NULL" []

    /// Translate query options to SQL clauses
    member this.TranslateOptions(options: QueryOptions<'T>) : string * (string * obj) list =
        let clauses = ResizeArray<string>()
        let params' = ResizeArray<string * obj>()
        let paramCounter = ref 0
        let nextParam () =
            incr paramCounter
            $"@opt{!paramCounter}"

        // ORDER BY
        if not (List.isEmpty options.Sort) then
            let sortClauses =
                options.Sort
                |> List.map (fun (field, dir) ->
                    let colSql = this.ResolveField(field)
                    let dirSql = match dir with SortDirection.Ascending -> "ASC" | _ -> "DESC"
                    $"{colSql} {dirSql}"
                )
            clauses.Add($"ORDER BY {String.concat ", " sortClauses}")

        // LIMIT
        match options.Limit with
        | Some n ->
            let p = nextParam()
            clauses.Add($"LIMIT {p}")
            params'.Add(p, box n)
        | None -> ()

        // OFFSET
        match options.Skip with
        | Some n ->
            let p = nextParam()
            clauses.Add($"OFFSET {p}")
            params'.Add(p, box n)
        | None -> ()

        String.concat " " (clauses |> Seq.toList), params' |> Seq.toList
```

---

## 9. Error Handling

### 9.1 FractalError Discriminated Union

```fsharp
namespace FractalDb.Core

/// All possible errors in FractalDb
[<RequireQualifiedAccess>]
type FractalError =
    | Validation of field: string option * message: string
    | UniqueConstraint of field: string * value: obj
    | Query of message: string * sql: string option
    | Connection of message: string
    | Transaction of message: string
    | NotFound of id: string
    | Serialization of message: string
    | InvalidOperation of message: string

    /// Human-readable error message
    member this.Message =
        match this with
        | Validation (Some f, msg) -> $"Validation failed for '{f}': {msg}"
        | Validation (None, msg) -> $"Validation failed: {msg}"
        | UniqueConstraint (f, v) -> $"Duplicate value for unique field '{f}': {v}"
        | Query (msg, Some sql) -> $"Query error: {msg}. SQL: {sql}"
        | Query (msg, None) -> $"Query error: {msg}"
        | Connection msg -> $"Connection error: {msg}"
        | Transaction msg -> $"Transaction error: {msg}"
        | NotFound id -> $"Document not found: {id}"
        | Serialization msg -> $"Serialization error: {msg}"
        | InvalidOperation msg -> $"Invalid operation: {msg}"

    /// Error category for grouping
    member this.Category =
        match this with
        | Validation _ -> "validation"
        | UniqueConstraint _ -> "database"
        | Query _ -> "query"
        | Connection _ -> "database"
        | Transaction _ -> "transaction"
        | NotFound _ -> "query"
        | Serialization _ -> "serialization"
        | InvalidOperation _ -> "operation"

/// Type alias for Result with FractalError
type FractalResult<'T> = Result<'T, FractalError>
```

### 9.2 Result Extensions

```fsharp
module FractalResult =
    /// Map over success value
    let map f result = Result.map f result

    /// Bind (flatMap) over success value
    let bind f result = Result.bind f result

    /// Map over error
    let mapError f result = Result.mapError f result

    /// Convert Option to Result with NotFound error
    let ofOption (id: string) (opt: 'T option) : FractalResult<'T> =
        match opt with
        | Some v -> Ok v
        | None -> Error (FractalError.NotFound id)

    /// Convert Result to Option (discards error)
    let toOption result =
        match result with
        | Ok v -> Some v
        | Error _ -> None

    /// Get value or raise exception
    let getOrRaise result =
        match result with
        | Ok v -> v
        | Error e -> failwith e.Message

    /// Traverse a list, collecting results (stops at first error)
    let traverse (f: 'T -> FractalResult<'U>) (xs: 'T list) : FractalResult<'U list> =
        let rec loop acc = function
            | [] -> Ok (List.rev acc)
            | x :: rest ->
                match f x with
                | Ok y -> loop (y :: acc) rest
                | Error e -> Error e
        loop [] xs

    /// Sequence a list of results into a result of list
    let sequence (results: FractalResult<'T> list) : FractalResult<'T list> =
        traverse id results

    /// Combine two results
    let combine (r1: FractalResult<'T>) (r2: FractalResult<'U>)
        : FractalResult<'T * 'U> =
        match r1, r2 with
        | Ok v1, Ok v2 -> Ok (v1, v2)
        | Error e, _ | _, Error e -> Error e
```

---

## 10. Testing Strategy

### 10.1 Test Project Structure

```
FractalDb.Tests/
├── Assertions.fs           # Custom FsUnit.Light assertions
├── TestHelpers.fs          # Shared utilities and fixtures
├── Core/
│   ├── QueryTests.fs       # Query DU construction and composition
│   ├── SchemaTests.fs      # Schema builder tests
│   └── ErrorTests.fs       # Error type tests
├── Unit/
│   ├── SqlTranslatorTests.fs     # Query → SQL translation
│   ├── IdGeneratorTests.fs       # UUID v7 generation
│   └── JsonSerializationTests.fs # FSharp.SystemTextJson
├── Integration/
│   ├── CrudTests.fs              # Full CRUD operations
│   ├── QueryExecutionTests.fs    # Complex query execution
│   ├── TransactionTests.fs       # Transaction commit/rollback
│   ├── ValidationTests.fs        # Schema validation
│   ├── BatchOperationsTests.fs   # Batch insert/update/delete
│   ├── AtomicOperationsTests.fs  # findOneAndUpdate, etc.
│   └── CursorPaginationTests.fs  # Cursor-based pagination
└── Benchmarks/
    ├── CrudBenchmarks.fs         # CRUD performance
    └── QueryTranslationBenchmarks.fs
```

### 10.2 Custom Assertions (FsUnit.Light)

```fsharp
module FractalDb.Tests.Assertions

open FsUnit.Light
open FractalDb.Core

/// Assert Result is Ok
let shouldBeOk (result: FractalResult<'T>) =
    match result with
    | Ok _ -> ()
    | Error e -> failwith $"Expected Ok but got Error: {e.Message}"

/// Assert Result is Ok and extract value
let shouldBeOkWith (f: 'T -> unit) (result: FractalResult<'T>) =
    match result with
    | Ok v -> f v
    | Error e -> failwith $"Expected Ok but got Error: {e.Message}"

/// Assert Result is Error
let shouldBeError (result: FractalResult<'T>) =
    match result with
    | Error _ -> ()
    | Ok v -> failwith $"Expected Error but got Ok: {v}"

/// Assert Result is specific error type
let shouldBeErrorOf (expected: FractalError) (result: FractalResult<'T>) =
    match result with
    | Error e when e = expected -> ()
    | Error e -> failwith $"Expected {expected} but got {e}"
    | Ok v -> failwith $"Expected Error but got Ok: {v}"

/// Assert Option is Some
let shouldBeSome (opt: 'T option) =
    match opt with
    | Some _ -> ()
    | None -> failwith "Expected Some but got None"

/// Assert Option is Some and extract value
let shouldBeSomeWith (f: 'T -> unit) (opt: 'T option) =
    match opt with
    | Some v -> f v
    | None -> failwith "Expected Some but got None"

/// Assert Option is None
let shouldBeNone (opt: 'T option) =
    match opt with
    | None -> ()
    | Some v -> failwith $"Expected None but got Some: {v}"

/// Assert string is not null or empty
let shouldNotBeEmpty (s: string) =
    if String.IsNullOrEmpty s then
        failwith "Expected non-empty string"

/// Assert Task<Result> is Ok
let shouldBeOkTask (task: Task<FractalResult<'T>>) =
    task.Result |> shouldBeOk

/// Assert Task<Option> is Some
let shouldBeSomeTask (task: Task<'T option>) =
    task.Result |> shouldBeSome
```

### 10.3 Test Examples

#### Unit Tests

```fsharp
module FractalDb.Tests.Unit.SqlTranslatorTests

open Xunit
open FsUnit.Light
open FractalDb.Core
open FractalDb.Builders
open FractalDb.Storage

let testSchema = schema<TestUser> {
    indexed "name" SqliteType.Text
    indexed "age" SqliteType.Integer
    unique "email" SqliteType.Text
    field "active" SqliteType.Integer
}

let translator = SqlTranslator<TestUser>(testSchema, false)

[<Fact>]
let ``Empty query translates to 1=1`` () =
    let result = translator.Translate(Query.empty)
    result.Sql |> shouldEqual "1=1"
    result.Parameters |> shouldEqual []

[<Fact>]
let ``Simple equality on indexed field uses generated column`` () =
    let q = query { field "name" (Query.eq "Alice") }
    let result = translator.Translate(q)
    result.Sql |> shouldContainText "_name = @p1"
    result.Parameters |> shouldHaveLength 1

[<Fact>]
let ``Simple equality on non-indexed field uses jsonb_extract`` () =
    let q = query { field "active" (Query.eq true) }
    let result = translator.Translate(q)
    result.Sql |> shouldContainText "jsonb_extract(body, '$.active')"

[<Fact>]
let ``Range query generates AND conditions`` () =
    let q = Query.all' [
        Query.field "age" (Query.gte 18)
        Query.field "age" (Query.lt 65)
    ]
    let result = translator.Translate(q)
    result.Sql |> shouldContainText "_age >= @p1"
    result.Sql |> shouldContainText "_age < @p2"
    result.Sql |> shouldContainText "AND"
    result.Parameters |> shouldHaveLength 2

[<Fact>]
let ``IN operator generates correct SQL`` () =
    let q = query { field "age" (Query.isIn [18; 25; 30]) }
    let result = translator.Translate(q)
    result.Sql |> shouldContainText "IN (@p1, @p2, @p3)"
    result.Parameters |> shouldHaveLength 3

[<Fact>]
let ``Empty IN generates 0=1`` () =
    let q = query { field "age" (Query.isIn []) }
    let result = translator.Translate(q)
    result.Sql |> shouldContainText "0=1"

[<Fact>]
let ``String contains generates LIKE with wildcards`` () =
    let q = query { field "name" (Query.contains "Smith") }
    let result = translator.Translate(q)
    result.Sql |> shouldContainText "LIKE @p1"
    let param = result.Parameters |> List.head |> snd :?> string
    param |> shouldEqual "%Smith%"

[<Fact>]
let ``Case-insensitive LIKE uses COLLATE NOCASE`` () =
    let q = query { field "name" (Query.ilike "%alice%") }
    let result = translator.Translate(q)
    result.Sql |> shouldContainText "COLLATE NOCASE"

[<Fact>]
let ``OR query generates correct SQL`` () =
    let q = Query.any [
        Query.field "role" (Query.eq "admin")
        Query.field "role" (Query.eq "moderator")
    ]
    let result = translator.Translate(q)
    result.Sql |> shouldContainText "OR"

[<Fact>]
let ``NOT query wraps condition`` () =
    let q = Query.not' (Query.field "banned" (Query.eq true))
    let result = translator.Translate(q)
    result.Sql |> shouldContainText "NOT ("
```

#### Integration Tests

```fsharp
module FractalDb.Tests.Integration.CrudTests

open System.Threading.Tasks
open Xunit
open FsUnit.Light
open FractalDb
open FractalDb.Core
open FractalDb.Builders
open FractalDb.Storage
open FractalDb.Tests.Assertions

type User = {
    Name: string
    Email: string
    Age: int
    Active: bool
    Tags: string list
}

let userSchema = schema<User> {
    indexed "name" SqliteType.Text
    unique "email" SqliteType.Text
    indexed "age" SqliteType.Integer
    field "active" SqliteType.Integer
    timestamps
}

type CrudTestFixture() =
    let db = FractalDb.InMemory()
    let users = db.Collection<User>("users", userSchema)

    member _.Db = db
    member _.Users = users

    interface IDisposable with
        member _.Dispose() = db.Close()

type CrudTests(fixture: CrudTestFixture) =
    interface IClassFixture<CrudTestFixture>

    let users = fixture.Users

    [<Fact>]
    member _.``insertOne creates document with auto-generated ID`` () = task {
        let! result = users |> Collection.insertOne {
            Name = "Alice"
            Email = "alice@test.com"
            Age = 30
            Active = true
            Tags = ["developer"]
        }

        result |> shouldBeOk
        result |> shouldBeOkWith (fun doc ->
            doc.Id |> shouldNotBeEmpty
            doc.Data.Name |> shouldEqual "Alice"
            doc.CreatedAt |> shouldBeGreaterThan 0L
        )
    }

    [<Fact>]
    member _.``insertOne fails with UniqueConstraint for duplicate email`` () = task {
        let! _ = users |> Collection.insertOne {
            Name = "User1"
            Email = "duplicate@test.com"
            Age = 25
            Active = true
            Tags = []
        }

        let! result = users |> Collection.insertOne {
            Name = "User2"
            Email = "duplicate@test.com"
            Age = 30
            Active = true
            Tags = []
        }

        result |> shouldBeError
        match result with
        | Error (FractalError.UniqueConstraint (field, _)) ->
            field |> shouldContainText "email"
        | _ -> failwith "Expected UniqueConstraint error"
    }

    [<Fact>]
    member _.``findById returns Some for existing document`` () = task {
        let! insertResult = users |> Collection.insertOne {
            Name = "Bob"
            Email = "bob@test.com"
            Age = 25
            Active = true
            Tags = []
        }

        let doc = insertResult |> FractalResult.getOrRaise
        let! found = users |> Collection.findById doc.Id

        found |> shouldBeSome
        found |> shouldBeSomeWith (fun d -> d.Data.Name |> shouldEqual "Bob")
    }

    [<Fact>]
    member _.``findById returns None for non-existent document`` () = task {
        let! found = users |> Collection.findById "non-existent-id"
        found |> shouldBeNone
    }

    [<Fact>]
    member _.``find with query filters correctly`` () = task {
        // Setup: insert 10 users
        for i in 1..10 do
            let! _ = users |> Collection.insertOne {
                Name = $"User{i}"
                Email = $"user{i}@test.com"
                Age = 20 + i
                Active = i % 2 = 0
                Tags = []
            }
            ()

        // Query active users
        let! activeUsers =
            users
            |> Collection.find (query { field "active" (Query.eq true) })

        activeUsers |> shouldHaveLength 5
        for doc in activeUsers do
            doc.Data.Active |> shouldEqual true
    }

    [<Fact>]
    member _.``updateById modifies document`` () = task {
        let! insertResult = users |> Collection.insertOne {
            Name = "Charlie"
            Email = "charlie@test.com"
            Age = 35
            Active = true
            Tags = []
        }

        let doc = insertResult |> FractalResult.getOrRaise

        let! updateResult =
            users
            |> Collection.updateById doc.Id (fun u -> { u with Age = 36 })

        updateResult |> shouldBeOk
        updateResult |> shouldBeOkWith (fun opt ->
            opt |> shouldBeSomeWith (fun d ->
                d.Data.Age |> shouldEqual 36
                d.UpdatedAt |> shouldBeGreaterThan doc.UpdatedAt
            )
        )
    }

    [<Fact>]
    member _.``deleteById removes document`` () = task {
        let! insertResult = users |> Collection.insertOne {
            Name = "ToDelete"
            Email = "delete@test.com"
            Age = 40
            Active = true
            Tags = []
        }

        let doc = insertResult |> FractalResult.getOrRaise

        let! deleted = users |> Collection.deleteById doc.Id
        deleted |> shouldEqual true

        let! found = users |> Collection.findById doc.Id
        found |> shouldBeNone
    }
```

#### Transaction Tests

```fsharp
module FractalDb.Tests.Integration.TransactionTests

[<Fact>]
member _.``transaction commits on success`` () = task {
    let! result = db.Transact {
        let! user1 = users |> Collection.insertOne user1Data
        let! user2 = users |> Collection.insertOne user2Data
        return (user1, user2)
    }

    result |> shouldBeOk

    // Verify both were persisted
    let! count = users |> Collection.count Query.empty
    count |> shouldBeGreaterThan 1
}

[<Fact>]
member _.``transaction rolls back on error`` () = task {
    // First insert a user
    let! _ = users |> Collection.insertOne {
        Name = "Existing"
        Email = "existing@test.com"
        Age = 30
        Active = true
        Tags = []
    }

    // Try transaction that will fail on second insert (duplicate email)
    let! result = db.Transact {
        let! _ = users |> Collection.insertOne {
            Name = "New1"
            Email = "new1@test.com"
            Age = 25
            Active = true
            Tags = []
        }
        // This should fail - duplicate email
        return! users |> Collection.insertOne {
            Name = "Dup"
            Email = "existing@test.com"
            Age = 35
            Active = true
            Tags = []
        }
    }

    result |> shouldBeError

    // Verify rollback - new1 should NOT exist
    let! found = users |> Collection.findOne (query {
        field "email" (Query.eq "new1@test.com")
    })
    found |> shouldBeNone
}
```

---

## 11. Performance Benchmarks

### 11.1 Benchmark Targets

Based on TypeScript StrataDB performance, FractalDb targets:

| Operation | TypeScript | F# Target | Notes |
|-----------|------------|-----------|-------|
| `insertOne` | < 500 µs | **< 400 µs** | Single document with validation |
| `insertMany` (100 docs) | < 15 ms | **< 12 ms** | Batch insert in transaction |
| `findById` | < 100 µs | **< 80 µs** | Primary key lookup |
| `find` (100 docs) | < 2 ms | **< 1.5 ms** | Full scan with deserialization |
| `find` (indexed, 10 results) | < 500 µs | **< 400 µs** | Index-assisted query |
| `updateOne` | < 300 µs | **< 250 µs** | Find + update + serialize |
| `deleteOne` | < 200 µs | **< 150 µs** | Find + delete |
| `count` | < 100 µs | **< 80 µs** | COUNT query |
| `transaction` (10 ops) | < 5 ms | **< 4 ms** | Multiple ops with commit |
| Query translation | < 50 µs | **< 30 µs** | Query DU → SQL |
| Query translation (cached) | < 5 µs | **< 3 µs** | Cache hit |

### 11.2 Benchmark Implementation

```fsharp
namespace FractalDb.Tests.Benchmarks

open BenchmarkDotNet.Attributes
open BenchmarkDotNet.Running
open FractalDb
open FractalDb.Core
open FractalDb.Builders
open FractalDb.Storage

type User = {
    Name: string
    Email: string
    Age: int
    Active: bool
    Tags: string list
}

[<MemoryDiagnoser>]
[<SimpleJob(warmupCount = 3, iterationCount = 10)>]
type CrudBenchmarks() =

    let mutable db: FractalDb = Unchecked.defaultof<_>
    let mutable users: Collection<User> = Unchecked.defaultof<_>
    let mutable testIds: string list = []
    let mutable emailCounter = 0

    let userSchema = schema<User> {
        indexed "name" SqliteType.Text
        unique "email" SqliteType.Text
        indexed "age" SqliteType.Integer
        field "active" SqliteType.Integer
        timestamps
    }

    let generateUser () =
        emailCounter <- emailCounter + 1
        {
            Name = $"User{emailCounter}"
            Email = $"user{emailCounter}@bench.com"
            Age = 20 + (emailCounter % 50)
            Active = emailCounter % 2 = 0
            Tags = ["tag1"; "tag2"]
        }

    [<GlobalSetup>]
    member _.Setup() =
        db <- FractalDb.InMemory()
        users <- db.Collection<User>("users", userSchema)

        // Seed with 1000 documents
        for _ in 1..1000 do
            let result = (users |> Collection.insertOne (generateUser())).Result
            match result with
            | Ok doc -> testIds <- doc.Id :: testIds
            | Error _ -> ()

    [<GlobalCleanup>]
    member _.Cleanup() =
        db.Close()

    [<Benchmark>]
    member _.InsertOne() =
        (users |> Collection.insertOne (generateUser())).Result

    [<Benchmark>]
    member _.FindById() =
        let id = testIds.[500]
        (users |> Collection.findById id).Result

    [<Benchmark>]
    member _.FindWithIndexedField() =
        (users
        |> Collection.findWith
            (query { field "age" (Query.gte 30) })
            (options<User> { limit 10 })).Result

    [<Benchmark>]
    member _.FindWithSort() =
        (users
        |> Collection.findWith
            Query.empty
            (options<User> { sortDesc "createdAt"; limit 20 })).Result

    [<Benchmark>]
    member _.Count() =
        (users
        |> Collection.count (query { field "active" (Query.eq true) })).Result

    [<Benchmark>]
    member _.UpdateOne() =
        let id = testIds.[500]
        (users
        |> Collection.updateById id (fun u -> { u with Age = u.Age + 1 })).Result

[<MemoryDiagnoser>]
type QueryTranslationBenchmarks() =

    let schema = schema<User> {
        indexed "name" SqliteType.Text
        indexed "age" SqliteType.Integer
        unique "email" SqliteType.Text
    }

    let translator = SqlTranslator<User>(schema, false)
    let cachedTranslator = SqlTranslator<User>(schema, true)

    let simpleQuery = query { field "name" (Query.eq "Alice") }

    let complexQuery = query {
        field "age" (Query.gte 18)
        field "active" (Query.eq true)
        orElse [
            Query.field "role" (Query.eq "admin")
            Query.field "tags" (Query.contains "premium")
        ]
    }

    [<Benchmark>]
    member _.SimpleQuery() =
        translator.Translate(simpleQuery)

    [<Benchmark>]
    member _.ComplexQuery() =
        translator.Translate(complexQuery)

    [<Benchmark>]
    member _.SimpleQueryCached() =
        cachedTranslator.Translate(simpleQuery)

    [<Benchmark>]
    member _.ComplexQueryCached() =
        cachedTranslator.Translate(complexQuery)

module Program =
    [<EntryPoint>]
    let main args =
        BenchmarkRunner.Run<CrudBenchmarks>() |> ignore
        BenchmarkRunner.Run<QueryTranslationBenchmarks>() |> ignore
        0
```

---

## 12. Implementation Roadmap

### Phase 1: Core Foundation (Week 1-2)

**Deliverables:**
- [ ] Project setup with .NET 9 SDK
- [ ] NuGet dependencies: Donald, Microsoft.Data.Sqlite, FSharp.SystemTextJson
- [ ] Core types: `Document<'T>`, `FractalError`, ID generation
- [ ] JSON serialization configuration
- [ ] Basic unit test infrastructure

**Files:**
- `FractalDb.fsproj`
- `Core/Types.fs`
- `Core/Errors.fs`
- `Json/Serialization.fs`

### Phase 2: Query System (Week 2-3)

**Deliverables:**
- [ ] Query discriminated unions (all operators)
- [ ] Query helper module
- [ ] QueryBuilder computation expression
- [ ] Query options types and OptionsBuilder CE
- [ ] Unit tests for query construction

**Files:**
- `Core/Query.fs`
- `Core/Operators.fs`
- `Core/Options.fs`
- `Builders/QueryBuilder.fs`
- `Builders/OptionsBuilder.fs`

### Phase 3: SQL Translation (Week 3-4)

**Deliverables:**
- [ ] SQL translator for all operators
- [ ] Field resolution (indexed vs non-indexed)
- [ ] Query options translation (ORDER BY, LIMIT, OFFSET)
- [ ] Query caching mechanism
- [ ] Comprehensive translation tests

**Files:**
- `Storage/SqlTranslator.fs`

### Phase 4: Schema & Storage (Week 4-5)

**Deliverables:**
- [ ] Schema types and SchemaBuilder CE
- [ ] Table builder (DDL generation)
- [ ] FractalDb database class
- [ ] Donald integration for DB operations
- [ ] Basic CRUD operations in Collection module

**Files:**
- `Core/Schema.fs`
- `Builders/SchemaBuilder.fs`
- `Storage/TableBuilder.fs`
- `Storage/Database.fs`
- `Storage/Collection.fs` (partial)

### Phase 5: Collection API (Week 5-6)

**Deliverables:**
- [ ] All 20+ Collection methods
- [ ] Batch operations (insertMany, updateMany, deleteMany)
- [ ] Atomic operations (findOneAndUpdate, etc.)
- [ ] Validation integration
- [ ] Integration tests for all operations

**Files:**
- `Storage/Collection.fs` (complete)

### Phase 6: Transactions (Week 6)

**Deliverables:**
- [ ] Transaction type and management
- [ ] TransactionBuilder computation expression
- [ ] Automatic commit/rollback based on Result
- [ ] Transaction integration tests

**Files:**
- `Storage/Transaction.fs`
- `Builders/TransactionBuilder.fs`

### Phase 7: Polish & Release (Week 7)

**Deliverables:**
- [ ] Public API exports in Library.fs
- [ ] XML documentation for all public types
- [ ] Performance benchmarks
- [ ] README with comprehensive examples
- [ ] NuGet package configuration
- [ ] Sample project

**Files:**
- `Library.fs`
- `README.md`
- `FractalDb.nuspec`

---

## 13. Project Configuration

### 13.1 FractalDb.fsproj

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
    <PackageId>FractalDb</PackageId>
    <Version>0.1.0</Version>
    <Authors>Your Name</Authors>
    <Description>Type-safe embedded document database for F# with MongoDB-like API backed by SQLite</Description>
    <PackageTags>fsharp;database;sqlite;document;nosql;embedded</PackageTags>
    <PackageLicenseExpression>MIT</PackageLicenseExpression>
    <PackageProjectUrl>https://github.com/yourname/FractalDb</PackageProjectUrl>
    <RepositoryUrl>https://github.com/yourname/FractalDb</RepositoryUrl>
  </PropertyGroup>

  <ItemGroup>
    <!-- Source files in compilation order (F# requires explicit ordering) -->
    <Compile Include="Core/Types.fs" />
    <Compile Include="Core/Operators.fs" />
    <Compile Include="Core/Query.fs" />
    <Compile Include="Core/Schema.fs" />
    <Compile Include="Core/Options.fs" />
    <Compile Include="Core/Errors.fs" />
    <Compile Include="Json/Serialization.fs" />
    <Compile Include="Storage/SqlTranslator.fs" />
    <Compile Include="Storage/TableBuilder.fs" />
    <Compile Include="Storage/Transaction.fs" />
    <Compile Include="Storage/Collection.fs" />
    <Compile Include="Storage/Database.fs" />
    <Compile Include="Builders/QueryBuilder.fs" />
    <Compile Include="Builders/SchemaBuilder.fs" />
    <Compile Include="Builders/OptionsBuilder.fs" />
    <Compile Include="Builders/TransactionBuilder.fs" />
    <Compile Include="Library.fs" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="Donald" Version="10.*" />
    <PackageReference Include="Microsoft.Data.Sqlite" Version="9.*" />
    <PackageReference Include="FSharp.SystemTextJson" Version="1.*" />
  </ItemGroup>

</Project>
```

### 13.2 FractalDb.Tests.fsproj

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <IsPackable>false</IsPackable>
    <GenerateProgramFile>false</GenerateProgramFile>
  </PropertyGroup>

  <ItemGroup>
    <Compile Include="Assertions.fs" />
    <Compile Include="TestHelpers.fs" />
    <Compile Include="Core/QueryTests.fs" />
    <Compile Include="Core/SchemaTests.fs" />
    <Compile Include="Core/ErrorTests.fs" />
    <Compile Include="Unit/SqlTranslatorTests.fs" />
    <Compile Include="Unit/IdGeneratorTests.fs" />
    <Compile Include="Unit/JsonSerializationTests.fs" />
    <Compile Include="Integration/CrudTests.fs" />
    <Compile Include="Integration/QueryExecutionTests.fs" />
    <Compile Include="Integration/TransactionTests.fs" />
    <Compile Include="Integration/ValidationTests.fs" />
    <Compile Include="Integration/BatchOperationsTests.fs" />
    <Compile Include="Integration/AtomicOperationsTests.fs" />
    <Compile Include="Integration/CursorPaginationTests.fs" />
    <Compile Include="Benchmarks/CrudBenchmarks.fs" />
    <Compile Include="Benchmarks/QueryTranslationBenchmarks.fs" />
    <Compile Include="Program.fs" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.*" />
    <PackageReference Include="xunit" Version="2.*" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.*">
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
    <PackageReference Include="FsUnit.Light.xUnit" Version="2.*" />
    <PackageReference Include="BenchmarkDotNet" Version="0.14.*" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\FractalDb\FractalDb.fsproj" />
  </ItemGroup>

</Project>
```

### 13.3 FractalDb.sln

```
Microsoft Visual Studio Solution File, Format Version 12.00
Project("{F2A71F9B-5D33-465A-A702-920D77279786}") = "FractalDb", "src\FractalDb\FractalDb.fsproj", "{GUID1}"
EndProject
Project("{F2A71F9B-5D33-465A-A702-920D77279786}") = "FractalDb.Tests", "tests\FractalDb.Tests\FractalDb.Tests.fsproj", "{GUID2}"
EndProject
Project("{F2A71F9B-5D33-465A-A702-920D77279786}") = "GettingStarted", "samples\GettingStarted\GettingStarted.fsproj", "{GUID3}"
EndProject
```

---

## 14. Complete Usage Examples

### 14.1 Getting Started

```fsharp
open FractalDb
open FractalDb.Core
open FractalDb.Builders
open FractalDb.Storage

// 1. Define your document type (pure immutable F# record)
type User = {
    Name: string
    Email: string
    Age: int
    Active: bool
    Tags: string list
}

// 2. Define schema using computation expression
let userSchema = schema<User> {
    indexed "name" SqliteType.Text
    unique "email" SqliteType.Text
    indexed "age" SqliteType.Integer
    field "active" SqliteType.Integer
    field "tags" SqliteType.Text
    timestamps
    validate (fun user ->
        if String.IsNullOrEmpty user.Email then
            Error "Email is required"
        elif user.Age < 0 then
            Error "Age must be non-negative"
        else
            Ok user
    )
}

// 3. Use the database
let main () = task {
    // Open database (use 'use' for automatic cleanup)
    use db = FractalDb.Open("app.db")
    let users = db.Collection<User>("users", userSchema)

    // ===== INSERT =====
    let! insertResult =
        users
        |> Collection.insertOne {
            Name = "Alice"
            Email = "alice@example.com"
            Age = 30
            Active = true
            Tags = ["developer"; "fsharp"]
        }

    match insertResult with
    | Ok doc -> printfn $"Inserted: {doc.Id}"
    | Error e -> printfn $"Error: {e.Message}"

    // ===== QUERY WITH CE =====
    let! activeDevs =
        users
        |> Collection.findWith
            (query {
                field "active" (Query.eq true)
                field "tags" (Query.contains "developer")
            })
            (options<User> {
                sortAsc "name"
                limit 10
            })

    for doc in activeDevs do
        printfn $"{doc.Data.Name} ({doc.Data.Email})"

    // ===== COMPLEX QUERY =====
    let! seniorDevs =
        users
        |> Collection.find (query {
            field "age" (Query.gte 30)
            field "active" (Query.eq true)
            orElse [
                Query.field "tags" (Query.contains "senior")
                Query.field "tags" (Query.contains "lead")
            ]
        })

    // ===== UPDATE =====
    match insertResult with
    | Ok doc ->
        let! updateResult =
            users
            |> Collection.updateById doc.Id (fun u ->
                { u with Age = u.Age + 1 })

        match updateResult with
        | Ok (Some updated) ->
            printfn $"Updated age to: {updated.Data.Age}"
        | Ok None ->
            printfn "User not found"
        | Error e ->
            printfn $"Update failed: {e.Message}"
    | Error _ -> ()

    // ===== TRANSACTION =====
    let! txResult = db.Transact {
        let! user1 = users |> Collection.insertOne {
            Name = "Bob"
            Email = "bob@example.com"
            Age = 25
            Active = true
            Tags = []
        }

        let! user2 = users |> Collection.insertOne {
            Name = "Charlie"
            Email = "charlie@example.com"
            Age = 35
            Active = false
            Tags = ["admin"]
        }

        return (user1, user2)
    }

    match txResult with
    | Ok (u1, u2) ->
        printfn $"Transaction succeeded: {u1.Id}, {u2.Id}"
    | Error e ->
        printfn $"Transaction failed (rolled back): {e.Message}"

    // ===== BATCH OPERATIONS =====
    let! batchResult =
        users
        |> Collection.insertMany [
            for i in 1..100 do
                { Name = $"User{i}"
                  Email = $"user{i}@example.com"
                  Age = 20 + i
                  Active = i % 2 = 0
                  Tags = [] }
        ]

    match batchResult with
    | Ok result -> printfn $"Inserted {result.InsertedCount} documents"
    | Error e -> printfn $"Batch failed: {e.Message}"

    // ===== ATOMIC FIND-AND-UPDATE =====
    let! atomicResult =
        users
        |> Collection.findOneAndUpdate
            (query { field "email" (Query.eq "alice@example.com") })
            (fun u -> { u with Tags = "updated" :: u.Tags })
            { Sort = []; ReturnDocument = After; Upsert = false }

    match atomicResult with
    | Ok (Some doc) -> printfn $"Updated: {doc.Data.Tags}"
    | Ok None -> printfn "Not found"
    | Error e -> printfn $"Failed: {e.Message}"

    // ===== SEARCH =====
    let! searchResults =
        users
        |> Collection.searchWith "alice" ["name"; "email"]
            (options<User> { limit 5 })

    printfn $"Found {List.length searchResults} matching users"

    // ===== COUNT =====
    let! activeCount =
        users
        |> Collection.count (query { field "active" (Query.eq true) })
    printfn $"Active users: {activeCount}"

    // ===== CURSOR PAGINATION =====
    let! firstPage =
        users
        |> Collection.findWith
            Query.empty
            (options<User> {
                sortDesc "createdAt"
                limit 20
            })

    match List.tryLast firstPage with
    | Some lastDoc ->
        let! secondPage =
            users
            |> Collection.findWith
                Query.empty
                (options<User> {
                    sortDesc "createdAt"
                    limit 20
                    cursorAfter lastDoc.Id
                })
        printfn $"Second page has {List.length secondPage} documents"
    | None -> ()
}

[<EntryPoint>]
let main' argv =
    main().GetAwaiter().GetResult()
    0
```

---

## Appendix A: Migration from StrataDB (TypeScript)

| TypeScript | F# (FractalDb) |
|------------|----------------|
| `new Strata({ database: 'app.db' })` | `FractalDb.Open("app.db")` |
| `createSchema<T>().field(...).build()` | `schema<T> { field ... }` |
| `{ age: { $gte: 18 } }` | `query { field "age" (Query.gte 18) }` |
| `collection.find(filter, { sort, limit })` | `Collection.findWith filter (options { ... })` |
| `await collection.insertOne(doc)` | `Collection.insertOne doc collection` |
| `throw new ValidationError(...)` | `Error (FractalError.Validation (...))` |
| `Promise<T>` | `Task<T>` |
| `T | null` | `'T option` |
| `try/catch` | `Result<'T, FractalError>` pattern matching |

---

## Appendix B: References

- [Donald - F# ADO.NET wrapper](https://github.com/pimbrouwers/Donald)
- [FSharp.SystemTextJson](https://github.com/Tarmil/FSharp.SystemTextJson)
- [FsUnit.Light](https://github.com/Lanayx/FsUnit.Light)
- [SQLite JSON1 Extension](https://www.sqlite.org/json1.html)
- [UUID v7 Specification (RFC 9562)](https://datatracker.ietf.org/doc/html/rfc9562)
- [BenchmarkDotNet](https://benchmarkdotnet.org/)
