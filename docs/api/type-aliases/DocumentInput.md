[stratadb](../index.md) / DocumentInput

# Type Alias: DocumentInput&lt;T&gt;

```ts
type DocumentInput<T> = Simplify<SetOptional<Except<T, "_id">, never> & object>;
```

Defined in: [src/core-types.ts:93](https://github.com/g5becks/StrataDb/blob/56b93c15dc2c602cd539356668e05ed574e9a8c7/src/core-types.ts#L93)

Document input type for insertion operations.

## Type Parameters

### T

`T` *extends* [`Document`](Document.md)

The full document type (Document\<YourShape\>)

## Remarks

All fields from the document type are required except for the ID.
The ID is optional - if omitted, a unique ID will be auto-generated.
This type is used for `insertOne()` and `insertMany()` operations.

## Example

```typescript
type User = Document<{
  name: string;
  email: string;
  age: number;
}>;

const userInput: DocumentInput<User> = {
  name: 'Alice',
  email: 'alice@example.com',
  age: 30
  // id is optional
};

await users.insertOne(userInput);
```
