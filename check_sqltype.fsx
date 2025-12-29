#r "nuget: Donald, 10.*"

open Donald

// Check what SqlType cases exist
printfn "SqlType cases:"
printfn "String: %A" (SqlType.String "test")
printfn "Null: %A" SqlType.Null

// Try to see what other cases exist
let reflection = typeof<SqlType>.GetFields()
for field in reflection do
    printfn "Field: %s" field.Name
