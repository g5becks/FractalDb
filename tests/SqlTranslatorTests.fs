module FractalDb.Tests.SqlTranslatorTests

// Suppress indentation linter warnings for test code - record field alignment is more readable
// fsharplint:disable FL0059

open Xunit
open FsUnit.Xunit
open FractalDb.Schema
open FractalDb.Operators
open FractalDb.Options
open FractalDb.SqlTranslator

// Test type for queries
type TestUser = {
    Name: string
    Age: int
    Email: string
    Status: string
}

// Test schema with indexed and non-indexed fields
let testSchema: SchemaDef<TestUser> = {
    Fields = [
        // Indexed fields - should use _fieldName
        { Name = "name"; Path = None; SqlType = SqliteType.Text
          Indexed = true; Unique = false; Nullable = false }
        { Name = "age"; Path = None; SqlType = SqliteType.Integer
          Indexed = true; Unique = false; Nullable = false }
        
        // Non-indexed fields - should use jsonb_extract
        { Name = "email"; Path = None; SqlType = SqliteType.Text
          Indexed = false; Unique = false; Nullable = false }
        { Name = "status"; Path = None; SqlType = SqliteType.Text
          Indexed = false; Unique = false; Nullable = false }
    ]
    Indexes = []
    Timestamps = true
    Validate = None
}

[<Fact>]
let ``Empty query translates to 1=1`` () =
    let translator = SqlTranslator<TestUser>(testSchema, false)
    let result = translator.Translate(Query.Empty)
    
    result.Sql |> should equal "1=1"
    result.Parameters |> should be Empty

[<Fact>]
let ``Indexed field uses generated column name`` () =
    let translator = SqlTranslator<TestUser>(testSchema, false)
    let query = Query.Field("name", FieldOp.Compare(box (CompareOp.Eq "Alice")))
    let result = translator.Translate(query)
    
    result.Sql |> should equal "_name = @p0"
    result.Parameters |> should haveLength 1
    
    let (paramName, paramValue) = result.Parameters.[0]
    paramName |> should equal "@p0"
    (unbox<string> paramValue) |> should equal "Alice"

[<Fact>]
let ``Non-indexed field uses jsonb_extract`` () =
    let translator = SqlTranslator<TestUser>(testSchema, false)
    let query = Query.Field("email", FieldOp.Compare(box (CompareOp.Eq "test@example.com")))
    let result = translator.Translate(query)
    
    result.Sql |> should equal "jsonb_extract(body, '$.email') = @p0"
    result.Parameters |> should haveLength 1

[<Fact>]
let ``Metadata field _id uses direct column reference`` () =
    let translator = SqlTranslator<TestUser>(testSchema, false)
    let query = Query.Field("_id", FieldOp.Compare(box (CompareOp.Eq "doc123")))
    let result = translator.Translate(query)
    
    result.Sql |> should equal "_id = @p0"

[<Fact>]
let ``Eq operator generates correct SQL`` () =
    let translator = SqlTranslator<TestUser>(testSchema, false)
    let query = Query.Field("age", FieldOp.Compare(box (CompareOp.Eq 30)))
    let result = translator.Translate(query)
    
    result.Sql |> should equal "_age = @p0"
    (unbox<int> (snd result.Parameters.[0])) |> should equal 30

[<Fact>]
let ``Ne operator generates correct SQL`` () =
    let translator = SqlTranslator<TestUser>(testSchema, false)
    let query = Query.Field("age", FieldOp.Compare(box (CompareOp.Ne 0)))
    let result = translator.Translate(query)
    
    result.Sql |> should equal "_age != @p0"

[<Fact>]
let ``Gt operator generates correct SQL`` () =
    let translator = SqlTranslator<TestUser>(testSchema, false)
    let query = Query.Field("age", FieldOp.Compare(box (CompareOp.Gt 18)))
    let result = translator.Translate(query)
    
    result.Sql |> should equal "_age > @p0"

