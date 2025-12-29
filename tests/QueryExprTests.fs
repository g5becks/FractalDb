module FractalDb.Tests.QueryExprTests

// Suppress linter warnings for test code
// fsharplint:disable FL0072

open System
open System.Threading.Tasks
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
type TestUser =
    { Name: string
      Email: string
      Age: int64
      Active: bool }

// Schema definition for the test collection
let testUserSchema: SchemaDef<TestUser> =
    { Fields =
        [ { Name = "name"
            Path = None
            SqlType = SqliteType.Text
            Indexed = true
            Unique = false
            Nullable = false }
          { Name = "email"
            Path = None
            SqlType = SqliteType.Text
            Indexed = true
            Unique = true
            Nullable = false }
          { Name = "age"
            Path = None
            SqlType = SqliteType.Integer
            Indexed = true
            Unique = false
            Nullable = false }
          { Name = "active"
            Path = None
            SqlType = SqliteType.Integer
            Indexed = false
            Unique = false
            Nullable = false } ]
      Indexes = []
      Timestamps = true
      Validate = None }

/// <summary>
/// Test fixture providing shared in-memory database and test collection.
/// </summary>
type QueryExprTestFixture() =
    let db = FractalDb.InMemory()
    let users = db.Collection<TestUser>("users", testUserSchema)

    // Seed test data for aggregation tests
    do
        let testUsers =
            [ { Name = "Alice"
                Email = "alice@test.com"
                Age = 25L
                Active = true }
              { Name = "Bob"
                Email = "bob@test.com"
                Age = 30L
                Active = true }
              { Name = "Charlie"
                Email = "charlie@test.com"
                Age = 35L
                Active = false } ]

        testUsers
        |> List.iter (fun user ->
            users |> Collection.insertOne user |> Async.AwaitTask |> Async.RunSynchronously |> ignore)

    member _.Db = db
    member _.Users = users

    interface IDisposable with
        member _.Dispose() = db.Close()

