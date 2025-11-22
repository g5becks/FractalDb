[stratadb](../index.md) / FieldOperator

# Type Alias: FieldOperator\<T\>

```ts
type FieldOperator<T> = T extends string ? ComparisonOperator<T> & StringOperator & ExistenceOperator : T extends readonly unknown[] ? ComparisonOperator<T> & ArrayOperator<T> & ExistenceOperator : ComparisonOperator<T> & ExistenceOperator;
```

Defined in: [src/query-types.ts:295](https://github.com/g5becks/StrataDB/blob/7791c9d2c0eca8b064c87359859d54870cd83af8/src/query-types.ts#L295)

Combined field operators for any field type.

## Type Parameters

### T

`T`

The field type

## Remarks

Combines all applicable operator types for a field. Uses conditional types
to only provide operators that make sense for the specific field type.

## Example

```typescript
type User = Document<{
  name: string;
  age: number;
  tags: string[];
  active: boolean;
  createdAt: Date;
}>;

// String field gets string operators
const nameField: FieldOperator<User['name']> = {
  $eq: 'Alice',
  $like: '%admin%',
  $startsWith: 'A'
};

// Number field gets comparison operators
const ageField: FieldOperator<User['age']> = {
  $gt: 18,
  $lte: 65
};

// Array field gets array operators
const tagsField: FieldOperator<User['tags']> = {
  $all: ['developer', 'typescript'],
  $size: 3
};

// Boolean field only gets basic operators
const activeField: FieldOperator<User['active']> = {
  $eq: true,
  $ne: false
};
```