[<Fact>]
let ``Gte operator generates correct SQL`` () =
    let translator = SqlTranslator<TestUser>(testSchema, false)
    let query = Query.Field("age", FieldOp.Compare(box (CompareOp.Gte 18)))
    let result = translator.Translate(query)
    
    result.Sql |> should equal "_age >= @p0"

[<Fact>]
let ``Lt operator generates correct SQL`` () =
    let translator = SqlTranslator<TestUser>(testSchema, false)
    let query = Query.Field("age", FieldOp.Compare(box (CompareOp.Lt 65)))
    let result = translator.Translate(query)
    
    result.Sql |> should equal "_age < @p0"

[<Fact>]
let ``Lte operator generates correct SQL`` () =
    let translator = SqlTranslator<TestUser>(testSchema, false)
    let query = Query.Field("age", FieldOp.Compare(box (CompareOp.Lte 65)))
    let result = translator.Translate(query)
    
    result.Sql |> should equal "_age <= @p0"

[<Fact>]
let ``In operator with values generates correct SQL`` () =
    let translator = SqlTranslator<TestUser>(testSchema, false)
    let query = Query.Field("status", FieldOp.Compare(box (CompareOp.In ["active"; "pending"])))
    let result = translator.Translate(query)
    
    result.Sql |> should equal "jsonb_extract(body, '$.status') IN (@p0, @p1)"
    result.Parameters |> should haveLength 2
    (unbox<string> (snd result.Parameters.[0])) |> should equal "active"
    (unbox<string> (snd result.Parameters.[1])) |> should equal "pending"

[<Fact>]
let ``In operator with empty list generates 0=1`` () =
    let translator = SqlTranslator<TestUser>(testSchema, false)
    let emptyList: list<string> = []
    let query = Query.Field("status", FieldOp.Compare(box (CompareOp.In emptyList)))
    let result = translator.Translate(query)
    
    result.Sql |> should equal "0=1"
    result.Parameters |> should be Empty

[<Fact>]
let ``NotIn operator with values generates correct SQL`` () =
    let translator = SqlTranslator<TestUser>(testSchema, false)
    let query = Query.Field("status", FieldOp.Compare(box (CompareOp.NotIn ["deleted"; "archived"])))
    let result = translator.Translate(query)
    
    result.Sql |> should equal "jsonb_extract(body, '$.status') NOT IN (@p0, @p1)"
    result.Parameters |> should haveLength 2

[<Fact>]
let ``NotIn operator with empty list generates 1=1`` () =
    let translator = SqlTranslator<TestUser>(testSchema, false)
    let emptyList: list<string> = []
    let query = Query.Field("status", FieldOp.Compare(box (CompareOp.NotIn emptyList)))
    let result = translator.Translate(query)
    
    result.Sql |> should equal "1=1"
    result.Parameters |> should be Empty

[<Fact>]
let ``Like operator generates correct SQL`` () =
    let translator = SqlTranslator<TestUser>(testSchema, false)
    let query = Query.Field("name", FieldOp.String(StringOp.Like "A%"))
    let result = translator.Translate(query)
    
    result.Sql |> should equal "_name LIKE @p0"
    (unbox<string> (snd result.Parameters.[0])) |> should equal "A%"

[<Fact>]
let ``ILike operator generates case-insensitive SQL`` () =
    let translator = SqlTranslator<TestUser>(testSchema, false)
    let query = Query.Field("name", FieldOp.String(StringOp.ILike "alice"))
    let result = translator.Translate(query)
    
    result.Sql |> should equal "_name LIKE @p0 COLLATE NOCASE"

[<Fact>]
let ``Contains operator wraps pattern with percent`` () =
    let translator = SqlTranslator<TestUser>(testSchema, false)
    let query = Query.Field("name", FieldOp.String(StringOp.Contains "admin"))
    let result = translator.Translate(query)
    
    result.Sql |> should equal "_name LIKE @p0"
    (unbox<string> (snd result.Parameters.[0])) |> should equal "%admin%"

