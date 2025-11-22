[stratadb](../index.md) / CollectionBuilder

# Type Alias: CollectionBuilder\<T\>

```ts
type CollectionBuilder<T> = object;
```

Defined in: src/collection-builder.ts:33

Fluent builder for creating collections with inline schema definition.

## Remarks

CollectionBuilder provides a fluent API for defining collection schemas inline
while creating the collection. This is an alternative to using createSchema()
separately.

## Example

```typescript
// Fluent collection creation
const users = db.collection<User>('users')
  .field('name', { type: 'TEXT', indexed: true })
  .field('email', { type: 'TEXT', indexed: true, unique: true })
  .timestamps(true)
  .build();
```

## Type Parameters

### T

`T` *extends* [`Document`](Document.md)

The document type extending Document

## Methods

### build()

```ts
build(): Collection<T>;
```

Defined in: src/collection-builder.ts:73

Build and return the collection with the defined schema.

#### Returns

[`Collection`](Collection.md)\<`T`\>

The collection instance ready for operations

***

### compoundIndex()

```ts
compoundIndex(
   name, 
   fields, 
options?): CollectionBuilder<T>;
```

Defined in: src/collection-builder.ts:52

Define a compound index spanning multiple fields.

#### Parameters

##### name

`string`

##### fields

readonly keyof `T`[]

##### options?

###### unique?

`boolean`

#### Returns

`CollectionBuilder`\<`T`\>

***

### field()

```ts
field<K>(name, options): CollectionBuilder<T>;
```

Defined in: src/collection-builder.ts:37

Define an indexed field with type checking.

#### Type Parameters

##### K

`K` *extends* `string` \| `number` \| `symbol`

#### Parameters

##### name

`K`

##### options

###### default?

`T`\[`K`\]

###### indexed?

`boolean`

###### nullable?

`boolean`

###### path?

[`JsonPath`](JsonPath.md)

###### type

[`TypeScriptToSQLite`](TypeScriptToSQLite.md)\<`T`\[`K`\]\>

###### unique?

`boolean`

#### Returns

`CollectionBuilder`\<`T`\>

***

### timestamps()

```ts
timestamps(enabled?): CollectionBuilder<T>;
```

Defined in: src/collection-builder.ts:61

Enable automatic timestamp management.

#### Parameters

##### enabled?

`boolean`

#### Returns

`CollectionBuilder`\<`T`\>

***

### validate()

```ts
validate(validator): CollectionBuilder<T>;
```

Defined in: src/collection-builder.ts:66

Add validation function using a type predicate.

#### Parameters

##### validator

(`doc`) => `doc is T`

#### Returns

`CollectionBuilder`\<`T`\>
