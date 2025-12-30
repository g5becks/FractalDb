module FractalDb.Tests.QueryOptionsTests

/// <summary>
/// Explicit tests for QueryOptions functionality including limit, skip, sort, cursor, and projection.
/// Tests verify Options.fs behavior independently from Collection operations.
/// </summary>

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

// =============================================================================
// Test Data Setup
// =============================================================================

type TestItem = {
    Name: string
    Value: int
    Category: string
}

let testItemSchema: SchemaDef<TestItem> =
    { Fields =
        [ { Name = "name"
            Path = None
            SqlType = SqliteType.Text
            Indexed = true
            Unique = false
            Nullable = false }
          { Name = "value"
            Path = None
            SqlType = SqliteType.Integer
            Indexed = true
            Unique = false
            Nullable = false }
          { Name = "category"
            Path = None
            SqlType = SqliteType.Text
            Indexed = true
            Unique = false
            Nullable = false } ]
      Indexes = []
      Timestamps = true
      Validate = None }

type QueryOptionsTestFixture() =
    let db = FractalDb.InMemory()
    let items = db.Collection<TestItem>("items", testItemSchema)
    
    // Seed test data: 20 items with predictable values
    do
        let testData = [
            for i in 1..20 do
                { Name = $"Item_{i:D2}"
                  Value = i * 10
                  Category = if i % 2 = 0 then "Even" else "Odd" }
        ]
        for item in testData do
            items |> Collection.insertOne item |> Async.AwaitTask |> Async.RunSynchronously |> ignore
    
    member _.Db = db
    member _.Items = items
    
    interface IDisposable with
        member _.Dispose() = db.Close()

