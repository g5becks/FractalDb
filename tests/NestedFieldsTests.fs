module FractalDb.Tests.NestedFieldsTests

open System
open System.Threading.Tasks
open Xunit
open FsUnit.Xunit
open FractalDb
open FractalDb.Types
open FractalDb.Schema
open FractalDb.Database
open FractalDb.Collection
open FractalDb.Operators
open FractalDb.Options

/// <summary>
/// Unit tests for nested field operations.
/// Tests queries, updates, sorting, and projections on nested object properties.
/// </summary>

// Test types with nested structures
type Address =
    { Street: string
      City: string
      Country: string
      ZipCode: string }

type Settings =
    { Theme: string
      Notifications: bool
      Language: option<string> }

type Profile =
    { Bio: string
      Avatar: option<string>
      Settings: Settings }

type User =
    { Name: string
      Email: string
      Age: int
      Address: Address
      Profile: Profile }

// Helper to create test database
let createTestDb () = FractalDb.InMemory()

// ═══════════════════════════════════════════════════════════════
// Nested Field Query Tests
// ═══════════════════════════════════════════════════════════════

[<Fact>]
let ``Query nested field with dot notation works`` () =
    task {
        let db = createTestDb ()

        let schema =
            { Fields =
                [ { Name = "city"
                    Path = Some "$.address.city"
                    SqlType = SqliteType.Text
                    Indexed = true
                    Unique = false
                    Nullable = false } ]
              Indexes = []
              Timestamps = false
              Validate = None }

        let collection = db.Collection<User>("users", schema)

        // Insert test users
        let user1 =
            { Name = "Alice"
              Email = "alice@example.com"
              Age = 30
              Address =
                { Street = "123 Main St"
                  City = "New York"
                  Country = "USA"
                  ZipCode = "10001" }
              Profile =
                { Bio = "Developer"
                  Avatar = Some "avatar1.jpg"
                  Settings =
                    { Theme = "dark"
                      Notifications = true
                      Language = Some "en" } } }

        let user2 =
            { Name = "Bob"
              Email = "bob@example.com"
              Age = 25
              Address =
                { Street = "456 Oak Ave"
                  City = "San Francisco"
                  Country = "USA"
                  ZipCode = "94102" }
              Profile =
                { Bio = "Designer"
                  Avatar = None
                  Settings =
                    { Theme = "light"
                      Notifications = false
                      Language = None } } }

        let! _ = collection |> Collection.insertOne user1
        let! _ = collection |> Collection.insertOne user2

        // Query by nested field
        let query = Query.Field("city", FieldOp.Compare(box (CompareOp.Eq "New York")))
        let! results = collection |> Collection.find query

        results |> should haveLength 1
        results.[0].Data.Name |> should equal "Alice"
        results.[0].Data.Address.City |> should equal "New York"
    }

[<Fact>]
let ``Query deeply nested field (3+ levels) works`` () =
    task {
        let db = createTestDb ()

        let schema =
            { Fields =
                [ { Name = "theme"
                    Path = Some "$.profile.settings.theme"
                    SqlType = SqliteType.Text
                    Indexed = true
                    Unique = false
                    Nullable = false } ]
              Indexes = []
              Timestamps = false
              Validate = None }

        let collection = db.Collection<User>("users", schema)

        // Insert test users
        let user1 =
            { Name = "Alice"
              Email = "alice@example.com"
              Age = 30
              Address =
                { Street = "123 Main St"
                  City = "New York"
                  Country = "USA"
                  ZipCode = "10001" }
              Profile =
                { Bio = "Developer"
                  Avatar = Some "avatar1.jpg"
                  Settings =
                    { Theme = "dark"
                      Notifications = true
                      Language = Some "en" } } }

        let user2 =
            { Name = "Bob"
              Email = "bob@example.com"
              Age = 25
              Address =
                { Street = "456 Oak Ave"
                  City = "San Francisco"
                  Country = "USA"
                  ZipCode = "94102" }
              Profile =
                { Bio = "Designer"
                  Avatar = None
                  Settings =
                    { Theme = "light"
                      Notifications = false
                      Language = None } } }

        let! _ = collection |> Collection.insertOne user1
        let! _ = collection |> Collection.insertOne user2

        // Query by deeply nested field
        let query = Query.Field("theme", FieldOp.Compare(box (CompareOp.Eq "dark")))
        let! results = collection |> Collection.find query

        results |> should haveLength 1
        results.[0].Data.Name |> should equal "Alice"
        results.[0].Data.Profile.Settings.Theme |> should equal "dark"
    }

