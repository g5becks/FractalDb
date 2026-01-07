[stratadb](../index.md) / mergeRetryOptions

# Function: mergeRetryOptions()

```ts
function mergeRetryOptions(
   database?, 
   collection?, 
   operation?): RetryOptions | undefined;
```

Defined in: [src/retry-utils.ts:155](https://github.com/g5becks/StrataDb/blob/56b93c15dc2c602cd539356668e05ed574e9a8c7/src/retry-utils.ts#L155)

Merges retry options from database, collection, and operation levels.
Operation-level options take precedence over collection-level, which take precedence over database-level.

## Parameters

### database?

Database-level retry options

`false` | [`RetryOptions`](../type-aliases/RetryOptions.md)

### collection?

Collection-level retry options

`false` | [`RetryOptions`](../type-aliases/RetryOptions.md)

### operation?

Operation-level retry options

`false` | [`RetryOptions`](../type-aliases/RetryOptions.md)

## Returns

[`RetryOptions`](../type-aliases/RetryOptions.md) \| `undefined`

Merged retry options, or undefined if retry is disabled

## Example

```typescript
const options = mergeRetryOptions(
  { retries: 3 },
  { retries: 5, minTimeout: 2000 },
  { maxTimeout: 10000 }
)
// Result: { retries: 5, minTimeout: 2000, maxTimeout: 10000 }
```
