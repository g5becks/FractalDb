[stratadb](../index.md) / OrderingOperators

# Type Alias: OrderingOperators\<T\>

```ts
type OrderingOperators<T> = T extends number | Date ? object : never;
```

Defined in: [src/query-types.ts:62](https://github.com/g5becks/StrataDB/blob/7791c9d2c0eca8b064c87359859d54870cd83af8/src/query-types.ts#L62)

Ordering comparison operators (greater than, less than).

## Type Parameters

### T

`T`

The field type (must be number or Date)

## Remarks

These operators are only available for types that support natural ordering:
numbers and dates. This prevents nonsensical comparisons like checking if
one string is "greater than" another string.