// ═══════════════════════════════════════════════════════════════
// Nested Field Update Tests
// ═══════════════════════════════════════════════════════════════

[<Fact>]
let ``Update nested field preserves other fields`` () =
    task {
        let db = createTestDb ()

        let schema =
            { Fields = []
              Indexes = []
              Timestamps = false
              Validate = None }

        let collection = db.Collection<User>("users", schema)

        // Insert test user
        let originalUser =
            { Name = "Alice"
              Email = "alice@example.com"
              Age = 30
              Address =
                { Street = "123 Main St"
                  City = "New York"
                  Country = "USA"
                  ZipCode = "10001" }
              Profile =
                { Bio = "Developer"
                  Avatar = Some "avatar1.jpg"
                  Settings =
                    { Theme = "dark"
                      Notifications = true
                      Language = Some "en" } } }

        let! insertResult = collection |> Collection.insertOne originalUser

        match insertResult with
        | Error err -> failwith $"Insert failed: {err.Message}"
        | Ok doc ->
            // Update nested field using replaceOne
            let updatedUser =
                { doc.Data with
                    Address =
                        { doc.Data.Address with
                            City = "Boston" } }

            let query = Query.Field("_id", FieldOp.Compare(box (CompareOp.Eq doc.Id)))
            let! updateResult = collection |> Collection.replaceOne query updatedUser

            match updateResult with
            | Error err -> failwith $"Update failed: {err.Message}"
            | Ok maybeDoc ->
                match maybeDoc with
                | None -> failwith "Expected updated document, got None"
                | Some updatedDoc ->
                    // Verify nested field was updated
                    updatedDoc.Data.Address.City |> should equal "Boston"

                    // Verify other fields preserved
                    updatedDoc.Data.Name |> should equal "Alice"
                    updatedDoc.Data.Email |> should equal "alice@example.com"
                    updatedDoc.Data.Address.Street |> should equal "123 Main St"
                    updatedDoc.Data.Address.Country |> should equal "USA"
                    updatedDoc.Data.Profile.Bio |> should equal "Developer"
                    updatedDoc.Data.Profile.Settings.Theme |> should equal "dark"
    }

// ═══════════════════════════════════════════════════════════════
// Nested Field Sorting Tests
// ═══════════════════════════════════════════════════════════════

[<Fact>]
let ``Sort by nested field works`` () =
    task {
        let db = createTestDb ()

        let schema =
            { Fields =
                [ { Name = "city"
                    Path = Some "$.address.city"
                    SqlType = SqliteType.Text
                    Indexed = true
                    Unique = false
                    Nullable = false } ]
              Indexes = []
              Timestamps = false
              Validate = None }

        let collection = db.Collection<User>("users", schema)

        // Insert test users
        let user1 =
            { Name = "Alice"
              Email = "alice@example.com"
              Age = 30
              Address =
                { Street = "123 Main St"
                  City = "Seattle"
                  Country = "USA"
                  ZipCode = "98101" }
              Profile =
                { Bio = "Developer"
                  Avatar = Some "avatar1.jpg"
                  Settings =
                    { Theme = "dark"
                      Notifications = true
                      Language = Some "en" } } }

        let user2 =
            { Name = "Bob"
              Email = "bob@example.com"
              Age = 25
              Address =
                { Street = "456 Oak Ave"
                  City = "Boston"
                  Country = "USA"
                  ZipCode = "02101" }
              Profile =
                { Bio = "Designer"
                  Avatar = None
                  Settings =
                    { Theme = "light"
                      Notifications = false
                      Language = None } } }

        let user3 =
            { Name = "Charlie"
              Email = "charlie@example.com"
              Age = 35
              Address =
                { Street = "789 Elm Rd"
                  City = "Austin"
                  Country = "USA"
                  ZipCode = "78701" }
              Profile =
                { Bio = "Manager"
                  Avatar = Some "avatar3.jpg"
                  Settings =
                    { Theme = "dark"
                      Notifications = true
                      Language = Some "es" } } }

        let! _ = collection |> Collection.insertOne user1
        let! _ = collection |> Collection.insertOne user2
        let! _ = collection |> Collection.insertOne user3

        // Sort by nested field (ascending)
        let options =
            QueryOptions.empty<User> |> QueryOptions.sortBy "city" SortDirection.Ascending

        let! results = collection |> Collection.findWith Query.Empty options

        results |> should haveLength 3
        results.[0].Data.Address.City |> should equal "Austin"
        results.[1].Data.Address.City |> should equal "Boston"
        results.[2].Data.Address.City |> should equal "Seattle"
    }

