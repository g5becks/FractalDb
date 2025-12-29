module FractalDb.Tests.BuilderTests

open System
open System.Threading.Tasks
open Xunit
open FsUnit.Xunit
open FractalDb.Types
open FractalDb.Errors
open FractalDb.Operators
open FractalDb.Query
open FractalDb.Schema
open FractalDb.Options
open FractalDb.Builders
open FractalDb.Database
open FractalDb.Collection

type TestUser =
    { Name: string
      Email: string
      Age: int
      Role: string }

//
// QueryBuilder Tests
//

[<Fact>]
let ``QueryBuilder with empty query returns Query.Empty`` () =
    let q = query { () }
    q |> should equal Query.Empty

[<Fact>]
let ``QueryBuilder with single field creates FieldOp`` () =
    let q = query { field "age" (Query.gte 18L) }

    match q with
    | Query.FieldOp(FieldOp.Field("age", CompareOp.Gte 18L)) -> ()
    | _ -> failwith $"Expected FieldOp with Gte, got {q}"

[<Fact>]
let ``QueryBuilder with multiple fields creates AND query`` () =
    let q =
        query {
            field "age" (Query.gte 18L)
            field "status" (Query.eq "active")
        }

    match q with
    | Query.And ops -> ops |> should haveLength 2
    | _ -> failwith $"Expected Query.And, got {q}"

[<Fact>]
let ``QueryBuilder where is alias for field`` () =
    let q1 = query { where "email" (Query.endsWith "@test.com") }

    let q2 = query { field "email" (Query.endsWith "@test.com") }

    q1 |> should equal q2

[<Fact>]
let ``QueryBuilder orElse creates OR branch`` () =
    let q =
        query {
            field "role" (Query.eq "admin")
            orElse [ Query.eq "role" "moderator"; Query.eq "role" "superuser" ]
        }

    match q with
    | Query.And [ _; Query.Or _ ] -> ()
    | _ -> failwith $"Expected And with Or branch, got {q}"

[<Fact>]
let ``QueryBuilder norElse creates NOR branch`` () =
    let q =
        query {
            field "status" (Query.eq "active")
            norElse [ Query.eq "role" "banned"; Query.eq "role" "suspended" ]
        }

    match q with
    | Query.And [ _; Query.Nor _ ] -> ()
    | _ -> failwith $"Expected And with Nor branch, got {q}"

[<Fact>]
let ``QueryBuilder not' negates condition`` () =
    let q =
        query {
            field "age" (Query.gte 18L)
            not' (Query.eq "status" "banned")
        }

    match q with
    | Query.And [ _; Query.Not _ ] -> ()
    | _ -> failwith $"Expected And with Not, got {q}"

[<Fact>]
let ``QueryBuilder andAlso explicitly combines with AND`` () =
    let subQuery =
        query {
            field "price" (Query.gte 100L)
            field "price" (Query.lt 500L)
        }

    let finalQuery =
        query {
            field "category" (Query.eq "electronics")
            andAlso subQuery
        }

    match finalQuery with
    | Query.And ops -> ops |> should haveLength 2
    | _ -> failwith $"Expected Query.And, got {finalQuery}"

//
// SchemaBuilder Tests
//

[<Fact>]
let ``SchemaBuilder with no fields creates empty schema`` () =
    let s = schema<TestUser> { () }
    s.Fields |> should be Empty
    s.Indexes |> should be Empty
    s.Timestamps |> should equal false
    s.Validate |> should equal None

[<Fact>]
let ``SchemaBuilder field adds field to schema`` () =
    let s = schema<TestUser> { field "name" SqliteType.Text (fun u -> u.Name) [] }

    s.Fields |> should haveLength 1
    let f = s.Fields.[0]
    f.Name |> should equal "name"
    f.SqlType |> should equal SqliteType.Text
    f.Indexed |> should equal false
    f.Unique |> should equal false
    f.Nullable |> should equal false

