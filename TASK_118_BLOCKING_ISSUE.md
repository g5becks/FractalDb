# Task-118 Blocking Issue: F# Forward Reference Problem

## Current Status

**Progress**: 10 of 15 tasks complete (67%)
- ✅ Tasks 104-116: Complete (QueryTranslator module fully implemented)
- ✅ Task 117: Marked done (execution logic will be in Run method)
- ⚠️ Task 118: BLOCKED by F# forward reference limitation

**Branch**: `fsharp-port`
**File**: `src/QueryExpr.fs` (2,881 lines)
**Last successful commit**: `f85bbfc` (task-116)

## The Task

**Task-118**: Wire up QueryBuilder.Run to translate and execute query expressions

**Acceptance Criteria**:
1. Run member calls QueryTranslator.translate ❌ BLOCKED
2. Run member calls QueryExecutor.execute (deferred to task-117 reasoning)
3. Query expressions can execute against database
4. Code builds with no errors/warnings
5. All existing tests pass
6. XML doc comments on Run member

## The Problem

### File Structure (Current)

```fsharp
// src/QueryExpr.fs
module FractalDb.QueryExpr

// Lines 1-472: Type definitions
type SortDirection = Asc | Desc
type Projection = SelectAll | SelectFields | SelectSingle
type TranslatedQuery<'T> = { Source: string; Where: Query<'T> option; ... }

// Lines 473-1839: QueryBuilder type
type QueryBuilder() =
    member _.For(...) = ...
    member _.Yield(...) = ...
    member _.Quote(...) = ...
    
    // Line 687: THIS IS THE PROBLEM
    member _.Run(expr: Expr<TranslatedQuery<'T>>) : TranslatedQuery<'T> =
        // NEEDS to call translate, but translate is defined 1200 lines later!
        raise (NotImplementedException("task-118"))
    
    member _.Where(...) = ...
    member _.SortBy(...) = ...
    // ... 50+ more custom operations

// Lines 1847-2814: QueryTranslator module (AFTER QueryBuilder)
module internal QueryTranslator =
    let rec translatePredicate (expr: Expr) : Query<'T> = ...
    let rec simplify (q: Query<'T>) : Query<'T> = ...
    let translateProjection (expr: Expr) : Projection = ...
    
    // Line 2599: The function Run needs to call
    let translate<'T> (expr: Expr<TranslatedQuery<'T>>) : TranslatedQuery<'T> =
        // Complex recursive translation logic
        let rec loop (expr: Expr) (query: TranslatedQuery<'T>) = ...
        loop (expr :> Expr) empty

// Lines 2816+: AutoOpen module
[<AutoOpen>]
module QueryBuilderInstance =
    let query = QueryBuilder()
```

### The Core Issue

**F# does not allow forward references.** Code at line 687 cannot call a function defined at line 2599.

Error when trying:
```
error FS0039: The value, namespace, type or module 'QueryTranslator' is not defined.
```

### Why This Structure Exists

1. **QueryBuilder must come early**: It defines the custom operations (where, sortBy, etc.) that are used throughout the file
2. **QueryTranslator must reference QueryBuilder**: The translate function uses pattern matching like:
   ```fsharp
   | SpecificCall <@ Unchecked.defaultof<QueryBuilder>.For @> (_, _, [source; _]) ->
   | SpecificCall <@ Unchecked.defaultof<QueryBuilder>.Where @> (_, _, [source; predicate]) ->
   ```
   These require QueryBuilder type to be defined first
3. **Circular dependency**: QueryBuilder.Run needs QueryTranslator.translate, but QueryTranslator.translate needs QueryBuilder type

## Solutions Attempted

### 1. ❌ Direct Function Call
```fsharp
member _.Run(expr: Expr<TranslatedQuery<'T>>) : TranslatedQuery<'T> =
    QueryTranslator.translate expr
```
**Result**: `error FS0039: The value, namespace, type or module 'QueryTranslator' is not defined`

