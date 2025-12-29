# FractalDb

A type-safe **embedded document database** for .NET 9+. No separate server process, no network overhead - just reference the library and use it.

> **FractalDb** is the F# port of [StrataDB](https://github.com/g5becks/StrataDB), bringing MongoDB-like operations to .NET with full type safety and functional programming idioms.

## Why Embedded?

Unlike MongoDB or other client-server databases, FractalDb runs **in-process** with your application:

- **Zero setup** - No database server to install or manage
- **Zero latency** - Direct memory access, no network round-trips  
- **Single file** - Your entire database is one portable `.db` file
- **Serverless ready** - Perfect for edge functions, CLI tools, desktop apps, and microservices

## Features

- **Embedded SQLite** - Runs in-process via `Microsoft.Data.Sqlite`, no external dependencies
- **Full Type Safety** - F# type system ensures compile-time validation of queries, schemas, and results
- **MongoDB-like API** - Familiar operators: `eq`, `gt`, `in`, logical combinators (`And`, `Or`, etc.)
- **JSONB Storage** - Flexible documents with indexed generated columns
- **Portable** - Single file database, easy to backup and deploy
- **Result-Based Error Handling** - No exceptions for business logic errors, all operations return `Result<'T, FractalError>`
- **Computation Expressions** - F#-idiomatic query, schema, and options builders
- **ACID Transactions** - Full transaction support with automatic rollback on errors
- **Batch Operations** - Efficient bulk inserts, updates, and deletes
- **Atomic Operations** - Find-and-modify operations with optional upsert support

## Installation

```bash
# Add package reference (once published to NuGet)
dotnet add package FractalDb

# Or clone and build locally
git clone https://github.com/g5becks/StrataDB.git
cd StrataDB
dotnet build FractalDb.slnx
```

Requires **.NET 9.0** or higher.

## Quick Start

```fsharp
open FractalDb

// Define your document type
type User = {
    Name: string
    Email: string
    Age: int
}

// Create schema with validation
let userSchema = 
    schema {
        field "name" SqliteType.Text (fun u -> u.Name) [ Indexed ]
        field "email" SqliteType.Text (fun u -> u.Email) [ Indexed; Unique ]
        field "age" SqliteType.Integer (fun u -> int64 u.Age) [ Indexed ]
        validate (fun user ->
            if user.Age < 0 then Error "Age must be positive"
            elif not (user.Email.Contains("@")) then Error "Invalid email"
            else Ok user
        )
    }

// Open database and get collection
use db = FractalDb.Open("app.db", DbOptions.Default)
let users = db.Collection<User>("users", userSchema)

// Insert documents
let! result = users.InsertOne { Name = "Alice"; Email = "alice@example.com"; Age = 30 }
match result with
| Ok doc -> printfn "Inserted: %s" doc._id
| Error err -> printfn "Error: %s" err.Message

// Query with type-safe operators
let! adults = 
    query {
        where (Query.gte "age" 18L)
        where (Query.endsWith "email" "@example.com")
    }
    |> users.Find

// Update
let! updateResult = 
    users.UpdateById(doc._id, {| age = 31 |})

// Find and modify atomically
let! atomicResult =
    users.FindOneAndUpdate(
        Query.eq "email" "alice@example.com",
        {| age = 32 |},
        options { upsert true; returnNew true }
    )

// Transactions
let! txResult = 
    db.Transact(fun tx -> task {
        let! user1 = users.InsertOne({ Name = "Bob"; Email = "bob@example.com"; Age = 25 }, tx)
        let! user2 = users.InsertOne({ Name = "Charlie"; Email = "charlie@example.com"; Age = 35 }, tx)
        return (user1, user2)
    })
```

## Type Safety

F# type system validates schemas and queries at compile time:

```fsharp
// ✅ Correct - types match
let schema = 
    schema {
        field "name" SqliteType.Text (fun (u: User) -> u.Name) []  // string -> TEXT
        field "age" SqliteType.Integer (fun u -> int64 u.Age) []   // int -> INTEGER
    }

// ❌ Compile error - type mismatch
let badSchema = 
    schema {
        field "name" SqliteType.Integer (fun (u: User) -> u.Name) []  // Error: string cannot be INTEGER
    }
```

## Query System

### Basic Queries

