[stratadb](../index.md) / PathValue

# Type Alias: PathValue&lt;T, P&gt;

```ts
type PathValue<T, P> = Get<T, P>;
```

Defined in: [src/path-types.ts:79](https://github.com/g5becks/StrataDb/blob/56b93c15dc2c602cd539356668e05ed574e9a8c7/src/path-types.ts#L79)

Gets the type at a specific document path.

## Type Parameters

### T

`T`

The document type

### P

`P` *extends* `string`

The path string (must be a valid path in T)

## Remarks

Extracts the TypeScript type of the value at the given path.
Uses type-fest's `Get` utility internally.
Provides compile-time type safety for nested property access.

## Example

```typescript
interface User extends Document {
  name: string;
  profile: {
    settings: {
      theme: 'light' | 'dark';
    };
  };
}

type ThemeType = PathValue<User, 'profile.settings.theme'>;  // 'light' | 'dark'
type NameType = PathValue<User, 'name'>;  // string
type ProfileType = PathValue<User, 'profile'>;  // { settings: { theme: 'light' | 'dark' } }

// Compiler errors for invalid paths:
// type Invalid = PathValue<User, 'profile.nonexistent'>;  // Error: Property doesn't exist
// type WrongType = PathValue<User, 'name'> extends number ? number : never;  // Error: string is not number
```