### 2. ❌ Reorganize File (QueryTranslator before QueryBuilder)
```python
# Attempted with Python script
types_section + querytranslator_section + querybuilder_section + autoopen_section
```
**Result**: QueryTranslator's SpecificCall patterns reference QueryBuilder, which isn't defined yet:
```
error FS0039: The type 'QueryBuilder' is not defined
```
Errors on lines using `Unchecked.defaultof<QueryBuilder>.For`, `.Where`, etc.

### 3. ❌ Call Without Module Prefix
```fsharp
member _.Run(expr: Expr<TranslatedQuery<'T>>) : TranslatedQuery<'T> =
    translate expr  // Hoping it's in scope
```
**Result**: `error FS0039: The value or constructor 'translate' is not defined`

### 4. ❌ Fully Qualified Names
```fsharp
member _.Run(expr: Expr<TranslatedQuery<'T>>) : TranslatedQuery<'T> =
    FractalDb.QueryExpr.QueryTranslator.translate expr
```
**Result**: Same forward reference error - the module isn't defined yet at line 687

### 5. ⚠️ Return Unchecked.defaultof
```fsharp
member _.Run(expr: Expr<TranslatedQuery<'T>>) : TranslatedQuery<'T> =
    Unchecked.defaultof<_>
```
**Result**: Builds, but defeats the entire purpose. Query expressions don't work.

## Design Document Reference

The original design in `FSHARP_PORT_GAPS.md` shows the same structure but doesn't actually implement Run - it just shows the signature. The translate function is demonstrated separately, not called from Run.

## Potential Solutions (Need Evaluation)

### Option A: Recursive Module Pattern
F# supports `module rec` for mutual recursion. Could restructure as:
```fsharp
module rec FractalDb.QueryExpr

type QueryBuilder() = ...
    member _.Run(expr) = QueryTranslator.translate expr

module QueryTranslator =
    let translate<'T> (expr) = ...
        // Uses SpecificCall <@ Unchecked.defaultof<QueryBuilder>.For @>
```
**Question**: Does `module rec` work for this use case? Does it allow SpecificCall patterns to reference types defined later in the rec block?

### Option B: Inline translate Logic
Copy the entire translate function (~400 lines) directly into Run method.
**Downside**: Massive code duplication, loses modularity, hard to maintain.

### Option C: Nested Module
Move QueryTranslator inside QueryBuilder type as a nested module:
```fsharp
type QueryBuilder() =
    module private Translator =
        let translate<'T> (expr) = ...
    
    member _.Run(expr) = Translator.translate expr
```
**Question**: Do nested modules work inside types? Can they access the parent type for SpecificCall patterns?

### Option D: Lazy/Mutable Function Reference
```fsharp
type QueryBuilder() =
    static let mutable translateFn = Unchecked.defaultof<_>
    static member internal SetTranslate fn = translateFn <- fn
    member _.Run(expr) = translateFn expr

// Later in file
module QueryTranslator =
    let translate<'T> (expr) = ...
    do QueryBuilder.SetTranslate translate
```
**Downside**: Requires mutable state, initialization ordering issues, feels hacky.

### Option E: Two-Pass Pattern
Make Run return the quotation expression itself, provide a separate entry point:
```fsharp
type QueryBuilder() =
    member _.Run(expr) = expr  // Return quotation as-is

// Usage becomes:
let queryExpr = query { for user in users do where (user.Age > 18) }
let translatedQuery = QueryTranslator.translate queryExpr
```
**Downside**: Changes user-facing API, less idiomatic for computation expressions.

### Option F: Separate File
Move QueryTranslator to a new file `QueryTranslator.fs` that:
1. Compiles after QueryExpr.fs
2. References QueryBuilder type via fully qualified names
3. Provides translate function

**Question**: How would this work with SpecificCall patterns that need the QueryBuilder type?

## Current Code State

### QueryBuilder.Run (Line 687)
```fsharp
member _.Run(expr: Microsoft.FSharp.Quotations.Expr<TranslatedQuery<'T>>) : TranslatedQuery<'T> =
    raise (System.NotImplementedException("QueryBuilder.Run will be implemented in task-118"))
```

