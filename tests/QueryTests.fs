module FractalDb.Tests.QueryTests

// Suppress linter warnings for test code - repeated failwith messages are acceptable in tests
// fsharplint:disable FL0072

open Xunit
open FsUnit.Xunit
open FractalDb.Operators
open FractalDb.Query

/// <summary>
/// Unit tests for Query construction and composition.
/// Tests the Query module helper functions to ensure they create correct Query structures.
/// </summary>

[<Fact>]
let ``Query.empty returns Query.Empty`` () =
    let result = Query.empty<int>
    match result with
    | Query.Empty -> ()
    | _ -> failwith "Expected Query.Empty"

[<Fact>]
let ``Query.eq creates Field with CompareOp.Eq`` () =
    let result = Query.eq 42
    match result with
    | Query.Field(fieldName, FieldOp.Compare op) ->
        fieldName |> should equal ""
        let unboxed = unbox<CompareOp<int>> op
        match unboxed with
        | CompareOp.Eq value -> value |> should equal 42
        | _ -> failwith "Expected CompareOp.Eq"
    | _ -> failwith (sprintf "Expected Query.Field in %s" __SOURCE_FILE__)

[<Fact>]
let ``Query.ne creates Field with CompareOp.Ne`` () =
    let result = Query.ne "test"
    match result with
    | Query.Field(fieldName, FieldOp.Compare op) ->
        fieldName |> should equal ""
        let unboxed = unbox<CompareOp<string>> op
        match unboxed with
        | CompareOp.Ne value -> value |> should equal "test"
        | _ -> failwith "Expected CompareOp.Ne"
    | _ -> failwith (sprintf "Expected Query.Field in %s" __SOURCE_FILE__)

[<Fact>]
let ``Query.gt creates Field with CompareOp.Gt`` () =
    let result = Query.gt 100
    match result with
    | Query.Field(_, FieldOp.Compare op) ->
        let unboxed = unbox<CompareOp<int>> op
        match unboxed with
        | CompareOp.Gt value -> value |> should equal 100
        | _ -> failwith "Expected CompareOp.Gt"
    | _ -> failwith (sprintf "Expected Query.Field in %s" __SOURCE_FILE__)

[<Fact>]
let ``Query.isIn creates Field with CompareOp.In`` () =
    let result = Query.isIn [1; 2; 3]
    match result with
    | Query.Field(_, FieldOp.Compare op) ->
        let unboxed = unbox<CompareOp<int>> op
        match unboxed with
        | CompareOp.In values -> values |> should equal [1; 2; 3]
        | _ -> failwith "Expected CompareOp.In"
    | _ -> failwith (sprintf "Expected Query.Field in %s" __SOURCE_FILE__)

[<Fact>]
let ``Query.field binds field name correctly`` () =
    let query = Query.eq 30
    let result = Query.field "age" query
    match result with
    | Query.Field(fieldName, FieldOp.Compare op) ->
        fieldName |> should equal "age"
        let unboxed = unbox<CompareOp<int>> op
        match unboxed with
        | CompareOp.Eq value -> value |> should equal 30
        | _ -> failwith "Expected CompareOp.Eq"
    | _ -> failwith (sprintf "Expected Query.Field in %s" __SOURCE_FILE__)

[<Fact>]
let ``Query.field with non-Field query returns unchanged`` () =
    let query = Query.Empty
    let result = Query.field "age" query
    result |> should equal Query.Empty

[<Fact>]
let ``Query.all' combines queries with And`` () =
    let q1 = Query.eq 30 |> Query.field "age"
    let q2 = Query.eq "active" |> Query.field "status"
    let result = Query.all' [q1; q2]
    match result with
    | Query.And queries ->
        queries |> should haveLength 2
        queries |> should equal [q1; q2]
    | _ -> failwith "Expected Query.And"

[<Fact>]
let ``Query.any combines queries with Or`` () =
    let q1 = Query.eq "admin" |> Query.field "role"
    let q2 = Query.eq "moderator" |> Query.field "role"
    let result = Query.any [q1; q2]
    match result with
    | Query.Or queries ->
        queries |> should haveLength 2
        queries |> should equal [q1; q2]
    | _ -> failwith "Expected Query.Or"

[<Fact>]
let ``Query.none combines queries with Nor`` () =
    let q1 = Query.eq "deleted" |> Query.field "status"
    let q2 = Query.eq "archived" |> Query.field "status"
    let result = Query.none [q1; q2]
    match result with
    | Query.Nor queries ->
        queries |> should haveLength 2
        queries |> should equal [q1; q2]
    | _ -> failwith "Expected Query.Nor"

[<Fact>]
let ``Query.not' wraps query with Not`` () =
    let query = Query.eq true |> Query.field "deleted"
    let result = Query.not' query
    match result with
    | Query.Not innerQuery ->
        innerQuery |> should equal query
    | _ -> failwith "Expected Query.Not"

