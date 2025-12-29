module FractalDb.Tests.QueryExprTests

// Suppress linter warnings for test code
// fsharplint:disable FL0072

open System
open Xunit
open FsUnit.Xunit
open FractalDb.Types
open FractalDb.Schema
open FractalDb.QueryExpr
open FractalDb.Operators
open FractalDb.Collection
open FractalDb.Database

/// <summary>
/// Integration tests for QueryExpr module - query expression translation.
/// Tests verify that query expressions correctly translate to TranslatedQuery structures
/// with proper Query<'T> predicates using real Collection<'T> objects.
/// </summary>

// Test type for query expressions
type TestUser = {
    Name: string
    Email: string
    Age: int64
    Active: bool
}

// Schema definition for the test collection
let testUserSchema: SchemaDef<TestUser> = {
    Fields = [
        { Name = "name"; Path = None; SqlType = SqliteType.Text; Indexed = true; Unique = false; Nullable = false }
        { Name = "email"; Path = None; SqlType = SqliteType.Text; Indexed = true; Unique = true; Nullable = false }
        { Name = "age"; Path = None; SqlType = SqliteType.Integer; Indexed = true; Unique = false; Nullable = false }
        { Name = "active"; Path = None; SqlType = SqliteType.Integer; Indexed = false; Unique = false; Nullable = false }
    ]
    Indexes = []
    Timestamps = true
    Validate = None
}

/// <summary>
/// Test fixture providing shared in-memory database and test collection.
/// </summary>
type QueryExprTestFixture() =
    let db = FractalDb.InMemory()
    let users = db.Collection<TestUser>("users", testUserSchema)
    
    member _.Db = db
    member _.Users = users
    
    interface IDisposable with
        member _.Dispose() = db.Close()

