[stratadb](../index.md) / ComparisonOperator

# Type Alias: ComparisonOperator\<T\>

```ts
type ComparisonOperator<T> = T extends number | Date ? EqualityOperators<T> & OrderingOperators<T> & MembershipOperators<T> : EqualityOperators<T> & MembershipOperators<T>;
```

Defined in: [src/query-types.ts:111](https://github.com/g5becks/StrataDB/blob/7791c9d2c0eca8b064c87359859d54870cd83af8/src/query-types.ts#L111)

## Type Parameters

### T

`T`
