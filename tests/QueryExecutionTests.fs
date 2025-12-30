module FractalDb.Tests.QueryExecutionTests

// Suppress linter warnings for test code
// fsharplint:disable FL0072

open System
open System.Threading.Tasks
open Xunit
open FsUnit.Xunit
open FractalDb.Types
open FractalDb.Schema
open FractalDb.Operators
open FractalDb.Options
open FractalDb.Collection
open FractalDb.Database

/// <summary>
/// Integration tests for query execution and filtering.
/// Tests Query operators, combinators, and QueryOptions.
/// </summary>

type Product =
    { Name: string
      Category: string
      Price: int
      InStock: bool
      Tags: list<string> }

let productSchema: SchemaDef<Product> =
    { Fields =
        [ { Name = "name"
            Path = None
            SqlType = SqliteType.Text
            Indexed = true
            Unique = false
            Nullable = false }
          { Name = "category"
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
            Nullable = false }
          { Name = "inStock"
            Path = None
            SqlType = SqliteType.Integer
            Indexed = true
            Unique = false
            Nullable = false } ]
      Indexes = []
      Timestamps = true
      Validate = None }

/// <summary>
/// Test fixture providing shared in-memory database with sample products.
/// </summary>
type QueryTestFixture() =
    let db = FractalDb.InMemory()
    let products = db.Collection<Product>("products", productSchema)

    // Seed with sample data
    do
        let sampleProducts =
            [ { Name = "Laptop"
                Category = "Electronics"
                Price = 1200
                InStock = true
                Tags = [ "tech"; "portable" ] }
              { Name = "Mouse"
                Category = "Electronics"
                Price = 25
                InStock = true
                Tags = [ "tech"; "accessory" ] }
              { Name = "Keyboard"
                Category = "Electronics"
                Price = 75
                InStock = false
                Tags = [ "tech"; "accessory" ] }
              { Name = "Desk"
                Category = "Furniture"
                Price = 300
                InStock = true
                Tags = [ "office"; "large" ] }
              { Name = "Chair"
                Category = "Furniture"
                Price = 150
                InStock = true
                Tags = [ "office"; "comfort" ] }
              { Name = "Monitor"
                Category = "Electronics"
                Price = 400
                InStock = true
                Tags = [ "tech"; "display" ] }
              { Name = "Lamp"
                Category = "Furniture"
                Price = 50
                InStock = false
                Tags = [ "office"; "light" ] }
              { Name = "Tablet"
                Category = "Electronics"
                Price = 600
                InStock = true
                Tags = [ "tech"; "portable" ] }
              { Name = "Bookshelf"
                Category = "Furniture"
                Price = 200
                InStock = true
                Tags = [ "office"; "storage" ] }
              { Name = "Webcam"
                Category = "Electronics"
                Price = 100
                InStock = false
                Tags = [ "tech"; "accessory" ] } ]

        for product in sampleProducts do
            products
            |> Collection.insertOne product
            |> Async.AwaitTask
            |> Async.RunSynchronously
            |> ignore

    member _.Db = db
    member _.Products = products

    interface IDisposable with
        member _.Dispose() = db.Close()

