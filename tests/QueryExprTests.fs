module FractalDb.Tests.QueryExprTests

// Suppress linter warnings for test code
// fsharplint:disable FL0072

open System
open Xunit
open FsUnit.Xunit
open FractalDb
open FractalDb.Types
open FractalDb.Schema
open FractalDb.Operators
open FractalDb.QueryExpr
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

// Test types for nested property testing
type NestedProfile = { Bio: string; Rating: int64 }

type NestedUser =
    { Name: string; Profile: NestedProfile }

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
            users
            |> Collection.insertOne user
            |> Async.AwaitTask
            |> Async.RunSynchronously
            |> ignore)

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
    // IN Operator - List.contains
    // ═══════════════════════════════════════════════════════════════

    [<Fact>]
    member _.``Query expression with List.contains translates to CompareOp.In``() =
        let validAges = [ 18L; 21L; 25L; 30L ]

        let result =
            query {
                for user in users do
                    where (List.contains user.Age validAges)
            }

        result.Source |> should equal "users"

        match result.Where with
        | Some(Query.Field(field, FieldOp.Compare op)) ->
            field |> should equal "age"

            match op :?> CompareOp<obj> with
            | CompareOp.In values ->
                values |> should haveLength 4
                values |> should contain (box 18L)
                values |> should contain (box 21L)
                values |> should contain (box 25L)
                values |> should contain (box 30L)
            | _ -> failwith "Expected CompareOp.In"
        | _ -> failwith "Expected Query.Field with Compare operator"

    [<Fact>]
    member _.``Query expression with inline List.contains translates to CompareOp.In``() =
        let result =
            query {
                for user in users do
                    where (List.contains user.Email [ "alice@test.com"; "bob@test.com" ])
            }

        result.Source |> should equal "users"

        match result.Where with
        | Some(Query.Field(field, FieldOp.Compare op)) ->
            field |> should equal "email"

            match op :?> CompareOp<obj> with
            | CompareOp.In values ->
                values |> should haveLength 2
                values |> should contain (box "alice@test.com")
                values |> should contain (box "bob@test.com")
            | _ -> failwith "Expected CompareOp.In"
        | _ -> failwith "Expected Query.Field with Compare operator"

    [<Fact>]
    member _.``Query expression with Array.contains translates to CompareOp.In``() =
        let validNames = [| "Alice"; "Bob"; "Charlie" |]

        let result =
            query {
                for user in users do
                    where (Array.contains user.Name validNames)
            }

        result.Source |> should equal "users"

        match result.Where with
        | Some(Query.Field(field, FieldOp.Compare op)) ->
            field |> should equal "name"

            match op :?> CompareOp<obj> with
            | CompareOp.In values ->
                values |> should haveLength 3
                values |> should contain (box "Alice")
                values |> should contain (box "Bob")
                values |> should contain (box "Charlie")
            | _ -> failwith "Expected CompareOp.In"
        | _ -> failwith "Expected Query.Field with Compare operator"

    // ═══════════════════════════════════════════════════════════════
    // SQL LIKE Pattern Matching - Sql.like
    // ═══════════════════════════════════════════════════════════════

    [<Fact>]
    member _.``Query expression with Sql.like percent wildcard translates to StringOp.Like``() =
        let result =
            query {
                for user in users do
                    where (Sql.like "admin%" user.Name)
            }

        result.Source |> should equal "users"

        match result.Where with
        | Some(Query.Field(field, FieldOp.String op)) ->
            field |> should equal "name"

            match op with
            | StringOp.Like pattern -> pattern |> should equal "admin%"
            | _ -> failwith "Expected StringOp.Like"
        | _ -> failwith "Expected Query.Field with String operator"

    [<Fact>]
    member _.``Query expression with Sql.like underscore wildcard translates to StringOp.Like``() =
        let result =
            query {
                for user in users do
                    where (Sql.like "_e%" user.Name)
            }

        result.Source |> should equal "users"

        match result.Where with
        | Some(Query.Field(field, FieldOp.String op)) ->
            field |> should equal "name"

            match op with
            | StringOp.Like pattern -> pattern |> should equal "_e%"
            | _ -> failwith "Expected StringOp.Like"
        | _ -> failwith "Expected Query.Field with String operator"

    [<Fact>]
    member _.``Query expression with Sql.like character set pattern translates to StringOp.Like``() =
        let result =
            query {
                for user in users do
                    where (Sql.like "[abc]%" user.Name)
            }

        result.Source |> should equal "users"

        match result.Where with
        | Some(Query.Field(field, FieldOp.String op)) ->
            field |> should equal "name"

            match op with
            | StringOp.Like pattern -> pattern |> should equal "[abc]%"
            | _ -> failwith "Expected StringOp.Like"
        | _ -> failwith "Expected Query.Field with String operator"

    // ═══════════════════════════════════════════════════════════════
    // SQL LIKE Pattern Matching - Sql.ilike (case-insensitive)
    // ═══════════════════════════════════════════════════════════════

    [<Fact>]
    member _.``Query expression with Sql.ilike translates to StringOp.ILike``() =
        let result =
            query {
                for user in users do
                    where (Sql.ilike "admin%" user.Email)
            }

        result.Source |> should equal "users"

        match result.Where with
        | Some(Query.Field(field, FieldOp.String op)) ->
            field |> should equal "email"

            match op with
            | StringOp.ILike pattern -> pattern |> should equal "admin%"
            | _ -> failwith "Expected StringOp.ILike"
        | _ -> failwith "Expected Query.Field with String operator"

    // ═══════════════════════════════════════════════════════════════
    // Distinct Operator
    // ═══════════════════════════════════════════════════════════════

    [<Fact>]
    member _.``Query expression with distinct sets Distinct = true``() =
        let result =
            query {
                for user in users do
                    distinct
            }

        result.Source |> should equal "users"
        result.Distinct |> should equal true

    [<Fact>]
    member _.``Query expression with distinct and select sets Distinct = true``() =
        let result =
            query {
                for user in users do
                    select user.Name
                    distinct
            }

        result.Source |> should equal "users"
        result.Distinct |> should equal true
        result.Projection |> should equal (Projection.SelectSingle "name")

    [<Fact>]
    member _.``Query expression without distinct has Distinct = false``() =
        let result =
            query {
                for user in users do
                    where (user.Age > 18L)
            }

        result.Source |> should equal "users"
        result.Distinct |> should equal false

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
                    select ((user: TestUser).Name, user.Email, user.Age)
            }

        result.Source |> should equal "users"

        match result.Projection with
        | Projection.SelectFields fields ->
            fields |> should haveLength 3
            fields |> should contain "name"
            fields |> should contain "email"
            fields |> should contain "age"
        | _ -> failwith "Expected Projection.SelectFields"

    // ═══════════════════════════════════════════════════════════════
    // Edge Cases - Empty Collection
    // ═══════════════════════════════════════════════════════════════

    [<Fact>]
    member _.``Query expression on empty collection returns empty results``() =
        // Create a new empty collection
        let db = fixture.Db
        let emptyUsers = db.Collection<TestUser>("empty_users", testUserSchema)

        let result =
            query {
                for user in emptyUsers do
                    where (user.Active = true)
            }

        // Translation should still work
        result.Source |> should equal "empty_users"
        result.Where |> should not' (equal None)

    // ═══════════════════════════════════════════════════════════════
    // Integration - Query Expression vs Query.field Equivalence
    // ═══════════════════════════════════════════════════════════════

    [<Fact>]
    member _.``Query expression produces same predicate as Query.field``() =
        // Query expression approach
        let exprQuery =
            query {
                for user in users do
                    where (user.Age >= 25L)
            }

        // Both approaches should produce equivalent Where predicates
        exprQuery.Where |> should not' (equal None)

        // Verify the predicate structure matches expected format
        match exprQuery.Where with
        | Some(Query.Field(name, FieldOp.Compare op)) ->
            name |> should equal "age"
            let unboxed = unbox<CompareOp<obj>> op

            match unboxed with
            | CompareOp.Gte value -> (value :?> int64) |> should equal 25L
            | _ -> failwith "Expected CompareOp.Gte"
        | _ -> failwith "Expected Query.Field with Compare"

    // ═══════════════════════════════════════════════════════════════
    // Integration - Query Expression with Transactions
    // ═══════════════════════════════════════════════════════════════

    [<Fact>]
    member _.``Query expression works within transaction context``() =
        let db = fixture.Db

        // Create query expression
        let activeUsersQuery =
            query {
                for user in users do
                    where (user.Active = true)
            }

        // Verify query translates correctly even in transaction context
        activeUsersQuery.Source |> should equal "users"
        activeUsersQuery.Where |> should not' (equal None)

        match activeUsersQuery.Where with
        | Some(Query.Field(name, FieldOp.Compare op)) ->
            name |> should equal "active"
            let unboxed = unbox<CompareOp<obj>> op

            match unboxed with
            | CompareOp.Eq value -> (value :?> bool) |> should equal true
            | _ -> failwith "Expected CompareOp.Eq"
        | _ -> failwith "Expected Query.Field"

    // ═══════════════════════════════════════════════════════════════
    // Edge Cases - Nested Property Access
    // ═══════════════════════════════════════════════════════════════

    [<Fact>]
    member _.``Query expression with nested property access works``() =
        let nestedSchema: SchemaDef<NestedUser> =
            { Fields =
                [ { Name = "name"
                    Path = None
                    SqlType = SqliteType.Text
                    Indexed = true
                    Unique = false
                    Nullable = false }
                  { Name = "bio"
                    Path = Some "$.profile.bio"
                    SqlType = SqliteType.Text
                    Indexed = true
                    Unique = false
                    Nullable = false }
                  { Name = "rating"
                    Path = Some "$.profile.rating"
                    SqlType = SqliteType.Integer
                    Indexed = true
                    Unique = false
                    Nullable = false } ]
              Indexes = []
              Timestamps = false
              Validate = None }

        let db = fixture.Db
        let nestedUsers = db.Collection<NestedUser>("nested_users", nestedSchema)

        let result =
            query {
                for user in nestedUsers do
                    where (user.Profile.Rating > 50L)
            }

        // Verify translation extracts nested property correctly
        result.Source |> should equal "nested_users"
        result.Where |> should not' (equal None)

        match result.Where with
        | Some(Query.Field(name, FieldOp.Compare op)) ->
            // Should extract full nested path "profile.rating" from Profile.Rating
            name |> should equal "profile.rating"
            let unboxed = unbox<CompareOp<obj>> op

            match unboxed with
            | CompareOp.Gt value -> (value :?> int64) |> should equal 50L
            | _ -> failwith "Expected CompareOp.Gt"
        | _ -> failwith "Expected Query.Field"

    // ═══════════════════════════════════════════════════════════════
    // Edge Cases - Deeply Nested Boolean Expressions
    // ═══════════════════════════════════════════════════════════════

    [<Fact>]
    member _.``Query expression with deeply nested boolean expressions translates correctly``() =
        let result =
            query {
                for user in users do
                    where (
                        (user.Age > 20L && user.Age < 40L)
                        && (user.Active = true || user.Name = "Charlie")
                    )
            }

        result.Source |> should equal "users"
        result.Where |> should not' (equal None)

        // Verify complex AND/OR nesting is preserved
        match result.Where with
        | Some(Query.And queries) ->
            queries |> should haveLength 2

            // First part: (age > 20 && age < 40)
            match queries.[0] with
            | Query.And innerQueries -> innerQueries |> should haveLength 2
            | _ -> failwith "Expected nested And"

            // Second part: (active = true || name = "Charlie")
            match queries.[1] with
            | Query.Or innerQueries -> innerQueries |> should haveLength 2
            | _ -> failwith "Expected Or"
        | _ -> failwith "Expected Query.And at top level"

    // ═══════════════════════════════════════════════════════════════
    // Collection.exec and TranslatedQuery.exec Tests
    // ═══════════════════════════════════════════════════════════════

    [<Fact>]
    member _.``TranslatedQuery.exec returns all documents when no filter specified``() =
        task {
            let queryExpr =
                query {
                    for user in users do
                        ()
                }

            let! results = queryExpr.exec users

            results |> should haveLength 3
        }

    [<Fact>]
    member _.``TranslatedQuery.exec with where clause filters correctly``() =
        task {
            let queryExpr =
                query {
                    for user in users do
                        where (user.Active = true)
                }

            let! results = queryExpr.exec users

            results |> should haveLength 2

            results |> List.forall (fun doc -> doc.Data.Active) |> should equal true

            results
            |> List.map (fun doc -> doc.Data.Name)
            |> List.sort
            |> should equal [ "Alice"; "Bob" ]
        }

    [<Fact>]
    member _.``TranslatedQuery.exec with multiple where clauses combines with AND``() =
        task {
            let queryExpr =
                query {
                    for user in users do
                        where (user.Active = true)
                        where (user.Age >= 30L)
                }

            let! results = queryExpr.exec users

            results |> should haveLength 1
            results.[0].Data.Name |> should equal "Bob"
            results.[0].Data.Age |> should equal 30L
        }

    [<Fact>]
    member _.``TranslatedQuery.exec with sortBy sorts ascending``() =
        task {
            let queryExpr =
                query {
                    for user in users do
                        sortBy user.Age
                }

            let! results = queryExpr.exec users

            results |> should haveLength 3

            let ages = results |> List.map (fun doc -> doc.Data.Age)

            ages |> should equal [ 25L; 30L; 35L ]
        }

    [<Fact>]
    member _.``TranslatedQuery.exec with sortByDescending sorts descending``() =
        task {
            let queryExpr =
                query {
                    for user in users do
                        sortByDescending user.Age
                }

            let! results = queryExpr.exec users

            results |> should haveLength 3

            let ages = results |> List.map (fun doc -> doc.Data.Age)

            ages |> should equal [ 35L; 30L; 25L ]
        }

    [<Fact>]
    member _.``TranslatedQuery.exec with take limits results``() =
        task {
            let queryExpr =
                query {
                    for user in users do
                        take 2
                }

            let! results = queryExpr.exec users

            results |> should haveLength 2
        }

    [<Fact>]
    member _.``TranslatedQuery.exec with skip and take for pagination``() =
        task {
            let queryExpr =
                query {
                    for user in users do
                        sortBy user.Age
                        skip 1
                        take 1
                }

            let! results = queryExpr.exec users

            results |> should haveLength 1
            results.[0].Data.Age |> should equal 30L
        }

    [<Fact>]
    member _.``TranslatedQuery.exec with complex query works end-to-end``() =
        task {
            let queryExpr =
                query {
                    for user in users do
                        where (user.Age >= 25L)
                        sortByDescending user.Age
                        skip 1
                        take 1
                }

            let! results = queryExpr.exec users

            results |> should haveLength 1
            results.[0].Data.Name |> should equal "Bob"
            results.[0].Data.Age |> should equal 30L
        }

    [<Fact>]
    member _.``TranslatedQuery.exec method and Collection.exec function produce same results``() =
        task {
            let queryExpr =
                query {
                    for user in users do
                        where (user.Active = true)
                        sortBy user.Name
                }

            let! moduleResults = users |> Collection.exec queryExpr
            let! fluentResults = queryExpr.exec users

            moduleResults |> should haveLength 2
            fluentResults |> should haveLength 2

            let moduleNames = moduleResults |> List.map (fun doc -> doc.Data.Name)

            let fluentNames = fluentResults |> List.map (fun doc -> doc.Data.Name)

            moduleNames |> should equal fluentNames
        }

    [<Fact>]
    member _.``TranslatedQuery.exec with List.contains IN query works end-to-end``() =
        task {
            let targetAges = [ 25L; 30L ]

            let queryExpr =
                query {
                    for user: TestUser in users do
                        where (List.contains user.Age targetAges)
                        sortBy user.Name
                }

            let! results = queryExpr.exec users

            // Alice (25) and Bob (30) should match, Charlie (20) should not
            results |> should haveLength 2
            results.[0].Data.Name |> should equal "Alice"
            results.[1].Data.Name |> should equal "Bob"
        }

    [<Fact>]
    member _.``TranslatedQuery.exec with Array.contains IN query works end-to-end``() =
        task {
            let targetNames = [| "Alice"; "Charlie" |]

            let queryExpr =
                query {
                    for user: TestUser in users do
                        where (Array.contains user.Name targetNames)
                        sortByDescending user.Age
                }

            let! results = queryExpr.exec users

            // Alice (25) and Charlie (35) should match, Bob should not
            // Sorted descending by age: Charlie (35), Alice (25)
            results |> should haveLength 2
            results.[0].Data.Name |> should equal "Charlie"
            results.[0].Data.Age |> should equal 35L
            results.[1].Data.Name |> should equal "Alice"
            results.[1].Data.Age |> should equal 25L
        }

    // ═══════════════════════════════════════════════════════════════
    // TranslatedQuery Composition Method Tests
    // ═══════════════════════════════════════════════════════════════

    [<Fact>]
    member _.``where() adds filter to empty query``() =
        task {
            let baseQuery =
                query {
                    for user in users do
                        ()
                }

            let filter = Query.Field("active", FieldOp.Compare(box (CompareOp.Eq true)))

            let! results = baseQuery.where(filter).exec users

            results |> should haveLength 2
            results |> List.forall (fun doc -> doc.Data.Active) |> should equal true
        }

    [<Fact>]
    member _.``where() combines multiple filters with AND``() =
        task {
            let baseQuery =
                query {
                    for user in users do
                        ()
                }

            let filter1 = Query.Field("active", FieldOp.Compare(box (CompareOp.Eq true)))
            let filter2 = Query.Field("age", FieldOp.Compare(box (CompareOp.Gte 30L)))

            let! results = baseQuery.where(filter1).where(filter2).exec users

            results |> should haveLength 1
            results.[0].Data.Name |> should equal "Bob"
            results.[0].Data.Age |> should equal 30L
            results.[0].Data.Active |> should equal true
        }

    [<Fact>]
    member _.``where() preserves existing query expression filters``() =
        task {
            let queryWithFilter =
                query {
                    for user in users do
                        where (user.Age >= 25L)
                }

            let additionalFilter =
                Query.Field("active", FieldOp.Compare(box (CompareOp.Eq true)))

            let! results = queryWithFilter.where(additionalFilter).exec users

            // Should have Age >= 25 AND Active = true
            results |> should haveLength 2

            results
            |> List.forall (fun doc -> doc.Data.Age >= 25L && doc.Data.Active)
            |> should equal true
        }

    [<Fact>]
    member _.``where() can be chained multiple times``() =
        task {
            let baseQuery =
                query {
                    for user in users do
                        ()
                }

            // Filters: active=true AND age>=25 AND age<=30
            // Data: Alice(25,true), Bob(30,true), Charlie(35,false)
            // Expected: Alice and Bob both match
            let! results =
                baseQuery
                    .where(Query.Field("active", FieldOp.Compare(box (CompareOp.Eq true))))
                    .where(Query.Field("age", FieldOp.Compare(box (CompareOp.Gte 25L))))
                    .where(Query.Field("age", FieldOp.Compare(box (CompareOp.Lte 30L))))
                    .exec
                    users

            results |> should haveLength 2
            let names = results |> List.map (fun doc -> doc.Data.Name) |> List.sort
            names |> should equal [ "Alice"; "Bob" ]
        }

    [<Fact>]
    member _.``orderBy() adds sorting to empty query``() =
        task {
            let baseQuery =
                query {
                    for user in users do
                        ()
                }

            let! results = baseQuery.orderBy("age", FractalDb.QueryExpr.SortDirection.Asc).exec users

            results |> should haveLength 3
            let ages = results |> List.map (fun doc -> doc.Data.Age)
            ages |> should equal [ 25L; 30L; 35L ]
        }

    [<Fact>]
    member _.``orderBy() can sort descending``() =
        task {
            let baseQuery =
                query {
                    for user in users do
                        ()
                }

            let! results = baseQuery.orderBy("age", FractalDb.QueryExpr.SortDirection.Desc).exec users

            results |> should haveLength 3
            let ages = results |> List.map (fun doc -> doc.Data.Age)
            ages |> should equal [ 35L; 30L; 25L ]
        }

    [<Fact>]
    member _.``orderBy() can be chained for multi-field sorting``() =
        task {
            // Insert users with same age to test secondary sort
            let! insertResult =
                users
                |> Collection.insertOne
                    { Name = "Zack"
                      Email = "zack@test.com"
                      Age = 30L
                      Active = true }

            try
                let baseQuery =
                    query {
                        for user in users do
                            ()
                    }

                let! results =
                    baseQuery
                        .orderBy("age", FractalDb.QueryExpr.SortDirection.Asc)
                        .orderBy("name", FractalDb.QueryExpr.SortDirection.Asc)
                        .exec
                        users

                results |> should not' (be Empty)
                // Age 30: Bob, Zack (alphabetical)
                let age30Users = results |> List.filter (fun doc -> doc.Data.Age = 30L)
                age30Users |> should haveLength 2
                age30Users.[0].Data.Name |> should equal "Bob"
                age30Users.[1].Data.Name |> should equal "Zack"
            finally
                // Clean up: delete Zack to avoid polluting other tests
                match insertResult with
                | Ok doc ->
                    users
                    |> Collection.deleteById doc.Id
                    |> Async.AwaitTask
                    |> Async.RunSynchronously
                    |> ignore
                | Error _ -> ()
        }

    [<Fact>]
    member _.``orderBy() preserves existing query expression sorting``() =
        task {
            let queryWithSort: TranslatedQuery<TestUser> =
                query {
                    for user in users do
                        sortBy user.Name
                }

            // Adding orderBy should append to existing sorts
            let! results = queryWithSort.orderBy("age", FractalDb.QueryExpr.SortDirection.Desc).exec users

            results |> should haveLength 3
            // Should be sorted by Name first, then Age desc
            let names = results |> List.map (fun doc -> doc.Data.Name)
            names.[0] |> should equal "Alice"
        }

    [<Fact>]
    member _.``skip() sets pagination offset``() =
        task {
            let baseQuery =
                query {
                    for user in users do
                        sortBy user.Age
                }

            let! results = baseQuery.skip(1).exec users

            results |> should haveLength 2
            results.[0].Data.Age |> should equal 30L
            results.[1].Data.Age |> should equal 35L
        }

    [<Fact>]
    member _.``skip() replaces existing skip value``() =
        task {
            let queryWithSkip =
                query {
                    for user in users do
                        sortBy user.Age
                        skip 2
                }

            // Should replace skip 2 with skip 1
            let! results = queryWithSkip.skip(1).exec users

            results |> should haveLength 2
            results.[0].Data.Age |> should equal 30L
        }

    [<Fact>]
    member _.``skip() with zero returns all results``() =
        task {
            let baseQuery =
                query {
                    for user in users do
                        ()
                }

            let! results = baseQuery.skip(0).exec users

            results |> should haveLength 3
        }

    [<Fact>]
    member _.``skip() beyond result count returns empty``() =
        task {
            let baseQuery =
                query {
                    for user in users do
                        ()
                }

            let! results = baseQuery.skip(100).exec users

            results |> should be Empty
        }

    [<Fact>]
    member _.``limit() sets maximum result count``() =
        task {
            let baseQuery =
                query {
                    for user in users do
                        sortBy user.Age
                }

            let! results = baseQuery.limit(2).exec users

            results |> should haveLength 2
            results.[0].Data.Age |> should equal 25L
            results.[1].Data.Age |> should equal 30L
        }

    [<Fact>]
    member _.``limit() replaces existing take value``() =
        task {
            let queryWithTake =
                query {
                    for user in users do
                        sortBy user.Age
                        take 1
                }

            // Should replace take 1 with limit 2
            let! results = queryWithTake.limit(2).exec users

            results |> should haveLength 2
        }

    [<Fact>]
    member _.``limit() with zero returns empty``() =
        task {
            let baseQuery =
                query {
                    for user in users do
                        ()
                }

            let! results = baseQuery.limit(0).exec users

            results |> should be Empty
        }

    [<Fact>]
    member _.``limit() larger than result count returns all results``() =
        task {
            let baseQuery =
                query {
                    for user in users do
                        ()
                }

            let! results = baseQuery.limit(100).exec users

            results |> should haveLength 3
        }

    [<Fact>]
    member _.``skip() and limit() work together for pagination``() =
        task {
            let baseQuery =
                query {
                    for user in users do
                        sortBy user.Age
                }

            // Page 2, size 1
            let! results = baseQuery.skip(1).limit(1).exec users

            results |> should haveLength 1
            results.[0].Data.Age |> should equal 30L
        }

    [<Fact>]
    member _.``All composition methods can be chained together``() =
        task {
            let baseQuery =
                query {
                    for user in users do
                        ()
                }

            let! results =
                baseQuery
                    .where(Query.Field("active", FieldOp.Compare(box (CompareOp.Eq true))))
                    .orderBy("age", FractalDb.QueryExpr.SortDirection.Desc)
                    .skip(0)
                    .limit(1)
                    .exec
                    users

            results |> should haveLength 1
            results.[0].Data.Name |> should equal "Bob"
            results.[0].Data.Age |> should equal 30L
            results.[0].Data.Active |> should equal true
        }

    [<Fact>]
    member _.``Composition methods return new TranslatedQuery instances``() =
        task {
            let baseQuery =
                query {
                    for user in users do
                        ()
                }

            let filter = Query.Field("active", FieldOp.Compare(box (CompareOp.Eq true)))

            let query1 = baseQuery.where filter
            let query2 = query1.skip 1

            // Original should be unchanged
            let! baseResults = baseQuery.exec users
            baseResults |> should haveLength 3

            // query1 should have filter
            let! query1Results = query1.exec users
            query1Results |> should haveLength 2

            // query2 should have filter + skip
            let! query2Results = query2.exec users
            query2Results |> should haveLength 1
        }

    [<Fact>]
    member _.``Composition methods work with query expressions``() =
        task {
            let queryExpr =
                query {
                    for user in users do
                        where (user.Age >= 25L)
                        sortBy user.Name
                }

            // Extend with composition
            let! results =
                queryExpr.where(Query.Field("active", FieldOp.Compare(box (CompareOp.Eq true)))).limit(1).exec users

            results |> should haveLength 1
            results.[0].Data.Active |> should equal true
            results.[0].Data.Age |> should be (greaterThanOrEqualTo 25L)
        }

    // ═══════════════════════════════════════════════════════════════
    // QueryOps Pipeline Module Tests
    // ═══════════════════════════════════════════════════════════════

    [<Fact>]
    member _.``QueryOps.where adds filter with pipeline``() =
        task {
            let baseQuery =
                query {
                    for user in users do
                        ()
                }

            let filter = Query.Field("active", FieldOp.Compare(box (CompareOp.Eq true)))

            let composedQuery = baseQuery |> QueryOps.where filter

            let! results = composedQuery.exec users

            results |> should haveLength 2
            results |> List.forall (fun doc -> doc.Data.Active) |> should equal true
        }

    [<Fact>]
    member _.``QueryOps.orderBy sorts with pipeline``() =
        task {
            let baseQuery =
                query {
                    for user in users do
                        ()
                }

            let composedQuery = baseQuery |> QueryOps.orderBy "age" SortDirection.Desc

            let! results = composedQuery.exec users

            results |> should haveLength 3
            let ages = results |> List.map (fun doc -> doc.Data.Age)
            ages |> should equal [ 35L; 30L; 25L ]
        }

    [<Fact>]
    member _.``QueryOps.skip and limit paginate with pipeline``() =
        task {
            let baseQuery =
                query {
                    for user in users do
                        sortBy user.Age
                }

            let composedQuery = baseQuery |> QueryOps.skip 1 |> QueryOps.limit 1

            let! results = composedQuery.exec users

            results |> should haveLength 1
            results.[0].Data.Age |> should equal 30L
        }

    [<Fact>]
    member _.``QueryOps functions can be chained with pipeline``() =
        task {
            let baseQuery =
                query {
                    for user in users do
                        ()
                }

            let activeFilter = Query.Field("active", FieldOp.Compare(box (CompareOp.Eq true)))

            let composedQuery =
                baseQuery
                |> QueryOps.where activeFilter
                |> QueryOps.orderBy "age" SortDirection.Desc
                |> QueryOps.skip 0
                |> QueryOps.limit 1

            let! results = composedQuery.exec users

            results |> should haveLength 1
            results.[0].Data.Name |> should equal "Bob"
            results.[0].Data.Age |> should equal 30L
        }

    [<Fact>]
    member _.``QueryOps and fluent methods produce same results``() =
        task {
            let baseQuery =
                query {
                    for user in users do
                        ()
                }

            let filter = Query.Field("active", FieldOp.Compare(box (CompareOp.Eq true)))

            // Pipeline style
            let pipelineQuery =
                baseQuery
                |> QueryOps.where filter
                |> QueryOps.orderBy "name" SortDirection.Asc
                |> QueryOps.limit 1

            let! pipelineResults = pipelineQuery.exec users

            // Fluent style
            let! fluentResults = baseQuery.where(filter).orderBy("name", SortDirection.Asc).limit(1).exec users

            pipelineResults |> should haveLength 1
            fluentResults |> should haveLength 1
            pipelineResults.[0].Data.Name |> should equal fluentResults.[0].Data.Name
        }

    // ═══════════════════════════════════════════════════════════════
    // Query Expression Composition Tests (<+> operator)
    // ═══════════════════════════════════════════════════════════════

    [<Fact>]
    member _.``<+> operator composes two query expressions``() =
        task {
            let filters: TranslatedQuery<TestUser> =
                query {
                    for user in users do
                        where (user.Active = true)
                }

            let sorting: TranslatedQuery<TestUser> =
                query {
                    for user in users do
                        sortBy user.Name
                }

            let combined = filters <+> sorting
            let! results = combined.exec users

            results |> should haveLength 2
            // Should be filtered (active only) and sorted by name
            results.[0].Data.Name |> should equal "Alice"
            results.[1].Data.Name |> should equal "Bob"
        }

    [<Fact>]
    member _.``<+> operator composes three query expressions``() =
        task {
            let filters: TranslatedQuery<TestUser> =
                query {
                    for user in users do
                        where (user.Active = true)
                }

            let sorting: TranslatedQuery<TestUser> =
                query {
                    for user in users do
                        sortByDescending user.Age
                }

            let pagination: TranslatedQuery<TestUser> =
                query {
                    for user in users do
                        take 1
                }

            let combined = filters <+> sorting <+> pagination
            let! results = combined.exec users

            results |> should haveLength 1
            // Active users sorted by age desc, take 1 = Bob (age 30)
            results.[0].Data.Name |> should equal "Bob"
        }

    [<Fact>]
    member _.``<+> operator merges Where clauses with AND``() =
        task {
            let activeFilter: TranslatedQuery<TestUser> =
                query {
                    for user in users do
                        where (user.Active = true)
                }

            let ageFilter: TranslatedQuery<TestUser> =
                query {
                    for user in users do
                        where (user.Age >= 30L)
                }

            let combined = activeFilter <+> ageFilter
            let! results = combined.exec users

            // Active AND Age >= 30 = only Bob (age 30, active)
            results |> should haveLength 1
            results.[0].Data.Name |> should equal "Bob"
        }

    [<Fact>]
    member _.``<+> operator appends OrderBy clauses``() =
        task {
            // Insert another user with same age for multi-sort test
            let! insertResult =
                users
                |> Collection.insertOne
                    { Name = "Zara"
                      Email = "zara@test.com"
                      Age = 30L
                      Active = true }

            try
                let sortByAge: TranslatedQuery<TestUser> =
                    query {
                        for user in users do
                            sortBy user.Age
                    }

                let sortByName: TranslatedQuery<TestUser> =
                    query {
                        for user in users do
                            sortByDescending user.Name
                    }

                let combined = sortByAge <+> sortByName
                let! results = combined.exec users

                // Should be sorted by Age ASC, then Name DESC
                // Age 25: Alice
                // Age 30: Zara, Bob (name desc)
                // Age 35: Charlie
                let age30Users = results |> List.filter (fun d -> d.Data.Age = 30L)
                age30Users |> should haveLength 2
                age30Users.[0].Data.Name |> should equal "Zara"
                age30Users.[1].Data.Name |> should equal "Bob"
            finally
                match insertResult with
                | Ok doc ->
                    users
                    |> Collection.deleteById doc.Id
                    |> Async.AwaitTask
                    |> Async.RunSynchronously
                    |> ignore
                | Error _ -> ()
        }

    [<Fact>]
    member _.``<+> operator: later Skip/Take wins``() =
        task {
            let firstPage: TranslatedQuery<TestUser> =
                query {
                    for user in users do
                        sortBy user.Name
                        skip 0
                        take 10
                }

            let secondPage: TranslatedQuery<TestUser> =
                query {
                    for user in users do
                        skip 1
                        take 2
                }

            let combined = firstPage <+> secondPage
            let! results = combined.exec users

            // Later skip/take wins: skip 1, take 2
            results |> should haveLength 2
            // Sorted by name: Alice, Bob, Charlie -> skip 1 -> Bob, Charlie
            results.[0].Data.Name |> should equal "Bob"
            results.[1].Data.Name |> should equal "Charlie"
        }

    [<Fact>]
    member _.``compose method works same as <+> operator``() =
        task {
            let filters: TranslatedQuery<TestUser> =
                query {
                    for user in users do
                        where (user.Active = true)
                }

            let sorting: TranslatedQuery<TestUser> =
                query {
                    for user in users do
                        sortBy user.Name
                }

            // Using compose method
            let composed = filters.compose sorting
            let! (composeResults: list<Document<TestUser>>) = composed.exec users

            // Using <+> operator
            let operated = filters <+> sorting
            let! (operatorResults: list<Document<TestUser>>) = operated.exec users

            composeResults |> should haveLength 2
            operatorResults |> should haveLength 2
            composeResults.[0].Data.Name |> should equal operatorResults.[0].Data.Name
            composeResults.[1].Data.Name |> should equal operatorResults.[1].Data.Name
        }

    [<Fact>]
    member _.``Reusable query parts can be composed``() =
        task {
            // Define reusable query parts
            let activeOnly: TranslatedQuery<TestUser> =
                query {
                    for user in users do
                        where (user.Active = true)
                }

            let sortedByName: TranslatedQuery<TestUser> =
                query {
                    for user in users do
                        sortBy user.Name
                }

            let firstTwo: TranslatedQuery<TestUser> =
                query {
                    for user in users do
                        take 2
                }

            // Compose in different ways
            let! (activeNamedResults: list<Document<TestUser>>) = (activeOnly <+> sortedByName).exec users
            let! (activeLimitedResults: list<Document<TestUser>>) = (activeOnly <+> firstTwo).exec users
            let! (allThreeResults: list<Document<TestUser>>) = (activeOnly <+> sortedByName <+> firstTwo).exec users

            activeNamedResults |> should haveLength 2
            activeLimitedResults |> should haveLength 2
            allThreeResults |> should haveLength 2

            // All three combined: active, sorted by name, take 2
            allThreeResults.[0].Data.Name |> should equal "Alice"
            allThreeResults.[1].Data.Name |> should equal "Bob"
        }

    // ═══════════════════════════════════════════════════════════════
    // Integration Tests - Sql.like Pattern Matching
    // ═══════════════════════════════════════════════════════════════

    [<Fact>]
    member _.``Integration: Sql.like returns documents matching pattern``() =
        task {
            // Test data: Alice, Bob, Charlie - Alice starts with "A"
            let queryResult: TranslatedQuery<TestUser> =
                query {
                    for user in users do
                        where (Sql.like "A%" user.Name)
                }

            let! results = queryResult.exec users

            results |> should haveLength 1
            results.[0].Data.Name |> should equal "Alice"
        }

    [<Fact>]
    member _.``Integration: Sql.like with underscore wildcard works``() =
        task {
            // Test data: Alice, Bob, Charlie - Bob has 'o' as second char
            let queryResult: TranslatedQuery<TestUser> =
                query {
                    for user in users do
                        where (Sql.like "_o%" user.Name)
                }

            let! results = queryResult.exec users

            results |> should haveLength 1
            results.[0].Data.Name |> should equal "Bob"
        }

    [<Fact>]
    member _.``Integration: Sql.like with complex pattern works end-to-end``() =
        task {
            // Test data: alice@test.com, bob@test.com, charlie@test.com
            // All end with @test.com
            let queryResult: TranslatedQuery<TestUser> =
                query {
                    for user in users do
                        where (Sql.like "%@test.com" user.Email)
                }

            let! results = queryResult.exec users

            results |> should haveLength 3
        }

    // ═══════════════════════════════════════════════════════════════
    // Integration Tests - Sql.ilike Case-Insensitive Pattern Matching
    // ═══════════════════════════════════════════════════════════════

    [<Fact>]
    member _.``Integration: Sql.ilike is case-insensitive``() =
        task {
            // Test data: Alice - should match "ALICE%", "alice%", "Alice%"
            let queryResult: TranslatedQuery<TestUser> =
                query {
                    for user in users do
                        where (Sql.ilike "ALICE%" user.Name)
                }

            let! results = queryResult.exec users

            results |> should haveLength 1
            results.[0].Data.Name |> should equal "Alice"
        }

    [<Fact>]
    member _.``Integration: Sql.ilike with mixed case pattern matches``() =
        task {
            // Test data: alice@test.com - should match "ALICE@TEST.COM%"
            let queryResult: TranslatedQuery<TestUser> =
                query {
                    for user in users do
                        where (Sql.ilike "aLiCe@TeSt.CoM" user.Email)
                }

            let! results = queryResult.exec users

            results |> should haveLength 1
            results.[0].Data.Email |> should equal "alice@test.com"
        }

    // ═══════════════════════════════════════════════════════════════
    // Integration Tests - Distinct Operator
    // ═══════════════════════════════════════════════════════════════

    [<Fact>]
    member _.``Integration: distinct returns unique documents``() =
        task {
            // Test with all 3 users - distinct should return all 3 (all unique)
            let queryResult: TranslatedQuery<TestUser> =
                query {
                    for user in users do
                        distinct
                }

            let! results = queryResult.exec users

            results |> should haveLength 3
        }

    [<Fact>]
    member _.``Integration: distinct with where clause works``() =
        task {
            // Test distinct with where - should return only active users
            let queryResult: TranslatedQuery<TestUser> =
                query {
                    for user in users do
                        where (user.Active = true)
                        distinct
                }

            let! results = queryResult.exec users

            // Alice and Bob are active
            results |> should haveLength 2
        }

    [<Fact>]
    member _.``Integration: distinct with sortBy works``() =
        task {
            // Test distinct with sorting
            let queryResult: TranslatedQuery<TestUser> =
                query {
                    for user in users do
                        sortBy user.Name
                        distinct
                }

            let! results = queryResult.exec users

            results |> should haveLength 3
            // Should be sorted: Alice, Bob, Charlie
            results.[0].Data.Name |> should equal "Alice"
            results.[1].Data.Name |> should equal "Bob"
            results.[2].Data.Name |> should equal "Charlie"
        }

    // ═══════════════════════════════════════════════════════════════
    // Integration Tests - Aggregate Operators (execAggregate)
    // ═══════════════════════════════════════════════════════════════

    [<Fact>]
    member _.``Integration: execAggregate with Min returns minimum value``() =
        task {
            // Test data: Alice=25, Bob=30, Charlie=35
            let! result = users |> Collection.execAggregate (AggregateOp.Min "age") Query.Empty

            // Result is returned as obj, need to handle SQLite int64
            let minAge = System.Convert.ToInt64(result)
            minAge |> should equal 25L
        }

    [<Fact>]
    member _.``Integration: execAggregate with Max returns maximum value``() =
        task {
            // Test data: Alice=25, Bob=30, Charlie=35
            let! result = users |> Collection.execAggregate (AggregateOp.Max "age") Query.Empty

            let maxAge = System.Convert.ToInt64(result)
            maxAge |> should equal 35L
        }

    [<Fact>]
    member _.``Integration: execAggregate with Sum returns sum of values``() =
        task {
            // Test data: Alice=25, Bob=30, Charlie=35 -> Sum = 90
            let! result = users |> Collection.execAggregate (AggregateOp.Sum "age") Query.Empty

            let sumAge = System.Convert.ToInt64(result)
            sumAge |> should equal 90L
        }

    [<Fact>]
    member _.``Integration: execAggregate with Avg returns average of values``() =
        task {
            // Test data: Alice=25, Bob=30, Charlie=35 -> Avg = 30.0
            let! result = users |> Collection.execAggregate (AggregateOp.Avg "age") Query.Empty

            let avgAge = System.Convert.ToDouble(result)
            avgAge |> should equal 30.0
        }

    [<Fact>]
    member _.``Integration: execAggregate with Count returns count of documents``() =
        task {
            // Test data: 3 users total
            let! result = users |> Collection.execAggregate AggregateOp.Count Query.Empty

            let count = System.Convert.ToInt32(result)
            count |> should equal 3
        }

    [<Fact>]
    member _.``Integration: execAggregate with filter returns aggregate of filtered results``() =
        task {
            // Test data: Active users are Alice=25, Bob=30
            // Min age of active users = 25
            let activeFilter = Query.Field("active", FieldOp.Compare(box (CompareOp.Eq true)))

            let! result = users |> Collection.execAggregate (AggregateOp.Min "age") activeFilter

            let minAge = System.Convert.ToInt64(result)
            minAge |> should equal 25L
        }

    [<Fact>]
    member _.``Integration: execAggregate with Sum and filter returns sum of filtered results``() =
        task {
            // Test data: Active users are Alice=25, Bob=30
            // Sum of active users' ages = 55
            let activeFilter = Query.Field("active", FieldOp.Compare(box (CompareOp.Eq true)))

            let! result = users |> Collection.execAggregate (AggregateOp.Sum "age") activeFilter

            let sumAge = System.Convert.ToInt64(result)
            sumAge |> should equal 55L
        }

    [<Fact>]
    member _.``Integration: execAggregate on empty result set returns null for Min/Max/Sum/Avg``() =
        task {
            // Filter that matches no documents
            let noMatchFilter = Query.Field("age", FieldOp.Compare(box (CompareOp.Gt 1000L)))

            // Min on empty set returns DBNull
            let! minResult = users |> Collection.execAggregate (AggregateOp.Min "age") noMatchFilter
            minResult |> should equal System.DBNull.Value

            // Max on empty set returns DBNull
            let! maxResult = users |> Collection.execAggregate (AggregateOp.Max "age") noMatchFilter
            maxResult |> should equal System.DBNull.Value

            // Sum on empty set returns DBNull
            let! sumResult = users |> Collection.execAggregate (AggregateOp.Sum "age") noMatchFilter
            sumResult |> should equal System.DBNull.Value

            // Avg on empty set returns DBNull
            let! avgResult = users |> Collection.execAggregate (AggregateOp.Avg "age") noMatchFilter
            avgResult |> should equal System.DBNull.Value
        }

    [<Fact>]
    member _.``Integration: execAggregate Count on empty result set returns 0``() =
        task {
            // Filter that matches no documents
            let noMatchFilter = Query.Field("age", FieldOp.Compare(box (CompareOp.Gt 1000L)))

            let! result = users |> Collection.execAggregate AggregateOp.Count noMatchFilter

            let count = System.Convert.ToInt32(result)
            count |> should equal 0
        }

    // ═══════════════════════════════════════════════════════════════
    // Integration Tests - execAll Operator
    // ═══════════════════════════════════════════════════════════════

    [<Fact>]
    member _.``Integration: execAll returns true when all documents match predicate``() =
        task {
            // Test data: Alice=25, Bob=30, Charlie=35 - all ages > 20
            let predicate = Query.Field("age", FieldOp.Compare(box (CompareOp.Gt 20L)))

            let! result = users |> Collection.execAll predicate Query.Empty

            result |> should equal true
        }

    [<Fact>]
    member _.``Integration: execAll returns false when any document doesn't match``() =
        task {
            // Test data: Alice=25, Bob=30, Charlie=35 - not all ages > 30
            let predicate = Query.Field("age", FieldOp.Compare(box (CompareOp.Gt 30L)))

            let! result = users |> Collection.execAll predicate Query.Empty

            result |> should equal false
        }

    // ═══════════════════════════════════════════════════════════════
    // Integration Tests - execFind Operator
    // ═══════════════════════════════════════════════════════════════

    [<Fact>]
    member _.``Integration: execFind returns matching document``() =
        task {
            // Find Alice by email
            let predicate =
                Query.Field("email", FieldOp.Compare(box (CompareOp.Eq "alice@test.com")))

            let! result = users |> Collection.execFind predicate

            result.Data.Name |> should equal "Alice"
            result.Data.Age |> should equal 25L
        }

    [<Fact>]
    member _.``Integration: execFind throws InvalidOperationException when no match``() =
        task {
            // Search for non-existent email
            let predicate =
                Query.Field("email", FieldOp.Compare(box (CompareOp.Eq "nonexistent@test.com")))

            let action = fun () -> (users |> Collection.execFind predicate).Result |> ignore

            action |> should throw typeof<System.AggregateException>
        }

    [<Fact>]
    member _.``Integration: execFind with complex predicate works``() =
        task {
            // Find user who is active AND age >= 30 (should find Bob)
            let activePredicate =
                Query.Field("active", FieldOp.Compare(box (CompareOp.Eq true)))

            let agePredicate = Query.Field("age", FieldOp.Compare(box (CompareOp.Gte 30L)))
            let predicate = Query.And [ activePredicate; agePredicate ]

            let! result = users |> Collection.execFind predicate

            result.Data.Name |> should equal "Bob"
            result.Data.Age |> should equal 30L
        }

    [<Fact>]
    member _.``Integration: execFind returns first match with multiple matches``() =
        task {
            // Find any active user (Alice=25 or Bob=30) - should return one of them
            let predicate = Query.Field("active", FieldOp.Compare(box (CompareOp.Eq true)))

            let! result = users |> Collection.execFind predicate

            // Should return either Alice or Bob (both are active)
            result.Data.Active |> should equal true
            [ "Alice"; "Bob" ] |> should contain result.Data.Name
        }


    [<Fact>]
    member _.``Integration: execAll with filter checks only filtered documents``() =
        task {
            // Test data: Active users are Alice=25, Bob=30 - all active users have age >= 25
            let filter = Query.Field("active", FieldOp.Compare(box (CompareOp.Eq true)))
            let predicate = Query.Field("age", FieldOp.Compare(box (CompareOp.Gte 25L)))

            let! result = users |> Collection.execAll predicate filter

            result |> should equal true
        }

    [<Fact>]
    member _.``Integration: execAll with filter returns false when filtered subset doesn't all match``() =
        task {
            // Test data: Active users are Alice=25, Bob=30 - not all have age >= 30
            let filter = Query.Field("active", FieldOp.Compare(box (CompareOp.Eq true)))
            let predicate = Query.Field("age", FieldOp.Compare(box (CompareOp.Gte 30L)))

            let! result = users |> Collection.execAll predicate filter

            result |> should equal false
        }

    // ═══════════════════════════════════════════════════════════════
    // Integration Tests - execAggregateNullable Operator
    // ═══════════════════════════════════════════════════════════════

    [<Fact>]
    member _.``Integration: execAggregateNullable Min returns Some when data exists``() =
        task {
            // Test data: Alice=25, Bob=30, Charlie=35 - min age is 25
            let! result = users |> Collection.execAggregateNullable (AggregateOp.Min "age") Query.Empty

            result.IsSome |> should equal true
            Convert.ToInt64(result.Value) |> should equal 25L
        }

    [<Fact>]
    member _.``Integration: execAggregateNullable Max returns Some when data exists``() =
        task {
            // Test data: Alice=25, Bob=30, Charlie=35 - max age is 35
            let! result = users |> Collection.execAggregateNullable (AggregateOp.Max "age") Query.Empty

            result.IsSome |> should equal true
            Convert.ToInt64(result.Value) |> should equal 35L
        }

    [<Fact>]
    member _.``Integration: execAggregateNullable Sum returns Some when data exists``() =
        task {
            // Test data: Alice=25 + Bob=30 + Charlie=35 = 90
            let! result = users |> Collection.execAggregateNullable (AggregateOp.Sum "age") Query.Empty

            result.IsSome |> should equal true
            Convert.ToInt64(result.Value) |> should equal 90L
        }

    [<Fact>]
    member _.``Integration: execAggregateNullable Avg returns Some when data exists``() =
        task {
            // Test data: (25 + 30 + 35) / 3 = 30.0
            let! result = users |> Collection.execAggregateNullable (AggregateOp.Avg "age") Query.Empty

            result.IsSome |> should equal true
            Convert.ToDouble(result.Value) |> should equal 30.0
        }

    [<Fact>]
    member _.``Integration: execAggregateNullable returns None when no documents match filter``() =
        task {
            // Filter that matches no documents (age > 1000)
            let noMatchFilter = Query.Field("age", FieldOp.Compare(box (CompareOp.Gt 1000L)))

            let! result = users |> Collection.execAggregateNullable (AggregateOp.Min "age") noMatchFilter

            result.IsNone |> should equal true
        }

    [<Fact>]
    member _.``Integration: execAggregateNullable with filter returns correct value``() =
        task {
            // Filter for active users only (Alice=25, Bob=30)
            let activeFilter = Query.Field("active", FieldOp.Compare(box (CompareOp.Eq true)))

            let! result = users |> Collection.execAggregateNullable (AggregateOp.Max "age") activeFilter

            result.IsSome |> should equal true
            Convert.ToInt64(result.Value) |> should equal 30L // Bob is oldest active user
        }

    // ═══════════════════════════════════════════════════════════════
    // Integration Tests - execGroupBy Operator
    // ═══════════════════════════════════════════════════════════════

    [<Fact>]
    member _.``Integration: execGroupBy groups by field and returns counts``() =
        task {
            // Test data: Alice=active, Bob=active, Charlie=inactive
            // Group by active status
            let q: FractalDb.QueryExpr.TranslatedQuery<TestUser> =
                query {
                    for user in users do
                        groupBy user.Active
                }

            let! results = users |> Collection.execGroupBy q

            // Should have 2 groups: true (2 users) and false (1 user)
            results |> List.length |> should equal 2

            let activeGroup = results |> List.find (fun (k, _) -> Convert.ToBoolean(k) = true)

            let inactiveGroup =
                results |> List.find (fun (k, _) -> Convert.ToBoolean(k) = false)

            snd activeGroup |> should equal 2L
            snd inactiveGroup |> should equal 1L
        }

    [<Fact>]
    member _.``Integration: execGroupBy with where clause filters before grouping``() =
        task {
            // Test data: Alice=25/active, Bob=30/active, Charlie=35/inactive
            // Filter for age >= 30, then group by active
            let q: FractalDb.QueryExpr.TranslatedQuery<TestUser> =
                query {
                    for user in users do
                        where (user.Age >= 30L)
                        groupBy user.Active
                }

            let! results = users |> Collection.execGroupBy q

            // Should have 2 groups: Bob (active) and Charlie (inactive)
            results |> List.length |> should equal 2

            let activeGroup = results |> List.find (fun (k, _) -> Convert.ToBoolean(k) = true)

            let inactiveGroup =
                results |> List.find (fun (k, _) -> Convert.ToBoolean(k) = false)

            snd activeGroup |> should equal 1L // Bob
            snd inactiveGroup |> should equal 1L // Charlie
        }

    [<Fact>]
    member _.``Integration: execGroupBy returns empty for no matches``() =
        task {
            // Filter that matches no documents
            let q: FractalDb.QueryExpr.TranslatedQuery<TestUser> =
                query {
                    for user in users do
                        where (user.Age > 1000L)
                        groupBy user.Active
                }

            let! results = users |> Collection.execGroupBy q

            results |> List.length |> should equal 0
        }

    [<Fact>]
    member _.``Integration: execGroupBy groups by string field``() =
        task {
            // Group by name - each user has unique name, so 3 groups with count 1 each
            let q: FractalDb.QueryExpr.TranslatedQuery<TestUser> =
                query {
                    for user in users do
                        groupBy user.Name
                }

            let! results = users |> Collection.execGroupBy q

            // Should have 3 groups (Alice, Bob, Charlie)
            results |> List.length |> should equal 3

            // Each group should have count 1
            results |> List.iter (fun (_, count) -> count |> should equal 1L)
        }

    // ═══════════════════════════════════════════════════════════════
    // Integration Tests - execLast / execLastOrDefault Operators
    // ═══════════════════════════════════════════════════════════════

    [<Fact>]
    member _.``Integration: execLast returns last element by reversing sort order``() =
        task {
            // Sort by age ascending: Alice=25, Bob=30, Charlie=35
            // last should return Charlie (age 35)
            let q: FractalDb.QueryExpr.TranslatedQuery<TestUser> =
                query {
                    for user in users do
                        sortBy user.Age
                }

            let! result = users |> Collection.execLast q

            result.Data.Name |> should equal "Charlie"
            result.Data.Age |> should equal 35L
        }

    [<Fact>]
    member _.``Integration: execLast with descending sort returns first in original order``() =
        task {
            // Sort by age descending: Charlie=35, Bob=30, Alice=25
            // last should return Alice (age 25)
            let q: FractalDb.QueryExpr.TranslatedQuery<TestUser> =
                query {
                    for user in users do
                        sortByDescending user.Age
                }

            let! result = users |> Collection.execLast q

            result.Data.Name |> should equal "Alice"
            result.Data.Age |> should equal 25L
        }

    [<Fact>]
    member _.``Integration: execLast with where clause returns last of filtered results``() =
        task {
            // Active users sorted by age: Alice=25, Bob=30
            // last should return Bob
            let q: FractalDb.QueryExpr.TranslatedQuery<TestUser> =
                query {
                    for user in users do
                        where (user.Active = true)
                        sortBy user.Age
                }

            let! result = users |> Collection.execLast q

            result.Data.Name |> should equal "Bob"
            result.Data.Age |> should equal 30L
        }

    [<Fact>]
    member _.``Integration: execLast throws InvalidOperationException when empty``() =
        task {
            // Filter that matches no documents
            let q: FractalDb.QueryExpr.TranslatedQuery<TestUser> =
                query {
                    for user in users do
                        where (user.Age > 100L)
                        sortBy user.Age
                }

            let action = fun () -> (users |> Collection.execLast q).Result |> ignore

            action |> should throw typeof<System.AggregateException>
        }

    [<Fact>]
    member _.``Integration: execLastOrDefault returns Some for non-empty result``() =
        task {
            // Sort by age ascending: Alice=25, Bob=30, Charlie=35
            // lastOrDefault should return Some(Charlie)
            let q: FractalDb.QueryExpr.TranslatedQuery<TestUser> =
                query {
                    for user in users do
                        sortBy user.Age
                }

            let! result = users |> Collection.execLastOrDefault q

            result |> should not' (be Null)
            result.IsSome |> should equal true
            result.Value.Data.Name |> should equal "Charlie"
        }

    [<Fact>]
    member _.``Integration: execLastOrDefault returns None for empty result``() =
        task {
            // Filter that matches no documents
            let q: FractalDb.QueryExpr.TranslatedQuery<TestUser> =
                query {
                    for user in users do
                        where (user.Age > 100L)
                        sortBy user.Age
                }

            let! result = users |> Collection.execLastOrDefault q

            result |> should equal None
        }

    [<Fact>]
    member _.``Integration: execLast with string sort returns last alphabetically``() =
        task {
            // Sort by name ascending: Alice, Bob, Charlie
            // last should return Charlie
            let q: FractalDb.QueryExpr.TranslatedQuery<TestUser> =
                query {
                    for user in users do
                        sortBy user.Name
                }

            let! result = users |> Collection.execLast q

            result.Data.Name |> should equal "Charlie"
        }

    // ═══════════════════════════════════════════════════════════════
    // Integration Tests - execExactlyOne / execExactlyOneOrDefault Operators
    // ═══════════════════════════════════════════════════════════════

    [<Fact>]
    member _.``Integration: execExactlyOne returns single matching document``() =
        task {
            // Alice has unique email
            let q: FractalDb.QueryExpr.TranslatedQuery<TestUser> =
                query {
                    for user in users do
                        where (user.Email = "alice@test.com")
                }

            let! result = users |> Collection.execExactlyOne q

            result.Data.Name |> should equal "Alice"
            result.Data.Email |> should equal "alice@test.com"
        }

    [<Fact>]
    member _.``Integration: execExactlyOne throws when no match``() =
        task {
            // Non-existent email
            let q: FractalDb.QueryExpr.TranslatedQuery<TestUser> =
                query {
                    for user in users do
                        where (user.Email = "nonexistent@test.com")
                }

            let action = fun () -> (users |> Collection.execExactlyOne q).Result |> ignore

            action |> should throw typeof<System.AggregateException>
        }

    [<Fact>]
    member _.``Integration: execExactlyOne throws when multiple matches``() =
        task {
            // Multiple active users exist (Alice and Bob are both active)
            let q: FractalDb.QueryExpr.TranslatedQuery<TestUser> =
                query {
                    for user in users do
                        where (user.Active = true)
                }

            let action = fun () -> (users |> Collection.execExactlyOne q).Result |> ignore

            action |> should throw typeof<System.AggregateException>
        }

    [<Fact>]
    member _.``Integration: execExactlyOneOrDefault returns Some for single match``() =
        task {
            // Alice has unique email
            let q: FractalDb.QueryExpr.TranslatedQuery<TestUser> =
                query {
                    for user in users do
                        where (user.Email = "alice@test.com")
                }

            let! result = users |> Collection.execExactlyOneOrDefault q

            result.IsSome |> should equal true
            result.Value.Data.Name |> should equal "Alice"
        }

    [<Fact>]
    member _.``Integration: execExactlyOneOrDefault returns None for no match``() =
        task {
            // Non-existent email
            let q: FractalDb.QueryExpr.TranslatedQuery<TestUser> =
                query {
                    for user in users do
                        where (user.Email = "nonexistent@test.com")
                }

            let! result = users |> Collection.execExactlyOneOrDefault q

            result |> should equal None
        }

    [<Fact>]
    member _.``Integration: execExactlyOneOrDefault throws when multiple matches``() =
        task {
            // Multiple active users exist (Alice and Bob are both active)
            let q: FractalDb.QueryExpr.TranslatedQuery<TestUser> =
                query {
                    for user in users do
                        where (user.Active = true)
                }

            let action =
                fun () -> (users |> Collection.execExactlyOneOrDefault q).Result |> ignore

            action |> should throw typeof<System.AggregateException>
        }

    [<Fact>]
    member _.``Integration: execExactlyOne with unique age returns correct user``() =
        task {
            // Charlie is the only one with age 35
            let q: FractalDb.QueryExpr.TranslatedQuery<TestUser> =
                query {
                    for user in users do
                        where (user.Age = 35L)
                }

            let! result = users |> Collection.execExactlyOne q

            result.Data.Name |> should equal "Charlie"
            result.Data.Age |> should equal 35L
        }

    // ═══════════════════════════════════════════════════════════════
    // Integration Tests - execNth Operator
    // ═══════════════════════════════════════════════════════════════

    [<Fact>]
    member _.``Integration: execNth 0 returns first element``() =
        task {
            // Sort by age ascending: Alice=25, Bob=30, Charlie=35
            // nth 0 should return Alice
            let q: FractalDb.QueryExpr.TranslatedQuery<TestUser> =
                query {
                    for user in users do
                        sortBy user.Age
                }

            let! result = users |> Collection.execNth 0 q

            result.Data.Name |> should equal "Alice"
            result.Data.Age |> should equal 25L
        }

    [<Fact>]
    member _.``Integration: execNth 1 returns second element``() =
        task {
            // Sort by age ascending: Alice=25, Bob=30, Charlie=35
            // nth 1 should return Bob
            let q: FractalDb.QueryExpr.TranslatedQuery<TestUser> =
                query {
                    for user in users do
                        sortBy user.Age
                }

            let! result = users |> Collection.execNth 1 q

            result.Data.Name |> should equal "Bob"
            result.Data.Age |> should equal 30L
        }

    [<Fact>]
    member _.``Integration: execNth 2 returns third element``() =
        task {
            // Sort by age ascending: Alice=25, Bob=30, Charlie=35
            // nth 2 should return Charlie
            let q: FractalDb.QueryExpr.TranslatedQuery<TestUser> =
                query {
                    for user in users do
                        sortBy user.Age
                }

            let! result = users |> Collection.execNth 2 q

            result.Data.Name |> should equal "Charlie"
            result.Data.Age |> should equal 35L
        }

    [<Fact>]
    member _.``Integration: execNth with descending sort works correctly``() =
        task {
            // Sort by age descending: Charlie=35, Bob=30, Alice=25
            // nth 0 should return Charlie
            let q: FractalDb.QueryExpr.TranslatedQuery<TestUser> =
                query {
                    for user in users do
                        sortByDescending user.Age
                }

            let! result = users |> Collection.execNth 0 q

            result.Data.Name |> should equal "Charlie"
        }

    [<Fact>]
    member _.``Integration: execNth throws when index out of bounds``() =
        task {
            // Only 3 users exist, so index 10 is out of bounds
            let q: FractalDb.QueryExpr.TranslatedQuery<TestUser> =
                query {
                    for user in users do
                        sortBy user.Age
                }

            let action = fun () -> (users |> Collection.execNth 10 q).Result |> ignore

            action |> should throw typeof<System.AggregateException>
        }

    [<Fact>]
    member _.``Integration: execNth throws when index is negative``() =
        task {
            let q: FractalDb.QueryExpr.TranslatedQuery<TestUser> =
                query {
                    for user in users do
                        sortBy user.Age
                }

            let action = fun () -> (users |> Collection.execNth -1 q).Result |> ignore

            action |> should throw typeof<System.ArgumentOutOfRangeException>
        }

    [<Fact>]
    member _.``Integration: execNth with where clause returns correct element``() =
        task {
            // Active users sorted by age: Alice=25, Bob=30
            // nth 1 should return Bob (2nd active user)
            let q: FractalDb.QueryExpr.TranslatedQuery<TestUser> =
                query {
                    for user in users do
                        where (user.Active = true)
                        sortBy user.Age
                }

            let! result = users |> Collection.execNth 1 q

            result.Data.Name |> should equal "Bob"
        }