/// <summary>
/// QueryExpr integration test suite using real Collection<'T> objects.
/// </summary>
type QueryExprTests(fixture: QueryExprTestFixture) =
    let users = fixture.Users // Real Collection<TestUser>

    interface IClassFixture<QueryExprTestFixture>

    // ═══════════════════════════════════════════════════════════════
    // Basic Predicate Tests - Equality
    // ═══════════════════════════════════════════════════════════════

    [<Fact>]
    member _.``Query expression with equality translates to Eq operator``() =
        let result =
            query {
                for user in users do
                    where (user.Age = 25L)
            }

        // Verify source
        result.Source |> should equal "users"

        // Verify Where clause exists
        result.Where |> should not' (equal None)

        // Verify it's a Field query with Compare operator
        match result.Where with
        | Some(Query.Field(field, FieldOp.Compare op)) ->
            field |> should equal "age"
            // CompareOp is created with obj type, so unbox to CompareOp<obj> then cast inner value
            let unboxed = unbox<CompareOp<obj>> op

            match unboxed with
            | CompareOp.Eq value -> (value :?> int64) |> should equal 25L
            | _ -> failwith "Expected CompareOp.Eq"
        | _ -> failwith "Expected Query.Field with Compare operator"

    [<Fact>]
    member _.``Query expression with string equality translates correctly``() =
        let result =
            query {
                for user in users do
                    where (user.Name = "Alice")
            }

        result.Source |> should equal "users"
        result.Where |> should not' (equal None)

        match result.Where with
        | Some(Query.Field(field, FieldOp.Compare op)) ->
            field |> should equal "name"
            let unboxed = unbox<CompareOp<obj>> op

            match unboxed with
            | CompareOp.Eq value -> (value :?> string) |> should equal "Alice"
            | _ -> failwith "Expected CompareOp.Eq"
        | _ -> failwith "Expected Query.Field with Compare operator"

    [<Fact>]
    member _.``Query expression with bool equality translates correctly``() =
        let result =
            query {
                for user in users do
                    where (user.Active = true)
            }

        result.Source |> should equal "users"

        match result.Where with
        | Some(Query.Field(field, FieldOp.Compare op)) ->
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
    member _.``Query expression with inequality translates to Ne operator``() =
        let result =
            query {
                for user in users do
                    where (user.Age <> 25L)
            }

        result.Source |> should equal "users"

        match result.Where with
        | Some(Query.Field(field, FieldOp.Compare op)) ->
            field |> should equal "age"
            let unboxed = unbox<CompareOp<obj>> op

            match unboxed with
            | CompareOp.Ne value -> (value :?> int64) |> should equal 25L
            | _ -> failwith "Expected CompareOp.Ne"
        | _ -> failwith "Expected Query.Field with Compare operator"

    [<Fact>]
    member _.``Query expression with string inequality translates correctly``() =
        let result =
            query {
                for user in users do
                    where (user.Name <> "Bob")
            }

        result.Source |> should equal "users"

        match result.Where with
        | Some(Query.Field(field, FieldOp.Compare op)) ->
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
    member _.``Query expression with greater than translates to Gt operator``() =
        let result =
            query {
                for user in users do
                    where (user.Age > 18L)
            }

        result.Source |> should equal "users"

        match result.Where with
        | Some(Query.Field(field, FieldOp.Compare op)) ->
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
    member _.``Query expression with greater than or equal translates to Gte operator``() =
        let result =
            query {
                for user in users do
                    where (user.Age >= 18L)
            }

        result.Source |> should equal "users"

        match result.Where with
        | Some(Query.Field(field, FieldOp.Compare op)) ->
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
    member _.``Query expression with less than translates to Lt operator``() =
        let result =
            query {
                for user in users do
                    where (user.Age < 65L)
            }

        result.Source |> should equal "users"

        match result.Where with
        | Some(Query.Field(field, FieldOp.Compare op)) ->
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
    member _.``Query expression with less than or equal translates to Lte operator``() =
        let result =
            query {
                for user in users do
                    where (user.Age <= 65L)
            }

        result.Source |> should equal "users"

        match result.Where with
        | Some(Query.Field(field, FieldOp.Compare op)) ->
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
    member _.``Query expression extracts correct source collection name``() =
        let result =
            query {
                for user in users do
                    where (user.Age > 0L)
            }

        result.Source |> should equal "users"

    [<Fact>]
    member _.``Query expression with no where clause extracts source``() =
        let result =
            query {
                for user in users do
                    select user
            }

        // Even without a where clause, source should be extracted
        result.Source |> should equal "users"
        result.Where |> should equal None

    // ═══════════════════════════════════════════════════════════════
    // Logical Operators - AND
    // ═══════════════════════════════════════════════════════════════

    [<Fact>]
    member _.``Query expression with AND operator translates to Query.And``() =
        let result =
            query {
                for user in users do
                    where (user.Age >= 18L && user.Age <= 65L)
            }

        result.Source |> should equal "users"
        result.Where |> should not' (equal None)

        match result.Where with
        | Some(Query.And conditions) ->
            conditions |> should haveLength 2
            // Verify first condition (Age >= 18)
            match conditions.[0] with
            | Query.Field(field, FieldOp.Compare op) ->
                field |> should equal "age"
                let unboxed = unbox<CompareOp<obj>> op

                match unboxed with
                | CompareOp.Gte value -> (value :?> int64) |> should equal 18L
                | _ -> failwith "Expected CompareOp.Gte"
            | _ -> failwith "Expected Query.Field"
            // Verify second condition (Age <= 65)
            match conditions.[1] with
            | Query.Field(field, FieldOp.Compare op) ->
                field |> should equal "age"
                let unboxed = unbox<CompareOp<obj>> op

                match unboxed with
                | CompareOp.Lte value -> (value :?> int64) |> should equal 65L
                | _ -> failwith "Expected CompareOp.Lte"
            | _ -> failwith "Expected Query.Field"
        | _ -> failwith "Expected Query.And"

    [<Fact>]
    member _.``Multiple where clauses combine with AND``() =
        let result =
            query {
                for user in users do
                    where (user.Age >= 18L)
                    where (user.Active = true)
            }

        result.Source |> should equal "users"

        match result.Where with
        | Some(Query.And conditions) -> conditions |> should haveLength 2
        | _ -> failwith "Expected Query.And for multiple where clauses"

    // ═══════════════════════════════════════════════════════════════
    // Logical Operators - OR
    // ═══════════════════════════════════════════════════════════════

    [<Fact>]
    member _.``Query expression with OR operator translates to Query.Or``() =
        let result =
            query {
                for user in users do
                    where (user.Name = "Alice" || user.Name = "Bob")
            }

        result.Source |> should equal "users"

        match result.Where with
        | Some(Query.Or conditions) ->
            conditions |> should haveLength 2
            // Verify first condition (Name = "Alice")
            match conditions.[0] with
            | Query.Field(field, FieldOp.Compare op) ->
                field |> should equal "name"
                let unboxed = unbox<CompareOp<obj>> op

                match unboxed with
                | CompareOp.Eq value -> (value :?> string) |> should equal "Alice"
                | _ -> failwith "Expected CompareOp.Eq"
            | _ -> failwith "Expected Query.Field"
        | _ -> failwith "Expected Query.Or"

    // ═══════════════════════════════════════════════════════════════
    // Logical Operators - NOT
    // ═══════════════════════════════════════════════════════════════

    [<Fact>]
    member _.``Query expression with NOT operator translates to Query.Not``() =
        let result =
            query {
                for user in users do
                    where (not user.Active)
            }

        result.Source |> should equal "users"

        match result.Where with
        | Some(Query.Not condition) ->
            match condition with
            | Query.Field(field, FieldOp.Compare op) ->
                field |> should equal "active"
                let unboxed = unbox<CompareOp<obj>> op

                match unboxed with
                | CompareOp.Eq value -> (value :?> bool) |> should equal true
                | _ -> failwith "Expected CompareOp.Eq"
            | _ -> failwith "Expected Query.Field"
        | _ -> failwith "Expected Query.Not"

    // ═══════════════════════════════════════════════════════════════
    // Logical Operators - Nested AND/OR
    // ═══════════════════════════════════════════════════════════════

    [<Fact>]
    member _.``Query expression with nested AND OR translates correctly``() =
        let result =
            query {
                for user in users do
                    where ((user.Age >= 18L && user.Age <= 30L) || (user.Age >= 50L && user.Age <= 65L))
            }

        result.Source |> should equal "users"

        match result.Where with
        | Some(Query.Or conditions) ->
            conditions |> should haveLength 2
            // Each OR branch should be an AND
            match conditions.[0] with
            | Query.And innerConditions -> innerConditions |> should haveLength 2
            | _ -> failwith "Expected Query.And in first OR branch"

            match conditions.[1] with
            | Query.And innerConditions -> innerConditions |> should haveLength 2
            | _ -> failwith "Expected Query.And in second OR branch"
        | _ -> failwith "Expected Query.Or with nested And conditions"

    // ═══════════════════════════════════════════════════════════════
    // String Methods - Contains
    // ═══════════════════════════════════════════════════════════════

    [<Fact>]
    member _.``Query expression with String Contains translates to StringOp.Contains``() =
        let result =
            query {
                for user in users do
                    where (user.Email.Contains("@test.com"))
            }

        result.Source |> should equal "users"

        match result.Where with
        | Some(Query.Field(field, FieldOp.String op)) ->
            field |> should equal "email"

            match op with
            | StringOp.Contains value -> value |> should equal "@test.com"
            | _ -> failwith "Expected StringOp.Contains"
        | _ -> failwith "Expected Query.Field with String operator"

    // ═══════════════════════════════════════════════════════════════
    // String Methods - StartsWith
    // ═══════════════════════════════════════════════════════════════

    [<Fact>]
    member _.``Query expression with String StartsWith translates to StringOp.StartsWith``() =
        let result =
            query {
                for user in users do
                    where (user.Name.StartsWith("A"))
            }

        result.Source |> should equal "users"

        match result.Where with
        | Some(Query.Field(field, FieldOp.String op)) ->
            field |> should equal "name"

            match op with
            | StringOp.StartsWith value -> value |> should equal "A"
            | _ -> failwith "Expected StringOp.StartsWith"
        | _ -> failwith "Expected Query.Field with String operator"

    // ═══════════════════════════════════════════════════════════════
    // String Methods - EndsWith
    // ═══════════════════════════════════════════════════════════════

    [<Fact>]
    member _.``Query expression with String EndsWith translates to StringOp.EndsWith``() =
        let result =
            query {
                for user in users do
                    where (user.Email.EndsWith(".com"))
            }

        result.Source |> should equal "users"

        match result.Where with
        | Some(Query.Field(field, FieldOp.String op)) ->
            field |> should equal "email"

            match op with
            | StringOp.EndsWith value -> value |> should equal ".com"
            | _ -> failwith "Expected StringOp.EndsWith"
        | _ -> failwith "Expected Query.Field with String operator"

    // ═══════════════════════════════════════════════════════════════
    // Sorting - sortBy
    // ═══════════════════════════════════════════════════════════════

    [<Fact>]
    member _.``Query expression with sortBy translates to ascending OrderBy``() =
        let result =
            query {
                for user in users do
                    sortBy user.Name
            }

        result.Source |> should equal "users"
        result.OrderBy |> should haveLength 1
        result.OrderBy.[0] |> should equal ("name", SortDirection.Asc)

    // ═══════════════════════════════════════════════════════════════
    // Sorting - sortByDescending
    // ═══════════════════════════════════════════════════════════════

    [<Fact>]
    member _.``Query expression with sortByDescending translates to descending OrderBy``() =
        let result =
            query {
                for user in users do
                    sortByDescending user.Age
            }

        result.Source |> should equal "users"
        result.OrderBy |> should haveLength 1
        result.OrderBy.[0] |> should equal ("age", SortDirection.Desc)

    // ═══════════════════════════════════════════════════════════════
    // Sorting - thenBy
    // ═══════════════════════════════════════════════════════════════

    [<Fact>]
    member _.``Query expression with thenBy translates to secondary ascending sort``() =
        let result =
            query {
                for user in users do
                    sortBy user.Active
                    thenBy user.Name
            }

        result.Source |> should equal "users"
        result.OrderBy |> should haveLength 2
        result.OrderBy.[0] |> should equal ("active", SortDirection.Asc)
        result.OrderBy.[1] |> should equal ("name", SortDirection.Asc)

    // ═══════════════════════════════════════════════════════════════
    // Sorting - thenByDescending
    // ═══════════════════════════════════════════════════════════════

    [<Fact>]
    member _.``Query expression with thenByDescending translates to secondary descending sort``() =
        let result =
            query {
                for user in users do
                    sortBy user.Active
                    thenByDescending user.Age
            }

        result.Source |> should equal "users"
        result.OrderBy |> should haveLength 2
        result.OrderBy.[0] |> should equal ("active", SortDirection.Asc)
        result.OrderBy.[1] |> should equal ("age", SortDirection.Desc)

    // ═══════════════════════════════════════════════════════════════
    // Pagination - take
    // ═══════════════════════════════════════════════════════════════

    [<Fact>]
    member _.``Query expression with take limits results``() =
        let result =
            query {
                for user in users do
                    take 10
            }

        result.Source |> should equal "users"
        result.Take |> should equal (Some 10)

    // ═══════════════════════════════════════════════════════════════
    // Pagination - skip
    // ═══════════════════════════════════════════════════════════════

    [<Fact>]
    member _.``Query expression with skip offsets results``() =
        let result =
            query {
                for user in users do
                    skip 20
            }

        result.Source |> should equal "users"
        result.Skip |> should equal (Some 20)

    // ═══════════════════════════════════════════════════════════════
    // Pagination - skip and take together
    // ═══════════════════════════════════════════════════════════════

    [<Fact>]
    member _.``Query expression with skip and take together works correctly``() =
        let result =
            query {
                for user in users do
                    where (user.Active = true)
                    sortBy user.Name
                    skip 20
                    take 10
            }

        result.Source |> should equal "users"
        result.Skip |> should equal (Some 20)
        result.Take |> should equal (Some 10)
        result.OrderBy |> should haveLength 1
        result.OrderBy.[0] |> should equal ("name", SortDirection.Asc)

        match result.Where with
        | Some(Query.Field(field, FieldOp.Compare op)) ->
            field |> should equal "active"
            let unboxed = unbox<CompareOp<obj>> op

            match unboxed with
            | CompareOp.Eq value -> (value :?> bool) |> should equal true
            | _ -> failwith "Expected CompareOp.Eq"
        | _ -> failwith "Expected Query.Field"

    // ═══════════════════════════════════════════════════════════════
    // Projections - select entire entity
    // ═══════════════════════════════════════════════════════════════

    [<Fact>]
    member _.``Query expression with select entire entity translates to SelectAll``() =
        let result =
            query {
                for user in users do
                    where (user.Age >= 18L)
                    select user
            }

        result.Source |> should equal "users"
        result.Projection |> should equal Projection.SelectAll

    // ═══════════════════════════════════════════════════════════════
    // Projections - select single field
    // ═══════════════════════════════════════════════════════════════

    [<Fact>]
    member _.``Query expression with select single field translates to SelectSingle``() =
        let result =
            query {
                for user in users do
                    select user.Email
            }

        result.Source |> should equal "users"
        result.Projection |> should equal (Projection.SelectSingle "email")

    // ═══════════════════════════════════════════════════════════════
    // Projections - select tuple (multiple fields)
    // ═══════════════════════════════════════════════════════════════

    [<Fact>]
    member _.``Query expression with select tuple translates to SelectFields``() =
        let result =
            query {
                for user in users do
                    select (user.Name, user.Email, user.Age)
            }

        result.Source |> should equal "users"
        
        match result.Projection with
        | Projection.SelectFields fields ->
            fields |> should haveLength 3
            fields |> should contain "name"
            fields |> should contain "email"
            fields |> should contain "age"
        | _ -> failwith "Expected Projection.SelectFields"