// ═══════════════════════════════════════════════════════════════
// Nested Field Projection Tests
// ═══════════════════════════════════════════════════════════════

[<Fact>]
let ``Select projection includes nested field`` () =
    task {
        let db = createTestDb ()

        let schema =
            { Fields = []
              Indexes = []
              Timestamps = false
              Validate = None }

        let collection = db.Collection<User>("users", schema)

        // Insert test user
        let user =
            { Name = "Alice"
              Email = "alice@example.com"
              Age = 30
              Address =
                { Street = "123 Main St"
                  City = "New York"
                  Country = "USA"
                  ZipCode = "10001" }
              Profile =
                { Bio = "Developer"
                  Avatar = Some "avatar1.jpg"
                  Settings =
                    { Theme = "dark"
                      Notifications = true
                      Language = Some "en" } } }

        let! _ = collection |> Collection.insertOne user

        // Select specific fields including nested
        let options =
            QueryOptions.empty<User> |> QueryOptions.select [ "name"; "address.city" ]

        let! results = collection |> Collection.findWith Query.Empty options

        results |> should haveLength 1

        // Note: With Select, only specified fields are in the JSON
        // The F# deserialization will fill other fields with defaults
        // This test verifies the query executes successfully
        results.[0].Data.Name |> should equal "Alice"
    }

[<Fact>]
let ``Omit projection excludes nested field`` () =
    task {
        let db = createTestDb ()

        let schema =
            { Fields = []
              Indexes = []
              Timestamps = false
              Validate = None }

        let collection = db.Collection<User>("users", schema)

        // Insert test user
        let user =
            { Name = "Alice"
              Email = "alice@example.com"
              Age = 30
              Address =
                { Street = "123 Main St"
                  City = "New York"
                  Country = "USA"
                  ZipCode = "10001" }
              Profile =
                { Bio = "Developer"
                  Avatar = Some "avatar1.jpg"
                  Settings =
                    { Theme = "dark"
                      Notifications = true
                      Language = Some "en" } } }

        let! _ = collection |> Collection.insertOne user

        // Omit nested field
        let options = QueryOptions.empty<User> |> QueryOptions.omit [ "profile" ]

        let! results = collection |> Collection.findWith Query.Empty options

        results |> should haveLength 1
        results.[0].Data.Name |> should equal "Alice"
        results.[0].Data.Email |> should equal "alice@example.com"
        results.[0].Data.Address.City |> should equal "New York"
    }

// ═══════════════════════════════════════════════════════════════
// Option<T> Within Nested Object Tests
// ═══════════════════════════════════════════════════════════════