// ═══════════════════════════════════════════════════════════════════════════
// Test Types and Fixtures for Nullable Sorting
// ═══════════════════════════════════════════════════════════════════════════

/// Test type with Option field for sortByNullable tests (Option serializes properly unlike Nullable)
type OptionalPriceProduct =
    { Name: string
      Price: int64 option
      Category: string }

/// Schema for optional price product collection
let optionalPriceProductSchema: SchemaDef<OptionalPriceProduct> =
    { Fields =
        [ { Name = "name"
            Path = None
            SqlType = SqliteType.Text
            Indexed = true
            Unique = false
            Nullable = false }
          { Name = "price"
            Path = None
            SqlType = SqliteType.Integer
            Indexed = true
            Unique = false
            Nullable = true }
          { Name = "category"
            Path = None
            SqlType = SqliteType.Text
            Indexed = true
            Unique = false
            Nullable = false } ]
      Indexes = []
      Timestamps = true
      Validate = None }

/// <summary>
/// Test fixture for nullable sorting tests with products that have optional prices.
/// </summary>
type NullableSortingTestFixture() =
    let db = FractalDb.InMemory()

    let products =
        db.Collection<OptionalPriceProduct>("products", optionalPriceProductSchema)

    // Seed test data with some null prices
    do
        let testProducts =
            [ { Name = "Apple"
                Price = Some 100L
                Category = "Food" }
              { Name = "Banana"
                Price = Some 50L
                Category = "Food" }
              { Name = "Mystery Box"
                Price = None
                Category = "Mystery" }
              { Name = "Cherry"
                Price = Some 75L
                Category = "Food" }
              { Name = "Unknown Item"
                Price = None
                Category = "Mystery" } ]

        testProducts
        |> List.iter (fun product ->
            products
            |> Collection.insertOne product
            |> Async.AwaitTask
            |> Async.RunSynchronously
            |> ignore)

    member _.Db = db
    member _.Products = products

    interface IDisposable with
        member _.Dispose() = db.Close()