/// <summary>
/// QueryExpr integration test suite using real Collection<'T> objects.
/// </summary>
type QueryExprTests(fixture: QueryExprTestFixture) =
    let users = fixture.Users  // Real Collection<TestUser>
    
    interface IClassFixture<QueryExprTestFixture>
    
    // ═══════════════════════════════════════════════════════════════
    // Basic Predicate Tests - Equality
    // ═══════════════════════════════════════════════════════════════
    
    [<Fact>]
    member _.``Query expression with equality translates to Eq operator`` () =
        let result = query {
            for user in users do
            where (user.Age = 25L)
        }
        
        // Verify source
        result.Source |> should equal "users"
        
        // Verify Where clause exists
        result.Where |> should not' (equal None)
        
        // Verify it's a Field query with Compare operator
        match result.Where with
        | Some (Query.Field(field, FieldOp.Compare op)) ->
            field |> should equal "age"
            // CompareOp is created with obj type, so unbox to CompareOp<obj> then cast inner value
            let unboxed = unbox<CompareOp<obj>> op
            match unboxed with
            | CompareOp.Eq value -> (value :?> int64) |> should equal 25L
            | _ -> failwith "Expected CompareOp.Eq"
        | _ -> failwith "Expected Query.Field with Compare operator"
    
    [<Fact>]
    member _.``Query expression with string equality translates correctly`` () =
        let result = query {
            for user in users do
            where (user.Name = "Alice")
        }
        
        result.Source |> should equal "users"
        result.Where |> should not' (equal None)
        
        match result.Where with
        | Some (Query.Field(field, FieldOp.Compare op)) ->
            field |> should equal "name"
            let unboxed = unbox<CompareOp<obj>> op
            match unboxed with
            | CompareOp.Eq value -> (value :?> string) |> should equal "Alice"
            | _ -> failwith "Expected CompareOp.Eq"
        | _ -> failwith "Expected Query.Field with Compare operator"
    
    [<Fact>]
    member _.``Query expression with bool equality translates correctly`` () =
        let result = query {
            for user in users do
            where (user.Active = true)
        }
        
        result.Source |> should equal "users"
        
        match result.Where with
        | Some (Query.Field(field, FieldOp.Compare op)) ->
            field |> should equal "active"
            let unboxed = unbox<CompareOp<obj>> op
            match unboxed with
            | CompareOp.Eq value -> (value :?> bool) |> should equal true
            | _ -> failwith "Expected CompareOp.Eq"
        | _ -> failwith "Expected Query.Field with Compare operator"
    
    // ═══════════════════════════════════════════════════════════════
    // Basic Predicate Tests - Inequality
    // ═══════════════════════════════════════════════════════════════
    
    [<Fact>]
    member _.``Query expression with inequality translates to Ne operator`` () =
        let result = query {
            for user in users do
            where (user.Age <> 25L)
        }
        
        result.Source |> should equal "users"
        
        match result.Where with
        | Some (Query.Field(field, FieldOp.Compare op)) ->
            field |> should equal "age"
            let unboxed = unbox<CompareOp<obj>> op
            match unboxed with
            | CompareOp.Ne value -> (value :?> int64) |> should equal 25L
            | _ -> failwith "Expected CompareOp.Ne"
        | _ -> failwith "Expected Query.Field with Compare operator"

    [<Fact>]
    member _.``Query expression with string inequality translates correctly`` () =
        let result = query {
            for user in users do
            where (user.Name <> "Bob")
        }

        result.Source |> should equal "users"

        match result.Where with
        | Some (Query.Field(field, FieldOp.Compare op)) ->
            field |> should equal "name"
            let unboxed = unbox<CompareOp<obj>> op
            match unboxed with
            | CompareOp.Ne value -> (value :?> string) |> should equal "Bob"
            | _ -> failwith "Expected CompareOp.Ne"
        | _ -> failwith "Expected Query.Field with Compare operator"
    
    // ═══════════════════════════════════════════════════════════════
    // Basic Predicate Tests - Greater Than
    // ═══════════════════════════════════════════════════════════════
    
    [<Fact>]
    member _.``Query expression with greater than translates to Gt operator`` () =
        let result = query {
            for user in users do
            where (user.Age > 18L)
        }
        
        result.Source |> should equal "users"
        
        match result.Where with
        | Some (Query.Field(field, FieldOp.Compare op)) ->
            field |> should equal "age"
            let unboxed = unbox<CompareOp<obj>> op
            match unboxed with
            | CompareOp.Gt value -> (value :?> int64) |> should equal 18L
            | _ -> failwith "Expected CompareOp.Gt"
        | _ -> failwith "Expected Query.Field with Compare operator"
    
    // ═══════════════════════════════════════════════════════════════
    // Basic Predicate Tests - Greater Than or Equal
    // ═══════════════════════════════════════════════════════════════
    
    [<Fact>]
    member _.``Query expression with greater than or equal translates to Gte operator`` () =
        let result = query {
            for user in users do
            where (user.Age >= 18L)
        }
        
        result.Source |> should equal "users"
        
        match result.Where with
        | Some (Query.Field(field, FieldOp.Compare op)) ->
            field |> should equal "age"
            let unboxed = unbox<CompareOp<obj>> op
            match unboxed with
            | CompareOp.Gte value -> (value :?> int64) |> should equal 18L
            | _ -> failwith "Expected CompareOp.Gte"
        | _ -> failwith "Expected Query.Field with Compare operator"
    
    // ═══════════════════════════════════════════════════════════════
    // Basic Predicate Tests - Less Than
    // ═══════════════════════════════════════════════════════════════
    
    [<Fact>]
    member _.``Query expression with less than translates to Lt operator`` () =
        let result = query {
            for user in users do
            where (user.Age < 65L)
        }
        
        result.Source |> should equal "users"
        
        match result.Where with
        | Some (Query.Field(field, FieldOp.Compare op)) ->
            field |> should equal "age"
            let unboxed = unbox<CompareOp<obj>> op
            match unboxed with
            | CompareOp.Lt value -> (value :?> int64) |> should equal 65L
            | _ -> failwith "Expected CompareOp.Lt"
        | _ -> failwith "Expected Query.Field with Compare operator"

    // ═══════════════════════════════════════════════════════════════
    // Basic Predicate Tests - Less Than or Equal
    // ═══════════════════════════════════════════════════════════════

    [<Fact>]
    member _.``Query expression with less than or equal translates to Lte operator`` () =
        let result = query {
            for user in users do
            where (user.Age <= 65L)
        }

        result.Source |> should equal "users"

        match result.Where with
        | Some (Query.Field(field, FieldOp.Compare op)) ->
            field |> should equal "age"
            let unboxed = unbox<CompareOp<obj>> op
            match unboxed with
            | CompareOp.Lte value -> (value :?> int64) |> should equal 65L
            | _ -> failwith "Expected CompareOp.Lte"
        | _ -> failwith "Expected Query.Field with Compare operator"
    
    // ═══════════════════════════════════════════════════════════════
    // Edge Cases - Source Extraction
    // ═══════════════════════════════════════════════════════════════
    
    [<Fact>]
    member _.``Query expression extracts correct source collection name`` () =
        let result = query {
            for user in users do
            where (user.Age > 0L)
        }
        
        result.Source |> should equal "users"
    
    [<Fact>]
    member _.``Query expression with no where clause extracts source`` () =
        let result = query {
            for user in users do
            select user
        }
        
        // Even without a where clause, source should be extracted
        result.Source |> should equal "users"
        result.Where |> should equal None