/// <summary>
/// Query execution test suite.
/// </summary>
type QueryExecutionTests(fixture: QueryTestFixture) =
    let products = fixture.Products

    interface IClassFixture<QueryTestFixture>

    [<Fact>]
    member _.``Query.eq filters correctly``() : Task =
        task {
            // Query products in Electronics category
            let query =
                Query.Field("category", FieldOp.Compare(box (CompareOp.Eq "Electronics")))

            let! results = products |> Collection.find query

            results.Length |> should equal 6

            for doc in results do
                doc.Data.Category |> should equal "Electronics"
        }

    [<Fact>]
    member _.``Query.gte and Query.lt range query works``() : Task =
        task {
            // Query products with price >= 100 AND price < 500
            let query =
                Query.And
                    [ Query.Field("price", FieldOp.Compare(box (CompareOp.Gte 100)))
                      Query.Field("price", FieldOp.Compare(box (CompareOp.Lt 500))) ]

            let! results = products |> Collection.find query

            // Should match: Webcam (100), Chair (150), Bookshelf (200), Desk (300), Monitor (400)
            results.Length |> should equal 5

            for doc in results do
                doc.Data.Price |> should be (greaterThanOrEqualTo 100)
                doc.Data.Price |> should be (lessThan 500)
        }

    [<Fact>]
    member _.``Query.contains string operator works``() : Task =
        task {
            // Query products with name containing "top"
            let query = Query.Field("name", FieldOp.String(StringOp.Contains "top"))
            let! results = products |> Collection.find query

            // Should match: Laptop
            results.Length |> should equal 1
            results.[0].Data.Name |> should equal "Laptop"
        }

    [<Fact>]
    member _.``Query.isIn filters by list``() : Task =
        task {
            // Query products in Furniture or with specific names
            let query =
                Query.Field("category", FieldOp.Compare(box (CompareOp.In [ "Furniture" ])))

            let! results = products |> Collection.find query

            // Should match: Desk, Chair, Lamp, Bookshelf (4 furniture items)
            results.Length |> should equal 4

            for doc in results do
                doc.Data.Category |> should equal "Furniture"
        }

    [<Fact>]
    member _.``Query.And combines conditions with AND``() : Task =
        task {
            // Query Electronics products that are in stock
            let query =
                Query.And
                    [ Query.Field("category", FieldOp.Compare(box (CompareOp.Eq "Electronics")))
                      Query.Field("inStock", FieldOp.Compare(box (CompareOp.Eq true))) ]

            let! results = products |> Collection.find query

            // Should match: Laptop, Mouse, Monitor, Tablet (4 items)
            results.Length |> should equal 4

            for doc in results do
                doc.Data.Category |> should equal "Electronics"
                doc.Data.InStock |> should equal true
        }

    [<Fact>]
    member _.``Query.Or combines conditions with OR``() : Task =
        task {
            // Query products that are either very cheap (<= 50) OR very expensive (>= 1000)
            let query =
                Query.Or
                    [ Query.Field("price", FieldOp.Compare(box (CompareOp.Lte 50)))
                      Query.Field("price", FieldOp.Compare(box (CompareOp.Gte 1000))) ]

            let! results = products |> Collection.find query

            // Should match: Mouse (25), Lamp (50), Laptop (1200)
            results.Length |> should equal 3

            for doc in results do
                let price = doc.Data.Price
                (price <= 50 || price >= 1000) |> should equal true
        }

    [<Fact>]
    member _.``QueryOptions sort orders correctly``() : Task =
        task {
            // Query all products sorted by price ascending
            let query = Query.Empty
            let options = QueryOptions.empty |> QueryOptions.sortAsc "price"

            let! results = products |> Collection.findWith query options

            results.Length |> should equal 10

            // Verify sorted order
            let prices = results |> List.map (fun doc -> doc.Data.Price)
            let sortedPrices = prices |> List.sort
            prices |> should equal sortedPrices

            // First should be cheapest
            results.[0].Data.Price |> should equal 25
            // Last should be most expensive
            results.[results.Length - 1].Data.Price |> should equal 1200
        }

    [<Fact>]
    member _.``QueryOptions limit restricts results``() : Task =
        task {
            // Query Electronics with limit of 3
            let query =
                Query.Field("category", FieldOp.Compare(box (CompareOp.Eq "Electronics")))

            let options = QueryOptions.empty |> QueryOptions.limit 3

            let! results = products |> Collection.findWith query options

            // Should return exactly 3 results even though 6 match
            results.Length |> should equal 3

            for doc in results do
                doc.Data.Category |> should equal "Electronics"
        }

    [<Fact>]
    member _.``QueryOptions skip and limit for pagination``() : Task =
        task {
            // Get second page of Electronics (skip 3, limit 3)
            let query =
                Query.Field("category", FieldOp.Compare(box (CompareOp.Eq "Electronics")))

            let options = QueryOptions.empty |> QueryOptions.skip 3 |> QueryOptions.limit 3

            let! results = products |> Collection.findWith query options

            // Should return 3 results (items 4-6 of 6 total)
            results.Length |> should equal 3

            for doc in results do
                doc.Data.Category |> should equal "Electronics"
        }

    [<Fact>]
    member _.``Complex query with multiple conditions and sorting``() : Task =
        task {
            // Find in-stock items under $500, sorted by price descending
            let query =
                Query.And
                    [ Query.Field("inStock", FieldOp.Compare(box (CompareOp.Eq true)))
                      Query.Field("price", FieldOp.Compare(box (CompareOp.Lt 500))) ]

            let options = QueryOptions.empty |> QueryOptions.sortDesc "price"

            let! results = products |> Collection.findWith query options

            // Should match: Monitor (400), Desk (300), Bookshelf (200), Chair (150), Webcam (100), Mouse (25)
            // But Webcam is not in stock, so: Desk (300), Bookshelf (200), Chair (150), Mouse (25)
            results.Length |> should be (greaterThan 0)

            for doc in results do
                doc.Data.InStock |> should equal true
                doc.Data.Price |> should be (lessThan 500)

            // Verify descending order
            let prices = results |> List.map (fun doc -> doc.Data.Price)
            let sortedDesc = prices |> List.sortDescending
            prices |> should equal sortedDesc
        }