// ═══════════════════════════════════════════════════════════════════════════
// Integration Tests - sortByNullable Operator
// ═══════════════════════════════════════════════════════════════════════════

type SortByNullableIntegrationTests(fixture: NullableSortingTestFixture) =
    let products = fixture.Products

    interface IClassFixture<NullableSortingTestFixture>

    [<Fact>]
    member _.``Integration: sortByNullable ascending puts NULL values last``() =
        task {
            // Sort by price ascending with nulls last
            // Expected order: Banana=50, Cherry=75, Apple=100, MysteryBox=None, UnknownItem=None
            let q: FractalDb.QueryExpr.TranslatedQuery<OptionalPriceProduct> =
                query {
                    for product in products do
                        sortByNullable product.Price
                }

            let! results = products |> Collection.exec q

            // First 3 should be non-null values in ascending order
            results[0].Data.Name |> should equal "Banana"
            results[0].Data.Price |> should equal (Some 50L)

            results[1].Data.Name |> should equal "Cherry"
            results[1].Data.Price |> should equal (Some 75L)

            results[2].Data.Name |> should equal "Apple"
            results[2].Data.Price |> should equal (Some 100L)

            // Last 2 should be null values
            results[3].Data.Price |> should equal None
            results[4].Data.Price |> should equal None
        }

    [<Fact>]
    member _.``Integration: sortByNullableDescending puts NULL values last``() =
        task {
            // Sort by price descending with nulls last
            // Expected order: Apple=100, Cherry=75, Banana=50, MysteryBox=None, UnknownItem=None
            let q: FractalDb.QueryExpr.TranslatedQuery<OptionalPriceProduct> =
                query {
                    for product in products do
                        sortByNullableDescending product.Price
                }

            let! results = products |> Collection.exec q

            // First 3 should be non-null values in descending order
            results[0].Data.Name |> should equal "Apple"
            results[0].Data.Price |> should equal (Some 100L)

            results[1].Data.Name |> should equal "Cherry"
            results[1].Data.Price |> should equal (Some 75L)

            results[2].Data.Name |> should equal "Banana"
            results[2].Data.Price |> should equal (Some 50L)

            // Last 2 should be null values
            results[3].Data.Price |> should equal None
            results[4].Data.Price |> should equal None
        }

    [<Fact>]
    member _.``Integration: sortByNullable with where clause filters then sorts``() =
        task {
            // Filter for Food category only, then sort by price with nulls last
            // Expected: Banana=50, Cherry=75, Apple=100 (no nulls in Food)
            let q: FractalDb.QueryExpr.TranslatedQuery<OptionalPriceProduct> =
                query {
                    for product in products do
                        where (product.Category = "Food")
                        sortByNullable product.Price
                }

            let! results = products |> Collection.exec q

            results |> List.length |> should equal 3
            results[0].Data.Name |> should equal "Banana"
            results[1].Data.Name |> should equal "Cherry"
            results[2].Data.Name |> should equal "Apple"
        }

    [<Fact>]
    member _.``Integration: sortByNullable with take limits results``() =
        task {
            // Sort and take first 2 (should be non-null lowest prices)
            let q: FractalDb.QueryExpr.TranslatedQuery<OptionalPriceProduct> =
                query {
                    for product in products do
                        sortByNullable product.Price
                        take 2
                }

            let! results = products |> Collection.exec q

            results |> List.length |> should equal 2
            results[0].Data.Name |> should equal "Banana"
            results[1].Data.Name |> should equal "Cherry"
        }

    [<Fact>]
    member _.``Integration: sortByNullableDescending with take limits results``() =
        task {
            // Sort descending and take first 2 (should be non-null highest prices)
            let q: FractalDb.QueryExpr.TranslatedQuery<OptionalPriceProduct> =
                query {
                    for product in products do
                        sortByNullableDescending product.Price
                        take 2
                }

            let! results = products |> Collection.exec q

            results |> List.length |> should equal 2
            results[0].Data.Name |> should equal "Apple"
            results[1].Data.Name |> should equal "Cherry"
        }

    [<Fact>]
    member _.``Integration: first element with sortByNullable is first non-null``() =
        task {
            let q: FractalDb.QueryExpr.TranslatedQuery<OptionalPriceProduct> =
                query {
                    for product in products do
                        sortByNullable product.Price
                        take 1
                }

            let! results = products |> Collection.exec q

            results |> List.length |> should equal 1
            results[0].Data.Name |> should equal "Banana"
            results[0].Data.Price |> should equal (Some 50L)
        }

    [<Fact>]
    member _.``Integration: execLast with sortByNullable returns null value``() =
        task {
            // Last element when sorted ascending with nulls-last should be a null
            let q: FractalDb.QueryExpr.TranslatedQuery<OptionalPriceProduct> =
                query {
                    for product in products do
                        sortByNullable product.Price
                }

            let! result = products |> Collection.execLast q

            result.Data.Price |> should equal None
        }

    [<Fact>]
    member _.``Integration: execLastOrDefault with sortByNullable returns null value``() =
        task {
            // Last element when sorted ascending with nulls-last should be a null
            let q: FractalDb.QueryExpr.TranslatedQuery<OptionalPriceProduct> =
                query {
                    for product in products do
                        sortByNullable product.Price
                }

            let! result = products |> Collection.execLastOrDefault q

            result.IsSome |> should equal true
            result.Value.Data.Price |> should equal None
        }

    [<Fact>]
    member _.``Integration: execLast with sortByNullableDescending returns null value``() =
        task {
            // Last element when sorted descending with nulls-last should also be a null
            let q: FractalDb.QueryExpr.TranslatedQuery<OptionalPriceProduct> =
                query {
                    for product in products do
                        sortByNullableDescending product.Price
                }

            let! result = products |> Collection.execLast q

            result.Data.Price |> should equal None
        }

    [<Fact>]
    member _.``Integration: thenByNullable adds secondary sort with NULLs last``() =
        task {
            // Sort by category first, then by price (NULLs last) within each category
            // Food: Banana=50, Cherry=75, Apple=100
            // Mystery: MysteryBox=None, UnknownItem=None (but sorted to end within Mystery)
            let q: FractalDb.QueryExpr.TranslatedQuery<OptionalPriceProduct> =
                query {
                    for product in products do
                        sortBy product.Category
                        thenByNullable product.Price
                }

            let! results = products |> Collection.exec q

            // Food category first (alphabetically)
            results[0].Data.Name |> should equal "Banana"
            results[0].Data.Category |> should equal "Food"
            results[0].Data.Price |> should equal (Some 50L)

            results[1].Data.Name |> should equal "Cherry"
            results[1].Data.Category |> should equal "Food"
            results[1].Data.Price |> should equal (Some 75L)

            results[2].Data.Name |> should equal "Apple"
            results[2].Data.Category |> should equal "Food"
            results[2].Data.Price |> should equal (Some 100L)

            // Mystery category second (alphabetically), both have None prices
            results[3].Data.Category |> should equal "Mystery"
            results[3].Data.Price |> should equal None

            results[4].Data.Category |> should equal "Mystery"
            results[4].Data.Price |> should equal None
        }

    [<Fact>]
    member _.``Integration: thenByNullableDescending adds secondary descending sort with NULLs last``() =
        task {
            // Sort by category first, then by price descending (NULLs last) within each category
            // Food: Apple=100, Cherry=75, Banana=50
            // Mystery: MysteryBox=None, UnknownItem=None (sorted to end within Mystery)
            let q: FractalDb.QueryExpr.TranslatedQuery<OptionalPriceProduct> =
                query {
                    for product in products do
                        sortBy product.Category
                        thenByNullableDescending product.Price
                }

            let! results = products |> Collection.exec q

            // Food category first (alphabetically), highest price first within Food
            results[0].Data.Name |> should equal "Apple"
            results[0].Data.Category |> should equal "Food"
            results[0].Data.Price |> should equal (Some 100L)

            results[1].Data.Name |> should equal "Cherry"
            results[1].Data.Category |> should equal "Food"
            results[1].Data.Price |> should equal (Some 75L)

            results[2].Data.Name |> should equal "Banana"
            results[2].Data.Category |> should equal "Food"
            results[2].Data.Price |> should equal (Some 50L)

            // Mystery category second (alphabetically), both have None prices
            results[3].Data.Category |> should equal "Mystery"
            results[3].Data.Price |> should equal None

            results[4].Data.Category |> should equal "Mystery"
            results[4].Data.Price |> should equal None
        }

    [<Fact>]
    member _.``Integration: sortByNullable with thenBy for multi-field nullable sorting``() =
        task {
            // Sort by price (NULLs last) first, then by name within same price
            let q: FractalDb.QueryExpr.TranslatedQuery<OptionalPriceProduct> =
                query {
                    for product in products do
                        sortByNullable product.Price
                        thenBy product.Name
                }

            let! results = products |> Collection.exec q

            // Non-null prices first in ascending order
            results[0].Data.Price |> should equal (Some 50L)
            results[1].Data.Price |> should equal (Some 75L)
            results[2].Data.Price |> should equal (Some 100L)

            // Null prices last, sorted by name
            // "Mystery Box" comes before "Unknown Item" alphabetically
            results[3].Data.Price |> should equal None
            results[3].Data.Name |> should equal "Mystery Box"

            results[4].Data.Price |> should equal None
            results[4].Data.Name |> should equal "Unknown Item"
        }

    [<Fact>]
    member _.``Integration: thenByNullable with take limits results``() =
        task {
            // Sort by category then by nullable price, take top 3
            let q: FractalDb.QueryExpr.TranslatedQuery<OptionalPriceProduct> =
                query {
                    for product in products do
                        sortBy product.Category
                        thenByNullable product.Price
                        take 3
                }

            let! results = products |> Collection.exec q

            results |> List.length |> should equal 3

            // All 3 should be from Food category (first alphabetically)
            results[0].Data.Category |> should equal "Food"
            results[1].Data.Category |> should equal "Food"
            results[2].Data.Category |> should equal "Food"

            // In ascending price order
            results[0].Data.Price |> should equal (Some 50L)
            results[1].Data.Price |> should equal (Some 75L)
            results[2].Data.Price |> should equal (Some 100L)
        }

    [<Fact>]
    member _.``Integration: sortByNullable with thenByNullable for all-nullable sorting``() =
        task {
            // Both sorts use nullable handling
            let q: FractalDb.QueryExpr.TranslatedQuery<OptionalPriceProduct> =
                query {
                    for product in products do
                        sortByNullable product.Price
                        thenByNullable product.Price // Same field - just tests translation works
                }

            let! results = products |> Collection.exec q

            // Should still work - non-null prices first
            results[0].Data.Price |> should equal (Some 50L)
            results[1].Data.Price |> should equal (Some 75L)
            results[2].Data.Price |> should equal (Some 100L)

            // Nulls last
            results[3].Data.Price |> should equal None
            results[4].Data.Price |> should equal None
        }