[<Fact>]
let ``SchemaBuilder field with constraints sets flags`` () =
    let s =
        schema<TestUser> { field "email" SqliteType.Text (fun u -> u.Email) [ Unique; Indexed ] }

    let f = s.Fields.[0]
    f.Indexed |> should equal true
    f.Unique |> should equal true
    f.Nullable |> should equal false

[<Fact>]
let ``SchemaBuilder multiple fields preserves order`` () =
    let s =
        schema<TestUser> {
            field "name" SqliteType.Text (fun u -> u.Name) []
            field "email" SqliteType.Text (fun u -> u.Email) []
            field "age" SqliteType.Integer (fun u -> int64 u.Age) []
        }

    s.Fields |> should haveLength 3
    s.Fields.[0].Name |> should equal "name"
    s.Fields.[1].Name |> should equal "email"
    s.Fields.[2].Name |> should equal "age"

[<Fact>]
let ``SchemaBuilder index adds compound index`` () =
    let s =
        schema<TestUser> {
            field "name" SqliteType.Text (fun u -> u.Name) []
            field "age" SqliteType.Integer (fun u -> int64 u.Age) []
            index "idx_name_age" [ "name"; "age" ]
        }

    s.Indexes |> should haveLength 1
    let idx = s.Indexes.[0]
    idx.Name |> should equal "idx_name_age"
    idx.Fields |> should equal [ "name"; "age" ]
    idx.Unique |> should equal false

[<Fact>]
let ``SchemaBuilder index with unique flag`` () =
    let s =
        schema<TestUser> {
            field "email" SqliteType.Text (fun u -> u.Email) []
            uniqueIndex "idx_email" [ "email" ]
        }

    let idx = s.Indexes.[0]
    idx.Unique |> should equal true

[<Fact>]
let ``SchemaBuilder validate adds validation function`` () =
    let s =
        schema<TestUser> {
            field "age" SqliteType.Integer (fun u -> int64 u.Age) []
            validate (fun u -> if u.Age < 18 then Error "Must be 18 or older" else Ok u)
        }

    s.Validate |> should not' (equal None)

    // Test validation
    match s.Validate with
    | Some validFn ->
        let validUser =
            { Name = "Alice"
              Email = "alice@test.com"
              Age = 25
              Role = "user" }

        let invalidUser =
            { Name = "Bob"
              Email = "bob@test.com"
              Age = 15
              Role = "user" }

        validFn validUser |> should equal (Ok validUser)
        validFn invalidUser |> should equal (Error "Must be 18 or older")
    | None -> failwith "Expected validation function"

//
// OptionsBuilder Tests
//

[<Fact>]
let ``OptionsBuilder with no operations creates default options`` () =
    let opts = options<TestUser> { () }
    opts.Limit |> should equal None
    opts.Skip |> should equal None
    opts.Sort |> should equal []
    opts.Projection |> should equal None
    opts.Search |> should equal None
    opts.Cursor |> should equal None

[<Fact>]
let ``OptionsBuilder limit sets limit value`` () =
    let opts = options<TestUser> { limit 20 }

    opts.Limit |> should equal (Some 20)

[<Fact>]
let ``OptionsBuilder skip sets skip value`` () =
    let opts = options<TestUser> { skip 10 }

    opts.Skip |> should equal (Some 10)

[<Fact>]
let ``OptionsBuilder sortBy adds sort field`` () =
    let opts = options<TestUser> { sortBy "age" Descending }

    opts.Sort |> should haveLength 1
    let (field, dir) = opts.Sort.[0]
    field |> should equal "age"
    dir |> should equal Descending

[<Fact>]
let ``OptionsBuilder multiple sortBy preserves order`` () =
    let opts =
        options<TestUser> {
            sortBy "role" Ascending
            sortBy "age" Descending
        }

    opts.Sort |> should haveLength 2
    let (field1, dir1) = opts.Sort.[0]
    let (field2, dir2) = opts.Sort.[1]
    field1 |> should equal "role"
    dir1 |> should equal Ascending
    field2 |> should equal "age"
    dir2 |> should equal Descending

