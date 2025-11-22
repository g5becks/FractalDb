[stratadb](../index.md) / LogicalOperator

# Type Alias: LogicalOperator\<T\>

```ts
type LogicalOperator<T> = object;
```

Defined in: [src/query-types.ts:364](https://github.com/g5becks/StrataDB/blob/89bee4bbe54bb52f1f1308d5950da4d385abbe16/src/query-types.ts#L364)

Logical operators for complex query composition.

## Remarks

Logical operators enable sophisticated query logic by combining multiple conditions.
These operators mirror MongoDB's logical query operators while maintaining full type safety.

All logical operators support recursive composition, allowing unlimited nesting depth
for complex business logic requirements.

## Example

```typescript
type User = Document<{
  name: string;
  age: number;
  status: 'active' | 'inactive' | 'pending';
  email: string;
}>;

// ✅ AND - All conditions must match
const activeAdults: LogicalOperator<User> = {
  $and: [
    { age: { $gte: 18 } },
    { status: 'active' }
  ]
};

// ✅ OR - At least one condition must match
const adminOrModerator: LogicalOperator<User> = {
  $or: [
    { name: { $regex: /^admin/i } },
    { status: 'active' }
  ]
};

// ✅ NOR - None of the conditions must match
const notInactiveOrPending: LogicalOperator<User> = {
  $nor: [
    { status: 'inactive' },
    { status: 'pending' }
  ]
};

// ✅ NOT - Negates a single condition
const notAdult: LogicalOperator<User> = {
  $not: { age: { $gte: 18 } }
};

// ✅ Complex nested queries
const complexQuery: LogicalOperator<User> = {
  $and: [
    {
      $or: [
        { age: { $gt: 18 } },
        { status: 'active' }
      ]
    },
    {
      $not: { email: { $regex: /@spam\.com$/ } }
    }
  ]
};
```

## Type Parameters

### T

`T`

The document type being queried

## Properties

### $and?

```ts
readonly optional $and: readonly QueryFilter<T>[];
```

Defined in: [src/query-types.ts:366](https://github.com/g5becks/StrataDB/blob/89bee4bbe54bb52f1f1308d5950da4d385abbe16/src/query-types.ts#L366)

All conditions in the array must match (logical AND)

***

### $nor?

```ts
readonly optional $nor: readonly QueryFilter<T>[];
```

Defined in: [src/query-types.ts:372](https://github.com/g5becks/StrataDB/blob/89bee4bbe54bb52f1f1308d5950da4d385abbe16/src/query-types.ts#L372)

None of the conditions in the array must match (logical NOR)

***

### $not?

```ts
readonly optional $not: QueryFilter<T>;
```

Defined in: [src/query-types.ts:375](https://github.com/g5becks/StrataDB/blob/89bee4bbe54bb52f1f1308d5950da4d385abbe16/src/query-types.ts#L375)

Negates the given condition (logical NOT)

***

### $or?

```ts
readonly optional $or: readonly QueryFilter<T>[];
```

Defined in: [src/query-types.ts:369](https://github.com/g5becks/StrataDB/blob/89bee4bbe54bb52f1f1308d5950da4d385abbe16/src/query-types.ts#L369)

At least one condition in the array must match (logical OR)