[<Fact>]
let ``StartsWith operator appends percent`` () =
    let translator = SqlTranslator<TestUser>(testSchema, false)
    let query = Query.Field("name", FieldOp.String(StringOp.StartsWith "user_"))
    let result = translator.Translate(query)
    
    result.Sql |> should equal "_name LIKE @p0"
    (unbox<string> (snd result.Parameters.[0])) |> should equal "user_%"

[<Fact>]
let ``EndsWith operator prepends percent`` () =
    let translator = SqlTranslator<TestUser>(testSchema, false)
    let query = Query.Field("email", FieldOp.String(StringOp.EndsWith "@company.com"))
    let result = translator.Translate(query)
    
    result.Sql |> should equal "jsonb_extract(body, '$.email') LIKE @p0"
    (unbox<string> (snd result.Parameters.[0])) |> should equal "%@company.com"

[<Fact>]
let ``ArrayOp.All with values generates EXISTS subqueries`` () =
    let translator = SqlTranslator<TestUser>(testSchema, false)
    let query = Query.Field("tags", FieldOp.Array(box (ArrayOp.All ["featured"; "public"])))
    let result = translator.Translate(query)
    
    result.Sql |> should haveSubstring "EXISTS(SELECT 1 FROM json_each"
    result.Sql |> should haveSubstring " AND "
    result.Parameters |> should haveLength 2

[<Fact>]
let ``ArrayOp.All with empty list generates 1=1`` () =
    let translator = SqlTranslator<TestUser>(testSchema, false)
    let emptyList: list<string> = []
    let query = Query.Field("tags", FieldOp.Array(box (ArrayOp.All emptyList)))
    let result = translator.Translate(query)
    
    result.Sql |> should equal "1=1"

[<Fact>]
let ``ArrayOp.Size generates json_array_length`` () =
    let translator = SqlTranslator<TestUser>(testSchema, false)
    // Use typed ArrayOp to ensure proper pattern matching
    let query = Query.Field("items", FieldOp.Array(box (ArrayOp<int>.Size 5)))
    let result = translator.Translate(query)
    
    result.Sql |> should equal "json_array_length(jsonb_extract(body, '$.items')) = @p0"
    (unbox<int> (snd result.Parameters.[0])) |> should equal 5

[<Fact>]
let ``ExistsOp true generates IS NOT NULL`` () =
    let translator = SqlTranslator<TestUser>(testSchema, false)
    let query = Query.Field("email", FieldOp.Exist(ExistsOp.Exists true))
    let result = translator.Translate(query)
    
    result.Sql |> should equal "json_type(jsonb_extract(body, '$.email')) IS NOT NULL"
    result.Parameters |> should be Empty

[<Fact>]
let ``ExistsOp false generates IS NULL`` () =
    let translator = SqlTranslator<TestUser>(testSchema, false)
    let query = Query.Field("deletedAt", FieldOp.Exist(ExistsOp.Exists false))
    let result = translator.Translate(query)
    
    result.Sql |> should equal "json_type(jsonb_extract(body, '$.deletedAt')) IS NULL"

[<Fact>]
let ``And combines queries with AND`` () =
    let translator = SqlTranslator<TestUser>(testSchema, false)
    let query = Query.And [
        Query.Field("name", FieldOp.Compare(box (CompareOp.Eq "Alice")))
        Query.Field("age", FieldOp.Compare(box (CompareOp.Gt 18)))
    ]
    let result = translator.Translate(query)
    
    result.Sql |> should equal "(_name = @p0 AND _age > @p1)"
    result.Parameters |> should haveLength 2

[<Fact>]
let ``Or combines queries with OR`` () =
    let translator = SqlTranslator<TestUser>(testSchema, false)
    let query = Query.Or [
        Query.Field("status", FieldOp.Compare(box (CompareOp.Eq "active")))
        Query.Field("status", FieldOp.Compare(box (CompareOp.Eq "pending")))
    ]
    let result = translator.Translate(query)
    
    result.Sql |> should equal "(jsonb_extract(body, '$.status') = @p0 OR jsonb_extract(body, '$.status') = @p1)"

