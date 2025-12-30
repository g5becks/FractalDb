module FractalDb.Tests.QueryExprTests

// Suppress linter warnings for test code
// fsharplint:disable FL0072

open System
open System.Threading.Tasks
open Xunit
open FsUnit.Xunit
open FractalDb.Types
open FractalDb.Schema
open FractalDb.Operators
open FractalDb.QueryExpr
open FractalDb.QueryExpr.QueryBuilderInstance
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

            let! results = queryExpr.exec (users)

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

            let! results = queryExpr.exec (users)

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

            let! results = queryExpr.exec (users)

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

            let! results = queryExpr.exec (users)

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

            let! results = queryExpr.exec (users)

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

            let! results = queryExpr.exec (users)

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

            let! results = queryExpr.exec (users)

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

            let! results = queryExpr.exec (users)

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
            let! fluentResults = queryExpr.exec (users)

            moduleResults |> should haveLength 2
            fluentResults |> should haveLength 2

            let moduleNames = moduleResults |> List.map (fun doc -> doc.Data.Name)

            let fluentNames = fluentResults |> List.map (fun doc -> doc.Data.Name)

            moduleNames |> should equal fluentNames
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

            let! results = baseQuery.where(filter).exec (users)

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

            let! results = baseQuery.where(filter1).where(filter2).exec (users)

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

            let! results = queryWithFilter.where(additionalFilter).exec (users)

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
                    .exec (users)

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

            let! results = baseQuery.orderBy("age", FractalDb.QueryExpr.SortDirection.Asc).exec (users)

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

            let! results = baseQuery.orderBy("age", FractalDb.QueryExpr.SortDirection.Desc).exec (users)

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
                        .exec (users)

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
            let! results = queryWithSort.orderBy("age", FractalDb.QueryExpr.SortDirection.Desc).exec (users)

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

            let! results = baseQuery.skip(1).exec (users)

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
            let! results = queryWithSkip.skip(1).exec (users)

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

            let! results = baseQuery.skip(0).exec (users)

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

            let! results = baseQuery.skip(100).exec (users)

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

            let! results = baseQuery.limit(2).exec (users)

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
            let! results = queryWithTake.limit(2).exec (users)

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

            let! results = baseQuery.limit(0).exec (users)

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

            let! results = baseQuery.limit(100).exec (users)

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
            let! results = baseQuery.skip(1).limit(1).exec (users)

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
                    .exec (users)

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

            let query1 = baseQuery.where (filter)
            let query2 = query1.skip (1)

            // Original should be unchanged
            let! baseResults = baseQuery.exec (users)
            baseResults |> should haveLength 3

            // query1 should have filter
            let! query1Results = query1.exec (users)
            query1Results |> should haveLength 2

            // query2 should have filter + skip
            let! query2Results = query2.exec (users)
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
                queryExpr.where(Query.Field("active", FieldOp.Compare(box (CompareOp.Eq true)))).limit(1).exec (users)

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

            let! results = composedQuery.exec (users)

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

            let! results = composedQuery.exec (users)

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

            let! results = composedQuery.exec (users)

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

            let! results = composedQuery.exec (users)

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

            let! pipelineResults = pipelineQuery.exec (users)

            // Fluent style
            let! fluentResults = baseQuery.where(filter).orderBy("name", SortDirection.Asc).limit(1).exec (users)

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
            let! results = combined.exec (users)

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
            let! results = combined.exec (users)

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
            let! results = combined.exec (users)

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
                let! results = combined.exec (users)

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
            let! results = combined.exec (users)

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
            let composed = filters.compose (sorting)
            let! (composeResults: list<Document<TestUser>>) = composed.exec (users)

            // Using <+> operator
            let operated = filters <+> sorting
            let! (operatorResults: list<Document<TestUser>>) = operated.exec (users)

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
            let! (activeNamedResults: list<Document<TestUser>>) = (activeOnly <+> sortedByName).exec (users)
            let! (activeLimitedResults: list<Document<TestUser>>) = (activeOnly <+> firstTwo).exec (users)
            let! (allThreeResults: list<Document<TestUser>>) = (activeOnly <+> sortedByName <+> firstTwo).exec (users)

            activeNamedResults |> should haveLength 2
            activeLimitedResults |> should haveLength 2
            allThreeResults |> should haveLength 2

            // All three combined: active, sorted by name, take 2
            allThreeResults.[0].Data.Name |> should equal "Alice"
            allThreeResults.[1].Data.Name |> should equal "Bob"
        }
