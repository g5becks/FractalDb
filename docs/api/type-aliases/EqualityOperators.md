[stratadb](../index.md) / EqualityOperators

# Type Alias: EqualityOperators\<T\>

```ts
type EqualityOperators<T> = object;
```

Defined in: [src/query-types.ts:86](https://github.com/g5becks/StrataDB/blob/7791c9d2c0eca8b064c87359859d54870cd83af8/src/query-types.ts#L86)

Equality comparison operators.

## Remarks

These operators work with all data types and provide basic equality/inequality checks.

## Type Parameters

### T

`T`

The field type

## Properties

### $eq?

```ts
readonly optional $eq: T;
```

Defined in: [src/query-types.ts:88](https://github.com/g5becks/StrataDB/blob/7791c9d2c0eca8b064c87359859d54870cd83af8/src/query-types.ts#L88)

Equal to the specified value

***

### $ne?

```ts
readonly optional $ne: T;
```

Defined in: [src/query-types.ts:91](https://github.com/g5becks/StrataDB/blob/7791c9d2c0eca8b064c87359859d54870cd83af8/src/query-types.ts#L91)

Not equal to the specified value