```fsharp
// Comparison operators
let! users = users.Find(Query.eq "status" "active")
let! adults = users.Find(Query.gte "age" 18L)
let! vips = users.Find(Query.inList "tier" ["gold"; "platinum"])

// String operators
let! gmailUsers = users.Find(Query.endsWith "email" "@gmail.com")
let! matches = users.Find(Query.contains "name" "smith")
let! caseInsensitive = users.Find(Query.ilike "name" "%alice%")

// Logical combinators
let! results = 
    Query.empty
    |> Query.gte_ "age" 18L
    |> Query.eq_ "status" "active"
    |> Query.inList_ "role" ["admin"; "moderator"]
    |> users.Find
```

### Computation Expression Style

```fsharp
let! activeAdults =
    query {
        where (Query.eq "status" "active")
        where (Query.gte "age" 18L)
        where (Query.contains "email" "@company.com")
    }
    |> users.Find
```

### Complex Queries

```fsharp
// Nested logical operations
let complexQuery =
    Query.and_ [
        Query.or_ [
            Query.eq "role" "admin"
            Query.eq "role" "moderator"
        ]
        Query.gte "age" 21L
        Query.not_ (Query.eq "status" "banned")
    ]

let! results = users.Find(complexQuery)
```

## Schema Definition

### Basic Schema

```fsharp
type Post = {
    Title: string
    Content: string
    AuthorId: string
    Tags: string list
    PublishedAt: int64 option
}

let postSchema =
    schema {
        field "title" SqliteType.Text (fun p -> p.Title) [ Indexed ]
        field "authorId" SqliteType.Text (fun p -> p.AuthorId) [ Indexed ]
        field "publishedAt" SqliteType.Integer (fun p -> 
            p.PublishedAt |> Option.defaultValue 0L
        ) []
        
        // Composite index for efficient queries
        index "idx_author_published" ["authorId"; "publishedAt"]
    }
```

### Schema with Validation

```fsharp
let userSchema =
    schema {
        field "email" SqliteType.Text (fun u -> u.Email) [ Unique; Indexed ]
        field "age" SqliteType.Integer (fun u -> int64 u.Age) []
        
        validate (fun user ->
            // Validation is explicit - call Collection.Validate before CRUD
            if user.Age < 13 then Error "Must be 13 or older"
            elif not (user.Email.Contains("@")) then Error "Invalid email format"
            else Ok user
        )
    }

// Explicit validation before insert
match Collection.validate userSchema newUser with
| Ok validUser ->
    let! result = users.InsertOne(validUser)
    // handle result
| Error validationError ->
    printfn "Validation failed: %s" validationError
```

## Query Options

### Sorting and Pagination

```fsharp
let! page1 = 
    options {
        sortBy "createdAt" Descending
        limit 20
        skip 0
    }
    |> users.Find Query.empty

let! page2 =
    options {
        sortBy "createdAt" Descending
        limit 20
        skip 20
    }
    |> users.Find Query.empty
```

### Field Projection

```fsharp
// Include only specific fields
let! names =
    options {
        project ["_id"; "name"; "email"]
    }
    |> users.Find Query.empty

// Projections are type-erased at runtime, return full document type
// Use pattern matching or record updates to extract needed fields
```

### Cursor-Based Pagination

```fsharp
let! page1 =
    options {
        sortBy "createdAt" Descending
        limit 20
    }
    |> users.Find Query.empty

// Get last document's ID for cursor
let cursor = page1 |> List.tryLast |> Option.map (fun doc -> doc._id)

let! page2 =
    match cursor with
    | Some id ->
        options {
            sortBy "createdAt" Descending
            limit 20
            cursorAfter id
        }
        |> users.Find Query.empty
    | None -> Task.FromResult(Ok [])
```

## CRUD Operations

### Insert

```fsharp
// Insert one
let! result = users.InsertOne({ Name = "Alice"; Email = "alice@example.com"; Age = 30 })

// Insert many (wrapped in transaction)
let! results = 
    users.InsertMany([
        { Name = "Bob"; Email = "bob@example.com"; Age = 25 }
        { Name = "Charlie"; Email = "charlie@example.com"; Age = 35 }
    ])
```

### Find

```fsharp
// Find all matching documents
let! activeUsers = users.Find(Query.eq "status" "active")

// Find one
let! user = users.FindOne(Query.eq "email" "alice@example.com")

// Find by ID
let! user = users.FindById("user-id-123")

// Count
let! count = users.Count(Query.eq "status" "active")
```

### Update

