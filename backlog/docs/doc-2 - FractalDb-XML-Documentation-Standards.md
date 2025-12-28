---
id: doc-2
title: FractalDb XML Documentation Standards
type: standard
created_date: '2025-12-28'
---
# FractalDb XML Documentation Standards

## Overview

ALL code in FractalDb MUST include proper XML documentation comments to enable API documentation generation using `fsdocs`. This is a **mandatory requirement** for every task that implements types, functions, modules, or any public API.

## Why This Matters

1. **API Documentation**: `fsdocs` generates HTML documentation from XML comments
2. **IDE Support**: XML comments provide IntelliSense/autocomplete documentation
3. **Discoverability**: Users can understand the API without reading source code
4. **Examples**: Code examples in docs help users get started quickly

## Project Configuration

The FractalDb.fsproj MUST include:

```xml
<PropertyGroup>
  <GenerateDocumentationFile>true</GenerateDocumentationFile>
</PropertyGroup>
```

## Required Documentation Elements

### For Types (Records, Discriminated Unions, Classes)

```fsharp
/// <summary>
/// Brief description of what this type represents.
/// </summary>
///
/// <example>
/// <code>
/// let meta = { Id = "abc123"; Version = 1L; CreatedAt = DateTime.UtcNow; UpdatedAt = None }
/// </code>
/// </example>
type DocumentMeta = { ... }
```

### For Discriminated Union Cases

```fsharp
/// <summary>
/// Represents query comparison operators.
/// </summary>
type CompareOp =
    /// <summary>Equals comparison ($eq)</summary>
    | Eq of JsonValue
    /// <summary>Not equals comparison ($ne)</summary>
    | Ne of JsonValue
    /// <summary>Greater than comparison ($gt)</summary>
    | Gt of JsonValue
```

### For Functions

```fsharp
/// <summary>
/// Creates a new document with auto-generated ID and timestamps.
/// </summary>
///
/// <param name="data">The user data to wrap in a document.</param>
///
/// <returns>A new Document with generated metadata.</returns>
///
/// <example>
/// <code>
/// let doc = Document.create { Name = "Test"; Value = 42 }
/// // doc.Meta.Id is auto-generated
/// // doc.Meta.CreatedAt is set to current UTC time
/// </code>
/// </example>
let create (data: 'T) : Document<'T> = ...
```

### For Modules

```fsharp
/// <summary>
/// Provides functions for creating and manipulating documents.
/// </summary>
///
/// <namespacedoc>
///   <summary>Core FractalDb document database functionality.</summary>
/// </namespacedoc>
module Document =
```

### For Record Fields (when non-obvious)

```fsharp
type QueryOptions = {
    /// <summary>Maximum number of results to return. None means no limit.</summary>
    Limit: int option
    /// <summary>Number of results to skip for pagination.</summary>
    Skip: int option
    /// <summary>Fields to sort by with direction.</summary>
    Sort: (string * SortDirection) list
}
```

## Documentation Quality Standards

### DO:
- Write summaries in third person ("Creates...", "Represents...", "Returns...")
- Include `<example>` blocks for all public API functions
- Use `<param>` for every parameter
- Use `<returns>` to describe return values
- Use `<exception>` when functions can throw
- Keep summaries concise but informative

### DON'T:
- Leave any public type/function undocumented
- Write vague summaries like "Does something"
- Skip examples for complex functions
- Use first person ("I create...", "We return...")

## Examples of Good Documentation

### Type with Multiple Cases

```fsharp
/// <summary>
/// Represents errors that can occur during FractalDb operations.
/// </summary>
///
/// <remarks>
/// All FractalDb operations return <c>FractalResult&lt;'T&gt;</c> which wraps
/// either a success value or a <c>FractalError</c>.
/// </remarks>
type FractalError =
    /// <summary>
    /// Document with the specified ID was not found.
    /// </summary>
    | NotFound of id: string
    
    /// <summary>
    /// A validation error occurred.
    /// </summary>
    /// <param name="field">The field that failed validation.</param>
    /// <param name="message">Description of the validation failure.</param>
    | ValidationError of field: string * message: string
    
    /// <summary>
    /// A database operation failed.
    /// </summary>
    /// <param name="operation">The operation that failed (e.g., "insert", "update").</param>
    /// <param name="message">Error message from the database.</param>
    | DatabaseError of operation: string * message: string
```

### Function with Full Documentation

```fsharp
/// <summary>
/// Finds a single document matching the specified query.
/// </summary>
///
/// <param name="query">The query to match documents against.</param>
///
/// <returns>
/// <c>Ok (Some doc)</c> if a matching document is found,
/// <c>Ok None</c> if no match, or <c>Error e</c> on failure.
/// </returns>
///
/// <exception cref="T:System.ArgumentException">
/// Thrown if query contains invalid field paths.
/// </exception>
///
/// <example>
/// <code>
/// let result = collection.findOne (Query.eq "status" "active")
/// match result with
/// | Ok (Some doc) -> printfn "Found: %A" doc.Data
/// | Ok None -> printfn "No match"
/// | Error e -> printfn "Error: %s" e.Message
/// </code>
/// </example>
///
/// <seealso cref="M:FractalDb.Collection.find"/>
/// <seealso cref="M:FractalDb.Collection.findById"/>
let findOne (query: Query) : FractalResult<Document<'T> option> = ...
```

## Verification

Every implementation task should verify:

1. All public types have `<summary>` documentation
2. All public functions have `<summary>`, `<param>`, `<returns>`, and `<example>`
3. Build produces no XML documentation warnings
4. Running `dotnet build` generates the `.xml` file alongside the assembly

## Cross-References

Use `<see cref="..."/>` and `<seealso cref="..."/>` to link related items:

- Types: `<see cref="T:FractalDb.Document`1"/>`
- Methods: `<see cref="M:FractalDb.Collection.findOne"/>`
- Properties: `<see cref="P:FractalDb.DocumentMeta.Id"/>`

## Integration with fsdocs

To generate documentation:

```bash
dotnet tool install fsdocs-tool
dotnet fsdocs build
```

Output will be in `output/reference/` by default.