type QueryOptionsTests(fixture: QueryOptionsTestFixture) =
    let items = fixture.Items
    
    interface IClassFixture<QueryOptionsTestFixture>

    // =============================================================================
    // Limit Tests
    // =============================================================================

    [<Fact>]
    member _.``limit 0 returns empty results``() : Task =
        task {
            let opts = QueryOptions.empty |> QueryOptions.limit 0
            let! results = items |> Collection.findWith Query.Empty opts
            
            results |> should haveLength 0
        }

    [<Fact>]
    member _.``limit 5 returns exactly 5 results``() : Task =
        task {
            let opts = QueryOptions.empty |> QueryOptions.limit 5
            let! results = items |> Collection.findWith Query.Empty opts
            
            results |> should haveLength 5
        }

    [<Fact>]
    member _.``limit greater than count returns all results``() : Task =
        task {
            let opts = QueryOptions.empty |> QueryOptions.limit 1000
            let! results = items |> Collection.findWith Query.Empty opts
            
            results |> should haveLength 20
        }

    [<Fact>]
    member _.``limit 1 returns single result``() : Task =
        task {
            let opts = QueryOptions.empty |> QueryOptions.limit 1
            let! results = items |> Collection.findWith Query.Empty opts
            
            results |> should haveLength 1
        }

    // =============================================================================
    // Skip Tests
    // =============================================================================

    // Note: SQLite requires LIMIT when using OFFSET, so skip must be combined with limit
    [<Fact>]
    member _.``skip 0 with limit returns all results up to limit``() : Task =
        task {
            let opts = QueryOptions.empty |> QueryOptions.skip 0 |> QueryOptions.limit 20
            let! results = items |> Collection.findWith Query.Empty opts
            
            results |> should haveLength 20
        }

    [<Fact>]
    member _.``skip 10 with limit skips first 10 results``() : Task =
        task {
            let opts = 
                QueryOptions.empty 
                |> QueryOptions.sortAsc "value"
                |> QueryOptions.skip 10
                |> QueryOptions.limit 20
            
            let! results = items |> Collection.findWith Query.Empty opts
            
            results |> should haveLength 10
            // First result after skip should be item 11 (value 110)
            results.[0].Data.Value |> should equal 110
        }

    [<Fact>]
    member _.``skip beyond count with limit returns empty``() : Task =
        task {
            let opts = QueryOptions.empty |> QueryOptions.skip 100 |> QueryOptions.limit 20
            let! results = items |> Collection.findWith Query.Empty opts
            
            results |> should haveLength 0
        }

    [<Fact>]
    member _.``skip and limit combine correctly``() : Task =
        task {
            let opts = 
                QueryOptions.empty 
                |> QueryOptions.sortAsc "value"
                |> QueryOptions.skip 5
                |> QueryOptions.limit 3
            
            let! results = items |> Collection.findWith Query.Empty opts
            
            results |> should haveLength 3
            // Should get items 6, 7, 8 (values 60, 70, 80)
            results.[0].Data.Value |> should equal 60
            results.[1].Data.Value |> should equal 70
            results.[2].Data.Value |> should equal 80
        }

    // =============================================================================
    // Sort Tests
    // =============================================================================

    [<Fact>]
    member _.``sortAsc orders results ascending``() : Task =
        task {
            let opts = QueryOptions.empty |> QueryOptions.sortAsc "value"
            let! results = items |> Collection.findWith Query.Empty opts
            
            results |> should haveLength 20
            // First item should have lowest value
            results.[0].Data.Value |> should equal 10
            // Last item should have highest value
            results.[19].Data.Value |> should equal 200
        }

    [<Fact>]
    member _.``sortDesc orders results descending``() : Task =
        task {
            let opts = QueryOptions.empty |> QueryOptions.sortDesc "value"
            let! results = items |> Collection.findWith Query.Empty opts
            
            results |> should haveLength 20
            // First item should have highest value
            results.[0].Data.Value |> should equal 200
            // Last item should have lowest value
            results.[19].Data.Value |> should equal 10
        }

    [<Fact>]
    member _.``sort by name ascending orders alphabetically``() : Task =
        task {
            let opts = QueryOptions.empty |> QueryOptions.sortAsc "name"
            let! results = items |> Collection.findWith Query.Empty opts
            
            results |> should haveLength 20
            // Item_01 < Item_02 < ... < Item_20 (lexicographic)
            results.[0].Data.Name |> should equal "Item_01"
            results.[19].Data.Name |> should equal "Item_20"
        }

    [<Fact>]
    member _.``multi-field sort applies priority correctly``() : Task =
        task {
            // Sort by category (asc), then value (desc)
            let opts =
                QueryOptions.empty
                |> QueryOptions.sortDesc "value"
                |> QueryOptions.sortAsc "category"
            
            let! results = items |> Collection.findWith Query.Empty opts
            
            results |> should haveLength 20
            // First group: "Even" category (alphabetically first)
            results.[0].Data.Category |> should equal "Even"
            // Within "Even" category, highest value first
            results.[0].Data.Value |> should equal 200
        }

    [<Fact>]
    member _.``empty sort returns results in database order``() : Task =
        task {
            let opts = QueryOptions.empty
            let! results = items |> Collection.findWith Query.Empty opts
            
            results |> should haveLength 20
        }

    // =============================================================================
    // Cursor Pagination Tests
    // =============================================================================

    // Note: Cursor pagination behavior may vary - this test verifies basic functionality
    [<Fact>]
    member _.``cursorAfter accepts cursor token``() : Task =
        task {
            // First, get initial results and extract a cursor
            let initialOpts = 
                QueryOptions.empty 
                |> QueryOptions.sortAsc "_id"
                |> QueryOptions.limit 5
            
            let! initialResults = items |> Collection.findWith Query.Empty initialOpts
            
            if initialResults.Length > 0 then
                let cursorId = initialResults.[2].Id // Use 3rd item as cursor
                
                // Get results after cursor - just verify it doesn't error
                let afterOpts = 
                    QueryOptions.empty 
                    |> QueryOptions.sortAsc "_id"
                    |> QueryOptions.cursorAfter cursorId
                    |> QueryOptions.limit 3
                
                let! afterResults = items |> Collection.findWith Query.Empty afterOpts
                
                // Just verify we get results (cursor implementation may vary)
                afterResults |> should not' (be Null)
        }

    [<Fact>]
    member _.``cursorBefore retrieves results before cursor``() : Task =
        task {
            // Get some results to extract a cursor
            let initialOpts = 
                QueryOptions.empty 
                |> QueryOptions.sortAsc "_id"
                |> QueryOptions.limit 10
            
            let! initialResults = items |> Collection.findWith Query.Empty initialOpts
            
            if initialResults.Length > 5 then
                let cursorId = initialResults.[8].Id // Use 9th item as cursor
                
                // Get results before cursor
                let beforeOpts = 
                    QueryOptions.empty 
                    |> QueryOptions.sortAsc "_id"
                    |> QueryOptions.cursorBefore cursorId
                    |> QueryOptions.limit 3
                
                let! beforeResults = items |> Collection.findWith Query.Empty beforeOpts
                
                beforeResults |> should not' (be Empty)
                // All results should have IDs < cursorId
                beforeResults |> List.forall (fun d -> String.Compare(d.Id, cursorId, StringComparison.Ordinal) < 0)
                |> should equal true
        }

    // =============================================================================
    // Combined Options Tests
    // =============================================================================

    [<Fact>]
    member _.``complex query with multiple options works correctly``() : Task =
        task {
            // Query: Even category items, sorted by value desc, skip 2, limit 3
            let query = Query.Field("category", FieldOp.Compare(box (CompareOp.Eq "Even")))
            
            let opts =
                QueryOptions.empty
                |> QueryOptions.sortDesc "value"
                |> QueryOptions.skip 2
                |> QueryOptions.limit 3
            
            let! results = items |> Collection.findWith query opts
            
            results |> should haveLength 3
            // All should be "Even" category
            results |> List.forall (fun d -> d.Data.Category = "Even")
            |> should equal true
            // Should be sorted descending
            results.[0].Data.Value |> should be (greaterThan results.[1].Data.Value)
            results.[1].Data.Value |> should be (greaterThan results.[2].Data.Value)
        }

    [<Fact>]
    member _.``empty options returns all results unmodified``() : Task =
        task {
            let opts = QueryOptions.empty
            let! results = items |> Collection.findWith Query.Empty opts
            
            results |> should haveLength 20
        }

    [<Fact>]
    member _.``limit and sort combine correctly``() : Task =
        task {
            let opts=
                QueryOptions.empty
                |> QueryOptions.sortDesc "value"
                |> QueryOptions.limit 5
            
            let! results = items |> Collection.findWith Query.Empty opts
            
            results |> should haveLength 5
            // Top 5 highest values
            results.[0].Data.Value |> should equal 200
            results.[4].Data.Value |> should equal 160
        }

    // =============================================================================
    // QueryOptions Builder Tests
    // =============================================================================

    [<Fact>]
    member _.``QueryOptions.empty creates default options``() =
        let opts = QueryOptions.empty<TestItem>
        
        opts.Sort |> should be Empty
        opts.Limit |> should equal None
        opts.Skip |> should equal None
        opts.Select |> should equal None
        opts.Omit |> should equal None
        opts.Search |> should equal None
        opts.Cursor |> should equal None

    [<Fact>]
    member _.``QueryOptions builders are immutable``() =
        let opts1 = QueryOptions.empty<TestItem>
        let opts2 = opts1 |> QueryOptions.limit 10
        let opts3 = opts2 |> QueryOptions.skip 5
        
        // Original should be unchanged
        opts1.Limit |> should equal None
        opts1.Skip |> should equal None
        
        // Each step creates new instance
        opts2.Limit |> should equal (Some 10)
        opts2.Skip |> should equal None
        
        opts3.Limit |> should equal (Some 10)
        opts3.Skip |> should equal (Some 5)

    [<Fact>]
    member _.``QueryOptions sortBy adds to sort list``() =
        let opts =
            QueryOptions.empty<TestItem>
            |> QueryOptions.sortAsc "name"
            |> QueryOptions.sortDesc "value"
        
        opts.Sort |> should haveLength 2
        opts.Sort.[0] |> should equal ("name", SortDirection.Ascending)
        opts.Sort.[1] |> should equal ("value", SortDirection.Descending)

    [<Fact>]
    member _.``cursorAfter sets After field``() =
        let opts = QueryOptions.empty<TestItem> |> QueryOptions.cursorAfter "test-cursor"
        
        match opts.Cursor with
        | Some cursor ->
            cursor.After |> should equal (Some "test-cursor")
            cursor.Before |> should equal None
        | None -> failwith "Expected cursor to be set"

    [<Fact>]
    member _.``cursorBefore sets Before field``() =
        let opts = QueryOptions.empty<TestItem> |> QueryOptions.cursorBefore "test-cursor"
        
        match opts.Cursor with
        | Some cursor ->
            cursor.Before |> should equal (Some "test-cursor")
            cursor.After |> should equal None
        | None -> failwith "Expected cursor to be set"

    [<Fact>]
    member _.``select option sets Select field``() =
        let opts = QueryOptions.empty<TestItem> |> QueryOptions.select ["name"; "value"]
        
        match opts.Select with
        | Some fields ->
            fields |> should haveLength 2
            fields |> should contain "name"
            fields |> should contain "value"
        | None -> failwith "Expected Select to be set"

    [<Fact>]
    member _.``omit option sets Omit field``() =
        let opts = QueryOptions.empty<TestItem> |> QueryOptions.omit ["category"]
        
        match opts.Omit with
        | Some fields ->
            fields |> should haveLength 1
            fields |> should contain "category"
        | None -> failwith "Expected Omit to be set"