```fsharp
// Update by ID
let! result = users.UpdateById(userId, {| age = 31; status = "active" |})

// Update one matching document
let! result = 
    users.UpdateOne(
        Query.eq "email" "alice@example.com",
        {| lastLogin = Timestamp.Now() |}
    )

// Update many
let! result =
    users.UpdateMany(
        Query.eq "status" "pending",
        {| status = "active" |}
    )

// Replace entire document
let! result =
    users.ReplaceOne(
        Query.eq "email" "alice@example.com",
        { Name = "Alice Smith"; Email = "alice@example.com"; Age = 31 }
    )
```

### Delete

```fsharp
// Delete by ID
let! result = users.DeleteById(userId)

// Delete one
let! result = users.DeleteOne(Query.eq "email" "old@example.com")

// Delete many
let! result = users.DeleteMany(Query.lt "lastLogin" oldTimestamp)
```

## Atomic Operations

Find-and-modify operations are atomic and support upsert:

```fsharp
// Find and update
let! result =
    users.FindOneAndUpdate(
        Query.eq "email" "alice@example.com",
        {| lastLogin = Timestamp.Now() |},
        options { returnNew true }
    )

// Find and replace
let! result =
    users.FindOneAndReplace(
        Query.eq "email" "alice@example.com",
        { Name = "Alice Smith"; Email = "alice@example.com"; Age = 31 },
        options { returnNew true; upsert true }
    )

// Find and delete
let! result =
    users.FindOneAndDelete(Query.eq "status" "deleted")
```

## Transactions

FractalDb provides full ACID transaction support:

```fsharp
// Automatic commit on success, rollback on error
let! result = 
    db.Transact(fun tx -> task {
        // All operations use same transaction
        let! user = users.InsertOne({ Name = "Alice"; Email = "alice@example.com"; Age = 30 }, tx)
        let! post = posts.InsertOne({ Title = "Hello"; AuthorId = user._id }, tx)
        
        // Return both documents
        return (user, post)
    })

match result with
| Ok (user, post) -> 
    printfn "Transaction committed: %s, %s" user._id post._id
| Error err -> 
    printfn "Transaction rolled back: %s" err.Message
```

## Batch Operations

Batch operations are automatically wrapped in transactions:

```fsharp
// Batch insert
let newUsers = [
    { Name = "Alice"; Email = "alice@example.com"; Age = 30 }
    { Name = "Bob"; Email = "bob@example.com"; Age = 25 }
    { Name = "Charlie"; Email = "charlie@example.com"; Age = 35 }
]

let! results = users.InsertMany(newUsers)

match results with
| Ok docs -> printfn "Inserted %d documents" docs.Length
| Error err -> printfn "Batch insert failed: %s" err.Message

// Batch update
let! updateResults =
    users.UpdateMany(
        Query.eq "status" "pending",
        {| status = "active"; activatedAt = Timestamp.Now() |}
    )

// Batch delete
let! deleteResults =
    users.DeleteMany(Query.lt "lastLogin" oldTimestamp)
```

## Error Handling

All operations return `Result<'T, FractalError>`:

```fsharp
type FractalError =
    | DatabaseError of message: string * innerError: exn option
    | SerializationError of message: string * innerError: exn option
    | QueryError of message: string
    | SchemaError of message: string
    | ValidationError of message: string
    | ConstraintViolation of message: string
    | NotFound of message: string
    | TransactionError of message: string

// Pattern match on results
match! users.InsertOne(newUser) with
| Ok doc -> 
    printfn "Success: %s" doc._id
| Error (ConstraintViolation msg) -> 
    printfn "Unique constraint failed: %s" msg
| Error (ValidationError msg) -> 
    printfn "Validation failed: %s" msg
| Error err -> 
    printfn "Other error: %s" err.Message

// Use Result combinators
let! processedUser =
    users.FindById(userId)
    |> TaskResult.bind (fun user -> 
        users.UpdateById(user._id, {| processed = true |})
    )
```

## Advanced Features

### Custom ID Generation

```fsharp
// ULID-based IDs (default)
let id = IdGenerator.Generate()  // e.g., "01HQZX3P8KF9QM8N7R6S5T4V3W"

// IDs are time-sortable and lexicographically ordered
```

### Timestamps

```fsharp
// Unix milliseconds (Int64)
let now = Timestamp.Now()  // 1703721600000L

// Documents automatically get createdAt/updatedAt
type Document<'T> = {
    _id: string
    body: 'T
    createdAt: int64
    updatedAt: int64
}
```

### Collection Management

```fsharp
// Drop collection
let! result = users.Drop()

// Create index manually
let! result = users.CreateIndex("idx_email", ["email"])
```

## Configuration

### Database Options

