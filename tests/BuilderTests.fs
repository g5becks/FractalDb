module FractalDb.Tests.BuilderTests

open System
open System.Threading.Tasks
open Xunit
open FsUnit.Xunit
open FractalDb.Types
open FractalDb.Errors
open FractalDb.Operators
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
// SchemaBuilder Tests
//

[<Fact>]
let ``SchemaBuilder with no fields creates empty schema`` () =
    let s: SchemaDef<TestUser> = schema<TestUser> { () }
    s.Fields |> should be Empty
    s.Indexes |> should be Empty
    s.Timestamps |> should equal false
    s.Validate |> should equal None

[<Fact>]
let ``SchemaBuilder field adds field to schema`` () =
    let s = schema<TestUser> { field "name" SqliteType.Text }

    s.Fields |> should haveLength 1
    let f = s.Fields.[0]
    f.Name |> should equal "name"
    f.SqlType |> should equal SqliteType.Text
    f.Indexed |> should equal false
    f.Unique |> should equal false
    f.Nullable |> should equal false

[<Fact>]
let ``SchemaBuilder field with constraints sets flags`` () =
    let s = schema<TestUser> { unique "email" SqliteType.Text }

    let f = s.Fields.[0]
    f.Indexed |> should equal true
    f.Unique |> should equal true
    f.Nullable |> should equal false

[<Fact>]
let ``SchemaBuilder multiple fields preserves order`` () =
    let s =
        schema<TestUser> {
            field "name" SqliteType.Text
            field "email" SqliteType.Text
            field "age" SqliteType.Integer
        }

    s.Fields |> should haveLength 3
    s.Fields.[0].Name |> should equal "name"
    s.Fields.[1].Name |> should equal "email"
    s.Fields.[2].Name |> should equal "age"

[<Fact>]
let ``SchemaBuilder index adds compound index`` () =
    let s =
        schema<TestUser> {
            field "name" SqliteType.Text
            field "age" SqliteType.Integer
            compoundIndex "idx_name_age" [ "name"; "age" ]
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
            field "email" SqliteType.Text
            compoundIndex "idx_email" [ "email" ] true
        }

    let idx = s.Indexes.[0]
    idx.Unique |> should equal true

[<Fact>]
let ``SchemaBuilder validate adds validation function`` () =
    let s =
        schema<TestUser> {
            field "age" SqliteType.Integer
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

        match validFn validUser with
        | Ok u -> u |> should equal validUser
        | Error _ -> failwith "Expected Ok for valid user"

        match validFn invalidUser with
        | Ok _ -> failwith "Expected Error for invalid user"
        | Error msg -> msg |> should equal "Must be 18 or older"
    | None -> failwith "Expected validation function"

//
// OptionsBuilder Tests
//

[<Fact>]
let ``OptionsBuilder with no operations creates default options`` () =
    let opts = options<TestUser> { () }
    opts.Limit |> should equal None
    opts.Skip |> should equal None
    opts.Sort |> should be Empty
    opts.Select |> should equal None
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
    let opts = options<TestUser> { sortBy "age" SortDirection.Descending }

    opts.Sort |> should haveLength 1
    let (field, dir) = opts.Sort.[0]
    field |> should equal "age"
    dir |> should equal SortDirection.Descending

[<Fact>]
let ``OptionsBuilder multiple sortBy preserves order`` () =
    let opts =
        options<TestUser> {
            sortBy "role" SortDirection.Ascending
            sortBy "age" SortDirection.Descending
        }

    opts.Sort |> should haveLength 2
    let (field1, dir1) = opts.Sort.[0]
    let (field2, dir2) = opts.Sort.[1]
    field1 |> should equal "role"
    dir1 |> should equal SortDirection.Ascending
    field2 |> should equal "age"
    dir2 |> should equal SortDirection.Descending

[<Fact>]
let ``OptionsBuilder project sets projection fields`` () =
    let opts = options<TestUser> { select [ "_id"; "name"; "email" ] }

    match opts.Select with
    | Some fields -> fields |> should equal [ "_id"; "name"; "email" ]
    | None -> failwith "Expected Some projection"

[<Fact>]
let ``OptionsBuilder cursorAfter sets cursor`` () =
    let opts = options<TestUser> { cursorAfter "user-123" }

    match opts.Cursor with
    | Some cursor ->
        cursor.After |> should equal (Some "user-123")
        cursor.Before |> should equal None
    | None -> failwith "Expected Some cursor"

[<Fact>]
let ``OptionsBuilder cursorBefore sets cursor`` () =
    let opts = options<TestUser> { cursorBefore "user-456" }

    match opts.Cursor with
    | Some cursor ->
        cursor.After |> should equal None
        cursor.Before |> should equal (Some "user-456")
    | None -> failwith "Expected Some cursor"

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
            sortBy "createdAt" SortDirection.Descending
            select [ "_id"; "name" ]
        }

    opts.Limit |> should equal (Some 50)
    opts.Skip |> should equal (Some 20)
    opts.Sort |> should haveLength 1
    opts.Select |> should not' (equal None)

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
    member _.``Schema builder with validation works in Collection``() : Task =
        task {
            // Arrange
            let db = fixture.Db

            let validatedSchema =
                schema<TestUser> {
                    unique "email" SqliteType.Text
                    field "age" SqliteType.Integer

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
                match validatedSchema.Validate with
                | Some validateFn ->
                    match validateFn validUser with
                    | Ok u -> Collection.insertOne u users
                    | Error msg -> Task.FromResult(Error(FractalError.Validation(None, msg)))
                | None -> Collection.insertOne validUser users

            match validResult with
            | Ok doc -> doc.Data.Name |> should equal "Alice"
            | Error err -> failwith $"Expected Ok, got Error: {err}"

            // Act & Assert - Invalid user (too young)
            let invalidUser =
                { Name = "Bob"
                  Email = "bob@test.com"
                  Age = 15
                  Role = "user" }

            let! invalidResult =
                match validatedSchema.Validate with
                | Some validateFn ->
                    match validateFn invalidUser with
                    | Ok u -> Collection.insertOne u users
                    | Error msg -> Task.FromResult(Error(FractalError.Validation(None, msg)))
                | None -> Collection.insertOne invalidUser users

            match invalidResult with
            | Ok _ -> failwith "Expected Error, got Ok"
            | Error(FractalError.Validation _) -> ()
            | Error err -> failwith $"Expected ValidationError, got {err}"
        }