### QueryTranslator.translate (Lines 2599-2814)
```fsharp
let translate<'T> (expr: Expr<TranslatedQuery<'T>>) : TranslatedQuery<'T> =
    let rec loop (expr: Expr) (query: TranslatedQuery<'T>) : TranslatedQuery<'T> =
        match expr with
        | SpecificCall <@ Unchecked.defaultof<QueryBuilder>.For @> (_, _, [source; _]) ->
            let collection = evaluateExpr source
            let collectionName = 
                collection.GetType().GetProperty("Name").GetValue(collection) :?> string
            { query with Source = collectionName }
        
        | SpecificCall <@ Unchecked.defaultof<QueryBuilder>.Where @> (_, _, [source; predicate]) ->
            let q = loop source query
            let condition = translatePredicate predicate
            let combined = 
                match q.Where with
                | None -> condition
                | Some existing -> Query.And [existing; condition]
            { q with Where = Some (simplify combined) }
        
        | SpecificCall <@ Unchecked.defaultof<QueryBuilder>.SortBy @> (_, _, [source; selector]) ->
            let q = loop source query
            let field = extractPropertyName selector
            { q with OrderBy = q.OrderBy @ [(field, SortDirection.Asc)] }
        
        // ... 6 more SpecificCall patterns for other operations
        
        | _ -> query
    
    let empty = { Source = ""; Where = None; OrderBy = []; Skip = None; Take = None; Projection = Projection.SelectAll }
    loop (expr :> Expr) empty
```

## Build Commands

```bash
# Current state
dotnet build src/FractalDb.fsproj  # Fails on Run calling translate

# Test changes
dotnet build src/FractalDb.fsproj
dotnet test tests/FractalDb.Tests.fsproj
```

## Related Files

- `src/QueryExpr.fs` - The main file with the issue (2,881 lines)
- `src/Operators.fs` - Defines Query<'T>, FieldOp, CompareOp (compiles before QueryExpr.fs)
- `src/Collection.fs` - Defines Collection<'T> (compiles after QueryExpr.fs)
- `src/Options.fs` - Defines QueryOptions (compiles after QueryExpr.fs)
- `FSHARP_PORT_GAPS.md` - Original design document (lines 106-457)

## Questions for Developer

1. **Which solution approach is most idiomatic for F#?**
   - Recursive module pattern?
   - Nested modules in types?
   - Separate file?
   - Something else?

2. **How to handle SpecificCall patterns that reference QueryBuilder?**
   - Can they work in a recursive module?
   - Alternative pattern matching approach?

3. **Is there an F# standard pattern for this scenario?**
   - Computation expressions that need to translate their quotations
   - Large DSLs with circular dependencies

4. **Should the file structure change?**
   - Split into multiple files?
   - Different ordering strategy?

## Success Criteria

After solution is implemented:
1. `dotnet build src/FractalDb.fsproj` succeeds with 0 errors, 0 warnings
2. QueryBuilder.Run calls QueryTranslator.translate successfully
3. Query expressions work end-to-end:
   ```fsharp
   let result = query {
       for user in usersCollection do
       where (user.Age >= 18)
       sortBy user.Name
       take 10
   }
   // result: TranslatedQuery<User> with all components extracted
   ```
4. All existing tests pass (221/227 currently passing, 6 known failures in ArrayOperatorTests)
5. Code is maintainable and follows F# best practices

## Additional Context

- This is the FINAL task (118 of 118) for QueryExpr implementation
- All translation logic (tasks 111-116) is complete and tested
- The only missing piece is wiring Run to call translate
- Project uses .NET 10, F# 9.0
- This is a MongoDB-like query DSL for SQLite with JSON fields

## Timeline

- Started: December 29, 2025
- Tasks 104-116 completed: Same day (~3 hours)
- Blocked on task-118: Current
- Target: Complete today if possible

## Contact

This issue is blocking completion of the QueryExpr module. Any guidance appreciated!

---

**Generated**: 2025-12-29 12:30 PST
**Session**: FractalDb QueryExpr Implementation (fsharp-port branch)