[<Fact>]
let ``Not wraps query with NOT`` () =
    let translator = SqlTranslator<TestUser>(testSchema, false)
    let query = Query.Not(Query.Field("age", FieldOp.Compare(box (CompareOp.Lt 18))))
    let result = translator.Translate(query)
    
    result.Sql |> should equal "NOT (_age < @p0)"

[<Fact>]
let ``Nor combines with OR then wraps with NOT`` () =
    let translator = SqlTranslator<TestUser>(testSchema, false)
    let query = Query.Nor [
        Query.Field("status", FieldOp.Compare(box (CompareOp.Eq "deleted")))
        Query.Field("status", FieldOp.Compare(box (CompareOp.Eq "archived")))
    ]
    let result = translator.Translate(query)
    
    result.Sql |> should equal "NOT (jsonb_extract(body, '$.status') = @p0 OR jsonb_extract(body, '$.status') = @p1)"

[<Fact>]
let ``Complex nested query with And/Or`` () =
    let translator = SqlTranslator<TestUser>(testSchema, false)
    let query = Query.And [
        Query.Field("age", FieldOp.Compare(box (CompareOp.Gte 18)))
        Query.Or [
            Query.Field("status", FieldOp.Compare(box (CompareOp.Eq "active")))
            Query.Field("status", FieldOp.Compare(box (CompareOp.Eq "pending")))
        ]
    ]
    let result = translator.Translate(query)
    
    result.Sql |> should haveSubstring "AND"
    result.Sql |> should haveSubstring "OR"
    result.Parameters |> should haveLength 3

[<Fact>]
let ``TranslateOptions with Sort generates ORDER BY`` () =
    let translator = SqlTranslator<TestUser>(testSchema, false)
    let options = {
        Sort = [("name", SortDirection.Ascending); ("age", SortDirection.Descending)]
        Limit = None
        Skip = None
        Select = None
        Omit = None
        Search = None
        Cursor = None
    }
    let (sql, params') = translator.TranslateOptions(options)
    
    sql |> should equal " ORDER BY _name ASC, _age DESC"
    params' |> should be Empty

[<Fact>]
let ``TranslateOptions with Limit generates LIMIT`` () =
    let translator = SqlTranslator<TestUser>(testSchema, false)
    let options = {
        Sort = []
        Limit = Some 10
        Skip = None
        Select = None
        Omit = None
        Search = None
        Cursor = None
    }
    let (sql, params') = translator.TranslateOptions(options)
    
    sql |> should equal " LIMIT @opt0"
    params' |> should haveLength 1
    (fst params'.[0]) |> should equal "@opt0"
    (unbox<int> (snd params'.[0])) |> should equal 10

[<Fact>]
let ``TranslateOptions with Skip generates OFFSET`` () =
    let translator = SqlTranslator<TestUser>(testSchema, false)
    let options = {
        Sort = []
        Limit = None
        Skip = Some 20
        Select = None
        Omit = None
        Search = None
        Cursor = None
    }
    let (sql, params') = translator.TranslateOptions(options)
    
    sql |> should equal " OFFSET @opt0"
    (unbox<int> (snd params'.[0])) |> should equal 20

[<Fact>]
let ``TranslateOptions with Sort, Limit, Skip`` () =
    let translator = SqlTranslator<TestUser>(testSchema, false)
    let options = {
        Sort = [("age", SortDirection.Descending)]
        Limit = Some 5
        Skip = Some 10
        Select = None
        Omit = None
        Search = None
        Cursor = None
    }
    let (sql, params') = translator.TranslateOptions(options)
    
    sql |> should equal " ORDER BY _age DESC LIMIT @opt0 OFFSET @opt1"
    params' |> should haveLength 2

[<Fact>]
let ``TranslateOptions with empty options returns empty string`` () =
    let translator = SqlTranslator<TestUser>(testSchema, false)
    let options = QueryOptions.empty<TestUser>
    let (sql, params') = translator.TranslateOptions(options)
    
    sql |> should equal ""
    params' |> should be Empty
