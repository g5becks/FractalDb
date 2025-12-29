module FractalDb.Tests.SerializationTests

open Xunit
open FsUnit.Xunit
open FractalDb.Serialization

/// <summary>
/// Unit tests for JSON serialization functions.
/// Tests serialize/deserialize roundtrips, camelCase naming, and F# type support.
/// </summary>

type TestUser = {
    Name: string
    Age: int
    Email: option<string>
}

[<Fact>]
let ``serialize then deserialize roundtrips TestUser correctly`` () =
    let user = { Name = "Alice"; Age = 30; Email = Some "alice@example.com" }
    let json = serialize user
    let deserialized = deserialize<TestUser> json
    
    deserialized.Name |> should equal "Alice"
    deserialized.Age |> should equal 30
    deserialized.Email |> should equal (Some "alice@example.com")

[<Fact>]
let ``serialize applies camelCase property naming`` () =
    let user = { Name = "Bob"; Age = 25; Email = None }
    let json = serialize user
    
    // JSON should have camelCase properties
    json |> should haveSubstring "\"name\":"
    json |> should haveSubstring "\"age\":"
    json |> should haveSubstring "\"email\":"
    
    // Should NOT have PascalCase properties
    json |> should not' (haveSubstring "\"Name\":")
    json |> should not' (haveSubstring "\"Age\":")

[<Fact>]
let ``serialize handles Some option as value`` () =
    let user = { Name = "Charlie"; Age = 35; Email = Some "charlie@test.com" }
    let json = serialize user
    
    // Some value should be serialized as the value itself
    json |> should haveSubstring "\"email\":\"charlie@test.com\""

[<Fact>]
let ``serialize handles None option as null`` () =
    let user = { Name = "Diana"; Age = 28; Email = None }
    let json = serialize user
    
    // None should be serialized as null
    json |> should haveSubstring "\"email\":null"

[<Fact>]
let ``deserialize handles null as None option`` () =
    let json = """{"name":"Eve","age":32,"email":null}"""
    let user = deserialize<TestUser> json
    
    user.Name |> should equal "Eve"
    user.Age |> should equal 32
    user.Email |> should equal None

[<Fact>]
let ``deserialize handles value as Some option`` () =
    let json = """{"name":"Frank","age":40,"email":"frank@test.com"}"""
    let user = deserialize<TestUser> json
    
    user.Name |> should equal "Frank"
    user.Age |> should equal 40
    user.Email |> should equal (Some "frank@test.com")

[<Fact>]
let ``serializeToBytes then deserializeFromBytes roundtrips correctly`` () =
    let user = { Name = "Grace"; Age = 27; Email = Some "grace@example.com" }
    let bytes = serializeToBytes user
    let deserialized = deserializeFromBytes<TestUser> bytes
    
    deserialized.Name |> should equal "Grace"
    deserialized.Age |> should equal 27
    deserialized.Email |> should equal (Some "grace@example.com")

[<Fact>]
let ``serializeToBytes produces byte array`` () =
    let user = { Name = "Henry"; Age = 45; Email = None }
    let bytes = serializeToBytes user
    
    // Should be a non-empty byte array
    bytes |> should not' (be Empty)
    bytes.Length |> should be (greaterThan 0)

[<Fact>]
let ``serialize and serializeToBytes produce equivalent JSON`` () =
    let user = { Name = "Ivy"; Age = 29; Email = Some "ivy@test.com" }
    
    let jsonString = serialize user
    let jsonBytes = serializeToBytes user
    let jsonFromBytes = System.Text.Encoding.UTF8.GetString(jsonBytes)
    
    jsonFromBytes |> should equal jsonString

[<Fact>]
let ``deserialize empty email as None`` () =
    let json = """{"name":"Jack","age":33,"email":null}"""
    let user = deserialize<TestUser> json
    
    user.Email |> should equal None