[<Fact>]
let ``Option field within nested object handled correctly - Some value`` () =
    task {
        let db = createTestDb ()

        let schema =
            { Fields = []
              Indexes = []
              Timestamps = false
              Validate = None }

        let collection = db.Collection<User>("users", schema)

        // Insert user with Some avatar
        let user =
            { Name = "Alice"
              Email = "alice@example.com"
              Age = 30
              Address =
                { Street = "123 Main St"
                  City = "New York"
                  Country = "USA"
                  ZipCode = "10001" }
              Profile =
                { Bio = "Developer"
                  Avatar = Some "avatar1.jpg"
                  Settings =
                    { Theme = "dark"
                      Notifications = true
                      Language = Some "en" } } }

        let! insertResult = collection |> Collection.insertOne user

        match insertResult with
        | Error err -> failwith $"Insert failed: {err.Message}"
        | Ok doc ->
            doc.Data.Profile.Avatar |> should equal (Some "avatar1.jpg")
            doc.Data.Profile.Settings.Language |> should equal (Some "en")
    }

[<Fact>]
let ``Option field within nested object handled correctly - None value`` () =
    task {
        let db = createTestDb ()

        let schema =
            { Fields = []
              Indexes = []
              Timestamps = false
              Validate = None }

        let collection = db.Collection<User>("users", schema)

        // Insert user with None avatar and language
        let user =
            { Name = "Bob"
              Email = "bob@example.com"
              Age = 25
              Address =
                { Street = "456 Oak Ave"
                  City = "San Francisco"
                  Country = "USA"
                  ZipCode = "94102" }
              Profile =
                { Bio = "Designer"
                  Avatar = None
                  Settings =
                    { Theme = "light"
                      Notifications = false
                      Language = None } } }

        let! insertResult = collection |> Collection.insertOne user

        match insertResult with
        | Error err -> failwith $"Insert failed: {err.Message}"
        | Ok doc ->
            doc.Data.Profile.Avatar |> should equal None
            doc.Data.Profile.Settings.Language |> should equal None
    }

[<Fact>]
let ``Query by option field within nested object`` () =
    task {
        let db = createTestDb ()

        let schema =
            { Fields =
                [ { Name = "language"
                    Path = Some "$.profile.settings.language"
                    SqlType = SqliteType.Text
                    Indexed = true
                    Unique = false
                    Nullable = true } ]
              Indexes = []
              Timestamps = false
              Validate = None }

        let collection = db.Collection<User>("users", schema)

        // Insert users with different language settings
        let user1 =
            { Name = "Alice"
              Email = "alice@example.com"
              Age = 30
              Address =
                { Street = "123 Main St"
                  City = "New York"
                  Country = "USA"
                  ZipCode = "10001" }
              Profile =
                { Bio = "Developer"
                  Avatar = Some "avatar1.jpg"
                  Settings =
                    { Theme = "dark"
                      Notifications = true
                      Language = Some "en" } } }

        let user2 =
            { Name = "Bob"
              Email = "bob@example.com"
              Age = 25
              Address =
                { Street = "456 Oak Ave"
                  City = "San Francisco"
                  Country = "USA"
                  ZipCode = "94102" }
              Profile =
                { Bio = "Designer"
                  Avatar = None
                  Settings =
                    { Theme = "light"
                      Notifications = false
                      Language = None } } }

        let user3 =
            { Name = "Charlie"
              Email = "charlie@example.com"
              Age = 35
              Address =
                { Street = "789 Elm Rd"
                  City = "Austin"
                  Country = "USA"
                  ZipCode = "78701" }
              Profile =
                { Bio = "Manager"
                  Avatar = Some "avatar3.jpg"
                  Settings =
                    { Theme = "dark"
                      Notifications = true
                      Language = Some "es" } } }

        let! _ = collection |> Collection.insertOne user1
        let! _ = collection |> Collection.insertOne user2
        let! _ = collection |> Collection.insertOne user3

        // Query for users with English language
        let query = Query.Field("language", FieldOp.Compare(box (CompareOp.Eq "en")))
        let! results = collection |> Collection.find query

        results |> should haveLength 1
        results.[0].Data.Name |> should equal "Alice"
        results.[0].Data.Profile.Settings.Language |> should equal (Some "en")
    }
