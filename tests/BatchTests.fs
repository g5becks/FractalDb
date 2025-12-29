module FractalDb.Tests.BatchTests

open System.Threading.Tasks
open Xunit
open FsUnit.Xunit
open FractalDb.Database
open FractalDb.Schema
open FractalDb.Collection
open FractalDb.Query
open FractalDb.Types
open FractalDb.Errors

type Product = {
    Name: string
    Category: string
    Price: int
    InStock: bool
}

let productSchema : SchemaDef<Product> = {
    Fields = [
        { Name = "name"; Path = None; SqlType = SqliteType.Text; Indexed = true; Unique = false; Nullable = false }
        { Name = "category"; Path = None; SqlType = SqliteType.Text; Indexed = true; Unique = false; Nullable = false }
        { Name = "price"; Path = None; SqlType = SqliteType.Integer; Indexed = false; Unique = false; Nullable = false }
        { Name = "inStock"; Path = None; SqlType = SqliteType.Integer; Indexed = false; Unique = false; Nullable = false }
    ]
    Indexes = []
    Timestamps = true
    Validate = None
}

type BatchTestFixture() =
    let db = FractalDb.InMemory()
    member _.Db = db
    interface System.IDisposable with
        member _.Dispose() = db.Close()

type BatchTests(fixture: BatchTestFixture) =
    interface IClassFixture<BatchTestFixture>

    [<Fact>]
    member _.``insertMany inserts all documents`` () : Task = task {
        // Arrange
        let db = fixture.Db
        let products = db.Collection<Product>("products_insertMany", productSchema)
        
        let productsToInsert = [
            { Name = "Laptop"; Category = "Electronics"; Price = 1000; InStock = true }
            { Name = "Mouse"; Category = "Electronics"; Price = 25; InStock = true }
            { Name = "Keyboard"; Category = "Electronics"; Price = 75; InStock = false }
        ]
        
        // Act
        let! result = products |> Collection.insertMany productsToInsert
        
        // Assert
        match result with
        | Ok insertResult ->
            insertResult.Documents |> should haveLength 3
            insertResult.InsertedCount |> should equal 3
            
            // Verify all documents have IDs
            for doc in insertResult.Documents do
                doc.Id |> should not' (be EmptyString)
                
            // Verify persistence
            let! allProducts = products |> Collection.find Query.empty
            allProducts |> should haveLength 3
            
        | Error err ->
            failwith $"Expected Ok, got Error: {err.Message}"
    }

    [<Fact>]
    member _.``insertMany returns correct InsertedCount`` () : Task = task {
        // Arrange
        let db = fixture.Db
        let products = db.Collection<Product>("products_insertCount", productSchema)
        
        let batch1 = [
            { Name = "Product1"; Category = "Cat1"; Price = 10; InStock = true }
            { Name = "Product2"; Category = "Cat1"; Price = 20; InStock = true }
        ]
        
        let batch2 = [
            { Name = "Product3"; Category = "Cat2"; Price = 30; InStock = false }
            { Name = "Product4"; Category = "Cat2"; Price = 40; InStock = false }
            { Name = "Product5"; Category = "Cat2"; Price = 50; InStock = false }
        ]
        
        // Act
        let! result1 = products |> Collection.insertMany batch1
        let! result2 = products |> Collection.insertMany batch2
        
        // Assert
        match result1, result2 with
        | Ok r1, Ok r2 ->
            r1.InsertedCount |> should equal 2
            r2.InsertedCount |> should equal 3
            
            // Verify total count
            let! allProducts = products |> Collection.find Query.empty
            allProducts |> should haveLength 5
            
        | Error err, _ | _, Error err ->
            failwith $"Expected Ok, got Error: {err.Message}"
    }

    [<Fact>]
    member _.``insertMany handles empty list`` () : Task = task {
        // Arrange
        let db = fixture.Db
        let products = db.Collection<Product>("products_empty", productSchema)
        
        // Act
        let! result = products |> Collection.insertMany []
        
        // Assert
        match result with
        | Ok insertResult ->
            insertResult.Documents |> should haveLength 0
            insertResult.InsertedCount |> should equal 0
            
        | Error err ->
            failwith $"Expected Ok, got Error: {err.Message}"
    }

    [<Fact>]
    member _.``updateMany updates all matching documents`` () : Task = task {
        // Arrange
        let db = fixture.Db
        let products = db.Collection<Product>("products_updateMany", productSchema)
        
        // Insert initial products
        let initialProducts = [
            { Name = "Laptop"; Category = "Electronics"; Price = 1000; InStock = true }
            { Name = "Mouse"; Category = "Electronics"; Price = 25; InStock = true }
            { Name = "Desk"; Category = "Furniture"; Price = 300; InStock = false }
            { Name = "Chair"; Category = "Furniture"; Price = 150; InStock = false }
        ]
        
        let! _ = products |> Collection.insertMany initialProducts
        
        // Act - Update all Electronics products to mark as InStock
        let query = Query.field "category" (Query.eq "Electronics")
        let! result = products |> Collection.updateMany query (fun p -> { p with InStock = false })
        
        // Assert
        match result with
        | Ok updateResult ->
            updateResult.MatchedCount |> should equal 2
            updateResult.ModifiedCount |> should equal 2
            
            // Verify updates
            let! electronics = products |> Collection.find query
            electronics |> should haveLength 2
            for doc in electronics do
                doc.Data.InStock |> should equal false
                
            // Verify furniture products unchanged
            let furnitureQuery = Query.field "category" (Query.eq "Furniture")
            let! furniture = products |> Collection.find furnitureQuery
            furniture |> should haveLength 2
            for doc in furniture do
                doc.Data.InStock |> should equal false  // Was already false
                
        | Error err ->
            failwith $"Expected Ok, got Error: {err.Message}"
    }

    [<Fact>]
    member _.``updateMany with price increase`` () : Task = task {
        // Arrange
        let db = fixture.Db
        let products = db.Collection<Product>("products_priceUpdate", productSchema)
        
        let initialProducts = [
            { Name = "Budget Mouse"; Category = "Electronics"; Price = 10; InStock = true }
            { Name = "Budget Keyboard"; Category = "Electronics"; Price = 20; InStock = true }
            { Name = "Premium Desk"; Category = "Furniture"; Price = 500; InStock = true }
        ]
        
        let! _ = products |> Collection.insertMany initialProducts
        
        // Act - Increase price of all products under $50 by 50%
        let query = Query.field "price" (Query.lt 50)
        let! result = products |> Collection.updateMany query (fun p -> { p with Price = p.Price * 3 / 2 })
        
        // Assert
        match result with
        | Ok updateResult ->
            updateResult.MatchedCount |> should equal 2
            updateResult.ModifiedCount |> should equal 2
            
            // Verify price increases
            let! allProducts = products |> Collection.find Query.empty
            let mouse = allProducts |> List.find (fun d -> d.Data.Name = "Budget Mouse")
            let keyboard = allProducts |> List.find (fun d -> d.Data.Name = "Budget Keyboard")
            let desk = allProducts |> List.find (fun d -> d.Data.Name = "Premium Desk")
            
            mouse.Data.Price |> should equal 15  // 10 * 3/2 = 15
            keyboard.Data.Price |> should equal 30  // 20 * 3/2 = 30
            desk.Data.Price |> should equal 500  // Unchanged
            
        | Error err ->
            failwith $"Expected Ok, got Error: {err.Message}"
    }

    [<Fact>]
    member _.``deleteMany removes all matching documents`` () : Task = task {
        // Arrange
        let db = fixture.Db
        let products = db.Collection<Product>("products_deleteMany", productSchema)
        
        let initialProducts = [
            { Name = "Laptop"; Category = "Electronics"; Price = 1000; InStock = true }
            { Name = "Mouse"; Category = "Electronics"; Price = 25; InStock = false }
            { Name = "Keyboard"; Category = "Electronics"; Price = 75; InStock = false }
            { Name = "Desk"; Category = "Furniture"; Price = 300; InStock = true }
            { Name = "Chair"; Category = "Furniture"; Price = 150; InStock = true }
        ]
        
        let! _ = products |> Collection.insertMany initialProducts
        
        // Act - Delete all out-of-stock products
        let query = Query.field "inStock" (Query.eq false)
        let! deleteResult = products |> Collection.deleteMany query
        
        // Assert
        deleteResult.DeletedCount |> should equal 2
        
        // Verify deletions
        let! remaining = products |> Collection.find Query.empty
        remaining |> should haveLength 3
        
        for doc in remaining do
            doc.Data.InStock |> should equal true
            
        // Verify specific products remain
        let names = remaining |> List.map (fun d -> d.Data.Name) |> List.sort
        names |> should equal ["Chair"; "Desk"; "Laptop"]
    }

    [<Fact>]
    member _.``deleteMany with complex query`` () : Task = task {
        // Arrange
        let db = fixture.Db
        let products = db.Collection<Product>("products_deleteComplex", productSchema)
        
        let initialProducts = [
            { Name = "Cheap Mouse"; Category = "Electronics"; Price = 10; InStock = true }
            { Name = "Expensive Mouse"; Category = "Electronics"; Price = 100; InStock = true }
            { Name = "Cheap Keyboard"; Category = "Electronics"; Price = 20; InStock = false }
            { Name = "Cheap Desk"; Category = "Furniture"; Price = 150; InStock = true }
        ]
        
        let! _ = products |> Collection.insertMany initialProducts
        
        // Act - Delete Electronics products under $50
        let query = Query.all' [
            Query.field "category" (Query.eq "Electronics")
            Query.field "price" (Query.lt 50)
        ]
        let! deleteResult = products |> Collection.deleteMany query
        
        // Assert
        deleteResult.DeletedCount |> should equal 2  // Cheap Mouse and Cheap Keyboard
        
        // Verify remaining products
        let! remaining = products |> Collection.find Query.empty
        remaining |> should haveLength 2
        
        let names = remaining |> List.map (fun d -> d.Data.Name) |> List.sort
        names |> should equal ["Cheap Desk"; "Expensive Mouse"]
    }
