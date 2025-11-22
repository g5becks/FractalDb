[stratadb](../index.md) / DocumentInput

# Type Alias: DocumentInput\<T\>

```ts
type DocumentInput<T> = Simplify<SetOptional<Except<T, "_id">, never> & object>;
```

Defined in: [src/core-types.ts:93](https://github.com/g5becks/StrataDB/blob/7791c9d2c0eca8b064c87359859d54870cd83af8/src/core-types.ts#L93)

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