[<Fact>]
let ``OptionsBuilder project sets projection fields`` () =
    let opts = options<TestUser> { project [ "_id"; "name"; "email" ] }

    match opts.Projection with
    | Some fields -> fields |> should equal [ "_id"; "name"; "email" ]
    | None -> failwith "Expected Some projection"

[<Fact>]
let ``OptionsBuilder cursorAfter sets cursor`` () =
    let opts = options<TestUser> { cursorAfter "user-123" }

    match opts.Cursor with
    | Some(CursorAfter id) -> id |> should equal "user-123"
    | _ -> failwith "Expected CursorAfter"

[<Fact>]
let ``OptionsBuilder cursorBefore sets cursor`` () =
    let opts = options<TestUser> { cursorBefore "user-456" }

    match opts.Cursor with
    | Some(CursorBefore id) -> id |> should equal "user-456"
    | _ -> failwith "Expected CursorBefore"

[<Fact>]
let ``OptionsBuilder search sets text search spec`` () =
    let opts = options<TestUser> { search "test query" [ "name"; "email" ] }

    match opts.Search with
    | Some spec ->
        spec.Text |> should equal "test query"
        spec.Fields |> should equal [ "name"; "email" ]
    | None -> failwith "Expected Some search spec"

[<Fact>]
let ``OptionsBuilder combined operations work together`` () =
    let opts =
        options<TestUser> {
            limit 50
            skip 20
            sortBy "createdAt" Descending
            project [ "_id"; "name" ]
        }

    opts.Limit |> should equal (Some 50)
    opts.Skip |> should equal (Some 20)
    opts.Sort |> should haveLength 1
    opts.Projection |> should not' (equal None)

//
// TransactionBuilder Integration Tests (already exist in TransactionTests.fs)
// These test that db.Transact { } works correctly with let! bindings
//

[<Fact>]
let ``TransactionBuilder type is accessible via db.Transact`` () : Task =
    task {
        use db = FractalDb.InMemory()

        // Just verify we can create a transaction builder
        let builder = db.Transact
        builder |> should not' (be Null)
    }

//
// Integration Tests - Builders Used Together
//

type BuilderIntegrationFixture() =
    let db = FractalDb.InMemory()
    member _.Db = db

    interface IDisposable with
        member _.Dispose() = db.Close()

