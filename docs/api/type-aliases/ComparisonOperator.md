[stratadb](../index.md) / ComparisonOperator

# Type Alias: ComparisonOperator&lt;T&gt;

```ts
type ComparisonOperator<T> = T extends number | Date ? EqualityOperators<T> & OrderingOperators<T> & MembershipOperators<T> : EqualityOperators<T> & MembershipOperators<T>;
```

Defined in: [src/query-types.ts:111](https://github.com/g5becks/StrataDb/blob/56b93c15dc2c602cd539356668e05ed574e9a8c7/src/query-types.ts#L111)

## Type Parameters

### T

`T`
