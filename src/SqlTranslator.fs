module FractalDb.SqlTranslator

/// <summary>
/// Result of translating a Query to SQL with parameterized values.
/// </summary>
///
/// <remarks>
/// Encapsulates the SQL WHERE clause and parameter bindings after translation.
/// Uses parameterized queries (@p0, @p1, ...) to prevent SQL injection.
/// </remarks>
///
/// <example>
/// <code>
/// let result = { Sql = "_name = @p0"; Parameters = [("@p0", box "Alice")] }
/// </code>
/// </example>
type TranslatorResult = {
    /// <summary>The SQL WHERE clause expression with parameter placeholders.</summary>
    Sql: string
    
    /// <summary>List of parameter bindings as (name, value) tuples.</summary>
    Parameters: list<(string * obj)>
}

/// <summary>
/// Helper functions for creating TranslatorResult values.
/// </summary>
module TranslatorResult =
    
    /// <summary>
    /// Returns an empty TranslatorResult that matches all documents (SQL: "1=1").
    /// </summary>
    ///
    /// <returns>
    /// A TranslatorResult with Sql = "1=1" and empty Parameters.
    /// </returns>
    ///
    /// <example>
    /// <code>
    /// let result = TranslatorResult.empty
    /// // result = { Sql = "1=1"; Parameters = [] }
    /// </code>
    /// </example>
    let empty : TranslatorResult = 
        { Sql = "1=1"; Parameters = [] }
    
    /// <summary>
    /// Creates a TranslatorResult with the specified SQL and parameters.
    /// </summary>
    ///
    /// <param name="sql">The SQL WHERE clause expression.</param>
    /// <param name="params'">List of parameter bindings.</param>
    ///
    /// <returns>
    /// A TranslatorResult with the given Sql and Parameters.
    /// </returns>
    ///
    /// <example>
    /// <code>
    /// let result = TranslatorResult.create "_name = @p0" [("@p0", box "Alice")]
    /// </code>
    /// </example>
    let create (sql: string) (params': list<(string * obj)>) : TranslatorResult =
        { Sql = sql; Parameters = params' }

/// <summary>
/// Translates FractalDb Query expressions to parameterized SQLite SQL.
/// </summary>
///
/// <remarks>
/// Converts type-safe Query&lt;'T&gt; to SQL WHERE clauses with parameter bindings.
/// Field resolution: indexed fields use _fieldName, non-indexed use jsonb_extract.
/// </remarks>
///
/// <example>
/// <code>
/// let translator = SqlTranslator(schema, false)
/// let result = translator.Translate(query)
/// // result.Sql = "_name = @p0"
/// </code>
/// </example>
type SqlTranslator<'T>(schema: FractalDb.Schema.SchemaDef<'T>, enableCache: bool) =
    
    /// <summary>Map of field names to their definitions for fast lookup.</summary>
    let fieldMap =
        schema.Fields
        |> List.map (fun f -> f.Name, f)
        |> Map.ofList
    
    /// <summary>Counter for generating unique parameter names (@p0, @p1, etc.).</summary>
    let mutable paramCounter = 0
    
    /// <summary>
    /// Generates the next unique parameter name and increments the counter.
    /// </summary>
    ///
    /// <returns>
    /// A unique parameter name in the format "@p0", "@p1", "@p2", etc.
    /// </returns>
    member private this.NextParam() : string =
        let paramName = $"@p{paramCounter}"
        paramCounter <- paramCounter + 1
        paramName
    
    /// <summary>
    /// Resolves a field name to its SQL column reference.
    /// </summary>
    ///
    /// <param name="fieldName">The field name to resolve.</param>
    ///
    /// <returns>
    /// The SQL column reference: direct column, generated column, or jsonb_extract call.
    /// </returns>
    ///
    /// <remarks>
    /// Resolution logic:
    /// 1. Metadata fields (_id, createdAt, updatedAt) return as-is
    /// 2. Indexed fields (found in fieldMap with Indexed = true) return "_fieldName"
    /// 3. All other fields return "jsonb_extract(body, '$.fieldName')"
    ///
    /// This enables the query optimizer to use indexes when available while
    /// still supporting dynamic field access for non-indexed fields.
    /// </remarks>
    member private this.ResolveField(fieldName: string) : string =
        match fieldName with
        | "_id" | "createdAt" | "updatedAt" -> 
            // Metadata fields are direct columns
            fieldName
        | name ->
            // Check if field is indexed in schema
            match Map.tryFind name fieldMap with
            | Some field when field.Indexed -> 
                // Indexed fields use generated columns
                $"_{name}"
            | _ -> 
                // Non-indexed fields use jsonb_extract
                $"jsonb_extract(body, '$.{name}')"
    
    /// <summary>
    /// Translates a CompareOp to SQL with appropriate comparison operator.
    /// </summary>
    ///
    /// <param name="fieldSql">The SQL column reference for the field.</param>
    /// <param name="compareOp">The comparison operator (boxed).</param>
    ///
    /// <returns>
    /// A TranslatorResult with SQL comparison expression and parameter bindings.
    /// </returns>
    ///
    /// <remarks>
    /// Handles all CompareOp cases:
    ///
    /// - Eq: Generates "field = @pN"
    /// - Ne: Generates "field != @pN"
    /// - Gt/Gte/Lt/Lte: Generates "field >/>=/</<= @pN"
    /// - In: Empty list returns "0=1" (matches nothing), otherwise "field IN (@p1, @p2, ...)"
    /// - NotIn: Empty list returns "1=1" (matches all), otherwise "field NOT IN (@p1, @p2, ...)"
    ///
    /// Special cases:
    /// - Empty In lists: Return "0=1" since no values can match
    /// - Empty NotIn lists: Return "1=1" since all values pass the filter
    ///
    /// The compareOp parameter is boxed and must be unboxed using type matching.
    /// </remarks>
    member private this.TranslateCompare(fieldSql: string, compareOp: obj) : TranslatorResult =
        // Pattern match on the unboxed CompareOp
        match compareOp with
        | :? FractalDb.Operators.CompareOp<int> as op ->
            match op with
            | FractalDb.Operators.CompareOp.Eq value ->
                let paramName = this.NextParam()
                TranslatorResult.create $"{fieldSql} = {paramName}" [(paramName, box value)]
            | FractalDb.Operators.CompareOp.Ne value ->
                let paramName = this.NextParam()
                TranslatorResult.create $"{fieldSql} != {paramName}" [(paramName, box value)]
            | FractalDb.Operators.CompareOp.Gt value ->
                let paramName = this.NextParam()
                TranslatorResult.create $"{fieldSql} > {paramName}" [(paramName, box value)]
            | FractalDb.Operators.CompareOp.Gte value ->
                let paramName = this.NextParam()
                TranslatorResult.create $"{fieldSql} >= {paramName}" [(paramName, box value)]
            | FractalDb.Operators.CompareOp.Lt value ->
                let paramName = this.NextParam()
                TranslatorResult.create $"{fieldSql} < {paramName}" [(paramName, box value)]
            | FractalDb.Operators.CompareOp.Lte value ->
                let paramName = this.NextParam()
                TranslatorResult.create $"{fieldSql} <= {paramName}" [(paramName, box value)]
            | FractalDb.Operators.CompareOp.In values ->
                if List.isEmpty values then
                    // Empty IN list matches nothing
                    TranslatorResult.create "0=1" []
                else
                    // Generate parameter for each value
                    let paramPairs = 
                        values 
                        |> List.map (fun v -> 
                            let paramName = this.NextParam()
                            (paramName, box v))
                    let paramNames = paramPairs |> List.map fst |> String.concat ", "
                    TranslatorResult.create $"{fieldSql} IN ({paramNames})" paramPairs
            | FractalDb.Operators.CompareOp.NotIn values ->
                if List.isEmpty values then
                    // Empty NOT IN list matches everything
                    TranslatorResult.create "1=1" []
                else
                    // Generate parameter for each value
                    let paramPairs = 
                        values 
                        |> List.map (fun v -> 
                            let paramName = this.NextParam()
                            (paramName, box v))
                    let paramNames = paramPairs |> List.map fst |> String.concat ", "
                    TranslatorResult.create $"{fieldSql} NOT IN ({paramNames})" paramPairs
        
        | :? FractalDb.Operators.CompareOp<string> as op ->
            match op with
            | FractalDb.Operators.CompareOp.Eq value ->
                let paramName = this.NextParam()
                TranslatorResult.create $"{fieldSql} = {paramName}" [(paramName, box value)]
            | FractalDb.Operators.CompareOp.Ne value ->
                let paramName = this.NextParam()
                TranslatorResult.create $"{fieldSql} != {paramName}" [(paramName, box value)]
            | FractalDb.Operators.CompareOp.Gt value ->
                let paramName = this.NextParam()
                TranslatorResult.create $"{fieldSql} > {paramName}" [(paramName, box value)]
            | FractalDb.Operators.CompareOp.Gte value ->
                let paramName = this.NextParam()
                TranslatorResult.create $"{fieldSql} >= {paramName}" [(paramName, box value)]
            | FractalDb.Operators.CompareOp.Lt value ->
                let paramName = this.NextParam()
                TranslatorResult.create $"{fieldSql} < {paramName}" [(paramName, box value)]
            | FractalDb.Operators.CompareOp.Lte value ->
                let paramName = this.NextParam()
                TranslatorResult.create $"{fieldSql} <= {paramName}" [(paramName, box value)]
            | FractalDb.Operators.CompareOp.In values ->
                if List.isEmpty values then
                    TranslatorResult.create "0=1" []
                else
                    let paramPairs = 
                        values 
                        |> List.map (fun v -> 
                            let paramName = this.NextParam()
                            (paramName, box v))
                    let paramNames = paramPairs |> List.map fst |> String.concat ", "
                    TranslatorResult.create $"{fieldSql} IN ({paramNames})" paramPairs
            | FractalDb.Operators.CompareOp.NotIn values ->
                if List.isEmpty values then
                    TranslatorResult.create "1=1" []
                else
                    let paramPairs = 
                        values 
                        |> List.map (fun v -> 
                            let paramName = this.NextParam()
                            (paramName, box v))
                    let paramNames = paramPairs |> List.map fst |> String.concat ", "
                    TranslatorResult.create $"{fieldSql} NOT IN ({paramNames})" paramPairs
        
        | :? FractalDb.Operators.CompareOp<int64> as op ->
            match op with
            | FractalDb.Operators.CompareOp.Eq value ->
                let paramName = this.NextParam()
                TranslatorResult.create $"{fieldSql} = {paramName}" [(paramName, box value)]
            | FractalDb.Operators.CompareOp.Ne value ->
                let paramName = this.NextParam()
                TranslatorResult.create $"{fieldSql} != {paramName}" [(paramName, box value)]
            | FractalDb.Operators.CompareOp.Gt value ->
                let paramName = this.NextParam()
                TranslatorResult.create $"{fieldSql} > {paramName}" [(paramName, box value)]
            | FractalDb.Operators.CompareOp.Gte value ->
                let paramName = this.NextParam()
                TranslatorResult.create $"{fieldSql} >= {paramName}" [(paramName, box value)]
            | FractalDb.Operators.CompareOp.Lt value ->
                let paramName = this.NextParam()
                TranslatorResult.create $"{fieldSql} < {paramName}" [(paramName, box value)]
            | FractalDb.Operators.CompareOp.Lte value ->
                let paramName = this.NextParam()
                TranslatorResult.create $"{fieldSql} <= {paramName}" [(paramName, box value)]
            | FractalDb.Operators.CompareOp.In values ->
                if List.isEmpty values then
                    TranslatorResult.create "0=1" []
                else
                    let paramPairs = 
                        values 
                        |> List.map (fun v -> 
                            let paramName = this.NextParam()
                            (paramName, box v))
                    let paramNames = paramPairs |> List.map fst |> String.concat ", "
                    TranslatorResult.create $"{fieldSql} IN ({paramNames})" paramPairs
            | FractalDb.Operators.CompareOp.NotIn values ->
                if List.isEmpty values then
                    TranslatorResult.create "1=1" []
                else
                    let paramPairs = 
                        values 
                        |> List.map (fun v -> 
                            let paramName = this.NextParam()
                            (paramName, box v))
                    let paramNames = paramPairs |> List.map fst |> String.concat ", "
                    TranslatorResult.create $"{fieldSql} NOT IN ({paramNames})" paramPairs
        
        | :? FractalDb.Operators.CompareOp<float> as op ->
            match op with
            | FractalDb.Operators.CompareOp.Eq value ->
                let paramName = this.NextParam()
                TranslatorResult.create $"{fieldSql} = {paramName}" [(paramName, box value)]
            | FractalDb.Operators.CompareOp.Ne value ->
                let paramName = this.NextParam()
                TranslatorResult.create $"{fieldSql} != {paramName}" [(paramName, box value)]
            | FractalDb.Operators.CompareOp.Gt value ->
                let paramName = this.NextParam()
                TranslatorResult.create $"{fieldSql} > {paramName}" [(paramName, box value)]
            | FractalDb.Operators.CompareOp.Gte value ->
                let paramName = this.NextParam()
                TranslatorResult.create $"{fieldSql} >= {paramName}" [(paramName, box value)]
            | FractalDb.Operators.CompareOp.Lt value ->
                let paramName = this.NextParam()
                TranslatorResult.create $"{fieldSql} < {paramName}" [(paramName, box value)]
            | FractalDb.Operators.CompareOp.Lte value ->
                let paramName = this.NextParam()
                TranslatorResult.create $"{fieldSql} <= {paramName}" [(paramName, box value)]
            | FractalDb.Operators.CompareOp.In values ->
                if List.isEmpty values then
                    TranslatorResult.create "0=1" []
                else
                    let paramPairs = 
                        values 
                        |> List.map (fun v -> 
                            let paramName = this.NextParam()
                            (paramName, box v))
                    let paramNames = paramPairs |> List.map fst |> String.concat ", "
                    TranslatorResult.create $"{fieldSql} IN ({paramNames})" paramPairs
            | FractalDb.Operators.CompareOp.NotIn values ->
                if List.isEmpty values then
                    TranslatorResult.create "1=1" []
                else
                    let paramPairs = 
                        values 
                        |> List.map (fun v -> 
                            let paramName = this.NextParam()
                            (paramName, box v))
                    let paramNames = paramPairs |> List.map fst |> String.concat ", "
                    TranslatorResult.create $"{fieldSql} NOT IN ({paramNames})" paramPairs
        
        | :? FractalDb.Operators.CompareOp<bool> as op ->
            match op with
            | FractalDb.Operators.CompareOp.Eq value ->
                let paramName = this.NextParam()
                TranslatorResult.create $"{fieldSql} = {paramName}" [(paramName, box value)]
            | FractalDb.Operators.CompareOp.Ne value ->
                let paramName = this.NextParam()
                TranslatorResult.create $"{fieldSql} != {paramName}" [(paramName, box value)]
            | FractalDb.Operators.CompareOp.Gt value ->
                let paramName = this.NextParam()
                TranslatorResult.create $"{fieldSql} > {paramName}" [(paramName, box value)]
            | FractalDb.Operators.CompareOp.Gte value ->
                let paramName = this.NextParam()
                TranslatorResult.create $"{fieldSql} >= {paramName}" [(paramName, box value)]
            | FractalDb.Operators.CompareOp.Lt value ->
                let paramName = this.NextParam()
                TranslatorResult.create $"{fieldSql} < {paramName}" [(paramName, box value)]
            | FractalDb.Operators.CompareOp.Lte value ->
                let paramName = this.NextParam()
                TranslatorResult.create $"{fieldSql} <= {paramName}" [(paramName, box value)]
            | FractalDb.Operators.CompareOp.In values ->
                if List.isEmpty values then
                    TranslatorResult.create "0=1" []
                else
                    let paramPairs = 
                        values 
                        |> List.map (fun v -> 
                            let paramName = this.NextParam()
                            (paramName, box v))
                    let paramNames = paramPairs |> List.map fst |> String.concat ", "
                    TranslatorResult.create $"{fieldSql} IN ({paramNames})" paramPairs
            | FractalDb.Operators.CompareOp.NotIn values ->
                if List.isEmpty values then
                    TranslatorResult.create "1=1" []
                else
                    let paramPairs = 
                        values 
                        |> List.map (fun v -> 
                            let paramName = this.NextParam()
                            (paramName, box v))
                    let paramNames = paramPairs |> List.map fst |> String.concat ", "
                    TranslatorResult.create $"{fieldSql} NOT IN ({paramNames})" paramPairs
        
        | _ ->
            // Unsupported type - return empty (will match nothing in practice)
            TranslatorResult.create "0=1" []
    
    /// <summary>
    /// Translates a StringOp to SQL LIKE pattern matching expressions.
    /// </summary>
    ///
    /// <param name="fieldSql">The SQL column reference for the field.</param>
    /// <param name="stringOp">The string operator to translate.</param>
    ///
    /// <returns>
    /// A TranslatorResult with SQL LIKE expression and parameter bindings.
    /// </returns>
    ///
    /// <remarks>
    /// Handles all StringOp cases:
    ///
    /// - Like: Generates "field LIKE @pN" with pattern as-is
    /// - ILike: Generates "field LIKE @pN COLLATE NOCASE" for case-insensitive matching
    /// - Contains: Wraps substring in %...% for LIKE pattern matching
    /// - StartsWith: Appends % suffix to prefix for LIKE pattern matching
    /// - EndsWith: Prepends % prefix to suffix for LIKE pattern matching
    ///
    /// Pattern matching notes:
    /// - LIKE is case-sensitive by default in SQLite
    /// - ILike uses COLLATE NOCASE for case-insensitive matching
    /// - Special LIKE wildcards: % (zero or more chars), _ (exactly one char)
    /// - User patterns in Like/ILike are passed through unchanged
    /// - Contains/StartsWith/EndsWith automatically add % wildcards
    ///
    /// SQL Injection Safety:
    /// All patterns are parameterized, preventing SQL injection even if
    /// user input contains special SQL characters.
    /// </remarks>
    ///
    /// <example>
    /// <code>
    /// // Like with user pattern
    /// TranslateString("_name", StringOp.Like "A%")
    /// // SQL: "_name LIKE @p0"
    /// // Params: [("@p0", "A%")]
    ///
    /// // Case-insensitive like
    /// TranslateString("_email", StringOp.ILike "admin%")
    /// // SQL: "_email LIKE @p0 COLLATE NOCASE"
    /// // Params: [("@p0", "admin%")]
    ///
    /// // Contains (adds % wildcards)
    /// TranslateString("_description", StringOp.Contains "urgent")
    /// // SQL: "_description LIKE @p0"
    /// // Params: [("@p0", "%urgent%")]
    ///
    /// // StartsWith (adds % suffix)
    /// TranslateString("_username", StringOp.StartsWith "admin")
    /// // SQL: "_username LIKE @p0"
    /// // Params: [("@p0", "admin%")]
    ///
    /// // EndsWith (adds % prefix)
    /// TranslateString("_email", StringOp.EndsWith "@company.com")
    /// // SQL: "_email LIKE @p0"
    /// // Params: [("@p0", "%@company.com")]
    /// </code>
    /// </example>
    member private this.TranslateString(fieldSql: string, stringOp: FractalDb.Operators.StringOp) : TranslatorResult =
        match stringOp with
        | FractalDb.Operators.StringOp.Like pattern ->
            // Direct LIKE with user-provided pattern
            let paramName = this.NextParam()
            TranslatorResult.create $"{fieldSql} LIKE {paramName}" [(paramName, box pattern)]
        
        | FractalDb.Operators.StringOp.ILike pattern ->
            // Case-insensitive LIKE using COLLATE NOCASE
            let paramName = this.NextParam()
            TranslatorResult.create $"{fieldSql} LIKE {paramName} COLLATE NOCASE" [(paramName, box pattern)]
        
        | FractalDb.Operators.StringOp.Contains substring ->
            // Wrap substring in % wildcards for contains matching
            let pattern = $"%%{substring}%%"
            let paramName = this.NextParam()
            TranslatorResult.create $"{fieldSql} LIKE {paramName}" [(paramName, box pattern)]
        
        | FractalDb.Operators.StringOp.StartsWith prefix ->
            // Append % wildcard for prefix matching
            let pattern = $"{prefix}%%"
            let paramName = this.NextParam()
            TranslatorResult.create $"{fieldSql} LIKE {paramName}" [(paramName, box pattern)]
        
        | FractalDb.Operators.StringOp.EndsWith suffix ->
            // Prepend % wildcard for suffix matching
            let pattern = $"%%{suffix}"
            let paramName = this.NextParam()
            TranslatorResult.create $"{fieldSql} LIKE {paramName}" [(paramName, box pattern)]
    
    /// <summary>
    /// Translates an ArrayOp to SQL using JSON array functions.
    /// </summary>
    ///
    /// <param name="fieldSql">The SQL column reference for the field.</param>
    /// <param name="arrayOp">The array operator (boxed).</param>
    ///
    /// <returns>
    /// A TranslatorResult with SQL array operation and parameter bindings.
    /// </returns>
    ///
    /// <remarks>
    /// Handles ArrayOp cases using SQLite JSON functions:
    ///
    /// - All: Empty list returns "1=1" (trivially true), otherwise generates EXISTS
    ///   subqueries with json_each to verify all values are present in the array
    /// - Size: Generates "json_array_length(field) = @pN" for exact length matching
    ///
    /// Array containment strategy for All operator:
    /// For each value, generate an EXISTS subquery that checks if the value appears
    /// in the JSON array using json_each. Combine all checks with AND.
    ///
    /// Example SQL for ArrayOp.All ["a", "b"]:
    /// EXISTS(SELECT 1 FROM json_each(field) WHERE value = @p0) AND
    /// EXISTS(SELECT 1 FROM json_each(field) WHERE value = @p1)
    ///
    /// Note: This implementation handles simple value containment. ElemMatch and Index
    /// operators (for complex object queries) will be added in future tasks if needed.
    /// </remarks>
    ///
    /// <example>
    /// <code>
    /// // ArrayOp.All with values
    /// TranslateArray("_tags", box (ArrayOp.All ["featured"; "public"]))
    /// // SQL: EXISTS(SELECT 1 FROM json_each(_tags) WHERE value = @p0) AND
    /// //      EXISTS(SELECT 1 FROM json_each(_tags) WHERE value = @p1)
    /// // Params: [("@p0", "featured"); ("@p1", "public")]
    ///
    /// // ArrayOp.All with empty list
    /// TranslateArray("_tags", box (ArrayOp.All []))
    /// // SQL: "1=1"
    /// // Params: []
    ///
    /// // ArrayOp.Size
    /// TranslateArray("_items", box (ArrayOp.Size 5))
    /// // SQL: "json_array_length(_items) = @p0"
    /// // Params: [("@p0", 5)]
    /// </code>
    /// </example>
    member private this.TranslateArray(fieldSql: string, arrayOp: obj) : TranslatorResult =
        // Pattern match on the unboxed ArrayOp
        match arrayOp with
        | :? FractalDb.Operators.ArrayOp<string> as op ->
            match op with
            | FractalDb.Operators.ArrayOp.All values ->
                if List.isEmpty values then
                    // Empty list - all values are trivially present
                    TranslatorResult.create "1=1" []
                else
                    // Generate EXISTS subquery for each value
                    let subqueries = 
                        values 
                        |> List.map (fun value ->
                            let paramName = this.NextParam()
                            let sql = $"EXISTS(SELECT 1 FROM json_each({fieldSql}) WHERE value = {paramName})"
                            (sql, paramName, box value))
                    
                    let sqlParts = subqueries |> List.map (fun (sql, _, _) -> sql)
                    let params' = subqueries |> List.map (fun (_, paramName, value) -> (paramName, value))
                    
                    let combinedSql = sqlParts |> String.concat " AND "
                    TranslatorResult.create combinedSql params'
            
            | FractalDb.Operators.ArrayOp.Size length ->
                let paramName = this.NextParam()
                TranslatorResult.create $"json_array_length({fieldSql}) = {paramName}" [(paramName, box length)]
            
            | FractalDb.Operators.ArrayOp.ElemMatch _query ->
                // Stub - complex array element queries not yet implemented
                TranslatorResult.create "1=1" []
            
            | FractalDb.Operators.ArrayOp.Index (_index, _query) ->
                // Stub - indexed element queries not yet implemented
                TranslatorResult.create "1=1" []
        
        | :? FractalDb.Operators.ArrayOp<int> as op ->
            match op with
            | FractalDb.Operators.ArrayOp.All values ->
                if List.isEmpty values then
                    TranslatorResult.create "1=1" []
                else
                    let subqueries = 
                        values 
                        |> List.map (fun value ->
                            let paramName = this.NextParam()
                            let sql = $"EXISTS(SELECT 1 FROM json_each({fieldSql}) WHERE value = {paramName})"
                            (sql, paramName, box value))
                    
                    let sqlParts = subqueries |> List.map (fun (sql, _, _) -> sql)
                    let params' = subqueries |> List.map (fun (_, paramName, value) -> (paramName, value))
                    
                    let combinedSql = sqlParts |> String.concat " AND "
                    TranslatorResult.create combinedSql params'
            
            | FractalDb.Operators.ArrayOp.Size length ->
                let paramName = this.NextParam()
                TranslatorResult.create $"json_array_length({fieldSql}) = {paramName}" [(paramName, box length)]
            
            | FractalDb.Operators.ArrayOp.ElemMatch _query ->
                TranslatorResult.create "1=1" []
            
            | FractalDb.Operators.ArrayOp.Index (_index, _query) ->
                TranslatorResult.create "1=1" []
        
        | _ ->
            // Unsupported array type
            TranslatorResult.create "1=1" []
    
    /// <summary>
    /// Translates an ExistsOp to SQL using JSON type checking.
    /// </summary>
    ///
    /// <param name="fieldSql">The SQL column reference for the field.</param>
    /// <param name="existsOp">The existence operator.</param>
    ///
    /// <returns>
    /// A TranslatorResult with SQL existence check (no parameters needed).
    /// </returns>
    ///
    /// <remarks>
    /// Handles ExistsOp using SQLite json_type function:
    ///
    /// - Exists true: Generates "json_type(field) IS NOT NULL"
    ///   Matches when the field is present in the JSON document
    /// - Exists false: Generates "json_type(field) IS NULL"
    ///   Matches when the field is absent from the JSON document
    ///
    /// The json_type function returns the JSON type of a value (e.g., "text", "integer",
    /// "array", "object", "null") if the field exists, or SQL NULL if the field is absent.
    ///
    /// This is distinct from checking for JSON null values:
    /// - json_type returns "null" for JSON null values (field exists, value is null)
    /// - json_type returns SQL NULL for absent fields (field doesn't exist)
    ///
    /// Therefore:
    /// - IS NOT NULL: Field exists (even if JSON value is null)
    /// - IS NULL: Field is completely absent from the document
    /// </remarks>
    ///
    /// <example>
    /// <code>
    /// // Check field exists
    /// TranslateExist("_email", ExistsOp.Exists true)
    /// // SQL: "json_type(_email) IS NOT NULL"
    /// // Params: []
    ///
    /// // Check field doesn't exist
    /// TranslateExist("_deletedAt", ExistsOp.Exists false)
    /// // SQL: "json_type(_deletedAt) IS NULL"
    /// // Params: []
    /// </code>
    /// </example>
    member private this.TranslateExist(fieldSql: string, existsOp: FractalDb.Operators.ExistsOp) : TranslatorResult =
        match existsOp with
        | FractalDb.Operators.ExistsOp.Exists shouldExist ->
            if shouldExist then
                // Field exists: json_type returns a type string (not SQL NULL)
                TranslatorResult.create $"json_type({fieldSql}) IS NOT NULL" []
            else
                // Field doesn't exist: json_type returns SQL NULL
                TranslatorResult.create $"json_type({fieldSql}) IS NULL" []
    
    /// <summary>
    /// Translates a FieldOp to SQL by dispatching to the appropriate operator handler.
    /// </summary>
    ///
    /// <param name="fieldSql">The SQL column reference for the field.</param>
    /// <param name="op">The field operator to translate.</param>
    ///
    /// <returns>
    /// A TranslatorResult with SQL and parameters for the field operation.
    /// </returns>
    ///
    /// <remarks>
    /// Dispatches based on FieldOp variant:
    ///
    /// - FieldOp.Compare: Delegates to TranslateCompare (Task 36) ✅
    /// - FieldOp.String: Delegates to TranslateString (Task 37) ✅
    /// - FieldOp.Array: Delegates to TranslateArray (Task 38) ✅
    /// - FieldOp.Exist: Delegates to TranslateExist (Task 38) ✅
    ///
    /// This method serves as the central dispatch point for all field operations.
    /// </remarks>
    member private this.TranslateFieldOp(fieldSql: string, op: FractalDb.Operators.FieldOp) : TranslatorResult =
        match op with
        | FractalDb.Operators.FieldOp.Compare compareOp ->
            // Delegate to comparison operator handler
            this.TranslateCompare(fieldSql, compareOp)
        
        | FractalDb.Operators.FieldOp.String stringOp ->
            // Delegate to string operator handler
            this.TranslateString(fieldSql, stringOp)
        
        | FractalDb.Operators.FieldOp.Array arrayOp ->
            // Delegate to array operator handler
            this.TranslateArray(fieldSql, arrayOp)
        
        | FractalDb.Operators.FieldOp.Exist existsOp ->
            // Delegate to existence operator handler
            this.TranslateExist(fieldSql, existsOp)
    
    /// <summary>
    /// Recursively translates a Query expression to SQL.
    /// </summary>
    ///
    /// <param name="query">The Query&lt;'T&gt; to translate.</param>
    ///
    /// <returns>
    /// A TranslatorResult containing the SQL WHERE clause and parameter bindings.
    /// </returns>
    ///
    /// <remarks>
    /// This is the core translation method that handles all Query cases:
    ///
    /// - Query.Empty: Returns "1=1" (matches all documents)
    /// - Query.Field: Resolves field and translates the operator
    /// - Query.And: Combines sub-queries with " AND ", wrapped in parentheses
    /// - Query.Or: Combines sub-queries with " OR ", wrapped in parentheses
    /// - Query.Nor: Combines with OR then wraps in "NOT (...)"
    /// - Query.Not: Wraps single query in "NOT (...)"
    ///
    /// Logical Combination Strategy:
    /// For And/Or/Nor operations:
    /// 1. Recursively translate each sub-query
    /// 2. Collect all SQL fragments and parameters
    /// 3. Join SQL fragments with appropriate operator
    /// 4. Wrap in parentheses for correct precedence
    /// 5. Flatten all parameters into a single list
    ///
    /// Empty Handling:
    /// - Empty queries in combinations are preserved (contribute "1=1")
    /// - This ensures logical correctness: AND [a, empty] → "(a AND 1=1)" → simplifies to "a"
    /// </remarks>
    member private this.TranslateQuery(query: FractalDb.Operators.Query<'T>) : TranslatorResult =
        match query with
        | FractalDb.Operators.Query.Empty ->
            // Empty query matches all documents
            TranslatorResult.empty
        
        | FractalDb.Operators.Query.Field(fieldName, op) ->
            // Resolve field to SQL column reference
            let fieldSql = this.ResolveField(fieldName)
            // Translate the field operator (stub for now)
            this.TranslateFieldOp(fieldSql, op)
        
        | FractalDb.Operators.Query.And(queries) ->
            // Translate each sub-query
            let results = queries |> List.map this.TranslateQuery
            
            // Collect SQL fragments and parameters
            let sqlParts = results |> List.map (fun r -> r.Sql)
            let allParams = results |> List.collect (fun r -> r.Parameters)
            
            // Combine with AND operator
            let combinedSql = sqlParts |> String.concat " AND "
            let wrappedSql = $"({combinedSql})"
            
            TranslatorResult.create wrappedSql allParams
        
        | FractalDb.Operators.Query.Or(queries) ->
            // Translate each sub-query
            let results = queries |> List.map this.TranslateQuery
            
            // Collect SQL fragments and parameters
            let sqlParts = results |> List.map (fun r -> r.Sql)
            let allParams = results |> List.collect (fun r -> r.Parameters)
            
            // Combine with OR operator
            let combinedSql = sqlParts |> String.concat " OR "
            let wrappedSql = $"({combinedSql})"
            
            TranslatorResult.create wrappedSql allParams
        
        | FractalDb.Operators.Query.Nor(queries) ->
            // Translate each sub-query
            let results = queries |> List.map this.TranslateQuery
            
            // Collect SQL fragments and parameters
            let sqlParts = results |> List.map (fun r -> r.Sql)
            let allParams = results |> List.collect (fun r -> r.Parameters)
            
            // Combine with OR then wrap in NOT
            let combinedSql = sqlParts |> String.concat " OR "
            let wrappedSql = $"NOT ({combinedSql})"
            
            TranslatorResult.create wrappedSql allParams
        
        | FractalDb.Operators.Query.Not(innerQuery) ->
            // Translate the inner query
            let result = this.TranslateQuery(innerQuery)
            
            // Wrap in NOT
            let wrappedSql = $"NOT ({result.Sql})"
            
            TranslatorResult.create wrappedSql result.Parameters
    
    /// <summary>
    /// Translates a Query expression to a parameterized SQL WHERE clause.
    /// </summary>
    ///
    /// <param name="query">The Query&lt;'T&gt; to translate.</param>
    ///
    /// <returns>
    /// A TranslatorResult containing the SQL WHERE clause and parameter bindings.
    /// </returns>
    ///
    /// <remarks>
    /// This is the main public entry point for query translation.
    /// It resets the parameter counter and delegates to TranslateQuery for the actual work.
    ///
    /// The parameter counter is reset to 0 at the start of each translation to ensure
    /// consistent parameter naming (@p0, @p1, @p2, ...) within a single query.
    ///
    /// Supported Query Types:
    /// - Query.Empty: Matches all documents (SQL: "1=1")
    /// - Query.Field: Field comparisons with various operators
    /// - Query.And/Or/Nor/Not: Logical combinations of queries
    ///
    /// The resulting SQL can be embedded in a SELECT statement:
    /// SELECT * FROM docs WHERE {result.Sql}
    ///
    /// Parameters should be bound using the database provider's parameter binding.
    /// </remarks>
    ///
    /// <example>
    /// <code>
    /// open FractalDb.Query
    ///
    /// let translator = SqlTranslator(schema, false)
    ///
    /// // Simple query
    /// let query1 = Query.field "name" (Query.eq "Alice")
    /// let result1 = translator.Translate(query1)
    /// // result1.Sql = "_name = @p0" (if indexed)
    /// // result1.Parameters = [("@p0", box "Alice")]
    ///
    /// // Complex query with logical operators
    /// let query2 = 
    ///     Query.and' [
    ///         Query.field "age" (Query.gt 18)
    ///         Query.field "status" (Query.eq "active")
    ///     ]
    /// let result2 = translator.Translate(query2)
    /// // result2.Sql = "(_age > @p0 AND _status = @p1)"
    /// // result2.Parameters = [("@p0", box 18); ("@p1", box "active")]
    ///
    /// // Empty query (match all)
    /// let query3 = Query.empty&lt;User&gt;
    /// let result3 = translator.Translate(query3)
    /// // result3.Sql = "1=1"
    /// // result3.Parameters = []
    /// </code>
    /// </example>
    member this.Translate(query: FractalDb.Operators.Query<'T>) : TranslatorResult =
        // Reset parameter counter for this translation
        paramCounter <- 0
        
        // Delegate to TranslateQuery for recursive translation
        this.TranslateQuery(query)
    
    /// <summary>
    /// Translates QueryOptions to SQL clauses (ORDER BY, LIMIT, OFFSET).
    /// </summary>
    ///
    /// <param name="options">The QueryOptions&lt;'T&gt; to translate.</param>
    ///
    /// <returns>
    /// A tuple of (SQL clauses string, parameter bindings list).
    /// </returns>
    ///
    /// <remarks>
    /// Generates SQL clauses for query result modification:
    ///
    /// - Sort: Generates ORDER BY clause with field names and ASC/DESC
    /// - Limit: Generates LIMIT @optN clause with parameter
    /// - Skip: Generates OFFSET @optN clause with parameter
    ///
    /// The method returns clauses that should be appended to a SELECT statement
    /// after the WHERE clause:
    ///
    /// SELECT * FROM docs WHERE {whereSql} {optionsSql}
    ///
    /// Clause ordering (SQL standard):
    /// 1. ORDER BY field1 ASC, field2 DESC
    /// 2. LIMIT @opt0
    /// 3. OFFSET @opt1
    ///
    /// Parameter naming:
    /// Options use separate parameter counter (@opt0, @opt1, ...) to avoid
    /// conflicts with WHERE clause parameters (@p0, @p1, ...).
    ///
    /// Field Resolution:
    /// Sort field names are resolved using ResolveField to handle indexed
    /// vs non-indexed fields correctly.
    ///
    /// Note: This method currently handles Sort, Limit, and Skip.
    /// Select, Omit, Search, and Cursor will be implemented in future tasks.
    /// </remarks>
    ///
    /// <example>
    /// <code>
    /// open FractalDb.Options
    ///
    /// let translator = SqlTranslator(schema, false)
    ///
    /// // Sort + Limit + Skip
    /// let options1 = {
    ///     Sort = [("name", SortDirection.Ascending); ("age", SortDirection.Descending)]
    ///     Limit = Some 10
    ///     Skip = Some 20
    ///     Select = None; Omit = None; Search = None; Cursor = None
    /// }
    /// let (sql1, params1) = translator.TranslateOptions(options1)
    /// // sql1 = " ORDER BY _name ASC, _age DESC LIMIT @opt0 OFFSET @opt1"
    /// // params1 = [("@opt0", 10); ("@opt1", 20)]
    ///
    /// // Just limit
    /// let options2 = {
    ///     Sort = []
    ///     Limit = Some 5
    ///     Skip = None
    ///     Select = None; Omit = None; Search = None; Cursor = None
    /// }
    /// let (sql2, params2) = translator.TranslateOptions(options2)
    /// // sql2 = " LIMIT @opt0"
    /// // params2 = [("@opt0", 5)]
    ///
    /// // Empty options
    /// let options3 = QueryOptions.empty&lt;User&gt;
    /// let (sql3, params3) = translator.TranslateOptions(options3)
    /// // sql3 = ""
    /// // params3 = []
    /// </code>
    /// </example>
    member this.TranslateOptions(options: FractalDb.Options.QueryOptions<'T>) : string * list<(string * obj)> =
        let mutable optionParamCounter = 0
        let nextOptionParam () =
            let paramName = $"@opt{optionParamCounter}"
            optionParamCounter <- optionParamCounter + 1
            paramName
        
        let mutable clauses = []
        let mutable parameters = []
        
        // ORDER BY clause
        if not (List.isEmpty options.Sort) then
            let sortFields =
                options.Sort
                |> List.map (fun (fieldName, direction) ->
                    let fieldSql = this.ResolveField(fieldName)
                    let directionSql =
                        match direction with
                        | FractalDb.Options.SortDirection.Ascending -> "ASC"
                        | FractalDb.Options.SortDirection.Descending -> "DESC"
                    $"{fieldSql} {directionSql}")
            
            let sortFieldsStr = String.concat ", " sortFields
            let orderByClause = $" ORDER BY {sortFieldsStr}"
            clauses <- clauses @ [orderByClause]
        
        // LIMIT clause
        match options.Limit with
        | Some limitValue ->
            let paramName = nextOptionParam()
            clauses <- clauses @ [$" LIMIT {paramName}"]
            parameters <- parameters @ [(paramName, box limitValue)]
        | None -> ()
        
        // OFFSET clause (Skip)
        match options.Skip with
        | Some skipValue ->
            let paramName = nextOptionParam()
            clauses <- clauses @ [$" OFFSET {paramName}"]
            parameters <- parameters @ [(paramName, box skipValue)]
        | None -> ()
        
        // Combine all clauses
        let combinedSql = String.concat "" clauses
        (combinedSql, parameters)