[<Fact>]
let ``Query.like creates Field with StringOp.Like`` () =
    let result = Query.like "admin%"
    match result with
    | Query.Field(fieldName, FieldOp.String op) ->
        fieldName |> should equal ""
        match op with
        | StringOp.Like pattern -> pattern |> should equal "admin%"
        | _ -> failwith "Expected StringOp.Like"
    | _ -> failwith (sprintf "Expected Query.Field in %s" __SOURCE_FILE__)

[<Fact>]
let ``Query.ilike creates Field with StringOp.ILike`` () =
    let result = Query.ilike "SMITH"
    match result with
    | Query.Field(fieldName, FieldOp.String op) ->
        fieldName |> should equal ""
        match op with
        | StringOp.ILike pattern -> pattern |> should equal "SMITH"
        | _ -> failwith "Expected StringOp.ILike"
    | _ -> failwith (sprintf "Expected Query.Field in %s" __SOURCE_FILE__)

[<Fact>]
let ``Query.contains creates Field with StringOp.Contains`` () =
    let result = Query.contains "test"
    match result with
    | Query.Field(_, FieldOp.String op) ->
        match op with
        | StringOp.Contains substring -> substring |> should equal "test"
        | _ -> failwith "Expected StringOp.Contains"
    | _ -> failwith (sprintf "Expected Query.Field in %s" __SOURCE_FILE__)

[<Fact>]
let ``Query.startsWith creates Field with StringOp.StartsWith`` () =
    let result = Query.startsWith "admin"
    match result with
    | Query.Field(_, FieldOp.String op) ->
        match op with
        | StringOp.StartsWith prefix -> prefix |> should equal "admin"
        | _ -> failwith "Expected StringOp.StartsWith"
    | _ -> failwith (sprintf "Expected Query.Field in %s" __SOURCE_FILE__)

[<Fact>]
let ``Query.endsWith creates Field with StringOp.EndsWith`` () =
    let result = Query.endsWith ".com"
    match result with
    | Query.Field(_, FieldOp.String op) ->
        match op with
        | StringOp.EndsWith suffix -> suffix |> should equal ".com"
        | _ -> failwith "Expected StringOp.EndsWith"
    | _ -> failwith (sprintf "Expected Query.Field in %s" __SOURCE_FILE__)

[<Fact>]
let ``Query.all creates Field with ArrayOp.All`` () =
    let result = Query.all ["featured"; "public"]
    match result with
    | Query.Field(_, FieldOp.Array op) ->
        // When boxed, the type is erased, so we need to handle it generically
        let unboxed = unbox op
        match unboxed with
        | ArrayOp.All values -> 
            values |> should haveLength 2
            values |> List.head |> should equal "featured"
            values |> List.last |> should equal "public"
        | _ -> failwith "Expected ArrayOp.All"
    | _ -> failwith (sprintf "Expected Query.Field in %s" __SOURCE_FILE__)

[<Fact>]
let ``Query.size creates Field with ArrayOp.Size`` () =
    let result = Query.size 3
    match result with
    | Query.Field(_, FieldOp.Array op) ->
        let unboxed = unbox<ArrayOp<obj>> op
        match unboxed with
        | ArrayOp.Size n -> n |> should equal 3
        | _ -> failwith "Expected ArrayOp.Size"
    | _ -> failwith (sprintf "Expected Query.Field in %s" __SOURCE_FILE__)

[<Fact>]
let ``Query.exists creates Field with ExistsOp.Exists true`` () =
    let result = Query.exists
    match result with
    | Query.Field(_, FieldOp.Exist op) ->
        match op with
        | ExistsOp.Exists value -> value |> should equal true
    | _ -> failwith (sprintf "Expected Query.Field in %s" __SOURCE_FILE__)

[<Fact>]
let ``Query.notExists creates Field with ExistsOp.Exists false`` () =
    let result = Query.notExists
    match result with
    | Query.Field(_, FieldOp.Exist op) ->
        match op with
        | ExistsOp.Exists value -> value |> should equal false
    | _ -> failwith (sprintf "Expected Query.Field in %s" __SOURCE_FILE__)

[<Fact>]
let ``Complex query composition with field binding`` () =
    let query = 
        Query.all' [
            Query.gte 18 |> Query.field "age"
            Query.lt 65 |> Query.field "age"
            Query.eq "active" |> Query.field "status"
        ]
    match query with
    | Query.And queries ->
        queries |> should haveLength 3
        // Verify first condition: age >= 18
        match queries.[0] with
        | Query.Field("age", FieldOp.Compare op) ->
            let unboxed = unbox<CompareOp<int>> op
            match unboxed with
            | CompareOp.Gte value -> value |> should equal 18
            | _ -> failwith "Expected CompareOp.Gte"
        | _ -> failwith (sprintf "Expected Query.Field in %s" __SOURCE_FILE__)
    | _ -> failwith "Expected Query.And"
