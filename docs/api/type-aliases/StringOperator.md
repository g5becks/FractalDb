[stratadb](../index.md) / StringOperator

# Type Alias: StringOperator

```ts
type StringOperator = object;
```

Defined in: [src/query-types.ts:151](https://github.com/g5becks/StrataDB/blob/7791c9d2c0eca8b064c87359859d54870cd83af8/src/query-types.ts#L151)

String-specific query operators for pattern matching.

## Remarks

These operators provide MongoDB-like string matching capabilities with full
type safety. Only available for string fields to prevent runtime errors.

For complex pattern matching that would typically use regular expressions,
consider using these alternatives:
- `$like`: For SQL-style pattern matching with % wildcards
- `$startsWith`: For prefix matching (more efficient than regex)
- `$endsWith`: For suffix matching (more efficient than regex)

## Example

```typescript
// ✅ Valid string queries
const emailQuery: StringOperator = {
  $like: '%@example.com',     // SQL LIKE pattern
  $startsWith: 'admin',      // Starts with prefix
  $endsWith: '@domain.com'   // Ends with suffix
};

// ✅ Pattern matching alternatives
const namePatterns: StringOperator = {
  $startsWith: 'A',          // Names starting with 'A'
  $endsWith: 'son',          // Names ending with 'son'
  $like: '%admin%'           // Contains 'admin' anywhere
};

// ✅ Combining multiple string conditions
const complexStringQuery: StringOperator = {
  $startsWith: 'Dr.',
  $like: '%PhD%'
};
```

## Properties

### $endsWith?

```ts
readonly optional $endsWith: string;
```

Defined in: [src/query-types.ts:159](https://github.com/g5becks/StrataDB/blob/7791c9d2c0eca8b064c87359859d54870cd83af8/src/query-types.ts#L159)

String ends with the specified suffix

***

### $like?

```ts
readonly optional $like: string;
```

Defined in: [src/query-types.ts:153](https://github.com/g5becks/StrataDB/blob/7791c9d2c0eca8b064c87359859d54870cd83af8/src/query-types.ts#L153)

SQL-style LIKE pattern matching

***

### $startsWith?

```ts
readonly optional $startsWith: string;
```

Defined in: [src/query-types.ts:156](https://github.com/g5becks/StrataDB/blob/7791c9d2c0eca8b064c87359859d54870cd83af8/src/query-types.ts#L156)

String starts with the specified prefix