```fsharp
let dbOptions = {
    CacheSize = -2000       // 2MB cache (negative = KB, positive = pages)
    BusyTimeout = 5000      // 5 seconds
    JournalMode = "WAL"     // Write-Ahead Logging for concurrency
    Synchronous = "NORMAL"  // Balance safety and performance
    TempStore = "MEMORY"    // Use memory for temp tables
    MmapSize = 30000000000L // 30GB memory-mapped I/O
}

use db = FractalDb.Open("app.db", dbOptions)
```

### In-Memory Database

```fsharp
// Perfect for testing
use db = FractalDb.InMemory()
let users = db.Collection<User>("users", userSchema)

// All data lost when db is disposed
```

## Differences from TypeScript StrataDB

| Feature | StrataDB (TypeScript) | FractalDb (F#) |
|---------|----------------------|----------------|
| Error Handling | Exceptions | `Result<'T, FractalError>` |
| Operators | `$eq`, `$gt`, etc. | `Query.eq`, `Query.gt`, etc. |
| Builders | Object literals | Computation expressions |
| Validation | Automatic on CRUD | Explicit via `Collection.validate` |
| IDs | ULID strings | ULID strings |
| Async | Promises | `Task<'T>` |
| Transactions | Callback-based | Computation expression |

## Performance Tips

1. **Create indexes** on frequently queried fields
2. **Use composite indexes** for multi-field queries
3. **Enable WAL mode** (default) for better concurrency
4. **Batch operations** when inserting/updating many documents
5. **Use transactions** for multiple related operations
6. **Avoid SELECT \*** with projections to reduce payload size
7. **Use cursor pagination** for large result sets

## Testing

```bash
# Run all tests
dotnet test FractalDb.slnx

# Run specific test file
dotnet test --filter "FullyQualifiedName~CrudTests"

# With detailed output
dotnet test FractalDb.slnx --verbosity normal
```

## Building from Source

```bash
# Clone repository
git clone https://github.com/g5becks/StrataDB.git
cd StrataDB

# Build
dotnet build FractalDb.slnx

# Run tests
dotnet test FractalDb.slnx

# Run linter
cd /Users/takinprofit/Dev/StrataDB && task lint

# Format code
cd /Users/takinprofit/Dev/StrataDB && task fmt
```

## Project Structure

```
/StrataDB/
├── src/                    # Source code
│   ├── Types.fs            # Core document types
│   ├── Errors.fs           # Error handling
│   ├── Operators.fs        # Query operators
│   ├── Query.fs            # Query helpers
│   ├── Schema.fs           # Schema definitions
│   ├── Options.fs          # Query options
│   ├── Serialization.fs    # JSON serialization
│   ├── SqlTranslator.fs    # Query translation
│   ├── TableBuilder.fs     # Schema management
│   ├── Transaction.fs      # Transactions
│   ├── Collection.fs       # Collection operations
│   ├── Database.fs         # Database management
│   ├── Builders.fs         # Computation expressions
│   └── Library.fs          # Public API
├── tests/                  # Test suite
│   ├── Assertions.fs       # Test helpers
│   ├── QueryTests.fs       # Query construction tests
│   ├── SerializationTests.fs
│   ├── SqlTranslatorTests.fs
│   ├── CrudTests.fs
│   ├── QueryExecutionTests.fs
│   ├── TransactionTests.fs
│   ├── BatchTests.fs
│   ├── AtomicTests.fs
│   └── ValidationTests.fs
└── FractalDb.slnx         # Solution file
```

## License

MIT License - see LICENSE file for details

## Contributing

Contributions welcome! Please:

1. Read `FSHARP_PORT_DESIGN.md` for architecture details
2. Follow existing code style (use `task fmt`)
3. Add tests for new features
4. Ensure `task check` passes (format + lint + build)
5. Update documentation

## Acknowledgments

- **StrataDB** - Original TypeScript implementation by [@g5becks](https://github.com/g5becks)
- **SQLite** - Rock-solid embedded database engine
- **F# Community** - For excellent libraries and tools

## Support

- 🐛 **Issues**: [GitHub Issues](https://github.com/g5becks/StrataDB/issues)
- 💬 **Discussions**: [GitHub Discussions](https://github.com/g5becks/StrataDB/discussions)
- 📖 **TypeScript Docs**: [StrataDB Documentation](https://g5becks.github.io/StrataDB/)

---

**FractalDb** brings the elegance of MongoDB-style document databases to the .NET ecosystem with F# type safety and functional programming principles. Perfect for embedded scenarios, testing, and applications that need zero-configuration data persistence.
