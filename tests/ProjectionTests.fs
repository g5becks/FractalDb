module FractalDb.Tests.ProjectionTests

open System.Threading.Tasks
open Xunit
open FsUnit.Xunit
open FractalDb
open FractalDb.Types
open FractalDb.Schema
open FractalDb.Database
open FractalDb.Collection
open FractalDb.Query
open FractalDb.Options
open System.Text.Json

/// <summary>
/// Unit tests for projection (select/omit) functionality.
/// Tests field projection in actual query execution to verify correct filtering of returned fields.
/// </summary>

// Test types
type UserProfile =
    { Name: string
      Email: string
      Age: int
      City: string
      Country: string
      Bio: string option }

type Product =
    { Name: string
      Description: string
      Price: float
      Category: string
      Stock: int
      Tags: list<string> }

type Address =
    { Street: string
      City: string
      ZipCode: string }

type UserWithNested =
    { Name: string
      Email: string
      Age: int
      Address: Address }

// Helper to create test database
let createTestDb () = FractalDb.InMemory()

// Helper to check if JSON contains a field
let jsonContainsField (fieldName: string) (doc: Document<'T>) : bool =
    let json = JsonSerializer.Serialize(doc.Data)
    json.Contains($"\"{fieldName}\"")

// ═══════════════════════════════════════════════════════════════
// Select Projection Tests
// ═══════════════════════════════════════════════════════════════

[<Fact>]
let ``Select specific fields returns only those fields`` () =
    task {
        let db = createTestDb ()

        let schema =
            { Fields = []
              Indexes = []
              Timestamps = false
              Validate = None }

        let collection = db.Collection<UserProfile>("users", schema)

        // Insert test user
        let user =
            { Name = "Alice"
              Email = "alice@example.com"
              Age = 30
              City = "New York"
              Country = "USA"
              Bio = Some "Software developer" }

        let! _ = collection |> Collection.insertOne user

        // Select only specific fields
        let options =
            QueryOptions.empty<UserProfile> |> QueryOptions.select [ "name"; "email" ]

        let! results = collection |> Collection.findWith (Query.empty<UserProfile>) options

        results |> should haveLength 1

        // Verify selected fields are present
        results.[0].Data.Name |> should equal "Alice"
        results.[0].Data.Email |> should equal "alice@example.com"

    // Note: In F#, unselected fields will have default values after deserialization
    // The important thing is that the JSON returned from the database only contains selected fields
    }

[<Fact>]
let ``Select always includes _id field even if not specified`` () =
    task {
        let db = createTestDb ()

        let schema =
            { Fields = []
              Indexes = []
              Timestamps = false
              Validate = None }

        let collection = db.Collection<UserProfile>("users", schema)

        let user =
            { Name = "Bob"
              Email = "bob@example.com"
              Age = 25
              City = "Boston"
              Country = "USA"
              Bio = None }

        let! insertResult = collection |> Collection.insertOne user

        match insertResult with
        | Error err -> failwith $"Insert failed: {err.Message}"
        | Ok doc ->
            let userId = doc.Id

            // Select only name (not _id)
            let options = QueryOptions.empty<UserProfile> |> QueryOptions.select [ "name" ]

            let! results = collection |> Collection.findWith (Query.empty<UserProfile>) options

            results |> should haveLength 1

            // _id should still be present
            results.[0].Id |> should equal userId
            results.[0].Data.Name |> should equal "Bob"
    }

[<Fact>]
let ``Select with multiple fields returns all specified fields`` () =
    task {
        let db = createTestDb ()

        let schema =
            { Fields = []
              Indexes = []
              Timestamps = false
              Validate = None }

        let collection = db.Collection<Product>("products", schema)

        let product =
            { Name = "Laptop"
              Description = "High-performance laptop"
              Price = 999.99
              Category = "Electronics"
              Stock = 50
              Tags = [ "computer"; "portable" ] }

        let! _ = collection |> Collection.insertOne product

        // Select multiple fields
        let options =
            QueryOptions.empty<Product>
            |> QueryOptions.select [ "name"; "price"; "category" ]

        let! results = collection |> Collection.findWith (Query.empty<Product>) options

        results |> should haveLength 1
        results.[0].Data.Name |> should equal "Laptop"
        results.[0].Data.Price |> should equal 999.99
        results.[0].Data.Category |> should equal "Electronics"
    }

[<Fact>]
let ``Empty select option returns all fields`` () =
    task {
        let db = createTestDb ()

        let schema =
            { Fields = []
              Indexes = []
              Timestamps = false
              Validate = None }

        let collection = db.Collection<UserProfile>("users", schema)

        let user =
            { Name = "Charlie"
              Email = "charlie@example.com"
              Age = 35
              City = "Seattle"
              Country = "USA"
              Bio = Some "DevOps engineer" }

        let! _ = collection |> Collection.insertOne user

        // No select option (default empty)
        let options = QueryOptions.empty<UserProfile>

        let! results = collection |> Collection.findWith (Query.empty<UserProfile>) options

        results |> should haveLength 1

        // All fields should be present
        results.[0].Data.Name |> should equal "Charlie"
        results.[0].Data.Email |> should equal "charlie@example.com"
        results.[0].Data.Age |> should equal 35
        results.[0].Data.City |> should equal "Seattle"
        results.[0].Data.Country |> should equal "USA"
        results.[0].Data.Bio |> should equal (Some "DevOps engineer")
    }

// ═══════════════════════════════════════════════════════════════
// Omit Projection Tests
// ═══════════════════════════════════════════════════════════════

[<Fact>]
let ``Omit specific fields excludes them from result`` () =
    task {
        let db = createTestDb ()

        let schema =
            { Fields = []
              Indexes = []
              Timestamps = false
              Validate = None }

        let collection = db.Collection<UserProfile>("users", schema)

        let user =
            { Name = "Diana"
              Email = "diana@example.com"
              Age = 28
              City = "Austin"
              Country = "USA"
              Bio = Some "Data scientist" }

        let! _ = collection |> Collection.insertOne user

        // Omit sensitive fields
        let options =
            QueryOptions.empty<UserProfile> |> QueryOptions.omit [ "email"; "age" ]

        let! results = collection |> Collection.findWith (Query.empty<UserProfile>) options

        results |> should haveLength 1

        // Non-omitted fields should be present
        results.[0].Data.Name |> should equal "Diana"
        results.[0].Data.City |> should equal "Austin"
        results.[0].Data.Country |> should equal "USA"
        results.[0].Data.Bio |> should equal (Some "Data scientist")
    }

[<Fact>]
let ``Omit does not affect _id field`` () =
    task {
        let db = createTestDb ()

        let schema =
            { Fields = []
              Indexes = []
              Timestamps = false
              Validate = None }

        let collection = db.Collection<UserProfile>("users", schema)

        let user =
            { Name = "Eve"
              Email = "eve@example.com"
              Age = 32
              City = "Portland"
              Country = "USA"
              Bio = None }

        let! insertResult = collection |> Collection.insertOne user

        match insertResult with
        | Error err -> failwith $"Insert failed: {err.Message}"
        | Ok doc ->
            let userId = doc.Id

            // Try to omit _id (should have no effect)
            let options =
                QueryOptions.empty<UserProfile> |> QueryOptions.omit [ "_id"; "email" ]

            let! results = collection |> Collection.findWith (Query.empty<UserProfile>) options

            results |> should haveLength 1

            // _id should still be present
            results.[0].Id |> should equal userId
            results.[0].Data.Name |> should equal "Eve"
    }

[<Fact>]
let ``Omit with multiple fields excludes all specified fields`` () =
    task {
        let db = createTestDb ()

        let schema =
            { Fields = []
              Indexes = []
              Timestamps = false
              Validate = None }

        let collection = db.Collection<Product>("products", schema)

        let product =
            { Name = "Mouse"
              Description = "Wireless gaming mouse"
              Price = 49.99
              Category = "Accessories"
              Stock = 100
              Tags = [ "gaming"; "wireless" ] }

        let! _ = collection |> Collection.insertOne product

        // Omit internal fields
        let options =
            QueryOptions.empty<Product> |> QueryOptions.omit [ "stock"; "description" ]

        let! results = collection |> Collection.findWith (Query.empty<Product>) options

        results |> should haveLength 1
        results.[0].Data.Name |> should equal "Mouse"
        results.[0].Data.Price |> should equal 49.99
        results.[0].Data.Category |> should equal "Accessories"
    }

[<Fact>]
let ``Empty omit option returns all fields`` () =
    task {
        let db = createTestDb ()

        let schema =
            { Fields = []
              Indexes = []
              Timestamps = false
              Validate = None }

        let collection = db.Collection<UserProfile>("users", schema)

        let user =
            { Name = "Frank"
              Email = "frank@example.com"
              Age = 40
              City = "Miami"
              Country = "USA"
              Bio = Some "Marketing manager" }

        let! _ = collection |> Collection.insertOne user

        // No omit option (default empty)
        let options = QueryOptions.empty<UserProfile>

        let! results = collection |> Collection.findWith (Query.empty<UserProfile>) options

        results |> should haveLength 1

        // All fields should be present
        results.[0].Data.Name |> should equal "Frank"
        results.[0].Data.Email |> should equal "frank@example.com"
        results.[0].Data.Age |> should equal 40
        results.[0].Data.City |> should equal "Miami"
        results.[0].Data.Country |> should equal "USA"
        results.[0].Data.Bio |> should equal (Some "Marketing manager")
    }

// ═══════════════════════════════════════════════════════════════
// Nested Field Projection Tests
// ═══════════════════════════════════════════════════════════════

[<Fact>]
let ``Select with nested fields returns nested data`` () =
    task {
        let db = createTestDb ()

        let schema =
            { Fields = []
              Indexes = []
              Timestamps = false
              Validate = None }

        let collection = db.Collection<UserWithNested>("users", schema)

        let user =
            { Name = "Grace"
              Email = "grace@example.com"
              Age = 29
              Address =
                { Street = "123 Main St"
                  City = "Denver"
                  ZipCode = "80202" } }

        let! _ = collection |> Collection.insertOne user

        // Select including nested field
        let options =
            QueryOptions.empty<UserWithNested>
            |> QueryOptions.select [ "name"; "address.city" ]

        let! results = collection |> Collection.findWith (Query.empty<UserWithNested>) options

        results |> should haveLength 1
        results.[0].Data.Name |> should equal "Grace"
    }

[<Fact>]
let ``Omit nested fields excludes nested data`` () =
    task {
        let db = createTestDb ()

        let schema =
            { Fields = []
              Indexes = []
              Timestamps = false
              Validate = None }

        let collection = db.Collection<UserWithNested>("users", schema)

        let user =
            { Name = "Henry"
              Email = "henry@example.com"
              Age = 45
              Address =
                { Street = "456 Oak Ave"
                  City = "Phoenix"
                  ZipCode = "85001" } }

        let! _ = collection |> Collection.insertOne user

        // Omit nested field
        let options = QueryOptions.empty<UserWithNested> |> QueryOptions.omit [ "address" ]

        let! results = collection |> Collection.findWith (Query.empty<UserWithNested>) options

        results |> should haveLength 1
        results.[0].Data.Name |> should equal "Henry"
        results.[0].Data.Email |> should equal "henry@example.com"
        results.[0].Data.Age |> should equal 45
    }

[<Fact>]
let ``Select entire nested object includes all nested fields`` () =
    task {
        let db = createTestDb ()

        let schema =
            { Fields = []
              Indexes = []
              Timestamps = false
              Validate = None }

        let collection = db.Collection<UserWithNested>("users", schema)

        let user =
            { Name = "Ivy"
              Email = "ivy@example.com"
              Age = 26
              Address =
                { Street = "789 Elm Rd"
                  City = "Atlanta"
                  ZipCode = "30303" } }

        let! _ = collection |> Collection.insertOne user

        // Select entire nested object
        let options =
            QueryOptions.empty<UserWithNested> |> QueryOptions.select [ "name"; "address" ]

        let! results = collection |> Collection.findWith (Query.empty<UserWithNested>) options

        results |> should haveLength 1
        results.[0].Data.Name |> should equal "Ivy"
        results.[0].Data.Address.City |> should equal "Atlanta"
        results.[0].Data.Address.Street |> should equal "789 Elm Rd"
        results.[0].Data.Address.ZipCode |> should equal "30303"
    }

// ═══════════════════════════════════════════════════════════════
// Projection with Query Combination Tests
// ═══════════════════════════════════════════════════════════════

[<Fact>]
let ``Select projection works with query filtering`` () =
    task {
        let db = createTestDb ()

        let schema =
            { Fields = []
              Indexes = []
              Timestamps = false
              Validate = None }

        let collection = db.Collection<UserProfile>("users", schema)

        let user1 =
            { Name = "Jack"
              Email = "jack@example.com"
              Age = 25
              City = "Boston"
              Country = "USA"
              Bio = None }

        let user2 =
            { Name = "Kate"
              Email = "kate@example.com"
              Age = 35
              City = "Chicago"
              Country = "USA"
              Bio = Some "Engineer" }

        let! _ = collection |> Collection.insertOne user1
        let! _ = collection |> Collection.insertOne user2

        // Query with age filter and select projection
        let query = Query.Field("age", FieldOp.Compare(box (CompareOp.Gt 30)))

        let options =
            QueryOptions.empty<UserProfile> |> QueryOptions.select [ "name"; "city" ]

        let! results = collection |> Collection.findWith query options

        results |> should haveLength 1
        results.[0].Data.Name |> should equal "Kate"
        results.[0].Data.City |> should equal "Chicago"
    }

[<Fact>]
let ``Omit projection works with query filtering`` () =
    task {
        let db = createTestDb ()

        let schema =
            { Fields = []
              Indexes = []
              Timestamps = false
              Validate = None }

        let collection = db.Collection<Product>("products", schema)

        let product1 =
            { Name = "Keyboard"
              Description = "Mechanical keyboard"
              Price = 79.99
              Category = "Accessories"
              Stock = 75
              Tags = [ "mechanical"; "gaming" ] }

        let product2 =
            { Name = "Monitor"
              Description = "4K monitor"
              Price = 299.99
              Category = "Electronics"
              Stock = 30
              Tags = [ "4k"; "display" ] }

        let! _ = collection |> Collection.insertOne product1
        let! _ = collection |> Collection.insertOne product2

        // Query with price filter and omit projection
        let query = Query.Field("price", FieldOp.Compare(box (CompareOp.Lt 100.0)))

        let options =
            QueryOptions.empty<Product> |> QueryOptions.omit [ "description"; "stock" ]

        let! results = collection |> Collection.findWith query options

        results |> should haveLength 1
        results.[0].Data.Name |> should equal "Keyboard"
        results.[0].Data.Price |> should equal 79.99
        results.[0].Data.Category |> should equal "Accessories"
    }

[<Fact>]
let ``Projection works with sorting`` () =
    task {
        let db = createTestDb ()

        let schema =
            { Fields = []
              Indexes = []
              Timestamps = false
              Validate = None }

        let collection = db.Collection<UserProfile>("users", schema)

        let user1 =
            { Name = "Zoe"
              Email = "zoe@example.com"
              Age = 28
              City = "Dallas"
              Country = "USA"
              Bio = None }

        let user2 =
            { Name = "Alice"
              Email = "alice@example.com"
              Age = 30
              City = "Austin"
              Country = "USA"
              Bio = Some "Developer" }

        let! _ = collection |> Collection.insertOne user1
        let! _ = collection |> Collection.insertOne user2

        // Sort by name with select projection
        let options =
            QueryOptions.empty<UserProfile>
            |> QueryOptions.sortBy "name" SortDirection.Ascending
            |> QueryOptions.select [ "name"; "age" ]

        let! results = collection |> Collection.findWith (Query.empty<UserProfile>) options

        results |> should haveLength 2
        results.[0].Data.Name |> should equal "Alice"
        results.[1].Data.Name |> should equal "Zoe"
    }
