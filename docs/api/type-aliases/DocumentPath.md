[stratadb](../index.md) / DocumentPath

# Type Alias: DocumentPath\<T\>

```ts
type DocumentPath<T> = Paths<T>;
```

Defined in: [src/path-types.ts:46](https://github.com/g5becks/StrataDB/blob/89bee4bbe54bb52f1f1308d5950da4d385abbe16/src/path-types.ts#L46)

Extracts all valid property paths from a document type.

## Type Parameters

### T

`T`

The document type

## Remarks

Generates a union type of all possible paths through the document structure,
including nested properties. Uses type-fest's `Paths` utility internally.

## Example

```typescript
interface User extends Document {
  name: string;
  profile: {
    bio: string;
    settings: {
      theme: 'light' | 'dark';
    };
  };
}

// DocumentPath<User> produces:
// 'name' | 'profile' | 'profile.bio' | 'profile.settings' | 'profile.settings.theme'
```
