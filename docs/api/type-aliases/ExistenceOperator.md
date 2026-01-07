[stratadb](../index.md) / ExistenceOperator

# Type Alias: ExistenceOperator

```ts
type ExistenceOperator = object;
```

Defined in: [src/query-types.ts:324](https://github.com/g5becks/StrataDb/blob/56b93c15dc2c602cd539356668e05ed574e9a8c7/src/query-types.ts#L324)

Existence operators for checking field presence.

## Remarks

Allows checking whether a field exists in a document, regardless of its value.
This is useful for distinguishing between missing fields and fields with null values.

## Example

```typescript
// Check if field exists (even if null)
const hasEmail = { email: { $exists: true } };

// Check if field is missing
const noPhone = { phone: { $exists: false } };

// Combined with other operators
const activeUsersWithEmail = {
  status: 'active',
  email: { $exists: true, $ne: null }
};
```

## Properties

### $exists?

```ts
readonly optional $exists: boolean;
```

Defined in: [src/query-types.ts:326](https://github.com/g5becks/StrataDb/blob/56b93c15dc2c602cd539356668e05ed574e9a8c7/src/query-types.ts#L326)

Field exists in document (true) or doesn't exist (false)
