[stratadb](../index.md) / LogicalOperator

# Type Alias: LogicalOperator\<T\>

```ts
type LogicalOperator<T> = object;
```

Defined in: [src/query-types.ts:367](https://github.com/g5becks/StrataDB/blob/7791c9d2c0eca8b064c87359859d54870cd83af8/src/query-types.ts#L367)

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
    { name: { $startsWith: 'admin' } },
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
      $not: { email: { $endsWith: '@spam.com' } }
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

Defined in: [src/query-types.ts:369](https://github.com/g5becks/StrataDB/blob/7791c9d2c0eca8b064c87359859d54870cd83af8/src/query-types.ts#L369)

All conditions in the array must match (logical AND)

***

### $nor?

```ts
readonly optional $nor: readonly QueryFilter<T>[];
```

Defined in: [src/query-types.ts:375](https://github.com/g5becks/StrataDB/blob/7791c9d2c0eca8b064c87359859d54870cd83af8/src/query-types.ts#L375)

None of the conditions in the array must match (logical NOR)

***

### $not?

```ts
readonly optional $not: QueryFilter<T>;
```

Defined in: [src/query-types.ts:378](https://github.com/g5becks/StrataDB/blob/7791c9d2c0eca8b064c87359859d54870cd83af8/src/query-types.ts#L378)

Negates the given condition (logical NOT)

***

### $or?

```ts
readonly optional $or: readonly QueryFilter<T>[];
```

Defined in: [src/query-types.ts:372](https://github.com/g5becks/StrataDB/blob/7791c9d2c0eca8b064c87359859d54870cd83af8/src/query-types.ts#L372)

At least one condition in the array must match (logical OR)