type BuilderIntegrationTests(fixture: BuilderIntegrationFixture) =
    interface IClassFixture<BuilderIntegrationFixture>

    [<Fact>]
    member _.``Query and Options builders work together in Collection.find``() : Task =
        task {
            // Arrange
            let db = fixture.Db

            let testSchema =
                schema<TestUser> {
                    field "name" SqliteType.Text (fun u -> u.Name) [ Indexed ]
                    field "email" SqliteType.Text (fun u -> u.Email) [ Unique; Indexed ]
                    field "age" SqliteType.Integer (fun u -> int64 u.Age) [ Indexed ]
                    field "role" SqliteType.Text (fun u -> u.Role) []
                }

            let users = db.Collection<TestUser>("users_query_opts", testSchema)

            // Insert test data
            let! _ =
                users
                |> Collection.insertOne
                    { Name = "Alice"
                      Email = "alice@test.com"
                      Age = 30
                      Role = "admin" }

            let! _ =
                users
                |> Collection.insertOne
                    { Name = "Bob"
                      Email = "bob@test.com"
                      Age = 25
                      Role = "user" }

            let! _ =
                users
                |> Collection.insertOne
                    { Name = "Charlie"
                      Email = "charlie@test.com"
                      Age = 35
                      Role = "user" }

            // Act - Use both query and options builders
            let q =
                query {
                    field "role" (Query.eq "user")
                    field "age" (Query.gte 25L)
                }

            let opts =
                options {
                    sortBy "age" Descending
                    limit 10
                }

            let! results = users |> Collection.find q opts

            // Assert
            results |> should haveLength 2
            results.[0].Data.Name |> should equal "Charlie" // Age 35, sorted descending
            results.[1].Data.Name |> should equal "Bob" // Age 25
        }

    [<Fact>]
    member _.``Schema builder with validation works in Collection``() : Task =
        task {
            // Arrange
            let db = fixture.Db

            let validatedSchema =
                schema<TestUser> {
                    field "email" SqliteType.Text (fun u -> u.Email) [ Unique ]
                    field "age" SqliteType.Integer (fun u -> int64 u.Age) []

                    validate (fun u ->
                        if u.Age < 18 then Error "Must be 18 or older"
                        elif not (u.Email.Contains("@")) then Error "Invalid email"
                        else Ok u)
                }

            let users = db.Collection<TestUser>("users_validated", validatedSchema)

            // Act & Assert - Valid user
            let validUser =
                { Name = "Alice"
                  Email = "alice@test.com"
                  Age = 25
                  Role = "user" }

            let! validResult =
                validatedSchema.Validate.Value validUser
                |> Result.bind (fun u -> users |> Collection.insertOne u)
                |> Task.FromResult
                |> Task.bind id

            match validResult with
            | Ok doc -> doc.Data.Name |> should equal "Alice"
            | Error err -> failwith $"Expected Ok, got Error: {err.Message}"

            // Act & Assert - Invalid user (too young)
            let invalidUser =
                { Name = "Bob"
                  Email = "bob@test.com"
                  Age = 15
                  Role = "user" }

            let! invalidResult =
                validatedSchema.Validate.Value invalidUser
                |> Result.bind (fun u -> users |> Collection.insertOne u)
                |> Task.FromResult
                |> Task.bind id

            match invalidResult with
            | Ok _ -> failwith "Expected Error, got Ok"
            | Error(FractalError.Validation _) -> ()
            | Error err -> failwith $"Expected ValidationError, got {err}"
        }

    [<Fact>]
    member _.``All four builders work together in complex scenario``() : Task =
        task {
            // Arrange
            let db = fixture.Db

            let userSchema =
                schema<TestUser> {
                    field "name" SqliteType.Text (fun u -> u.Name) [ Indexed ]
                    field "email" SqliteType.Text (fun u -> u.Email) [ Unique; Indexed ]
                    field "age" SqliteType.Integer (fun u -> int64 u.Age) [ Indexed ]
                    field "role" SqliteType.Text (fun u -> u.Role) [ Indexed ]
                    index "idx_role_age" [ "role"; "age" ]
                }

            let users = db.Collection<TestUser>("users_all_builders", userSchema)

            // Act - Use TransactionBuilder to insert multiple users
            let! txResult =
                db.Transact {
                    let! doc1 =
                        users
                        |> Collection.insertOne
                            { Name = "Alice"
                              Email = "alice@test.com"
                              Age = 30
                              Role = "admin" }

                    let! doc2 =
                        users
                        |> Collection.insertOne
                            { Name = "Bob"
                              Email = "bob@test.com"
                              Age = 25
                              Role = "user" }

                    let! doc3 =
                        users
                        |> Collection.insertOne
                            { Name = "Charlie"
                              Email = "charlie@test.com"
                              Age = 35
                              Role = "user" }

                    return (doc1.Id, doc2.Id, doc3.Id)
                }

            txResult |> should be (ofCase <@ Ok @>)

            // Use QueryBuilder and OptionsBuilder to query
            let searchQuery =
                query {
                    field "role" (Query.eq "user")
                    field "age" (Query.gte 25L)
                }

            let searchOptions =
                options {
                    sortBy "age" Ascending
                    limit 5
                    project [ "_id"; "name"; "age" ]
                }

            let! searchResults = users |> Collection.find searchQuery searchOptions

            // Assert
            searchResults |> should haveLength 2
            searchResults.[0].Data.Name |> should equal "Bob" // Age 25
            searchResults.[1].Data.Name |> should equal "Charlie" // Age 35
        }
